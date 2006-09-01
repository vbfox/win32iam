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
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace BlackFox.Win32.UninstallInformations
{
    static public class Informations
    {
        public static RegistryKey Key
        {
            get
            {
                return Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            }
        }

        #region Windows Installer Icons

        static Dictionary<string, string> GetWindowsInstallerIcons()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            using (RegistryKey wiProducts = Registry.ClassesRoot.OpenSubKey(@"Installer\Products"))
            {
                foreach (string subKeyName in wiProducts.GetSubKeyNames())
                    using (RegistryKey subKey = wiProducts.OpenSubKey(subKeyName))
                    {
                        string iconPath = (string)subKey.GetValue("ProductIcon");
                        if (iconPath != null)
                        {
                            result.Add((string)subKey.GetValue("ProductName"), iconPath);
                        }
                    }
            }
            return result;
        }

        static void PathInformationsWithWindowsInstallerIcons(List<Information> infos)
        {
            Dictionary<string, string> icons = GetWindowsInstallerIcons();
            foreach (Information info in infos)
            {
                string iconPath;
                if ( ((info.DisplayIconPath == null) || (info.DisplayIconPath == "")) 
                    && icons.TryGetValue(info.DisplayName, out iconPath) )
                {
                    info.DisplayIconPath = iconPath;
                }
            }
        }

        #endregion

        #region GetInformations

        public delegate bool InformationFilterDelegate(Information info);

        public static List<Information> GetInformations(InformationFilterDelegate filter)
        {
            
            List<Information> infos = new List<Information>();
            
            using (RegistryKey uninstallKey = Key)
            {
                foreach (string subKeyName in uninstallKey.GetSubKeyNames())
                {
                    using (RegistryKey subKey = uninstallKey.OpenSubKey(subKeyName))
                    {
                        Information info = new Information(subKey);
                        if (filter(info))
                        {
                            infos.Add(info);
                        }
                    }
                }
            }
            infos.Sort();
            PathInformationsWithWindowsInstallerIcons(infos);
            return infos;
        }

        static InformationFilterDelegate BaseFilterDelegate
        {
            get
            {
                return delegate(Information info)
                {
                    return (info.DisplayName != null)
                    && (info.ParentKeyName == null)
                    && (info.Uninstallable);
                };
            }
        }

        public static List<Information> GetInformations()
        {
            return GetInformations(BaseFilterDelegate);
        }

        public static List<Information> GetInformations(Regex regexp, InformationFilterDelegate filter)
        {
            List<Information> infos = new List<Information>();
            List<Information> originalInfos = GetInformations(delegate(Information info)
                {
                    return filter(info) && regexp.IsMatch(info.DisplayName);
                });
            PathInformationsWithWindowsInstallerIcons(infos);
            return infos;
        }

        public static List<Information> GetInformations(Regex regexp)
        {
            return GetInformations(regexp, BaseFilterDelegate);
        }

        public static List<Information> GetInformations(string regexpString)
        {
            return GetInformations(regexpString, BaseFilterDelegate);
        }

        public static List<Information> GetInformations(string regexpString, InformationFilterDelegate filter)
        {
            return GetInformations(new Regex(regexpString, RegexOptions.IgnoreCase), filter);
        }

        #endregion
    }
}
