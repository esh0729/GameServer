using System;
using System.Data;

namespace GameServer
{
	public class DBUtil
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//
		// Guid
		//

		public static Guid ToGuid(object obj)
		{
			return ToGuid(obj, Guid.Empty);
		}

		public static Guid ToGuid(object obj, Guid whenNullValue)
		{
			if (obj == null)
				return whenNullValue;

			return obj != DBNull.Value ? (Guid)obj : whenNullValue;
		}

		//
		// DateTimeOffset
		//

		public static DateTimeOffset ToDateTimeOffset(object obj)
		{
			return ToDateTimeOffset(obj, DateTimeOffset.MinValue);
		}

		public static DateTimeOffset ToDateTimeOffset(object obj, DateTimeOffset whenNullValue)
		{
			if (obj == null)
				return whenNullValue;

			return obj != DBNull.Value ? (DateTimeOffset)obj : whenNullValue;
		}
	}
}
