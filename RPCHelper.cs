using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace IngresoRPC
{
    public class RPCHelper
    {
        string keyword;
        public RPCHelper(string kw)
        {
            keyword = kw;
        }
        public List<RPCFan> RetrieveFirstObject(WorkbookPart wbPart, Sheet theSheet)
        {
            string abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            WorksheetPart wsPart = 
                (WorksheetPart)(wbPart.GetPartById(theSheet.Id));
            Cell theCell = wsPart.Worksheet.Descendants<Cell>().
              Where(c => c != null).FirstOrDefault();
            string first = FindCellInnerValue(wbPart, theCell);
            var letra = first.Substring(0, 1);
            int numeros = int.Parse(first.Substring(1));
            int i = abc.IndexOf(letra);
            List<RPCFan> retorno = new List<RPCFan>();
            int countFans = 0;
            int numeroscopy = numeros;
            while (theCell != null)
            {
                string newRef = abc[i].ToString() + (numeroscopy++ + 1).ToString();
                theCell = wsPart.Worksheet.Descendants<Cell>().
              Where(c => c.CellReference == newRef).FirstOrDefault();
                countFans++;
            }
            for (int ii = 0; ii < (countFans - 1); ii++)
            {
                string newRef;
                i = abc.IndexOf(letra);
                theCell = new Cell();
                var fan = new RPCFan();
                List<string> data = new List<string>();
                while (theCell != null)
                {
                    newRef = abc[i++].ToString() + (numeros + 1).ToString();
                    theCell = wsPart.Worksheet.Descendants<Cell>().
                  Where(c => c.CellReference == newRef).FirstOrDefault();
                    var value = FindCellInnerValue(wbPart, theCell, false);
                    data.Add(value);
                    //i++;
                }
                fan.YaEntro = false;
                fan.LastCell = abc[i-2].ToString() + (numeros + 1).ToString();
                fan.ID = data[0];
                fan.Nombre = data[1];
                fan.CardNum = data[2];
                fan.DNI = data[3];
                fan.Ingreso = data[4];
                if (data[5]!="0")
                    fan.YaEntro = true;
                
                retorno.Add(fan);
                numeros++;
            }
    

            return retorno;

        }

        private string FindCellInnerValue(WorkbookPart wbPart, Cell theCell,bool reference=true)
        {
            if (theCell != null)
            {
                var value = theCell.InnerText;

                if (theCell.DataType != null)
                {
                    switch (theCell.DataType.Value)
                    {
                        case CellValues.SharedString:
                            var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                            if (stringTable != null)
                            {
                                value = stringTable.SharedStringTable.
                                  ElementAt(int.Parse(value)).InnerText;
                                if (reference)
                                    return theCell.CellReference;
                                else
                                    return value;
                            }
                            break;
                    }
                }
                if(theCell.CellValue!=null)
                    return theCell.CellValue.InnerText;
            }
            return null;
        }

        internal RPCFan KeyPressLengthAlert(string cardPrimaryKey, List<RPCFan> list, List<string> enter)
        {
           
            if(enter.Contains(cardPrimaryKey))
            {
                return new RPCFan();
            }
            else
            {
                
                var found =  list.Find(x => x.ID == cardPrimaryKey);
                if (found != null)
                {
                    enter.Add(cardPrimaryKey);
                    return found;
                }
                else
                    return null;
            }
        }
    }
}
