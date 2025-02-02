﻿using System;
using System.Reflection;
using Espionage.Engine.Components;

namespace Espionage.Engine
{
	public sealed class Property : IMember
	{
		public Library Owner { get; }

		internal Property( Library owner, PropertyInfo info, string name, object value )
		{
			// Ownership
			Owner = owner;
			Info = info;

			// Required
			Name = name;
			Default = value;

			// Meta
			Title = info.Name;
			Group = owner.Title;
			Editable = info.SetMethod != null;

			// Set the Default Value if we are static.
			if ( IsStatic && Default != null )
			{
				this[null] = Default;
			}

			// Components
			Components = new( this );

			// This is really expensive (6ms)...
			// Get Components attached to type
			foreach ( var item in Info.GetCustomAttributes() )
			{
				if ( item is IComponent<Property> property )
				{
					Components.Add( property );
				}
			}
		}

		public PropertyInfo Info { get; }
		public Components<Property> Components { get; }

		public string Name { get; }
		public object Default { get; }

		public string Title { get; set; }
		public string Group { get; set; }
		public string Help { get; set; }

		public bool Editable { get; set; }
		public bool Serialized { get; set; }

		// Helpers

		public bool IsStatic => Info.GetMethod.IsStatic;
		public Type Type => Info.PropertyType;

		public object this[ object from ]
		{
			get => Info.GetValue( from );
			set
			{
				if ( !Editable )
				{
					Dev.Log.Error( $"Can't edit {Name}" );
					return;
				}

				Info.SetValue( @from, value );
			}
		}
	}
}
