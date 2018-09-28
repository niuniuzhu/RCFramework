using System.Collections.Generic;

namespace ProtocolGenerator
{
	public class ModuleEntry
	{
		public string id;
		public string key;
		public readonly List<PacketEntry> packets = new List<PacketEntry>();
	}
}