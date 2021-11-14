using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bingo
{
    public partial class Form1 : Form
    {
        DataTable dt;
        DataTable DtNew;
        int dtrowCount;
        int dtnewrowCount;
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
    }
}
