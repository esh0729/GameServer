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

		public static void SendHeroMove(IEnumerable<ClientPeer> clientPeers, Guid heroId, PDVector3 position, float fYRotation)
		{
			SEBHeroMoveEventBody body = new SEBHeroMoveEventBody();
			body.heroId = heroId;
			body.position = position;
			body.yRotation = fYRotation;

			Send(ServerEventName.HeroMove, body, clientPeers);
		}

		public static void SendHeroActionStarted(IEnumerable<ClientPeer> clientPeers, Guid heroId, int nActionId, PDVector3 position, float fYRotation)
		{
			SEBHeroActionStartedEventBody body = new SEBHeroActionStartedEventBody();
			body.heroId = heroId;
			body.actionId = nActionId;
			body.position = position;
			body.yRotation = fYRotation;

			Send(ServerEventName.HeroActionStarted, body, clientPeers);
		}

		//
		// 관심영역
		//

		public static void SendInterestedAreaChanged(ClientPeer clientPeer, PDHero[] addedHeroes, Guid[] removedHeroes)
		{
			SEBInterestedAreaChangedEventBody body = new SEBInterestedAreaChangedEventBody();
			body.addedHeroes = addedHeroes;
			body.removedHeroes = removedHeroes;

			Send(ServerEventName.InterestedAreaChanged, body, clientPeer);
		}

		public static void SendHeroInterestedAreaEnter(IEnumerable<ClientPeer> clientPeers, PDHero hero)
		{
			SEBHeroInterestedAreaEnterEventBody body = new SEBHeroInterestedAreaEnterEventBody();
			body.hero = hero;

			Send(ServerEventName.HeroInterestedAreaEnter, body, clientPeers);
		}

		public static void SendHeroInterestedAreaExit(IEnumerable<ClientPeer> clientPeers, Guid heroId)
		{
			SEBHeroInterestedAreaExitEventBody body = new SEBHeroInterestedAreaExitEventBody();
			body.heroid = heroId;

			Send(ServerEventName.HeroInterestedAreaExit, body, clientPeers);
		}
	}
}
