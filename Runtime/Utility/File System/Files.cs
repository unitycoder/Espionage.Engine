﻿using System.Diagnostics;
using System.IO;
using System.Reflection;
using Espionage.Engine.IO;

namespace Espionage.Engine
{
	/// <summary>
	/// Files, is Espionage.Engines File System.
	/// All Saving, Loading, ETC. You can use
	/// short hands for defining paths.
	/// </summary>
	[Library, Group( "Files" ), Title( "File System" )]
	public static partial class Files
	{
		public static Pathing Pathing { get; } = new();
		public static Serializer Serializer { get; } = new();
		public static Deserializer Deserializer { get; } = new();


		/// <summary>
		/// Deletes the file at the given path
		/// </summary>
		public static void Delete( string path )
		{
			path = Pathing.Get( path );

			if ( File.Exists( path ) )
			{
				File.Delete( path );
			}

			Debugging.Log.Error( $"File [{path}], couldn't be deleted." );
		}

		/// <summary>
		/// Deletes all files with the given extension at the path
		/// </summary>
		public static void Delete( string path, string extension )
		{
			path = Pathing.Get( path );

			var files = Directory.GetFiles( path, $"*.{extension}" );
			foreach ( var item in files )
			{
				File.Delete( item );
			}
		}

		/// <inheritdoc cref="Delete(string, string)"/> 
		public static void Delete( string path, params string[] extension )
		{
			if ( !Pathing.Exists( path ) )
			{
				Debugging.Log.Error( $"Path [{path}], doesn't exist" );
				return;
			}

			foreach ( var item in extension )
			{
				Delete( path, item );
			}
		}


		/// <summary>
		/// Copies the source file to the target path
		/// </summary>
		public static void Copy( string file, string path )
		{
			file = Pathing.Get( file );
			path = Pathing.Get( path );

			var fileInfo = new FileInfo( file );

			if ( !File.Exists( file ) )
			{
				throw new FileNotFoundException();
			}

			if ( !Directory.Exists( path ) )
			{
				Directory.CreateDirectory( path );
			}

			fileInfo.CopyTo( path );
		}

		/// <summary>
		/// Moves the source file to the target destination
		/// </summary>
		public static void Move( string source, string destination, bool overwrite = true )
		{
			source = Pathing.Get( source );
			destination = Pathing.Get( destination );

			if ( !File.Exists( source ) )
			{
				throw new FileNotFoundException();
			}

			File.Move( source, destination );
		}

		/// <summary>
		/// Opens the given directory in the OS's File Explorer,
		/// or opens the given file in the default application
		/// </summary>
		public static void Open( string path )
		{
			path = Pathing.Get( path );

			if ( !Pathing.Exists( path ) )
			{
				Debugging.Log.Warning( $"Path or File [{path}], doesn't exist" );
			}

			Process.Start( $"file://{path}" );
		}
	}
}
