using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
	public abstract class PhysicalPlace : Place
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		protected Sector[,] m_sectors = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public abstract Location location { get; }
		public abstract Rect3D rect { get; }

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected void InitPhysicalPlace()
		{
			float fSectorCellSize = Resource.instance.sectorCellSize;

			float fWidth = rect.x - rect.xSize;
			float fHeight = rect.z - rect.zSize;

			int nRowCount = (int)(fHeight / fSectorCellSize) + 1;
			int nColCount = (int)(fWidth / fSectorCellSize) + 1;

			m_sectors = new Sector[nRowCount, nColCount];
			for (int nRow = 0; nRow < nRowCount; nRow++)
			{
				for (int nCol = 0; nCol < nColCount; nCol++)
				{
					Sector sector = new Sector(this, nRow, nCol,
						new Vector3(rect.x + (nRow * fSectorCellSize), 0, rect.z + (nRow * fSectorCellSize)));

					m_sectors[nRow, nCol] = sector;
				}
			}
		}

		protected Sector GetSector(Vector3 position)
		{
			float fSectorCellSize = Resource.instance.sectorCellSize;

			float fX = position.x - rect.x;
			float fZ = position.z - rect.z;

			int nRow = (int)(fZ / fSectorCellSize);
			int nCol = (int)(fX / fSectorCellSize);

			return GetSector(nRow, nCol);
		}

		protected Sector GetSector(int nRow, int nCol)
		{
			if (nRow < 0 || nRow >= m_sectors.GetLength(0))
				return null;

			if (nCol < 0 || nCol >= m_sectors.GetLength(1))
				return null;

			return m_sectors[nRow, nCol];
		}

		protected List<Sector> GetInterestedSector(Vector3 position)
		{
			return GetInterestedSector(GetSector(position));
		}

		protected List<Sector> GetInterestedSector(Sector standardSector)
		{
			List<Sector> sectors = new List<Sector>();

			for (int nRow = standardSector.row - 1; nRow <= standardSector.row + 1; nRow++)
			{
				for (int nCol = standardSector.col - 1; nCol <= standardSector.col + 1; nCol++)
				{
					Sector sector = GetSector(nRow, nCol);

					if (sector == null)
						continue;

					sectors.Add(sector);
				}
			}

			return sectors;
		}

		public List<ClientPeer> GetInterestedClientPeers(Sector standardSector, Guid heroIdToExclude)
		{
			List<ClientPeer> clientPeers = new List<ClientPeer>();

			foreach (Sector sector in GetInterestedSector(standardSector))
			{
				sector.GetClientPeers(clientPeers, heroIdToExclude);
			}

			return clientPeers;
		}

		protected override void OnHeroEnter(Hero hero)
		{
			base.OnHeroEnter(hero);

			//
			//
			//

			hero.SetCurrentPlace(this);
			hero.SetSector(GetSector(hero.position));

			//
			// 이벤트 전송
			//

			ServerEvent.SendHeroEnter(GetInterestedClientPeers(hero.sector, hero.id), hero.ToPDHero());
		}

		protected override void OnHeroExit(Hero hero)
		{
			base.OnHeroExit(hero);

			//
			// 이벤트 전송
			//

			ServerEvent.SendHeroExit(GetInterestedClientPeers(hero.sector, hero.id), hero.id);

			//
			//
			//

			hero.SetCurrentPlace(null);
			hero.SetSector(null);
		}
	}
}
