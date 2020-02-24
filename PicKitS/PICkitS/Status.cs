using System;

namespace PICkitS
{
	public class Status
	{
		private const int CBUF_START_INDEX = 53;

		public static bool There_Is_A_Status_Error(ref uint p_error)
		{
			p_error = 0u;
			Utilities.m_flags.g_status_packet_mutex.WaitOne();
			p_error = (uint)Constants.STATUS_PACKET_DATA[32];
			p_error += (uint)((uint)Constants.STATUS_PACKET_DATA[36] << 8);
			p_error += (uint)((uint)Constants.STATUS_PACKET_DATA[44] << 16);
			Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
			return p_error != 0u;
		}

		private static void calculate_and_display_I2CM_bit_rate(ref double p_rate, ref string p_units)
		{
			Utilities.m_flags.g_status_packet_mutex.WaitOne();
			p_rate = 20000.0 / (4.0 * ((double)Constants.STATUS_PACKET_DATA[51] + 1.0));
			Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
			p_units = " kbps";
		}

		private static void calculate_and_display_USART_baud_rate(ref double p_rate, ref string p_units)
		{
			Utilities.m_flags.g_status_packet_mutex.WaitOne();
			int num = (int)Constants.STATUS_PACKET_DATA[50] + ((int)Constants.STATUS_PACKET_DATA[51] << 8);
			Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
			p_rate = 20.0 / (4.0 * ((double)num + 1.0));
			p_units = "BAUD";
		}

		private static void calculate_and_display_SPI_bit_rate(ref double p_rate, ref string p_units)
		{
			p_units = "Mhz";
			p_rate = 0.0;
			Utilities.m_flags.g_status_packet_mutex.WaitOne();
			byte b = Constants.STATUS_PACKET_DATA[50];
			double num;
			switch (b)
			{
				case 0:
					num = 8.0;
					break;
				case 1:
					num = 32.0;
					break;
				default:
					if (b != 16)
					{
						num = 0.0;
					}
					else
					{
						num = 128.0;
					}
					break;
			}
			double num2 = (double)(Constants.STATUS_PACKET_DATA[51] + 1);
			Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
			if (num == 0.0)
			{
				p_rate = 0.0;
				return;
			}
			p_rate = 20.0 / num / num2;
			if (num2 > 125.0)
			{
				p_rate *= 1000.0;
				p_units = "Khz";
			}
		}
	}
}
