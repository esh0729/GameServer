using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ServerFramework
{
	//=====================================================================================================================
	// 데이터베이스 작업의 동시 처리 위해 대기 및 스레드 이벤트 신호를 처리하는 동기처리 클래스
	//=====================================================================================================================
	public class SFSync
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 작업 요청자 ID
		private object m_id = null;
		// 스레드 이벤트 신호
		private ManualResetEvent m_waitingSignal = new ManualResetEvent(true);

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 생성자
		//
		// id : 작업 요청자 ID
		//=====================================================================================================================
		public SFSync(object id)
		{
			if (id == null)
				throw new ArgumentNullException("id");

			m_id = id;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 작업 요청자 ID
		public object id
		{
			get { return m_id; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 스레드 대기 요청 함수
		//=====================================================================================================================
		public void Waiting()
		{
			// 이미 해당 ID로 작업을 진행 중일 경우 스레드 이벤트 신호차단 함수를 호출하기 때문에 해당 부분에서 대기
			m_waitingSignal.WaitOne();
		}

		//=====================================================================================================================
		// 스레드 이벤트 신호 받음 처리 함수
		//=====================================================================================================================
		public void Set()
		{
			m_waitingSignal.Set();
		}

		//=====================================================================================================================
		// 스레드 이벤트 신호 차단 처리 함수
		//=====================================================================================================================
		public void Reset()
		{
			m_waitingSignal.Reset();
		}
	}
}
