
namespace DataBooster.DbWebApi
{
	public interface IDbWebApiAuthorization
	{
		bool IsAuthorized(string userName, string storedProcedure, object state = null);
	}
}
