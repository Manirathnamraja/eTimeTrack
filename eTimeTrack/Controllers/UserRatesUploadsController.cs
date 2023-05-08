using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Xml;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class UserRatesUploadsController : BaseController
    {
        public ActionResult ImportRatesTemplates()
        {

            return View();
        }

        [HttpPost]
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public JsonResult UserRatesUpload2(UserRatesUploadCreateViewModel importExcel)
        {
            if (ModelState.IsValid)
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                string path = Server.MapPath("~/Content/Upload/" + importExcel.file.FileName);
                importExcel.file.SaveAs(path);

                string excelConnectionString = @"Provider='Microsoft.ACE.OLEDB.12.0';Data Source='" + path + "';Extended Properties='Excel 12.0 Xml;IMEX=1'";
                OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);

                //Sheet Name
                excelConnection.Open();
                string tableName = excelConnection.GetSchema("Tables").Rows[0]["TABLE_NAME"].ToString();
                excelConnection.Close();
                //End

                OleDbCommand cmd = new OleDbCommand("Select * from [" + tableName + "]", excelConnection);

                excelConnection.Open();

                OleDbDataReader dReader;


                dReader = cmd.ExecuteReader();
                SqlBulkCopy sqlBulk = new SqlBulkCopy(connectionString);

                //Give your Destination table name
                sqlBulk.DestinationTableName = "UserRates";

                //Mappings
                sqlBulk.ColumnMappings.Add("StartDate", "StartDate");
                sqlBulk.ColumnMappings.Add("EndDate", "EndDate");
                //sqlBulk.ColumnMappings.Add("Person", "Person");
                //sqlBulk.ColumnMappings.Add("Item", "Item");
                //sqlBulk.ColumnMappings.Add("Units", "Units");
                //sqlBulk.ColumnMappings.Add("Unit Cost", "UnitCost");
                //sqlBulk.ColumnMappings.Add("Total", "Total");

                sqlBulk.WriteToServer(dReader);
                excelConnection.Close();

                ViewBag.Result = "Successfully Imported";
            }
            return Json(true);
        }

        [HttpPost]
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public JsonResult UserRatesUpload(UserRatesUploadCreateViewModel importExcel)
        {
            if (ModelState.IsValid)
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                string path = Server.MapPath("~/Content/Upload/" + importExcel.file.FileName);
                importExcel.file.SaveAs(path);

                string excelConnectionString = @"Provider='Microsoft.ACE.OLEDB.12.0';Data Source='" + path + "';Extended Properties='Excel 12.0 Xml;IMEX=1'";
                OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);

                //Sheet Name
                excelConnection.Open();
                string tableName = excelConnection.GetSchema("Tables").Rows[0]["TABLE_NAME"].ToString();
                excelConnection.Close();
                //End

                OleDbCommand cmd = new OleDbCommand("Select * from [" + tableName + "]", excelConnection);

                excelConnection.Open();

                DataSet ds = new DataSet();
                OleDbDataAdapter oda = new OleDbDataAdapter("Select * from [" + tableName + "]", excelConnection);
                excelConnection.Close();
                oda.Fill(ds);

                DataTable Exceldt = ds.Tables[0];
                Exceldt.Clear();

                if (!Exceldt.Columns.Contains("StartDate"))
                    Exceldt.Columns.Add("StartDate", typeof(DateTime));
                if (!Exceldt.Columns.Contains("EndDate"))
                    Exceldt.Columns.Add("EndDate", typeof(DateTime));
                //   Exceldt.Columns.Add("EndDate", typeof(DateTime));
                if (!Exceldt.Columns.Contains("ProjectUserClassificationID"))
                    Exceldt.Columns.Add("ProjectUserClassificationID", typeof(int));

                // Exceldt = cmd.ExecuteReader();
                //  connection();
                excelConnection.Open();
                //creating object of SqlBulkCopy
                SqlBulkCopy objbulk = new SqlBulkCopy(connectionString);
                //assigning Destination table name
                objbulk.DestinationTableName = "UserRates";
                //Mapping Table column
                objbulk.ColumnMappings.Add("StartDate", "StartDate");
                objbulk.ColumnMappings.Add("EndDate", "EndDate");
                objbulk.ColumnMappings.Add("ProjectUserClassificationID", "ProjectUserClassificationID");
                //inserting Datatable Records to DataBase
                //   con.Open();
                objbulk.WriteToServer(Exceldt);
                excelConnection.Close();

            }

            ViewBag.Result = "Successfully Imported";

            return Json(true);
        }

        [HttpPost]
        public ActionResult ImportRatesTemplates(UserRatesUploadCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return InvokeHttp404(HttpContext);
            }
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }
            // string path = Server.MapPath("~/Content/Upload/" + model.file.FileName);
            // model.file.SaveAs(path);

            //   UserRatesUpload userRateUpload = Db.UserRatesUploads.SingleOrDefault(x => x.ProjectId == project.ProjectID);
            model.ProjectList = GenerateDropdownUserProjects();
            InfoMessage message;
            UserRatesUpload userRatesUpload = new UserRatesUpload()
            {
                ProjectId = projectId,
                ProjectUserClassificationIDColumn = model.ProjectUserClassification,
                StartDateColumn = model.StartDate,
                EndDateColumn = model.EndDate,
                IsRatesConfirmedColumn = model.IsRatesConfirmed,
                //Fee Rates and Cost Rates
                NTFeeRateColumn = model.NTFeeRate,
                NTCostRateColumn = model.NTCostRate,
                OT1FeeRateColumn = model.OT1FeeRate,
                OT1CostRateColumn = model.OT1CostRate,
                OT2FeeRateColumn = model.OT2FeeRate,
                OT2CostRateColumn = model.OT2CostRate,
                OT3FeeRateColumn = model.OT3FeeRate,
                OT3CostRateColumn = model.OT3CostRate,
                OT4FeeRateColumn = model.OT4FeeRate,
                OT4CostRateColumn = model.OT4CostRate,
                OT5FeeRateColumn = model.OT5FeeRate,
                OT5CostRateColumn = model.OT5CostRate,
                OT6FeeRateColumn = model.OT6FeeRate,
                OT6CostRateColumn = model.OT6CostRate,
                OT7FeeRateColumn = model.OT7FeeRate,
                OT7CostRateColumn = model.OT7CostRate,
                FilePath = model.file.FileName,
                AddedBy = UserHelpers.GetCurrentUserId(),
                AddedDate = DateTime.Now,
            };
            Db.UserRatesUploads.Add(userRatesUpload);

            Db.SaveChanges();
            message = new InfoMessage
            {
                MessageType = InfoMessageType.Success,
                MessageContent = "Successfully updated User Rates Uploads."
            };
            TempData["message"] = message;


            return View();
        }

        private void ImportRatesTemplates2(HttpPostedFileBase file)
        {
            try
            {
                var dbConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                DataSet ds = new DataSet();

                if (Request.Files["file"].ContentLength > 0)
                {
                    string fileExtension =
                                         System.IO.Path.GetExtension(Request.Files["file"].FileName);
                    if (fileExtension == ".xls" || fileExtension == ".xlsx")
                    {
                        string fileLocation = Server.MapPath("~/Content/Upload") + Request.Files["file"].FileName;
                        if (System.IO.File.Exists(fileLocation))
                        {
                            System.IO.File.Delete(fileLocation);
                        }
                        Request.Files["file"].SaveAs(fileLocation);
                        string excelConnectionString = string.Empty;
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                        //connection String for xls file format.
                        if (fileExtension == ".xls")
                        {
                            excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                        }
                        //connection String for xlsx file format.
                        else if (fileExtension == ".xlsx")
                        {
                            excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                        }
                        //Create Connection to Excel work book and add oledb namespace
                        OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                        excelConnection.Open();
                        DataTable dt = new DataTable();

                        dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                        if (dt == null)
                        {
                            // return null;
                        }
                        String[] excelSheets = new String[dt.Rows.Count];
                        int t = 0;
                        //excel data saves in temp file here.
                        foreach (DataRow row in dt.Rows)
                        {
                            excelSheets[t] = row["TABLE_NAME"].ToString();
                            t++;
                        }
                        OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                        string query = string.Format("Select * from [{0}]", excelSheets[0]);
                        using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                        {
                            dataAdapter.Fill(ds);
                        }
                    }
                    if (fileExtension.ToString().ToLower().Equals(".xml"))
                    {
                        string fileLocation = Server.MapPath("~/Content/Upload") + Request.Files["FileUpload"].FileName;
                        if (System.IO.File.Exists(fileLocation))
                        {
                            System.IO.File.Delete(fileLocation);
                        }

                        Request.Files["FileUpload"].SaveAs(fileLocation);
                        XmlTextReader xmlreader = new XmlTextReader(fileLocation);
                        ds.ReadXml(xmlreader);
                        xmlreader.Close();
                    }
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string constr = dbConnectionString;
                        SqlConnection con = new SqlConnection(constr);
                        // CONVERT(DATE, '5/10/2016', 103)
                        string query = "Insert into UserRates(EmployeeId,ProjectId,StartDate,EndDate) Values(19463,1541,'" + ds.Tables[0].Rows[i][0].ToString() + "','" + ds.Tables[0].Rows[i][1].ToString() + "')";
                        SqlCommand cmd = new SqlCommand(query, con);
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        ViewBag.Message = "value inserted ";
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

    }
}