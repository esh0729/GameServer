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

			float fWidth = rect.xSize - rect.x;
			float fHeight = rect.zSize - rect.z;

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

			//
			//
			//

			InitPlace();
		}

		public bool ContainsPosition(Vector3 position)
		{
			return rect.Contains(position);
		}

		//
		//
		//

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

		public List<Hero> GetInterestedHeroes(Sector standardSector, Guid heroIdToExclude)
		{
			List<Hero> heroes = new List<Hero>();

			foreach (Sector sector in GetInterestedSector(standardSector))
			{
				foreach (Hero hero in sector.GetHeroes(heroIdToExclude))
				{
					heroes.Add(hero);
				}
			}

			return heroes;
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

		//
		//
		//

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

		protected override void OnHeroExit(Hero hero, bool bIsLogout, EntranceParam entranceParam)
		{
			base.OnHeroExit(hero, bIsLogout, entranceParam);

			//
			// 이동 종료
			//

			hero.EndMove();

			//
			// 행동 종료
			//

			hero.EndAction();

			//
			// 이벤트 전송
			//

			ServerEvent.SendHeroExit(GetInterestedClientPeers(hero.sector, hero.id), hero.id);

			//
			//
			//

			hero.SetCurrentPlace(null);
			hero.SetSector(null);
			hero.SetEntranceParam(entranceParam);
		}

		public InterestedAreaInfo MoveHero(Hero hero, Vector3 position, float fYRotation, bool bSendInterestedChangedEvent)
		{
			if (hero == null)
				throw new ArgumentNullException("hero");

			InterestedAreaInfo info = ChangeHeroPosition(hero, position, fYRotation, bSendInterestedChangedEvent);

			//
			// 이벤트 전송
			//

			ServerEvent.SendHeroMove(info.GetNotChangedSectorClientPeers(hero.id), hero.id, hero.position, hero.yRotation);

			return info;
		}

		protected InterestedAreaInfo ChangeHeroPosition(Hero hero, Vector3 position, float fYRotation, bool bSendInterestedChangedEvent)
		{
			Sector oldSector = hero.sector;
			Sector newSector = GetSector(position);

			InterestedAreaInfo info = GetInterestedAreaInfo(oldSector, newSector);

			hero.SetPosition(position, fYRotation);

			if (oldSector != newSector)
			{
				hero.SetSector(newSector);

				//
				// 이벤트 전송(관심영역변경)
				//

				if (bSendInterestedChangedEvent)
				{
					ServerEvent.SendInterestedAreaChanged(hero.clientPeer,
						Hero.ToPDHeroes(info.GetAddedSectorHeroes(hero.id)).ToArray(), info.GetRemovedSectorHeroIds(hero.id).ToArray());
				}

				//
				// 이벤트 전송(추가 관심영역)
				//

				ServerEvent.SendHeroInterestedAreaEnter(info.GetAddedSectorClientPeers(hero.id), hero.ToPDHero());

				//
				// 이벤트 전송(삭제 관심영역)
				//

				ServerEvent.SendHeroInterestedAreaExit(info.GetRemovedSectorClientPeers(hero.id), hero.id);
			}

			//
			//
			//

			return info;
		}

		private InterestedAreaInfo GetInterestedAreaInfo(Sector oldSector, Sector newSector)
		{
			InterestedAreaInfo info = new InterestedAreaInfo();

			if (oldSector == newSector)
			{
				foreach (Sector sector in GetInterestedSector(oldSector))
				{
					info.AddNotChangedSector(sector);
				}
			}
			else
			{
				List<Sector> oldSectorInterestedSectors = GetInterestedSector(oldSector);
				List<Sector> newSectorInterestedSectors = GetInterestedSector(newSector);

				//
				// NotChangedSector
				//

				foreach (Sector oldSectorInterestedSector in oldSectorInterestedSectors)
				{
					foreach (Sector newSectorInterestedSector in newSectorInterestedSectors)
					{
						if (oldSectorInterestedSector == newSectorInterestedSector)
						{
							info.AddNotChangedSector(oldSectorInterestedSector);
							break;
						}
					}
				}

				//
				// AddedSector
				//

				foreach (Sector newSectorInterestedSector in newSectorInterestedSectors)
				{
					if (info.ContainsNotChangedSector(newSectorInterestedSector))
						continue;

					info.AddAddedSector(newSectorInterestedSector);
				}

				//
				// RemovedSector
				//

				foreach (Sector oldSectorInterestedSector in oldSectorInterestedSectors)
				{
					if (info.ContainsNotChangedSector(oldSectorInterestedSector))
						continue;

					info.AddRemovedSector(oldSectorInterestedSector);
				}
			}

			return info;
		}
	}
}
