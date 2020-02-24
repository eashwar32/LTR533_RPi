using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace PICkitS
{
	internal class GenericHID
	{
		public delegate void USBNotifier();

		public struct SP_DEVICE_INTERFACE_DATA
		{
			public int cbSize;

			public Guid InterfaceClassGuid;

			public int Flags;

			public int Reserved;
		}

		public struct SP_DEVICE_INTERFACE_DETAIL_DATA
		{
			public int cbSize;

			public string DevicePath;
		}

		public struct SP_DEVINFO_DATA
		{
			public int cbSize;

			public Guid ClassGuid;

			public int DevInst;

			public int Reserved;
		}

		public struct HIDD_ATTRIBUTES
		{
			public int Size;

			public ushort VendorID;

			public ushort ProductID;

			public ushort VersionNumber;
		}

		public struct SECURITY_ATTRIBUTES
		{
			public int nLength;

			public int lpSecurityDescriptor;

			public int bInheritHandle;
		}

		public struct HIDP_CAPS
		{
			public short Usage;

			public short UsagePage;

			public short InputReportByteLength;

			public short OutputReportByteLength;

			public short FeatureReportByteLength;

			[MarshalAs(30, SizeConst = 17)]
			public short[] Reserved;

			public short NumberLinkCollectionNodes;

			public short NumberInputButtonCaps;

			public short NumberInputValueCaps;

			public short NumberInputDataIndices;

			public short NumberOutputButtonCaps;

			public short NumberOutputValueCaps;

			public short NumberOutputDataIndices;

			public short NumberFeatureButtonCaps;

			public short NumberFeatureValueCaps;

			public short NumberFeatureDataIndices;
		}

		private const uint GENERIC_READ = 2147483648u;

		private const uint GENERIC_WRITE = 1073741824u;

		private const uint FILE_SHARE_READ = 1u;

		private const uint FILE_SHARE_WRITE = 2u;

		private const uint FILE_FLAG_OVERLAPPED = 1073741824u;

		private const int INVALID_HANDLE_VALUE = -1;

		private const short OPEN_EXISTING = 3;

		public const int WAIT_TIMEOUT = 258;

		public const int ERROR_IO_PENDING = 997;

		public const short WAIT_OBJECT_0 = 0;

		public const int ERROR_HANDLE_EOF = 38;

		public const int ERROR_IO_INCOMPLETE = 996;

		private const short DIGCF_PRESENT = 2;

		private const short DIGCF_DEVICEINTERFACE = 16;

		private static volatile bool m_we_are_in_read_loop = false;

		private static Thread m_read_thread;

		internal static Mutex m_usb_packet_mutex = new Mutex(false);

		public static AutoResetEvent m_packet_is_copied = null;

		private static byte[] m_read_buffer = new byte[1000];

		public static event GenericHID.USBNotifier USBDataAvailable;

		/*
		{
			[MethodImpl(32)]
			add
			{
				GenericHID.USBDataAvailable = (GenericHID.USBNotifier)Delegate.Combine(GenericHID.USBDataAvailable, value);
			}
			[MethodImpl(32)]
			remove
			{
				GenericHID.USBDataAvailable = (GenericHID.USBNotifier)Delegate.Remove(GenericHID.USBDataAvailable, value);
			}
		}
		*/

		
		/*
		[DllImport("hid.dll")]
		public static extern void HidD_GetHidGuid(ref Guid HidGuid);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, string Enumerator, int hwndParent, int Flags);

		[DllImport("setupapi.dll")]
		public static extern int SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, int DeviceInfoData, ref Guid InterfaceClassGuid, int MemberIndex, ref GenericHID.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto)]
		public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref GenericHID.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, IntPtr DeviceInfoData);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, ref GenericHID.SECURITY_ATTRIBUTES lpSecurityAttributes, int dwCreationDisposition, uint dwFlagsAndAttributes, int hTemplateFile);

		[DllImport("hid.dll")]
		public static extern int HidD_GetAttributes(IntPtr HidDeviceObject, ref GenericHID.HIDD_ATTRIBUTES Attributes);

		[DllImport("hid.dll")]
		public static extern bool HidD_GetPreparsedData(IntPtr HidDeviceObject, ref IntPtr PreparsedData);

		[DllImport("hid.dll")]
		public static extern int HidP_GetCaps(IntPtr PreparsedData, ref GenericHID.HIDP_CAPS Capabilities);

		[DllImport("setupapi.dll")]
		public static extern int SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

		[DllImport("hid.dll")]
		public static extern bool HidD_FreePreparsedData(ref IntPtr PreparsedData);

		[DllImport("kernel32.dll")]
		public static extern int CloseHandle(IntPtr hObject);

		[DllImport("hid.dll")]
		public static extern bool HidD_GetNumInputBuffers(IntPtr HidDeviceObject, ref int NumberBuffers);

		[DllImport("hid.dll")]
		public static extern bool HidD_SetNumInputBuffers(IntPtr HidDeviceObject, int NumberBuffers);

		[DllImport("kernel32.dll")]
		public static extern int GetLastError();

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GetOverlappedResult(IntPtr hFile, ref Utilities.OVERLAPPED lpOverlapped, ref int lpNumberOfBytesTransferred, int bWait);

		[DllImport("kernel32", SetLastError = true)]
		public static extern bool ReadFile(IntPtr hFile, byte[] Buffer, int NumberOfBytesToRead, ref int pNumberOfBytesRead, ref Utilities.OVERLAPPED pOverlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern int ClearCommError(IntPtr hFile, out uint lpErrors, IntPtr lpStat);
		*/

		public static bool Find_HID_Device(ushort p_VendorID, ushort p_PoductID)
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr zero2 = IntPtr.Zero;
			Guid empty = Guid.Empty;
			string text = "";
			ushort num = 0;
			bool flag = GenericHID.Get_HID_Device(p_VendorID, p_PoductID, 0, ref zero, ref zero2, ref text, false, ref empty, ref num);
			if (flag)
			{
				flag = GenericHID.Kick_Off_Read_Thread();
				if (flag)
				{
					flag = USBWrite.kick_off_write_thread();
				}
			}
			return flag;
		}

		public static bool Find_HID_Device(ushort p_VendorID, ushort p_PoductID, ushort p_index, ref IntPtr p_ReadHandle, ref IntPtr p_WriteHandle, ref string p_devicepath, ref Guid p_HidGuid)
		{
			ushort num = 0;
			bool flag = GenericHID.Get_HID_Device(p_VendorID, p_PoductID, p_index, ref p_ReadHandle, ref p_WriteHandle, ref p_devicepath, true, ref p_HidGuid, ref num);
			if (flag)
			{
				flag = GenericHID.Kick_Off_Read_Thread();
				if (flag)
				{
					flag = USBWrite.kick_off_write_thread();
				}
			}
			return flag;
		}

		public static bool Get_HID_Device(ushort p_VendorID, ushort p_PoductID, ushort p_index, ref IntPtr p_ReadHandle, ref IntPtr p_WriteHandle, ref string p_devicepath, bool p_pass_ptr_to_handle, ref Guid p_HidGuid, ref ushort p_num_devices_attached)
		{
			Utilities.InitializeParams();
			LIN.initialize_LIN_frames();
			IntPtr deviceInfoSet = IntPtr.Zero;
			IntPtr zero = IntPtr.Zero;
			GenericHID.HIDP_CAPS hIDP_CAPS = default(GenericHID.HIDP_CAPS);
			ushort num = 0;
			IntPtr intPtr = IntPtr.Zero;
			int num2 = 0;
			GenericHID.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = default(GenericHID.SECURITY_ATTRIBUTES);
			IntPtr intPtr2 = new IntPtr(-1);
			sECURITY_ATTRIBUTES.lpSecurityDescriptor = 0;
			sECURITY_ATTRIBUTES.bInheritHandle = Convert.ToInt32(true);
			sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
			Guid empty = Guid.Empty;
			GenericHID.SP_DEVICE_INTERFACE_DATA sP_DEVICE_INTERFACE_DATA;
			sP_DEVICE_INTERFACE_DATA.cbSize = 0;
			sP_DEVICE_INTERFACE_DATA.Flags = 0;
			sP_DEVICE_INTERFACE_DATA.InterfaceClassGuid = Guid.Empty;
			sP_DEVICE_INTERFACE_DATA.Reserved = 0;
			GenericHID.SP_DEVICE_INTERFACE_DETAIL_DATA sP_DEVICE_INTERFACE_DETAIL_DATA;
			sP_DEVICE_INTERFACE_DETAIL_DATA.cbSize = 0;
			sP_DEVICE_INTERFACE_DETAIL_DATA.DevicePath = "";
			GenericHID.HIDD_ATTRIBUTES hIDD_ATTRIBUTES;
			hIDD_ATTRIBUTES.ProductID = 0;
			hIDD_ATTRIBUTES.Size = 0;
			hIDD_ATTRIBUTES.VendorID = 0;
			hIDD_ATTRIBUTES.VersionNumber = 0;
			bool result = false;
			sECURITY_ATTRIBUTES.lpSecurityDescriptor = 0;
			sECURITY_ATTRIBUTES.bInheritHandle = Convert.ToInt32(true);
			sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
			/*
			GenericHID.HidD_GetHidGuid(ref empty);
			deviceInfoSet = GenericHID.SetupDiGetClassDevs(ref empty, null, 0, 18);
			sP_DEVICE_INTERFACE_DATA.cbSize = Marshal.SizeOf(sP_DEVICE_INTERFACE_DATA);
			for (int i = 0; i < 30; i++)
			{
				int num3 = GenericHID.SetupDiEnumDeviceInterfaces(deviceInfoSet, 0, ref empty, i, ref sP_DEVICE_INTERFACE_DATA);
				if (num3 != 0)
				{
					GenericHID.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref sP_DEVICE_INTERFACE_DATA, IntPtr.Zero, 0, ref num2, IntPtr.Zero);
					sP_DEVICE_INTERFACE_DETAIL_DATA.cbSize = Marshal.SizeOf(sP_DEVICE_INTERFACE_DETAIL_DATA);
					IntPtr intPtr3 = Marshal.AllocHGlobal(num2);
					Marshal.WriteInt32(intPtr3, 4 + Marshal.SystemDefaultCharSize);
					GenericHID.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref sP_DEVICE_INTERFACE_DATA, intPtr3, num2, ref num2, IntPtr.Zero);
					IntPtr intPtr4 = new IntPtr(intPtr3.ToInt32() + 4);
					string text = Marshal.PtrToStringAuto(intPtr4);
					intPtr = GenericHID.CreateFile(text, 3221225472u, 3u, ref sECURITY_ATTRIBUTES, 3, 0u, 0);
					if (intPtr != intPtr2)
					{
						hIDD_ATTRIBUTES.Size = Marshal.SizeOf(hIDD_ATTRIBUTES);
						num3 = GenericHID.HidD_GetAttributes(intPtr, ref hIDD_ATTRIBUTES);
						if (num3 != 0)
						{
							if (hIDD_ATTRIBUTES.VendorID == p_VendorID && hIDD_ATTRIBUTES.ProductID == p_PoductID)
							{
								if (num == p_index)
								{
									result = true;
									if (p_pass_ptr_to_handle)
									{
										p_WriteHandle = intPtr;
									}
									p_devicepath = text;
									p_HidGuid = empty;
								//	Utilities.m_flags.HID_write_handle = intPtr;
									GenericHID.HidD_GetPreparsedData(intPtr, ref zero);
									GenericHID.HidP_GetCaps(zero, ref hIDP_CAPS);
									Utilities.m_flags.irbl = (ushort)hIDP_CAPS.InputReportByteLength;
								//	Utilities.m_flags.HID_read_handle = GenericHID.CreateFile(text, 3221225472u, 3u, ref sECURITY_ATTRIBUTES, 3, 1073741824u, 0);
									if (p_pass_ptr_to_handle)
									{
									//	p_ReadHandle = Utilities.m_flags.HID_read_handle;
									}
									GenericHID.HidD_FreePreparsedData(ref zero);
									break;
								}
								num += 1;
							}
							else
							{
								result = false;
								GenericHID.CloseHandle(intPtr);
							}
						}
						else
						{
							result = false;
							GenericHID.CloseHandle(intPtr);
						}
					}
				}
			}
			GenericHID.SetupDiDestroyDeviceInfoList(deviceInfoSet);
			p_num_devices_attached = num;
			*/
			return result;
		}

		public static void Kill_Read_Thread()
		{
			if (GenericHID.m_read_thread != null && GenericHID.m_read_thread.IsAlive)
			{
				GenericHID.m_we_are_in_read_loop = false;
				GenericHID.m_read_thread.Join();
			}
		}

		public static bool Kick_Off_Read_Thread()
		{
			bool result = true;
			if (!GenericHID.m_we_are_in_read_loop)
			{
				GenericHID.m_read_thread = new Thread(new ThreadStart(GenericHID.Read_USB_Thread));
				GenericHID.m_read_thread.IsBackground=true;
				GenericHID.m_read_thread.Start();
			}
			else
			{
				result = false;
			}
			return result;
		}

		private static void Read_USB_Thread()
		{
			int num = 0;
			int num2 = 0;
			GenericHID.m_we_are_in_read_loop = true;
			Utilities.OVERLAPPED oVERLAPPED = default(Utilities.OVERLAPPED);
			Utilities.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = default(Utilities.SECURITY_ATTRIBUTES);
			sECURITY_ATTRIBUTES.lpSecurityDescriptor = 0;
			sECURITY_ATTRIBUTES.bInheritHandle = Convert.ToInt32(true);
			sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
			//int hEvent = Utilities.CreateEvent(ref sECURITY_ATTRIBUTES, Convert.ToInt32(false), Convert.ToInt32(false), "");
			//oVERLAPPED.hEvent = hEvent;
			while (GenericHID.m_we_are_in_read_loop)
			{
				Array.Clear(GenericHID.m_read_buffer, 0, GenericHID.m_read_buffer.Length);
				oVERLAPPED.Offset = 0;
				oVERLAPPED.OffsetHigh = 0;
				//bool flag = GenericHID.ReadFile(Utilities.m_flags.HID_read_handle, GenericHID.m_read_buffer, (int)Utilities.m_flags.irbl, ref num2, ref oVERLAPPED);
				bool flag = false;
				if (flag)
				{
					if (GenericHID.USBDataAvailable != null)
					{
						GenericHID.USBDataAvailable();
						GenericHID.m_packet_is_copied.WaitOne(1000, false);
					}
				}
				else
				{
					int lastError = 0;// USB.GetLastError();
					int num3 = lastError;
					if (num3 != 38)
					{
						if (num3 != 997)
						{
							num++;
						}
						else
						{
							//sUtilities.WaitForSingleObject(oVERLAPPED.hEvent, 2000);
							//bool overlappedResult = USB.GetOverlappedResult(Utilities.m_flags.HID_read_handle, ref oVERLAPPED, ref num2, 0);
							bool overlappedResult = false;
							if (overlappedResult && GenericHID.USBDataAvailable != null)
							{
								GenericHID.USBDataAvailable();
								GenericHID.m_packet_is_copied.WaitOne(1000, false);
							}
						}
					}
					else
					{
						num++;
					}
				}
			}
			num++;
		}

		public static void Create_Single_Sync_object()
		{
			GenericHID.m_packet_is_copied = new AutoResetEvent(false);
		}

		public static void Get_USB_Data_Packet(ref byte[] p_data)
		{
			for (int i = 0; i < (int)Utilities.m_flags.irbl; i++)
			{
				p_data[i] = GenericHID.m_read_buffer[i];
			}
			GenericHID.m_packet_is_copied.Set();
		}

		public static ushort Get_USB_IRBL()
		{
			return Utilities.m_flags.irbl;
		}

		public static void Cleanup()
		{
			if (GenericHID.m_read_thread.IsAlive)
			{
				GenericHID.Kill_Read_Thread();
			}
			USBWrite.Kill_Write_Thread();
			USBWrite.Dispose_Of_Write_Objects();
			//Utilities.CloseHandle(Utilities.m_flags.HID_write_handle);
			//Utilities.CloseHandle(Utilities.m_flags.HID_read_handle);
			GenericHID.m_usb_packet_mutex.Close();
		}
	}
}
