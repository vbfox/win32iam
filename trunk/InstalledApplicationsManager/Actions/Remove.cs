﻿/*
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
    public class Remove : IAction
    {

        #region IAction Members

        public int ParametersCount
        {
            get { return 1; }
        }

        public void Execute(IList<string> parameters)
        {
            if (parameters.Count > 0)
            {
                Information selectedInformation;
                IList<Information> infos = Informations.GetInformations(parameters[0]);
                if (infos.Count > 0)
                {
                    if (infos.Count == 1)
                    {
                        selectedInformation = infos[0];
                    }
                    else
                    {
                        selectedInformation = InformationsUtils.SelectInformation(infos);
                    }

                    if (selectedInformation != null)
                    {
                        Console.WriteLine(string.Format("Running \"{0}\" uninstaller...", selectedInformation.DisplayName));
                        selectedInformation.Uninstall();
                    }
                }
                else
                {
                    Console.WriteLine("Not found");
                }
            }
        }

        #endregion

        public static string Name { get { return "remove"; } }
}
}
