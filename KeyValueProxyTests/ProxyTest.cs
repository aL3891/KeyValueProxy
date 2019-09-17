using FluentAssertions;
using KeyValueProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Reflection;

namespace KeyValueProxyTests
{
    [TestClass]
    public class ProxyTest
    {
        bool gotEvent = false;

        [TestMethod]
        public void RegularProperty()
        {
            var store = new DictionaryKVStore();
            var target = KeyValueProxyFactory.Create<IPerson>(store);

            target.PropertyChanged += Target_PropertyChanged;
            target.FirstName = "test";
            target.PropertyChanged -= Target_PropertyChanged;
            target.FirstName = "test";
            target.FirstName.Should().Be("test");
            store.Store.Should().Contain("FirstName", "test");
        }

        private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            gotEvent.Should().BeFalse();
            gotEvent = true;
            e.PropertyName.Should().Be("FirstName");
        }

        [TestMethod]
        public void SubProperty()
        {
            var store = new DictionaryKVStore();
            var target = KeyValueProxyFactory.Create<IPerson>(store);

            target.Address.Street = "test";
            target.Address.Street.Should().Be("test");
            store.Store.Should().Contain("Address.Street", "test");
        }

        [TestMethod]
        public void SubSubProperty()
        {
            var store = new DictionaryKVStore();
            var target = KeyValueProxyFactory.Create<IPerson>(store);

            target.Address.State.Code = "test";
            target.Address.State.Code.Should().Be("test");
            store.Store.Should().Contain("Address.State.Code", "test");
        }
    }
}
