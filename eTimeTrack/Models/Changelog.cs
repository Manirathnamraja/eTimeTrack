using System;
using System.Data.Entity;

namespace eTimeTrack.Models
{
    public class Changelog
    {
        public Guid Id { get; set; }
        public DateTime DateTimeUtc { get; set; }
        public int EmployeeId { get; set; }
        public string OriginalData { get; set; }
        public string NewData { get; set; }

        public string IdOfChangedObject { get; set; }
        public string TypeOfChangedObject { get; set; }
        public EntityState ChangeType { get; set; }


        public virtual Employee Employee { get; set; }
    }
}