using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ModeSwitcher.Utilities
{
	public enum DeviceModes
	{
		Laptop = 1,
		Tablet,
		Tent,
		Book,
		Undefined,
		PTablet,
		RTablet,
		XTablet,
		LidClosed
	}

	public static class Win32
	{
		public delegate bool EnumThreadWndProc(IntPtr hwnd, IntPtr lParam);

		public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lParam);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void ModeDelegate(DeviceModes mode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void DockDelegate(int state);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void DockStateDelegate(int status);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void OrientationDelegate();

		public delegate int HookCallbackDel(int code, IntPtr message, IntPtr state);

		public delegate int DeviceNotifyCallbackRoutine(IntPtr context, int type, IntPtr setting);

		public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

		internal class NativeMethods
		{
			[DllImport("ModeLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RegisterEventHandler", SetLastError = true)]
			public static extern void RegisterModeChangeEvent(ModeDelegate functionCallback);

			[DllImport("ModeLib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
			public static extern void RegisterOrientationChangeEvent(OrientationDelegate functionCallback);

			[DllImport("ModeLib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
			public static extern int GetHingleAngleOnDemand();

			[DllImport("ModeLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetEmbeddedMonitorDeviceName", SetLastError = true)]
			public static extern bool GetEmbeddedDeviceName(StringBuilder devicename, ref ulong apiresult);

			[DllImport("ModeLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "DockDeviceCallback", SetLastError = true)]
			public static extern void DeviceDock(DockDelegate functionCallback);

			[DllImport("ModeLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "DockStateCallback", SetLastError = true)]
			public static extern void DeviceDockState(DockStateDelegate functionCallback);

			[DllImport("ModeLib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
			public static extern void ChangeResoToKeyboardDockedResolution();

			[DllImport("ModeLib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
			public static extern int GetDeviceDockState(bool isFromModeSwitcher);

			[DllImport("ModeLib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
			public static extern uint ChangeResolution(double w, double h);

			[DllImport("ModeLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetEmbeddedMonitorTopology", SetLastError = true)]
			public static extern DISPLAYCONFIG_TOPOLOGY_ID GetScreenTopology();

			[DllImport("ModeLib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
			public static extern void UnregisterEventHandler();

			[DllImport("ModeLib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
			public static extern void StopDockDeviceCallback();

			[DllImport("user32.dll", CharSet = CharSet.Auto)]
			public static extern IntPtr GetDesktopWindow();

			[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
			public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

			[DllImport("user32.dll")]
			public static extern bool EnumWindows(EnumWindowsDelegate enumDel, IntPtr lParam);

			[DllImport("user32.dll")]
			public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

			[DllImport("user32.dll", CharSet = CharSet.Unicode)]
			public static extern int GetWindowTextLength(IntPtr hWnd);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

			[DllImport("user32.dll")]
			public static extern IntPtr GetForegroundWindow();

			[DllImport("user32.dll")]
			public static extern IntPtr GetWindow(IntPtr hWnd, int nCmd);

			[DllImport("user32.dll")]
			public static extern bool SetForegroundWindow(IntPtr hWnd);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern bool GetWindowRect(IntPtr hwnd, out RECT rc);

			[DllImport("user32.dll")]
			public static extern bool IsWindowVisible(IntPtr hwnd);

			[DllImport("user32.dll")]
			public static extern bool IsIconic(IntPtr hwnd);

			[DllImport("dwmapi.dll")]
			public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

			[DllImport("dwmapi.dll")]
			public static extern int DwmUnregisterThumbnail(IntPtr thumb);

			[DllImport("dwmapi.dll")]
			public static extern int DwmUpdateThumbnailProperties(IntPtr thumb, ref ThumbnailProperties props);

			[DllImport("dwmapi.dll")]
			public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

			[DllImport("user32.dll")]
			public static extern void SwitchToThisWindow(IntPtr hwnd, bool fAltTab);

			[DllImport("oleacc.dll", SetLastError = true)]
			internal static extern IntPtr GetProcessHandleFromHwnd(IntPtr hwnd);

			[DllImport("user32.dll")]
			internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

			[DllImport("user32.dll")]
			internal static extern bool UnhookWinEvent(IntPtr winEventHook);

			[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern bool SetWindowPlacement(IntPtr hwnd, ref WINDOWPLACEMENT wp);

			[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern bool GetWindowPlacement(IntPtr hwnd, ref WINDOWPLACEMENT wp);

			[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

			[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
			public static extern int GetMenuState(IntPtr hMenu, int uId, int uFlags);

			[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern uint RegisterWindowMessage(string lpString);

			[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern int DeregisterShellHookWindow(IntPtr hWnd);

			[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern int RegisterShellHookWindow(IntPtr hWnd);

			[DllImport("user32.dll", EntryPoint = "#2507")]
			public static extern bool SetAutoRotation(bool bEnable);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern IntPtr SetWindowsHookEx(int hookType, HookCallbackDel hookDelegate, IntPtr module, uint threadId);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern bool UnhookWindowsHookEx(IntPtr hook);

			[DllImport("user32.dll")]
			public static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr state);

			[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
			public static extern IntPtr GetModuleHandle(string lpModuleName);

			[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern IntPtr LoadLibrary(string lpFileName);

			[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

			[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern bool FreeLibrary(IntPtr hModule);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern bool RegisterTouchWindow(IntPtr hWnd, int flags);

			[DllImport("user32.dll")]
			public static extern IntPtr DefWindowProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);

			[DllImport("gdi32.dll")]
			public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

			[DllImport("user32.dll")]
			public static extern IntPtr GetDC(IntPtr hdc);

			[DllImport("dwmapi.dll")]
			public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

			[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "WindowFromPoint", ExactSpelling = true)]
			public static extern IntPtr IntWindowFromPoint(POINTSTRUCT pt);

			[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "WindowFromPhysicalPoint", ExactSpelling = true)]
			public static extern IntPtr IntWindowFromPhysicalPoint(POINTSTRUCT pt);

			[DllImport("Powrprof.dll", SetLastError = true)]
			public static extern int PowerSettingRegisterNotification(ref Guid settingGuid, uint flags, ref DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS recipient, ref IntPtr registrationHandle);

			[DllImport("Powrprof.dll", SetLastError = true)]
			public static extern int PowerSettingUnregisterNotification(IntPtr registrationHandle);

			[DllImport("shell32.dll", SetLastError = true)]
			public static extern IntPtr SHAppBarMessage(ABM dwMessage, [In] ref APPBARDATA pData);

			[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

			[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

			[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern uint SendInput(uint numberOfInputs, TypeINPUT[] inputs, int sizeOfInputStructure);

			[DllImport("user32.dll")]
			public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

			[DllImport("user32.dll")]
			public static extern int GetSystemMetrics(int smIndex);

			[DllImport("user32.dll")]
			public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

			[DllImport("user32.dll")]
			public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

			[DllImport("user32.dll")]
			public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

			[DllImport("user32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);
		}

		public struct WINDOWINFO
		{
			public uint cbSize;

			public RECT rcWindow;

			public RECT rcClient;

			public uint dwStyle;

			public uint dwExStyle;

			public uint dwWindowStatus;

			public uint cxWindowBorders;

			public uint cyWindowBorders;

			public ushort atomWindowType;

			public ushort wCreatorVersion;

			public WINDOWINFO(bool? filler)
			{
				this = default(WINDOWINFO);
				cbSize = (uint)Marshal.SizeOf(typeof(WINDOWINFO));
			}
		}



		public struct RECT
		{
			public int left;

			public int top;

			public int right;

			public int bottom;

			public bool IsEmpty
			{
				get
				{
					if (left == 0 && right == 0 && top == 0)
					{
						return bottom == 0;
					}
					return false;
				}
			}

			public RECT(int left, int top, int right, int bottom)
			{
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}
		}

		public struct POINT
		{
			public int x;

			public int y;

			public POINT(int x, int y)
			{
				this.x = x;
				this.y = y;
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct GUITHREADINFO
		{
			public int cbSize;

			public int dwFlags;

			public IntPtr hwndActive;

			public IntPtr hwndFocus;

			public IntPtr hwndCapture;

			public IntPtr hwndMenuOwner;

			public IntPtr hwndMoveSize;

			public IntPtr hwndCaret;

			public RECT rc;
		}

		public struct ThumbnailProperties
		{
			public ThumbnailFlags Flags;

			public RECT Destination;

			public RECT Source;

			public byte Opacity;

			public bool Visible;

			public bool SourceClientAreaOnly;
		}

		public struct INPUT
		{
			public int type;

			public INPUTUNION union;
		}

		internal struct TypeINPUT
		{
			public uint Type;

			public MOUSEKEYBDHARDWAREINPUT Data;
		}

		[StructLayout(LayoutKind.Explicit)]
		internal struct MOUSEKEYBDHARDWAREINPUT
		{
			[FieldOffset(0)]
			public HARDWAREINPUT Hardware;

			[FieldOffset(0)]
			public KEYBDINPUT Keyboard;

			[FieldOffset(0)]
			public MOUSEINPUT Mouse;
		}

		internal struct HARDWAREINPUT
		{
			public uint Msg;

			public ushort ParamL;

			public ushort ParamH;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct INPUTUNION
		{
			[FieldOffset(0)]
			public MOUSEINPUT mouseInput;

			[FieldOffset(0)]
			public KEYBDINPUT keyboardInput;
		}

		public struct MOUSEINPUT
		{
			public int dx;

			public int dy;

			public int mouseData;

			public int dwFlags;

			public int time;

			public IntPtr dwExtraInfo;
		}

		public struct KEYBDINPUT
		{
			public short wVk;

			public short wScan;

			public int dwFlags;

			public int time;

			public IntPtr dwExtraInfo;
		}

		public struct CWPSTRUCT
		{
			public IntPtr lparam;

			public IntPtr wparam;

			public int message;

			public IntPtr hwnd;
		}

		public struct WINDOWPLACEMENT
		{
			public int length;

			public int flags;

			public int showCmd;

			public POINT ptMinPosition;

			public POINT ptMaxPosition;

			public RECT rcNormalPosition;
		}

		public struct POINTSTRUCT
		{
			public int x;

			public int y;

			public POINTSTRUCT(int x, int y)
			{
				this.x = x;
				this.y = y;
			}
		}

		internal struct MENUBARINFO
		{
			internal int cbSize;

			internal RECT rcBar;

			internal IntPtr hMenu;

			internal IntPtr hwndMenu;

			internal int focusFlags;
		}

		[StructLayout(LayoutKind.Sequential)]
		public class MouseHookStruct
		{
			public POINT pt;

			public int hwnd;

			public int wHitTestCode;

			public int dwExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		public class MouseLLHookStruct
		{
			public POINT pt;

			public int mouseData;

			public int flags;

			public int time;

			public int dwExtraInfo;
		}

		public struct DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS
		{
			public DeviceNotifyCallbackRoutine Callback;

			public IntPtr Context;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct POWERBROADCAST_SETTING
		{
			public Guid PowerSetting;

			public uint DataLength;

			public byte Data;
		}

		public struct APPBARDATA
		{
			public uint cbSize;

			internal IntPtr hWnd;

			public uint uCallbackMessage;

			public ABE uEdge;

			public RECT rc;

			public int lParam;
		}

		public static class ABS
		{
			public const int Autohide = 1;

			public const int AlwaysOnTop = 2;
		}

		public enum SetWindowPosFlags : uint
		{
			SWP_NOSIZE = 1u,
			SWP_NOMOVE = 2u,
			SWP_NOZORDER = 4u,
			SWP_NOREDRAW = 8u,
			SWP_NOACTIVATE = 0x10u,
			SWP_FRAMECHANGED = 0x20u,
			SWP_SHOWWINDOW = 0x40u,
			SWP_HIDEWINDOW = 0x80u,
			SWP_NOCOPYBITS = 0x100u,
			SWP_NOREPOSITION = 0x200u,
			SWP_NOSENDCHANGING = 0x400u,
			SWP_DEFERERASE = 0x2000u,
			SWP_ASYNCWINDOWPOS = 0x4000u
		}

		public enum ShowWindowCommands
		{
			SW_SHOWNORMAL = 1,
			SW_SHOWMINIMIZED = 2,
			SW_MAXIMIZE = 3,
			SW_SHOWMAXIMIZED = 3,
			SW_SHOWNOACTIVATE = 4,
			SW_SHOWNA = 8,
			SW_RESTORE = 9,
			SW_FORCEMINIMIZE = 11
		}

		[Flags]
		public enum ThumbnailFlags
		{
			RectDestination = 0x1,
			RectSource = 0x2,
			Opacity = 0x4,
			Visible = 0x8
		}

		[Flags]
		public enum DwmWindowAttribute : uint
		{
			DWMWA_NCRENDERING_ENABLED = 0x1u,
			DWMWA_NCRENDERING_POLICY = 0x2u,
			DWMWA_TRANSITIONS_FORCEDISABLED = 0x3u,
			DWMWA_ALLOW_NCPAINT = 0x4u,
			DWMWA_CAPTION_BUTTON_BOUNDS = 0x5u,
			DWMWA_NONCLIENT_RTL_LAYOUT = 0x6u,
			DWMWA_FORCE_ICONIC_REPRESENTATION = 0x7u,
			DWMWA_FLIP3D_POLICY = 0x8u,
			DWMWA_EXTENDED_FRAME_BOUNDS = 0x9u,
			DWMWA_HAS_ICONIC_BITMAP = 0xAu,
			DWMWA_DISALLOW_PEEK = 0xBu,
			DWMWA_EXCLUDED_FROM_PEEK = 0xCu,
			DWMWA_CLOAK = 0xDu,
			DWMWA_CLOAKED = 0xEu,
			DWMWA_FREEZE_REPRESENTATION = 0xFu,
			DWMWA_LAST = 0x10u
		}

		public enum ShellEvents
		{
			HSHELL_WINDOWCREATED = 1,
			HSHELL_WINDOWDESTROYED = 2,
			HSHELL_ACTIVATESHELLWINDOW = 3,
			HSHELL_WINDOWACTIVATED = 4,
			HSHELL_GETMINRECT = 5,
			HSHELL_REDRAW = 6,
			HSHELL_TASKMAN = 7,
			HSHELL_LANGUAGE = 8,
			HSHELL_ACCESSIBILITYSTATE = 11,
			HSHELL_16 = 0x10,
			HSHELL_32772 = 32772
		}

		public enum DeviceCap
		{
			HORZSIZE = 4,
			VERTSIZE = 6,
			HORZRES = 8,
			VERTRES = 10,
			LOGPIXELSX = 88,
			DESKTOPVERTRES = 117,
			DESKTOPHORZRES = 118
		}

		public enum DockState
		{
			Undocked,
			Docked
		}

		public enum ABM : uint
		{
			New,
			Remove,
			QueryPos,
			SetPos,
			GetState,
			GetTaskbarPos,
			Activate,
			GetAutoHideBar,
			SetAutoHideBar,
			WindowPosChanged,
			SetState
		}

		public enum ABE : uint
		{
			Left,
			Top,
			Right,
			Bottom
		}

		public enum DISPLAYCONFIG_TOPOLOGY_ID : uint
		{
			DISPLAYCONFIG_TOPOLOGY_INTERNAL = 1u,
			DISPLAYCONFIG_TOPOLOGY_CLONE = 2u,
			DISPLAYCONFIG_TOPOLOGY_EXTEND = 4u,
			DISPLAYCONFIG_TOPOLOGY_EXTERNAL = 8u,
			DISPLAYCONFIG_TOPOLOGY_FORCE_UINT32 = uint.MaxValue
		}

		public enum ActionVal
		{
			Arrange,
			Enumerate
		}

		public static class NotifyIcon
		{
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			public struct NOTIFYICONDATA
			{
				public uint cbSize;

				public IntPtr hWnd;

				public uint uID;

				public NIF uFlags;

				public uint uCallbackMessage;

				public IntPtr hIcon;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
				public string szTip;

				public NIS dwState;

				public NIS dwStateMask;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
				public string szInfo;

				public uint uVersion;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
				public string szInfoTitle;

				public NIIF dwInfoFlags;

				public Guid guidItem;

				public IntPtr hBalloonIcon;
			}

			[Flags]
			public enum NIF : uint
			{
				NIF_MESSAGE = 0x1u,
				NIF_ICON = 0x2u,
				NIF_TIP = 0x4u,
				NIF_STATE = 0x8u,
				NIF_INFO = 0x10u,
				NIF_GUID = 0x20u,
				NIF_REALTIME = 0x40u,
				NIF_SHOWTIP = 0x80u
			}

			[Flags]
			public enum NIS : uint
			{
				NIS_HIDDEN = 0x1u,
				NIS_SHAREDICON = 0x2u
			}

			[Flags]
			public enum NIIF : uint
			{
				NIIF_NONE = 0x0u,
				NIIF_INFO = 0x1u,
				NIIF_WARNING = 0x2u,
				NIIF_ERROR = 0x3u,
				NIIF_USER = 0x4u,
				NIIF_NOSOUND = 0x10u,
				NIIF_LARGE_ICON = 0x20u,
				NIIF_RESPECT_QUIET_TIME = 0x80u,
				NIIF_ICON_MASK = 0xFu
			}

			public enum NIM : uint
			{
				NIM_ADD,
				NIM_MODIFY,
				NIM_DELETE,
				NIM_SETFOCUS,
				NIM_SETVERSION
			}



			public const uint NOTIFYICON_VERSION = 3u;

			public const uint NOTIFYICON_VERSION_4 = 4u;

			[DllImport("shell32.dll", CharSet = CharSet.Auto)]
			public static extern bool Shell_NotifyIcon(NIM dwMessage, [In] ref NOTIFYICONDATA lpdata);
		}

		public const int GWL_HINSTANCE = -6;

		public const int GWL_ID = -12;

		public const int GWL_STYLE = -16;

		public const int GWL_EXSTYLE = -20;

		public const int WS_MINIMIZE = 536870912;

		public const int WS_MAXIMIZE = 16777216;

		public const int WS_THICKFRAME = 262144;

		public const int WS_SYSMENU = 524288;

		public const int WS_BORDER = 8388608;

		public const int WS_DLGFRAME = 4194304;

		public const int WS_CAPTION = 12582912;

		public const int WS_MINIMIZEBOX = 131072;

		public const int WS_MAXIMIZEBOX = 65536;

		public const int WS_DISABLED = 134217728;

		public const int WS_CHILD = 1073741824;

		public const int WS_POPUP = int.MinValue;

		public const int WS_SIZEBOX = 262144;

		public const int WS_VISIBLE = 268435456;

		public const int WS_EX_DLGMODALFRAME = 1;

		public const int WS_EX_TOPMOST = 8;

		public const int WS_EX_TRANSPARENT = 32;

		public const int WS_EX_MDICHILD = 64;

		public const int WS_EX_TOOLWINDOW = 128;

		public const int WS_EX_APPWINDOW = 262144;

		public const int WS_EX_LAYERED = 524288;

		public const int WS_EX_NOACTIVATE = 134217728;

		public const int WS_EX_NOREDIRECTIONBITMAP = 2097152;

		public const int SM_CXPADDEDBORDER = 92;

		public const int TARGETWINDOW = 276824064;

		public const int NORESIZEWINDOWBIT = -17039361;

		public const int GA_PARENT = 1;

		public const int GA_ROOT = 2;

		public const int GW_HWNDFIRST = 0;

		public const int GW_HWNDLAST = 1;

		public const int GW_HWNDNEXT = 2;

		public const int GW_HWNDPREV = 3;

		public const int GW_OWNER = 4;

		public const int GW_CHILD = 5;

		public const int SWP_NOSIZE = 1;

		public const int SWP_NOMOVE = 2;

		public const int SWP_NOZORDER = 4;

		public const int SWP_NOREDRAW = 8;

		public const int SWP_NOACTIVATE = 16;

		public const int SWP_FRAMECHANGED = 32;

		public const int SWP_SHOWWINDOW = 64;

		public const int SWP_HIDEWINDOW = 128;

		public const int SWP_NOCOPYBITS = 256;

		public const int SWP_NOREPOSITION = 512;

		public const int SWP_NOSENDCHANGING = 1024;

		public const int SWP_DEFERERASE = 8192;

		public const int SWP_ASYNCWINDOWPOS = 16384;

		public const int HC_ACTION = 0;

		public const int WH_MOUSE = 7;

		public const int WH_MOUSE_LL = 14;

		public const uint WM_SETTINGCHANGE = 26u;

		public const uint WM_DESTROY = 2u;

		public const uint WM_DISPLAYCHANGE = 126u;

		public const int KEYEVENTF_EXTENDEDKEY = 1;

		public const int KEYEVENTF_KEYUP = 2;

		public const int VK_LWIN = 91;

		public const int EVENT_SYSTEM_MOVESIZEEND = 11;

		public const int EVENT_OBJECT_CREATE = 32768;

		public const int EVENT_OBJECT_DESTROY = 32769;

		public const int EVENT_SYSTEM_MINIMIZEEND = 23;

		public const int WINEVENT_OUTOFCONTEXT = 0;

		public const int OBJID_WINDOW = 0;

		public static ModeDelegate modeDelegate;

		public static DockDelegate dockDelegate;

		public static DockStateDelegate dockStateDelegate;

		public static OrientationDelegate orientationDelegate;

		public static IntPtr WindowFromPhysicalPoint(int x, int y)
		{
			POINTSTRUCT pt = new POINTSTRUCT(x, y);
			if (Environment.OSVersion.Version.Major >= 6)
			{
				return NativeMethods.IntWindowFromPhysicalPoint(pt);
			}
			return NativeMethods.IntWindowFromPoint(pt);
		}
	}
}