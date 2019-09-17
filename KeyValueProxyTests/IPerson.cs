using System.Threading.Tasks;

using System.ComponentModel;

namespace KeyValueProxyTests
{
	public interface IPerson
	{
		string GetFirstName();
		void SetFirstName(string name);

		Task<string> GetLastName();
		Task SetLastName(string lastName);
		int Age { get; set; }
		IAddress Address { get; set; }

    }
}
