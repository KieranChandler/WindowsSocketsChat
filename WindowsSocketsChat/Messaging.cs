using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsSocketsChat
{
	public enum Command : uint
	{
		None		= 0,
		SendMsg		= 1,
		ListClients = 2,
	}

	public struct ChatMessage
	{
		public String UserName;
		public String MessageText;
		public DateTime TimeStamp;

		public ChatMessage(String UserName, String MessageText)
		{
			this.UserName = UserName;
			this.MessageText = MessageText;

			this.TimeStamp = new DateTime();
		}

		public ChatMessage(String UserName, String MessageText, DateTime TimeStamp)
		{
			this.UserName = UserName;
			this.MessageText = MessageText;
			this.TimeStamp = TimeStamp;
		}
	}

	//-------------------------------------------
	// message format follows:
	// COMMAND|BINARYDATA
	//-------------------------------------------

	public class Messaging
    {
		public static Command GetMessageCommand(String Message)
		{
			String strCommand = "";

			int CommandLength = Message.IndexOf('|');
			if (CommandLength > 0)
				strCommand = Message.Substring(0, CommandLength);

			strCommand = strCommand.ToUpper();

			switch (strCommand)
			{
				default:
					return Command.None;

				case "SNDMSG":
					return Command.SendMsg;

				case "LSTCS":
					return Command.ListClients;
			}
		}

		public static String GetMessageData(String Message)
		{
			int CommandLength = Message.IndexOf('|');

			if (CommandLength > 0)
				return Message.Substring(CommandLength, Message.Length - CommandLength);
			else
				return "Invalid";
		}
	}
}
