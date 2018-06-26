namespace KeyValueProxy
{
	public interface IKeyValueProxyStore
	{
		void SetValue(string property, object value);
		object GetValue(string property);
	}
}
