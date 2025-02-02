﻿using System;

namespace Espionage.Engine
{
	/// <summary>
	/// Espionage.Engine's string to object converter.
	/// We sometimes use this for deserializing Properties.
	/// </summary>
	public static class Converter
	{
		/// <summary>
		/// Converts a string to the type of T. 
		/// </summary>
		public static T Convert<T>( string value )
		{
			if ( typeof( T ).IsEnum )
			{
				return (T)Enum.Parse( typeof( T ), value );
			}

			var library = Library.Database.Find<IConverter<T>>();

			if ( library == null )
			{
				Dev.Log.Error( $"No Valid converters for {typeof( T ).Name}." );
				return default;
			}

			var converter = Library.Database.Create<IConverter<T>>( library );

			try
			{
				return converter.Convert( value );
			}
			catch ( Exception e )
			{
				Dev.Log.Exception( e );
				return default;
			}
		}

		/// <summary>
		/// Converts a string to an object. Based off the
		/// inputted type. 
		/// </summary>
		/// <remarks>
		/// This uses reflection, and is pretty slow..
		/// Be careful where you put this method.
		/// </remarks>
		public static object Convert( string value, Type type )
		{
			// Doing explicit enum shit here, cause fuck it, this class is already painful
			if ( type.IsEnum )
			{
				return Enum.Parse( type, value );
			}

			// JAKE: This is so aids.... But can't do much about that.

			var interfaceType = typeof( IConverter<> ).MakeGenericType( type );
			var library = Library.Database.Find( interfaceType );

			if ( library == null )
			{
				throw new( "No Valid Converters for this Type" );
			}

			var converter = Library.Database.Create( library.Class );
			var method = interfaceType.GetMethod( "Convert" );

			try
			{
				return method?.Invoke( converter, new object[] { value } );
			}
			catch ( Exception e )
			{
				Dev.Log.Exception( e );
				return default;
			}
		}
	}
}
