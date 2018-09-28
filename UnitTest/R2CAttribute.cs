using System;

namespace UnitTest
{
	public class R2CAttribute : Attribute
	{
		public string name;

		public R2CAttribute( string name )
		{
			this.name = name;
		}
	}
}