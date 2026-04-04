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

' {Workspace.Current.PLTHR.queries}{}{}
Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.queries
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						'Dim names As New List(Of String)()
						'names.Add("MyDataSet")
						'Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
						
					#Region "Hours Hierarchy"		
					
						If args.DataSetName.XFEqualsIgnoreCase("Hours_Hierarchy") Then
							
							Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							Dim sql As String = String.Empty							
							
							' VARIABLES													
							Dim factory 	As String = args.NameValuePairs.GetValueOrEmpty("factory")
							Dim scenario 	As String = args.NameValuePairs.GetValueOrEmpty("scenario")
							Dim year 		As String = args.NameValuePairs.GetValueOrEmpty("year")
							
							Dim sql_DailyHours As String =  shared_queries.AllQueries(si, "DailyHours",factory,, year, scenario, "", "", "",,,"")
						  
							sql = sql_DailyHours & $"
						
								SELECT  
									
									'M' + RIGHT('0' + CAST(R.Month AS VARCHAR), 2) AS Month
									, CASE WHEN R.wf_type = 1 THEN 'MOD' ELSE 'MOS' END AS [WF Type]
									, RIGHT('0' + CAST(R.orden AS VARCHAR), 2) + ' ' + R.description2 AS Indicator
									, Value
							
								FROM (		
											SELECT DISTINCT id_factory, month, wf_type, id, description1, description2, orden
											FROM Report
								) R
								
								LEFT JOIN (	
											SELECT Id_Factory, wf_type, Month, Indicator, SUM(Value) AS Value 
											FROM AllHours 
											GROUP BY Id_Factory, Month, Indicator, wf_type
								) AS A
									ON R.id_factory = A.id_factory
									AND R.Month = A.Month
									AND R.wf_type = A.wf_type
									AND R.id = A.indicator
							
								INNER JOIN XFC_PLT_MASTER_Factory F
									ON R.id_factory = F.id
							
								ORDER BY R.Id_Factory, R.Month, R.wf_type, R.orden
							"
							
'							BRApi.ErrorLog.LogMessage(si,sql)
							
							Dim dt As New DataTable	
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt							
							
					#End Region			
							
                    #Region "Hours CC"							

						Else If args.DataSetName.XFEqualsIgnoreCase("Hours_CC") Then
							
							Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							Dim sql As String = String.Empty							
							
							' VARIABLES													
							Dim factory 	As String = args.NameValuePairs.GetValueOrEmpty("factory")
							Dim scenario 	As String = args.NameValuePairs.GetValueOrEmpty("scenario")
							Dim year 		As String = args.NameValuePairs.GetValueOrEmpty("year")			
							Dim indicator	As String = args.NameValuePairs.GetValueOrEmpty("indicator")	
													  
						  	Dim sql_DailyHours As String =  shared_queries.AllQueries(si, "DailyHours",factory,, year, scenario, "", "", "",,,"")
						  
							sql = sql_DailyHours & $"
						
								SELECT  
									'M' + RIGHT('0' + CAST(R.Month AS VARCHAR), 2) AS Month
									, CASE WHEN R.wf_type = 1 THEN 'MOD' ELSE 'MOS' END AS [WF Type]
									, R.id_costcenter AS [Id Cost Center]
									, R.type_costcenter AS [Type Cost Center]
									, R.description1 AS Indicator
									, ISNULL(Value,0) AS Value 
							
								FROM (		
											SELECT DISTINCT id_factory, month, wf_type, id_costcenter, type_costcenter, id, description1, description2, orden 
											FROM Report
											WHERE orden <> 18 -- (-) Extra Hours																		
								) R
								
								LEFT JOIN (	
											SELECT Id_Factory, wf_type, Month, Indicator, id_costcenter, type_costcenter, SUM(ISNULL(Value,0)) AS Value 
											FROM AllHours											
											GROUP BY Id_Factory, Month, Indicator, wf_type, id_costcenter, type_costcenter
								) AS A
									ON R.id_factory = A.id_factory
									AND R.Month = A.Month
									AND R.wf_type = A.wf_type
									AND R.id = A.indicator
									AND R.id_costcenter = A.id_costcenter
									AND R.type_costcenter = A.type_costcenter															
							
								INNER JOIN XFC_PLT_MASTER_Factory F
									ON R.id_factory = F.id
							
								WHERE NULLIF(value,0) <> 0							
							
								ORDER BY R.Id_Factory, R.Month, R.wf_type, R.id_costcenter, R.orden
							"
							
'							BRApi.ErrorLog.LogMessage(si,sql)

							Dim dt As New DataTable	
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt	
							
					#End Region	
					
					#Region "FTE"							

						Else If args.DataSetName.XFEqualsIgnoreCase("FTE") Then
							
							Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							Dim sql As String = String.Empty							
							
							' VARIABLES													
							Dim factory 	As String = args.NameValuePairs.GetValueOrEmpty("factory")
							Dim scenario 	As String = args.NameValuePairs.GetValueOrEmpty("scenario")
							Dim year 		As String = args.NameValuePairs.GetValueOrEmpty("year")				
													  
						  	Dim sql_FTE As String =  shared_queries.AllQueries(si, "FTE",factory,, year, scenario, "", "", "",,,"")
						  
							sql = $"
								{sql_FTE}
							
								SELECT
										description AS Factory
										, month AS Month
										, CASE WHEN wf_type = 1 THEN 'MOD' ELSE 'MOS' END AS [WF Type]
										, id_costcenter AS [Id Cost Center]
										, description1 AS [Hours Indicator]
										, shift AS Shift
										, type AS [Type Cost Center]		
										, hours AS [Hours]									
										, worked_days AS [Worked Days]
										, hours_shift AS [Hours by Shift]
										, FTE
								
								FROM FTE_data
							"
															
'							BRApi.ErrorLog.LogMessage(si,sql)

							Dim dt As New DataTable	
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt	
							
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
