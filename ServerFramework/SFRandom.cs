using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerFramework
{
	public class SFRandom
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		private static Random s_random = new Random();
		private static object s_syncObject = new object();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		//
		// int
		//

		public static int NextInt(int nMaxValue)
		{
			lock (s_syncObject)
			{
				return s_random.Next(nMaxValue);
			}
		}

		public static int NextInt(int nMinValue, int nMaxValue)
		{
			lock (s_syncObject)
			{
				return s_random.Next(nMinValue, nMaxValue);
			}
		}

		//
		// float
		//

		public static float NextFloat(float fMaxValue)
		{
			lock (s_syncObject)
			{
				return (float)(s_random.NextDouble() * fMaxValue);
			}
		}

		public static float NextFloat(float fMinValue, float fMaxValue)
		{
			lock (s_syncObject)
			{
				return fMaxValue - (float)(s_random.NextDouble() * (fMaxValue - fMinValue));
			}
		}
	}
}
