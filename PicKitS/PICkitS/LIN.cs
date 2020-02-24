using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PICkitS
{
	public class LIN
	{
		public delegate void GUINotifierOR(byte masterid, byte[] data, byte length, byte error, ushort baud, double time);

		public delegate void GUINotifierOA(byte masterid, byte[] data, byte length, byte error, ushort baud, double time);

		internal enum OPMODE
		{
			LISTEN,
			TRANSMIT,
			DISPLAY_ALL
		}

		internal struct FRAMEINFO
		{
			internal volatile byte FrameID;

			internal volatile byte[] FrameData;

			internal volatile byte bytecount;

			internal volatile ushort baud;

			internal double time;

			internal long frame_timeout_time;
		}

		internal struct SLAVE_PROFILE_ID
		{
			internal byte FrameID;

			internal byte ByteCount;

			internal byte[] Data;
		}

		internal struct BUILD_STATE
		{
			internal volatile bool we_have_transmitted;

			internal volatile bool we_are_building_a_frame;

			internal volatile bool we_have_an_id;

			internal volatile bool we_had_a_status_error;

			internal volatile bool we_timed_out;

			internal volatile bool next_frame_header_received;

			internal volatile bool transmit_data_byte_count_zero;
		}

		internal struct WORKING_FRAME
		{
			internal LIN.FRAMEINFO FrameInfo;

			internal LIN.BUILD_STATE BuildState;
		}

		internal const int MAX_NUM_DATA_BYTES = 9;

		internal const int FRAME_ARRAY_COUNT = 256;

		private static long[] m_time = new long[11];

		public static Stopwatch m_stopwatch;

		internal static Timer m_FrameStartTimer;

		private static AutoResetEvent m_working_frame_is_done;

		internal static AutoResetEvent m_slave_profile_id_read;

		private static byte m_OnReceive_error = 0;

		internal static Thread m_reset_timer;

		private static volatile bool m_we_are_finishing_a_frame = false;

		internal static LIN.OPMODE m_opmode;

		internal static int FRAME_TIMEOUT = 100;

		internal static double m_interbyte_timeout = 0.01;

		internal static bool m_next_frame_is_first_frame = true;

		internal static bool m_use_baud_rate_timeout = false;

		internal static ushort m_last_master_baud_rate = 0;

		internal static volatile bool m_autobaud_is_on = true;

		internal static LIN.WORKING_FRAME m_working_frame = default(LIN.WORKING_FRAME);

		internal static LIN.FRAMEINFO[] m_Frames = new LIN.FRAMEINFO[256];

		internal static LIN.SLAVE_PROFILE_ID m_slave_profile_id;

		public static event LIN.GUINotifierOR OnReceive;
		/*
		{
			[MethodImpl(32)]
			add
			{
				LIN.OnReceive = (LIN.GUINotifierOR)Delegate.Combine(LIN.OnReceive, value);
			}
			[MethodImpl(32)]
			remove
			{
				LIN.OnReceive = (LIN.GUINotifierOR)Delegate.Remove(LIN.OnReceive, value);
			}
		}
		*/

		public static event LIN.GUINotifierOA OnAnswer;

		/*
		{
			[MethodImpl(32)]
			add
			{
				OnAnswer = (LIN.GUINotifierOA)Delegate.Combine(LIN.OnAnswer, value);
			}
			[MethodImpl(32)]
			remove
			{
				OnAnswer = (LIN.GUINotifierOA)Delegate.Remove(LIN.OnAnswer, value);
			}
		}
		*/
		public static bool Configure_PICkitSerial_For_LIN()
		{
			bool result = false;
			if (Basic.Configure_PICkitSerial(10, true))
			{
				LIN.Get_LIN_BAUD_Rate();
				result = true;
			}
			return result;
		}

		public static bool Configure_PICkitSerial_For_LIN_No_Autobaud()
		{
			bool result = false;
			if (Basic.Configure_PICkitSerial(19, true))
			{
				LIN.Get_LIN_BAUD_Rate();
				result = true;
			}
			return result;
		}

		public static bool Configure_PICkitSerial_For_LIN(bool p_chip_select_hi, bool p_receive_enable, bool p_autobaud)
		{
			bool flag = false;
			if ((Utilities.m_flags.HID_DeviceReady != false) && Basic.Configure_PICkitSerial(10, true))
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
					if (p_chip_select_hi)
					{
						byte[] expr_8D_cp_0 = array2;
						int expr_8D_cp_1 = 23;
						expr_8D_cp_0[expr_8D_cp_1] |= 8;
					}
					else
					{
						byte[] expr_A6_cp_0 = array2;
						int expr_A6_cp_1 = 23;
						expr_A6_cp_0[expr_A6_cp_1] &= 247;
					}
					if (p_receive_enable)
					{
						byte[] expr_C4_cp_0 = array2;
						int expr_C4_cp_1 = 23;
						expr_C4_cp_0[expr_C4_cp_1] |= 64;
					}
					else
					{
						byte[] expr_DE_cp_0 = array2;
						int expr_DE_cp_1 = 23;
						expr_DE_cp_0[expr_DE_cp_1] &= 191;
					}
					if (p_autobaud)
					{
						byte[] expr_FC_cp_0 = array2;
						int expr_FC_cp_1 = 23;
						expr_FC_cp_0[expr_FC_cp_1] |= 128;
					}
					else
					{
						byte[] expr_119_cp_0 = array2;
						int expr_119_cp_1 = 23;
						expr_119_cp_0[expr_119_cp_1] &= 127;
					}
					USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
					flag = USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
					if (flag)
					{
						LIN.Get_LIN_BAUD_Rate();
					}
				}
			}
			return flag;
		}

		public static bool Get_LIN_Options(ref bool p_chip_select_hi, ref bool p_receive_enable, ref bool p_autobaud)
		{
			byte[] array = new byte[65];
			bool result = false;
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				if (Basic.Get_Status_Packet(ref array))
				{
					p_chip_select_hi = ((array[23] & 8) > 0);
					p_receive_enable = ((array[23] & 64) > 0);
					p_autobaud = ((array[23] & 128) > 0);
					result = true;
				}
			}
			return result;
		}

		public static bool Set_LIN_Options(bool p_chip_select_hi, bool p_receive_enable, bool p_autobaud)
		{
			bool result = false;
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
				if (p_chip_select_hi)
				{
					byte[] expr_67_cp_0 = array2;
					int expr_67_cp_1 = 23;
					expr_67_cp_0[expr_67_cp_1] |= 8;
				}
				else
				{
					byte[] expr_80_cp_0 = array2;
					int expr_80_cp_1 = 23;
					expr_80_cp_0[expr_80_cp_1] &= 247;
				}
				if (p_receive_enable)
				{
					byte[] expr_9E_cp_0 = array2;
					int expr_9E_cp_1 = 23;
					expr_9E_cp_0[expr_9E_cp_1] |= 64;
				}
				else
				{
					byte[] expr_B8_cp_0 = array2;
					int expr_B8_cp_1 = 23;
					expr_B8_cp_0[expr_B8_cp_1] &= 191;
				}
				if (p_autobaud)
				{
					byte[] expr_D6_cp_0 = array2;
					int expr_D6_cp_1 = 23;
					expr_D6_cp_0[expr_D6_cp_1] |= 128;
				}
				else
				{
					byte[] expr_F3_cp_0 = array2;
					int expr_F3_cp_1 = 23;
					expr_F3_cp_0[expr_F3_cp_1] &= 127;
				}
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				result = USBWrite.write_and_verify_LIN_config_block(ref array, ref text2, true, ref text);
			}
			return result;
		}

		public static bool Set_OnReceive_Timeout(int Timeout)
		{
			if (Timeout != 65535)
			{
				LIN.m_use_baud_rate_timeout = false;
				LIN.FRAME_TIMEOUT = Timeout;
				return true;
			}
			if (LIN.set_OnReceive_timeout_from_baud_rate())
			{
				LIN.m_use_baud_rate_timeout = true;
				return true;
			}
			Timeout = 0;
			return false;
		}

		public static int Get_OnReceive_Timeout()
		{
			return LIN.FRAME_TIMEOUT;
		}

		public static bool OnReceive_Timeout_Is_Baud_Dependent()
		{
			return LIN.m_use_baud_rate_timeout;
		}

		public static bool SetModeListen()
		{
			LIN.m_opmode = LIN.OPMODE.LISTEN;
			return true;
		}

		public static bool SetModeTransmit()
		{
			LIN.m_opmode = LIN.OPMODE.TRANSMIT;
			return true;
		}

		public static bool SetModeDisplayAll()
		{
			LIN.m_opmode = LIN.OPMODE.DISPLAY_ALL;
			return true;
		}

		public static bool Transmit_mode_Is_Set()
		{
			return LIN.m_opmode == LIN.OPMODE.TRANSMIT;
		}

		public static bool Listen_mode_Is_Set()
		{
			return LIN.m_opmode == LIN.OPMODE.LISTEN;
		}

		public static bool DisplayAll_mode_Is_Set()
		{
			return LIN.m_opmode == LIN.OPMODE.DISPLAY_ALL;
		}

		internal static void initialize_LIN_frames()
		{
			LIN.m_stopwatch = new Stopwatch();
			LIN.m_opmode = LIN.OPMODE.LISTEN;
			LIN.m_working_frame.FrameInfo.FrameData = new byte[9];
			LIN.reset_working_frame();
			LIN.Reset_LIN_Frame_Buffers();
			LIN.m_FrameStartTimer = new Timer(new TimerCallback(LIN.frame_has_timed_out), null, -1, -1);
			LIN.m_working_frame.BuildState.we_have_transmitted = false;
			LIN.m_working_frame.BuildState.transmit_data_byte_count_zero = false;
			LIN.m_working_frame_is_done = new AutoResetEvent(false);
			LIN.m_slave_profile_id_read = new AutoResetEvent(false);
			LIN.m_slave_profile_id.ByteCount = 0;
			LIN.m_slave_profile_id.FrameID = 0;
			LIN.m_slave_profile_id.Data = new byte[255];
		}

		internal static void reset_LIN_timeout()
		{
			LIN.m_reset_timer = new Thread(new ThreadStart(LIN.reset_lin_timer));
			LIN.m_reset_timer.Start();
			LIN.m_reset_timer.Join();
		}

		private static void reset_lin_timer()
		{
			LIN.m_FrameStartTimer.Change(LIN.FRAME_TIMEOUT, -1);
			LIN.m_stopwatch.Reset();
			LIN.m_stopwatch.Start();
		}

		public static void Reset_LIN_Frame_Buffers()
		{
			for (int i = 0; i < LIN.m_Frames.Length; i++)
			{
				LIN.m_Frames[i].FrameID = (byte)i;
				LIN.m_Frames[i].FrameData = new byte[9];
				LIN.m_Frames[i].bytecount = 0;
				LIN.m_Frames[i].baud = 0;
				LIN.m_Frames[i].time = 0.0;
				LIN.m_Frames[i].frame_timeout_time = 0L;
				for (int j = 0; j < LIN.m_Frames[i].FrameData.Length; j++)
				{
					LIN.m_Frames[i].FrameData[j] = 0;
				}
			}
		}

		public static void Reset_Timer()
		{
			LIN.m_next_frame_is_first_frame = true;
		}

		private static void frame_has_timed_out(object state)
		{
			while (LIN.m_stopwatch.ElapsedMilliseconds < (long)LIN.FRAME_TIMEOUT)
			{
				Thread.Sleep(10);
			}
			LIN.m_working_frame.FrameInfo.frame_timeout_time = LIN.m_stopwatch.ElapsedMilliseconds;
			if (!LIN.m_we_are_finishing_a_frame)
			{
				if (LIN.m_working_frame.FrameInfo.FrameID == 0)
				{
					byte arg_5C_0 = USBRead.m_raw_cbuf2_data_array[(int)((UIntPtr)USBRead.m_cb2_array_tag_index)];
				}
				LIN.m_working_frame.BuildState.we_timed_out = true;
				LIN.finish_this_frame();
			}
		}

		internal static void reset_working_frame()
		{
			for (int i = 0; i < LIN.m_working_frame.FrameInfo.FrameData.Length; i++)
			{
				LIN.m_working_frame.FrameInfo.FrameData[i] = 0;
			}
			LIN.m_working_frame.FrameInfo.FrameID = 0;
			LIN.m_working_frame.FrameInfo.bytecount = 0;
			LIN.m_working_frame.FrameInfo.baud = 0;
			LIN.m_working_frame.FrameInfo.time = 0.0;
			LIN.m_working_frame.FrameInfo.frame_timeout_time = 0L;
			LIN.m_working_frame.BuildState.we_had_a_status_error = false;
			LIN.m_working_frame.BuildState.we_are_building_a_frame = false;
			LIN.m_working_frame.BuildState.we_have_an_id = false;
			LIN.m_working_frame.BuildState.we_timed_out = false;
			LIN.m_working_frame.BuildState.next_frame_header_received = false;
			LIN.m_working_frame.BuildState.we_have_transmitted = false;
			LIN.m_working_frame.BuildState.transmit_data_byte_count_zero = false;
		}

		private static bool this_is_a_valid_frame()
		{
			return true;
		}

		private static bool this_frame_is_different_than_last()
		{
			if (LIN.m_Frames[(int)LIN.m_working_frame.FrameInfo.FrameID].bytecount != LIN.m_working_frame.FrameInfo.bytecount)
			{
				return true;
			}
			if (LIN.m_Frames[(int)LIN.m_working_frame.FrameInfo.FrameID].baud != LIN.m_working_frame.FrameInfo.baud)
			{
				return true;
			}
			for (int i = 0; i < LIN.m_working_frame.FrameInfo.FrameData.Length; i++)
			{
				if (LIN.m_Frames[(int)LIN.m_working_frame.FrameInfo.FrameID].FrameData[i] != LIN.m_working_frame.FrameInfo.FrameData[i])
				{
					return true;
				}
			}
			return false;
		}

		private static void copy_this_frame_into_array()
		{
			LIN.m_Frames[(int)LIN.m_working_frame.FrameInfo.FrameID].bytecount = LIN.m_working_frame.FrameInfo.bytecount;
			LIN.m_Frames[(int)LIN.m_working_frame.FrameInfo.FrameID].baud = LIN.m_working_frame.FrameInfo.baud;
			LIN.m_Frames[(int)LIN.m_working_frame.FrameInfo.FrameID].time = LIN.m_working_frame.FrameInfo.time;
			for (int i = 0; i < LIN.m_working_frame.FrameInfo.FrameData.Length; i++)
			{
				LIN.m_Frames[(int)LIN.m_working_frame.FrameInfo.FrameID].FrameData[i] = LIN.m_working_frame.FrameInfo.FrameData[i];
			}
		}

		internal static void finalize_working_frame()
		{
			if (!LIN.m_we_are_finishing_a_frame)
			{
				LIN.finish_this_frame();
			}
		}

		private static void finish_this_frame()
		{
			LIN.m_we_are_finishing_a_frame = true;
			LIN.m_working_frame.BuildState.we_are_building_a_frame = false;
			LIN.m_FrameStartTimer.Change(-1, -1);
			bool flag = false;
			if (LIN.this_is_a_valid_frame())
			{
				if (LIN.m_next_frame_is_first_frame)
				{
					LIN.m_working_frame.FrameInfo.time = 0.0;
					LIN.m_next_frame_is_first_frame = false;
					flag = !USBRead.reset_timer_params();
				}
				if (LIN.m_working_frame.FrameInfo.baud != 0)
				{
					LIN.m_working_frame.FrameInfo.baud = LIN.calculate_baud_rate(LIN.m_working_frame.FrameInfo.baud);
				}
				else
				{
					LIN.m_working_frame.FrameInfo.baud = LIN.m_last_master_baud_rate;
				}
				LIN.m_OnReceive_error = 0;
				uint num = 0u;
				if (Status.There_Is_A_Status_Error(ref num))
				{
					LIN.m_OnReceive_error = 4;
					Device.Clear_Status_Errors();
				}
				if (LIN.m_OnReceive_error == 0 && LIN.m_working_frame.BuildState.we_had_a_status_error)
				{
					LIN.m_OnReceive_error = 5;
				}
				else if (LIN.m_working_frame.BuildState.we_timed_out)
				{
					LIN.m_OnReceive_error = 1;
				}
				else if (flag)
				{
					LIN.m_OnReceive_error = 3;
				}
				else if (LIN.m_working_frame.BuildState.next_frame_header_received)
				{
					LIN.m_OnReceive_error = 6;
				}
				if ((LIN.this_frame_is_different_than_last() && LIN.m_opmode == LIN.OPMODE.LISTEN) || LIN.m_opmode == LIN.OPMODE.DISPLAY_ALL)
				{
					if (LIN.m_working_frame.BuildState.we_have_transmitted && LIN.m_working_frame.BuildState.transmit_data_byte_count_zero)
					{
						OnAnswer(LIN.m_working_frame.FrameInfo.FrameID, LIN.m_working_frame.FrameInfo.FrameData, LIN.m_working_frame.FrameInfo.bytecount, LIN.m_OnReceive_error, LIN.m_working_frame.FrameInfo.baud, LIN.m_working_frame.FrameInfo.time);
					}
					else if ((!LIN.m_working_frame.BuildState.we_have_transmitted || !LIN.m_working_frame.BuildState.transmit_data_byte_count_zero) && LIN.OnReceive != null)
					{
						if (LIN.m_OnReceive_error == 6)
						{
							LIN.m_OnReceive_error = 0;
						}
						OnReceive(LIN.m_working_frame.FrameInfo.FrameID, LIN.m_working_frame.FrameInfo.FrameData, LIN.m_working_frame.FrameInfo.bytecount, LIN.m_OnReceive_error, LIN.m_working_frame.FrameInfo.baud, LIN.m_working_frame.FrameInfo.time);
					}
				}
				LIN.copy_this_frame_into_array();
			}
			LIN.reset_working_frame();
			LIN.m_working_frame_is_done.Set();
			LIN.m_we_are_finishing_a_frame = false;
		}

		internal static void send_on_answer(byte p_id, double p_time, ref byte[] p_data)
		{
			if (OnAnswer != null)
			{
				OnAnswer(p_id, p_data, 9, 0, 0, p_time);
			}
		}

		public static bool Transmit(byte MasterID, byte[] Data, byte DataByteCount, ref string ErrorString)
		{
			bool flag = false;
			byte[] array = new byte[30];
			Array.Clear(array, 0, array.Length);
			if (DataByteCount > 9)
			{
				ErrorString = "DataByteCount cannot exceed 9.";
				return flag;
			}
			array[0] = 0;
			array[1] = 3;
			array[2] = (byte)(DataByteCount + 5);
			array[3] = 132;
			array[4] = (byte)(DataByteCount + 1);
			array[5] = MasterID;
			array[6] = 31;
			array[7] = 119;
			array[8] = 0;
			if (DataByteCount == 0)
			{
				LIN.m_working_frame.BuildState.transmit_data_byte_count_zero = true;
				LIN.m_working_frame.BuildState.we_have_transmitted = true;
				LIN.m_working_frame_is_done.Reset();
				flag = USBWrite.Send_Script_To_PICkitS(ref array);
				if (!flag)
				{
					ErrorString = "Error sending script.";
					return false;
				}
				if (!LIN.m_working_frame_is_done.WaitOne(6000, false))
				{
					ErrorString = "No data returned";
					return false;
				}
			}
			else
			{
				int i;
				for (i = 0; i < (int)DataByteCount; i++)
				{
					array[i + 6] = Data[i];
				}
				array[i + 6] = 31;
				array[i + 7] = 119;
				array[i + 8] = 0;
				LIN.m_working_frame.BuildState.transmit_data_byte_count_zero = false;
				LIN.m_working_frame.BuildState.we_have_transmitted = true;
				LIN.m_working_frame_is_done.Reset();
				flag = USBWrite.Send_Script_To_PICkitS(ref array);
				if (!flag)
				{
					ErrorString = "Error sending script.";
					return false;
				}
			}
			return flag;
		}

		public static bool Change_LIN_BAUD_Rate(ushort Baud)
		{
			bool flag = false;
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
				int num = (int)Math.Round(20000000.0 / (double)Baud / 4.0) - 1;
				array2[29] = (byte)num;
				array2[30] = (byte)(num >> 8);
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				flag = USBWrite.write_and_verify_LIN_config_block(ref array, ref text2, true, ref text);
			}
			if (flag)
			{
				LIN.Get_LIN_BAUD_Rate();
			}
			return flag;
		}

		public static ushort Get_LIN_BAUD_Rate()
		{
			byte[] array = new byte[65];
			ushort num = 0;
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				if (Basic.Get_Status_Packet(ref array))
				{
					num = LIN.calculate_baud_rate((ushort)((int)array[50] + ((int)array[51] << 8)));
					LIN.m_last_master_baud_rate = num;
				}
			}
			return num;
		}

		private static bool set_OnReceive_timeout_from_baud_rate()
		{
			byte[] array = new byte[65];
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				if (!Basic.Get_Status_Packet(ref array))
				{
					return false;
				}
				int p_baud = (int)LIN.calculate_baud_rate((ushort)((int)array[50] + ((int)array[51] << 8)));
				LIN.calculate_new_baud_dependent_onreceive_timeout(p_baud);
			}
			return true;
		}

		internal static void calculate_new_baud_dependent_onreceive_timeout(int p_baud)
		{
			double num = 11.0 / (double)p_baud * 10.0 * 1.5;
			LIN.FRAME_TIMEOUT = (int)(num * 1000.0);
		}

		private static ushort calculate_baud_rate(ushort p_baud)
		{
			double num = 20.0 / (4.0 * ((double)p_baud + 1.0));
			int num2 = (int)Math.Round(num * 1000000.0);
			return (ushort)num2;
		}

		private static bool Toggle_AutoBaud_Set(bool p_turn_autobaudset_on, ref ushort p_baud, ref string p_error_detail)
		{
			bool flag = false;
			int num = 0;
			string text = "";
			string text2 = "";
			byte[] array = new byte[65];
			byte[] array2 = new byte[65];
			p_error_detail = "";
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				Array.Clear(array2, 0, array2.Length);
				if (!Basic.Get_Status_Packet(ref array2))
				{
					p_error_detail = "Could not poll PKSA for status.";
					return false;
				}
				p_baud = LIN.calculate_baud_rate((ushort)((int)array2[29] + ((int)array2[30] << 8)));
				if (p_turn_autobaudset_on)
				{
					byte[] expr_8F_cp_0 = array2;
					int expr_8F_cp_1 = 23;
					expr_8F_cp_0[expr_8F_cp_1] |= 128;
				}
				else
				{
					byte[] expr_AC_cp_0 = array2;
					int expr_AC_cp_1 = 23;
					expr_AC_cp_0[expr_AC_cp_1] &= 127;
				}
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				while (!flag && num < 3)
				{
					num++;
					flag = USBWrite.write_and_verify_LIN_config_block(ref array, ref text2, false, ref text);
					p_error_detail += text2;
				}
			}
			return flag;
		}

		public static bool Configure_PICkitSerial_For_LINSlave_Mode(byte p_array_byte_count, ref byte[] p_profile_array, ref string p_result_str, bool p_autobaud, ref int p_error_code)
		{
			bool flag = false;
			byte[] array = new byte[65];
			byte[] array2 = new byte[255];
			byte[] array3 = new byte[65];
			bool result = false;
			byte b = 0;
			byte b2 = 0;
			string text = "";
			p_error_code = 0;
			Array.Clear(array, 0, array.Length);
			Array.Clear(array3, 0, array3.Length);
			byte b3 = LIN.Number_Of_Bytes_In_CBUF3(ref b, ref b2);
			if (p_array_byte_count > b3)
			{
				p_result_str = string.Format("Byte count of {0} greater than allowed value of {1}.", p_array_byte_count, b3);
				p_error_code = 1;
				return flag;
			}
			USBWrite.Clear_CBUF(3);
			if (p_autobaud)
			{
				Mode.update_status_packet_data(10, ref array3);
			}
			else
			{
				Mode.update_status_packet_data(19, ref array3);
			}
			USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array3);
			USBWrite.Send_Cold_Reset_Cmd();
			flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			USBWrite.Send_Warm_Reset_Cmd();
			Array.Clear(array2, 0, array2.Length);
			array2[0] = 0;
			array2[1] = 5;
			array2[2] = p_array_byte_count;
			for (int i = 3; i < (int)(p_array_byte_count + 3); i++)
			{
				array2[i] = p_profile_array[i - 3];
			}
			USBWrite.Send_Script_To_PICkitS(ref array2);
			byte[] expr_F9_cp_0 = array3;
			int expr_F9_cp_1 = 23;
			expr_F9_cp_0[expr_F9_cp_1] |= 32;
			USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array3);
			flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
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
							p_error_code = 3;
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
				p_error_code = 2;
				p_result_str = string.Format("Error sending config packet - Config Block may not be updated correctly", new object[0]);
			}
			return result;
		}

		public static bool Add_LIN_Slave_Profile_To_PKS(byte p_array_byte_count, ref byte[] p_profile_array, ref string p_result_str, ref int p_error_code)
		{
			bool flag = false;
			byte[] array = new byte[65];
			byte[] array2 = new byte[255];
			byte[] array3 = new byte[65];
			bool result = false;
			byte b = 0;
			byte b2 = 0;
			string text = "";
			p_error_code = 0;
			Array.Clear(array, 0, array.Length);
			Array.Clear(array3, 0, array3.Length);
			byte b3 = LIN.Number_Of_Bytes_In_CBUF3(ref b, ref b2);
			if (p_array_byte_count > b3)
			{
				p_result_str = string.Format("Byte count of {0} greater than allowed value of {1}.", p_array_byte_count, b3);
				p_error_code = 1;
				return flag;
			}
			USBWrite.Clear_CBUF(3);
			if (!Basic.Get_Status_Packet(ref array3))
			{
				p_result_str = string.Format("Error reading status packet.", new object[0]);
				p_error_code = 2;
				return false;
			}
			byte[] expr_A6_cp_0 = array3;
			int expr_A6_cp_1 = 23;
			expr_A6_cp_0[expr_A6_cp_1] |= 32;
			USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array3);
			flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			Array.Clear(array2, 0, array2.Length);
			array2[0] = 0;
			array2[1] = 5;
			array2[2] = p_array_byte_count;
			for (int i = 3; i < (int)(p_array_byte_count + 3); i++)
			{
				array2[i] = p_profile_array[i - 3];
			}
			bool flag2 = USBWrite.Send_Script_To_PICkitS(ref array2);
			if (flag & flag2)
			{
				if (USBWrite.Update_Status_Packet())
				{
					Utilities.m_flags.g_status_packet_mutex.WaitOne();
					int j;
					for (j = 7; j < 31; j++)
					{
						if (Constants.STATUS_PACKET_DATA[j] != array[j - 5])
						{
							p_error_code = 3;
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
				p_error_code = 2;
				p_result_str = string.Format("Error sending config packet - Config Block may not be updated correctly", new object[0]);
			}
			return result;
		}

		public static byte Number_Of_Bytes_In_CBUF3(ref byte p_used_bytes, ref byte p_unused_bytes)
		{
			byte result = 0;
			if (USBWrite.Update_Special_Status_Packet())
			{
				Utilities.m_flags.g_status_packet_mutex.WaitOne();
				result =(byte)( Constants.STATUS_PACKET_DATA[57] + Constants.STATUS_PACKET_DATA[58]);
				p_used_bytes = Constants.STATUS_PACKET_DATA[57];
				p_unused_bytes = Constants.STATUS_PACKET_DATA[58];
				Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
			}
			return result;
		}

		public static bool Read_Slave_Profile(byte p_masterid, ref byte[] p_data, byte p_expected_byte_count, ref byte p_actual_byte_count, ref byte p_error_code)
		{
			bool result = false;
			p_error_code = 0;
			byte[] array = new byte[30];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 4;
			array[3] = 135;
			array[4] = p_masterid;
			array[5] = 31;
			array[6] = 119;
			array[7] = 0;
			Array.Clear(LIN.m_slave_profile_id.Data, 0, LIN.m_slave_profile_id.Data.Length);
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = LIN.m_slave_profile_id_read.WaitOne(2000, false);
				if (flag2)
				{
					if (p_masterid == LIN.m_slave_profile_id.FrameID)
					{
						p_actual_byte_count = LIN.m_slave_profile_id.ByteCount;
						if (p_expected_byte_count >= LIN.m_slave_profile_id.ByteCount)
						{
							for (int i = 0; i < (int)LIN.m_slave_profile_id.ByteCount; i++)
							{
								p_data[i] = LIN.m_slave_profile_id.Data[i];
							}
							result = true;
						}
						else
						{
							p_error_code = 3;
						}
					}
					else
					{
						p_error_code = 4;
					}
				}
				else
				{
					p_error_code = 1;
				}
			}
			else
			{
				p_error_code = 2;
			}
			return result;
		}

		public static bool Write_Slave_Profile(byte p_masterid, ref byte[] p_data, byte p_byte_count, ref byte p_error_code)
		{
			bool result = false;
			p_error_code = 0;
			byte[] array = new byte[255];
			byte[] array2 = new byte[255];
			byte b = 0;
			byte b2 = 0;
			if (p_byte_count > 244)
			{
				p_error_code = 3;
				return false;
			}
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = (byte)(p_byte_count + 5);
			array[3] = 134;
			array[4] = p_masterid;
			array[5] = p_byte_count;
			byte b3;
			for (b3 = 0; b3 < p_byte_count; b3 += 1)
			{
				array[(int)(6 + b3)] = p_data[(int)b3];
			}
			array[(int)(b3 + 6)] = 31;
			array[(int)(b3 + 7)] = 119;
			array[(int)(b3 + 8)] = 0;
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			if (flag)
			{
				if (!LIN.Read_Slave_Profile(p_masterid, ref array2, p_byte_count, ref b2, ref b))
				{
					p_error_code = 1;
				}
				else
				{
					for (b3 = 0; b3 < p_byte_count; b3 += 1)
					{
						if (array2[(int)b3] != p_data[(int)b3])
						{
							p_error_code = 1;
							return false;
						}
					}
					result = true;
				}
			}
			else
			{
				p_error_code = 2;
			}
			return result;
		}
	}
}
