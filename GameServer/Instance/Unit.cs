using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
	//=====================================================================================================================
	// 영웅 및 몬스터의 기본이 되는 추상 클래스
	//=====================================================================================================================
	public abstract class Unit
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 현재 장소
		protected PhysicalPlace m_currentPlace = null;
		// 위치
		protected Vector3 m_position = Vector3.zero;
		// 방향
		protected float m_fYRotation = 0f;

		// 섹터
		protected Sector m_sector = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 현재 장소
		public PhysicalPlace currentPlace
		{
			get { return m_currentPlace; }
		}

		// 위치
		public Vector3 position
		{
			get { return m_position; }
		}

		// 방향
		public float yRotation
		{
			get { return m_fYRotation; }
		}

		// 이동 스피드
		public virtual float moveSpeed
		{
			get { return 0f; }
		}

		// 섹터
		public Sector sector
		{
			get { return m_sector; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 현재 위치를 설정하는 함수
		//
		// place : 설정 장소
		//=====================================================================================================================
		public void SetCurrentPlace(PhysicalPlace place)
		{
			m_currentPlace = place;
		}

		//=====================================================================================================================
		// 현재 위치와 방향을 설정하는 함수
		//
		// position : 설정 위치
		// fYRotation : 설정 방향
		//=====================================================================================================================
		public void SetPosition(Vector3 position, float fYRotation)
		{
			m_position = position;
			m_fYRotation = fYRotation;
		}

		//=====================================================================================================================
		// 현재 섹터를 설정하는 함수
		//
		// sector : 설정 섹터
		//=====================================================================================================================
		public void SetSector(Sector sector)
		{
			Sector oldSector = m_sector;
			m_sector = sector;

			// 섹터 변경 이후 처리할 작업 실행
			OnSetSector(oldSector);
		}

		//=====================================================================================================================
		// 섹터 변경 이후 처리할 작업을 실행하는 가상 함수
		//
		// oldSector : 이전 섹터
		//=====================================================================================================================
		protected virtual void OnSetSector(Sector oldSector)
		{

		}
	}
}
