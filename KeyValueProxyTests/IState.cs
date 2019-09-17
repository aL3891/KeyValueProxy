using System.ComponentModel;

namespace KeyValueProxyTests
{
	public interface IState : INotifyPropertyChanged
    {
		string Code { get; set; }
		string Name { get; set; }
	}
}
