Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Diagnostics
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace OneStream.BusinessRule.Finance.SALES_SharedFunctions
	Public Class MainClass
		
		'Get UTI_SharedFunctions business rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.MemberListHeaders
						'Dim objMemberListHeaders As New List(Of MemberListHeader)
						'objMemberListHeaders.Add(new MemberListHeader("Sample Member List"))
						'Return objMemberListHeaders
						
					Case Is = FinanceFunctionType.MemberList
						'Example: E#Root.CustomMemberList(BRName=MyBusinessRuleName, MemberListName=[Sample Member List], Name1=Value1)
						'If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("Sample Member List") Then
							'Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							'Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "E#Root.Base", Nothing)
							'Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)
							'Return objMemberList
						'End If
						
					Case Is = FinanceFunctionType.DataCell
						'If args.DataCellArgs.FunctionName.XFEqualsIgnoreCase("Profit") Then
							'Return api.Data.GetDataCell("A#Sales * 0.9")
						'End If
						
					Case Is = FinanceFunctionType.FxRate
						'Try to get the FxRateType from the account's Text1 field.
						'Dim fxRateTypeForAccount As String = api.Account.Text(api.Pov.Account.MemberId, 1)
						'If Not String.IsNullOrEmpty(fxRateTypeForAccount) Then
							'Dim rate as Decimal = api.FxRates.GetCalculatedFxRate(fxRateTypeForAccount, args.FxRateArgs.SourceCurrencyId, args.FxRateArgs.DestCurrencyId)
							'Return new FxRateResult(rate)
						'End If
						
					Case Is = FinanceFunctionType.Calculate
						'api.Data.Calculate("A#Profit = A#Sales - A#Costs")
						
					Case Is = FinanceFunctionType.ConditionalInput
						'If api.Pov.Account.Name.XFEqualsIgnoreCase("ReadOnlyAccount") Then
							'Return ConditionalInputResultType.NoInput
						'End If
						Return ConditionalInputResultType.Default
					Case Is = FinanceFunctionType.CustomCalculate
						'If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Test") Then
						'api.Data.Calculate("A#Profit = A#Sales - A#Costs")
						'End If
						
					Case Is = FinanceFunctionType.ReadSourceDataRecords
						'Dim drCollection As New DataRecordCollection()
						
						'Read all dataRecords from another scenario.
						'Dim duCachePkForSourceScenario As DataUnitCachePk = api.Pov.GetDataUnitCachePk()
						'duCachePkForSourceScenario.ScenarioId = api.Members.GetMemberId(DimType.Scenario.Id, "Actual")
						'drCollection.AddRange(api.Data.ReadDataRecordsFromDatabase(duCachePkForSourceScenario, True))
						
						'Manually create a data record.
						'Dim dr As New DataRecord(12)
						'dr.DataRecordPk.SetMembers(api, True, "Sales", "None", "Forms", "None", "None", "None", "None", "None", "None", "None", "None", "None")
						'dr.DataCells(0).SetData(100.0, DataCellExistenceType.IsRealData, DataCellStorageType.Calculation)
						'dr.DataCells(1).SetData(101.0, DataCellExistenceType.IsRealData, DataCellStorageType.Calculation)
						'dr.DataCells(2).SetData(102.0, DataCellExistenceType.IsRealData, DataCellStorageType.Calculation)
						'dr.DataCells(3).SetData(103.0, DataCellExistenceType.IsRealData, DataCellStorageType.Calculation)
						'dr.DataCells(4).SetData(104.0, DataCellExistenceType.IsRealData, DataCellStorageType.Calculation)
						'dr.DataCells(5).SetData(105.0, DataCellExistenceType.IsRealData, DataCellStorageType.Calculation)
						'dr.DataCells(6).SetData(106.0, DataCellExistenceType.IsRealData, DataCellStorageType.Calculation)
						'dr.DataCells(7).SetData(107.0, DataCellExistenceType.IsRealData, DataCellStorageType.Calculation)
						'dr.DataCells(8).SetData(108.0, DataCellExistenceType.IsRealData, DataCellStorageType.Calculation)
						'dr.DataCells(9).SetData(109.0, DataCellExistenceType.IsRealData, DataCellStorageType.Calculation)
						'dr.DataCells(10).SetData(110.0, DataCellExistenceType.IsRealData, DataCellStorageType.Calculation)
						'dr.DataCells(11).SetData(111.0, DataCellExistenceType.IsRealData, DataCellStorageType.Calculation)
						'drCollection.Add(dr)
						
						'Return drCollection
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Functions"
		
		#Region "Import Sales to Cube"
		
		Public Sub ImportSalesToCube(ByVal si As SessionInfo, ByVal ParamBrand As String, ByVal ParamScenario As String)
			'Get parameters
			Dim forecastMonth As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Forecast_Month")
			Dim forecastYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Actual_Year")
			Dim budgetYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Budget_Year")
			Dim scenarioYear As String = If(ParamScenario = "Forecast", forecastYear, budgetYear)
			'Build a dictionary to get the brand entity ancestor
			Dim brandAncestorDict As New Dictionary(Of String, String) From {
				{"Burger King", "M_BK"},
				{"Fridays", "M_FR"},
				{"Domino''s Pizza", "M_DP"},
				{"Foster''s Hollywood", "M_FH"},
				{"Foster''s Hollywood Valencia", "M_FH"},
				{"Starbucks", "M_SB"},
				{"Starbucks Portugal", "M_SB"},
				{"Starbucks Bélgica", "M_SB"},
				{"Starbucks Francia", "M_SB"},
				{"Starbucks Holanda", "M_SB"},
				{"Ginos", "M_GI"},
				{"Ginos Portugal", "M_GI"},
				{"VIPS", "M_VI"}
			}
			'Build a dictionary to get the brand wf profile name
			Dim brandWfProfileDict As New Dictionary(Of String, String) From {
				{"Burger King", "BK"},
				{"Fridays", "Fridays"},
				{"Domino''s Pizza", "Dominos"},
				{"Foster''s Hollywood", "FH"},
				{"Foster''s Hollywood Valencia", "FHVLC"},
				{"Starbucks", "Starbucks"},
				{"Starbucks Portugal", "StarbucksPT"},
				{"Starbucks Bélgica", "StarbucksBE"},
				{"Starbucks Francia", "StarbucksFR"},
				{"Starbucks Holanda", "StarbucksNL"},
				{"Ginos", "Ginos"},
				{"Ginos Portugal", "GinosPT"},
				{"VIPS", "VIPS"}
			}
			'Ignore brands not in dictionaries
			If Not brandWfProfileDict.ContainsKey(ParamBrand) Then Return
			
			'Get current Workflow Cluster Pk
			Dim profileName As String = $"Planning_{brandWfProfileDict(ParamBrand)}.01 Sales Load"
			Dim profileKey As Guid = BRApi.Workflow.Metadata.GetProfile(si, profileName).ProfileKey
			Dim scenarioKey As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, ParamScenario)
			Dim timeKey As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, scenarioYear)
			
			Dim wfClusterPK As New WorkflowUnitClusterPk(profileKey, scenarioKey, timeKey)
			
			'Set Brand value to use it in the load process
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_BrandFilter_Value", ParamBrand)
			
			'Clear Stage Data
			BRApi.Import.Process.ClearStageData(si, wfClusterPK, "")

			'Execute extraction
			Dim piLoadTransform As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteParseAndTransform(si, wfClusterPK, Nothing, Nothing, TransformLoadMethodTypes.Replace, SourceDataOriginTypes.FromDirectConnection, False)

			'Execute validations
			Dim piValidateTransformation As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, wfClusterPK, True)
			Dim piValidateIntersections As ValidateIntersectionProcessInfo = BRApi.Import.Process.ValidateIntersections(si, wfClusterPK, True)

			'Execute Load & Process
			Dim piLoadCube As LoadCubeProcessInfo = BRApi.Import.Process.LoadCube(si, wfClusterPK)
			
			'Declare variables to execute sales copy from aux to main account
			Dim unitCodes As String
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				'Get the non franchise unit codes
				Dim selectQuery As String = $"
					SELECT DISTINCT c.cebe
					FROM XFC_DailySales ds
					JOIN XFC_CEBES c ON ds.unit_code = c.cebe
					WHERE
					    (
							ds.scenario = '{ParamScenario}'
							OR
							(
								ds.scenario = 'Actual'
								AND MONTH(ds.date) = {forecastMonth}
							)
						)
						AND YEAR(ds.date) = {scenarioYear}
						AND ds.brand = '{ParamBrand}'
						AND c.unit_type IN ('Unidades Propias', 'Unidades ECI')
				"
				Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
				'Iterate through each cebe to copy data from sales aux account to sales account
				If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
					For Each row As DataRow In dt.Rows
						unitCodes += If(String.IsNullOrEmpty(unitCodes), $"E#{row("cebe")}", $", E#{row("cebe")}")
					Next
				
					'Generate a time filter for only open months
					Dim timeFilter As String
					
					For i As Integer = If(ParamScenario = "Budget", 1, forecastMonth) To 12
						timeFilter += If(String.IsNullOrEmpty(timeFilter), $"T#{scenarioYear}M{i}", $", T#{scenarioYear}M{i}")
					Next
					'Create a dictionary with the data management sequence parameters
					Dim customSubstVars As New Dictionary(Of String, String) From {
						{"p_entity_filter", unitCodes},
						{"p_scenario", ParamScenario},
						{"p_year", scenarioYear},
						{"p_time_filter", timeFilter},
						{"p_brand", brandAncestorDict(ParamBrand)}
					}
					BRApi.Utilities.ExecuteDataMgmtSequence(si, guid.Empty, "SALES_LoadSalesToOwnUnits", customSubstVars)
				End If
			End Using
		End Sub
		
		#End Region
		
		#Region "Calendar Effect"
		
		#Region "Execute Calendar Effect"
		
		Public Function ExecuteCalendarEffect(
			ByVal si As SessionInfo, ByVal sScenario As String, ByVal sYear As String, ByVal sBrand As String, Optional paramStep As String = ""
		) As XFSelectionChangedTaskResult
					
		
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = "Projection made succesfully."
		
			Try
				'Get forecast month if scenario is forecast, else 1
				Dim sMonth As String = If(
					sScenario = "Forecast",
					BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Forecast_Month"),
					"1"
				)
				Dim sComparativeYearDifference As Integer
				If sScenario = "Budget" Then
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Try
							Dim selectQuery As String = $"	SELECT TOP(1) comparative_year
															FROM XFC_ComparativeYearByBrand
															WHERE brand = '{sBrand}' AND year = {sYear} AND scenario = '{sScenario}'"
							
							Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
							
							sComparativeYearDifference = CInt(sYear) - CInt(dt.Rows(0)("comparative_year"))
							
						Catch ex As Exception
							selectionChangedTaskResult.IsOK = False				
							selectionChangedTaskResult.ShowMessageBox = True				
							selectionChangedTaskResult.Message = "You must select a comparative year in event behaviors"
							Return selectionChangedTaskResult
						End Try
					End Using
				Else
					sComparativeYearDifference = 1
				End If

				' Get the name of the comparative column
				Dim comparativeDateColumnNameDict As New Dictionary(Of Integer, String) From {				                                                                            
		            {1, "date_comparable"},
		            {2, "date_comparable_2"},
		            {3, "date_comparable_3"},
		            {4, "date_comparable_4"},
		            {5, "date_comparable_5"},
		            {6, "date_comparable_6"}
		        }
																						
				Dim comparativeDateColumnName As String = comparativeDateColumnNameDict(sComparativeYearDifference)
				Dim comparativeDateColumnName2 As String = comparativeDateColumnNameDict(sComparativeYearDifference + 1)
				
				Dim sSales As String = "
					(
				  	-- AT
  					(((ISNULL(sales, 0) + ISNULL(rem_sales_adj, 0) + ISNULL(daily_sales_adj, 0)) / NULLIF((ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0)), 0))
  					+ ISNULL(week_at_yoyprice_adj, 0) + ISNULL(week_at_newprice_adj, 0) + ISNULL(week_at_prom_adj, 0) + ISNULL(week_at_prodmix_adj, 0) + ISNULL(week_at_gen1_adj, 0) + ISNULL(week_at_gen2_adj, 0) + ISNULL(week_at_trend_adj, 0))
  
  					-- CUSTOMERS
  					* (ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0) + ISNULL(week_customers_prom_adj, 0) + ISNULL(week_customers_camp_adj, 0) + ISNULL(week_customers_growth_adj, 0) + ISNULL(week_customers_remgrowth_adj, 0) + ISNULL(week_customers_gen1_adj, 0) + ISNULL(week_customers_gen2_adj, 0) + ISNULL(week_customers_trend_adj, 0) + ISNULL(month_customers_adj, 0))
  					)"
				
				Dim sCustomers As String = " ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0) + ISNULL(week_customers_prom_adj, 0) + ISNULL(week_customers_camp_adj, 0) + ISNULL(week_customers_growth_adj, 0) + ISNULL(week_customers_remgrowth_adj, 0) + ISNULL(week_customers_gen1_adj, 0) + ISNULL(week_customers_gen2_adj, 0) + ISNULL(week_customers_trend_adj, 0) + ISNULL(month_customers_adj, 0) "				
	
				' Query for clear sales projection for this exercise
				Dim sQuery_Delete As String = 	
					$"DELETE ds
					FROM XFC_DailySales ds
					INNER JOIN dbo.XFC_ComparativeCEBESAux cca
						ON ds.unit_code = cca.cebe
						AND cca.year = {sYear}
						AND cca.desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
					WHERE ds.scenario = '{sScenario}' 
					AND ds.year = {sYear}
					AND ds.brand = '{sBrand}'"	
							
					
				Dim sScenariosData As String = If (sScenario = "Budget","'Actual','Forecast'","'Actual'")	
				Dim sMonthMin As String = If (sScenario = "Budget","1",sMonth)	
					
				' Query for insert sales projection for this exercise
				Dim sQuery_Insert_ProjectedMonth As String = 	
					$"
					DECLARE @min_custom_week_date DATE;
					DECLARE @max_custom_week_date DATE;
					DECLARE @min_year_date DATE;
					DECLARE @max_year_date DATE;
					DECLARE @max_day INTEGER;
					DECLARE @max_custom_week_number INTEGER;
					DECLARE @max_date DATE;
				
					SET @min_year_date = CONVERT(DATE, '1-{sMonthMin}-' + CAST({sYear} AS VARCHAR), 105);
					SET @max_year_date = CONVERT(DATE, '31-12-' + CAST({sYear} AS VARCHAR), 105);
				
					SELECT
						@max_date = MAX(date)
					FROM XFC_DailySales
					WHERE brand = '{sBrand}'
						AND scenario = 'Actual';
				
					--SELECT
					--	  @max_day = MAX(DAY(date))
					--FROM XFC_DailySales
					--WHERE
						--scenario = 'Actual'
						--AND brand = '{sBrand}'
						--AND YEAR(date) = {sYear}
						--AND MONTH(date) = {sMonth};
					
					SELECT
						@min_custom_week_date = CASE WHEN '{sScenario}' = 'Budget' THEN MIN(date) ELSE CONVERT(DATE, '1-{sMonthMin}-' + CAST({sYear} AS VARCHAR), 105) END,
						@max_custom_week_date = MAX(date)
					FROM DateWeekNumber
					WHERE WeekYear = {sYear};
				
					SELECT
						@max_custom_week_number = MAX(CustomWeekNumber)
					FROM DateWeekNumber
					WHERE WeekYear = {sYear};				
				
					INSERT INTO XFC_DailySales (
					scenario
					,year
					,brand
					,unit_code	
					,unit_description
					,date
					,date_comparable
					,week_num
					,week_day
					,event
					,channel
					,sales
					,sales_comparable
					,customers
					,customers_comparable					
					)
					
					SELECT 
					'{sScenario}' 
					,{sYear}
					,D2.brand
					,D2.unit_code	
					,CE.description
					,C.date
					,C.{comparativeDateColumnName}
					,CASE
						WHEN C.date >  @max_custom_week_date THEN @max_custom_week_number + 1
						WHEN C.date < @min_custom_week_date  THEN 0
						ELSE DWN.CustomWeekNumber
					END AS week_number
					,DATENAME(weekday, C.date) AS week_day
					,''
					,D2.channel					
					,{sSales}
					,{sSales}
					,{sCustomers}
					,{sCustomers}		
				
					FROM (
						SELECT *
						FROM dbo.XFC_ComparativeDates
						WHERE date BETWEEN 
							CASE 
							  WHEN @min_year_date < 
							       @min_custom_week_date 
							  THEN @min_year_date 
							  ELSE @min_custom_week_date 
							END
							AND 
							CASE 
							  WHEN @max_year_date > 
							       @max_custom_week_date 
							  THEN @max_year_date 
							  ELSE @max_custom_week_date 
							END
					) C	
					CROSS JOIN (SELECT DISTINCT brand, unit_code, channel 
								FROM dbo.XFC_DailySales 				
								WHERE brand = '{sBrand}') AS D2	
					INNER JOIN (SELECT DISTINCT cebe 
					                   FROM XFC_ComparativeCEBES 
					                   WHERE desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
					                   AND Year(date) = {sYear}) CO
						ON D2.unit_code = CO.cebe         				
					INNER JOIN XFC_CEBES CE 
						ON D2.unit_code = CE.cebe		
				
					LEFT JOIN (
							SELECT *
							FROM dbo.XFC_DailySales
							WHERE brand = '{sBrand}'
							AND (
								(
									'{sScenario}' = 'Budget'
									AND (
										date <= @max_date
										AND scenario = 'Actual'
									) OR (
										date > @max_date
										AND scenario = 'Forecast'
									)
								) OR (
									'{sScenario}' = 'Forecast'
									AND scenario IN ({sScenariosData})
								)
							)
						) D
						ON C.{comparativeDateColumnName} = D.date
						AND D2.unit_code = D.unit_code
						AND D2.channel = D.channel
					INNER JOIN DateWeekNumber DWN
						ON C.date = DWN.Date

					WHERE
						(
							(
								'{sScenario}' = 'Budget'
								AND MONTH(C.date) >= {sMonth}
							)
							OR (
								'{sScenario}' = 'Forecast'
								AND (
									MONTH(C.date) >= {sMonth}
									--OR (
									--	MONTH(C.date) = {sMonth}
									--	AND DAY(C.date) > @max_day
									--)
								)
							)
							OR ('{sScenario}' = 'Budget' AND DWN.MonthNumber = 12)
						)		
					
					ORDER BY
					D2.brand
				 	,C.date
					,D2.unit_code
					,D2.channel;"	
					
				' Query for clear closings
				Dim sQuery_Delete_Closings As String = 	
					$"DELETE D
					FROM XFC_DailySales D
					INNER JOIN XFC_CEBESClosings C
						ON D.unit_code = C.cebe
					WHERE D.Scenario = '{sScenario}' 
					AND D.year = {sYear}
					AND D.Brand = '{sBrand}'
					AND D.date > C.close_date;"							
					
				Dim sQueryTOPFilter As String = If(sBrand ="Starbucks Holanda"," TOP 1000000 ","")	
					
				' Query for update sales projection for this exercise with special days (holidays and events)
				Dim sQuery_UpdateSalesDates As String =					
					$"UPDATE D
						SET D.sales = EB.sales * (1 + ISNULL(EV.perc_var,0))
							,D.customers = EB.customers * (1 + ISNULL(EV.perc_var,0))	
					
     				FROM (
						SELECT *
						FROM dbo.XFC_DailySales 
						WHERE scenario = '{sScenario}'
							AND year = {sYear}
							AND brand = '{sBrand}'
					) D
				
					LEFT JOIN XFC_CEBES CE 
						ON D.unit_code = CE.cebe
					INNER JOIN (
						SELECT *
						FROM dbo.XFC_ComparativeCEBESAux
						WHERE year = {sYear}
							AND desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
					) cca ON D.unit_code = cca.cebe		
				
     				INNER JOIN (SELECT E.date, E.event_name, EE.state, EE.region, EE.location, EE.channel, EE.unit, EE.id AS eventeffect_id
				
								, CASE WHEN E.event_name LIKE '(Prev%' AND ISNULL(EE.next_year_var,0) <> 0 THEN EE.next_year_var
									   WHEN E.event_name NOT LIKE '(Prev%' AND ISNULL(EE.ef_year_var,0) <> 0 THEN EE.ef_year_var
									   ELSE E.perc_var END AS perc_var
				
								FROM (
									SELECT *
									FROM dbo.XFC_EventBehavior
									WHERE brand = '{sBrand}'
								) E
								INNER JOIN (
									SELECT *
									FROM dbo.XFC_EventEffects
									WHERE brand = '{sBrand}'
								) EE ON E.event_id = EE.event_id
						) EV
					
						ON D.date = EV.date
						AND (CE.state = EV.state OR EV.state = 'All')
						AND (CE.region = EV.region OR EV.region = 'All')
						AND (CE.location = EV.location OR EV.location = 'All')
						AND (D.channel = EV.channel OR EV.channel = 'All')        
						AND (D.unit_code= EV.unit OR EV.unit = 'All')         	
								
					INNER JOIN (SELECT {sQueryTOPFilter} ES.eventeffect_id, ES.event_date, FIL.unit_code, FIL.channel, AVG({sSales}) AS sales, AVG({sCustomers}) AS customers
								FROM (	SELECT * 
										FROM XFC_EventSalesDates
										WHERE brand = '{sBrand}') ES
								--Clear All Data Inexistent
								CROSS JOIN (SELECT DISTINCT brand, unit_code, channel 
											FROM dbo.XFC_DailySales 
											WHERE brand = '{sBrand}'
											AND Year = {sYear} ) FIL
								LEFT JOIN (
									SELECT *
									FROM XFC_DailySales
									WHERE scenario IN ({sScenariosData})
										AND brand = '{sBrand}'
								) D ON ES.sales_date = D.date
									AND FIL.unit_code = D.unit_code
									AND FIL.channel = D.channel
								GROUP BY ES.eventeffect_id, ES.event_date, FIL.unit_code, FIL.channel)	EB
						
						ON D.unit_code = EB.unit_code
						AND D.channel = EB.channel
						AND EV.eventeffect_id = EB.eventeffect_id
						AND EV.date = EB.event_date"					
					
				Dim sQuery_UpdateEvents As String =	
					$"
					WITH filtered_events AS (
						SELECT DISTINCT E.date, E.event_name, EE.state, EE.region, EE.location, EE.unit, EE.channel
						FROM (
							SELECT *
							FROM dbo.XFC_EventBehavior
							WHERE brand = '{sBrand}'
						) E
						INNER JOIN (
							SELECT *
							FROM dbo.XFC_EventEffects
							WHERE brand = '{sBrand}'
						) EE ON E.event_id = EE.event_id
					), filtered_comparative_dates AS (
						SELECT *
						FROM XFC_ComparativeDates cd
						WHERE date IN (
							SELECT DISTINCT date
							FROM filtered_events
						)
					), events AS (
								SELECT CE.cebe, CD.date, EV.channel, STRING_AGG(EV.event_name, ', ') AS event_names
								FROM filtered_comparative_dates CD
								CROSS JOIN (SELECT cebe, brand, state, region, location FROM XFC_CEBES WHERE ((brand In ('SDH', 'Genéricos Grupo') AND sales_brand = '{sBrand}') OR (brand NOT IN ('SDH', 'Genéricos Grupo') AND brand = '{sBrand}'))) CE 							
     							INNER JOIN filtered_events EV ON CD.date = EV.date
									AND (CE.state = EV.state OR EV.state = 'All')
									AND (CE.region = EV.region OR EV.region = 'All')
									AND (CE.location = EV.location OR EV.location = 'All')
									AND (CE.cebe = EV.unit OR EV.unit = 'All')
				
								GROUP BY CE.cebe, CD.date, EV.channel			
						)
				
					UPDATE D
						SET D.event = RES.event_names
					
     				FROM (
						SELECT *
						FROM dbo.XFC_DailySales
						WHERE scenario = '{sScenario}'
							AND year = {sYear}
							AND brand = '{sBrand}'
					) D
					INNER JOIN (
						SELECT *
						FROM dbo.XFC_ComparativeCEBESAux
						WHERE year = {sYear}
						AND desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
					) cca ON D.unit_code = cca.cebe
				
					INNER JOIN events RES						
						ON D.date = RES.date
						AND D.unit_code = RES.cebe
						AND (D.channel = RES.channel OR RES.channel = 'All')              									
					
					"					
					
				Dim sQuery_UpdateComments As String =					
					$"
					WITH filtered_events AS (
						SELECT DISTINCT E.date, E.event_id, EE.state, EE.region, EE.location, EE.unit, EE.channel, E.sales_dates
	
						, CASE WHEN E.event_name LIKE '(Prev%' AND ISNULL(EE.next_year_var,0) <> 0 THEN EE.next_year_var
							   WHEN E.event_name NOT LIKE '(Prev%' AND ISNULL(EE.ef_year_var,0) <> 0 THEN EE.ef_year_var
							   ELSE E.perc_var END AS perc_var				

						FROM (
							SELECT *
							FROM dbo.XFC_EventBehavior
							WHERE brand = '{sBrand}'
						) E
						INNER JOIN (
							SELECT *
							FROM dbo.XFC_EventEffects
							WHERE brand = '{sBrand}'
						) EE ON E.event_id = EE.event_id
					), filtered_comparative_dates AS (
						SELECT *
						FROM XFC_ComparativeDates cd
						WHERE date IN (
							SELECT DISTINCT date
							FROM filtered_events
						)
					), events AS (
								SELECT CE.cebe, CD.date, EV.channel, EV.sales_dates AS sales_dates, EV.perc_var
								FROM filtered_comparative_dates CD
								CROSS JOIN (SELECT cebe, brand, state, region, location FROM XFC_CEBES WHERE ((brand In ('SDH', 'Genéricos Grupo') AND sales_brand = '{sBrand}') OR (brand NOT IN ('SDH', 'Genéricos Grupo') AND brand = '{sBrand}'))) CE 							
     							INNER JOIN filtered_events EV ON CD.date = EV.date
									AND (CE.state = EV.state OR EV.state = 'All')
									AND (CE.region = EV.region OR EV.region = 'All')
									AND (CE.location = EV.location OR EV.location = 'All')
									AND (CE.cebe = EV.unit OR EV.unit = 'All')		
						)
						
					UPDATE D
						SET D.comment = 'Sales dates: ' + RES.sales_dates + ' + ' + CONVERT(VARCHAR,ISNULL(RES.perc_var,0) * 100) + '%'
					
     				FROM (
						SELECT *
						FROM dbo.XFC_DailySaleS
						WHERE scenario = '{sScenario}'
						AND year = {sYear}
						AND brand = '{sBrand}'
					) D
					INNER JOIN (
						SELECT *
						FROM dbo.XFC_ComparativeCEBESAux
						WHERE year = {sYear}
						AND desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
					) cca ON D.unit_code = cca.cebe			
				
					INNER JOIN events RES			
						ON D.date = RES.date
						AND D.unit_code = RES.cebe
						AND (D.channel = RES.channel OR RES.channel = 'All')              									
					"										
						
				Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				
					If paramStep = "" OrElse paramStep = "Delete" Then BRApi.Database.ExecuteSql(dbcon, sQuery_delete, False)
					
'					BRApi.ErrorLog.LogMessage(si, "Comienza efecto calendario")
					
					'BRApi.ErrorLog.LogMessage(si, "1. sQuery_Insert_ProjectedMonth: " & sQuery_Insert_ProjectedMonth)
					If paramStep = "" OrElse paramStep = "InsertProjectedMonth" Then BRApi.Database.ExecuteSql(dbcon, sQuery_Insert_ProjectedMonth, False)

					'BRApi.ErrorLog.LogMessage(si, "2. sQuery_Delete_Closings: " & sQuery_Delete_Closings)
					If paramStep = "" OrElse paramStep = "DeleteClosings" Then BRApi.Database.ExecuteSql(dbcon, sQuery_Delete_Closings, False)
					
					'BRApi.ErrorLog.LogMessage(si, "3. sQuery_UpdateSalesDates: " & sQuery_UpdateSalesDates)
					If paramStep = "" OrElse paramStep = "UpdateSalesDates" Then BRApi.Database.ExecuteSql(dbcon, sQuery_UpdateSalesDates, False)
					
					''BRApi.ErrorLog.LogMessage(si, "4. sQuery_UpdateEvents: " & sQuery_UpdateEvents)
					If paramStep = "" OrElse paramStep = "UpdateEvents" Then BRApi.Database.ExecuteSql(dbcon, sQuery_UpdateEvents, False)
					
					'BRApi.ErrorLog.LogMessage(si, "5. sQuery_UpdateComments: " & sQuery_UpdateComments)
					If paramStep = "" OrElse paramStep = "UpdateComments" Then BRApi.Database.ExecuteSql(dbcon, sQuery_UpdateComments, False)
					
					'BRApi.ErrorLog.LogMessage(si, "Finaliza efecto calendario")
															
				End Using
			
			Catch ex As Exception
				selectionChangedTaskResult.IsOK = False				
				selectionChangedTaskResult.ShowMessageBox = True				
				selectionChangedTaskResult.Message = "Error: " & ex.Message
				
			End Try
			
			Return selectionChangedTaskResult
										
		End Function
		
		#End Region
		
		#End Region
		
		#Region "Weekly Variations"
		
		#Region "Recover Weekly Variations"
		
		Public Sub RecoverWeeklyVariations(ByVal si As SessionInfo, ByVal ParamScenario As String, ByVal ParamBrand As String)
		
			'Get year based on scenario
			Dim ParamYear As String = If(
				ParamScenario = "Forecast",
				BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Actual_Year"),
				BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Budget_Year")
			)
			
			'Build a query to get all the adjustment for the scenario and brand
			Dim selectAdjustmentsQuery As String = $"
				SELECT *, CASE WHEN Unit = 'All' THEN 0 ELSE 1 END AS Orden
				FROM dbo.XFC_WeeklyAdjustments
				WHERE
					scenario = '{ParamScenario}'
					AND brand = '{ParamBrand}'
					AND year = {ParamYear}
				ORDER BY Orden"
			
			Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
			'Reset all the adjustments for that brand and scenario
			Dim resetQuery As String = $"
				UPDATE dbo.XFC_DailySales
				SET week_customers_prom_adj = 0,
					week_customers_camp_adj = 0,
					week_customers_growth_adj = 0,
					week_customers_remgrowth_adj = 0,
					week_customers_gen1_adj = 0,
					week_customers_gen2_adj = 0,
					week_at_yoyprice_adj = 0,
					week_at_newprice_adj = 0,
					week_at_prom_adj = 0,
					week_at_prodmix_adj = 0,
					week_at_gen1_adj = 0,
					week_at_gen2_adj = 0
				WHERE 
					scenario = '{ParamScenario}'
					AND brand = '{ParamBrand}'
					AND year = {ParamYear}"
		
			Dim resetDt As DataTable = BRApi.Database.ExecuteSql(dbcon,resetQuery,False)
			
			Dim adjustmentDt As DataTable = BRApi.Database.ExecuteSql(dbcon,selectAdjustmentsQuery,False)
			If adjustmentDt IsNot Nothing AndAlso adjustmentDt.Rows.Count > 0 Then Me.ExecuteWeeklyVariations(si, adjustmentDt)
				
			End Using
		
		End Sub
		
		#End Region
		
		#Region "Execute Weekly Variations"
		
		Public Sub ExecuteWeeklyVariations(ByVal si As SessionInfo, ByVal adjustmentDt As DataTable)
		
			'Create a adj type 1 dict
			Dim adjType1Dict As New Dictionary(Of String, String) From {
	            {"Customers", "week_customers_"},
	            {"AT", "week_at_"}
	        }
																		
			'Create a adj type 2 dict
			Dim adjType2Dict As New Dictionary(Of String, String) From {
	            {"Promotion", "prom_adj"},
	            {"Campaign", "camp_adj"},
				{"Growth", "growth_adj"},
	            {"Remodeling Growth", "remgrowth_adj"},
	            {"Generic 1", "gen1_adj"},
	            {"Generic 2", "gen2_adj"},
	            {"YoY Price Growth", "yoyprice_adj"},
	            {"New Price Growth", "newprice_adj"},
	            {"Product Mix", "prodmix_adj"}
	        }
																		
			'Create a adj type 1 dict from
			Dim adjType1FromDict As New Dictionary(Of String, String) From {
	            {"Customers", "COALESCE(customers, 0) + COALESCE(rem_customers_adj, 0) + COALESCE(daily_customers_adj, 0)"},
	            {"AT", "CASE WHEN COALESCE(customers, 0) + COALESCE(rem_customers_adj, 0) + COALESCE(daily_customers_adj, 0) = 0 THEN 0
					        	ELSE (CASE WHEN COALESCE(customers, 0.0) + COALESCE(rem_customers_adj, 0.0) = 0 THEN 0.0
									ELSE (
										   (
										   COALESCE(sales, 0.0) + COALESCE(rem_sales_adj, 0.0)
										   )
										   /
										   (
										   COALESCE(customers, 0.0) + COALESCE(rem_customers_adj, 0.0)
										   ) 
										  ) END																													   
									)
					    END"}
	        }
			
			Dim columnToUpdate As String
			Dim columnFrom As String
			
			Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
			If adjustmentDt IsNot Nothing AndAlso adjustmentDt.Rows.Count > 0 Then
				
				For Each row As DataRow In adjustmentDt.Rows
					
					Dim rowBrand As String = row("brand").ToString.Replace("'", "''")
					Dim rowStartWeek As String = row("start_week").ToString
					Dim rowEndWeek As String = row("end_week").ToString
					Dim rowYear As String = row("year").ToString
					Dim rowChannel As String = row("channel").ToString
					Dim rowCountry As String = row("country").ToString
					Dim rowState As String = row("state").ToString
					Dim rowRegion As String = row("region").ToString
					Dim rowLocation As String = row("location").ToString
					Dim rowUnit As String = row("unit").ToString
					Dim rowCluster As String = row("cluster").ToString
					Dim rowScenario As String = row("scenario").ToString
					Dim rowAdjType1 As String = row("adj_type_1").ToString
					Dim rowAdjType2 As String = row("adj_type_2").ToString
					Dim rowAdjType3 As String = row("adj_type_3").ToString
'					Dim rowAmount As String = row("amount").ToString                   
                    Dim rowAmount As Decimal
					Decimal.TryParse(row("amount").ToString.Replace(",","."), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, rowAmount)
																		
					'Create the column based on the Adj Types
					columnToUpdate = $"{adjType1Dict(rowAdjType1)}{adjType2Dict(rowAdjType2)}"
																				
					'Set the column from depending on the type
					columnFrom = adjType1FromDict(rowAdjType1)
					
					'Get parameter filters
					Dim filterWeek = $"AND ds.week_num BETWEEN {rowStartWeek} AND {rowEndWeek}"
					Dim filterBrand = UTISharedFunctionsBR.CreateSQLFilter(si, rowBrand, "ds.brand")
					Dim filterChannel = UTISharedFunctionsBR.CreateSQLFilter(si, rowChannel, "ds.channel")
					Dim filterCountry = UTISharedFunctionsBR.CreateSQLFilter(si, rowCountry, "c.country")
					Dim filterState = UTISharedFunctionsBR.CreateSQLFilter(si, rowState, "c.state")
					Dim filterRegion = UTISharedFunctionsBR.CreateSQLFilter(si, rowRegion, "c.region")
					Dim filterLocation = UTISharedFunctionsBR.CreateSQLFilter(si, rowLocation, "c.location")
					Dim filterUnit = UTISharedFunctionsBR.CreateSQLFilter(si, rowUnit, "c.cebe")
					Dim filterCluster As String' = UTISharedFunctionsBR.CreateSQLFilter(si, rowCluster, "c.cluster")
					Dim filterScenario = UTISharedFunctionsBR.CreateSQLFilter(si, rowScenario, "ds.scenario")	
					
					Dim mainFilter As String = $"{filterBrand} {filterCountry} {filterState} {filterRegion} {filterLocation} {filterUnit} {filterChannel} {filterCluster} {filterWeek} {filterScenario}"
					
					Dim dt As DataTable
				
					Dim updateQuery As String
					
					'Get update query depending on if it's Unit or %
					If rowAdjType3 = "Unit" Then

						
						'Update the sales of that week and filters based on the percent that the unit increment represents
						updateQuery = $"
							WITH WeeklySales AS (
							SELECT
								ds.week_num AS week_num,
								SUM({columnFrom}) AS week_sales
							FROM dbo.XFC_DailySales ds
							INNER JOIN dbo.XFC_CEBES c ON ds.unit_code = c.cebe
							INNER JOIN dbo.XFC_ComparativeCEBESAux cca
								ON ds.unit_code = cca.cebe
								AND cca.year = {rowYear}
								AND cca.desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
							WHERE YEAR(date) = {rowYear} {mainFilter}
							GROUP BY
								ds.week_num
							)
						
							UPDATE ds
							SET {columnToUpdate} = ({columnFrom}) * {rowAmount.toString.Replace(",",".")} / NULLIF(ws.week_sales,0)
							FROM XFC_DailySales ds
							INNER JOIN XFC_CEBES c ON ds.unit_code = c.cebe
							INNER JOIN dbo.XFC_ComparativeCEBESAux cca
								ON ds.unit_code = cca.cebe
								AND cca.year = {rowYear}
								AND cca.desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
							INNER JOIN WeeklySales ws ON ds.week_num = ws.week_num
							WHERE YEAR(date) = {rowYear} {mainFilter};"
						
						dt = BRApi.Database.ExecuteSql(dbcon,updateQuery,False)
						
					Else
						
						rowAmount = rowAmount / 100
						updateQuery = $"
							UPDATE ds
						   	SET {columnToUpdate} = ({columnFrom}) * {rowAmount.toString.Replace(",",".")}
							FROM XFC_DailySales ds
							INNER JOIN XFC_CEBES c ON ds.unit_code = c.cebe
							INNER JOIN dbo.XFC_ComparativeCEBESAux cca
								ON ds.unit_code = cca.cebe
								AND cca.year = {rowYear}
								AND cca.desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
						   	WHERE YEAR(date) = {rowYear} {mainFilter};"
									
						rowAmount = rowAmount * 100
						
						dt = BRApi.Database.ExecuteSql(dbcon,updateQuery,False)
						
					End If
				
				Next
				
			End If
				
			End Using
		
		End Sub
		
		#End Region
		
		#End Region
		
		#Region "Monthly Adjustments"
		
		#Region "Recover Monthly Adjustments"
		
		Public Sub RecoverMonthlyAdjustments(ByVal si As SessionInfo, ByVal ParamScenario As String, ByVal ParamBrand As String)
		
			'Get year based on scenario
			Dim ParamYear As String = If(
				ParamScenario = "Forecast",
				BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Actual_Year"),
				BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Budget_Year")
			)
			
			Dim adjustmentDt As DataTable
			Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
				'Build a query to get all the adjustment for the scenario and brand
				Dim selectAdjustmentsQuery As String = $"
					SELECT *, CASE WHEN Unit = 'All' THEN 0 ELSE 1 END AS Orden
					FROM dbo.XFC_MonthlyAdjustments
					WHERE
						scenario = '{ParamScenario}'
						AND brand = '{ParamBrand}'
						AND year = {ParamYear}
					ORDER BY Orden"
				adjustmentDt = BRApi.Database.ExecuteSql(dbcon,selectAdjustmentsQuery,False)
				
				'Reset all the adjustments for that brand and scenario
				
				Dim resetQuery As String = $"
					UPDATE dbo.XFC_DailySales
					SET month_customers_adj = 0,
				        month_sales_adj = 0
					WHERE 
						scenario = '{ParamScenario}'
						AND brand = '{ParamBrand}'
						AND year = {ParamYear}"
				Dim resetDt As DataTable = BRApi.Database.ExecuteSql(dbcon,resetQuery,False)
				
			End Using
			
			If adjustmentDt IsNot Nothing AndAlso adjustmentDt.Rows.Count > 0 Then Me.ExecuteMonthlyAdjustments(si, adjustmentDt)
		
		End Sub
		
		#End Region
		
		#Region "Execute Monthly Adjustments"
		
		Public Sub ExecuteMonthlyAdjustments(ByVal si As SessionInfo, ByVal adjustmentDt As DataTable)
			
			Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
			If adjustmentDt IsNot Nothing AndAlso adjustmentDt.Rows.Count > 0 Then
				
				For Each row As DataRow In adjustmentDt.Rows
					
					Dim rowScenario As String = row("scenario").ToString
					Dim rowYear As String = row("year").ToString
					Dim rowMonth As String = row("month").ToString
					Dim rowBrand As String = row("brand").ToString.Replace("'", "''")
					Dim rowCountry As String = row("country").ToString
					Dim rowState As String = row("state").ToString
					Dim rowLocation As String = row("location").ToString
					Dim rowRegion As String = row("region").ToString
					Dim rowUnit As String = row("unit").ToString
					Dim rowCluster As String = row("cluster").ToString
					Dim rowProperty As String = row("property").ToString
					Dim rowAdjType As String = row("adj_type").ToString
					Dim rowChannel As String = row("channel").ToString
                    Dim rowAmount As Decimal
					Decimal.TryParse(row("amount").ToString.Replace(",","."), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, rowAmount)
					
					'Get parameter filters
					Dim filterBrand = UTISharedFunctionsBR.CreateSQLFilter(si, rowBrand, "c.sales_brand")
					Dim filterState = UTISharedFunctionsBR.CreateSQLFilter(si, rowState, "c.state")
					Dim filterLocation = UTISharedFunctionsBR.CreateSQLFilter(si, rowLocation, "c.location")
					Dim filterRegion = UTISharedFunctionsBR.CreateSQLFilter(si, rowRegion, "c.region")
					Dim filterChannel = UTISharedFunctionsBR.CreateSQLFilter(si, rowChannel, "ds.channel")
'					Dim filterCluster = UTISharedFunctionsBR.CreateSQLFilter(si, rowCluster, "cluster")
					Dim filterUnit = UTISharedFunctionsBR.CreateSQLFilter(si, rowUnit, "c.cebe")
					Dim filterScenario = UTISharedFunctionsBR.CreateSQLFilter(si, rowScenario, "ds.scenario")
					Dim filterScenarioYear = UTISharedFunctionsBR.CreateSQLFilter(si, rowYear, "YEAR(ds.date)")
					Dim filterMonth = UTISharedFunctionsBR.CreateSQLFilter(si, rowMonth, "MONTH(ds.date)")
					Dim filterProperty = $"AND (
											('{rowProperty}' = 'All')
											OR
											('{rowProperty}' = 'Own' AND c.unit_type IN ('Unidades Propias', 'Unidades ECI'))
											OR
											('{rowProperty}' = 'Franchises' AND c.unit_type NOT IN ('Unidades Propias', 'Unidades ECI'))
										)"
					
					Dim cebesFilter As String = $"{filterBrand.Replace("AND", "")} {filterState} {filterRegion} {filterLocation} {filterProperty} {filterUnit}"
					
					Dim globalFilter As String = $"{filterBrand} {filterState} {filterRegion} {filterLocation} {filterProperty} {filterUnit} {filterScenario} {filterScenarioYear} {filterChannel}"
					
					Dim salesFilter As String = $"{filterScenario.Replace("AND", "")} {filterScenarioYear} {filterMonth} {filterChannel}"
					
					Dim selectQuery As String = String.Empty
					Dim updateQuery As String = String.Empty
					Dim dt As DataTable
					
					'Get update query depending on if it's Eur or %
					If rowAdjType = "EUR" Then
						
						'Get SUM(columnFrom) of that week and rest of the filters
						Dim sumSales As Decimal
						 selectQuery = $"
						 	WITH filtered_comparative_cebes AS (
						 		SELECT cebe
					 			FROM dbo.XFC_ComparativeCEBESAux WITH(NOLOCK)
					 			WHERE 
					 				year = {rowYear}
									AND desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
						 	), filtered_cebes AS (
						 		SELECT cebe
						 		FROM (
						 			SELECT cebe
						 			FROM dbo.XFC_CEBES c WITH(NOLOCK)
						 			WHERE {cebesFilter}
						 		) cOuter
						 		WHERE EXISTS (
						 			SELECT 1
						 			FROM filtered_comparative_cebes fcc
						 			WHERE cOuter.cebe = fcc.cebe
						 		)
						 	), filtered_sales AS (
						 		SELECT *
						 		FROM dbo.XFC_DailySales ds
						 		WHERE {salesFilter}
						 			AND brand = '{rowBrand}'
						 	)
						 
							SELECT SUM(
							  	-- AT
			  					(
						 			COALESCE(
						 				(ISNULL(sales, 0) + ISNULL(rem_sales_adj, 0) + ISNULL(daily_sales_adj, 0))
						 				/ NULLIF(ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0), 0),
						 				0
						 			)
				  					+ ISNULL(week_at_yoyprice_adj, 0) + ISNULL(week_at_newprice_adj, 0) + ISNULL(week_at_prom_adj, 0) + ISNULL(week_at_prodmix_adj, 0)
							 		+ ISNULL(week_at_gen1_adj, 0) + ISNULL(week_at_gen2_adj, 0) + ISNULL(week_at_trend_adj, 0)
						 		)
			  					-- CUSTOMERS
			  					* (
						 			ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0) + ISNULL(week_customers_prom_adj, 0)
						 			+ ISNULL(week_customers_camp_adj, 0) + ISNULL(week_customers_growth_adj, 0) + ISNULL(week_customers_remgrowth_adj, 0)
						 			+ ISNULL(week_customers_gen1_adj, 0) + ISNULL(week_customers_gen2_adj, 0) + ISNULL(week_customers_trend_adj, 0)
						 		)
			  				) AS sales
							FROM filtered_sales fs
							WHERE EXISTS (
						 		SELECT 1
						 		FROM filtered_cebes fc
						 		WHERE fs.unit_code = fc.cebe
						 	);"
						dt = BRApi.Database.ExecuteSql(dbcon,selectQuery,False)
						
						sumSales = If(dt IsNot Nothing AndAlso dt.Rows.Count > 0 AndAlso Not String.IsNullOrEmpty(dt.Rows(0)("sales").ToString), CDec(dt.Rows(0)("sales").ToString), 0)
						'Get the percent increase based on the amount of units
						rowAmount = If(sumSales = 0, 0, rowAmount / sumSales)

						updateQuery = $"
							UPDATE dbo.XFC_DailySales
						   	SET month_customers_adj = (
								ISNULL(customers,0) + ISNULL(rem_customers_adj,0) + ISNULL(daily_customers_adj,0) + ISNULL(week_customers_prom_adj,0)
								+ ISNULL(week_customers_camp_adj,0) + ISNULL(week_customers_growth_adj,0) + ISNULL(week_customers_remgrowth_adj,0)
								+ ISNULL(week_customers_gen1_adj,0) + ISNULL(week_customers_gen2_adj,0) + ISNULL(week_customers_trend_adj, 0)
							) * {rowAmount.ToString.Replace(",",".")}
							FROM dbo.XFC_DailySales ds
							INNER JOIN dbo.XFC_CEBES c ON ds.unit_code = c.cebe
							INNER JOIN dbo.XFC_ComparativeCEBESAux cca
								ON ds.unit_code = cca.cebe
								AND cca.year = {rowYear}
								AND cca.desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
						   	WHERE MONTH(date) = {rowMonth} {globalFilter};"
						
					Else
						
						rowAmount = rowAmount / 100
						
						updateQuery = $"
							UPDATE dbo.XFC_DailySales
							SET month_customers_adj = (
								ISNULL(customers,0) + ISNULL(rem_customers_adj,0) + ISNULL(daily_customers_adj,0) + ISNULL(week_customers_prom_adj,0)
								+ ISNULL(week_customers_camp_adj,0) + ISNULL(week_customers_growth_adj,0) + ISNULL(week_customers_remgrowth_adj,0)
								+ ISNULL(week_customers_gen1_adj,0) + ISNULL(week_customers_gen2_adj,0) + ISNULL(week_customers_trend_adj, 0)
							) * {rowAmount.ToString.Replace(",",".")}
							FROM dbo.XFC_DailySales ds
							INNER JOIN dbo.XFC_CEBES c ON ds.unit_code = c.cebe
							INNER JOIN dbo.XFC_ComparativeCEBESAux cca
								ON ds.unit_code = cca.cebe
								AND cca.year = {rowYear}
								AND cca.desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
							WHERE MONTH(date) = {rowMonth} {globalFilter};"
						
					End If
						
					dt = BRApi.Database.ExecuteSql(dbcon,updateQuery,False)
				
				Next
				
			End If
				
			End Using
		
		End Sub
		
		#End Region
		
		#End Region
		
		#Region "Remodelings"
		
		#Region "Execute Remodelings"
		
		Public Sub ExecuteRemodelings(ByVal si As SessionInfo, ByVal sScenario As String, ByVal sBrand As String, ByVal sYear As String, ByVal sMonth As String)
			Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				'Reset all the adjustments for that brand and scenario
				 Dim resetQuery As String = $"UPDATE dbo.XFC_DailySales
											 SET rem_sales_adj = 0,
												rem_customers_adj = 0							
											 WHERE year = {sYear} AND scenario = '{sScenario}' AND brand = '{sBrand}'"
				Dim resetDt As DataTable = BRApi.Database.ExecuteSql(dbcon,resetQuery,False)			
			
				'Query REMODELINGS YEAR
				Dim updateQuery1 As String = $"UPDATE XFC_DailySales
												SET XFC_DailySales.rem_sales_adj = D.sales * -1
													, XFC_DailySales.rem_customers_adj = D.customers * -1
											
											FROM XFC_DailySales D	
											INNER JOIN XFC_Remodelings R
												ON D.unit_code = R.unit_code
												AND D.date BETWEEN R.start_date AND R.end_date
											
											WHERE D.scenario = '{sScenario}' 
											AND D.year = {sYear}
											AND Month(D.date) >= {sMonth} 
											AND year(D.date) = {sYear}
											AND D.brand = '{sBrand}'"
				BRApi.Database.ExecuteSql(dbcon, updateQuery1, False)	
				
				Dim sScenarioInit As String = If (sScenario = "Budget", "'Actual','Forecast'","'Actual'")
				
				'Query REMODELINGS PREV. YEAR
'				Dim updateQuery2 As String = $"UPDATE XFC_DailySales
'												SET XFC_DailySales.rem_sales_adj = ISNULL(RES.sales,0) - ISNULL( D.sales,0)
'													,XFC_DailySales.rem_customers_adj = ISNULL(RES.customers,0) - ISNULL( D.customers,0)
											
'											FROM XFC_DailySales D	
'											INNER JOIN XFC_Remodelings R
'												ON D.unit_code = R.unit_code
'											INNER JOIN XFC_RemodelingAffectedDates RA
'												ON R.id = RA.remodeling_id
'												AND D.date = RA.date
'											INNER JOIN (SELECT remodeling_id, RS.week_day, channel, AVG(sales) AS sales, AVG(customers) AS customers																		
'														FROM XFC_Remodelings R
'														INNER JOIN XFC_RemodelingSourceDates RS
'															ON R.id = RS.remodeling_id
'														INNER JOIN XFC_DailySales D
'															ON R.unit_code = D.unit_code
'															AND D.date = RS.date
'															AND D.scenario IN ({sScenarioInit}) 
'														WHERE D.brand = '{sBrand}'
'														GROUP BY RS.remodeling_id, RS.week_day, channel) AS RES
'												ON R.id = RES.remodeling_id
'												AND RA.week_day = RES.week_day
'												AND D.channel = RES.channel
'											WHERE D.scenario = '{sScenario}' 
'											AND D.year = {sYear}
'											AND Month(D.date) >= {sMonth} 
'											AND year(D.date) = {sYear}
'											AND D.brand = '{sBrand}'"
				
				' Get final sales and customers queries
				Dim defaultWorkspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "Default")
				Dim queryFinalSales As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(
					si,
					False,
					defaultWorkspaceID,
					"prm_SQL_FinalSales"
				)
				Dim queryFinalCustomers As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(
					si,
					False,
					defaultWorkspaceID,
					"prm_SQL_FinalCustomers"
				)

				Dim updateQuery2 As String = $"UPDATE XFC_DailySales
												SET XFC_DailySales.rem_sales_adj = ISNULL(RES.sales,0) - ISNULL( D.sales,0)
													,XFC_DailySales.rem_customers_adj = ISNULL(RES.customers,0) - ISNULL( D.customers,0)
											
											FROM XFC_DailySales D	
											INNER JOIN XFC_Remodelings R
												ON D.unit_code = R.unit_code
											INNER JOIN XFC_RemodelingAffectedDates RA
												ON R.id = RA.remodeling_id
												AND D.date = RA.date
											INNER JOIN (SELECT remodeling_id, RS.week_day, channel, AVG({queryFinalSales}) AS sales, AVG({queryFinalCustomers}) AS customers																		
														FROM XFC_Remodelings R
														INNER JOIN XFC_RemodelingSourceDates RS
															ON R.id = RS.remodeling_id
														INNER JOIN (
															SELECT *
															FROM XFC_DailySales
															WHERE brand = '{sBrand}'
																AND scenario IN ({sScenarioInit})
																OR (
																	'{sScenario}' = 'Budget'
																	AND year = {sYear}
																	AND scenario = 'Budget'
																	AND YEAR(date) = {sYear}
																)
														) D ON R.unit_code = D.unit_code
															AND D.date = RS.date
														GROUP BY RS.remodeling_id, RS.week_day, channel
											) AS RES ON R.id = RES.remodeling_id
												AND RA.week_day = RES.week_day
												AND D.channel = RES.channel
											WHERE D.scenario = '{sScenario}' 
											AND D.year = {sYear}
											AND Month(D.date) >= {sMonth} 
											AND year(D.date) = {sYear}
											AND D.brand = '{sBrand}'"
				BRApi.Database.ExecuteSql(dbcon, updateQuery2, False)
			End Using
		End Sub
		
		#End Region
		
		#End Region
		
		#End Region
		
	End Class
End Namespace