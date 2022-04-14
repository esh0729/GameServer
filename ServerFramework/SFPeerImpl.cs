using System;
using System.Text;
using System.Collections.Generic;

using Server;

namespace ServerFramework
{
	public abstract class SFPeerImpl : PeerBase
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Construcotrs

		public SFPeerImpl(PeerInit peerInit)
			: base(peerInit)
		{
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnOperationRequest(OperationRequest request)
		{
			switch (request.operationCode)
			{
				case (byte)RequestType.Command:
					{
						int nName = (int)((byte)request[(byte)CommandParameter.Name]);
						byte[] packet = (byte[])request[(byte)CommandParameter.Packet];
						long lnCommandId = (long)request[(byte)CommandParameter.Id];

						OnCommand(nName, packet, lnCommandId);
					}
					break;

				case (byte)RequestType.Event:
					{
						int nName = (int)((byte)request[(byte)ClientEventParameter.Name]);
						byte[] packet = (byte[])request[(byte)ClientEventParameter.Packet];

						OnEvent(nName, packet);
					}
					break;
			}
		}

		protected abstract void OnCommand(int nName, byte[] packet, long lnCommandId);
		protected abstract void OnEvent(int nName, byte[] packet);

		//
		//
		//

		public Dictionary<byte, object> CreateCommandParameters(int nName, byte[] packet, long lnId)
		{
			Dictionary<byte, object> parameters = new Dictionary<byte, object>();
			parameters[(byte)CommandParameter.Name] = nName;
			parameters[(byte)CommandParameter.Packet] = packet;
			parameters[(byte)CommandParameter.Id] = lnId;

			return parameters;
		}

		public Dictionary<byte, object> CreateEventParameters(int nName, byte[] packet)
		{
			Dictionary<byte, object> parameters = new Dictionary<byte, object>();
			parameters[(byte)ClientEventParameter.Name] = nName;
			parameters[(byte)ClientEventParameter.Packet] = packet;

			return parameters;
		}

		public OperationResponse CreateOperationResponse(short nReturnCode, Dictionary<byte, object> parameters)
		{
			return new OperationResponse(0, nReturnCode, parameters);
		}

		public EventData CreateEventData(Dictionary<byte, object> parameters)
		{
			return new EventData(0, parameters);
		}
	}
}
