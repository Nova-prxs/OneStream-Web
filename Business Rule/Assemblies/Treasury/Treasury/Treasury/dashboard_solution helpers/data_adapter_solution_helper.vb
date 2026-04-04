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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.data_adapter_solution_helper
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						Dim names As New List(Of String)()
						names.Add("TestFunction")
						names.Add("CompanyNames")
						names.Add("DefaultWeekNumber")
						names.Add("WeekNumber")
						Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
						
						If args.DataSetName.XFEqualsIgnoreCase("TestFunction") Then
							
					#Region "Company Names"
					
				ElseIf args.DataSetName.XFEqualsIgnoreCase("CompanyNames") Then
					
					'Get available companies depending on security group
					Dim availableCompanies As List(Of Integer) = HelperFunctionsBR.GetCompanyIDsForUser(si)
					
					'Declare data table for results
					Dim companiesDt As New DataTable()
					companiesDt.Columns.Add("id")
					companiesDt.Columns.Add("name")
					
					'Query XFC_TRS_MASTER_Companies table
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Dim query As String
						
						If availableCompanies.Contains(0) Then
							'User has access to all companies - return all from table
							query = "
								SELECT id, name 
								FROM XFC_TRS_MASTER_Companies 
								ORDER BY 
									CASE WHEN id = 0 THEN 0 ELSE 1 END,
									name
							"
							
							Dim allCompaniesDt As DataTable = BRApi.Database.ExecuteSql(dbConn, query, False)
							
							For Each sourceRow As DataRow In allCompaniesDt.Rows
								Dim row As DataRow = companiesDt.NewRow()
								row("id") = Convert.ToInt32(sourceRow("id"))
								row("name") = sourceRow("name").ToString()
								companiesDt.Rows.Add(row)
							Next
						Else
							'User has restricted access - only show companies they have access to
							For Each companyId As Integer In availableCompanies
								query = $"
									SELECT id, name 
									FROM XFC_TRS_MASTER_Companies 
									WHERE id = {companyId}
								"
								
								Dim companyDt As DataTable = BRApi.Database.ExecuteSql(dbConn, query, False)
								
								If companyDt.Rows.Count > 0 Then
									Dim row As DataRow = companiesDt.NewRow()
									row("id") = Convert.ToInt32(companyDt.Rows(0)("id"))
									row("name") = companyDt.Rows(0)("name").ToString()
									companiesDt.Rows.Add(row)
								End If
							Next
						End If
					End Using
					
					Return companiesDt						
					
					#End Region
						
						#Region "Default Week Number"
						
						ElseIf args.DataSetName.XFEqualsIgnoreCase("DefaultWeekNumber") Then
							
							'Get current date
							Dim currentDate As Date = DateTime.Now.Date
							
						'Create DataTable for result
						Dim weekNumberDt As New DataTable()
						weekNumberDt.Columns.Add("weekNumber", GetType(String))
						
						'Query to get week number for current date
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							Dim selectQuery As String = "
								SELECT TOP 1 weekNumber
								FROM XFC_TRS_AUX_Date
								WHERE @currentDate >= weekStartDate 
									AND @currentDate <= weekEndDate
								ORDER BY weekStartDate
							"
							
							Dim dbParamInfos As New List(Of DbParamInfo) From {
								New DbParamInfo("currentDate", currentDate)
							}
							
							Dim resultDt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, dbParamInfos, False)
							
							If resultDt.Rows.Count > 0 Then
								'Week found in database
								Dim row As DataRow = weekNumberDt.NewRow()
								row("weekNumber") = Convert.ToInt32(resultDt.Rows(0)("weekNumber")).ToString()
								weekNumberDt.Rows.Add(row)
							Else
								'No week found, return error indicator
								Dim row As DataRow = weekNumberDt.NewRow()
								row("weekNumber") = "-1" ' Error indicator
								weekNumberDt.Rows.Add(row)
							End If
						End Using							
                        
							Return weekNumberDt
						
						#End Region
						
						#Region "Week Numbers"
						
						ElseIf args.DataSetName.XFEqualsIgnoreCase("WeekNumber") Then
							
							'Get year parameter (default to current year if not provided)
							Dim paramYear As Integer = DateTime.Now.Year
							If args.NameValuePairs.ContainsKey("p_year") AndAlso Not String.IsNullOrEmpty(args.NameValuePairs("p_year")) Then
								Integer.TryParse(args.NameValuePairs("p_year"), paramYear)
							End If
							
							'Create DataTable for result
							Dim allWeeksNumberDt As New DataTable()
							allWeeksNumberDt.Columns.Add("weekNumber", GetType(String))
							allWeeksNumberDt.Columns.Add("weekStartDate", GetType(Date))
							allWeeksNumberDt.Columns.Add("weekEndDate", GetType(Date))
							
							'Query to get all week numbers for specified year + Week 1 of next year (for cross-year support)
							'Week 1 of next year will be displayed as Week 53 for UI continuity
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim selectQuery As String = "
									SELECT DISTINCT
										CASE 
											WHEN year = @nextYear AND weekNumber = 1 THEN 53
											ELSE weekNumber
										END AS weekNumber,
										weekStartDate,
										weekEndDate,
										year
									FROM XFC_TRS_AUX_Date
									WHERE year = @paramYear
									   OR (year = @nextYear AND weekNumber = 1)
									ORDER BY year, weekNumber
								"
								
								Dim dbParamInfos As New List(Of DbParamInfo) From {
									New DbParamInfo("paramYear", paramYear),
									New DbParamInfo("nextYear", paramYear + 1)
								}
								
								Dim resultDt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, dbParamInfos, False)
								
								If resultDt.Rows.Count > 0 Then
									'Copy all weeks to result DataTable
									For Each sourceRow As DataRow In resultDt.Rows
										Dim row As DataRow = allWeeksNumberDt.NewRow()
										row("weekNumber") = Convert.ToInt32(sourceRow("weekNumber")).ToString()
										row("weekStartDate") = Convert.ToDateTime(sourceRow("weekStartDate"))
										row("weekEndDate") = Convert.ToDateTime(sourceRow("weekEndDate"))
										allWeeksNumberDt.Rows.Add(row)
									Next
								Else
								End If
							End Using
							
							Return allWeeksNumberDt
						
						#End Region
						
						End If				
                        
                        End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
