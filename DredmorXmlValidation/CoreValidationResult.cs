using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DredmorXmlValidation
{
	/// <summary>
	/// Validation result class for the <see cref="CoreValidator"/>.
	/// </summary>
	public class CoreValidationResult
	{
		public CoreValidationResult()
		{
			XmlErrors = new List<XmlFileValidationErrorList>();
		}

		/// <summary>
		/// True if the count of <see cref="XmlErrors"/> is zero.
		/// </summary>
		public bool IsValid
		{
			get
			{
				return XmlErrors.Count == 0;
			}
		}

		public List<XmlFileValidationErrorList> XmlErrors { get; set; }
	}
}
