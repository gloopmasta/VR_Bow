using System;

namespace PandaBT
{
	public interface IVariable
	{
		string name { get; }
		object value { get; set; }
		Type valueType { get; }
	}
}
