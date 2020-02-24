using System;
using System.Diagnostics;

namespace PICkitS
{
	public class mTouchLV
	{
		public const byte NUM_SENSORS = 11;

		public const byte SLAVE_ADDR = 66;

		public const byte KEYPAD_SELECTOR = 130;

		public const byte MODE_SELECTOR = 134;

		public const byte READ_BUFFER_IS_READY = 136;

		public const byte READ_BUFFER = 144;

		public const byte WRITE_DEFAULTSETTINGS = 146;

		public static bool InitializePksaForMtouchLV()
		{
			return Device.Initialize_PICkitSerial() && I2CM.Configure_PICkitSerial_For_I2CMaster() && Device.Set_Buffer_Flush_Parameters(true, true, 10, 5.0);
		}

		public static void Cleanup()
		{
			Device.Cleanup();
		}

		internal static bool ReadRawSensorData2(int p_num_reads, ref ushort[] p_raw_data_array, ref long[] p_time_array)
		{
			bool result = true;
			byte[] array = new byte[44];
			Stopwatch stopwatch = new Stopwatch();
			long[] array2 = new long[p_num_reads];
			stopwatch.Reset();
			stopwatch.Start();
			for (int i = 0; i < p_num_reads; i++)
			{
				if (!mTouchCap.ReadRawAvg(66, 0, 11, ref array))
				{
					result = false;
					break;
				}
				array2[i] = stopwatch.ElapsedMilliseconds;
				if (i == 0)
				{
					p_time_array[i] = 0L;
				}
				else
				{
					p_time_array[i] = array2[i] - array2[i - 1];
				}
				for (int j = 0; j < 22; j += 2)
				{
					p_raw_data_array[j / 2 + i * 11] = (ushort)((int)array[j + 1] + ((int)array[j] << 8));
				}
			}
			stopwatch.Stop();
			return result;
		}

		internal static bool ReadRawSensorData2(int p_num_reads, ref string[] p_data_array)
		{
			bool result = true;
			ushort[] array = new ushort[11 * p_num_reads];
			long[] array2 = new long[p_num_reads];
			if (mTouchLV.ReadRawSensorData2(p_num_reads, ref array, ref array2))
			{
				for (int i = 0; i < p_num_reads; i++)
				{
					string text = "";
					for (int j = 0; j < 11; j++)
					{
						string text2 = string.Format("{0}, ", array[j + i * 11]);
						text += text2;
					}
					text = string.Format("{0}, {1}", array2[i], text);
					p_data_array[i] = text;
				}
				result = true;
			}
			return result;
		}

		public static bool SelectKeypad(byte p_selection)
		{
			bool result = false;
			byte[] array = new byte[300];
			byte b = 1;
			byte[] array2 = new byte[1];
			Array.Clear(array, 0, array.Length);
			if (p_selection > 1)
			{
				return result;
			}
			array[0] = 0;
			array[1] = 3;
			array[2] = 15;
			array[3] = 129;
			array[4] = 132;
			array[5] = 3;
			array[6] = 66;
			array[7] = 130;
			array[8] = p_selection;
			array[9] = 131;
			array[10] = 132;
			array[11] = 1;
			array[12] = 67;
			array[13] = 137;
			array[14] = b;
			array[15] = 130;
			array[16] = 31;
			array[17] = 119;
			array[18] = 0;
			USBRead.Clear_Data_Array((uint)b);
			USBRead.Clear_Raw_Data_Array();
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_data_arrived_event.WaitOne(50, false);
				if (flag2 && USBRead.Retrieve_Data(ref array2, (uint)b) && array2[0] == p_selection)
				{
					result = true;
				}
			}
			return result;
		}

		public static bool WriteDefaultSettings()
		{
			bool result = false;
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 8;
			array[3] = 129;
			array[4] = 132;
			array[5] = 2;
			array[6] = 66;
			array[7] = 146;
			array[8] = 130;
			array[9] = 31;
			array[10] = 119;
			array[11] = 0;
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			if (flag)
			{
				result = true;
			}
			return result;
		}

		public static bool ReadBufferIsReady()
		{
			bool result = false;
			byte[] array = new byte[300];
			byte b = 1;
			byte[] array2 = new byte[1];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 14;
			array[3] = 129;
			array[4] = 132;
			array[5] = 2;
			array[6] = 66;
			array[7] = 136;
			array[8] = 131;
			array[9] = 132;
			array[10] = 1;
			array[11] = 67;
			array[12] = 137;
			array[13] = b;
			array[14] = 130;
			array[15] = 31;
			array[16] = 119;
			array[17] = 0;
			USBRead.Clear_Data_Array((uint)b);
			USBRead.Clear_Raw_Data_Array();
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_data_arrived_event.WaitOne(50, false);
				if (flag2 && USBRead.Retrieve_Data(ref array2, (uint)b) && array2[0] == 1)
				{
					result = true;
				}
			}
			return result;
		}

		public static bool SelectMode(byte p_mode)
		{
			bool result = false;
			byte[] array = new byte[300];
			byte b = 1;
			byte[] array2 = new byte[1];
			Array.Clear(array, 0, array.Length);
			if (p_mode > 2)
			{
				return result;
			}
			array[0] = 0;
			array[1] = 3;
			array[2] = 15;
			array[3] = 129;
			array[4] = 132;
			array[5] = 3;
			array[6] = 66;
			array[7] = 134;
			array[8] = p_mode;
			array[9] = 131;
			array[10] = 132;
			array[11] = 1;
			array[12] = 67;
			array[13] = 137;
			array[14] = b;
			array[15] = 130;
			array[16] = 31;
			array[17] = 119;
			array[18] = 0;
			USBRead.Clear_Data_Array((uint)b);
			USBRead.Clear_Raw_Data_Array();
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_data_arrived_event.WaitOne(50, false);
				if (flag2 && USBRead.Retrieve_Data(ref array2, (uint)b) && array2[0] == p_mode)
				{
					result = true;
				}
			}
			return result;
		}

		public static bool ReadBuffer(ref byte[] p_data_array)
		{
			bool result = false;
			byte[] array = new byte[300];
			byte b = 69;
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 14;
			array[3] = 129;
			array[4] = 132;
			array[5] = 2;
			array[6] = 66;
			array[7] = 144;
			array[8] = 131;
			array[9] = 132;
			array[10] = 1;
			array[11] = 67;
			array[12] = 137;
			array[13] = b;
			array[14] = 130;
			array[15] = 31;
			array[16] = 119;
			array[17] = 0;
			USBRead.Clear_Data_Array((uint)b);
			USBRead.Clear_Raw_Data_Array();
			bool flag = USBWrite.Send_Script_To_PICkitS(ref array);
			if (flag)
			{
				bool flag2 = Utilities.m_flags.g_data_arrived_event.WaitOne(200, false);
				if (flag2 && USBRead.Retrieve_Data(ref p_data_array, (uint)b))
				{
					result = true;
				}
			}
			return result;
		}

		public static bool ReadRawSensorData(int p_num_reads, ref string[] p_data_array, ref string p_status_str)
		{
			ushort[] array = new ushort[80];
			byte[] array2 = new byte[80];
			bool result = true;
			p_status_str = "Successful read";
			int num = 0;
			if (p_num_reads != 0)
			{
				int num2 = p_num_reads / 3 + 1;
				for (int i = 0; i < num2; i++)
				{
					int num3 = 0;
					while (num < 100 && !mTouchLV.ReadBufferIsReady())
					{
						num++;
					}
					if (mTouchLV.ReadBufferIsReady())
					{
						num = 0;
						Array.Clear(array2, 0, array2.Length);
						if (mTouchLV.ReadBuffer(ref array2))
						{
							if (array2[0] == 1)
							{
								for (int j = 0; j < 66; j += 2)
								{
									array[j / 2] = (ushort)((int)array2[j + 3] + ((int)array2[j + 4] << 8));
								}
								int num4 = (int)array2[1] + ((int)array2[2] << 8);
								for (int k = 0; k < 3; k++)
								{
									int num5 = i * 3 + k;
									if (num5 >= p_num_reads)
									{
										break;
									}
									string text = string.Format("{0},", num4);
									for (int l = 0; l < 11; l++)
									{
										string text2 = string.Format("{0},", array[l + num3]);
										text += text2;
									}
									p_data_array[num5] = text;
									num4++;
									num3 += 11;
								}
							}
							else
							{
								p_status_str = "1st byte not=1\n";
								result = false;
							}
						}
						else
						{
							p_status_str = "ReadBuffer function failed\n";
							result = false;
						}
					}
					else
					{
						p_status_str = "ReadBuffer not ready\n";
						result = false;
					}
				}
				return result;
			}
			if (mTouchLV.WriteDefaultSettings())
			{
				p_status_str = "PASS";
				return true;
			}
			p_status_str = "FAIL";
			return false;
		}
	}
}
