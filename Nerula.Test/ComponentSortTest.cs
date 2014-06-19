using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nerula.Data;
using System.Linq;
using NHibernate.Linq;

namespace Nerula.Test
{
	[TestClass]
	public class ComponentSortTest : DatabaseTest
	{

		[TestInitialize]
		public void TestInit()
		{
			DatabaseSetup(typeof(Complex));
		}

		[TestCleanup]
		public void TestCleanup()
		{
			DatabaseTearDown();
		}

		[TestMethod]
		public void LINQ_SortingComponent()
		{
			CreateMultipleComplex();

			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var query = session.Query<Complex>().OrderBy(x => x.Component.Number);

					foreach (var res in query.ToList())
					{
						Console.WriteLine("Id: {0} Number {1} Desc {2}", res.ComplexId, res.Component.Number, res.Component.Description);
					}
					tx.Commit();
				}
			}
		}

		private void CreateMultipleComplex()
		{
			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					session.Save(new Complex { Component = new MyComponent { Number = 5, Description = "C" } });
					session.Save(new Complex { Component = new MyComponent { Number = 3, Description = "D" } });
					session.Save(new Complex { Component = new MyComponent { Number = 6, Description = "A" } });
					session.Save(new Complex { Component = new MyComponent { Number = 9, Description = "E" } });
					session.Save(new Complex { Component = new MyComponent { Number = 1, Description = "B" } });

					tx.Commit();
				}
			}
		}


		[TestMethod]
		public void HQL_SortingComponent()
		{
			CreateMultipleComplex();

			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var query = session.CreateQuery("from Complex c order by c.Component.Number asc");

					foreach (var res in query.List<Complex>())
					{
						Console.WriteLine("Id: {0} Number {1} Desc {2}", res.ComplexId, res.Component.Number, res.Component.Description);
					}
					tx.Commit();
				}
			}
		}
	}
}
