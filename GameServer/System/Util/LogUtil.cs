using System;
using System.IO;
using System.Text;

using ServerFramework;

namespace GameServer
{
    public class LogUtil
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Constants

        private const string kTimeStringFormat = "[yyyy'-'MM'-'dd' 'HH':'mm':'ss,fff]";

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Static member functions

        //
        // INFO
        //

        public static void Info(Type type, string sMessage)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTimeOffset.Now.ToString(kTimeStringFormat));
            sb.Append(" | ");
            sb.Append("INFO");
            sb.Append(" | ");

            if (type != null)
            {
                sb.Append(type.Namespace);
                sb.Append(".");
                sb.Append(type.Name);
                sb.Append(" : ");
            }

            sb.Append(sMessage);

            Server.instance.AddLogWork(new SFAction<StringBuilder>(WriteLog, sb));
        }

        //
        // WARN
        //

        public static void Warn(Type type, string sMessage)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTimeOffset.Now.ToString(kTimeStringFormat));
            sb.Append(" | ");
            sb.Append("WARN");
            sb.Append(" | ");

            if (type != null)
            {
                sb.Append(type.Namespace);
                sb.Append(".");
                sb.Append(type.Name);
                sb.Append(" : ");
            }

            sb.Append(sMessage);

            Server.instance.AddLogWork(new SFAction<StringBuilder>(WriteLog, sb));
        }

        //
        // ERROR
        //

        public static void Error(Type type, Exception ex)
        {
            Error(type, ex.Message, true, ex.StackTrace);
        }

        public static void Error(Type type, string sMessage)
        {
            Error(type, sMessage, false, null);
        }

        public static void Error(Type type, string sMessage, bool bLoggingTrace, string sStackTrace)
        {
            Error(type, null, sMessage, bLoggingTrace, sStackTrace);
        }

        public static void Error(Type type, StringBuilder sb, string sMessage, bool bLoggingTrace, string sStackTrace)
		{
            if (sb == null)
                sb = new StringBuilder();

            sb.Append(DateTimeOffset.Now.ToString(kTimeStringFormat));
            sb.Append(" | ");
            sb.Append("ERROR");
            sb.Append(" | ");

            if (type != null)
            {
                sb.Append(type.Namespace);
                sb.Append(".");
                sb.Append(type.Name);
                sb.Append(" : ");
            }

            sb.Append(sMessage);

            if (bLoggingTrace)
            {
                sb.AppendLine();
                sb.Append(sStackTrace);
            }

            Server.instance.AddLogWork(new SFAction<StringBuilder>(WriteLog, sb));
        }

        //
        //
        //

        private static void WriteLog(StringBuilder sb)
        {
            string sPath = Environment.CurrentDirectory + @"\Error";
            DirectoryInfo di = new DirectoryInfo(sPath);

            if (!di.Exists)
                di.Create();

            string sDate = DateTimeOffset.Now.ToString("yyyy-MM-dd");
            string sFilePath = sPath + @"\" + sDate + ".txt";

            if (!File.Exists(sFilePath))
            {
                using (StreamWriter writer = File.CreateText(sFilePath))
                {
                    writer.WriteLine(sb.ToString());
                }
            }
            else
            {
                using (StreamWriter writer = File.AppendText(sFilePath))
                {
                    writer.WriteLine(sb.ToString());
                }
            }
        }
    }
}
