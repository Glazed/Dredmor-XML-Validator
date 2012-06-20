using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DredmorUtilities;

namespace DredmorXmlCapitalizationFixer
{
	public partial class Main : Form
	{
		public Main()
		{
			InitializeComponent();
		}

		private void browseForInput_Click( object sender, EventArgs e )
		{
			if ( folderBrowserDialog.ShowDialog() == DialogResult.OK )
			{
				inputPath.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private void browseForOutput_Click( object sender, EventArgs e )
		{
			if ( folderBrowserDialog.ShowDialog() == DialogResult.OK )
			{
				outputPath.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private void go_Click( object sender, EventArgs e )
		{
			try
			{
				if ( !PathIsValid( inputPath.Text, "input", true ) )
					return;

				if ( !PathIsValid( outputPath.Text, "output", false ) )
					return;

				DirectoryInfo inputDir = new DirectoryInfo( inputPath.Text );

				Directory.CreateDirectory( outputPath.Text );

				var xmlFiles = inputDir.EnumerateFiles( "*.xml", SearchOption.TopDirectoryOnly ).Where( p => DredmorInfo.XmlFilesToValidate.Any( q => q == p.Name.ToLower() ) );

				var count = xmlFiles.Count();

				if ( count > 0 )
				{
					foreach ( var xmlFile in xmlFiles )
					{
						string newContents;

						using ( StreamReader fileStream = xmlFile.OpenText() )
						{
							newContents = CapitalizationFixer.Fix( fileStream.ReadToEnd() );
						}

						File.WriteAllText( Path.Combine( outputPath.Text, xmlFile.Name ), newContents );
					}

					MessageBox.Show( String.Format( "{0} files transformed", count ), "Done", MessageBoxButtons.OK );
				}
				else
				{
					MessageBox.Show( "No transformable XML files found.", "Done", MessageBoxButtons.OK );
				}
			}
			catch ( Exception ex )
			{
				ShowException( ex );
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

		private bool PathIsValid( string path, string pathName, bool requireExistence )
		{
			if ( String.IsNullOrWhiteSpace( path ) )
			{
				MessageBox.Show( String.Format( "Please enter the {0} path before clicking Go.", pathName ), "Error", MessageBoxButtons.OK );
				return false;
			}

			if ( requireExistence && !Directory.Exists( path ) )
			{
				MessageBox.Show( String.Format( "The input path was not found.", pathName ), "Error", MessageBoxButtons.OK );
				return false;
			}

			return true;
		}
	}
}
