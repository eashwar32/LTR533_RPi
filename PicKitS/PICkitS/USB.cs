using System;
using System.Runtime.InteropServices;
using USBInterface;

namespace PICkitS
{
	public class USB
	{
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

		/*
		[DllImport("hid.dll")]
		public static extern void HidD_GetHidGuid(ref Guid HidGuid);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, string Enumerator, int hwndParent, int Flags);

		[DllImport("setupapi.dll")]
		public static extern int SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, int DeviceInfoData, ref Guid InterfaceClassGuid, int MemberIndex, ref USB.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto)]
		public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref USB.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, IntPtr DeviceInfoData);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, ref USB.SECURITY_ATTRIBUTES lpSecurityAttributes, int dwCreationDisposition, uint dwFlagsAndAttributes, int hTemplateFile);

		[DllImport("hid.dll")]
		public static extern int HidD_GetAttributes(IntPtr HidDeviceObject, ref USB.HIDD_ATTRIBUTES Attributes);

		[DllImport("hid.dll")]
		public static extern bool HidD_GetPreparsedData(IntPtr HidDeviceObject, ref IntPtr PreparsedData);

		[DllImport("hid.dll")]
		public static extern int HidP_GetCaps(IntPtr PreparsedData, ref USB.HIDP_CAPS Capabilities);

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
		*/

		public static bool Find_Our_Device(ushort p_VendorID, ushort p_PoductID)
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr zero2 = IntPtr.Zero;
			Guid empty = Guid.Empty;
			string text = "";
			ushort num = 0;
			return USB.Get_This_Device(p_VendorID, p_PoductID, 0, ref zero, ref zero2, ref text, false, ref empty, ref num);
		}

		public static bool Find_Our_Device(ushort p_VendorID, ushort p_PoductID, ushort p_index, ref IntPtr p_ReadHandle, ref IntPtr p_WriteHandle, ref string p_devicepath, ref Guid p_HidGuid)
		{
			ushort num = 0;
			return USB.Get_This_Device(p_VendorID, p_PoductID, p_index, ref p_ReadHandle, ref p_WriteHandle, ref p_devicepath, true, ref p_HidGuid, ref num);
		}

		internal static ushort Count_Attached_PKSA(ushort p_VendorID, ushort p_PoductID, ushort p_index, ref IntPtr p_ReadHandle, ref IntPtr p_WriteHandle, ref string p_devicepath, bool p_pass_ptr_to_handle, ref Guid p_HidGuid)
		{
			IntPtr deviceInfoSet = IntPtr.Zero;
			IntPtr arg_0B_0 = IntPtr.Zero;
			ushort num = 0;
			IntPtr intPtr = IntPtr.Zero;
			int num2 = 0;
			USB.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = default(USB.SECURITY_ATTRIBUTES);
			IntPtr intPtr2 = new IntPtr(-1);
			sECURITY_ATTRIBUTES.lpSecurityDescriptor = 0;
			sECURITY_ATTRIBUTES.bInheritHandle = Convert.ToInt32(true);
			sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
			Guid empty = Guid.Empty;
			USB.SP_DEVICE_INTERFACE_DATA sP_DEVICE_INTERFACE_DATA;
			sP_DEVICE_INTERFACE_DATA.cbSize = 0;
			sP_DEVICE_INTERFACE_DATA.Flags = 0;
			sP_DEVICE_INTERFACE_DATA.InterfaceClassGuid = Guid.Empty;
			sP_DEVICE_INTERFACE_DATA.Reserved = 0;
			USB.SP_DEVICE_INTERFACE_DETAIL_DATA sP_DEVICE_INTERFACE_DETAIL_DATA;
			sP_DEVICE_INTERFACE_DETAIL_DATA.cbSize = 0;
			sP_DEVICE_INTERFACE_DETAIL_DATA.DevicePath = "";
			USB.HIDD_ATTRIBUTES hIDD_ATTRIBUTES;
			hIDD_ATTRIBUTES.ProductID = 0;
			hIDD_ATTRIBUTES.Size = 0;
			hIDD_ATTRIBUTES.VendorID = 0;
			hIDD_ATTRIBUTES.VersionNumber = 0;
			sECURITY_ATTRIBUTES.lpSecurityDescriptor = 0;
			sECURITY_ATTRIBUTES.bInheritHandle = Convert.ToInt32(true);
			sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);

			/*
			USB.HidD_GetHidGuid(ref empty);
			deviceInfoSet = USB.SetupDiGetClassDevs(ref empty, null, 0, 18);
			sP_DEVICE_INTERFACE_DATA.cbSize = Marshal.SizeOf(sP_DEVICE_INTERFACE_DATA);
			for (int i = 0; i < 30; i++)
			{
				int num3 = USB.SetupDiEnumDeviceInterfaces(deviceInfoSet, 0, ref empty, i, ref sP_DEVICE_INTERFACE_DATA);
				if (num3 != 0)
				{
					USB.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref sP_DEVICE_INTERFACE_DATA, IntPtr.Zero, 0, ref num2, IntPtr.Zero);
					sP_DEVICE_INTERFACE_DETAIL_DATA.cbSize = Marshal.SizeOf(sP_DEVICE_INTERFACE_DETAIL_DATA);
					IntPtr intPtr3 = Marshal.AllocHGlobal(num2);
					Marshal.WriteInt32(intPtr3, 4 + Marshal.SystemDefaultCharSize);
					USB.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref sP_DEVICE_INTERFACE_DATA, intPtr3, num2, ref num2, IntPtr.Zero);
					IntPtr intPtr4 = new IntPtr(intPtr3.ToInt32() + 4);
					string lpFileName = Marshal.PtrToStringAuto(intPtr4);
					intPtr = USB.CreateFile(lpFileName, 3221225472u, 3u, ref sECURITY_ATTRIBUTES, 3, 0u, 0);
					if (intPtr != intPtr2)
					{
						hIDD_ATTRIBUTES.Size = Marshal.SizeOf(hIDD_ATTRIBUTES);
						num3 = USB.HidD_GetAttributes(intPtr, ref hIDD_ATTRIBUTES);
						if (num3 != 0)
						{
							if (hIDD_ATTRIBUTES.VendorID == p_VendorID && hIDD_ATTRIBUTES.ProductID == p_PoductID)
							{
								num += 1;
								USB.CloseHandle(intPtr);
							}
							else
							{
								USB.CloseHandle(intPtr);
							}
						}
						else
						{
							USB.CloseHandle(intPtr);
						}
					}
				}
			}
			USB.SetupDiDestroyDeviceInfoList(deviceInfoSet);
			*/
			return num;
		}

		public static bool Get_This_Device(ushort p_VendorID, ushort p_PoductID, ushort p_index, ref IntPtr p_ReadHandle, ref IntPtr p_WriteHandle, ref string p_devicepath, bool p_pass_ptr_to_handle, ref Guid p_HidGuid, ref ushort p_num_devices_attached)
		{
			Utilities.InitializeParams();
			LIN.initialize_LIN_frames();
			IntPtr deviceInfoSet = IntPtr.Zero;
			IntPtr zero = IntPtr.Zero;
			USB.HIDP_CAPS hIDP_CAPS = default(USB.HIDP_CAPS);
			ushort num = 0;
			IntPtr intPtr = IntPtr.Zero;
			int num2 = 0;
			USB.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = default(USB.SECURITY_ATTRIBUTES);
			IntPtr intPtr2 = new IntPtr(-1);
			sECURITY_ATTRIBUTES.lpSecurityDescriptor = 0;
			sECURITY_ATTRIBUTES.bInheritHandle = Convert.ToInt32(true);
			sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
			Guid empty = Guid.Empty;
			USB.SP_DEVICE_INTERFACE_DATA sP_DEVICE_INTERFACE_DATA;
			sP_DEVICE_INTERFACE_DATA.cbSize = 0;
			sP_DEVICE_INTERFACE_DATA.Flags = 0;
			sP_DEVICE_INTERFACE_DATA.InterfaceClassGuid = Guid.Empty;
			sP_DEVICE_INTERFACE_DATA.Reserved = 0;
			USB.SP_DEVICE_INTERFACE_DETAIL_DATA sP_DEVICE_INTERFACE_DETAIL_DATA;
			sP_DEVICE_INTERFACE_DETAIL_DATA.cbSize = 0;
			sP_DEVICE_INTERFACE_DETAIL_DATA.DevicePath = "";
			USB.HIDD_ATTRIBUTES hIDD_ATTRIBUTES;
			hIDD_ATTRIBUTES.ProductID = 0;
			hIDD_ATTRIBUTES.Size = 0;
			hIDD_ATTRIBUTES.VendorID = 0;
			hIDD_ATTRIBUTES.VersionNumber = 0;
			bool result = false;
			sECURITY_ATTRIBUTES.lpSecurityDescriptor = 0;
			sECURITY_ATTRIBUTES.bInheritHandle = Convert.ToInt32(true);
			sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);


			/*
			USB.HidD_GetHidGuid(ref empty);
			deviceInfoSet = USB.SetupDiGetClassDevs(ref empty, null, 0, 18);
			sP_DEVICE_INTERFACE_DATA.cbSize = Marshal.SizeOf(sP_DEVICE_INTERFACE_DATA);
			for (int i = 0; i < 30; i++)
			{
				int num3 = USB.SetupDiEnumDeviceInterfaces(deviceInfoSet, 0, ref empty, i, ref sP_DEVICE_INTERFACE_DATA);
				if (num3 != 0)
				{
					USB.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref sP_DEVICE_INTERFACE_DATA, IntPtr.Zero, 0, ref num2, IntPtr.Zero);
					sP_DEVICE_INTERFACE_DETAIL_DATA.cbSize = Marshal.SizeOf(sP_DEVICE_INTERFACE_DETAIL_DATA);
					IntPtr intPtr3 = Marshal.AllocHGlobal(num2);
					Marshal.WriteInt32(intPtr3, 4 + Marshal.SystemDefaultCharSize);
					USB.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref sP_DEVICE_INTERFACE_DATA, intPtr3, num2, ref num2, IntPtr.Zero);
					IntPtr intPtr4 = new IntPtr(intPtr3.ToInt32() + 4);
					string text = Marshal.PtrToStringAuto(intPtr4);
					intPtr = USB.CreateFile(text, 3221225472u, 3u, ref sECURITY_ATTRIBUTES, 3, 0u, 0);
					if (intPtr != intPtr2)
					{
						hIDD_ATTRIBUTES.Size = Marshal.SizeOf(hIDD_ATTRIBUTES);
						num3 = USB.HidD_GetAttributes(intPtr, ref hIDD_ATTRIBUTES);
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
									//Utilities.m_flags.HID_write_handle = intPtr;
									USB.HidD_GetPreparsedData(intPtr, ref zero);
									USB.HidP_GetCaps(zero, ref hIDP_CAPS);
									Utilities.m_flags.irbl = (ushort)hIDP_CAPS.InputReportByteLength;
									//Utilities.m_flags.HID_read_handle = USB.CreateFile(text, 3221225472u, 3u, ref sECURITY_ATTRIBUTES, 3, 0u, 0);
									if (p_pass_ptr_to_handle)
									{
										//p_ReadHandle = Utilities.m_flags.HID_read_handle;
									}
									USB.HidD_FreePreparsedData(ref zero);
									break;
								}
								num += 1;
							}
							else
							{
								result = false;
								USB.CloseHandle(intPtr);
							}
						}
						else
						{
							result = false;
							USB.CloseHandle(intPtr);
						}
					}
				}
			}
			USB.SetupDiDestroyDeviceInfoList(deviceInfoSet);
			p_num_devices_attached = num;
			*/
			Utilities.m_flags.irbl = 65;
			 if(DeviceScanner.ScanOnce(p_VendorID, p_PoductID))
			 {
				Utilities.m_flags.HID_Handle = new USBDevice(p_VendorID, p_PoductID, null, false, 65);
				Utilities.m_flags.HID_DeviceReady = true;
				result = true;
			 }
			 else
			 {
				Utilities.m_flags.HID_DeviceReady = false;
				result = false;
			 }
			return result;
		}
	}
}
