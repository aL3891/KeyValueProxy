using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KeyValueProxy
{

    public class ProxyNode : DispatchProxy
    {
        private static MethodInfo KVGetMethod;
        public static MethodInfo KVGetMethodAsync;
        private static MethodInfo KVSetMethod;
        private static MethodInfo KVSetMethodAsync;

        private Dictionary<MethodInfo, ProxyNode> children = new Dictionary<MethodInfo, ProxyNode>();
        private Dictionary<MethodInfo, (MethodInfo storeMethod, string key, string name, bool setter)> Meth = new Dictionary<MethodInfo, (MethodInfo storeMethod, string key, string name, bool setter)>();
        private RootProxyNode root;
        List<PropertyChangedEventHandler> eventHandlers = null;

        static ProxyNode()
        {
            var t = typeof(IKeyValueProxyStore);
            KVGetMethod = t.GetMethod("GetValue");
            KVGetMethodAsync = t.GetMethod("GetValueAsync");
            KVSetMethod = t.GetMethod("SetValue");
            KVSetMethodAsync = t.GetMethod("SetValueAsync");
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (children.TryGetValue(targetMethod, out var node))
            {
                return node;
            }
            else if (Meth.TryGetValue(targetMethod, out var m))
            {
                var a = new object[args.Length + 1];
                Array.Copy(args, 0, a, 1, args.Length);
                a[0] = m.key;
                var res = m.storeMethod.Invoke(root.store, a);
                if (eventHandlers != null && m.setter)
                {
                    foreach (var handler in eventHandlers)
                    {
                        handler(this, new PropertyChangedEventArgs(m.name));
                    }
                }
                return res;
            }
            else if (targetMethod.Name == "add_PropertyChanged")
            {
                if (eventHandlers == null)
                    eventHandlers = new List<PropertyChangedEventHandler>();
                eventHandlers.Add((PropertyChangedEventHandler)args[0]);
                return null;
            }
            else if (targetMethod.Name == "remove_PropertyChanged")
            {
                if (eventHandlers != null)
                    eventHandlers.Remove((PropertyChangedEventHandler)args[0]);
                return null;
            }

            throw new NotSupportedException("Proxy does not support method " + targetMethod.Name);
        }

        internal void Initialize(Type type, string path, RootProxyNode root)
        {
            this.root = root;
            foreach (var p in type.GetMethods())
            {
                var propPath = (path != null ? path + "." : "") + p.Name.Substring(p.Name[3] == '_' ? 4 : 3);
                var isAsync = p.ReturnType == typeof(Task) || p.ReturnType.IsGenericType && p.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
                Type methodType = null;
                MethodInfo storeMethod = null;
                var setter = false;

                if (p.Name.StartsWith("set", StringComparison.OrdinalIgnoreCase))
                {
                    methodType = p.GetParameters()[0].ParameterType;
                    storeMethod = isAsync ? KVSetMethodAsync : KVSetMethod;
                    setter = true;
                }
                else if (p.Name.StartsWith("get", StringComparison.OrdinalIgnoreCase))
                {
                    methodType = p.ReturnType;
                    if (methodType.IsInterface)
                    {
                        var c = KeyValueProxyFactory.CreateChild(methodType, propPath, root);
                        children.Add(p, c);
                        continue;
                    }

                    if (isAsync)
                    {
                        methodType = methodType.GetGenericArguments()[0];
                        storeMethod = KVGetMethodAsync;
                    }
                    else
                    {
                        storeMethod = KVGetMethod;
                    }
                }
                else
                {
                    continue;
                }

                Meth.Add(p, (storeMethod.MakeGenericMethod(methodType), propPath, propPath.Split('.').Last(), setter));
            }
        }
    }
}
