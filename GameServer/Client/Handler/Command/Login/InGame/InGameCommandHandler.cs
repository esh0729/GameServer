using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClientCommon;

namespace GameServer
{
	//=====================================================================================================================
	// (LoginRequiredCommandHandler 상속) 영웅 로그인이 필요한 클라이언트 명령을 지원하는 추상 클래스
	//=====================================================================================================================
	public abstract class InGameCommandHandler<T1,T2> : LoginRequiredCommandHandler<T1,T2>
		where T1 : CommandBody where T2 : ResponseBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 영웅 객체
		protected Hero m_myHero = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 유효성 검사
		protected override bool isValid
		{
			get { return base.isValid && m_myHero.isLoggedIn; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 영웅 로그인 확인 후 작업 처리 함수를 호출하는 함수
		//=====================================================================================================================
		protected override void OnLoginRequiredCommandHandle()
		{
			if (m_myAccount.currentHero == null)
				throw new CommandHandleException(kResult_Error, "영웅로그인이 필요한 명령입니다.");

			m_myHero = m_myAccount.currentHero;

			OnInGameCommandHandle();
		}

		//=====================================================================================================================
		// 영웅 로그인이 필요한 클라이언트 명령 작업을 처리하는 함수
		//=====================================================================================================================
		protected abstract void OnInGameCommandHandle();

		//=====================================================================================================================
		// 에러가 발생한 클라이언트에 대한 정보를 처리하는 함수
		//
		// sb : 에러에 대한 출력 메세지를 저장하는 객체
		//=====================================================================================================================
		protected override void ErrorFrom(StringBuilder sb)
		{
			base.ErrorFrom(sb);

			// 클라이언트 영웅 ID 추가
			sb.Append("# HeroId : ");
			sb.Append(m_myHero.id);
			sb.AppendLine();
		}
	}
}
