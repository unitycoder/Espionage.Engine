using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Espionage.Engine
{
	[Manager( nameof( Initialize ), Layer = Layer.Runtime | Layer.Editor, Order = 500 )]
	public static class Engine
	{
		public static Game Game { get; private set; }

		private static void Initialize()
		{
			using ( Debugging.Stopwatch( "Engine / Game Ready" ) )
			{
				// Setup Callbacks
				Application.quitting -= OnShutdown;
				Application.quitting += OnShutdown;

				if ( Application.isPlaying )
				{
					// Frame Update
					Application.onBeforeRender -= OnFrame;
					Application.onBeforeRender += OnFrame;
				}

				// Setup Game
				var target = Library.Database.GetAll<Game>().FirstOrDefault( e => !e.Class.IsAbstract );

				if ( target is null )
				{
					Debugging.Log.Warning( "Game couldn't be found." );
					Callback.Run( "game.not_found" );
					return;
				}

				Game = Library.Database.Create<Game>( target.Class );

				// Ready Up Project
				Callback.Run( "game.ready" );
				Game.OnReady();
			}

			// Setup PlayerSettings based off of Project
#if UNITY_EDITOR
			PlayerSettings.productName = Game.ClassInfo.Title;

			if ( Game.ClassInfo.Components.TryGet<CompanyAttribute>( out var item ) )
			{
				PlayerSettings.companyName = item.Company;
			}
#endif
		}

		private static void OnFrame()
		{
			// Setup Camera
		}

		private static void OnShutdown()
		{
			Game?.OnShutdown();
		}

		//
		// Camera Building
		//

		private static Tripod.Setup _lastSetup;

		private static void SetupCamera()
		{
			if ( Game != null )
			{
				_lastSetup = Game.BuildCamera( _lastSetup );

				// Get Camera Component
			}
		}
	}
}
