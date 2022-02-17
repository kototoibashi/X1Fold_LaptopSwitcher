using CommandLine;
using ModeSwitcher.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Management;
using System.IO;
using System.Net;

namespace X1Fold_LaptopSwitcher
{
    internal class Program
    {
        const int PollingInterval = 5000;
        static readonly object lockObj = new object();
        private static readonly CancellationTokenSource cancelationTokenSource = new CancellationTokenSource();
        private static readonly EventWaitHandle programEndWaitHandle = new EventWaitHandle(false,EventResetMode.ManualReset);

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public static void Main(string[] args)
        {
            cancelationTokenSource.Token.Register(() => programEndWaitHandle.Set());
            Console.CancelKeyPress += Console_CancelKeyPress;

            var parseResult = Parser.Default.ParseArguments<Options>(args);

            if (parseResult.Tag != ParserResultType.Parsed) {
                Console.Error.WriteLine("Commandline option error");
                return;
            }

            var parsed = parseResult as Parsed<Options>;
            var opt = parsed.Value;
            Task task = null;

            CheckModeLibDllExists();

            if (!opt.Verbose)
            {
                FreeConsole();
            }

            if (opt.ChangeToLaptopView)
            {
                DockModeChange(1);
            }
            else if (opt.ChangeToHorizontalView)
            {
                DockModeChange(0);
                DeviceEmbeddedDisplay.Rotate(3);
            }
            else if (opt.ChangeToVerticalView)
            {
                DockModeChange(0);
                DeviceEmbeddedDisplay.Rotate(0);
            }
            else if (opt.Auto) { task = StartAutoDisplayMode(); }
            else { task = StartAutoDisplayMode(); }

            task?.Wait();
        }

        static async Task StartAutoDisplayMode() {

            using(var dockWatcher = new DockWatcher())
            using(var osWatcher = new EventHandlerOS())
            {
                var semaphore = new SemaphoreSlim(0, 1);

                // キーボード着脱イベント
                var dockEvent = (IObservable<int>)dockWatcher;

                // OSのさまざまなイベント
                var osEvent = osWatcher
                    .Where(x =>
                        x == WindowOSEvents.OS_DisplaySettingsChanged
                        || x == WindowOSEvents.OS_UserPreferenceChanged
                        || x == WindowOSEvents.OS_DisplayOff
                        || x == WindowOSEvents.OS_SessionEnd
                        || x == WindowOSEvents.OS_SessionSwitch
                        || x == WindowOSEvents.OS_PowerModeChanged
                    )
                    .Sample(TimeSpan.FromMilliseconds(500))
                    .Select(x => GetCurrentDockState());

                // 切り替えが頻繁に発生しないよう500ms間隔で間引く
                var stateChangeEvent = 
                    dockEvent.Merge(osEvent)
                    .Sample(TimeSpan.FromMilliseconds(500))
                    .DistinctUntilChanged();

                // OSイベントとキーボード着脱イベントで実行する
                stateChangeEvent.Subscribe(async (dockState) =>
                {
                    try
                    {
                        await semaphore.WaitAsync();

                        // 画面輝度を取得
                        var brightness = GetBrightness();

                        // モードチェンジ中は暗転させる
                        SetBrightness(brightness == 0 ? 1 : 0);

                        // モードチェンジ
                        await DockModeChange(dockState);

                        // モードチェンジすると輝度が勝手に変わるので、戻す
                        SetBrightness(brightness);
                    }
                    finally { semaphore.Release(); }
                }
                );
                
                // スリープ復帰イベント
                var osWakeUpEvent = osWatcher
                        .Where(x => x == WindowOSEvents.OS_DisplayOn)
                        .Select(x => GetCurrentDockState());

                // 初回起動イベント
                var initializeEvent = Observable.Timer(TimeSpan.FromMilliseconds(200))
                    .Select(x => GetCurrentDockState());

                // スリープ復帰時に実行する
                osWakeUpEvent.Merge(initializeEvent).Subscribe(async (dockState) =>
                {
                    try
                    {
                        await semaphore.WaitAsync();

                        // 画面輝度を取得
                        var brightness = GetBrightness();

                        // モードチェンジ中は暗転させる
                        SetBrightness(brightness == 0 ? 1 : 0);

                        // なぜか1回ではうまくいかないので、何度か繰り返す
                        for (var i = 0; i < 6; i++)
                        {
                            var invertedDockState = dockState == 1 ? 0 : 1;
                            await DockModeChange(invertedDockState);
                            await Task.Delay(1);
                            await DockModeChange(dockState);
                            await Task.Delay(1);
                        }

                        // モードチェンジすると輝度が勝手に変わるので、戻す
                        SetBrightness(brightness);
                    }
                    finally { semaphore.Release(); }
                }
                );

                // キャンセルされるまで待つ
                while (!cancelationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(10000,cancelationTokenSource.Token);
                    programEndWaitHandle.WaitOne();
                }

                // タブレットモードにしてから終了する
                await DockModeChange(0);
            }
        }

        static Task DockModeChange(int dockState)
        {
            return Task.Run(() =>
            {

                if (dockState == 1)
                {
                    Console.WriteLine("Set to laptop mode");
                    OSRegistry.DisableAutoRotation();
                    DeviceEmbeddedDisplay.Rotate(0);
                    DeviceEmbeddedDisplay.SetScreenToDockDisplay();
                }
                else
                {
                    Console.WriteLine("Set to tablet mode");
                    DeviceEmbeddedDisplay.SetScreenToUndockDisplay();
                    OSRegistry.EnableAutoRotation();
                }
            });
        }

        static int GetCurrentDockState()
        {
            return Win32.NativeMethods.GetDeviceDockState(isFromModeSwitcher: true);
        }

        private static bool CheckModeLibDllExists()
        {
            const string dll_url = @"https://github.com/kototoibashi/X1Fold_LaptopSwitcher/releases/download/v0.0.2/ModeLib.dll";
            const string lenovo_dll_path = @"C:\Program Files\Lenovo\Mode Switcher\ModeLib.dll";
            var dll_path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "ModeLib.dll");

            if (File.Exists(dll_path)) return true;

            try {
                Win32.NativeMethods.GetDeviceDockState(isFromModeSwitcher: true);

                // DLL OK
                return true;
            } 
            catch (DllNotFoundException ex)
            {
                Console.Error.WriteLine("ModeLib.DLL NOT FOUND!");
                // DLL not found
                if (File.Exists(lenovo_dll_path))
                {
                    // Copy from Lenovo Mode Switcher
                    Console.Error.WriteLine("Copy ModeLib.dll");
                    File.Copy(lenovo_dll_path, Path.Combine(lenovo_dll_path, dll_path));
                    return true;
                }
                else {
                    // Download dll
                    Console.Error.WriteLine("Download ModeLib.dll");
                    var mywebClient = new WebClient();
                    mywebClient.DownloadFile(dll_url, dll_path);
                    return true;
                }
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            cancelationTokenSource?.Cancel();
        }

        static int GetBrightness()
        {
            var brightness = 100;
            var WmiMonitorBrightness = new ManagementClass("root/wmi", "WmiMonitorBrightness", null);
            foreach (ManagementObject mo in WmiMonitorBrightness.GetInstances())
            {
                return int.Parse(mo["CurrentBrightness"].ToString());
            }
            return brightness;
        }

        static void SetBrightness(int brightness)
        {
            var WmiMonitorBrightnessMethod = new ManagementClass("root/wmi", "WmiMonitorBrightnessMethods", null);
            foreach (ManagementObject mo in WmiMonitorBrightnessMethod.GetInstances())
            {
                var x = mo.GetMethodParameters("WmiSetBrightness");
                x["Brightness"] = brightness;
                x["Timeout"] = 5;
                mo.InvokeMethod("WmiSetBrightness", x, null);
            }
        }

    }

    enum DisplayMode
    {
        Laptop,Horizontal,Vertical
    }

    internal class Options
    {
        [Option('a', "auto", Required = false, HelpText = "Auto mode")]
        public bool Auto { get; set; } = true;

        [Option('l', "laptop", Required = false, HelpText = "Change to Laptop mode")]
        public bool ChangeToLaptopView { get; set; }

        [Option('h',"horizontal", Required = false, HelpText = "Change to Horizontal view")]
        public bool ChangeToHorizontalView { get; set; }

        [Option('v', "vertical", Required = false, HelpText = "Change to Vertical view")]
        public bool ChangeToVerticalView { get; set; }

        [Option("verbose", Required = false)]
        public bool Verbose { get; set; }
    }


}
