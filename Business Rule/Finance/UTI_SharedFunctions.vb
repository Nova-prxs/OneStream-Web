Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
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



Namespace OneStream.BusinessRule.Finance.UTI_SharedFunctions
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Variables"
		
		#Region "Profile Name To Brand Parent Entity Dict"
		
		Public Function getProfileNameToBrandParentEntityDict(ByVal si As SessionInfo, ByVal profileName As String) As String
			'Workflow Profile Name - Brand Parent Entity dictionary to map
			Dim profileNameToBrandParentEntityDict As New Dictionary(Of String, String) From {
				{"Planning_BK", "M_BK"},
				{"Planning_Dominos", "M_DP"},
				{"Planning_FH", "M_FH"},
				{"Planning_FHVLC", "M_FH"},
				{"Planning_Fridays", "M_FR"},
				{"Planning_Ginos", "M_GI"},
				{"Planning_GinosPT", "M_GI"},
				{"Planning_Starbucks", "M_SB"},
				{"Planning_StarbucksBE", "M_SB"},
				{"Planning_StarbucksFR", "M_SB"},
				{"Planning_StarbucksNL", "M_SB"},
				{"Planning_StarbucksPT", "M_SB"},
				{"Planning_VIPS", "M_VI"}
			}
			Return profileNameToBrandParentEntityDict(profileName)
		End Function
		
		#End Region
		
		#Region "Brand Name To Brand Parent Entity Dict"
		
		Public Function getBrandNameToBrandParentEntityDict(ByVal si As SessionInfo, ByVal brandName As String) As String
			'Brand Name - Brand Parent Entity dictionary to map
			Dim brandNameToBrandParentEntityDict As New Dictionary(Of String, String) From {
				{"Burger King", "F_BK"},
				{"Domino''s Pizza", "F_DP"},
				{"Foster''s Hollywood", "F_FH"},
				{"Foster''s Hollywood Valencia", "F_FHV"},
				{"Fridays", "F_FR"},
				{"Ginos", "F_GI"},
				{"Ginos Portugal", "F_GI_PT"},
				{"Starbucks", "F_SB"},
				{"Starbucks Bélgica", "F_SB_BEL"},
				{"Starbucks Francia", "F_SB_FR"},
				{"Starbucks Holanda", "F_SB_HO"},
				{"Starbucks Portugal", "F_SB_PT"},
				{"VIPS", "F_VI"}
			}
			Return brandNameToBrandParentEntityDict(brandName)
		End Function
		
		#End Region
		
		#End Region
		
		#Region "Is Closed"
		
		Public Function IsClosed(ByVal si As SessionInfo, ByVal currentYear As String, ByVal currentMonth As Integer, ByVal sEntity As String) As Boolean
		
			'Declare closing datatable variable
			Dim closingDt As DataTable
		
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
				'Select the closing month and year for that entity and populate the datatable
				Dim selectString As String = $"
					SELECT TOP(1)
					    MONTH(close_date) AS month,
						YEAR(close_date) AS year
					FROM XFC_CEBESClosings
					WHERE cebe = '{sEntity}'
				"
				
				closingDt = BRApi.Database.ExecuteSql(dbConn, selectString, False)
				
			End Using
				
			'Get the day, month and year
			Dim closingMonth As Integer
			Dim closingYear As Integer
			
			If closingDt.Rows.Count > 0 Then
				
				closingMonth = CInt(closingDt.Rows(0)("month"))
				closingYear = CInt(closingDt.Rows(0)("year"))
				
			Else
				
				closingMonth = 12
				closingYear = 2100
				
			End If
			
			'Control if month is 13 to change month to 1 and year to the next year
			If closingMonth = 13 Then
				
				closingMonth = 1
				closingYear += 1
				
			End If
			
			'Create integers that define year and month for current and closing dates
			Dim closingDate As Integer = closingYear * 100 + closingMonth
			Dim currentDate As Integer = currentYear * 100 + currentMonth
			
			'Return True if current date is bigger than or equals closing date, else False
			
			'JG 18/07/2025
			'If (currentDate >= closingDate) 
			If (currentDate > closingDate) 
				Return True
			Else				
				Return False
			End If
		
		End Function
		
		#End Region
		
		#Region "Member Lookup"

		Public Function CreateMemberLookupUsingFilter(ByVal si As SessionInfo, ByVal dimensionName As String, ByVal memberFilter As String) As Dictionary(Of String, MemberInfo)			
			Try
				'Define the dictionary that will act as the lookup (Note, the last part of the declaration makes the look case insensitive)
				Dim memLookup As New Dictionary(Of String, MemberInfo)(StringComparer.InvariantCultureIgnoreCase)
				
				'Execute the filter and check the result
				Dim memList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, dimensionName, memberFilter, True)
				If Not memList Is Nothing Then
					'Loop over the members and add them to the lookup dictionary (Keyed by name)
					For Each memInfo As MemberInfo In memList
						memLookup.Add(memInfo.Member.Name, memInfo)		
					Next											
				End If									
					
				'Add the bypass/unmapped members (Not a real member, but we do not to add / suspense items mapped to bypass)
				memLookup.Add(StageConstants.TransformationGeneral.BypassRow, Nothing)
				memLookup.Add(StageConstants.TransformationGeneral.DimUnmapped, Nothing)
				
				Return memLookup
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#End Region
		
		#Region "Get First Five Rows"
		
		Function GetFirstFiveRows(ByVal dt As DataTable) As String
	        Dim sb As New StringBuilder()
	
	        ' Check if the DataTable has any rows
	        If dt.Rows.Count = 0 Then
	            Return "DataTable is empty."
	        End If
	
	        ' Append the headers
	        For Each column As DataColumn In dt.Columns
	            sb.Append(column.ColumnName & vbTab)
	        Next
	        sb.AppendLine()
	
	        ' Append the first 5 rows or less if there are fewer rows in the DataTable
	        For i As Integer = 0 To Math.Min(4, dt.Rows.Count - 1)
	            For Each column As DataColumn In dt.Columns
	                sb.Append(dt.Rows(i)(column).ToString() & vbTab)
	            Next
	            sb.AppendLine()
	        Next
	
	        ' Return the built string
	        Return sb.ToString()
	    End Function
		
		#End Region
		
		#Region "Generate Insert SQL List"
		
		Public Function GenerateInsertSQLList(ByVal dataTable As DataTable, ByVal tableName As String) As List(Of String)
		    Dim insertStatements As New List(Of String)()
		    Dim sb As New StringBuilder()
		
		    ' Validate inputs
		    If dataTable Is Nothing OrElse dataTable.Rows.Count = 0 Then
		        Throw New ArgumentException("The DataTable is empty or null.")
		    End If
		
		    If String.IsNullOrEmpty(tableName) Then
		        Throw New ArgumentException("The table name is null or empty.")
		    End If
		
		    ' Get the column names
		    Dim columnNames As String = String.Join(", ", dataTable.Columns.Cast(Of DataColumn)().[Select](Function(c) c.ColumnName))
		
		    ' Batch size limit
		    Dim batchSize As Integer = 1000
		    Dim currentBatchSize As Integer = 0
		
		    ' Build the initial part of the INSERT INTO statement
		    sb.AppendLine($"INSERT INTO {tableName} ({columnNames}) VALUES")
		
		    ' Iterate through each row in the DataTable
		    For i As Integer = 0 To dataTable.Rows.Count - 1
		        Dim rowValues As New List(Of String)()
		
		        For Each column As DataColumn In dataTable.Columns
		            Dim value As Object = dataTable.Rows(i)(column)
		
		            If value Is DBNull.Value Then
		                rowValues.Add("NULL")
		            ElseIf TypeOf value Is String OrElse TypeOf value Is Char Then
		                rowValues.Add($"'{value.ToString().Replace("'", "''").Replace(",", ".")}'") ' Escape single quotes
		            ElseIf TypeOf value Is DateTime Then
		                rowValues.Add($"'{CType(value, DateTime).ToString("yyyy-MM-dd HH:mm:ss.fff")}'")
		            Else
		                rowValues.Add(value.ToString().Replace(",", "."))
		            End If
		        Next
		
		        ' Combine row values into a comma-separated string and add to StringBuilder
		        sb.AppendLine($"({String.Join(", ", rowValues)}){If(currentBatchSize < batchSize - 1 AndAlso i < dataTable.Rows.Count - 1, ",", ";")}")
		        currentBatchSize += 1
		
		        ' If the batch size limit is reached, or it's the last row, finalize the current statement and start a new one
		        If currentBatchSize = batchSize OrElse i = dataTable.Rows.Count - 1 Then
		            insertStatements.Add(sb.ToString())
		            sb.Clear()
		            If i < dataTable.Rows.Count - 1 Then
		                sb.AppendLine($"INSERT INTO {tableName} ({columnNames}) VALUES")
		            End If
		            currentBatchSize = 0
		        End If
		    Next
		
		    Return insertStatements
		End Function
		
		#End Region
		
		#Region "Copy XLSX File"
		
		Public Sub CopyXLSXFile(ByVal si As SessionInfo, fileName As String, filePath As XFFolderEx, newFileName As String)
		
			'Creamos el fichero en la ruta especificada
			Dim objXFFileEx As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath.XFFolder.FullName & "/" & fileName, True, True)		
			Dim folderName As String = objXFFileEx.XFFile.FileInfo.FolderFullName 
			Dim bytesOfFile As Byte() = objXFFileEx.XFFile.ContentFileBytes
			Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, newFileName, filePath.XFFolder.FullName & "/")
			Dim userName As String = si.AuthToken.UserName
			
			'Añadimos detalle adicional del fichero
			fileInfo.ContentFileExtension = "xlsx"
			fileInfo.Description = "Loaded by " & userName
			fileInfo.ContentFileContainsData = True
			fileInfo.XFFileType = True
			
			'Ejecutamos el copiado
			Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, bytesOfFile)
			BRApi.FileSystem.InsertOrUpdateFile(si, fileFile)
			
		End Sub
		
		#End Region
		
		#Region "Set Dates For Closed Stores"
		
		Public Sub SetDatesForClosedStores(ByVal si As SessionInfo)
			
			'TEMPORAL: Get not updating entities to set them comparable
			Dim notUpdatingEntities = Me.GetNotUpdatingEntities(si)
			
			' Convert to string
			Dim notUpdatingEntitiesString As String = String.Empty
			For Each notUpdatingEntity In notUpdatingEntities
				notUpdatingEntitiesString = notUpdatingEntitiesString &
					If(String.IsNullOrEmpty(notUpdatingEntitiesString), $"'{notUpdatingEntity}'", $", '{notUpdatingEntity}'")
			Next
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)	
				Dim sqlQuery As String = $"	-- Insert New Rows
											INSERT INTO XFC_CEBESClosings (brand, cebe, description, close_date)
											SELECT 
												CASE
													WHEN XFC_CEBES.brand IN ('SDH', 'Genéricos Grupo') THEN XFC_CEBES.sales_brand
													ELSE XFC_CEBES.brand
												END AS brand,
											    XFC_CEBES.cebe,
												XFC_CEBES.description,
											    XFC_CEBES.close_date
											FROM 
											    XFC_CEBES
											LEFT JOIN 
											    XFC_CEBESClosings ON XFC_CEBES.cebe = XFC_CEBESClosings.cebe
											WHERE 
											    XFC_CEBESClosings.cebe IS NULL;
											
											-- Update Existing Rows
											UPDATE 
											    XFC_CEBESClosings
											SET 
											    XFC_CEBESClosings.close_date = XFC_CEBES.close_date
											FROM 
											    XFC_CEBESClosings
											INNER JOIN 
											    XFC_CEBES ON XFC_CEBESClosings.cebe = XFC_CEBES.cebe
											WHERE 
											    YEAR(XFC_CEBES.close_date) != 2100;"
				
				BRApi.Database.ExecuteSql(dbConn, sqlQuery, False)
				
				Dim DeleteStatement As String = $"	DELETE cba
													FROM XFC_ComparativeCEBESAux cba
													INNER JOIN (
														SELECT cebe
														FROM XFC_CEBES
														WHERE cebe NOT IN ({notUpdatingEntitiesString})
													) c ON cba.cebe = c.cebe"
				
				BRApi.Database.ExecuteSql(dbConn, DeleteStatement, False)
				
				Dim InsertStatement As String = $"WITH CTE AS (
													    SELECT 
													        cebe,
													        date,
													        desc_annualcomparability,
													        ROW_NUMBER() OVER (PARTITION BY cebe, YEAR(date) ORDER BY date) AS rn
													    FROM 
													        XFC_ComparativeCEBES
													), RealComparability AS (
													SELECT 
													    cebe,
													    YEAR(date) AS year,
													    desc_annualcomparability
													FROM 
													    CTE
													WHERE 
													    rn = 1
													)
										
												INSERT INTO XFC_ComparativeCEBESAux (cebe, year, desc_annualcomparability)
												SELECT
													rc.cebe,
													rc.year,
													CASE
														WHEN YEAR(cc.close_date) <= rc.year THEN 'Cerradas'
														ELSE rc.desc_annualcomparability END AS desc_annualcomparability
												FROM RealComparability rc
												JOIN XFC_CEBESClosings cc ON rc.cebe = cc.cebe
												INNER JOIN (
													SELECT cebe
													FROM XFC_CEBES
													WHERE CEBE NOT IN ({notUpdatingEntitiesString})
												) c ON rc.cebe = c.cebe;
				
												UPDATE XFC_ComparativeCEBESAux
													SET desc_annualcomparability = 'Comparables'
												WHERE year IN (2025, 2026)
													AND cebe IN ({notUpdatingEntitiesString});
				
												UPDATE XFC_ComparativeCEBESAux
													SET desc_annualcomparability = 'Operativas Segundo Año'
												WHERE year IN (2025, 2026)
													AND cebe IN ('SF016347');"
				
				BRApi.Database.ExecuteSql(dbConn, InsertStatement, False)
			End Using
		End Sub
		
		#End Region
		
		#Region "Create SQL Filter"
		
		Public Function CreateSQLFilter(ByVal si As SessionInfo, ByVal parameter As String, ByVal parameterField As String)
		
			Dim parameterFilter As String
			
			'If parameter is All, no filter is added
			If parameter = "All" Or parameter = "" Then
				
				parameterFilter = ""
				
			Else
				
				parameterFilter = $"AND {parameterField} = '{parameter}'"
				
			End If
			
			Return parameterFilter
		
		End Function
		
		#End Region
		
		#Region "Create DataTable from Edited Data Rows"
		
		Public Function CreateDataTableFromEditedDataRows(ByVal si As SessionInfo, ByVal editedDataRows As List(Of XFEditedDataRow))
			'Declare DataTable
			Dim dt As New DataTable()
			
			'Set columns
			For Each kvp In editedDataRows(0).ModifiedDataRow.Items
				dt.Columns.Add(kvp.Key)
			Next
			
			'Add DataRows
			For Each editedRow In editedDataRows
				Dim dataRow As DataRow = dt.NewRow
				For Each kvp In editedRow.ModifiedDataRow.Items
					dataRow(kvp.Key) = kvp.Value
				Next
				dt.Rows.Add(dataRow)
			Next
			
			Return dt
		End Function
		
		#End Region
		
		#Region "Map and Filter Columns in DataTable"
		
		Public Function MapAndFilterColumnsInDataTable(ByVal si As SessionInfo, ByVal dt As DataTable,
			ByVal columnMappingDict As Dictionary(Of String, String)
		)
			'Loop through each column to map the name of the columns and delete non existing ones
			Dim columnsToRemove As New List(Of String)
			For Each column As DataColumn In dt.Columns
				If columnMappingDict.Keys.Contains(column.ColumnName) Then
					column.ColumnName = columnMappingDict(column.ColumnName)
				Else
					columnsToRemove.Add(column.ColumnName)
				End If
			Next
			
			'Remove all non existing columns
			For Each columnToRemove In columnsToRemove
				dt.Columns.Remove(columnToRemove)
			Next
			
			Return dt
		
		End Function
		
		#End Region
		
		#Region "Load DataTable to Custom Table"
		
		Public Sub LoadDataTableToCustomTable(ByVal si As SessionInfo, ByVal customTableName As String,
			ByVal dt As DataTable, ByVal method As String
		)
			'Clean column names
			For Each column As DataColumn In dt.Columns
				'Clean the column name
				Dim cleanColumnName As String = System.Text.RegularExpressions.Regex.Replace(column.ColumnName, "[^\w\d]", "")
				column.ColumnName = cleanColumnName
			Next
		
			'Insert to data base
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				'Handle replace method
				If method.ToLower = "replace" Then
					'Truncate table
					Dim truncateQuery As String = $"
						TRUNCATE TABLE {customTableName};
					"
					BRAPi.Database.ExecuteSql(dbConn, truncateQuery, False)
				End If
				'Declare the list of sql insert queries
				Dim insertSQLList As List(Of String) = Me.GenerateInsertSQLList(dt, customTableName)
				'Loop through all the list of sql insert queries and execute
				For Each insertSQL In insertSQLList
					BRApi.Database.ExecuteSql(dbConn, insertSQL, False)
				Next
			End Using
		End Sub
		
		#End Region
		
		#Region "Clear Data Buffer"
		
		Public Sub ClearDataBuffer(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal targetDataBuffer As DataBuffer)
			'Build clean data buffer
			Dim cleanDataBuffer As New DataBuffer
			Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
			For Each targetDataBufferCell In targetDataBuffer.DataBufferCells.Values
				If Not targetDataBufferCell.CellStatus.IsNoData Then
					targetDataBufferCell.CellAmount = 0
					targetDataBufferCell.CellStatus = New DataCellStatus(targetDataBufferCell.CellStatus, DataCellExistenceType.NoData)
					cleanDataBuffer.SetCell(si, targetDataBufferCell)
				End If
			Next
			'Clean data buffer
			api.Data.SetDataBuffer(cleanDataBuffer, cleanExpDestInfo,,,,,,,,,,,,,True)
		End Sub
		
		#End Region
		
		#Region "Get not updating entities"
		
		Public Function GetNotUpdatingEntities(si As SessionInfo) As HashSet(Of String)
		
			Return New HashSet(Of String) From {
				"F001194", "F001468", "F001581", "SF011145", "SF011010", "SF016347"
			}
			
		End Function
				
		#End Region
		
	End Class
End Namespace