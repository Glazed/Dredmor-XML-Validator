//Copyright (c) 2012 Gaslamp Games Inc. See license.txt for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using System.Xml;
using System.IO;
using System.Reflection;
using DredmorUtilities;

namespace DredmorXmlValidation
{
	/// <summary>
	/// Validates XMl files against the Dredmor schema.
	/// </summary>
	public class XsdValidator
	{
		static readonly XmlSchema schema = GetSchema();

		/// <summary>
		/// Validates each file, optionally ignoring the case of elements, attributes, and enumerated attribute values.
		/// </summary>
		/// <param name="filesToValidate"></param>
		/// <param name="ignoreCase">If true, uses the CapitalizationFixer.Fix method to fix the capitalization of the file before validating.</param>
		/// <returns></returns>
		public List<XmlFileValidationErrorList> Validate( IEnumerable<ContentFile> filesToValidate, bool ignoreCase )
		{
			List<XmlFileValidationErrorList> result = new List<XmlFileValidationErrorList>();

			foreach ( var file in filesToValidate )
			{
				try
				{
					using ( StreamReader fileStream = file.OpenText() )
					{
						using ( TextReader xmlTextReader = ( ignoreCase ) ? (TextReader) new StringReader( CapitalizationFixer.Fix( fileStream.ReadToEnd() ) ) : (TextReader) fileStream )
						{
							var errors = ValidateFile( xmlTextReader, ignoreCase );

							if ( errors.Count > 0 )
							{
								result.Add(
									new XmlFileValidationErrorList
									{
										Path = file.FullName,
										Errors = errors
									}
								);
							}
						}
					}					
				}
				catch ( XmlException ex )
				{
					result.Add( new XmlFileValidationErrorList( file.FullName, ex ) );
				}
			}

			return result;
		}

		private List<XmlValidationError> ValidateFile( TextReader xmlTextReader, bool ignoreCase )
		{
			List<XmlValidationError> errors = new List<XmlValidationError>();

			ValidationEventHandler errorHandler = 
				( object sender, ValidationEventArgs e ) =>
				{
					errors.Add(
						new XmlValidationError
						{
							LineNumber = e.Exception.LineNumber,
							LinePosition = e.Exception.LinePosition,
							Message = e.Message,
							Severity = e.Severity
						}
					);
				};

			XmlReaderSettings settings = new XmlReaderSettings();
			settings.ValidationType = ValidationType.Schema;
			settings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings;
			settings.ValidationEventHandler += errorHandler;
			settings.Schemas.Add( schema );

			using ( XmlReader reader = XmlReader.Create( xmlTextReader, settings ) )
			{
				while ( reader.Read() );
			}

			return errors;
		}

		/// <summary>
		/// Gets the embedded schema document.
		/// </summary>
		/// <returns></returns>
		private static XmlSchema GetSchema()
		{
			Assembly assembly = Assembly.GetAssembly( typeof( XsdValidator ) );

			return XmlSchema.Read( assembly.GetManifestResourceStream( "DredmorXmlValidation.DredmorSchema.xsd" ), null );
		}
	}
}
