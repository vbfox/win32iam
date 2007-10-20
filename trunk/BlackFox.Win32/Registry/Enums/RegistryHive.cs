namespace BlackFox.Win32
{
    public enum RegistryHive : uint
    {
        /// <summary>
        /// Registry entries subordinate to this key define types (or classes)
        /// of documents and the properties associated with those types. Shell
        /// and COM applications use the information stored under this key.
        /// </summary>
        ClassesRoot = 0x80000000,
        /// <summary>
        /// Registry entries subordinate to this key define the preferences of
        /// the current user.
        /// </summary>
        CurrentUser = 0x80000001,
        /// <summary>
        /// Registry entries subordinate to this key define the physical state
        /// of the computer, including data about the bus type, system memory,
        /// and installed hardware and software. 
        /// </summary>
        LocalMachine = 0x80000002,
        /// <summary>
        /// Registry entries subordinate to this key define the default user
        /// configuration for new users on the local computer and the user
        /// configuration for the current user.
        /// </summary>
        Users = 0x80000003,
        /// <summary>
        /// Registry entries subordinate to this key allow you to access
        /// performance data. 
        /// </summary>
        PerformanceData = 0x80000004,
        /// <summary>
        /// Contains information about the current hardware profile of the local 
        /// computer system.
        /// </summary>
        CurrentConfig = 0x80000005,
        /// <summary>
        /// Windows Me/98/95:  Registry entries subordinate to this key allow 
        /// you to collect performance data.
        /// </summary>
        DynData = 0x80000006
    }
}
