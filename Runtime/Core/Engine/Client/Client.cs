using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Espionage.Engine
{
	/// <summary>
	/// A Client is a person that is currently playing the game.
	/// Clients controls Input and their current possessed Pawn.
	/// </summary>
	[Group( "Networking" ), Spawnable( false )]
	public class Client : Entity
	{
		public new static IEnumerable<Client> All => Entity.All.OfType<Client>();

		internal static Client Create( string name )
		{
			var obj = new GameObject( $"[ Client ] {name} / id=0" ).AddComponent<Client>();
			Engine.Scene.Grab( obj.gameObject );
			return obj;
		}

		//
		// Instance
		//

		/// <summary> A Nice name for the Client. </summary>
		public string Name { get; set; }

		/// <summary> Is this client ready to enter the game world? </summary>
		public bool IsReady { get; internal set; }

		internal virtual void Simulate()
		{
			Controls.SetSetup( this );
			Engine.Game.Simulate( this );
		}

		// Camera

		/// <summary>
		/// The clients active tripod.
		/// This overrides the current pawns tripod
		/// </summary>
		public ITripod Tripod { get; set; }

		//
		// Input
		//

		/// <summary>
		/// The clients current input buffer.
		/// <remarks>
		/// you should be using this instead of Unity's default
		/// input system.
		/// </remarks>
		/// </summary>
		public Controls.Setup Input { get; set; }

		//
		// Pawn
		//

		private Pawn _pawn;

		/// <summary> The pawn this client is currently possessing. </summary>
		public Pawn Pawn
		{
			get => _pawn;
			set
			{
				if ( _pawn != null )
				{
					_pawn.Client = null;
					_pawn.UnPossess();
				}

				_pawn = value;

				if ( _pawn != null )
				{
					Input.ViewAngles = _pawn.transform.rotation.eulerAngles;

					_pawn.Client = this;
					_pawn.Posses( this );
				}
			}
		}
	}
}
