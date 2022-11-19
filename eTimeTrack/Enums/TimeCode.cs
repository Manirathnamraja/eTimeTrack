using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.Enums
{
    public enum TimeCode
    {
        [Display(Name = "NT: Normal Time")]
        NT = 0,
        [Display(Name = "OT1: Other Time 1")]
        OT1 = 1,
        [Display(Name = "OT2: Other Time 2")]
        OT2 = 2,
        [Display(Name = "OT3: Other Time 3")]
        OT3 = 3
    }
}