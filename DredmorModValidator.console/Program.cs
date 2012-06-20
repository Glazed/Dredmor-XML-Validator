using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DredmorXmlValidation;
using DredmorUtilities;

namespace DredmorModValidator
{
	public enum ExitCodes
	{
		SuccessNoErrors = 0,
		SuccessWithErrors = 1,
		NoPathSpecified = 2,
		FileNotFound = 3,
		FileIsNotZip = 4,
		UnknownError = 10
	}

	class Program
	{
		static int Main( string[] args )
		{
			try
			{
				if ( args.Length == 0 )
				{
					Console.Error.WriteLine( "No path specified." );
					return (int) ExitCodes.NoPathSpecified;
				}

				string path = args[ 0 ];

				if ( !File.Exists( path ) )
				{
					Console.Error.WriteLine( "File not found." );
					return (int) ExitCodes.FileNotFound;
				}

				if ( Path.GetExtension( path ).ToLower() != ".zip" )
				{
					Console.Error.WriteLine( "Specified file is not a zip file." );
					return (int) ExitCodes.FileIsNotZip;
				}

				ModValidator v = new ModValidator( path );
				var result = v.Validate();

				StringBuilder builder = new StringBuilder();

				BuildErrors( result, builder );

				try
				{
					File.WriteAllText( Path.Combine( Path.GetDirectoryName( path ), Path.GetFileName( path ) + ".ValidationLog.txt" ), builder.ToString() );
				}
				catch ( Exception )
				{
					//we do nothing if the log file fails to write.
				}

				if ( result.IsValid )
				{
					return (int) ExitCodes.SuccessNoErrors;
				}
				else
				{
					return (int) ExitCodes.SuccessWithErrors;
				}
			}
			catch ( Exception ex )
			{
				Console.Error.WriteLine( ex.GetType().FullName );
				Console.Error.WriteLine( ex.Message );
				Console.Error.WriteLine( ex.StackTrace );
				return (int) ExitCodes.UnknownError;
			}
		}

		private static void BuildErrors( ModValidationResult result, StringBuilder builder )
		{
			foreach ( var modError in result.ModErrors )
			{
				builder.AppendLine( modError );
			}

			foreach ( var fileValidationErrors in result.XmlErrors )
			{
				if ( fileValidationErrors.Errors.Count > 0 )
				{
					builder.AppendLine( fileValidationErrors.Path );
					builder.AppendLine( String.Format( "{0} error(s) found.", fileValidationErrors.Errors.Count ) );

					foreach ( var error in fileValidationErrors.Errors )
					{
						builder.AppendLine(
							String.Format( 
								"Line {0}, Position: {1} -- {2}",
								error.LineNumber,
								error.LinePosition,
								XmlErrorTranslator.Translate( error.Message )
							)
						);
					}

					builder.AppendLine();
				}
			}		
		}
	}
}
