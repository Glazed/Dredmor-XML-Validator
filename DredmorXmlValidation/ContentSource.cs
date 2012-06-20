using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ionic.Zip;
using System.Text.RegularExpressions;
using DredmorUtilities;

namespace DredmorXmlValidation
{
    /// <summary>
    /// An abstract base class to encapsulate directory operations. Used to simplify
    /// the differences between accessing files and subdirectories on physical disks
    /// and zip files.
    /// </summary>
    /// <remarks>
    /// When a mod zip is laoded, it's considered a 'directory' with a path of an empty string.
    /// </remarks>
	public abstract class ContentDirectory
	{
        /// <summary>
        /// If true, this directory is part of the base game or an expansion, not a mod.
        /// </summary>
		public bool IsCoreGame { get; set; }

		protected ContentDirectory()
		{

		}

		protected ContentDirectory( bool isCoreGame )
		{
			this.IsCoreGame = isCoreGame;
		}

        /// <summary>
        /// Factory method for generating a ContentDirectory from a path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isCoreGame"></param>
        /// <returns></returns>
		public static ContentDirectory Create( string path, bool isCoreGame )
		{
			FileInfo file = new FileInfo( path );

			if ( file.Exists && file.Extension.ToLower() == ".zip" )
			{
				ZipFile zip = ZipFile.Read( path );

				return new ZipFileDirectory( zip, String.Empty, isCoreGame );
			}
			else if ( Directory.Exists( path ) )
			{
				DirectoryInfo directory = new DirectoryInfo( path );
				return new FileSystemDirectory( directory, path, isCoreGame );
			}
			else
			{
				throw new Exception( "The path was not a valid zip file or directory." );
			}
		}

        /// <summary>
        /// Enumerate all files in the directory, and optionally all subdirectories.
        /// </summary>
        /// <param name="searchOption"></param>
        /// <returns></returns>
		public abstract IEnumerable<ContentFile> EnumerateFiles( SearchOption searchOption );
		
        /// <summary>
        /// Enumerate all files in the directory that match the specified pattern, and optionally all subdirectories.
        /// </summary>
        /// <param name="pattern">A string which can include the * and ? wildcards.</param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public abstract IEnumerable<ContentFile> EnumerateFiles( string pattern, SearchOption searchOption );
		
        /// <summary>
        /// Enumerate all XML files in the directory whose name is contained in the filenames array, and optionally all subdirectories.
        /// </summary>
        /// <param name="filenames"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public abstract IEnumerable<ContentFile> EnumerateXmlFiles( string[] filenames, SearchOption searchOption );
		
        /// <summary>
        /// Gets a specific file with a path relative to this directory.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public abstract ContentFile GetFile( string relativePath );

        /// <summary>
        /// Gets a specific directory with a path relative to this directory.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
		public abstract ContentDirectory GetDirectory( string relativePath );

        /// <summary>
        /// The full path of this directory.
        /// </summary>
		public abstract string FullName { get; }

        /// <summary>
        /// The path relative to the source this directory came from.
        /// </summary>
		public abstract string SourceRelativePath { get; }

        /// <summary>
        /// The name of the directory.
        /// </summary>
		public abstract string Name { get; }
	}

    /// <summary>
    /// An abstract base class that represents a file in a <see cref="ContentDirectory"/>.
    /// </summary>
	public abstract class ContentFile
	{
        /// <summary>
        /// Opens the file as a stream for reading.
        /// </summary>
        /// <returns></returns>
		public abstract Stream OpenRead();

        /// <summary>
        /// Opens the file as text.
        /// </summary>
        /// <returns></returns>
		public abstract StreamReader OpenText();

        /// <summary>
        /// The full path and name of the file.
        /// </summary>
		public abstract string FullName { get; }

        /// <summary>
        /// The path relative to the source this file came from.
        /// </summary>
		public abstract string SourceRelativePath { get; }

        /// <summary>
        /// The 'expansion' number of this file. The base game is 0, expansions are 1 or higher. Mods sources are set to 100.
        /// </summary>
		public abstract int ExpansionNumber { get; }

        /// <summary>
        /// The name of this file.
        /// </summary>
		public abstract string Name { get; }

        /// <summary>
        /// The directory which contains this file.
        /// </summary>
		public abstract ContentDirectory Directory { get; }
	}

    /// <summary>
    /// An implementation of <see cref="ContentDirectory"/> for physical directories.
    /// </summary>
	public class FileSystemDirectory : ContentDirectory
	{
		private DirectoryInfo _directory;
		private string _sourceRelativePath;
		private string _sourcePath;

		public FileSystemDirectory( DirectoryInfo directory, string sourcePath, bool isCoreGame )
			: base( isCoreGame )
		{
			_directory = directory;
			_sourcePath = sourcePath;
			_sourceRelativePath = directory.FullName.Replace( sourcePath, String.Empty );

			if ( _sourceRelativePath.StartsWith( "\\" ) )
			{
				_sourceRelativePath = _sourceRelativePath.Substring( 1 );
			}
		}

		public override IEnumerable<ContentFile> EnumerateFiles( SearchOption searchOption )
		{
			return EnumerateFiles( "*", searchOption );
		}

		public override IEnumerable<ContentFile> EnumerateXmlFiles( string[] filenames, SearchOption searchOption )
		{
			return EnumerateFiles( "*.xml", searchOption )
				.Where( p => filenames.Any( q => q.ToLower() == p.Name.ToLower() ) );
		}

		public override IEnumerable<ContentFile> EnumerateFiles( string pattern, SearchOption searchOption )
		{
			return _directory.EnumerateFiles( pattern, searchOption )
				.Select( p => new FileSystemFile( p, new FileSystemDirectory( p.Directory, _sourcePath, IsCoreGame ) ) );
		}

		public override ContentFile GetFile( string relativePath )
		{
			FileInfo file = new FileInfo( Path.Combine( this.FullName, relativePath ) );

			if ( file.Exists )
			{
				return new FileSystemFile( file, new FileSystemDirectory( file.Directory, _sourcePath, IsCoreGame ) );
			}
			else
			{
				return null;
			}
		}

		public override ContentDirectory GetDirectory( string relativePath )
		{
			relativePath = relativePath.ToBackSlashes();

			DirectoryInfo dir = new DirectoryInfo( Path.Combine( this.FullName, relativePath ) );

			if ( dir.Exists )
			{
				return new FileSystemDirectory( dir, _sourcePath, IsCoreGame );
			}
			else
			{
				return null;
			}
		}

		public override string FullName
		{
			get
			{
				return _directory.FullName;
			}
		}

		public override string Name
		{
			get
			{
				return _directory.Name;
			}
		}

		public override string SourceRelativePath
		{
			get
			{
				return _sourceRelativePath;
			}
		}

		public override string ToString()
		{
			return _sourceRelativePath;
		}
	}

    /// <summary>
    /// An implementation of <see cref="ContentFile"/> for physical files.
    /// </summary>
	public class FileSystemFile : ContentFile
	{
		private FileInfo _file;
		private string _relativePath;
		private int _expansionNumber;
		private FileSystemDirectory _directory;

		public FileSystemFile( FileInfo file, FileSystemDirectory directory )
		{
			_file = file;
			_directory = directory;

			_relativePath = Path.Combine( directory.SourceRelativePath, file.Name );

            //determine if this file is in one of the expansion folders.
			if ( directory.IsCoreGame && _relativePath.ToLower().StartsWith( "expansion" ) )
			{
				Regex r = new Regex( @"expansion(\d*)\\(.*)", RegexOptions.IgnoreCase );

				Match m = r.Match( _relativePath );

				if ( m.Success )
				{
					if ( !String.IsNullOrEmpty( m.Groups[ 1 ].Captures[ 0 ].Value ) )
					{
						_expansionNumber = int.Parse( m.Groups[ 1 ].Captures[ 0 ].Value );
					}
					else
					{
						//expansion 1 (RotDG) doesn't have a number in it's path
						_expansionNumber = 1;
					}
					
					_relativePath = m.Groups[ 2 ].Captures[ 0 ].Value;
				}
			}
			else if ( _directory.IsCoreGame )
			{
				_expansionNumber = 0;
			}
			else
			{
                //This is a mod.
				_expansionNumber = 100;
			}
		}

		public override Stream OpenRead()
		{
			return _file.OpenRead();
		}

		public override StreamReader OpenText()
		{
			return _file.OpenText();
		}

		public override string FullName
		{
			get
			{
				return _file.FullName;
			}
		}

		public override string Name
		{
			get
			{
				return _file.Name;
			}
		}

		public override string SourceRelativePath
		{
			get
			{
				return _relativePath;
			}
		}

		public override int ExpansionNumber
		{
			get
			{
				return _expansionNumber;
			}
		}

		public override ContentDirectory Directory
		{
			get
			{
				return _directory;
			}
		}

		public override string ToString()
		{
			return _relativePath;
		}
	}

    /// <summary>
    /// An implementation of <see cref="ContentDirectory"/> for zip files themselves as well as directories within them.
    /// </summary>
	public class ZipFileDirectory : ContentDirectory
	{
		private ZipFile _zip;
		private string _path;
		private string _name;

		public ZipFileDirectory( ZipFile zip, string path, bool isCoreGame )
			:base( isCoreGame )
		{
			_zip = zip;

			_path = path.ToBackSlashes();

			if ( _path.EndsWith( "\\" ) )
			{
				_path = _path.Substring( 0, _path.Length - 1 );
			}

			if ( _path.Length > 0 )
			{
				_name = _path.Split( '\\' ).Last();
			}
			else
			{
				_name = String.Empty;
			}
		}

		public ZipFileDirectory( ZipFile file, ZipEntry entry, bool isCoreGame )
			: this( 
				file, 
				( entry.FileName.Contains( "/" ) ) ? entry.FileName.Substring( 0, entry.FileName.LastIndexOf( "/" ) ) : String.Empty,
				isCoreGame
			)
		{
		}

		public override IEnumerable<ContentFile> EnumerateFiles( SearchOption searchOption )
		{
			if ( searchOption == SearchOption.TopDirectoryOnly )
			{
				//This method will not recurse subdirectories.
				return _zip.SelectEntries( "*", _path )
					.Select( p => new ZipFileFile( p, new ZipFileDirectory( _zip, p, IsCoreGame ) ) );
			}
			else
			{
				return _zip.Where( p => !p.IsDirectory && p.FileName.StartsWith( _path.ToForwardSlashes() ) )
					.Select( p => new ZipFileFile( p, new ZipFileDirectory( _zip, p, IsCoreGame ) ) );
			}
		}

		public override IEnumerable<ContentFile> EnumerateFiles( string pattern, SearchOption searchOption )
		{
			if ( searchOption == SearchOption.TopDirectoryOnly )
			{
				//This method will not recurse subdirectories.
				return _zip.SelectEntries( pattern, _path )
					.Select( p => new ZipFileFile( p, new ZipFileDirectory( _zip, p, IsCoreGame ) ) );
			}
			else
			{
				Regex regex = new Regex( "^" + pattern.Replace( ".", @"\." ).Replace( "*", ".*" ).Replace( "?", "." ) + "$", RegexOptions.IgnoreCase );

				return _zip
					.Where( entry => !entry.IsDirectory )
					.Select( fileEntry => new ZipFileFile( fileEntry, new ZipFileDirectory( _zip, fileEntry, IsCoreGame ) ) )
					.Where( fileEntry => regex.IsMatch( fileEntry.Name ) );
			}
		}

		public override IEnumerable<ContentFile> EnumerateXmlFiles( string[] filenames, SearchOption searchOption )
		{
			return EnumerateFiles( "*.xml", searchOption )
				.Where( p => filenames.Any( q => q.ToLower() == p.Name.ToLower() ) );
		}

		public override ContentFile GetFile( string relativePath )
		{
			var sourceRelativePath = Path.Combine( this.SourceRelativePath, relativePath );

			var fileEntry = _zip[ sourceRelativePath ];

			if ( fileEntry != null && !fileEntry.IsDirectory )
			{
				return new ZipFileFile( fileEntry, new ZipFileDirectory( _zip, fileEntry, IsCoreGame ) );
			}
			else
			{
				return null;
			}			
		}

		public override ContentDirectory GetDirectory( string relativePath )
		{
			relativePath = relativePath.ToForwardSlashes();

			//this slash will be removed later, but we need it to make sure we are matching mod/ and not mod.txt
			if ( !relativePath.EndsWith( "/" ) )
			{
				relativePath = relativePath + "/";
			}

			var sourceRelativePath = Path.Combine( this.SourceRelativePath, relativePath ).ToLower();

			var exists = _zip.Any( p => p.FileName.ToLower().StartsWith( sourceRelativePath ) );

			if ( exists )
			{
				return new ZipFileDirectory( _zip, sourceRelativePath, IsCoreGame );
			}
			else
			{
				return null;
			}
		}

		public override string FullName
		{
			get
			{
				return _path;
			}
		}

		public override string Name
		{
			get
			{
				return _name;
			}
		}

		public override string SourceRelativePath
		{
			get
			{
				return _path;
			}
		}

		public override string ToString()
		{
			return this.SourceRelativePath;
		}
	}

    /// <summary>
    /// An implementation of <see cref="ContentFile"/> that represents a file within a zip file.
    /// </summary>
	public class ZipFileFile : ContentFile
	{
		private ZipEntry _entry;
		private string _name;
		private string _fullName;
		private ZipFileDirectory _directory;

		public ZipFileFile( ZipEntry entry, ZipFileDirectory directory )
		{
			_entry = entry;
			_directory = directory;
			_name = entry.FileName.Split( '/' ).Last();
			_fullName = entry.FileName.ToBackSlashes();		
		}

		public override Stream OpenRead()
		{
			return _entry.OpenReader();
		}

		public override StreamReader OpenText()
		{
			StreamReader reader = new StreamReader( _entry.OpenReader() );

			return reader;
		}

		public override string FullName
		{
			get
			{
				return _fullName;
			}
		}

		public override string Name
		{
			get
			{
				return _name;
			}
		}

		public override string SourceRelativePath
		{
			get
			{
				return _fullName;
			}
		}

		public override int ExpansionNumber
		{
			get
			{
				return 100;
			}
		}

		public override ContentDirectory Directory
		{
			get
			{
				return _directory;
			}
		}

		public override string ToString()
		{
			return _fullName;
		}
	}
}
