using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientCommon;

namespace GameServer
{
	//=====================================================================================================================
	// (CommandHandler 상속) 계정 로그인이 필요한 클라이언트 명령을 지원하는 추상 클래스
	//=====================================================================================================================
	public abstract class LoginRequiredCommandHandler<T1, T2> : CommandHandler<T1, T2>
		where T1 : CommandBody where T2 : ResponseBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 계정 객체
		protected Account m_myAccount = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 유효성 검사
		protected override bool isValid
		{
			get { return base.isValid && m_myAccount.isLoggedIn; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 계정 로그인 확인 후 작업 처리 함수를 호출하는 함수
		//=====================================================================================================================
		protected override void OnCommandHandle()
		{
			if (clientPeer.account == null)
				throw new CommandHandleException(kResult_Error, "로그인이 필요한 명령입니다.");

			m_myAccount = clientPeer.account;

			OnLoginRequiredCommandHandle();
		}

		//=====================================================================================================================
		// 계정 로그인이 필요한 클라이언트 명령 작업을 처리하는 함수
		//=====================================================================================================================
		protected abstract void OnLoginRequiredCommandHandle();

		//=====================================================================================================================
		// 에러가 발생한 클라이언트에 대한 정보를 처리하는 함수
		//
		// sb : 에러에 대한 출력 메세지를 저장하는 객체
		//=====================================================================================================================
		protected override void ErrorFrom(StringBuilder sb)
		{
			base.ErrorFrom(sb);

			// 클라이언트 계정 ID 추가
			sb.Append("# AccoutId : ");
			sb.Append(m_myAccount.id);
			sb.AppendLine();
		}
	}
}
