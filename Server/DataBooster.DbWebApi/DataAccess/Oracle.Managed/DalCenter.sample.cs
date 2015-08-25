using DbParallel.DataAccess;

namespace DataBooster.DbWebApi.DataAccess
{
	public partial class DalCenter : DbContextBase
	{
		public DalCenter()
			: base(ConfigHelper.DbProviderFactory, ConfigHelper.ConnectionString)
		{
		}

		public PropertyNamingConvention DynamicPropertyNamingConvention
		{
			get { return _DbAccess.DynamicPropertyNamingConvention; }
			set { _DbAccess.DynamicPropertyNamingConvention = value; }
		}
	}
}
