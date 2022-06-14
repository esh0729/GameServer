﻿using System.IO;

namespace ClientCommon
{
	//=====================================================================================================================
	// 클래스 단위의 데이터 송수신에 처리될 데이터의 직렬화, 역직렬화 함수를 지원하는 추상 클래스
	//=====================================================================================================================
	public abstract class PacketData
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 직렬화 처리를 하는 가상 함수
		//
		// writer : 직렬화 하여 내부 스트림에 저장하는 객체
		//=====================================================================================================================
		public virtual void Serialize(PacketWriter writer)
		{
		}

		//=====================================================================================================================
		// 역직렬화 처리를 하는 가상 함수
		//
		// reader : 내부 스트림에서 역직렬화 하여 다시 사용할 수 있는 데이터로 변환하는 객체
		//=====================================================================================================================
		public virtual void Deserialize(PacketReader reader)
		{
		}
	}
}
