using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text;
using System.Web;
using System.Drawing.Text;
using Font = System.Drawing.Font;

namespace Bingo
{
    public partial class Form1 : Form
    {

        DataTable dt;
        DataTable DtNew;
        int dtrowCount;
        int dtnewrowCount;
        string path;
        string fullPath;
        string strtDate = string.Empty;
        string strtEnd = string.Empty;
        string strExcelPath;
        string strCsvPath;
        int ifirstprice = Convert.ToInt32(ConfigurationManager.AppSettings["firstprize"].ToString());
        int isecondprice = Convert.ToInt32(ConfigurationManager.AppSettings["secondprize"]?.ToString());
        // int ithirdprice = Convert.ToInt32(ConfigurationManager.AppSettings["thirdprize"].ToString());
        string strFirstprizetext = ConfigurationManager.AppSettings["firstprizetxt"].ToString();
        string strSecondprizetext = ConfigurationManager.AppSettings["secondprizetxt"]?.ToString();
        string strThirdprizetext = ConfigurationManager.AppSettings["thirdprizetxt"]?.ToString();
        string myValue = ConfigurationManager.AppSettings["Headertext"]?.ToString();
        string strColourFirst = ConfigurationManager.AppSettings["colourfirst"]?.ToString();
        string strColourSecond = ConfigurationManager.AppSettings["coloursecond"]?.ToString();
        string strCsvfolderpath = ConfigurationManager.AppSettings["Csvfolderpath"]?.ToString();
        bool isAutoUpload = Convert.ToBoolean(ConfigurationManager.AppSettings["Autoupload"]?.ToString());
        public Form1()
        {
            InitializeComponent();
            PrivateFontCollection privateFonts = new PrivateFontCollection();
            privateFonts.AddFontFile(@".\Sample File\Poppins-Bold.ttf");
            lblHeading.Font = new Font(privateFonts.Families[0], 20, FontStyle.Bold);
            lblHeading.Text = HttpUtility.HtmlDecode(myValue);
            if(isAutoUpload)
            {
                autoUpload.Visible = false;
                btnUpload.Visible = false;
                EventHandler handler = autoUpload_Click;
                handler.Invoke(this, EventArgs.Empty);
                
            }
            else
            {
                btnUpload.Visible = true;
                autoUpload.Visible = false;
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {

            textBox1.Text = string.Empty;
            recordCount.Text = string.Empty;
            OpenFileDialog op1 = new OpenFileDialog();
            op1.Multiselect = true;
            op1.ShowDialog();
            op1.Filter = "CSV files (*.csv)|*.csv";
            dataGridView1.DataSource = ConvertCSVtoDataTable1(op1.FileName);
            dataGridView2.DataSource = DtNew = null;
            strtDate = string.Empty;

            strtEnd = string.Empty;

        }



        public DataTable ConvertCSVtoDataTable1(string strFilePath)
        {
            try
            {

                StreamReader sr = new StreamReader(strFilePath);
                string[] headers = Regex.Split(sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                DataTable dtcsv = new DataTable();
                foreach (string header in headers)
                {
                    dtcsv.Columns.Add(header.Replace("\"", "").Replace("'", ""));
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = Regex.Split(sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                    if (rows.Length == headers.Length)
                    {
                        DataRow dr = dtcsv.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i];
                            if (dtcsv.Columns[i].DataType == typeof(string))
                            {
                                rows[i] = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes((string)rows[i].Replace("\"", "").Replace("'", "")));
                                //rows[i] = ((string)rows[i].Replace("\"", "").Replace("'", ""));
                            }
                        }

                        dtcsv.Rows.Add(dr);
                    }
                }
                dt = dtcsv;
                dtrowCount = dt.Rows.Count;
                recordCount.Text = "Count  :" + dtrowCount.ToString() + "  Records Loaded";

                int rowCount = 1;

                foreach (DataColumn column in dtcsv.Columns)
                {
                    if (column.ColumnName == "TXNDATE")
                    {
                        foreach (DataRow row in dtcsv.Rows)
                        {

                            if (rowCount == 1)
                                strtDate = "Transaction loaded between " + row.ItemArray[column.Ordinal].ToString().Replace("00:00", " ");
                            if (rowCount == dtcsv.Rows.Count)
                                strtEnd = " to " + row.ItemArray[column.Ordinal].ToString().Replace("00:00", " ");
                            rowCount++;

                        }
                    }
                }
                txtHeader.Text = string.Concat(strtDate, strtEnd);
                return dtcsv;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured while uploading data :" + ex.Message.ToString());
                return null;
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                int count;
                bool isValid = int.TryParse(textBox1.Text.ToString(), out count);
                if (dt != null && dt.Rows.Count > 0 && dtrowCount != dtnewrowCount)
                {
                    if (dtrowCount >= (dtnewrowCount + count))
                    {
                        var rand = new Random();
                        List<DataRow> list1 = dt.AsEnumerable().ToList();


                        List<DataRow> randomList = new List<DataRow>();

                        var list = dt.AsEnumerable().ToList();
                        List<int> randomNumbers = new List<int>();
                        var random = new Random();
                        if (!string.IsNullOrEmpty(textBox1.Text.ToString()))
                        {

                            if (isValid && count > 0 && count <= dt.Rows.Count)
                            {
                                do
                                {
                                    int index = random.Next(list.Count);
                                    if (!randomNumbers.Contains(index))
                                    {
                                        randomNumbers.Add(index);
                                        randomList.Add(list[index]);


                                    }
                                } while (randomList.Count() < count);

                                DataTable DtRandom = new DataTable();
                                DtRandom = randomList.CopyToDataTable();
                                if (DtNew != null && DtNew.Rows.Count > 0)
                                    DtNew.Merge(DtRandom);
                                else
                                {
                                    DtNew = new DataTable();
                                    DtNew.Merge(DtRandom);
                                }

                                //Logic to add extra columns that is not available in the csv
                                AddAdditionalColumns();
                                //

                                var rows = dt.AsEnumerable().Except(DtNew.AsEnumerable(), DataRowComparer.Default);
                                if (rows.Count() != 0)
                                    dt = rows.CopyToDataTable();

                                var groups = DtNew.AsEnumerable().GroupBy(row => row.Field<string>("REFNO")).Where(group => group.Count() > 1);
                                if (groups.Any())
                                {
                                    MessageBox.Show("Duplicate Ref.Number exits.Please verify");

                                }
                                dataGridView1.DataSource = dt;
                                int j = 0;
                                int k = ifirstprice+1;
                                int rowCount = DtNew.Rows.Count;
                                for(int i=0;i< rowCount;i++)
                                {
                                    if (j <= ifirstprice - 1)
                                    {

                                        dataGridView2.Rows[j].DefaultCellStyle.BackColor = ColorTranslator.FromHtml(strColourFirst);
                                    }
                                    else if (j > ifirstprice && j <= (ifirstprice + isecondprice))
                                    {
                                            dataGridView2.Rows[j-1].DefaultCellStyle.BackColor = ColorTranslator.FromHtml(strColourSecond);
                                            k++;
                                    }
                                    j++;

                                }

                                j = 0;
                                //if (DtNew.Rows.Count <= ifirstprice)
                                //{
                                //    dataGridView2.Rows[DtNew.Rows.Count].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FFD700");
                                //}
                                //else if (DtNew.Rows.Count >= ifirstprice && DtNew.Rows.Count <= isecondprice)
                                //{
                                //    dataGridView2.Rows[0].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("Blue");
                                //}

                            }
                            else
                            {
                                if (count > dt.Rows.Count)
                                {
                                    MessageBox.Show("Number should be less than uploaded data!!");
                                }
                                if (!isValid)
                                {
                                    MessageBox.Show("Enter a Valid Number!!");
                                }

                            }
                        }
                        else
                        {
                            MessageBox.Show("Enter Count");
                        }
                    }
                    else
                    {
                        MessageBox.Show("No more records to add");
                    }
                }
                else
                {
                    if (dtrowCount == dtnewrowCount)
                    {
                        MessageBox.Show("No Data Available!!!");
                    }
                    else
                    {
                        MessageBox.Show("Upload Data");
                    }
                }
                Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception : " + ex.Message.ToString());
                Cursor = Cursors.Arrow;
            }
        }

        private void AddAdditionalColumns()
        {
            try
            {
                AdditionalColumns(DtNew);
                DataView view = new DataView(DtNew);
                DtNew = view.ToTable("DtNew", false, "SNO", "TXNDATE", "REFNO", "CUSTOMERNAME", "IDNO", "AMOUNT", "CONTACTNO", "RESULT");
                dataGridView2.DataSource = DtNew;
                dtnewrowCount = DtNew.Rows.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception :" + ex.Message.ToString() + " Uploaded data doesnot contain valid columns");
                throw;

            }
        }

        public void AdditionalColumns(DataTable DtNew)
        {
            try
            {
                if (!ContainColumn("SNO", DtNew))
                {
                    DataColumn newCol = new DataColumn("SNO");
                    DtNew.Columns.Add(newCol);
                }
                int i = 1;
                foreach (DataRow row in DtNew.Rows)
                {

                    row["SNO"] = i;
                    i++;

                }
                if (!ContainColumn("RESULT", DtNew))
                {
                    DataColumn newCol = new DataColumn("RESULT");
                    DtNew.Columns.Add(newCol);
                }
                int j = 1;

                foreach (DataRow row in DtNew.Rows)
                {
                    if (j <= ifirstprice)
                    {
                        row["Result"] = strFirstprizetext;
                    }
                    else if (j > ifirstprice && j <= (ifirstprice + isecondprice) && isecondprice > 0)
                    {
                        row["Result"] = strSecondprizetext;
                    }
                    else if (isecondprice > 0)
                    {
                        row["Result"] = strThirdprizetext;
                    }

                    j++;

                }
                i = 0;
                j = 0;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        private static bool ContainColumn(string columnName, DataTable table)
        {
            DataColumnCollection columns = table.Columns;
            if (columns.Contains(columnName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void clear()
        {
            dtnewrowCount = 0;
            dtrowCount = 0;
            dt = null;
            DtNew = null;
            dataGridView1.DataSource = null;
            dataGridView2.DataSource = null;
            textBox1.Text = string.Empty;
            txtHeader.Text = string.Empty;
            recordCount.Text = string.Empty;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            clear();
            if (isAutoUpload)
            {
                
                btnUpload.Visible = true;
                EventHandler handler = autoUpload_Click;
                handler.Invoke(this, EventArgs.Empty);

            }
        }
        public void ExportToPdf(DataTable myDataTable)
        {
            DataTable dt = myDataTable;
            Document pdfDoc = new Document();
            iTextSharp.text.Font font13 = FontFactory.GetFont("ARIAL", 13);
            iTextSharp.text.Font font6 = FontFactory.GetFont("ARIAL", 6);
            iTextSharp.text.Font headerFont = FontFactory.GetFont("HELVETICA", 15);
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                path = ConfigurationManager.AppSettings["PDFDownloadPath"].ToString();
                fullPath = Path.Combine(path, "Dohaexchange_Promotion_Draw_Results" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf");
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                pdfDoc.Open();

                if (dt.Rows.Count > 0)
                {
                    PdfPTable PdfTable = new PdfPTable(1);

                    PdfPCell PdfPCell = new PdfPCell();
                    string imageURL = @".\Sample File\DEX_2.png";
                    BaseFont bf = BaseFont.CreateFont("c:\\windows\\fonts\\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 8);

                    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
                    jpg.ScaleToFit(pdfDoc.PageSize.Width, 75);
                    pdfDoc.Add(jpg);
                    string texttoDisplay = "Doha Exchange Promotion Draw Results";
                    Paragraph para = new Paragraph(texttoDisplay, headerFont);
                    para.Alignment = Element.ALIGN_CENTER;
                    pdfDoc.Add(para);



                    PdfTable = new PdfPTable(dt.Columns.Count);
                    PdfTable.SpacingBefore = 25f;
                    for (int columns = 0; columns <= dt.Columns.Count - 1; columns++)
                    {
                        PdfPCell = new PdfPCell(new Phrase(new Chunk(dt.Columns[columns].ColumnName, font6)));
                        PdfTable.AddCell(PdfPCell);
                    }

                    for (int rows = 0; rows <= dt.Rows.Count - 1; rows++)
                    {
                        for (int column = 0; column <= dt.Columns.Count - 1; column++)
                        {

                            PdfPCell = new PdfPCell(new Phrase(new Chunk(Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(dt.Rows[rows][column].ToString())), font)));
                            PdfTable.AddCell(PdfPCell);
                        }
                    }
                    pdfDoc.Add(PdfTable);
                }
                pdfDoc.AddCreationDate();
                pdfDoc.Close();






            }
            catch (DocumentException de)
            {
                MessageBox.Show("Exception :" + de.Message.ToString());
            }

            catch (IOException ioEx)
            {
                MessageBox.Show("Exception :" + ioEx.Message.ToString());
            }

            catch (Exception ex)
            {
                MessageBox.Show("Exception :" + ex.Message.ToString());
            }
        }

        private static void DrawLine(PdfWriter writer, float x1, float y1, float x2, float y2, BaseColor color)
        {
            PdfContentByte contentByte = writer.DirectContent;
            contentByte.SetColorStroke(color);
            contentByte.MoveTo(x1, y1);
            contentByte.LineTo(x2, y2);
            contentByte.Stroke();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (DtNew != null && DtNew.Rows.Count > 0)
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains("PDFDownloadPath"))
                {
                    ExportToPdf(DtNew);
                    MessageBox.Show("Exported to " + fullPath);
                }
                else
                {
                    MessageBox.Show("PDF Download path not configured. Please update path in appconfig");
                }

            }
            else
                MessageBox.Show("No data available");
        }



        private void btnExcel_Click(object sender, EventArgs e)
        {

            if (DtNew != null && DtNew.Rows.Count > 0)
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains("ExcelDownloadPath"))
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                    string filepath = timestamp + "_" + "data.xlsx";

                    strExcelPath = ConfigurationManager.AppSettings["ExcelDownloadPath"].ToString() + filepath;
                    ExportToExcel(DtNew, strExcelPath);
                    MessageBox.Show("Exported to " + strExcelPath);
                }
                else
                {
                    MessageBox.Show("Excel Download path not configured. Please update path in appconfig");
                }

            }
            else
            {
                MessageBox.Show("No data available");
                return;
            }


        }

        public void ExportToExcel(DataTable dtExcel, string strExcelPath)
        {
            // Create a new Excel application
            Excel.Application excel = new Excel.Application();

            // Create a new workbook
            Excel.Workbook workbook = excel.Workbooks.Add();

            // Create a new worksheet
            Excel.Worksheet worksheet = workbook.ActiveSheet;

            // Set the column headers
            for (int i = 0; i < dtExcel.Columns.Count; i++)
            {
                worksheet.Cells[1, i + 1] = dtExcel.Columns[i].ColumnName;
            }

            // Set the cell values
            for (int i = 0; i < dtExcel.Rows.Count; i++)
            {
                for (int j = 0; j < dtExcel.Columns.Count; j++)
                {
                    worksheet.Cells[i + 2, j + 1] = dtExcel.Rows[i][j].ToString();
                }
            }

            workbook.SaveAs(strExcelPath);
            // Save the workbook


            // Close the workbook and release the resources
            workbook.Close();
            excel.Quit();




        }

        private void autoUpload_Click(object sender, EventArgs e)
        {
            
            if (Directory.Exists(strCsvfolderpath))
            {
                string[] files = Directory.GetFiles(strCsvfolderpath, "*.csv");

                if (files.Length > 0)
                {
                    strCsvPath = Path.GetFileName(files[0]);
                    strCsvPath = strCsvfolderpath + "\\" + strCsvPath;


                }
                textBox1.Text = string.Empty;
                recordCount.Text = string.Empty;
                dataGridView1.DataSource = ConvertCSVtoDataTable1(strCsvPath);
                dataGridView2.DataSource = DtNew = null;
                strtDate = string.Empty;

                strtEnd = string.Empty;
            }
            else
            {
                MessageBox.Show("Invalid Path "+ strCsvfolderpath);
            }
        }
    }
}
