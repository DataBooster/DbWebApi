using System;
using System.Linq;
using System.Text;
using DataBooster.DbWebApi;

namespace DbWebApi.OwinSample.Filters
{
	public class MyDbWebApiAuthorization : IDbWebApiAuthorization
	{
		public bool IsAuthorized(string userName, string storedProcedure, object state = null)
		{
			// TO DO, to implementate your own authorization logic
			return true;	// If allow permission
			return false;	// If deny permission
		}
	}
}
