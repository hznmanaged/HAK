using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.AppointmentKeeper
{
    public class CalendarEvent
    {
        public DateTimeOffset StartTime;
        public DateTimeOffset EndTime;
        public string Name;
        public string ID;
        public CalendarEvent NextEvent;
    }
}
