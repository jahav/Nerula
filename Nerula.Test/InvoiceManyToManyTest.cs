using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nerula.Data;

namespace Nerula.Test
{
	[TestClass]
	public class InvoiceManyToManyTest : DatabaseTest
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
		public void TestMethod1()
		{
		}
	}
}
