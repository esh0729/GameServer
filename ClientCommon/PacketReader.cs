using System;
using System.IO;

namespace ClientCommon
{
	//=====================================================================================================================
	// (BinaryReader 상속) 데이터 송수신에 사용되는 패킷의 역직렬화를 추가로 처리하는 클래스(BinaryReader로 역직렬화 할수 없는 객체를 역직렬화)
	//=====================================================================================================================
	public class PacketReader : BinaryReader
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 생성자
		//
		// input : 패킷의 정보를 가지고 있는 스트림
		//=====================================================================================================================
		public PacketReader(Stream input) :
			base(input)
		{
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 패킷에서 Guid(고유식별자)의 정보를 역직렬화 하는 함수
		//=====================================================================================================================
		public Guid ReadGuid()
		{
			// 패킷에서 문자열 상태로 읽어온 후 해당 문자열을 Guid로 캐스팅
			return new Guid(ReadString());
		}

		//=====================================================================================================================
		// 패킷에서 Guid의 배열 정보를 역직렬화 하는 함수
		//=====================================================================================================================
		public Guid[] ReadGuids()
		{
			// 직렬화 하였을때 배열의 값이 null인지 체크
			if (!ReadBoolean())
				return null;

			// 배열의 길이 체크
			int nLength = ReadInt32();

			// 읽어온 배열의 길이만큼 배열 생성
			Guid[] guids = new Guid[nLength];
			// 배열의 길이만큼 Guid의 정보를 읽어오는 함수를 호출
			for (int i = 0; i < nLength; i++)
			{
				guids[i] = ReadGuid();
			}

			return guids;
		}

		//=====================================================================================================================
		// 패킷에서 PDVector3(좌표정보)의 정보를 역직렬화 하는 함수
		//=====================================================================================================================
		public PDVector3 ReadPDVector3()
		{
			// PDVector3 객체 생성 후 각 좌표에 해당되는 정보를 float로 읽어 생성된 객체에 저장
			PDVector3 vector3 = new PDVector3();
			vector3.x = ReadSingle();
			vector3.y = ReadSingle();
			vector3.z = ReadSingle();

			return vector3;
		}

		//=====================================================================================================================
		// 패킷에서 PacketData(일반화된 송수신에 필요한 객체)의 정보를 역직렬화 하는 함수
		//=====================================================================================================================
		public T ReadPacketData<T>() where T : PacketData
		{
			// 직렬화 하였을때 해당 객체가 null인지 체크
			if (!ReadBoolean())
				return null;

			// 넘겨받은 제네릭 타입으로 해당 클래스 인스턴스 생성
			T instance = Activator.CreateInstance<T>();
			// PacketData를 상속받은 객체의 오버라이딩 된 Deserialize 함수 호출
			instance.Deserialize(this);

			return instance;
		}

		//=====================================================================================================================
		// 패킷에서 PacketData(일반화된 송수신에 필요한 객체)의 배열 정보를 역직렬화 하는 함수
		//=====================================================================================================================
		public T[] ReadPacketDatas<T>() where T : PacketData
		{
			// 직렬화 하였을때 배열의 값이 null인지 체크
			if (!ReadBoolean())
				return null;

			// 배열의 길이 체크
			int nLength = ReadInt32();

			// 읽어온 배열의 길이만큼 배열 생성
			T[] instances = new T[nLength];
			// 배열의 길이만큼 PacketData의 정보를 읽어오는 함수를 호출
			for (int i = 0; i < nLength; i++)
			{
				instances[i] = ReadPacketData<T>();
			}

			return instances;
		}
	}
}
