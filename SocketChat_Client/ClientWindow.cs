using System;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketChat_Client
{
	class ClientWindow : Form
	{
		// Config values
		IPAddress ConnectionAddress;
		int ConnectionPort;

		// Windows Forms Controls
		Panel		TopGroup;
		ListView	ClientsView;
		TextBox		MsgsBox;

		Panel	BottomGroup;
		Label	ConnectionStatus;
		TextBox InputBox;
		Button	SendButton;


		Socket Sckt;


		public ClientWindow()
		{
			if (!LoadConfig())
				Close();

			InitializeComponents();

			Sckt = new Socket(SocketType.Stream, ProtocolType.Tcp);

			ConnectToServer();
		}

		private void InitializeComponents()
		{
			Text = "SocketMessenger - Client";
			Size = new Size(800, 600);
			SuspendLayout();

			TopGroup = new Panel();
			ClientsView = new ListView();
			MsgsBox = new TextBox();

			BottomGroup = new Panel();
			ConnectionStatus = new Label();
			InputBox = new TextBox();
			SendButton = new Button();

			//
			// ClientsView
			ClientsView.View = View.List;
			ClientsView.Dock = DockStyle.Left;

			//
			// MessageBox
			MsgsBox.Dock = DockStyle.Fill;
			MsgsBox.Multiline = true;
			MsgsBox.Enabled = false;
			MsgsBox.Text = "";

			//
			// Top Group
			TopGroup.Dock = DockStyle.Fill;
			TopGroup.Controls.AddRange(new Control[] { MsgsBox, ClientsView });

			//
			// ConnectionStatus
			ConnectionStatus.Text = "Not Connected";
			ConnectionStatus.Dock = DockStyle.Left;

			//
			// InputBox
			InputBox.Dock = DockStyle.Fill;
			InputBox.KeyDown += InputBox_KeyDown;
			InputBox.Multiline = true;

			//
			// Send Button
			SendButton.Text = "Send";
			SendButton.AutoSize = true;
			SendButton.Dock = DockStyle.Right;
			SendButton.Click += SendButton_Click;
			SendButton.MaximumSize = new Size(SendButton.Width, 30);

			//
			// BottomGroup
			BottomGroup.Dock = DockStyle.Bottom;
			BottomGroup.Padding = new Padding(5);
			BottomGroup.Controls.AddRange(new Control[] { InputBox, ConnectionStatus, SendButton });
			BottomGroup.MaximumSize = new Size(MaximumSize.Width, SendButton.Height);


			// Finalise form
			Controls.AddRange(new Control[] { TopGroup, BottomGroup });
			ResumeLayout(true);
		}

		private bool LoadConfig()
		{
			try
			{
				String strAddress	= ConfigurationManager.AppSettings["ServerAddress"];
				String strPort		= ConfigurationManager.AppSettings["ServerPort"];

				if (strAddress == null || strPort == null)
				{
					MessageBox.Show("Configuration file invalid! ServerAddress or ServerPort incorrect.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				ConnectionAddress = Dns.GetHostEntry(strAddress).AddressList[0];

				if(!int.TryParse(strPort, out ConnectionPort))
				{
					MessageBox.Show("Port number \"" + strPort + "\" is not valid");
					return false;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			return true;
		}

		private void SendMessage()
		{
			try
			{
				Byte[] Bytes = Encoding.ASCII.GetBytes(InputBox.Text);
				Sckt.Send(Bytes);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Exception Occurred");
			}
		}

		private bool ConnectToServer()
		{
			try
			{
				IPEndPoint RemoteEP = new IPEndPoint(ConnectionAddress, ConnectionPort);
				// TODO: Need to make an async request via BeginConnect, so that UI 
				// thread is not waiting on response from server
				Sckt.Connect(RemoteEP);

				Byte[] Buffer = new Byte[1024];

				Sckt.BeginReceive( Buffer, 0, Buffer.Length, SocketFlags.None, 
									new AsyncCallback(OnReceiveData), 
									new object[] { Buffer, Sckt });
			}
			catch (SocketException ex)
			{
				// TODO: Handle connection error
				MessageBox.Show(ex.ToString(), "Exception Occurred");
				return false;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Exception Occurred");
				return false;
			}

			return true;
		}

		private void ProcessMessage(String Message)
		{

		}


		// Callbacks
		public void OnReceiveData(IAsyncResult result)
		{
			object[] state = new object[2];
			state = (object[])result.AsyncState;

			Byte[] Buffer = (Byte[])state[0];
			Sckt = (Socket)state[1];

			try
			{
				int NumBytes = Sckt.EndReceive(result); 
				if (NumBytes > 0)
				{
					String Message = Encoding.ASCII.GetString(Buffer);
					ProcessMessage(Message);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Exception Occurred");
			}

			try
			{
				Buffer = new Byte[1024];
				state[0] = Buffer;

				Sckt.BeginReceive(Buffer, 0, Buffer.Length,
								SocketFlags.None, new AsyncCallback(OnReceiveData), state);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Exception Occurred");
			}
		}


		// Events
		private void InputBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;

				SendMessage();
			}
		}

		private void SendButton_Click(object sender, EventArgs e)
		{
			SendMessage();
		}
	}
}
