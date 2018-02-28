using System;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Web.Administration;
using Optional;

namespace PLS.Utils
{
    public static class ServerManagerExtensions
    {
        public static Site GetDefaultWebsite(this ServerManager self) => self.Sites.Any()
            ? self.Sites[0]
            : throw new Exception("This server has no website configured.");
    }

    public static class ServiceControllerExtensions
    {
        public static bool IsRunning(this ServiceController self) => self.Status == ServiceControllerStatus.Running || self.Status == ServiceControllerStatus.StartPending;
        public static bool IsStopped(this ServiceController self) => self.Status == ServiceControllerStatus.Stopped || self.Status == ServiceControllerStatus.StopPending;
    }
}