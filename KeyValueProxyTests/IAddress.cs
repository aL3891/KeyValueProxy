namespace KeyValueProxyTests
{
	public interface IAddress
	{
		string Street { get; set; }
		string City { get; set; }
		int PostalCode { get; set; }
		IState State { get; set; }
	}
}
