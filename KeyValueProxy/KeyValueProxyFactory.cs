using System;
using System.Reflection;

namespace KeyValueProxy
{
	public class KeyValueProxyFactory
	{
		private static MethodInfo method = typeof(DispatchProxy).GetMethod("Create");

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

		internal static ProxyNode CreateChild(Type type, string Path, RootProxyNode root)
		{
			var res = (ProxyNode)method.MakeGenericMethod(type, typeof(ProxyNode)).Invoke(null, null);
			res.Initialize(type, Path, root);
			return res;
		}
	}
}
