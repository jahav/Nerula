using NHibernate;
using NHibernate.SqlCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nerula.Test
{
	/// <summary>
	/// Interceptor that logs all SQL statements to be send to database.
	/// </summary>
	public class SqlQueryInterceptor : EmptyInterceptor, IInterceptor
	{
		private IList<string> sqlQueries;

		public SqlQueryInterceptor()
		{
			this.sqlQueries = new List<string>();
		}

		/// <summary>
		/// Gets last executed sql query or null.
		/// </summary>
		public string LastSqlQuery { get { return sqlQueries.LastOrDefault(); } }

		/// <summary>
		/// Intercepted sql queries in order of interception.
		/// </summary>
		public IEnumerable<string> SqlQueries { get { return sqlQueries; } }

		/// <summary>
		/// Clear all recorded sql queries from <see cref="SqlQueries"/>.
		/// </summary>
		public void ClearSqlQueries()
		{
			sqlQueries.Clear();
		}

		public override SqlString OnPrepareStatement(SqlString sql)
		{
			sqlQueries.Add(sql.ToString());
			return sql;
		}
	}
}
