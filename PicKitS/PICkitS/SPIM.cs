using System;

namespace PICkitS
{
	public class SPIM
	{
		public static bool Configure_PICkitSerial_For_SPIMaster()
		{
			return Basic.Configure_PICkitSerial(2, true);
		}

		public static bool Configure_PICkitSerial_For_SPIMaster(bool p_sample_phase, bool p_clock_edge_select, bool p_clock_polarity, bool p_auto_output_disable, bool p_chip_sel_polarity, bool p_supply_5V)
		{
			bool result = false;
			if (Basic.Configure_PICkitSerial(2, true))
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
					if (p_sample_phase)
					{
						byte[] expr_73_cp_0 = array2;
						int expr_73_cp_1 = 24;
						expr_73_cp_0[expr_73_cp_1] |= 1;
					}
					else
					{
						byte[] expr_8C_cp_0 = array2;
						int expr_8C_cp_1 = 24;
						expr_8C_cp_0[expr_8C_cp_1] &= 254;
					}
					if (p_clock_edge_select)
					{
						byte[] expr_AA_cp_0 = array2;
						int expr_AA_cp_1 = 24;
						expr_AA_cp_0[expr_AA_cp_1] |= 2;
					}
					else
					{
						byte[] expr_C3_cp_0 = array2;
						int expr_C3_cp_1 = 24;
						expr_C3_cp_0[expr_C3_cp_1] &= 253;
					}
					if (p_clock_polarity)
					{
						byte[] expr_E1_cp_0 = array2;
						int expr_E1_cp_1 = 24;
						expr_E1_cp_0[expr_E1_cp_1] |= 4;
					}
					else
					{
						byte[] expr_FA_cp_0 = array2;
						int expr_FA_cp_1 = 24;
						expr_FA_cp_0[expr_FA_cp_1] &= 251;
					}
					if (p_auto_output_disable)
					{
						byte[] expr_118_cp_0 = array2;
						int expr_118_cp_1 = 24;
						expr_118_cp_0[expr_118_cp_1] |= 8;
					}
					else
					{
						byte[] expr_131_cp_0 = array2;
						int expr_131_cp_1 = 24;
						expr_131_cp_0[expr_131_cp_1] &= 247;
					}
					if (p_chip_sel_polarity)
					{
						byte[] expr_150_cp_0 = array2;
						int expr_150_cp_1 = 24;
						expr_150_cp_0[expr_150_cp_1] |= 128;
					}
					else
					{
						byte[] expr_16D_cp_0 = array2;
						int expr_16D_cp_1 = 24;
						expr_16D_cp_0[expr_16D_cp_1] &= 127;
					}
					if (p_supply_5V)
					{
						byte[] expr_189_cp_0 = array2;
						int expr_189_cp_1 = 16;
						expr_189_cp_0[expr_189_cp_1] |= 32;
					}
					else
					{
						byte[] expr_1A3_cp_0 = array2;
						int expr_1A3_cp_1 = 16;
						expr_1A3_cp_0[expr_1A3_cp_1] &= 223;
					}
					USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
					result = USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
				}
			}
			return result;
		}

		private static bool Configure_PICkitSerial_For_SPISlave()
		{
			return Basic.Configure_PICkitSerial(3, true);
		}

		public static bool Send_Data(byte p_byte_count, ref byte[] p_data_array, bool p_assert_cs, bool p_de_assert_cs, ref string p_script_view)
		{
			return Basic.Send_SPI_Send_Cmd(p_byte_count, ref p_data_array, p_assert_cs, p_de_assert_cs, ref p_script_view);
		}

		public static bool Receive_Data(byte p_byte_count, ref byte[] p_data_array, bool p_assert_cs, bool p_de_assert_cs, ref string p_script_view)
		{
			return Basic.Send_SPI_Receive_Cmd(p_byte_count, ref p_data_array, p_assert_cs, p_de_assert_cs, ref p_script_view);
		}

		public static bool Send_And_Receive_Data(byte p_byte_count, ref byte[] p_send_data_array, ref byte[] p_receive_data_array, bool p_assert_cs, bool p_de_assert_cs, ref string p_script_view)
		{
			bool result = false;
			byte[] array = new byte[255];
			int num = 3;
			p_script_view = "";
			if (p_byte_count > 245)
			{
				return false;
			}
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			if (p_assert_cs)
			{
				array[num++] = 139;
				p_script_view = "[CSON]";
			}
			array[num++] = 134;
			array[num++] = p_byte_count;
			p_script_view += "[DIO]";
			string text = string.Format("[{0:X2}]", p_byte_count);
			p_script_view += text;
			for (int i = 0; i < (int)p_byte_count; i++)
			{
				array[num++] = p_send_data_array[i];
				text = string.Format("[{0:X2}]", p_send_data_array[i]);
				p_script_view += text;
			}
			if (p_de_assert_cs)
			{
				array[num++] = 140;
				p_script_view += "[CSOF]";
			}
			array[2] = (byte)(num - 1);
			array[num++] = 31;
			array[num++] = 119;
			array[num] = 0;
			USBRead.Clear_Data_Array((uint)p_byte_count);
			USBRead.Clear_Raw_Data_Array();
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_data_arrived_event.WaitOne(Basic.m_spi_receive_wait_time, false);
				if (flag2 && USBRead.Retrieve_Data(ref p_receive_data_array, (uint)p_byte_count))
				{
					result = true;
				}
			}
			return result;
		}

		public static bool Set_SPI_BitRate(double p_Bit_Rate)
		{
			if (p_Bit_Rate < 0.61 || p_Bit_Rate > 1250.0)
			{
				return false;
			}
			double num = 200000.0;
			byte[] array = new byte[16];
			double num2 = 2.5 / p_Bit_Rate * 1000.0;
			double num3 = 0.625 / p_Bit_Rate * 1000.0;
			double num4 = 0.15625 / p_Bit_Rate * 1000.0;
			ushort num5;
			if (num2 > 0.0 && num2 <= 256.0)
			{
				num5 = (ushort)Math.Round(num2);
			}
			else
			{
				num5 = 0;
			}
			ushort num6;
			if (num3 > 0.0 && num3 <= 256.0)
			{
				num6 = (ushort)Math.Round(num3);
			}
			else
			{
				num6 = 0;
			}
			ushort num7;
			if (num4 > 0.0 && num4 <= 256.0)
			{
				num7 = (ushort)Math.Round(num4);
			}
			else
			{
				num7 = 0;
			}
			double num8;
			if (num5 != 0)
			{
				num8 = 2.5 / (double)num5 * 1000.0;
			}
			else
			{
				num8 = num;
			}
			double num9;
			if (num6 != 0)
			{
				num9 = 0.625 / (double)num6 * 1000.0;
			}
			else
			{
				num9 = num;
			}
			double num10;
			if (num7 != 0)
			{
				num10 = 0.15625 / (double)num7 * 1000.0;
			}
			else
			{
				num10 = num;
			}
			byte b;
			byte b2;
			if (Math.Abs(num8 - p_Bit_Rate) < Math.Abs(num9 - p_Bit_Rate))
			{
				if (Math.Abs(num8 - p_Bit_Rate) < Math.Abs(num10 - p_Bit_Rate))
				{
					b = (byte)(num5 - 1);
					b2 = 0;
				}
				else
				{
					b = (byte)(num7 - 1);
					b2 = 2;
				}
			}
			else if (Math.Abs(num9 - p_Bit_Rate) < Math.Abs(num10 - p_Bit_Rate))
			{
				b = (byte)(num6 - 1);
				b2 = 1;
			}
			else
			{
				b = (byte)(num7 - 1);
				b2 = 2;
			}
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 5;
			array[3] = 131;
			array[4] = b;
			array[5] = b2;
			array[6] = 31;
			array[7] = 119;
			array[8] = 0;
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static double Get_SPI_Bit_Rate()
		{
			byte[] array = new byte[65];
			double result = 0.0;
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				if (Basic.Get_Status_Packet(ref array))
				{
					double num;
					switch (array[50])
					{
						case 0:
							num = 8.0;
							break;
						case 1:
							num = 32.0;
							break;
						case 2:
							num = 128.0;
							break;
						default:
							num = 0.0;
							break;
					}
					double num2 = (double)(array[51] + 1);
					if (num == 0.0)
					{
						result = 0.0;
					}
					else
					{
						result = 20.0 / num / num2 * 1000.0;
					}
				}
			}
			return result;
		}

		public static bool Get_SPI_Status(ref bool p_sample_phase, ref bool p_clock_edge_select, ref bool p_clock_polarity, ref bool p_auto_output_disable, ref bool p_SDI_state, ref bool p_SDO_state, ref bool p_SCK_state, ref bool p_chip_select_state)
		{
			byte[] array = new byte[65];
			bool result = false;
			if(Utilities.m_flags.HID_DeviceReady != false) //(Utilities.m_flags.HID_read_handle != IntPtr.Zero)
			{
				Array.Clear(array, 0, array.Length);
				if (Basic.Get_Status_Packet(ref array))
				{
					p_sample_phase = ((array[45] & 1) > 0);
					p_clock_edge_select = ((array[45] & 2) > 0);
					p_clock_polarity = ((array[45] & 4) > 0);
					p_auto_output_disable = ((array[45] & 8) > 0);
					p_SDI_state = ((array[46] & 1) > 0);
					p_SDO_state = ((array[46] & 2) > 0);
					p_SCK_state = ((array[46] & 4) > 0);
					p_chip_select_state = ((array[46] & 8) > 0);
					result = true;
				}
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

		public static bool Tell_PKSA_To_Power_My_Device()
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
				expr_61_cp_0[expr_61_cp_1] |= 32;
				USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
				result = USBWrite.write_and_verify_config_block(ref array, ref text2, false, ref text);
			}
			return result;
		}

		public static void Set_Receive_Wait_Time(int p_time)
		{
			Basic.m_spi_receive_wait_time = p_time;
		}

		public static int Get_Receive_Wait_Time()
		{
			return Basic.m_spi_receive_wait_time;
		}
	}
}
