using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
	public class InterestedAreaInfo
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private HashSet<Sector> m_notChangedSectors = new HashSet<Sector>();
		private HashSet<Sector> m_addedSectors = new HashSet<Sector>();
		private HashSet<Sector> m_removedSectors = new HashSet<Sector>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public HashSet<Sector> notChangedSectors
		{
			get { return m_notChangedSectors; }
		}

		public HashSet<Sector> addedSectors
		{
			get { return m_addedSectors; }
		}

		public HashSet<Sector> removedSectors
		{
			get { return m_removedSectors; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void AddNotChangedSector(Sector sector)
		{
			if (sector == null)
				throw new ArgumentNullException("sector");

			m_notChangedSectors.Add(sector);
		}

		public bool ContainsNotChangedSector(Sector sector)
		{
			if (sector == null)
				return false;

			return m_notChangedSectors.Contains(sector);
		}

		public void AddAddedSector(Sector sector)
		{
			if (sector == null)
				throw new ArgumentNullException("sector");

			m_addedSectors.Add(sector);
		}

		public void AddRemovedSector(Sector sector)
		{
			if (sector == null)
				throw new ArgumentNullException("sector");

			m_removedSectors.Add(sector);
		}

		//
		//
		//

		public List<Hero> GetAddedSectorHeroes(Guid heroIdToExclude)
		{
			List<Hero> addedHeroes = new List<Hero>();

			foreach (Sector sector in m_addedSectors)
			{
				sector.GetHeroes(addedHeroes, heroIdToExclude);
			}

			return addedHeroes;
		}

		//
		//
		//

		public List<Guid> GetRemovedSectorHeroIds(Guid heroIdToExclude)
		{
			List<Guid> heroIds = new List<Guid>();

			foreach (Sector sector in m_removedSectors)
			{
				sector.GetHeroIds(heroIds, heroIdToExclude);
			}

			return heroIds;
		}

		//
		//
		//

		public List<ClientPeer> GetNotChangedSectorClientPeers(Guid heroIdToExclude)
		{
			List<ClientPeer> clientPeers = new List<ClientPeer>();

			foreach (Sector sector in m_notChangedSectors)
			{
				sector.GetClientPeers(clientPeers, heroIdToExclude);
			}

			return clientPeers;
		}

		public List<ClientPeer> GetAddedSectorClientPeers(Guid heroIdToExclude)
		{
			List<ClientPeer> clientPeers = new List<ClientPeer>();

			foreach (Sector sector in m_addedSectors)
			{
				sector.GetClientPeers(clientPeers, heroIdToExclude);
			}

			return clientPeers;
		}

		public List<ClientPeer> GetRemovedSectorClientPeers(Guid heroIdToExclude)
		{
			List<ClientPeer> clientPeers = new List<ClientPeer>();

			foreach (Sector sector in m_removedSectors)
			{
				sector.GetClientPeers(clientPeers, heroIdToExclude);
			}

			return clientPeers;
		}
	}
}
