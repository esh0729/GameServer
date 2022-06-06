using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
	//=====================================================================================================================
	// (IMessage 상속) 클라이언트의 요청에대한 응답 데이터를 송신할 때 사용되는 데이터를 저장하는 클래스
	//=====================================================================================================================
	public class OperationResponse : IMessage
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 연산코드
		private byte m_bOperationCode = 0;
		// 결과코드
		private short m_nReturnCode = 0;
		// 오류 메세지
		private string m_sDebugMessage = "";
		// 송신할 데이터를 저장하는 컬렉션
		private Dictionary<byte, object> m_parameters = new Dictionary<byte, object>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 매개변수가 없는 생성자
		//=====================================================================================================================
		public OperationResponse() :
			this(0, 0)
		{
		}

		//=====================================================================================================================
		// 연산코드와 결과코드를 매개변수로 받는 생성자
		//
		// bOperationCode : 연산코드
		// nReturnCode : 결과코드
		//=====================================================================================================================
		public OperationResponse(byte bOperationCode, short nReturnCode) :
			this(bOperationCode, nReturnCode, new Dictionary<byte, object>())
		{
		}

		//=====================================================================================================================
		// 연산코드와 결과코드, 송신할 데이터를 매개변수로 받는 생성자
		//
		// bOperationCode : 연산코드
		// nReturnCode : 결과코드
		// parameters : 송신할 데이터 컬렉션
		//=====================================================================================================================
		public OperationResponse(byte bOperationCode, short nReturnCode, Dictionary<byte, object> parameters)
		{
			m_bOperationCode = bOperationCode;
			m_nReturnCode = nReturnCode;
			m_parameters = parameters;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 패킷의 전송타입(IMessage 구현 부분)
		public PacketType type
		{
			get { return PacketType.OperationResponse; }
		}

		// 연산코드
		public byte operationCode
		{
			get { return m_bOperationCode; }
			set { m_bOperationCode = value; }
		}

		// 결과코드
		public short returnCode
		{
			get { return m_nReturnCode; }
			set { m_nReturnCode = value; }
		}

		// 오류 메세지
		public string debugMessage
		{
			get { return m_sDebugMessage; }
			set { m_sDebugMessage = value; }
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
			writer.Write(m_bOperationCode);
			// 결과코드를 직렬화하여 저장
			writer.Write(m_nReturnCode);
			// 오류 메세지를 직렬화하여 저장
			writer.Write(m_sDebugMessage);

			// Dictionary 컬렉션을 직렬화 하기위한 BinaryFormatter 객체 생성
			BinaryFormatter formatter = new BinaryFormatter();
			// 송신할 데이터를 저장하고 있는 컬렉션 직렬화하여 저장
			formatter.Serialize(stream, m_parameters);

			// MemoryStream에서 사용하고 있는 데이터의 길이 반환
			lnLength = stream.Position;
		}
	}
}
