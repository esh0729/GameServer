using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ServerFramework;

namespace GameServer
{
	//=====================================================================================================================
	// 생성된 시스템관련 객체를 관리하는 클래스
	//=====================================================================================================================
	public class Cache
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		// 업데이트 발생 간격
		public const short kUpdateTimeTicks = 500;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// Cache 데이터 접근을 위한 싱크객체
		private object m_syncObject = new object();

		// Cache 작업자
		private SFWorker m_worker = null;
		// 업데이트 타이머
		private Timer m_timer = null;

		// 현재 업데이트 시각
		private DateTimeOffset m_currentUpdateTime = DateTimeOffset.MinValue;
		// 이전 업데이트 시각
		private DateTimeOffset m_prevUpdateTime = DateTimeOffset.MinValue;

		//
		// 계정
		//

		// 접속중인 계정 목록
		private Dictionary<Guid, Account> m_accounts = new Dictionary<Guid, Account>();

		//
		// 영웅
		//

		// 접속중인 영웅 목록
		private Dictionary<Guid, Hero> m_heroes = new Dictionary<Guid, Hero>();

		//
		// 장소
		//

		// 현재 생성된 장소 목록
		private Dictionary<Guid, Place> m_places = new Dictionary<Guid, Place>();

		//
		// 대륙
		//

		// 현재 생성된 대륙인스턴스 목록
		private Dictionary<int, ContinentInstance> m_continentInstances = new Dictionary<int, ContinentInstance>();

		//
		//
		//

		// 리소스해제 여부
		private bool m_disposed = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 싱글톤 구현을 위한 private 생성자
		//=====================================================================================================================
		private Cache()
		{

		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// Cache 데이터 접근을 위한 싱크객체
		public object syncObject
		{
			get { return m_syncObject; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 초기화 함수
		//=====================================================================================================================
		public void Init()
		{
			LogUtil.System(GetType(), "Cache Init Started.");

			//
			//
			//

			// 작업자 생성 및 작업 시작
			m_worker = new SFWorker();
			m_worker.Start();

			// 업데이트 타이머 시작
			m_timer = new Timer(Update, null, kUpdateTimeTicks, kUpdateTimeTicks);

			//
			// 대륙
			//

			// 대륙 초기화 함수 호출
			InitContinent();

			//
			//
			//

			LogUtil.System(GetType(), "Cache Init Completed.");
		}

		//=====================================================================================================================
		// 대륙인스턴스 객체를 생성하는 함수
		//=====================================================================================================================
		private void InitContinent()
		{
			// 리소스 데이터중 대륙 목록을 모두 돌면서 대륙인스턴스 객체 생성
			foreach (Continent continent in Resource.instance.continents.Values)
			{
				// 대륙  생성
				ContinentInstance continentInstance = new ContinentInstance();
				
				// 대륙인스턴스 객체 접근을 위한 싱크객체 lock 처리
				lock (continentInstance.syncObject)
				{
					// 대륙인스턴스 객체 초기화
					continentInstance.Init(continent);

					// 장소 컬렉션에 추가
					AddPlace(continentInstance);
				}
			}
		}

		//=====================================================================================================================
		// 업데이트 호출 함수
		//
		// state : Timer에서 호출되기 위한 상태 변수
		//=====================================================================================================================
		private void Update(object state)
		{
			// 작업자에 업데이트를 처리하는 함수를 큐잉
			AddWork(new SFAction(Onupdate));
		}

		//=====================================================================================================================
		// 업데이트 함수
		//=====================================================================================================================
		private void Onupdate()
		{
			// 에러 처리를 하기 위해 실제 업데이트를 처리하는 함수를 만들어 try - catch 문으로 처리
			try
			{
				OnUpdateInternal();
			}
			catch (Exception ex)
			{
				LogUtil.Error(GetType(), ex);
			}
		}

		//=====================================================================================================================
		// 실제 업데이트 함수
		//=====================================================================================================================
		private void OnUpdateInternal()
		{
			// 이전 업데이트 시각 설정
			m_prevUpdateTime = m_currentUpdateTime;
			// 현재 업데이트 시각 설정
			m_currentUpdateTime = DateTimeUtil.currentTime;

			// 첫 업데이트 함수 호출일 경우
			if (m_prevUpdateTime == DateTimeOffset.MinValue)
				m_prevUpdateTime = m_currentUpdateTime;
		}

		//
		//
		//

		//=====================================================================================================================
		// Cache 작업 추가 함수
		//
		// work : Cache 작업자에 큐잉할 작업
		//=====================================================================================================================
		public void AddWork(ISFWork work)
		{
			// 들어온 작업을 순서대로 처리하기 위해 들어온 작업을 RunWork의 매개변수로 전달하는 대리자 객체를 생성후 작업자에 큐잉
			m_worker.Add(new SFAction<ISFWork>(RunWork, work));
		}

		//=====================================================================================================================
		// Cache 작업을 실행하는 함수
		//
		// work : 처리할 작업
		//=====================================================================================================================
		private void RunWork(ISFWork work)
		{
			// Cache 데이터 접근을 위하 싱크객체 lock 처리
			lock (m_syncObject)
			{
				// 작업 실행
				work.Run();
			}
		}

		//
		// 장소
		//

		//=====================================================================================================================
		// 장소 객체 저장 함수
		//
		// place : 생성된 장소 객체
		//=====================================================================================================================
		public void AddPlace(Place place)
		{
			// 장소 객체 저장
			m_places.Add(place.instanceId, place);

			// 장소 객체 타입에 따른 추가 저장
			switch (place.type)
			{
				// 장소 객체 대륙 타입일 경우 대륙인스턴스 객체 컬렉션에 저장
				case PlaceType.Continent: AddContinentInstance((ContinentInstance)place); break;
			}
		}

		//=====================================================================================================================
		// 장소 객체 삭제 함수
		//
		// place : 삭제할 장소 객체
		//=====================================================================================================================
		public void RemovePlace(Place place)
		{
			// 장소 객체 삭제
			m_places.Remove(place.instanceId);

			// 장소 객체 타입에 따른 추가 삭제
			switch (place.type)
			{
				// 장소 객체 대륙 타입일 경우 대륙인스턴스 객체 컬렉션에서 삭제
				case PlaceType.Continent: RemoveContinentInstance(((ContinentInstance)place).continent.id); break;
			}
		}

		//
		// 대륙
		//

		//=====================================================================================================================
		// 대륙인스턴스 객체를 컬렉션에 저장하는 함수
		//
		// continentInstance : 저장할 대륙인스턴스 객체
		//=====================================================================================================================
		private void AddContinentInstance(ContinentInstance continentInstance)
		{
			m_continentInstances.Add(continentInstance.continent.id, continentInstance);
		}

		//=====================================================================================================================
		// 대륙인스턴스 객체를 컬렉션에서 삭제하는 함수
		//
		// nContinentId : 삭제할 대륙 ID
		//=====================================================================================================================
		private void RemoveContinentInstance(int nContinentId)
		{
			m_continentInstances.Remove(nContinentId);
		}

		//=====================================================================================================================
		// 대륙인스턴스 객체를 컬렉션에서 호출하는 함수
		// Return : 매개변수와 매칭되는 대륙인스턴스 객체, 없을 경우 null 반환
		//
		// nContinentId : 호출할 대륙 ID
		//=====================================================================================================================
		public ContinentInstance GetContinentInstance(int nContinentId)
		{
			ContinentInstance value;

			return m_continentInstances.TryGetValue(nContinentId, out value) ? value : null;
		}

		//
		// 계정
		//

		//=====================================================================================================================
		// 계정 객체를 컬렉션에 저장하는 함수
		//
		// account : 저장할 계정 객체
		//=====================================================================================================================
		public void AddAccount(Account account)
		{
			if (account == null)
				throw new ArgumentNullException("account");

			m_accounts.Add(account.id, account);
		}

		//=====================================================================================================================
		// 계정 객체를 컬렉션에서 호출하는 함수
		// Return : 매개변수와 매칭되는 계정 객체, 없을 경우 null 반환
		//
		// accountId : 호출할 계정 ID
		//=====================================================================================================================
		public Account GetAccount(Guid accountId)
		{
			Account value;

			return m_accounts.TryGetValue(accountId, out value) ? value : null;
		}

		//=====================================================================================================================
		// 계정 객체를 컬렉션에서 삭제하는 함수
		//
		// accountId : 삭제할 계정 ID
		//=====================================================================================================================
		public void RemoveAccount(Guid accountId)
		{
			m_accounts.Remove(accountId);
		}

		//
		// 영웅
		//

		//=====================================================================================================================
		// 영웅 객체를 컬렉션에 저장하는 함수
		//
		// hero : 저장할 영웅 객체
		//=====================================================================================================================
		public void AddHero(Hero hero)
		{
			if (hero == null)
				throw new ArgumentNullException("hero");

			m_heroes.Add(hero.id, hero);
		}

		//=====================================================================================================================
		// 영웅 객체를 컬렉션에서 호출하는 함수
		// Return : 매개변수와 매칭되는 영웅 객체, 없을 경우 null 반환
		//
		// heroId : 호출할 영웅 ID
		//=====================================================================================================================
		public Hero GetHero(Guid heroId)
		{
			Hero value;

			return m_heroes.TryGetValue(heroId, out value) ? value : null;
		}

		//=====================================================================================================================
		// 영웅 객체를 컬렉션에서 삭제하는 함수
		//
		// heroId : 삭제할 영웅 ID
		//=====================================================================================================================
		public void RemoveHero(Guid heroId)
		{
			m_heroes.Remove(heroId);
		}

		//
		//
		//

		//=====================================================================================================================
		// 리소스해제 함수
		//=====================================================================================================================
		public void Dispose()
		{
			// 이미 리소스해제를 했을 경우 리턴
			if (m_disposed)
				return;

			m_disposed = true;

			//
			//
			//

			// 업데이트 타이머 리소스해제
			m_timer.Dispose();

			// 계정 로그아웃 처리
			foreach (Account account in m_accounts.Values.ToArray())
			{
				AccountSynchronizer accountSynchronizer = new AccountSynchronizer(account, new SFAction(account.Logout), false);
				accountSynchronizer.Start();
			}

			// 장소 리소스 해제 처리
			foreach (Place place in m_places.Values.ToArray())
			{
				place.Dispose();
			}

			//
			//
			//

			// 작업자 종료 처리
			m_worker.Stop();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		// 싱글톤 객체 인스턴스
		private static Cache s_instance = new Cache();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static properties

		// 싱클톤 객체 인스턴스
		public static Cache instance
		{
			get { return s_instance; }
		}
	}
}
