using System.Reflection;

namespace KeyValueProxy
{
	public class KeyValueProxyFactory
	{
		static MethodInfo method = typeof(DispatchProxy).GetMethod("Create");

		public static T Create<T>(IKeyValueProxyStore store)
		{
			var res = DispatchProxy.Create<T, RootProxyNode>();
			var root = (RootProxyNode)(object)res;
			root.store = store;
			root.Initialize(typeof(T), null, root);
			return res;
		}

		public static void ChangeStore(object proxy, IKeyValueProxyStore store)
		{
			var root = (RootProxyNode)proxy;
			root.store = store;	
		}

		internal static ProxyNode CreateChild(PropertyInfo prop, string Path, RootProxyNode root)
		{
			var res = (ProxyNode)method.MakeGenericMethod(prop.PropertyType, typeof(ProxyNode)).Invoke(null, null);
			res.Initialize(prop.PropertyType, Path, root);
			return res;
		}
	}
}
