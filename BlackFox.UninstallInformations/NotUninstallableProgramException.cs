namespace BlackFox.Win32.UninstallInformations
{
    using System;

    public class NotUninstallableProgramException : Exception
    {
        public NotUninstallableProgramException(Information info)
            : base(info.DisplayName + " : This program isn't uninstallable")
        {
        }
    }
}
