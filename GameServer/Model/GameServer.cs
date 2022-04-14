using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;

namespace GameServer
{
	public class GameServer
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private int m_nId = 0;

		private string m_sDBPath = null;
		private Regex m_characterNameRegex = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public int id
		{
			get { return m_nId; }
		}

		public string dbPath
		{
			get { return m_sDBPath; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Set(DataRow dr)
		{
			if (dr == null)
				throw new ArgumentNullException("dr");

			m_nId = Convert.ToInt32(dr["gameServerId"]);

			m_sDBPath = Convert.ToString(dr["dbPath"]);

			string sCharacterNameRegex = Convert.ToString(dr["characterNameRegex"]);
			m_characterNameRegex = new Regex(sCharacterNameRegex);
		}
	}
}
