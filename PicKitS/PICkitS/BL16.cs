using System;
using System.IO;

namespace PICkitS
{
	public class BL16
	{
		public const uint BLOCK_SIZE = 1536u;

		public static bool Erase_Block(byte p_slave_addr, uint p_mem_addr)
		{
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 11;
			array[3] = 129;
			array[4] = 132;
			array[5] = 5;
			array[6] = p_slave_addr;
			array[7] = 0;
			array[8] = (byte)(p_mem_addr >> 16);
			array[9] = (byte)(p_mem_addr >> 8);
			array[10] = (byte)p_mem_addr;
			array[11] = 130;
			array[12] = 31;
			array[13] = 119;
			array[14] = 0;
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Read(byte p_slave_addr, uint p_mem_addr, byte p_num_bytes_to_read, ref byte[] p_data_array)
		{
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 17;
			array[3] = 129;
			array[4] = 132;
			array[5] = 5;
			array[6] = p_slave_addr;
			array[7] = 1;
			array[8] = (byte)(p_mem_addr >> 16);
			array[9] = (byte)(p_mem_addr >> 8);
			array[10] = (byte)p_mem_addr;
			array[11] = 131;
			array[12] = 132;
			array[13] = 1;
			array[14] = (byte)(p_slave_addr + 1);
			array[15] = 137;
			array[16] = p_num_bytes_to_read;
			array[17] = 130;
			array[18] = 31;
			array[19] = 119;
			array[20] = 0;
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

		public static bool Write(byte p_slave_addr, uint p_mem_addr, byte p_num_bytes_to_write, ref byte[] p_data_array)
		{
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = (byte)(12 + p_num_bytes_to_write);
			array[3] = 129;
			array[4] = 132;
			array[5] = (byte)(6 + p_num_bytes_to_write);
			array[6] = p_slave_addr;
			array[7] = 2;
			array[8] = (byte)(p_mem_addr >> 16);
			array[9] = (byte)(p_mem_addr >> 8);
			array[10] = (byte)p_mem_addr;
			array[11] = p_num_bytes_to_write;
			int i;
			for (i = 0; i < (int)p_num_bytes_to_write; i++)
			{
				array[i + 12] = p_data_array[i];
			}
			array[i + 12] = 130;
			array[i + 13] = 31;
			array[i + 14] = 119;
			array[i + 15] = 0;
			USBRead.Clear_Data_Array(0u);
			USBRead.Clear_Raw_Data_Array();
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Program(byte p_slave_addr)
		{
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 8;
			array[3] = 129;
			array[4] = 132;
			array[5] = 2;
			array[6] = p_slave_addr;
			array[7] = 3;
			array[8] = 130;
			array[9] = 31;
			array[10] = 119;
			array[11] = 0;
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Row_Init(byte p_slave_addr)
		{
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 8;
			array[3] = 129;
			array[4] = 132;
			array[5] = 2;
			array[6] = p_slave_addr;
			array[7] = 4;
			array[8] = 130;
			array[9] = 31;
			array[10] = 119;
			array[11] = 0;
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool Read_Status(byte p_slave_addr, ref byte[] p_data_array)
		{
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 14;
			array[3] = 129;
			array[4] = 132;
			array[5] = 2;
			array[6] = p_slave_addr;
			array[7] = 5;
			array[8] = 131;
			array[9] = 132;
			array[10] = 1;
			array[11] = (byte)(p_slave_addr + 1);
			array[12] = 137;
			array[13] = 3;
			array[14] = 130;
			array[15] = 31;
			array[16] = 119;
			array[17] = 0;
			USBRead.Clear_Data_Array(3u);
			USBRead.Clear_Raw_Data_Array();
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			bool result;
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_data_arrived_event.WaitOne(Basic.m_i2cs_read_wait_time, false);
				result = (flag2 && USBRead.Retrieve_Data(ref p_data_array, 3u));
			}
			else
			{
				result = false;
			}
			return result;
		}

		public static bool Issue_Password(byte p_slave_addr)
		{
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 12;
			array[3] = 129;
			array[4] = 132;
			array[5] = 6;
			array[6] = p_slave_addr;
			array[7] = 6;
			array[8] = 0;
			array[9] = 17;
			array[10] = 34;
			array[11] = 51;
			array[12] = 130;
			array[13] = 31;
			array[14] = 119;
			array[15] = 0;
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool LCD_Write(byte p_slave_addr, byte p_LCD, string p_text)
		{
			if (p_text.Length != 16)
			{
				return false;
			}
			byte b;
			if (p_LCD == 1)
			{
				b = 16;
			}
			else
			{
				b = 17;
			}
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 24;
			array[3] = 129;
			array[4] = 132;
			array[5] = 18;
			array[6] = p_slave_addr;
			array[7] = b;
			int i;
			for (i = 0; i < 16; i++)
			{
				array[i + 8] = Convert.ToByte(p_text[i]);
			}
			array[i + 8] = 130;
			array[i + 9] = 31;
			array[i + 10] = 119;
			array[i + 11] = 0;
			USBRead.Clear_Data_Array(0u);
			USBRead.Clear_Raw_Data_Array();
			return USBWrite.Send_Script_To_PICkitS(ref array);
		}

		public static bool BootLoader_Is_Enabled(byte p_slave_addr)
		{
			bool result = false;
			byte[] array = new byte[3];
			byte[] array2 = array;
			if (BL16.Read_Status(p_slave_addr, ref array2) && (array2[2] & 1) == 1)
			{
				result = true;
			}
			return result;
		}

		public static bool Count_Num_Blocks(string p_file_name, ref int p_num_blocks, ref int p_err_code)
		{
			uint num = 0u;
			uint num2 = 0u;
			uint num3 = 0u;
			bool flag = true;
			bool result = true;
			p_err_code = 0;
			if (File.Exists(p_file_name))
			{
				string text = " ,\t";
				text.ToCharArray();
				try
				{
					StreamReader streamReader = File.OpenText(p_file_name);
					p_num_blocks = 0;
					while (streamReader.Peek() >= 0)
					{
						string text2 = streamReader.ReadLine();
						if (text2 == ":00000001FF")
						{
							break;
						}
						if (text2 != "" && text2[0] == ':')
						{
							string text3 = "0x";
							string text4 = "0x";
							string text5 = "0x";
							string text6 = "0x";
							text3 += text2[1];
							text3 += text2[2];
							int num4 = Utilities.Convert_Value_To_Int(text3);
							text4 += text2[3];
							text4 += text2[4];
							text4 += text2[5];
							text4 += text2[6];
							text5 += text2[7];
							text5 += text2[8];
							uint num5 = (uint)Utilities.Convert_Value_To_Int(text4);
							byte b = (byte)Utilities.Convert_Value_To_Int(text5);
							if (b == 4)
							{
								text6 += text2[9];
								text6 += text2[10];
								text6 += text2[11];
								text6 += text2[12];
								num3 = (uint)Utilities.Convert_Value_To_Int(text6);
								num3 <<= 16;
							}
							else
							{
								num5 += num3;
								if ((num5 / 2u > num2 || num5 / 2u < num) && !flag)
								{
									flag = true;
								}
								if (flag)
								{
									p_num_blocks++;
									flag = false;
									if (!BL16.Calculate_Device_Starting_Block_Addr(num5, ref num, ref num2))
									{
										streamReader.Close();
										p_err_code = 1;
										bool result2 = false;
										return result2;
									}
								}
								for (int i = 0; i < 2 * num4; i += 2)
								{
									string text7 = "0x";
									text7 += text2[9 + i];
									text7 += text2[10 + i];
									if (num > num5 / 2u)
									{
										streamReader.Close();
										p_err_code = 1;
										bool result2 = false;
										return result2;
									}
								}
							}
						}
					}
					streamReader.Close();
					return result;
				}
				catch (Exception)
				{
					p_err_code = 2;
					result = false;
					return result;
				}
			}
			result = false;
			p_err_code = 2;
			return result;
		}

		public static bool Calculate_Device_Starting_Block_Addr(uint p_start_file_addr, ref uint p_dev_start_addr, ref uint p_end_dev_start_addr)
		{
			uint num = p_start_file_addr / 2u;
			int num2 = 1024;
			if (num < 3072u)
			{
				return false;
			}
			for (int i = 2; i < 1000; i++)
			{
				if ((long)(i * num2) == (long)((ulong)num))
				{
					p_dev_start_addr = (uint)(i * num2);
					p_end_dev_start_addr = (uint)(i * num2 + num2 - 1);
					return true;
				}
				if ((long)(i * num2) > (long)((ulong)num))
				{
					p_dev_start_addr = (uint)((i - 1) * num2);
					p_end_dev_start_addr = (uint)((i - 1) * num2 + num2 - 1);
					return true;
				}
			}
			return false;
		}

		private bool Read_Block(byte p_slave_addr, uint p_start_addr, ref byte[] p_array)
		{
			byte[] array = new byte[252];
			int num = 0;
			uint num2 = p_start_addr;
			for (int i = 0; i < 6; i++)
			{
				if (!BL16.Read(p_slave_addr, num2, 252, ref array))
				{
					return false;
				}
				for (int j = 0; j < 252; j++)
				{
					p_array[num++] = array[j];
				}
				num2 += 168u;
			}
			if (!BL16.Read(p_slave_addr, num2, 24, ref array))
			{
				return false;
			}
			for (int k = 0; k < 24; k++)
			{
				p_array[num++] = array[k];
			}
			return true;
		}

		private bool Write_Block(byte p_slave_addr, uint p_start_addr, ref byte[] p_array)
		{
			byte[] array = new byte[48];
			uint num = p_start_addr;
			int num2 = 0;
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					for (int k = 0; k < 48; k++)
					{
						array[k] = p_array[num2++];
					}
					if (!BL16.Write(p_slave_addr, num, 48, ref array))
					{
						return false;
					}
					num += 32u;
				}
				if (!BL16.Program(p_slave_addr))
				{
					return false;
				}
			}
			return true;
		}
	}
}
