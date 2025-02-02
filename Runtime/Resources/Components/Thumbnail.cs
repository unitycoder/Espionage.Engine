﻿using Espionage.Engine.Components;

namespace Espionage.Engine.Resources
{
	public class Thumbnail : IComponent<Map>
	{
		public string Path { get; set; }

		public Thumbnail( string path )
		{
			Path = path;
		}

		public void OnAttached( Map item ) { }
	}
}
