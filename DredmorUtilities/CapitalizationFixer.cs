//Copyright (c) 2012 Gaslamp Games Inc. See license.txt for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;

namespace DredmorUtilities
{
	public class CapitalizationFixer
	{
		static Dictionary<string, string> words = GetWords();

		/// <summary>
		/// Fixes the capilizations of element names, attribute names, and whole attribute values.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static string Fix( string xml )
		{
			string[] wordsSortedByLength = words.Keys.OrderByDescending( w => w.Length ).ToArray();

			xml = Regex.Replace(
				xml,
				"(</?)(" + String.Join( "|", wordsSortedByLength ) + @")([\s>])",
				match =>
					match.Groups[ 1 ].Captures[ 0 ].Value
					+ words[ match.Groups[ 2 ].Captures[ 0 ].Value.ToLower() ]
					+ match.Groups[ 3 ].Captures[ 0 ].Value,
				RegexOptions.IgnoreCase
			);

			xml = Regex.Replace(
				xml,
				@"(\s)(" + String.Join( "|", wordsSortedByLength ) + @")(\s*=)",
				match =>
					match.Groups[ 1 ].Captures[ 0 ].Value
					+ words[ match.Groups[ 2 ].Captures[ 0 ].Value.ToLower() ]
					+ match.Groups[ 3 ].Captures[ 0 ].Value,
				RegexOptions.IgnoreCase
			);

			xml = Regex.Replace(
				xml,
				@"(=""\s*)(" + String.Join( "|", wordsSortedByLength ) + @")(\s*"")",
				match =>
					match.Groups[ 1 ].Captures[ 0 ].Value
					+ words[ match.Groups[ 2 ].Captures[ 0 ].Value.ToLower() ]
					+ match.Groups[ 3 ].Captures[ 0 ].Value,
				RegexOptions.IgnoreCase
			);

			return xml;
		}

		/// <summary>
		/// Fixes the capitalization of all whole words, no matter where they appear in the document. 
		/// </summary>
		/// <remarks>This is much faster than <see cref="Fix"/>. However, it should not be used to create a fixed version of a document for saving because it will recapitalize words in descriptions, comments, etc. It shoudl only be used for case-insensitive schema validation.</remarks>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static string FixAllInstances( string xml )
		{
			string[] wordsSortedByLength = words.Keys.OrderByDescending( w => w.Length ).ToArray();

			string output = Regex.Replace(
				xml,
				"(" + String.Join( "|", wordsSortedByLength ) + ")",
				match => words[ match.Value.ToLower() ],
				RegexOptions.IgnoreCase
			);

			return output;
		}

		/// <summary>
		/// Lowercases element names, attribute names, and whole attribute values.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static string LowercaseWords( string xml )
		{
			string[] wordsSortedByLength = words.Keys.OrderByDescending( w => w.Length ).ToArray();

			xml = Regex.Replace(
				xml,
				"(</?)(" + String.Join( "|", wordsSortedByLength ) + @")([\s>])",
				match =>
					match.Groups[ 1 ].Captures[ 0 ].Value
					+ match.Groups[ 2 ].Captures[ 0 ].Value.ToLower()
					+ match.Groups[ 3 ].Captures[ 0 ].Value,
				RegexOptions.IgnoreCase
			);

			xml = Regex.Replace(
				xml,
				@"(\s)(" + String.Join( "|", wordsSortedByLength ) + @")(\s*)=",
				match =>
					match.Groups[ 1 ].Captures[ 0 ].Value
					+ match.Groups[ 2 ].Captures[ 0 ].Value.ToLower()
					+ match.Groups[ 3 ].Captures[ 0 ].Value,
				RegexOptions.IgnoreCase
			);

			xml = Regex.Replace(
				xml,
				@"(=""\s*)(" + String.Join( "|", wordsSortedByLength ) + @")(\s*"")",
				match =>
					match.Groups[ 1 ].Captures[ 0 ].Value
					+ match.Groups[ 2 ].Captures[ 0 ].Value.ToLower()
					+ match.Groups[ 3 ].Captures[ 0 ].Value,
				RegexOptions.IgnoreCase
			);

			return xml;
		}

		private static Dictionary<string, string> GetWords()
		{
			Dictionary<string, string> words = new Dictionary<string, string>();

			Assembly assembly = Assembly.GetAssembly( typeof( CapitalizationFixer ) );
			StreamReader reader = new StreamReader( assembly.GetManifestResourceStream( "DredmorUtilities.words.txt" ) );

			while ( !reader.EndOfStream )
			{
				string word = reader.ReadLine();

				if ( !String.IsNullOrWhiteSpace( word ) )
				{
					words.Add( word.ToLower(), word );
				}
			}

			return words;
		}
	}
}
