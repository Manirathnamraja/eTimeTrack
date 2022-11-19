using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eTimeTrack.Models;

namespace eTimeTrack.Exceptions
{
    public class StatusException : Exception
    {
        public InfoMessageType Status { get; set; }

        public StatusException(string message, InfoMessageType status) : base(message)
        {
            Status = status;
        }
    }
}