using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server;
using ServerFramework;
using ClientCommon;

namespace GameServer
{
	public abstract class PeerImpl : SFPeerImpl
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public PeerImpl(PeerInit peerInit)
			: base(peerInit)
		{
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnCommand(int nName, byte[] packet, long lnCommandId)
		{
			try
			{
				//
				// CommandHandlerFactory에 ClienName 별로 Handler Type이 저장되있음
				// 

				Type handlerType = Server.instance.commandHnadlerFactory.GetCommandHandler(nName);
				if (handlerType == null)
					throw new Exception("해당 핸들러가 존재하지 않습니다. name : " + (CommandName)nName);

				//
				// 해당 Type으로 인스턴스 생성 및 초기화
				//

				SFHandler handler = (SFHandler)Activator.CreateInstance(handlerType);
				handler.Init(this, nName, lnCommandId, packet);

				//
				// 핸들러 실행
				//

				handler.Run();
			}
			catch (Exception ex)
			{
				//
				// 핸들러 작업중 에러 발생시 클라이언트에 에러 응답 코드 및 에러 메시지 전달
				//

				LogUtil.Error(GetType(), ex);

				OperationResponse response = CreateOperationResponse(Handler.kResult_Error, new Dictionary<byte, object>());
				response.debugMessage = ex.Message;

				SendResponse(response);
			}
		}

		protected override void OnEvent(int nName, byte[] packet)
		{

		}
	}
}
