using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
	public class OperationRequest : IMessage
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private byte m_bOperationCode = 0;
		private Dictionary<byte, object> m_parameters = new Dictionary<byte, object>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public OperationRequest() :
			this(0)
		{
		}

		public OperationRequest(byte bOperationCode) :
			this(bOperationCode, new Dictionary<byte, object>())
		{
		}

		public OperationRequest(byte bOperationCode, Dictionary<byte, object> parameters)
		{
			m_bOperationCode = bOperationCode;
			m_parameters = parameters;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public PacketType type
		{
			get { return PacketType.OperationRequest; }
		}

		public byte operationCode
		{
			get { return m_bOperationCode; }
			set { m_bOperationCode = value; }
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
			writer.Write(m_bOperationCode);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, m_parameters);

			return stream.ToArray();
		}

		public void GetBytes(byte[] buffer, out long lnLength)
		{
			MemoryStream stream = new MemoryStream(buffer);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_bOperationCode);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, m_parameters);

			lnLength = stream.Position;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static OperationRequest ToOperationRequest(byte[] bytes)
		{
			MemoryStream stream = new MemoryStream(bytes);
			BinaryReader reader = new BinaryReader(stream);
			byte bOperationCode = reader.ReadByte();

			BinaryFormatter formatter = new BinaryFormatter();
			object obj = formatter.Deserialize(stream);
			Dictionary<byte, object> parameters = (Dictionary<byte, object>)obj;

			return new OperationRequest(bOperationCode, parameters);
		}
	}
}
