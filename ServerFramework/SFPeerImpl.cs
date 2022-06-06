using System;
using System.Text;
using System.Collections.Generic;

using Server;

namespace ServerFramework
{
	//=====================================================================================================================
	// 클라이언트 요청의 처리에 대한 분류와 클라이언트 송신에대한 응답 또는 이벤트 객체의 생성을 관리하는 추상 클래스
	//=====================================================================================================================
	public abstract class SFPeerImpl : PeerBase
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Construcotrs

		//=====================================================================================================================
		// 생성자
		//
		// peerInit : 서버의 메인 클래스 객체와 소켓 정보를 담고 있는 객체
		//=====================================================================================================================
		public SFPeerImpl(PeerInit peerInit)
			: base(peerInit)
		{
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//
		// 수신
		//

		//=====================================================================================================================
		// 클라이언트 요청 메세지를 Request 타입별로 분류하여 처리하는 함수
		//
		// request : 클라이언트 요청 메세지
		//=====================================================================================================================
		protected override void OnOperationRequest(OperationRequest request)
		{
			// operationCode를 타입으로 하여 분류
			switch (request.operationCode)
			{
				// 응답이 필요한 명령 타입
				case (byte)RequestType.Command:
					{
						// 역직렬화된 데이터를 분류

						// 명령에 대한 타입
						int nName = (int)((byte)request[(byte)CommandParameter.Name]);
						// 인게임에 사용될 내부 패킷
						byte[] packet = (byte[])request[(byte)CommandParameter.Packet];
						// 명령의 고유 ID
						long lnCommandId = (long)request[(byte)CommandParameter.Id];

						// 분류된 데이터에 대한 내용을 전달
						OnCommand(nName, packet, lnCommandId);
					}
					break;

				// 응답이 필요 없는 이벤트 타입
				case (byte)RequestType.Event:
					{
						// 역직렬화된 데이터를 분류

						// 이벤트에 대한 타입
						int nName = (int)((byte)request[(byte)ClientEventParameter.Name]);
						// 인게임에 사용될 내부 패킷
						byte[] packet = (byte[])request[(byte)ClientEventParameter.Packet];

						// 분류된 데이터에 대한 내용을 전달
						OnEvent(nName, packet);
					}
					break;
			}
		}

		//=====================================================================================================================
		// 클라이언트 요청이 명령 타입일 경우 호출되는 추상 함수
		//
		// nName : 명령에 대한 타입
		// packet : 인게임에 사용될 패킷
		// lnCommandId : 명령의 고유ID
		//=====================================================================================================================
		protected abstract void OnCommand(int nName, byte[] packet, long lnCommandId);
		//=====================================================================================================================
		// 클라이언트 요청이 이벤트 타입일 경우 호출되는 추상 함수
		//
		// nName : 이벤트에 대한 타입
		// packet : 인게임에 사용될 패킷
		//=====================================================================================================================
		protected abstract void OnEvent(int nName, byte[] packet);

		//
		// 송신
		//

		//=====================================================================================================================
		// 응답의 데이터를 저장하는 컬렉션을 반환하는 함수
		//
		// nName : 명령 응답에 대한 타입
		// packet : 명령 응답에 대한 패킷
		// lnId : 처리된 명령의 고유ID
		//=====================================================================================================================
		public Dictionary<byte, object> CreateCommandParameters(int nName, byte[] packet, long lnId)
		{
			Dictionary<byte, object> parameters = new Dictionary<byte, object>();
			parameters[(byte)CommandParameter.Name] = nName;
			parameters[(byte)CommandParameter.Packet] = packet;
			parameters[(byte)CommandParameter.Id] = lnId;

			return parameters;
		}

		//=====================================================================================================================
		// 요청 없이 서버에서 클라이언트로 송신하는 이벤트 데이터를 저장하는 컬렉션을 반환하는 함수
		//
		// nName : 이벤트에 대한 타입
		// packet : 이벤트에 대한 패킷
		//=====================================================================================================================
		public Dictionary<byte, object> CreateEventParameters(int nName, byte[] packet)
		{
			Dictionary<byte, object> parameters = new Dictionary<byte, object>();
			parameters[(byte)ClientEventParameter.Name] = nName;
			parameters[(byte)ClientEventParameter.Packet] = packet;

			return parameters;
		}

		//=====================================================================================================================
		// 요청에 대한 응답 객체를 생성하는 함수
		// Return : 생성된 응답 객체
		//
		// nReturnCode : 요청에 대한 결과코드
		// parameters : 응답 데이터가 저장되어 있는 컬렉션
		//=====================================================================================================================
		public OperationResponse CreateOperationResponse(short nReturnCode, Dictionary<byte, object> parameters)
		{
			return new OperationResponse(0, nReturnCode, parameters);
		}

		//=====================================================================================================================
		// 요청 없이 서버에서 클라이언트로 송신하는 이벤트 객체를 생성하는 함수
		// Return : 생성된 이벤트 객체
		//
		// parameters : 클라이언트로 송신하는 데이터가 저장되어 있는 컬렉션
		//=====================================================================================================================
		public EventData CreateEventData(Dictionary<byte, object> parameters)
		{
			return new EventData(0, parameters);
		}
	}
}
