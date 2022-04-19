using System;
using System.IO;

namespace ClientCommon
{
	public class PacketWriter : BinaryWriter
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public PacketWriter(Stream output) :
			base(output)
		{
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Write(Guid guid)
		{
			Write(guid.ToString());
		}

		public void Write(Guid[] guids)
		{
			if (guids == null)
			{
				Write(false);
				return;
			}

			Write(true);

			int nLength = guids.Length;
			Write(nLength);

			for(int i = 0; i < nLength; i++)
			{
				Write(guids[i]);
			}
		}

		public void Write(PDVector3 vector3)
		{
			Write(vector3.x);
			Write(vector3.y);
			Write(vector3.z);
		}

		public void Write(PacketData instance)
		{
			if (instance == null)
			{
				Write(false);
				return;
			}

			Write(true);

			instance.Serialize(this);
		}

		public void Write(PacketData[] instances)
		{
			if (instances == null)
			{
				Write(false);
				return;
			}

			Write(true);

			int nLength = instances.Length;
			Write(nLength);

			for (int i = 0; i < nLength; i++)
			{
				Write(instances[i]);
			}
		}
	}
}
