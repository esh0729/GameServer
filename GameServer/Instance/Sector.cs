using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
	public class Sector
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		private PhysicalPlace m_physicalPlace = null;
		private int m_nRow = -1;
		private int m_nCol = -1;
		private Vector3 m_position = Vector3.zero;

		private Dictionary<Guid,Hero> m_heroes = new Dictionary<Guid,Hero>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public Sector(PhysicalPlace physicalPlace, int nRow, int nCol, Vector3 position)
		{
			if (physicalPlace == null)
				throw new ArgumentNullException("physicalPlace");

			m_physicalPlace = physicalPlace;
			m_nRow = nRow;
			m_nCol = nCol;
			m_position = position;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public PhysicalPlace physicalPlace
		{
			get { return m_physicalPlace; }
		}

		public int row
		{
			get { return m_nRow; }
		}

		public int col
		{
			get { return m_nCol; }
		}

		public Vector3 position
		{
			get { return m_position; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void AddHero(Hero hero)
		{
			if (hero == null)
				throw new ArgumentNullException("hero");

			m_heroes.Add(hero.id, hero);
		}

		public void RemoveHero(Guid heroId)
		{
			m_heroes.Remove(heroId);
		}

		public Hero GetHero(Guid heroId)
		{
			Hero value;

			return m_heroes.TryGetValue(heroId, out value) ? value : null;
		}

		public List<ClientPeer> GetClientPeers(Guid heroIdToExclude)
		{
			List<ClientPeer> clientPeers = new List<ClientPeer>();

			foreach (Hero hero in m_heroes.Values)
			{
				if (hero.id == heroIdToExclude)
					continue;

				clientPeers.Add(hero.clientPeer);
			}

			return clientPeers;
		}

		public void GetClientPeers(List<ClientPeer> clientPeers, Guid heroIdToExclude)
		{
			foreach (Hero hero in m_heroes.Values)
			{
				if (hero.id == heroIdToExclude)
					continue;

				clientPeers.Add(hero.clientPeer);
			}
		}
	}
}
