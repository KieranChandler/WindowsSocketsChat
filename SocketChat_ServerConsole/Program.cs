﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsSocketsChat;

namespace SocketChat_ServerConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			ServerProcess Process = new ServerProcess();
			Process.StartProcess();

			Console.WriteLine("Press any key to exit the program...");
			Console.ReadLine();

			Process.StopProcess();
		}
	}
}
