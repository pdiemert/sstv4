using System;
using Griffin.Net.Protocols.Http;

namespace SSTV4.Interfaces
{
	public interface IApi
	{
		object Execute(IParameterCollection form);
	}
}
