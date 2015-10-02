using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Xml;
using DocumentFormat.OpenXml;

namespace IngresoRPC
{
    public partial class Ingreso : Form
    {
        List<string> entered = new List<string>();
        public RPCHelper rpc;
        List<RPCFan> firstCell;
        WorkbookPart wbPart;
        Sheet theSheet;
        SpreadsheetDocument document;
        string cardPrimaryKey = "";

        public Ingreso()
        {
            InitializeComponent();
        }

        void OpenKeywordsFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OpenFileDialog fileDialog = sender as OpenFileDialog;
            string selectedFile = fileDialog.FileName;
            if (string.IsNullOrEmpty(selectedFile) || selectedFile.Contains(".lnk"))
            {
                MessageBox.Show("Please select a valid Excel File");
                e.Cancel = true;
            }
            return;
        }

        string selected;
        private void Ingreso_Load(object sender, EventArgs e)
        {
            LoadXML();
            timer1.Start();
            OpenFileDialog openKeywordsFileDialog = new OpenFileDialog();
            openKeywordsFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openKeywordsFileDialog.Multiselect = false;
            openKeywordsFileDialog.ValidateNames = true;
            openKeywordsFileDialog.DereferenceLinks = false;
            openKeywordsFileDialog.Filter = "Excel |*.xlsx";
            openKeywordsFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(OpenKeywordsFileDialog_FileOk);
            var dialogResult =  openKeywordsFileDialog.ShowDialog();

            selected = openKeywordsFileDialog.FileName;
            //var doc = SpreadsheetDocument.Open(selected,true);
            using (document = SpreadsheetDocument.Open(selected, true))
            {
                wbPart = document.WorkbookPart;
                theSheet = wbPart.Workbook.Descendants<Sheet>().First();
                //  Where(s => s.Name == sheetName).FirstOrDefault();
                firstCell = rpc.RetrieveFirstObject(wbPart, theSheet);
                this.entered=(from p in firstCell where p.YaEntro select p.ID).ToList(); 
            }
            
        }

        private void LoadXML()
        {
             XmlDocument xml = new XmlDocument();
             xml.Load("ConfigRPC.xml");
             this.KeyWord = xml.SelectSingleNode("keyWord").InnerText;
             rpc = new RPCHelper(this.KeyWord);
             //foreach (XmlNode xn in xnList)
             //{
             //    string firstName = xn["FirstName"].InnerText;
             //    string lastName = xn["LastName"].InnerText;
             //    Console.WriteLine("Name: {0} {1}", firstName, lastName);
             //}
        }

        private void Ingreso_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar >= 48 && e.KeyChar <= 57)
                {
                    cardPrimaryKey += e.KeyChar;
                    if (cardPrimaryKey.Length == 10)
                    {
                        Clean();

                        txtInfo.Text = "";
                        
                        RPCFan myfan = rpc.KeyPressLengthAlert(cardPrimaryKey, firstCell,entered);
                        if (myfan == null)
                            txtInfo.Text = "La tarjeta no tiene ingresos asignados!";
                        else if (myfan.ID == null)
                            txtInfo.Text = "Esta tarjeta ya fue ingresada!";
                        else
                        {
                            this.ultimo = myfan;
                            txtDNI.Text = myfan.DNI;
                            txtIngreso.Text = myfan.Ingreso;
                            txtNom.Text = myfan.Nombre;
                            txtRPC.Text = myfan.CardNum;
                        }

                        cardPrimaryKey = "";
                    }
                }
                else if (e.KeyChar == (char)Keys.Enter)
                {

                    e.Handled = true;

                }
            }
            catch (Exception)
            {
            }
        }

        private void MarkCell(RPCFan myfan)
        {

            WorksheetPart wsPart =
              (WorksheetPart)(wbPart.GetPartById(theSheet.Id));
            Cell theCellular = wsPart.Worksheet.Descendants<Cell>().
              Where(c => c.CellReference == myfan.LastCell).FirstOrDefault();
            theCellular.CellValue = new CellValue("X");
            theCellular.DataType =
                new EnumValue<CellValues>(CellValues.String);

            // Save 
            wsPart.Worksheet.Save();
        }


        private void Clean()
        {
            this.bkpDNI = txtDNI.Text;
            this.bkpING = txtIngreso.Text;
            this.bkpNom = txtNom.Text;
            this.bkpRPC = txtRPC.Text;

            txtDNI.Text = "";
            txtIngreso.Text = "";
            txtNom.Text = "";
            txtRPC.Text = "";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (cardPrimaryKey.Length != 0)
            {
                this.lastTick = this.nowtick;
                this.nowtick = cardPrimaryKey.Length;
                if (lastTick == nowtick)
                    cardPrimaryKey = "";
            }
        }

        private void Ingreso_Activated(object sender, EventArgs e)
        {
            //timer1_Tick(this, null);
        }


        public string KeyWord { get; set; }

        public string bkpRPC { get; set; }

        public string bkpNom { get; set; }

        public string bkpING { get; set; }

        public string bkpDNI { get; set; }

        private void button1_Click(object sender, EventArgs e)
        {
             txtDNI.Text = ultimo.DNI;
             txtIngreso.Text = ultimo.Ingreso;
             txtNom.Text = ultimo.Nombre;
             txtRPC.Text = ultimo.CardNum;
             ultimo = new RPCFan();
             this.Focus();
        }

        public RPCFan ultimo { get; set; }

        public int lastTick { get; set; }

        public int nowtick { get; set; }

        private void button2_Click(object sender, EventArgs e)
        {
            using (document = SpreadsheetDocument.Open(selected, true))
            {
                wbPart = document.WorkbookPart;
                theSheet = wbPart.Workbook.Descendants<Sheet>().First();
                //  Where(s => s.Name == sheetName).FirstOrDefault();
                //firstCell = rpc.RetrieveFirstObject(wbPart, theSheet);
                //this.entered = (from p in firstCell where p.YaEntro select p.ID).ToList();
         
                foreach (var item in entered)
                {
                    var myfan = firstCell.First(x => x.ID == item);
                    MarkCell(myfan);
                    
                }
            }
        }
    }
}
