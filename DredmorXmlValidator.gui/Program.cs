//Copyright (c) 2012 Gaslamp Games Inc. See license.txt for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DredmorModValidator;

namespace DredmorXmlValidation
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );
			Application.Run( new Main() );
		}
	}
}
