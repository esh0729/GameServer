using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using ServerFramework;

namespace GameServer
{
	public class Resource
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const int kStartYRotationType_Fiexed = 1;
		public const int kStartYRotationType_Random = 2;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private int m_nStartContinentId = 0;
		private Vector3 m_startPosition = Vector3.zero;
		private float m_fStartRadius = 0f;
		private int m_nStartYRotationType = 0;
		private float m_fStartYRotation = 0f;

		private float m_fSectorCellSize = 0f;

		private int m_nHeroCreationLimitCount = 0;

		//
		// 대륙
		//

		private Dictionary<int, Continent> m_continents = new Dictionary<int, Continent>();

		//
		// 캐릭터
		//

		private Dictionary<int, Character> m_characters = new Dictionary<int, Character>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		private Resource()
		{

		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public int startContinentId
		{
			get { return m_nStartContinentId; }
		}

		public float sectorCellSize
		{
			get { return m_fSectorCellSize; }
		}

		public int heroCreationLimitCount
		{
			get { return m_nHeroCreationLimitCount; }
		}

		//
		// 대륙
		//

		public Dictionary<int, Continent> continents
		{
			get { return m_continents; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Init(SqlConnection conn)
		{
			if (conn == null)
				throw new ArgumentNullException("conn");

			LoadResource_GameConfig(conn);

			LoadResource_Place(conn);

			LoadResource_Character(conn);
		}

		private void LoadResource_GameConfig(SqlConnection conn)
		{
			DataRow drGameConfig = UserDBDoc.GameConfig(conn, null);
			if (drGameConfig == null)
			{
				LogUtil.Warn(GetType(), "게임설정이 존재하지 않습니다.");
				return;
			}

			m_nStartContinentId = Convert.ToInt32(drGameConfig["startContinentId"]);
			m_startPosition.x = Convert.ToSingle(drGameConfig["startXPosition"]);
			m_startPosition.y = Convert.ToSingle(drGameConfig["startYPosition"]);
			m_startPosition.z = Convert.ToSingle(drGameConfig["startZPosition"]);
			m_fStartRadius = Convert.ToSingle(drGameConfig["startRadius"]);
			m_nStartYRotationType = Convert.ToInt32(drGameConfig["startYRotationType"]);
			if (!IsDefinedStartYRotationType(m_nStartYRotationType))
				LogUtil.Warn(GetType(), "시작방향타입이 유효하지 않습니다. m_nStartYRotationType = " + m_nStartYRotationType);

			m_fStartYRotation = Convert.ToSingle(drGameConfig["startYRotation"]);

			m_fSectorCellSize = Convert.ToSingle(drGameConfig["sectorCellSize"]);

			m_nHeroCreationLimitCount = Convert.ToInt32(drGameConfig["heroCreationLimitCount"]);
		}

		private void LoadResource_Place(SqlConnection conn)
		{
			//
			// 대륙 목록
			//

			foreach (DataRow dr in UserDBDoc.Continents(conn, null))
			{
				Continent continent = new Continent();
				continent.Set(dr);

				m_continents.Add(continent.id, continent);
			}
		}

		private void LoadResource_Character(SqlConnection conn)
		{
			//
			// 캐릭터 목록
			//

			foreach (DataRow dr in UserDBDoc.Characters(conn, null))
			{
				Character character = new Character();
				character.Set(dr);

				m_characters.Add(character.id, character);
			}
		}

		//
		//
		//

		public Vector3 SelectStartPosition()
		{
			return new Vector3(
				SFRandom.NextFloat(m_startPosition.x - m_fStartRadius, m_startPosition.x + m_fStartRadius),
				m_startPosition.y,
				SFRandom.NextFloat(m_startPosition.z - m_fStartRadius, m_startPosition.z + m_fStartRadius));
		}

		public float SelectStartYRotation()
		{
			return m_nStartYRotationType == kStartYRotationType_Fiexed ? m_fStartYRotation : SFRandom.NextFloat(m_fStartYRotation);
		}

		//
		// 대륙
		//

		public Continent GetContinent(int nContinentId)
		{
			Continent value;

			return m_continents.TryGetValue(nContinentId, out value) ? value : null;
		}

		//
		// 캐릭터
		//

		public Character GetCharacter(int nCharacterId)
		{
			Character value;

			return m_characters.TryGetValue(nCharacterId, out value) ? value : null;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		private static Resource s_instance = new Resource();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static properties

		public static Resource instance
		{
			get { return s_instance; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static bool IsDefinedStartYRotationType(int nType)
		{
			return nType == kStartYRotationType_Fiexed
				|| nType == kStartYRotationType_Random;
		}
	}
}
