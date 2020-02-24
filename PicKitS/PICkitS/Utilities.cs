using System;
using System.Runtime.InteropServices;
using System.Threading;
using USBInterface;

namespace PICkitS
{
	public class Utilities
	{
		public struct FLAGS
		{
			public ushort orbl;

			public ushort irbl;

			public IntPtr HID_write_handle;

			public IntPtr HID_read_handle;

			public USBDevice HID_Handle;
			public bool HID_DeviceReady;

			public byte[] write_buffer;

			public byte[] read_buffer;

			public byte[] bl_buffer;

			public Mutex g_status_packet_mutex;

			public AutoResetEvent g_status_packet_data_update_event;

			public AutoResetEvent g_data_arrived_event;

			public AutoResetEvent g_bl_data_arrived_event;

			public AutoResetEvent g_special_status_request_event;

			internal AutoResetEvent g_PKSA_has_completed_script;

			public volatile bool g_need_to_copy_bl_data;
		}

		public enum COMM_MODE
		{
			IDLE,
			I2C_M,
			SPI_M,
			SPI_S,
			USART_A,
			USART_SM,
			USART_SS,
			I2C_S,
			I2C_BBM,
			I2C_SBBM,
			LIN,
			UWIRE,
			MTOUCH2,
			CM_ERROR
		}

		internal enum I2CS_MODE
		{
			DEFAULT,
			INTERACTIVE,
			AUTO
		}

		public struct OVERLAPPED
		{
			public int Internal;

			public int InternalHigh;

			public int Offset;

			public int OffsetHigh;

			public int hEvent;
		}

		public struct SECURITY_ATTRIBUTES
		{
			public int nLength;

			public int lpSecurityDescriptor;

			public int bInheritHandle;
		}

		public enum ThreadAccess
		{
			TERMINATE = 1,
			SUSPEND_RESUME,
			GET_CONTEXT = 8,
			SET_CONTEXT = 16,
			SET_INFORMATION = 32,
			QUERY_INFORMATION = 64,
			SET_THREAD_TOKEN = 128,
			IMPERSONATE = 256,
			DIRECT_IMPERSONATION = 512
		}

		public static Utilities.FLAGS m_flags;

		public static Utilities.COMM_MODE g_comm_mode;

		internal static Utilities.I2CS_MODE g_i2cs_mode;

		public static int AddTwo(int p_int1, int p_int2)
		{
			return p_int1 + p_int2;
		}

		/*
		[DllImport("User32.dll")]
		public static extern int MessageBox(int h, string m, string c, int type);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern int CreateEvent(ref Utilities.SECURITY_ATTRIBUTES SecurityAttributes, int bManualReset, int bInitialState, string lpName);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool WriteFile(IntPtr hFile, byte[] Buffer, int numBytesToWrite, ref int numBytesWritten, int Overlapped);

		[DllImport("kernel32", SetLastError = true)]
		public static extern bool ReadFile(IntPtr hFile, byte[] Buffer, int NumberOfBytesToRead, ref int pNumberOfBytesRead, int Overlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int ReadFileEx(IntPtr hFile, byte[] lpBuffer, int nNumberOfBytesToRead, ref Utilities.OVERLAPPED lpOverlapped, int lpCompletionRoutine);

		[DllImport("kernel32.dll")]
		public static extern int CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll")]
		public static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenThread(Utilities.ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

		[DllImport("kernel32.dll")]
		public static extern int WaitForSingleObject(int hHandle, int dwMilliseconds);
		*/

		public static void InitializeParams()
		{
			Utilities.m_flags.HID_write_handle = IntPtr.Zero;
			Utilities.m_flags.HID_read_handle = IntPtr.Zero;
			Utilities.m_flags.write_buffer = new byte[65];
			Utilities.m_flags.read_buffer = new byte[65];
			Utilities.m_flags.bl_buffer = new byte[65];
			Utilities.m_flags.orbl = 65;
			Utilities.m_flags.irbl = 65;
			Utilities.m_flags.g_status_packet_mutex = new Mutex(false);
			Utilities.g_comm_mode = Utilities.COMM_MODE.IDLE;
			Utilities.g_i2cs_mode = Utilities.I2CS_MODE.DEFAULT;
			Constants.STATUS_PACKET_DATA = new byte[65];
			Mode.configure_run_mode_arrays();
			Utilities.m_flags.g_status_packet_data_update_event = new AutoResetEvent(false);
			Utilities.m_flags.g_data_arrived_event = new AutoResetEvent(false);
			Utilities.m_flags.g_bl_data_arrived_event = new AutoResetEvent(false);
			Utilities.m_flags.g_PKSA_has_completed_script = new AutoResetEvent(false);
			Utilities.m_flags.g_special_status_request_event = new AutoResetEvent(false);
			USBWrite.Initialize_Write_Objects();
			USBRead.Initialize_Read_Objects();
		}

		public static int Convert_Value_To_Int(string p_value)
		{
			if (p_value == "")
			{
				return 0;
			}
			uint[] array = new uint[]
			{
				0u,
				0u,
				2147483648u,
				1073741824u,
				536870912u,
				268435456u,
				134217728u,
				67108864u,
				33554432u,
				16777216u,
				8388608u,
				4194304u,
				2097152u,
				1048576u,
				524288u,
				262144u,
				131072u,
				65536u,
				32768u,
				16384u,
				8192u,
				4096u,
				2048u,
				1024u,
				512u,
				256u,
				128u,
				64u,
				32u,
				16u,
				8u,
				4u,
				2u,
				1u
			};
			uint[] array2 = new uint[]
			{
				0u,
				0u,
				268435456u,
				16777216u,
				1048576u,
				65536u,
				4096u,
				256u,
				16u,
				1u
			};
			int num = 0;
			if (p_value[0] == '\0')
			{
				num = 0;
			}
			else if (p_value[0] == 'Y' || p_value[0] == 'y')
			{
				num = 1;
			}
			else if (p_value[0] == 'N' || p_value[0] == 'n')
			{
				num = 0;
			}
			else if (p_value.Length > 1)
			{
				if ((p_value[0] == '0' && (p_value[1] == 'b' || p_value[1] == 'B')) || p_value[0] == 'b' || p_value[0] == 'B')
				{
					if (p_value.Length > 36)
					{
						num = 0;
					}
					else
					{
						int num2 = p_value.Length - 1;
						int num3;
						if (p_value[0] == '0')
						{
							num3 = 2;
						}
						else
						{
							num3 = 1;
						}
						for (int i = num3; i <= num2; i++)
						{
							int num4;
							if (p_value[i] == '1')
							{
								num4 = 1;
							}
							else
							{
								num4 = 0;
							}
							num += (int)(array[i + 34 - p_value.Length] * (uint)num4);
						}
					}
				}
				else if (p_value[0] == '0' && (p_value[1] == 'x' || p_value[1] == 'X'))
				{
					if (p_value.Length > 12)
					{
						num = 0;
					}
					else
					{
						int num2 = p_value.Length - 1;
						int i = 2;
						while (i <= num2)
						{
							char c = p_value[i];
							int num4;
							switch (c)
							{
								case 'A':
									goto IL_1C8;
								case 'B':
									goto IL_1CD;
								case 'C':
									goto IL_1D2;
								case 'D':
									goto IL_1D7;
								case 'E':
									goto IL_1DC;
								case 'F':
									goto IL_1E1;
								default:
									switch (c)
									{
										case 'a':
											goto IL_1C8;
										case 'b':
											goto IL_1CD;
										case 'c':
											goto IL_1D2;
										case 'd':
											goto IL_1D7;
										case 'e':
											goto IL_1DC;
										case 'f':
											goto IL_1E1;
										default:
											{
												string text = p_value[i].ToString();
												if (!int.TryParse(text, out num4))
												{
													num4 = 0;
												}
												break;
											}
									}
									break;
							}
						IL_206:
							num += (int)(array2[i + 10 - p_value.Length] * (uint)num4);
							i++;
							continue;
						IL_1C8:
							num4 = 10;
							goto IL_206;
						IL_1CD:
							num4 = 11;
							goto IL_206;
						IL_1D2:
							num4 = 12;
							goto IL_206;
						IL_1D7:
							num4 = 13;
							goto IL_206;
						IL_1DC:
							num4 = 14;
							goto IL_206;
						IL_1E1:
							num4 = 15;
							goto IL_206;
						}
					}
				}
				else if (!int.TryParse(p_value, out num))
				{
					num = 0;
				}
			}
			else if (!int.TryParse(p_value, out num))
			{
				num = 0;
			}
			return num;
		}

		public static bool This_Is_A_Valid_Number(string p_text)
		{
			bool result = false;
			if (p_text.Length > 0)
			{
				char[] array = "0123456789aAbBcCdDeEfFxX".ToCharArray();
				result = true;
				for (int i = 0; i < p_text.Length; i++)
				{
					if (p_text.LastIndexOfAny(array, i, 1) < 0)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		public static void Set_Comm_Mode(byte p_comm_mode, byte p_i2cs_mode)
		{
			switch (p_comm_mode)
			{
				case 0:
					Utilities.g_comm_mode = Utilities.COMM_MODE.IDLE;
					return;
				case 1:
					Utilities.g_comm_mode = Utilities.COMM_MODE.I2C_M;
					return;
				case 2:
					Utilities.g_comm_mode = Utilities.COMM_MODE.SPI_M;
					return;
				case 3:
					Utilities.g_comm_mode = Utilities.COMM_MODE.SPI_S;
					return;
				case 4:
					Utilities.g_comm_mode = Utilities.COMM_MODE.USART_A;
					return;
				case 5:
					Utilities.g_comm_mode = Utilities.COMM_MODE.USART_SM;
					return;
				case 6:
					Utilities.g_comm_mode = Utilities.COMM_MODE.USART_SS;
					return;
				case 7:
					Utilities.g_comm_mode = Utilities.COMM_MODE.I2C_S;
					Utilities.g_i2cs_mode = (Utilities.I2CS_MODE)p_i2cs_mode;
					return;
				case 8:
					Utilities.g_comm_mode = Utilities.COMM_MODE.I2C_BBM;
					return;
				case 9:
					Utilities.g_comm_mode = Utilities.COMM_MODE.I2C_SBBM;
					return;
				case 10:
					Utilities.g_comm_mode = Utilities.COMM_MODE.LIN;
					return;
				case 11:
					Utilities.g_comm_mode = Utilities.COMM_MODE.UWIRE;
					return;
				case 12:
					Utilities.g_comm_mode = Utilities.COMM_MODE.MTOUCH2;
					return;
				default:
					Utilities.g_comm_mode = Utilities.COMM_MODE.CM_ERROR;
					return;
			}
		}

		public static byte calculate_crc8(byte p_data, byte p_start_crc)
		{
			uint num = (uint)((p_data ^ p_start_crc) & 255);
			for (int i = 0; i < 8; i++)
			{
				if ((num & 128u) == 128u)
				{
					num = (num * 2u ^ 263u);
				}
				else
				{
					num *= 2u;
				}
			}
			return (byte)(num & 255u);
		}
	}
}
