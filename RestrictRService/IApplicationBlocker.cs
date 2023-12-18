using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestrictRService
{
    public interface IApplicationBlocker
    {
        public void SetBlockedApps(List<string> appsInstallLocations);
        public void RemoveBlockedApps();
    }
}
