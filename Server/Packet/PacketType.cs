namespace Server
{
	public enum PacketType : byte
	{
		PingCheck = 0,
		OperationRequest,
		EventData,
		OperationResponse
	}
}
