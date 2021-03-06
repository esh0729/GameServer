using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClientCommon;

namespace GameServer
{
	public struct Vector3
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public float x;
		public float y;
		public float z;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public Vector3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override string ToString()
		{
			return String.Format("({0}, {1}, {2})", x, y, z);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		public static readonly Vector3 zero = new Vector3(0, 0, 0);

		public static implicit operator PDVector3(Vector3 vector3) => new PDVector3(vector3.x, vector3.y, vector3.z);
		public static implicit operator Vector3(PDVector3 vector3) => new Vector3(vector3.x, vector3.y, vector3.z);
	}
}
