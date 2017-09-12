using System;
using System.IO;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WindowsSocketsChat
{
	public class ServerProcess
	{
		// Config values
		int PortNumber;

		Socket LstnSckt;

		public void StartProcess()
		{
			// Load config and exit if it's not successful
			if (!LoadConfig())
				return;

			// Get path to EXE in a way that works for Windows services
			// Requires System.Windows.Forms, which is not ideal
			String LogFileName = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\" + "Log.txt";
			Logger.Open(LogFileName);


			// Initialize the socket
			LstnSckt = new Socket(SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint LocalEP = new IPEndPoint(Dns.GetHostEntry("").AddressList[0], PortNumber);
			// Bind the socket to a local port
			LstnSckt.Bind(LocalEP);
			

			// Place the socket in a listening state
			LstnSckt.Listen(10);
			// Begin listening for connection requests
			AsyncCallback AcceptCallback = new AsyncCallback(OnAcceptConnection);
			LstnSckt.BeginAccept(AcceptCallback, LstnSckt);
		}

		public void StopProcess()
		{
			// Perform any cleanup/shutdown necessary
			try
			{
				LstnSckt.Shutdown(SocketShutdown.Receive);
				LstnSckt.Close();
			} catch (Exception ex)
			{
				Logger.Log(ex.ToString());
			}
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

		public void ProcessMessage(String Message)
		{

		}

		// Callbacks
		public void OnAcceptConnection(IAsyncResult result)
		{
			try
			{
				// TODO: Register this client as a user

				Socket ClientSckt = LstnSckt.EndAccept(result);
				ClientSckt.NoDelay = false;

				Logger.Log("Client connected");

				
				// Start listening for data on the new client socket
				Byte[] Buffer = new Byte[1024];

				ClientSckt.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None,
										new AsyncCallback(OnReceiveData),
										new object[] { Buffer, ClientSckt });


				// Continue to listen for incoming connection requests again
				AsyncCallback AcceptCallback = new AsyncCallback(OnAcceptConnection);
				LstnSckt.BeginAccept(AcceptCallback, LstnSckt);
			}
			catch (Exception ex)
			{
				Logger.Log(ex.ToString()); // TODO: More specific error handling
			}
		}

		public void OnReceiveData(IAsyncResult result)
		{
			try
			{
				object[] state = new object[2];
				state = (object[])result.AsyncState;


				Byte[] Buffer = (Byte[])state[0];
				Socket ClientSckt = (Socket)state[1];

				// Finish the async request, giving us the length of the message inside the buffer
				int NumBytes = ClientSckt.EndReceive(result);
				if(NumBytes > 0)
				{
					// Convert buffer to string and remove unwanted chars
					String Message = Encoding.ASCII.GetString(Buffer);
					Message = Message.Substring(0, NumBytes);

					Logger.Log("Message received from client: " + Message);

					ProcessMessage(Message);


					// Continue to receive data
					ClientSckt.BeginReceive(Buffer, 0, Buffer.Length,
									SocketFlags.None, new AsyncCallback(OnReceiveData), state);
				}
			}
			catch (Exception ex)
			{
				Logger.Log(ex.ToString()); // TODO: More specific error handling
			}
		}
	}
}
