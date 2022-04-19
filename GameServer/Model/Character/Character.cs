using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace GameServer
{
	public class Character
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private int m_nId = 0;

		private float m_fMoveSpeed = 0f;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public int id
		{
			get { return m_nId; }
		}

		public float moveSpeed
		{
			get { return m_fMoveSpeed; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Set(DataRow dr)
		{
			if (dr == null)
				throw new ArgumentNullException("dr");

			m_nId = Convert.ToInt32(dr["characterId"]);

			m_fMoveSpeed = Convert.ToSingle(dr["moveSpeed"]);
		}
	}
}
