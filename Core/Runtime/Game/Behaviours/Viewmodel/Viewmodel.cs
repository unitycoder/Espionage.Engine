using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Espionage.Engine
{
	public sealed class Viewmodel : Entity
	{
		public static List<Viewmodel> All { get; } = new();

		public static void Apply( ref ITripod.Setup setup )
		{
			// Build Viewmodels...
			foreach ( var viewmodel in All )
			{
				if ( viewmodel.gameObject.activeInHierarchy )
				{
					viewmodel.PostCameraSetup( ref setup );
				}
			}
		}

		public static void Show( bool value )
		{
			foreach ( var viewmodel in All )
			{
				viewmodel.gameObject.SetActive( value );
			}
		}

		// Instance

		protected override void OnAwake()
		{
			All.Add( this );
			Effects = GetComponents<IEffect>().ToList();

			foreach ( var render in GetComponentsInChildren<Renderer>() )
			{
				render.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
				render.receiveShadows = receiveShadows;
				render.gameObject.layer = LayerMask.NameToLayer( "Viewmodel" );
			}
		}

		protected override void OnDelete()
		{
			All.Remove( this );
		}

		public List<IEffect> Effects { get; private set; }

		public void PostCameraSetup( ref ITripod.Setup setup )
		{
			// Basically if the current tripod is not the 
			// Pawns one, don't move...
			if ( Local.Client.Tripod != null )
			{
				return;
			}

			var trans = transform;
			trans.localPosition = setup.Position;
			trans.localRotation = setup.Rotation;

			foreach ( var effect in Effects )
			{
				effect.PostCameraSetup( ref setup );
			}
		}

		public interface IEffect
		{
			void PostCameraSetup( ref ITripod.Setup setup );
		}

		// Fields

		[SerializeField]
		private bool castShadows = false;

		[SerializeField]
		private bool receiveShadows = false;
	}
}
