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

Namespace OneStream.BusinessRule.Extender.RES_Services_LoadWOFactFromStage
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						#Region "Initialize Workflow and Context Variables"
						
		                Dim wfPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
		                Dim wfProfileGuid As Guid = wfPk.ProfileKey
		                Dim entityInfos As List(Of WorkflowProfileEntityInfo) = BRApi.Workflow.Metadata.GetProfileEntities(si, wfProfileGuid)
		                Dim entityNames As List(Of String) = entityInfos.Select(Function(e) e.EntityName).ToList()
		                Dim entityFilter As String = String.Join(",", entityNames.Select(Function(e) $"'{e}'"))
		
		                Dim scenarioKey As Integer = wfPk.ScenarioKey
		                Dim timeKey As Integer = wfPk.TimeKey
		                Dim scenario As String = BRApi.Finance.Members.GetMemberName(si, DimType.Scenario.Id, scenarioKey)
		                Dim time As String = BRApi.Finance.Members.GetMemberName(si, DimType.Time.Id, timeKey)
		
		                Dim currentYear As String = time.Substring(0, 4)
		                Dim currentMonth As Integer = Integer.Parse(time.Substring(5))
						Dim prevMonth As Integer = currentMonth - 1
						
						' Generate list of dbparaminfos
						Dim dbParamInfos As New List(Of DbParamInfo) From {
							New DbParamInfo("paramScenario", scenario),
							New DbParamInfo("paramTime", time),
							New DbParamInfo("paramYear", currentYear),
							New DbParamInfo("paramMonth", currentMonth),
							New DbParamInfo("paramPrevMonth", prevMonth)
						}
						
						#End Region
						
						#Region "Pre-validation"
						
						#Region "Pre-validation: Each Id must be associated with only one Technology"					
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						    Dim validationSql As String = $"
						    WITH base_data AS (
						        SELECT
						            UPPER(LTRIM(RTRIM(tgt.EtT))) AS Entity,
						            UPPER(
						                LTRIM(RTRIM(
						                    COALESCE(NULLIF(attr.A2, ''), CONCAT('NoWorkOrder-', tgt.U1T))
						                ))
						            ) AS Id,
						            UPPER(LTRIM(RTRIM(tgt.U1T))) AS Technology
						        -- FROM StageSourceData src
						        -- JOIN StageTargetData tgt ON src.Ri = tgt.Ri
						        -- LEFT JOIN StageAttributeData attr ON src.Ri = attr.Ri
								FROM StageTargetData tgt
								LEFT JOIN StageAttributeData attr ON tgt.Ri = attr.Ri
						        WHERE
						            tgt.EtT IN ({entityFilter})
						            AND tgt.TmT = @paramTime
						            AND tgt.SnT = @paramScenario
						            AND (tgt.AcT LIKE 'A4%' OR tgt.AcT LIKE 'A5%')
						    ),
						    conflicts AS (
						        SELECT
						            Entity,
						            Id
						        FROM base_data
						        WHERE Id IS NOT NULL
						        GROUP BY Entity, Id
						        HAVING COUNT(DISTINCT Technology) > 1
						    ),
						    error_rows AS (
						        SELECT
						            bd.Entity,
						            bd.Id,
						            bd.Technology,
						            ROW_NUMBER() OVER (
						                PARTITION BY bd.Entity, bd.Id, bd.Technology
						                ORDER BY bd.Id
						            ) AS rn
						        FROM base_data bd
						        JOIN conflicts c ON bd.Entity = c.Entity AND bd.Id = c.Id
						    )
						    SELECT
						        @paramTime AS [Time],
						        Entity,
						        Id,
						        Technology,
						        'Each Id must be associated with only one Technology.' AS [ValidationMessage],
						        1 AS SortOrder
						    FROM error_rows
						    WHERE rn = 1
						    ORDER BY Entity, Id, Technology;
						    "
							
							Dim validationResult As DataTable = BRApi.Database.ExecuteSql(dbConn, validationSql, dbParamInfos, True)
							
							If validationResult IsNot Nothing AndAlso validationResult.Rows.Count > 0 Then
							    Dim msg As String = "Validation error: Each Id must be associated with only one Technology." & vbCrLf &
							                        "For more details: Go to Validate Step > Dashboards Section > Open Check WO & Technology." & vbCrLf
							
							    ' --- calculate unique WO in conflict ---
							    Dim allIds As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
							    For Each row As DataRow In validationResult.Rows
							        allIds.Add(row("Id").ToString())
							    Next
							    Dim totalUniqueIds As Integer = allIds.Count
							
							    ' --- display up to 10 unique WO_IDs ---
							    Dim shownIds As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
							    Dim maxUniqueIds As Integer = 10
							
							    For Each row As DataRow In validationResult.Rows
							        Dim woId As String = row("Id").ToString()
							
							        ' Add the row if we haven't exceeded 10 unique WO_IDs,
							        ' or if this WO_ID is already being shown (to include all its technologies).
							        If shownIds.Count < maxUniqueIds OrElse shownIds.Contains(woId) Then
							            msg &= $"Entity: {row("Entity")}, Id: {row("Id")}, Technology: {row("Technology")}" & vbCrLf
							            shownIds.Add(woId)
							        Else
							            Exit For
							        End If
							    Next
							
							    ' --- calculate WO_IDs not shown ---
							    Dim remainingIds As Integer = totalUniqueIds - shownIds.Count
							    If remainingIds > 0 Then
							        msg &= $"... and {remainingIds} other work orders ..." & vbCrLf
							    End If
							
							    Throw New XFException(si, New Exception(msg))
							End If
						End Using

						#End Region
						
						#Region "Pre-validation: Project ID must be <= 30 chars"
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						    Dim validationSqlProjectLen As String = $"
						    WITH base_data AS (
						        SELECT DISTINCT
						            tgt.EtT AS Entity,
						            LTRIM(RTRIM(CAST(attr.A1 AS NVARCHAR(255)))) AS project_id_norm
						        FROM StageSourceData src
						        JOIN StageTargetData  tgt  ON src.Ri = tgt.Ri
						        LEFT JOIN StageAttributeData attr ON src.Ri = attr.Ri
						        WHERE
						            tgt.EtT IN ({entityFilter})
						            AND tgt.TmT = @paramTime
						            AND tgt.SnT = @paramScenario
						            AND (tgt.AcT LIKE 'A4%' OR tgt.AcT LIKE 'A5%')
						            AND NULLIF(LTRIM(RTRIM(CAST(attr.A1 AS NVARCHAR(255)))),'') IS NOT NULL
						    ),
						    bad AS (
						        SELECT
						            Entity,
						            project_id_norm AS ProjectId
						        FROM base_data
						        WHERE LEN(project_id_norm) > 30
						        GROUP BY Entity, project_id_norm
						    ),
						    bad_by_entity AS (
						        SELECT
						            Entity,
						            MIN(ProjectId) AS SampleProjectId,
						            COUNT(*)       AS OffendingCount
						        FROM bad
						        GROUP BY Entity
						    )
						    SELECT
						        @paramTime AS [Time],
						        b.Entity,
						        b.SampleProjectId,
						        b.OffendingCount
						    FROM bad_by_entity b
						    ORDER BY b.Entity;"
						
						    Dim badEntities As DataTable = BRApi.Database.ExecuteSql(dbConn, validationSqlProjectLen, dbParamInfos, True)
						
						    If badEntities IsNot Nothing AndAlso badEntities.Rows.Count > 0 Then
						        Dim msg As String = "Validation error: Invalid Project IDs found." & vbCrLf & _
						                            "Please correct the source data and retry." & vbCrLf & _
													"For more details: Validate Step > Dashboards > Project ID Checks." & vbCrLf & _
						                            "Entities with issues:" & vbCrLf
						
						        For Each row As DataRow In badEntities.Rows
						            msg &= $" - Entity: {row("Entity")}, Sample Project ID: '{row("SampleProjectId")}', Offending IDs: {row("OffendingCount")}" & vbCrLf
						        Next
						
						        Throw New XFException(si, New Exception(msg))
						    End If
						End Using
						
						#End Region
						
						#End Region
						
						#Region "Clean, Calculate and Insert Periodic"
						
		                Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							BRApi.ErrorLog.LogMessage(si, entityFilter)
		
		                    Dim insertPeriodicSql As String = $"
								-- Clear data for selected entities, time and scenario
		                        DELETE FROM XFC_RES_FACT_work_order_figures
								WHERE entity IN ({entityFilter}) AND year = @paramYear AND month = @paramMonth AND scenario = @paramScenario;
							
		                        DELETE FROM XFC_RES_FACT_structure_figures
								WHERE entity IN ({entityFilter}) AND year = @paramYear AND month = @paramMonth AND scenario = @paramScenario;
								
								-- Populate staging table
								SELECT
									tgt.type						AS type,
							        tgt.EtT                        	AS entity,
									CASE
										WHEN attr.A2 <> '' THEN attr.A2
										ELSE CONCAT('NoWorkOrder-', tgt.U1T)
									END 						   	AS wo_id,
							        tgt.SnT                        	AS scenario,
							        @paramYear                      AS year,
							        @paramMonth                     AS month,
							        tgt.AcT             			AS account,
							        tgt.IcT                        	AS intercompany,
									tgt.U1T							AS ud1,
							        tgt.U3T                        	AS ud3,	
									attr.A1							AS project_id,
									attr.A4							AS rev_type,
									attr.A6							AS p_description,
									attr.A7							AS w_description,
							        -- SUM(src.Am)                    	AS amount
									SUM(CASE WHEN tgt.Fs = 1 THEN -src.Am ELSE src.Am END) AS amount
								INTO #staging_table
							    FROM (
							        SELECT *,
										CASE
											WHEN AcT LIKE 'A4%' OR AcT LIKE 'A5%' THEN 'work_order'
											ELSE 'structure'
										END AS type
							        FROM StageTargetData
							        WHERE
							            EtT IN ({entityFilter}) AND TmT = @paramTime AND SnT = @paramScenario AND
							            (
							                AcT LIKE 'A4%' OR AcT LIKE 'A5%' OR AcT LIKE 'A6%' OR AcT LIKE 'A7%'
											 OR AcT LIKE 'A8%' OR AcT LIKE 'A9%'
							            ) AND AcT NOT LIKE '%(bypass)%' AND IcT NOT LIKE '%(bypass)%'
							            AND U1T NOT LIKE '%(bypass)%' AND U3T NOT LIKE '%(bypass)%'
							            AND FwT NOT LIKE '%(bypass)%'
							    )   tgt
							    JOIN StageSourceData   src  ON src.Ri = tgt.Ri
							    LEFT JOIN StageAttributeData attr ON src.Ri = attr.Ri
							    GROUP BY
							        tgt.type, tgt.EtT, attr.A2, tgt.SnT,
									tgt.AcT, tgt.IcT,
									tgt.U1T, tgt.U3T,
									attr.A1, attr.A4, attr.A6, attr.A7;
							
								/* WORK ORDER TABLE */
							
								-- Calculate periodic
								WITH periodic_table AS (
								    SELECT
								        COALESCE(st.entity       , pf.entity)        AS entity,
								        COALESCE(st.wo_id        , pf.wo_id)         AS wo_id,
								        COALESCE(st.scenario     , pf.scenario)      AS scenario,
								        COALESCE(st.year         , pf.year)          AS year,
										@paramMonth                     			 AS month,
								        COALESCE(st.account      , pf.account)       AS account,
								        COALESCE(st.intercompany , pf.intercompany)  AS intercompany,
								        COALESCE(st.ud3          , pf.ud3)           AS ud3,
								        COALESCE(st.amount, 0) - COALESCE(pf.amount_ytd, 0) AS amount
								    FROM (
										SELECT
											entity,
								        	wo_id,
								        	scenario,
								        	year,
											month,
								        	account,
								        	intercompany,
								        	ud3,
								        	SUM(amount) AS amount
										FROM #staging_table 
										WHERE type = 'work_order'
										GROUP BY
											entity,
								        	wo_id,
								        	scenario,
								        	year,
											month,
								        	account,
								        	intercompany,
								        	ud3
									) st
								    FULL OUTER JOIN (
								        /* snapshot YTD del mes anterior */
								        SELECT
								            entity,
								            wo_id,
								            scenario,
								            year,
								            month,
								            account,
								            intercompany,
								           	ud3,
								            amount_ytd
								        FROM XFC_RES_VIEW_work_order_figures_ytd
								        WHERE year = @paramYear
											AND month = @paramPrevMonth
								          	AND scenario = @paramScenario
											AND entity IN ({entityFilter})
								    ) pf ON  st.entity       = pf.entity
								        AND st.wo_id        = pf.wo_id
								        AND st.scenario     = pf.scenario
								        AND st.account      = pf.account
								        AND st.intercompany = pf.intercompany
								        AND st.ud3          = pf.ud3
								)
								
								-- Insert data into fact table
								INSERT INTO XFC_RES_FACT_work_order_figures (
								    entity, wo_id, scenario, year, month, account, intercompany, ud3, amount
								)
								SELECT
								    entity, wo_id, scenario, year, month, account, intercompany, ud3, SUM(amount) AS amount
								FROM periodic_table
								GROUP BY entity, wo_id, scenario, year, month, account, intercompany, ud3;

								-- Insertar o actualizar la tabla maestra de work orders
								MERGE INTO XFC_RES_MASTER_work_order AS target
								USING (
									SELECT
										entity,
										wo_id,
										technology,
										project_id,
										rev_type,
										p_description,
										w_description
									FROM (
										SELECT
											st.entity,
											st.wo_id,
											st.ud1 AS technology,
											st.project_id,
											st.rev_type,
											st.p_description,
											st.w_description,
											ROW_NUMBER() OVER (
												PARTITION BY st.entity, st.wo_id
												ORDER BY st.ud1
											) AS rn
										FROM #staging_table st
										WHERE st.type = 'work_order'
									) ranked
									WHERE rn = 1
								) AS source
								ON target.entity = source.entity AND target.id = source.wo_id
								WHEN MATCHED THEN
									UPDATE SET
										technology = source.technology
								WHEN NOT MATCHED THEN
									INSERT (
										entity, id, description,
										project_id, project_description, technology,
										rev_type, client
									)
									VALUES (
										source.entity,
										source.wo_id,
										COALESCE(NULLIF(LTRIM(RTRIM(source.w_description)), ''), 'No Work Order Description'),
										source.project_id,
										COALESCE(NULLIF(LTRIM(RTRIM(source.p_description)), ''), 'No Project Description'),
										source.technology,
										COALESCE(NULLIF(LTRIM(RTRIM(source.rev_type)), ''), 'FIX'),
										'None'
									);

							
								/* STRUCTURE TABLE */
							
								-- Calculate periodic data
								WITH periodic_table AS (
								    SELECT
								        COALESCE(st.entity       , pf.entity)        AS entity,
								        COALESCE(st.ud1        , pf.ud1)         	 AS ud1,
								        COALESCE(st.scenario     , pf.scenario)      AS scenario,
								        COALESCE(st.year         , pf.year)          AS year,
										@paramMonth                     			 AS month,
								        COALESCE(st.account      , pf.account)       AS account,
								        COALESCE(st.intercompany , pf.intercompany)  AS intercompany,
								        COALESCE(st.ud3          , pf.ud3)           AS ud3,
								        COALESCE(st.amount, 0) - COALESCE(pf.amount_ytd, 0) AS amount
								    FROM (
										SELECT
											entity,
								        	ud1,
								        	scenario,
								        	year,
											month,
								        	account,
								        	intercompany,
								        	ud3,
								        	SUM(amount) AS amount
										FROM #staging_table 
										WHERE type = 'structure'
										GROUP BY
											entity,
								        	ud1,
								        	scenario,
								        	year,
											month,
								        	account,
								        	intercompany,
								        	ud3
									) st
								    FULL OUTER JOIN (
								        /* snapshot YTD del mes anterior */
								        SELECT
								            entity,
								            ud1,
								            scenario,
								            year,
								            month,
								            account,
								            intercompany,
								           	ud3,
								            amount_ytd
								        FROM XFC_RES_VIEW_structure_figures_ytd
								        WHERE year = @paramYear
											AND month = @paramPrevMonth
								          	AND scenario = @paramScenario
											AND entity IN ({entityFilter})
								    ) pf ON  st.entity      = pf.entity
								        AND st.ud1        	= pf.ud1
								        AND st.scenario     = pf.scenario
								        AND st.account      = pf.account
								        AND st.intercompany = pf.intercompany
								        AND st.ud3          = pf.ud3
								)
								
								-- Insert data into fact table
								INSERT INTO XFC_RES_FACT_structure_figures (
								    entity, ud1, scenario, year, month, account, intercompany, ud3, amount
								)
								SELECT
								    entity, ud1, scenario, year, month, account, intercompany, ud3, SUM(amount) AS amount
								FROM periodic_table
								GROUP BY entity, ud1, scenario, year, month, account, intercompany, ud3;
							
								DROP TABLE #staging_table;
							"
		                    BRApi.Database.ExecuteSql(dbConn, insertPeriodicSql, dbParamInfos, False)
		                End Using
						
						#End Region
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace