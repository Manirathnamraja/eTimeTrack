using System;
using System.Linq;
using OfficeOpenXml;

namespace eTimeTrack.Extensions
{
    public static class ExcelExtensions
    {
        public static int GetColumn(this ExcelWorksheet ws, string name)
        {
            int? column = null;
            try
            {
                column = ws.Cells["1:1"].SingleOrDefault(x => x.Text == name).FirstOrDefault()?.Start.Column;
            }
            catch (ArgumentNullException e)
            {
                ThrowColumnError(name);
            }

            if (column == null)
            {
                ThrowColumnError(name);
            }

            return (int)column;
        }

        private static void ThrowColumnError(string name)
        {
            throw new Exception($"Invalid file - required fields not found: '{name}'. Please use a template downloaded from eTimeTrack to ensure the structure is correct");
        }
    }
}