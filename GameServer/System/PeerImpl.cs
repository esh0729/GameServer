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
				Type handlerType = Server.instance.commandHnadlerFactory.GetCommandHandler(nName);
				if (handlerType == null)
					throw new Exception("해당 핸들러가 존재하지 않습니다. name : " + (CommandName)nName);

				SFHandler handler = (SFHandler)Activator.CreateInstance(handlerType);
				handler.Init(this, nName, lnCommandId, packet);

				handler.Run();
			}
			catch (Exception ex)
			{
				LogUtil.Error(GetType(), ex);

				OperationResponse response = CreateOperationResponse(Handler.kResult_Error, new Dictionary<byte, object>());
				response.debugMessage = ex.Message;

				SendResponse(response);
			}
		}

		protected override void OnEvent(int nName, byte[] packet)
		{
			Console.WriteLine("OnEvent");
		}
	}
}
