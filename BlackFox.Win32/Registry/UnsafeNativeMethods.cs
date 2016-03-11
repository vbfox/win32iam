namespace BlackFox.Win32.Registry
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using BlackFox.Win32.Registry.Enums;
    using Microsoft.Win32.SafeHandles;

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        private const string DLL = "advapi32.dll";

        [DllImport(DLL, SetLastError = true)]
        public static extern int RegOpenKeyEx(
            RegistryHive key,
            string subKey,
            uint options,
            RegistryRights samDesired,
            out IntPtr result);

        [DllImport(DLL, SetLastError = true)]
        public static extern int RegNotifyChangeKeyValue(
            SafeRegistryHandle key,
            bool watchSubtree,
            RegistryNotifyFilter notifyFilter,
            SafeWaitHandle eventHandle,
            bool asynchronous);

        [DllImport(DLL, SetLastError = true)]
        public static extern int RegCloseKey(IntPtr key);
    }
}
