using CommandLine;
using ModeSwitcher.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace X1Fold_LaptopSwitcher
{
    internal class Program
    {
        const int PollingInterval = 1000;
        static object _objSyncDock = new object();

        static void Main(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<Options>(args);
            Options opt = null;


            switch (parseResult.Tag)
            {

                case ParserResultType.Parsed:

                    var parsed = parseResult as Parsed<Options>;


                    opt = parsed.Value;

                    if (opt.ChangeToLaptopView) { SetScreenToDockDisplay(); }
                    else if (opt.ChangeToHorizontalView) { SetScreenToUndockDisplay(false); }
                    else if (opt.ChangeToVerticalView) { SetScreenToUndockDisplay(true); }
                    else if (opt.Auto) { StartAutoDisplayMode(); }


                    break;

                case ParserResultType.NotParsed:

                    var notParsed = parseResult as NotParsed<Options>;

                    break;
            }
        }

        static void SetScreenToDockDisplay()
        {
            OSRegistry.DisableAutoRotation();
            Win32.NativeMethods.ChangeResoToKeyboardDockedResolution();
        }

        static void SetScreenToUndockDisplay(bool vertical = false)
        {
            const int device_width = 2048;
            const int device_height = 1536;

            var width = vertical ? device_width : device_height;
            var height = vertical ? device_height : device_width;


            uint num = Win32.NativeMethods.ChangeResolution(width, height);
            int num2 = 0;
            while (num != 0 && num2 < 2)
            {
                num = Win32.NativeMethods.ChangeResolution(width, height);
                Thread.Sleep(100);
                num2++;
            }


            OSRegistry.EnableAutoRotation();
        }

        static void StartAutoDisplayMode() {

            // Dockイベントハンドラ初期化
            StartDockThread();

            var pastDockState = Win32.NativeMethods.GetDeviceDockState(isFromModeSwitcher: true);
            while (true) {
                Thread.Sleep(PollingInterval);

                // 定期的に確認する
                var currentDockState = Win32.NativeMethods.GetDeviceDockState(isFromModeSwitcher: true);
                if (pastDockState != currentDockState)
                {
                    Task.Run(() => AutoModeChangeWorker());
                }
                pastDockState = currentDockState;
            }
        }

        public static void StartDockThread()
        {
            Task task = null;
            lock (_objSyncDock)
            {
                try
                {
                    Win32.dockDelegate = CallbackDockChanged;
                    task = Task.Run(() =>{
                        Win32.NativeMethods.DeviceDock(Win32.dockDelegate);
                    });
                }
                catch
                {
                    Console.Error.WriteLine("DockChanged Thread FAILED!");
                    return;
                }
            }
        }

        private static void CallbackDockChanged(int dockInterruptState)
        {
            Console.WriteLine($"Dock Changed: {dockInterruptState}");
            Task.Run(() => StartDockThread());
            AutoModeChangeWorker();
        }

        static int AutoModeChangeWorker()
        {
            var currentDockState = Win32.NativeMethods.GetDeviceDockState(isFromModeSwitcher: true);

            if (currentDockState == 1)
            {
                Console.WriteLine("Set to laptop mode");
                SetScreenToDockDisplay();
            }
            else
            {
                Console.WriteLine("Set to undock mode");
                SetScreenToUndockDisplay();
            }

            return currentDockState;
        }
    }

    enum DisplayMode
    {
        Laptop,Horizontal,Vertical
    }

    internal class Options
    {
        // -a と -aaa の二つ指定可能
        [Option('l', "laptop", Required = false, HelpText = "Change to Laptop mode")]
        public bool ChangeToLaptopView { get; set; }
        [Option('h',"horizontal", Required = false)]
        public bool ChangeToHorizontalView { get; set; }
        [Option('v', "vertical", Required = false)]
        public bool ChangeToVerticalView { get; set; }
        [Option('a', "auto", Required = false)]
        public bool Auto { get; set; }
    }


}
