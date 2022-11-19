using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using eTimeTrack.Extensions;
using eTimeTrack.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace eTimeTrack.Helpers
{
    public static class PrintFriendlyTimesheetExcel
    {
        public static FileInfo WriteTimesheet(EmployeeTimesheet timesheet, FileInfo savePath, DateTime dateTime)
        {
            using (ExcelPackage package = new ExcelPackage(
                new FileInfo(HttpContext.Current.Server.MapPath("~/Content/Templates/print_timesheet_template.xlsx")), false))
            {
                ExcelWorkbook workbook = package.Workbook;
                ExcelWorksheet ws = workbook.Worksheets[1];

                // write header information
                WriteHeaderInfo(timesheet, ws);

                // write timesheet items and comments
                WriteItems(timesheet, workbook, ws);

                // write time
                ws.HeaderFooter.differentFirst = false;
                ws.HeaderFooter.differentOddEven = false;
                ws.HeaderFooter.OddFooter.LeftAlignedText = dateTime.ToDateTimeStringGeneral();

                // delete templates sheet
                package.Workbook.Worksheets.Delete(package.Workbook.Worksheets[2]);

                package.SaveAs(savePath);

                return savePath;
            }
        }

        private static void WriteItems(EmployeeTimesheet timesheet, ExcelWorkbook workbook, ExcelWorksheet ws)
        {
            const int firstRow = 9;
            const int colWeekDay = 2;
            const int colComments = 3;
            int row = firstRow;

            ExcelRange itemTemplate = workbook.Worksheets[2].Cells[2, 1, 2, 13];
            ExcelRange commentsTemplate = workbook.Worksheets[2].Cells[5, 1, 5, 13];

            foreach (EmployeeTimesheetItem item in timesheet.TimesheetItems.Where(x => 
                (x.Day1Hrs.HasValue && x.Day1Hrs != 0) ||
                (x.Day2Hrs.HasValue && x.Day2Hrs != 0) ||
                (x.Day3Hrs.HasValue && x.Day3Hrs != 0) ||
                (x.Day4Hrs.HasValue && x.Day4Hrs != 0) ||
                (x.Day5Hrs.HasValue && x.Day5Hrs != 0) ||
                (x.Day6Hrs.HasValue && x.Day6Hrs != 0) ||
                (x.Day7Hrs.HasValue && x.Day7Hrs != 0)
                ))
            {
                int itemStartRow = row;

                // write item values
                itemTemplate.Copy(ws.Cells[row, 2]);
                ws.Cells[row, 2].Value = item.ProjectTask.Project.DisplayName;
                ws.Cells[row, 3].Value = item.ProjectTask.GetParentProjectPart().DisplayName;
                ws.Cells[row, 4].Value = item.ProjectTask.DisplayName;
                ws.Cells[row, 5].Value = item.Variation.DisplayName;
                ws.Cells[row, 6].Value = item.Day1Hrs;
                ws.Cells[row, 7].Value = item.Day2Hrs;
                ws.Cells[row, 8].Value = item.Day3Hrs;
                ws.Cells[row, 9].Value = item.Day4Hrs;
                ws.Cells[row, 10].Value = item.Day5Hrs;
                ws.Cells[row, 11].Value = item.Day6Hrs;
                ws.Cells[row, 12].Value = item.Day7Hrs;
                ws.Cells[row, 13].Value = item.TotalHours();
                ws.Cells[row, 14].Value = item.Comments;

                ws.Cells[row, 2, row, 5].Style.Font.Bold = true;
                ws.Cells[row, 6, row, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                row++;

                // write comments if any
                List<Tuple<string, string>> comments = GetDailyComments(item).ToList();

                foreach (Tuple<string, string> comment in comments)
                {
                    commentsTemplate.Copy(ws.Cells[row, 2]);
                    ws.Cells[row, colWeekDay].Value = comment.Item1;
                    ws.Cells[row, colComments].Value = comment.Item2;
                    row++;
                }

                ws.Cells[itemStartRow, 2, row - 1, 14].Style.Border.BorderAround(ExcelBorderStyle.Medium);

                row++;
            }

            // write totals
            WriteTotalRow(timesheet, workbook, ws, row);
        }

        private static void WriteTotalRow(EmployeeTimesheet timesheet, ExcelWorkbook workbook, ExcelWorksheet ws, int row)
        {
            decimal day1Hours = timesheet.TimesheetItems.Select(x => x.Day1Hrs ?? 0).Sum();
            decimal day2Hours = timesheet.TimesheetItems.Select(x => x.Day2Hrs ?? 0).Sum();
            decimal day3Hours = timesheet.TimesheetItems.Select(x => x.Day3Hrs ?? 0).Sum();
            decimal day4Hours = timesheet.TimesheetItems.Select(x => x.Day4Hrs ?? 0).Sum();
            decimal day5Hours = timesheet.TimesheetItems.Select(x => x.Day5Hrs ?? 0).Sum();
            decimal day6Hours = timesheet.TimesheetItems.Select(x => x.Day6Hrs ?? 0).Sum();
            decimal day7Hours = timesheet.TimesheetItems.Select(x => x.Day7Hrs ?? 0).Sum();

            ExcelRange totalTemplate = workbook.Worksheets[2].Cells[8, 1, 8, 13];
            totalTemplate.Copy(ws.Cells[row, 2]);
            ws.Cells[row, 6].Value = day1Hours;
            ws.Cells[row, 7].Value = day2Hours;
            ws.Cells[row, 8].Value = day3Hours;
            ws.Cells[row, 9].Value = day4Hours;
            ws.Cells[row, 10].Value = day5Hours;
            ws.Cells[row, 11].Value = day6Hours;
            ws.Cells[row, 12].Value = day7Hours;
            ws.Cells[row, 13].Value = day1Hours + day2Hours + day3Hours + day4Hours + day5Hours + day6Hours + day7Hours;
        }

        private static IEnumerable<Tuple<string, string>> GetDailyComments(EmployeeTimesheetItem item)
        {
            if (!string.IsNullOrWhiteSpace(item.Day1Comments))
                yield return new Tuple<string, string>("Saturday", item.Day1Comments);
            if (!string.IsNullOrWhiteSpace(item.Day2Comments))
                yield return new Tuple<string, string>("Sunday", item.Day2Comments);
            if (!string.IsNullOrWhiteSpace(item.Day3Comments))
                yield return new Tuple<string, string>("Monday", item.Day3Comments);
            if (!string.IsNullOrWhiteSpace(item.Day4Comments))
                yield return new Tuple<string, string>("Tuesday", item.Day4Comments);
            if (!string.IsNullOrWhiteSpace(item.Day5Comments))
                yield return new Tuple<string, string>("Wednesday", item.Day5Comments);
            if (!string.IsNullOrWhiteSpace(item.Day6Comments))
                yield return new Tuple<string, string>("Thursday", item.Day6Comments);
            if (!string.IsNullOrWhiteSpace(item.Day7Comments))
                yield return new Tuple<string, string>("Friday", item.Day7Comments);
        }

        private static void WriteHeaderInfo(EmployeeTimesheet timesheet, ExcelWorksheet ws)
        {
            const int rowHeaderEmployee = 2;
            const int rowHeaderNames = 3;
            const int rowHeaderEmail = 4;
            const int rowHeaderPeriod = 5;
            const int colHeaders = 3;

            ws.Cells[rowHeaderEmployee, colHeaders].Value = timesheet.Employee.EmployeeNo;
            ws.Cells[rowHeaderNames, colHeaders].Value = timesheet.Employee.Names;
            ws.Cells[rowHeaderEmail, colHeaders].Value = timesheet.Employee.Email;
            ws.Cells[rowHeaderPeriod, colHeaders].Value = timesheet.TimesheetPeriod.GetStartEndDates();
        }
    }
}