using System;
using System.Collections.Generic;
using System.Data;

using ServerFramework;

namespace GameServer
{
	//=====================================================================================================================
	// 사용자가 서버에 로그인하여 생성되는 사용자의 정보를 저장하고 있는 클래스
	//=====================================================================================================================
	public class Account
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 클라이언트 피어
		private ClientPeer m_clientPeer = null;
		// 계정 ID
		private Guid m_id = Guid.Empty;

		// 사용자 ID
		private Guid m_userId = Guid.Empty;
		// 계정생성시각
		private DateTimeOffset m_regTime = DateTimeOffset.MinValue;

		// 현재 로그인중인 영웅
		private Hero m_currentHero = null;

		// 계정 상태
		private AccountStatus m_status = AccountStatus.Logout;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 생성자
		//
		// clientPeer : 클라이언트 피어
		//=====================================================================================================================
		public Account(ClientPeer clientPeer)
		{
			if (clientPeer == null)
				throw new ArgumentNullException("clientPeer");

			m_clientPeer = clientPeer;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 클라이언트 피어
		public ClientPeer clientPeer
		{
			get { return m_clientPeer; }
		}

		// 계정 ID
		public Guid id
		{
			get { return m_id; }
		}

		// 사용자 ID
		public Guid userId
		{
			get { return m_userId; }
		}

		// 계정생성시각
		public DateTimeOffset regTime
		{
			get { return m_regTime; }
		}

		// 계정의 동기화를 위한 싱크 객체
		public object syncObject
		{
			get { return m_clientPeer.syncObject; }
		}

		// 현재 로그인중인 영웅
		public Hero currentHero
		{
			get { return m_currentHero; }
			set { m_currentHero = value; }
		}

		// 계정 상태
		public AccountStatus status
		{
			get { return m_status; }
		}

		// 계정의 로그인 여부
		public bool isLoggedIn
		{
			get { return m_status == AccountStatus.Login; }
		}

		// 영웅의 로그인 여부
		public bool isHeroLoggedIn
		{
			get { return m_currentHero != null; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 초기화 함수(새로 생성시)
		//
		// accountId : 계정 ID
		// userId : 유저 ID
		// time : 현재 시각
		//=====================================================================================================================
		public void Init(Guid accountId, Guid userId, DateTimeOffset time)
		{
			m_id = accountId;

			m_userId = userId;
			m_regTime = time;

			// 현재 상태를 로그인 상태로 변경
			m_status = AccountStatus.Login;
		}

		//=====================================================================================================================
		// 초기화 함수(기존에 데이터가 있을 경우)
		//
		// dr : 계정정보가 담긴 데이터베이스의 Row
		//=====================================================================================================================
		public void Init(DataRow dr)
		{
			if (dr == null)
				throw new ArgumentNullException("dr");

			m_id = DBUtil.ToGuid(dr["accountId"]);

			m_userId = DBUtil.ToGuid(dr["userId"]);
			m_regTime = DBUtil.ToDateTimeOffset(dr["regTime"]);

			// 현재 상태를 로그인 상태로 변경
			m_status = AccountStatus.Login;
		}

		//=====================================================================================================================
		// 로그아웃 함수
		//=====================================================================================================================
		public void Logout()
		{
			// 현재 로그인상태가 아닐경우 리턴
			if (!isLoggedIn)
				return;

			// 현재 상태를 로그아웃 상태로 변경
			m_status = AccountStatus.Logout;

			// 영웅이 로그인 상태일 경우 로그아웃 처리
			if (m_currentHero != null)
				m_currentHero.Logout();

			// 클라이언트 피어에 있는 계정정보를 삭제
			m_clientPeer.account = null;

			// 캐시의 계정 컬렉션에서 해당 ID의 계정 정보 삭제
			Cache.instance.RemoveAccount(m_id);
		}
	}
}
