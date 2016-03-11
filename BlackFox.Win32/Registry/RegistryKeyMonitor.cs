using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using System.ComponentModel;

namespace BlackFox.Win32
{
    public enum RegistryKeyWatch { KeyOnly, KeyAndSubKeys };

    public class RegistryKeyMonitor : IDisposable
    {
        public delegate void KeyChangedDelegate(RegistryKeyMonitor monitor, bool keyDeleted);
        public delegate void ExceptionRaisedDelegate(RegistryKeyMonitor monitor, Exception e);

        KeyChangedDelegate m_keyChanged;
        ExceptionRaisedDelegate m_exceptionRaised;

        RegistryHive m_hive;
        public RegistryHive RegistryHive { get { return m_hive; } }

        string m_key;
        public string Key { get { return m_key; } }

        bool m_watchSubTree;
        bool WatchSubTree { get { return m_watchSubTree; } }

        RegistryNotifyFilter m_filter;
        RegistryNotifyFilter Filter { get { return m_filter; } }

        const RegistryNotifyFilter ALL_FILTERS = RegistryNotifyFilter.Attribute
            | RegistryNotifyFilter.Key
            | RegistryNotifyFilter.Security
            | RegistryNotifyFilter.Value;

        const int ERROR_FILE_NOT_FOUND = 2;
        bool KeyStillExists
        {
            get
            {
                try
                {
                    (new SafeRegistryHandle(m_hive, m_key, RegistryRights.Notify)).Close();
                    return true;
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode == ERROR_FILE_NOT_FOUND) return false;
                    throw;
                }
            }
        }

        public RegistryKeyMonitor(RegistryHive hive, string key, KeyChangedDelegate keyChanged,
            ExceptionRaisedDelegate exceptionRaised)
            : this(hive, key, ALL_FILTERS, keyChanged, exceptionRaised) { }

        public RegistryKeyMonitor(RegistryHive hive, string key, RegistryNotifyFilter filter,
            KeyChangedDelegate keyChanged, ExceptionRaisedDelegate exceptionRaised)
            : this(hive, key, filter, RegistryKeyWatch.KeyAndSubKeys, keyChanged, exceptionRaised) { }

        public RegistryKeyMonitor(RegistryHive hive, string key, RegistryNotifyFilter filter, RegistryKeyWatch watch,
            KeyChangedDelegate keyChanged, ExceptionRaisedDelegate exceptionRaised)
        {
            if (keyChanged == null) throw new ArgumentNullException("keyChanged");
            if (exceptionRaised == null) throw new ArgumentNullException("exceptionRaised");

            m_hive = hive;
            m_key = key;
            m_filter = filter;
            m_watchSubTree = (watch == RegistryKeyWatch.KeyAndSubKeys);
            m_keyChanged = keyChanged;
            m_exceptionRaised = exceptionRaised;

            StartMonitor();
        }

        Thread m_thread;
        SafeRegistryHandle m_registryHandle;
        ManualResetEvent m_stopEvent = new ManualResetEvent(false);

        void StartMonitor()
        {
            m_registryHandle = new SafeRegistryHandle(m_hive, m_key, RegistryRights.Notify);
            m_thread = new Thread(ThreadStart);
            m_thread.Start();
        }

        void ThreadStart()
        {
            try
            {
                AutoResetEvent notifyEvent = new AutoResetEvent(false);
                WaitHandle[] waitHandles = new WaitHandle[] { m_stopEvent, notifyEvent };
                int waitResult = 0;
                do
                {
                    int notifyResult = UnsafeNativeMethods.RegNotifyChangeKeyValue(m_registryHandle,
                        m_watchSubTree, m_filter, notifyEvent.SafeWaitHandle, true);
                    if (notifyResult != 0) throw new Win32Exception(notifyResult);

                    waitResult = WaitHandle.WaitAny(waitHandles);
                    if (waitResult == 1)
                    {
                        bool deleted = !KeyStillExists;
                        m_keyChanged(this, deleted);
                        if (deleted) return;
                    }
                }
                while (waitResult != 0);
            }
            catch (Exception e)
            {
                m_exceptionRaised(this, e);
            }
        }

        bool m_disposed = false;

        public void Dispose()
        {
            if (m_disposed) throw new ObjectDisposedException("RegistryKeyMonitor");

            m_stopEvent.Set();
            m_thread.Join();
            m_registryHandle.Dispose();
        }
    }
}
