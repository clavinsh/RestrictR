using DataPacketLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestrictRService
{
    public interface IWebsiteBlocker
    {
        public void SetBlockedWebsites(BlockedWebsites blockedWebsites);
        public void RemoveBlockedWebsites();
    }
}
