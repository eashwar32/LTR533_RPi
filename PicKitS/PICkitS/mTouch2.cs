using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PICkitS
{
	public class mTouch2
	{
		internal struct BROADCAST_ENABLE_FLAGS
		{
			internal bool trip;

			internal bool guardband;

			internal bool raw;

			internal bool avg;

			internal bool detect_flags;

			internal bool aux1;

			internal bool aux2;

			internal bool status;
		}

		internal struct DATA_STATUS
		{
			internal ushort comm_fw_ver;

			internal ushort touch_fw_ver;

			internal byte hardware_id;

			internal byte max_num_sensors;

			internal byte broadcast_group_id;

			internal mTouch2.BROADCAST_ENABLE_FLAGS broadcast_enable_flags;

			internal ushort time_interval;
		}

		public delegate void Broadcast_All_Data(byte sensor_id, byte num_sensors, ref ushort[] raw, ref ushort[] avg, ref ushort[] trip, ref ushort[] gdbnd, ref byte[] detect);

		public const byte MAX_NUM_SENSORS = 16;

		public const byte NUM_DETECT_BYTES = 5;

		public const ushort AMAD_USB_PRODUCTID = 80;

		internal const byte MT2_END_OF_DATA = 0;

		internal const byte MT2_RESET = 1;

		internal const byte MT2_ARCHIVE = 2;

		internal const byte MT2_RD_STATUS = 17;

		internal const byte MT2_RD_DETECT = 18;

		internal const byte MT2_RD_USERGROUP = 19;

		internal const byte MT2_RD = 20;

		internal const byte MT2_RD_AUTO = 21;

		internal const byte MT2_WR_USERGROUP = 33;

		internal const byte MT2_WR_TRIP = 34;

		internal const byte MT2_WR_GBAND = 35;

		internal const byte MT2_WR_AUX1 = 36;

		internal const byte MT2_WR_AUX2 = 37;

		internal const byte MT2_COMM_TAG_WR_USE_USB = 38;

		internal const byte MT2_DATA_STATUS = 65;

		internal const byte MT2_DATA_DETECT = 66;

		internal const byte MT2_DATA_USERGROUP = 67;

		internal const byte MT2_DATA_TRIP = 68;

		internal const byte MT2_DATA_GBAND = 69;

		internal const byte MT2_DATA_RAW = 70;

		internal const byte MT2_DATA_AVG = 71;

		internal const byte MT2_DATA_AUX1 = 72;

		internal const byte MT2_DATA_AUX2 = 73;

		internal static mTouch2.DATA_STATUS m_data_status = default(mTouch2.DATA_STATUS);

		internal static Mutex m_sensor_data_mutex = new Mutex(false);

		internal static Mutex m_sensor_status_mutex = new Mutex(false);

		internal static AutoResetEvent m_detect_data_is_ready = new AutoResetEvent(false);

		internal static AutoResetEvent m_trip_data_is_ready = new AutoResetEvent(false);

		internal static AutoResetEvent m_gdb_data_is_ready = new AutoResetEvent(false);

		internal static AutoResetEvent m_status_data_is_ready = new AutoResetEvent(false);

		internal static AutoResetEvent m_user_sensor_values_are_ready = new AutoResetEvent(false);

		private static volatile bool m_we_are_broadcasting = false;

		internal static volatile byte m_num_current_sensors = 0;

		internal static volatile byte m_current_sensor_id = 0;

		internal static ushort[] m_raw_values = new ushort[16];

		internal static ushort[] m_avg_values = new ushort[16];

		internal static ushort[] m_trp_values = new ushort[16];

		internal static ushort[] m_gdb_values = new ushort[16];

		internal static ushort[] m_au1_values = new ushort[16];

		internal static ushort[] m_au2_values = new ushort[16];

		internal static byte[] m_user_sensor_values = new byte[17];

		internal static byte[] m_detect_values = new byte[5];

		private static ushort[] m_local_raw_values = new ushort[16];

		private static ushort[] m_local_avg_values = new ushort[16];

		private static ushort[] m_local_trp_values = new ushort[16];

		private static ushort[] m_local_gdb_values = new ushort[16];

		private static byte[] m_local_detect_values = new byte[5];

		public static event mTouch2.Broadcast_All_Data broadcast_all_data;

		/*
		{
			[MethodImpl(32)]
			add
			{
				mTouch2.broadcast_all_data = (mTouch2.Broadcast_All_Data)Delegate.Combine(mTouch2.broadcast_all_data, value);
			}
			[MethodImpl(32)]
			remove
			{
				mTouch2.broadcast_all_data = (mTouch2.Broadcast_All_Data)Delegate.Remove(mTouch2.broadcast_all_data, value);
			}
		}
		*/
		public static bool Configure_PICkitSerial_For_MTouch2()
		{
			return Basic.Configure_PICkitSerial(12, true);
		}

		
		internal static void broadcast_latest_data()
		{
			if (mTouch2.m_we_are_broadcasting)
			{
				mTouch2.m_sensor_data_mutex.WaitOne();
				for (int i = 0; i < (int)mTouch2.m_num_current_sensors; i++)
				{
					mTouch2.m_local_raw_values[i] = mTouch2.m_raw_values[i];
					mTouch2.m_local_avg_values[i] = mTouch2.m_avg_values[i];
					mTouch2.m_local_trp_values[i] = mTouch2.m_trp_values[i];
					mTouch2.m_local_gdb_values[i] = mTouch2.m_gdb_values[i];
				}
				for (int j = 0; j < 5; j++)
				{
					mTouch2.m_local_detect_values[j] = mTouch2.m_detect_values[j];
				}
				m_sensor_data_mutex.ReleaseMutex();
				broadcast_all_data(mTouch2.m_current_sensor_id, mTouch2.m_num_current_sensors, ref mTouch2.m_local_raw_values, ref mTouch2.m_local_avg_values, ref mTouch2.m_local_trp_values, ref mTouch2.m_local_gdb_values, ref mTouch2.m_local_detect_values);
			}
		}

		public static bool Get_Sensor_Data(byte p_sensor_id, byte p_num_sensors, ref ushort[] p_raw, ref ushort[] p_avg, ref ushort[] p_trip, ref ushort[] p_gdbnd, ref byte[] p_detect)
		{
			bool result = false;
			mTouch2.m_detect_data_is_ready.Reset();
			if (mTouch2.Send_MT2_RD_Command(p_sensor_id, true, true, true, true, true, false, false, false))
			{
				bool flag = mTouch2.m_detect_data_is_ready.WaitOne(500, false);
				if (flag && p_sensor_id == mTouch2.m_current_sensor_id && p_num_sensors == mTouch2.m_num_current_sensors)
				{
					mTouch2.m_sensor_data_mutex.WaitOne();
					for (int i = 0; i < (int)mTouch2.m_num_current_sensors; i++)
					{
						p_raw[i] = mTouch2.m_raw_values[i];
						p_avg[i] = mTouch2.m_avg_values[i];
						p_trip[i] = mTouch2.m_trp_values[i];
						p_gdbnd[i] = mTouch2.m_gdb_values[i];
					}
					for (int j = 0; j < 5; j++)
					{
						p_detect[j] = mTouch2.m_detect_values[j];
					}
					mTouch2.m_sensor_data_mutex.ReleaseMutex();
					result = true;
				}
			}
			return result;
		}

		private static bool Send_MT2_RD_Command(byte p_sensor_id, bool p_trip, bool p_gdbnd, bool p_raw, bool p_avg, bool p_detect, bool p_aux1, bool p_aux2, bool p_status)
		{
			byte[] array = new byte[65];
			byte b = 0;
			if (p_trip)
			{
				b |= 1;
			}
			if (p_gdbnd)
			{
				b |= 2;
			}
			if (p_raw)
			{
				b |= 4;
			}
			if (p_avg)
			{
				b |= 8;
			}
			if (p_detect)
			{
				b |= 16;
			}
			if (p_aux1)
			{
				b |= 32;
			}
			if (p_aux2)
			{
				b |= 64;
			}
			if (p_status)
			{
				b |= 128;
			}
			array[0] = 0;
			array[1] = 20;
			array[2] = p_sensor_id;
			array[3] = b;
			array[4] = 0;
			bool result = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			mTouch2.m_we_are_broadcasting = false;
			return result;
		}

		private static bool Send_MT2_RD_AUTO_Command(byte p_sensor_id, bool p_trip, bool p_gdbnd, bool p_raw, bool p_avg, bool p_detect, bool p_aux1, bool p_aux2, bool p_status, ushort p_interval)
		{
			byte[] array = new byte[65];
			byte b = 0;
			if (p_trip)
			{
				b |= 1;
			}
			if (p_gdbnd)
			{
				b |= 2;
			}
			if (p_raw)
			{
				b |= 4;
			}
			if (p_avg)
			{
				b |= 8;
			}
			if (p_detect)
			{
				b |= 16;
			}
			if (p_aux1)
			{
				b |= 32;
			}
			if (p_aux2)
			{
				b |= 64;
			}
			if (p_status)
			{
				b |= 128;
			}
			array[0] = 0;
			array[1] = 21;
			array[2] = p_sensor_id;
			array[3] = b;
			array[4] = (byte)p_interval;
			array[5] = (byte)(p_interval >> 8);
			array[6] = 0;
			bool result = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			if (p_interval == 0)
			{
				mTouch2.m_we_are_broadcasting = false;
			}
			else
			{
				mTouch2.m_we_are_broadcasting = true;
			}
			return result;
		}

		public static bool Send_MT2_RESET_Command()
		{
			byte[] array = new byte[65];
			array[0] = 0;
			array[1] = 1;
			array[2] = 0;
			bool result = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			mTouch2.m_we_are_broadcasting = false;
			return result;
		}

		public static bool Send_MT2_ARCHIVE_Command()
		{
			byte[] array = new byte[65];
			array[0] = 0;
			array[1] = 2;
			array[2] = 0;
			return USBWrite.Send_Data_Packet_To_PICkitS(ref array);
		}

		public static bool Send_MT2_COMM_TAG_WR_USE_USB_Command(bool p_enable)
		{
			byte[] array = new byte[65];
			byte b = 0;
			if (p_enable)
			{
				b = 1;
			}
			array[0] = 0;
			array[1] = 38;
			array[2] = b;
			array[3] = 0;
			return USBWrite.Send_Data_Packet_To_PICkitS(ref array);
		}

		public static bool Create_User_Defined_Sensor_Group(byte p_sensor_count, ref byte[] p_sensor_array)
		{
			byte[] array = new byte[65];
			array[0] = 0;
			array[1] = 33;
			array[2] = p_sensor_count;
			for (int i = 3; i < (int)(p_sensor_count + 3); i++)
			{
				array[i] = p_sensor_array[i - 3];
			}
			array[(int)(p_sensor_count + 3)] = 0;
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			byte b = 0;
			byte[] array2 = new byte[17];
			flag = mTouch2.Read_User_Defined_Sensor_Group(ref b, ref array2);
			if (flag && b == p_sensor_count)
			{
				for (int j = 0; j < (int)p_sensor_count; j++)
				{
					if (array2[j] != p_sensor_array[j])
					{
						flag = false;
						break;
					}
				}
			}
			return flag;
		}

		public static bool Read_User_Defined_Sensor_Group(ref byte p_sensor_count, ref byte[] p_sensor_array)
		{
			byte[] array = new byte[65];
			array[0] = 0;
			array[1] = 19;
			array[2] = 0;
			mTouch2.m_user_sensor_values_are_ready.Reset();
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			if (flag)
			{
				flag = mTouch2.m_user_sensor_values_are_ready.WaitOne(500, false);
				if (flag)
				{
					p_sensor_count = mTouch2.m_user_sensor_values[0];
					for (int i = 0; i < (int)p_sensor_count; i++)
					{
						p_sensor_array[i] = mTouch2.m_user_sensor_values[1 + i];
					}
				}
			}
			return flag;
		}

		internal static bool Send_MT2_RD_STATUS_Command()
		{
			byte[] array = new byte[65];
			bool result = false;
			array[0] = 0;
			array[1] = 17;
			array[2] = 0;
			mTouch2.m_status_data_is_ready.Reset();
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			if (flag)
			{
				result = mTouch2.m_status_data_is_ready.WaitOne(500, false);
			}
			return result;
		}

		public static bool Get_MT2_DATA_STATUS(ref ushort p_comm_fw_ver, ref ushort p_touch_fw_ver, ref byte p_hardware_id, ref byte p_max_num_sensors, ref byte p_broadcast_group_id, ref bool p_trip, ref bool p_gdbnd, ref bool p_raw, ref bool p_avg, ref bool p_detect, ref bool p_aux1, ref bool p_aux2, ref bool p_status, ref ushort p_interval)
		{
			bool result = false;
			if (mTouch2.Send_MT2_RD_STATUS_Command())
			{
				mTouch2.m_sensor_status_mutex.WaitOne();
				p_comm_fw_ver = mTouch2.m_data_status.comm_fw_ver;
				p_touch_fw_ver = mTouch2.m_data_status.touch_fw_ver;
				p_hardware_id = mTouch2.m_data_status.hardware_id;
				p_max_num_sensors = mTouch2.m_data_status.max_num_sensors;
				p_broadcast_group_id = mTouch2.m_data_status.broadcast_group_id;
				p_trip = mTouch2.m_data_status.broadcast_enable_flags.trip;
				p_gdbnd = mTouch2.m_data_status.broadcast_enable_flags.guardband;
				p_raw = mTouch2.m_data_status.broadcast_enable_flags.raw;
				p_avg = mTouch2.m_data_status.broadcast_enable_flags.avg;
				p_detect = mTouch2.m_data_status.broadcast_enable_flags.detect_flags;
				p_aux1 = mTouch2.m_data_status.broadcast_enable_flags.aux1;
				p_aux2 = mTouch2.m_data_status.broadcast_enable_flags.aux2;
				p_status = mTouch2.m_data_status.broadcast_enable_flags.status;
				p_interval = mTouch2.m_data_status.time_interval;
				mTouch2.m_sensor_status_mutex.ReleaseMutex();
				result = true;
			}
			return result;
		}

		public static bool Write_Trip_Value(byte p_sensor_id, ushort p_trip)
		{
			mTouch2.Send_MT2_RESET_Command();
			byte[] array = new byte[65];
			ushort time_interval = 0;
			if (mTouch2.m_we_are_broadcasting && mTouch2.Send_MT2_RD_STATUS_Command())
			{
				time_interval = mTouch2.m_data_status.time_interval;
			}
			array[0] = 0;
			array[1] = 34;
			array[2] = p_sensor_id;
			array[3] = 2;
			array[4] = (byte)p_trip;
			array[5] = (byte)(p_trip >> 8);
			array[6] = 0;
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			if (flag)
			{
				mTouch2.m_trip_data_is_ready.Reset();
				if (mTouch2.Send_MT2_RD_Command(p_sensor_id, true, false, false, false, false, false, false, false))
				{
					flag = mTouch2.m_trip_data_is_ready.WaitOne(500, false);
					if (flag)
					{
						mTouch2.m_sensor_data_mutex.WaitOne();
						if (p_trip != mTouch2.m_trp_values[0])
						{
							flag = false;
						}
						mTouch2.m_sensor_data_mutex.ReleaseMutex();
					}
				}
			}
			if (mTouch2.m_we_are_broadcasting && mTouch2.Send_MT2_RD_STATUS_Command())
			{
				mTouch2.m_data_status.time_interval = time_interval;
				mTouch2.Send_MT2_RD_AUTO_Command(mTouch2.m_data_status.broadcast_group_id, mTouch2.m_data_status.broadcast_enable_flags.trip, mTouch2.m_data_status.broadcast_enable_flags.guardband, mTouch2.m_data_status.broadcast_enable_flags.raw, mTouch2.m_data_status.broadcast_enable_flags.avg, mTouch2.m_data_status.broadcast_enable_flags.detect_flags, mTouch2.m_data_status.broadcast_enable_flags.aux1, mTouch2.m_data_status.broadcast_enable_flags.aux2, mTouch2.m_data_status.broadcast_enable_flags.status, mTouch2.m_data_status.time_interval);
			}
			return flag;
		}

		public static bool Write_Gdbnd_Value(byte p_sensor_id, ushort p_gdbnd)
		{
			mTouch2.Send_MT2_RESET_Command();
			byte[] array = new byte[65];
			ushort time_interval = 0;
			if (mTouch2.m_we_are_broadcasting && mTouch2.Send_MT2_RD_STATUS_Command())
			{
				time_interval = mTouch2.m_data_status.time_interval;
			}
			array[0] = 0;
			array[1] = 35;
			array[2] = p_sensor_id;
			array[3] = 2;
			array[4] = (byte)p_gdbnd;
			array[5] = (byte)(p_gdbnd >> 8);
			array[6] = 0;
			bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
			if (flag)
			{
				mTouch2.m_gdb_data_is_ready.Reset();
				if (mTouch2.Send_MT2_RD_Command(p_sensor_id, false, true, false, false, false, false, false, false))
				{
					flag = mTouch2.m_gdb_data_is_ready.WaitOne(500, false);
					if (flag)
					{
						mTouch2.m_sensor_data_mutex.WaitOne();
						if (p_gdbnd != mTouch2.m_gdb_values[0])
						{
							flag = false;
						}
						mTouch2.m_sensor_data_mutex.ReleaseMutex();
					}
				}
			}
			if (mTouch2.m_we_are_broadcasting && mTouch2.Send_MT2_RD_STATUS_Command())
			{
				mTouch2.m_data_status.time_interval = time_interval;
				mTouch2.Send_MT2_RD_AUTO_Command(mTouch2.m_data_status.broadcast_group_id, mTouch2.m_data_status.broadcast_enable_flags.trip, mTouch2.m_data_status.broadcast_enable_flags.guardband, mTouch2.m_data_status.broadcast_enable_flags.raw, mTouch2.m_data_status.broadcast_enable_flags.avg, mTouch2.m_data_status.broadcast_enable_flags.detect_flags, mTouch2.m_data_status.broadcast_enable_flags.aux1, mTouch2.m_data_status.broadcast_enable_flags.aux2, mTouch2.m_data_status.broadcast_enable_flags.status, mTouch2.m_data_status.time_interval);
			}
			return flag;
		}

		public static bool Get_Trip_and_Gdbnd_Data(byte p_sensor_id, int p_num_sensors, ref ushort[] p_trip, ref ushort[] p_gdbnd)
		{
			bool result = false;
			mTouch2.m_gdb_data_is_ready.Reset();
			if (mTouch2.Send_MT2_RD_Command(p_sensor_id, true, true, false, false, false, false, false, false))
			{
				bool flag = mTouch2.m_gdb_data_is_ready.WaitOne(500, false);
				if (flag)
				{
					mTouch2.m_sensor_data_mutex.WaitOne();
					for (int i = 0; i < p_num_sensors; i++)
					{
						p_trip[i] = mTouch2.m_trp_values[i];
						p_gdbnd[i] = mTouch2.m_gdb_values[i];
					}
					mTouch2.m_sensor_data_mutex.ReleaseMutex();
					result = true;
				}
			}
			return result;
		}
	}
}
