using System.Threading.Tasks;

namespace KeyValueProxy
{
	public interface IKeyValueProxyStore
	{
		void SetValue<T>(string property, T value);
		T GetValue<T>(string property);

		Task SetValueAsync<T>(string property, T value);
		Task<T> GetValueAsync<T>(string property);
	}
}
