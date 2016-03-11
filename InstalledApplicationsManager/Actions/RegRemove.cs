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

namespace BlackFox.InstalledApplicationsManager.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BlackFox.Win32.UninstallInformations;

    public class RegRemove : IAction
    {
        public int ParametersCount
        {
            get { return 1; }
        }

        public void Execute(IList<string> parameters)
        {
            if (parameters.Count > 0)
            {
                Information selectedInformation;
                IList<Information> infos = Informations.GetInformations(parameters[0]).ToList();
                if (infos.Count > 0)
                {
                    bool sureToRemove;
                    if (infos.Count == 1)
                    {
                        selectedInformation = infos[0];
                        sureToRemove = false;
                    }
                    else
                    {
                        selectedInformation = InformationsUtils.SelectInformation(infos);
                        sureToRemove = true;
                    }

                    if (selectedInformation != null)
                    {
                        if (!sureToRemove)
                        {
                            Console.WriteLine("Are you sure to remove the installer of \"{0}\"\r\nfrom the uninstall list, without removing \"{0}\" ? (y/n)", selectedInformation.DisplayName);
                            char answer = Console.ReadKey(true).KeyChar;
                            sureToRemove = (answer == 'y');
                        }
                        if (sureToRemove)
                        {

                            Console.WriteLine(string.Format("\"{0}\" removed from uninstall list.", selectedInformation.DisplayName));
                            selectedInformation.RemoveFromRegistry();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Not found");
                }
            }
        }


        public static string Name { get { return "regremove"; } }
}
}
