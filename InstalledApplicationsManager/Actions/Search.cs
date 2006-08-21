/*
 * InstalledApplicationManager
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

#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using BlackFox.Win32.UninstallInformations;

#endregion

namespace BlackFox.InstalledApplicationsManager.Actions
{
    public class Search : IAction
    {

        #region IAction Members

        public void Execute(IList<string> parameters)
        {
            if (parameters.Count > 0)
            {
                List<Information> infos = Informations.GetInformations(parameters[0]);
                if (infos.Count > 0)
                {
                    foreach (Information info in infos)
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

        public int ParametersCount
        {
            get { return 1; }
        }

        #endregion

        public static string Name { get { return "search"; } }
}
}
