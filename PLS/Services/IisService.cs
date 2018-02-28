using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Microsoft.Web.Administration;
using Optional;
using Optional.Collections;
using Optional.Unsafe;
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

        public bool Start()
        {
            using (var ctrl = new ServiceController("w3svc", "."))
            {
                if (ctrl.IsRunning())
                {
                    return true;
                }
                ctrl.Start();
                ctrl.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
            }
            return true;
        }

        public Microsoft.Web.Administration.Application CreateApplication(ApplicationPool pool, string path, string physicalPath)
        {
            var site = _server.GetDefaultWebsite();
            var app = site.Applications.Add(path, physicalPath);
            app.ApplicationPoolName = pool.Name;
            _server.CommitChanges();
            return app;
        }

        public ApplicationPool CreatePool(string site)
        {
            var pool = _server.ApplicationPools.Add(site);
            pool.Enable32BitAppOnWin64 = true;
            pool.ManagedRuntimeVersion = "v4.0";
            pool.ProcessModel.IdleTimeout = new TimeSpan();
            pool.ProcessModel.IdentityType = ProcessModelIdentityType.ApplicationPoolIdentity;
            pool.ProcessModel.PingingEnabled = false;
            _server.CommitChanges();
            return pool;
        }

        public bool Stop()
        {
            using (var ctrl = new ServiceController("w3svc", "."))
            {
                if (ctrl.IsStopped())
                {
                    return true;
                }
                ctrl.Stop();
                ctrl.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));
            }
            return true;
        }
    }
}