using System;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace SocketChat_Client
{
	class ClientWindow : Form
	{
		// Config values
		String ConnectionAddress;
		int ConnectionPort;

		// Windows Forms Controls
		Panel		TopGroup;
		ListView	ClientsView;
		TextBox		MsgsBox;

		Panel	BottomGroup;
		Label	ConnectionStatus;
		TextBox InputBox;
		Button	SendButton;

		public ClientWindow()
		{
			if (!LoadConfig())
				Close();

			InitializeComponents();
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
			BottomGroup.Controls.AddRange(new Control[] { InputBox, ConnectionStatus, SendButton });


			// Finalise form
			Controls.AddRange(new Control[] { TopGroup, BottomGroup });
			ResumeLayout(true);
		}

		private bool LoadConfig()
		{
			try
			{
				ConnectionAddress = ConfigurationManager.AppSettings["ServerAddress"];

				String Port = ConfigurationManager.AppSettings["ServerPort"];

				if (ConnectionAddress == null || Port == null)
				{
					MessageBox.Show("Configuration file invalid! ServerAddress or ServerPort incorrect.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				ConnectionPort = int.Parse(Port);
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
			MessageBox.Show("Message sending is not enabled yet");
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
