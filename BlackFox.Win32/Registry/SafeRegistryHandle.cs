using System;
using System.ComponentModel;
using System.Security.AccessControl;
using Microsoft.Win32.SafeHandles;

namespace BlackFox.Win32
{
    class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeRegistryHandle(RegistryHive hive, string key, RegistryRights samDesired)
            : base(true)
        {
            IntPtr handle;

            int result = UnsafeNativeMethods.RegOpenKeyEx(hive, key, 0, samDesired, out handle);
            if (result != 0) throw new Win32Exception(result);

            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return UnsafeNativeMethods.RegCloseKey(base.handle) == 0;
        }
    }
}
