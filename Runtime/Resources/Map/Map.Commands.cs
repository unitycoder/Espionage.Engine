﻿namespace Espionage.Engine.Resources
{
	public sealed partial class Map
	{
		[Function( "map" ), Terminal]
		private static void LoadMap( string path = null )
		{
			if ( string.IsNullOrEmpty( path ) )
			{
				if ( Current == null )
				{
					Dev.Log.Info( "Nothing" );
					return;
				}

				if ( Current.Components.TryGet( out Meta meta ) )
				{
					Dev.Log.Info( $"Map [{Current.Identifier}], Title [{meta.Title}], Author [{meta.Author}]" );
					return;
				}

				Dev.Log.Info( $"Map [{Current.Identifier}]" );
				return;
			}

			var map = Find( path );

			if ( map == null )
			{
				return;
			}

			Loader.Start(
				Load( map )
			);
		}

		[Function( "map.reload" ), Terminal]
		private static void Restart()
		{
			Loader.Start(
				Load( Current )
			);
		}
	}
}
