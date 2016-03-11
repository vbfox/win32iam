/*
 * UninstallInformations
 *
 * Copyright (C) 2006 Julien Roncaglia
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

namespace BlackFox.Win32.UninstallInformations
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using JetBrains.Annotations;
    using Microsoft.Win32;

    public class Information : IComparable<Information>
    {
        public Information(RegistryKey key)
        {
            KeyName = key.Name;
            DisplayName = (string)key.GetValue("DisplayName");
            UninstallString = (string)key.GetValue("UninstallString");
            UninstallDir = (string)key.GetValue("UninstallDir");
            DisplayIconPath = (string)key.GetValue("DisplayIcon");
            Comments = (string)key.GetValue("Comments");
            Publisher = (string)key.GetValue("Publisher");
            ParentKeyName = (string)key.GetValue("ParentKeyName");
        }

        public string Publisher { get; }

        public string Comments { get; }

        public string DisplayName { get; }

        public string KeyName { get; }

        public string UninstallString { get; }

        public string UninstallDir { get; }

        public string ParentKeyName { get; }

        public string DisplayIconPath { get; set; }

        public Icon Icon
        {
            get
            {
                if (!string.IsNullOrEmpty(DisplayIconPath))
                {
                    try
                    {
                        return Icons.ExtractFromRegistryString(
                            DisplayIconPath,
                            Icons.SystemIconSize.Large);
                    }
                    catch (Icons.IconNotFoundException)
                    {
                    }
                }

                return null;
            }
        }

        public bool Uninstallable => UninstallString != null;

        public int CompareTo(Information other)
        {
            return DisplayName?.CompareTo(other.DisplayName) ?? 0;
        }

        public bool Equals(Information other)
        {
            return other.KeyName == KeyName;
        }

        public void Uninstall()
        {
            if (UninstallString != null)
            {
                PROCESS_INFORMATION pi;
                STARTUPINFO si = default(STARTUPINFO);
                si.wShowWindow = 1;
                CreateProcess(
                    null,
                    UninstallString,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    0,
                    IntPtr.Zero,
                    UninstallDir,
                    ref si,
                    out pi);
                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);

                // FIXME: This line can throw. process already closed ?
                CloseHandle(pi.dwProcessId);
                CloseHandle(pi.dwThreadId);
            }
            else
            {
                throw new NotUninstallableProgramException(this);
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }

        /// <summary>
        ///     Remove the program from registry without running the uninstaller.
        /// </summary>
        public void RemoveFromRegistry()
        {
            string keyName = Regex.Replace(KeyName, @"HKEY_LOCAL_MACHINE\\", "");
            Registry.LocalMachine.DeleteSubKeyTree(keyName);
        }

#pragma warning disable SA1307 // Accessible fields must begin with upper-case letter
#pragma warning disable 649
#pragma warning disable 169
#pragma warning disable 414

        [DllImport("kernel32.dll")]
        private static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public IntPtr dwProcessId;

            public IntPtr dwThreadId;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [UsedImplicitly]
        private struct STARTUPINFO
        {
            public int cb;

            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;

            public short wShowWindow;
            public short cbReserved2;
            public byte lpReserved2;
            public int hStdInput;
            public int hStdOutput;
            public int hStdError;
        }

#pragma warning restore 414
#pragma warning restore 169
#pragma warning restore 649
#pragma warning restore SA1307 // Accessible fields must begin with upper-case letter
    }
}
