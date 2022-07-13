using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server;
using ServerFramework;
using ClientCommon;

namespace GameServer
{
	//=====================================================================================================================
	// 서버 이벤트 객체 생성 및 클라이언트 피어에게 서버 이벤트 객체 전달하는 클래스
	//=====================================================================================================================
	public class ServerEvent
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		//=====================================================================================================================
		// 클라이언트에게 송신할 필요 데이터와 실제 데이터를 저장하는 객체를 생성하는 함수
		// Return : 이벤트 데이터
		//
		// parameters : 송신할 데이터가 저장되어있는 컬렉션
		//=====================================================================================================================
		private static EventData CreateEventData(Dictionary<byte,object> parameters)
		{
			return new EventData(0, parameters);
		}

		//=====================================================================================================================
		// 클라이언트에게 송신할 실제 데이터를 저장하고 있는 컬렉션을 생성하는 함수
		// Return : 송신할 데이터가 저장되어있는 컬렉션
		//
		// nName : 서버 이벤트 타입
		// packet : 직렬화된 전달 데이터
		//=====================================================================================================================
		private static Dictionary<byte,object> CreateEventParameters(int nName, byte[] packet)
		{
			Dictionary<byte, object> parameters = new Dictionary<byte, object>();
			parameters[(byte)ServerEventParameter.Name] = nName;
			parameters[(byte)ServerEventParameter.Packet] = packet;

			return parameters;
		}

		//=====================================================================================================================
		// 클라이언트에게 서버 이벤트를 전송하는 함수를 호출하는 함수
		//
		// eventData : 전송할 서버 이벤트 데이터
		// clientPeer : 전송할 클라이언트 피어
		//=====================================================================================================================
		private static void Send(EventData eventData, ClientPeer clientPeer)
		{
			clientPeer.SendEvent(eventData);
		}

		//=====================================================================================================================
		// 다수의 클라이언트에게 서버 이벤트를 전송하는 함수를 호출하는 함수
		//
		// eventData : 전송할 서버 이벤트 데이터
		// clientPeers : 전송할 클라이언트 피어의 컬렉션
		//=====================================================================================================================
		private static void Send(EventData eventData, IEnumerable<ClientPeer> clientPeers)
		{
			eventData.SendTo(clientPeers);
		}

		//=====================================================================================================================
		// 송신할 데이터를 직렬화 하여 서버 이벤트 타입과 직렬화 데이터를 컬렉션에 저장 및 전달하는 함수
		//
		// name : 서버 이벤트 타입
		// body : 송신할 데이터가 저장되어 있는 객체
		// clientPeer : 전송할 클라이언트 피어
		//=====================================================================================================================
		public static void Send(ServerEventName name, ServerEventBody body, ClientPeer clientPeer)
		{
			Send(CreateEventData(CreateEventParameters((int)name, body != null ? body.SerializeRaw() : new byte[] { })), clientPeer);
		}

		//=====================================================================================================================
		// 송신할 데이터를 직렬화 하여 서버 이벤트 타입과 직렬화 데이터를 컬렉션에 저장 및 전달하는 함수
		//
		// name : 서버 이벤트 타입
		// body : 송신할 데이터가 저장되어 있는 객체
		// clientPeer : 전송할 클라이언트 피어의 컬렉션
		//=====================================================================================================================
		public static void Send(ServerEventName name, ServerEventBody body, IEnumerable<ClientPeer> clientPeers)
		{
			Send(CreateEventData(CreateEventParameters((int)name, body != null ? body.SerializeRaw() : new byte[] { })), clientPeers);
		}

		//
		// 계정
		//

		//=====================================================================================================================
		// 계정중복로그인 서버 이벤트를 처리하는 함수
		//
		// clientPeer : 전송할 클라이언트 피어
		//=====================================================================================================================
		public static void SendLoginDuplicatedEvent(ClientPeer clientPeer)
		{
			Send(ServerEventName.LoginDuplicated, null, clientPeer);
		}

		//
		// 영웅
		//

		//=====================================================================================================================
		// 영웅입장 서버 이벤트를 처리하는 함수
		//
		// clientPeers : 전송할 클라이언트 피어 컬렉션
		// hero : 입장 영웅
		//=====================================================================================================================
		public static void SendHeroEnter(IEnumerable<ClientPeer> clientPeers, PDHero hero)
		{
			SEBHeroEnterEventBody body = new SEBHeroEnterEventBody();
			body.hero = hero;

			Send(ServerEventName.HeroEnter, body, clientPeers);
		}

		//=====================================================================================================================
		// 영웅퇴장 서버 이벤트를 처리하는 함수
		//
		// clientPeers : 전송할 클라이언트 피어 컬렉션
		// heroId : 퇴장 영웅 ID
		//=====================================================================================================================
		public static void SendHeroExit(IEnumerable<ClientPeer> clientPeers, Guid heroId)
		{
			SEBHeroExitEventBody body = new SEBHeroExitEventBody();
			body.heroId = heroId;

			Send(ServerEventName.HeroExit, body, clientPeers);
		}

		//=====================================================================================================================
		// 영웅이동 서버 이벤트를 처리하는 함수
		//
		// clientPeers : 전송할 클라이언트 피어 컬렉션
		// heroId : 이동 영웅 ID
		// position : 위치
		// fYRotation : 방향
		//=====================================================================================================================
		public static void SendHeroMove(IEnumerable<ClientPeer> clientPeers, Guid heroId, PDVector3 position, float fYRotation)
		{
			SEBHeroMoveEventBody body = new SEBHeroMoveEventBody();
			body.heroId = heroId;
			body.position = position;
			body.yRotation = fYRotation;

			Send(ServerEventName.HeroMove, body, clientPeers);
		}

		//=====================================================================================================================
		// 영웅행동시작 서버 이벤트를 처리하는 함수
		//
		// clientPeers : 전송할 클라이언트 피어 컬렉션
		// heroId : 행동을 시작한 영웅 ID
		// nActionId : 행동 ID
		// position : 위치
		// fYRotation : 방향
		//=====================================================================================================================
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

		//=====================================================================================================================
		// 관심영역변경 서버 이벤트를 처리하는 함수
		//
		// clientPeer : 전송할 클라이언트 피어
		// addedHeroes : 추가된 영웅 배열
		// removedHeroes : 삭제된 영웅 ID 배열
		//=====================================================================================================================
		public static void SendInterestedAreaChanged(ClientPeer clientPeer, PDHero[] addedHeroes, Guid[] removedHeroes)
		{
			SEBInterestedAreaChangedEventBody body = new SEBInterestedAreaChangedEventBody();
			body.addedHeroes = addedHeroes;
			body.removedHeroes = removedHeroes;

			Send(ServerEventName.InterestedAreaChanged, body, clientPeer);
		}

		//=====================================================================================================================
		// 영웅관심영역입장 서버 이벤트를 처리하는 함수
		//
		// clientPeers : 전송할 클라이언트 피어 컬렉션
		// hero : 입장 영웅
		//=====================================================================================================================
		public static void SendHeroInterestedAreaEnter(IEnumerable<ClientPeer> clientPeers, PDHero hero)
		{
			SEBHeroInterestedAreaEnterEventBody body = new SEBHeroInterestedAreaEnterEventBody();
			body.hero = hero;

			Send(ServerEventName.HeroInterestedAreaEnter, body, clientPeers);
		}

		//=====================================================================================================================
		// 영웅관심영역퇴장 서버 이벤트를 처리하는 함수
		//
		// clientPeers : 전송할 클라이언트 피어 컬렉션
		// hero : 입장 영웅
		//=====================================================================================================================
		public static void SendHeroInterestedAreaExit(IEnumerable<ClientPeer> clientPeers, Guid heroId)
		{
			SEBHeroInterestedAreaExitEventBody body = new SEBHeroInterestedAreaExitEventBody();
			body.heroid = heroId;

			Send(ServerEventName.HeroInterestedAreaExit, body, clientPeers);
		}
	}
}
