﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Espionage.Engine.Entities.Nodes
{
	public abstract partial class Node
	{
		[Serializable]
		public sealed class Port
		{
			public enum IO { Input, Output }

			// Helpers
			public Node Node => _node;
			public IO Direction => _direction;
			public bool IsInput => _direction is IO.Input;
			public bool IsOutput => _direction is IO.Output;

			// Data
			[SerializeField]
			private string _name;

			[SerializeField]
			private Node _node;

			[SerializeField]
			private IO _direction;

			[SerializeField]
			private List<Connection> _connections;

			//
			// Connect
			//

			public bool IsConnectedTo( Port port )
			{
				return _connections.Any( t => t.target == port );
			}

			public void Connect( Port port )
			{
				if ( !CanConnect( port ) )
				{
					Debugging.Log.Warning( "Cannot Connect Port" );
					return;
				}

				Node.OnCreateConnection( this, port );
				port.Node.OnCreateConnection( this, port );
			}

			public bool CanConnect( Port port )
			{
				if ( port is null )
				{
					return false;
				}

				if ( port.Direction == Direction )
				{
					return false;
				}

				return true;
			}

			//
			// Disconnect
			//

			public void Disconnect( Port port ) { }

			//
			// Serialization
			//

			[Serializable]
			private class Connection
			{
				public Port target;
				public List<Vector2> reroutes;

				public Connection( Port target )
				{
					this.target = target;
				}
			}
		}
	}
}
