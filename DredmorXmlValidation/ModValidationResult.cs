using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DredmorXmlValidation
{
    /// <summary>
    /// The result class for the mod validator.
    /// </summary>
	public class ModValidationResult
	{
		public ModValidationResult()
		{
			XmlErrors = new List<XmlFileValidationErrorList>();
			ModErrors = new List<string>();
		}

        /// <summary>
        /// True if the count of XML errors and mod errors is zero.
        /// </summary>
		public bool IsValid
		{
			get
			{
				return XmlErrors.Count == 0 && ModErrors.Count == 0;
			}
		}

        /// <summary>
        /// Errors with the mod structure.
        /// </summary>
		public List<string> ModErrors{ get; set; }

        /// <summary>
        /// Errors with the XML files, including well-formedness and schema errors.
        /// </summary>
		public List<XmlFileValidationErrorList> XmlErrors { get; set; }

        /// <summary>
        /// A collection of resources from expansions that were found to be used by the mod.
        /// </summary>
		public GameResources ExpansionResourcedUsed { get; set; }

        /// <summary>
        /// The numbers of expansions that this mod references resources from.
        /// </summary>
		public List<int> ExpansionNumbersUsed { get; set; }
	}
}
