using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
	//=====================================================================================================================
	// (IMessage 상속) 클라이언트로부터 수신한 요청 데이터를 저장하는 클래스
	//=====================================================================================================================
	public class OperationRequest : IMessage
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 요청데이터의 타입을 저장하는 연산코드(1. 응답이 필요한 요청, 2. 응답이 필요없는 요쳥)
		private byte m_bOperationCode = 0;
		// 수신한 데이터를 저장하는 컬렉션
		private Dictionary<byte, object> m_parameters;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		// 매개변수가 없는 생성자
		public OperationRequest() :
			this(0)
		{
		}

		// 연산코드만을 매개변수로 받는 생성자
		public OperationRequest(byte bOperationCode) :
			this(bOperationCode, new Dictionary<byte, object>())
		{
		}

		// 연산코드와 수신한 데이터를 매개변수로 받는 생성자
		public OperationRequest(byte bOperationCode, Dictionary<byte, object> parameters)
		{
			m_bOperationCode = bOperationCode;
			m_parameters = parameters;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 패킷의 전송타입(IMessage 구현 부분)
		public PacketType type
		{
			get { return PacketType.OperationRequest; }
		}

		// 연산코드
		public byte operationCode
		{
			get { return m_bOperationCode; }
			set { m_bOperationCode = value; }
		}

		// 수신한 데이터를 저장하는 컬렉션
		public Dictionary<byte, object> parameters
		{
			get { return m_parameters; }
			set { m_parameters = value; }
		}

		// 수신한 데이터를 저장하는 컬렉션의 인덱서
		public object this[byte bIndex]
		{
			get { return m_parameters[bIndex]; }
			set { m_parameters[bIndex] = value; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 내부 데이터를 직렬화하는 함수(IMessage 구현 부분) - 서버에서는 사용X
		// Return : 내부 데이터를 직렬화한 결과
		//=====================================================================================================================
		public void GetBytes(byte[] buffer, out long lnLength)
		{
			// 직렬화 데이터를 매개변수로 전달받은 버퍼에 저장하는 MemoryStream 객체 생성
			MemoryStream stream = new MemoryStream(buffer);
			// 데이터를 바이너리 데이터로 변환하여 MemoryStream 객체로 저장하기 위한 BinaryWriter 객체 생성
			BinaryWriter writer = new BinaryWriter(stream);
			// 연산코드를 직렬화하여 저장
			writer.Write(m_bOperationCode);

			// Dictionary 컬렉션을 직렬화 하기위한 BinaryFormatter 객체 생성
			BinaryFormatter formatter = new BinaryFormatter();
			// 송신할 데이터를 저장하고 있는 컬렉션 직렬화하여 저장
			formatter.Serialize(stream, m_parameters);

			// MemoryStream에서 사용하고 있는 데이터의 길이 반환
			lnLength = stream.Position;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		//=====================================================================================================================
		// 패킷을 전달 받아 OperationRequest 객체를 생성하는 정적함수
		// Return : 수신받은 패킷을 역직렬화 하여 저장하고 있는 OperationRequest 객체
		//
		// bytes : 패킷을 담고 있는 버퍼
		//=====================================================================================================================
		public static OperationRequest ToOperationRequest(byte[] bytes)
		{
			// 매개변수로 전달받은 패킷버퍼를 기반으로한 MemoryStream 객체 생성
			MemoryStream stream = new MemoryStream(bytes);
			// 패킷을 사용할 수 있는 데이터로 변환하기 위한 BinaryReader 객체 생성
			BinaryReader reader = new BinaryReader(stream);
			// 연산코드를 역직렬화
			byte bOperationCode = reader.ReadByte();

			// 패킷을 Dictionary 컬렉션으로 역직렬화 하기위한 BinaryFormatter 객체 생성
			BinaryFormatter formatter = new BinaryFormatter();
			// Dictionary와 관련된 패킷을 object 타입으로 역직렬화
			object obj = formatter.Deserialize(stream);
			// object 타입의 데이터를 Dictionary<byte, object> 타입으로 캐스팅
			Dictionary<byte, object> parameters = (Dictionary<byte, object>)obj;

			// 변환한 데이터를 저장하는 OperationRequest 객체 생성하여 return
			return new OperationRequest(bOperationCode, parameters);
		}
	}
}
