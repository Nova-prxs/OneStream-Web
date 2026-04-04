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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class finance_custom_calculate
		Implements IWsasFinanceCustomCalculateV800
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass

        Public Sub CustomCalculate(ByVal si As SessionInfo, ByVal brGlobals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) Implements IWsasFinanceCustomCalculateV800.CustomCalculate
            Try
				Select args.CustomCalculateArgs.FunctionName
					
					#Region "Load Data To Cube"
					
					Case "LoadDataToCube"
						
						' Declare and clear data buffer
						Dim dataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula(
							$"FilterMembers(S#{api.Pov.Scenario.name}:O#Import,A#PL.Base)",, False)
						UTISharedFunctionsBR.ClearDataBuffer(si, api, dataBuffer)
						
						' Get data using globals
						Dim entityMonthDt As DataTable = Me.GetEntityMonthDataTable(
							api.Pov.Entity.Name, api.Pov.Time.Name, api.Pov.Scenario.Name, si, brGlobals
						)
						
						' If no data, exit sub
						If entityMonthDt Is Nothing OrElse entityMonthDt.Rows.Count = 0 Then Exit Sub
						' Instantiate data buffer and destination info
						Dim newDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{api.Pov.Scenario.name}:O#Import,A#PL.Base)")
						Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo(
							$"S#{api.Pov.Scenario.Name}:V#Periodic:O#Import"
						)
						
						' Set default member names
						Dim defaultFlow As String = "None"
						Dim defaultOrigin As String = "Import"
						Dim defaultUD2 As String = "None"
						Dim defaultUD4 As String = "None"
						Dim defaultUD5 As String = "None"
						Dim defaultUD6 As String = "None"
						Dim defaultUD7 As String = "None"
						Dim defaultUD8 As String = "None"
						
						' Populate data buffer
						For Each row In entityMonthDt.Rows					
								
							Dim newDataBufferCell As New DataBufferCell()
							' Set members, amount and status to real data
							newDataBufferCell.SetMembers(
								api, row("account"), defaultFlow, defaultOrigin, row("intercompany"), row("ud1"),
								defaultUD2, row("ud3"), defaultUD4, defaultUD5, defaultUD6, defaultUD7, defaultUD8
							)
							newDataBufferCell.CellAmount = row("amount")
							newDataBufferCell.CellStatus = New DataCellStatus(DataCellExistenceType.IsRealData, DataCellStorageType.Input)
							
							newDataBuffer.SetCell(si, newDataBufferCell, True)
							
						Next
						
						' Set Data Buffer
						api.Data.SetDataBuffer(newDataBuffer, destinationInfo,,,,,,,,,,,,, True)
						
					#End Region
					
					#Region "Clear Cube Data"
					
					Case "ClearCubeData"
						
						' Declare and clear data buffer
						Dim dataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula(
							$"FilterMembers(S#{api.Pov.Scenario.name}:O#Import,A#PL.Base)",, False)
						UTISharedFunctionsBR.ClearDataBuffer(si, api, dataBuffer)
					
					#End Region
					
					#Region "Copy Scenario"
					
					Case "CopyScenario"
						
						' Get parameters
						Dim paramScenarioSource As String = args.CustomCalculateArgs.NameValuePairs("p_scenario_source")
						Dim paramYearSource As String = args.CustomCalculateArgs.NameValuePairs("p_year_source")
						Dim month As Integer = CInt(api.Pov.Time.Name.Split("M")(1))
						
						' Declare and clear data buffer
						Dim dataBufferTarget As DataBuffer = api.Data.GetDataBufferUsingFormula(
							$"FilterMembers(S#{api.Pov.Scenario.name})",, False)
						UTISharedFunctionsBR.ClearDataBuffer(si, api, dataBufferTarget)
						
						' Get and convert data buffer in case of extensibility
						Dim dataBufferSource As DataBuffer = api.Data.GetDataBufferUsingFormula(
							$"FilterMembers(S#{paramScenarioSource}:T#{paramYearSource}M{month})",, False)
						Dim convertedDataBufferSource As DataBuffer = api.Data.ConvertDataBufferExtendedMembers(
							api.Pov.Cube.Name, paramScenarioSource, dataBufferSource)
							
						' Declare destination info and set data buffer
						Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
						
						api.Data.SetDataBuffer(convertedDataBufferSource, destinationInfo,,,,,,,,,,,,, True)
					
					#End Region
						
				End Select
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Sub

		#Region "Helper Functions"
		
		Public Function GetEntityMonthDataTable(entity As String, time As String, scenario As String, si As SessionInfo, ByRef brGlobals As BRGlobals) As DataTable
			' Lock globals to avoid race conditions
			SyncLock brGlobals.LockObjectForInitialization
				' If data has been processed, get the object if the entity has data, else return an empty dt
				If brGlobals.GetBoolValue("data_processed", False)
					Return If(brGlobals.GetObject($"{entity}_{time}_{scenario}"), Nothing)
				End If
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					' Declare db param infos
					Dim dbParamInfos As New List(Of DbParamInfo) From {
						New DbParamInfo("paramYear", Left(time, 4)),
						New DbParamInfo("paramScenario", scenario)
					}
					' Get data for selected year and scenario
					' Query depends on scenario
					Dim selectQuery As String = String.Empty
					
'					Select Left(scenario.ToLower, 3)
'					Case "fct"
'						selectQuery = "
'						SELECT
'							wof.entity, wof.year, wof.month,
'							wo.technology AS ud1, wof.ud3, wof.intercompany, wof.account,
'							SUM(wof.amount) AS amount
'						FROM (
'							SELECT 
'								entity, wo_id, year, month, ud3, intercompany,
'								CASE
'									WHEN account = 'Index' THEN 'A4200'
'									ELSE account
'								END AS account,
'								amount
'							FROM XFC_RES_FACT_work_order_figures
'							WHERE year = @paramYear
'								AND scenario = @paramScenario
'								-- AND amount <> 0
'						) wof
'						INNER JOIN XFC_RES_MASTER_work_order wo
'						ON wof.entity = wo.entity
'							AND wof.wo_id = wo.id
'						GROUP BY
'							wof.entity, wof.year, wof.month, wo.technology, wof.ud3,
'							wof.intercompany, wof.account
'						UNION ALL
'						SELECT
'							entity, year, month, ud1, ud3, intercompany, account,
'							SUM(amount) AS amount
'						FROM XFC_RES_FACT_structure_figures
'						WHERE year = @paramYear
'							AND scenario = @paramScenario
'							-- AND amount <> 0
'						GROUP BY entity, year, month, ud1, ud3, intercompany, account
'						ORDER BY entity, year, month;
'						"
'					Case "bud"
'						selectQuery = "
'						-- Generate a months cte to cross join planning scenario for structure
'						WITH months AS (
'							SELECT
'								1 AS month
'							UNION ALL
'							SELECT
'								month + 1
'							FROM months
'							WHERE month < 12
'						), monthly_structure_figures AS (
'							SELECT
'								entity, year, m.month, ud1, ud3, intercompany, account,
'								amount / 12 AS amount
'							FROM (
'								SELECT *
'								FROM XFC_RES_FACT_structure_figures
'								WHERE year = @paramYear
'									AND month = 12
'									AND scenario = @paramScenario
'									AND amount <> 0
'							) sf
'							CROSS JOIN months m
'						)
						
'						SELECT
'							wof.entity, wof.year, wof.month,
'							wo.technology AS ud1, wof.ud3, wof.intercompany, wof.account,
'							SUM(wof.amount) AS amount
'						FROM (
'							SELECT 
'								entity, wo_id, year, month, ud3, intercompany,
'								CASE
'									WHEN account = 'Index' THEN 'A4200'
'									ELSE account
'								END AS account,
'								amount
'							FROM XFC_RES_FACT_work_order_figures
'							WHERE year = @paramYear
'								AND scenario = @paramScenario
'								AND amount <> 0
'						) wof
'						INNER JOIN XFC_RES_MASTER_work_order wo
'						ON wof.entity = wo.entity
'							AND wof.wo_id = wo.id
'						GROUP BY
'							wof.entity, wof.year, wof.month, wo.technology, wof.ud3,
'							wof.intercompany, wof.account
'						UNION ALL
'						SELECT
'							entity, year, month, ud1, ud3, intercompany, account,
'							SUM(amount) AS amount
'						FROM monthly_structure_figures
'						GROUP BY entity, year, month, ud1, ud3, intercompany, account
'						ORDER BY entity, year, month;"
'					Case "pln"
'						selectQuery = "
'						-- Generate a months cte to cross join planning scenario for structure and project
'						WITH months AS (
'							SELECT
'								1 AS month
'							UNION ALL
'							SELECT
'								month + 1
'							FROM months
'							WHERE month < 12
'						), monthly_structure_figures AS (
'							SELECT
'								entity, year, m.month, ud1, ud3, intercompany, account,
'								amount / 12 AS amount
'							FROM (
'								SELECT *
'								FROM XFC_RES_FACT_structure_figures
'								WHERE year = @paramYear
'									AND month = 12
'									AND scenario = @paramScenario
'									AND amount <> 0
'							) sf
'							CROSS JOIN months m
'						), monthly_wo_figures AS (
'							SELECT 
'								entity, wo_id, year, m.month, ud3, intercompany, account,
'								amount / 12 AS amount
'							FROM (
'								SELECT 
'									entity, wo_id, year, month, ud3, intercompany,
'									CASE
'										WHEN account = 'Index' THEN 'A4200'
'										ELSE account
'									END AS account,
'									amount
'								FROM XFC_RES_FACT_work_order_figures
'								WHERE year = @paramYear
'									AND scenario = @paramScenario
'									AND amount <> 0
'							) wof
'							CROSS JOIN months m
'						)
						
'						SELECT
'							wof.entity, wof.year, wof.month,
'							wo.technology AS ud1, wof.ud3, wof.intercompany, wof.account,
'							SUM(wof.amount) AS amount
'						FROM monthly_wo_figures wof
'						INNER JOIN XFC_RES_MASTER_work_order wo
'						ON wof.entity = wo.entity
'							AND wof.wo_id = wo.id
'						GROUP BY
'							wof.entity, wof.year, wof.month, wo.technology, wof.ud3,
'							wof.intercompany, wof.account
'						UNION ALL
'						SELECT
'							entity, year, month, ud1, ud3, intercompany, account,
'							SUM(amount) AS amount
'						FROM monthly_structure_figures
'						GROUP BY entity, year, month, ud1, ud3, intercompany, account
'						ORDER BY entity, year, month;"
'					End Select

					Select Case Left(scenario.ToLower(), 4)
				
						Case "pln3"
					        selectQuery = "
					        WITH months AS (
					            SELECT 1 AS month
					            UNION ALL
					            SELECT month + 1
					            FROM months
					            WHERE month < 12
					        ),
					        monthly_structure_figures AS (
					            SELECT
					                entity, year, m.month, ud1, ud3, intercompany, account,
					                amount / 12 AS amount
					            FROM (
					                SELECT *
					                FROM XFC_RES_FACT_structure_figures
					                WHERE year = @paramYear
					                    AND month = 12
					                    AND scenario = @paramScenario
					                    AND amount <> 0
					            ) sf
					            CROSS JOIN months m
					        ),
					        monthly_wo_figures AS (
					            SELECT 
					                entity, wo_id, year, m.month, ud3, intercompany, account,
					                amount / 12 AS amount
					            FROM (
					                SELECT 
					                    entity, wo_id, year, month, ud3, intercompany,
					                    account,
					                    amount
					                FROM XFC_RES_FACT_work_order_figures
					                WHERE year = @paramYear
					                    AND scenario = @paramScenario
					                    AND amount <> 0
					                    AND account NOT LIKE 'OE_%'
					            ) wof
					            CROSS JOIN months m
					        )
					
					        SELECT
					            wof.entity, wof.year, wof.month,
					            wo.technology AS ud1, wof.ud3, wof.intercompany, wof.account,
					            SUM(wof.amount) AS amount
					        FROM monthly_wo_figures wof
					        INNER JOIN XFC_RES_MASTER_work_order wo
					            ON wof.entity = wo.entity AND wof.wo_id = wo.id
					        GROUP BY
					            wof.entity, wof.year, wof.month, wo.technology, wof.ud3,
					            wof.intercompany, wof.account
					        UNION ALL
					        SELECT
					            entity, year, month, ud1, ud3, intercompany, account,
					            SUM(amount) AS amount
					        FROM monthly_structure_figures
					        GROUP BY entity, year, month, ud1, ud3, intercompany, account
					        ORDER BY entity, year, month;
							"
					
						Case Else
						    selectQuery = "
						    WITH months AS (
						        SELECT 1 AS month
						        UNION ALL
						        SELECT month + 1 FROM months WHERE month < 12
						    ),
						    fact_data AS (
						        SELECT
						            wof.entity,
						            wof.year,
						            wof.month,
						            wo.technology AS ud1,
						            wof.ud3,
						            wof.intercompany,
						            wof.account,
						            wof.amount
						        FROM (
					                SELECT 
					                    entity, wo_id, year, month, ud3, intercompany,
					                    account,
					                    amount
					                FROM XFC_RES_FACT_work_order_figures
					                WHERE year = @paramYear AND scenario = @paramScenario 
					                    AND amount <> 0
					                    AND account NOT LIKE 'OE_%'
						        ) wof
						        INNER JOIN XFC_RES_MASTER_work_order wo
						            ON wof.entity = wo.entity AND wof.wo_id = wo.id
						
						        UNION ALL
						
						        SELECT
						            entity, year, month, ud1, ud3, intercompany, account, amount
						        FROM XFC_RES_FACT_structure_figures
						        WHERE year = @paramYear AND scenario = @paramScenario AND amount <> 0
						    ),
						    unique_fact_data AS (
						        SELECT DISTINCT entity, year, ud1, ud3, intercompany, account
						        FROM fact_data
						    ),
						    monthly_figures AS (
						        SELECT u.entity, u.year, m.month, u.ud1, u.ud3, u.intercompany, u.account
						        FROM unique_fact_data u
						        CROSS JOIN months m
						    ),
						    final_monthly_figures AS (
						        SELECT
						            mf.entity,
						            mf.year,
						            mf.month,
						            mf.ud1,
						            mf.ud3,
						            mf.intercompany,
						            mf.account,
						            COALESCE(SUM(fd.amount), 0) AS amount
						        FROM monthly_figures mf
						        LEFT JOIN fact_data fd ON
						            fd.entity = mf.entity AND fd.year = mf.year AND fd.month = mf.month AND
						            fd.ud1 = mf.ud1 AND fd.ud3 = mf.ud3 AND fd.intercompany = mf.intercompany AND fd.account = mf.account
						        GROUP BY mf.entity, mf.year, mf.month, mf.ud1, mf.ud3, mf.intercompany, mf.account
						    )
						
						    SELECT * FROM final_monthly_figures
						    ORDER BY entity, year, month;
							"
	
					End Select
					
					Dim dt As DataTable = BRApi.Database.ExecuteSql(
						dbConn,
						selectQuery,
						dbParamInfos,
						False
					)
					' If no data set data processed to true and return nothing
					If dt Is Nothing OrElse dt.Rows.Count = 0 Then
						brGlobals.SetBoolValue("data_processed", True)
						Return Nothing
					End If
					' Populate globals
					' Instantiate a new data table
					Dim innerDt As New DataTable()
					innerDt.Columns.Add("ud1")
					innerDt.Columns.Add("ud3")
					innerDt.Columns.Add("intercompany")
					innerDt.Columns.Add("account")
					innerDt.Columns.Add("amount")
					' Declare control variables
					Dim lastTime As String = String.Empty
					Dim lastEntity As String = String.Empty
					For Each row In dt.Rows
						' Get time and entity
						Dim innerTime As String = row("year") & "M" & row("month")
						Dim innerEntity As String = row("entity")
						' First iteration should skip this logic
						If Not String.IsNullOrEmpty(lastTime) Then
							' If innerTime or innerEntity changes, set the dt to a global variable and begin a new data table
							If lastTime <> innerTime OrElse lastEntity <> innerEntity Then
								brGlobals.SetObject($"{lastEntity}_{lastTime}_{scenario}", innerDt)
								innerDt = New DataTable
								innerDt.Columns.Add("ud1")
								innerDt.Columns.Add("ud3")
								innerDt.Columns.Add("intercompany")
								innerDt.Columns.Add("account")
								innerDt.Columns.Add("amount")
							End If
						End If
						' Initiate row
						Dim newRow As DataRow = innerDt.NewRow
						lastTime = innerTime
						lastEntity = innerEntity
						
						' Populate and add new row
						newRow("ud1") = row("ud1")
						newRow("ud3") = row("ud3")
						newRow("intercompany") = row("intercompany")
						newRow("account") = row("account")
						newRow("amount") = row("amount")
						innerDt.Rows.Add(newRow)
					Next
					brGlobals.SetObject($"{lastEntity}_{lastTime}_{scenario}", innerDt)

				End Using
			End SyncLock
			' Set data processed to true and return data table
			brGlobals.SetBoolValue("data_processed", True)
			Return If(brGlobals.GetObject($"{entity}_{time}_{scenario}"), Nothing)
		End Function
		
		#End Region
		
	End Class
End Namespace
