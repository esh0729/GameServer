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
