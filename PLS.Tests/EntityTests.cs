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
        public void ListTenant()
        {
            Application.Parse("tenant", "list");
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
                    AppName = "Alcuin1",
                    ConfigDb = "Alcuin1_ADM",
                    PublicDb = "Alcuin1",
                    Id = "Alcuin1",
                    ServerId = "localhost"
                });
                db.SaveChanges();

                var tenant = db.Find<Tenant>("Alcuin1");

                Assert.Equal("Alcuin1", tenant.Id);
                Assert.Equal("Alcuin1", tenant.AppName);
                Assert.Equal("Alcuin1_ADM", tenant.ConfigDb);
                Assert.Equal("Alcuin1", tenant.PublicDb);
                Assert.Equal("localhost", tenant.ServerId);
            });
        }
    }
}