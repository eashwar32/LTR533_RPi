using System;
using System.Diagnostics;
using System.IO;

namespace PICkitS
{
	public class Device
	{
		public static bool Initialize_PICkitSerial()
		{
			return Basic.Initialize_PICkitSerial();
		}

		public static bool Initialize_PICkitSerial(ushort USBIndex)
		{
			return Basic.Initialize_PICkitSerial(USBIndex);
		}

		public static bool Initialize_MyDevice(ushort USBIndex, ushort ProductID)
		{
			return Basic.Initialize_MyDevice(USBIndex, ProductID);
		}

		public static bool Find_ThisDevice(ushort VendorID, ushort ProductID)
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr zero2 = IntPtr.Zero;
			Guid empty = Guid.Empty;
			string text = "";
			ushort num = 0;
			bool flag = USB.Get_This_Device(VendorID, ProductID, 0, ref zero, ref zero2, ref text, false, ref empty, ref num);
			if (flag)
			{
				flag = USBRead.Kick_Off_Read_Thread();
				if (flag)
				{
					flag = USBWrite.kick_off_write_thread();
				}
			}
			return flag;
		}

		public static ushort How_Many_PICkitSerials_Are_Attached()
		{
			return Basic.How_Many_PICkitSerials_Are_Attached();
		}

		public static ushort How_Many_Of_MyDevices_Are_Attached(ushort ProductID)
		{
			return Basic.How_Many_Of_MyDevices_Are_Attached(ProductID);
		}

		public static void Terminate_Comm_Threads()
		{
			Basic.Terminate_Comm_Threads();
		}

		public static bool ReEstablish_Comm_Threads()
		{
			return Basic.ReEstablish_Comm_Threads();
		}

		public static void Cleanup()
		{
			Basic.Cleanup();
		}

		public static bool There_Is_A_Status_Error(ref uint p_error)
		{
			return Basic.There_Is_A_Status_Error(ref p_error);
		}

		public static int Get_Script_Timeout()
		{
			return Basic.Get_Script_Timeout();
		}

		public static void Set_Script_Timeout(int p_time)
		{
			Basic.Set_Script_Timeout(p_time);
		}

		public static bool Get_Status_Packet(ref byte[] p_status_packet)
		{
			return Basic.Get_Status_Packet(ref p_status_packet);
		}

		public static void Reset_Control_Block()
		{
			Basic.Reset_Control_Block();
		}

		public static void Clear_Status_Errors()
		{
			USBWrite.Clear_Status_Errors();
		}

		public static bool Clear_Comm_Errors()
		{
			bool result = false;
			I2CS.reset_buffers();
			if (USBWrite.Send_CommReset_Cmd() && USBWrite.Send_Warm_Reset_Cmd())
			{
				result = true;
			}
			return result;
		}

		public static void Flash_LED1_For_2_Seconds()
		{
			Basic.Flash_LED1_For_2_Seconds();
		}

		public static bool Set_Buffer_Flush_Parameters(bool p_flush_on_count, bool p_flush_on_time, byte p_flush_byte_count, double p_flush_interval)
		{
			bool result = false;
			string text = "";
			string text2 = "";
			byte[] array = new byte[65];
			byte[] array2 = new byte[65];
			if (Utilities.m_flags.HID_DeviceReady != false)//(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				Array.Clear(array2, 0, array2.Length);
				if (!Basic.Get_Status_Packet(ref array2))
				{
					return false;
				}
				if (p_flush_on_count)
				{
					byte[] expr_66_cp_0 = array2;
					int expr_66_cp_1 = 7;
					expr_66_cp_0[expr_66_cp_1] |= 64;
				}
				else
				{
					byte[] expr_7F_cp_0 = array2;
					int expr_7F_cp_1 = 7;
					expr_7F_cp_0[expr_7F_cp_1] &= 191;
				}
				if (p_flush_on_time)
				{
					byte[] expr_9C_cp_0 = array2;
					int expr_9C_cp_1 = 7;
					expr_9C_cp_0[expr_9C_cp_1] |= 128;
				}
				else
				{
					byte[] expr_B8_cp_0 = array2;
					int expr_B8_cp_1 = 7;
					expr_B8_cp_0[expr_B8_cp_1] &= 127;
				}
				array2[10] = p_flush_byte_count;
				byte b = (byte)Math.Round(p_flush_interval / 0.409);
				array2[11] = b;
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				result = USBWrite.write_and_verify_config_block(ref array, ref text2, false, ref text);
			}
			return result;
		}

		public static bool Get_Buffer_Flush_Parameters(ref bool p_flush_on_count, ref bool p_flush_on_time, ref byte p_flush_byte_count, ref double p_flush_interval)
		{
			bool result = false;
			byte[] array = new byte[65];
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				if (Basic.Get_Status_Packet(ref array))
				{
					p_flush_on_count = ((array[7] & 64) > 0);
					p_flush_on_time = ((array[7] & 128) > 0);
					p_flush_byte_count = array[10];
					p_flush_interval = (double)array[11] * 0.409;
					if (p_flush_interval == 0.0)
					{
						p_flush_interval = 0.409;
					}
					result = true;
				}
			}
			return result;
		}

		public static bool Set_Buffer_Flush_Time(double p_time)
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
				byte b = (byte)Math.Round(p_time / 0.409);
				array2[11] = b;
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				result = USBWrite.write_and_verify_config_block(ref array, ref text2, false, ref text);
			}
			return result;
		}

		public static double Get_Buffer_Flush_Time()
		{
			double num = 9999.0;
			byte[] array = new byte[65];
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				if (!Basic.Get_Status_Packet(ref array))
				{
					return 9999.0;
				}
				num = (double)array[11] * 0.409;
				if (num == 0.0)
				{
					num = 0.409;
				}
			}
			return num;
		}

		public static void Set_Script_Timeout_Option(bool p_use_timeout)
		{
			USBWrite.m_use_script_timeout = p_use_timeout;
		}

		public static bool Get_Script_Timeout_Option()
		{
			return USBWrite.m_use_script_timeout;
		}

		public static bool Get_PKSA_FW_Version(ref ushort p_version, ref string p_str_fw_ver)
		{
			bool result = false;
			byte[] array = new byte[65];
			if (Basic.Get_Status_Packet(ref array))
			{
				p_version = (ushort)(((int)array[4] << 8) + (int)array[3]);
				p_str_fw_ver = string.Format("0x{0:X4}", p_version);
				result = true;
			}
			return result;
		}

		public static bool Get_PickitS_DLL_Version(ref string p_version)
		{
			bool result = false;
			string text = Directory.GetCurrentDirectory();
			text += "\\PICkitS.dll";
			if (File.Exists(text))
			{
				result = true;
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(text);
				p_version = versionInfo.FileVersion;
			}
			return result;
		}
	}
}
