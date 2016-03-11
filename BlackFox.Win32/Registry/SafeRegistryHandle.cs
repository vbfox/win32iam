namespace BlackFox.Win32.Registry
{
    using System;
    using System.ComponentModel;
    using System.Security.AccessControl;
    using BlackFox.Win32.Registry.Enums;
    using Microsoft.Win32.SafeHandles;

    internal class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeRegistryHandle(RegistryHive hive, string key, RegistryRights samDesired)
            : base(true)
        {
            IntPtr handle;

            int result = UnsafeNativeMethods.RegOpenKeyEx(hive, key, 0, samDesired, out handle);
            if (result != 0)
            {
                throw new Win32Exception(result);
            }

            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return UnsafeNativeMethods.RegCloseKey(handle) == 0;
        }
    }
}
