//Copyright (c) 2012 Gaslamp Games Inc. See license.txt for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using System.Xml;

namespace DredmorXmlValidation
{
	/// <summary>
	/// Contains a list of <see cref="XmlValidationError"/> for a specific file.
	/// </summary>
	public class XmlFileValidationErrorList
	{	
		/// <summary>
		/// The absolute path to the physical file, or the relative path for a mod file.
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// The list of errors.
		/// </summary>
		public List<XmlValidationError> Errors { get; set; }

		/// <summary>
		/// If true, indicates that an exception occurred loading the XML file.
		/// </summary>
		public bool XmlExceptionOccurred { get; private set; }

		public XmlFileValidationErrorList()
		{

		}


		/// <summary>
		/// Constructs an instance based on a <see cref="XmlException"/> that occurred.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="ex"></param>
		public XmlFileValidationErrorList( string filePath, XmlException ex )
		{
			Path = filePath;
			Errors = new List<XmlValidationError> { new XmlValidationError( ex ) };
			XmlExceptionOccurred = true;
		}

		public override string ToString()
		{
			return String.Format( "{0}: {1} Errors", Path, Errors.Count );
		}
	}

	/// <summary>
	/// A specific XML error.
	/// </summary>
	public class XmlValidationError
	{
		public int LineNumber { get; set; }
		public int LinePosition { get; set; }
		public string Message { get; set; }
		public XmlSeverityType Severity { get; set; }

		public XmlValidationError()
		{
			
		}

		public XmlValidationError( XmlException ex )
		{
			LineNumber = ex.LineNumber;
			LinePosition = ex.LinePosition;
			Message = ex.Message;
			Severity = XmlSeverityType.Error;
		}

		public override string ToString()
		{
			return String.Format( "Line: {0}, Position: {1}, {2}", LineNumber, LinePosition, Message );
		}
	}
}
