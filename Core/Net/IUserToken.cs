namespace Core.Net
{
	public interface IUserToken : INetTransmitter
	{
		ushort id { get; }
	}
}