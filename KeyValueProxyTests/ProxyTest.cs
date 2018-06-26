using FluentAssertions;
using KeyValueProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KeyValueProxyTests
{
	[TestClass]
    public class ProxyTest
    {
        [TestMethod]
        public void RegularProperty()
        {
			var store = new DictionaryKVStore();
			var target = KeyValueProxyFactory.Create<IPerson>(store);

			target.FirstName = "test";
			target.FirstName.Should().Be("test");
			store.Store.Should().Contain("FirstName", "test");
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
