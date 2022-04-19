using System;
using System.IO;

namespace ClientCommon
{
	public class PacketReader : BinaryReader
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public PacketReader(Stream input) :
			base(input)
		{
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public Guid ReadGuid()
		{
			return new Guid(ReadString());
		}

		public Guid[] ReadGuids()
		{
			if (!ReadBoolean())
				return null;

			int nLength = ReadInt32();

			Guid[] guids = new Guid[nLength];
			for (int i = 0; i < nLength; i++)
			{
				guids[i] = ReadGuid();
			}

			return guids;
		}

		public PDVector3 ReadPDVector3()
		{
			PDVector3 vector3 = new PDVector3();
			vector3.x = ReadSingle();
			vector3.y = ReadSingle();
			vector3.z = ReadSingle();

			return vector3;
		}

		public T ReadPacketData<T>() where T : PacketData
		{
			if (!ReadBoolean())
				return null;

			T instance = Activator.CreateInstance<T>();
			instance.Deserialize(this);

			return instance;
		}

		public T[] ReadPacketDatas<T>() where T : PacketData
		{
			if (!ReadBoolean())
				return null;

			int nLength = ReadInt32();

			T[] instances = new T[nLength];
			for (int i = 0; i < nLength; i++)
			{
				instances[i] = ReadPacketData<T>();
			}

			return instances;
		}
	}
}
