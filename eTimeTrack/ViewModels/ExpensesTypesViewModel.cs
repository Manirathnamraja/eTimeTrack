using eTimeTrack.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eTimeTrack.ViewModels
{
    public class ExpensesTypesViewModel
    {
        public List<ExpensesTypes> ExpensesTypesDetails { get; set; }
    }

    public class ExpensesTypes
    {
        public int ExpenseTypeID { get; set; }
        public int ProjectId { get; set; }
        public string ExpenseType { get; set; }
        public int TaskID { get; set; }
        public string Name { get; set; }
        public int VariationID { get; set; }
        public string Description { get; set; }
        public bool IsClosed { get; set; }

    }
}