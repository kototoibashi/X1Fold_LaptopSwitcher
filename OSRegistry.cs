using System;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ModeSwitcher.Utilities
{
	public enum OSTabletModes
	{
		Off,
		On
	}

	public enum AutoRotationStates
	{
		Enabled = 1,
		Disabled = 0
	}

	public class OSRegistry
	{
		public static OSTabletModes TabletModeState
		{
			get
			{
				try
				{
					return (OSTabletModes)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ImmersiveShell", "TabletMode", OSTabletModes.Off);
				}
				catch
				{
					//LogCodess.LOG_ERROR(LogCodess.ERROR_CODE.OSTABLET_REGISTRY_GET_FAILED);
					return OSTabletModes.Off;
				}
			}
		}

		public static int AutoRotationState
		{
			get
			{
				try
				{
					return (int)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AutoRotation", "Enable", 0);
				}
				catch
				{
					//LogCodess.LOG_ERROR(LogCodess.ERROR_CODE.OSAUTOROTATE_REGISTRY_GET_FAILED);
					return 0;
				}
			}
		}

		public static int OSBuildNumber
		{
			get
			{
				try
				{
					return Convert.ToInt32(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuild", 0));
				}
				catch
				{
					return 19043;
				}
			}
		}

		public static int MajorVersionNumber
		{
			get
			{
				try
				{
					return Convert.ToInt32(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentMajorVersionNumber", 0));
				}
				catch
				{
					return 10;
				}
			}
		}

		public static bool IsOSWindows11()
		{
			if (GetCurrentOSVersion().Contains("Windows 11"))
			{
				return true;
			}
			return false;
		}

		public static string GetCurrentOSVersion()
		{
			object obj = (from ManagementObject OS in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get()
						  select OS.GetPropertyValue("Caption")).FirstOrDefault();
			if (obj == null)
			{
				return "Unknown";
			}
			return obj.ToString();
		}

		public static void RevertAutoRotation()
		{
			AutoRotationStates keyValue = AutoRotationStates.Enabled;//(AutoRotationStates)ModeSwitcherRegistry.Instance.GetKeyValue(RegistryType.RotationLockStateWhenKbdUndocked);
			if (AutoRotationStates.Enabled == keyValue)
			{
				EnableAutoRotation();
			}
			else
			{
				DisableAutoRotation();
			}
		}

		public static void DisableAutoRotation()
		{
			Win32.NativeMethods.SetAutoRotation(bEnable: false);
		}

		public static void EnableAutoRotation()
		{
			Win32.NativeMethods.SetAutoRotation(bEnable: true);
		}
	}

	internal static class DeviceEmbeddedDisplay
	{
		public enum DisplayDeviceStateFlags
		{
			AttachedToDesktop = 1,
			MultiDriver = 2,
			PrimaryDevice = 4,
			MirroringDriver = 8,
			VGACompatible = 0x10,
			Removable = 0x20,
			ModesPruned = 0x8000000,
			Remote = 0x4000000,
			Disconnect = 0x2000000
		}

		public struct DISPLAY_DEVICE
		{
			[MarshalAs(UnmanagedType.U4)]
			public int cb;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string DeviceName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string DeviceString;

			[MarshalAs(UnmanagedType.U4)]
			public DisplayDeviceStateFlags StateFlags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string DeviceID;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string DeviceKey;
		}

		public struct DEVMODE
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string dmDeviceName;

			public short dmSpecVersion;

			public short dmDriverVersion;

			public short dmSize;

			public short dmDriverExtra;

			public int dmFields;

			public int dmPositionX;

			public int dmPositionY;

			public int dmDisplayOrientation;

			public int dmDisplayFixedOutput;

			public short dmColor;

			public short dmDuplex;

			public short dmYResolution;

			public short dmTTOption;

			public short dmCollate;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string dmFormName;

			public short dmLogPixels;

			public short dmBitsPerPel;

			public int dmPelsWidth;

			public int dmPelsHeight;

			public int dmDisplayFlags;

			public int dmDisplayFrequency;

			public int dmICMMethod;

			public int dmICMIntent;

			public int dmMediaType;

			public int dmDitherType;

			public int dmReserved1;

			public int dmReserved2;

			public int dmPanningWidth;

			public int dmPanningHeight;
		}

		public enum DeviceSensorOrientations
		{
			Portrait,
			LandscapeFlipped,
			PortraitFlipped,
			Landscape,
			FaceDown
		}

		public enum DISP_CHANGE
		{
			Successful = 0,
			Restart = 1,
			Failed = -1,
			BadMode = -2,
			NotUpdated = -3,
			BadFlags = -4,
			BadParam = -5,
			BadDualView = -6
		}

		public enum DisplaySettingsFlags
		{
			CDS_NONE = 0,
			CDS_UPDATEREGISTRY = 1,
			CDS_TEST = 2,
			CDS_FULLSCREEN = 4,
			CDS_GLOBAL = 8,
			CDS_SET_PRIMARY = 0x10,
			CDS_VIDEOPARAMETERS = 0x20,
			CDS_ENABLE_UNSAFE_MODES = 0x100,
			CDS_DISABLE_UNSAFE_MODES = 0x200,
			CDS_RESET = 0x40000000,
			CDS_RESET_EX = 0x20000000,
			CDS_NORESET = 0x10000000
		}

		private static string _deviceName = null;

		private static double _dpix = 1.0;

		private static double _dpiy = 1.0;

		public static bool _isPrimary = true;

		public static DisplayDeviceStateFlags State
		{
			get
			{
				DISPLAY_DEVICE lpDisplayDevice = default(DISPLAY_DEVICE);
				lpDisplayDevice.cb = Marshal.SizeOf(lpDisplayDevice);
				if (!EnumDisplayDevices(_deviceName, 0u, ref lpDisplayDevice, 0u))
				{
					return (DisplayDeviceStateFlags)0;
				}
				return lpDisplayDevice.StateFlags;
			}
		}

		private static int RawScreenOrientation
		{
			get
			{
				int[] array = new int[4] { 0, 1, 2, 3 };
				DEVMODE lpDevMode = CreateDevmode();
				EnumDisplaySettings(_deviceName, -1, ref lpDevMode);
				//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.GET_EMBEDDED_MONITOR_ORIENTATION, lpDevMode.dmDisplayOrientation);
				return Array.IndexOf((Array)array, (object)lpDevMode.dmDisplayOrientation);
			}
		}

        public static DeviceSensorOrientations CurrentOrientation
        {
            get
            {
                switch(RawScreenOrientation)
                {
					case 0: return DeviceSensorOrientations.Portrait;
					case 1: return DeviceSensorOrientations.LandscapeFlipped;
                    case 2: return DeviceSensorOrientations.PortraitFlipped;
                    case 3: return DeviceSensorOrientations.Landscape;
                    default: return DeviceSensorOrientations.Portrait;
                };
            }
        }

        public static double DPIScalingWithoutUsingHDC => Math.Round((double)GetScreen().Bounds.Width / SystemParameters.PrimaryScreenWidth, 2, MidpointRounding.AwayFromZero);

		public static bool IsTopologyDuplicate
		{
			get
			{
				if (Win32.DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_CLONE == Win32.NativeMethods.GetScreenTopology())
				{
					//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.CHECK_IS_SCREEN_TOPOLOGY_DUPLICATE, 1);
					return true;
				}
				//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.CHECK_IS_SCREEN_TOPOLOGY_DUPLICATE);
				return false;
			}
		}

		public static bool IsTopologySecondaryOnly
		{
			get
			{
				if (Win32.DISPLAYCONFIG_TOPOLOGY_ID.DISPLAYCONFIG_TOPOLOGY_EXTERNAL == Win32.NativeMethods.GetScreenTopology())
				{
					//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.CHECK_IS_SCREEN_TOPOLOGY_SECONDARY, 1);
					return true;
				}
				//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.CHECK_IS_SCREEN_TOPOLOGY_SECONDARY);
				return false;
			}
		}

		public static bool IsPrimary
		{
			get
			{
				if (IsTopologySecondaryOnly)
				{
					//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.CHECK_IS_PRIMARY_SCREEN);
					return false;
				}
				return _isPrimary;
			}
		}

		public static int PhysicalWidth
		{
			get
			{
				//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.GET_DPI_USING_HDC);
				DEVMODE lpDevMode = CreateDevmode();
				if (EnumDisplaySettings(_deviceName, -1, ref lpDevMode) != 0)
				{
					int dmPelsWidth = lpDevMode.dmPelsWidth;
					//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.GET_EMBEDDE_DISPLAY_PWIDTH, dmPelsWidth);
					return dmPelsWidth;
				}
				try
				{
					IntPtr hdc = CreateDC(_deviceName, null, null, IntPtr.Zero);
					int deviceCaps = Win32.NativeMethods.GetDeviceCaps(hdc, 118);
					DeleteDC(hdc);
					//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.GET_EMBEDDE_DISPLAY_PWIDTH, deviceCaps);
					return deviceCaps;
				}
				catch
				{
					//LogCodess.LOG_WARNING(LogCodess.WARNING_CODE.DPI_COMPUTE_USING_HDC_FAILED);
					return 1536;
				}
			}
		}

		public static int PhysicalHeight
		{
			get
			{
				DEVMODE lpDevMode = CreateDevmode();
				if (EnumDisplaySettings(_deviceName, -1, ref lpDevMode) != 0)
				{
					int dmPelsHeight = lpDevMode.dmPelsHeight;
					//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.GET_EMBEDDE_DISPLAY_PWIDTH, dmPelsHeight);
					return dmPelsHeight;
				}
				try
				{
					IntPtr hdc = CreateDC(_deviceName, null, null, IntPtr.Zero);
					int deviceCaps = Win32.NativeMethods.GetDeviceCaps(hdc, 117);
					DeleteDC(hdc);
					//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.GET_EMBEDDE_DISPLAY_PHEIGHT, deviceCaps);
					return deviceCaps;
				}
				catch
				{
					//LogCodess.LOG_WARNING(LogCodess.WARNING_CODE.DPI_COMPUTE_USING_HDC_FAILED);
					return 2048;
				}
			}
		}

		public static bool IsDocked
		{
			get
			{
				DEVMODE lpDevMode = CreateDevmode();
				if (EnumDisplaySettings(_deviceName, -1, ref lpDevMode) != 0)
				{
					int dmPelsWidth = lpDevMode.dmPelsWidth;
					int dmPelsHeight = lpDevMode.dmPelsHeight;
					bool flag = (1006 == dmPelsHeight && 1536 == dmPelsWidth) || (1006 == dmPelsWidth && 1536 == dmPelsHeight);
					//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.CHECK_IS_DOCK_RESOLUTION, flag ? 1 : 0);
					return flag;
				}
				try
				{
					IntPtr hdc = CreateDC(_deviceName, null, null, IntPtr.Zero);
					int deviceCaps = Win32.NativeMethods.GetDeviceCaps(hdc, 117);
					int deviceCaps2 = Win32.NativeMethods.GetDeviceCaps(hdc, 118);
					DeleteDC(hdc);
					bool flag2 = (1006 == deviceCaps && 1536 == deviceCaps2) || (1006 == deviceCaps2 && 1536 == deviceCaps);
					//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.CHECK_IS_DOCK_RESOLUTION, flag2 ? 1 : 0);
					return flag2;
				}
				catch
				{
					//LogCodess.LOG_WARNING(LogCodess.WARNING_CODE.DPI_COMPUTE_USING_HDC_FAILED);
					return false;
				}
			}
		}

		public static bool IsLidOpened
		{
			get
			{
				try
				{
					return State != (DisplayDeviceStateFlags)0;
				}
				catch
				{
					return false;
				}
			}
		}

		[DllImport("user32.dll")]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		internal static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, DisplaySettingsFlags dwflags, IntPtr lParam);

		[DllImport("user32.dll")]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		internal static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

		[DllImport("user32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, SetLastError = true)]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		internal static extern int EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

		[DllImport("gdi32.dll")]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		internal static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

		[DllImport("gdi32.dll")]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		internal static extern bool DeleteDC([In] IntPtr hdc);

		public static DEVMODE CreateDevmode()
		{
			DEVMODE dEVMODE = default(DEVMODE);
			dEVMODE.dmDeviceName = new string(new char[32]);
			dEVMODE.dmFormName = new string(new char[32]);
			DEVMODE dEVMODE2 = dEVMODE;
			dEVMODE2.dmSize = (short)Marshal.SizeOf(dEVMODE2);
			return dEVMODE2;
		}

		public static bool Rotate(int Orientation)
		{
			//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.CHANGE_SCREEN_TO_ORIENTATION_TO_PORTRAIT);
			DEVMODE lpDevMode = CreateDevmode();
			if (EnumDisplaySettings(_deviceName, -1, ref lpDevMode) == 0)
			{
				return false;
			}
			//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.GET_EMBEDDED_MONITOR_ORIENTATION, lpDevMode.dmDisplayOrientation);
			if ((lpDevMode.dmDisplayOrientation + Orientation) % 2 == 1)
			{
				int dmPelsHeight = lpDevMode.dmPelsHeight;
				lpDevMode.dmPelsHeight = lpDevMode.dmPelsWidth;
				lpDevMode.dmPelsWidth = dmPelsHeight;
			}
			lpDevMode.dmDisplayOrientation = Orientation;
			return ChangeDisplaySettingsEx(_deviceName, ref lpDevMode, IntPtr.Zero, DisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero) == DISP_CHANGE.Successful;
		}

		public static void GetUpdates()
		{
			_deviceName = GetDeviceNameUpdate();
			_isPrimary = GetIsPrimarySettingUpdate();
			GetDPIScaleUsingHDCUpdate();
		}

		private static string GetDeviceNameUpdate()
		{
			//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.WITH_EXTERNAL_MONITOR_ATTACHED);
			StringBuilder stringBuilder = new StringBuilder();
			ulong apiresult = 0uL;
			if (Win32.NativeMethods.GetEmbeddedDeviceName(stringBuilder, ref apiresult))
			{
				return stringBuilder.ToString();
			}
			//LogCodess.LOG_WARNING(LogCodess.WARNING_CODE.GET_EMBEDDED_DEVICENAME_FAILED);
			return null;
		}

		public static Screen GetScreen()
		{
			//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.GET_SCREEN_INFO);
			//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.GET_MONITOR_COUNT, Screen.AllScreens.Count());
			if (Screen.AllScreens.Count() <= 1)
			{
				return Screen.AllScreens[0];
			}
			if (_deviceName == null)
			{
				return Screen.AllScreens[0];
			}
			try
			{
				return Screen.AllScreens.Where((Screen scr) => scr.DeviceName.ToString().ToLower().Contains(_deviceName.ToLower())).FirstOrDefault();
			}
			catch
			{
				try
				{
					//LogCodess.LOG_WARNING(LogCodess.WARNING_CODE.GET_SCREEN_INFO_LINQ_FAILED);
					Screen[] allScreens = Screen.AllScreens;
					foreach (Screen screen in allScreens)
					{
						if (screen.DeviceName.ToString().ToLower().Contains(_deviceName.ToLower()))
						{
							return screen;
						}
					}
				}
				catch
				{
					//LogCodess.LOG_ERROR(LogCodess.ERROR_CODE.GET_EMBEDDED_SCREEN_ERROR);
					return Screen.AllScreens[0];
				}
			}
			//LogCodess.LOG_WARNING(LogCodess.WARNING_CODE.GET_SCREEN_INFO_FAILED);
			return Screen.AllScreens[0];
		}

		public static void SetScreenToDockDisplay()
		{
			//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.CHANGE_SCREEN_TO_DOCK);
			Rotate(0);
			Win32.NativeMethods.ChangeResoToKeyboardDockedResolution();
		}

		private static void GetReccomendedResolution(ref int width, ref int height)
		{
			if (1006 == height || 1006 == width)
			{
				if (CurrentOrientation == DeviceSensorOrientations.Portrait || DeviceSensorOrientations.PortraitFlipped == CurrentOrientation)
				{
					height = 2048;
					width = 1536;
				}
				else
				{
					width = 2048;
					height = 1536;
				}
			}
		}

		public static void SetScreenToUndockDisplay()
		{
			//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.CHANGE_SCREEN_TO_UNDOCK);
			int width = 2048;
			int height = 1536;
			if (CurrentOrientation == DeviceSensorOrientations.Portrait || DeviceSensorOrientations.PortraitFlipped == CurrentOrientation)
			{
				height = 2048;
				width = 1536;
			}
			else
			{
				width = 2048;
				height = 1536;
			}
			uint num = Win32.NativeMethods.ChangeResolution(width, height);
			int num2 = 0;
			while (num != 0 && num2 < 2)
			{
				num = Win32.NativeMethods.ChangeResolution(width, height);
				Thread.Sleep(100);
				num2++;
			}
			if (num != 0)
			{
				//LogCodess.LOG_WARNING(LogCodess.WARNING_CODE.CHANGE_RESOLUTION_FAILED);
			}
		}

		public static void GetDPIScaleUsingHDCUpdate()
		{
			//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.GET_DPI_USING_HDC);
			if (!IsPrimary)
			{
				_dpix = (_dpiy = 1.0);
				return;
			}
			DEVMODE lpDevMode = CreateDevmode();
			if (EnumDisplaySettings(_deviceName, -1, ref lpDevMode) != 0)
			{
				int height = GetScreen().Bounds.Height;
				int width = GetScreen().Bounds.Width;
				int dmPelsHeight = lpDevMode.dmPelsHeight;
				int dmPelsWidth = lpDevMode.dmPelsWidth;
				_dpix = (double)dmPelsHeight / (double)height;
				_dpiy = (double)dmPelsWidth / (double)width;
				return;
			}
			try
			{
				IntPtr hdc = CreateDC(_deviceName, null, null, IntPtr.Zero);
				int deviceCaps = Win32.NativeMethods.GetDeviceCaps(hdc, 10);
				int deviceCaps2 = Win32.NativeMethods.GetDeviceCaps(hdc, 117);
				int deviceCaps3 = Win32.NativeMethods.GetDeviceCaps(hdc, 8);
				int deviceCaps4 = Win32.NativeMethods.GetDeviceCaps(hdc, 118);
				_dpix = (double)deviceCaps2 / (double)deviceCaps;
				_dpiy = (double)deviceCaps4 / (double)deviceCaps3;
				DeleteDC(hdc);
			}
			catch
			{
				//LogCodess.LOG_WARNING(LogCodess.WARNING_CODE.DPI_COMPUTE_USING_HDC_FAILED);
				_dpix = (_dpiy = 1.0);
			}
		}

		public static void GetDPIScalingUsingHDC(out double dpix, out double dpiy)
		{
			dpix = _dpix;
			dpiy = _dpiy;
		}

		private static bool GetIsPrimarySettingUpdate()
		{
			Screen screen = GetScreen();
			if (screen == null)
			{
				//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.CHECK_IS_PRIMARY_SCREEN);
				return false;
			}
			//LogCodess.LOG_INFORMATION(LogCodess.INFORMATION_CODE.CHECK_IS_PRIMARY_SCREEN, screen.Primary ? 1 : 0);
			return screen.Primary;
		}
	}
}
