using System.ServiceProcess;
using WindowsSocketsChat;

namespace SocketChat_Server
{
	class ServerService : ServiceBase
	{
		ServerProcess Process;

		public ServerService()
		{
			Process = new ServerProcess();
		}

		protected override void OnStart(string[] args)
		{
			Process.StartProcess();

			base.OnStart(args);
		}

		protected override void OnStop()
		{
			Process.StartProcess();

			base.OnStop();
		}
	}
}
