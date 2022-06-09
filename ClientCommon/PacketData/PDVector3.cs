﻿using System.IO;

namespace ClientCommon
{
	//=====================================================================================================================
	// 송수신에 사용 될 위치 정보를 저장하고 있는 구조체
	//=====================================================================================================================
	public struct PDVector3
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// x좌표
		public float x;
		// y좌표
		public float y;
		// z좌표
		public float z;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public PDVector3(float fX, float fY, float fZ)
		{
			x = fX;
			y = fY;
			z = fZ;
		}
	}
}
