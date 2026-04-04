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
					Dim entityCurrency As String = api.Entity.GetLocalCurrency(api.Pov.Entity.MemberId).Name
					dbParamInfos.Add(New DbParamInfo("paramCurrency", entityCurrency))
					Dim RateTypeRevenueExp As String = api.FxRates.GetFxRateTypeForRevenueExp(,api.Pov.Scenario.MemberId).Name
					dbParamInfos.Add(New DbParamInfo("paramRateTypeRevenueExp", RateTypeRevenueExp))
					Dim RateTypeAssetLiability As String = api.FxRates.GetFxRateTypeForAssetLiability(,api.Pov.Scenario.MemberId).Name
					dbParamInfos.Add(New DbParamInfo("paramRateTypeAssetLiab", RateTypeAssetLiability))
					Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
					dbParamInfos.Add(New DbParamInfo("paramForecastMonth", paramForecastMonth))
					Dim month As Integer = api.Pov.Time.Name.Split("M")(1)
					
					'Get global variables for the data to be uploaded
					Dim capitalizationDataDict As Dictionary(Of Integer, Dictionary(Of String, Decimal)) = Me.GetCapitalizationDataDict(si, dbParamInfos, brGlobals)
					Dim cashDataDict As Dictionary(Of Integer, Decimal) = Me.GetCashDataDict(si, dbParamInfos, brGlobals)
					Dim supplierDataDict As Dictionary(Of Integer, Decimal) = Me.GetSupplierDataDict(si, dbParamInfos, brGlobals)
					Dim depreciationDataDict As Dictionary(Of Integer, Dictionary(Of String, Decimal)) = Me.GetDepreciationDataDict(si, dbParamInfos, brGlobals)
					Dim depreciationAccountList As List(Of String) = Me.GetDepreciationAccounts(si, brGlobals)
					Dim capitalizationAccountList As List(Of String) = Me.GetCapitalizationAccounts(si, brGlobals)
					
					'CAPITALIZATION
					'Clear capitalization accounts
					For Each capitalizationAccount In capitalizationAccountList
						api.Data.ClearCalculatedData($"A#{capitalizationAccount}:F#None:U4#CAPEXRD",True,True,True,True)
					Next
					'Load depreciation data if month exists
					If capitalizationDataDict.ContainsKey(month) Then
						'Loop through each account to calculate
						For Each accountToAmount In capitalizationDataDict(month)
							api.Data.FormulaVariables.SetDecimalVariable("capitalizationAmount", accountToAmount.Value)
							api.Data.Calculate($"V#YTD:A#{accountToAmount.Key}:F#F20:O#Import:I#None:U1#None:U2#None:U3#None:U4#CAPEXRD:U5#None:U6#None:U7#None:U8#None =
								$capitalizationAmount", True
							)
						Next
					End If
					
					'CASH
					api.Data.ClearCalculatedData("A#1681000CF:U4#CAPEXRD",True,True,True,True)
					'Load cash data if month exists
					If cashDataDict.ContainsKey(month) Then
						Dim amount As Decimal = cashDataDict(month)
						api.Data.FormulaVariables.SetDecimalVariable("cashAmount", amount)
						'Check the difference with the last month
						Dim isIncrease As Boolean = If(
							cashDataDict.ContainsKey(month - 1),
							(amount - cashDataDict(month - 1)) >= 0,
							amount >= 0)
						If isIncrease Then
							api.Data.Calculate($"V#YTD:A#1681000CF:F#F30:O#Import:I#None:U1#None:U2#None:U3#None:U4#CAPEXRD:U5#None:U6#None:U7#None:U8#None =
								$cashAmount", True
							)
						Else
							api.Data.Calculate($"V#YTD:A#1681000CF:F#F30:O#Import:I#None:U1#None:U2#None:U3#None:U4#CAPEXRD:U5#None:U6#None:U7#None:U8#None =
								$cashAmount", True
							)
						End If
					End If
					
					'SUPPLIER
					api.Data.ClearCalculatedData("A#2701000C:U4#CAPEXRD",True,True,True,True)
					'Load supplier data if month exists
					If supplierDataDict.ContainsKey(month) Then
						Dim amount As Decimal = supplierDataDict(month)
						api.Data.FormulaVariables.SetDecimalVariable("supplierAmount", amount)
						'Check the difference with the last month
						Dim isIncrease As Boolean = If(
							cashDataDict.ContainsKey(month - 1),
							(amount - cashDataDict(month - 1)) >= 0,
							amount >= 0)
						If isIncrease Then
							api.Data.Calculate($"V#YTD:A#2701000C:F#F20:O#Import:I#None:U1#None:U2#None:U3#None:U4#CAPEXRD:U5#None:U6#None:U7#None:U8#None =
								$supplierAmount", True
							)
						Else
							api.Data.Calculate($"V#YTD:A#2701000C:F#F20:O#Import:I#None:U1#None:U2#None:U3#None:U4#CAPEXRD:U5#None:U6#None:U7#None:U8#None =
								$supplierAmount", True
							)
						End If
					End If
					
					'DEPRECIATION
					'Clear depreciation accounts
					api.Data.ClearCalculatedData($"A#551107:F#None:U4#CAPEXRD",True,True,True,True)
					For Each depreciationAccount In depreciationAccountList
						api.Data.ClearCalculatedData($"A#{depreciationAccount}:F#None:U4#CAPEXRD",True,True,True,True)
					Next
					'Load depreciation data if month exists
					If depreciationDataDict.ContainsKey(month) Then
						'Loop through each account to calculate
						For Each accountToAmount In depreciationDataDict(month)
							api.Data.FormulaVariables.SetDecimalVariable("depreciationAmount", accountToAmount.Value)
							api.Data.Calculate($"V#YTD:A#{accountToAmount.Key}:F#None:O#Import:I#None:U1#None:U2#None:U3#None:U4#CAPEXRD:U5#None:U6#None:U7#None:U8#None =
								$depreciationAmount", True
							)
						Next
					End If
					
				#End Region
					
				#Region "Clear Sent Data"
				
				Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("ClearSentData") Then
					'Get parameters
					Dim queryParams As String = args.CustomCalculateArgs.NameValuePairs("p_query_params")
					Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
					Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
					
					'Get global variables for the data to be cleared
					Dim depreciationAccountList As List(Of String) = Me.GetDepreciationAccounts(si, brGlobals)
					
					'CAPITALIZATION
					api.Data.ClearCalculatedData("A#1041000CF:U4#CAPEXRD",True,True,True,True)
					
					'CASH
					api.Data.ClearCalculatedData("A#1681000CF:U4#CAPEXRD",True,True,True,True)
					
					'SUPPLIER
					api.Data.ClearCalculatedData("A#2701000C:F#F20:U4#CAPEXRD",True,True,True,True)
					api.Data.ClearCalculatedData("A#2701000C:F#F30:U4#CAPEXRD",True,True,True,True)
					
					'DEPRECIATION
					'Clear depreciation accounts
					api.Data.ClearCalculatedData($"A#551107:F#None:U4#CAPEXRD",True,True,True,True)
					For Each depreciationAccount In depreciationAccountList
						api.Data.ClearCalculatedData($"A#{depreciationAccount}:F#None:U4#CAPEXRD",True,True,True,True)
					Next
					
					#End Region
					
				End If
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Sub

		#Region "Helper Functions"
		
		#Region "Get Capitalization Data Dict"
		
		Private Function GetCapitalizationDataDict(ByVal si As SessionInfo, ByRef dbParamInfos As List(Of DbParamInfo), ByRef brGlobals As BRGlobals) As Dictionary(Of Integer, Dictionary(Of String, Decimal))
			'Check if the global variable exists
			If brGlobals.GetObject("capitalizationDataDict") IsNot Nothing
				Return brGlobals.GetObject("capitalizationDataDict")
			Else
				'Declare dictionary
				Dim dataDictionary As New Dictionary(Of Integer, Dictionary(Of String, Decimal))
				'Get monthly capitalization data
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim dt As DataTable = BRApi.Database.ExecuteSql(
						dbConn,
						"
							WITH ordered_asset AS (
								SELECT
									a.project_id,
									ac.asset_account,
									ROW_NUMBER() OVER (
										PARTITION BY a.project_id
										ORDER BY a.initial_value DESC
									) AS rn
								FROM XFC_INV_MASTER_asset a
								INNER JOIN XFC_INV_MASTER_asset_category ac ON a.category_id = ac.id
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
									COALESCE(
										oa.asset_account,
										'1041000CF'
									) AS account,
									SUM(pc.amount) OVER(
										PARTITION BY COALESCE(
											oa.asset_account,
											'1041000CF'
										)
										ORDER BY pc.month ASC
									) AS amount
								FROM XFC_INV_FACT_project_capitalization pc
								INNER JOIN XFC_INV_MASTER_project p
								ON pc.project_id = p.project
								LEFT JOIN ordered_asset oa
								ON pc.project_id = oa.project_id
									AND oa.rn = 1
								WHERE p.company_id = @paramCompany
									AND pc.year = @paramYear
									AND pc.scenario = @paramScenario
									AND pc.month > @paramForecastMonth
							) AS subquery
						",
						dbParamInfos,
						False
					)
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						For Each row In dt.Rows
							'If month has not been added before, add a dictionary first
							If Not dataDictionary.ContainsKey(row("month")) Then dataDictionary(row("month")) = New Dictionary(Of String, Decimal)
							dataDictionary(row("month")).Add(row("account"), row("amount"))
						Next
					End If
				End Using
				'Set global variable and return
				brGlobals.SetObject("capitalizationDataDict", dataDictionary)
				Return dataDictionary
			End If
		End Function
		
		#End Region
		
		#Region "Get Cash Data Dict"
		
		Private Function GetCashDataDict(ByVal si As SessionInfo, ByRef dbParamInfos As List(Of DbParamInfo), ByRef brGlobals As BRGlobals) As Dictionary(Of Integer, Decimal)
			'Check if the global variable exists
			If brGlobals.GetObject("cashDataDict") IsNot Nothing
				Return brGlobals.GetObject("cashDataDict")
			Else
				'Declare dictionary
				Dim dataDictionary As New Dictionary(Of Integer, Decimal)
				'Get monthly capitalization data
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim dt As DataTable = BRApi.Database.ExecuteSql(
						dbConn,
						"
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
								FROM XFC_INV_FACT_project_cash pc
								INNER JOIN XFC_INV_MASTER_project p
								ON pc.project_id = p.project
								WHERE p.company_id = @paramCompany
									AND pc.year = @paramYear
									AND pc.scenario = @paramScenario
									AND pc.month > @paramForecastMonth
							) AS subquery
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
				Return dataDictionary
			End If
		End Function
		
		#End Region
		
		#Region "Get Supplier Data Dict"
		
		Private Function GetSupplierDataDict(ByVal si As SessionInfo, ByRef dbParamInfos As List(Of DbParamInfo), ByRef brGlobals As BRGlobals) As Dictionary(Of Integer, Decimal)
			'Check if the global variable exists
			If brGlobals.GetObject("supplierDataDict") IsNot Nothing
				Return brGlobals.GetObject("supplierDataDict")
			Else
				'Declare dictionary
				Dim dataDictionary As New Dictionary(Of Integer, Decimal)
				'Get monthly supplier data
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim dt As DataTable = BRApi.Database.ExecuteSql(
						dbConn,
						"
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
									FROM XFC_INV_FACT_project_capitalization pc
									INNER JOIN XFC_INV_MASTER_project p
									ON pc.project_id = p.project
									WHERE p.company_id = @paramCompany
										AND pc.year = @paramYear
										AND pc.scenario = @paramScenario
										AND pc.month > @paramForecastMonth
									GROUP BY pc.month
								) AS pcap
								FULL JOIN (
									SELECT pc.month, SUM(pc.amount) AS amount
									FROM XFC_INV_FACT_project_cash pc
									INNER JOIN XFC_INV_MASTER_project p
									ON pc.project_id = p.project
									WHERE p.company_id = @paramCompany
										AND pc.year = @paramYear
										AND pc.scenario = @paramScenario
										AND pc.month > @paramForecastMonth
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
				Return dataDictionary
			End If
		End Function
		
		#End Region
		
		#Region "Get Depreciation Data Dict"
		
		Private Function GetDepreciationDataDict(ByVal si As SessionInfo, ByRef dbParamInfos As List(Of DbParamInfo), ByRef brGlobals As BRGlobals) As Dictionary(Of Integer, Dictionary(Of String, Decimal))
			'Check if the global variable exists
			If brGlobals.GetObject("depreciationDataDict") IsNot Nothing
				Return brGlobals.GetObject("depreciationDataDict")
			Else
				'Define dictionary to map asset account to depreciation account
				Dim assetToDepreciationAccountDictionary As Dictionary(Of String, String) = Me.GetAssetToDepreciationAccountDictionary(si)
				'Declare dictionary
				Dim dataDictionary As New Dictionary(Of Integer, Dictionary(Of String, Decimal))
				'Get monthly depreciation data
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim dt As DataTable = BRApi.Database.ExecuteSql(
						dbConn,
						"
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
									ad.month AS month,
									ac.asset_account AS account,
									SUM(ad.amount) OVER(
										ORDER BY ad.month ASC
									) AS amount
								FROM XFC_INV_FACT_asset_depreciation ad
								INNER JOIN XFC_INV_MASTER_asset a
								ON ad.asset_id = a.id
								INNER JOIN XFC_INV_MASTER_asset_category ac
								ON a.category_id = ac.id
								INNER JOIN XFC_INV_MASTER_project p
								ON a.project_id = p.project
								WHERE p.company_id = @paramCompany
									AND ad.year = @paramYear
									AND ad.scenario = @paramScenario
									AND ad.month > @paramForecastMonth
							) AS subquery
						",
						dbParamInfos,
						False
					)
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						For Each row In dt.Rows
							'If month has not been added before, add a dictionary first
							If Not dataDictionary.ContainsKey(row("month")) Then dataDictionary(row("month")) = New Dictionary(Of String, Decimal)
							dataDictionary(row("month")).Add(assetToDepreciationAccountDictionary(row("account")), - row("amount"))
						Next
					End If
				End Using
				'Set global variable and return
				brGlobals.SetObject("depreciationDataDict", dataDictionary)
				Return dataDictionary
			End If
		End Function
		
		#End Region
		
		#Region "Get Depreciation Accounts"
		
		Private Function GetDepreciationAccounts(ByVal si As SessionInfo, ByRef brGlobals As BRGlobals) As List(Of String)
			'Check if the global variable exists
			If brGlobals.GetObject("depreciationAccountList") IsNot Nothing
				Return brGlobals.GetObject("depreciationAccountList")
			Else
				'Declare list and get accountss
				Dim dataList As New List(Of String)(Me.GetAssetToDepreciationAccountDictionary(si).Select(Function(item) item.Value))
				'Set global variable and return
				brGlobals.SetObject("depreciationAccountList", dataList)
				Return dataList
			End If
		End Function
		
		#End Region
		
		#Region "Get Capitalization Accounts"
		
		Private Function GetCapitalizationAccounts(ByVal si As SessionInfo, ByRef brGlobals As BRGlobals) As List(Of String)
			'Check if the global variable exists
			If brGlobals.GetObject("capitalizationAccountList") IsNot Nothing
				Return brGlobals.GetObject("capitalizationAccountList")
			Else
				'Declare list and get accountss
				Dim dataList As New List(Of String)(Me.GetAssetToDepreciationAccountDictionary(si).Select(Function(item) item.Key))
				'Set global variable and return
				brGlobals.SetObject("capitalizationAccountList", dataList)
				Return dataList
			End If
		End Function
		
		#End Region
		
		#Region "Get Asset to Depreciation Account Dictionary"
		
		Private Function GetAssetToDepreciationAccountDictionary(ByVal si As SessionInfo) As Dictionary(Of String, String)
			'Define dictionary to map asset account to depreciation account
			Return New Dictionary(Of String, String) From {
				{"1009900", "572107"},
				{"1021004C", "572115"},
				{"1039100", "563107"},
				{"1041000CF", "551107"},
				{"1501004CF", "555103R"}
			}
		End Function
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
