using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
	public class EventData : IMessage
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private byte m_bCode = 0;
		private Dictionary<byte, object> m_parameters = new Dictionary<byte, object>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public EventData() :
			this(0)
		{
		}

		public EventData(byte bCode) :
			this(bCode, new Dictionary<byte, object>())
		{
		}

		public EventData(byte bCode, Dictionary<byte, object> parameters)
		{
			m_bCode = bCode;
			m_parameters = parameters;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public PacketType type
		{
			get { return PacketType.EventData; }
		}

		public byte code
		{
			get { return m_bCode; }
			set { m_bCode = value; }
		}

		public Dictionary<byte, object> parameters
		{
			get { return m_parameters; }
			set { m_parameters = value; }
		}

		public object this[byte bIndex]
		{
			get { return m_parameters[bIndex]; }
			set { m_parameters[bIndex] = value; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public byte[] GetBytes()
		{
			MemoryStream stream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_bCode);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, m_parameters);

			return stream.ToArray();
		}

		public void GetBytes(byte[] buffer, out long lnLength)
		{
			MemoryStream stream = new MemoryStream(buffer);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_bCode);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, m_parameters);

			lnLength = stream.Position;
		}

		public void SendTo(IEnumerable<PeerBase> peers)
		{
			foreach (PeerBase peer in peers)
			{
				peer.SendEvent(this);
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static EventData ToEventData(byte[] bytes)
		{
			MemoryStream stream = new MemoryStream(bytes);
			BinaryReader reader = new BinaryReader(stream);
			byte bCode = reader.ReadByte();

			BinaryFormatter formatter = new BinaryFormatter();
			object obj = formatter.Deserialize(stream);
			Dictionary<byte, object> parameters = (Dictionary<byte, object>)obj;

			return new EventData(bCode, parameters);
		}
	}
}
