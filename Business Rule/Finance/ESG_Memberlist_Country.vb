Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace OneStream.BusinessRule.Finance.ESG_Memberlist
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.MemberList
'						Example: E#Root.CustomMemberList(BRName=ESG_Memberlist, MemberListName=[UD1List])
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("UD1List") Then
						
							Dim wfentity As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).GetAttribute(ScenarioTypeID.Sustainability, SharedConstants.WorkflowProfileAttributeIndexes.Text1).ToString
'							Brapi.ErrorLog.LogMessage(si, "ESG_Memberlist " & WFEntity)
'							Dim wfentity As String = "E#Root.WFProfileEntities"
'							Dim entitytxt2 As String = api.Entity.Text(2).Replace(" ","_")
							Dim ud1country As String = ""
							Dim ud1integer As Integer = api.Members.GetMemberId(dimtype.UD1.Id, wfentity)
							If ud1integer <> -1
								ud1country = $"UD1#{wfentity}.Base.Remove(LOC_{wfentity})"
							Else
								ud1country = "UD1#Root.Base"
							End If
							Dim dimpkUD1 As DimPk = api.Dimensions.GetDim("UD1_600_ESG").DimPk
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(dimpkUD1, ud1country, Nothing)
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)
							Return objMemberList
						End If
						
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
	End Class
End Namespace