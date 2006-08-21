﻿/*
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

        #region GetInformations

        public static List<Information> GetInformations(bool OnlyUninstallable)
        {
            List<Information> infos = new List<Information>();
            using (RegistryKey uninstallKey = Key)
            {
                foreach (string subKeyName in uninstallKey.GetSubKeyNames())
                {
                    using (RegistryKey subKey = uninstallKey.OpenSubKey(subKeyName))
                    {
                        Information info = new Information(subKey);

                        if (info.DisplayName != null)
                        {
                            if ( (!OnlyUninstallable) || (OnlyUninstallable && info.Uninstallable) )
                            {
                                infos.Add(info);
                            }
                        }
                    }
                }
            }
            infos.Sort();
            return infos;
        }

        public static List<Information> GetInformations()
        {
            return GetInformations(true);
        }

        public static List<Information> GetInformations(Regex regexp, bool OnlyUninstallable)
        {
            List<Information> infos = new List<Information>();
            List<Information> originalInfos = GetInformations(OnlyUninstallable);
            foreach (Information info in originalInfos)
            {
                bool match = true;
                match &= regexp.IsMatch(info.DisplayName);

                if (match)
                {
                    infos.Add(info);
                }
            }
            return infos;
        }

        public static List<Information> GetInformations(Regex regexp)
        {
            return GetInformations(regexp, true);
        }

        public static List<Information> GetInformations(string regexpString)
        {
            return GetInformations(regexpString, true);
        }

        public static List<Information> GetInformations(string regexpString, bool OnlyUninstallable)
        {
            return GetInformations(new Regex(regexpString, RegexOptions.IgnoreCase), OnlyUninstallable);
        }

        #endregion
    }
}
