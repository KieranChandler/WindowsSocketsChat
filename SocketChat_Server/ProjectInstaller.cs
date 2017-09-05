using System.Configuration.Install;
using System.ComponentModel;
using System.ServiceProcess;

namespace SocketChat_Server
{
	[RunInstaller(true)]
	public class ProjectInstaller : Installer
	{
		ServiceProcessInstaller ProcInstaller;
		ServiceInstaller SvcInstaller;

		public ProjectInstaller()
		{
			ProcInstaller = new ServiceProcessInstaller();
			SvcInstaller = new ServiceInstaller();

			ProcInstaller.Account = ServiceAccount.NetworkService;
			ProcInstaller.Username = null;
			ProcInstaller.Password = null;

			SvcInstaller.ServiceName = "SocketChatServer";

			Installers.AddRange(new Installer[] { ProcInstaller, SvcInstaller });
		}
	}
}
