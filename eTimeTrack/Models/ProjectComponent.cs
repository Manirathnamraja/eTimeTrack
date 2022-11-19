using System;
using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.Models
{
    public class ProjectComponent
    {
        [Key]
        public Guid Id { get; set; }
        public IProjectComponent Item { get; set; }
        public bool CanDelete { get; set; }
    }
}