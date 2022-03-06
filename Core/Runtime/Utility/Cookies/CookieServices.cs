﻿using System.Collections.Generic;
using System.Linq;

namespace Espionage.Engine.Services
{
	internal class CookieServices : Service
	{
		public List<Property> Registry { get; } = new();

		public void Register( Property property )
		{
			if ( Values.ContainsKey( property.Name ) )
			{
				property[null] = Values[property.Name];
			}

			Registry.Add( property );
		}

		// States

		public override void OnReady()
		{
			Load();

			// Set Object Values.
			foreach ( var item in Library.Database.All.SelectMany( e => e.Properties.All.Where( property => property.Components.Has<CookieAttribute>() ) ) )
			{
				Register( item );
			}

			Values = null;
		}

		public override void OnShutdown()
		{
			// Serialize Registry
			Save();
		}

		// Serialization

		private Dictionary<string, object> Values { get; set; } = new();

		private void Load()
		{
			if ( Files.Exists( "config://.cookies" ) )
			{
				var sheet = Files.Deserialize<string>( "config://.cookies" );
				Debugging.Log.Info( sheet );
			}
		}

		public void Save()
		{
			Files.Save( "penis", "config://.cookies" );
			Debugging.Log.Info( Files.GetPath( "config://.cookies" ) );
		}
	}
}
