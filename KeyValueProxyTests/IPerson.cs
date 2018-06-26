namespace KeyValueProxyTests
{
	public interface IPerson
	{
		string FirstName { get; set; }
		string LastName { get; set; }
		int Age { get; set; }
		IAddress Address { get; set; }

	}
}
