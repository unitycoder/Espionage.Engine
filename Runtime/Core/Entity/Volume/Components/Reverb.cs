﻿namespace Espionage.Engine.Volumes
{
	public class Reverb : Component<Volume>, Volume.ICallbacks
	{
		public void OnEnter( Entity entity ) { }
		public void OnExit( Entity entity ) { }
		public void OnStay( float distance ) { }
	}
}
