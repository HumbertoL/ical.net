﻿using System;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// An abstract class from which all iCalendar data types inherit.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class EncodableDataType :
        CalendarDataType,
        IEncodableDataType
    {
        virtual public string Encoding
        {
            get { return Parameters.Get("ENCODING"); }
            set { Parameters.Set("ENCODING", value); }
        }
    }
}
