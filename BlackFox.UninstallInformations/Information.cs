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

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text.RegularExpressions;

namespace BlackFox.Win32.UninstallInformations
{
    public class NotUninstallableProgramException : Exception {
        public NotUninstallableProgramException(Information info)
            : base(info.DisplayName + " : This program isn't uninstallable")
        {
        }
    };

    public class Information : IComparable<Information>
    {
        #region Interop (CreateProcess)

        struct PROCESS_INFORMATION{
            public IntPtr hProcess;
            public IntPtr hThread;
            public IntPtr dwProcessId;
            public IntPtr dwThreadId;
        }

	    struct STARTUPINFO{
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

        [DllImport("kernel32.dll")]
        static extern bool CreateProcess(string lpApplicationName,
          string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
          bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment,
          string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo,
          out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        #endregion

        #region Infos - Private Members

        string m_keyName;
        string m_displayName;
        string m_uninstallString;
        string m_uninstallDir;
        string m_installLocation;
        string m_publisher;
        string m_comments;
        string m_displayIconPath;
        int m_versionMajor;
        int m_versionMinor;

        #endregion

        #region Infos - Public Properties

        public string Comments
        {
            get
            {
                return m_comments;
            }
        }

        public string DisplayName
        {
            get
            {
                return m_displayName;
            }
        }

        public string KeyName
        {
            get
            {
                return m_keyName;
            }
        }

        public string UninstallString
        {
            get
            {
                return m_uninstallString;
            }
        }

        public string UninstallDir
        {
            get
            {
                return m_uninstallDir;
            }
        }

        #endregion

        public Information(RegistryKey key)
        {
            m_keyName = key.Name;
            m_displayName = (string)key.GetValue("DisplayName");
            m_uninstallString = (string)key.GetValue("UninstallString");
            m_uninstallDir = (string)key.GetValue("UninstallDir");
            m_displayIconPath = (string)key.GetValue("DisplayIcon");
            m_comments = (string)key.GetValue("Comments");
        }

        #region IComparable<Information> Members

        public int CompareTo(Information other)
        {
            return m_displayName.CompareTo(other.m_displayName);
        }

        public bool Equals(Information other)
        {
            return (other.m_keyName == m_keyName);
        }

        #endregion

        public void Uninstall()
        {
            if (UninstallString != null)
            {
                PROCESS_INFORMATION pi;
                STARTUPINFO si = new STARTUPINFO();
                si.wShowWindow = 1;
                CreateProcess(null, UninstallString, IntPtr.Zero, IntPtr.Zero,
                    false, 0, IntPtr.Zero, UninstallDir, ref si, out pi);
                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);
                CloseHandle(pi.dwProcessId);
                CloseHandle(pi.dwThreadId);
            }
            else
            {
                throw new NotUninstallableProgramException(this);
            }
        }

        public Icon Icon
        {
            get
            {
                if ( (m_displayIconPath != null) && (m_displayIconPath != "") )
                {
                    try
                    {
                        return Icons.ExtractFromRegistryString(m_displayIconPath,
                            Icons.SystemIconSize.Large);
                    }
                    catch (Icons.IconNotFoundException) { }
                }
                return null;
            }
        }

        public bool Uninstallable
        {
            get
            {
                return (UninstallString != null);
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }

        /// <summary>
        /// Remove the program from registry without running the uninstaller.
        /// </summary>
        public void RemoveFromRegistry()
        {
            string keyName = Regex.Replace(m_keyName, @"HKEY_LOCAL_MACHINE\\", "");
            Registry.LocalMachine.DeleteSubKeyTree(keyName);
        }

    }
}
