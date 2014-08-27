using System.Linq;
using NHibernate.Linq;
using NHibernate.Cfg;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Caches.SysCache;
using Nerula.Data;
using System.Collections.Generic;
using log4net;

namespace Nerula.Test
{
    /// <summary>
    /// Test evicion from 2nd lvl cache with SQL query.
    /// </summary>
    [TestClass]
    public class Cache2ndLevel : DatabaseTest
    {
        private static ILog log = LogManager.GetLogger(typeof(Cache2ndLevel));

        [TestInitialize]
        public void Init2ndLvlCache()
        {
            log4net.Config.XmlConfigurator.Configure();
            DatabaseSetup(cfg =>
            {
                cfg
                    .SetProperty(Environment.UseSecondLevelCache, "true")
                    .SetProperty(Environment.CacheDefaultExpiration, "10") // Default 2nd lvl cache expiration, in seconds
                    .SetProperty(Environment.CacheProvider, typeof(SysCacheProvider).AssemblyQualifiedName)
                    .AddAssembly(typeof(Blog).Assembly)
                    .SetCacheConcurrencyStrategy(typeof(Blog).FullName, "read-write")
                    .SetCacheConcurrencyStrategy(typeof(Post).FullName, "read-write")
                    .SetCacheConcurrencyStrategy(typeof(Project).FullName, "read-write");
            });
        }

        [TestMethod]
        public void UpdateQueryWithoutSynchronizeClearsWhole2ndLvlCache()
        {
            CreateData();

            using (var session = SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.GetNamedQuery("UpdatePostTitleWithoutSynchronize")
                        .SetParameter<string>("Title", "New post title")
                        .SetParameter<int>("Id", 2)
                        .ExecuteUpdate();

                    tx.Commit();
                }
            }

        }

        [TestMethod]
        public void UpdateQueryWithSynchronizeClearsOnlyRelated2ndLvlCache()
        {
            CreateData();

            using (var session = SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.GetNamedQuery("UpdateBlogTitleWithSynchronize")
                        .SetParameter<string>("Name", "New blog name")
                        .SetParameter<int>("Id", 2)
                        .ExecuteUpdate();

                    session.Get<Blog>(2);
                    tx.Commit();
                }
            }
        }

        [TestMethod]
        public void UpdateObject2ndLvlCache()
        {
            CreateData();

            SessionFactory.Evict(typeof(Blog));

            using (var session = SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.Load<Blog>(2).Name = "New post title";
                    tx.Commit();
                }
            }
        }

        public void CreateData()
        {
            using (var session = SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.Save(CreateBlog(1, 2));
                    session.Save(CreateBlog(2, 0));
                    session.Save(CreateBlog(4, 4));
                    tx.Commit();
                }
            }
        }

        private Blog CreateBlog(int blogId, int postCount)
        {
            var blog = new Blog { Name = string.Format("Test blog #{0}", blogId) };
            blog.Posts = new HashSet<Post>();
            for (int i = 1; i <= postCount; i++)
            {
                blog.Posts.Add(new Post
                {
                    Blog = blog,
                    Title = string.Format("Post title #{0}", i),
                    Body = string.Format("Post body #{0}", i),
                });
            }
            return blog;
        }
    }
}
