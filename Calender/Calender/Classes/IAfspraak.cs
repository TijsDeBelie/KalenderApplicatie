using System;

namespace Calender
{
    interface IAfspraak
    {
        DateTime EndTime { get; set; }
        DateTime StartTime { get; set; }
        string Subject { get; set; }
    }
}