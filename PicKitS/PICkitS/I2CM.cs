using System;

namespace PICkitS
{
	public class I2CM
	{
		public static bool Configure_PICkitSerial_For_I2CMaster()
		{
			return Basic.Configure_PICkitSerial(1, true);
		}

		public static bool Configure_PICkitSerial_For_I2CMaster(bool p_aux1_def, bool p_aux2_def, bool p_aux1_dir, bool p_aux2_dir, bool p_enable_pu, double p_voltage)
		{
			bool result = false;
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				if (p_voltage < 0.0 || p_voltage > 5.0)
				{
					return result;
				}
				if (Basic.Configure_PICkitSerial(1, true))
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
						if (p_aux1_def)
						{
							byte[] expr_AD_cp_0 = array2;
							int expr_AD_cp_1 = 28;
							expr_AD_cp_0[expr_AD_cp_1] |= 1;
						}
						else
						{
							byte[] expr_C6_cp_0 = array2;
							int expr_C6_cp_1 = 28;
							expr_C6_cp_0[expr_C6_cp_1] &= 254;
						}
						if (p_aux2_def)
						{
							byte[] expr_E4_cp_0 = array2;
							int expr_E4_cp_1 = 28;
							expr_E4_cp_0[expr_E4_cp_1] |= 2;
						}
						else
						{
							byte[] expr_FD_cp_0 = array2;
							int expr_FD_cp_1 = 28;
							expr_FD_cp_0[expr_FD_cp_1] &= 253;
						}
						if (p_aux1_dir)
						{
							byte[] expr_11B_cp_0 = array2;
							int expr_11B_cp_1 = 28;
							expr_11B_cp_0[expr_11B_cp_1] |= 4;
						}
						else
						{
							byte[] expr_134_cp_0 = array2;
							int expr_134_cp_1 = 28;
							expr_134_cp_0[expr_134_cp_1] &= 251;
						}
						if (p_aux2_dir)
						{
							byte[] expr_152_cp_0 = array2;
							int expr_152_cp_1 = 28;
							expr_152_cp_0[expr_152_cp_1] |= 8;
						}
						else
						{
							byte[] expr_16B_cp_0 = array2;
							int expr_16B_cp_1 = 28;
							expr_16B_cp_0[expr_16B_cp_1] &= 247;
						}
						if (p_enable_pu)
						{
							byte[] expr_18A_cp_0 = array2;
							int expr_18A_cp_1 = 16;
							expr_18A_cp_0[expr_18A_cp_1] |= 16;
						}
						else
						{
							byte[] expr_1A4_cp_0 = array2;
							int expr_1A4_cp_1 = 16;
							expr_1A4_cp_0[expr_1A4_cp_1] &= 239;
						}
						int num = (int)Math.Round((p_voltage * 1000.0 + 43.53) / 21.191);
						array2[19] = (byte)num;
						array2[20] = (byte)(num / 4);
						byte[] expr_1F7_cp_0 = array2;
						int expr_1F7_cp_1 = 16;
						expr_1F7_cp_0[expr_1F7_cp_1] |= 32;
						byte[] expr_20F_cp_0 = array2;
						int expr_20F_cp_1 = 16;
						expr_20F_cp_0[expr_20F_cp_1] |= 64;
						USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
						result = USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
					}
				}
			}
			return result;
		}

		public static bool Write(byte p_slave_addr, byte p_start_data_addr, byte p_num_bytes_to_write, ref byte[] p_data_array, ref string p_script_view)
		{
			return Basic.Send_I2CWrite_Cmd(p_slave_addr, p_start_data_addr, p_num_bytes_to_write, ref p_data_array, ref p_script_view);
		}

		public static bool Write(byte p_slave_addr, byte p_command1, byte p_command2, byte p_num_bytes_to_write, ref byte[] p_data_array, ref string p_script_view)
		{
			return Basic.Send_I2CWrite_Word_Cmd(p_slave_addr, p_command1, p_command2, p_num_bytes_to_write, ref p_data_array, ref p_script_view);
		}

		public static bool Write_Using_PEC(byte p_slave_addr, byte p_start_data_addr, byte p_num_bytes_to_write, ref byte[] p_data_array, ref byte p_PEC, ref string p_script_view)
		{
			if (p_num_bytes_to_write > 253)
			{
				return false;
			}
			byte[] array = new byte[300];
			byte b = 0;
			b = Utilities.calculate_crc8(p_slave_addr, b);
			b = Utilities.calculate_crc8(p_start_data_addr, b);
			int i;
			for (i = 0; i < (int)p_num_bytes_to_write; i++)
			{
				b = Utilities.calculate_crc8(p_data_array[i], b);
			}
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = (byte)(9 + p_num_bytes_to_write);
			array[3] = 129;
			array[4] = 132;
			array[5] = (byte)(3 + p_num_bytes_to_write);
			array[6] = p_slave_addr;
			array[7] = p_start_data_addr;
			for (i = 0; i < (int)p_num_bytes_to_write; i++)
			{
				array[i + 8] = p_data_array[i];
			}
			array[i + 8] = b;
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
			for (i = 0; i < (int)p_num_bytes_to_write; i++)
			{
				text = string.Format("[{0:X2}]", array[i + 8]);
				p_script_view += text;
			}
			text = string.Format("[{0:X2}]", b);
			p_script_view += text;
			p_script_view += "[P_]";
			USBRead.Clear_Data_Array(0u);
			USBRead.Clear_Raw_Data_Array();
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Read(byte p_slave_addr, byte p_command1, byte p_command2, byte p_num_bytes_to_read, ref byte[] p_data_array, ref string p_script_view)
		{
			return Basic.Send_I2CRead_Word_Cmd(p_slave_addr, p_command1, p_command2, p_num_bytes_to_read, ref p_data_array, ref p_script_view);
		}

		public static bool Read(byte p_slave_addr, byte p_start_data_addr, byte p_num_bytes_to_read, ref byte[] p_data_array, ref string p_script_view)
		{
			return Basic.Send_I2CRead_Cmd(p_slave_addr, p_start_data_addr, p_num_bytes_to_read, ref p_data_array, ref p_script_view);
		}

		public static bool Receive(byte p_slave_addr, byte p_num_bytes_to_read, ref byte[] p_data_array, ref string p_script_view)
		{
			return Basic.Send_I2C_SimpleRead_Cmd(p_slave_addr, p_num_bytes_to_read, ref p_data_array, ref p_script_view);
		}

		public static bool Set_I2C_Bit_Rate(double p_Bit_Rate)
		{
			bool result = false;
			string text = "";
			string text2 = "";
			byte[] array = new byte[65];
			byte[] array2 = new byte[65];
			if (p_Bit_Rate < 39.1 || p_Bit_Rate > 5000.0)
			{
				return result;
			}
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				Array.Clear(array2, 0, array2.Length);
				if (!Basic.Get_Status_Packet(ref array2))
				{
					return false;
				}
				byte b = (byte)(Math.Round(20000.0 / p_Bit_Rate / 4.0) - 1.0);
				array2[30] = b;
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				result = USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
			}
			return result;
		}

		public static double Get_I2C_Bit_Rate()
		{
			byte[] array = new byte[65];
			double result = 0.0;
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				if (Basic.Get_Status_Packet(ref array))
				{
					result = I2CM.calculate_baud_rate((ushort)array[51]);
				}
			}
			return result;
		}

		private static double calculate_baud_rate(ushort p_baud)
		{
			double num = 20.0 / (4.0 * ((double)p_baud + 1.0));
			return num * 1000.0;
		}

		public static void Set_Read_Wait_Time(int p_time)
		{
			Basic.m_i2cs_read_wait_time = p_time;
		}

		public static int Get_Read_Wait_Time()
		{
			return Basic.m_i2cs_read_wait_time;
		}

		public static void Set_Receive_Wait_Time(int p_time)
		{
			Basic.m_i2cs_receive_wait_time = p_time;
		}

		public static int Get_Receive_Wait_Time()
		{
			return Basic.m_i2cs_receive_wait_time;
		}

		public static bool Get_Source_Voltage(ref double p_voltage, ref bool p_PKSA_power)
		{
			return USART.Get_Source_Voltage(ref p_voltage, ref p_PKSA_power);
		}

		public static bool Set_Source_Voltage(double p_voltage)
		{
			return USART.Set_Source_Voltage(p_voltage);
		}

		public static bool Tell_PKSA_To_Use_External_Voltage_Source()
		{
			return USART.Tell_PKSA_To_Use_External_Voltage_Source();
		}

		public static bool Get_Aux_Status(ref bool p_aux1_state, ref bool p_aux2_state, ref bool p_aux1_dir, ref bool p_aux2_dir)
		{
			return USART.Get_Aux_Status(ref p_aux1_state, ref p_aux2_state, ref p_aux1_dir, ref p_aux2_dir);
		}

		public static bool Set_Aux1_Direction(bool p_dir)
		{
			return USART.Set_Aux1_Direction(p_dir);
		}

		public static bool Set_Aux2_Direction(bool p_dir)
		{
			return USART.Set_Aux2_Direction(p_dir);
		}

		public static bool Set_Aux1_State(bool p_state)
		{
			return USART.Set_Aux1_State(p_state);
		}

		public static bool Set_Aux2_State(bool p_state)
		{
			return USART.Set_Aux2_State(p_state);
		}

		public static bool Set_Pullup_State(bool p_enable)
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
				if (p_enable)
				{
					byte[] expr_64_cp_0 = array2;
					int expr_64_cp_1 = 16;
					expr_64_cp_0[expr_64_cp_1] |= 16;
				}
				else
				{
					byte[] expr_7E_cp_0 = array2;
					int expr_7E_cp_1 = 16;
					expr_7E_cp_0[expr_7E_cp_1] &= 239;
				}
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				result = USBWrite.write_and_verify_config_block(ref array, ref text2, false, ref text);
			}
			return result;
		}
	}
}
