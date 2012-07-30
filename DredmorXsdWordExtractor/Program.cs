using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DredmorXsdWordExtractor
{
	class Program
	{
		static void Main( string[] args )
		{
			if ( args.Length == 0 )
			{
				Console.WriteLine( "No file specified." );
			}
			else if ( !File.Exists( args[ 0 ] ) )
			{
				Console.WriteLine( "File not found." );
			}
			else
			{
				var path = args[ 0 ];

				string xsd = File.ReadAllText( path );

				//Make sure each element is on its own line.
				xsd = Regex.Replace( xsd, @">\s*<", ">" + Environment.NewLine + "<" );

				Regex wordRegex = new Regex( @"<xs:(?:(?:element )|(?:attribute )|(?:enumeration )).*(?:(?:name)|(?:value))=""(\w+)""" );

				MatchCollection matches = wordRegex.Matches( xsd );

				List<string> words = new List<string>();

				foreach ( Match match in matches )
				{
					words.Add( match.Groups[ 1 ].Value );
				}

				var distinctWords = words.Distinct().OrderBy( p => p ).ToList();

#if DEBUG
				WriteWordsFile( distinctWords, @".\words.txt" );
#else
				WriteWordsFile( distinctWords, @"..\..\..\DredmorUtilities\words.txt" );
#endif
			}

			Console.Beep();
		}

		private static void WriteWordsFile( List<string> words, string path )
		{
			File.WriteAllLines( path, words );
		}
	}
}
