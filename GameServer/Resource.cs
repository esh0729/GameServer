using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using ServerFramework;

namespace GameServer
{
	//=====================================================================================================================
	// 시스템에서 사용될 리소스 데이터를 호출 및 관리하는 클래스
	//=====================================================================================================================
	public class Resource
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		// 시작방향의 타입에 대한 상수
		public const int kStartYRotationType_Fiexed = 1;
		public const int kStartYRotationType_Random = 2;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 시작대륙ID
		private int m_nStartContinentId = 0;
		// 시작위치
		private Vector3 m_startPosition = Vector3.zero;
		// 시작반경
		private float m_fStartRadius = 0f;
		// 시작방향타입
		private int m_nStartYRotationType = 0;
		// 시작방향
		private float m_fStartYRotation = 0f;

		// 장소섹터 사이즈
		private float m_fSectorCellSize = 0f;

		// 영우생성제한수
		private int m_nHeroCreationLimitCount = 0;

		//
		// 대륙
		//

		// 대륙 컬렉션
		private Dictionary<int, Continent> m_continents = new Dictionary<int, Continent>();

		//
		// 캐릭터
		//

		// 캐릭터 컬렉션
		private Dictionary<int, Character> m_characters = new Dictionary<int, Character>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 싱글톤 객체를 위한 private 생성자
		//=====================================================================================================================
		private Resource()
		{

		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 시작대륙ID
		public int startContinentId
		{
			get { return m_nStartContinentId; }
		}

		// 장소섹터사이즈
		public float sectorCellSize
		{
			get { return m_fSectorCellSize; }
		}

		// 영웅생성제한수
		public int heroCreationLimitCount
		{
			get { return m_nHeroCreationLimitCount; }
		}

		//
		// 대륙
		//

		// 대륙 컬렉션
		public Dictionary<int, Continent> continents
		{
			get { return m_continents; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 초기화 함수
		//
		// conn : 데이터베이스 접근을 위한 연결 객체
		//=====================================================================================================================
		public void Init(SqlConnection conn)
		{
			if (conn == null)
				throw new ArgumentNullException("conn");

			// 게임 설정 데이터리소스 호출
			LoadResource_GameConfig(conn);

			// 장소 데이터리소스 호출
			LoadResource_Place(conn);

			// 캐릭터 데이터리소스 호출
			LoadResource_Character(conn);
		}

		//=====================================================================================================================
		// 게임 설정 데이터리소스 호출 함수
		//
		// conn : 데이터베이스 접근을 위한 연결 객체
		//=====================================================================================================================
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

		//=====================================================================================================================
		// 장소 데이터리소스 호출 함수
		//
		// conn : 데이터베이스 접근을 위한 연결 객체
		//=====================================================================================================================
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

		//=====================================================================================================================
		// 캐릭터 데이터리소스 호출 함수
		//
		// conn : 데이터베이스 접근을 위한 연결 객체
		//=====================================================================================================================
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

			//
			// 캐릭터행동 목록
			//

			foreach (DataRow dr in UserDBDoc.CharacterActions(conn, null))
			{
				int nCharacterId = Convert.ToInt32(dr["characterId"]);
				Character character = GetCharacter(nCharacterId);
				if (character == null)
				{
					LogUtil.Warn(GetType(), "[캐릭터행동 목록] 캐릭터가 존재하지 않습니다. nCharacterId = " + nCharacterId);
					continue;
				}

				CharacterAction action = new CharacterAction(character);
				action.Set(dr);

				character.AddAction(action);
			}
		}

		//
		//
		//

		//=====================================================================================================================
		// 랜덤 시작위치 생성 클래스
		// Return : 랜덤하게 생성된 위치 데이터
		//=====================================================================================================================
		public Vector3 SelectStartPosition()
		{
			return new Vector3(
				SFRandom.NextFloat(m_startPosition.x - m_fStartRadius, m_startPosition.x + m_fStartRadius),
				m_startPosition.y,
				SFRandom.NextFloat(m_startPosition.z - m_fStartRadius, m_startPosition.z + m_fStartRadius));
		}

		//=====================================================================================================================
		// 타입에 따른 시작방향 생성 클래스
		// Return : 타입에 맞는 랜덤 또는 고정 방향 데이터
		//=====================================================================================================================
		public float SelectStartYRotation()
		{
			return m_nStartYRotationType == kStartYRotationType_Fiexed ? m_fStartYRotation : SFRandom.NextFloat(m_fStartYRotation);
		}

		//
		// 대륙
		//

		//=====================================================================================================================
		// 대륙 데이터 호출 함수
		// Return : 해당 대륙ID에 맞는 대륙 ID 또는 null
		//=====================================================================================================================
		public Continent GetContinent(int nContinentId)
		{
			Continent value;

			return m_continents.TryGetValue(nContinentId, out value) ? value : null;
		}

		//
		// 캐릭터
		//

		//=====================================================================================================================
		// 캐릭터 데이터 호출 함수
		// Return : 해당 캐릭터ID에 맞는 대륙 ID 또는 null
		//=====================================================================================================================
		public Character GetCharacter(int nCharacterId)
		{
			Character value;

			return m_characters.TryGetValue(nCharacterId, out value) ? value : null;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		// 싱글톤 객체 인스턴스
		private static Resource s_instance = new Resource();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static properties

		// 싱글톤 객체 인스턴스
		public static Resource instance
		{
			get { return s_instance; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		//=====================================================================================================================
		// 시작방향타입의 유효성을 체크하는 함수
		// Return : 유효한 데이터의 경우 true, 유효하지 않은 데이터의 경우 false 반환
		//
		// nType : 시작방향타입에 대한 타입
		//=====================================================================================================================
		public static bool IsDefinedStartYRotationType(int nType)
		{
			return nType == kStartYRotationType_Fiexed
				|| nType == kStartYRotationType_Random;
		}
	}
}
