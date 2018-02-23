using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace PLS.Tests
{
    public static class IntegTestUtils
    {
        public static void WithDb(Action<PlsDbContext> task)
        {
            var container = Application.BuildContainer();
            var db = container.GetService<PlsDbContext>();
            db.Database.EnsureCreated();
            try
            {
                task(db);
            }
            finally
            {
                db.Database.EnsureDeleted();
                db.Dispose();
            }
        }
    }

    public class Test3
    {
        [Fact]
        public void ExportAlcuin1()
        {
            Application.Parse("config", "export", "http://localhost/ConfigApi/Alcuin1", "admin", "123", "alcuin1.json");
        }
        [Fact]
        public void ServerList()
        {
            Application.Parse("server", "list");
        }
        [Fact]
        public void ListTenant()
        {
            Application.Parse("tenant", "list");
        }

        [Fact]
        public void Copy()
        {
            var hom = new ServerTasks(new Server
            {
                Id = "HOM",
                Hostname = "HOM-BDD02-2014",
                Login = "alcuinSQL_HOM",
                Password = "YFLILKOyqxxG9q9RTWeS"
            });
            var server = new ServerTasks(new Server
            {
                Id = "localhost",
                Hostname = "localhost",
                Login = "sa",
                Password = "P@ssw0rd"
            });
            server.Copy(hom, "MDC_ENSSUP_HOMOL_EVO_ADM");
        }

        [Fact]
        public void FullTest()
        {
            Application.Parse("server", "add", "localhost", "localhost", "sa", "P@ssw0rd", "C:\\Program Files\\Microsoft SQL Server\\MSSQL14.MSSQLSERVER\\MSSQL");
            Application.Parse("tenant", "add", "Alcuin1", "localhost", "Alcuin1", "Alcuin1_ADM");
            Application.Parse("tenant", "restore", "Alcuin1");
        }
    }

    public class Test2
    {
        [Fact]
        public void CreateServer_ShouldWork()
        {
            IntegTestUtils.WithDb(db =>
            {
                db.Add(new Server
                {
                    Hostname = "localhost",
                    Id = "localhost",
                    Login = "admin",
                    Password = "123"
                });
                db.SaveChanges();

                var server = db.Find<Server>("localhost");

                Assert.Equal("admin", server.Login);
                Assert.Equal("localhost", server.Hostname);
                Assert.Equal("localhost", server.Id);
                Assert.Equal("123", server.Password);
            });
        }

        [Fact]
        public void CreateTenant_ShouldWork()
        {
            IntegTestUtils.WithDb(db => {
                db.Add(new Server
                {
                    Hostname = "localhost",
                    Id = "localhost",
                    Login = "admin",
                    Password = "123"
                });
                db.Add(new Tenant
                {
                    ConfigDb = "Alcuin1_ADM",
                    PublicDb = "Alcuin1",
                    Id = "Alcuin1",
                    ServerId = "localhost"
                });
                db.SaveChanges();

                var tenant = db.Find<Tenant>("Alcuin1");

                Assert.Equal("Alcuin1", tenant.Id);
                Assert.Equal("Alcuin1_ADM", tenant.ConfigDb);
                Assert.Equal("Alcuin1", tenant.PublicDb);
                Assert.Equal("localhost", tenant.ServerId);
            });
        }
    }
}