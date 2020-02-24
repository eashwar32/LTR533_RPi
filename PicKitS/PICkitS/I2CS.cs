using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PICkitS
{
	public class I2CS
	{
		public delegate void GUINotifierReceive(byte slave_addr);

		public delegate void GUINotifierRead(byte slave_addr, ushort byte_count, ref byte[] data);

		public delegate void GUINotifierWrite(byte slave_addr, ushort byte_count, ref byte[] data);

		public delegate void GUINotifierError();

		private const int m_slave_array_size = 1000;

		private const ushort SLAVE_ADDR_RESET = 512;

		private static Mutex m_read_mutex = new Mutex(false);

		private static Thread m_read;

		private static Mutex m_receive_mutex = new Mutex(false);

		private static Thread m_receive;

		private static Mutex m_write_mutex = new Mutex(false);

		private static Thread m_write;

		private static byte[] m_slave_data_array = new byte[1000];

		private static uint m_slave_array_index = 0u;

		internal static ushort m_last_slave_addr_received = 512;

		internal static ushort m_previous_slave_addr_received = 512;

		internal static byte m_start_data_addr_received = 0;

		internal static bool m_slave_address_was_just_set = false;

		internal static bool m_master_is_waiting_for_data = false;

		internal static bool m_stop_command_issued = false;

		public static event I2CS.GUINotifierWrite Write;

		/*
		{
			[MethodImpl(32)]
			add
			{
				I2CS.Write = (I2CS.GUINotifierWrite)Delegate.Combine(I2CS.Write, value);
			}
			[MethodImpl(32)]
			remove
			{
				I2CS.Write = (I2CS.GUINotifierWrite)Delegate.Remove(I2CS.Write, value);
			}
		}
		*/

		public static event I2CS.GUINotifierRead Read;

		/*
		{
			[MethodImpl(32)]
			add
			{
				I2CS.Read = (I2CS.GUINotifierRead)Delegate.Combine(I2CS.Read, value);
			}
			[MethodImpl(32)]
			remove
			{
				I2CS.Read = (I2CS.GUINotifierRead)Delegate.Remove(I2CS.Read, value);
			}
		}
		*/

		public static event I2CS.GUINotifierReceive Receive;
		/*
		{
			[MethodImpl(32)]
			add
			{
				I2CS.Receive = (I2CS.GUINotifierReceive)Delegate.Combine(I2CS.Receive, value);
			}
			[MethodImpl(32)]
			remove
			{
				I2CS.Receive = (I2CS.GUINotifierReceive)Delegate.Remove(I2CS.Receive, value);
			}
		}
		*/

		public static event I2CS.GUINotifierError Error;

		/*
		{
			[MethodImpl(32)]
			add
			{
				I2CS.Error = (I2CS.GUINotifierError)Delegate.Combine(I2CS.Error, value);
			}
			[MethodImpl(32)]
			remove
			{
				I2CS.Error = (I2CS.GUINotifierError)Delegate.Remove(I2CS.Error, value);
			}
		}
		*/

		internal static void issue_error()
		{
			Basic.Reset_Control_Block();
			I2CS.Error();
		}

		internal static void issue_event()
		{
			if (Utilities.g_i2cs_mode == Utilities.I2CS_MODE.INTERACTIVE)
			{
				if (I2CS.m_last_slave_addr_received == I2CS.m_previous_slave_addr_received + 1 && I2CS.m_master_is_waiting_for_data && !I2CS.m_stop_command_issued)
				{
					I2CS.issue_read_command();
				}
				else if (I2CS.m_previous_slave_addr_received == 512 && I2CS.m_last_slave_addr_received % 2 != 0 && I2CS.m_master_is_waiting_for_data && !I2CS.m_stop_command_issued)
				{
					I2CS.issue_receive_command();
				}
				else if (I2CS.m_previous_slave_addr_received == 512 && I2CS.m_last_slave_addr_received % 2 == 0 && !I2CS.m_master_is_waiting_for_data && I2CS.m_stop_command_issued)
				{
					I2CS.issue_write_command();
				}
				else if (I2CS.m_last_slave_addr_received != I2CS.m_previous_slave_addr_received + 1 && !I2CS.m_master_is_waiting_for_data && I2CS.m_previous_slave_addr_received != 512 && I2CS.m_slave_address_was_just_set && !I2CS.m_stop_command_issued)
				{
					I2CS.issue_write_command();
					USBRead.Clear_Data_Array(0u);
					USBRead.Clear_Raw_Data_Array();
				}
				else if (I2CS.m_previous_slave_addr_received != 512 && I2CS.m_last_slave_addr_received != I2CS.m_previous_slave_addr_received + 1 && I2CS.m_last_slave_addr_received % 2 != 0 && I2CS.m_slave_address_was_just_set && !I2CS.m_stop_command_issued)
				{
					I2CS.issue_read_command();
					USBRead.Clear_Data_Array(0u);
					USBRead.Clear_Raw_Data_Array();
				}
				else if (I2CS.m_previous_slave_addr_received != 512 && I2CS.m_last_slave_addr_received % 2 == 0 && I2CS.m_slave_address_was_just_set && !I2CS.m_stop_command_issued)
				{
					I2CS.issue_receive_command();
					USBRead.Clear_Data_Array(0u);
					USBRead.Clear_Raw_Data_Array();
				}
			}
		}

		internal static void reset_buffers()
		{
			USBRead.Clear_Data_Array(0u);
			USBRead.Clear_Raw_Data_Array();
			I2CS.m_last_slave_addr_received = 512;
			I2CS.m_previous_slave_addr_received = 512;
		}

		private static void issue_read_command()
		{
			I2CS.m_slave_array_index = USBRead.m_cbuf2_data_array_index;
			int num = 0;
			while ((long)num < (long)((ulong)I2CS.m_slave_array_index))
			{
				I2CS.m_slave_data_array[num] = USBRead.m_cbuf2_data_array[num];
				num++;
			}
			I2CS.m_read = new Thread(new ThreadStart(I2CS.fire_and_forget_read));
			I2CS.m_read.IsBackground =true;
			I2CS.m_read.Start();
		}

		private static void issue_receive_command()
		{
			I2CS.m_receive = new Thread(new ThreadStart(I2CS.fire_and_forget_receive));
			I2CS.m_receive.IsBackground =true;
			I2CS.m_receive.Start();
		}

		private static void issue_write_command()
		{
			I2CS.m_slave_array_index = USBRead.m_cbuf2_data_array_index;
			int num = 0;
			while ((long)num < (long)((ulong)I2CS.m_slave_array_index))
			{
				I2CS.m_slave_data_array[num] = USBRead.m_cbuf2_data_array[num];
				num++;
			}
			I2CS.m_write = new Thread(new ThreadStart(I2CS.fire_and_forget_write));
			I2CS.m_write.IsBackground = true;
			I2CS.m_write.Start();
		}

		private static void fire_and_forget_read()
		{
			I2CS.m_read_mutex.WaitOne();
			if (I2CS.Read != null)
			{
				I2CS.Read((byte)I2CS.m_last_slave_addr_received, (ushort)I2CS.m_slave_array_index, ref I2CS.m_slave_data_array);
			}
			I2CS.m_read_mutex.ReleaseMutex();
		}

		private static void fire_and_forget_receive()
		{
			I2CS.m_receive_mutex.WaitOne();
			if (I2CS.Receive != null)
			{
				I2CS.Receive((byte)I2CS.m_last_slave_addr_received);
			}
			I2CS.m_receive_mutex.ReleaseMutex();
		}

		private static void fire_and_forget_write()
		{
			I2CS.m_write_mutex.WaitOne();
			if (I2CS.Write != null)
			{
				I2CS.Write((byte)I2CS.m_last_slave_addr_received, (ushort)I2CS.m_slave_array_index, ref I2CS.m_slave_data_array);
			}
			I2CS.m_write_mutex.ReleaseMutex();
		}

		public static bool Configure_PICkitSerial_For_I2CSlave_Default_Mode(byte p_slave_addr, byte p_slave_mask, byte p_read_byte_0_data, byte p_read_bytes_1_N_data)
		{
			bool result = false;
			if (Basic.Configure_PICkitSerial(7, true))
			{
				string text = "";
				string text2 = "";
				byte[] array = new byte[65];
				byte[] array2 = new byte[65];
				if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
				{
					Array.Clear(array, 0, array.Length);
					Array.Clear(array2, 0, array2.Length);
					if (!Basic.Get_Status_Packet(ref array2))
					{
						return false;
					}
					array2[23] = 0;
					array2[27] = p_read_bytes_1_N_data;
					array2[28] = p_read_byte_0_data;
					array2[29] = p_slave_addr;
					array2[30] = p_slave_mask;
					USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
					result = USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
				}
			}
			return result;
		}

		public static bool Configure_PICkitSerial_For_I2CSlave_Interactive_Mode(byte p_slave_addr, byte p_slave_mask)
		{
			bool result = false;
			string text = "";
			string text2 = "";
			byte[] array = new byte[65];
			byte[] array2 = new byte[65];
			if (!Basic.Get_Status_Packet(ref array2))
			{
				return false;
			}
			byte b = array2[28];
			byte b2 = array2[27];
			if (Basic.Configure_PICkitSerial(7, true) && (Utilities.m_flags.HID_DeviceReady != false))
			{
				Array.Clear(array, 0, array.Length);
				Array.Clear(array2, 0, array2.Length);
				if (!Basic.Get_Status_Packet(ref array2))
				{
					return false;
				}
				array2[23] = 1;
				array2[27] = b2;
				array2[28] = b;
				array2[29] = p_slave_addr;
				array2[30] = p_slave_mask;
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				result = USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
			}
			return result;
		}

		public static bool Set_I2CSlave_Address_and_Mask(byte p_slave_addr, byte p_slave_mask)
		{
			bool result = false;
			string text = "";
			string text2 = "";
			byte[] array = new byte[65];
			byte[] array2 = new byte[65];
			if (!Basic.Get_Status_Packet(ref array2))
			{
				return false;
			}
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				Array.Clear(array2, 0, array2.Length);
				if (!Basic.Get_Status_Packet(ref array2))
				{
					return false;
				}
				array2[29] = p_slave_addr;
				array2[30] = p_slave_mask;
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				result = USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
			}
			return result;
		}

		public static bool Get_I2CSlave_Address_and_Mask(ref byte p_slave_addr, ref byte p_slave_mask)
		{
			byte[] array = new byte[65];
			if (!Basic.Get_Status_Packet(ref array))
			{
				return false;
			}
			p_slave_addr = array[29];
			p_slave_mask = array[30];
			return true;
		}

		public static bool Send_Bytes(byte p_num_bytes_to_write, ref byte[] p_data_array, ref string p_script_view)
		{
			if (p_num_bytes_to_write > 253)
			{
				return false;
			}
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = (byte)(4 + p_num_bytes_to_write);
			array[3] = 193;
			array[4] = p_num_bytes_to_write;
			int i;
			for (i = 0; i < (int)p_num_bytes_to_write; i++)
			{
				array[i + 5] = p_data_array[i];
			}
			array[i + 5] = 31;
			array[i + 6] = 119;
			array[i + 7] = 0;
			p_script_view = "[SB]";
			string text = string.Format("[{0:X2}]", array[4]);
			p_script_view += text;
			for (i = 0; i < (int)p_num_bytes_to_write; i++)
			{
				text = string.Format("[{0:X2}]", array[i + 5]);
				p_script_view += text;
			}
			USBRead.Clear_Data_Array(0u);
			USBRead.Clear_Raw_Data_Array();
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Configure_PICkitSerial_For_I2CSlave_Auto_Mode(byte p_slave_addr, byte p_slave_mask, byte p_array_byte_count, ref byte[] p_profile_array, ref string p_result_str)
		{
			byte[] array = new byte[65];
			byte[] array2 = new byte[255];
			byte[] array3 = new byte[65];
			bool result = false;
			string text = "";
			Array.Clear(array, 0, array.Length);
			Array.Clear(array3, 0, array3.Length);
			Mode.update_status_packet_data(7, ref array3);
			array3[23] = 2;
			array3[29] = p_slave_addr;
			array3[30] = p_slave_mask;
			USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array3);
			USBWrite.Send_Cold_Reset_Cmd();
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			Array.Clear(array2, 0, array2.Length);
			array2[0] = 0;
			array2[1] = 5;
			array2[2] = p_array_byte_count;
			for (int i = 3; i < (int)(p_array_byte_count + 3); i++)
			{
				array2[i] = p_profile_array[i - 3];
			}
			USBWrite.Send_Script_To_PICkitS(ref array2);
			USBWrite.Send_Warm_Reset_Cmd();
			if (flag)
			{
				if (USBWrite.Update_Status_Packet())
				{
					Utilities.m_flags.g_status_packet_mutex.WaitOne();
					int j;
					for (j = 7; j < 31; j++)
					{
						if (Constants.STATUS_PACKET_DATA[j] != array[j - 5])
						{
							p_result_str = string.Format("Byte {0} failed verification in config block write.\n Value reads {1:X2}, but should be {2:X2}.", j - 7, Constants.STATUS_PACKET_DATA[j], array[j - 5]);
							break;
						}
					}
					if (j == 31)
					{
						result = true;
						p_result_str = string.Format("PICkit Serial Analyzer correctly updated.", new object[0]);
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

		public static byte Get_Slave_Addresses_That_Will_Acknowledge(byte p_slave_addr, byte p_slave_mask, ref byte[] p_addr_array, ref string p_addr_str)
		{
			byte b = 0;
			p_addr_str = "";
			byte b2 = (byte)(p_slave_mask & 62);
			byte b3 = (byte)(p_slave_addr & ~b2);
			for (int i = 0; i <= 255; i++)
			{
				byte b4 = (byte)(i & (int)(~(int)b2));
				if (b4 == b3)
				{
					byte[] arg_2F_0 = p_addr_array;
					byte expr_28 = b;
					b = (byte)(expr_28 + 1);
					arg_2F_0[(int)expr_28] = (byte)i;
					p_addr_str += string.Format("{0:X2} ", i);
				}
			}
			return b;
		}
	}
}
