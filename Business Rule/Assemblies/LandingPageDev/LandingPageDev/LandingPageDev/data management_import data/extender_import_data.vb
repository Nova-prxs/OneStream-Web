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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_import_data
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Get parameters and create full path and declare column mapping dictionary
						Dim tableName As String = args.NameValuePairs("p_table")
						Dim sectionName As String = args.NameValuePairs("p_section")
						Dim filesType As String = args.NameValuePairs("p_files_type")
						Dim delimiter As String = String.Empty
						If filesType = "Delimited" Then delimiter = args.NameValuePairs("p_delimiter")
						Dim filesFolderName As String = args.NameValuePairs("p_folder")
						Dim method As String = args.NameValuePairs("p_method")
						Dim fullPath As String = $"Documents/Public/{sectionName}/{filesType} Files/{filesFolderName}"
						Dim columnMappingDict As New Dictionary(Of String, String)
						
						'Get column mapping dictionary
						columnMappingDict = Me.GetColumnMappingDict(si, tableName)
						
						'Create data table from files folder
						Dim dt As New DataTable 
						If filesType = "Excel" Then
							dt = UTISharedFunctionsBR.CreateDataTableFromExcelFilesFolder(si, fullPath, FileSystemLocation.ApplicationDatabase)
						Else If filesType = "Delimited" Then
							dt = UTISharedFunctionsBR.CreateDataTableFromDelimitedFilesFolder(si, fullPath, FileSystemLocation.ApplicationDatabase, delimiter)
						Else
							Throw ErrorHandler.LogWrite(si, New XFException("Files type must me 'Excel' or 'Delimited'"))
						End If
						
						'Map and filter columns in DataTable
						dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
						
						'Remove all files in folder
						UTISharedFunctionsBR.DeleteAllFilesInFolder(si, fullPath, FileSystemLocation.ApplicationDatabase)
						
						'Load data table to custom table
						UTISharedFunctionsBR.LoadDataTableToCustomTable(si, tableName, dt, method)
						
						'Run up population queries for dependent tables
						Me.RunPopulationQueries(si, tableName)
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Helper Functions"
		
		#Region "Get Column Mapping Dictionary"
		
		Private Function GetColumnMappingDict(ByVal si As SessionInfo, ByVal tableName As String) As Dictionary(Of String, String)
			'Declare new dictionary
			Dim columnMappingDict As Dictionary(Of String, String)
		
			'Control table column mapping dictionaries
			If tableName = "XFC_INV_RAW_project" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Function", "func"},
			        {"Label position 1", "label_position_1"},
			        {"Label position 2", "label_position_2"},
			        {"Label position 3", "label_position_3"},
			        {"Label position 4", "label_position_4"},
			        {"Label position 5", "label_position_5"},
			        {"Region", "region"},
			        {"Company name", "company_name"},
			        {"Company ID", "company_id"},
			        {"Country", "country"},
			        {"Branch", "branch"},
			        {"Site", "site"},
			        {"Professional area", "professional_area"},
			        {"POI / POE", "poi_poe"},
			        {"CC", "cc"},
			        {"CC name", "cc_name"},
			        {"CPI", "cpi"},
			        {"CPI name", "cpi_name"},
			        {"Project", "project"},
			        {"Project name", "project_name"},
			        {"Decision criteria", "decision_criteria"},
			        {"Aggregate", "aggregate"},
			        {"Libre", "libre"},
			        {"Delivered N-3 and before", "delivered_before"},
			        {"Delivered Year N-2", "delivered_year_py_2"},
			        {"Delivered Year N-1", "delivered_year_py_1"},
			        {"Visi Year N", "visibility_year_cy"},
			        {"Draft Visi Year N", "draft_visibility_year_cy"},
			        {"Requirement Year N", "requirement_year_cy"},
			        {"Decided Year N", "decided_year_cy"},
			        {"Ordered Year N", "ordered_year_cy"},
			        {"Delivered YTD Year N", "delivered_ytd_year_cy"},
			        {"Visi Year N+1", "visibility_year_ny_1"},
			        {"Draft Visi Year N+1", "draft_visibility_year_ny_1"},
			        {"Requirement Year N+1", "requirement_year_ny_1"},
			        {"Decided Year N+1", "decided_year_ny_1"},
			        {"Ordered Year N+1", "ordered_year_ny_1"},
			        {"Visi Year N+2", "visibility_year_ny_2"},
			        {"Draft Visi Year N+2", "draft_visibility_year_ny_2"},
			        {"Requirement Year N+2", "requirement_year_ny_2"},
			        {"Decided Year N+2", "decided_year_ny_2"},
			        {"Ordered Year N+2", "ordered_year_ny_2"},
			        {"Visi Year N+3", "visibility_year_ny_3"},
			        {"Draft Visi Year N+3", "draft_visibility_year_ny_3"},
			        {"Requirement Year N+3", "requirement_year_ny_3"},
			        {"Decided Year N+3", "decided_year_ny_3"},
			        {"Visi Year N+4", "visibility_year_ny_4"},
			        {"Draft Visi Year N+4", "draft_visibility_year_ny_4"},
			        {"Requirement Year N+4", "requirement_year_ny_4"},
			        {"Decided Year N+4", "decided_year_ny_4"},
			        {"Total Visi", "total_visibility"},
			        {"Total Draft Visi", "total_draft_visibility"},
			        {"Total Requirement", "total_requirement"},
			        {"Contract Commitment", "contract_commitment"},
			        {"Total Decided", "total_decided"},
			        {"Total Ordered", "total_ordered"},
			        {"Total Delivered", "total_delivered"},
			        {"Strategic axis name", "strategic_axis_name"},
			        {"Project status", "project_status"},
			        {"CPIL name", "cpil_name"},
			        {"Program Position", "program_position"},
			        {"DPCI Analyst", "dpci_analyst"},
			        {"Reason", "reason"},
			        {"Start date of project", "start_date"},
			        {"End date of project", "end_date"},
			        {"Cash before", "cash_before"},
			        {"Cash Year N-2", "cash_year_py_2"},
			        {"Cash Year N-1", "cash_year_py_1"},
			        {"Cash YTD Year N", "cash_ytd_year_cy"},
			        {"Total Cash", "total_cash"},
			        {"Reporting Budget Owner", "budget_owner"},
			        {"Date of data extraction", "date_of_data_extraction"},
			        {"Type", "type"},
					{"MA-DATE", "ma_date"}
				}
			Else If tableName = "XFC_INV_RAW_project_cash" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Project", "project_id"},
					{"Year", "year"},
					{"Month", "month"},
					{"Cash", "amount"}
				}
			Else If tableName = "XFC_INV_RAW_asset" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Objet", "id"},
					{"Ctre coûts", "cost_center_id"},
					{"Elt d'OTP", "project_id"},
					{"Désignation", "designation"},
					{"Catégor.", "category_id"},
					{"Val.d'acq. - CONSO 40", "initial_value"}
				}
			Else If tableName = "XFC_RND_RAW_project" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Année", "year"},
					{"Société", "company_name"},
					{"Grde Dir Responsable", "responsible_dir"},
					{"Métier", "job"},
					{"CeCos", "cc"},
					{"Centre de coûts", "cc_name"},
					{"Activité", "activity"},
					{"Segment d'activité", "business_segment"},
					{"Définition de projet", "project"},
					{"Nombre de projet", "project_name"},
					{"Date Pré-contrat", "pre_contract_date"},
					{"Date SOP", "ma_date"},
					{"Périmètre bénéfic", "ip_owner"},
					{"Société partenaire 1", "client"},
					{"Version", "version"},
					{"Type de données", "data_type"},
					{"Nature de coût TEI", "cost_nature"},
					{"K€ Jan.", "m1"},
					{"K€ Feb.", "m2"},
					{"K€ Mar.", "m3"},
					{"K€ Apr.", "m4"},
					{"K€ May.", "m5"},
					{"K€ Jun.", "m6"},
					{"K€ Jul.", "m7"},
					{"K€ Aug.", "m8"},
					{"K€ Sept.", "m9"},
					{"K€ Oct.", "m10"},
					{"K€ Nov.", "m11"},
					{"K€ Dec.", "m12"}
				}
			Else
				Throw ErrorHandler.LogWrite(si, New XFException($"There is no column mapping dictionary for table {tableName}"))
			End If
			
			Return columnMappingDict
				
		End Function
		
		#End Region
		
		#Region "Run Population Queries"
		
		Private Sub RunPopulationQueries(ByVal si As SessionInfo, ByVal tableName As String)
			'Declare list of objects to populate with tables
			Dim tableList As New List(Of Object)
			'Control table name
			If tableName = "XFC_INV_RAW_project" Then
				tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_project)
				tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_project_financial_figures)
			Else If tableName = "XFC_INV_RAW_project_cash" Then
				tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_project_cash)
			Else If tableName = "XFC_INV_RAW_asset" Then
				tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_asset)
			End If
			
			'Return if no tables
			If tableList.Count < 1 Then Return
			
			'Get migration queries
			Dim populationQueries As List(Of String) = Me.GetPopulationQueries(si, tableList)
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				'Populate tables
				For Each query In populationQueries
					BRApi.Database.ExecuteSql(dbConn, query, False)
				Next
			End Using
		End Sub
		
		Private Function GetPopulationQueries(ByVal si As SessionInfo, ByVal tables As List(Of Object)) As List(Of String)
			'Declare list of queries
			Dim queries As New List(Of String)
			
			'Loop through all the tables to get the queries
			For Each table In tables
				queries.Add(table.GetPopulationQuery(si, "up"))
			Next
			
            Return queries
		End Function
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
