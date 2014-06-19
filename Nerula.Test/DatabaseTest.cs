using Nerula.Data;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;

namespace Nerula.Test
{
	/// <summary>
	/// Base class for tests that utilize the in memory database.
	/// 
	/// IMPORTANT: You will probably encounter "The Process cannot access the file 'SQLite.Interop.dll' because it is being used by another process"
	/// when trying to build test multiple times. The error is cause by test runner of Visual Studio, the runner is 
	/// kept alive between test runs and while it is alive, it is accessing the SQLite.Interop.dll because property of the dll is set 
	/// to "Copy Always", it tries to override the running version. There are two ways to solve this: set properties to "Copy If Newer"
	/// or change test runner settings so it is not alive between tests runs (may cause performance penalty). The option is in TOOLS ->
	/// Options -> Web Performance Test Tools: Uncheck the "Test Execution Keep test execution engine running between test runs".
	/// Source: stackoverflow#12919447
	/// </summary>
	public class DatabaseTest
	{
		private ISession keepAliveSession;

		public ISessionFactory SessionFactory { get; private set; }

		public void DatabaseSetup(params System.Reflection.Assembly[] assembliesWithMappings)
		{
			DatabaseSetup(configuration =>
				{
					foreach (var assembly in assembliesWithMappings)
						configuration.AddAssembly(assembly);
				}
			);
		}

		public void DatabaseSetup(params System.Type[] mappedEntityTypes)
		{
			DatabaseSetup(configuration =>
				{
					foreach (var entityType in mappedEntityTypes)
						configuration.AddClass(entityType);
				}
			);
		}

		/// <summary>
		/// Setup database and create schema in the in-memory database.
		/// </summary>
		/// <param name="addMapping">Action that adds mappings to the configuration so the application can work with some mappings.</param>
		private void DatabaseSetup(System.Action<Configuration> addMapping)
		{
			// Also note the ReleaseConnections "on_close"
			var configuration = new Configuration()
				.SetProperty(Environment.ReleaseConnections, "on_close") // http://www.codethinked.com/nhibernate-20-sqlite-and-in-memory-databases
				.SetProperty(Environment.Dialect, typeof(SQLiteDialect).AssemblyQualifiedName)
				.SetProperty(Environment.ConnectionProvider, typeof(SQLiteInMemoryConnectionProvider).AssemblyQualifiedName)
				.SetProperty(Environment.ConnectionDriver, typeof(SQLite20Driver).AssemblyQualifiedName)
				.SetProperty(Environment.ShowSql, "true")
				.SetProperty(Environment.ConnectionString, "data source=:memory:;version=3")
				.SetProperty(Environment.CollectionTypeFactoryClass, typeof(Net4CollectionTypeFactory).AssemblyQualifiedName);

			addMapping(configuration);

			SessionFactory = configuration.BuildSessionFactory();

			keepAliveSession = SessionFactory.OpenSession();

			// Note: since in-memory db is destroyed when session is closed, you can't use
			// new SchemaExport(configuration).Execute(true, true, false), because it creates a session, creates schemas 
			// and then disposes of the created session - effectively destroying db. Thus the other opened session
			// doesn't have schemas ready. The result are errors like SQL logic error or missing database no such table
			// We must create schema using our session.
			new SchemaExport(configuration).Execute(true, true, false, keepAliveSession.Connection, null);
		}

		public void DatabaseTearDown()
		{
			keepAliveSession.Dispose();
		}
	}
}
