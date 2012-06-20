using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DredmorXmlValidation;
using System.IO;

namespace DredmorResourceManifestGenerator
{
	class Program
	{
		static void Main( string[] args )
		{
			if ( args.Length == 0 )
			{
				Console.WriteLine( "No directory specified." );
			}
			else if ( !Directory.Exists( args[ 0 ] ) )
			{
				Console.WriteLine( "Directory not found." );
			}
			else
			{
				var path = args[ 0 ];

				ContentDirectory core = ContentDirectory.Create( path, true );

				GameResources resources = new GameResources();
				resources.LoadResources( core );

#if DEBUG
				resources.SerializeToFile( @".\ResourceManifest.xml" );
#else
				resources.SerializeToFile( @"..\..\..\DredmorXmlValidation\ResourceManifest.xml" );
#endif
			}

			Console.Beep();
		}
	}
}
