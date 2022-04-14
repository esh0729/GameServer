using System;
using System.Configuration;

namespace GameServer
{
	public class AppConfig
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static Properties

		public static int gameServerId
		{
			get { return Convert.ToInt32(ConfigurationManager.AppSettings["gameServerId"]); }
		}

		public static string userDBConnection
		{
			get { return ConfigurationManager.AppSettings["userDBConnection"]; }
		}

		public static string accessTokeySecurityKey
		{
			get { return ConfigurationManager.AppSettings["accessTokenSecurityKey"]; }
		}
	}
}
