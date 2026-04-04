using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.VisualBasic;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace OneStream.BusinessRule.Finance.UTI_SharedFunctionsCS
{
    public partial class MainClass
    {
        public object Main(SessionInfo si, BRGlobals globals, object api, ExtenderArgs args)
        {
            try
            {

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region Data Buffers

        #region Clear Data Buffer

        public void ClearDataBuffer(SessionInfo si, FinanceRulesApi api, DataBuffer targetDataBuffer)
        {
            // Build clean data buffer
            var cleanDataBuffer = new DataBuffer();
            ExpressionDestinationInfo cleanExpDestInfo = api.Data.GetExpressionDestinationInfo("");
            foreach (var targetDataBufferCell in targetDataBuffer.DataBufferCells.Values)
            {
                if (System.Convert.ToBoolean(!((dynamic)targetDataBufferCell).CellStatus.IsNoData))
                {
                    ((dynamic)targetDataBufferCell).CellAmount = 0;
                    ((dynamic)targetDataBufferCell).CellStatus = new DataCellStatus(((dynamic)targetDataBufferCell).CellStatus, DataCellExistenceType.NoData);
                    cleanDataBuffer.SetCell(si, targetDataBufferCell);
                }
            }
            // Clean data buffer
            api.Data.SetDataBuffer(cleanDataBuffer, cleanExpDestInfo, default, default, default, default, default, default, default, default, default, default, default, default, true);
        }

        #endregion

        #endregion

        #region Dimension Members

        #region Member Lookup

        public Dictionary<string, MemberInfo> CreateMemberLookupUsingFilter(SessionInfo si, string dimensionName, string memberFilter)
        {
            try
            {
                // Define the dictionary that will act as the lookup (Note, the last part of the declaration makes the look case insensitive)
                var memLookup = new Dictionary<string, MemberInfo>(StringComparer.InvariantCultureIgnoreCase);

                // Execute the filter and check the result
                List<MemberInfo> memList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, dimensionName, memberFilter, true);
                if (memList is not null)
                {
                    // Loop over the members and add them to the lookup dictionary (Keyed by name)
                    foreach (MemberInfo memInfo in memList)
                        memLookup.Add(memInfo.Member.Name, memInfo);
                }

                // Add the bypass/unmapped members (Not a real member, but we do not to add / suspense items mapped to bypass)
                memLookup.Add(StageConstants.TransformationGeneral.BypassRow, default);
                memLookup.Add(StageConstants.TransformationGeneral.DimUnmapped, default);

                return memLookup;
            }

            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #endregion

        #region Data Tables

        #region Get First DataTable Rows

        public string GetFirstDataTableRows(DataTable dt, int numberOfRows)
        {
            var sb = new StringBuilder();

            // Check if the DataTable has any rows
            if (dt.Rows.Count == 0)
            {
                return "DataTable is empty.";
            }

            // Append the headers
            foreach (DataColumn column in dt.Columns)
                sb.Append(column.ColumnName + Constants.vbTab);
            sb.AppendLine();

            // Append the first 5 rows or less if there are fewer rows in the DataTable
            for (int i = 0, loopTo = Math.Min(numberOfRows - 1, dt.Rows.Count - 1); i <= loopTo; i++)
            {
                foreach (DataColumn column in dt.Columns)
                    sb.Append(dt.Rows[i][column].ToString() + Constants.vbTab);
                sb.AppendLine();
            }

            // Return the built string
            return sb.ToString();
        }

        #endregion

        #region Create Data Table

        #region Create DataTable from Delimited Files Folder

        public DataTable CreateDataTableFromDelimitedFilesFolder(SessionInfo si, string fullPath, FileSystemLocation location, char delimiter)
        {
            // Get file names in the delimited files folder
            List<NameAndAccessLevel> fileNames = BRApi.FileSystem.GetAllFileNames(si, location, fullPath, XFFileType.All, false, false, false);

            // Handle number of files
            if (fileNames.Count < 1)
            {
                throw ErrorHandler.LogWrite(si, new XFException($"No files found on Path: '{fullPath}'"));
            }

            // Declare DataTable
            var dt = new DataTable();

            // Loop through all the files to populate the DataTable
            foreach (NameAndAccessLevel fileName in fileNames)
            {
                // Get file content as string
                string fileString = Encoding.UTF8.GetString(BRApi.FileSystem.GetFile(si, location, fileName.Name, true, true).XFFile.ContentFileBytes);
				
				char empty = (char)65279;
				char pipe = (char)124;
				
				// fileString = fileString.TrimStart(empty);
				// fileString = fileString.TrimStart(pipe);
				string cleanedFileString = "";

			    // Verificar si la cadena empieza con BOM y '|' (ambos caracteres)
			    if (fileString.StartsWith(empty.ToString() + pipe)) 
			    {
			        // Dividir el contenido en líneas
			        string[] lines = fileString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			
			        // Limpiar cada línea
			        for (int i = 0; i < lines.Length; i++)
			        {
			            // Eliminar los espacios al principio de la línea
			            lines[i] = lines[i].TrimStart(empty);
			            lines[i] = lines[i].TrimStart(pipe);
			        }
			
			        // Unir las líneas limpias de nuevo en un solo string
			        cleanedFileString = string.Join(Environment.NewLine, lines);
			    }
			    else
			    {
			        cleanedFileString = fileString;  // Si no empieza con BOM y '|', usamos el contenido original
			    }
                // Convert delimited file to dt and merge
                dt.Merge(CreateDataTableFromDelimitedString(si, cleanedFileString, delimiter), false, MissingSchemaAction.Add);
            }

            return dt;
        }

        #endregion

        #region Create DataTable from Excel Files Folder

        public DataTable CreateDataTableFromExcelFilesFolder(SessionInfo si, string fullPath, FileSystemLocation location)
        {
            // Get file names in the delimited files folder
            List<NameAndAccessLevel> fileNames = BRApi.FileSystem.GetAllFileNames(si, location, fullPath, XFFileType.All, false, false, false);

            // Handle number of files
            if (fileNames.Count < 1)
            {
                throw ErrorHandler.LogWrite(si, new XFException($"No files found on Path: '{fullPath}'"));
            }

            // Declare DataTable
            var dt = new DataTable();

            // Loop through all the files to populate the DataTable
            foreach (NameAndAccessLevel fileName in fileNames)
            {
                // Get file content as an stream of bytes
                Stream fileStream = new MemoryStream(BRApi.FileSystem.GetFile(si, location, fileName.Name, true, true).XFFile.ContentFileBytes);
                // Convert delimited file to dt and merge
                dt.Merge(CreateDataTableFromExcelFile(si, fileStream), false, MissingSchemaAction.Add);
            }

            return dt;
        }

        #endregion

        #region Create DataTable from Delimited String

        public DataTable CreateDataTableFromDelimitedString(SessionInfo si, string delimitedString, char delimiter, bool allDataAsString = false)
        {
            // Create a new DataTable
            var dt = new DataTable();

            // Split the string into rows
            string[] rows = delimitedString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Check if we have any rows
            if (rows.Length > 1)
            {
                // Split the first row to get column names and clean in case of repeated delimiters
                string line = rows[0].Replace($"{delimiter}{delimiter}", System.Convert.ToString(delimiter));
                var columns = ParseCsvLine(line, System.Convert.ToString(delimiter));

                // Split the second row to get data types
                string[] dataTypes = rows[1].Split(delimiter);

                // Add columns to the DataTable
                for (int i = 0, loopTo = columns.Count - 1; i <= loopTo; i++)
                {
                    string columnName = columns[i];
                    // Jump empty columns
                    if (Strings.Len(columnName) == 0)
                        continue;

                    columnName = columnName.Trim().Trim('"');
                    var columnType = allDataAsString ? typeof(string) : GetColumnType(si, dataTypes[i].Trim());
                    // dt.Columns.Add(columnName, columnType);
                    dt.Columns.Add(columnName);
                }

                // Add data rows to the DataTable
                for (int i = 1, loopTo1 = rows.Length - 1; i <= loopTo1; i++)
                {
                    if (!string.IsNullOrWhiteSpace(rows[i]))
                    {
                        // Dim values() As String = rows(i).Split(delimiter)
                        line = rows[i];
                        var values = ParseCsvLine(line, System.Convert.ToString(delimiter));

                        var row = dt.NewRow();

                        for (int j = 0, loopTo2 = Math.Min(values.Count - 1, dt.Columns.Count - 1); j <= loopTo2; j++)
                            row[j] = ConvertToColumnType(values[j].Trim().Trim('"'), dt.Columns[j].DataType);

                        dt.Rows.Add(row);
                    }
                }
            }

            return dt;
        }

        private List<string> ParseCsvLine(string line, string delimiter = ",")
        {
            List<string> ParseCsvLineRet = default;
            // Remove BOM if present
            if (Strings.Len(line) > 0 && Strings.AscW(Strings.Left(line, 1)) == 65279)
            {
                line = Strings.Mid(line, 2);
            }
            var columns = new List<string>();
            string @field = "";
            bool inQuotes = false;
            int i;
            string c;
            var loopTo = Strings.Len(line);
            for (i = 1; i <= loopTo; i++)
            {
                c = Strings.Mid(line, i, 1);
                if (c == "\"")
                {
                    if (inQuotes)
                    {
                        // Check for escaped quote
                        if (i < Strings.Len(line) && (Strings.Mid(line, i + 1, 1) ?? "") != (delimiter ?? ""))
                        {
                            @field = @field + c;
                        }
                        // i = i + 1 ' Skip the escaped quote
                        else
                        {
                            inQuotes = false;
                        } // End of quoted field
                    }
                    else
                    {
                        inQuotes = true;
                    } // Start of quoted field
                }
                else if ((c ?? "") == (delimiter ?? "") & !inQuotes)
                {
                    columns.Add(@field);
                    @field = "";
                }
                else
                {
                    @field = @field + c;
                }
            }
            columns.Add(@field); // Add the last field
            ParseCsvLineRet = columns;
            return ParseCsvLineRet;
        }

        private Type GetColumnType(SessionInfo si, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return typeof(string);
            }

            double argresult = (double)default;
            DateTime argresult1 = default;
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out argresult))
            {
                return typeof(decimal);
            }
            else if (DateTime.TryParse(value, out argresult1))
            {
                return typeof(DateTime);
            }
            else
            {
                return typeof(string);
            }
        }

        private object ConvertToColumnType(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DBNull.Value;
            }

            switch (targetType)
            {
                case var @case when @case == typeof(bool):
                    {
                        bool result;
                        if (bool.TryParse(value, out result))
                        {
                            return result;
                        }

                        break;
                    }
                case var case1 when case1 == typeof(int):
                    {
                        int result;
                        if (int.TryParse(value, out result))
                        {
                            return result;
                        }

                        break;
                    }
                case var case2 when case2 == typeof(decimal):
                    {
                        decimal result;
                        // First decimal try parse
                        if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                        {
                            return decimal.Round(result, 2);
                        }

                        // If it fails, it tries double and convert to decimal
                        double doubleResult;
                        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out doubleResult))
                        {
                            return decimal.Round((decimal)doubleResult, 2);
                        }

                        break;
                    }
                case var case3 when case3 == typeof(DateTime):
                    {
                        DateTime result;
                        if (DateTime.TryParse(value, out result))
                        {
                            return result;
                        }

                        break;
                    }
            }

            return value;
        }

        #endregion

        #region Create DataTable from Excel File

        public DataTable CreateDataTableFromExcelFile(SessionInfo si, Stream fileStream, bool allDataAsString = false)
        {
            var dataTable = new DataTable();

            try
            {
                // Create sheetData object from the stream of file bytes
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileStream, false))
                {
                    WorkbookPart workbookPart = document.WorkbookPart;
                    Sheet sheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();
                    WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id.Value);
                    SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

                    // Get headers and create columns
                    Row headerRow = sheetData.Elements<Row>().FirstOrDefault();
                    Row dataTypeRow = sheetData.Elements<Row>().ElementAt(1);
                    if (headerRow is null)
                    {
                        throw new Exception("No header row found in the Excel file.");
                    }

                    int maxColumnIndex = GetMaxColumnIndex(headerRow);

                    for (int i = 0, loopTo = maxColumnIndex; i <= loopTo; i++)
                    {
                        var cell = GetCellByIndex(headerRow, i);
                        var dateTypeCell = GetCellByIndex(dataTypeRow, i);
                        string headerValue = cell is not null ? GetCellValue(si, document, cell) : $"Column{i + 1}";
                        string dateTypeValue = dateTypeCell is not null ? GetCellValue(si, document, dateTypeCell) : $"Column{i + 1}";
                        dataTable.Columns.Add(headerValue.Trim(), allDataAsString ? typeof(string) : GetColumnType(si, dateTypeValue));
                    }

                    // Load rows data to DataTable
                    var dataRows = sheetData.Elements<Row>().Skip(1);

                    foreach (Row row in dataRows)
                    {
                        var dataRow = dataTable.NewRow();

                        for (int i = 0, loopTo1 = maxColumnIndex; i <= loopTo1; i++)
                        {
                            var cell = GetCellByIndex(row, i);
                            string cellValue = cell is not null ? GetCellValue(si, document, cell) : string.Empty;
                            dataRow[i] = ConvertToColumnType(cellValue, dataTable.Columns[i].DataType);
                        }

                        dataTable.Rows.Add(dataRow);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException($"Error creating DataTable: {ex.Message}"));
            }

            return dataTable;
        }

        private int GetMaxColumnIndex(Row row)
        {
            if (row is null || !row.Elements<Cell>().Any())
            {
                return -1;
            }
            return row.Elements<Cell>().Max(c => GetColumnIndex(System.Convert.ToString(c.CellReference)));
        }

        private int GetColumnIndex(string cellReference)
        {
            return ExcelColumnNameToNumber(Regex.Replace(cellReference, @"\d+", "")) - 1;
        }

        private int ExcelColumnNameToNumber(string columnName)
        {
            int sum = 0;
            for (int i = 0, loopTo = columnName.Length - 1; i <= loopTo; i++)
            {
                sum *= 26;
                sum += Strings.Asc(columnName[i]) - Strings.Asc('A') + 1;
            }
            return sum;
        }

        private Cell GetCellByIndex(Row row, int index)
        {
            return row.Elements<Cell>().FirstOrDefault(c => GetColumnIndex(System.Convert.ToString(c.CellReference)) == index);
        }

        private string GetCellValue(SessionInfo si, SpreadsheetDocument document, Cell cell)
        {
            try
            {
                if (cell is null || cell.CellValue is null)
                    return string.Empty;

                string value = cell.CellValue.InnerText;
                if (cell.DataType is not null && cell.DataType.Value == CellValues.SharedString)
                {
                    var sharedStringTable = document.WorkbookPart.SharedStringTablePart?.SharedStringTable;
                    if (sharedStringTable is null)
                        return string.Empty;
                    value = sharedStringTable.ElementAt(int.Parse(value)).InnerText;
                }

                return value;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException($"Error obtaining the cell value: {ex.Message}"));
            }
        }

        #endregion

        #region Create DataTable from Edited Data Rows

        public object CreateDataTableFromEditedDataRows(SessionInfo si, List<XFEditedDataRow> editedDataRows)
        {
            // Declare DataTable
            var dt = new DataTable();

            // Set columns
            foreach (var kvp in editedDataRows[0].ModifiedDataRow.Items)
                dt.Columns.Add(((dynamic)kvp).Key);

            // Add DataRows
            foreach (var editedRow in editedDataRows)
            {
                var dataRow = dt.NewRow();
                foreach (var kvp in editedRow.ModifiedDataRow.Items)
                {
                    var value = ((dynamic)kvp).Value;
                    decimal tempDecimal;
                    // Modify separator in case it's a decimal
                    if (decimal.TryParse(((dynamic)kvp).Value, out tempDecimal))
                        value = value.ToString().Replace(",", ".");
                    dataRow[((dynamic)kvp).Key] = value;
                }
                dt.Rows.Add(dataRow);
            }

            return dt;
        }

        #endregion

        #region Create Data Table from Sheet Name

        public DataTable CreateDataTableFromSheetName(SessionInfo si, string sheetName, string queryParams, string WSPrefix)
        {
            // Declare Data Table, select query, dbParamInfos and WSPrefix
            var dt = new DataTable();
            string selectQuery = string.Empty;
            var dbParamInfos = CreateQueryParams(si, queryParams);

            // Get query depending on table name
            selectQuery = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, Guid.Empty, $"prm_{WSPrefix}_ExportQuery_{sheetName.Replace(" ", "")}");

            // Throw error if no query
            if (string.IsNullOrEmpty(selectQuery))
                throw ErrorHandler.LogWrite(si, new XFException($"There is no sheet to export for name: {sheetName}"));

            // Execute query to get Data Table
            using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            {
                dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, dbParamInfos, false);
            }

            return dt;
        }

        #endregion

        #endregion

        #region Map and Filter Columns in DataTable

        public object MapAndFilterColumnsInDataTable(SessionInfo si, DataTable dt, Dictionary<string, string> columnMappingDict)
        {
            // Loop through each column to map the name of the columns and delete non existing ones
            var columnsToRemove = new List<string>();
            foreach (DataColumn column in dt.Columns)
            {
                if (columnMappingDict.Keys.Contains(column.ColumnName))
                {
                    column.ColumnName = columnMappingDict[column.ColumnName];
                }
                else
                {
                    columnsToRemove.Add(column.ColumnName);
                }
            }

            // Remove all non existing columns
            foreach (var columnToRemove in columnsToRemove)
                dt.Columns.Remove(columnToRemove);

            return dt;

        }

        #endregion

        #region DataTable To CSV String

        public string DataTableToCsvString(DataTable dt, string delimiter)
        {
            var sb = new StringBuilder();

            // Write headers
            var columnNames = dt.Columns.Cast<DataColumn>().Select(col => QuoteValue(col.ColumnName, delimiter));
            sb.AppendLine(string.Join(delimiter, columnNames));

            // Write each row
            foreach (DataRow row in dt.Rows)
            {
                string stringLine = string.Empty;
                for (int i = 0, loopTo = dt.Columns.Count - 1; i <= loopTo; i++)
                {
                    if (i != 0)
                        stringLine = stringLine + delimiter;
                    var value = row[i];
                    stringLine = stringLine + QuoteValue(value is DBNull ? "" : value.ToString(), delimiter);
                }
                sb.AppendLine(stringLine);
            }

            return sb.ToString();
        }

        private string QuoteValue(string value, string delimiter)
        {
            if (value.Contains(delimiter) || value.Contains("\"") || value.Contains(Constants.vbLf) || value.Contains(Constants.vbCr))
            {
                value = value.Replace("\"", "\"\""); // Escape double quotes by doubling them
                return $"\"{value}\""; // Wrap in double quotes
            }
            else
            {
                return value;
            }
        }

        #endregion

        #endregion

        #region Excel

        #region Generate Excel File from Data Table

        public void GenerateExcelFileFromDataTable(SessionInfo si, DataTable dataTable, string sheetName, FileSystemLocation location, string fullPath)
        {
            // Create file info object
            var fileInfo = new XFFileInfo(FileSystemLocation.ApplicationDatabase, sheetName + " Export.xlsx", fullPath + "/");

            // Add detail to the file
            fileInfo.ContentFileExtension = "xlsx";
            fileInfo.ContentFileContainsData = true;
            fileInfo.XFFileType = XFFileType.AllXFSpreadsheet;

            // Execute the creation
            var fileFile = new XFFile(fileInfo, string.Empty, GenerateExcelBytesFromDataTable(si, dataTable, sheetName));
            BRApi.FileSystem.InsertOrUpdateFile(si, fileFile);
        }

        #endregion

        #region Generate Excel Bytes from Data Table

        public byte[] GenerateExcelBytesFromDataTable(SessionInfo si, DataTable dataTable, string sheetName)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
                    {
                        WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
                        workbookPart.Workbook = new Workbook();

                        // --- 1. AÑADIR HOJA DE ESTILOS MÍNIMA ---
                        WorkbookStylesPart stylesPart;
                        if (workbookPart.GetPartsOfType<WorkbookStylesPart>().Count() > 0)
                        {
                            stylesPart = workbookPart.GetPartsOfType<WorkbookStylesPart>().First();
                        }
                        else
                        {
                            stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                        }
                        stylesPart.Stylesheet = CreateMinimalStylesheet(); // Usar la nueva función mínima
                        stylesPart.Stylesheet.Save();

                        // --- 2. DEFINIR ÍNDICES DE ESTILO (Basados en CreateMinimalStylesheet) ---
                        const uint DEFAULT_STYLE_INDEX = 0U; // Índice 0: Default/General
                        const uint DATE_STYLE_INDEX = 1U;    // Índice 1: Nuestra fecha dd/mm/yyyy
                        const uint NUMBER_STYLE_INDEX = 2U;  // Índice 2: Nuestro número 0.00

                        // --- 3. Crear Worksheet Part ---
                        WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                        worksheetPart.Worksheet = new Worksheet(new SheetData());

                        // --- 4. Crear Sheets Collection ---
                        Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
                        var sheet = new Sheet()
                        {
                            Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                            SheetId = 1, // SheetId debe ser 1-based
                            Name = sheetName
                        };
                        sheets.Append(sheet);

                        SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                        // --- 5. Añadir Fila de Cabecera con RowIndex y CellReference ---
                        var headerRow = new Row() { RowIndex = 1 }; // Añadir RowIndex (1-based)
                        int headerCellRefIndex = 0;
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            var headerCell = new Cell()
                            {
                                CellValue = new CellValue(column.ColumnName),
                                DataType = new EnumValue<CellValues>(CellValues.String), // Cabeceras siempre como texto
                                CellReference = GetExcelColumnName(headerCellRefIndex) + "1", // Añadir CellReference (A1, B1, ..)
                                StyleIndex = DEFAULT_STYLE_INDEX // Aplicar estilo por defecto (o uno específico de cabecera si se crea)
                            };
                            headerRow.Append(headerCell);
                            headerCellRefIndex += 1;
                        }
                        sheetData.Append(headerRow);

                        // --- 6. Añadir Filas de Datos con RowIndex, CellReference y Estilos ---
                        uint currentRowIndex = 2U; // Las filas de datos empiezan en el índice 2 (1-based)
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            var newRow = new Row() { RowIndex = currentRowIndex }; // Establecer RowIndex
                            int currentCellRefIndex = 0; // Resetear índice de celda para la nueva fila
                            for (int colIdx = 0, loopTo = dataTable.Columns.Count - 1; colIdx <= loopTo; colIdx++)
                            {
                                var itemValue = dataRow[colIdx]; // Obtener el valor original (Object)
                                // Crear la celda usando la función modificada y pasando los índices de estilo
                                var dataCell = CreateCell(itemValue, DATE_STYLE_INDEX, NUMBER_STYLE_INDEX, DEFAULT_STYLE_INDEX);
                                // Establecer la referencia de celda explícita (ej: "A2", "B2")
                                dataCell.CellReference = GetExcelColumnName(currentCellRefIndex) + currentRowIndex.ToString();
                                newRow.Append(dataCell);
                                currentCellRefIndex += 1;
                            }
                            sheetData.Append(newRow);
                            currentRowIndex = (uint)(currentRowIndex + 1L); // Incrementar el índice para la siguiente fila
                        }

                        workbookPart.Workbook.Save(); // Guardar el contenido del workbook part
                    } // Dispose spreadsheetDocument cierra partes automáticamente si es necesario
                    return memoryStream.ToArray(); // Devolver array de bytes del stream en memoria
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException($"Error generating Excel file bytes: {ex.Message}"));
            }
        }

        // Helper function to create a cell with a given text value
        private Cell CreateCell(object value, uint dateStyleIndex = 0U, uint numberStyleIndex = 0U, uint defaultStyleIndex = 0U)
        {
            var cell = new Cell();
            uint cellStyleToApply = defaultStyleIndex; // Empezar con el estilo por defecto

            if (value is null || ReferenceEquals(value, DBNull.Value))
            {
                // Celda vacía para null/DBNull
                if (defaultStyleIndex > 0L)
                    cell.StyleIndex = defaultStyleIndex;
                return cell;
            }

            if (IsNumeric(value)) // Usar helper existente IsNumeric
            {
                // --- VALOR NUMÉRICO ---
                try
                {
                    cell.CellValue = new CellValue(Convert.ToDouble(value, CultureInfo.InvariantCulture)); // Guardar como número (Double)
                    // No establecer DataType (Excel infiere número)
                    cellStyleToApply = numberStyleIndex > 0L ? numberStyleIndex : defaultStyleIndex; // Elegir estilo numérico o por defecto
                }
                catch (Exception ex) when (ex is FormatException || ex is OverflowException || ex is InvalidCastException)
                {
                    // Si falla la conversión, tratar como texto
                    cell.CellValue = new CellValue(value.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.String);
                    cellStyleToApply = defaultStyleIndex;
                }
            }

            else if (value is DateTime)
            {
                // --- VALOR FECHA/HORA ---
                try
                {
                    cell.CellValue = new CellValue(Convert.ToDateTime(value).ToOADate()); // Guardar como número OADate
                    // No establecer DataType (Excel infiere número)
                    cellStyleToApply = dateStyleIndex > 0L ? dateStyleIndex : defaultStyleIndex; // Elegir estilo de fecha o por defecto
                }
                catch (Exception ex) when (ex is InvalidCastException)
                {
                    // Si falla la conversión, tratar como texto
                    cell.CellValue = new CellValue(value.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.String);
                    cellStyleToApply = defaultStyleIndex;
                }
            }

            else
            {
                // --- VALOR TEXTO (o cualquier otro tipo tratado como texto) ---
                cell.CellValue = new CellValue(value.ToString());
                cell.DataType = new EnumValue<CellValues>(CellValues.String); // Marcar explícitamente como Texto
                cellStyleToApply = defaultStyleIndex;
            } // Usar estilo por defecto

            // Aplicar el índice de estilo determinado (si es mayor que 0, ya que 0 es implícito)
            if (cellStyleToApply > 0L)
            {
                cell.StyleIndex = cellStyleToApply;
            }

            return cell;
        }

        #endregion

        #region Stylesheet Helper

        private Stylesheet CreateMinimalStylesheet()
        {
            var stylesheet = new Stylesheet();

            // 1. Numbering Formats (Define our custom date and number formats)
            var numberingFormats = new NumberingFormats();
            var dateFormat = new NumberingFormat() { NumberFormatId = 165, FormatCode = "dd/mm/yyyy" }; // Custom Date Format ID >= 164
            var numberDecimalFormat = new NumberingFormat() { NumberFormatId = 166, FormatCode = "0.00" }; // Custom Number Format ID >= 164
            numberingFormats.Append(dateFormat);
            numberingFormats.Append(numberDecimalFormat);
            numberingFormats.Count = (uint)numberingFormats.ChildElements.Count;
            stylesheet.Append(numberingFormats);

            // 2. Fonts (Minimal: 1 default font)
            var fonts = new Fonts(new Font(new FontSize() { Val = 11 }, new FontName() { Val = "Calibri" })) { Count = 1 };
            stylesheet.Append(fonts);

            // 3. Fills (Minimal: 2 required fills - None and Gray125)
            var fills = new Fills(new Fill(new PatternFill() { PatternType = PatternValues.None }), new Fill(new PatternFill() { PatternType = PatternValues.Gray125 })) { Count = 2 }; // Index 0 (None)
                                                                                                                                                                                        // Index 1 (Gray125)
            stylesheet.Append(fills);

            // 4. Borders (Minimal: 1 default border)
            var borders = new Borders(new Border()) { Count = 1 }; // Empty border elements default to no border lines
            stylesheet.Append(borders);

            // 5. Cell Style Formats (Minimal: 1 default referencing formatId 0)
            var cellStyleFormats = new CellStyleFormats(new CellFormat() { NumberFormatId = 0, FontId = 0, FillId = 0, BorderId = 0, FormatId = 0 }) { Count = 1 };
            stylesheet.Append(cellStyleFormats);

            // 6. Cell Formats (The main definitions linking styles)
            var cellFormats = new CellFormats();

            // Index 0: Default (General) - Font 0, Fill 0, Border 0, NumFmt 0 (General Built-in)
            cellFormats.Append(new CellFormat() { FormatId = 0, FontId = 0, FillId = 0, BorderId = 0, NumberFormatId = 0 });

            // Index 1: Date - Font 0, Fill 0, Border 0, NumFmt 165 (Our Custom Date)
            cellFormats.Append(new CellFormat() { FormatId = 0, FontId = 0, FillId = 0, BorderId = 0, NumberFormatId = 165, ApplyNumberFormat = true });

            // Index 2: Number (Decimal) - Font 0, Fill 0, Border 0, NumFmt 166 (Our Custom Number)
            cellFormats.Append(new CellFormat() { FormatId = 0, FontId = 0, FillId = 0, BorderId = 0, NumberFormatId = 166, ApplyNumberFormat = true });

            cellFormats.Count = (uint)cellFormats.ChildElements.Count;
            stylesheet.Append(cellFormats);

            // 7. Cell Styles (Minimal: 1 "Normal" style referencing CellStyleFormat index 0)
            var cellStyles = new CellStyles(new CellStyle() { Name = "Normal", FormatId = 0, BuiltinId = 0 }) { Count = 1 };
            stylesheet.Append(cellStyles);

            // 8. Differential Formats (Required but can be empty)
            stylesheet.Append(new DifferentialFormats() { Count = 0 });

            // 9. Table Styles (Required but can be empty)
            stylesheet.Append(new TableStyles() { Count = 0, DefaultTableStyle = "TableStyleMedium2", DefaultPivotStyle = "PivotStyleLight16" });

            return stylesheet;
        }

        #endregion

        #region Populate Worksheet

        /// <summary>
		/// 		''' Populates an Excel worksheet with data from a DataTable
		/// 		''' </summary>
		/// 		''' <param name="worksheet">Target worksheet</param>
		/// 		''' <param name="dataTable">Source DataTable</param>
		/// 		''' <param name="startRow">1-based starting row index</param>
		/// 		''' <param name="startColumn">1-based starting column index</param>
        public void PopulateWorksheet(ref Worksheet worksheet, DataTable dataTable, int startRow, int startColumn)
        {
            // If data table has no data, exit sub
            if (dataTable is null || dataTable.Rows.Count == 0)
                return;

            // Get or create SheetData element
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            if (sheetData is null)
            {
                sheetData = new SheetData();
                worksheet.AppendChild(sheetData);
            }

            // Populate data rows
            for (int rowIdx = 0, loopTo = dataTable.Rows.Count - 1; rowIdx <= loopTo; rowIdx++)
            {
                var dataRow = dataTable.Rows[rowIdx];
                var newRow = new Row() { RowIndex = (uint)(startRow + rowIdx) };

                for (int colIdx = 0, loopTo1 = dataTable.Columns.Count - 1; colIdx <= loopTo1; colIdx++)
                {
                    var cellValue = dataRow[colIdx];
                    var cell = new Cell();

                    // Determine data type and format
                    if (IsNumeric(cellValue))
                    {
                        cell.DataType = CellValues.Number;
                        cell.CellValue = new CellValue(Convert.ToDouble(cellValue).ToString());
                    }
                    else
                    {
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(ReferenceEquals(cellValue, DBNull.Value) ? "" : cellValue.ToString());
                    }

                    newRow.AppendChild(cell);
                }

                sheetData.AppendChild(newRow);
            }
        }

        #endregion

        #region Clear Worksheet Data

        /// <summary>
		/// 		''' Clears cell contents from a worksheet, starting at the specified row.
		/// 		''' </summary>
		/// 		''' <param name="worksheet">The Worksheet to clear.</param>
		/// 		''' <param name="startRow">1-based starting row index.</param>
        public void ClearWorksheetData(Worksheet worksheet, int startRow)
        {
            // Validate input
            if (startRow < 1)
                throw new Exception("Error clearing worksheet data: Invalid range parameters");

            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            if (sheetData is null)
                return;

            if (sheetData is not null)
            {
                foreach (Row row in sheetData.Elements<Row>().Where(r => r.RowIndex.Value >= startRow).ToList())
                    row.Remove();
            }
        }

        #endregion

        #region Hide Columns

        /// <summary>
		/// 		''' Hides columns in a worksheet from startColumn to endColumn (1-based, inclusive).
		/// 		''' </summary>
		/// 		''' <param name="worksheet">The Worksheet to modify.</param>
		/// 		''' <param name="startColumn">The first column to hide (1-based, e.g., 1 = A).</param>
		/// 		''' <param name="endColumn">The last column to hide (1-based, e.g., 3 = C).</param>
        public void HideColumns(Worksheet worksheet, int startColumn, int endColumn)
        {
            if (startColumn < 1 || endColumn < startColumn)
            {
                throw new ArgumentException("Error hiding columns: Invalid column range.");
            }

            // Get or create the Columns element
            Columns columns = worksheet.Elements<Columns>().FirstOrDefault();
            if (columns is null)
            {
                columns = new Columns();
                // Insert Columns after SheetFormatProperties if present, else as first child
                var sheetFormatProps = worksheet.Elements<SheetFormatProperties>().FirstOrDefault();
                if (sheetFormatProps is not null)
                {
                    worksheet.InsertAfter(columns, sheetFormatProps);
                }
                else
                {
                    worksheet.PrependChild(columns);
                }
            }

            // Check if there is already a Column definition for this range
            var existingColumn = columns.Elements<Column>().FirstOrDefault(c => c.Min.Value == (uint)startColumn && c.Max.Value == (uint)endColumn);

            if (existingColumn is not null)
            {
                existingColumn.Hidden = true;
            }
            else
            {
                // Create a new Column definition for the range
                var col = new Column()
                {
                    Min = (uint)startColumn,
                    Max = (uint)endColumn,
                    Hidden = true
                };
                columns.Append(col);
            }
        }

        #endregion

        #region Excel Column Helper

        private string GetExcelColumnName(int columnIndex)
        {
            if (columnIndex < 0)
                throw new ArgumentOutOfRangeException("columnIndex", "Column index must be non-negative.");

            int dividend = columnIndex + 1; // Convertir índice 0-based a 1-based para cálculo
            string columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName; // 65 es el código ASCII de 'A'
                dividend = (int)Math.Round(Math.Truncate((dividend - modulo) / 26d)); // Asegurar división entera
            }

            return columnName;
        }

        #endregion

        #endregion

        #region SQL

        #region Generate Insert SQL List

        public List<string> GenerateInsertSQLList(DataTable dataTable, string tableName)
        {
            var insertStatements = new List<string>();
            var sb = new StringBuilder();

            // Validate inputs
            if (dataTable is null || dataTable.Rows.Count == 0)
            {
                throw new ArgumentException("The DataTable is empty or null.");
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("The table name is null or empty.");
            }

            // Get the column names
            string columnNames = string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => $"[{c.ColumnName}]"));

            // Batch size limit
            int batchSize = 1000;
            int currentBatchSize = 0;

            // Build the initial part of the INSERT INTO statement
            sb.AppendLine($"INSERT INTO {tableName} ({columnNames}) VALUES");

            // Iterate through each row in the DataTable
            for (int i = 0, loopTo = dataTable.Rows.Count - 1; i <= loopTo; i++)
            {
                var rowValues = new List<string>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    var value = dataTable.Rows[i][column];

                    if (ReferenceEquals(value, DBNull.Value))
                    {
                        rowValues.Add("NULL");
                    }
                    else if (value is string || value is char)
                    {
                        string valueString = $"'{value.ToString().Replace("'", "''")}'";

                        // If it can be converted to a decimal number with "," as the decimal separator, replace "," with "."
                        var nfi = new NumberFormatInfo();
                        nfi.NumberDecimalSeparator = ",";
                        decimal argresult = default;
                        if (decimal.TryParse(valueString, NumberStyles.Number, nfi, out argresult))
                            valueString.Replace(",", ".");

                        rowValues.Add(valueString == "''" ? "NULL" : valueString); // Escape single quotes
                    }
                    else if (value is DateTime)
                    {
                        rowValues.Add($"'{((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fff")}'");
                    }
                    else
                    {
                        rowValues.Add(value.ToString().Replace(",", "."));
                    }
                }

                // Combine row values into a comma-separated string and add to StringBuilder
                sb.AppendLine($"({string.Join(", ", rowValues)}){(currentBatchSize < batchSize - 1 && i < dataTable.Rows.Count - 1 ? "," : ";")}");
                currentBatchSize += 1;

                // If the batch size limit is reached, or it's the last row, finalize the current statement and start a new one
                if (currentBatchSize == batchSize || i == dataTable.Rows.Count - 1)
                {
                    insertStatements.Add(sb.ToString());
                    sb.Clear();
                    if (i < dataTable.Rows.Count - 1)
                    {
                        sb.AppendLine($"INSERT INTO {tableName} ({columnNames}) VALUES");
                    }
                    currentBatchSize = 0;
                }
            }

            return insertStatements;
        }

        #endregion

        #region Create SQL Filter

        public object CreateSQLFilter(SessionInfo si, string parameter, string parameterField)
        {

            string parameterFilter;

            // If parameter is All, no filter is added
            if (parameter == "All" | string.IsNullOrEmpty(parameter))
            {

                parameterFilter = "";
            }

            else
            {

                parameterFilter = $"AND {parameterField} = '{parameter}'";

            }

            return parameterFilter;

        }

        #endregion

        #endregion

        #region Navigation

        #region Input Parameter Controller

        public XFLoadDashboardTaskResult InputParameterController(SessionInfo si, DashboardExtenderArgs args, XFLoadDashboardTaskResult loadDashboardTaskResult, Dictionary<string, string> parameterDict)
        {

            // For loop through all the dictionary elements
            foreach (KeyValuePair<string, string> parameterElement in parameterDict)
            {

                // Manage modified input values, rest default
                if (!args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(parameterElement.Key).Equals(parameterElement.Value))
                {

                    loadDashboardTaskResult.ModifiedCustomSubstVars.Add(parameterElement.Key, args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(parameterElement.Key));
                }

                else
                {

                    loadDashboardTaskResult.ModifiedCustomSubstVars.Add(parameterElement.Key, parameterElement.Value);

                }

            }

            return loadDashboardTaskResult;

        }

        #endregion

        #endregion

        #region File System

        #region Copy XLSX File

        public void CopyXLSXFile(SessionInfo si, string fileName, string filePath, string newFileName, string newFilePath)
        {

            // Create the file in the specified path
            XFFileEx objXFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath + "/" + fileName, true, true);
            string folderName = objXFFileEx.XFFile.FileInfo.FolderFullName;
            byte[] bytesOfFile = objXFFileEx.XFFile.ContentFileBytes;
            var fileInfo = new XFFileInfo(FileSystemLocation.ApplicationDatabase, newFileName, newFilePath + "/");

            // Aditional detail
            fileInfo.ContentFileExtension = "xlsx";
            fileInfo.ContentFileContainsData = true;
            fileInfo.XFFileType = XFFileType.AllXFSpreadsheet;

            // Execute copy
            var fileFile = new XFFile(fileInfo, string.Empty, bytesOfFile);
            BRApi.FileSystem.InsertOrUpdateFile(si, fileFile);

        }

        #endregion

        #region Copy File

        public void CopyFile(SessionInfo si, FileSystemLocation location, string fileName, string filePath, string newFileName, string newFilePath)
        {

            // Create the file in the specified path
            XFFileEx objXFFileEx = BRApi.FileSystem.GetFile(si, location, filePath + "/" + fileName, true, true);
            string folderName = objXFFileEx.XFFile.FileInfo.FolderFullName;
            byte[] bytesOfFile = objXFFileEx.XFFile.ContentFileBytes;
            var fileInfo = new XFFileInfo(location, newFileName, newFilePath + "/");

            // Aditional detail
            fileInfo.ContentFileContainsData = true;

            // Execute copy
            var fileFile = new XFFile(fileInfo, string.Empty, bytesOfFile);
            BRApi.FileSystem.InsertOrUpdateFile(si, fileFile);

        }

        #endregion

        #region Delete All Files in Folder

        public void DeleteAllFilesInFolder(SessionInfo si, string fullPath, FileSystemLocation location)
        {
            // Get file names in the delimited files folder
            List<NameAndAccessLevel> fileNames = BRApi.FileSystem.GetAllFileNames(si, location, fullPath, XFFileType.All, false, false, false);

            // Loop through all the files to populate the DataTable
            foreach (NameAndAccessLevel fileName in fileNames)
                // Delete file
                BRApi.FileSystem.DeleteFile(si, location, fileName.Name);
        }

        #endregion

        #endregion

        #region Custom Tables

        #region Load Data to Custom Tables

        #region Load Delimited Files to Custom Table

        public void LoadDelimitedFilesToCustomTable(SessionInfo si, string customTableName, string fullPath, FileSystemLocation location, char delimiter, string @method)
        {
            // Handle method
            if (@method.ToLower() != "merge" && @method.ToLower() != "replace")
            {
                throw ErrorHandler.LogWrite(si, new XFException("Method must be 'Merge' or 'Replace'"));
            }

            // Declare and populate DataTable
            var dt = CreateDataTableFromDelimitedFilesFolder(si, fullPath, location, delimiter);

            // Load datatable to custom table
            LoadDataTableToCustomTable(si, customTableName, dt, @method);
        }

        #endregion

        #region Load Excel Files to Custom Table

        public void LoadExcelFilesToCustomTable(SessionInfo si, string customTableName, string fullPath, FileSystemLocation location, string @method)
        {
            // Handle method
            if (@method.ToLower() != "merge" && @method.ToLower() != "replace")
            {
                throw ErrorHandler.LogWrite(si, new XFException("Method must be 'Merge' or 'Replace'"));
            }

            // Declare and populate DataTable
            var dt = CreateDataTableFromExcelFilesFolder(si, fullPath, location);

            // Load datatable to custom table
            LoadDataTableToCustomTable(si, customTableName, dt, @method);
        }

        #endregion

        #region Load DataTable to Custom Table

        public void LoadDataTableToCustomTable(SessionInfo si, string customTableName, DataTable dt, string @method)
        {
            // Clean column names
            foreach (DataColumn column in dt.Columns)
            {
                // Clean the column name
                string cleanColumnName = Regex.Replace(column.ColumnName, @"[^\w\d]", "");
                column.ColumnName = cleanColumnName;
            }

            // Insert to data base
            using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            {
                // Handle replace method
                if (@method.ToLower() == "replace")
                {
                    // Truncate table
                    string truncateQuery = $@"
						TRUNCATE TABLE {customTableName};
					";
                    BRApi.Database.ExecuteSql(dbConn, truncateQuery, false);
                }
                // Declare the list of sql insert queries
                var insertSQLList = GenerateInsertSQLList(dt, customTableName);
                // Loop through all the list of sql insert queries and execute
                foreach (var insertSQL in insertSQLList)
                    BRApi.Database.ExecuteSql(dbConn, insertSQL, false);
            }
        }

        #endregion

        #endregion

        #endregion

        #region Parameter helper

        #region Create Query Params

        public List<DbParamInfo> CreateQueryParams(SessionInfo si, string queryParams)
        {
            // Create a new list to store the query parameters
            var dbParamInfos = new List<DbParamInfo>();

            // Populate param infos from query params
            var queryParamDict = SplitQueryParams(si, queryParams);
            foreach (KeyValuePair<string, string> queryParam in queryParamDict)
                dbParamInfos.Add(new DbParamInfo(queryParam.Key, queryParam.Value));

            return dbParamInfos;
        }

        #endregion

        #region Split Query Params

        public Dictionary<string, string> SplitQueryParams(SessionInfo si, string queryParams)
        {
            // Create a new dictionary to store the key-value pairs
            var result = new Dictionary<string, string>();

            // Define the regular expression pattern
            // (\w+) captures the key (one or more word characters)
            // = matches the equals sign
            // (?:\[(.*?)\]|([^,]+)) is a non-capturing group that matches either:
            // - \[(.*?)\] content within square brackets (non-greedy)
            // - ([^,]+) any characters except comma (for non-bracketed values)
            // (?:,|$) matches either a comma or the end of the string (non-capturing group)
            string pattern = @"(\w+)=(?:\[(.*?)\]|([^,]+))(?:,|$)";

            // Find all matches of the pattern in the input string
            var matches = Regex.Matches(queryParams, pattern);

            // Iterate through each match
            foreach (Match match in matches)
            {
                // Extract the key from the first capturing group
                string key = match.Groups[1].Value;

                // Extract the value from either the second or third capturing group
                // Group 2 is for bracketed values, Group 3 is for non-bracketed values
                string value = match.Groups[2].Success ? match.Groups[2].Value : match.Groups[3].Value;

                // Add the key-value pair to the dictionary, trimming any whitespace from the value
                result.Add(key, value.Trim());
            }

            // Return the populated dictionary
            return result;
        }

        #endregion

        #endregion

        #region Small Helpers

        #region Is Numeric

        /// <summary>
		/// 		''' Gets if a specific value is numeric.
		/// 		''' </summary>
        private bool IsNumeric(object value)
        {
            if (ReferenceEquals(value, DBNull.Value))
                return false;
            return value is int || value is double || value is decimal || value is long || value is short;
        }

        #endregion

        #endregion

    }
}