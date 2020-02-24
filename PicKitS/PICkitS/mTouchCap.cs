using System;

namespace PICkitS
{
	public class mTouchCap
	{
		private const byte READ_RAW_AVG_CMD = 3;

		private const byte READ_NUM_SENSORS_CMD = 0;

		private const byte READ_ALL_DATA_CMD = 12;

		private const byte WRITE_SENSOR_DATA = 48;

		private const byte READ_TRIGGER_DATA = 238;

		private const byte READ_DEVTOOL_DATA_CMD = 85;

		private const byte READ_FIRMWARE_OPTIONS_CMD = 148;

		private const byte CHANGE_CSM_DAUGHTER_BOARD_CMD = 150;

		public static bool ReadDevToolData(byte p_slave_addr, byte p_num_bytes, ref byte[] p_data_array)
		{
			string text = "";
			return I2CM.Read(p_slave_addr, 85, p_num_bytes, ref p_data_array, ref text);
		}

		public static bool ReadRawAvg(byte p_slave_addr, byte p_index, byte p_num_sensors, ref byte[] p_data_array)
		{
			bool result = false;
			byte[] array = new byte[300];
			byte b = (byte)(p_num_sensors * 4);
			if (p_num_sensors * 4 > 255)
			{
				return result;
			}
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 16;
			array[3] = 129;
			array[4] = 132;
			array[5] = 4;
			array[6] = p_slave_addr;
			array[7] = 3;
			array[8] = p_index;
			array[9] = p_num_sensors;
			array[10] = 131;
			array[11] = 132;
			array[12] = 1;
			array[13] = (byte)(p_slave_addr + 1);
			array[14] = 137;
			array[15] = b;
			array[16] = 130;
			array[17] = 31;
			array[18] = 119;
			array[19] = 0;
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

		public static bool ReadTriggerData(byte p_slave_addr, ref byte[] p_data_array)
		{
			bool result = false;
			byte[] array = new byte[300];
			byte b = 3;
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 16;
			array[3] = 129;
			array[4] = 132;
			array[5] = 4;
			array[6] = p_slave_addr;
			array[7] = 238;
			array[8] = 17;
			array[9] = 18;
			array[10] = 131;
			array[11] = 132;
			array[12] = 1;
			array[13] = (byte)(p_slave_addr + 1);
			array[14] = 137;
			array[15] = b;
			array[16] = 130;
			array[17] = 31;
			array[18] = 119;
			array[19] = 0;
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

		public static bool ReadAllData(byte p_slave_addr, byte p_index, byte p_num_sensors, ref byte[] p_data_array)
		{
			bool result = false;
			byte[] array = new byte[300];
			byte b = (byte)(p_num_sensors * 8);
			if (p_num_sensors * 8 > 255)
			{
				return false;
			}
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 16;
			array[3] = 129;
			array[4] = 132;
			array[5] = 4;
			array[6] = p_slave_addr;
			array[7] = 12;
			array[8] = p_index;
			array[9] = p_num_sensors;
			array[10] = 131;
			array[11] = 132;
			array[12] = 1;
			array[13] = (byte)(p_slave_addr + 1);
			array[14] = 137;
			array[15] = b;
			array[16] = 130;
			array[17] = 31;
			array[18] = 119;
			array[19] = 0;
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

		public static bool ReadNumSensors(byte p_slave_addr, ref byte p_num_sensors)
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
			array[6] = p_slave_addr;
			array[7] = 0;
			array[8] = 131;
			array[9] = 132;
			array[10] = 1;
			array[11] = (byte)(p_slave_addr + 1);
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
				if (flag2 && USBRead.Retrieve_Data(ref array2, (uint)b))
				{
					result = true;
					p_num_sensors = array2[0];
				}
			}
			return result;
		}

		public static bool ReadFirmwareOptions(byte p_slave_addr, ref byte p_options)
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
			array[6] = p_slave_addr;
			array[7] = 148;
			array[8] = 131;
			array[9] = 132;
			array[10] = 1;
			array[11] = (byte)(p_slave_addr + 1);
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
				if (flag2 && USBRead.Retrieve_Data(ref array2, (uint)b))
				{
					result = true;
					p_options = array2[0];
				}
			}
			return result;
		}

		public static bool ChangeCSMDaughterBoard(byte p_slave_addr, byte p_board)
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
			array[6] = p_slave_addr;
			array[7] = 150;
			array[8] = 131;
			array[9] = 132;
			array[10] = 1;
			array[11] = (byte)(p_slave_addr + 1);
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
				if (flag2 && USBRead.Retrieve_Data(ref array2, (uint)b) && array2[0] == p_board)
				{
					result = true;
				}
			}
			return result;
		}

		public static bool WriteTripGuardband(byte p_slave_addr, byte p_index, ushort p_trip, ushort p_guardband)
		{
			bool result = false;
			byte[] array = new byte[300];
			Array.Clear(array, 0, array.Length);
			array[0] = 0;
			array[1] = 3;
			array[2] = 14;
			array[3] = 129;
			array[4] = 132;
			array[5] = 8;
			array[6] = p_slave_addr;
			array[7] = 48;
			array[8] = p_index;
			array[9] = 1;
			array[10] = (byte)(p_trip >> 8);
			array[11] = (byte)p_trip;
			array[12] = (byte)(p_guardband >> 8);
			array[13] = (byte)p_guardband;
			array[14] = 130;
			array[15] = 31;
			array[16] = 119;
			array[17] = 0;
			USBRead.Clear_Data_Array(0u);
			USBRead.Clear_Raw_Data_Array();
			if (USBWrite.Send_Script_To_PICkitS(ref array))
			{
				result = true;
			}
			return result;
		}
	}
}
