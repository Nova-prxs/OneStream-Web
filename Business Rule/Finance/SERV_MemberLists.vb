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

Namespace OneStream.BusinessRule.Finance.SERV_MemberLists
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

						Dim MemberListstart As String = args.MemberListArgs.NameValuePairs("Filter").ToString
						
						'Order the Filter parameter in Alphabetical Order
						If args.MemberListArgs.MemberListName = "Alphabetical" Then						
		                    Dim objMemberListHeader = New MemberListHeader( _
															args.MemberListArgs.MemberListName)
							
		                    'Read the members
		                    Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter( _
                            							args.MemberListArgs.DimPk, MemberListstart, Nothing)
		                    'Sort the members
		                    Dim objMembers As List(Of Member) = Nothing
		                    If Not objMemberInfos Is Nothing Then
		                    	objMembers = (From memberInfo In objMemberInfos _
								              Order By memberInfo.Member.Name Ascending _
											  Select memberInfo.Member).ToList()
		                	End If
		                 							
		                    'Return
		                    Return New MemberList(objMemberListHeader, objMembers)
						
						'Order the Filter parameter in Reverse Alphabetical Order
						Else If args.MemberListArgs.MemberListName = "AlphabeticalR" Then						
		                    Dim objMemberListHeader = New MemberListHeader( _
															args.MemberListArgs.MemberListName)
															
		                    'Read the members
		                    Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter( _
                            							args.MemberListArgs.DimPk, MemberListstart, Nothing)
				
		                    'Sort the members
		                    Dim objMembers As List(Of Member) = Nothing
		                    If Not objMemberInfos Is Nothing Then
		                    	objMembers = (From memberInfo In objMemberInfos _
								              Order By memberInfo.Member.Name Descending _
											  Select memberInfo.Member).ToList()
		                End If
		                    
		                    'Return
		                    Return New MemberList(objMemberListHeader, objMembers)
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