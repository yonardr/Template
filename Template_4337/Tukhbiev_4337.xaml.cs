using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Office.Interop.Word;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OfficeOpenXml;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;
using Task = System.Threading.Tasks.Task;
using Window = System.Windows.Window;




namespace Template_4337
{
    public partial class Tukhbiev_4337 : Window
    {
        string connectionString = "Server=localhost;Database=isrpo_2;Trusted_Connection=True;";

        public Tukhbiev_4337()
        {
            InitializeComponent();
        }

        private void BnImport_Click(object sender, RoutedEventArgs e)
        {
            
            string path1 = "C:\\Users\\Admin\\Documents\\1.xlsx";
            var excelFile = new FileInfo(path1);
            var excelTuples = new List<(int id, string orderId, string date, int clientId, string services, string status)>();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (ExcelPackage excelPackage = new ExcelPackage(excelFile))
            {
                // Getting the complete workbook...
                var currentWorkbook = excelPackage.Workbook;

                foreach (var currentSheet in currentWorkbook.Worksheets)
                {
                    for (int i = 2; i < currentSheet.Dimension.Rows ; i++)
                    {
                        //var range = currentSheet.Cells[1, 3, i, 3];
                        //range.Style.Numberformat.Format = "dd/MM/yyyy hh:mm:ss";
                        int id = Convert.ToInt32(currentSheet.Cells[i, 1].Value);
                        string orderId = Convert.ToString(currentSheet.Cells[i, 2].Value);
                        string date = Convert.ToString(currentSheet.Cells[i, 3].Value);
                        var clientId = Convert.ToInt32(currentSheet.Cells[i, 5].Value);
                        var services = Convert.ToString(currentSheet.Cells[i, 6].Value);
                        var status = Convert.ToString(currentSheet.Cells[i, 7].Value);
                        excelTuples.Add(new ValueTuple<int, string, string, int, string, string>(
                            (int)id, orderId, date, (int)clientId, services, status));
                    }
                }
            }

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (var tuple in excelTuples)
                {
                    SqlCommand command =
                        new SqlCommand(
                            $"INSERT INTO main (m_or_id, m_date, m_cl_id, m_services, m_status) VALUES ('{tuple.orderId}', '{tuple.date}', {tuple.clientId}, '{tuple.services}', '{tuple.status}')",
                            conn);
                    command.ExecuteNonQuery();
                }
            }


        }

        private void BnExport_Click(object sender, RoutedEventArgs e)
        {
            string path2 = "C:\\Users\\Admin\\Documents\\2.xlsx";
            var excelFile = new FileInfo(path2);

            var excelSheets = new List<List<(int id, string orderId, string date, int clientId, string services, string status)>>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand command =
                    new SqlCommand(
                        "SELECT m_id, m_or_id, m_date, m_cl_id, m_services, m_status FROM main  GROUP BY m_id, m_or_id, m_date, m_cl_id, m_services, m_status ORDER BY m_status",
                        conn);

                using (var reader = command.ExecuteReader())
                {
                    var previousStatus = "В прокате";
                    if (reader.HasRows) // если есть данные
                    {
                        var excelTuples =
                            new List<(int id, string orderId, string date, int clientId, string services, string status)>();

                        while (reader.Read()) // построчно считываем данные
                        {
                            var id = (int)reader.GetValue(0);
                            var orderId = (string)reader.GetValue(1);
                            var date = (string)reader.GetValue(2);
                            var clientId = (int)reader.GetValue(3);
                            var services = (string)reader.GetValue(4);
                            var status = (string)reader.GetValue(5);
                            if (previousStatus != status)
                            {
                                previousStatus = status;
                                excelSheets.Add(excelTuples.ToArray().ToList());
                                excelTuples.Clear();
                            }

                            excelTuples.Add(new ValueTuple<int, string, string, int, string, string>(
                                id, orderId, date, clientId, services, status));
                        }

                        excelSheets.Add(excelTuples.ToArray().ToList());
                    }
                }
            }

            foreach (var sheet in excelSheets)
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (ExcelPackage excelPackage = new ExcelPackage(excelFile))
                {
                    // Getting the complete workbook...
                    var currentWorkbook = excelPackage.Workbook;

                    if (sheet.Count != 0)
                    {
                        var currentSheet = currentWorkbook.Worksheets.FirstOrDefault();
                        if (currentSheet is null)
                        {
                            currentSheet = currentWorkbook.Worksheets.Add($"{sheet}");
                        }

                        var currentRow = 1;

                        foreach (var row in sheet)
                        {
                            var range = currentSheet.Cells[1, 3, currentRow, 3];
                            range.Style.Numberformat.Format = "dd/MM/yyyy hh:mm:ss";
                            currentSheet.Cells[currentRow, 1].Value = row.id;
                            currentSheet.Cells[currentRow, 2].Value = row.orderId;
                            currentSheet.Cells[currentRow, 3].Value = row.date.ToString();
                            currentSheet.Cells[currentRow, 4].Value = row.clientId;
                            currentSheet.Cells[currentRow, 5].Value = row.services;
                            currentRow++;
                        }

                        // Saving the change...
                        excelPackage.Save();
                    }
                }
            }



        }

    }

}


