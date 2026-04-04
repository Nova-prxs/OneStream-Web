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
			If tableName = "XFC_RND_RAW_project" Then
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
			If tableName = "XFC_RND_RAW_project" Then
				tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_project)
				tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_project_company)
				tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_project_expense)
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
