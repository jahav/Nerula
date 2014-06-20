using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nerula.Data;

namespace Nerula.Test
{
	/// <summary>
	/// Tests demonstrating how does caching works with database procedures/queries.
	/// </summary>
	[TestClass]
	public class CacheEvict : DatabaseTest
	{
		private int savedBlogId;

		[TestInitialize]
		public void Init()
		{
			DatabaseSetup(typeof(Blog), typeof(Post));

			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var blog = new Blog { Name = "Stop worrying..." };
					session.Save(blog);
					savedBlogId = blog.Id;
					session.Save(new Post { Blog = blog, Title = "... and start programming." });
					session.Save(new Post { Blog = blog, Title = "... and start testing." });

					tx.Commit();
				}
			}
		}

		[TestCleanup]
		public void TestCleanup()
		{
			DatabaseTearDown();
		}


		[TestMethod]
		[Description("Only 1st lvl cache")]
		public void EvictThenFlush_DoesntKeepChanges()
		{
			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var blog = session.Get<Blog>(savedBlogId);
					var oldName = blog.Name;
					var newName = "Another Name";
					blog.Name = newName;

					// We schedule an update, but as long as it is not flushed, the DB is not aware of change
					// The change is only tracked
					session.Update(blog);
					Assert.AreEqual(newName, blog.Name);

					// We remove the object from session tracking, thus NH is not aware it should update it
					session.Evict(blog);

					// We update all tracked objects, but the blog is not.
					session.Flush();
					Assert.AreEqual(newName, blog.Name);

					blog = session.Get<Blog>(savedBlogId);
					Assert.AreEqual(oldName, blog.Name);
					tx.Commit();
				}
			}
		}

		[TestMethod]
		[Description("Only 1st lvl cache")]
		public void FlushThenEvict_KeepsChanges()
		{
			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var blog = session.Get<Blog>(savedBlogId);
					var oldName = blog.Name;
					var newName = "Another Name";
					blog.Name = newName;
					session.Update(blog);
					Assert.AreEqual(newName, blog.Name);

					session.Flush();

					session.Evict(blog);
					Assert.AreEqual(newName, blog.Name);

					blog = session.Get<Blog>(savedBlogId);
					Assert.AreEqual(newName, blog.Name);
					tx.Commit();
				}
			}
		}

		[TestMethod]
		[Description("Only 1st lvl cache")]
		public void SqlUpdateDoesntAffectingTrackedData()
		{
			using (var session = SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var blog = session.Get<Blog>(savedBlogId);
					var oldName = blog.Name;
					var newName = "Another Title";
					var sqlQuery = session.CreateSQLQuery("UPDATE Blog set Name=:newName").SetString("newName", newName);
					sqlQuery.UniqueResult();

					Assert.AreEqual(oldName, blog.Name);
					session.Evict(blog);
					blog = session.Get<Blog>(savedBlogId);
					Assert.AreEqual(newName, blog.Name);

					tx.Commit();
				}
			}
		}
	}
}
