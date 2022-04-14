using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
	public class DateTimeUtil
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public static DateTimeOffset currentTime
		{
			get { return DateTimeOffset.Now; }
		}
	}
}
