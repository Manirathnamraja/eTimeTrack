using System;
using System.ComponentModel;

namespace eTimeTrack.ViewModels
{
    public class ReconciliationTemplatesIndexViewModel
    {
        public int Id { get; set; }
        [DisplayName("Template Name")]
        public string Name { get; set; }
        [DisplayName("Company")]
        public string CompanyName { get; set; }
        [DisplayName("Last Modified")]
        public DateTime? LastModifiedDateTime { get; set; }
    }
}