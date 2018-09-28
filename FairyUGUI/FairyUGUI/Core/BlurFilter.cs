namespace FairyUGUI.Core
{
	public struct BlurFilter
	{
		public readonly float size;

		internal float worldSize;

		public bool isIdentity => this.worldSize == 0;

		public BlurFilter( BlurFilter blurFilter )
		{
			this.size = blurFilter.size;
			this.worldSize = blurFilter.worldSize;
		}

		public BlurFilter( float size )
		{
			this.worldSize = this.size = size;
		}

		public static BlurFilter operator +( BlurFilter lhs, BlurFilter rhs )
		{
			lhs.worldSize = rhs.worldSize + lhs.size;
			return lhs;
		}

		public static bool operator ==( BlurFilter lhs, BlurFilter rhs )
		{
			return lhs.size == rhs.size && lhs.worldSize == rhs.worldSize;
		}

		public static bool operator !=( BlurFilter lhs, BlurFilter rhs )
		{
			return !( lhs == rhs );
		}

		public override bool Equals( object other )
		{
			if ( !( other is BlurFilter ) )
				return false;

			BlurFilter obj = ( BlurFilter )other;
			return this.size == obj.size && this.worldSize == obj.worldSize;
		}

		public override int GetHashCode()
		{
			return this.size.GetHashCode();
		}
	}
}