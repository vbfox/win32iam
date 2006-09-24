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

using System;
using System.Collections.Generic;
using System.Text;
using BlackFox.Win32.UninstallInformations;

namespace BlackFox.InstalledApplicationsManager
{
    static class InformationsUtils
    {
        public static Information SelectInformation(IList<Information> infos)
        {
            Information selectedInformation = null;

            for (int i = 0; i < infos.Count; i++)
            {
                Console.WriteLine("[{0}] {1}", i, infos[i].DisplayName);
            }
            Console.Write("Choice (Return to cancel): ");
            string inputString = Console.ReadLine();

            int inputInteger;
            if (int.TryParse(inputString, out inputInteger))
            {
                if ((inputInteger >= 0) && (inputInteger < infos.Count))
                {
                    selectedInformation = infos[inputInteger];
                }
            }

            return selectedInformation;
        }
    }
}
