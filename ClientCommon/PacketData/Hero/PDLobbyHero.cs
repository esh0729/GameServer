using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommon
{
	//=====================================================================================================================
	// (PacketData 상속) 송수신에 사용 될 계정의 생성된 영웅의 정보를 저장하는 클래스
	//=====================================================================================================================
	public class PDLobbyHero : PacketData
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 영웅 ID
		public Guid heroId;
		// 영웅 이름
		public string name;
		// 영웅 캐릭터ID
		public int characterId;

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
		}
	}
}
