using System.ComponentModel;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class ReconciliationFileViewModel
    {
        [DisplayName("Template")]
        public int ReconciliationTemplateId { get; set; }
        public int ProjectID { get; set; }
        public HttpPostedFileBase File { get; set; }
        public SelectList TemplateList { get; set; }
        public ReconciliationTemplateBaseViewModel DummyTemplateVm { get; set; }
        public SelectList ProjectList { get; set; }
    }
}