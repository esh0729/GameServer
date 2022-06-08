using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFramework
{
	//=====================================================================================================================
	// SFSync 객체를 생성 및 관리하는 클래스
	//=====================================================================================================================
	public class SFSyncFactory
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		//
		// User
		//

		// 유저의 데이터베이스 처리에 대한 순서 처리 객체를 저장하는 컬렉션
		private static Dictionary<object, SFSync> m_sUserSyncFactories = new Dictionary<object, SFSync>();

		//
		// Hero
		//

		// 영웅의 데이터베이스 처리에 대한 순서 처리 객체를 저장하는 컬렉션
		private static Dictionary<object, SFSync> m_sHeroSyncFactories = new Dictionary<object, SFSync>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		//=====================================================================================================================
		// SFSync 객체를 호출하는 함수
		// Return : 내부에 저장 되어 있는 SFSync 객체
		//
		// type : 데이터베이스 작업의 타입
		// id : 작업 타입에 대한 id
		//=====================================================================================================================
		public static SFSync GetSync(SyncWorkType type, object id)
		{
			SFSync sync = null;

			// 작업 타입에 따른 객체 호출
			switch (type)
			{
				case SyncWorkType.User: sync = GetOrCreateUserSync(id); break;
				case SyncWorkType.Hero: sync = GetOrCreateHeroSync(id); break;

				default:
					throw new Exception("Not exist type.");
			}

			return sync;
		}

		//
		// User
		//

		//=====================================================================================================================
		// 유저 SFSync 객체를 생성 또는 호출하는 함수
		// Return : 생성 또는 내부에 저장 되어 있는 SFSync 객체
		//
		// id : 유저id
		//=====================================================================================================================
		private static SFSync GetOrCreateUserSync(object id)
		{
			SFSync sync = null;

			// 유저 SFSync 객체가 존재할 경우 컬렉션에서 호출, 없을 경우 새로 생성하여 컬렉션에 저장
			if (!m_sUserSyncFactories.TryGetValue(id, out sync))
			{
				sync = new SFSync(id);
				m_sUserSyncFactories.Add(id, sync);
			}

			return sync;
		}

		//
		// Hero
		//

		//=====================================================================================================================
		// 영웅 SFSync 객체를 생성 또는 호출하는 함수
		// Return : 생성 또는 내부에 저장 되어 있는 SFSync 객체
		//
		// id : 영웅id
		//=====================================================================================================================
		private static SFSync GetOrCreateHeroSync(object id)
		{
			SFSync sync = null;

			// 영웅 SFSync 객체가 존재할 경우 컬렉션에서 호출, 없을 경우 새로 생성하여 컬렉션에 저장
			if (!m_sHeroSyncFactories.TryGetValue(id, out sync))
			{
				sync = new SFSync(id);
				m_sHeroSyncFactories.Add(id, sync);
			}

			return sync;
		}
	}
}
