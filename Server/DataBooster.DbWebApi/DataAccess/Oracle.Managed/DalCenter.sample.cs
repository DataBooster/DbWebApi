using System;
using DbParallel.DataAccess;

namespace DataBooster.DbWebApi.DataAccess
{
	public partial class DalCenter : DbContextBase
	{
		partial void OnInit();

		public DalCenter()
			: base(ConfigHelper.DbProviderFactory, ConfigHelper.ConnectionString)
		{
			OnInit();
		}

		public PropertyNamingConvention DynamicPropertyNamingConvention
		{
			get { return AccessChannel.DynamicPropertyNamingConvention; }
			set { AccessChannel.DynamicPropertyNamingConvention = value; }
		}

		protected string CompleteSpName(string sp)
		{
			if (string.IsNullOrEmpty(sp))
				throw new ArgumentNullException("sp");
			else
				return ConfigHelper.DatabasePackage + sp;
		}
	}
}
