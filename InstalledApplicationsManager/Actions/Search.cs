/*
 * InstalledApplicationManager
 *
 * Copyright (C) 2006-2010 Julien Roncaglia
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

namespace BlackFox.InstalledApplicationsManager.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BlackFox.Win32.UninstallInformations;

    public class Search : IAction
    {
        public static string Name => "search";

        public int ParametersCount => 1;

        public void Execute(IList<string> parameters)
        {
            if (parameters.Count > 0)
            {
                var infos = Informations.GetInformations(parameters[0]).ToList();
                if (infos.Count > 0)
                {
                    foreach (var info in infos)
                    {
                        Console.WriteLine(info.DisplayName);
                    }
                }
                else
                {
                    Console.WriteLine("No results");
                }
            }
            else
            {
                Console.WriteLine("Unable to search nothing, if you want to list all programs use the \"list\" action.");
            }
        }
    }
}
