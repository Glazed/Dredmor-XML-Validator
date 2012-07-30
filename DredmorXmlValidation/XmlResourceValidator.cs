//Copyright (c) 2012 Gaslamp Games Inc. See license.txt for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DredmorUtilities;
using System.Xml;
using System.Xml.Schema;
using System.Text.RegularExpressions;
using System.IO;

namespace DredmorXmlValidation
{
	/// <summary>
	/// Performs XML reference and physical resource validation.
	/// </summary>
	/// <remarks>
	/// Contains many protected methods used by each of the implementations of this class.
	/// </remarks>
	public abstract class XmlResourceValidator
	{
		protected XDocument doc;
		protected bool isCoreGame;
		protected GameResources resources;
		protected string path;
		protected XmlFileValidationErrorList result;
		protected GameResources expansionResourcesUsed;
		private int expansionNumber;

		protected XmlResourceValidator( ContentFile file, GameResources resources, GameResources expansionResourcesUsed )
		{
			this.resources = resources;
			this.path = file.FullName;
			this.expansionResourcesUsed = expansionResourcesUsed;

			using ( Stream xmlStream = file.OpenRead() )
			{
				this.doc = XDocument.Load( xmlStream, LoadOptions.SetLineInfo );
			}

			this.isCoreGame = file.Directory.IsCoreGame;
			this.expansionNumber = file.ExpansionNumber;

			this.result = new XmlFileValidationErrorList
			{
				Path = this.path,
				Errors = new List<XmlValidationError>()
			};
		}

		/// <summary>
		/// Factory method to create appropriate validator for the specified file.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="resources">An instance of <see cref="GameResources"/> containing all valid resources from the core game and optionally a mod.</param>
		/// <param name="expansionResourcesUsed">An instance of <see cref="GameResources"/> to which expansion resources found to be used will be added.</param>
		/// <returns></returns>
		public static XmlResourceValidator Create( ContentFile file, GameResources resources, GameResources expansionResourcesUsed = null )
		{
			switch ( file.Name.ToLower() )
			{
				case "craftdb.xml":
					return new CraftXmlResourceValidator( file, resources, expansionResourcesUsed );
				case "itemdb.xml":
					return new ItemXmlResourceValidator( file, resources, expansionResourcesUsed );
				case "mondb.xml":
					return new MonsterXmlResourceValidator( file, resources, expansionResourcesUsed );
				case "soundfx.xml":
					return new SoundXmlResourceValidator( file, resources, expansionResourcesUsed );
				case "spelldb.xml":
					return new SpellXmlResourceValidator( file, resources, expansionResourcesUsed );
				case "skilldb.xml":
					return new SkillXmlResourceValidator( file, resources, expansionResourcesUsed );
				case "rooms.xml":
					return new RoomXmlResourceValidator( file, resources, expansionResourcesUsed );
				case "encrustdb.xml":
					return new EncrustXmlResourceValidator( file, resources, expansionResourcesUsed );
				default:
					throw new InvalidOperationException( "The specified file is not supported." );
			}
		}

		/// <summary>
		/// Validates all fo the XML files.
		/// </summary>
		/// <param name="xmlFiles"></param>
		/// <param name="resources">An instance of <see cref="GameResources"/> containing all valid resources from the core game and optionally a mod.</param>
		/// <param name="expansionResourcesUsed">An instance of <see cref="GameResources"/> to which expansion resources found to be used will be added.</param>
		/// <returns></returns>
		public static List<XmlFileValidationErrorList> Validate( IEnumerable<ContentFile> xmlFiles, GameResources resources, GameResources expansionResourcesUsed = null )
		{
			List<XmlFileValidationErrorList> result = new List<XmlFileValidationErrorList>();

			foreach ( var file in xmlFiles )
			{
				try
				{
					XmlResourceValidator res = XmlResourceValidator.Create( file, resources, expansionResourcesUsed );
					result.Add( res.Validate() );
				}
				catch ( XmlException ex )
				{
					result.Add( new XmlFileValidationErrorList( file.FullName, ex ) );
				}
			}

			return result;
		}

		public abstract XmlFileValidationErrorList Validate();

		protected XmlValidationError CreateError( XElement element, string message, params object[] messageArgs )
		{
			return new XmlValidationError
			{
				LineNumber = ( (IXmlLineInfo) element ).LineNumber,
				LinePosition = ( (IXmlLineInfo) element ).LinePosition,
				Severity = XmlSeverityType.Error,
				Message = String.Format( message, messageArgs )
			};
		}

		protected XmlValidationError CreateError( XAttribute attribute, string message, params object[] messageArgs )
		{
			return new XmlValidationError
			{
				LineNumber = ( (IXmlLineInfo) attribute ).LineNumber,
				LinePosition = ( (IXmlLineInfo) attribute ).LinePosition,
				Severity = XmlSeverityType.Error,
				Message = String.Format( message, messageArgs )
			};
		}

		protected void AddMissingImageFileErrors( params ElementAttribute[] elementAttributes )
		{
			foreach ( var ea in elementAttributes )
			{
				AddMissingFileErrors(
					doc.Root.Descendants( ea.Element, true ).Attributes( ea.Attribute, true ),
					resources.Files.Where( p => p.Type == FileResourceType.Image )
				);
			}
		}

		protected void AddMissingImageOrSpriteFileErrors( params ElementAttribute[] elementAttributes )
		{
			foreach ( var ea in elementAttributes )
			{
				AddMissingFileErrors(
					doc.Root.Descendants( ea.Element, true ).Attributes( ea.Attribute, true ),
					resources.Files.Where( p =>
						p.Type == FileResourceType.Image
						|| p.Type == FileResourceType.Sprite
						|| p.Type == FileResourceType.XmlSprite
					)
				);
			}
		}

		protected void AddMissingFileErrors( IEnumerable<XAttribute> attributes, IEnumerable<FileResource> files )
		{
			//join the elements in the XML file to the known resources, find values that have no matching resource.
			var missingFiles =
				from attribute in attributes
				join file in files
				on attribute.Value.ToLower() equals file.Path.ToLower() into fileGroup
				from file in fileGroup.DefaultIfEmpty()
				where file == null
				select attribute;

			result.Errors.AddRange(
				missingFiles.Select(
					attribute => CreateError(
						attribute,
						"The file, '{0}', was not found.",
						attribute.Value
					)
				)
			);

			//If this is an expansion file or the base game, validate that it only references itself and the base game files.
			if ( isCoreGame )
			{
				var expansionIssueFiles =
					from attribute in attributes.Where( p => !missingFiles.Any( q => q == p ) )
					join file in files.Where( p => p.ExpansionNumber == 0 || p.ExpansionNumber == this.expansionNumber )
					on attribute.Value.ToLower() equals file.Path.ToLower() into fileGroup
					from file in fileGroup.DefaultIfEmpty()
					where file == null
					select attribute;

				AddResourceExpansionErrors( expansionIssueFiles, "file" );
			}
			else
			{
				var expansionFiles = files.Where( p => p.ExpansionNumber != 0 && p.ExpansionNumber != this.expansionNumber );
				var baseOrModFiles = files.Where( p => p.ExpansionNumber == 0 || p.ExpansionNumber == this.expansionNumber );

				//inner join attributes to expansion files to find matches. Then left join those to base or mod files to remove
				//ones that overlap. The remaining expansion files are actually used. If some of *them* overlap then add both
				//as used. Later we can figure out which expansions are actually required.
				var usedExpansionFiles =
					from attribute in attributes
					join expansionFile in expansionFiles
					on attribute.Value.ToLower() equals expansionFile.Path.ToLower()
					join baseOrModFile in baseOrModFiles
					on expansionFile.Path.ToLower() equals baseOrModFile.Path.ToLower() into baseOrModFileGroup
					from baseOrModFile in baseOrModFileGroup.DefaultIfEmpty()
					where baseOrModFile == null
					select expansionFile;

				this.expansionResourcesUsed.Files.AddRange( usedExpansionFiles.Distinct() );				
			}
		}

		private void AddResourceExpansionErrors( IEnumerable<XAttribute> expansionIssueFiles, string resourceTypeName )
		{
			result.Errors.AddRange(
				expansionIssueFiles.Select(
					attribute => CreateError(
						attribute,
						"The {0}, '{1}', was found, but this is {2} and it was found in a different expansion.",
						resourceTypeName,
						attribute.Value,
						( this.expansionNumber == 0 ) ? "the base game" : "expansion " + this.expansionNumber
					)
				)
			);
		}

		protected void AddAnimationErrors( params AnimationElementAttribute[] elementAttributes )
		{
			foreach ( var ea in elementAttributes )
			{
				AddAnimationErrors(
					doc.Root.Descendants( ea.Element, true ).Attributes( ea.Attribute, true ),
					ea.FrameCountAttribute
				);
			}
		}

		protected void AddAnimationErrors( IEnumerable<XAttribute> attributes, string frameCountAttributeName )
		{
			var missingAnimationAttributes =
				from attribute in attributes
				join anim in resources.Animations
				on attribute.Value.ToLower() equals anim.Name.ToLower() into animGroup
				from anim in animGroup.DefaultIfEmpty()
				where anim == null
				select attribute;

			result.Errors.AddRange(
				missingAnimationAttributes.Select(
					attribute => CreateError(
						attribute,
						"The animation, '{0}', was not found.",
						attribute.Value
					)
				)
			);

			var foundAnimations =
				from attribute in attributes
				join anim in resources.Animations
				on attribute.Value.ToLower() equals anim.Name.ToLower()
				select new
				{
					RequestedCount = attribute.Parent.GetIntAttribute( frameCountAttributeName ),
					ActualCount = anim.FrameCount,
					CountAttribute = attribute.Parent.Attribute( frameCountAttributeName, true ),
					AnimationName = anim.Name
				};

			result.Errors.AddRange(
				foundAnimations
				.Where( p => p.RequestedCount > p.ActualCount )
				.Select(
					p => CreateError(
						p.CountAttribute,
						"The animation, '{0}', only has {1} frames, but a count of {2} was specified.",
						p.AnimationName,
						p.ActualCount,
						p.RequestedCount
					)
				)
			);

			//If this is an expansion file or the base game, validate that it only references itself and the base game files.
			if ( isCoreGame )
			{
				var expansionIssueFiles =
					from attribute in attributes.Where( p => !missingAnimationAttributes.Any( q => q == p ) )
					join anim in resources.Animations.Where( p => p.ExpansionNumber == 0 || p.ExpansionNumber == this.expansionNumber )
					on attribute.Value.ToLower() equals anim.Name.ToLower() into animGroup
					from anim in animGroup.DefaultIfEmpty()
					where anim == null
					select attribute;

				AddResourceExpansionErrors( expansionIssueFiles, "animation" );
			}
			else
			{
				var expansionAnimations = resources.Animations.Where( p => p.ExpansionNumber != 0 && p.ExpansionNumber != this.expansionNumber );
				var baseOrModAnimations = resources.Animations.Where( p => p.ExpansionNumber == 0 || p.ExpansionNumber == this.expansionNumber );

				//inner join attributes to expansion resources to find matches. Then left join those to base or mod resources to remove
				//ones that overlap. The remaining expansion resources are actually used. If some of *them* overlap then add both
				//as used. Later we can figure out which expansions are actually required.
				var usedExpansionAnimations =
					from attribute in attributes
					join expansionAnimation in expansionAnimations
					on attribute.Value.ToLower() equals expansionAnimation.Name.ToLower()
					join baseOrModAnimation in baseOrModAnimations
					on expansionAnimation.Name.ToLower() equals baseOrModAnimation.Name.ToLower() into baseOrModFileGroup
					from baseOrModAnimation in baseOrModFileGroup.DefaultIfEmpty()
					where baseOrModAnimation == null
					select expansionAnimation;

				this.expansionResourcesUsed.Animations.AddRange( usedExpansionAnimations.Distinct() );
			}
		}

		protected void AddMissingCraftErrors( params ElementAttribute[] elementAttributes )
		{
			foreach ( var ea in elementAttributes )
			{
				AddMissingRelationshipErrors(
					doc.Root.Descendants( ea.Element, true ).Attributes( ea.Attribute, true ),
					resources.Crafts,
					"craft item"
				);
			}
		}

		protected void AddMissingItemErrors( params ElementAttribute[] elementAttributes )
		{
			foreach ( var ea in elementAttributes )
			{
				AddMissingRelationshipErrors(
					doc.Root.Descendants( ea.Element, true ).Attributes( ea.Attribute, true ),
					resources.Items,
					"item"
				);
			}
		}

		protected void AddMissingMonsterErrors( params ElementAttribute[] elementAttributes )
		{
			foreach ( var ea in elementAttributes )
			{
				AddMissingRelationshipErrors(
					doc.Root.Descendants( ea.Element, true ).Attributes( ea.Attribute, true ),
					resources.Monsters,
					"monster"
				);
			}
		}

		protected void AddMissingSoundErrors( params ElementAttribute[] elementAttributes )
		{
			foreach ( var ea in elementAttributes )
			{
				//Oh, you sneaky numbered sounds!
				foreach ( var attribute in doc.Root.Descendants( ea.Element, true ).Attributes( ea.Attribute, true ) )
				{
					bool found = false;
					Regex soundMatcher = new Regex( String.Format( @"^{0}\d*$", attribute.Value ), RegexOptions.IgnoreCase );

					foreach ( var sound in resources.Sounds )
					{
						if ( soundMatcher.IsMatch( sound.Name ) )
						{
							found = true;
							break;
						}
					}

					if ( !found )
					{
						result.Errors.Add(
							CreateError(
								attribute,
								"The sound, '{0}', or a numbered variant of it could not be found.",
								attribute.Value
							)
						);								
					}
				}
			}
		}

		protected void AddMissingSpellErrors( params ElementAttribute[] elementAttributes )
		{
			foreach ( var ea in elementAttributes )
			{
				AddMissingRelationshipErrors(
					doc.Root.Descendants( ea.Element, true ).Attributes( ea.Attribute, true ),
					resources.Spells,
					"spell"
				);
			}
		}

		protected void AddMissingRelationshipErrors( IEnumerable<XAttribute> attributes, IEnumerable<ContentResource> resources, string resourceName, bool validateExpansionNumber = true )
		{
			var missingResourceAttributes =
				from attribute in attributes
				join resource in resources
				on attribute.Value equals resource.Name into resourceGroup
				from resource in resourceGroup.DefaultIfEmpty()
				where resource == null
				select attribute;

			result.Errors.AddRange(
				missingResourceAttributes.Select(
					attribute => CreateError(
						attribute,
						"The {0}, '{1}', was not found.",
						resourceName,
						attribute.Value
					)
				)
			);

			//If this is an expansion file or the base game, validate that it only references itself and the base game files.
			if ( isCoreGame && validateExpansionNumber )
			{
				var expansionIssueFiles =
					from attribute in attributes.Where( p => !missingResourceAttributes.Any( q => q == p ) )
					join resource in resources
					on attribute.Value equals resource.Name into resourceGroup
					from resource in resourceGroup.DefaultIfEmpty()
					where resource == null
					select attribute;

				AddResourceExpansionErrors( expansionIssueFiles, resourceName );
			}
			else if ( !isCoreGame )
			{
				var expansionResources = resources.Where( p => p.ExpansionNumber != 0 && p.ExpansionNumber != this.expansionNumber );
				var baseOrModResources = resources.Where( p => p.ExpansionNumber == 0 || p.ExpansionNumber == this.expansionNumber );

				//inner join attributes to expansion resources to find matches. Then left join those to base or mod resources to remove
				//ones that overlap. The remaining expansion resources are actually used. If some of *them* overlap then add both
				//as used. Later we can figure out which expansions are actually required.
				var usedExpansionResources =
					from attribute in attributes
					join expansionFile in expansionResources
					on attribute.Value.ToLower() equals expansionFile.Name.ToLower()
					join baseOrModFile in baseOrModResources
					on expansionFile.Name.ToLower() equals baseOrModFile.Name.ToLower() into baseOrModFileGroup
					from baseOrModFile in baseOrModFileGroup.DefaultIfEmpty()
					where baseOrModFile == null
					select expansionFile;

				this.expansionResourcesUsed.ContentResources.AddRange( usedExpansionResources.Distinct() );
			}
		}

		protected void AddTriggeredEffectSpellAndTaxaErrors( IEnumerable<XElement> elements )
		{
			var taxaPlusMonsters =
				 resources.Taxa
				 .Concat( resources.Monsters );

			foreach ( var triggerName in DredmorInfo.TriggeredEffectNames )
			{
				AddMissingRelationshipErrors(
					elements.Elements( triggerName, true ).Attributes( "name", true ),
					resources.Spells,
					"spell"
				);

				AddMissingRelationshipErrors(
					elements.Elements( triggerName, true ).Attributes( "taxa", true ),
					taxaPlusMonsters,
					"taxa",
					false
				);
			}
		}

		protected void AddMissingTaxaErrors( params ElementAttribute[] elementAttributes )
		{
			var taxaPlusMonsters =
				 resources.Taxa
				 .Concat( resources.Monsters );

			foreach ( var ea in elementAttributes )
			{
				AddMissingRelationshipErrors(
					doc.Root.Descendants( ea.Element, true ).Attributes( ea.Attribute, true ),
					taxaPlusMonsters,
					"taxa",
					false
				);
			}
		}

		protected void AddMissingPowerErrors( params ElementAttribute[] elementAttributes )
		{
			foreach ( var ea in elementAttributes )
			{
				AddMissingRelationshipErrors(
					doc.Root.Descendants( ea.Element, true ).Attributes( ea.Attribute, true ),
					resources.Powers,
					"power"
				);
			}
		}

		/// <summary>
		/// Finds all attributes of elements where some other attribute has one of a number of values.
		/// </summary>
		/// <param name="ea">The <see cref="ElementAttribute"/> expressing the attributes you want to select</param>
		/// <param name="relatedAttributeSelector">A Func that returns the related attribute.</param>
		/// <param name="relatedAttributeValues">A list of values that the related attribute's value must be amongst.</param>
		/// <returns></returns>
		protected IEnumerable<XAttribute> FindAttributesWhereRelatedAttributeMatchesList( ElementAttribute ea, Func<XAttribute, XAttribute> relatedAttributeSelector, params string[] relatedAttributeValues )
		{
			var matchingAttributes =
				doc.Root.Descendants( ea.Element, true )
				.Attributes( ea.Attribute, true )
				.Where(
					p =>
					{
						var type = relatedAttributeSelector.Invoke( p );

						if ( type != null )
						{
							var value = type.Value.ToLower();

							return relatedAttributeValues.Any( q => q.ToLower() == value );
						}

						return false;
					}
				);
			return matchingAttributes;
		}

		/// <summary>
		/// Checks the elements for multiples of the same descendant elements where only one should be allowed.
		/// </summary>
		/// <param name="elements">The elements to check.</param>
		/// <param name="elementNames">An array of descendant element names that only one of each is allowed.</param>
		protected void AddSingletonDecendantErrors( IEnumerable<XElement> elements, params string[] elementNames )
		{
			foreach ( var elementName in elementNames )
			{
				var badOnes =
						elements.Where(
							parent => parent.Descendants()
								.Where( descendant => elementName.ToLower() == descendant.Name.LocalName.ToLower() ).Count() > 1
						);

				foreach ( var badElement in badOnes )
				{
					result.Errors.Add(
						CreateError(
							badElement,
							"The element, '{0}', can only have a one of these descendants: {1}.",
							badElement.Name.LocalName,
							elementName
						)
					);
				}
			}
		}

		/// <summary>
		/// Checks the elements for multiples of the same child elements where only one should be allowed.
		/// </summary>
		/// <param name="elements">The elements to check.</param>
		/// <param name="elementNames">An array of child element names that only one of each is allowed.</param>
		protected void AddSingletonChildErrors(IEnumerable<XElement> elements, params string[] elementNames)
		{
			foreach ( var elementName in elementNames )
			{
				var badOnes =
						elements.Where(
							parent => parent.Elements()
								.Where( child => elementName.ToLower() == child.Name.LocalName.ToLower() ).Count() > 1
						);

				foreach ( var badElement in badOnes )
				{
					result.Errors.Add(
						CreateError(
							badElement,
							"The element, '{0}', can only have a one of these descendants: {1}.",
							badElement.Name.LocalName,
							elementName
						)
					);
				}
			}
		}

		/// <summary>
		/// Checks the elements for the error condition where it has more than one of the specified desendants.
		/// </summary>
		/// <param name="elements"></param>
		/// <param name="descendantElementNames"></param>
		protected void AddMutuallyExclusiveSingletonDecendantErrors( IEnumerable<XElement> elements, params string[] descendantElementNames )
		{
			var badOnes =
				elements.Where(
					parent => parent.Descendants().Where(
						descendant => descendantElementNames.Any(
							descendantElementName => descendantElementName.ToLower() == descendant.Name.LocalName.ToLower()
						)
					).Count() > 1
				);

			string joinedChildElementNames = String.Join( ", ", descendantElementNames );

			foreach ( var badElement in badOnes )
			{
				result.Errors.Add(
					CreateError(
						badElement,
						"The element, '{0}', can only have a one of these descendants: {1}.",
						badElement.Name.LocalName,
						joinedChildElementNames
					)
				);
			}
		}
	}

	/// <summary>
	/// Represents an element and attribute name.
	/// </summary>
	public class ElementAttribute
	{
		public ElementAttribute( string element, string attribute )
		{
			this.Element = element;
			this.Attribute = attribute;
		}

		public string Element { get; set; }
		public string Attribute { get; set; }
	}

	/// <summary>
	/// Represents an element and attribute name along with a frame count.
	/// </summary>
	public class AnimationElementAttribute : ElementAttribute
	{
		public AnimationElementAttribute( string element, string attribute, string frameCountAttribute )
			: base( element, attribute )
		{
			this.FrameCountAttribute = frameCountAttribute;
		}

		public string FrameCountAttribute { get; set; }
	}

	/// <summary>
	/// An implementation of <see cref="XmlResourceValidator"/> for the item file.
	/// </summary>
	public class ItemXmlResourceValidator : XmlResourceValidator
	{
		public ItemXmlResourceValidator( ContentFile file, GameResources resources, GameResources expansionResourcesUsed )
			: base( file, resources, expansionResourcesUsed )
		{
		}

		public override XmlFileValidationErrorList Validate()
		{
			AddMissingImageFileErrors(
				new ElementAttribute( "item", "iconfile" ),
				new ElementAttribute( "toolkit", "missing" ),
				new ElementAttribute( "toolkit", "present" ),
				new ElementAttribute( "toolkit", "active" ),
				new ElementAttribute( "toolkit", "bg" )
			);

			AddMissingSpellErrors(
				new ElementAttribute( "casts", "spell" ),
				new ElementAttribute( "food", "effect" ),
				new ElementAttribute( "potion", "spell" ),
				new ElementAttribute( "trap", "casts" ),
				new ElementAttribute( "wand", "spell" ),
				new ElementAttribute( "weapon", "hit" ),
				new ElementAttribute( "power", "spell" )
			);

			AddMissingImageOrSpriteFileErrors(
				new ElementAttribute( "trap", "origin" ),
				new ElementAttribute( "weapon", "thrown" )
			);

			AddTriggeredEffectSpellAndTaxaErrors(
				doc.Root.Descendants( "item", true )
			);

			AddMutuallyExclusiveSingletonDecendantErrors(
				doc.Root.Descendants( "item", true ),
				"weapon",
				"food",
				"armour",
				"mushroom",
				"gem",
				"toolkit",
				"trap",
				"potion",
				"wand"	
			);

			AddSingletonDecendantErrors(
				doc.Root.Descendants( "item", true ),
				"price",
				"artifact",
				"description",
				"casts"
			);
			
			return result;
		}
	}

	/// <summary>
	/// An implementation of <see cref="XmlResourceValidator"/> for the spell file.
	/// </summary>
	public class SpellXmlResourceValidator : XmlResourceValidator
	{
		public SpellXmlResourceValidator( ContentFile file, GameResources resources, GameResources expansionResourcesUsed )
			: base( file, resources, expansionResourcesUsed )
		{
		}

		public override XmlFileValidationErrorList Validate()
		{
			AddMissingImageFileErrors(
				new ElementAttribute( "spell", "icon" ),
				new ElementAttribute( "spell", "minesprite" ),
				new ElementAttribute( "buff", "icon" ),
				new ElementAttribute( "buff", "smallicon" )
			);

			AddAnimationErrors(
				new AnimationElementAttribute( "spell", "minespritepngseries", "minespritepngnum" ),
				new AnimationElementAttribute( "halo", "name", "frames" ),
				new AnimationElementAttribute( "impact", "sprite", "frames" )
			);

			//Special handler for spell animations because missile-type spells can be PNGs.
			AddSpellAnimationErrors();

			AddMissingSpellErrors(
				new ElementAttribute( "fountain", "name" ),
				new ElementAttribute( "effect", "spell" ),
				new ElementAttribute( "buff", "insufficientFunds" )
			);

			var itemOptions = FindAttributesWhereRelatedAttributeMatchesList(
				new ElementAttribute( "option", "name" ),
				p => p.Parent.Attribute( "type" ),
				"spawnItemFromList"
			);

			AddMissingRelationshipErrors(
				itemOptions,
				resources.Items,
				"item"
			);

			var spellOptions = FindAttributesWhereRelatedAttributeMatchesList(
				new ElementAttribute( "option", "name" ),
				p => p.Parent.Attribute( "type" ),
				"triggerFromList"
			);

			AddMissingRelationshipErrors(
				spellOptions,
				resources.Spells,
				"spell"
			);

			AddMissingMonsterErrors(
				new ElementAttribute( "polymorph", "name" )
			);

			AddTriggeredEffectSpellAndTaxaErrors(
				doc.Root.Descendants( "buff" )
			);

			AddSingletonDecendantErrors(
				doc.Root.Descendants( "spell", true ),
				"anim",
				"buff",
				"requirements",
				"description",
				"ai",
				"impact"
			);

			return result;
		}

		/// <summary>
		/// Spell animations can contain animation sequences or a PNG. PNG is only allowed for missile spells.
		/// </summary>
		private void AddSpellAnimationErrors()
		{
			var spellsWithAnimSprites =
				doc.Root.Descendants( "spell", true )
				.Select( p =>
					{
						var type = p.GetStringAttribute( "type" );
						var anim = p.Descendants( "anim", true ).LastOrDefault();

						XAttribute sprite = null;
						bool spriteIsPng = false;

						if ( anim != null )
						{
							sprite = anim.Attribute( "sprite", true );

							if ( sprite != null )
							{
								spriteIsPng = Path.GetExtension( sprite.Value.ToLower() ) == ".png";
							}
						}

						return new
						{
							Spell = p,
							SpellType = type,
							Anim = anim,
							Sprite = sprite,
							SpriteIsPng = spriteIsPng
						};
					}
				)
				.Where( p => p.Sprite != null );

			var pngMissileSpells = spellsWithAnimSprites.Where( 
				p => 
				p.SpellType != null 
				&& p.SpellType.ToLower() == "missile" 
				&& p.SpriteIsPng
			);
						
			AddMissingFileErrors(
				pngMissileSpells.Select( p => p.Sprite ),
				resources.Files.Where( p => p.Type == FileResourceType.Image )
			);

			var pngNonmissileSpells = spellsWithAnimSprites.Where(
				p =>
				p.SpellType != null
				&& p.SpellType.ToLower() != "missile"
				&& p.SpriteIsPng
			);

			foreach ( var pngNonmissileSpell in pngNonmissileSpells )
			{
				result.Errors.Add(
					CreateError(
						pngNonmissileSpell.Sprite,
						"The animation sprite of a non-missile spell cannot be a PNG."
					)
				);
			};

			var nonPngAnimSpells = spellsWithAnimSprites.Where( 
				p =>
				p.Sprite != null
				&& !p.SpriteIsPng
			);

			AddAnimationErrors(
				nonPngAnimSpells.Select( p => p.Sprite ),
				"frames"
			);
		}	
		
	}

	/// <summary>
	/// An implementation of <see cref="XmlResourceValidator"/> for the monster file.
	/// </summary>
	public class MonsterXmlResourceValidator : XmlResourceValidator
	{
		public MonsterXmlResourceValidator( ContentFile file, GameResources resources, GameResources expansionResourcesUsed )
			: base( file, resources, expansionResourcesUsed )
		{
		}

		public override XmlFileValidationErrorList Validate()
		{
			AddMissingSpellErrors(
				new ElementAttribute( "ondeath", "spell" ),
				new ElementAttribute( "onhit", "spell" ),
				new ElementAttribute( "spell", "spell" ),
				new ElementAttribute( "dash", "hitSpell" ),
				new ElementAttribute( "dash", "missSpell" )
			);

			AddMissingItemErrors(
				new ElementAttribute( "drop", "name" )
			);

			AddMissingSoundErrors(
				new ElementAttribute( "sfx", "attack" ),
				new ElementAttribute( "sfx", "die" ),
				new ElementAttribute( "sfx", "hit" ),
				new ElementAttribute( "sfx", "spell" )
			);

			AddMissingDirectionalSpriteFileErrors(
				"attackSprite",
				"beamSprite",
				"hitSprite",
				"idleSprite"
			);

			AddMissingSpriteFileErrors(
				new ElementAttribute( "castSpellSprite", "name" ),
				new ElementAttribute( "dieSprite", "name" ),
				new ElementAttribute( "morphsprites", "drinksprite" ),
				new ElementAttribute( "morphsprites", "eatsprite" ),
				new ElementAttribute( "morphsprites", "levelupfsprite" ),
				new ElementAttribute( "morphsprites", "levelupmsprite" ),
				new ElementAttribute( "morphsprites", "longidlesprite" ),
				new ElementAttribute( "morphsprites", "vanishsprite" )
			);

			AddMissingFileErrors(
				doc.Root.Descendants( "palette", true ).Attributes( "name" ),
				resources.Files.Where( p => p.Type == FileResourceType.Palette )
			);

			AddSingletonChildErrors(
				doc.Root.Descendants( "monster", true ),
				"idlesprite",
				"attackSprite",
				"hitSprite",
				"beamSprite",
				"castSpellSprite",
				"morphSprites",
				"dieSprite",
				"sfx",
				"ai",
				"stats",
				"damage",
				"resistances",
				"info",
				"palette"
			);

			return result;
		}

		private void AddMissingDirectionalSpriteFileErrors( params string[] elementNames )
		{
			string[] directions = 
			{
				"up",
				"down",
				"left",
				"right"
			};

			foreach ( var elementName in elementNames )
			{
				AddMissingSpriteFileErrors( 
					directions.Select( p => new ElementAttribute( elementName, p ) )
					.ToArray()
				);
			}
		}

		protected void AddMissingSpriteFileErrors( params ElementAttribute[] elementAttributes )
		{
			foreach ( var ea in elementAttributes )
			{
				AddMissingFileErrors(
					doc.Root.Descendants( ea.Element, true ).Attributes( ea.Attribute, true ),
					resources.Files.Where( p => p.Type == FileResourceType.XmlSprite || p.Type == FileResourceType.Sprite )
				);
			}
		}
	}

	/// <summary>
	/// An implementation of <see cref="XmlResourceValidator"/> for the skill file.
	/// </summary>
	public class SkillXmlResourceValidator : XmlResourceValidator
	{
		public SkillXmlResourceValidator( ContentFile file, GameResources resources, GameResources expansionResourcesUsed )
			: base( file, resources, expansionResourcesUsed )
		{
		}

		public override XmlFileValidationErrorList Validate()
		{
			AddMissingImageFileErrors(
				new ElementAttribute( "art", "icon" ),
				new ElementAttribute( "ability", "icon" )
			);

			AddMissingItemErrors(
				new ElementAttribute( "ability", "giveitem" ),
				new ElementAttribute( "loadout", "subtype" )
			);

			AddMissingCraftErrors(
				new ElementAttribute( "ability", "learnrecipe" )
			);

			AddMissingTaxaErrors(
				new ElementAttribute( "flags", "friendlyTaxa" )
			);

			AddMissingSpellErrors(
				new ElementAttribute( "spell", "name" )
			);

			AddMissingSkillErrors(
				new ElementAttribute( "ability", "skill" ),
				new ElementAttribute( "skill", "id" )
			);

			AddMissingSkillErrors(
				new ElementAttribute( "ability", "skillname" ),
				new ElementAttribute( "skill", "skillname" )
			);

			AddSingletonDecendantErrors(
				doc.Root.Descendants( "skill", true ),
				"art",
				"flags"
			);

			AddSingletonDecendantErrors(
				doc.Root.Descendants( "ability", true ),
				"description",
				"flags",
				"spell",
				"resistBuff",
				"damageBuff",
				"recoveryBuff"
			);

			return result;
		}

		private void AddMissingSkillErrors( ElementAttribute abilitySelector, ElementAttribute skillSelector )
		{
			var abilitySkillIDs = 
				doc.Root.Descendants( abilitySelector.Element, true )
				.Attributes( abilitySelector.Attribute, true );
			
			var skillIDs = 
				doc.Root.Descendants( skillSelector.Element, true )
				.Attributes( skillSelector.Attribute, true );

			var missingSkills =
				from abilitySkill in abilitySkillIDs
				join skill in skillIDs
				on abilitySkill.Value equals skill.Value into skillGroup
				from skill in skillGroup.DefaultIfEmpty()
				where skill == null
				select abilitySkill;

			result.Errors.AddRange(
				missingSkills.Select(
					attribute => CreateError(
						attribute,
						"The skill with {0} '{1}' was not found.",
						skillSelector.Attribute,
						attribute.Value
					)
				)
			);
		}
	}

	/// <summary>
	/// An implementation of <see cref="XmlResourceValidator"/> for the craft file.
	/// </summary>
	public class CraftXmlResourceValidator : XmlResourceValidator
	{
		public CraftXmlResourceValidator( ContentFile file, GameResources resources, GameResources expansionResourcesUsed )
			: base( file, resources, expansionResourcesUsed )
		{
		}

		public override XmlFileValidationErrorList Validate()
		{
			AddMissingItemErrors(
				new ElementAttribute( "input", "name" ),
				new ElementAttribute( "output", "name" )
			);

			AddSingletonDecendantErrors(
				doc.Root.Descendants( "craft", true ),
				"tool"
			);

			return result;
		}
	}

	/// <summary>
	/// An implementation of <see cref="XmlResourceValidator"/> for the room file.
	/// </summary>
	public class RoomXmlResourceValidator : XmlResourceValidator
	{
		public RoomXmlResourceValidator( ContentFile file, GameResources resources, GameResources expansionResourcesUsed )
			: base( file, resources, expansionResourcesUsed )
		{
		}

		public override XmlFileValidationErrorList Validate()
		{
			AddMissingImageOrSpriteFileErrors(
				new ElementAttribute( "customblocker", "png" ),
				new ElementAttribute( "custombreakable", "png" ),
				new ElementAttribute( "custombreakable", "broken" ),
				new ElementAttribute( "customengraving", "png" )
			);

			AddAnimationErrors(
				new AnimationElementAttribute( "customblocker", "pngsprite", "pngnum" ),
				new AnimationElementAttribute( "customengraving", "pngsprite", "pngnum" ),
				new AnimationElementAttribute( "action", "pngsprite", "pngnum" )
			);

			AddMissingMonsterErrors(
				new ElementAttribute( "horde", "name" ),
				new ElementAttribute( "monster", "name" )
			);

			AddMissingItemErrors(
				new ElementAttribute( "loot", "subtype" ),
				new ElementAttribute( "trap", "name" )
			);

			AddMissingSpellErrors(
				new ElementAttribute( "action", "casts" )
			);

			var namesForMonsterCreateActions = FindAttributesWhereRelatedAttributeMatchesList(
				new ElementAttribute( "action", "name" ),
				p => p.Parent.Attribute( "create_type" ),
				"monster",
				"horde"
			);

			AddMissingRelationshipErrors(
				namesForMonsterCreateActions,
				resources.Monsters,
				"monster"
			);

			var namesForLootCreateActions = FindAttributesWhereRelatedAttributeMatchesList(
				new ElementAttribute( "action", "subtype" ),
				p => p.Parent.Attribute( "create_type" ),
				"loot"
			);

			AddMissingRelationshipErrors(
				namesForLootCreateActions,
				resources.Items,
				"item"
			);

			AddSingletonDecendantErrors(
				doc.Root.Descendants( "room", true ),
				"flags"
			);		

			return result;
		}
	}

	/// <summary>
	/// An implementation of <see cref="XmlResourceValidator"/> for the soundfx file.
	/// </summary>
	public class SoundXmlResourceValidator : XmlResourceValidator
	{
		public SoundXmlResourceValidator( ContentFile file, GameResources resources, GameResources expansionResourcesUsed )
			: base( file, resources, expansionResourcesUsed )
		{
		}

		public override XmlFileValidationErrorList Validate()
		{
			AddMissingFileErrors(
				doc.Root.Elements( "sound", true ).Attributes( "wave", true ),
				resources.Files.Where( p => p.Type == FileResourceType.Wave )
			);

			return result;
		}
	}

	public class EncrustXmlResourceValidator : XmlResourceValidator
	{
		public EncrustXmlResourceValidator( ContentFile file, GameResources resources, GameResources expansionResourcesUsed )
			: base( file, resources, expansionResourcesUsed )
		{
		}

		public override XmlFileValidationErrorList Validate()
		{
			AddMissingPowerErrors(
				new ElementAttribute( "power", "name" )
			);

			AddMissingItemErrors(
				new ElementAttribute( "input", "name" )
			);

			AddSingletonDecendantErrors(
				doc.Root.Descendants( "encrust", true ),
				"description",
				"tool",
				"skill",
				"damageBuff",
				"resistBuff",
				"instability"
			);

			return result;
		}
	}
}
