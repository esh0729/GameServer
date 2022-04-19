using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
	public struct Rect3D
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public float x;
		public float y;
		public float z;
		public float xSize;
		public float ySize;
		public float zSize;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public Rect3D(float x, float y, float z, float xSize, float ySize, float zSize)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.xSize = xSize;
			this.ySize = ySize;
			this.zSize = zSize;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public bool Contains(Vector3 position)
		{
			return x >= position.x && x + xSize < position.x
				&& y >= position.y && y + ySize < position.y
				&& z >= position.z && z + zSize < position.z;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		public static readonly Rect3D zero = new Rect3D(0, 0, 0, 0, 0, 0);
	}
}
