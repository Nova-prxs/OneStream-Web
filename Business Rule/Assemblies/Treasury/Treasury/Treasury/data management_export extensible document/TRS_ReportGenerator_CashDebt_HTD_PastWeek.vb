Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Spreadsheet

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.TRS_ReportGenerator_CashDebt_HTD_PastWeek
    Public Class MainClass
        
        ' ==================================================================================
        ' TRS_ReportGenerator_CashDebt_HTD_PastWeek
        ' ==================================================================================
        ' This business rule populates the CashDebtPosition_HTD_New.xlsx template with data
        ' from XFC_TRS_Master_CashDebtPosition table based on (paramWeek - 1) and paramYear.
        ' 
        ' NOTE: This is a copy of TRS_ReportGenerator_CashDebt_HTD that uses the PREVIOUS week
        '       (paramWeek - 1) instead of paramWeek.
        '
        ' IMPORTANT: This rule does NOT modify the template structure (no row/column deletion).
        ' It only populates cell values while preserving all original formatting.
        '
        ' EXCEL STRUCTURE (NEW FORMAT - Nov 2025):
        ' - Column A is EMPTY
        ' - Column B = Description (labels - from template, don't modify)
        ' - Column C = HTD (Europe, Latam) - Total of all entities
        ' - Column D = Horse Aveiro
        ' - Column E = Horse Turkey OYAK
        ' - Column F = Horse Romania
        ' - Column G = Horse Brazil
        ' - Column H = Horse Chile
        ' - Column I = Horse Argentina
        ' - Column J = Horse Spain
        ' - Column K = Horse Holding (Horse Powertrain Solution)
        '
        ' ROW STRUCTURE:
        ' Rows 1-11: Header section (Week info, entity names, etc.)
        ' Row 12: Entity column headers (HTD, Horse Aveiro, etc.)
        ' Row 13: "ACTUAL" label row
        ' Row 14: "CASH AND FINANCING BALANCE" section header
        ' Rows 15-36: ACTUAL (StartWeek) data section
        ' Rows 39-66: END WEEK FORECAST data section  
        ' Rows 69-96: END OF MONTH data section
        ' ==================================================================================
        
        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
            Try
                Select Case args.FunctionType
                    Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
                        Dim functionName As String = String.Empty
                        If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey("p_function") Then
                            functionName = args.NameValuePairs("p_function")
                        End If
                        
                        If Not String.IsNullOrEmpty(functionName) Then
                            If functionName.Equals("GenerateCashDebtHTDReportPastWeek", StringComparison.InvariantCultureIgnoreCase) Then
                                Return GenerateCashDebtHTDReportPastWeek(si, args)
                            End If
                        End If
                End Select

                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        Private Function GenerateCashDebtHTDReportPastWeek(ByVal si As SessionInfo, ByVal args As ExtenderArgs) As Object
            Try
                ' ==========================================================================================
                ' 1. GET PARAMETERS
                ' ==========================================================================================
                Dim paramWeek As String = String.Empty
                If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey("paramWeek") Then
                    paramWeek = args.NameValuePairs("paramWeek")
                End If
                
                Dim paramYear As String = String.Empty
                If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey("paramYear") Then
                    paramYear = args.NameValuePairs("paramYear")
                End If
                If String.IsNullOrEmpty(paramWeek) OrElse String.IsNullOrEmpty(paramYear) Then
                    Throw New Exception("Parameters paramWeek and paramYear are required.")
                End If

                ' ==========================================================================================
                ' 2. CALCULATE PARAMETERS - USE PREVIOUS WEEK (paramWeek - 1)
                ' ==========================================================================================
                Dim validWeek As Integer
                If Not Integer.TryParse(paramWeek, validWeek) Then
                    validWeek = 1
                End If
                
                ' Calculate the target week (paramWeek - 1)
                Dim targetWeek As Integer = If(validWeek > 1, validWeek - 1, 1)
                Dim targetWeekStr As String = targetWeek.ToString()
                Dim timeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, paramYear, targetWeekStr)
                Dim mondayDate As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMondayDateForWeek(si, paramYear, targetWeekStr).ToUpper()
                
                ' Calculate expected scenario based on week number (FW + 2-digit week)
                Dim expectedScenario As String = "FW" + targetWeekStr.PadLeft(2, "0"c)
                
                ' Calculate previous week (for Flow to last week comparison)
                Dim previousWeek As Integer = If(targetWeek > 1, targetWeek - 1, 1)
                
                ' Calculate end week date (4 weeks later)
                Dim currentDate As DateTime = DateTime.ParseExact(timeKey, "yyyyMMdd", CultureInfo.InvariantCulture)
                Dim endWeekDate As DateTime = currentDate.AddDays(28)
                Dim endWeekFormatted As String = endWeekDate.ToString("dd-MMM", New CultureInfo("es-ES")).ToUpper()
                
                ' Calculate EOM date
                Dim eomDate As DateTime = New DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month))
                Dim eomFormatted As String = eomDate.ToString("dd-MMM", New CultureInfo("es-ES")).ToUpper()
                ' ==========================================================================================
                ' 3. GET DATA FROM DATABASE
                ' ==========================================================================================
                Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Dim allData As DataTable = Me.GetCashDebtData(si, dbConn, timeKey, expectedScenario, targetWeek, previousWeek)
                    ' Aggregate data by projection type, account, flow, and entity
                    Dim aggregatedData As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal)))) = _
                        Me.AggregateAllData(allData)
                    
                    ' ==========================================================================================
                    ' 4. LOAD EXCEL TEMPLATE
                    ' ==========================================================================================
                    Dim templatePath As String = "Documents/Public/Treasury/Extensible Document/CashDebtPosition_HTD_New.xlsx"
                    Dim outputFolder As String = "Documents/Public/Treasury"
                    Dim outputFileName As String = "CashDebtPosition_HTD_Report_PastWeek.xlsx"
                    Dim outputPath As String = $"{outputFolder}/{outputFileName}"
                    
                    Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, templatePath, True, False)
                    
                    If fileInfo Is Nothing OrElse fileInfo.XFFile.ContentFileBytes Is Nothing Then
                        Throw New Exception($"Template file not found at {templatePath}")
                    End If

                    Dim fileBytes As Byte() = fileInfo.XFFile.ContentFileBytes
                    ' ==========================================================================================
                    ' 5. POPULATE EXCEL (PRESERVE ALL FORMATTING)
                    ' ==========================================================================================
                    Using ms As New MemoryStream()
                        ms.Write(fileBytes, 0, fileBytes.Length)
                        ms.Position = 0

                        Using doc As SpreadsheetDocument = SpreadsheetDocument.Open(ms, isEditable:=True)
                            ' Delete Calculation Chain to avoid corruption
                            If doc.WorkbookPart.CalculationChainPart IsNot Nothing Then
                                doc.WorkbookPart.DeletePart(doc.WorkbookPart.CalculationChainPart)
                            End If

                            Dim workbookPart As WorkbookPart = doc.WorkbookPart
                            Dim sheet As Sheet = workbookPart.Workbook.Descendants(Of Sheet)().FirstOrDefault()
                            
                            If sheet Is Nothing Then
                                Throw New Exception("No sheet found in template file.")
                            End If

                            Dim worksheetPart As WorksheetPart = CType(workbookPart.GetPartById(sheet.Id), WorksheetPart)
                            Dim sheetData As SheetData = worksheetPart.Worksheet.Elements(Of SheetData)().First()
                            
                            ' Define column letters for data (C-K based on new Excel structure)
                            ' C = HTD, D = Horse Aveiro, E = Horse OYAK, F = Horse Romania, 
                            ' G = Horse Brazil, H = Horse Chile, I = Horse Argentina, J = Horse Spain, K = Horse Holding
                            Dim entityColumns As String() = {"C", "D", "E", "F", "G", "H", "I", "J", "K"}
                            Dim entityKeys As String() = {"HTD", "HorseAveiro", "HorseOYAK", "HorseRomania", "HorseBrazil", "HorseChile", "HorseArgentina", "HorseSpain", "HorseHolding"}
                            
                            ' ==========================================================================================
                            ' UPDATE HEADER DATES (Column C for dates based on new structure)
                            ' Row 4: STARTING week of -> C4
                            ' Row 5: ENDING week of -> C5
                            ' Row 6: END OF CURRENT MONTH -> C6
                            ' Row 10: WEEK X - Start of DD-MMM -> B10
                            ' Row 40: WEEK X - Start of DD-MMM (EndWeek) -> B40
                            ' Row 70: WEEK X - Start of DD-MMM (EOM) -> B70
                            ' ==========================================================================================
                            Me.UpdateCellValue(sheetData, "C4", mondayDate)
                            Me.UpdateCellValue(sheetData, "C5", endWeekFormatted)
                            Me.UpdateCellValue(sheetData, "C6", eomFormatted)
                            Me.UpdateCellValue(sheetData, "B10", "WEEK " & targetWeekStr & " - Start of " & mondayDate)
                            Me.UpdateCellValue(sheetData, "B40", "WEEK " & targetWeekStr & " - Start of " & endWeekFormatted)
                            Me.UpdateCellValue(sheetData, "B70", "WEEK " & targetWeekStr & " - Start Of " & eomFormatted)
                            
                            ' ==========================================================================================
                            ' SECTION 1: ACTUAL (StartWeek) - Data rows start at row 15
                            ' Row 14: CASH AND FINANCING BALANCE (header)
                            ' Row 15: INTernal Cash (+)
                            ' Row 16: EXTernal Cash (+)
                            ' Row 17: (blank)
                            ' Row 18: Cash available (total)
                            ' Row 19: Flow to last week
                            ' Row 20: (blank)
                            ' Row 21: FINANCING - USED LINES (header)
                            ' Row 22: INTernal Debt (-)
                            ' Row 23: EXTernal Debt (-)
                            ' Row 24: (blank)
                            ' Row 25: Utilized debt (total)
                            ' Row 26: Flow to last week
                            ' Row 27: (blank)
                            ' Row 28: FINANCING - AVAILABLE LINES (header)
                            ' Row 29: INTernal Debt (-)
                            ' Row 30: EXTernal Debt (-)
                            ' Row 31: (blank)
                            ' Row 32: Available financing (total)
                            ' Row 33: Flow to last week
                            ' Row 34: (blank)
                            ' Row 35: Net Funding position
                            ' Row 36: Flow to last week
                            ' ==========================================================================================
                            Me.PopulateSectionNew(sheetData, aggregatedData, "StartWeek", "StartWeekPrev", entityColumns, entityKeys, 15)
                            
                            ' ==========================================================================================
                            ' SECTION 2: END WEEK FORECAST - Data rows start at row 45
                            ' Same structure as ACTUAL, offset by 30 rows
                            ' ==========================================================================================
                            Me.PopulateSectionNew(sheetData, aggregatedData, "EndWeek", "StartWeek", entityColumns, entityKeys, 45)
                            
                            ' ==========================================================================================
                            ' SECTION 3: END OF MONTH - Data rows start at row 75
                            ' Same structure as ACTUAL, offset by 60 rows
                            ' ==========================================================================================
                            Me.PopulateSectionNew(sheetData, aggregatedData, "EOM", "StartWeek", entityColumns, entityKeys, 75)
                            
                            ' Save worksheet and workbook
                            worksheetPart.Worksheet.Save()
                            workbookPart.Workbook.Save()
                        End Using

                        ' ==========================================================================================
                        ' 6. SAVE OUTPUT FILE
                        ' ==========================================================================================
                        Try
                            BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, outputPath)
                        Catch deleteEx As Exception
                            ' File may not exist, continue
                        End Try
                        
                        Dim outputBytes As Byte() = ms.ToArray()
                        Dim newFileInfo As New XFFileInfo(FileSystemLocation.ApplicationDatabase, outputFileName, outputFolder)
                        Dim newFile As New XFFile(newFileInfo, String.Empty, outputBytes)
                        BRApi.FileSystem.InsertOrUpdateFile(si, newFile)
                    End Using
                End Using

                Return True

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        #Region "SQL Query - Get CashDebt Data"
        
        Private Function GetCashDebtData(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, _
                                          ByVal timeKey As String, ByVal scenario As String, _
                                          ByVal validWeek As Integer, ByVal previousWeek As Integer) As DataTable
            Try
                ' Calculate previous week timekey and scenario
                Dim currentDate As DateTime = DateTime.ParseExact(timeKey, "yyyyMMdd", CultureInfo.InvariantCulture)
                Dim previousDate As DateTime = currentDate.AddDays(-7)
                Dim previousTimeKey As String = previousDate.ToString("yyyyMMdd")
                Dim previousScenario As String = "FW" + previousWeek.ToString().PadLeft(2, "0"c)
                
                ' Calculate end week timekey (4 weeks later)
                Dim endWeekDate As DateTime = currentDate.AddDays(28)
                Dim endWeekTimeKey As String = endWeekDate.ToString("yyyyMMdd")
                
                ' Calculate EOM timekey
                Dim eomDate As DateTime = New DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month))
                Dim eomTimeKey As String = eomDate.ToString("yyyyMMdd")
                
                Dim sql As String = $"
                    -- Get all projection types for current week
                    SELECT 
                        Entity,
                        Account,
                        Flow,
                        Bank,
                        ProjectionType,
                        Amount
                    FROM XFC_TRS_Master_CashDebtPosition
                    WHERE UploadTimekey = '{timeKey}'
                    AND Scenario = '{scenario.Replace("'", "''")}'
                    
                    UNION ALL
                    
                    -- Get previous week StartWeek for comparison
                    SELECT 
                        Entity,
                        Account,
                        Flow,
                        Bank,
                        'StartWeekPrev' AS ProjectionType,
                        Amount
                    FROM XFC_TRS_Master_CashDebtPosition
                    WHERE UploadTimekey = '{previousTimeKey}'
                    AND Scenario = '{previousScenario}'
                    AND ProjectionType = 'StartWeek'
                "
                
                Return BRApi.Database.ExecuteSql(dbConn, sql, False)
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
        
        #End Region
        
        #Region "Data Aggregation"
        
        ''' <summary>
        ''' Aggregates all data into a nested dictionary structure:
        ''' ProjectionType -> Account -> Flow -> Entity -> Amount
        ''' </summary>
        Private Function AggregateAllData(ByVal data As DataTable) _
            As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))))
            Try
                ' Structure: ProjectionType -> Account -> Flow -> Entity -> Amount
                Dim result As New Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))))
                
                ' Define HTD entities
                Dim htdEntities As String() = {"Horse Powertrain Solution", "Horse Spain", "OYAK HORSE", "Horse Brazil", 
                                                "Horse Chile", "Horse Argentina", "Horse Romania", "Horse Aveiro"}
                
                For Each dr As DataRow In data.Rows
                    Dim entity As String = If(IsDBNull(dr("Entity")), "", dr("Entity").ToString().Trim())
                    Dim account As String = If(IsDBNull(dr("Account")), "", dr("Account").ToString())
                    Dim flow As String = If(IsDBNull(dr("Flow")), "", dr("Flow").ToString())
                    Dim projType As String = If(IsDBNull(dr("ProjectionType")), "", dr("ProjectionType").ToString())
                    
                    Dim amount As Decimal = 0
                    If Not IsDBNull(dr("Amount")) Then
                        amount = Convert.ToDecimal(dr("Amount"))
                    End If
                    
                    ' Initialize projection type dictionary if needed
                    If Not result.ContainsKey(projType) Then
                        result(projType) = New Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal)))
                    End If
                    
                    ' Initialize account dictionary if needed
                    If Not result(projType).ContainsKey(account) Then
                        result(projType)(account) = New Dictionary(Of String, Dictionary(Of String, Decimal))
                    End If
                    
                    ' Initialize flow dictionary if needed
                    If Not result(projType)(account).ContainsKey(flow) Then
                        result(projType)(account)(flow) = New Dictionary(Of String, Decimal) From {
                            {"HTD", 0}, {"HorseAveiro", 0}, {"HorseOYAK", 0}, {"HorseRomania", 0}, {"HorseBrazil", 0},
                            {"HorseChile", 0}, {"HorseArgentina", 0}, {"HorseSpain", 0}, {"HorseHolding", 0}
                        }
                    End If
                    
                    ' Normalize entity name for comparison
                    Dim entityUpper As String = entity.ToUpper()
                    
                    ' Aggregate values by entity group
                    If htdEntities.Any(Function(e) e.ToUpper() = entityUpper) Then
                        result(projType)(account)(flow)("HTD") += amount
                        
                        ' Also map to individual entities
                        If String.Equals(entity, "Horse Aveiro", StringComparison.OrdinalIgnoreCase) Then 
                            result(projType)(account)(flow)("HorseAveiro") += amount
                        End If
                        If String.Equals(entity, "OYAK HORSE", StringComparison.OrdinalIgnoreCase) Then 
                            result(projType)(account)(flow)("HorseOYAK") += amount
                        End If
                        If String.Equals(entity, "Horse Romania", StringComparison.OrdinalIgnoreCase) Then 
                            result(projType)(account)(flow)("HorseRomania") += amount
                        End If
                        If String.Equals(entity, "Horse Brazil", StringComparison.OrdinalIgnoreCase) Then 
                            result(projType)(account)(flow)("HorseBrazil") += amount
                        End If
                        If String.Equals(entity, "Horse Chile", StringComparison.OrdinalIgnoreCase) Then 
                            result(projType)(account)(flow)("HorseChile") += amount
                        End If
                        If String.Equals(entity, "Horse Argentina", StringComparison.OrdinalIgnoreCase) Then 
                            result(projType)(account)(flow)("HorseArgentina") += amount
                        End If
                        If String.Equals(entity, "Horse Spain", StringComparison.OrdinalIgnoreCase) Then 
                            result(projType)(account)(flow)("HorseSpain") += amount
                        End If
                        If String.Equals(entity, "Horse Powertrain Solution", StringComparison.OrdinalIgnoreCase) Then 
                            result(projType)(account)(flow)("HorseHolding") += amount
                        End If
                    End If
                Next
                
                Return result
            Catch ex As Exception
                Throw ex
            End Try
        End Function
        
        #End Region
        
        #Region "Section Population"
        
        ''' <summary>
        ''' Populates a complete section (StartWeek, EndWeek, or EOM) with data
        ''' CORRECT STRUCTURE (Nov 2025 - Verified from Excel):
        ''' Row +0: INTernal Cash (+)       [Fila 15/45/75]
        ''' Row +1: EXTernal Cash (+)       [Fila 16/46/76]
        ''' Row +2: Cash available (total)  [Fila 17/47/77]
        ''' Row +3: (blank)                 [Fila 18/48/78]
        ''' Row +4: Flow to last week       [Fila 19/49/79]
        ''' Row +5: (blank)                 [Fila 20/50/80]
        ''' Row +6: FINANCING - USED LINES (header) [Fila 21/51/81]
        ''' Row +7: INTernal Debt (-)       [Fila 22/52/82]
        ''' Row +8: EXTernal Debt (-)       [Fila 23/53/83]
        ''' Row +9: Utilized debt (total)   [Fila 24/54/84]
        ''' Row +10: (blank)                [Fila 25/55/85]
        ''' Row +11: Flow to last week      [Fila 26/56/86]
        ''' Row +12: (blank)                [Fila 27/57/87]
        ''' Row +13: FINANCING - AVAILABLE LINES (header) [Fila 28/58/88]
        ''' Row +14: INTernal Debt (-)      [Fila 29/59/89]
        ''' Row +15: EXTernal Debt (-)      [Fila 30/60/90]
        ''' Row +16: Available financing    [Fila 31/61/91]
        ''' Row +17: (blank)                [Fila 32/62/92]
        ''' Row +18: Flow to last week      [Fila 33/63/93]
        ''' Row +19: (blank)                [Fila 34/64/94]
        ''' Row +20: Net Funding position   [Fila 35/65/95]
        ''' Row +21: Flow to last week      [Fila 36/66/96]
        ''' </summary>
        Private Sub PopulateSectionNew(ByVal sheetData As SheetData, _
                                     ByVal aggregatedData As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal)))), _
                                     ByVal projectionType As String, _
                                     ByVal comparisonType As String, _
                                     ByVal entityColumns As String(), _
                                     ByVal entityKeys As String(), _
                                     ByVal startRow As Integer)
            Try
                ' Get data for this projection type
                Dim sectionData As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))) = Nothing
                If aggregatedData.ContainsKey(projectionType) Then
                    sectionData = aggregatedData(projectionType)
                End If
                
                ' Get comparison data (for "Flow to last week" rows)
                Dim comparisonData As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))) = Nothing
                If aggregatedData.ContainsKey(comparisonType) Then
                    comparisonData = aggregatedData(comparisonType)
                End If
                
                ' ========== SECTION: CASH AND FINANCING BALANCE ==========
                Dim cashData As Dictionary(Of String, Dictionary(Of String, Decimal)) = Nothing
                If sectionData IsNot Nothing AndAlso sectionData.ContainsKey("CASH AND FINANCING BALANCE") Then
                    cashData = sectionData("CASH AND FINANCING BALANCE")
                End If
                
                Dim cashCompData As Dictionary(Of String, Dictionary(Of String, Decimal)) = Nothing
                If comparisonData IsNot Nothing AndAlso comparisonData.ContainsKey("CASH AND FINANCING BALANCE") Then
                    cashCompData = comparisonData("CASH AND FINANCING BALANCE")
                End If
                
                ' Row +0: INTernal Cash (+) [Fila 15/45/75]
                Me.PopulateDataRow(sheetData, startRow, entityColumns, entityKeys, cashData, "INTernal Cash (+)")
                
                ' Row +1: EXTernal Cash (+) [Fila 16/46/76]
                Me.PopulateDataRow(sheetData, startRow + 1, entityColumns, entityKeys, cashData, "EXTernal Cash (+)")
                
                ' Row +2: Cash available (total = INTernal + EXTernal) [Fila 17/47/77]
                Dim cashTotals As Dictionary(Of String, Decimal) = Me.CalculateFlowTotals(cashData, New String() {"INTernal Cash (+)", "EXTernal Cash (+)"})
                Me.PopulateTotalRow(sheetData, startRow + 2, entityColumns, entityKeys, cashTotals)
                
                ' Row +4: Flow to last week [Fila 19/49/79]
                Dim cashCompTotals As Dictionary(Of String, Decimal) = Me.CalculateFlowTotals(cashCompData, New String() {"INTernal Cash (+)", "EXTernal Cash (+)"})
                Me.PopulateFlowRow(sheetData, startRow + 4, entityColumns, entityKeys, cashTotals, cashCompTotals)
                
                ' ========== SECTION: FINANCING - USED LINES ==========
                Dim usedLinesData As Dictionary(Of String, Dictionary(Of String, Decimal)) = Nothing
                If sectionData IsNot Nothing AndAlso sectionData.ContainsKey("FINANCING - USED LINES") Then
                    usedLinesData = sectionData("FINANCING - USED LINES")
                End If
                
                Dim usedLinesCompData As Dictionary(Of String, Dictionary(Of String, Decimal)) = Nothing
                If comparisonData IsNot Nothing AndAlso comparisonData.ContainsKey("FINANCING - USED LINES") Then
                    usedLinesCompData = comparisonData("FINANCING - USED LINES")
                End If
                
                ' Row +7: INTernal Debt (-) [Fila 22/52/82]
                Me.PopulateDataRow(sheetData, startRow + 7, entityColumns, entityKeys, usedLinesData, "INTernal Debt (-)")
                
                ' Row +8: EXTernal Debt (-) [Fila 23/53/83]
                Me.PopulateDataRow(sheetData, startRow + 8, entityColumns, entityKeys, usedLinesData, "EXTernal Debt (-)")
                
                ' Row +9: Utilized debt (total) [Fila 24/54/84]
                Dim usedLinesTotals As Dictionary(Of String, Decimal) = Me.CalculateFlowTotals(usedLinesData, New String() {"INTernal Debt (-)", "EXTernal Debt (-)"})
                Me.PopulateTotalRow(sheetData, startRow + 9, entityColumns, entityKeys, usedLinesTotals)
                
                ' Row +11: Flow to last week [Fila 26/56/86]
                Dim usedLinesCompTotals As Dictionary(Of String, Decimal) = Me.CalculateFlowTotals(usedLinesCompData, New String() {"INTernal Debt (-)", "EXTernal Debt (-)"})
                Me.PopulateFlowRow(sheetData, startRow + 11, entityColumns, entityKeys, usedLinesTotals, usedLinesCompTotals)
                
                ' ========== SECTION: FINANCING - AVAILABLE LINES ==========
                Dim availableLinesData As Dictionary(Of String, Dictionary(Of String, Decimal)) = Nothing
                If sectionData IsNot Nothing AndAlso sectionData.ContainsKey("FINANCING - AVAILABLE LINES") Then
                    availableLinesData = sectionData("FINANCING - AVAILABLE LINES")
                End If
                
                Dim availableLinesCompData As Dictionary(Of String, Dictionary(Of String, Decimal)) = Nothing
                If comparisonData IsNot Nothing AndAlso comparisonData.ContainsKey("FINANCING - AVAILABLE LINES") Then
                    availableLinesCompData = comparisonData("FINANCING - AVAILABLE LINES")
                End If
                
                ' Row +14: INTernal Debt (-) [Fila 29/59/89]
                Me.PopulateDataRow(sheetData, startRow + 14, entityColumns, entityKeys, availableLinesData, "INTernal Debt (-)")
                
                ' Row +15: EXTernal Debt (-) [Fila 30/60/90]
                Me.PopulateDataRow(sheetData, startRow + 15, entityColumns, entityKeys, availableLinesData, "EXTernal Debt (-)")
                
                ' Row +16: Available financing (total) [Fila 31/61/91]
                ' Note: Available financing uses only EXTernal Debt (not INTernal + EXTernal)
                Dim availableLinesTotals As Dictionary(Of String, Decimal) = Me.CalculateFlowTotals(availableLinesData, New String() {"EXTernal Debt (-)"})
                Me.PopulateTotalRow(sheetData, startRow + 16, entityColumns, entityKeys, availableLinesTotals)
                
                ' Row +18: Flow to last week [Fila 33/63/93]
                Dim availableLinesCompTotals As Dictionary(Of String, Decimal) = Me.CalculateFlowTotals(availableLinesCompData, New String() {"EXTernal Debt (-)"})
                Me.PopulateFlowRow(sheetData, startRow + 18, entityColumns, entityKeys, availableLinesTotals, availableLinesCompTotals)
                
                ' ========== NET FUNDING POSITION ==========
                ' Row +20: Net Funding position = Cash available - Utilized debt
                Dim netFundingCurrent As New Dictionary(Of String, Decimal)
                Dim netFundingComp As New Dictionary(Of String, Decimal)
                
                For Each entityKey As String In entityKeys
                    Dim cashVal As Decimal = If(cashTotals.ContainsKey(entityKey), cashTotals(entityKey), 0)
                    Dim debtVal As Decimal = If(usedLinesTotals.ContainsKey(entityKey), usedLinesTotals(entityKey), 0)
                    netFundingCurrent(entityKey) = cashVal - debtVal
                    
                    Dim cashCompVal As Decimal = If(cashCompTotals.ContainsKey(entityKey), cashCompTotals(entityKey), 0)
                    Dim debtCompVal As Decimal = If(usedLinesCompTotals.ContainsKey(entityKey), usedLinesCompTotals(entityKey), 0)
                    netFundingComp(entityKey) = cashCompVal - debtCompVal
                Next
                
                Me.PopulateTotalRow(sheetData, startRow + 20, entityColumns, entityKeys, netFundingCurrent)
                
                ' Row +21: Flow to last week/forecast start
                Me.PopulateFlowRow(sheetData, startRow + 21, entityColumns, entityKeys, netFundingCurrent, netFundingComp)
                
            Catch ex As Exception
                ' Log error but don't fail the whole process
            End Try
        End Sub
        
        #End Region
        
        #Region "Excel Population Helpers"
        
        ''' <summary>
        ''' Updates a specific cell value while preserving its style
        ''' </summary>
        Private Sub UpdateCellValue(ByVal sheetData As SheetData, ByVal cellReference As String, ByVal value As String)
            Try
                ' Extract row number from cell reference (e.g., "B3" -> 3)
                Dim rowIndex As UInteger = UInteger.Parse(System.Text.RegularExpressions.Regex.Match(cellReference, "\d+").Value)
                Dim colName As String = System.Text.RegularExpressions.Regex.Match(cellReference, "[A-Z]+").Value
                
                ' Find or create the row
                Dim row As Row = sheetData.Elements(Of Row)().FirstOrDefault(Function(r) r.RowIndex.Value = rowIndex)
                
                If row Is Nothing Then
                    row = New Row() With {.RowIndex = rowIndex}
                    sheetData.Append(row)
                End If
                
                ' Find or create the cell
                Dim cell As Cell = row.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = cellReference)
                
                If cell Is Nothing Then
                    cell = New Cell() With {.CellReference = cellReference}
                    row.Append(cell)
                End If
                
                ' Preserve existing style
                Dim currentStyleIndex As UInteger? = cell.StyleIndex
                
                ' Set value as inline string
                cell.DataType = CellValues.InlineString
                cell.RemoveAllChildren()
                Dim inlineString As New InlineString()
                inlineString.Append(New Text(value))
                cell.Append(inlineString)
                
                ' Restore style
                If currentStyleIndex.HasValue Then
                    cell.StyleIndex = currentStyleIndex
                End If
            Catch ex As Exception
                ' Silently fail for header updates
            End Try
        End Sub
        
        ''' <summary>
        ''' Updates a specific cell with a numeric value while preserving its style
        ''' </summary>
        Private Sub UpdateCellNumberValue(ByVal sheetData As SheetData, ByVal cellReference As String, ByVal value As Decimal)
            Try
                ' Extract row number from cell reference
                Dim rowIndex As UInteger = UInteger.Parse(System.Text.RegularExpressions.Regex.Match(cellReference, "\d+").Value)
                Dim colName As String = System.Text.RegularExpressions.Regex.Match(cellReference, "[A-Z]+").Value
                
                ' Find the row
                Dim row As Row = sheetData.Elements(Of Row)().FirstOrDefault(Function(r) r.RowIndex.Value = rowIndex)
                
                If row Is Nothing Then Return
                
                ' Find the cell
                Dim cell As Cell = row.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = cellReference)
                
                If cell Is Nothing Then Return
                
                ' Preserve existing style
                Dim currentStyleIndex As UInteger? = cell.StyleIndex
                
                ' Set value as number
                cell.DataType = CellValues.Number
                cell.CellValue = New CellValue(value.ToString(CultureInfo.InvariantCulture))
                
                ' Restore style
                If currentStyleIndex.HasValue Then
                    cell.StyleIndex = currentStyleIndex
                End If
            Catch ex As Exception
                ' Silently fail
            End Try
        End Sub
        
        ''' <summary>
        ''' Populates a data row with values from aggregated data
        ''' </summary>
        Private Sub PopulateDataRow(ByVal sheetData As SheetData, ByVal rowIndex As Integer, _
                                     ByVal entityColumns As String(), ByVal entityKeys As String(), _
                                     ByVal accountData As Dictionary(Of String, Dictionary(Of String, Decimal)), _
                                     ByVal flowName As String)
            Try
                If accountData Is Nothing Then Return
                
                Dim flowData As Dictionary(Of String, Decimal) = Nothing
                If accountData.ContainsKey(flowName) Then
                    flowData = accountData(flowName)
                End If
                
                For i As Integer = 0 To entityColumns.Length - 1
                    Dim col As String = entityColumns(i)
                    Dim entityKey As String = entityKeys(i)
                    Dim cellRef As String = col & rowIndex.ToString()
                    
                    Dim value As Decimal = 0
                    If flowData IsNot Nothing AndAlso flowData.ContainsKey(entityKey) Then
                        value = flowData(entityKey)
                    End If
                    
                    Me.UpdateCellNumberValue(sheetData, cellRef, value)
                Next
            Catch ex As Exception
                ' Silently fail
            End Try
        End Sub
        
        ''' <summary>
        ''' Populates a total row with pre-calculated totals
        ''' </summary>
        Private Sub PopulateTotalRow(ByVal sheetData As SheetData, ByVal rowIndex As Integer, _
                                      ByVal entityColumns As String(), ByVal entityKeys As String(), _
                                      ByVal totals As Dictionary(Of String, Decimal))
            Try
                For i As Integer = 0 To entityColumns.Length - 1
                    Dim col As String = entityColumns(i)
                    Dim entityKey As String = entityKeys(i)
                    Dim cellRef As String = col & rowIndex.ToString()
                    
                    Dim value As Decimal = 0
                    If totals IsNot Nothing AndAlso totals.ContainsKey(entityKey) Then
                        value = totals(entityKey)
                    End If
                    
                    Me.UpdateCellNumberValue(sheetData, cellRef, value)
                Next
            Catch ex As Exception
                ' Silently fail
            End Try
        End Sub
        
        ''' <summary>
        ''' Populates a "Flow to last week" row with the difference between current and comparison
        ''' </summary>
        Private Sub PopulateFlowRow(ByVal sheetData As SheetData, ByVal rowIndex As Integer, _
                                     ByVal entityColumns As String(), ByVal entityKeys As String(), _
                                     ByVal currentTotals As Dictionary(Of String, Decimal), _
                                     ByVal comparisonTotals As Dictionary(Of String, Decimal))
            Try
                For i As Integer = 0 To entityColumns.Length - 1
                    Dim col As String = entityColumns(i)
                    Dim entityKey As String = entityKeys(i)
                    Dim cellRef As String = col & rowIndex.ToString()
                    
                    Dim currentValue As Decimal = 0
                    Dim compValue As Decimal = 0
                    
                    If currentTotals IsNot Nothing AndAlso currentTotals.ContainsKey(entityKey) Then
                        currentValue = currentTotals(entityKey)
                    End If
                    
                    If comparisonTotals IsNot Nothing AndAlso comparisonTotals.ContainsKey(entityKey) Then
                        compValue = comparisonTotals(entityKey)
                    End If
                    
                    Dim difference As Decimal = currentValue - compValue
                    Me.UpdateCellNumberValue(sheetData, cellRef, difference)
                Next
            Catch ex As Exception
                ' Silently fail
            End Try
        End Sub
        
        ''' <summary>
        ''' Calculates totals for specified flows
        ''' </summary>
        Private Function CalculateFlowTotals(ByVal accountData As Dictionary(Of String, Dictionary(Of String, Decimal)), _
                                              ByVal flowNames As String()) As Dictionary(Of String, Decimal)
            Try
                Dim totals As New Dictionary(Of String, Decimal) From {
                    {"HTD", 0}, {"HorseAveiro", 0}, {"HorseOYAK", 0}, {"HorseRomania", 0}, {"HorseBrazil", 0},
                    {"HorseChile", 0}, {"HorseArgentina", 0}, {"HorseSpain", 0}, {"HorseHolding", 0}
                }
                
                If accountData Is Nothing Then Return totals
                
                For Each flowName As String In flowNames
                    If accountData.ContainsKey(flowName) Then
                        Dim flowData As Dictionary(Of String, Decimal) = accountData(flowName)
                        For Each entityKey As String In totals.Keys.ToList()
                            If flowData.ContainsKey(entityKey) Then
                                totals(entityKey) += flowData(entityKey)
                            End If
                        Next
                    End If
                Next
                
                Return totals
            Catch ex As Exception
                Return New Dictionary(Of String, Decimal) From {
                    {"HTD", 0}, {"HorseAveiro", 0}, {"HorseOYAK", 0}, {"HorseRomania", 0}, {"HorseBrazil", 0},
                    {"HorseChile", 0}, {"HorseArgentina", 0}, {"HorseSpain", 0}, {"HorseHolding", 0}
                }
            End Try
        End Function
        
        #End Region

    End Class
End Namespace
