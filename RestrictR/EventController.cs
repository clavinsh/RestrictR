#nullable enable
using DataPacketLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static RestrictR.PipeCommunication;

namespace RestrictR
{
    // This class partially corresponds to Notikums (Event) module in the documentation
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

        // Documentation function ID - EVENT_VIEW
        public async Task<DataPacketLibrary.Models.Event?> GetEvent(int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.BlockedApps)
                .Include(e => e.BlockedSites).ThenInclude(e => e.BlockedWebsiteUrls)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            return ev;
        }

        // Documentation function ID - EVENT_CREATE
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

        // Documentation function ID - EVENT_EDIT
        public async Task<OperationResult> EditEvent(Event eventEdited)
        {
            try
            {
                var eventForEditing = await _context.Events
                   .Include(e => e.BlockedApps)
                   .Include(e => e.BlockedSites).ThenInclude(e => e.BlockedWebsiteUrls)
                   .FirstOrDefaultAsync(e => e.EventId == eventEdited.EventId)
                    ?? throw new KeyNotFoundException("Event does not exist");

                eventForEditing.Title = eventEdited.Title;
                eventForEditing.Start = eventEdited.Start;
                eventForEditing.Duration = eventEdited.Duration;
                eventForEditing.Recurrence = eventEdited.Recurrence;
                eventForEditing.BlockedApps = eventEdited.BlockedApps;
                eventForEditing.BlockedSites = eventEdited.BlockedSites;

                await _context.SaveChangesAsync();
                await SendConfig("updated");
                return new OperationResult(success: true, eventEdited);
            }
            catch (Exception ex)
            {
                // Log the exception details, return an error message, etc.
                return new OperationResult(success: false, error: ex.Message);
            }
        }

        // Documentation function ID - EVENT_DELETE
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
