using System;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Espionage.Engine
{
	[AttributeUsage( AttributeTargets.Class )]
	public sealed class StyleSheetAttribute : Attribute, Library.IComponent
	{
		/// <summary>Don't use path if you are distributing this class.</summary>
		public string Path { get; set; }

		public string GUID { get; set; }

#if UNITY_EDITOR
		public StyleSheet Style
		{
			get
			{
				if ( !string.IsNullOrEmpty( Path ) )
				{
					return AssetDatabase.LoadAssetAtPath<StyleSheet>( Path );
				}

				if ( !string.IsNullOrEmpty( GUID ) && UnityEditor.GUID.TryParse( GUID, out var guid ) )
				{
					var path = AssetDatabase.GUIDToAssetPath( guid );
					return AssetDatabase.LoadAssetAtPath<StyleSheet>( path );
				}

				return null;
			}
		}
#else
		public StyleSheet Style => throw new NotImplementedException();
#endif
		public void OnAttached( ref Library library ) { }
	}
}
