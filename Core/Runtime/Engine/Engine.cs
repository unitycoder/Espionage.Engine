using System;
using System.Collections.Generic;
using Espionage.Engine.Internal;
using Espionage.Engine.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Espionage.Engine
{
	/// <summary>
	/// Espionage.Engine Entry Point. Initializes all its services, and sets up the Game.
	/// </summary>
	[Manager( nameof( Initialize ), Layer = Layer.Runtime, Order = 600 )]
	public sealed class Engine : MonoBehaviour
	{
		public static Game Game { get; private set; }

		private static void Initialize()
		{
			using ( Debugging.Stopwatch( "Engine / Game Ready", true ) )
			{
				CreateEngineLayer();

				var engineObj = new GameObject( "Engine" ).AddComponent<Engine>();
				AddToLayer( engineObj.gameObject );

				if ( !SetupGame() )
				{
					Debugging.Log.Error( "Game couldn't be found. Make sure to make a class inherited from Game" );
					return;
				}

				Services = new ServiceDatabase();

				// TODO: THIS IS TEMP
				Local.Client = Client.Create( "Local" );

				// Tell Services we're ready
				foreach ( var service in Services.All )
				{
					service.OnReady();
				}

				Game?.OnReady();

				Callback.Run( "engine.ready" );
			}
		}

		private static bool SetupGame()
		{
			var target = Library.Database.Find<Game>();

			if ( target is null )
			{
				Callback.Run( "game.not_found" );
				return false;
			}

			Game = Library.Database.Create<Game>( target.Class );
			Callback.Run( "game.ready" );

			return true;
		}

		//
		// Services
		//

		public static IDatabase<IService> Services { get; private set; }

		private class ServiceDatabase : IDatabase<IService>
		{
			public IEnumerable<IService> All => _services;

			private readonly List<IService> _services = new();

			public ServiceDatabase()
			{
				foreach ( var service in Library.Database.GetAll<IService>() )
				{
					Add( Library.Database.Create<IService>( service.Class ) );
				}
			}

			public void Add( IService item )
			{
				_services.Add( item );
			}

			public bool Contains( IService item )
			{
				return _services.Contains( item );
			}

			public void Remove( IService item )
			{
				_services.Remove( item );
			}

			public void Clear()
			{
				_services.Clear();
			}
		}

		//
		// Layer
		//

		/// <summary>
		/// The Engine Layer scene. This should never be unloaded.
		/// </summary>
		public static Scene Scene { get; private set; }

		private static void CreateEngineLayer()
		{
			// Create engine layer scene
			Scene = SceneManager.CreateScene( "Engine Layer" );
			Callback.Run( "engine.layer_created" );
		}

		/// <summary>
		/// Adds an Object to the Engine Layer scene.
		/// </summary>
		/// <param name="gameObject">The GameObject to add</param>
		public static void AddToLayer( GameObject gameObject )
		{
			SceneManager.MoveGameObjectToScene( gameObject, Scene );
		}

		//
		// Callbacks
		//

		private void Update()
		{
			foreach ( var service in Services.All )
			{
				service.OnUpdate();
			}

			// More temp - this should 
			// Be called at an engine level
			Game.Simulate( Local.Client );

			Callback.Run( "application.frame" );
		}

		private void OnApplicationQuit()
		{
			foreach ( var service in Services.All )
			{
				service.OnShutdown();
			}

			Game?.OnShutdown();

			Callback.Run( "application.quit" );
		}
	}
}
