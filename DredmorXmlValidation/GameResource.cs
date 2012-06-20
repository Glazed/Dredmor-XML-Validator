using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DredmorUtilities;
using System.Collections;

namespace DredmorXmlValidation
{
	/// <summary>
	/// A class which contains representations of XML content (references) and physical files (resources). 
	/// This class is XML-serialized into the resource manifest.
	/// </summary>
	public class GameResources
	{
		List<ContentResource> contentResources = new List<ContentResource>();
		List<FileResource> files = new List<FileResource>();
		List<AnimationResource> animations = new List<AnimationResource>();

		#region Properties

		/// <summary>
		/// XML content such as items, spells, and abilities.
		/// </summary>
		public List<ContentResource> ContentResources
		{
			get
			{
				return contentResources;
			}
		}				

		/// <summary>
		/// Physical resource files.
		/// </summary>
		public List<FileResource> Files
		{
			get
			{
				return files;
			}
		}

		/// <summary>
		/// Physical animation sequences.
		/// </summary>
		public List<AnimationResource> Animations
		{
			get
			{
				return animations;
			}
		}

		/// <summary>
		/// A filter on <see cref="ContentResources"/> for crafts.
		/// </summary>
		[XmlIgnore]
		public ContentResourceFilter Crafts
		{
			get
			{
				return new ContentResourceFilter( contentResources, ContentResourceType.Craft );
			}
		}

		/// <summary>
		/// A filter on <see cref="ContentResources"/> for items.
		/// </summary>
		[XmlIgnore]
		public ContentResourceFilter Items
		{
			get
			{
				return new ContentResourceFilter( contentResources, ContentResourceType.Item );
			}
		}

		/// <summary>
		/// A filter on <see cref="ContentResources"/> for monsters.
		/// </summary>
		[XmlIgnore]
		public ContentResourceFilter Monsters
		{
			get
			{
				return new ContentResourceFilter( contentResources, ContentResourceType.Monster );
			}
		}

		/// <summary>
		/// A filter on <see cref="ContentResources"/> for sounds.
		/// </summary>
		[XmlIgnore]
		public ContentResourceFilter Sounds
		{
			get
			{
				return new ContentResourceFilter( contentResources, ContentResourceType.Sound );
			}
		}

		/// <summary>
		/// A filter on <see cref="ContentResources"/> for spells.
		/// </summary>
		[XmlIgnore]
		public ContentResourceFilter Spells
		{
			get
			{
				return new ContentResourceFilter( contentResources, ContentResourceType.Spell );
			}
		}

		/// <summary>
		/// A filter on <see cref="ContentResources"/> for taxa.
		/// </summary>
		[XmlIgnore]
		public ContentResourceFilter Taxa
		{
			get
			{
				return new ContentResourceFilter( contentResources, ContentResourceType.Taxa );
			}
		}

		#endregion

		/// <summary>
		/// Loads all references and resources from the specified source.
		/// </summary>
		/// <param name="source"></param>
		public void LoadResources( ContentDirectory source )
		{
			PopulateResources( source );
		}

		private void PopulateResources( ContentDirectory source )
		{
			PopulatePhysicalResources( source );
			PopulateXmlResources( source );

			if ( source.IsCoreGame )
			{
				AddHardCodedResources();
			}
		}

		/// <summary>
		/// Adds hard-coded game items.
		/// </summary>
		private void AddHardCodedResources()
		{
			contentResources.Add( 
				new ContentResource
				{
					Name = "lockpick",
					ExpansionNumber = 0,
					Type = ContentResourceType.Item
				}
			);
		}

		#region PopulatePhysicalResources

		private void PopulatePhysicalResources( ContentDirectory source )
		{
			AddFileResources( source, "png", FileResourceType.Image );
			AddFileResources( source, "spr", FileResourceType.Sprite );
			AddFileResources( source, "wav", FileResourceType.Wave );
			AddFileResources( source, "pal", FileResourceType.Palette );
			AddXmlSpriteResources( source );
			AddAnimationResources( source );
		}

		private void AddXmlSpriteResources( ContentDirectory source )
		{
			//This ignores the "DB" XML files from the search and considers all other XMl files as sprite files.
			var xmlSpriteFiles =
				source.EnumerateFiles( "*.xml", SearchOption.AllDirectories )
				.Where( p => !DredmorInfo.AllContentXmlFiles.Any( q => q == p.Name.ToLower() ) );

			foreach ( var file in xmlSpriteFiles )
			{
				AddFileResource( source, FileResourceType.XmlSprite, file );
			}
		}

		private void AddAnimationResources( ContentDirectory source )
		{
			var animationFirstFrames =
				source.EnumerateFiles( "*0000.png", SearchOption.AllDirectories );

			foreach ( var file in animationFirstFrames )
			{
				this.Animations.Add(
					new AnimationResource
					{
						Name = file.SourceRelativePath.Replace( "0000.png", String.Empty ).ToLower().ToForwardSlashes(),
						FrameCount = GetAnimationFrameCount( file ),
						ExpansionNumber = file.ExpansionNumber
					}
				);
			}
		}

		private int GetAnimationFrameCount( ContentFile file )
		{
			var files = file.Directory.EnumerateFiles( "*.png", SearchOption.TopDirectoryOnly )
				.Where( p => Regex.IsMatch( p.Name, "^" + file.Name.Replace( "0000.png", String.Empty ) + @"\d{4}.png$", RegexOptions.IgnoreCase ) );

			return files.Count();
		}

		private void AddFileResources( ContentDirectory source, string extension, FileResourceType type )
		{
			foreach ( var file in source.EnumerateFiles( String.Format( "*.{0}", extension ), SearchOption.AllDirectories ) )
			{
				AddFileResource( source, type, file );
			}
		}

		private void AddFileResource( ContentDirectory source, FileResourceType type, ContentFile file )
		{
			this.Files.Add(
				new FileResource
				{
					Path = file.SourceRelativePath.ToForwardSlashes(),
					ExpansionNumber = file.ExpansionNumber,
					Type = type
				}
			);
		} 

		#endregion

		#region PopulateXmlResources

		private void PopulateXmlResources( ContentDirectory source )
		{
			var files = source.EnumerateXmlFiles( DredmorInfo.XmlFilesToLoadResources, SearchOption.AllDirectories );

			foreach ( var file in files )
			{
				XDocument doc = XDocument.Load( file.OpenRead() );

				switch ( file.Name.ToLower() )
				{
					case "craftdb.xml":
						PopulateCraftResources( doc, file.ExpansionNumber );
						break;
					case "itemdb.xml":
						PopulateItemResources( doc, file.ExpansionNumber );
						break;
					case "mondb.xml":
						PopulateMonsterResources( doc, file.ExpansionNumber );
						PopulateTaxaResources( doc, file.ExpansionNumber );
						break;
					case "soundfx.xml":
						PopulateSfxResources( doc, file.ExpansionNumber );
						break;
					case "spelldb.xml":
						PopulateSpellResources( doc, file.ExpansionNumber );
						break;
				}
			}
		}

		private void PopulateSpellResources( XDocument doc, int expansionNumber )
		{
			var names =
				doc.Root.Descendants( "spell", true )
				.Select( p => p.GetStringAttribute( "name" ) )
				.Distinct()
				.Where( p => !String.IsNullOrWhiteSpace( p ) );

			foreach ( var name in names )
			{
				this.contentResources.Add(
					new ContentResource
					{
						Name = name,
						ExpansionNumber = expansionNumber,
						Type = ContentResourceType.Spell
					}
				);
			}
		}

		private void PopulateSfxResources( XDocument doc, int expansionNumber )
		{
			var names =
				doc.Root.Descendants( "sound", true )
				.Select( p => p.GetStringAttribute( "name" ) )
				.Distinct()
				.Where( p => !String.IsNullOrWhiteSpace( p ) );

			foreach ( var name in names )
			{
				this.contentResources.Add(
					new ContentResource
					{
						Name = name,
						ExpansionNumber = expansionNumber,
						Type = ContentResourceType.Sound
					}
				);
			}
		}

		private void PopulateMonsterResources( XDocument doc, int expansionNumber )
		{
			var names =
				doc.Root.Descendants( "monster", true )
				.Select( p => p.GetStringAttribute( "name" ) )
				.Distinct()
				.Where( p => !String.IsNullOrWhiteSpace( p ) );

			foreach ( var name in names )
			{
				this.contentResources.Add(
					new ContentResource
					{
						Name = name,
						ExpansionNumber = expansionNumber,
						Type = ContentResourceType.Monster
					}
				);
			}
		}

		private void PopulateTaxaResources( XDocument doc, int expansionNumber )
		{
			var taxa =
				doc.Root.Descendants( "monster", true )
				.Select( p => p.GetStringAttribute( "taxa" ) )
				.Distinct()
				.Where( p => !String.IsNullOrWhiteSpace( p ) );

			foreach ( var taxon in taxa )
			{
				this.contentResources.Add(
					new ContentResource
					{
						Name = taxon,
						ExpansionNumber = expansionNumber,
						Type = ContentResourceType.Taxa
					}
				);
			}
		}

		private void PopulateItemResources( XDocument doc, int expansionNumber )
		{
			var names =
				doc.Root.Elements( "item", true )
				.Select( p => p.GetStringAttribute( "name" ) )
				.Distinct()
				.Where( p => !String.IsNullOrWhiteSpace( p ) );

			foreach ( var name in names )
			{
				this.contentResources.Add(
					new ContentResource
					{
						Name = name,
						ExpansionNumber = expansionNumber,
						Type = ContentResourceType.Item
					}
				);
			}
		}

		private void PopulateCraftResources( XDocument doc, int expansionNumber )
		{
			var names =
				doc.Root.Elements( "craft", true ).Elements( "output", true )
				.Select( p => p.GetStringAttribute( "name" ) )
				.Distinct()
				.Where( p => !String.IsNullOrWhiteSpace( p ) );

			foreach ( var name in names )
			{
				this.contentResources.Add(
					new ContentResource
					{
						Name = name,
						ExpansionNumber = expansionNumber,
						Type = ContentResourceType.Craft
					}
				);
			}
		} 
		#endregion

		#region Serialization
		public string Serialize()
		{
			StringBuilder sb = new StringBuilder();
			XmlWriter writer = XmlWriter.Create( sb );

			XmlSerializer serializer = new XmlSerializer( typeof( GameResources ) );
			serializer.Serialize( writer, this );

			return sb.ToString();
		}

		public void SerializeToFile( string path )
		{
			var output = File.Open( path, FileMode.Create );
			XmlWriter writer = XmlWriter.Create( output );

			XmlSerializer serializer = new XmlSerializer( typeof( GameResources ) );
			serializer.Serialize( writer, this );
		}

		public static GameResources Deserialize( string xml )
		{
			StringReader reader = new StringReader( xml );

			XmlSerializer serializer = new XmlSerializer( typeof( GameResources ) );
			var resources = (GameResources) serializer.Deserialize( reader );

			return resources;
		} 
		#endregion
	}

	/// <summary>
	/// Base class for all game resources and XML content.
	/// </summary>
	public class GameResource
	{
		public int ExpansionNumber { get; set; }
	}

	/// <summary>
	/// An XML content resource like an item or spell.
	/// </summary>
	public class ContentResource : GameResource
	{
		public string Name { get; set; }
		public ContentResourceType Type { get; set; }

		public override string ToString()
		{
			return String.Format( "Exp {0} {1}: {2}", ExpansionNumber, Type, Name );
		}
	}

	/// <summary>
	/// A physical file resource.
	/// </summary>
	public class FileResource : GameResource
	{
		public string Path { get; set; }
		public FileResourceType Type { get; set; }

		public override string ToString()
		{
			return String.Format( "Exp {0} {1}: {2}", ExpansionNumber, Type, Path );
		}
	}

	/// <summary>
	/// An animation sequence resource.
	/// </summary>
	public class AnimationResource : GameResource
	{
		public string Name { get; set; }
		public int FrameCount { get; set; }

		public override string ToString()
		{
			return String.Format( "Exp {0}: {1}; Frames: {2}", ExpansionNumber, Name, FrameCount );
		}
	}

	/// <summary>
	/// The type of physical file resource.
	/// </summary>
	public enum FileResourceType
	{
		Wave,
		Image,
		Sprite,
		Animation,
		XmlSprite,
		Palette
	}

	/// <summary>
	/// The type of XML content.
	/// </summary>
	public enum ContentResourceType
	{
		Item,
		Monster,
		Taxa,
		Spell,
		Craft,
		Sound
	}

	/// <summary>
	/// An enumerable filter for content resources.
	/// </summary>
	public class ContentResourceFilter : IEnumerable<ContentResource>
	{
		private ContentResourceType _type;
		private List<ContentResource> _resources;

		public ContentResourceFilter( List<ContentResource> resources, ContentResourceType type )
		{
			_type = type;
			_resources = resources;
		}

		public IEnumerator<ContentResource> GetEnumerator()
		{
			return _resources.Where( p => p.Type == _type ).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _resources.Where( p => p.Type == _type ).GetEnumerator();
		}
	}
}
