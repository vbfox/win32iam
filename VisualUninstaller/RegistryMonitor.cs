using System;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics;

namespace RegistryUtils
{
	/// <summary>
	/// Filter for notifications reported by <seealso cref="RegistryUtils.RegistryMonitor"/>.
	/// </summary>
	[Flags]
	public enum RegChangeNotifyFilter
	{
		/// <summary>Notify the caller if a subkey is added or deleted.</summary>
//		REG_NOTIFY_CHANGE_NAME = 1,
		Key = 1,
		/// <summary>Notify the caller of changes to the attributes of the key,
		/// such as the security descriptor information.</summary>
//		REG_NOTIFY_CHANGE_ATTRIBUTES = 2,
		Attribute = 2,
		/// <summary>Notify the caller of changes to a value of the key. This can
		/// include adding or deleting a value, or changing an existing value.</summary>
//		REG_NOTIFY_CHANGE_LAST_SET = 4,
		Value = 4,
		/// <summary>Notify the caller of changes to the security descriptor
		/// of the key.</summary>
//		REG_NOTIFY_CHANGE_SECURITY = 8,
		Security = 8,
	}

	/// <summary>
	/// <b>RegistryMonitor</b> allows you to monitor specific registry key.
	/// </summary>
	/// <remarks>
	/// If a monitored registry key changes, an event is fired. You can subscribe to these
	/// events by adding a delegate to <see cref="RegChanged"/>.
	/// <para>The Windows API provides a function
	/// <a href="http://msdn.microsoft.com/library/en-us/sysinfo/base/regnotifychangekeyvalue.asp">
	/// RegNotifyChangeKeyValue</a>, which is not covered by the
	/// <see cref="Microsoft.Win32.RegistryKey"/> class. <see cref="RegistryMonitor"/> imports
	/// that function and encapsulates it in a convenient manner.
	/// </para>
	/// </remarks>
	/// <example>
	/// This sample shows how to monitor <c>HKEY_CURRENT_USER\Environment</c> for changes:
	/// <code>
	/// public class MonitorSample
	/// {
	///     static void Main() 
	///     {
	///         RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Environment", false);
	///         RegistryMonitor monitor = new RegistryMonitor(key);
	///         monitor.RegChanged += new EventHandler(OnRegChanged);
	///         monitor.Start();
	///
	///         while(true);
	///     }
	///
	///     private void OnRegChanged(object sender, EventArgs e)
	///     {
	///         Console.WriteLine("registry key has changed");
	///     }
	/// }
	/// </code>
	/// </example>
	public class RegistryMonitor : IDisposable
	{
		#region DllImport

		[DllImport("advapi32.dll")]
		private static extern Int32 RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree, RegChangeNotifyFilter dwNotifyFilter, IntPtr hEvent, bool fAsynchronous);

		#endregion

		#region Event handling

		/// <summary>
		/// Fired when the registry key has changed.
		/// </summary>
		public event EventHandler RegChanged;

		/// <summary>
		/// Fired when the monitoring thread has thrown an error.
		/// </summary>
		public event System.IO.ErrorEventHandler Error;

		#endregion

		#region Private member variables

		private RegistryKey registryKey;
		private Thread thread;
		private AutoResetEvent eventNotify = new AutoResetEvent(false);
		private ManualResetEvent eventTerminate = new ManualResetEvent(false);
		private RegChangeNotifyFilter regFilter = RegChangeNotifyFilter.Key | RegChangeNotifyFilter.Attribute | RegChangeNotifyFilter.Value | RegChangeNotifyFilter.Security;
		private Exception thrownException = null;

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="registryKey">The registry key to monitor.</param>
		public RegistryMonitor(RegistryKey registryKey)
		{
			this.registryKey = registryKey;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public RegistryMonitor()
		{
		}

		/// <summary>
		/// Disposes this object.
		/// </summary>
		public void Dispose()
		{
			Stop();
		}

		/// <summary>
		/// Gets or sets the <see cref="RegChangeNotifyFilter">RegChangeNotifyFilter</see>.
		/// </summary>
		public RegChangeNotifyFilter RegChangeNotifyFilter
		{
			get { return regFilter; }
			set
			{
				if (IsMonitoring)
					throw new InvalidOperationException("Monitoring thread is already running");

				regFilter = value;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="RegistryKey"RegistryKey</see> which should be monitored.
		/// </summary>
		public RegistryKey RegistryKey
		{
			get { return registryKey; }
			set
			{
				if (IsMonitoring)
					throw new InvalidOperationException("Monitoring thread is already running");

				registryKey = value;
			}
		}

		/// <summary>
		/// If the monitoring thread has thrown an exception, you can retrieve it
		/// via this property.
		/// </summary>
		public Exception ThrownException
		{
			get { return thrownException; }
		}

		/// <summary>
		/// <b>true</b> if this <see cref="RegistryMonitor"/> object is currently monitoring;
		/// otherwise, <b>false</b>.
		/// </summary>
		public bool IsMonitoring
		{
			get { return thread != null; }
		}

		/// <summary>
		/// Gets the Win32 handle for a given RegistryKey.
		/// </summary>
		/// <param name="registryKey">registry key you want the handle for</param>
		/// <returns>Desired handle</returns>
		public static IntPtr GetRegistryHandle(RegistryKey registryKey)
		{
			Type type = registryKey.GetType();
			FieldInfo fieldInfo = type.GetField("hkey", BindingFlags.Instance | BindingFlags.NonPublic);
			return (IntPtr)fieldInfo.GetValue(registryKey);
		}

		/// <summary>
		/// Waits for both a registry change and the terminate event, which is set by <see cref="Stop"/>.
		/// </summary>
		/// <returns><c>True</c>, if the registry key has changed.</returns>
		private bool WaitForChange()
		{
			WaitHandle[] waitHandles = new WaitHandle[] { eventNotify, eventTerminate };
			Int32 retValue = RegNotifyChangeKeyValue(GetRegistryHandle(registryKey), true, regFilter, eventNotify.SafeWaitHandle.DangerousGetHandle(), true);
			if (retValue == 0)
			{
				return WaitHandle.WaitAny(waitHandles) == 0;
			}
			else
			{
				throw new System.ComponentModel.Win32Exception(retValue, "RegNotifyChangeKeyValue");
			}
		}        

		/// <summary>
		/// Start monitoring.
		/// </summary>
		public void Start()
		{
			if (!IsMonitoring)
			{
				eventTerminate.Reset();
				thread = new Thread(new ThreadStart(ThreadLoop));
				thread.IsBackground = true;
				thread.Start();
			}
		}

		/// <summary>
		/// Stops the monitoring thread.
		/// </summary>
		public void Stop()
		{
			if (IsMonitoring)
			{
				eventTerminate.Set();
				thread.Join();
				thread = null;
			}
		}

		/// <summary>
		/// Monitoring thread. Calls <see cref="WaitForChange"/> till the terminate event is set by <see cref="Stop"/>.
		/// </summary>
		private void ThreadLoop()
		{	
			try
			{
				while (!eventTerminate.WaitOne(0, true))
				{
					if (WaitForChange())
					{
						if (RegChanged != null)
							RegChanged(this, EventArgs.Empty);
					}
				}
			}
			catch (Exception e)
			{
				thrownException = e;

				if (Error != null)
					Error(this, new System.IO.ErrorEventArgs(e));

				thread = null;
			}
		}
	}
}
