using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using eTimeTrack.Extensions;
using eTimeTrack.Models;
using Novacode;
using OfficeOpenXml.Style;

namespace eTimeTrack.Helpers
{
    public static class PrintFriendlyTimesheetWord
    {
        public static FileInfo WriteTimesheet(EmployeeTimesheet timesheet, FileInfo savePath, DateTime dateTime)
        {
            using (DocX document = DocX.Create(savePath.FullName))
            //using (ExcelPackage package = new ExcelPackage(
            //    new FileInfo(HttpContext.Current.Server.MapPath("~/Content/Templates/print_timesheet_template.xlsx")), false))
            {
                document.ApplyTemplate(HttpContext.Current.Server.MapPath("~/Content/Templates/print_timesheet_template.docx"));
                //ExcelWorkbook workbook = package.Workbook;
                //ExcelWorksheet ws = workbook.Worksheets[1];

                Table table = document.Tables.First();

                // write header information
                WriteHeaderInfo(timesheet, table);

                // write timesheet items and comments
                WriteItems(timesheet, table);

                // write time
                //ws.HeaderFooter.differentFirst = false;
                //ws.HeaderFooter.differentOddEven = false;
                //ws.HeaderFooter.OddFooter.LeftAlignedText = dateTime.ToDateTimeStringGeneral();

                // delete templates sheet
                //package.Workbook.Worksheets.Delete(package.Workbook.Worksheets[2]);

                document.Save();

                return savePath;
            }
        }

        private static void WriteItems(EmployeeTimesheet timesheet, Table table)
        {
            const int firstRow = 5;
            const int colWeekDay = 0;
            const int colComments = 1;
            int row = firstRow;

            //ExcelRange itemTemplate = workbook.Worksheets[2].Cells[2, 1, 2, 13];
            //ExcelRange commentsTemplate = workbook.Worksheets[2].Cells[5, 1, 5, 13];

            List<double> widths = table.Rows[row].Cells.Select(x => x.Width).ToList();

            foreach (EmployeeTimesheetItem item in timesheet.TimesheetItems)
            {
                int itemStartRow = row;

                table.InsertRow();
                for (int i = 0; i < widths.Count; i++)
                {
                    table.Rows[row].Cells[i].Width = widths[i];
                }

                int col = 0;
                // write item values
                //itemTemplate.Copy(table.Cells[row, 2]);
                table.Rows[row].Cells[col++].Paragraphs.First().InsertText(item.ProjectTask.Project.DisplayName);
                table.Rows[row].Cells[col++].Paragraphs.First().InsertText(item.ProjectTask.GetParentProjectPart().DisplayName);
                table.Rows[row].Cells[col++].Paragraphs.First().InsertText(item.ProjectTask.DisplayName);
                table.Rows[row].Cells[col++].Paragraphs.First().InsertText(item.Variation.DisplayName);
                table.Rows[row].Cells[col++].Paragraphs.First().InsertText(item.Day1Hrs.ToString());
                table.Rows[row].Cells[col++].Paragraphs.First().InsertText(item.Day2Hrs.ToString());
                table.Rows[row].Cells[col++].Paragraphs.First().InsertText(item.Day3Hrs.ToString());
                table.Rows[row].Cells[col++].Paragraphs.First().InsertText(item.Day4Hrs.ToString());
                table.Rows[row].Cells[col++].Paragraphs.First().InsertText(item.Day5Hrs.ToString());
                table.Rows[row].Cells[col++].Paragraphs.First().InsertText(item.Day6Hrs.ToString());
                table.Rows[row].Cells[col++].Paragraphs.First().InsertText(item.Day7Hrs.ToString());
                table.Rows[row].Cells[col++].Paragraphs.First().InsertText(item.TotalHours().ToString());
                table.Rows[row].Cells[col].Paragraphs.First().InsertText(item.Comments);

                //table.Cells[row, 2, row, 5].Style.Font.Bold = true;

       
                row++;

                // write comments if any
                foreach (var comment in GetDailyComments(item))
                {
                    table.InsertRow();
                    for (int i = 0; i < widths.Count; i++)
                    {
                        table.Rows[row].Cells[i].Width = widths[i];
                    }
                    //commentsTemplate.Copy(table.Cells[row, 2]);
                    table.Rows[row].Cells[colWeekDay].Paragraphs.First().InsertText(comment.Item1);
                    table.Rows[row].MergeCells(colComments, table.Rows[row].Cells.Count - 1);

                    // delete list of paragraphs in merged cells
                    var commentsParagraphs = table.Rows[row].Cells[colComments].Paragraphs;
                    for (int i = 1; i < commentsParagraphs.Count; i++)
                    {
                        table.Rows[row].Cells[colComments].RemoveParagraphAt(0);
                    }

                    //foreach (Paragraph para in commentsParagraphs)
                    //{
                    //    table.Rows[row].Cells[colComments].RemoveParagraph(para);
                    //}


                    table.Rows[row].Cells[colComments].Paragraphs.First().InsertText(comment.Item2.Trim());
                    row++;
                }

                //table.Cells[itemStartRow, 2, row - 1, 14].Style.Border.BorderAround(ExcelBorderStyle.Medium);

                //row++;
            }
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

        private static void WriteHeaderInfo(EmployeeTimesheet timesheet, Table table)
        {
            const int rowHeaderEmployee = 0;
            const int rowHeaderEmail = 1;
            const int rowHeaderPeriod = 2;
            const int colHeaders = 1;

            table.Rows[rowHeaderEmployee].Cells[colHeaders].Paragraphs.First().InsertText(timesheet.Employee.EmployeeNo);
            table.Rows[rowHeaderEmail].Cells[colHeaders].Paragraphs.First().InsertText(timesheet.Employee.Email);
            table.Rows[rowHeaderPeriod].Cells[colHeaders].Paragraphs.First().InsertText(timesheet.TimesheetPeriod.GetStartEndDates());
        }
    }
}