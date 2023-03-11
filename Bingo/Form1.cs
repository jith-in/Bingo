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
        public Form1()
        {
            InitializeComponent();
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
                                rows[i] = ((string)rows[i].Replace("\"", "").Replace("'", ""));
                            }
                        }

                        dtcsv.Rows.Add(dr);
                    }
                }
                dt = dtcsv;
                dtrowCount = dt.Rows.Count;
                recordCount.Text = dtrowCount.ToString() + " Records Loaded";

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
                if (dt != null && dt.Rows.Count > 0 && dtrowCount != dtnewrowCount)
                {
                    var rand = new Random();
                    List<DataRow> list1 = dt.AsEnumerable().ToList();


                    List<DataRow> randomList = new List<DataRow>();

                    var list = dt.AsEnumerable().ToList();
                    List<int> randomNumbers = new List<int>();
                    var random = new Random();
                    if (!string.IsNullOrEmpty(textBox1.Text.ToString()))
                    {
                        bool isValid = int.TryParse(textBox1.Text.ToString(), out count);
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
                            dataGridView1.DataSource = dt;
                            if (DtNew.Rows.Count >= 1)
                            {
                                dataGridView2.Rows[0].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FFD700");
                            }
                            //if (DtNew.Rows.Count >= 2)
                            //{
                            //    dataGridView2.Rows[1].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FFD700");
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
                DtNew = view.ToTable("DtNew", false, "SNO", "TXNDATE", "REFNO", "CUSTOMERNAME", "IDNO", "AMOUNT", "RESULT");
                dataGridView2.DataSource = DtNew;
                dtnewrowCount = DtNew.Rows.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception :" + ex.Message.ToString() + " Uploaded data doesnot contain valid columns");
                throw;

            }
        }

        private static void AdditionalColumns(DataTable DtNew)
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
                    if (j <= 1)
                    {
                        row["Result"] = "Bumper Prize-50 gm Golden Ball";
                    }
                    else
                    {
                        row["Result"] = "Winner-10 gm Gold Bars";
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
        }
        public void ExportToPdf(DataTable myDataTable)
        {
            DataTable dt = myDataTable;
            Document pdfDoc = new Document();
            iTextSharp.text.Font font13 = FontFactory.GetFont("ARIAL", 13);
            iTextSharp.text.Font font8 = FontFactory.GetFont("ARIAL", 8);
            iTextSharp.text.Font headerFont = FontFactory.GetFont("HELVETICA", 15);
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                path = ConfigurationManager.AppSettings["PDFDownloadPath"].ToString();
                fullPath = Path.Combine(path, "Alzamanexchange_Promotion_Draw_Results.pdf");
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                pdfDoc.Open();

                if (dt.Rows.Count > 0)
                {
                    PdfPTable PdfTable = new PdfPTable(1);

                    PdfPCell PdfPCell = new PdfPCell();
                    string imageURL = @".\Sample File\al-zaman-Logo.png";

                    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
                    pdfDoc.Add(jpg);
                    string texttoDisplay = "Alzaman Exchange Promotion Draw Results";
                    Paragraph para = new Paragraph(texttoDisplay, headerFont);
                    para.Alignment = Element.ALIGN_CENTER;
                    pdfDoc.Add(para);



                    PdfTable = new PdfPTable(dt.Columns.Count);
                    PdfTable.SpacingBefore = 25f;
                    for (int columns = 0; columns <= dt.Columns.Count - 1; columns++)
                    {
                        PdfPCell = new PdfPCell(new Phrase(new Chunk(dt.Columns[columns].ColumnName, font13)));
                        PdfTable.AddCell(PdfPCell);
                    }

                    for (int rows = 0; rows <= dt.Rows.Count - 1; rows++)
                    {
                        for (int column = 0; column <= dt.Columns.Count - 1; column++)
                        {
                            PdfPCell = new PdfPCell(new Phrase(new Chunk(dt.Rows[rows][column].ToString(), font8)));
                            PdfTable.AddCell(PdfPCell);
                        }
                    }
                    pdfDoc.Add(PdfTable);
                }
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
                ExportToPdf(DtNew);
                MessageBox.Show("Exported to " + fullPath);
            }
            else
                MessageBox.Show("No data available");
        }
    }
}
