using System;
using System.Collections.Generic;
using Espionage.Engine.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using Espionage.Engine.Resources.Binders;

namespace Espionage.Engine.Resources
{
	/// <summary>
	/// Allows the loading and unloading of maps at runtime.
	/// You should be using this instead of UnityEngine.SceneManager.
	/// </summary>
	[Group( "Maps" ), Path( "maps", "assets://Maps/" )]
	public sealed partial class Map : IResource, ILibrary, ILoadable
	{
		public static Map Current { get; private set; }

		[Function( "maps.init" ), Callback( "engine.getting_ready" )]
		private static void Initialize()
		{
			Binder provider = Application.isEditor ? new EditorSceneMapProvider() : new BuildIndexMapProvider( 0 );

			// Get main scene at start, that isn't engine layer.
			for ( var i = 0; i < SceneManager.sceneCount; i++ )
			{
				var scene = SceneManager.GetSceneAt( i );

				if ( scene.name != Engine.Scene.name )
				{
					SceneManager.SetActiveScene( scene );
					break;
				}
			}

			Current = new( provider );
			Callback.Run( "map.loaded" );

			// Cache Maps

			for ( var i = 0; i < SceneManager.sceneCountInBuildSettings; i++ )
			{
				var scene = SceneManager.GetSceneByBuildIndex( i );
				Setup( new BuildIndexMapProvider( i ) ).Meta( scene.name ).Build();
			}

			foreach ( var item in Files.Pathing.All( "maps://" ) )
			{
				Setup( item ).Meta( Files.Pathing.Name( item ) ).Build();
			}
		}

		/// <summary>
		/// Trys to find the map by path. If it couldn't find the map in the database,
		/// it'll create a new reference to that map for use later.
		/// </summary>
		public static Map Find( string path )
		{
			path = Files.Pathing.Absolute( path );

			// Use the Database Map if we have it
			if ( Database[path] != null )
			{
				return Database[path];
			}

			if ( !Files.Pathing.Exists( path ) )
			{
				Dev.Log.Error( $"Map Path [{Files.Pathing.Absolute( path )}], couldn't be found." );
				return null;
			}

			var file = Files.Serialization.Load<File>( path );
			return new( file.Binder );
		}

		/// <summary>
		/// Checks if the map / path already exists in the maps database. 
		/// </summary>
		public static bool Exists( string path )
		{
			path = Files.Pathing.Absolute( path );
			return Database[path] != null;
		}

		/// <summary>
		/// Sets up a load request for the the map at the given path.
		/// Use Loader.Start(), to load the map
		/// </summary>
		public static ILoadable Load( string path )
		{
			var map = Find( path );
			return map;
		}

		/// <summary>
		/// Sets up a load request for a map.
		/// Use Loader.Start(), to load the map
		/// </summary>
		public static ILoadable Load( Map map )
		{
			return map;
		}

		//
		// Instance
		//

		public Library ClassInfo { get; } = Library.Database[typeof( Map )];
		[Property] public string Identifier => Provider.Identifier;

		public Components<Map> Components { get; }
		private Binder Provider { get; }

		// Loadable 

		[Property] float ILoadable.Progress => Provider.Progress;
		[Property] string ILoadable.Text => Components.TryGet( out Meta meta ) ? $"Loading {meta.Title}" : "Loading";

		private Map( Binder provider )
		{
			Components = new( this );

			Provider = provider ?? throw new NullReferenceException();
			Database.Add( this );
		}

		public Action Loaded { get; set; }
		public Action Unloaded { get; set; }

		[Function, Button, Title( "Force Load" ), Help( "Forcefully load Map" )]
		void ILoadable.Load( Action loaded = null )
		{
			if ( Engine.Game.Loader?.Current != this )
			{
				Dev.Log.Error( "Don't forcefully load a map without using the loader!" );
				Dev.Log.Warning( "Forcefully reattempting map load through loader." );

				var queue = new Queue<ILoadable>();
				queue.Enqueue( this );

				Engine.Game.Loader?.Start( queue, loaded );
				return;
			}

			loaded += () => Callback.Run( "map.loaded" );
			loaded += Loaded;

			Callback.Run( "map.loading" );

			// Unload first, then load the next map
			if ( Current != null )
			{
				Dev.Log.Info( "Unloading, then loading" );
				Current.Unload( () => LoadRequest( loaded ) );
			}
			else
			{
				Dev.Log.Info( "Loading Map" );
				LoadRequest( loaded );
			}

			Current = this;
		}

		private void LoadRequest( Action onLoad )
		{
			// This maybe stupid?!
			Action<Scene> loaded = ( scene ) =>
			{
				SceneManager.SetActiveScene( scene );

				foreach ( var gameObject in scene.GetRootGameObjects() )
				{
					foreach ( var camera in gameObject.GetComponentsInChildren<Camera>() )
					{
						camera.enabled = false;
					}

					foreach ( var listener in gameObject.GetComponentsInChildren<AudioListener>() )
					{
						listener.enabled = false;
					}
				}

				onLoad?.Invoke();
			};

			Provider.Load( loaded );
		}

		private void Unload( Action unloaded = null )
		{
			// Add Callback
			unloaded += () => Callback.Run( "map.unloaded" );
			unloaded += Unloaded;

			if ( Current == this )
			{
				Current = null;
			}

			Provider.Unload( unloaded );
		}
	}
}
