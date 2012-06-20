using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DredmorUtilities;
using System.IO;
using System.Xml;

namespace DredmorXmlValidation
{
    /// <summary>
    /// The validator for the core game.
    /// </summary>
	public class CoreValidator
	{
		GameResources resources = new GameResources();
		ContentDirectory core;

		public CoreValidator( string path )
		{
			core = ContentDirectory.Create( path, true );
		}

        /// <summary>
        /// Performs XML and XSD validation. If no files are malformed the performs resource validation.
        /// </summary>
        /// <returns></returns>
		public CoreValidationResult Validate()
		{
			CoreValidationResult result = new CoreValidationResult();

			var xmlFiles = core.EnumerateXmlFiles( DredmorInfo.XmlFilesToValidate, SearchOption.AllDirectories );

			XsdValidator xsd = new XsdValidator();
			result.XmlErrors = xsd.Validate( xmlFiles, true );

			if ( !result.IsValid && result.XmlErrors.Any( p => p.XmlExceptionOccurred ) )
			{
				return result;
			}

			resources.LoadResources( core );

			result.XmlErrors.AddRange( 
				XmlResourceValidator.Validate( xmlFiles, resources )
				.Where( p => p.Errors.Count > 0 )
			);

			return result;
		}
	}
}
