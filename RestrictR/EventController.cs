#nullable enable
using Microsoft.EntityFrameworkCore;
using DataPacketLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RestrictR.PipeCommunication;

namespace RestrictR
{
    public class EventController
    {
        private RestrictRDbContext _context;

        public EventController(RestrictRDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DataPacketLibrary.Models.Event>> GetEvents()
        {
            var list = await _context.Events
                .Include(e => e.BlockedApps)
                .Include(e => e.BlockedSites).ThenInclude(e => e.BlockedWebsiteUrls)
                .ToListAsync();

            return list;
        }

        public async Task<DataPacketLibrary.Models.Event?> GetEvent(int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.BlockedApps)
                .Include(e => e.BlockedSites).ThenInclude(e => e.BlockedWebsiteUrls)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            return ev;
        }

        public async Task<OperationResult> CreateEvent(Event newEvent)
        {
            try
            {
                _context.Add(newEvent);
                await _context.SaveChangesAsync();
                await SendConfig("updated");
                return new OperationResult(success: true, newEvent);
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details, return an error message, etc.
                return new OperationResult(success: false, error: ex.Message);
            }
        }

        public async Task<OperationResult> DeleteEvent(Event eventForDeletion)
        {
            try
            {
                _context.Remove(eventForDeletion);
                await _context.SaveChangesAsync();
                await SendConfig("updated");
                return new OperationResult(success: true);
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details, return an error message, etc.
                return new OperationResult(success: false, error: ex.Message);
            }
        }
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public DataPacketLibrary.Models.Event? Event { get; set; }
        public string? Error { get; set; }

        public OperationResult(bool success, DataPacketLibrary.Models.Event? body = null, string? error = null)
        {
            Success = success;
            Event = body;
            Error = error;
        }
    }
}
