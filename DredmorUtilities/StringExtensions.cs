using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DredmorUtilities
{
	public static class StringExtensions
	{
        /// <summary>
        /// Returns a new string with the first character capitalized.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
		public static string Capitalize( this string s )
		{
			if ( s == null ) return null;
			if ( s.Length == 0 ) return s;
			return s.Substring( 0, 1 ).ToUpper() + s.Substring( 1 );
		}

		/// <summary>
		/// Replaces backslashes with the forward slashes used in Dredmor XML file paths.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string ToForwardSlashes( this string s )
		{
			return s.Replace( "\\", "/" );
		}

		/// <summary>
		/// Replaces the forward slashes used in Dredmor XML file paths with backslashes.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string ToBackSlashes( this string s )
		{
			return s.Replace( "/", "\\" );
		}
	}
}
