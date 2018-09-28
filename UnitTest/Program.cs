using System.IO;

namespace UnitTest
{
	class Program
	{
		static int Main( string[] args )
		{
			string str = CSharpToTS.Convert( File.ReadAllText( args[0] ) );
			File.WriteAllText( args[0] + ".ts", str );
			return 0;
		}
	}
}