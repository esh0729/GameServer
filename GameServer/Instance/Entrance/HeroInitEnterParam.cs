using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
	//=====================================================================================================================
	// 영웅초기입장에 대한 데이터를 저장하기 위한 클래스
	//=====================================================================================================================
	public class HeroInitEnterParam : EntranceParam
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 입장대륙
		private Continent m_continent = null;
		// 입장위치
		private Vector3 m_position = Vector3.zero;
		// 입장방향
		private float m_fYRotation = 0f;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 생성자
		//
		// continent : 입장대륙
		// position : 입장위치
		// fYRotation : 입장방향
		//=====================================================================================================================
		public HeroInitEnterParam(Continent continent, Vector3 position, float fYRotation)
		{
			if (continent == null)
				throw new ArgumentNullException("continent");

			m_continent = continent;
			m_position = position;
			m_fYRotation = fYRotation;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 입장대륙
		public Continent continent
		{ 
			get { return m_continent; }
		}

		// 입장위치
		public Vector3 position
		{
			get { return m_position; }
		}

		// 입장방향
		public float yRotation
		{
			get { return m_fYRotation; }
		}
	}
}
