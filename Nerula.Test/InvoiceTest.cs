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
			object id = CreateConjecture("First conjecture", 100);
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

		private object CreateConjecture(string conjectureName, int amount)
		{
			object id = null;
			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var conjecture = new Conjecture(conjectureName, 100);
					id = session.Save(conjecture);
					tx.Commit();
				}
			}
			return id;
		}

		[TestMethod]
		public void CanAddAllocation()
		{
			object invoiceId = CreateInvoiceWithAllocations("Invoice", 2);

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
		public void CanCreateAllocationInOneQuery()
		{
			int allocatinsCount = 5;
			object invoiceId = CreateInvoiceWithAllocations("VS001", allocatinsCount);

			var interceptor = new SqlQueryInterceptor();
			using (var session = SessionFactory.OpenSession(interceptor))
			{
				using (var tx = session.BeginTransaction())
				{
					var invoice = session.Get<Invoice>(invoiceId);
					invoice.AddAllocation(invoice.Allocations.First().Conjecture, 20);

					interceptor.ClearSqlQueries();
					session.Update(invoice);
					tx.Commit();

					Assert.AreEqual(1, interceptor.SqlQueries.Count());
					Assert.AreEqual(allocatinsCount + 1, invoice.Allocations.Count);
				}
			}
		}

		[TestMethod]
		public void CanUpdateAllocationInOneQuery()
		{
			int allocatinsCount = 5;
			object invoiceId = CreateInvoiceWithAllocations("VS001", allocatinsCount);

			var interceptor = new SqlQueryInterceptor();
			using (var session = SessionFactory.OpenSession(interceptor))
			{
				using (var tx = session.BeginTransaction())
				{
					var invoice = session.Get<Invoice>(invoiceId);
					invoice.Allocations[0].Amount = 10;
					invoice.Allocations[0].UpdaterName = "HAL9000";

					interceptor.ClearSqlQueries();
					session.Update(invoice);
					tx.Commit();

					Assert.AreEqual(1, interceptor.SqlQueries.Count());
				}
			}
		}


		[TestMethod]
		public void CanDeleteAllocationInOneQuery()
		{
			int allocatinsCount = 5;
			object invoiceId = CreateInvoiceWithAllocations("VS001", allocatinsCount);

			var interceptor = new SqlQueryInterceptor();
			using (var session = SessionFactory.OpenSession(interceptor))
			{
				using (var tx = session.BeginTransaction())
				{
					var invoice = session.Get<Invoice>(invoiceId);
					invoice.Allocations.RemoveAt(2);
					
					interceptor.ClearSqlQueries();
					session.Update(invoice);
					tx.Commit();
		
					Assert.AreEqual(allocatinsCount - 1, invoice.Allocations.Count);
					Assert.AreEqual(1, interceptor.SqlQueries.Count());
				}
			}
		}

		private object CreateInvoiceWithAllocations(string invoiceNumber, int allocationsCount)
		{
			object invoiceId = null;
			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var invoice = new Invoice { Number = invoiceNumber };

					for (int i = 0; i < allocationsCount; i++)
					{
						invoice.AddAllocation(new Conjecture(string.Format("Conj_{0:00}", i), 100), 50);
					}

					// I am saving this explicitely, but I could simply add  cascade="all" to Conjecture property of Allocation
					foreach (var conjecture in invoice.Allocations.Select(x => x.Conjecture))
					{
						session.Save(conjecture);
					}

					invoiceId = session.Save(invoice);
					tx.Commit();
				}
			}
			return invoiceId;
		}
	}
}
