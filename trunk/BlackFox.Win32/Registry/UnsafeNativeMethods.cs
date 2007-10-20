using System;
using System.Security;
using System.Security.AccessControl;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace BlackFox.Win32
{
    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        const string DLL = "advapi32.dll";

        [DllImport(DLL, SetLastError = true)]
        public static extern int RegOpenKeyEx(RegistryHive key, string subKey, uint options,
            RegistryRights samDesired, out IntPtr result);

        [DllImport(DLL, SetLastError = true)]
        public static extern int RegNotifyChangeKeyValue(SafeRegistryHandle key, bool watchSubtree,
            RegistryNotifyFilter notifyFilter, SafeWaitHandle eventHandle, bool asynchronous);

        [DllImport(DLL, SetLastError = true)]
        public static extern int RegCloseKey(IntPtr key);
    }
}
