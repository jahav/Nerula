﻿using System;
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
		public void CanUpdateAllocationInOneQuery()
		{
			int allocatinsCount = 5;
			object invoiceId = CreateInvoiceWithAllocations("VS001", allocatinsCount);

			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var invoice = session.Get<Invoice>(invoiceId);
					Assert.IsNotNull(invoice);
					Assert.AreEqual(allocatinsCount, invoice.Allocations.Count);

					invoice.Allocations[0].Amount = 10;
					invoice.Allocations[0].UpdaterName = "HAL9000";

					session.Update(invoice);

					tx.Commit();
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
