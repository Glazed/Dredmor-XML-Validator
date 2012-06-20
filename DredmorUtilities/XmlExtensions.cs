//Copyright (c) 2012 Gaslamp Games Inc. See license.txt for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DredmorUtilities
{
	public static class XmlExtensions
	{
		/// <summary>
		/// Gets the value of an attribute as a string. The attribute is searched for in a case-insensitive way.
		/// </summary>
		/// <returns>The string value, or null if the attribute doesn't exist.</returns>
		public static string GetStringAttribute( this XElement element, string name )
		{
			string value = null;

			foreach ( var att in element.Attributes() )
			{
				if ( att.Name.ToString().ToLower() == name.ToLower() )
				{
					value = att.Value;
				}
			}

			return value;
		}

		/// <summary>
		/// Gets the value of an attribute as a boolean. The attribute is searched for in a case-insensitive way.
		/// </summary>
		/// <returns>True if the string equals "1", false if not, or null if the attribute was not found.</returns>
		public static bool? GetBoolAttribute(this XElement element, string name)
		{
			bool? value = null;

			foreach ( var att in element.Attributes() )
			{
				if ( att.Name.ToString().ToLower() == name.ToLower() )
				{
					value = att.Value == "1" ? true : false;
				}
			}

			return value;
		}

		/// <summary>
		/// Gets the value of an attribute as a integer. The attribute is searched for in a case-insensitive way.
		/// </summary>
		/// <returns>The integer value, or null if the attribute doesn't exist or is not a valid integer.</returns>
		public static int? GetIntAttribute(this XElement element, string name)
		{
			int? value = null;

			foreach ( var att in element.Attributes() )
			{
				if ( att.Name.ToString().ToLower() == name.ToLower() )
				{
					int i;

					if ( int.TryParse( att.Value, out i ) )
					{
						value = i;
					}
				}
			}

			return value;
		}

		/// <summary>
		/// Gets the value of an attribute as a decimal. The attribute is searched for in a case-insensitive way.
		/// </summary>
		/// <returns>The decimal value, or null if the attribute doesn't exist or is not a valid decimal.</returns>
		public static decimal? GetDecimalAttribute(this XElement element, string name)
		{
			decimal? value = null;

			foreach ( var att in element.Attributes() )
			{
				if ( att.Name.ToString().ToLower() == name.ToLower() )
				{
					decimal i;

					if ( decimal.TryParse( att.Value, out i ) )
					{
						value = i;
					}
				}
			}

			return value;
		}

		/// <summary>
		/// Returns a collection of the descendant elements for this document or element, in document order. Optionally ignores case.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="name"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>
		public static IEnumerable<XElement> Descendants( this XElement element, XName name, bool ignoreCase )
		{
			var collection = element.Descendants();

			if ( ignoreCase )
			{
				collection = collection.Where( p => p.Name.ToString().ToLower() == name.ToString().ToLower() );
			}

			return collection;
		}

		// <summary>
		/// Returns a collection of the child elements for this document or element, in document order. Optionally ignores case.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="name"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>
		public static IEnumerable<XElement> Elements(this XElement element, XName name, bool ignoreCase)
		{
			var collection = element.Elements();

			if ( ignoreCase )
			{
				collection = collection.Where( p => p.Name.ToString().ToLower() == name.ToString().ToLower() );
			}

			return collection;
		}

		/// <summary>
		/// Returns a collection of the child elements of every element and document in the source collection. Optionally ignores case.
		/// </summary>
		/// <param name="elements"></param>
		/// <param name="name"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>
		public static IEnumerable<XElement> Elements( this IEnumerable<XElement> elements, XName name, bool ignoreCase )
		{
			var collection = elements.Elements();

			if ( ignoreCase )
			{
				collection = collection.Where( p => p.Name.ToString().ToLower() == name.ToString().ToLower() );
			}

			return collection;
		}

		/// <summary>
		/// Returns the <see cref="System.Xml.Linq.XAttribute"/> of this <see cref="System.Xml.Linq.XElement"/> that
		/// has the specified <see cref="System.Xml.Linq.XName"/>, optionally ignoring case.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="name"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>
		public static XAttribute Attribute( this XElement element, XName name, bool ignoreCase )
		{
			if ( ignoreCase )
			{
				return element.Attributes().Where( p => p.Name.ToString().ToLower() == name.ToString().ToLower() ).FirstOrDefault();
			}
			else
			{
				return element.Attribute( name );
			}
		}

		/// <summary>
		/// Returns a filtered collection of the attributes of every element in the source
		/// collection. Only elements that have a matching <see cref="System.Xml.Linq.XName"/> are
		/// included in the collection.
		/// </summary>
		/// <param name="elements"></param>
		/// <param name="name"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>
		public static IEnumerable<XAttribute> Attributes( this IEnumerable<XElement> elements, XName name, bool ignoreCase )
		{
			if ( ignoreCase )
			{
				return elements.Attributes().Where( p => p.Name.ToString().ToLower() == name.ToString().ToLower() );
			}
			else
			{
				return elements.Attributes( name );
			}
		}
	}	
}
