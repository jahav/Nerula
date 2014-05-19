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
			using (var tx = Session.BeginTransaction())
			{
				var conjecture = new Conjecture("First conjecture", 100);
				id = Session.Save(conjecture);
				tx.Commit();
			}
			Session.Clear();
			using (var tx = Session.BeginTransaction())
			{
				var loadedConjecture = Session.Get<Conjecture>(id);
				Assert.IsNotNull(loadedConjecture);
				tx.Commit();
			}
		}

		[TestMethod]
		public void CanAddAllocation()
		{
			object invoiceId = null;
			using (var tx = Session.BeginTransaction())
			{
				var invoice = new Invoice { Number = "VS001" };

				invoice.AddAllocation(new Conjecture("First", 100), 70);
				invoice.AddAllocation(new Conjecture("Second", 200), 100);

				foreach (var conjecture in invoice.Allocations.Select(x => x.Conjecture))
				{
					Session.Save(conjecture);
				}

				invoiceId = Session.Save(invoice);

				tx.Commit();
			}
			Session.Clear();
			using (var tx = Session.BeginTransaction())
			{
				var invoice = Session.Get<Invoice>(invoiceId);
				Assert.IsNotNull(invoice);
				Assert.AreEqual(2, invoice.Allocations.Count);
				tx.Commit();
			}
		}


		[TestMethod]
		public void CanUpdateAllocationInOneQuery()
		{
			object invoiceId = null;
			using (var tx = Session.BeginTransaction())
			{
				var invoice = new Invoice { Number = "VS001" };

				invoice.AddAllocation(new Conjecture("First", 100), 50);
				invoice.AddAllocation(new Conjecture("Second", 200), 10);

				foreach (var conjecture in invoice.Allocations.Select(x => x.Conjecture))
				{
					Session.Save(conjecture);
				}

				invoiceId = Session.Save(invoice);

				tx.Commit();
			}
			Console.WriteLine("Clear session");
			Session.Clear();
			using (var tx = Session.BeginTransaction())
			{
				var invoice = Session.Get<Invoice>(invoiceId);
				Assert.IsNotNull(invoice);
				Assert.AreEqual(2, invoice.Allocations.Count);

				invoice.Allocations[0].Amount = 10;
				invoice.Allocations[0].UpdaterName = "HAL9000";

				Session.Update(invoice);

				tx.Commit();
			}
		}
	}
}
