﻿using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Espionage.Engine.AI
{
	[Group( "AI" ), Title( "AI Brain" )]
	public class Brain : Component<Actor>, Pawn.ICallbacks, Actor.ICallbacks
	{
		protected override void OnAttached( Actor actor )
		{
			OnAwake();

			if ( !TryGetComponent( out _agent ) )
			{
				_agent = gameObject.AddComponent<NavMeshAgent>();
			}

			Entity.Thinking.Add( Think, 0.2f );
		}

		protected override void OnReady()
		{
			// Cache Sensors			
			Senses = Entity.Components.GetAll<Sense>().ToArray();
		}

		public void Think()
		{
			if ( Local.Pawn != null )
			{
				_agent.destination = Local.Pawn.transform.position;
			}

			Entity.Tick = 0.2f;
		}

		// Senses

		public Sense[] Senses { get; private set; }

		// Actor Health

		public void Respawn()
		{
			// We, don't care if this pawn respawns
			// Cause it gets destroyed when it dies.
		}

		public bool OnDamaged( ref IDamageable.Info info ) { return true; }

		public void OnKilled( IDamageable.Info info ) { }

		// Pawn Possession

		void Pawn.ICallbacks.Possess( Client client )
		{
			_agent.enabled = false;
			Entity.Thinking.Remove( Think );
		}

		void Pawn.ICallbacks.UnPossess()
		{
			_agent.enabled = true;
			Entity.Thinking.Add( Think, 0.2f );
		}

		// Fields

		private NavMeshAgent _agent;

		[SerializeField]
		private string graphPath;
	}
}
