using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;

using ClientCommon;
using ServerFramework;

namespace GameServer
{
	public class Hero : Unit
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const short kUpdateTimeTicks = 500;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Account m_account = null;
		private Guid m_id = Guid.Empty;
		private string m_sName = null;
		private Character m_character = null;

		private SFWorker m_worker = null;
		private Timer m_timer = null;

		private DateTimeOffset m_currentUpdateTime = DateTimeOffset.MinValue;
		private DateTimeOffset m_prevUpdateTime = DateTimeOffset.MinValue;

		private DateTimeOffset m_regTime = DateTimeOffset.MinValue;

		private HeroStatus m_status = HeroStatus.Logout;

		private DateTimeOffset m_lastLoginTime = DateTimeOffset.MinValue;
		private DateTimeOffset m_lastLogoutTime = DateTimeOffset.MinValue;

		private Continent m_lastContinent = null;
		private Vector3 m_lastPosition = Vector3.zero;
		private float m_fLastYRotation = 0f;

		private Continent m_previousContinent = null;
		private Vector3 m_previousPosition = Vector3.zero;
		private float m_fPreviousYRotation = 0f;

		//
		// 장소입장
		//

		private EntranceParam m_entranceParam = null;
		private bool m_bIsInitEntered = false;

		//
		// 이동
		//

		private bool m_bMoving = false;

		//
		// 행동
		//

		private CharacterAction m_currentAction = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public Hero(Account account)
		{
			if (account == null)
				throw new ArgumentNullException("account");

			m_account = account;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public Account account
		{
			get { return m_account; }
		}

		public ClientPeer clientPeer
		{
			get { return m_account.clientPeer; }
		}

		public object syncObject
		{
			get { return m_account.syncObject; }
		}

		public Guid id
		{ 
			get { return m_id; }
		}

		public string name
		{
			get { return m_sName; }
		}

		public Character character
		{
			get { return m_character; }
		}

		public DateTimeOffset lastLoginTime
		{
			get { return m_lastLoginTime; }
		}

		public DateTimeOffset lastLogoutTime
		{
			get { return m_lastLogoutTime; }
		}

		public DateTimeOffset regTime
		{
			get { return m_regTime; }
		}

		public bool isLoggedIn
		{
			get { return m_status == HeroStatus.Login; }
		}

		public Continent lastContinent
		{
			get { return m_lastContinent; }
		}

		public int lastContinentId
		{
			get { return m_lastContinent != null ? m_lastContinent.id : 0; }
		}

		public Vector3 lastPosition
		{
			get { return m_lastPosition; }
		}

		public float lastYRotation
		{
			get { return m_fLastYRotation; }
		}

		public Continent previousContinent
		{
			get { return m_previousContinent; }
		}

		public int previousContinentId
		{
			get { return m_previousContinent != null ? m_previousContinent.id : 0; }
		}

		public Vector3 previousPosition
		{
			get { return m_previousPosition; }
		}

		public float previousYRotation
		{
			get { return m_fPreviousYRotation; }
		}

		//
		// 입장관련
		//

		public EntranceParam entranceParam
		{
			get { return m_entranceParam; }
		}

		public bool isInitEntered
		{
			get { return m_bIsInitEntered; }
			set { m_bIsInitEntered = value; }
		}

		//
		// 이동
		//

		public bool moving
		{
			get { return m_bMoving; }
		}

		//
		// 행동
		//

		public CharacterAction currentAction
		{
			get { return m_currentAction; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void CompleteLogin(
			DateTimeOffset time,

			DataRow drHero)
		{
			if (drHero == null)
				throw new ArgumentNullException("dr");

			m_worker = new SFWorker();
			m_worker.Start();

			//
			//
			//

			CompleteLogin_Base(time, drHero);

			//
			//
			//

			m_account.currentHero = this;
			m_timer = new Timer(Update, null, kUpdateTimeTicks, kUpdateTimeTicks);
			m_status = HeroStatus.Login;
		}

		private void CompleteLogin_Base(DateTimeOffset time, DataRow drHero)
		{
			m_id = DBUtil.ToGuid(drHero["heroId"]);
			m_sName = Convert.ToString(drHero["name"]);

			int nCharacterId = Convert.ToInt32(drHero["characterId"]);
			m_character = Resource.instance.GetCharacter(nCharacterId);
			if (m_character == null)
				throw new Exception("캐릭터가 존재하지 않습니다. nCharacterId = " + nCharacterId);

			m_lastLoginTime = time;
			m_lastLogoutTime = DBUtil.ToDateTimeOffset(drHero["lastLogoutTime"]);

			m_regTime = DBUtil.ToDateTimeOffset(drHero["regTime"]);

			int nLastContinentId = Convert.ToInt32(drHero["lastContinentId"]);
			m_lastContinent = Resource.instance.GetContinent(nLastContinentId);
			m_lastPosition.x = Convert.ToSingle(drHero["lastXPosition"]);
			m_lastPosition.y = Convert.ToSingle(drHero["lastYPosition"]);
			m_lastPosition.z = Convert.ToSingle(drHero["lastZPosition"]);
			m_fLastYRotation = Convert.ToSingle(drHero["lastYRotation"]);

			int nPreviousContinentId = Convert.ToInt32(drHero["previousContinentId"]);
			m_previousContinent = Resource.instance.GetContinent(nPreviousContinentId);
			m_previousPosition.x = Convert.ToSingle(drHero["previousXPosition"]);
			m_previousPosition.y = Convert.ToSingle(drHero["previousYPosition"]);
			m_previousPosition.z = Convert.ToSingle(drHero["previousZPosition"]);
			m_fPreviousYRotation = Convert.ToSingle(drHero["previousYRotation"]);

			Continent enterContinent = null;
			Vector3 enterPosition = Vector3.zero;
			float fEnterYRotation = 0f;

			if (m_lastContinent != null)
			{
				enterContinent = m_lastContinent;
				enterPosition = m_lastPosition;
				fEnterYRotation = m_fLastYRotation;
			}
			else
			{
				if (m_previousContinent != null)
				{
					enterContinent = m_previousContinent;
					enterPosition = m_previousPosition;
					fEnterYRotation = m_fPreviousYRotation;
				}
				else
				{
					enterContinent = Resource.instance.GetContinent(Resource.instance.startContinentId);
					enterPosition = Resource.instance.SelectStartPosition();
					fEnterYRotation = Resource.instance.SelectStartYRotation();
				}
			}

			HeroInitEnterParam param = new HeroInitEnterParam(enterContinent, enterPosition, fEnterYRotation);
			SetEntranceParam(param);
		}

		private void Update(object state)
		{
			AddWork(new SFAction(OnUpdate), false);
		}

		private void OnUpdate()
		{
			if (!isLoggedIn)
				return;

			try
			{
				OnUpdateInternal();
			}
			catch (Exception ex)
			{
				LogUtil.Error(GetType(), ex);
			}
		}

		private void OnUpdateInternal()
		{
			m_prevUpdateTime = m_currentUpdateTime;
			m_currentUpdateTime = DateTimeUtil.currentTime;

			if (m_prevUpdateTime == DateTimeOffset.MinValue)
				m_prevUpdateTime = m_currentUpdateTime;
		}

		//
		//
		//

		public void AddWork(ISFWork work, bool bRequiredGlobalLock)
		{
			m_worker.Add(new SFAction<ISFWork, bool>(RunWork, work, bRequiredGlobalLock));
		}

		private void RunWork(ISFWork work, bool bRequiredGlobalLock)
		{
			HeroSynchronizer heroSynchronizer = new HeroSynchronizer(this, work, bRequiredGlobalLock);
			heroSynchronizer.Start();
		}

		//
		//
		//

		public void SetLastLocation()
		{
			ContinentInstance continentInstance = m_currentPlace as ContinentInstance;

			if (continentInstance != null)
			{
				m_lastContinent = continentInstance.continent;
				m_lastPosition = m_position;
				m_fLastYRotation = m_fYRotation;
			}
			else
			{
				m_lastContinent = null;
				m_lastPosition = Vector3.zero;
				m_fLastYRotation = 0f;
			}
		}

		public void SetPreviousContinent()
		{
			ContinentInstance continentInstance = m_currentPlace as ContinentInstance;

			if (continentInstance == null)
				return;

			m_previousContinent = continentInstance.continent;
			m_previousPosition = m_position;
			m_fPreviousYRotation = m_fYRotation;
		}

		public void SetEntranceParam(EntranceParam param)
		{
			m_entranceParam = param;
		}

		//
		//
		//

		protected override void OnSetSector(Sector oldSector)
		{
			base.OnSetSector(oldSector);

			if (oldSector != null)
				oldSector.RemoveHero(m_id);

			if (m_sector != null)
				m_sector.AddHero(this);
		}

		//
		// 이동
		//

		public void StartMove()
		{
			if (m_bMoving)
				return;

			m_bMoving = true;
		}

		public void EndMove()
		{
			if (!m_bMoving)
				return;

			m_bMoving = false;
		}

		//
		// 행동
		//

		public void StartAction(CharacterAction action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			m_currentAction = action;

			//
			// 이벤트 전송
			//

			ServerEvent.SendHeroActionStarted(m_currentPlace.GetInterestedClientPeers(m_sector, m_id), m_id, m_currentAction.id, m_position, m_fYRotation);
		}

		public void EndAction()
		{
			m_currentAction = null;
		}

		//
		//
		//

		public void Logout()
		{
			if (!isLoggedIn)
				return;

			m_status = HeroStatus.Logout;

			m_timer.Dispose();

			{
				m_lastLogoutTime = DateTimeUtil.currentTime;

				//
				// 마지막위치정보
				//

				if (m_bIsInitEntered)
					SetLastLocation();

				//
				// 장소
				//

				if (m_currentPlace != null)
					m_currentPlace.Exit(this, true, null);

				//
				// DB 저장
				//

				Logout_SaveToDB();
			}

			//
			//
			//

			m_account.currentHero = null;
			Cache.instance.RemoveHero(m_id);

			AddWork(new SFAction(m_worker.Stop), false);
		}

		private void Logout_SaveToDB()
		{
			SFSqlWork dbWork = SqlWorkUtil.CreateHeroGameDBWork(m_id);

			//
			// 영웅 로그아웃
			//

			dbWork.AddCommand(GameDBDocEx.CSC_HeroLogout(this));

			//
			//
			//

			dbWork.Schedule();
		}

		//
		//
		//

		public PDHero ToPDHero()
		{
			PDHero inst = new PDHero();
			inst.heroId = m_id;
			inst.name = m_sName;
			inst.characterId = m_character.id;
			inst.position = m_position;
			inst.yRotation = m_fYRotation;
			inst.actionId = m_currentAction != null ? m_currentAction.id : 0;

			return inst;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static List<PDHero> ToPDHeroes(IEnumerable<Hero> heroes)
		{
			List<PDHero> results = new List<PDHero>();

			foreach (Hero hero in heroes)
			{
				lock (hero.syncObject)
				{
					results.Add(hero.ToPDHero());
				}
			}

			return results;
		}
	}
}
