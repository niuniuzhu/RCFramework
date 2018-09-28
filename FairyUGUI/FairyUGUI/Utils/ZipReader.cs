using System.Text;
using Core.Net;

namespace FairyUGUI.Utils
{
	public class ZipReader
	{
		public class ZipEntry
		{
			public string name;
			public int compress;
			public uint crc;
			public int size;
			public int sourceSize;
			public int offset;
			public bool isDirectory;
		}

		readonly StreamBuffer _stream;
		readonly int _entryCount;
		int _pos;
		int _index;

		public ZipReader( byte[] stream )
		{
			this._stream = new StreamBuffer( stream );

			int pos = ( int ) this._stream.length - 22;
			this._stream.position = pos + 10;
			this._entryCount = this._stream.ReadUShort();
			this._stream.position = pos + 16;
			this._pos = this._stream.ReadInt();
		}

		public int entryCount => this._entryCount;

		public bool GetNextEntry( ZipEntry entry )
		{
			if ( this._index >= this._entryCount )
				return false;

			this._stream.position = this._pos + 28;
			int len = this._stream.ReadUShort();
			int len2 = this._stream.ReadUShort() + this._stream.ReadUShort();

			this._stream.position = this._pos + 46;
			byte[] bytes = this._stream.ReadBytes( len );
			string name = Encoding.UTF8.GetString( bytes );
			name = name.Replace( "\\", "/" );

			entry.name = name;
			if ( name[name.Length - 1] == '/' ) //directory
			{
				entry.isDirectory = true;
				entry.compress = 0;
				entry.crc = 0;
				entry.size = entry.sourceSize = 0;
				entry.offset = 0;
			}
			else
			{
				entry.isDirectory = false;
				this._stream.position = this._pos + 10;
				entry.compress = this._stream.ReadUShort();
				this._stream.position = this._pos + 16;
				entry.crc = this._stream.ReadUInt();
				entry.size = this._stream.ReadInt();
				entry.sourceSize = this._stream.ReadInt();
				this._stream.position = this._pos + 42;
				entry.offset = this._stream.ReadInt() + 30 + len;
			}

			this._pos += 46 + len + len2;
			this._index++;

			return true;
		}

		public byte[] GetEntryData( ZipEntry entry )
		{
			byte[] data = null;
			if ( entry.size > 0 )
			{
				this._stream.position = entry.offset;
				data = this._stream.ReadBytes( entry.size );
			}
			return data;
		}
	}
}
