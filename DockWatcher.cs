using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace X1Fold_LaptopSwitcher
{
    internal class DockWatcher : IObservable<int>, IDisposable
    {
        private static List<IObserver<int>> observers = new List<IObserver<int>>();
        private bool disposedValue;
        private static CancellationTokenSource cancelationTokenSource = new CancellationTokenSource();

        public DockWatcher()
        {
            StartDockWatchThread();
        }

        public DockWatcher(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => cancelationTokenSource.Cancel());
            StartDockWatchThread();
        }

        private static void StartDockWatchThread()
        {
            if(cancelationTokenSource.Token.IsCancellationRequested == true) return;

            try
            {
                var task = Task.Factory.StartNew(() => {
                    ModeSwitcher.Utilities.Win32.dockDelegate = CallbackDockChanged;
                    ModeSwitcher.Utilities.Win32.NativeMethods.DeviceDock(ModeSwitcher.Utilities.Win32.dockDelegate);
                }, 
                creationOptions : TaskCreationOptions.LongRunning, 
                cancellationToken: cancelationTokenSource.Token,
                scheduler: TaskScheduler.Current);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine("DockChanged Thread FAILED!");
                throw ex;
            }
        }

        private static void CallbackDockChanged(int dockInterruptState)
        {
            StartDockWatchThread();
            NotifyDockChanged(dockInterruptState);
            Debug.WriteLine($"Dock Changed: {dockInterruptState}");
        }

        private static void NotifyDockChanged(int dockInterruptState)
        {
            foreach (var observer in observers)
            {
                observer.OnNext(dockInterruptState);
            }
        }

        IDisposable IObservable<int>.Subscribe(IObserver<int> observer)
        {
            observers.Add(observer);
            return new RxDisposer<int>(observers, observer);
        }

        private class RxDisposer<T> : IDisposable
        {
            private readonly ICollection<IObserver<T>> _observers;
            private readonly IObserver<T> _observer;

            public RxDisposer(ICollection<IObserver<T>> observers,IObserver<T> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                _observers.Remove(_observer);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    cancelationTokenSource.Cancel();
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                observers.Clear();
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~DockWatcher()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
