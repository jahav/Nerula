using System.Linq;
using NHibernate.Linq;
using NHibernate.Cfg;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Caches.SysCache;
using Nerula.Data;
using System.Collections.Generic;
using log4net;
using log4net.Appender;
using System.Text.RegularExpressions;

namespace Nerula.Test
{
    /// <summary>
    /// Test evicion from 2nd lvl cache with SQL query.
    /// </summary>
    [TestClass]
    public class Cache2ndLevel : DatabaseTest
    {
        /// <summary>
        /// Appender to catch 2nd level cache events
        /// </summary>
        private readonly MemoryAppender memoryAppender = new MemoryAppender();

        [TestInitialize]
        public void Init2ndLvlCache()
        {
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

        /// <summary>
        /// When I query without a synchronize tag, I evict all entites that are in the cache.
        /// </summary>
        [TestMethod]
        public void UpdateQueryWithoutSynchronizeClearsWhole2ndLvlCache()
        {
            CreateData();

            log4net.Config.BasicConfigurator.Configure(memoryAppender);

            using (var session = SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.GetNamedQuery("UpdateBlogNameWithoutSynchronize")
                        .SetParameter<string>("Name", "New blog title")
                        .SetParameter<int>("Id", 2)
                        .ExecuteUpdate();

                    var expectedEntitesSet = new HashSet<string>()
                    {
                        "Nerula.Data.Project",
                        "Nerula.Data.Blog",
                        "Nerula.Data.Post",
                    };
                    var evictedEntities = GetEvictedEntities();

                    Assert.IsTrue(expectedEntitesSet.SetEquals(evictedEntities));
                    
                    tx.Commit();
                }
            }
        }

        /// <summary>
        /// When ythe synchronize tag in the query is specified, I only clear the entites that are in cache are synchronize tag.
        /// </summary>
        [TestMethod]
        public void UpdateQueryWithSynchronizeClearsOnlyRelated2ndLvlCache()
        {
            CreateData();

            log4net.Config.BasicConfigurator.Configure(memoryAppender);

            using (var session = SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.GetNamedQuery("UpdateBlogNameWithSynchronize")
                        .SetParameter<string>("Name", "New blog name")
                        .SetParameter<int>("Id", 2)
                        .ExecuteUpdate();

                    var expectedEntitesSet = new HashSet<string>()
                    {
                        "Nerula.Data.Blog",
                    };
                    var evictedEntities = GetEvictedEntities();
                    Assert.IsTrue(expectedEntitesSet.SetEquals(evictedEntities));

                    tx.Commit();
                }
            }
        }

        [TestMethod]
        public void SecondGetInSessionDoesntHit2ndLvlCache()
        {
            CreateData();

            log4net.Config.BasicConfigurator.Configure(memoryAppender);

            using (var session = SessionFactory.OpenSession())
            {
                // first cache lookup is hited, because we inserted data
                using (var tx = session.BeginTransaction())
                {
                    session.Get<Blog>(2);
                    tx.Commit();
                } 
                // Second get doesn't hit the cache, because it already is in session level cache
                using (var tx = session.BeginTransaction())
                {
                    session.Get<Blog>(2);
                    tx.Commit();
                }
            }

            var cacheEvents = GetCacheEvents();
            var expectedEvents = new string[] 
            {
                "Cache lookup: Nerula.Data.Blog#2",
                "Cache hit: Nerula.Data.Blog#2",
            };

            CollectionAssert.AreEqual(expectedEvents, cacheEvents);
        }

        [TestMethod]
        public void Full2ndLvlCacheMiss()
        {
            CreateData();

            SessionFactory.EvictEntity(typeof(Blog).FullName);

            log4net.Config.BasicConfigurator.Configure(memoryAppender);

            using (var session = SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.Get<Blog>(2);
                    tx.Commit();
                }
            }

            var cacheEvents = GetCacheEvents();
            var expectedEvents = new string[] 
            {
                "Cache lookup: Nerula.Data.Blog#2",
                "Cache miss: Nerula.Data.Blog#2",
                "Caching: Nerula.Data.Blog#2",
                "Cached: Nerula.Data.Blog#2"
            };

            CollectionAssert.AreEqual(expectedEvents, cacheEvents);
        }

        [TestMethod]
        public void Update2ndLvlCache()
        {
            CreateData();

            log4net.Config.BasicConfigurator.Configure(memoryAppender);

            using (var session = SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.Get<Blog>(2).Name = "New blog name";
                    tx.Commit();
                }
            }

            var cacheEvents = GetCacheEvents();
            var expectedEvents = new string[] 
            {
                "Cache lookup: Nerula.Data.Blog#2",
                "Cache hit: Nerula.Data.Blog#2",
                "Invalidating: Nerula.Data.Blog#2",
                "Updating: Nerula.Data.Blog#2",
                "Updated: Nerula.Data.Blog#2"
            };

            CollectionAssert.AreEqual(expectedEvents, cacheEvents);
        }

        private string[] GetCacheEvents()
        {
            var cacheEvents = memoryAppender.GetEvents()
                .Where(e => e.LoggerName == "NHibernate.Cache.ReadWriteCache")
                .Select(e => e.RenderedMessage)
                .ToArray();
            return cacheEvents;
        }

        private IEnumerable<string> GetEvictedEntities()
        {
            var evictedEntities = memoryAppender.GetEvents()
                //.Where(e => e.RenderedMessage.Contains("evicting second-level cache"))
                .Select(e => Regex.Match(e.RenderedMessage, ".*evicting second-level cache: (.*)").Groups)
                .Where(g => g.Count > 1)
                .Select(g => g[1].Value);
            return evictedEntities;
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
