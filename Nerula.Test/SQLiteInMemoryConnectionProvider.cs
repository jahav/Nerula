using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nerula.Test
{
	/// <summary>
	/// Because SQLite will destoy database as soon as the connection is closed, we are using this in memory provider
	/// that doesn't actually close connection, unless we are already disposing of the connection provider.
	/// </summary>
	public class SQLiteInMemoryConnectionProvider : NHibernate.Connection.DriverConnectionProvider, IDisposable
	{
		/// <summary>
		/// Connection to the database.
		/// </summary>
		private System.Data.IDbConnection connection;

		/// <summary>
		/// Should we keep connection alive despite requests to close connection?
		/// </summary>
		private bool keepAlive = true;

		public override System.Data.IDbConnection GetConnection()
		{
			if (connection == null)
				connection = base.GetConnection();
			return connection;
		}

		/// <summary>
		/// Fake closing connection, unless the provider is being disposed of.
		/// </summary>
		/// <param name="conn">Connection to close.</param>
		public override void CloseConnection(System.Data.IDbConnection conn)
		{
			if (!keepAlive)
			{
				base.CloseConnection(conn);
			}
		}

		public new void Dispose()
		{
			keepAlive = false;
			base.Dispose();
		}
	}
}
