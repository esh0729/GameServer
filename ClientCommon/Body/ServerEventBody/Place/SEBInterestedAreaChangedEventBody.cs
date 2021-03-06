using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommon
{
	//=====================================================================================================================
	// (ServerEventBody 상속) 관심영역변경 서버 이벤트 데이터를 담고 있는 클래스
	//=====================================================================================================================
	public class SEBInterestedAreaChangedEventBody : ServerEventBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 추가된 영웅 정보 배열
		public PDHero[] addedHeroes;

		// 삭제된 영웅 ID 배열
		public Guid[] removedHeroes;

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

			writer.Write(addedHeroes);

			writer.Write(removedHeroes);
		}

		//=====================================================================================================================
		// 역직렬화 처리를 하는 함수
		//
		// reader : 내부 스트림에서 역직렬화 하여 다시 사용할 수 있는 데이터로 변환하는 객체
		//=====================================================================================================================
		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			addedHeroes = reader.ReadPacketDatas<PDHero>();

			removedHeroes = reader.ReadGuids();
		}
	}
}
