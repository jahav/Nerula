using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nerula.Data;
using NHibernate.Linq;
using System.Linq;
using NHibernate.Proxy;

namespace Nerula.Test
{
	/// <summary>
	/// Testing about NHibernate equals.
	/// Source: http://adrianphinney.com/post/76778957253/an-nhibernate-mess-or-how-i-learned-to-stop-worrying
	/// </summary>
	[TestClass]
	public class NHibernateEqualsTest : DatabaseTest
	{
		[TestInitialize]
		public void Init()
		{
			DatabaseSetup(typeof(Blog), typeof(Post));

			using (var session = this.SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var blog = new Blog { Name = "Stop worrying..." };
					session.Save(blog);
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
		public void RemovePostFromBlog()
		{
			using (var session = this.SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var post = session.Load<Post>(1);
					Assert.IsTrue(post is INHibernateProxy, "Post through load is not a proxy");
					Assert.AreEqual(2, post.Blog.Posts.Count);
					post.Remove();
					Assert.AreEqual(1, post.Blog.Posts.Count);
				}
			}
		}

		[TestMethod]
		public void EqualsIsCalledOnProxy()
		{
			using (var session = this.SessionFactory.OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var post0 = session.Load<Post>(100);
					var post = session.Load<Post>(1);
					Assert.IsTrue(post is INHibernateProxy, "Post through load is not a proxy");

					var post2 = session.Load<Post>(2);
					Assert.IsTrue(post2 is INHibernateProxy, "Post through load is not a proxy");
					var equals = post.Equals(post2);
					Assert.IsFalse(equals);
				}
			}
		}
	}
}
