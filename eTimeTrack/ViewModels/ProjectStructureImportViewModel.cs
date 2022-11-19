using System.ComponentModel;
using System.Web;
using System.Web.Mvc;

namespace eTimeTrack.ViewModels
{
    public class ProjectStructureImportViewModel
    {
        [DisplayName("Project")]
        public int ProjectId { get; set; }
        public HttpPostedFileBase File { get; set; }
        public SelectList ProjectList { get; set; }
        [DisplayName("Concatenate Level Codes?")]
        public bool ConcatenateCodes { get; set; }
        public string ConcatenateCharacter { get; set; }
    }
}