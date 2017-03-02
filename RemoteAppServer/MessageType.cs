namespace RemoteApp
{
	public enum MessageType
	{
		Unknown = 0,
		HelloRequest,
		HelloResponse,
		ConnectRequest,
		ConnectResponse,
		ListenRequest,
		ListenResponse,
		AcceptRequest,
		StreamData,
		StreamError,
	}
}