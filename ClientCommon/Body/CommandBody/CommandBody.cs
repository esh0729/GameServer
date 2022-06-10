﻿using System.IO;

namespace ClientCommon
{
	//=====================================================================================================================
	// (Body 상속) 클라이언트 명령(응답이 필요한 요청)의 데이터를 담고 있는 메인 추상 클래스
	//=====================================================================================================================
	public abstract class CommandBody : Body
	{
	}

	//=====================================================================================================================
	// (Body 상속) 클라이언트 명령(응답이 필요한 요청)의 응답 데이터를 담고 있는 메인 추상 클래스
	//=====================================================================================================================
	public abstract class ResponseBody : Body
	{
	}
}
