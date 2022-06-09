using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientCommon
{
	//=====================================================================================================================
	// (PacketData 상속) 송수신에 사용 될 다른 영웅의 정보를 저장하는 클래스
	//=====================================================================================================================
	public class PDHero : PacketData
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 영웅 ID
		public Guid heroId;
		// 영웅 이름
		public string name;
		// 영웅 캐릭터ID
		public int characterId;
		// 영웅 위치
		public PDVector3 position;
		// 영웅 방향
		public float yRotation;
		// 영웅의 현재 행동
		public int actionId;

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

			writer.Write(heroId);
			writer.Write(name);
			writer.Write(characterId);
			writer.Write(position);
			writer.Write(yRotation);
			writer.Write(actionId);
		}

		//=====================================================================================================================
		// 역직렬화 처리를 하는 함수
		//
		// reader : 내부 스트림에서 역직렬화 하여 다시 사용할 수 있는 데이터로 변환하는 객체
		//=====================================================================================================================
		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			heroId = reader.ReadGuid();
			name = reader.ReadString();
			characterId = reader.ReadInt32();
			position = reader.ReadPDVector3();
			yRotation = reader.ReadSingle();
			actionId = reader.ReadInt32();
		}
	}
}
