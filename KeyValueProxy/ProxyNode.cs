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
        private Dictionary<MethodInfo, (MethodInfo storeMethod, string key, string name, bool sendNpc)> Meth = new Dictionary<MethodInfo, (MethodInfo storeMethod, string key, string name, bool sendNpc)>();
        private RootProxyNode root;
        List<WeakReference<PropertyChangedEventHandler>> eventHandlers = null;

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
                if (m.sendNpc && eventHandlers != null)
                {
                    List<WeakReference<PropertyChangedEventHandler>> toRemove = null;
                    foreach (var handler in eventHandlers)
                    {
                        if (handler.TryGetTarget(out var r))
                            r(this, new PropertyChangedEventArgs(m.name));
                        else
                        {
                            if (toRemove == null)
                                toRemove = new List<WeakReference<PropertyChangedEventHandler>>();
                        }
                    }
                    if (toRemove != null)
                    {
                        foreach (var tr in toRemove)
                        {
                            eventHandlers.Remove(tr);
                        }
                    }
                }
                return res;
            }
            else if (targetMethod.Name == "add_PropertyChanged")
            {
                if (eventHandlers == null)
                    eventHandlers = new List<WeakReference<PropertyChangedEventHandler>>();

                eventHandlers.Add(new WeakReference<PropertyChangedEventHandler>((PropertyChangedEventHandler)args[0]));
                return null;
            }
            else if (targetMethod.Name == "remove_PropertyChanged")
            {
                if (eventHandlers != null)
                {
                    eventHandlers.RemoveAll(w => !w.TryGetTarget(out var r) || r == (PropertyChangedEventHandler)args[0]);
                }

                return null;
            }

            throw new NotSupportedException("Proxy does not support method " + targetMethod.Name);
        }

        internal void Initialize(Type type, string path, RootProxyNode root)
        {
            this.root = root;
            var isNPC = typeof(INotifyPropertyChanged).IsAssignableFrom(type);
            foreach (var p in type.GetMethods())
            {
                var name = p.Name.Substring(p.Name[3] == '_' ? 4 : 3);
                var propPath = (path != null ? path + "." : "") + name;
                var isAsync = p.ReturnType == typeof(Task) || p.ReturnType.IsGenericType && p.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
                Type methodType = null;
                MethodInfo storeMethod = null;
                var sendNpc = false;

                if (p.Name.StartsWith("set", StringComparison.OrdinalIgnoreCase))
                {
                    methodType = p.GetParameters()[0].ParameterType;
                    storeMethod = isAsync ? KVSetMethodAsync : KVSetMethod;
                    if (isNPC)
                        sendNpc = true;
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

                Meth.Add(p, (storeMethod.MakeGenericMethod(methodType), propPath, sendNpc ? name : null, sendNpc));
            }
        }
    }
}
