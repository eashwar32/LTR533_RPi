using System;
using System.IO;
using System.Threading;

namespace PICkitS
{
	public class Firmware
	{
		private enum DEVICETYPE
		{
			PKSA,
			LIN
		}

		private const int m_NUM_CONFIG_BYTES = 14;

		private const int m_max_upper_fw_value = 32768;

		private const int m_lower_fw_addr = 8192;

		private static byte[] m_config_bytes_array = new byte[14];

		private static bool[] m_config_bool_array = new bool[14];

		private static int m_bootloader_delay = 30;

		private static int m_upper_fw_addr = 32768;

		private static int m_array_size = 24576;

		private static byte[] m_flash_data_array = new byte[24576];

		private static byte[] m_prior_flash_data_array = new byte[24576];

		private static Firmware.DEVICETYPE m_devicetype;

		public static bool Load_Firmware(string p_file_name, int p_device_type, ref string p_error_str, ref int p_error_code)
		{
			bool result = false;
			if (p_device_type == 2)
			{
				Firmware.m_devicetype = Firmware.DEVICETYPE.LIN;
				Firmware.m_upper_fw_addr = 16384;
			}
			else
			{
				Firmware.m_devicetype = Firmware.DEVICETYPE.PKSA;
				Firmware.m_upper_fw_addr = 32768;
			}
			Firmware.m_array_size = Firmware.m_upper_fw_addr - 8192;
			if (Firmware.parse_hex_file_put_in_array(p_file_name))
			{
				if (Firmware.Force_PKS_Into_Prog_Mode_Cmd())
				{
					bool flag = false;
					byte[] array = new byte[14];
					if (Firmware.Clear_BL_Flash())
					{
						if (!Firmware.Write_BL_Flash(ref Firmware.m_flash_data_array))
						{
							p_error_str = "Error - writing flash";
							flag = true;
							p_error_code = 4;
						}
						else if (!Bootloader.Write_BL_Config_Bytes(ref Firmware.m_config_bytes_array, ref Firmware.m_config_bool_array))
						{
							flag = true;
							p_error_str = "Error - writing config bytes";
							p_error_code = 5;
						}
						else
						{
							Firmware.copy_current_fw_array_into_prior_array();
							if (Firmware.Read_BL_Flash(8192, Firmware.m_upper_fw_addr - 8192, ref Firmware.m_flash_data_array))
							{
								for (int i = 0; i < Firmware.m_array_size; i++)
								{
									if (Firmware.m_flash_data_array[i] != Firmware.m_prior_flash_data_array[i])
									{
										p_error_str = string.Format("Error - Verification error:  flash byte {0:X2} reads as {1:X2}\nbut should be {2:X2}\n", i, Firmware.m_flash_data_array[i], Firmware.m_prior_flash_data_array[i]);
										flag = true;
										p_error_code = 8;
										break;
									}
								}
								if (Bootloader.Read_BL_Config_Data(ref array))
								{
									for (int j = 0; j < array.Length; j++)
									{
										if (Firmware.m_config_bool_array[j] && array[j] != Firmware.m_config_bytes_array[j])
										{
											p_error_str = string.Format("Error - Verification error:  Config byte {0:X2} reads as {1:X2}\nbut should be {2:X2}\n", j, array[j], Firmware.m_config_bytes_array[j]);
											flag = true;
											p_error_code = 9;
											break;
										}
									}
								}
								else
								{
									flag = true;
									p_error_str = "Error - reading config bytes";
									p_error_code = 7;
								}
							}
							else
							{
								flag = true;
								p_error_str = "Error - reading flash.";
								p_error_code = 6;
							}
						}
					}
					else
					{
						flag = true;
						p_error_str = "Error - clearing flash prior to writing firmware";
						p_error_code = 3;
					}
					if (!flag)
					{
						if (Firmware.m_devicetype == Firmware.DEVICETYPE.LIN)
						{
							p_error_str = string.Format("LIN firmware written and verified using hex file {0}.\n", p_file_name);
						}
						else
						{
							p_error_str = string.Format("PICkit Serial firmware written and verified using hex file {0}.\n", p_file_name);
						}
						p_error_code = 0;
						result = true;
					}
					if (Bootloader.Issue_BL_Reset())
					{
						Firmware.restart_the_device();
					}
					else
					{
						p_error_code = 10;
						p_error_str = "Error - sending BL Reset Command.  Firmware may not have been updated correctly.";
						result = false;
					}
				}
				else
				{
					p_error_code = 2;
					p_error_str = "Error - Could not enter programming mode prior to updating firmware - download aborted";
				}
			}
			else
			{
				p_error_code = 1;
				p_error_str = "Error - Could not read hex file - download aborted";
			}
			return result;
		}

		private static void restart_the_device()
		{
			Device.Terminate_Comm_Threads();
			Thread.Sleep(4000);
			if (Firmware.m_devicetype == Firmware.DEVICETYPE.LIN)
			{
				Device.Initialize_MyDevice(0, 2564);
			}
			else
			{
				Device.Initialize_PICkitSerial();
			}
			Thread.Sleep(2000);
		}

		private static void copy_current_fw_array_into_prior_array()
		{
			for (int i = 0; i < Firmware.m_array_size; i++)
			{
				Firmware.m_prior_flash_data_array[i] = Firmware.m_flash_data_array[i];
			}
		}

		private static bool Force_PKS_Into_Prog_Mode_Cmd()
		{
			bool result = false;
			ushort num = 0;
			byte[] array = new byte[65];
			Array.Clear(array, 0, array.Length);
			array[1] = 66;
			array[2] = 0;
			if (Bootloader.Retrieve_BL_FW_Version_Cmd(ref num))
			{
				return true;
			}
			if (USBWrite.Send_Data_Packet_To_PICkitS(ref array))
			{
				Firmware.restart_the_device();
				if (Bootloader.Retrieve_BL_FW_Version_Cmd(ref num))
				{
					result = true;
				}
			}
			return result;
		}

		private static bool Write_BL_Flash(ref byte[] p_data)
		{
			bool result = true;
			int num = 0;
			byte b = 32;
			byte[] array = new byte[(int)b];
			for (int i = 8192; i < Firmware.m_upper_fw_addr; i += (int)b)
			{
				Thread.Sleep(Firmware.m_bootloader_delay);
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

		private static bool Read_BL_Flash(ushort p_start_addr, int p_byte_count, ref byte[] p_data)
		{
			bool result = true;
			bool flag = false;
			int num = 0;
			byte[] array = new byte[58];
			ushort num2 = p_start_addr;
			int num3 = p_byte_count / 58;
			if (p_byte_count % 58 != 0)
			{
				flag = true;
				num3++;
			}
			for (int i = 0; i < num3; i++)
			{
				Thread.Sleep(Firmware.m_bootloader_delay);
				byte b;
				if (i == num3 - 1 && flag)
				{
					b = (byte)(p_byte_count % 58);
				}
				else
				{
					b = 58;
				}
				if (!Bootloader.Read_One_BL_Flash_USB_Packet(num2, b, ref array))
				{
					result = false;
					break;
				}
				for (int j = 0; j < (int)b; j++)
				{
					p_data[num++] = array[j];
				}
				num2 += (ushort)b;
			}
			return result;
		}

		private static bool Clear_BL_Flash()
		{
			bool result = true;
			byte b = 64;
			for (int i = 8192; i < Firmware.m_upper_fw_addr; i += (int)b)
			{
				Thread.Sleep(Firmware.m_bootloader_delay);
				if (!Bootloader.Clear_64_Bytes_of_Flash((ushort)i))
				{
					result = false;
					break;
				}
			}
			return result;
		}

		private static bool parse_hex_file_put_in_array(string p_file_name)
		{
			bool result = true;
			int num = 0;
			if (File.Exists(p_file_name))
			{
				string text = " ,\t";
				text.ToCharArray();
				try
				{
					StreamReader streamReader = File.OpenText(p_file_name);
					for (int i = 0; i < Firmware.m_config_bool_array.Length; i++)
					{
						Firmware.m_config_bool_array[i] = false;
					}
					while (streamReader.Peek() >= 0)
					{
						string text2 = streamReader.ReadLine();
						if (text2 != "" && text2[0] == ':')
						{
							string text3 = "0x";
							string text4 = "0x";
							string text5 = "0x";
							text3 += text2[1];
							text3 += text2[2];
							int num2 = Utilities.Convert_Value_To_Int(text3);
							text4 += text2[3];
							text4 += text2[4];
							text4 += text2[5];
							text4 += text2[6];
							text5 += text2[7];
							text5 += text2[8];
							int num3 = Utilities.Convert_Value_To_Int(text4);
							byte b = (byte)Utilities.Convert_Value_To_Int(text5);
							if (b == 4)
							{
								num++;
							}
							if (num3 >= 8192 && num3 < Firmware.m_upper_fw_addr && b == 0)
							{
								for (int j = 0; j < 2 * num2; j += 2)
								{
									string text6 = "0x";
									text6 += text2[9 + j];
									text6 += text2[10 + j];
									Firmware.m_flash_data_array[num3 + j / 2 - 8192] = (byte)Utilities.Convert_Value_To_Int(text6);
								}
							}
							else if (num >= 2 && num2 > 0 && b == 0)
							{
								for (int k = 0; k < 2 * num2; k += 2)
								{
									string text6 = "0x";
									text6 += text2[9 + k];
									text6 += text2[10 + k];
									Firmware.m_config_bytes_array[num3 + k / 2] = (byte)Utilities.Convert_Value_To_Int(text6);
									Firmware.m_config_bool_array[num3 + k / 2] = true;
								}
							}
						}
					}
					streamReader.Close();
					return result;
				}
				catch (Exception)
				{
					result = false;
					return result;
				}
			}
			result = false;
			return result;
		}
	}
}
