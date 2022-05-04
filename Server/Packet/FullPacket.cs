using System;
using System.IO;

namespace Server
{
	public class FullPacket
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public PacketType m_type;
		public int m_nPacketLength;
		public byte[] m_packet;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public PacketType type
		{
			get { return m_type; }
		}

		public int packetLength
		{
			get { return m_nPacketLength; }
		}

		public byte[] packet
		{
			get { return m_packet; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Set(PacketType type, byte[] packet)
		{
			if (packet == null)
				throw new ArgumentNullException("packet");

			m_type = type;
			m_nPacketLength = packet.Length;
			m_packet = packet;
		}

		public void Clear()
		{
			m_type = default(PacketType);
			m_nPacketLength = 0;
			m_packet = null;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static byte[] ToBytes(FullPacket fullPacket)
		{
			MemoryStream stream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write((byte)fullPacket.type);
			writer.Write(fullPacket.packetLength);
			writer.Write(fullPacket.packet);

			return stream.ToArray();
		}

		public static void ToFullPacket(byte[] bytes, ref FullPacket fullPacket)
		{
			MemoryStream stream = new MemoryStream(bytes);
			BinaryReader reader = new BinaryReader(stream);
			byte bType = reader.ReadByte();
			int nPacketLength = reader.ReadInt32();
			byte[] packet = reader.ReadBytes(nPacketLength);

			fullPacket.Set((PacketType)bType, packet);
		}
	}
}
