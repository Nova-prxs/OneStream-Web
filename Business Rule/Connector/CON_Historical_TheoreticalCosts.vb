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

Namespace OneStream.BusinessRule.Connector.CON_Historical_TheoreticalCosts
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As Object
			Try
				'Get the query information
                'Dim connectionString As String = GetConnectionString(si, globals, api)
				
				'Get the Field name list or load the data    
                Select Case args.ActionType
                    Case Is = ConnectorActionTypes.GetFieldList
						
                        'Return Field Name List
						'In case we use OneStream tables
						Dim fieldList As New List(Of String)
						fieldList.Add("Scenario")
						fieldList.Add("Entity")
						fieldList.Add("Period")
						fieldList.Add("Channel")
						fieldList.Add("Theo_Costs_Curr")
						fieldList.Add("Waste")
						fieldList.Add("Personnel_Cost")						
						fieldList.Add("Inventory_Waste")						
						
						Return fieldList
						
						'In case we use SIC
'						-------------------------------------------------------------------------------------------------------------------------
'                        Dim fieldListSQL As String = GetFieldListSQL(si, globals, api)
'                        Return api.Parser.GetFieldNameListForSQLQuery(si, DbProviderType.OLEDB, connectionString, True, fieldListSQL, False)
'						-------------------------------------------------------------------------------------------------------------------------
                        
                    Case Is = ConnectorActionTypes.GetData
						
                        'Process Data
						
						'In case we use OneStream Tables
'						-------------------------------------------------------------------------------------------------------------------------

						'Get Workflow information
						Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, api.WorkflowUnitPk.TimeKey)
						Dim oTime As TimeMemberSubComponents = BRApi.Finance.Time.GetSubComponentsFromName(si, wfTime)
						Dim wfYear As Integer = oTime.year
						
						Dim selectQuery As String = $"
							WITH TotalCosts AS (
								SELECT
				                	'Actual' As Scenario
				                	,TC.cebe AS Entity 
				                	,YEAR(TC.date) * 100 + MONTH(TC.date) AS Period 
				                	,CH.desc_channel1 AS Channel 
				                	,ISNULL(TC.theo_cost_sales_units,0) AS Theo_Costs_Curr
				                	,0 AS Waste
				               		,0 AS Personnel_Cost
									,0 AS Inventory_Waste
				                FROM XFC_TheoreticalCosts TC
				                INNER JOIN XFC_ChannelHierarchy CH 
				                	ON TC.channel3 = CH.cod_channel3
								INNER JOIN XFC_CEBES CE 
									ON TC.cebe = CE.cebe
								WHERE YEAR(TC.date) = {wfYear}
									AND CH.desc_channel1 <> ''
						
								UNION ALL

								SELECT
									'Actual' As Scenario
				                	,TWPC.cebe AS Entity
									,YEAR(TWPC.date) * 100 + MONTH(TWPC.date) AS Period
									,'None' As Channel
									,0 AS Theo_Costs_Curr
				                	,ISNULL(TWPC.waste,0) AS Waste
				               		,ISNULL(TWPC.personnel_cost,0) AS Personnel_Cost
									,0 AS Inventory_Waste
								FROM XFC_TheoreticalWastePersonnelCosts TWPC
								WHERE YEAR(date) = {wfYear}

								UNION ALL

								SELECT 
				                	'Actual' As Scenario
				                	,W.cebe AS Entity
				                	,YEAR(W.date) * 100 + MONTH(W.date) AS Period 
				                	,'None' AS Channel 
				                	,0 AS Theo_Costs_Curr
				                	,0 AS Waste
				               		,0 AS Personnel_Cost
									,CASE WHEN (
												C.brand IN ('SDH', 'Genéricos Grupo')
												AND
												--C.sales_brand NOT IN ('Domino''s Pizza', 'VIPS', 'Ginos', 'Ginos Portugal', 'Fridays', 'Starbucks', 'Starbucks Portugal', 'Starbucks Bélgica', 'Starbucks Holanda', 'Starbucks Francia')
												C.sales_brand NOT IN ('Domino''s Pizza', 'VIPS', 'Ginos', 'Ginos Portugal', 'Fridays', 'Starbucks', 'Starbucks Portugal')
												)
												OR
												(
												C.brand NOT IN ('SDH', 'Genéricos Grupo')
												AND
												--C.brand NOT IN ('Domino''s Pizza', 'VIPS', 'Ginos', 'Ginos Portugal', 'Fridays', 'Starbucks', 'Starbucks Portugal', 'Starbucks Bélgica', 'Starbucks Holanda', 'Starbucks Francia')
												C.brand NOT IN ('Domino''s Pizza', 'VIPS', 'Ginos', 'Ginos Portugal', 'Fridays', 'Starbucks', 'Starbucks Portugal')
												) THEN ISNULL(-W.amount,0)
									ELSE ISNULL(W.amount,0) END
									AS Inventory_Waste
				                FROM XFC_Waste W
								INNER JOIN XFC_CEBES C
								ON W.cebe = C.cebe
								WHERE YEAR(date) = {wfYear} AND W.cebe <> ''
							)

							SELECT 
			                	'Actual' As Scenario
			                	,Entity
			                	,Period 
			                	,Channel 
			                	,SUM(Theo_Costs_Curr) AS Theo_Costs_Curr
			                	,SUM(Waste) AS Waste
			               		,SUM(Personnel_Cost) AS Personnel_Cost
			               		,SUM(Inventory_Waste) AS Inventory_Waste
			                FROM TotalCosts
							GROUP BY
								Entity
								,Period
								,Channel;"
						
						Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						
							Dim dt As DataTable = BRApi.Database.ExecuteSql(dbcon, selectQuery, False)

							api.Parser.ProcessDataTable(si, dt, False, api.ProcessInfo)
						
						End Using
'						-------------------------------------------------------------------------------------------------------------------------
						
						'In case we use SIC
'						-------------------------------------------------------------------------------------------------------------------------
'                        Dim sourceDataSQL As String = GetSourceDataSQL(si, globals, api)
'                        api.Parser.ProcessSQLQuery(si, DbProviderType.OLEDB, connectionString, True, sourceDataSQL, False, api.ProcessInfo)
'						-------------------------------------------------------------------------------------------------------------------------
						
                        Return Nothing
 
                    Case Is = ConnectorActionTypes.GetDrillBackTypes
						
                        'Return the list of Drill Types (Options) to present to the end user
                        Return Nothing 'Me.GetDrillBackTypeList(si, globals, api, args)
 
                    Case Is = ConnectorActionTypes.GetDrillBack
                        'Process the specific Drill-Back type
                        Return Nothing 'Me.GetDrillBack(si, globals, api, args, args.DrillBackType.DisplayType, connectionString)
				
                End Select
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
		'Create a Connection string to the External Database
        Private Function GetConnectionString(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer) As String
		
            Try
                                
                'Connection String Method
                '-----------------------------------------------------------
'                Dim connection As New Text.StringBuilder'                
'                connection.Append("Provider=SQLOLEDB.1;")
'                connection.Append("Data Source=LocalHost\MSSQLSERVER2008;")
'                connection.Append("Initial Catalog=SampleData;")
'                connection.Append("Integrated Security=SSPI")                
'                Return connection.ToString
                
                'Named External Connection
                '-----------------------------------------------------------
                Return "Revenue Mgmt System"
                
                Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try            
        End Function    
 
        'Create the field list SQL Statement
        Private Function GetFieldListSQL(ByVal si As SessionInfo, ByVal globals As BRGlobals, 
		ByVal api As Transformer) As String
            Try
				
                'Create the SQL Statement
                Dim sql As New Text.StringBuilder
                
                sql.Append("SELECT Top(1)")
                
                Return sql.ToString
                
            Catch ex As Exception
				
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
            End Try    
			
        End Function
 
        'Create the data load SQL Statement
        Private Function GetSourceDataSQL(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer) As String
		
            Try
                'Create the SQL Statement
                Dim statement As New Text.StringBuilder
                Dim selectClause As New Text.StringBuilder
                Dim fromClause As New Text.StringBuilder
                Dim whereClause As New Text.StringBuilder
                Dim orderByClause As New Text.StringBuilder
                                
                selectClause.Append("SELECT ")
                
                fromClause.Append("FROM ")
                
                whereClause.Append("WHERE ")
 
                orderByClause.Append("ORDER BY ")               
                
                'Create the full SQL Statement
                statement.Append(selectClause.ToString)
                statement.Append(fromClause.ToString)
                statement.Append(whereClause.ToString)
                statement.Append(orderByClause.ToString)
                
                Return statement.ToString
                
            Catch ex As Exception
				
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
            End Try            
        End Function
 
        'Create the drill back options list
        Private Function GetDrillBackTypeList(ByVal si As SessionInfo,
 		ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As List(Of DrillBackTypeInfo)
		
            Try
                'Create the SQL Statement
                Dim drillTypes As New List(Of DrillBackTypeInfo)
                
                drillTypes.Add(New DrillBackTypeInfo(ConnectorDrillBackDisplayTypes.FileShareFile,
 				New NameAndDesc("","")))
                drillTypes.Add(New DrillBackTypeInfo(ConnectorDrillBackDisplayTypes.DataGrid,
 				New  NameAndDesc("","")))
                        
                Return drillTypes
                
            Catch ex As Exception
					
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
            End Try            
        End Function
        
        'Execute specific drill back type
        Private Function GetDrillBack(ByVal si As SessionInfo, ByVal globals As BRGlobals,
		ByVal api As Transformer,
		ByVal args As ConnectorArgs, ByVal drillBackType As ConnectorDrillBackDisplayTypes, 
		ByVal connectionString As String) As DrillBackResultInfo
            Try
                Select Case drillBackType 
                    Case Is = ConnectorDrillBackDisplayTypes.FileShareFile
						
                        'Show FileShare File
                        Dim drillBackInfo As New DrillBackResultInfo
                        drillBackInfo.DisplayType = ConnectorDrillBackDisplayTypes.FileShareFile
                        drillBackInfo.DocumentPath = Me.GetDrillBackDocPath(si, globals, api, args)      
						
                        Return drillBackInfo
 
                    Case Is = ConnectorDrillBackDisplayTypes.DataGrid
						
                        'Return Drill Back Detail
                        Dim drillBackSQL As String = GetDrillBackSQL(si, globals, api, args)
                        Dim drillBackInfo As New DrillBackResultInfo
                        drillBackInfo.DisplayType = ConnectorDrillBackDisplayTypes.DataGrid                                
                        drillBackInfo.DataTable = api.Parser.GetXFDataTableForSQLQuery(si,  
                        DbProviderType.OLEDB, connectionString, True, drillBackSQL,
 						False, args.PageSize, args.PageNumber)
						
                        Return drillBackInfo
                                            
                        Case Else
                        Return Nothing    
                End Select    
                        
            Catch ex As Exception
					
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
            End Try
			
        End Function
 
        'Create the drill back Document Path
        Private Function GetDrillBackDocPath(ByVal si As SessionInfo, 
		ByVal globals As BRGlobals,ByVal api As Transformer, ByVal args As ConnectorArgs) As String
		
            Try
                'Get the values for the source row that we are drilling back to
                Dim sourceValues As Dictionary(Of String, Object) =  
				api.Parser.GetFieldValuesForSourceDataRow(si, args.RowID)
                If (Not sourceValues Is Nothing) And (sourceValues.Count > 0) Then
					
                    Return "Applications/GolfStream_v24/DataManagement/RevenueMgmtInvoices/" &
					sourceValues.Item(StageConstants.MasterDimensionNames.Attribute1).ToString & ".pdf"
					
                Else
					
                    Return String.Empty
					
                End If
				
            Catch ex As Exception
				
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
            End Try
			
        End Function
 
    	'Create the drill back SQL Statement
        Private Function GetDrillBackSQL(ByVal si As SessionInfo, ByVal globals As BRGlobals,
 		ByVal api As Transformer, ByVal args As ConnectorArgs) As String
		
            Try
				
                'Get the values for the source row that we are drilling back to
                Dim sourceValues As Dictionary(Of String, Object) =  
				api.Parser.GetFieldValuesForSourceDataRow(si, args.RowID)
				
                If (Not sourceValues Is Nothing) And (sourceValues.Count > 0) Then                
 
                    Dim statement As New Text.StringBuilder
                    Dim selectClause As New Text.StringBuilder
                    Dim fromClause As New Text.StringBuilder
                    Dim whereClause As New Text.StringBuilder
                    Dim orderByClause As New Text.StringBuilder
 
                    'Create the SQL Statement                    
                    selectClause.Append("SELECT ") 
                    
                    fromClause.Append("FROM ")
                    
                    whereClause.Append("WHERE ")
                    
                    orderByClause.Append("ORDER BY ")
                    
                    'Create the full SQL Statement
                    statement.Append(selectClause.ToString)
                    statement.Append(fromClause.ToString)
                    statement.Append(whereClause.ToString)
                    statement.Append(orderByClause.ToString)
					
                    Return statement.ToString
					
                Else
					
                    Return String.Empty
					
                End If
            Catch ex As Exception
				
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
            End Try   
			
        End Function
		
	End Class
End Namespace