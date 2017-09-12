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
			try
			{
				if (FileName == "" || FileName == null)
					return false;

				Logger.FileName = FileName;

				if (!File.Exists(FileName))
					File.Create(FileName);

				Stream s = File.Open(FileName, FileMode.Truncate);
				s.Close();

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static void Log(String Message)
		{
			if (FileName != "")
			{
				try
				{
					FileStream s = File.Open(FileName, FileMode.Append);

					Byte[] strBytes = Encoding.ASCII.GetBytes(Message + CRLF);
					s.Write(strBytes, 0, strBytes.Length);

					s.Close();
				}
				catch (Exception ex)
				{
					// TODO: Log here...
				}
			}
		}
		public static void Log(Byte[] Message)
		{
			if (FileName != "")
			{
				try
				{
					FileStream s = File.Open(FileName, FileMode.Append);

					s.Write(Message, 0, Message.Length);

					Byte[] strBytes = Encoding.ASCII.GetBytes(CRLF);
					s.Write(strBytes, 0, strBytes.Length);

					s.Close();
				}
				catch (Exception ex)
				{
					// TODO: Log here...
				}
			}
		}
	}
}
