using System;
using System.IO;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace WindowsSocketsChat
{
	struct ClientInfo
	{
		public Guid ID; // Hash of local & remote endpoints

		public String UserName;
		public Socket ClientSocket;

		public ClientInfo(Socket ClientSocket)
		{
			this.UserName = "";
			this.ClientSocket = ClientSocket;
			this.ID = new Guid();
			this.ID = CreateClientID(ClientSocket);

		}

		public static Guid CreateClientID(Socket Sckt)
		{
			// When accepting a connection from a client, a port is opened locally for communication
			// between server and client. Meaning each client's connection has its own port, this 
			// can be used to identify that connection
			String strToHash =	Sckt.LocalEndPoint.ToString() +
								Sckt.RemoteEndPoint.ToString();

			Byte[] bytesToHash = Encoding.ASCII.GetBytes(strToHash);


			// Hash this into a GUID
			MD5 Hasher = MD5.Create();
			Byte[] Hash = Hasher.ComputeHash(bytesToHash);
			Guid id = new Guid(Hash);

			return id;//Encoding.ASCII.GetString(Hash);
		}
	}

	public class ServerProcess
	{
		// Config values
		int PortNumber;

		List<ClientInfo> Clients = new List<ClientInfo>();

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

		public void ProcessMessage(Socket FromSocket, Byte[] Message)
		{
			if(Messaging.GetMessageCommand(Message) == Command.SendMsg)
			{
				// Fill a struct to provide all clients with all the info they need about this received message
				foreach(ClientInfo client in Clients)
				{
					ClientInfo Client;
					if (GetClientBySocket(FromSocket, out Client))
					{
						String MsgText = Messaging.GetDataAsString(Message);
						ChatMessage ChatMsg = new ChatMessage(Client.UserName, MsgText);

						// Convert the ChatMessage object into a byte array
						Byte[] MsgObj = ChatMsg.ToByteArray();

						String strMessageToSend = "SNDMSG|";
						Byte[] BytesMsgText = Encoding.ASCII.GetBytes(strMessageToSend);


						Byte[] BytesToSend = new Byte[BytesMsgText.Length + MsgObj.Length];
						Array.Copy(BytesMsgText, 0, BytesToSend, 0, BytesMsgText.Length);
						Array.Copy(MsgObj, 0, BytesToSend, BytesMsgText.Length, MsgObj.Length);

						Logger.Log("Sending message: " + Encoding.ASCII.GetString(BytesToSend));

						client.ClientSocket.Send(BytesToSend);
					}
				}
			}
		}

		// Need to tidy this up
		private bool GetClientBySocket(Socket ClientSocket, out ClientInfo Client)
		{
			Guid ID = ClientInfo.CreateClientID(ClientSocket);

			foreach(ClientInfo ThisClient in Clients)
			{
				if (ThisClient.ID == ID)
				{
					Client = ThisClient;
					return true;
				}
			}

			Client = new ClientInfo();
			return false;
		}

		// Callbacks
		public void OnAcceptConnection(IAsyncResult result)
		{
			try
			{
				Socket ClientSckt = LstnSckt.EndAccept(result);
				ClientSckt.NoDelay = false;

				// Register the client
				ClientInfo client = new ClientInfo(ClientSckt);
				Clients.Add(client);
				Logger.Log("Client " + client.ID + " connected");

				
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

					ProcessMessage(ClientSckt, Buffer);


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
