using Core.Net.Protocol;

namespace Core.Net
{
	public interface INetTransmitter
	{
		void Send( Packet packet );
		//void Send( byte[] data );//todo 这两个接口以后取消
		//void Send( byte[] data, int offset, int size );
	}
}