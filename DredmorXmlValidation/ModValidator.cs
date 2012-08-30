//Copyright (c) 2012 Gaslamp Games Inc. See license.txt for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DredmorUtilities;
using System.IO;
using Ionic.Zip;
using System.Reflection;
using System.Xml.Linq;

namespace DredmorXmlValidation
{
	public class ModValidator
	{
		GameResources resources;
		ContentDirectory mod;

		/// <summary>
		/// A collection of resources from expansions that were found to be used by the mod.
		/// </summary>		
		public GameResources ExpansionResourcesUsed { get; private set; }

		public ModValidator( string path )
		{
			resources = GameResources.Deserialize( GetResourceManifest() );
			ExpansionResourcesUsed = new GameResources();

			mod = ContentDirectory.Create( path, false );
		}

		public ModValidator( Stream stream )
		{
			resources = GameResources.Deserialize( GetResourceManifest() );
			ExpansionResourcesUsed = new GameResources();

			mod = ContentDirectory.Create( stream );
		}

		/// <summary>
		/// Performs mod structure validation, XML and XSD validation, resource validation and expansion requirement validation.
		/// </summary>
		/// <returns></returns>
		public ModValidationResult Validate()
		{
			ModValidationResult result = new ModValidationResult();

			var modFolder = mod.GetDirectory( @"mod" );

			if ( modFolder == null )
			{
				result.ModErrors.Add( "The required folder, 'mod', was not found." );
				return result;
			}

			var modFile = modFolder.GetFile( "mod.xml" );

			if ( modFile == null )
			{
				result.ModErrors.Add( "The required file, 'mod/mod.xml', was not found." );
				return result;
			}

			var xmlFiles = modFolder.EnumerateXmlFiles( DredmorInfo.XmlFilesToValidate, SearchOption.TopDirectoryOnly );

			XsdValidator xsd = new XsdValidator();
			result.XmlErrors = xsd.Validate( xmlFiles, true );

			if ( !result.IsValid && result.XmlErrors.Any( p => p.XmlExceptionOccurred ) )
			{
				return result;
			}

			resources.LoadResources( mod );

			result.XmlErrors.AddRange(
				XmlResourceValidator.Validate(
					xmlFiles.Where( p => p.Name.ToLower() != "mod.xml" ),
					resources,
					ExpansionResourcesUsed
				)
				.Where( p => p.Errors.Count > 0 )
			);

			result.ExpansionResourcedUsed = ExpansionResourcesUsed;

			result.ExpansionNumbersUsed =
				ExpansionResourcesUsed.Animations.Select( p => p.ExpansionNumber )
				.Concat( result.ExpansionResourcedUsed.Files.Select( p => p.ExpansionNumber ) )
				.Concat( result.ExpansionResourcedUsed.ContentResources.Select( p => p.ExpansionNumber ) )
				.Distinct()
				.OrderBy( p => p )
				.ToList();

			AddRequirementErrors( result, modFile );

			return result;
		}

		private static void AddRequirementErrors( ModValidationResult result, ContentFile modFile )
		{
			//we already loaded and used XSD validation on mod.xml annd exited if it had malformed XML, so it's safe to just load it here.
			//However we are not sure their "expansion" attributes exist or are integers.
			using ( var modXml = modFile.OpenRead() )
			{
				XDocument doc = XDocument.Load( modXml );

				var requireElements = doc.Root.Elements( "require", true );

				//Need nullables to support left joins.				
				List<int?> statedRequirements = new List<int?>();
				List<int?> knownRequirements = result.ExpansionNumbersUsed.Select( p => new Nullable<int>( p ) ).ToList();

				foreach ( var element in requireElements )
				{
					var expansion = element.GetIntAttribute( "expansion" );
					if ( expansion != null )
					{
						statedRequirements.Add( expansion.Value );
					}
				}

				//distinct list in case they repeat themselves.
				statedRequirements = statedRequirements.Distinct().ToList();

				//left join to find missing requirements.
				var missingRequirements =
					from knownRequirement in knownRequirements
					join statedRequirement in statedRequirements
					on knownRequirement equals statedRequirement into statedRequirementGroup
					from statedRequirement in statedRequirementGroup.DefaultIfEmpty()
					where statedRequirement == null
					select knownRequirement;

				result.ModErrors.AddRange(
					missingRequirements.Select(
						p =>
						String.Format( "Expansion {0} is used by your mod, but you didn't include <require expansion=\"{0}\"/> in mod.xml", p )
					)
				);

				//the opposite left join to find overstated requirements.
				var overstatedRequirements =
					from overStatedRequirement in statedRequirements
					join knownRequirement in knownRequirements
					on overStatedRequirement equals knownRequirement into knownRequirementGroup
					from knownRequirement in knownRequirementGroup.DefaultIfEmpty()
					where knownRequirement == null
					select overStatedRequirement;

				result.ModErrors.AddRange(
					overstatedRequirements.Select(
						p =>
						String.Format( "You have <require expansion=\"{0}\"/> in mod.xml, but expansion {0} is not used by your mod.", p )
					)
				);
			}
		}

		/// <summary>
		/// Returns the embedded resource manifest.
		/// </summary>
		/// <returns></returns>
		private string GetResourceManifest()
		{
			Assembly assmebly = Assembly.GetAssembly( this.GetType() );
			Stream manifest = assmebly.GetManifestResourceStream( "DredmorXmlValidation.ResourceManifest.xml" );
			StreamReader reader = new StreamReader( manifest );

			return reader.ReadToEnd();
		}
	}
}
