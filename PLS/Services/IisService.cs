using System;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Web.Administration;
using Optional;
using Optional.Collections;
using PLS.Utils;

namespace PLS.Services
{
    public class IisService : IIisService
    {
        private readonly ServerManager _server;

        public IisService(ServerManager server)
        {
            _server = server;
        }

        public Option<ApplicationPool> GetPool(string name)
        {
            return _server.ApplicationPools.FirstOrNone(_ => _.Name == name);
        }

        public ApplicationPool GetPoolOrCreate(string pool)
        {
            return _server.ApplicationPools.FirstOrDefault(_ => _.Name == pool) ?? _server.ApplicationPools.Add(pool);
        }

        public void DropApplication(string path)
        {
            var maybeApp =
                        (from site in _server.Sites
                        from app in site.Applications
                        where app.Path == path
                        select new { App=app, List=site.Applications }).FirstOrNone();
            maybeApp.MatchSome(pair => { pair.List.Remove(pair.App); });
        }

        public void CreateApplication(string pool, string path, string physicalPath, Site site = null)
        {
            if (!_server.Sites.Any())
                throw new Exception("No website set in IIS.");
            var adminPool = GetPoolOrCreate(pool);
            site = site ?? _server.Sites[0];

            site.Applications
                .FirstOrNone(_ => _.Path == path)
                .MatchSome(site.Applications.Remove);

            var app = site.Applications.Add(path, physicalPath);
            app.ApplicationPoolName = adminPool.Name;
            _server.CommitChanges();
        }
    }
}