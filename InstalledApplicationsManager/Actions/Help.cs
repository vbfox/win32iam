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
    using System.Reflection;

    public class Help : IAction
    {
        public static string Name { get; } = "help";

        public int ParametersCount { get; } = 0;

        public void Execute(IList<string> parameters)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string versionString = $"{version.Major}.{version.Minor}";

            Console.WriteLine("Installed Applications Manager v{0}", versionString);
            Console.WriteLine();
            Console.WriteLine("Usage: iam Action [parameter]");
            Console.WriteLine();

            var verbs = "";
            foreach (string verb in ActionManager.ActionNamesList)
            {
                verbs += $", {verb}";
            }

            verbs = verbs.TrimStart(',');
            Console.WriteLine("Actions:{0}.", verbs);
        }
    }
}
