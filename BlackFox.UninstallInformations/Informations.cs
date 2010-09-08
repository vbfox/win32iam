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
using System.Linq;

namespace BlackFox.Win32.UninstallInformations
{
    static public class Informations
    {
        public static RegistryKey OpenUninstallKey(RegistryKey hive)
        {
            return hive.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
        }

        public static RegistryKey OpenUninstallKeyWow6432(RegistryKey hive)
        {
            return hive.OpenSubKey(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
        }

        #region Windows Installer Icons

        static Dictionary<string, string> GetWindowsInstallerIcons()
        {
            var result = new Dictionary<string, string>();
            using (var wiProducts = Registry.ClassesRoot.OpenSubKey(@"Installer\Products"))
            {
                foreach (var subKeyName in wiProducts.GetSubKeyNames())
                {
                    using (var subKey = wiProducts.OpenSubKey(subKeyName))
                    {
                        var productIcon = subKey.GetValue("ProductIcon") as string;
                        var productName = subKey.GetValue("ProductName") as string;

                        if ( (productIcon != null) && (productName != null) && !result.ContainsKey(productName))
                        {
                            result.Add(productName, productIcon);
                        }
                    }
                }
            }
            return result;
        }

        static void PathInformationsWithWindowsInstallerIcons(List<Information> infos)
        {
            var icons = GetWindowsInstallerIcons();
            foreach (var info in infos)
            {
                if (info.DisplayName == null) continue;
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

        static IEnumerable<Information> EnumerateInformationsFromKey(RegistryKey key, InformationFilterDelegate filter)
        {
            if (key == null) yield break;
            try
            {
                foreach (string subKeyName in key.GetSubKeyNames())
                {
                    using (var subKey = key.OpenSubKey(subKeyName, false))
                    {
                        if (subKey == null) continue;

                        var info = new Information(subKey);
                        if (filter(info))
                        {
                            yield return info;
                        }
                    }
                }
            }
            finally
            {
                key.Close();
            }
        }

        public static IEnumerable<Information> GetInformations(InformationFilterDelegate filter)
        {
            var infos = new List<Information>();

            infos.AddRange(EnumerateInformationsFromKey(OpenUninstallKey(Registry.LocalMachine), filter));
            infos.AddRange(EnumerateInformationsFromKey(OpenUninstallKeyWow6432(Registry.LocalMachine), filter));
            infos.AddRange(EnumerateInformationsFromKey(OpenUninstallKey(Registry.CurrentUser), filter));
            infos.AddRange(EnumerateInformationsFromKey(OpenUninstallKeyWow6432(Registry.CurrentUser), filter));

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

        public static IEnumerable<Information> GetInformations()
        {
            return GetInformations(BaseFilterDelegate);
        }

        public static IEnumerable<Information> GetInformations(Regex regexp, InformationFilterDelegate filter)
        {
            return GetInformations(info => filter(info) && regexp.IsMatch(info.DisplayName));
        }

        public static IEnumerable<Information> GetInformations(Regex regexp)
        {
            return GetInformations(regexp, BaseFilterDelegate);
        }

        public static IEnumerable<Information> GetInformations(string regexpString)
        {
            return GetInformations(regexpString, BaseFilterDelegate);
        }

        public static IEnumerable<Information> GetInformations(string regexpString, InformationFilterDelegate filter)
        {
            return GetInformations(new Regex(regexpString, RegexOptions.IgnoreCase), filter);
        }

        #endregion
    }
}
