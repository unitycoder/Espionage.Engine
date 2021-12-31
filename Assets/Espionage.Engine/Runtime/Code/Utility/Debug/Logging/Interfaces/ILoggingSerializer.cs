using System.Collections.Generic;

namespace Espionage.Engine.Internal
{
	public interface ILoggingSerializer
	{
		void Serialize();
		IEnumerable<Entry> Deserialize();
	}
}
