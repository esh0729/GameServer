using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server;
using ServerFramework;
using ClientCommon;

namespace GameServer
{
	public class ServerEvent
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		private static EventData CreateEventData(Dictionary<byte,object> parameters)
		{
			return new EventData(0, parameters);
		}

		private static Dictionary<byte,object> CreateEventParameters(int nName, byte[] packet)
		{
			Dictionary<byte, object> parameters = new Dictionary<byte, object>();
			parameters[(byte)ServerEventParameter.Name] = nName;
			parameters[(byte)ServerEventParameter.Packet] = packet;

			return parameters;
		}

		private static void Send(EventData eventData, ClientPeer clientPeer)
		{
			clientPeer.SendEvent(eventData);
		}

		private static void Send(EventData eventData, IEnumerable<ClientPeer> clientPeers)
		{
			eventData.SendTo(clientPeers);
		}

		public static void Send(ServerEventName name, ServerEventBody body, ClientPeer clientPeer)
		{
			Send(CreateEventData(CreateEventParameters((int)name, body != null ? body.SerializeRaw() : new byte[] { })), clientPeer);
		}

		public static void Send(ServerEventName name, ServerEventBody body, IEnumerable<ClientPeer> clientPeers)
		{
			Send(CreateEventData(CreateEventParameters((int)name, body != null ? body.SerializeRaw() : new byte[] { })), clientPeers);
		}

		//
		// 계정
		//

		public static void SendLoginDuplicatedEvent(ClientPeer clientPeer)
		{
			Send(ServerEventName.LoginDuplicated, null, clientPeer);
		}

		//
		// 영웅
		//

		public static void SendHeroEnter(IEnumerable<ClientPeer> clientPeers, PDHero hero)
		{
			SEBHeroEnterEventBody body = new SEBHeroEnterEventBody();
			body.hero = hero;

			Send(ServerEventName.HeroEnter, body, clientPeers);
		}

		public static void SendHeroExit(IEnumerable<ClientPeer> clientPeers, Guid heroId)
		{
			SEBHeroExitEventBody body = new SEBHeroExitEventBody();
			body.heroId = heroId;

			Send(ServerEventName.HeroExit, body, clientPeers);
		}
	}
}
