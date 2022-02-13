using CommandLine;
using ModeSwitcher.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

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
            Options opt = null;

            Task task = null;

            switch (parseResult.Tag)
            {

                case ParserResultType.Parsed:

                    var parsed = parseResult as Parsed<Options>;


                    opt = parsed.Value;
                    if (!opt.Verbose)
                    {
                        FreeConsole();
                    }

                    if (opt.ChangeToLaptopView) { 
                        DeviceEmbeddedDisplay.SetScreenToDockDisplay();
                        OSRegistry.DisableAutoRotation();
                    }
                    else if (opt.ChangeToHorizontalView) { 
                        DeviceEmbeddedDisplay.SetScreenToUndockDisplay();
                        DeviceEmbeddedDisplay.Rotate(3);
                    }
                    else if (opt.ChangeToVerticalView) { 
                        DeviceEmbeddedDisplay.SetScreenToUndockDisplay();
                        DeviceEmbeddedDisplay.Rotate(0);
                    }
                    else if (opt.Auto) { task = StartAutoDisplayMode(); }
                    else { task = StartAutoDisplayMode(); }




                    break;
                case ParserResultType.NotParsed:

                    var notParsed = parseResult as NotParsed<Options>;

                    break;
            }


            task?.Wait();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            cancelationTokenSource?.Cancel();
        }

        static async Task StartAutoDisplayMode() {

            using(var dockWatcher = new DockWatcher())
            using(var osWatcher = new EventHandlerOS())
            {
                
                var osWakeUpEvent = osWatcher
                        .Where(x =>x == WindowOSEvents.OS_DisplayOn)
                        .Select(x => GetCurrentDockState());

                var osEvent = osWatcher
                    .Where(x =>
                        x == WindowOSEvents.OS_DisplaySettingsChanged
                        || x == WindowOSEvents.OS_UserPreferenceChanged
                        || x == WindowOSEvents.OS_DisplayOff
                        || x == WindowOSEvents.OS_SessionEnd
                        || x == WindowOSEvents.OS_SessionSwitch
                        || x == WindowOSEvents.OS_PowerModeChanged
                    )
                    .Sample(TimeSpan.FromMilliseconds(100))
                    .Select(x => GetCurrentDockState());

                var TimerEvent = Observable.Timer(TimeSpan.FromSeconds(0),TimeSpan.FromMilliseconds(200)).Take(10).Select(x => GetCurrentDockState());

                var stateChangeEvent = 
                    TimerEvent.Merge(dockWatcher).Merge(osEvent)
                    .Sample(TimeSpan.FromMilliseconds(300))
                    .DistinctUntilChanged()
                    .Merge(osWakeUpEvent)
                    .Sample(TimeSpan.FromMilliseconds(50));

                stateChangeEvent.Subscribe((dockState) => {
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

                while (!cancelationTokenSource.Token.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                    programEndWaitHandle.WaitOne();
                }
            }
        }


        static int AutoModeChange()
        {
            var currentDockState = Win32.NativeMethods.GetDeviceDockState(isFromModeSwitcher: true);

            if (currentDockState == 1)
            {
                Console.WriteLine("Set to laptop mode");
                DeviceEmbeddedDisplay.SetScreenToDockDisplay();
            }
            else
            {
                Console.WriteLine("Set to undock mode");
                DeviceEmbeddedDisplay.SetScreenToUndockDisplay();
            }

            return currentDockState;
        }

        static int GetCurrentDockState()
        {
            return Win32.NativeMethods.GetDeviceDockState(isFromModeSwitcher: true);
        }

    }

    enum DisplayMode
    {
        Laptop,Horizontal,Vertical
    }

    internal class Options
    {
        [Option('l', "laptop", Required = false, HelpText = "Change to Laptop mode")]
        public bool ChangeToLaptopView { get; set; }
        [Option('h',"horizontal", Required = false)]
        public bool ChangeToHorizontalView { get; set; }
        [Option('v', "vertical", Required = false)]
        public bool ChangeToVerticalView { get; set; }
        [Option('a', "auto", Required = false)]
        public bool Auto { get; set; }
        [Option("verbose", Required = false)]
        public bool Verbose { get; set; }
    }


}
