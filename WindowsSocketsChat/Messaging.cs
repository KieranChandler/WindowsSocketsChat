using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace WindowsSocketsChat
{
	public enum Command : uint
	{
		None		= 0,
		SendMsg		= 1,
		ListClients = 2,
		ChangeName	= 3
	}

	[Serializable]
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

		public static ChatMessage FromByteArray(Byte[] Bytes)
		{
			BinaryFormatter Formatter = new BinaryFormatter();

			MemoryStream Stream = new MemoryStream(Bytes);

			return ((ChatMessage)Formatter.Deserialize(Stream));
		}

		public Byte[] ToByteArray()
		{
			BinaryFormatter Formatter	= new BinaryFormatter();
			MemoryStream	Stream		= new MemoryStream();

			Formatter.Serialize(Stream, this);

			return Stream.ToArray();
		}
	}

	//-------------------------------------------
	// message format follows:
	// COMMAND|BINARYDATA
	//-------------------------------------------

	public class Messaging
	{
		public static Byte CommandSeparator = 124; // ASCII code 124 = '|'

		public static Command GetMessageCommand(Byte[] Message)
		{

			int CommandLength = Array.IndexOf(Message, CommandSeparator);	// -1 because the command identifier 
			if (CommandLength > 0)                                              // should not include the |
			{
				// Extract the command identifier bytes
				Byte[] CommandBytes = new Byte[CommandLength];
				Array.Copy(Message, CommandBytes, CommandLength);

				String strCommand = Encoding.ASCII.GetString(CommandBytes);
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
			else
			{
				return Command.None;
			}
		}

		public static Byte[] GetData(Byte[] Message)
		{
			int CommandLength = Array.IndexOf(Message, CommandSeparator) + 1;
			if (CommandLength > 0)                                          // should not include the |
			{
				int MsgDataLength = Message.Length - CommandLength;

				Byte[] MsgData = new Byte[MsgDataLength];
				Array.Copy(Message, CommandLength, MsgData, 0, MsgDataLength);

				return MsgData;
			}
			else
				return new Byte[0];
		}

		public static String GetDataAsString(Byte[] Message)
		{
			return Encoding.ASCII.GetString(GetData(Message));
		}
	}
}
