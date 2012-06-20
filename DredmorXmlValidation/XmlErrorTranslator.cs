//Copyright (c) 2012 Gaslamp Games Inc. See license.txt for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DredmorXmlValidation
{
	/// <summary>
	/// Translates the raw XSD errors from XmlReader into friendlier ones.
	/// </summary>
	public class XmlErrorTranslator
	{
		static readonly Regex attributeValueError = new Regex( @"The '(\w+)' attribute is invalid - The value '(\w*)' is invalid according to its datatype", RegexOptions.IgnoreCase );
		static readonly Regex attributeError1 = new Regex( @"The '(\w+)' attribute is not declared.", RegexOptions.IgnoreCase );
		static readonly Regex attributeError2 = new Regex( @"Could not find schema information for the attribute '(\w+)'.", RegexOptions.IgnoreCase );
		static readonly Regex childElementError = new Regex( @"The element '(\w+)' has invalid child element '(\w+)'.", RegexOptions.IgnoreCase );

		/// <summary>
		/// Translates the raw XSD error from XmlReader into a friendlier one.
		/// </summary>
		public static string Translate(string error)
		{
			Match match;

			match = attributeValueError.Match( error );
			if ( match.Success )
			{
				return String.Format(
					"'{0}' is not a valid value for the '{1}' attribute.",
					match.Groups[ 2 ].Value,
					match.Groups[ 1 ].Value
				);
			}

			match = attributeError1.Match( error );
			if ( match.Success )
			{
				return String.Format(
					"'{0}' is not a valid attribute for this element.",
					match.Groups[ 1 ].Value
				);
			}

			match = attributeError2.Match( error );
			if ( match.Success )
			{
				return String.Format(
					"'{0}' is not a valid attribute for this element.",
					match.Groups[ 1 ].Value
				);
			}

			match = childElementError.Match( error );
			if ( match.Success )
			{
				return String.Format(
					"'{0}' is not a valid child for '{1}'",
					match.Groups[ 2 ].Value,
					match.Groups[ 1 ].Value
				);
			}

			return error;
		}
	}
}
