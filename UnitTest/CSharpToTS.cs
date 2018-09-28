using System.Text.RegularExpressions;
using Xunit;

namespace UnitTest
{
	public class CSharpToTS
	{
		private static readonly Regex R1 = new Regex( @"([^\s]+\s)([^\s]+\s)(\=)", RegexOptions.Singleline );
		private static readonly Regex R2 = new Regex( @"(public\s[static\s]*)([^\s]+\s)([\w|\(]+[^\)]+\))", RegexOptions.Singleline );
		private static readonly Regex R3 = new Regex( @"(private\s[static\s]*)([^\s]+\s)([\w|\(]+[^\)]+\))", RegexOptions.Singleline );
		private static readonly Regex R4 = new Regex( @"([0-9]+)f", RegexOptions.Singleline );
		private static readonly Regex R5 = new Regex( @"(Vec2|Vec3|Vec4|Mat2|Mat3|Mat4|Quat|number)\s(\w+)", RegexOptions.Singleline );

		public static string Convert( string input )
		{
			input = input.Replace( "float", "number" );
			input = input.Replace( "int", "number" );
			input = input.Replace( "double", "number" );
			input = input.Replace( "ushort", "number" );
			input = input.Replace( "short", "number" );
			input = input.Replace( "uint", "number" );
			input = input.Replace( "ulong", "number" );
			input = input.Replace( "long", "number" );
			input = input.Replace( "bool", "boolean" );
			input = input.Replace( "operator +", "Add" );
			input = input.Replace( "operator -", "Sub" );
			input = input.Replace( "operator *", "Mul" );
			input = input.Replace( "operator /", "Div" );
			MatchCollection matches = R1.Matches( input );
			foreach ( Match match in matches )
			{
				input = input.Replace( match.Value, match.Groups[2].Value + ":" + match.Groups[1].Value + match.Groups[3].Value );
			}

			Regex[] regexes = new Regex[2];
			regexes[0] = R2;
			regexes[1] = R3;
			foreach ( Regex regex in regexes )
			{
				matches = regex.Matches( input );
				foreach ( Match match in matches )
				{
					string s = match.Groups[1].Value + match.Groups[3].Value + ":" + match.Groups[2].Value;
					input = input.Replace( match.Value, s );
				}
			}

			matches = R4.Matches( input );
			foreach ( Match match in matches )
			{
				input = input.Replace( match.Value, match.Groups[1].Value );
			}

			matches = R5.Matches( input );
			foreach ( Match match in matches )
			{
				input = input.Replace( match.Value, match.Groups[2].Value + ":" + match.Groups[1].Value );
			}
			return input;
		}
	}
}