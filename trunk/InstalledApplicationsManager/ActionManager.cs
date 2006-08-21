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
using System.Collections.ObjectModel;
using System.Reflection;

#endregion

namespace BlackFox.InstalledApplicationsManager
{
    static public class ActionManager
    {
        static Dictionary<string, Type> actionClasses = new Dictionary<string, Type>();
        
        static ActionManager()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    foreach(Type interfaceType in type.GetInterfaces())
                    {
                        if (interfaceType == typeof(IAction))
                        {
                            PropertyInfo nameProperty = type.GetProperty("Name");
                            if (nameProperty != null)
                            {
                                MethodInfo getMethod = nameProperty.GetGetMethod();
                                if ((getMethod != null) && (getMethod.IsStatic))
                                {
                                    string name = (string)getMethod.Invoke(null, null);
                                    actionClasses.Add(name, type);
                                }
                            }
                        }
                    }
                }
            }
        }

        static IAction GetActionInstance(string userName)
        {
            return (IAction)Activator.CreateInstance(actionClasses[userName]);
        }

        static bool ActionExists(string actionName)
        {
            return actionClasses.ContainsKey(actionName);
        }

        public static List<string> ActionNamesList {
            get
            {
                return new List<string>(actionClasses.Keys);
            }
        }

        public static void ExecuteAction(string actionName, IList<string> parameters)
        {
            IAction actionInstance;
            if (ActionExists(actionName))
            {
                actionInstance = GetActionInstance(actionName);
            }
            else
            {
                actionInstance = new Actions.Help();
                parameters.Clear();
            }

            if (parameters.Count != actionInstance.ParametersCount)
            {
                // -1 signifie un nombre illimité d'arguments (mais pas 0)
                if ((actionInstance.ParametersCount != -1))
                {
                    string puralIfNeeded = (actionInstance.ParametersCount != 1) ? "s" : "";
                    Console.WriteLine("This action require {0} parameter{1}.", actionInstance.ParametersCount, puralIfNeeded);
                    return;
                }
            }
            actionInstance.Execute(parameters);
        }
    }
}
