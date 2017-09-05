using System;
using System.IO;
using System.Configuration;

namespace WindowsSocketsChat
{
	public class ServerProcess
	{
		// Config values
		int PortNumber;

		public void StartProcess()
		{
			// Load config and exit if it's not successful
			if (!LoadConfig())
				return;

			// Get path to EXE in a way that works for Windows services
			// Requires System.Windows.Forms, which is not ideal
			String LogFileName = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\" + "Log.txt";
			Logger.Open(LogFileName);
		}

		public void StopProcess()
		{
			// Perform any cleanup/shutdown necessary
		}

		public bool LoadConfig()
		{
			try
			{
				String strPort = ConfigurationManager.AppSettings["ListenPort"];

				if (strPort == "")
				{
					Console.WriteLine("Configuration file invalid:\n ServerPort incorrect.");
					return false;
				}

				PortNumber = int.Parse(strPort);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return false;
			}

			return true;
		}
	}
}
