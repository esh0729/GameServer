using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommon
{
	//=====================================================================================================================
	// (CommandBody 상속) 영웅초기입장 명령 데이터를 담고 있는 클래스
	//=====================================================================================================================
	public class HeroInitEnterCommandBody : CommandBody
	{
	}

	//=====================================================================================================================
	// (ResponseBody 상속) 영웅초기입장 명령의 응답 데이터를 담고 있는 클래스
	//=====================================================================================================================
	public class HeroInitEnterResponseBody : ResponseBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		// 장소 인스턴스ID
		public Guid placeInstanceId;

		// 위치
		public PDVector3 position;
		// 방향
		public float yRotation;

		// 관심영역에 있는 영웅 배열
		public PDHero[] heroes;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 직렬화 처리를 하는 함수
		//
		// writer : 직렬화 하여 내부 스트림에 저장하는 객체
		//=====================================================================================================================
		public override void Serialize(PacketWriter writer)
		{
			base.Serialize(writer);

			writer.Write(placeInstanceId);

			writer.Write(position);
			writer.Write(yRotation);

			writer.Write(heroes);
		}

		//=====================================================================================================================
		// 역직렬화 처리를 하는 함수
		//
		// reader : 내부 스트림에서 역직렬화 하여 다시 사용할 수 있는 데이터로 변환하는 객체
		//=====================================================================================================================
		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			placeInstanceId = reader.ReadGuid();

			position = reader.ReadPDVector3();
			yRotation = reader.ReadSingle();

			heroes = reader.ReadPacketDatas<PDHero>();
		}
	}
}
