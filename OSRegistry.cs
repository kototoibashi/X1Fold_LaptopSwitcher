using System;
using System.Linq;
using System.Management;
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
}
