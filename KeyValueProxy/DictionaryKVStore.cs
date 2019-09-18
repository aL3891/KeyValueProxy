using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeyValueProxy
{
	public class DictionaryKVStore : IKeyValueProxyStore
	{
		public Dictionary<string, object> Store { get; set; } = new Dictionary<string, object>();

		public void SetValue<T>(string property, T value)
		{
			Store[property] = value;
		}

		public T GetValue<T>(string property)
		{
            return Store.TryGetValue(property, out var res) ? (T)res : default(T);
        }

		public async Task SetValueAsync<T>(string property, T value)
		{
			Store[property] = value;
		}

		public async Task<T> GetValueAsync<T>(string property)
		{
            return Store.TryGetValue(property, out var res) ? (T)res : default(T);
        }
	}
}
