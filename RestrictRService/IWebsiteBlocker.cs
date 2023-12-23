using DataPacketLibrary.Models;

namespace RestrictRService
{
    public interface IWebsiteBlocker
    {
        public void SetBlockedWebsites(BlockedWebsites blockedWebsites);
        public void RemoveBlockedWebsites();
    }
}
