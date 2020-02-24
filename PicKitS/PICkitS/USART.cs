using System;

namespace PICkitS
{
	public class USART
	{
		public static bool Configure_PICkitSerial_For_USARTAsync()
		{
			return Basic.Configure_PICkitSerial(4, true);
		}

		public static bool Configure_PICkitSerial_For_USARTAsync(bool p_aux1_def, bool p_aux2_def, bool p_aux1_dir, bool p_aux2_dir, bool p_rcv_dis, double p_voltage)
		{
			bool result = false;
			if (Basic.Configure_PICkitSerial(4, true))
			{
				string text = "";
				string text2 = "";
				byte[] array = new byte[65];
				byte[] array2 = new byte[65];
				if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
				{
					if (p_voltage < 0.0 || p_voltage > 5.0)
					{
						return result;
					}
					Array.Clear(array, 0, array.Length);
					Array.Clear(array2, 0, array2.Length);
					if (!Basic.Get_Status_Packet(ref array2))
					{
						return false;
					}
					if (p_aux1_def)
					{
						byte[] expr_94_cp_0 = array2;
						int expr_94_cp_1 = 28;
						expr_94_cp_0[expr_94_cp_1] |= 1;
					}
					else
					{
						byte[] expr_AD_cp_0 = array2;
						int expr_AD_cp_1 = 28;
						expr_AD_cp_0[expr_AD_cp_1] &= 254;
					}
					if (p_aux2_def)
					{
						byte[] expr_CB_cp_0 = array2;
						int expr_CB_cp_1 = 28;
						expr_CB_cp_0[expr_CB_cp_1] |= 2;
					}
					else
					{
						byte[] expr_E4_cp_0 = array2;
						int expr_E4_cp_1 = 28;
						expr_E4_cp_0[expr_E4_cp_1] &= 253;
					}
					if (p_aux1_dir)
					{
						byte[] expr_102_cp_0 = array2;
						int expr_102_cp_1 = 28;
						expr_102_cp_0[expr_102_cp_1] |= 4;
					}
					else
					{
						byte[] expr_11B_cp_0 = array2;
						int expr_11B_cp_1 = 28;
						expr_11B_cp_0[expr_11B_cp_1] &= 251;
					}
					if (p_aux2_dir)
					{
						byte[] expr_139_cp_0 = array2;
						int expr_139_cp_1 = 28;
						expr_139_cp_0[expr_139_cp_1] |= 8;
					}
					else
					{
						byte[] expr_152_cp_0 = array2;
						int expr_152_cp_1 = 28;
						expr_152_cp_0[expr_152_cp_1] &= 247;
					}
					if (p_rcv_dis)
					{
						byte[] expr_171_cp_0 = array2;
						int expr_171_cp_1 = 24;
						expr_171_cp_0[expr_171_cp_1] |= 4;
					}
					else
					{
						byte[] expr_18A_cp_0 = array2;
						int expr_18A_cp_1 = 24;
						expr_18A_cp_0[expr_18A_cp_1] &= 251;
					}
					int num = (int)Math.Round((p_voltage * 1000.0 + 43.53) / 21.191);
					array2[19] = (byte)num;
					array2[20] = (byte)(num / 4);
					byte[] expr_1DD_cp_0 = array2;
					int expr_1DD_cp_1 = 16;
					expr_1DD_cp_0[expr_1DD_cp_1] |= 32;
					byte[] expr_1F5_cp_0 = array2;
					int expr_1F5_cp_1 = 16;
					expr_1F5_cp_0[expr_1F5_cp_1] |= 64;
					USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
					result = USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
				}
			}
			return result;
		}

		public static bool Configure_PICkitSerial_For_USARTSyncMaster(bool p_aux1_def, bool p_aux2_def, bool p_aux1_dir, bool p_aux2_dir, bool p_clock_pol, double p_voltage)
		{
			bool result = false;
			if (Basic.Configure_PICkitSerial(5, true))
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
						byte[] expr_78_cp_0 = array2;
						int expr_78_cp_1 = 28;
						expr_78_cp_0[expr_78_cp_1] |= 1;
					}
					else
					{
						byte[] expr_91_cp_0 = array2;
						int expr_91_cp_1 = 28;
						expr_91_cp_0[expr_91_cp_1] &= 254;
					}
					if (p_aux2_def)
					{
						byte[] expr_AF_cp_0 = array2;
						int expr_AF_cp_1 = 28;
						expr_AF_cp_0[expr_AF_cp_1] |= 2;
					}
					else
					{
						byte[] expr_C8_cp_0 = array2;
						int expr_C8_cp_1 = 28;
						expr_C8_cp_0[expr_C8_cp_1] &= 253;
					}
					if (p_aux1_dir)
					{
						byte[] expr_E6_cp_0 = array2;
						int expr_E6_cp_1 = 28;
						expr_E6_cp_0[expr_E6_cp_1] |= 4;
					}
					else
					{
						byte[] expr_FF_cp_0 = array2;
						int expr_FF_cp_1 = 28;
						expr_FF_cp_0[expr_FF_cp_1] &= 251;
					}
					if (p_aux2_dir)
					{
						byte[] expr_11D_cp_0 = array2;
						int expr_11D_cp_1 = 28;
						expr_11D_cp_0[expr_11D_cp_1] |= 8;
					}
					else
					{
						byte[] expr_136_cp_0 = array2;
						int expr_136_cp_1 = 28;
						expr_136_cp_0[expr_136_cp_1] &= 247;
					}
					if (p_clock_pol)
					{
						byte[] expr_155_cp_0 = array2;
						int expr_155_cp_1 = 24;
						expr_155_cp_0[expr_155_cp_1] |= 1;
					}
					else
					{
						byte[] expr_16E_cp_0 = array2;
						int expr_16E_cp_1 = 24;
						expr_16E_cp_0[expr_16E_cp_1] &= 254;
					}
					int num = (int)Math.Round((p_voltage * 1000.0 + 43.53) / 21.191);
					array2[19] = (byte)num;
					array2[20] = (byte)(num / 4);
					byte[] expr_1C1_cp_0 = array2;
					int expr_1C1_cp_1 = 16;
					expr_1C1_cp_0[expr_1C1_cp_1] |= 32;
					byte[] expr_1D9_cp_0 = array2;
					int expr_1D9_cp_1 = 16;
					expr_1D9_cp_0[expr_1D9_cp_1] |= 64;
					USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
					result = USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
				}
			}
			return result;
		}

		public static bool Configure_PICkitSerial_For_USARTSyncMaster()
		{
			return Basic.Configure_PICkitSerial(5, true);
		}

		public static bool Configure_PICkitSerial_For_USARTSyncSlave()
		{
			return Basic.Configure_PICkitSerial(6, true);
		}

		public static uint Retrieve_Data_Byte_Count()
		{
			return USBRead.Retrieve_Data_Byte_Count();
		}

		public static bool Retrieve_Data(uint p_byte_count, ref byte[] p_data_array)
		{
			bool result = false;
			if (USBRead.Retrieve_Data(ref p_data_array, p_byte_count))
			{
				result = true;
			}
			return result;
		}

		public static bool Send_Data(byte p_byte_count, ref byte[] p_data_array, ref string p_script_view)
		{
			byte[] array = new byte[310];
			int num = 5;
			p_script_view = "";
			if (p_byte_count > 251)
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
				array[num++] = p_data_array[i];
				text = string.Format("[{0:X2}]", p_data_array[i]);
				p_script_view += text;
			}
			array[num++] = 31;
			array[num++] = 119;
			array[num] = 0;
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Set_Baud_Rate(ushort p_baud)
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
				int num = (int)Math.Round(20000000.0 / (double)p_baud / 4.0) - 1;
				array2[29] = (byte)num;
				array2[30] = (byte)(num >> 8);
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				result = USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
			}
			return result;
		}

		public static ushort Get_Baud_Rate()
		{
			byte[] array = new byte[65];
			ushort result = 0;
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				if (Basic.Get_Status_Packet(ref array))
				{
					result = USART.calculate_baud_rate((ushort)((int)array[50] + ((int)array[51] << 8)));
				}
			}
			return result;
		}

		private static ushort calculate_baud_rate(ushort p_baud)
		{
			double num = 20.0 / (4.0 * ((double)p_baud + 1.0));
			int num2 = (int)Math.Round(num * 1000000.0);
			return (ushort)num2;
		}

		public static bool Get_Source_Voltage(ref double p_voltage, ref bool p_PKSA_power)
		{
			byte[] array = new byte[65];
			bool result = false;
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				if (Basic.Get_Status_Packet(ref array))
				{
					p_voltage = (double)array[39] * 5.0 / 255.0;
					p_PKSA_power = ((array[16] & 32) > 0);
					result = true;
				}
			}
			return result;
		}

		public static bool Set_Source_Voltage(double p_voltage)
		{
			bool result = false;
			string text = "";
			string text2 = "";
			byte[] array = new byte[65];
			byte[] array2 = new byte[65];
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				if (p_voltage < 0.0 || p_voltage > 5.0)
				{
					return result;
				}
				Array.Clear(array, 0, array.Length);
				Array.Clear(array2, 0, array2.Length);
				if (!Basic.Get_Status_Packet(ref array2))
				{
					return false;
				}
				int num = (int)Math.Round((p_voltage * 1000.0 + 43.53) / 21.191);
				array2[19] = (byte)num;
				array2[20] = (byte)(num / 4);
				byte[] expr_BA_cp_0 = array2;
				int expr_BA_cp_1 = 16;
				expr_BA_cp_0[expr_BA_cp_1] |= 32;
				byte[] expr_D2_cp_0 = array2;
				int expr_D2_cp_1 = 16;
				expr_D2_cp_0[expr_D2_cp_1] |= 64;
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				result = USBWrite.write_and_verify_config_block(ref array, ref text2, false, ref text);
			}
			return result;
		}

		public static bool Tell_PKSA_To_Use_External_Voltage_Source()
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
				byte[] expr_61_cp_0 = array2;
				int expr_61_cp_1 = 16;
				expr_61_cp_0[expr_61_cp_1] &= 223;
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				result = USBWrite.write_and_verify_config_block(ref array, ref text2, false, ref text);
			}
			return result;
		}

		public static bool Get_Aux_Status(ref bool p_aux1_state, ref bool p_aux2_state, ref bool p_aux1_dir, ref bool p_aux2_dir)
		{
			byte[] array = new byte[65];
			bool result = false;
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				if (Basic.Get_Status_Packet(ref array))
				{
					p_aux1_state = ((array[48] & 1) > 0);
					p_aux2_state = ((array[48] & 2) > 0);
					p_aux1_dir = ((array[48] & 4) > 0);
					p_aux2_dir = ((array[48] & 8) > 0);
					result = true;
				}
			}
			return result;
		}

		public static bool Set_Aux1_Direction(bool p_dir)
		{
			byte[] array = new byte[16];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 3;
			if (p_dir)
			{
				array[3] = 147;
			}
			else
			{
				array[3] = 146;
			}
			array[4] = 31;
			array[5] = 119;
			array[6] = 0;
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Set_Aux2_Direction(bool p_dir)
		{
			byte[] array = new byte[16];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 3;
			if (p_dir)
			{
				array[3] = 153;
			}
			else
			{
				array[3] = 152;
			}
			array[4] = 31;
			array[5] = 119;
			array[6] = 0;
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Set_Aux1_State(bool p_state)
		{
			byte[] array = new byte[16];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 3;
			if (p_state)
			{
				array[3] = 145;
			}
			else
			{
				array[3] = 144;
			}
			array[4] = 31;
			array[5] = 119;
			array[6] = 0;
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Set_Aux2_State(bool p_state)
		{
			byte[] array = new byte[16];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 3;
			if (p_state)
			{
				array[3] = 151;
			}
			else
			{
				array[3] = 150;
			}
			array[4] = 31;
			array[5] = 119;
			array[6] = 0;
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}
	}
}
