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
        OT3 = 3,
        //Added new 4 time codes
        [Display(Name = "OT4: Other Time 4")]
        OT4 = 4,
        [Display(Name = "OT5: Other Time 5")]
        OT5 = 5,
        [Display(Name = "OT6: Other Time 6")]
        OT6 = 6,
        [Display(Name = "OT7: Other Time 7")]
        OT7 = 7
    }
}