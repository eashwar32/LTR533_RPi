using System;
using System.Threading;

namespace PICkitS
{
	public class Bootloader
	{
		private const int m_upper_fw_addr = 32768;

		private const int m_lower_fw_addr = 8192;

		public static bool Retrieve_BL_FW_Version_Cmd(ref ushort p_bl_ver)
		{
			bool result = false;
			byte[] array = new byte[65];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 118;
			Utilities.m_flags.g_need_to_copy_bl_data = true;
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_bl_data_arrived_event.WaitOne(1000, false);
				if (flag2 && Utilities.m_flags.bl_buffer[1] == 118)
				{
					p_bl_ver = (ushort)(((int)Utilities.m_flags.bl_buffer[7] << 8) + (int)Utilities.m_flags.bl_buffer[8]);
					result = true;
				}
			}
			return result;
		}

		public static bool Read_One_BL_Flash_USB_Packet(ushort p_addr, byte p_byte_count, ref byte[] p_data)
		{
			bool result = false;
			byte[] array = new byte[65];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 1;
			array[2] = p_byte_count;
			array[3] = (byte)p_addr;
			array[4] = (byte)(p_addr >> 8);
			array[5] = 0;
			Utilities.m_flags.g_need_to_copy_bl_data = true;
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_bl_data_arrived_event.WaitOne(1000, false);
				if (flag2 && Utilities.m_flags.bl_buffer[1] == 1 && Utilities.m_flags.bl_buffer[2] == p_byte_count)
				{
					for (int i = 6; i < (int)(6 + p_byte_count); i++)
					{
						p_data[i - 6] = Utilities.m_flags.bl_buffer[i];
					}
					result = true;
				}
			}
			return result;
		}

		public static bool Read_BL_Config_Data(ref byte[] p_data)
		{
			byte b = 14;
			bool result = false;
			byte[] array = new byte[65];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 6;
			array[2] = b;
			array[3] = 0;
			array[4] = 0;
			array[5] = 48;
			Utilities.m_flags.g_need_to_copy_bl_data = true;
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_bl_data_arrived_event.WaitOne(1000, false);
				if (flag2 && Utilities.m_flags.bl_buffer[1] == 6 && Utilities.m_flags.bl_buffer[2] == b)
				{
					for (int i = 6; i < (int)(6 + b); i++)
					{
						p_data[i - 6] = Utilities.m_flags.bl_buffer[i];
					}
					result = true;
				}
			}
			return result;
		}

		public static bool Read_BL_Flash(ushort p_start_addr, int p_byte_count, ref byte[] p_data)
		{
			bool result = true;
			bool flag = false;
			int num = 0;
			byte[] array = new byte[32];
			ushort num2 = p_start_addr;
			int num3 = p_byte_count / 32;
			if (p_byte_count % 32 != 0)
			{
				flag = true;
				num3++;
			}
			for (int i = 0; i < num3; i++)
			{
				byte b;
				if (i == num3 - 1 && flag)
				{
					b = (byte)(p_byte_count % 32);
				}
				else
				{
					b = 32;
				}
				if (!Bootloader.Read_One_BL_Flash_USB_Packet(num2, b, ref array))
				{
					result = false;
					break;
				}
				Thread.Sleep(15);
				for (int j = 0; j < (int)b; j++)
				{
					p_data[num++] = array[j];
				}
				num2 += (ushort)b;
			}
			return result;
		}

		public static bool Write_32_Bytes_to_Flash(ushort p_addr, ref byte[] p_data)
		{
			bool result = false;
			byte[] array = new byte[65];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 2;
			array[2] = 32;
			array[3] = (byte)p_addr;
			array[4] = (byte)(p_addr >> 8);
			array[5] = 0;
			for (int i = 0; i < 32; i++)
			{
				array[6 + i] = p_data[i];
			}
			Utilities.m_flags.g_need_to_copy_bl_data = true;
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_bl_data_arrived_event.WaitOne(1000, false);
				if (flag2 && Utilities.m_flags.bl_buffer[1] == 2 && Utilities.m_flags.bl_buffer[2] == 32 && Utilities.m_flags.bl_buffer[3] == (byte)p_addr)
				{
					result = true;
				}
			}
			return result;
		}

		public static bool Write_BL_Config_Bytes(ref byte[] p_config_data, ref bool[] p_config_bool)
		{
			bool result = true;
			byte[] array = new byte[65];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 7;
			array[2] = 1;
			array[3] = 0;
			array[4] = 0;
			array[5] = 48;
			for (int i = 0; i < p_config_data.Length; i++)
			{
				if (p_config_bool[i])
				{
					array[3] = (byte)i;
					array[6] = p_config_data[i];
					Utilities.m_flags.g_need_to_copy_bl_data = true;
					bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
					if (flag)
					{
						bool flag2 = Utilities.m_flags.g_bl_data_arrived_event.WaitOne(1000, false);
						if (flag2 && (Utilities.m_flags.bl_buffer[1] != 7 || Utilities.m_flags.bl_buffer[2] != 1 || Utilities.m_flags.bl_buffer[3] != (byte)i))
						{
							result = false;
							break;
						}
					}
				}
			}
			return result;
		}

		public static bool Clear_64_Bytes_of_Flash(ushort p_addr)
		{
			bool result = false;
			byte[] array = new byte[65];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 1;
			array[3] = (byte)p_addr;
			array[4] = (byte)(p_addr >> 8);
			array[5] = 0;
			Utilities.m_flags.g_need_to_copy_bl_data = true;
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_bl_data_arrived_event.WaitOne(1000, false);
				if (flag2 && Utilities.m_flags.bl_buffer[1] == 3 && Utilities.m_flags.bl_buffer[2] == 1 && Utilities.m_flags.bl_buffer[3] == (byte)p_addr)
				{
					result = true;
				}
			}
			return result;
		}

		public static bool Clear_64_Bytes_of_Flash_return_fail_info(ushort p_addr, ref byte[] p_data, ref bool p_write_result, ref bool p_wait_result)
		{
			bool result = false;
			bool flag = false;
			byte[] array = new byte[65];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 1;
			array[3] = (byte)p_addr;
			array[4] = (byte)(p_addr >> 8);
			array[5] = 0;
			Utilities.m_flags.g_need_to_copy_bl_data = true;
			bool flag2 = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			if (flag2)
			{
				flag = Utilities.m_flags.g_bl_data_arrived_event.WaitOne(1000, false);
				if (flag)
				{
					if (Utilities.m_flags.bl_buffer[1] == 3 && Utilities.m_flags.bl_buffer[2] == 1 && Utilities.m_flags.bl_buffer[3] == (byte)p_addr)
					{
						result = true;
					}
					else
					{
						for (int i = 0; i < 5; i++)
						{
							p_data[i] = Utilities.m_flags.bl_buffer[i];
						}
					}
				}
			}
			p_write_result = flag2;
			p_wait_result = flag;
			return result;
		}

		public static bool Issue_BL_Reset()
		{
			bool result = false;
			byte[] array = new byte[65];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 255;
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			if (flag)
			{
				result = true;
			}
			return result;
		}

		public static bool Write_BL_Flash(ref byte[] p_data)
		{
			bool result = true;
			int num = 0;
			byte b = 32;
			byte[] array = new byte[(int)b];
			for (int i = 8192; i < 32768; i += (int)b)
			{
				for (int j = 0; j < (int)b; j++)
				{
					array[j] = p_data[num++];
				}
				if (!Bootloader.Write_32_Bytes_to_Flash((ushort)i, ref array))
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public static bool Clear_BL_Flash()
		{
			bool result = true;
			byte b = 64;
			for (int i = 8192; i < 32768; i += (int)b)
			{
				if (!Bootloader.Clear_64_Bytes_of_Flash((ushort)i))
				{
					result = false;
					break;
				}
			}
			return result;
		}
	}
}
