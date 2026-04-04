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
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions
		
        Public Sub CustomCalculate(ByVal si As SessionInfo, ByVal brGlobals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) Implements IWsasFinanceCustomCalculateV800.CustomCalculate
            Try
				
				#Region "Send Data to Cube"
				
				If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("SendDataToCube") Then
					'Get parameters
					Dim queryParams As String = args.CustomCalculateArgs.NameValuePairs("p_query_params")
					Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
					Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
					Dim entityMemberName As String = api.Pov.Entity.Name
					dbParamInfos.Add(New DbParamInfo("paramEntityMember", entityMemberName))
					Dim month As Integer = api.Pov.Time.Name.Split("M")(1)
					
					'Set parameters for foreign currencies
					Dim entityCurrency As String = api.Entity.GetLocalCurrency(api.Pov.Entity.MemberId).Name
					dbParamInfos.Add(New DbParamInfo("paramCurrency", entityCurrency))
					Dim RateTypeRevenueExp As String = api.FxRates.GetFxRateTypeForRevenueExp(,api.Pov.Scenario.MemberId).Name
					dbParamInfos.Add(New DbParamInfo("paramRateTypeRevenueExp", RateTypeRevenueExp))
					Dim RateTypeAssetLiability As String = api.FxRates.GetFxRateTypeForAssetLiability(,api.Pov.Scenario.MemberId).Name
					dbParamInfos.Add(New DbParamInfo("paramRateTypeAssetLiab", RateTypeAssetLiability))
					Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
					dbParamInfos.Add(New DbParamInfo("paramForecastMonth", paramForecastMonth))
					
					'Clear data unit
					Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(A#All, U4#CAPEXRD, U4#INV_CAPEX, U4#AMORT)",,False)
					UTISharedFunctionsBR.ClearDataBuffer(si, api, targetDataBuffer)
					
					'Declare target data buffer and expression destination info
					targetDataBuffer = New DataBuffer()
					Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#YTD")
					
					' Set default members
					Dim defaultOrigin As String = "Import"
					Dim defaultIC As String = "None"
					Dim defaultU1 As String = "None"
					Dim defaultU2 As String = "None"
					Dim defaultU3 As String = "None"
					Dim defaultU4 As String = "INV_CAPEX"
					Dim defaultU5 As String = "None"
					Dim defaultU6 As String = "None"
					Dim defaultU7 As String = "None"
					Dim defaultU8 As String = "None"
					
					'Get global variables for the data to be uploaded, capitalization, cash and supplier data only for first base entity
					If entityMemberName.EndsWith("001") Then
						Dim capitalizationDataDict As Dictionary(Of Integer, Dictionary(Of String, Decimal)) = Me.GetCapitalizationDataDict(si, dbParamInfos, parameterDict, brGlobals)
						Dim cashDataDict As Dictionary(Of Integer, Decimal) = Me.GetCashDataDict(si, dbParamInfos, parameterDict, brGlobals)
						'Dim supplierDataDict As Dictionary(Of Integer, Decimal) = Me.GetSupplierDataDict(si, dbParamInfos, parameterDict, brGlobals)
						
						'CAPITALIZATION
						'Load capitalization data if month exists and accumulate amounts for supplier data calculation
						Dim accumulatedCapitalization As Decimal = 0.0
						If capitalizationDataDict.ContainsKey(month) Then
							'Loop through each account to create data buffer cells
							For Each accountToAmount In capitalizationDataDict(month)
								Dim dataCell As New DataBufferCell()
							
								' Skip accounts that don't exist
								If BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, accountToAmount.Key) = -1 Then Continue For
								
								' Create and set data cell
								dataCell.SetAccount(api, accountToAmount.Key)
								dataCell.SetFlow(api, "F20")
								dataCell.SetOrigin(api, defaultOrigin)
								dataCell.SetIC(api, defaultIC)
								dataCell.SetUD1(api, defaultU1)
								dataCell.SetUD2(api, defaultU2)
								dataCell.SetUD3(api, defaultU3)
								dataCell.SetUD4(api, defaultU4)
								dataCell.SetUD5(api, defaultU5)
								dataCell.SetUD6(api, defaultU6)
								dataCell.SetUD7(api, defaultU7)
								dataCell.SetUD8(api, defaultU8)
								dataCell.CellAmount = accountToAmount.Value
								dataCell.CellStatus = New DataCellStatus(DataCellExistenceType.IsRealData, DataCellStorageType.Input)
								
								' Accumulate capitalization
								accumulatedCapitalization += accountToAmount.Value
								
								targetDataBuffer.SetCell(si, dataCell, True)
							Next
						End If
						
						'CASH
						'Load cash data if month exists and set a variable with the amount for supplier calculation
						Dim cashAmount As Decimal = 0.0
						If cashDataDict.ContainsKey(month) Then
							Dim dataCell As New DataBufferCell()

							' Create and set data cell
							dataCell.SetAccount(api, "1681000CF")
							dataCell.SetFlow(api, "F30")
							dataCell.SetOrigin(api, defaultOrigin)
							dataCell.SetIC(api, defaultIC)
							dataCell.SetUD1(api, defaultU1)
							dataCell.SetUD2(api, defaultU2)
							dataCell.SetUD3(api, defaultU3)
							dataCell.SetUD4(api, defaultU4)
							dataCell.SetUD5(api, defaultU5)
							dataCell.SetUD6(api, defaultU6)
							dataCell.SetUD7(api, defaultU7)
							dataCell.SetUD8(api, defaultU8)
							dataCell.CellAmount = cashDataDict(month)
							dataCell.CellStatus = New DataCellStatus(DataCellExistenceType.IsRealData, DataCellStorageType.Input)
							
							' Set Cash amount variable
							cashAmount = cashDataDict(month)
							
							targetDataBuffer.SetCell(si, dataCell, True)
						End If
						
						'SUPPLIER
						'Load supplier data if it has data
						If Decimal.Abs(accumulatedCapitalization + cashAmount) > 0.001 Then
							Dim dataCell As New DataBufferCell()

							' Create and set data cell
							dataCell.SetAccount(api, "2701000C")
							dataCell.SetFlow(api, "F20")
							dataCell.SetOrigin(api, defaultOrigin)
							dataCell.SetIC(api, defaultIC)
							dataCell.SetUD1(api, defaultU1)
							dataCell.SetUD2(api, defaultU2)
							dataCell.SetUD3(api, defaultU3)
							dataCell.SetUD4(api, defaultU4)
							dataCell.SetUD5(api, defaultU5)
							dataCell.SetUD6(api, defaultU6)
							dataCell.SetUD7(api, defaultU7)
							dataCell.SetUD8(api, defaultU8)
							dataCell.CellAmount = accumulatedCapitalization + cashAmount
							dataCell.CellStatus = New DataCellStatus(DataCellExistenceType.IsRealData, DataCellStorageType.Input)
							
							targetDataBuffer.SetCell(si, dataCell, True)
						End If
					End If
					
					'DEPRECIATION
					'Get data
					Dim entityDepreciationDataDict As Dictionary(
						Of String,
						Dictionary(
							Of Integer,
							Dictionary(Of String, Tuple(Of Decimal, String))
						)
					) = Me.GetDepreciationDataDict(si, dbParamInfos, parameterDict, brGlobals)
					
					'If entity has no depreciation data, set data buffer and return
					If Not entityDepreciationDataDict.ContainsKey(entityMemberName) Then
						api.Data.SetDataBuffer(targetDataBuffer, destinationInfo,,,,,,,,,,,,, True)
						Exit Sub
					End If
					Dim depreciationDataDict As Dictionary(Of Integer, Dictionary(Of String, Tuple(Of Decimal, String))) = entityDepreciationDataDict(entityMemberName)
					'Load depreciation data if month exists
					If depreciationDataDict.ContainsKey(month) Then
					    'Loop through each account to calculate
					    For Each accountToAmount In depreciationDataDict(month)
					        Dim dataCell As New DataBufferCell()
					        
					        ' Extraer account y costCenterGroup de la clave compuesta
					        Dim keyParts() As String = accountToAmount.Key.Split("|"c)
					        Dim actualAccount As String = keyParts(0)
					        Dim actualCostCenterGroup As String = If(keyParts.Length > 1, keyParts(1), "None")
					        
					        ' Skip accounts that don't exist
					        If BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, actualAccount) = -1 Then 
					            Throw New Exception($"{actualAccount} doesn't exist")
					        End If
					            
					        ' Create and set data cell
					        dataCell.SetAccount(api, actualAccount)
					        dataCell.SetFlow(api, "None")
					        dataCell.SetOrigin(api, defaultOrigin)
					        dataCell.SetIC(api, defaultIC)
					        dataCell.SetUD1(api, defaultU1)
					        dataCell.SetUD2(api, defaultU2)
					        ' Usar el CostCenterGroup extraído
					        Dim costCenterGroup As String = If(actualCostCenterGroup = "None", defaultU3, actualCostCenterGroup)
					        dataCell.SetUD3(api, costCenterGroup)
					        dataCell.SetUD4(api, defaultU4)
					        dataCell.SetUD5(api, defaultU5)
					        dataCell.SetUD6(api, defaultU6)
					        dataCell.SetUD7(api, defaultU7)
					        dataCell.SetUD8(api, defaultU8)
					        dataCell.CellAmount = accountToAmount.Value.Item1
					        dataCell.CellStatus = New DataCellStatus(DataCellExistenceType.IsRealData, DataCellStorageType.Input)
					        
					        targetDataBuffer.SetCell(si, dataCell, True)
					    Next
					End If
					
					api.Data.SetDataBuffer(targetDataBuffer, destinationInfo,,,,,,,,,,,,, True)
					
				#End Region
					
				#Region "Clear Sent Data"
				
				Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("ClearSentData") Then
					
					' Get and clear data buffer
					Dim dataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(A#All, U4#CAPEXRD, U4#INV_CAPEX, U4#AMORT)",,False)
					UTISharedFunctionsBR.ClearDataBuffer(si, api, dataBuffer)
					
					#End Region
					
				End If
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Sub

		#Region "Helper Functions"
		
		#Region "Get Capitalization Data Dict"
		
			Private Function GetCapitalizationDataDict(
			    ByVal si As SessionInfo, ByRef dbParamInfos As List(Of DbParamInfo),
			    ByRef parameterDict As Dictionary(Of String, String), ByRef brGlobals As BRGlobals
			    ) As Dictionary(Of Integer, Dictionary(Of String, Decimal))
			    
			    'Check if the global variable exists
			    If brGlobals.GetObject("capitalizationDataDict") IsNot Nothing
			        Return brGlobals.GetObject("capitalizationDataDict")
			    Else
			        'Lock brGlobals
			        SyncLock brGlobals
			        If brGlobals.GetObject("capitalizationDataDict") IsNot Nothing Then Return brGlobals.GetObject("capitalizationDataDict")
			        'Declare dictionary and populate it with an empty dictionary per month
			        Dim dataDictionary As New Dictionary(Of Integer, Dictionary(Of String, Decimal))
			        For i As Integer = 1 To 12
			            dataDictionary(i) = New Dictionary(Of String, Decimal)
			        Next
			        'Get monthly capitalization data
			        Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			            Dim dt As DataTable = BRApi.Database.ExecuteSql(
			                dbConn,
			                "
			                    WITH recursive_months AS (
			                        SELECT 1 AS month
			                        UNION ALL
			                        SELECT month + 1 AS month
			                        FROM recursive_months
			                        WHERE month < 12
			                    ), filtered_project AS (
			                        -- Get projects for the selected company
			                        SELECT project
			                        FROM XFC_INV_MASTER_project WITH(NOLOCK)
			                        WHERE company_id = @paramCompany
			                    ), filtered_asset_by_project AS (
			                        SELECT
			                            a.id AS asset_id, a.project_id, a.cost_center_id, a.initial_value
			                        FROM XFC_INV_MASTER_asset a WITH(NOLOCK)
			                        WHERE EXISTS (
			                            SELECT 1
			                            FROM filtered_project fp
			                            WHERE a.project_id = fp.project
			                        )
			                    ), asset_with_dynamic_mapping AS (
			                        -- Implementar lógica de mapeo dinámico para obtener asset_account
			                        SELECT
			                            abp.project_id,
			                            -- Asset account usando lógica de mapeo dinámico
			                            COALESCE(
			                                new_cc_direct_type.asset_account,      -- Si es cost center directo new
			                                new_cc_mapped_type.asset_account,      -- Si tiene mapeo válido
			                                original_cc_type.asset_account         -- Si es cost center original
			                            ) AS asset_account,
			                            abp.initial_value
			                        FROM filtered_asset_by_project abp
			                        
			                        -- 1. Verificar si el cost_center ya es 'new'
			                        LEFT JOIN XFC_MAIN_MASTER_costCenters new_cc_direct ON abp.cost_center_id = new_cc_direct.id
			                        
			                        -- 2. Buscar mapeo old->new
			                        LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew mapping ON abp.cost_center_id = mapping.id_old
			                        LEFT JOIN XFC_MAIN_MASTER_costCenters new_cc_mapped ON mapping.id = new_cc_mapped.id
			                            AND (new_cc_mapped.end_date IS NULL OR new_cc_mapped.end_date >= GETDATE())
			                        
			                        -- 3. Información del cost center original
			                        LEFT JOIN XFC_INV_MASTER_Cost_Center inv_cc ON abp.cost_center_id = inv_cc.id
			                        
			                        -- 4. Información de tipos para asset_account
			                        -- Tipo para cost center directo new
			                        LEFT JOIN XFC_INV_MASTER_Cost_center_type new_cc_direct_type ON new_cc_direct.class_id = new_cc_direct_type.id
			                        -- Tipo para cost center mapeado
			                        LEFT JOIN XFC_INV_MASTER_Cost_center_type new_cc_mapped_type ON new_cc_mapped.class_id = new_cc_mapped_type.id
			                        -- Tipo para cost center original  
			                        LEFT JOIN XFC_INV_MASTER_Cost_center_type original_cc_type ON inv_cc.cost_center_type = original_cc_type.id
			                        
			                        -- Filtrar solo donde tengamos asset_account
			                        WHERE COALESCE(
			                            new_cc_direct_type.asset_account,
			                            new_cc_mapped_type.asset_account, 
			                            original_cc_type.asset_account
			                        ) IS NOT NULL
			                    ), ordered_filtered_asset AS (
			                        -- Order assets based on initial value to get the first capitalization account
			                        SELECT
			                            project_id,
			                            asset_account,
			                            ROW_NUMBER() OVER (
			                                PARTITION BY project_id
			                                ORDER BY initial_value DESC
			                            ) AS rn
			                        FROM asset_with_dynamic_mapping
			                    ), project_account AS (
			                        -- Get first asset account
			                        SELECT project_id, asset_account
			                        FROM ordered_filtered_asset
			                        WHERE rn = 1
			                    ), month_project AS (
			                        SELECT month, project_id
			                        FROM project_account
			                        CROSS JOIN recursive_months
			                    )
			                
			                    SELECT
			                        month,
			                        account,
			                        COALESCE(
			                            dbo.TranslateAmount(
			                                @paramYear,
			                                month,
			                                @paramRateTypeRevenueExp,
			                                'EUR',
			                                @paramCurrency,
			                                amount
			                            ) / 1000,
			                            0
			                        ) AS amount
			                    FROM (
			                        SELECT DISTINCT
			                            pc.month,
			                            pa.asset_account AS account,
			                            SUM(pc.amount) OVER(
			                                PARTITION BY pa.asset_account
			                                ORDER BY pc.month ASC
			                            ) AS amount
			                        FROM (
			                            SELECT 
			                                COALESCE(pcinner.project_id, mp.project_id) AS project_id,
			                                COALESCE(pcinner.month, mp.month) AS month,
			                                COALESCE(pcinner.amount, 0) AS amount
			                            FROM (
			                                SELECT project_id, month, amount
			                                FROM XFC_INV_FACT_project_capitalization WITH(NOLOCK)
			                                WHERE year = @paramYear
			                                    AND scenario = @paramScenario
			                                    AND month > @paramForecastMonth
			                            ) pcinner
			                            FULL OUTER JOIN month_project mp
			                            ON pcinner.project_id = mp.project_id
			                                AND pcinner.month = mp.month
			                        ) pc
			                        INNER JOIN project_account pa ON pc.project_id = pa.project_id
			                    ) AS subquery
			                    ORDER BY month, account
			                ",
			                dbParamInfos,
			                False
			            )
			            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
			                For Each row In dt.Rows
			                    'Add the account and amount
			                    dataDictionary(row("month"))(row("account")) = row("amount")
			                Next
			            End If
			        End Using
			        'Set global variable and return
			        brGlobals.SetObject("capitalizationDataDict", dataDictionary)
			        End SyncLock
			        Return brGlobals.GetObject("capitalizationDataDict")
			    End If
			End Function
		
		#End Region
		
		#Region "Get Cash Data Dict"
		
		Private Function GetCashDataDict(
			ByVal si As SessionInfo, ByRef dbParamInfos As List(Of DbParamInfo),
			ByRef parameterDict As Dictionary(Of String, String), ByRef brGlobals As BRGlobals
			) As Dictionary(Of Integer, Decimal)
			'Check if the global variable exists
			If brGlobals.GetObject("cashDataDict") IsNot Nothing
				Return brGlobals.GetObject("cashDataDict")
			Else
				'Lock brGlobals
				SyncLock brGlobals
				If brGlobals.GetObject("cashDataDict") IsNot Nothing Then Return brGlobals.GetObject("cashDataDict")
				'Declare dictionary
				Dim dataDictionary As New Dictionary(Of Integer, Decimal)
				'Get monthly capitalization data
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim dt As DataTable = BRApi.Database.ExecuteSql(
						dbConn,
						"
							WITH project_filtered AS (
								-- Filter project by company
								SELECT project
								FROM XFC_INV_MASTER_project WITH(NOLOCK)
								WHERE company_id = @paramCompany
							)
						
							SELECT
								month,
								COALESCE(
									dbo.TranslateAmount(
										@paramYear,
										month,
										@paramRateTypeRevenueExp,
										'EUR',
										@paramCurrency,
										amount
									) / 1000,
									0
								) AS amount
							FROM (
								SELECT DISTINCT
									pc.month,
									SUM(pc.amount) OVER(
										ORDER BY pc.month ASC
									) AS amount
								FROM XFC_INV_FACT_project_cash pc WITH(NOLOCK)
								WHERE year = @paramYear
									AND scenario = @paramScenario
									AND month > @paramForecastMonth
									AND EXISTS (
										SELECT 1
										FROM project_filtered pf
										WHERE pf.project = pc.project_id
									)
							) AS subquery;
						",
						dbParamInfos,
						False
					)
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						For Each row In dt.Rows
							dataDictionary(row("month")) = - row("amount")
						Next
					End If
				End Using
				'Set global variable and return
				brGlobals.SetObject("cashDataDict", dataDictionary)
				End SyncLock
				Return brGlobals.GetObject("cashDataDict")
			End If
		End Function
		
		#End Region
		
		#Region "Get Supplier Data Dict"
		
		Private Function GetSupplierDataDict(
			ByVal si As SessionInfo, ByRef dbParamInfos As List(Of DbParamInfo),
			ByRef parameterDict As Dictionary(Of String, String), ByRef brGlobals As BRGlobals
			) As Dictionary(Of Integer, Decimal)
			'Check if the global variable exists
			If brGlobals.GetObject("supplierDataDict") IsNot Nothing
				Return brGlobals.GetObject("supplierDataDict")
			Else
				'Lock brGlobals
				SyncLock brGlobals
				If brGlobals.GetObject("supplierDataDict") IsNot Nothing Then Return brGlobals.GetObject("supplierDataDict")
				'Declare dictionary
				Dim dataDictionary As New Dictionary(Of Integer, Decimal)
				'Get monthly supplier data
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim dt As DataTable = BRApi.Database.ExecuteSql(
						dbConn,
						"
							WITH project_filtered AS (
								-- Filter project by company
								SELECT project
								FROM XFC_INV_MASTER_project WITH(NOLOCK)
								WHERE company_id = @paramCompany
							)
						
							SELECT
								month,
								COALESCE(
									dbo.TranslateAmount(
										@paramYear,
										month,
										@paramRateTypeRevenueExp,
										'EUR',
										@paramCurrency,
										amount
									) / 1000,
									0
								) AS amount
							FROM (
								SELECT
									COALESCE(pcap.month, pcash.month) AS month,
									SUM(COALESCE(pcap.amount, 0) - COALESCE(pcash.amount, 0)) OVER(
										ORDER BY COALESCE(pcap.month, pcash.month) ASC
									) AS amount
								FROM (
									SELECT pc.month, SUM(pc.amount) AS amount
									FROM XFC_INV_FACT_project_capitalization pc WITH(NOLOCK)
									WHERE pc.year = @paramYear
										AND pc.scenario = @paramScenario
										AND pc.month > @paramForecastMonth
										AND EXISTS (
											SELECT 1
											FROM project_filtered pf
											WHERE pc.project_id = pf.project
										)
									GROUP BY pc.month
								) AS pcap
								FULL JOIN (
									SELECT pc.month, SUM(pc.amount) AS amount
									FROM XFC_INV_FACT_project_cash pc WITH(NOLOCK)
									WHERE pc.year = @paramYear
										AND pc.scenario = @paramScenario
										AND pc.month > @paramForecastMonth
										AND EXISTS (
											SELECT 1
											FROM project_filtered pf
											WHERE pc.project_id = pf.project
										)
									GROUP BY pc.month
								) AS pcash
								ON pcap.month = pcash.month
							) AS subquery
						",
						dbParamInfos,
						False
					)
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						For Each row In dt.Rows
							dataDictionary(row("month")) = row("amount")
						Next
					End If
				End Using
				'Set global variable and return
				brGlobals.SetObject("supplierDataDict", dataDictionary)
				End SyncLock
				Return brGlobals.GetObject("supplierDataDict")
			End If
		End Function
		
		#End Region
		
		#Region "Get Depreciation Data Dict"
		
		Private Function GetDepreciationDataDict(
		    ByVal si As SessionInfo, ByRef dbParamInfos As List(Of DbParamInfo),
		    ByRef parameterDict As Dictionary(Of String, String), ByRef brGlobals As BRGlobals
		    ) As Dictionary(
		        Of String,
		        Dictionary(
		            Of Integer, Dictionary(Of String, Tuple(Of Decimal, String))
		        )
		    )
		    'Check if the global variable exists
		    If brGlobals.GetObject("depreciationDataDict") IsNot Nothing
		        Return brGlobals.GetObject("depreciationDataDict")
		    Else
		        ' Lock brGlobals
		        SyncLock brGlobals
		        If brGlobals.GetObject("depreciationDataDict") IsNot Nothing Then Return brGlobals.GetObject("depreciationDataDict")
		        'Declare dictionary
		        Dim dataDictionary As New Dictionary(
		            Of String,
		            Dictionary(
		                Of Integer, Dictionary(Of String, Tuple(Of Decimal, String))
		            )
		        )
		        'Get monthly depreciation data
		        Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
		            Dim dt As DataTable = BRApi.Database.ExecuteSql(
		                dbConn,
		                "
		                    WITH project_filetered AS (
		                        -- Filter project by company
		                        SELECT project
		                        FROM XFC_INV_MASTER_project WITH(NOLOCK)
		                        WHERE company_id = @paramCompany
		                    ), asset_filtered AS (
		                        -- Filter assets by company, using DYNAMIC cost center mapping logic
		                        -- CORRECCIÓN FINAL: Entity member + CostCenterGroup con mapeo inverso
		                        SELECT af.asset_id, af.project_id, 
		                               -- Entity member con mapeo inverso para cost centers nuevos
		                               COALESCE(
		                                   inv_cc_from_reverse_mapping.entity_member,
		                                   inv_cc_direct.entity_member
		                               ) AS entity_member,
		                               -- Depreciation account con lógica dinámica
		                               cc_type.depreciation_account AS account, 
		                               -- ⭐ COST CENTER GROUP CON LÓGICA DINÁMICA
		                               COALESCE(
		                                   -- Para cost centers ""nuevos"": usar Cost Center Group del id_old
		                                   ccg_from_old.CostCenterGroup,
		                                   -- Para cost centers originales: usar Cost Center Group directo  
		                                   ccg_direct.CostCenterGroup,
		                                   'None'
		                               ) AS CostCenterGroup
		                        FROM (
		                            SELECT a.id AS asset_id, a.project_id, a.cost_center_id
		                            FROM XFC_INV_MASTER_asset a WITH(NOLOCK)
		                            LEFT JOIN XFC_INV_AUX_fictitious_assets fa 
		                                ON a.id = fa.asset_id 
		                                AND a.project_id = fa.project_id 
		                                AND fa.scenario = @paramScenario
		                                AND fa.year = @paramYear
		                            WHERE EXISTS (
		                                SELECT 1
		                                FROM project_filetered p
		                                WHERE a.project_id = p.project
		                            )
		                            AND (a.is_real = 1 OR (a.is_real = 0 AND fa.asset_id IS NOT NULL))
		                        ) af
		                        
		                        -- 1. Verificar si el cost_center ya es ""new""
		                        LEFT JOIN XFC_MAIN_MASTER_costCenters new_cc_direct ON af.cost_center_id = new_cc_direct.id
		                        
		                        -- 2. Mapeo INVERSO para cost centers ""nuevos"" (new → old → entity_member)
		                        LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew reverse_mapping 
		                            ON af.cost_center_id = reverse_mapping.id
		                        LEFT JOIN XFC_INV_MASTER_Cost_Center inv_cc_from_reverse_mapping 
		                            ON reverse_mapping.id_old = inv_cc_from_reverse_mapping.id
		                        
		                        -- ⭐ NUEVO: Cost Center Group desde el id_old para cost centers ""nuevos""
		                        LEFT JOIN XFC_MAIN_MASTER_Cost_Center_Group ccg_from_old 
		                            ON reverse_mapping.id_old = ccg_from_old.CostCenter
		                        
		                        -- 3. Mapeo tradicional old→new (para otros casos)
		                        LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew mapping ON af.cost_center_id = mapping.id_old
		                        LEFT JOIN XFC_MAIN_MASTER_costCenters new_cc_mapped ON mapping.id = new_cc_mapped.id
		                        
		                        -- 4. Información del cost center original directo
		                        LEFT JOIN XFC_INV_MASTER_Cost_Center inv_cc_direct ON af.cost_center_id = inv_cc_direct.id
		                        
		                        -- ⭐ Cost Center Group directo (para cost centers originales)
		                        LEFT JOIN XFC_MAIN_MASTER_Cost_Center_Group ccg_direct 
		                            ON af.cost_center_id = ccg_direct.CostCenter
		                        
		                        -- 5. Información de tipos (depreciation_account, asset_account)
		                        LEFT JOIN XFC_INV_MASTER_Cost_center_type cc_type ON 
		                            COALESCE(
		                                new_cc_direct.class_id,
		                                new_cc_mapped.class_id,
		                                inv_cc_direct.cost_center_type
		                            ) = cc_type.id
		                        
		                        -- FILTROS
		                        WHERE cc_type.depreciation_account IS NOT NULL
		                            AND (
		                                new_cc_direct.id IS NOT NULL
		                                OR mapping.id IS NULL
		                                OR (mapping.id IS NOT NULL
		                                    AND (new_cc_mapped.end_date IS NULL OR new_cc_mapped.end_date >= GETDATE()))
		                            )
		                    )
		
		                    SELECT
		                        entity_member, month, account, CostCenterGroup,
		                        COALESCE(
		                            dbo.TranslateAmount(
		                                @paramYear,
		                                month,
		                                @paramRateTypeRevenueExp,
		                                'EUR',
		                                @paramCurrency,
		                                amount
		                            ) / 1000,
		                            0
		                        ) AS amount
		                    FROM (
		                        SELECT DISTINCT
		                            af.entity_member,
		                            ad.month AS month,
		                            af.account,
		                            af.CostCenterGroup,
		                            SUM(ad.amount) OVER(
		                                PARTITION BY
		                                    af.entity_member,
		                                    af.account,
		                                    af.CostCenterGroup
		                                ORDER BY ad.month ASC
		                            ) AS amount
		                        FROM (
		                            SELECT *
		                            FROM XFC_INV_FACT_asset_depreciation ad WITH(NOLOCK)
		                            WHERE year = @paramYear
		                                AND scenario = @paramScenario
		                                AND month > @paramForecastMonth
		                        ) ad
		                        INNER JOIN asset_filtered af
		                        ON ad.asset_id = af.asset_id
		                            AND ad.project_id = af.project_id
		                    ) AS subquery
		                    ORDER BY entity_member, month, account
		                ",
		                dbParamInfos,
		                False
		            )
		            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
		                For Each row In dt.Rows
		                    'If entity member has not been added before, add a dictionary first
		                    Dim entityMember = Trim(row("entity_member").ToString)
		                    Dim accountMember = Trim(row("account").ToString)
		                    Dim costCenterGroup = If(IsDBNull(row("CostCenterGroup")), "None", Trim(row("CostCenterGroup").ToString))
		                    
		                    ' Validación: No procesar si entity_member es NULL o vacío
		                    If String.IsNullOrEmpty(entityMember) Then
		                        Continue For
		                    End If
		                    
		                    If Not dataDictionary.ContainsKey(entityMember) Then 
		                        dataDictionary(entityMember) = New Dictionary(Of Integer, Dictionary(Of String, Tuple(Of Decimal, String)))
		                    End If
		                    
		                    'If month has not been added before, add a dictionary first
		                    If Not dataDictionary(entityMember).ContainsKey(row("month")) Then 
		                        dataDictionary(entityMember)(row("month")) = New Dictionary(Of String, Tuple(Of Decimal, String))
		                    End If
		                    
		                    ' Usar clave compuesta Account|CostCenterGroup
		                    Dim compositeKey As String = accountMember & "|" & costCenterGroup
		                    dataDictionary(entityMember)(row("month")).Add(compositeKey, New Tuple(Of Decimal, String)(-row("amount"), costCenterGroup))
		                Next
		            End If
		        End Using
		        'Set global variable and return
		        brGlobals.SetObject("depreciationDataDict", dataDictionary)
		        End SyncLock
		        Return brGlobals.GetObject("depreciationDataDict")
		    End If
		End Function
					
		#End Region
		
		#End Region
		
	End Class
End Namespace