using System;

namespace PICkitS
{
	public class Mode
	{
		private static byte[] m_default_idle_data = new byte[65];

		private static byte[] m_default_i2c_m_data = new byte[65];

		private static byte[] m_default_i2c_s_data = new byte[65];

		private static byte[] m_default_i2c_bbm_data = new byte[65];

		private static byte[] m_default_i2c_sbbm_data = new byte[65];

		private static byte[] m_default_spi_m_data = new byte[65];

		private static byte[] m_default_spi_s_data = new byte[65];

		private static byte[] m_default_usart_a_data = new byte[65];

		private static byte[] m_default_usart_sm_data = new byte[65];

		private static byte[] m_default_usart_ss_data = new byte[65];

		private static byte[] m_default_LIN_data = new byte[65];

		private static byte[] m_default_uwire_m_data = new byte[65];

		private static byte[] m_default_mtouch2_data = new byte[65];

		private static byte[] m_default_LIN_no_autobaud_data = new byte[65];

		private static byte[] m_test_5_volt_src_data = new byte[65];

		private static byte[] m_test_2p5_volt_src_data = new byte[65];

		private static byte[] m_test_0_volt_src_data = new byte[65];

		private static byte[] m_test_5_volt_src_no_pullup_data = new byte[65];

		private static byte[] m_test_i2c_axout_a11_a20_data = new byte[65];

		private static byte[] m_test_i2c_axout_a10_a21_data = new byte[65];

		private static byte[] m_test_i2c_axout_a11_a21_data = new byte[65];

		private static byte[] m_test_i2c_test_sw_enable_data = new byte[65];

		public static void configure_run_mode_arrays()
		{
			Array.Clear(Mode.m_default_idle_data, 0, Mode.m_default_idle_data.Length);
			Array.Clear(Mode.m_default_i2c_m_data, 0, Mode.m_default_i2c_m_data.Length);
			Array.Clear(Mode.m_default_i2c_s_data, 0, Mode.m_default_i2c_s_data.Length);
			Array.Clear(Mode.m_default_i2c_bbm_data, 0, Mode.m_default_i2c_bbm_data.Length);
			Array.Clear(Mode.m_default_i2c_sbbm_data, 0, Mode.m_default_i2c_sbbm_data.Length);
			Array.Clear(Mode.m_default_spi_m_data, 0, Mode.m_default_spi_m_data.Length);
			Array.Clear(Mode.m_default_spi_s_data, 0, Mode.m_default_spi_s_data.Length);
			Array.Clear(Mode.m_default_usart_a_data, 0, Mode.m_default_usart_a_data.Length);
			Array.Clear(Mode.m_default_usart_sm_data, 0, Mode.m_default_usart_sm_data.Length);
			Array.Clear(Mode.m_default_usart_ss_data, 0, Mode.m_default_usart_ss_data.Length);
			Array.Clear(Mode.m_default_LIN_data, 0, Mode.m_default_LIN_data.Length);
			Array.Clear(Mode.m_default_LIN_no_autobaud_data, 0, Mode.m_default_LIN_no_autobaud_data.Length);
			Array.Clear(Mode.m_default_uwire_m_data, 0, Mode.m_default_uwire_m_data.Length);
			Array.Clear(Mode.m_default_mtouch2_data, 0, Mode.m_default_mtouch2_data.Length);
			Array.Clear(Mode.m_test_5_volt_src_data, 0, Mode.m_test_5_volt_src_data.Length);
			Array.Clear(Mode.m_test_2p5_volt_src_data, 0, Mode.m_test_2p5_volt_src_data.Length);
			Array.Clear(Mode.m_test_0_volt_src_data, 0, Mode.m_test_0_volt_src_data.Length);
			Array.Clear(Mode.m_test_5_volt_src_no_pullup_data, 0, Mode.m_test_5_volt_src_no_pullup_data.Length);
			Array.Clear(Mode.m_test_i2c_axout_a11_a20_data, 0, Mode.m_test_i2c_axout_a11_a20_data.Length);
			Array.Clear(Mode.m_test_i2c_axout_a10_a21_data, 0, Mode.m_test_i2c_axout_a10_a21_data.Length);
			Array.Clear(Mode.m_test_i2c_axout_a11_a21_data, 0, Mode.m_test_i2c_axout_a11_a21_data.Length);
			Array.Clear(Mode.m_test_i2c_test_sw_enable_data, 0, Mode.m_test_i2c_test_sw_enable_data.Length);
			Mode.m_default_idle_data[7] = 192;
			Mode.m_default_idle_data[10] = 10;
			Mode.m_default_idle_data[11] = 255;
			Mode.m_default_idle_data[16] = 1;
			Mode.m_default_idle_data[17] = 32;
			Mode.m_default_idle_data[23] = 1;
			Mode.m_default_idle_data[24] = 7;
			Mode.m_default_i2c_m_data[7] = 192;
			Mode.m_default_i2c_m_data[10] = 10;
			Mode.m_default_i2c_m_data[11] = 255;
			Mode.m_default_i2c_m_data[15] = 1;
			Mode.m_default_i2c_m_data[16] = 49;
			Mode.m_default_i2c_m_data[17] = 32;
			Mode.m_default_i2c_m_data[23] = 3;
			Mode.m_default_i2c_m_data[24] = 6;
			Mode.m_default_i2c_m_data[30] = 49;
			Mode.m_default_spi_m_data[7] = 192;
			Mode.m_default_spi_m_data[10] = 10;
			Mode.m_default_spi_m_data[11] = 255;
			Mode.m_default_spi_m_data[15] = 2;
			Mode.m_default_spi_m_data[16] = 33;
			Mode.m_default_spi_m_data[17] = 32;
			Mode.m_default_spi_m_data[19] = 176;
			Mode.m_default_spi_m_data[20] = 44;
			Mode.m_default_spi_m_data[23] = 0;
			Mode.m_default_spi_m_data[24] = 3;
			Mode.m_default_spi_m_data[29] = 0;
			Mode.m_default_spi_m_data[30] = 255;
			Mode.m_default_spi_s_data[7] = 192;
			Mode.m_default_spi_s_data[10] = 10;
			Mode.m_default_spi_s_data[11] = 255;
			Mode.m_default_spi_s_data[15] = 3;
			Mode.m_default_spi_s_data[16] = 33;
			Mode.m_default_spi_s_data[17] = 32;
			Mode.m_default_spi_s_data[19] = 176;
			Mode.m_default_spi_s_data[20] = 44;
			Mode.m_default_spi_s_data[23] = 0;
			Mode.m_default_spi_s_data[24] = 3;
			Mode.m_default_spi_s_data[29] = 0;
			Mode.m_default_spi_s_data[30] = 255;
			Mode.m_default_uwire_m_data[7] = 192;
			Mode.m_default_uwire_m_data[10] = 10;
			Mode.m_default_uwire_m_data[11] = 255;
			Mode.m_default_uwire_m_data[15] = 11;
			Mode.m_default_uwire_m_data[16] = 33;
			Mode.m_default_uwire_m_data[17] = 32;
			Mode.m_default_uwire_m_data[19] = 176;
			Mode.m_default_uwire_m_data[20] = 44;
			Mode.m_default_uwire_m_data[23] = 0;
			Mode.m_default_uwire_m_data[24] = 131;
			Mode.m_default_uwire_m_data[29] = 0;
			Mode.m_default_uwire_m_data[30] = 255;
			Mode.m_default_usart_a_data[7] = 192;
			Mode.m_default_usart_a_data[10] = 10;
			Mode.m_default_usart_a_data[11] = 255;
			Mode.m_default_usart_a_data[15] = 4;
			Mode.m_default_usart_a_data[16] = 33;
			Mode.m_default_usart_a_data[17] = 32;
			Mode.m_default_usart_a_data[23] = 1;
			Mode.m_default_usart_a_data[24] = 0;
			Mode.m_default_usart_a_data[25] = 15;
			Mode.m_default_usart_a_data[30] = 16;
			Mode.m_default_usart_sm_data[7] = 192;
			Mode.m_default_usart_sm_data[10] = 10;
			Mode.m_default_usart_sm_data[11] = 255;
			Mode.m_default_usart_sm_data[15] = 5;
			Mode.m_default_usart_sm_data[16] = 33;
			Mode.m_default_usart_sm_data[17] = 32;
			Mode.m_default_usart_sm_data[23] = 1;
			Mode.m_default_usart_sm_data[24] = 0;
			Mode.m_default_usart_sm_data[25] = 15;
			Mode.m_default_usart_sm_data[30] = 16;
			Mode.m_default_usart_ss_data[7] = 192;
			Mode.m_default_usart_ss_data[10] = 10;
			Mode.m_default_usart_ss_data[11] = 255;
			Mode.m_default_usart_ss_data[15] = 6;
			Mode.m_default_usart_ss_data[16] = 33;
			Mode.m_default_usart_ss_data[17] = 32;
			Mode.m_default_usart_ss_data[23] = 1;
			Mode.m_default_usart_ss_data[24] = 0;
			Mode.m_default_usart_ss_data[25] = 15;
			Mode.m_default_usart_ss_data[30] = 16;
			Mode.m_default_i2c_s_data[7] = 192;
			Mode.m_default_i2c_s_data[10] = 10;
			Mode.m_default_i2c_s_data[11] = 255;
			Mode.m_default_i2c_s_data[15] = 7;
			Mode.m_default_i2c_s_data[16] = 33;
			Mode.m_default_i2c_s_data[17] = 32;
			Mode.m_default_i2c_s_data[23] = 1;
			Mode.m_default_i2c_s_data[24] = 255;
			Mode.m_default_i2c_s_data[25] = 6;
			Mode.m_default_i2c_s_data[27] = 0;
			Mode.m_default_i2c_s_data[28] = 0;
			Mode.m_default_i2c_s_data[29] = 0;
			Mode.m_default_i2c_s_data[30] = 0;
			Mode.m_default_i2c_bbm_data[7] = 192;
			Mode.m_default_i2c_bbm_data[10] = 10;
			Mode.m_default_i2c_bbm_data[11] = 255;
			Mode.m_default_i2c_bbm_data[15] = 8;
			Mode.m_default_i2c_bbm_data[16] = 33;
			Mode.m_default_i2c_bbm_data[17] = 32;
			Mode.m_default_i2c_bbm_data[23] = 3;
			Mode.m_default_i2c_bbm_data[24] = 6;
			Mode.m_default_i2c_bbm_data[30] = 127;
			Mode.m_default_i2c_sbbm_data[7] = 192;
			Mode.m_default_i2c_sbbm_data[10] = 10;
			Mode.m_default_i2c_sbbm_data[11] = 255;
			Mode.m_default_i2c_sbbm_data[15] = 9;
			Mode.m_default_i2c_sbbm_data[16] = 33;
			Mode.m_default_i2c_sbbm_data[17] = 32;
			Mode.m_default_i2c_sbbm_data[23] = 3;
			Mode.m_default_i2c_sbbm_data[24] = 6;
			Mode.m_default_i2c_sbbm_data[30] = 127;
			Mode.m_default_LIN_data[7] = 192;
			Mode.m_default_LIN_data[10] = 53;
			Mode.m_default_LIN_data[11] = 124;
			Mode.m_default_LIN_data[15] = 10;
			Mode.m_default_LIN_data[16] = 51;
			Mode.m_default_LIN_data[17] = 32;
			Mode.m_default_LIN_data[23] = 200;
			Mode.m_default_LIN_data[24] = 152;
			Mode.m_default_LIN_data[25] = 15;
			Mode.m_default_LIN_data[29] = 243;
			Mode.m_default_LIN_data[30] = 1;
			Mode.m_default_LIN_no_autobaud_data[7] = 192;
			Mode.m_default_LIN_no_autobaud_data[10] = 53;
			Mode.m_default_LIN_no_autobaud_data[11] = 124;
			Mode.m_default_LIN_no_autobaud_data[15] = 10;
			Mode.m_default_LIN_no_autobaud_data[16] = 51;
			Mode.m_default_LIN_no_autobaud_data[17] = 32;
			Mode.m_default_LIN_no_autobaud_data[23] = 72;
			Mode.m_default_LIN_no_autobaud_data[24] = 152;
			Mode.m_default_LIN_no_autobaud_data[25] = 15;
			Mode.m_default_LIN_no_autobaud_data[29] = 243;
			Mode.m_default_LIN_no_autobaud_data[30] = 1;
			Mode.m_default_mtouch2_data[7] = 192;
			Mode.m_default_mtouch2_data[10] = 10;
			Mode.m_default_mtouch2_data[11] = 255;
			Mode.m_default_mtouch2_data[15] = 12;
			Mode.m_default_mtouch2_data[16] = 49;
			Mode.m_default_mtouch2_data[17] = 32;
			Mode.m_default_mtouch2_data[23] = 3;
			Mode.m_default_mtouch2_data[24] = 6;
			Mode.m_default_mtouch2_data[30] = 49;
			Mode.m_test_5_volt_src_data[7] = 192;
			Mode.m_test_5_volt_src_data[10] = 10;
			Mode.m_test_5_volt_src_data[11] = 255;
			Mode.m_test_5_volt_src_data[15] = 1;
			Mode.m_test_5_volt_src_data[16] = 113;
			Mode.m_test_5_volt_src_data[17] = 32;
			Mode.m_test_5_volt_src_data[19] = 255;
			Mode.m_test_5_volt_src_data[20] = 63;
			Mode.m_test_5_volt_src_data[23] = 3;
			Mode.m_test_5_volt_src_data[24] = 6;
			Mode.m_test_5_volt_src_data[30] = 127;
			Mode.m_test_2p5_volt_src_data[7] = 192;
			Mode.m_test_2p5_volt_src_data[10] = 10;
			Mode.m_test_2p5_volt_src_data[11] = 255;
			Mode.m_test_2p5_volt_src_data[15] = 1;
			Mode.m_test_2p5_volt_src_data[16] = 113;
			Mode.m_test_2p5_volt_src_data[17] = 32;
			Mode.m_test_2p5_volt_src_data[19] = 120;
			Mode.m_test_2p5_volt_src_data[20] = 30;
			Mode.m_test_2p5_volt_src_data[23] = 3;
			Mode.m_test_2p5_volt_src_data[24] = 6;
			Mode.m_test_2p5_volt_src_data[30] = 127;
			Mode.m_test_0_volt_src_data[7] = 192;
			Mode.m_test_0_volt_src_data[10] = 10;
			Mode.m_test_0_volt_src_data[11] = 255;
			Mode.m_test_0_volt_src_data[15] = 1;
			Mode.m_test_0_volt_src_data[16] = 113;
			Mode.m_test_0_volt_src_data[17] = 32;
			Mode.m_test_0_volt_src_data[23] = 3;
			Mode.m_test_0_volt_src_data[24] = 6;
			Mode.m_test_0_volt_src_data[30] = 127;
			Mode.m_test_5_volt_src_no_pullup_data[7] = 192;
			Mode.m_test_5_volt_src_no_pullup_data[10] = 10;
			Mode.m_test_5_volt_src_no_pullup_data[11] = 255;
			Mode.m_test_5_volt_src_no_pullup_data[15] = 1;
			Mode.m_test_5_volt_src_no_pullup_data[16] = 97;
			Mode.m_test_5_volt_src_no_pullup_data[17] = 32;
			Mode.m_test_5_volt_src_no_pullup_data[19] = 255;
			Mode.m_test_5_volt_src_no_pullup_data[20] = 63;
			Mode.m_test_5_volt_src_no_pullup_data[23] = 3;
			Mode.m_test_5_volt_src_no_pullup_data[24] = 6;
			Mode.m_test_5_volt_src_no_pullup_data[30] = 127;
			Mode.m_test_i2c_axout_a11_a20_data[7] = 192;
			Mode.m_test_i2c_axout_a11_a20_data[10] = 10;
			Mode.m_test_i2c_axout_a11_a20_data[11] = 255;
			Mode.m_test_i2c_axout_a11_a20_data[15] = 1;
			Mode.m_test_i2c_axout_a11_a20_data[16] = 49;
			Mode.m_test_i2c_axout_a11_a20_data[17] = 32;
			Mode.m_test_i2c_axout_a11_a20_data[23] = 3;
			Mode.m_test_i2c_axout_a11_a20_data[24] = 6;
			Mode.m_test_i2c_axout_a11_a20_data[28] = 1;
			Mode.m_test_i2c_axout_a11_a20_data[30] = 127;
			Mode.m_test_i2c_axout_a10_a21_data[7] = 192;
			Mode.m_test_i2c_axout_a10_a21_data[10] = 160;
			Mode.m_test_i2c_axout_a10_a21_data[11] = 255;
			Mode.m_test_i2c_axout_a10_a21_data[15] = 1;
			Mode.m_test_i2c_axout_a10_a21_data[16] = 49;
			Mode.m_test_i2c_axout_a10_a21_data[17] = 32;
			Mode.m_test_i2c_axout_a10_a21_data[23] = 3;
			Mode.m_test_i2c_axout_a10_a21_data[24] = 6;
			Mode.m_test_i2c_axout_a10_a21_data[28] = 2;
			Mode.m_test_i2c_axout_a10_a21_data[30] = 127;
			Mode.m_test_i2c_axout_a11_a21_data[7] = 192;
			Mode.m_test_i2c_axout_a11_a21_data[10] = 160;
			Mode.m_test_i2c_axout_a11_a21_data[11] = 255;
			Mode.m_test_i2c_axout_a11_a21_data[15] = 1;
			Mode.m_test_i2c_axout_a11_a21_data[16] = 49;
			Mode.m_test_i2c_axout_a11_a21_data[17] = 32;
			Mode.m_test_i2c_axout_a11_a21_data[23] = 3;
			Mode.m_test_i2c_axout_a11_a21_data[24] = 6;
			Mode.m_test_i2c_axout_a11_a21_data[28] = 3;
			Mode.m_test_i2c_axout_a11_a21_data[30] = 127;
			Mode.m_test_i2c_test_sw_enable_data[7] = 192;
			Mode.m_test_i2c_test_sw_enable_data[10] = 160;
			Mode.m_test_i2c_test_sw_enable_data[8] = 1;
			Mode.m_test_i2c_test_sw_enable_data[10] = 0;
			Mode.m_test_i2c_test_sw_enable_data[11] = 255;
			Mode.m_test_i2c_test_sw_enable_data[15] = 1;
			Mode.m_test_i2c_test_sw_enable_data[16] = 49;
			Mode.m_test_i2c_test_sw_enable_data[17] = 32;
			Mode.m_test_i2c_test_sw_enable_data[23] = 3;
			Mode.m_test_i2c_test_sw_enable_data[24] = 6;
			Mode.m_test_i2c_test_sw_enable_data[30] = 127;
		}

		public static void update_status_packet_data(int p_index, ref byte[] p_status_packet_data)
		{
			Device.Set_Script_Timeout_Option(true);
			switch (p_index)
			{
				case 0:
					for (int i = 7; i < Mode.m_default_idle_data.Length; i++)
					{
						p_status_packet_data[i] = Mode.m_default_idle_data[i];
					}
					return;
				case 1:
					for (int j = 7; j < Mode.m_default_idle_data.Length; j++)
					{
						p_status_packet_data[j] = Mode.m_default_i2c_m_data[j];
					}
					return;
				case 2:
					for (int k = 7; k < Mode.m_default_idle_data.Length; k++)
					{
						p_status_packet_data[k] = Mode.m_default_spi_m_data[k];
					}
					return;
				case 3:
					for (int l = 7; l < Mode.m_default_idle_data.Length; l++)
					{
						p_status_packet_data[l] = Mode.m_default_spi_s_data[l];
					}
					return;
				case 4:
					for (int m = 7; m < Mode.m_default_idle_data.Length; m++)
					{
						p_status_packet_data[m] = Mode.m_default_usart_a_data[m];
					}
					return;
				case 5:
					for (int n = 7; n < Mode.m_default_idle_data.Length; n++)
					{
						p_status_packet_data[n] = Mode.m_default_usart_sm_data[n];
					}
					return;
				case 6:
					for (int num = 7; num < Mode.m_default_idle_data.Length; num++)
					{
						p_status_packet_data[num] = Mode.m_default_usart_ss_data[num];
					}
					return;
				case 7:
					for (int num2 = 7; num2 < Mode.m_default_idle_data.Length; num2++)
					{
						p_status_packet_data[num2] = Mode.m_default_i2c_s_data[num2];
					}
					return;
				case 8:
					for (int num3 = 7; num3 < Mode.m_default_idle_data.Length; num3++)
					{
						p_status_packet_data[num3] = Mode.m_default_i2c_bbm_data[num3];
					}
					return;
				case 9:
					for (int num4 = 7; num4 < Mode.m_default_idle_data.Length; num4++)
					{
						p_status_packet_data[num4] = Mode.m_default_i2c_sbbm_data[num4];
					}
					return;
				case 10:
					Device.Set_Script_Timeout_Option(false);
					for (int num5 = 7; num5 < Mode.m_default_idle_data.Length; num5++)
					{
						p_status_packet_data[num5] = Mode.m_default_LIN_data[num5];
					}
					return;
				case 11:
					for (int num6 = 7; num6 < Mode.m_default_idle_data.Length; num6++)
					{
						p_status_packet_data[num6] = Mode.m_default_uwire_m_data[num6];
					}
					return;
				case 12:
				case 13:
				case 14:
				case 15:
				case 16:
				case 17:
				case 18:
					break;
				case 19:
					Device.Set_Script_Timeout_Option(false);
					for (int num7 = 7; num7 < Mode.m_default_idle_data.Length; num7++)
					{
						p_status_packet_data[num7] = Mode.m_default_LIN_no_autobaud_data[num7];
					}
					return;
				case 20:
					for (int num8 = 7; num8 < Mode.m_default_idle_data.Length; num8++)
					{
						p_status_packet_data[num8] = Mode.m_test_5_volt_src_data[num8];
					}
					return;
				case 21:
					for (int num9 = 7; num9 < Mode.m_default_idle_data.Length; num9++)
					{
						p_status_packet_data[num9] = Mode.m_test_2p5_volt_src_data[num9];
					}
					return;
				case 22:
					for (int num10 = 7; num10 < Mode.m_default_idle_data.Length; num10++)
					{
						p_status_packet_data[num10] = Mode.m_test_0_volt_src_data[num10];
					}
					return;
				case 23:
					for (int num11 = 7; num11 < Mode.m_default_idle_data.Length; num11++)
					{
						p_status_packet_data[num11] = Mode.m_test_5_volt_src_no_pullup_data[num11];
					}
					return;
				case 24:
					for (int num12 = 7; num12 < Mode.m_default_idle_data.Length; num12++)
					{
						p_status_packet_data[num12] = Mode.m_test_i2c_axout_a11_a20_data[num12];
					}
					return;
				case 25:
					for (int num13 = 7; num13 < Mode.m_default_idle_data.Length; num13++)
					{
						p_status_packet_data[num13] = Mode.m_test_i2c_axout_a10_a21_data[num13];
					}
					return;
				case 26:
					for (int num14 = 7; num14 < Mode.m_default_idle_data.Length; num14++)
					{
						p_status_packet_data[num14] = Mode.m_test_i2c_axout_a11_a21_data[num14];
					}
					return;
				case 27:
					for (int num15 = 7; num15 < Mode.m_default_idle_data.Length; num15++)
					{
						p_status_packet_data[num15] = Mode.m_test_i2c_test_sw_enable_data[num15];
					}
					break;
				default:
					return;
			}
		}
	}
}
