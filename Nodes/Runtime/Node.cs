﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Espionage.Engine.Nodes
{
	/// <summary>
	/// Node that is only applicable to graph T
	/// </summary>
	/// <typeparam name="T"> Node only works with this type of graph </typeparam>
	public abstract class Node<T> : Node where T : Graph { }

	[Title( "Node" )]
	[Help( "Abstract Node" )]
	public abstract partial class Node : ScriptableObject, ILibrary, ICallbacks
	{
		//
		// API
		//

		public Library ClassInfo { get; set; }
		public IEnumerable<Port> Ports => ports.Values;
		public IEnumerable<Port> Outputs => Ports.Where( port => port.IsOutput );
		public IEnumerable<Port> Inputs => Ports.Where( port => port.IsInput );

		//
		// Object
		//

		private void OnEnable()
		{
			Callback.Register( this );
		}

		private void OnDestroy()
		{
			// Clear port connections
			foreach ( var port in Ports )
			{
				port.Clear();
			}

			Callback.Unregister( this );
		}

		//
		// Logic Flow
		//

		public virtual void Initialize() { }

		public virtual bool Execute()
		{
			return true;
		}

		//
		// Helpers
		//
		public Port GetPort( string portName )
		{
			return ports.TryGetValue( portName, out var port ) ? port : null;
		}

		public bool HasPort( string portName )
		{
			return ports.ContainsKey( portName );
		}

		public virtual void OnCreateConnection( Port from, Port to ) { }
		public virtual void OnRemoveConnection( Port port ) { }

		//
		// Serialization
		//

		public Graph graph;
		public Vector2 position;

		[SerializeField]
		private PortMap ports = new();

		[Serializable]
		private class PortMap : Map<string, Port> { }
	}
}
