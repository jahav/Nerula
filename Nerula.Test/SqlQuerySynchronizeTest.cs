using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nerula.Data;
using NHibernate.Linq;
using System.Linq;

namespace Nerula.Test
{
    /// <summary>
    /// Tests demonstrating the synchronize tag functionality for sql-query.
    /// </summary>
    [TestClass]
    public class SqlQuerySynchronizeTest : DatabaseTest
    {
        [TestInitialize]
        public void Init()
        {
            DatabaseSetup(typeof(Project), typeof(Blog), typeof(Post));
        }

        public void AddProjects(int count)
        {
            using (var session = SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    for (int i = 1; i<= count; i++) {
                        session.Save(new Project { Code = string.Format("Project {0}", i) });
                    }
                    tx.Commit();
                }
            }
        }

        [TestMethod]
        public void SqlQueryWithSynchronize()
        {
            AddProjects(3);
            using (var session = SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    var project = session.Query<Project>().First();
                    project.Code = "Test";

                    // Because the query contains synchronize tag for table Project (which affects the entity Project), we flush
                    // all entites Project, thus syncing with DB
                    var deletedCount = session.GetNamedQuery("DeleteProjectSync").SetString("Code", "Test").ExecuteUpdate();
                    Assert.AreEqual(1, deletedCount);

                    tx.Commit();
                }
            }
        }

        [TestMethod]
        public void SqlQueryWithoutSynchronize()
        {
            AddProjects(3);
            using (var session = SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    var project = session.Query<Project>().First();
                    project.Code = "Test";

                    // The change of the Code in the code above is not flushed, because of missing Synchronization, thus
                    // from DB point of view, the project hasn't changed its code and no row is deleted
                    var deletedCount = session.GetNamedQuery("DeleteProject").SetString("Code", "Test").ExecuteUpdate();
                    Assert.AreEqual(0, deletedCount);

                    tx.Commit();
                }
            }
        }

        [TestMethod]
        public void SqlQueryWithWrongSynchronize()
        {
            AddProjects(3);
            using (var session = SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    var project = session.Query<Project>().First();
                    project.Code = "Test";

                    // We are flushing entities related to the Blog table, not the Product table,
                    // so in the DB, the project row still has original code and isn't deleted
                    var deletedCount = session.GetNamedQuery("DeleteProjectSyncWrongTable").SetString("Code", "Test").ExecuteUpdate();
                    Assert.AreEqual(0, deletedCount);

                    tx.Commit();
                }
            }
        }
    }
}
