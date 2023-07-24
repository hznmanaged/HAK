using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.AppointmentKeeper.Services
{
    public class CalendarService
    {
        private readonly GraphService _graphService;
        private readonly SettingsContext _settingsContext;

        public CalendarService(GraphService graphService, SettingsContext settingsContext)
        {
            _graphService = graphService;
            _settingsContext = settingsContext;
        }

        public async Task<CalendarEvent> GetNextCalendarEvent(DateTimeOffset? currentTime = null)
        {
            if(!currentTime.HasValue)
            {
                currentTime = DateTimeOffset.Now;
            }

            if(_settingsContext.GraphEnabled)
            {
                var client = await _graphService.GetClient();
                return await _graphService.GetNextCalendarEvent(client, currentTime.Value);
            }

            return new CalendarEvent()
            {
                StartTime = DateTimeOffset.Now
            };
        }
    }
}
