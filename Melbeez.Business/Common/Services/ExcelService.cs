using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System;
using System.Data;
using System.IO;
using System.Linq;

namespace Melbeez.Business.Common.Services
{
    public class ExcelService
    {
        public static byte[] CreateExcelDocumentAsStream(DataTable ds1, string FileName, int SheetIndex, string SheetName)
        {
            byte[] byteArray = null;
            byteArray = File.ReadAllBytes(FileName);

            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(byteArray, 0, (int)byteArray.Length);
                using (SpreadsheetDocument spreadsheetDoc = SpreadsheetDocument.Open(stream, true))
                {
                    #region style
                    WorkbookPart workbookPart = spreadsheetDoc.WorkbookPart;
                    var wsp = workbookPart.WorkbookStylesPart;
                    #endregion

                    WriteExcelFile(workbookPart, ds1, spreadsheetDoc, SheetIndex, SheetName);
                    spreadsheetDoc.WorkbookPart.Workbook.Save();
                    spreadsheetDoc.Close();
                }
                stream.Flush();
                stream.Position = 0;
                byte[] data1 = new byte[stream.Length];
                stream.Read(data1, 0, data1.Length);
                return stream.ToArray();
            }
        }
        public static void WriteExcelFile(WorkbookPart workbookPart, DataTable dt, SpreadsheetDocument spreadsheetDocument, int worksheetNumber, string worksheetName)
        {
            string relId = workbookPart.Workbook.Descendants<Sheet>().First(s => worksheetName.Equals(s.Name)).Id;

            WorksheetPart newWorksheetPart = (WorksheetPart)workbookPart.GetPartById(relId);
            WriteDataTableToExcel(dt, newWorksheetPart);

            newWorksheetPart.Worksheet.Save();
        }
        private static void WriteDataTableToExcel(DataTable dt, WorksheetPart worksheetPart)
        {
            var worksheet = worksheetPart.Worksheet;
            var sheetData = worksheet.GetFirstChild<SheetData>();

            string cellValue = "";

            int numberOfColumns = dt.Columns.Count;
            bool[] IsNumericColumn = new bool[numberOfColumns];

            string[] excelColumnNames = new string[numberOfColumns];
            for (int n = 0; n < numberOfColumns; n++)
                excelColumnNames[n] = GetExcelColumnName(n);

            uint rowIndex = 1;

            var headerRow = new Row { RowIndex = rowIndex };  // add a row at the top of spreadsheet
            sheetData.Append(headerRow);

            for (int colInx = 0; colInx < numberOfColumns; colInx++)
            {
                DataColumn col = dt.Columns[colInx];
                AppendTextCell(excelColumnNames[colInx] + "1", col.ColumnName, headerRow);

                IsNumericColumn[colInx] = (col.DataType.FullName == "System.Decimal") || (col.DataType.FullName == "System.Int32");
            }
            //  Now, step through each row of data in our DataTable...
            double cellNumericValue = 0;
            foreach (DataRow dr in dt.Rows)
            {
                // ...create a new row, and append a set of this row's data to it.
                ++rowIndex;
                var newExcelRow = new Row { RowIndex = rowIndex };  // add a row at the top of spreadsheet
                sheetData.Append(newExcelRow);

                for (int colInx = 0; colInx < numberOfColumns; colInx++)
                {
                    cellValue = dr.ItemArray[colInx].ToString();

                    // Create cell with data
                    if (IsNumericColumn[colInx])
                    {
                        //  For numeric cells, make sure our input data IS a number, then write it out to the Excel file.
                        //  If this numeric value is NULL, then don't write anything to the Excel file.
                        cellNumericValue = 0;
                        if (double.TryParse(cellValue, out cellNumericValue))
                        {
                            cellValue = cellNumericValue.ToString();
                            AppendNumericCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow);
                        }
                    }
                    else
                    {
                        //  For text cells, just write the input data straight out to the Excel file.
                        AppendTextCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow);
                    }
                }
            }
        }
        private static string GetExcelColumnName(int columnIndex)
        {
            if (columnIndex < 26)
                return ((char)('A' + columnIndex)).ToString();

            char firstChar = (char)('A' + (columnIndex / 26) - 1);
            char secondChar = (char)('A' + (columnIndex % 26));

            return string.Format("{0}{1}", firstChar, secondChar);
        }
        private static void AppendTextCell(string cellReference, string cellStringValue, Row excelRow)
        {
            //  Add a new Excel Cell to our Row 
            Cell cell = new Cell() { CellReference = cellReference, DataType = CellValues.String };
            CellValue cellValue = new CellValue();
            cellValue.Text = cellStringValue;
            cell.Append(cellValue);
            excelRow.Append(cell);
        }
        private static void AppendNumericCell(string cellReference, string cellStringValue, Row excelRow)
        {
            //  Add a new Excel Cell to our Row 
            Cell cell = new Cell() { CellReference = cellReference };
            CellValue cellValue = new CellValue();
            cellValue.Text = cellStringValue;
            cell.Append(cellValue);
            excelRow.Append(cell);
        }
        public static DataTable GetDataTableBulkImport(byte[] byteArray)
        {
            DataTable dt = new DataTable();
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(byteArray, 0, (int)byteArray.Length);

                using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(stream, true))
                {
                    WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                    IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                    string relationshipId = sheets.First().Id.Value;
                    WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                    Worksheet workSheet = worksheetPart.Worksheet;
                    SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                    IEnumerable<Row> rows = sheetData.Descendants<Row>();

                    foreach (Cell cell in rows.ElementAt(0))
                    {
                        dt.Columns.Add(GetCellValue(spreadSheetDocument, cell));
                    }

                    try
                    {
                        foreach (Row row in rows)
                        {
                            DataRow tempRow = dt.NewRow();

                            for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                            {
                                try
                                {
                                    Cell cell = row.Descendants<Cell>().ElementAt(i);
                                    int actualCellIndex = CellReferenceToIndex(cell);
                                    string val = GetCellValue(spreadSheetDocument, cell);

                                    //string val = GetCellValue(spreadSheetDocument, row.Descendants<Cell>().ElementAt(i));
                                    tempRow[actualCellIndex] = val;
                                }
                                catch (Exception ex)
                                {
                                }
                            }

                            dt.Rows.Add(tempRow);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                }
            }
            return dt;
        }
        private static int CellReferenceToIndex(Cell cell)
        {
            int index = 0;
            string reference = cell.CellReference.ToString().ToUpper();
            foreach (char ch in reference)
            {
                if (Char.IsLetter(ch))
                {
                    int value = (int)ch - (int)'A';
                    index = (index == 0) ? value : ((index + 1) * 26) + value;
                }
                else
                    return index;
            }
            return index;
        }
        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            try
            {
                SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
                string value = null;
                if (cell != null && cell.CellValue != null)
                {
                    value = cell.CellValue.InnerXml;
                }
                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                {
                    return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
                }
                else
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}