using DataPacketLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace RestrictRService
{
    //used to to retrieve events from the database
    public class EventController
    {

        private RestrictRDbContext _context;

        public EventController(RestrictRDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetEvents()
        {
            var list = await _context.Events
                .Include(e => e.BlockedApps)
                .Include(e => e.BlockedSites).ThenInclude(e => e.BlockedWebsiteUrls)
                .ToListAsync();

            return list;
        }

        public async Task<Event?> GetEvent(int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.BlockedApps)
                .Include(e => e.BlockedSites).ThenInclude(e => e.BlockedWebsiteUrls)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            return ev;
        }
    }
}
