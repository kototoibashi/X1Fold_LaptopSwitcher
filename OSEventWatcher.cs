using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Interop;
using Microsoft.Win32;
using ModeSwitcher.Utilities;

namespace X1Fold_LaptopSwitcher
{


	public enum WindowOSEvents
	{
		Window_MoveSizeEnd,
		Window_Restored,
		Window_FocusChanged,
		Window_Opened,
		Window_Closed,
		Application_MouseEvent,
		Application_Closing,
		OS_DisplaySettingsChanged,
		OS_DisplaySettingsChanging,
		OS_DisplayOff,
		OS_DisplayOn,
		OS_EventsThreadShutdown,
		OS_PowerModeChanged,
		OS_SessionSwitch,
		OS_SessionEnd,
		OS_UserPreferenceChanged,
		Registry_TabletModeChanged
	}

	public class EventHandlerOS : IObservable<WindowOSEvents>, IDisposable
	{
		private static List<IObserver<WindowOSEvents>> observers = new List<IObserver<WindowOSEvents>>();

		public class WindowOSEventArgs : EventArgs
		{


			public WindowOSEvents OSEvent { get; private set; }

			public IntPtr WindowHandle { get; private set; }

			public int OSSubEvent { get; private set; }

			public WindowOSEventArgs(WindowOSEvents hookEvent, IntPtr windowHandle)
			{
				OSEvent = hookEvent;
				WindowHandle = windowHandle;
			}

			public WindowOSEventArgs(WindowOSEvents hookEvent, IntPtr windowHandle, int hookSubEvent)
			{
				OSEvent = hookEvent;
				WindowHandle = windowHandle;
				OSSubEvent = hookSubEvent;
			}
		}

		private IntPtr _applicationHandle = IntPtr.Zero;

		private HwndSource _source;

		private IntPtr _windowClosedHook;

		private Win32.DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS _recipient;

		private IntPtr _handle;

		private const int DEVICE_NOTIFY_CALLBACK = 2;

		private const int PBT_POWERSETTINGCHANGE = 32787;

		private static Guid GUID_CONSOLE_DISPLAY_STATE = new Guid(1877382486, 28746, 18336, 143, 36, 194, 141, 147, 111, 218, 71);
		private bool disposedValue;

		public static event EventHandler<WindowOSEventArgs> EventRedirectToMainWindowModel;

		public EventHandlerOS()
		{
			EventRedirectToMainWindowModel += EventToRx;
			Automation.RemoveAllEventHandlers();
			RegisterToOSEvents();
		}

		static void EventToRx(object sender, WindowOSEventArgs e)
		{
			foreach (var observer in observers)
			{
				Task.Run(() => observer.OnNext(e.OSEvent));
			}
		}

		public void UnregisterEvents()
		{
			UnregisterFromOSEvents();
		}


		private void RegisterToOSEvents()
		{
			SystemEvents.DisplaySettingsChanged += CallbackDisplaySettingsChanged;
			SystemEvents.EventsThreadShutdown += CallbackEventsThreadShutdown;
			SystemEvents.PowerModeChanged += CallbackPowerModeChanged;
			SystemEvents.SessionSwitch += CallbackSessionSwitched;
			SystemEvents.UserPreferenceChanged += CallbackUserPreferenceChanged;
			SystemEvents.SessionEnding += CallbackSystemEnding;
			RegisterToMonitorStateEvents();
		}

		public void RegisterToMonitorStateEvents()
		{
			_recipient = new Win32.DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS
			{
				Callback = CallbackPowerStateChanged,
				Context = IntPtr.Zero
			};
			IntPtr registrationHandle = default(IntPtr);
			if (Win32.NativeMethods.PowerSettingRegisterNotification(ref GUID_CONSOLE_DISPLAY_STATE, 2u, ref _recipient, ref registrationHandle) == 0)
			{
				_handle = registrationHandle;
			}
		}

		private void UnregisterFromOSEvents()
		{
			SystemEvents.DisplaySettingsChanged -= CallbackDisplaySettingsChanged;
			SystemEvents.EventsThreadShutdown -= CallbackEventsThreadShutdown;
			SystemEvents.PowerModeChanged -= CallbackPowerModeChanged;
			SystemEvents.SessionSwitch -= CallbackSessionSwitched;
			SystemEvents.UserPreferenceChanged -= CallbackUserPreferenceChanged;
			SystemEvents.SessionEnding -= CallbackSystemEnding;
			UnregisterFromMonitorStateEvents();
		}

		private void CallbackDisplaySettingsChanged(object sender, EventArgs e)
		{
			if (EventRedirectToMainWindowModel != null)
			{
				EventRedirectToMainWindowModel(null, new WindowOSEventArgs(WindowOSEvents.OS_DisplaySettingsChanged, IntPtr.Zero));
			}
		}

		private void CallbackDisplaySettingsChanging(object sender, EventArgs e)
		{
			if (EventRedirectToMainWindowModel != null)
			{
				EventRedirectToMainWindowModel(null, new WindowOSEventArgs(WindowOSEvents.OS_DisplaySettingsChanging, IntPtr.Zero));
			}
		}

		private void CallbackEventsThreadShutdown(object sender, EventArgs e)
		{
			if (EventRedirectToMainWindowModel != null)
			{
				EventRedirectToMainWindowModel(null, new WindowOSEventArgs(WindowOSEvents.OS_EventsThreadShutdown, IntPtr.Zero));
			}
		}

		private void CallbackPowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			if (PowerModes.StatusChange == e.Mode && EventRedirectToMainWindowModel != null)
			{
				EventRedirectToMainWindowModel(null, new WindowOSEventArgs(WindowOSEvents.OS_PowerModeChanged, IntPtr.Zero, (int)e.Mode));
			}
		}

		private void CallbackSessionSwitched(object sender, SessionSwitchEventArgs e)
		{
			if (EventRedirectToMainWindowModel != null)
			{
				EventRedirectToMainWindowModel(null, new WindowOSEventArgs(WindowOSEvents.OS_SessionSwitch, IntPtr.Zero, (int)e.Reason));
			}
		}

		public void CallbackUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
		{
			if (e.Category.ToString() == "Desktop" && EventRedirectToMainWindowModel != null)
			{
				EventRedirectToMainWindowModel(null, new WindowOSEventArgs(WindowOSEvents.OS_UserPreferenceChanged, IntPtr.Zero));
			}
		}

		private void CallbackSystemEnding(object sender, SessionEndingEventArgs e)
		{
			if (EventRedirectToMainWindowModel != null)
			{
				EventRedirectToMainWindowModel(null, new WindowOSEventArgs(WindowOSEvents.OS_SessionEnd, IntPtr.Zero, (int)e.Reason));
			}
		}

		private void CallbackTabletModeChanged(object sender, EventArrivedEventArgs e)
		{
			if (EventRedirectToMainWindowModel != null)
			{
				EventRedirectToMainWindowModel(null, new WindowOSEventArgs(WindowOSEvents.Registry_TabletModeChanged, IntPtr.Zero));
			}
		}

		private void CallbackAutoRotattionModeChanged(object sender, EventArrivedEventArgs e)
		{
			if (EventRedirectToMainWindowModel != null)
			{
				EventRedirectToMainWindowModel(null, new WindowOSEventArgs(WindowOSEvents.Registry_TabletModeChanged, IntPtr.Zero));
			}
		}

		public void UnregisterFromMonitorStateEvents()
		{
			if (!(_handle == IntPtr.Zero))
			{
				Win32.NativeMethods.PowerSettingUnregisterNotification(_handle);
				_handle = IntPtr.Zero;
			}
		}

		private int CallbackPowerStateChanged(IntPtr context, int eventType, IntPtr setting)
		{
			if (EventRedirectToMainWindowModel == null)
			{
				return 0;
			}
			WindowOSEvents hookEvent = WindowOSEvents.OS_DisplayOn;
			bool flag = false;
			object obj;
			if (eventType == 32787 && (obj = Marshal.PtrToStructure(setting, typeof(Win32.POWERBROADCAST_SETTING))) is Win32.POWERBROADCAST_SETTING)
			{
				Win32.POWERBROADCAST_SETTING pOWERBROADCAST_SETTING = (Win32.POWERBROADCAST_SETTING)obj;
				if (pOWERBROADCAST_SETTING.PowerSetting == GUID_CONSOLE_DISPLAY_STATE)
				{
					switch (pOWERBROADCAST_SETTING.Data)
					{
						case 0:
							flag = true;
							hookEvent = WindowOSEvents.OS_DisplayOff;
							break;
						case 1:
							flag = true;
							hookEvent = WindowOSEvents.OS_DisplayOn;
							break;
					}
				}
			}
			if (!flag)
			{
				return 0;
			}
			EventRedirectToMainWindowModel(null, new WindowOSEventArgs(hookEvent, IntPtr.Zero));
			return 0;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: マネージド状態を破棄します (マネージド オブジェクト)
				}

				// TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
				UnregisterEvents();
				Automation.RemoveAllEventHandlers();
				// TODO: 大きなフィールドを null に設定します
				disposedValue = true;
			}
		}

		// // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
		~EventHandlerOS()
		{
			// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		IDisposable IObservable<WindowOSEvents>.Subscribe(IObserver<WindowOSEvents> observer)
		{
			observers.Add(observer);
			return new RxDisposer<WindowOSEvents>(observers, observer);
		}

		private class RxDisposer<T> : IDisposable
		{
			private readonly ICollection<IObserver<T>> _observers;
			private readonly IObserver<T> _observer;

			public RxDisposer(ICollection<IObserver<T>> observers, IObserver<T> observer)
			{
				_observers = observers;
				_observer = observer;
			}

			public void Dispose()
			{
				_observers.Remove(_observer);
			}
		}
	}
}

