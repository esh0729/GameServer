using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace GameServer
{
	public class Continent : Location
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private int m_nId = 0;

		private Rect3D m_rect = Rect3D.zero;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public int id
		{
			get { return m_nId; }
		}

		public Rect3D rect
		{
			get { return m_rect; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Set(DataRow dr)
		{
			if (dr == null)
				throw new ArgumentNullException("dr");

			m_nId = Convert.ToInt32(dr["continentId"]);

			m_rect.x = Convert.ToSingle(dr["x"]);
			m_rect.y = Convert.ToSingle(dr["y"]);
			m_rect.z = Convert.ToSingle(dr["z"]);
			m_rect.xSize = Convert.ToSingle(dr["xSize"]);
			m_rect.ySize = Convert.ToSingle(dr["ySize"]);
			m_rect.zSize = Convert.ToSingle(dr["zSize"]);
		}

		public bool Contains(Vector3 position)
		{
			return m_rect.Contains(position);
		}
	}
}
