using System;
using System.Threading;

namespace PICkitS
{
	public class Basic
	{
		private static Mutex m_reset_cb = new Mutex(false);

		private static Thread m_flash_led;

		private static Thread m_reset_control_block;

		internal static int m_i2cs_read_wait_time = 200;

		internal static int m_i2cs_receive_wait_time = 200;

		internal static int m_spi_receive_wait_time = 200;

		public static uint Retrieve_USART_Data_Byte_Count()
		{
			return USBRead.Retrieve_Data_Byte_Count();
		}

		public static bool Retrieve_USART_Data(uint p_byte_count, ref byte[] p_data_array)
		{
			bool result = false;
			if (USBRead.Retrieve_Data(ref p_data_array, p_byte_count))
			{
				result = true;
			}
			return result;
		}

		public static bool Initialize_PICkitSerial()
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr zero2 = IntPtr.Zero;
			Guid empty = Guid.Empty;
			string text = "";
			ushort p_PoductID = 54;
			ushort p_VendorID = 1240;
			ushort num = 0;
			bool flag = USB.Get_This_Device(p_VendorID, p_PoductID, 0, ref zero, ref zero2, ref text, false, ref empty, ref num);
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

		public static bool Initialize_PICkitSerial(ushort USBIndex)
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr zero2 = IntPtr.Zero;
			Guid empty = Guid.Empty;
			string text = "";
			ushort p_PoductID = 54;
			ushort p_VendorID = 1240;
			ushort num = 0;
			bool flag = USB.Get_This_Device(p_VendorID, p_PoductID, USBIndex, ref zero, ref zero2, ref text, false, ref empty, ref num);
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

		public static bool Initialize_MyDevice(ushort USBIndex, ushort ProductID)
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr zero2 = IntPtr.Zero;
			Guid empty = Guid.Empty;
			string text = "";
			ushort p_VendorID = 1240;
			ushort num = 0;
			bool flag = USB.Get_This_Device(p_VendorID, ProductID, USBIndex, ref zero, ref zero2, ref text, false, ref empty, ref num);
			if (flag)
			{
				flag = USBRead.Kick_Off_Read_Thread();
				if (flag)
				{
					flag = USBWrite.kick_off_write_thread();
					if (ProductID == 80)
					{
						Utilities.g_comm_mode = Utilities.COMM_MODE.MTOUCH2;
					}
				}
			}
			return flag;
		}

		public static ushort How_Many_PICkitSerials_Are_Attached()
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr zero2 = IntPtr.Zero;
			Guid empty = Guid.Empty;
			string text = "";
			ushort p_PoductID = 54;
			ushort p_VendorID = 1240;
			return USB.Count_Attached_PKSA(p_VendorID, p_PoductID, 29, ref zero, ref zero2, ref text, false, ref empty);
		}

		public static ushort How_Many_Of_MyDevices_Are_Attached(ushort ProductID)
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr zero2 = IntPtr.Zero;
			Guid empty = Guid.Empty;
			string text = "";
			ushort p_VendorID = 1240;
			return USB.Count_Attached_PKSA(p_VendorID, ProductID, 29, ref zero, ref zero2, ref text, false, ref empty);
		}

		public static void Terminate_Comm_Threads()
		{
			USBRead.Kill_Read_Thread();
			USBWrite.Kill_Write_Thread();
		}

		public static bool ReEstablish_Comm_Threads()
		{
			return USBRead.Kick_Off_Read_Thread() && USBWrite.kick_off_write_thread();
		}

		public static bool Configure_PICkitSerial_For_I2C()
		{
			return Basic.Configure_PICkitSerial(1, true);
		}

		public static bool Configure_PICkitSerial_For_LIN()
		{
			return Basic.Configure_PICkitSerial(10, true);
		}

		public static bool Configure_PICkitSerial(int p_mode, bool p_reset)
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
				Mode.update_status_packet_data(p_mode, ref array2);
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				result = USBWrite.write_and_verify_config_block(ref array, ref text2, p_reset, ref text);
			}
			return result;
		}

		public static void Cleanup()
		{
			if (USBRead.Read_Thread_Is_Active())
			{
				USBRead.Kill_Read_Thread();
				Thread.Sleep(500);
			}
			USBWrite.Kill_Write_Thread();
			USBWrite.Dispose_Of_Write_Objects();
			//Utilities.CloseHandle(Utilities.m_flags.HID_write_handle);
			//Utilities.CloseHandle(Utilities.m_flags.HID_read_handle);
			Utilities.m_flags.g_status_packet_data_update_event.Close();
			Utilities.m_flags.g_data_arrived_event.Close();
			Utilities.m_flags.g_bl_data_arrived_event.Close();
			Utilities.m_flags.g_status_packet_mutex.Close();
			Utilities.m_flags.g_PKSA_has_completed_script.Close();
			USBRead.m_usb_packet_mutex.Close();
			USBRead.m_cbuf2_data_array_mutex.Close();
		}

		public static bool Send_I2CWrite_Cmd(byte p_slave_addr, byte p_start_data_addr, byte p_num_bytes_to_write, ref byte[] p_data_array, ref string p_script_view)
		{
			if (p_num_bytes_to_write > 253)
			{
				return false;
			}
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = (byte)(8 + p_num_bytes_to_write);
			array[3] = 129;
			array[4] = 132;
			array[5] = (byte)(2 + p_num_bytes_to_write);
			array[6] = p_slave_addr;
			array[7] = p_start_data_addr;
			int i;
			for (i = 0; i < (int)p_num_bytes_to_write; i++)
			{
				array[i + 8] = p_data_array[i];
			}
			array[i + 8] = 130;
			array[i + 9] = 31;
			array[i + 10] = 119;
			array[i + 11] = 0;
			p_script_view = "[S_][W_]";
			string text = string.Format("[{0:X2}]", array[5]);
			p_script_view += text;
			text = string.Format("[{0:X2}]", array[6]);
			p_script_view += text;
			text = string.Format("[{0:X2}]", array[7]);
			p_script_view += text;
			for (i = 0; i < (int)p_num_bytes_to_write; i++)
			{
				text = string.Format("[{0:X2}]", array[i + 8]);
				p_script_view += text;
			}
			p_script_view += "[P_]";
			USBRead.Clear_Data_Array(0u);
			USBRead.Clear_Raw_Data_Array();
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Send_I2CWrite_Word_Cmd(byte p_slave_addr, byte p_command1, byte p_command2, byte p_num_bytes_to_write, ref byte[] p_data_array, ref string p_script_view)
		{
			if (p_num_bytes_to_write > 253)
			{
				return false;
			}
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = (byte)(9 + p_num_bytes_to_write);
			array[3] = 129;
			array[4] = 132;
			array[5] = (byte)(3 + p_num_bytes_to_write);
			array[6] = p_slave_addr;
			array[7] = p_command1;
			array[8] = p_command2;
			int i;
			for (i = 0; i < (int)p_num_bytes_to_write; i++)
			{
				array[i + 9] = p_data_array[i];
			}
			array[i + 9] = 130;
			array[i + 10] = 31;
			array[i + 11] = 119;
			array[i + 12] = 0;
			p_script_view = "[S_][W_]";
			string text = string.Format("[{0:X2}]", array[5]);
			p_script_view += text;
			text = string.Format("[{0:X2}]", array[6]);
			p_script_view += text;
			text = string.Format("[{0:X2}]", array[7]);
			p_script_view += text;
			text = string.Format("[{0:X2}]", array[8]);
			p_script_view += text;
			for (i = 0; i < (int)p_num_bytes_to_write; i++)
			{
				text = string.Format("[{0:X2}]", array[i + 9]);
				p_script_view += text;
			}
			p_script_view += "[P_]";
			USBRead.Clear_Data_Array(0u);
			USBRead.Clear_Raw_Data_Array();
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Send_SPI_Send_Cmd(byte p_byte_count, ref byte[] p_data, bool p_first_cmd, bool p_last_cmd, ref string p_script_view)
		{
			byte[] array = new byte[255];
			int num = 3;
			if (p_byte_count > 246)
			{
				return false;
			}
			p_script_view = "";
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			if (p_first_cmd)
			{
				array[num++] = 139;
				p_script_view = "[CSON]";
			}
			array[num++] = 133;
			array[num++] = p_byte_count;
			p_script_view += "[DO]";
			string text = string.Format("[{0:X2}]", p_byte_count);
			p_script_view += text;
			for (int i = 0; i < (int)p_byte_count; i++)
			{
				array[num++] = p_data[i];
				text = string.Format("[{0:X2}]", p_data[i]);
				p_script_view += text;
			}
			if (p_last_cmd)
			{
				array[num++] = 140;
				p_script_view += "[CSOF]";
			}
			array[2] = (byte)(num - 1);
			array[num++] = 31;
			array[num++] = 119;
			array[num] = 0;
			USBRead.Clear_Data_Array(0u);
			USBRead.Clear_Raw_Data_Array();
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Send_USART_Cmd(byte p_byte_count, ref byte[] p_data, ref string p_script_view)
		{
			byte[] array = new byte[255];
			int num = 5;
			p_script_view = "";
			if (p_byte_count > 247)
			{
				return false;
			}
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = (byte)(p_byte_count + 4);
			array[3] = 130;
			array[4] = p_byte_count;
			string text = string.Format("[{0:X2}]", p_byte_count);
			for (int i = 0; i < (int)p_byte_count; i++)
			{
				array[num++] = p_data[i];
				text = string.Format("[{0:X2}]", p_data[i]);
				p_script_view += text;
			}
			array[num++] = 31;
			array[num++] = 119;
			array[num] = 0;
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Send_I2CRead_Cmd(byte p_slave_addr, byte p_start_data_addr, byte p_num_bytes_to_read, ref byte[] p_data_array, ref string p_script_view)
		{
			if (p_num_bytes_to_read == 0)
			{
				return false;
			}
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 14;
			array[3] = 129;
			array[4] = 132;
			array[5] = 2;
			array[6] = p_slave_addr;
			array[7] = p_start_data_addr;
			array[8] = 131;
			array[9] = 132;
			array[10] = 1;
			array[11] = (byte)(p_slave_addr + 1);
			array[12] = 137;
			array[13] = p_num_bytes_to_read;
			array[14] = 130;
			array[15] = 31;
			array[16] = 119;
			array[17] = 0;
			p_script_view = "[S_][W_][02]";
			string text = string.Format("[{0:X2}]", array[6]);
			p_script_view += text;
			text = string.Format("[{0:X2}]", array[7]);
			p_script_view += text;
			p_script_view += "[RS][W_][01]";
			text = string.Format("[{0:X2}]", array[11]);
			p_script_view += text;
			p_script_view += "[RN]";
			text = string.Format("[{0:X2}]", array[13]);
			p_script_view += text;
			p_script_view += "[P_]";
			USBRead.Clear_Data_Array((uint)p_num_bytes_to_read);
			USBRead.Clear_Raw_Data_Array();
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			bool result;
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_data_arrived_event.WaitOne(Basic.m_i2cs_read_wait_time, false);
				result = (flag2 && USBRead.Retrieve_Data(ref p_data_array, (uint)p_num_bytes_to_read));
			}
			else
			{
				result = false;
			}
			return result;
		}

		public static bool Send_I2CRead_Word_Cmd(byte p_slave_addr, byte p_command1, byte p_command2, byte p_num_bytes_to_read, ref byte[] p_data_array, ref string p_script_view)
		{
			if (p_num_bytes_to_read == 0)
			{
				return false;
			}
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 15;
			array[3] = 129;
			array[4] = 132;
			array[5] = 3;
			array[6] = p_slave_addr;
			array[7] = p_command1;
			array[8] = p_command2;
			array[9] = 131;
			array[10] = 132;
			array[11] = 1;
			array[12] = (byte)(p_slave_addr + 1);
			array[13] = 137;
			array[14] = p_num_bytes_to_read;
			array[15] = 130;
			array[16] = 31;
			array[17] = 119;
			array[18] = 0;
			p_script_view = "[S_][W_][03]";
			string text = string.Format("[{0:X2}]", array[6]);
			p_script_view += text;
			text = string.Format("[{0:X2}]", array[7]);
			p_script_view += text;
			text = string.Format("[{0:X2}]", array[8]);
			p_script_view += text;
			p_script_view += "[RS][W_][01]";
			text = string.Format("[{0:X2}]", array[12]);
			p_script_view += text;
			p_script_view += "[RN]";
			text = string.Format("[{0:X2}]", array[14]);
			p_script_view += text;
			p_script_view += "[P_]";
			USBRead.Clear_Data_Array((uint)p_num_bytes_to_read);
			USBRead.Clear_Raw_Data_Array();
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			bool result;
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_data_arrived_event.WaitOne(Basic.m_i2cs_read_wait_time, false);
				result = (flag2 && USBRead.Retrieve_Data(ref p_data_array, (uint)p_num_bytes_to_read));
			}
			else
			{
				result = false;
			}
			return result;
		}

		public static bool Send_I2C_SimpleRead_Cmd(byte p_slave_addr, byte p_num_bytes_to_read, ref byte[] p_data_array, ref string p_script_view)
		{
			bool result = false;
			if (p_num_bytes_to_read == 0)
			{
				return false;
			}
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 9;
			array[3] = 129;
			array[4] = 132;
			array[5] = 1;
			array[6] = p_slave_addr;
			array[7] = 137;
			array[8] = p_num_bytes_to_read;
			array[9] = 130;
			array[10] = 31;
			array[11] = 119;
			array[12] = 0;
			p_script_view = "[S_][W_][01]";
			string text = string.Format("[{0:X2}]", array[6]);
			p_script_view += text;
			p_script_view += "[RN]";
			text = string.Format("[{0:X2}]", array[8]);
			p_script_view += text;
			p_script_view += "[P_]";
			USBRead.Clear_Data_Array((uint)p_num_bytes_to_read);
			USBRead.Clear_Raw_Data_Array();
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_data_arrived_event.WaitOne(Basic.m_i2cs_receive_wait_time, false);
				if (flag2 && USBRead.Retrieve_Data(ref p_data_array, (uint)p_num_bytes_to_read))
				{
					result = true;
				}
			}
			return result;
		}

		public static bool Send_SPI_Receive_Cmd(byte p_num_bytes_to_read, ref byte[] p_data_array, bool p_first_cmd, bool p_last_cmd, ref string p_script_view)
		{
			bool result = false;
			byte[] array = new byte[255];
			int num = 3;
			p_script_view = "";
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			if (p_first_cmd)
			{
				array[num++] = 139;
				p_script_view = "[CSON]";
			}
			array[num++] = 132;
			array[num++] = p_num_bytes_to_read;
			p_script_view += "[DI]";
			string text = string.Format("[{0:X2}]", p_num_bytes_to_read);
			p_script_view += text;
			if (p_last_cmd)
			{
				array[num++] = 140;
				p_script_view += "[CSOF]";
			}
			array[2] = (byte)(num - 1);
			array[num++] = 31;
			array[num++] = 119;
			array[num] = 0;
			USBRead.Clear_Data_Array((uint)p_num_bytes_to_read);
			USBRead.Clear_Raw_Data_Array();
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_data_arrived_event.WaitOne(Basic.m_spi_receive_wait_time, false);
				if (flag2 && USBRead.Retrieve_Data(ref p_data_array, (uint)p_num_bytes_to_read))
				{
					result = true;
				}
			}
			return result;
		}

		public static bool There_Is_A_Status_Error(ref uint p_error)
		{
			return Status.There_Is_A_Status_Error(ref p_error);
		}

		public static int Get_Script_Timeout()
		{
			return USBWrite.m_universal_timeout;
		}

		public static void Set_Script_Timeout(int p_time)
		{
			USBWrite.m_universal_timeout = p_time;
		}

		public static bool Get_Status_Packet(ref byte[] p_status_packet)
		{
			bool result = false;
			if (USBWrite.Update_Status_Packet())
			{
				Utilities.m_flags.g_status_packet_mutex.WaitOne();
				for (int i = 0; i < Constants.STATUS_PACKET_DATA.Length; i++)
				{
					p_status_packet[i] = Constants.STATUS_PACKET_DATA[i];
				}
				Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
				result = true;
			}
			return result;
		}

		public static void Reset_Control_Block()
		{
			Basic.m_reset_control_block = new Thread(new ThreadStart(Basic.Reset_Control_Block_Thread));
			//Basic.m_reset_control_block.set_IsBackground(true);
			Basic.m_reset_control_block.IsBackground = true;
			Basic.m_reset_control_block.Start();
		}

		private static void Reset_Control_Block_Thread()
		{
			Basic.m_reset_cb.WaitOne();
			string text = "";
			byte[] array = new byte[65];
			byte[] array2 = new byte[65];
			string text2 = "";
			Array.Clear(array, 0, array.Length);
			Array.Clear(array2, 0, array2.Length);
			I2CS.reset_buffers();
			if (USBWrite.Update_Status_Packet())
			{
				Utilities.m_flags.g_status_packet_mutex.WaitOne();
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = Constants.STATUS_PACKET_DATA[i];
				}
				Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
				USBWrite.configure_outbound_control_block_packet(ref array2, ref text, ref array);
				USBWrite.write_and_verify_config_block(ref array2, ref text, true, ref text2);
			}
			Basic.m_reset_cb.ReleaseMutex();
		}

		public static void Flash_LED1_For_2_Seconds()
		{
			Basic.m_flash_led = new Thread(new ThreadStart(Basic.flash_the_busy_led));
			//Basic.m_flash_led.set_IsBackground(true);
			Basic.m_flash_led.IsBackground=true;
			Basic.m_flash_led.Start();
		}

		private static void flash_the_busy_led()
		{
			byte p_value = 193;
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
					return;
				}
				byte[] expr_66_cp_0 = array2;
				int expr_66_cp_1 = 7;
				expr_66_cp_0[expr_66_cp_1] |= 32;
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
				if (USBWrite.Send_LED_State_Cmd(1, p_value))
				{
					byte[] expr_9E_cp_0 = array2;
					int expr_9E_cp_1 = 7;
					expr_9E_cp_0[expr_9E_cp_1] &= 223;
					Thread.Sleep(2000);
					USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
					USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
				}
			}
		}
	}
}
