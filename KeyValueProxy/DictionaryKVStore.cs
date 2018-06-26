using System.Collections.Generic;

namespace KeyValueProxy
{
	public class DictionaryKVStore : IKeyValueProxyStore
	{
		public Dictionary<string, object> Store { get; set; } = new Dictionary<string, object>();

		public void SetValue(string property, object value)
		{
			Store[property] = value;
		}

		public object GetValue(string property)
		{
			return Store.TryGetValue(property, out var res) ? res : null;
		}
	}
}
