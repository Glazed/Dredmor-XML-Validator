﻿//Copyright (c) 2012 Gaslamp Games Inc. See license.txt for license information.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DredmorXmlValidation;

namespace DredmorModValidator
{
	public partial class Main : Form
	{
		Font headingFont = new Font( "Consolas", 12f, FontStyle.Bold );
		Font errorCountFont = new Font( "Consolas", 10f, FontStyle.Italic );
		Font errorFont = new Font( "Consolas", 10f );

		public Main()
		{
			InitializeComponent();

			this.openFileDialog.InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );

			this.MinimumSize = this.MaximumSize = this.Size;
		}

		private void browseForMod_Click( object sender, EventArgs e )
		{
			if ( modTypeDirectory.Checked )
			{
				if ( folderBrowserDialog.ShowDialog() == DialogResult.OK )
				{
					modLocation.Text = folderBrowserDialog.SelectedPath;
				}
			}
			else
			{
				if ( openFileDialog.ShowDialog() == DialogResult.OK )
				{
					modLocation.Text = openFileDialog.FileName;
				}
			}
		}

		private void validate_Click( object sender, EventArgs e )
		{
			if ( String.IsNullOrWhiteSpace( modLocation.Text ) )
			{
				MessageBox.Show( "Please enter a mod location before clicking Validate.", "Error", MessageBoxButtons.OK );
				return;
			}
			else if ( !File.Exists( modLocation.Text ) && !Directory.Exists( modLocation.Text ) )
			{
				MessageBox.Show( "The zip file or directory was not found.", "Error", MessageBoxButtons.OK );
				return;
			}

			try
			{
				ModValidator v = new ModValidator( modLocation.Text );
				var result = v.Validate();

				output.Clear();

				WriteErrors( result );

				WriteExpansionsUsed( result );
			}
			catch ( Exception ex )
			{
				ShowException( ex );
			}
		}

		private void WriteExpansionsUsed( ModValidationResult result )
		{
			if ( result.ExpansionNumbersUsed != null )
			{
				this.expansionNumbers.Text =
					String.Join(
						", ",
						result.ExpansionNumbersUsed
					);
			}
		}

		private static void ShowException( Exception ex )
		{
			MessageBox.Show(
				String.Format(
@"{0}
{1}

{2}",
					ex.GetType().Name,
					ex.Message,
					ex.StackTrace
				)
			);
		}

		private void WriteErrors( ModValidationResult result )
		{
			if ( result.ModErrors.Count == 0 && result.XmlErrors.Count == 0 )
			{
				WriteHeading( "No errors! Congratulations!" );
			}
			else
			{
				foreach ( var modError in result.ModErrors )
				{
					WriteHeading( modError );

					output.AppendText( Environment.NewLine );
					output.AppendText( Environment.NewLine );
				}

				foreach ( var fileValidationErrors in result.XmlErrors.Where( p => p.Errors.Count > 0 ) )
				{
					WriteHeading( fileValidationErrors.Path );
					WriteSubheading( fileValidationErrors.Errors.Count );
					WriteErrors( fileValidationErrors.Errors );

					output.AppendText( Environment.NewLine );
					output.AppendText( Environment.NewLine );
				}
			}
		}

		private void WriteHeading( string path )
		{
			output.SelectionFont = headingFont;
			output.AppendText( path );
			output.AppendText( Environment.NewLine );
		}

		private void WriteSubheading( int count )
		{
			output.SelectionFont = errorCountFont;				
			output.AppendText( String.Format( "{0} error(s) found.", count ) );			
		}

		private void WriteErrors( List<XmlValidationError> list )
		{
			output.AppendText( Environment.NewLine );
			foreach ( var error in list )
			{
				output.SelectionFont = errorFont;
				output.AppendText(
					String.Format(
						"Line {0}, Position: {1} -- {2}",
						error.LineNumber,
						error.LinePosition,
						XmlErrorTranslator.Translate( error.Message )
					)
				);

				output.AppendText( Environment.NewLine );
				output.AppendText( Environment.NewLine );
			}
		}

		private void zoomOut_Click( object sender, EventArgs e )
		{
			output.ZoomFactor -= 0.1f;
		}

		private void zoomIn_Click( object sender, EventArgs e )
		{
			output.ZoomFactor += 0.1f;
		}

		private void save_Click( object sender, EventArgs e )
		{
			if ( saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK )
			{
				try
				{
					output.SaveFile( saveFileDialog.FileName );
				}
				catch ( Exception ex )
				{
					ShowException( ex );
				}
			}
		}
	}
}
