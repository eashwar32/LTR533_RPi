using System;

namespace PICkitS
{
	public class MicrowireM
	{
		public static bool Configure_PICkitSerial_For_MicrowireMaster()
		{
			return Basic.Configure_PICkitSerial(11, true);
		}

		public static bool Configure_PICkitSerial_For_MicrowireMaster(bool p_sample_phase, bool p_clock_edge_select, bool p_clock_polarity, bool p_auto_output_disable, bool p_chip_sel_polarity, bool p_supply_5V)
		{
			bool result = false;
			if (Basic.Configure_PICkitSerial(11, true))
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
						byte[] expr_74_cp_0 = array2;
						int expr_74_cp_1 = 24;
						expr_74_cp_0[expr_74_cp_1] |= 1;
					}
					else
					{
						byte[] expr_8D_cp_0 = array2;
						int expr_8D_cp_1 = 24;
						expr_8D_cp_0[expr_8D_cp_1] &= 254;
					}
					if (p_clock_edge_select)
					{
						byte[] expr_AB_cp_0 = array2;
						int expr_AB_cp_1 = 24;
						expr_AB_cp_0[expr_AB_cp_1] |= 2;
					}
					else
					{
						byte[] expr_C4_cp_0 = array2;
						int expr_C4_cp_1 = 24;
						expr_C4_cp_0[expr_C4_cp_1] &= 253;
					}
					if (p_clock_polarity)
					{
						byte[] expr_E2_cp_0 = array2;
						int expr_E2_cp_1 = 24;
						expr_E2_cp_0[expr_E2_cp_1] |= 4;
					}
					else
					{
						byte[] expr_FB_cp_0 = array2;
						int expr_FB_cp_1 = 24;
						expr_FB_cp_0[expr_FB_cp_1] &= 251;
					}
					if (p_auto_output_disable)
					{
						byte[] expr_119_cp_0 = array2;
						int expr_119_cp_1 = 24;
						expr_119_cp_0[expr_119_cp_1] |= 8;
					}
					else
					{
						byte[] expr_132_cp_0 = array2;
						int expr_132_cp_1 = 24;
						expr_132_cp_0[expr_132_cp_1] &= 247;
					}
					if (p_chip_sel_polarity)
					{
						byte[] expr_151_cp_0 = array2;
						int expr_151_cp_1 = 24;
						expr_151_cp_0[expr_151_cp_1] |= 128;
					}
					else
					{
						byte[] expr_16E_cp_0 = array2;
						int expr_16E_cp_1 = 24;
						expr_16E_cp_0[expr_16E_cp_1] &= 127;
					}
					if (p_supply_5V)
					{
						byte[] expr_18A_cp_0 = array2;
						int expr_18A_cp_1 = 16;
						expr_18A_cp_0[expr_18A_cp_1] |= 32;
					}
					else
					{
						byte[] expr_1A4_cp_0 = array2;
						int expr_1A4_cp_1 = 16;
						expr_1A4_cp_0[expr_1A4_cp_1] &= 223;
					}
					USBWrite.configure_outbound_control_block_packet(ref array, ref text, ref array2);
					result = USBWrite.write_and_verify_config_block(ref array, ref text2, true, ref text);
				}
			}
			return result;
		}

		public static bool Send_Data(byte p_byte_count, ref byte[] p_data_array, bool p_assert_cs, bool p_de_assert_cs, ref string p_script_view)
		{
			return Basic.Send_SPI_Send_Cmd(p_byte_count, ref p_data_array, p_assert_cs, p_de_assert_cs, ref p_script_view);
		}

		public static bool Receive_Data(byte p_byte_count, ref byte[] p_data_array, bool p_assert_cs, bool p_de_assert_cs, ref string p_script_view)
		{
			return Basic.Send_SPI_Receive_Cmd(p_byte_count, ref p_data_array, p_assert_cs, p_de_assert_cs, ref p_script_view);
		}

		public static bool Set_Microwire_BitRate(double p_Bit_Rate)
		{
			return SPIM.Set_SPI_BitRate(p_Bit_Rate);
		}

		public static double Get_Microwire_Bit_Rate()
		{
			return SPIM.Get_SPI_Bit_Rate();
		}

		public static bool Get_Microwire_Status(ref bool p_sample_phase, ref bool p_clock_edge_select, ref bool p_clock_polarity, ref bool p_auto_output_disable, ref bool p_SDI_state, ref bool p_SDO_state, ref bool p_SCK_state, ref bool p_chip_select_state)
		{
			return SPIM.Get_SPI_Status(ref p_sample_phase, ref p_clock_edge_select, ref p_clock_polarity, ref p_auto_output_disable, ref p_SDI_state, ref p_SDO_state, ref p_SCK_state, ref p_chip_select_state);
		}

		public static bool Tell_PKSA_To_Use_External_Voltage_Source()
		{
			return SPIM.Tell_PKSA_To_Use_External_Voltage_Source();
		}

		public static bool Tell_PKSA_To_Power_My_Device()
		{
			return SPIM.Tell_PKSA_To_Power_My_Device();
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
