using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Espionage.Engine
{
	public partial class Library
	{
		/// <summary>
		/// Registers the target object with the Library.
		/// Which allows it to receive instance callbacks and
		/// returns its library instance.
		/// </summary>
		public static Library Register( ILibrary value )
		{
			Callback.Register( value );
			return Database[value.GetType()];
		}

		/// <summary>
		/// Cleans up ILibrary object, removes it from instance
		/// callback database so the garbage collector picks it up.
		/// </summary>
		/// <param name="value"></param>
		public static void Unregister( ILibrary value )
		{
			Callback.Unregister( value );
		}

		/// <summary>
		/// Constructs ILibrary, if it it has a custom constructor
		/// it'll use that to create the ILibrary
		/// </summary>
		public static ILibrary Create( Library library )
		{
			if ( library is null )
			{
				Debugging.Log.Error( "Can't construct, Library is null" );
				return null;
			}

			if ( !library.Spawnable )
			{
				Debugging.Log.Error( $"{library.Name} is not spawnable. Set Spawnable to true in classes meta." );
				return null;
			}

			// If we are a singleton, Check if an instance already exists 
			if ( library.Components.TryGet<SingletonAttribute>( out var singleton ) )
			{
				if ( _singletons.ContainsKey( library.Class ) )
				{
					Debugging.Log.Info( $"Using Singleton [{library.Name}]" );
					return _singletons[library.Class];
				}

				Debugging.Log.Info( $"Creating Singleton [{library.Name}]" );
				var newSingleton = Construct( library );
				_singletons.Add( library.Class, newSingleton );
				return newSingleton;
			}

			return Construct( library );
		}

		private static ILibrary Construct( Library library )
		{
			// Check if we have a custom Constructor
			if ( library.Components.TryGet<ConstructorAttribute>( out var constructor ) )
			{
				return constructor.Invoke() as ILibrary;
			}

			if ( !library.Class.IsAbstract )
			{
				return Activator.CreateInstance( library.Class ) as ILibrary;
			}

			Debugging.Log.Error( $"Can't construct {library.Name}, is abstract and doesn't have constructor predefined." );
			return null;
		}

		//
		// Manager
		//

		public static bool Initialized { get; private set; }

		public static void Initialize()
		{
			if ( Initialized )
			{
				return;
			}

			Database = new InternalDatabase();
			Database.Add( new( typeof( Global ) ) );

			using ( Debugging.Stopwatch( "Library Initialized", 0 ) )
			{
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();

				// Select all types where ILibrary exists or if it has the correct attribute
				for ( var assemblyIndex = 0; assemblyIndex < assemblies.Length; assemblyIndex++ )
				{
					var assembly = assemblies[assemblyIndex];

					// Some fully awesome code  - maybe stupid? i dont care its awesome, basically checks if the assembly references library
					// and if it doesn't skip it. Resulting in crazy fast init
					var libAssembly = typeof( Library ).Assembly;
					if ( assembly != libAssembly && assembly.GetReferencedAssemblies().All( e => e.FullName != libAssembly.FullName ) )
					{
						continue;
					}

					var types = assembly.GetTypes();
					for ( var typeIndex = 0; typeIndex < types.Length; typeIndex++ )
					{
						var type = types[typeIndex];

						if ( type.HasInterface( typeof( ILibrary ) ) || type.IsDefined( typeof( LibraryAttribute ), true ) )
						{
							Database.Add( CreateRecord( type ) );
						}
					}
				}
			}

			Initialized = true;
			Callback.Run( "library.ready" );
		}

		private static Library CreateRecord( Type type )
		{
			if ( !type.IsDefined( typeof( LibraryAttribute ), false ) )
			{
				return new( type );
			}

			// If we have meta present, use it
			var attribute = type.GetCustomAttribute<LibraryAttribute>();
			return attribute.CreateRecord( type );
		}

		private static Guid GenerateID( string name )
		{
			if ( string.IsNullOrEmpty( name ) )
			{
				return default;
			}

			var random = new Random( name.GetHashCode( StringComparison.InvariantCultureIgnoreCase ) );
			var guid = new byte[16];
			random.NextBytes( guid );

			return new( guid );
		}

		//
		// Singletons
		//

		private static Dictionary<Type, ILibrary> _singletons = new();
	}
}
