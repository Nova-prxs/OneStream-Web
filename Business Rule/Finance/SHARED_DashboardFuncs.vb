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

Namespace OneStream.BusinessRule.Finance.SHARED_DashboardFuncs
	Public Class MainClass
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
		
		#Region "Get Tab Checks"
		
		Public Function GetTabChecks(ByVal si As SessionInfo, ByVal scenario As String, ByVal year As String) As (Dictionary(Of String, String), Dictionary(Of String, String))
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				'Get the tab check information or create if it doesn't exist
				Dim selectQuery As String = $"
					IF NOT EXISTS (
						SELECT 1
						FROM XFC_Budget_DataDocumentation_Checks
						WHERE
							year = {year}
							AND scenario = '{scenario}'
					)
					BEGIN
						INSERT INTO XFC_Budget_DataDocumentation_Checks
							(year, scenario, tab, is_checked, is_underreview)
						VALUES
							({year}, '{scenario}', 'global_assumptions', 0, 0),
							({year}, '{scenario}', 'pl_vnp', 0, 0),
							({year}, '{scenario}', 'pl_dc', 0, 0),
							({year}, '{scenario}', 'bs_wcap', 0, 0),
							({year}, '{scenario}', 'capex_rnd', 0, 0),
							({year}, '{scenario}', 'bs_dc', 0, 0),
							({year}, '{scenario}', 'fte_dc', 0, 0),
							({year}, '{scenario}', 'fs', 0, 0)
					END
				
					SELECT tab, is_checked, is_underreview
					FROM XFC_Budget_DataDocumentation_Checks
					WHERE
						year = {year}
						AND scenario = '{scenario}'
				"
				Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
				'Convert that information into a dictionary to map each tab with its' is_checked value
				Dim tabCheckDictionary As New Dictionary(Of String, String)
				Dim tabUnderReviewDictionary As New Dictionary(Of String, String)
				For Each row As DataRow In dt.Rows
					tabCheckDictionary.Add(row("tab"), row("is_checked"))
					tabUnderReviewDictionary.Add(row("tab"), row("is_underreview"))
				Next
				
				Return (tabCheckDictionary, tabUnderReviewDictionary)
			End Using
		End Function
		
		#End Region
		
		#Region "Input Parameter Controller"
		
		Public Function InputParameterController(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal loadDashboardTaskResult As XFLoadDashboardTaskResult, ByVal parameterDict As Dictionary(Of String, String)) As XFLoadDashboardTaskResult
		
			'For loop through all the dictionary elements
			For Each parameterElement As KeyValuePair(Of String, String) In parameterDict
			
				'Manage modified input values, rest default
				If Not(args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(parameterElement.Key).Equals(parameterElement.Value)) Then
	            
		            loadDashboardTaskResult.ModifiedCustomSubstVars.Add(parameterElement.Key, args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(parameterElement.Key))
	
	          	Else
	
		            loadDashboardTaskResult.ModifiedCustomSubstVars.Add(parameterElement.Key, parameterElement.Value)
					
				End If
			
			Next
			
            Return loadDashboardTaskResult
		
		End Function
		
		#End Region		
		
	End Class
End Namespace