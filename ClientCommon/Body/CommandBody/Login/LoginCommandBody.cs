using System.IO;

namespace ClientCommon
{
	public class LoginCommandBody : CommandBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public string accessToken;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Serialize(PacketWriter writer)
		{
			base.Serialize(writer);

			writer.Write(accessToken);
		}

		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			accessToken = reader.ReadString();
		}
	}

	public class LoginResponseBody : ResponseBody
	{
	}
}
