using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.AppointmentKeeper.Exceptions
{
    public class CalendarNotSetException: Exception
    {
        public CalendarNotSetException(string message) : base(message: message) { }

    }
}
