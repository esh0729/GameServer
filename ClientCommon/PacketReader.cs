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
