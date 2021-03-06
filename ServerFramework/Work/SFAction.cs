using System;

namespace ServerFramework
{
	//=====================================================================================================================
	// (ISFWork 상속) 특정 시점에 실행을 위한 매개변수를 가지고 있지 않은 함수 대리자를 가지고 있는 클래스
	//=====================================================================================================================
	public class SFAction : ISFWork
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 매개변수를 가지고 있지 않는 함수의 대리자
		private Action m_action = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 생성자
		//
		// action : 매개변수를 가지고 있지 않는 함수의 대리자
		//=====================================================================================================================
		public SFAction(Action action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			m_action = action;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 함수 대리자 실행 함수
		//=====================================================================================================================
		public void Run()
		{
			// 함수 대리자를 통한 함수 실행
			m_action();
		}
	}

	//=====================================================================================================================
	// (ISFWork 상속) 특정 시점에 실행을 위한 매개변수를 1개 가지고 있는 함수 대리자를 가지고 있는 클래스
	//=====================================================================================================================
	public class SFAction<T1> : ISFWork
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 매개변수를 1개 가지고 있는 함수 대리자
		private Action<T1> m_action = null;
		// 첫번째 매개 변수
		private T1 m_arg1 = default(T1);

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 생성자
		//
		// action : 매개변수를 1개 가지고 있는 함수의 대리자
		// arg1 : 첫번째 매개변수
		//=====================================================================================================================
		public SFAction(Action<T1> action, T1 arg1)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			m_action = action;
			m_arg1 = arg1;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 함수 대리자 실행 함수
		//=====================================================================================================================
		public void Run()
		{
			// 함수 대리자를 통한 함수 실행
			m_action(m_arg1);
		}
	}

	//=====================================================================================================================
	// (ISFWork 상속) 특정 시점에 실행을 위한 매개변수를 2개 가지고 있는 함수 대리자를 가지고 있는 클래스
	//=====================================================================================================================
	public class SFAction<T1, T2> : ISFWork
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 매개변수를 2개 가지고 있는 함수 대리자
		private Action<T1, T2> m_action = null;
		// 첫번째 매개 변수
		private T1 m_arg1 = default(T1);
		// 두번째 매개 변수
		private T2 m_arg2 = default(T2);

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 생성자
		//
		// action : 매개변수를 2개 가지고 있는 함수의 대리자
		// arg1 : 첫번째 매개변수
		// arg2 : 두번째 매개변수
		//=====================================================================================================================
		public SFAction(Action<T1, T2> action, T1 arg1, T2 arg2)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			m_action = action;
			m_arg1 = arg1;
			m_arg2 = arg2;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 함수 대리자 실행 함수
		//=====================================================================================================================
		public void Run()
		{
			// 함수 대리자를 통한 함수 실행
			m_action(m_arg1, m_arg2);
		}
	}

	//=====================================================================================================================
	// (ISFWork 상속) 특정 시점에 실행을 위한 매개변수를 3개 가지고 있는 함수 대리자를 가지고 있는 클래스
	//=====================================================================================================================
	public class SFAction<T1, T2, T3> : ISFWork
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 매개변수를 3개 가지고 있는 함수 대리자
		private Action<T1, T2, T3> m_action = null;
		// 첫번째 매개 변수
		private T1 m_arg1 = default(T1);
		// 두번째 매개 변수
		private T2 m_arg2 = default(T2);
		// 세번째 매개 변수
		private T3 m_arg3 = default(T3);

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 생성자
		//
		// action : 매개변수를 3개 가지고 있는 함수의 대리자
		// arg1 : 첫번째 매개변수
		// arg2 : 두번째 매개변수
		// arg3 : 세번째 매개변수
		//=====================================================================================================================
		public SFAction(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			m_action = action;
			m_arg1 = arg1;
			m_arg2 = arg2;
			m_arg3 = arg3;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 함수 대리자 실행 함수
		//=====================================================================================================================
		public void Run()
		{
			// 함수 대리자를 통한 함수 실행
			m_action(m_arg1, m_arg2, m_arg3);
		}
	}
}
