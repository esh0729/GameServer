using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitJson;

namespace GameServer
{
	public class JsonUtil
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static JsonData CreateObject()
		{
			JsonData jsonData = new JsonData();
			jsonData.SetJsonType(JsonType.Object);

			return jsonData;
		}

		public static JsonData CreateArray()
		{
			JsonData jsonData = new JsonData();
			jsonData.SetJsonType(JsonType.Array);

			return jsonData;
		}
	}
}
