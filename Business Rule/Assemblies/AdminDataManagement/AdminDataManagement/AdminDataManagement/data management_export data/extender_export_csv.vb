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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_export_csv
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						' Get parameters
						Dim tableName As String = args.NameValuePairs("p_import_table_name")
						Dim folderPath As String = args.NameValuePairs("p_import_folder") & "/Processed"
						Dim delimiter As String = args.NameValuePairs("p_import_delimiter")
						
						' Declare available tables to update the import file
						Dim availableTables As New HashSet(Of String) From {
							"XFC_MAIN_RAW_AccountPnLToConso",
							"XFC_MAIN_RAW_AccountBSToConso",
							"XFC_MAIN_RAW_AccountsOldToNew",
							"XFC_MAIN_RAW_CostCenterClassToRubric",
							"XFC_MAIN_MASTER_FlowMappings",
							"XFC_MAIN_MASTER_CostCentersOldToNew",
							"XFC_MAIN_MASTER_CostCentersOldToOneStreamMember",
							"XFC_MAIN_RAW_CustomersToUD1IC"
						}
						
						If Not availableTables.Contains(tableName) Then Return Nothing
						
						' Get the table data, convert it to csv and save the file
						Dim dt As DataTable = Me.GetTableData(si, tableName)
						dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, Me.GetColumnMappingDict(si, tableName))
						Dim csvString As String = UTISharedFunctionsBR.DataTableToCsvString(dt, delimiter)
						Dim fileBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(csvString)
						
						Dim fileName As String = Me.GetFileName(si, tableName)
						Dim dbFileInfo As New XFFileInfo(FileSystemLocation.ApplicationDatabase, fileName, folderPath, XFFileType.Unknown)
						dbFileInfo.ContentFileContainsData = True
						dbFileInfo.ContentFileExtension = dbFileInfo.Extension
						Dim dbFile As New XFFile(dbFileInfo, String.Empty, fileBytes)
						BRApi.FileSystem.InsertOrUpdateFile(si, dbFile)
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Helper Functions"
		
		#Region "Get File Name"
		
		Private Function GetFileName(ByVal si As SessionInfo, ByVal tableName As String) As String
			'Declare new dictionary
			Dim tableToFileName As New Dictionary(Of String, String) From {
				{"XFC_MAIN_RAW_AccountPnLToConso", "P&L Rubrics"},
				{"XFC_MAIN_RAW_AccountBSToConso", "BS Rubrics"},
				{"XFC_MAIN_RAW_AccountsOldToNew", "Accounts old to new"},
				{"XFC_MAIN_RAW_CostCenterClassToRubric", "Cost Center Class to Rubric"},
				{"XFC_MAIN_MASTER_FlowMappings", "Flow Mappings"},
				{"XFC_MAIN_MASTER_CostCentersOldToNew", "CC Old to New"},
				{"XFC_MAIN_MASTER_CostCentersOldToOneStreamMember", "Cost Center Old to UD3"},
				{"XFC_MAIN_RAW_CustomersToUD1IC", "Customer to UD1 - IC"}
			}
		
			If Not tableToFileName.ContainsKey(tableName) Then Throw New Exception($"error getting file name: no file name found for table '{tableName}'")
				
			' Get file full name from table import info
			Dim fileName As String = tableToFileName(tableName)
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim dbParamInfos As New List(Of dbParamInfo) From {
					New DbParamInfo("paramTableName", tableName)
				}
				Dim dt As DataTable = BRApi.Database.ExecuteSql(
					dbConn,
					"
					SELECT file_name
					FROM XFC_MAIN_MASTER_TableImportInfo
					WHERE table_name = @paramTableName
					",
					dbParamInfos,
					False
				)
				If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then fileName = dt.Rows(0)("file_name")
			End Using
			
			Return fileName
				
		End Function
		
		#End Region
		
		#Region "Get Column Mapping Dictionary"
		
		Private Function GetColumnMappingDict(ByVal si As SessionInfo, ByVal tableName As String) As Dictionary(Of String, String)
			'Declare new dictionary
			Dim columnMappingDict As Dictionary(Of String, String)
		
			'Control table column mapping dictionaries
			
			#Region "Raw Account PL to Conso"
			
			If tableName = "XFC_MAIN_RAW_AccountPnLToConso" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"account_name", "S4H"},
					{"account_description", "Description"},
					{"A", "A"},
					{"B", "B"},
					{"C", "C"},
					{"D", "D"},
					{"E", "E"},
					{"F", "F"},
					{"G", "G"},
					{"H", "H"},
					{"I", "I"},
					{"J", "J"},
					{"K", "K"},
					{"L", "L"},
					{"M", "M"},
					{"N", "N"},
					{"O", "O"},
					{"P", "P"},
					{"Q", "Q"},
					{"0", "0"},
					{"R", "R"},
					{"S", "S"},
					{"T", "T"},
					{"U", "U"},
					{"V", "V"},
					{"W", "W"},
					{"X", "X"},
					{"Y", "Y"},
					{"1", "1"},
					{"2", "2"},
					{"Z", "Z"},
					{"PD", "PD"},
					{"PC", "PC"},
					{"none", "None"}
				}
				
			#End Region
			
			#Region "Raw Account BS to Conso"
			
			Else If tableName = "XFC_MAIN_RAW_AccountBSToConso" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"conso_rubric", "RUBRIC"},
					{"account_name", "S4/HANA 8D"},
					{"account_description", "DESCRIPTION"}
				}
				
			#End Region
			
			#Region "Raw Accounts Old to New"
			
			Else If tableName = "XFC_MAIN_RAW_AccountsOldToNew" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"name_old", "ALCOR"},
					{"name", "S4H"}
				}
				
			#End Region
			
			#Region "Raw Cost Center Class To Rubric"
			
			Else If tableName = "XFC_MAIN_RAW_CostCenterClassToRubric" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"id", "id"},
					{"depreciation_account", "depreciation_account"},
					{"asset_account", "asset_account"}
				}
				
			#End Region
			
			#Region "Master Profit Center Mappings"
			
			Else If tableName = "XFC_MAIN_RAW_ProfitCenterMappings" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"sap_flow", "sap flow"}
				}
				
			#End Region
			
			#Region "Master Flow Mappings"
			
			Else If tableName = "XFC_MAIN_MASTER_FlowMappings" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"sap_flow", "sap flow"},
					{"flow", "os flow"}
				}
				
			#End Region
			
			#Region "Master Cost Centers Old to New"
			
			Else If tableName = "XFC_MAIN_MASTER_CostCentersOldToNew" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"id", "COST CENTER SAP4H"},
					{"id_old", "Cost center ALCOR"}
				}
				
			#End Region
			
			#Region "Master Cost Centers Old to OneStream Member"
			
			Else If tableName = "XFC_MAIN_MASTER_CostCentersOldToOneStreamMember" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"onestream_member_name", "UD3"},
					{"id_old", "Cost Center Old"}
				}
				
			#End Region
			
			#Region "Raw Customers To UD1 IC"
			
			Else If tableName = "XFC_MAIN_RAW_CustomersToUD1IC" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"id", "Interl.comercial"},
					{"customer_member_name", "UD1_Customer_OneStream"},
					{"ic_member_name", "IC_Interco_OneStream"}
				}
				
			#End Region
			
			Else
				Throw ErrorHandler.LogWrite(si, New XFException($"There is no column mapping dictionary for table {tableName}"))
			End If
			
			Return columnMappingDict
				
		End Function
		
		#End Region
		
		#Region "Get Table Data"
		
		Public Function GetTableData(ByVal si As SessionInfo, ByVal tableName As String) As DataTable
		
			' Get the table data based on the table name
			Dim dt As New DataTable
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim selectQuery As String = String.Empty
				Select tableName
					
				#Region "Raw Account PnL To Conso"
					
				Case "XFC_MAIN_RAW_AccountPnLToConso"
					selectQuery = "
						SELECT ar.account_name, a.description AS account_description,
							[A], [B], [C], [D], [E], [F], [G], [H], [I], [J], [K], [L], [M], [N], [O], [P], [Q],
							[0], [R], [S], [T], [U], [V], [W], [X], [Y], [1], [2], [Z], [PD], [PC], [none]
						FROM (
						    SELECT account_name, cost_center_class_id, conso_rubric
						    FROM XFC_MAIN_MASTER_AccountRubrics
							WHERE account_name LIKE '6%' OR account_name LIKE '7%'
						) AS SourceTable
						PIVOT (
						    MAX(conso_rubric)
						    FOR cost_center_class_id IN (
								[A], [B], [C], [D], [E], [F], [G], [H], [I], [J], [K], [L], [M], [N], [O], [P], [Q],
								[0], [R], [S], [T], [U], [V], [W], [X], [Y], [1], [2], [Z], [PD], [PC], [none]
							)
						) AS ar
						LEFT JOIN XFC_MAIN_MASTER_Accounts a ON ar.account_name = a.name;
					"
					
				#End Region
					
				#Region "Raw Account BS To Conso"
					
				Case "XFC_MAIN_RAW_AccountBSToConso"
					selectQuery = "
						SELECT ar.account_name, a.description AS account_description, ar.conso_rubric
						FROM XFC_MAIN_MASTER_AccountRubrics ar
						LEFT JOIN XFC_MAIN_MASTER_Accounts a
						ON ar.account_name = a.name
						WHERE account_name NOT LIKE '6%' AND account_name NOT LIKE '7%';
					"
					
				#End Region
					
				#Region "Raw Accounts Old to New"
					
				Case "XFC_MAIN_RAW_AccountsOldToNew"
					selectQuery = "
						SELECT name_old, name
						FROM XFC_MAIN_MASTER_AccountsOldToNew;
					"
					
				#End Region
					
				#Region "Raw Cost Center Class to Rubric"
					
				Case "XFC_MAIN_RAW_CostCenterClassToRubric"
					selectQuery = "
						SELECT id, depreciation_account, asset_account
						FROM XFC_MAIN_MASTER_CostCenterClasses;
					"
					
				#End Region

				#Region "Master Flow Mappings"
					
				Case "XFC_MAIN_MASTER_FlowMappings"
					selectQuery = "
						SELECT sap_flow, flow
						FROM XFC_MAIN_MASTER_FlowMappings;
					"
					
				#End Region
					
				#Region "Master Cost Centers Old to New"
					
				Case "XFC_MAIN_MASTER_CostCentersOldToNew"
					selectQuery = "
						SELECT id, id_old
						FROM XFC_MAIN_MASTER_CostCentersOldToNew;
					"
					
				#End Region
					
				#Region "Master Cost Centers Old to OneStream Member"
					
				Case "XFC_MAIN_MASTER_CostCentersOldToOneStreamMember"
					selectQuery = "
						SELECT id_old, onestream_member_name
						FROM XFC_MAIN_MASTER_CostCentersOldToOneStreamMember;
					"
					
				#End Region
					
				#Region "Raw Customers to UD1 IC"
					
				Case "XFC_MAIN_RAW_CustomersToUD1IC"
					selectQuery = "
						SELECT id, customer_member_name, ic_member_name
						FROM XFC_MAIN_MASTER_BusinessPartners;
					"
					
				#End Region
					
				End Select
				dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
			End Using
			
			Return dt
			
		End Function
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
