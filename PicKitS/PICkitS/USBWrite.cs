using System;
using System.Runtime.CompilerServices;
using System.Threading;
using USBInterface;

namespace PICkitS
{
	public class USBWrite
	{
		public delegate void GUINotifierLockUp();

		private const byte EVENT_TIMER_RESET_V = 32;

		private static Thread m_write_thread;

		private static Thread m_clear_status_errors;

		private static volatile bool m_we_are_in_write_loop;

		private static volatile bool m_we_had_an_error_writing;

		private static Mutex m_cbuf1_avail_bytes_mutex = new Mutex(false);

		private static AutoResetEvent m_write_thread_has_started_up_event;

		private static AutoResetEvent m_ready_to_write_event;

		private static AutoResetEvent m_have_written_event;

		private static Mutex m_last_script_array_mutex;

		private static object m_priv_write_buffer_lock = new object();

		private static object m_priv_write_script_lock = new object();

		private static byte[] m_command_byte_array;

		private static byte[] m_last_script_array;

		private static byte m_last_script_array_byte_count;

		private static byte m_cbuf1_avail_bytes;

		private static volatile bool m_data_buffer_is_empty;

		public static int m_universal_timeout = 3000;

		internal static volatile bool m_use_script_timeout = true;

		public static event GUINotifierLockUp Tell_Host_PKSA_Needs_Reset;

/*
		{
			[MethodImpl(32)]
			add
			{
				USBWrite.Tell_Host_PKSA_Needs_Reset = (USBWrite.GUINotifierLockUp)Delegate.Combine(USBWrite.Tell_Host_PKSA_Needs_Reset, value);
			}
			[MethodImpl(32)]
			remove
			{
				USBWrite.Tell_Host_PKSA_Needs_Reset = (USBWrite.GUINotifierLockUp)Delegate.Remove(USBWrite.Tell_Host_PKSA_Needs_Reset, value);
			}
		}
*/

		public static void Initialize_Write_Objects()
		{
			USBWrite.m_we_are_in_write_loop = false;
			USBWrite.m_we_had_an_error_writing = false;
			USBWrite.m_cbuf1_avail_bytes = 0;
			USBWrite.m_data_buffer_is_empty = true;
			USBWrite.m_command_byte_array = new byte[65];
			USBWrite.m_last_script_array = new byte[20480];
			Array.Clear(USBWrite.m_last_script_array, 0, USBWrite.m_last_script_array.Length);
			Array.Clear(USBWrite.m_command_byte_array, 0, USBWrite.m_command_byte_array.Length);
			USBWrite.m_last_script_array_byte_count = 0;
			USBWrite.m_command_byte_array[0] = 0;
			USBWrite.m_command_byte_array[1] = 1;
			USBWrite.m_command_byte_array[2] = 0;
			USBWrite.m_command_byte_array[3] = 0;
			USBWrite.m_command_byte_array[4] = 0;
			USBWrite.m_write_thread_has_started_up_event = new AutoResetEvent(false);
			USBWrite.m_ready_to_write_event = new AutoResetEvent(false);
			USBWrite.m_have_written_event = new AutoResetEvent(false);
			USBWrite.m_priv_write_buffer_lock = new object();
			USBWrite.m_priv_write_script_lock = new object();
			USBWrite.m_last_script_array_mutex = new Mutex(false);
		}

		public static bool Update_Status_Packet()
		{
			bool result = false;
			if ((Utilities.m_flags.HID_DeviceReady != false) && USBRead.Read_Thread_Is_Active())
			{
				Utilities.m_flags.g_status_packet_data_update_event.Reset();
				if (USBWrite.Send_Status_Request())
				{
					bool flag = Utilities.m_flags.g_status_packet_data_update_event.WaitOne(2000, false);
					if (flag)
					{
						result = true;
					}
				}
			}
			return result;
		}

		public static bool Update_Special_Status_Packet()
		{
			bool result = false;
			if ((Utilities.m_flags.HID_DeviceReady != false) && USBRead.Read_Thread_Is_Active())
			{
				Utilities.m_flags.g_special_status_request_event.Reset();
				if (USBWrite.Send_Special_Status_Request())
				{
					bool flag = Utilities.m_flags.g_special_status_request_event.WaitOne(2000, false);
					if (flag)
					{
						result = true;
					}
				}
			}
			return result;
		}

		private static bool there_is_room_in_cbuf1(byte p_num_bytes_to_write)
		{
			bool result = false;
			int num = 0;
			if (p_num_bytes_to_write > 255)
			{
				return result;
			}
			if (p_num_bytes_to_write > USBWrite.m_cbuf1_avail_bytes)
			{
				while (p_num_bytes_to_write >= USBWrite.m_cbuf1_avail_bytes)
				{
					if (num++ >= 6)
					{
						break;
					}
					USBWrite.m_cbuf1_avail_bytes_mutex.WaitOne();
					USBWrite.m_cbuf1_avail_bytes = USBWrite.this_many_bytes_are_actually_available_in_cbuf1();
					USBWrite.m_cbuf1_avail_bytes_mutex.ReleaseMutex();
					if (p_num_bytes_to_write < USBWrite.m_cbuf1_avail_bytes)
					{
						result = true;
						break;
					}
					Thread.Sleep(100);
				}
			}
			else
			{
				result = true;
			}
			return result;
		}

		private static byte this_many_bytes_are_actually_available_in_cbuf1()
		{
			byte result = 0;
			if (USBWrite.Update_Special_Status_Packet())
			{
				Utilities.m_flags.g_status_packet_mutex.WaitOne();
				result = Constants.STATUS_PACKET_DATA[54];
				Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
			}
			return result;
		}

		public static bool kick_off_write_thread()
		{
			bool result;
			if (!USBWrite.m_we_are_in_write_loop)
			{
				USBWrite.m_write_thread = new Thread(new ThreadStart(USBWrite.Write_USB_Thread));
				USBWrite.m_write_thread.IsBackground=true;
				USBWrite.m_write_thread.Start();
				result = USBWrite.m_write_thread_has_started_up_event.WaitOne(5000, false);
			}
			else
			{
				result = false;
			}
			return result;
		}

		public static bool There_Was_A_Write_Error()
		{
			return USBWrite.m_we_had_an_error_writing;
		}

		private static bool We_Are_Done_Writing_Data()
		{
			return USBWrite.m_data_buffer_is_empty;
		}

		public static bool Transaction_Is_Complete()
		{
			bool result = false;
			if (USBWrite.We_Are_Done_Writing_Data() && !USBRead.m_read_thread_is_processing_a_USB_packet && USBWrite.Update_Status_Packet())
			{
				Utilities.m_flags.g_status_packet_mutex.WaitOne();
				if ((Constants.STATUS_PACKET_DATA[37] & 1) == 0 && Constants.STATUS_PACKET_DATA[55] == 0)
				{
					result = true;
				}
				Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
			}
			return result;
		}

		public static bool Write_Thread_Is_Active()
		{
			return USBWrite.m_we_are_in_write_loop;
		}

		public static void Dispose_Of_Write_Objects()
		{
			USBWrite.m_write_thread_has_started_up_event.Close();
			USBWrite.m_ready_to_write_event.Close();
			USBWrite.m_have_written_event.Close();
			USBWrite.m_cbuf1_avail_bytes_mutex.Close();
		}

		public static void Kill_Write_Thread()
		{
			if (USBWrite.m_write_thread != null && USBWrite.m_write_thread.IsAlive)
			{
				USBWrite.m_we_are_in_write_loop = false;
				USBWrite.m_write_thread.Join();
			}
		}

		public static bool Send_Data_Packet_To_PICkitS(ref byte[] p_data)
		{
			bool result = true;
			lock (USBWrite.m_priv_write_buffer_lock)
			{
				if (USBWrite.m_we_are_in_write_loop && (Utilities.m_flags.HID_DeviceReady != false))
				{
					for (int i = 0; i < Utilities.m_flags.write_buffer.Length; i++)
					{
						Utilities.m_flags.write_buffer[i] = p_data[i];
					}
					USBWrite.m_ready_to_write_event.Set();
					if (!USBWrite.m_have_written_event.WaitOne(3000, false))
					{
						issue_Tell_Host_PKSA_Needs_Reset();
					}
					if (USBWrite.There_Was_A_Write_Error())
					{
						result = false;
					}
				}
				else
				{
					string.Format("Error writing to USB device", new object[0]);
					result = false;
				}
			}
			return result;
		}

		private static void issue_Tell_Host_PKSA_Needs_Reset()
		{
			if (Tell_Host_PKSA_Needs_Reset != null)
			{
				Tell_Host_PKSA_Needs_Reset();
			}
		}

		public static void Write_USB_Thread()
		{
			USBWrite.m_we_are_in_write_loop = true;
			USBWrite.m_write_thread_has_started_up_event.Set();
			int num = 0;
			bool flag2 = false;


			while (USBWrite.m_we_are_in_write_loop)
			{
				//bool flag = USBWrite.m_ready_to_write_event.WaitOne(500, false);
				bool flag = USBWrite.m_ready_to_write_event.WaitOne(500, false);
				if (flag)
				{
					//flag2 = Utilities.WriteFile(Utilities.m_flags.HID_write_handle, Utilities.m_flags.write_buffer, (int)Utilities.m_flags.orbl, ref num, 0);
					/* Replaced HID.dll write api */

					/*
					byte[] tempwrite = new byte[64];
					for( int i = 0; i<64; i++ )
					{
						tempwrite[i] = Utilities.m_flags.write_buffer[i+1];

					}
					flag2 = Utilities.m_flags.HID_Handle.Write(tempwrite, ref num);
					num = num + 1;
					*/ 
					flag2 = Utilities.m_flags.HID_Handle.Write(Utilities.m_flags.write_buffer, ref num);
				//	Console.WriteLine("Written data: " + num.ToString());
					USBWrite.m_have_written_event.Set();
					if (!flag2 || num != (int)Utilities.m_flags.orbl)
					{
						USBWrite.m_we_had_an_error_writing = true;
						Console.WriteLine("Error in writing");
					}
					else
					{
						USBWrite.m_we_had_an_error_writing = false;
					}
				}
			}
		}

		public static bool Send_Status_Request()
		{
			USBWrite.m_command_byte_array[0] = 0;
			USBWrite.m_command_byte_array[1] = 1;
			USBWrite.m_command_byte_array[2] = 2;
			USBWrite.m_command_byte_array[3] = 0;
			USBWrite.m_command_byte_array[4] = 0;
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref USBWrite.m_command_byte_array);
			if (!flag)
			{
				flag = false;
			}
			return flag;
		}

		public static bool Send_Special_Status_Request()
		{
			USBWrite.m_command_byte_array[0] = 0;
			USBWrite.m_command_byte_array[1] = 11;
			USBWrite.m_command_byte_array[2] = 171;
			USBWrite.m_command_byte_array[3] = 0;
			USBWrite.m_command_byte_array[4] = 0;
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref USBWrite.m_command_byte_array);
			if (!flag)
			{
				flag = false;
			}
			return flag;
		}

		public static bool Send_Warm_Reset_Cmd()
		{
			USBRead.m_EVENT_TIME_ROLLOVER = 0.0;
			USBRead.m_RUNNING_TIME = 0.0;
			USBWrite.m_command_byte_array[0] = 0;
			USBWrite.m_command_byte_array[1] = 1;
			USBWrite.m_command_byte_array[2] = 1;
			USBWrite.m_command_byte_array[3] = 0;
			USBWrite.m_command_byte_array[4] = 0;
			bool result = USBWrite.Send_Data_Packet_To_PICkitS(ref USBWrite.m_command_byte_array);
			USBWrite.Update_Status_Packet();
			return result;
		}

		public static bool Send_Cold_Reset_Cmd()
		{
			USBWrite.m_command_byte_array[0] = 0;
			USBWrite.m_command_byte_array[1] = 1;
			USBWrite.m_command_byte_array[2] = 0;
			USBWrite.m_command_byte_array[3] = 0;
			USBWrite.m_command_byte_array[4] = 0;
			bool result = USBWrite.Send_Data_Packet_To_PICkitS(ref USBWrite.m_command_byte_array);
			USBWrite.Update_Status_Packet();
			return result;
		}

		public static bool Send_CtrlBlk2EE_Cmd()
		{
			USBWrite.m_command_byte_array[0] = 0;
			USBWrite.m_command_byte_array[1] = 1;
			USBWrite.m_command_byte_array[2] = 3;
			USBWrite.m_command_byte_array[3] = 0;
			USBWrite.m_command_byte_array[4] = 0;
			bool result = USBWrite.Send_Data_Packet_To_PICkitS(ref USBWrite.m_command_byte_array);
			USBWrite.Update_Status_Packet();
			return result;
		}

		public static bool Send_EE2CtrlBlk_Cmd()
		{
			USBWrite.m_command_byte_array[0] = 0;
			USBWrite.m_command_byte_array[1] = 1;
			USBWrite.m_command_byte_array[2] = 4;
			USBWrite.m_command_byte_array[3] = 0;
			USBWrite.m_command_byte_array[4] = 0;
			bool result = USBWrite.Send_Data_Packet_To_PICkitS(ref USBWrite.m_command_byte_array);
			USBWrite.Update_Status_Packet();
			return result;
		}

		public static bool Send_FlushCbuf2_Cmd()
		{
			USBWrite.m_command_byte_array[0] = 0;
			USBWrite.m_command_byte_array[1] = 1;
			USBWrite.m_command_byte_array[2] = 5;
			USBWrite.m_command_byte_array[3] = 0;
			USBWrite.m_command_byte_array[4] = 0;
			bool result = USBWrite.Send_Data_Packet_To_PICkitS(ref USBWrite.m_command_byte_array);
			USBWrite.Update_Status_Packet();
			return result;
		}

		public static bool Send_CommReset_Cmd()
		{
			USBWrite.m_command_byte_array[0] = 0;
			USBWrite.m_command_byte_array[1] = 1;
			USBWrite.m_command_byte_array[2] = 6;
			USBWrite.m_command_byte_array[3] = 0;
			USBWrite.m_command_byte_array[4] = 0;
			bool result = USBWrite.Send_Data_Packet_To_PICkitS(ref USBWrite.m_command_byte_array);
			USBWrite.Update_Status_Packet();
			return result;
		}

		public static void Send_CommClear_Cmd()
		{
			USBWrite.m_command_byte_array[0] = 0;
			USBWrite.m_command_byte_array[1] = 1;
			USBWrite.m_command_byte_array[2] = 7;
			USBWrite.m_command_byte_array[3] = 0;
			USBWrite.m_command_byte_array[4] = 0;
			USBWrite.Send_Data_Packet_To_PICkitS(ref USBWrite.m_command_byte_array);
			USBWrite.Update_Status_Packet();
		}

		public static void Clear_Status_Errors()
		{
			USBWrite.m_clear_status_errors = new Thread(new ThreadStart(USBWrite.Send_CommClear_Cmd));
			USBWrite.m_clear_status_errors.IsBackground=true;
			USBWrite.m_clear_status_errors.Start();
		}

		public static bool Send_Event_Timer_Reset_Cmd()
		{
			byte[] array = new byte[65];
			array[0] = 0;
			array[1] = 3;
			array[2] = 1;
			array[3] = 32;
			return USBWrite.Send_Data_Packet_To_PICkitS(ref array);
		}

		public static bool Clear_CBUF(byte p_buffer)
		{
			if (p_buffer < 1 || p_buffer > 3)
			{
				return false;
			}
			byte[] array = new byte[65];
			array[0] = 0;
			array[1] =(byte)( p_buffer + 7);
			return USBWrite.Send_Data_Packet_To_PICkitS(ref array);
		}

		public static bool Send_CtrlBlkWrite_Cmd(ref byte[] p_data_array)
		{
			byte[] array = new byte[65];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 2;
			int i;
			for (i = 0; i < 24; i++)
			{
				array[i + 2] = p_data_array[i];
			}
			array[i] = 0;
			USBWrite.Send_Cold_Reset_Cmd();
			bool result = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			USBWrite.Send_Warm_Reset_Cmd();
			USBWrite.Update_Status_Packet();
			return result;
		}

		public static bool Send_LED_State_Cmd(int p_LED_num, byte p_value)
		{
			byte[] array = new byte[65];
			Array.Clear(array, 0, array.Length);
			array[1] = 3;
			array[2] = 2;
			switch (p_LED_num)
			{
				case 1:
					array[3] = 18;
					break;
				case 2:
					array[3] = 19;
					break;
				default:
					return false;
			}
			array[4] = p_value;
			array[5] = 0;
			bool result = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			USBWrite.Update_Status_Packet();
			return result;
		}

		public static void Get_Last_Script_Sent(ref byte[] p_array, byte p_byte_count)
		{
			USBWrite.m_last_script_array_mutex.WaitOne();
			for (int i = 0; i < (int)p_byte_count; i++)
			{
				p_array[i] = USBWrite.m_last_script_array[i];
			}
			USBWrite.m_last_script_array_mutex.ReleaseMutex();
		}

		public static byte Get_Last_Script_ByteCount()
		{
			return USBWrite.m_last_script_array_byte_count;
		}

		public static bool Send_Script_To_PICkitS(ref byte[] p_send_byte_array)
		{
			bool result;
			lock (USBWrite.m_priv_write_script_lock)
			{
				USBWrite.m_data_buffer_is_empty = false;
				bool flag = false;
				uint num = 0u;
				uint num2 = (uint)p_send_byte_array[2];
				byte[] array = new byte[65];
				USBWrite.m_last_script_array_mutex.WaitOne();
				USBWrite.m_last_script_array_byte_count = (byte)(num2 + 2u);
				Array.Clear(USBWrite.m_last_script_array, 0, USBWrite.m_last_script_array.Length);
				for (int i = 0; i < (int)USBWrite.m_last_script_array_byte_count; i++)
				{
					USBWrite.m_last_script_array[i] = p_send_byte_array[i + 1];
				}
				Utilities.m_flags.g_PKSA_has_completed_script.Reset();
				USBWrite.m_last_script_array_mutex.ReleaseMutex();
				uint num3 = num2 / 62u;
				byte b = (byte)(num2 % 62u);
				if (b != 0)
				{
					num3 += 1u;
				}
				int num4 = 0;
				while ((long)num4 < (long)((ulong)num3))
				{
					Array.Clear(array, 0, array.Length);
					array[0] = p_send_byte_array[0];
					array[1] = p_send_byte_array[1];
					uint num5;
					uint num6;
					if (num3 != 1u)
					{
						num5 = 3u;
						if ((long)num4 == (long)((ulong)(num3 - 1u)) && b != 0)
						{
							num6 = (uint)(b + 4);
							array[2] = b;
						}
						else
						{
							num6 = 65u;
							array[2] = 62;
						}
					}
					else
					{
						num6 = num2 + 4u;
						num5 = 2u;
					}
					for (uint num7 = num5; num7 < num6; num7 += 1u)
					{
						array[(int)((UIntPtr)num7)] = p_send_byte_array[(int)((UIntPtr)(num + num7))];
					}
					if (num3 != 1u)
					{
						num += 62u;
						if ((long)num4 == (long)((ulong)(num3 - 1u)))
						{
							num += 1u;
						}
					}
					if (array[1] == 3)
					{
						if (!USBWrite.there_is_room_in_cbuf1(array[2]))
						{
							result = false;
							return result;
						}
						USBWrite.m_cbuf1_avail_bytes_mutex.WaitOne();
						USBWrite.m_cbuf1_avail_bytes -= array[2];
						USBWrite.m_cbuf1_avail_bytes_mutex.ReleaseMutex();
						if (!USBWrite.Send_Data_Packet_To_PICkitS(ref array))
						{
							result = false;
							return result;
						}
						if (USBWrite.There_Was_A_Write_Error())
						{
							result = false;
							return result;
						}
					}
					else
					{
						if (!USBWrite.Send_Data_Packet_To_PICkitS(ref array))
						{
							result = false;
							return result;
						}
						if (USBWrite.There_Was_A_Write_Error())
						{
							result = false;
							return result;
						}
					}
					num4++;
				}
				if (array[1] == 3)
				{
					if (USBWrite.m_use_script_timeout)
					{
						bool flag2 = Utilities.m_flags.g_PKSA_has_completed_script.WaitOne(USBWrite.m_universal_timeout, false);
						if (flag2 && USBWrite.Update_Status_Packet())
						{
							uint num8 = 0u;
							if (!Status.There_Is_A_Status_Error(ref num8))
							{
								flag = true;
							}
						}
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				USBWrite.m_data_buffer_is_empty = true;
				result = flag;
			}
			return result;
		}

		public static void configure_outbound_control_block_packet(ref byte[] p_data, ref string p_str, ref byte[] p_status_packet_data)
		{
			p_str = "";
			p_data[0] = 0;
			p_data[1] = 2;
			int i;
			for (i = 2; i < 31; i++)
			{
				string text = string.Format("{0:X2} ", p_status_packet_data[7 + i - 2]);
				p_str += text;
				p_data[i] = p_status_packet_data[7 + i - 2];
			}
			p_data[i] = 0;
			if ((p_status_packet_data[23] & 128) == 128)
			{
				LIN.m_autobaud_is_on = true;
				return;
			}
			LIN.m_autobaud_is_on = false;
		}

		public static bool write_and_verify_config_block(ref byte[] p_control_block_data, ref string p_result_str, bool p_perform_warm_and_cold_reset, ref string p_cb_data_str)
		{
			bool result = false;
			if (p_perform_warm_and_cold_reset)
			{
				USBWrite.Send_Cold_Reset_Cmd();
			}
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref p_control_block_data);
			if (p_perform_warm_and_cold_reset)
			{
				USBWrite.Send_Warm_Reset_Cmd();
			}
			if (flag)
			{
				if (USBWrite.Update_Status_Packet())
				{
					Utilities.m_flags.g_status_packet_mutex.WaitOne();
					int i;
					for (i = 7; i < 31; i++)
					{
						if (Constants.STATUS_PACKET_DATA[i] != p_control_block_data[i - 5])
						{
							p_result_str = string.Format("Byte {0} failed verification in config block write.\n Value reads {1:X2}, but should be {2:X2}.", i - 7, Constants.STATUS_PACKET_DATA[i], p_control_block_data[i - 5]);
							break;
						}
					}
					if (i == 31)
					{
						result = true;
						p_result_str = string.Format("PICkit Serial Analyzer correctly updated.", new object[0]);
						p_cb_data_str = "";
						for (i = 7; i < 31; i++)
						{
							p_cb_data_str += string.Format("{0:X2} ", Constants.STATUS_PACKET_DATA[i]);
						}
					}
					Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
				}
				else
				{
					p_result_str = string.Format("Error requesting config verification - Config Block may not be updated correctly", new object[0]);
				}
			}
			else
			{
				p_result_str = string.Format("Error sending config packet - Config Block may not be updated correctly", new object[0]);
			}
			return result;
		}

		public static bool write_and_verify_LIN_config_block(ref byte[] p_control_block_data, ref string p_result_str, bool p_perform_warm_and_cold_reset, ref string p_cb_data_str)
		{
			bool result = false;
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref p_control_block_data);
			Device.Clear_Status_Errors();
			if (flag)
			{
				if (USBWrite.Update_Status_Packet())
				{
					Utilities.m_flags.g_status_packet_mutex.WaitOne();
					int i;
					for (i = 7; i < 31; i++)
					{
						if (Constants.STATUS_PACKET_DATA[i] != p_control_block_data[i - 5])
						{
							p_result_str = string.Format("Byte {0} failed verification in config block write.\n Value reads {1:X2}, but should be {2:X2}.", i - 7, Constants.STATUS_PACKET_DATA[i], p_control_block_data[i - 5]);
							break;
						}
					}
					if (i == 31)
					{
						result = true;
						p_result_str = string.Format("PICkit Serial Analyzer correctly updated.", new object[0]);
						p_cb_data_str = "";
						for (i = 7; i < 31; i++)
						{
							p_cb_data_str += string.Format("{0:X2} ", Constants.STATUS_PACKET_DATA[i]);
						}
					}
					Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
				}
				else
				{
					p_result_str = string.Format("Error requesting config verification - Config Block may not be updated correctly", new object[0]);
				}
			}
			else
			{
				p_result_str = string.Format("Error sending config packet - Config Block may not be updated correctly", new object[0]);
			}
			return result;
		}
	}
}
