using System;
using System.IO;
using System.Text;

namespace WindowsSocketsChat
{
	public static class Logger
	{
		private static String FileName;
		//private static FileStream Stream;

		public static String CRLF = "\r\n";

		public static bool Open(String FileName)
		{
			FileStream s;
			return Open(FileName, out s);
		}

		public static void Log(String Message)
		{
			if (FileName != "")
			{
				FileStream s;

				if (Open(FileName, out s))
				{
					Byte[] strBytes = Encoding.ASCII.GetBytes(Message + CRLF);
					try
					{
						s.Write(strBytes, 0, strBytes.Length);
					}
					catch (Exception ex)
					{
						// TODO: Log here...
					}
					finally
					{
						s.Close();
					}
				}
			}
		}
		public static void Log(Byte[] Message)
		{
			if (FileName != "")
			{
				FileStream s;

				if (Open(FileName, out s))
				{
					try
					{
						s.Write(Message, 0, Message.Length);

						Byte[] strBytes = Encoding.ASCII.GetBytes(CRLF);
						s.Write(strBytes, 0, strBytes.Length);
					}
					catch (Exception ex)
					{
						// TODO: Log here...
					}
					finally
					{
						s.Close();
					}
				}
			}
		}

		private static bool Open(String FileName, out FileStream Stream)
		{
			try
			{
				if (!File.Exists(FileName))
					File.Create(FileName);
			}
			catch (Exception ex)
			{
				Stream = null;
				return false;
			}

			Stream = File.Open(FileName, FileMode.Truncate);

			return true;
		}
	}
}
