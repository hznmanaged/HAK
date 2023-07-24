using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.AppointmentKeeper.Exceptions
{
    public class NoCalenderEventsException : Exception
    {
        public NoCalenderEventsException(string message) : base(message: message) { }
    }
}
