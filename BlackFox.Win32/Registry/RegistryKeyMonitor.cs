namespace BlackFox.Win32.Registry
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.AccessControl;
    using System.Threading;
    using BlackFox.Win32.Registry.Enums;

    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder")]
    public class RegistryKeyMonitor : IDisposable
    {
        public delegate void ExceptionRaisedDelegate(RegistryKeyMonitor monitor, Exception e);

        public delegate void KeyChangedDelegate(RegistryKeyMonitor monitor, bool keyDeleted);

        #pragma warning disable SA1310 // Field names must not contain underscore
        private const RegistryNotifyFilter ALL_FILTERS = RegistryNotifyFilter.Attribute
                                                         | RegistryNotifyFilter.Key
                                                         | RegistryNotifyFilter.Security
                                                         | RegistryNotifyFilter.Value;

        private const int ERROR_FILE_NOT_FOUND = 2;
#pragma warning restore SA1310 // Field names must not contain underscore

        private readonly bool disposed = false;
        private readonly ExceptionRaisedDelegate exceptionRaised;

        private readonly KeyChangedDelegate keyChanged;
        private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);
        private SafeRegistryHandle registryHandle;

        private Thread thread;

        public RegistryKeyMonitor(
            RegistryHive hive,
            string key,
            KeyChangedDelegate keyChanged,
            ExceptionRaisedDelegate exceptionRaised)
            : this(hive, key, ALL_FILTERS, keyChanged, exceptionRaised)
        {
        }

        public RegistryKeyMonitor(
            RegistryHive hive,
            string key,
            RegistryNotifyFilter filter,
            KeyChangedDelegate keyChanged,
            ExceptionRaisedDelegate exceptionRaised)
            : this(hive, key, filter, RegistryKeyWatch.KeyAndSubKeys, keyChanged, exceptionRaised)
        {
        }

        public RegistryKeyMonitor(
            RegistryHive hive,
            string key,
            RegistryNotifyFilter filter,
            RegistryKeyWatch watch,
            KeyChangedDelegate keyChanged,
            ExceptionRaisedDelegate exceptionRaised)
        {
            if (keyChanged == null)
            {
                throw new ArgumentNullException(nameof(keyChanged));
            }

            if (exceptionRaised == null)
            {
                throw new ArgumentNullException(nameof(exceptionRaised));
            }

            RegistryHive = hive;
            Key = key;
            Filter = filter;
            WatchSubTree = watch == RegistryKeyWatch.KeyAndSubKeys;
            this.keyChanged = keyChanged;
            this.exceptionRaised = exceptionRaised;

            StartMonitor();
        }

        public RegistryHive RegistryHive { get; }

        public string Key { get; }

        public bool WatchSubTree { get; }

        private RegistryNotifyFilter Filter { get; }

        private bool KeyStillExists
        {
            get
            {
                try
                {
                    new SafeRegistryHandle(RegistryHive, Key, RegistryRights.Notify).Close();
                    return true;
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode == ERROR_FILE_NOT_FOUND)
                    {
                        return false;
                    }

                    throw;
                }
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("RegistryKeyMonitor");
            }

            stopEvent.Set();
            thread.Join();
            registryHandle.Dispose();
        }

        private void StartMonitor()
        {
            registryHandle = new SafeRegistryHandle(RegistryHive, Key, RegistryRights.Notify);
            thread = new Thread(ThreadStart);
            thread.Start();
        }

        private void ThreadStart()
        {
            try
            {
                var notifyEvent = new AutoResetEvent(false);
                var waitHandles = new WaitHandle[] { stopEvent, notifyEvent };
                int waitResult;
                do
                {
                    int notifyResult = UnsafeNativeMethods.RegNotifyChangeKeyValue(
                        registryHandle,
                        WatchSubTree,
                        Filter,
                        notifyEvent.SafeWaitHandle,
                        true);

                    if (notifyResult != 0)
                    {
                        throw new Win32Exception(notifyResult);
                    }

                    waitResult = WaitHandle.WaitAny(waitHandles);
                    if (waitResult == 1)
                    {
                        bool deleted = !KeyStillExists;
                        keyChanged(this, deleted);
                        if (deleted)
                        {
                            return;
                        }
                    }
                }
                while (waitResult != 0);
            }
            catch (Exception e)
            {
                exceptionRaised(this, e);
            }
        }
    }
}
