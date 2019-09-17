using System.Threading.Tasks;
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
        private bool gotEvent;

        [TestMethod]
		public async Task RegularProperty()
		{
			var store = new DictionaryKVStore();
			var target = KeyValueProxyFactory.Create<IPerson>(store);

			target.SetFirstName("test");
			target.GetFirstName().Should().Be("test");
			await target.SetLastName("test2");
			(await target.GetLastName()).Should().Be("test2");
			target.Age = 1;
			target.Age.Should().Be(1);

			store.Store.Should().Contain("FirstName", "test");
			store.Store.Should().Contain("LastName", "test2");

            target.PropertyChanged += Target_PropertyChanged;
            target.Age = 1;
            target.PropertyChanged -= Target_PropertyChanged;
            target.Age = 1;
        }

        private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            gotEvent.Should().BeFalse();
            gotEvent = true;
            e.PropertyName.Should().Be("Age");
        }

        [TestMethod]
		public async Task SubProperty()
		{
			var store = new DictionaryKVStore();
			var target = KeyValueProxyFactory.Create<IPerson>(store);

			target.Address.Street = "test";
			target.Address.Street.Should().Be("test");

			target.Address.SetPostalCode("test2");
			target.Address.GetPostalCode().Should().Be("test2");

			await target.Address.SetCity("test2");
			(await target.Address.GetCity()).Should().Be("test2");

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
