using System;
using System.Collections.Generic;
using System.Reflection;

namespace KeyValueProxy
{

	internal class ProxyNode : DispatchProxy
	{
		Dictionary<MethodInfo, ProxyNode> children = new Dictionary<MethodInfo, ProxyNode>();
		Dictionary<MethodInfo, string> getters = new Dictionary<MethodInfo, string>();
		Dictionary<MethodInfo, string> setters = new Dictionary<MethodInfo, string>();
		RootProxyNode root;
		string path;

		protected override object Invoke(MethodInfo targetMethod, object[] args)
		{
			if (children.TryGetValue(targetMethod, out var node))
			{
				return node;
			}
			else if (getters.TryGetValue(targetMethod, out var gName))
			{
				var res = root?.store?.GetValue(gName);
				if (res == null && targetMethod.ReturnType.IsValueType)
					return Activator.CreateInstance(targetMethod.ReturnType);
				else
					return res;
			}
			else if (setters.TryGetValue(targetMethod, out var sName))
			{
				root?.store?.SetValue(sName, args[0]);
				return null;
			}

			throw new ArgumentException("oopsie");
		}

		internal void Initialize(Type type, string path, RootProxyNode root)
		{
			this.root = root;
			this.path = path;

			foreach (var p in type.GetProperties())
			{
				if (p.PropertyType.IsInterface)
				{
					var c = KeyValueProxyFactory.CreateChild(p, (path != null ? path + "." : "") + p.Name, root);
					children.Add(p.GetMethod, c);
				}
				else
				{
					getters.Add(p.GetMethod, (path != null ? path + "." : "") + p.Name);
					setters.Add(p.SetMethod, (path != null ? path + "." : "") + p.Name);
				}
			}
		}
	}
}
