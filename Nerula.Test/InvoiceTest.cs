using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nerula.Data;

namespace Nerula.Test
{
	[TestClass]
	public class InvoiceTest : DatabaseTest
	{

		[TestInitialize]
		public void TestInit()
		{
			DatabaseSetup(typeof(Invoice), typeof(Conjecture));
		}

		[TestCleanup]
		public void TestCleanup()
		{
			DatabaseTearDown();
		}

		[TestMethod]
		public void CanSaveConjecture()
		{
			object id = null;
			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var conjecture = new Conjecture("First conjecture", 100);
					id = session.Save(conjecture);
					tx.Commit();
				}
			}
			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var loadedConjecture = session.Get<Conjecture>(id);
					Assert.IsNotNull(loadedConjecture);
					tx.Commit();
				}
			}
		}

		[TestMethod]
		public void CanAddAllocation()
		{
			object invoiceId = null;
			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var invoice = new Invoice { Number = "VS001" };

					invoice.AddAllocation(new Conjecture("First", 100), 70);
					invoice.AddAllocation(new Conjecture("Second", 200), 100);

					foreach (var conjecture in invoice.Allocations.Select(x => x.Conjecture))
					{
						session.Save(conjecture);
					}

					invoiceId = session.Save(invoice);

					tx.Commit();
				}
			}
			using (var session = SessionFactory.OpenSession())
			using (var tx = session.BeginTransaction())
			{
				{
					var invoice = session.Get<Invoice>(invoiceId);
					Assert.IsNotNull(invoice);
					Assert.AreEqual(2, invoice.Allocations.Count);
					tx.Commit();
				}
			}
		}


		[TestMethod]
		public void CanUpdateAllocationInOneQuery()
		{
			object invoiceId = null;
			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var invoice = new Invoice { Number = "VS001" };

					invoice.AddAllocation(new Conjecture("First", 100), 50);
					invoice.AddAllocation(new Conjecture("Second", 200), 10);

					foreach (var conjecture in invoice.Allocations.Select(x => x.Conjecture))
					{
						session.Save(conjecture);
					}

					invoiceId = session.Save(invoice);

					tx.Commit();
				}
			}

			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var invoice = session.Get<Invoice>(invoiceId);
					Assert.IsNotNull(invoice);
					Assert.AreEqual(2, invoice.Allocations.Count);

					invoice.Allocations[0].Amount = 10;
					invoice.Allocations[0].UpdaterName = "HAL9000";

					session.Update(invoice);

					tx.Commit();
				}
			}
		}
	}
}
