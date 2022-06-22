using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server;
using ClientCommon;
using ServerFramework;

namespace GameServer
{
	//=====================================================================================================================
	// (Handler 상속) 클라이언트 명령에 대한 작업의 초기화, 처리, 응답, 에러처리에 대한 함수를 제공하는 클래스
	//=====================================================================================================================
	public abstract class CommandHandler<T1, T2> : Handler
		where T1 : CommandBody where T2 : ResponseBody	
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 명령에 대한 타입
		private CommandName m_name = CommandName.None;
		// 명령의 고유 ID
		private long m_lnCommandId = 0;
		// 명령에 대한 필요 정보를 담고 있는 객체
		protected T1 m_body = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 명령에 대한 타입
		public CommandName name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		// 명령의 고유 ID
		public long commandId
		{
			get { return m_lnCommandId; }
			set { m_lnCommandId = value; }
		}

		// 명령에 대한 필요 정보를 담고 있는 객체
		public T1 body
		{
			get { return m_body; }
			set { m_body = value; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 필요 정보를 저장하거나 패킷의 역직렬화를 처리하는 초기화 함수
		//
		// nName : 명령 타입
		// lnCommandId : 명령의 고유 ID
		// packet : 인게임에 사용될 내부 패킷
		//=====================================================================================================================
		protected override void InitInternal(int nName, long lnCommandId, byte[] packet)
		{
			m_name = (CommandName)nName;
			m_lnCommandId = lnCommandId;
			m_body = (T1)Activator.CreateInstance(typeof(T1));

			// 받은 packet은 내부에서 직렬화 되있으므로 역직렬화
			m_body.DeserializeRaw(packet);
		}

		//=====================================================================================================================
		// 작업 시작 함수
		//=====================================================================================================================
		protected override void OnHandle()
		{
			try
			{
				// 세부 처리 함수 호출
				OnCommandHandle();
			}
			catch (CommandHandleException ex)
			{
				// 미리 정의한 클라이언트 명령 에러 발생시 에러로그등 등록한 후 클라이언트 응답 전송

				ErrorLog("Error " + ex.result + ", " + ex.Message, true, ex.StackTrace);

				SendResponse(ex.result, ex.Message);
			}
			catch (Exception ex)
			{
				// 미리 정의하지 않은 클라이언트 명령 에러 발생시 에러로그등 등록한 후 클라이언트 응답 전송

				ErrorLog("Error " + kResult_Error + ", " + ex.Message, true, ex.StackTrace);

				SendResponse(kResult_Error, ex.Message);
			}
		}

		//=====================================================================================================================
		// 클라이언트 명령의 세부적인 처리를 위한 추상 함수
		//=====================================================================================================================
		protected abstract void OnCommandHandle();

		//=====================================================================================================================
		// 비동기 독립 작업 등록 함수
		//
		// action : 비동기 독립 작업에 대한 대리자
		//=====================================================================================================================
		protected void RunnableStandaloneWork(Action action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			// 서버 객체에 있는 비동기 독립 작업자 등록 함수 호출
			Server.instance.AddStandalonWork(new SFAction<Action>(Runnable, action));
		}

		//=====================================================================================================================
		// 비동기 독립 작업을 실행 하는 함수
		//
		// action : 비동기 독립 작업에 대한 대리자
		//=====================================================================================================================
		private void Runnable(Action action)
		{
			try
			{
				// 핸들러 유효성 검사
				if (!isValid)
					throw new Exception("Invalid Handler.");

				// 작업 처리
				action();

				// 작업 처리 이후 처리할 작업 진행 함수 호출
				FinishWork(null);
			}
			catch (Exception ex)
			{
				// 에러 발생시 처리할 작업 진행 함수 호출
				FinishWork(ex);
			}
		}

		//=====================================================================================================================
		// 작업 처리 이후 처리할 작업을 진행하는 함수
		//
		// ex : 에러 발생시 전달하는 에러 객체, 에러가 발생하지 않았을 경우 null로 전달
		//=====================================================================================================================
		protected void FinishWork(Exception ex)
		{
			if (ex == null)
			{
				// 에러가 발생하지 않았을 경우

				// 비동기 독립 작업 처리 완료시 호출되는 함수 동기 처리 하여 호출
				ClientPeerSynchronizer synchronizer = new ClientPeerSynchronizer(clientPeer, new SFAction(OnWorkSuccess));

				if (globalLockRequired)
				{
					lock (Cache.instance.syncObject)
					{
						synchronizer.Start();
					}
				}
				else
					synchronizer.Start();
			}
			else
			{
				// 에러가 발생하였을 경우

				// 에러 로그 등록
				ErrorLog("Error " + kResult_Error + ", " + ex.Message, true, ex.StackTrace);

				// 클라이언트에 실패에 대한 응답 전송
				SendResponse(kResult_Error, ex.Message);

				// 실패시 처리할 함수 호출
				OnWorkFail(ex);
			}
		}

		//=====================================================================================================================
		// 비동기 독립 작업 처리 완료 이후 처리할 작업을 처리하는 함수
		//=====================================================================================================================
		protected virtual void OnWorkSuccess()
		{
			if (!isValid)
				throw new Exception("Invalid Handler.");
		}

		//=====================================================================================================================
		// 비동기 독립 작업 처리 실패 이후 처리할 작업을 처리하는 함수
		//
		// ex : 발생 에러
		//=====================================================================================================================
		protected virtual void OnWorkFail(Exception ex)
		{

		}

		//
		//
		//

		//=====================================================================================================================
		// 클라이언트 명령에 대한 응답을 피어 객체에 전달하는 함수
		//
		// response : 클라이언트 명령에 대한 응답
		//=====================================================================================================================
		private void SendResponse(OperationResponse response)
		{
			m_peer.SendResponse(response);
		}

		//=====================================================================================================================
		// 클라이언트 명령에 대한 실패 응답을 생성하고 전달하는 함수
		//
		// nResult : 결과 코드
		// sDebugMessage : 에러 메세지
		//=====================================================================================================================
		protected void SendResponse(short nResult, string sDebugMessage)
		{
			OperationResponse response = m_peer.CreateOperationResponse(nResult, m_peer.CreateCommandParameters((int)m_name, new byte[] { }, m_lnCommandId));
			response.debugMessage = sDebugMessage;

			SendResponse(response);
		}

		//=====================================================================================================================
		// 클라이언트 명령에 대한 응답을 생성하여 전달하는 함수
		//
		// nResult : 결과 코드
		// responseBody : 클라이언트에게 전달할 응답 데이터
		//=====================================================================================================================
		protected void SendResponse(short nResult, T2 responseBody)
		{
			SendResponse(m_peer.CreateOperationResponse(
				nResult, m_peer.CreateCommandParameters((int)m_name, responseBody != null ? responseBody.SerializeRaw() : new byte[] { }, m_lnCommandId)));
		}

		//=====================================================================================================================
		// 클라이언트 명령에 대한 성공 응답 데이터를 전달하는 함수
		//
		// responseBody : 클라이언트에게 전달할 응답 데이터
		//=====================================================================================================================
		protected void SendResponseOK(T2 responseBody)
		{
			SendResponse(kResult_OK, responseBody);
		}

		//
		//
		//

		//=====================================================================================================================
		// 핸들러 내에서 발생한 에러 메세지를 전달하는 함수
		//
		// sMessage : 발생한 에러 메세지
		//=====================================================================================================================
		protected void ErrorLog(string sMessage)
		{
			// 에러 경로에 대한 내용은 제외하고 메세지만을 전달하는 함수 호출
			ErrorLog(sMessage, false, null);
		}

		//=====================================================================================================================
		// 핸들러 내에서 발생한 에러 메세지와 에러 경로에 대한 처리 내용을 전달하는 함수
		//
		// sMessage : 발생한 에러 메세지
		// bLoggingTrace : 에러 경로 저장 여부
		// sStackTrace : 에러 발생 경로
		//=====================================================================================================================
		protected void ErrorLog(string sMessage, bool bLoggingTrace, string sStackTrace)
		{
			StringBuilder sb = new StringBuilder();

			// 클라이언트 정보 추가
			ErrorFrom(sb);

			// 에러 메세지를 출력하는 함수 호출
			LogUtil.Error(GetType(), sb, sMessage, bLoggingTrace, sStackTrace); 
		}

		//=====================================================================================================================
		// 에러가 발생한 클라이언트에 대한 정보를 처리하는 가상 함수(하위의 세부 정보는 오버라이딩하여 추가로 처리)
		//
		// sb : 에러에 대한 출력 메세지를 저장하는 객체
		//=====================================================================================================================
		protected virtual void ErrorFrom(StringBuilder sb)
		{
			// 클라이언트 피어ID 추가
			sb.Append("# PeerId : ");
			sb.Append(m_peer.id);
			sb.AppendLine();
		}
	}
}
