using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Espionage.Engine
{
	/// <summary>
	/// Prefabs are used for spawning unity prefabs at runtime using C#
	/// </summary>
	[Group( "Prefabs" ), Spawnable]
	public abstract class Prefab : ILibrary
	{
		public Library ClassInfo { get; private set; }
		public string Path { get; }

		public Prefab()
		{
			ClassInfo = Library.Database[GetType()];

			if ( !ClassInfo.Components.TryGet<FileAttribute>( out var fileAttribute ) )
			{
				Dev.Log.Error( $"{ClassInfo.Name} doesn't have, FileAttribute" );
				return;
			}

			Path = fileAttribute.Path;
		}

		//
		// Spawners
		//

		/// <summary>
		/// <inheritdoc cref="Spawn"/>, and returns
		/// a component from it.
		/// </summary>
		/// <typeparam name="T">Component</typeparam>
		/// <returns>Component of type T</returns>
		public T Spawn<T>( Vector3 pos = default, Quaternion rot = default ) where T : MonoBehaviour
		{
			return Spawn( pos, rot ).GetComponent<T>();
		}

		/// <summary>
		/// Spawns the GameObject
		/// </summary>
		public GameObject Spawn( Vector3 pos = default, Quaternion rot = default )
		{
		#if UNITY_EDITOR
			var asset = AssetDatabase.LoadAssetAtPath<GameObject>( Path );
			var newObject = Object.Instantiate( asset, pos, rot );
			OnSpawn( newObject );
			return newObject;

		#elif UNITY_STANDALONE
			// Get Object from AssetBundle and Spawn
			return null;

		#else
			return null;

		#endif
		}

		/// <summary>
		/// Called when the object has just spawned.
		/// Use this for setting up predefined values.
		/// </summary>
		/// <param name="gameObject"> The Object that's being spawned </param>
		protected virtual void OnSpawn( GameObject gameObject )
		{
			gameObject.AddComponent<Identity>().Library = ClassInfo;
		}
	}
}
