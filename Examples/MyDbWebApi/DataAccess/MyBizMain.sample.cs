#region Usage Examples
// To turn off following sample code in DEBUG mode, just add NO_SAMPLE into the project properties ...
// Project Properties Dialog -> Build -> General -> Conditional Compilation Symbols.
#if (DEBUG && !NO_SAMPLE)
using System;
using System.Threading.Tasks;
using DbParallel.DataAccess;
using DbParallel.DataAccess.Booster.SqlServer;
using MyDbWebApi.DataAccess;

namespace MyDbWebApi
{
	class MyBizMain
	{
		public void MyTestAccess()
		{
			using (DbAccess db = DbPackage.CreateConnection())
			{
				// ...
				var test1 = db.GetSampleSetting("TestDomain");
				// ...
				var test2 = db.LoadSampleObjByAction(test1.Item1, test1.Item2);
				// ...
				using (DbTransactionScope tran = db.NewTransactionScope())	// Start a transaction
				{
					var test3 = db.LoadSampleObjByMap(test1.Item1, test1.Item2);
					// ...
					db.LogSampleError("Test source", "Test message ...");
					// ...
					tran.Complete();
				}	// Exit (Commit) the transaction
				// ...
				var test4 = db.LoadSampleObjAutoMap(test1.Item1, test1.Item2);
				// ...
			}
		}

		public void MyTestSqlLauncher()
		{
			using (SqlLauncher launcher = DbPackage.CreateSampleSqlLauncher())
			{
				Parallel.For(0, 100, i =>   // Just simulating multiple(100) producers
				{
					for (int j = 0; j < 200000; j++)
					{
						launcher.AddSampleSqlRow(i, j.ToString(), (i * 200000 + j) * 0.618m);
					}
				});
			}
		}
	}
}
#endif
#endregion
