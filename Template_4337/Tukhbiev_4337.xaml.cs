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

        private void ImportJson_OnClick(object sender, RoutedEventArgs e)
        {
            List<Fourth> fourthList;
            using (var fs = new FileStream("C:\\Users\\Admin\\Documents\\2.json", FileMode.OpenOrCreate))
            {
                using (var reader = new StreamReader(fs))
                {
                    fourthList = JsonConvert.DeserializeObject<List<Fourth>>(reader.ReadToEnd(), new IsoDateTimeConverter ()
                    {
                        DateTimeFormat = "dd.MM.yyyy"
                    });
                }
            }

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                if (fourthList is null) return;
                foreach (var command in fourthList.Select(fourth => new SqlCommand(
                             $"INSERT INTO main (m_or_id, m_date, m_cl_id, m_services, m_status) VALUES ({fourth.CodeOrder.Split('/').First()}, '{fourth.CreateDate}', {fourth.CodeClient}, '{fourth.Services}', '{fourth.Status}')",
                             conn)))
                {
                    command.ExecuteNonQuery();
                }
            }

        }

        private void Exword_OnClick(object sender, RoutedEventArgs e)
        {
            var fourthList = new List<List<Fourth>>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand command =
                    new SqlCommand(
                        "SELECT m_id, m_or_id, m_date, m_cl_id, m_services, m_status FROM main GROUP BY m_id, m_or_id, m_date, m_cl_id, m_services, m_status ORDER BY m_status",
                        conn);

                using (var reader = command.ExecuteReader())
                {
                    var previousStatus = "В прокате";
                    if (reader.HasRows) // если есть данные
                    {
                        var fourthTuples =
                            new List<Fourth>();

                        while (reader.Read()) // построчно считываем данные
                        {
                            var fourth = new Fourth();
                            fourth.Id = (int) reader.GetValue(0);
                            fourth.CodeOrder = (string) reader.GetValue(1);
                            fourth.CreateDate = (string) reader.GetValue(2);
                            fourth.CodeClient = (int) reader.GetValue(3);
                            fourth.Services = (string) reader.GetValue(4);
                            fourth.Status = (string) reader.GetValue(5);
                            if (previousStatus != fourth.Status)
                            {
                                previousStatus = fourth.Status;
                                fourthList.Add(fourthTuples.ToArray().ToList());
                                fourthTuples.Clear();
                            }

                            fourthTuples.Add(fourth);
                        }
                        fourthList.Add(fourthTuples.ToArray().ToList());
                    }
                }
            }

            try
            {
                var winword = new Microsoft.Office.Interop.Word.Application();

                winword.ShowAnimation = false;

                winword.Visible = false;

                object missing = System.Reflection.Missing.Value;
                
                var document =
                    winword.Documents.Add(ref missing, ref missing, ref missing, ref missing);

                document.Content.SetRange(0, 0);

                var para1 = document.Content.Paragraphs.Add(ref missing);

                foreach (var fourthsTable in fourthList)
                {
                    var firstTable = document.Tables.Add(para1.Range, fourthsTable.Count + 1, 6, ref missing, ref missing);

                    firstTable.Borders.Enable = 1;
                    
                    var firstRow = firstTable.Rows[1];
                    firstRow.Cells[1].Range.Text = "Id";
                    firstRow.Cells[2].Range.Text = "Код заказа";
                    firstRow.Cells[3].Range.Text = "Дата";
                    firstRow.Cells[4].Range.Text = "Код клиента";
                    firstRow.Cells[5].Range.Text = "Услуги";
                    firstRow.Cells[6].Range.Text = "Статус";

                    for (var i = 0; i < fourthsTable.Count; i++)
                    {
                        var row = firstTable.Rows[i + 2];
                        row.Cells[1].Range.Text = $"{fourthsTable[i].Id}";
                        row.Cells[2].Range.Text = $"{fourthsTable[i].CodeOrder}";
                        row.Cells[3].Range.Text = $"{fourthsTable[i].CreateDate}";
                        row.Cells[4].Range.Text = $"{fourthsTable[i].CodeClient}";
                        row.Cells[5].Range.Text = $"{fourthsTable[i].Services}";
                        row.Cells[6].Range.Text = $"{fourthsTable[i].Status}";
                    }
                    foreach (Row row in firstTable.Rows)
                    {
                        foreach (Cell cell in row.Cells)
                        {
                            if (cell.RowIndex == 1)
                            {
                                cell.Range.Font.Bold = 1;
                                cell.Range.Font.Name = "verdana";
                                cell.Range.Font.Size = 10;                            
                                cell.Shading.BackgroundPatternColor = WdColor.wdColorGray25;
                                cell.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                                cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                            }
                        }
                    }
                    para1.Range.InsertBreak();
                }

                //Save the document  
                object filename = Path.Combine("C:\\Users\\Admin\\Documents\\", "2.docx");
                document.SaveAs2(ref filename);
                document.Close(ref missing, ref missing, ref missing);
                document = null;
                winword.Quit(ref missing, ref missing, ref missing);
                winword = null;
                MessageBox.Show("Document created successfully !");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        public class Fourth
        {
            public int Id { get; set; }
            public string CodeOrder { get; set; }
            public string CreateDate { get; set; }
            public int CodeClient { get; set; }
            public string Services { get; set; }
            public string Status { get; set; }
        }

    }

}


