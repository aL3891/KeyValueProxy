using System.Threading.Tasks;

namespace KeyValueProxyTests
{
	public interface IAddress
	{
		string Street { get; set; }
		string GetPostalCode();
		void SetPostalCode(string name);
		Task<string> GetCity();
		Task SetCity(string lastName);
		string City { get; set; }
		int PostalCode { get; set; }
		IState State { get; set; }
	}
}
