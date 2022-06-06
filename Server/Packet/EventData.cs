using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
	//=====================================================================================================================
	// (IMessage 상속) 클라이언트의 요청 없이 서버에서 클라이언트로 데이터를 송신할 때 사용하는 데이터를 저장하는 클래스
	//=====================================================================================================================
	public class EventData : IMessage
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 연산코드(현재는 사용하지 않으므로 0을 전달)
		private byte m_bCode = 0;
		// 송신할 데이터를 저장하는 컬렉션
		private Dictionary<byte, object> m_parameters;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		// 매개변수가 없는 생성자
		public EventData() :
			this(0)
		{
		}

		// 연산코드만을 매개변수로 받는 생성자
		public EventData(byte bCode) :
			this(bCode, new Dictionary<byte, object>())
		{
		}

		// 연산코드와 송신할 데이터를 매개변수로 받는 생성자
		public EventData(byte bCode, Dictionary<byte, object> parameters)
		{
			m_bCode = bCode;
			m_parameters = parameters;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 패킷의 전송타입(IMessage 구현 부분)
		public PacketType type
		{
			get { return PacketType.EventData; }
		}

		// 연산코드
		public byte code
		{
			get { return m_bCode; }
			set { m_bCode = value; }
		}

		// 송신할 데이터를 저장하는 컬렉션
		public Dictionary<byte, object> parameters
		{
			get { return m_parameters; }
			set { m_parameters = value; }
		}

		// 송신할 데이터를 저장하는 컬렉션의 인덱서
		public object this[byte bIndex]
		{
			get { return m_parameters[bIndex]; }
			set { m_parameters[bIndex] = value; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 내부 데이터를 직렬화하는 함수(IMessage 구현 부분 / 결과를 Return하지 않고 매개변수로 결과를 반환)
		//
		// buffer : 직렬화한 데이터를 저장할 버퍼
		// lnLength : 직렬화한 데이터의 길이
		//=====================================================================================================================
		public void GetBytes(byte[] buffer, out long lnLength)
		{
			// 직렬화 데이터를 매개변수로 전달받은 버퍼에 저장하는 MemoryStream 객체 생성
			MemoryStream stream = new MemoryStream(buffer);
			// 데이터를 바이너리 데이터로 변환하여 MemoryStream 객체로 저장하기 위한 BinaryWriter 객체 생성
			BinaryWriter writer = new BinaryWriter(stream);
			// 연산코드를 직렬화하여 저장
			writer.Write(m_bCode);

			// Dictionary 컬렉션을 직렬화 하기위한 BinaryFormatter 객체 생성
			BinaryFormatter formatter = new BinaryFormatter();
			// 송신할 데이터를 저장하고 있는 컬렉션 직렬화하여 저장
			formatter.Serialize(stream, m_parameters);

			// MemoryStream에서 사용하고 있는 데이터의 길이 반환
			lnLength = stream.Position;
		}

		//=====================================================================================================================
		// 해당 데이터를 브로드캐스트 또는 특정 클라이언트들에게 송신하기 위해 호출하는 함수
		//
		// peers : 송신할 클라이언트의 피어 컬렉션
		//=====================================================================================================================
		public void SendTo(IEnumerable<PeerBase> peers)
		{
			// 피어를 순회하며 피어에 연결된 클라이언트에 해당 데이터를 송신하는 함수 호출
			foreach (PeerBase peer in peers)
			{
				peer.SendEvent(this);
			}
		}
	}
}
