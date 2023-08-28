using eTimeTrack.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eTimeTrack.ViewModels
{
    public class ExpensesTypesViewModel
    {
        public ExpensesTypes ExpensesTypesDetails { get; set; }
    }

    public class ExpensesTypes
    {
        public int ExpenseTypeID { get; set; }
        public int TaskId { get; set; }
        public int VariationId { get; set; }
        public int ProjectId { get; set; }
        public string ExpenseType { get; set; }
        public ProjectTask DefaultTask { get; set; }
        public ProjectVariation DefaultVariation { get; set; }

    }
}