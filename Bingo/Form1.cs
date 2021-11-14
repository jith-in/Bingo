using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog op1 = new OpenFileDialog();
            op1.Multiselect = true;
            op1.ShowDialog();
            op1.Filter = "CSV files (*.csv)|*.csv";
            dataGridView1.DataSource = ConvertCSVtoDataTable1(op1.FileName);
            dataGridView2.DataSource = DtNew = null;
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
               
                return dtcsv;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error occured while uploading data :"+ ex.Message.ToString());
                return null;
            }
        }
        private void button2_Click(object sender, EventArgs e)
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
                        
                        dataGridView2.DataSource = DtNew;

                        dtnewrowCount = DtNew.Rows.Count;

                        var rows = dt.AsEnumerable().Except(DtNew.AsEnumerable(), DataRowComparer.Default);
                        if (rows.Count() != 0)
                            dt = rows.CopyToDataTable();
                        dataGridView1.DataSource = dt;
                        

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

        private void clear()
        {
            dtnewrowCount = 0;
            dtrowCount = 0;
            dt = null;
            DtNew = null;
            dataGridView1.DataSource = null;
            dataGridView2.DataSource = null;
            textBox1.Text = string.Empty;
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
            iTextSharp.text.Font font10 = FontFactory.GetFont("ARIAL", 10);
            try
            {
                path = ConfigurationManager.AppSettings["PDFDownloadPath"].ToString();
                fullPath = Path.Combine(path, "Data.pdf");
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                pdfDoc.Open();

                if (dt.Rows.Count > 0)
                {
                    PdfPTable PdfTable = new PdfPTable(1);

                    PdfPCell PdfPCell = new PdfPCell();
                  
                   

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
                            PdfPCell = new PdfPCell(new Phrase(new Chunk(dt.Rows[rows][column].ToString(), font10)));
                            PdfTable.AddCell(PdfPCell);
                        }
                    }
                    pdfDoc.Add(PdfTable);
                }
                pdfDoc.Close();
               
            }
            catch (DocumentException de)
            {
                MessageBox.Show("Exception :"+ de.Message.ToString());
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
                MessageBox.Show("Exported to "+ fullPath);
            }
            else
                MessageBox.Show("No data available");
        }
    }
}
