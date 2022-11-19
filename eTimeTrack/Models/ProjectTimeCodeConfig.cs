using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eTimeTrack.Models
{
    public class ProjectTimeCodeConfig
    {
        [Key]
        public int ProjectTimeCodeConfigId { get; set; }
        public int ProjectID { get; set; }

        public bool DisplayOT1 { get; set; }
        public bool DisplayOT2 { get; set; }
        public bool DisplayOT3 { get; set; }

        public string NTName { get; set; }
        public string OT1Name { get; set; }
        public string OT2Name { get; set; }
        public string OT3Name { get; set; }

        public string NTNotes { get; set; }
        public string OT1Notes { get; set; }
        public string OT2Notes { get; set; }
        public string OT3Notes { get; set; }
        //Added new 4 time codes
        public bool DisplayOT4 { get; set; }
        public bool DisplayOT5 { get; set; }
        public bool DisplayOT6 { get; set; }
        public bool DisplayOT7 { get; set; }
        public string OT4Name { get; set; }
        public string OT5Name { get; set; }
        public string OT6Name { get; set; }
        public string OT7Name { get; set; }
        public string OT4Notes { get; set; }
        public string OT5Notes { get; set; }
        public string OT6Notes { get; set; }
        public string OT7Notes { get; set; }

        [ForeignKey("ProjectID")]
        [Required]
        public virtual Project Project { get; set; }
    }
}