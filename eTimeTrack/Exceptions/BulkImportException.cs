using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eTimeTrack.Models;

namespace eTimeTrack.Exceptions
{
    public class BulkImportException : Exception
    {
        public ImportStatus Status { get; set; }
    }
}