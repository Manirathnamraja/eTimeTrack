using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class LU_PayType : ITrackableModel
    {
        [Key]
        public int PayTypeID { get; set; }
        [StringLength(10, ErrorMessage = "Maximum length is 10")]
        public string PayTypeCode { get; set; }
        [StringLength(100, ErrorMessage = "Maximum length is 100")]
        public string PayTypeDescription { get; set; }
        public bool IsActive { get; set; }

        public string GetId()
        {
            return PayTypeID.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}