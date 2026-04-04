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

Namespace OneStream.BusinessRule.Finance.Central_Rule
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
		
		
#Region "GetUltimatePCON"
		Private Function GetUltimatePCON(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal intIC As Integer, ByVal intTime As Integer)
			Dim entityDimPk As DimPk = api.Pov.EntityDim.DimPk
			Dim ASG_CY_CONS As String = "ASG_CY_CONS" 
			Dim scenarioTypeId As Integer = api.Pov.ScenarioTypeId
			Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId	

			Dim myScenarioTypeMember As Integer = api.Pov.ScenarioTypeId
			Dim LegalStructure As Integer = api.Members.GetMemberid(DimType.Entity.Id, ASG_CY_CONS)
			Dim boolLS As Boolean = True 
			Dim PConlist As New List(Of Decimal)
			Dim indirectper As Decimal = 0
			'Added For setup PCON, ConsMethod In seperate Cube.	
			Dim PCON As Decimal = 0
			Dim PCON1 As Decimal = 0
			Dim PCON2 As Decimal = 0
			Dim PCON3 As Decimal = 0
			Dim PCON4 As Decimal = 0
			Dim PCON5 As Decimal = 0
			Dim PCON6 As Decimal = 0
			Dim PCON7 As Decimal = 0
			Dim PCON8 As Decimal = 0
			Dim PCON9 As Decimal = 0
			Dim PCON10 As Decimal = 0
			Dim PCON11 As Decimal = 0
'			Dim Reclass As String = "RECLASS_FROM"
			'Getting list of all entities below legal
			Dim DescendantsofParentmemlist As List(Of Member) = api.Members.GetDescendants(entityDimPk,api.Pov.Parent.MemberId)
			Dim DescendantsofParentIDList As New List(Of Integer)
			For Each DescendantsofParentmem As Member In DescendantsofParentmemlist
				DescendantsofParentIDList.Add(DescendantsofParentmem.MemberId)
			Next
			If api.Pov.Entity.Name.XFEqualsIgnoreCase(ASG_CY_CONS)
				DescendantsofParentIDList.add(api.Pov.Entity.MemberId)
			Else
				DescendantsofParentIDList.add(api.Pov.Parent.MemberId)
			End If 
			'Getting the count of all the possible parents of the entity
			Dim ICCount As Integer = DescendantsofParentIDList.FindAll(Function(value As Integer) Value = intIC).Count
			Dim ICParentlist As List(Of Member) = api.Members.GetParents(entityDimPk, intIC, False)
			'Check if the entity has a parent and if the entity is a is descendant of LegalStructure
			If ICParentlist Is Nothing Or 
			ICParentlist.Count = 0 Or 
			Not api.Members.IsDescendant(entityDimPk, api.Pov.Parent.MemberId, intIC)
				boolLS = False
			End If
			If boolLS
				Dim ParentmemidList1 As New List(Of Integer)
				For Each possibleEntityParentmem1 As Member In ICParentlist
					If DescendantsofParentIDList.CONTAINS(possibleEntityParentmem1.MemberId)
						ParentmemidList1.Add(possibleEntityParentmem1.MemberId)
					End If
				Next
				PCONlist.Add(0)
				For Each ParentID1 As Integer In ParentmemidList1
					If api.Members.IsDescendant(api.Pov.EntityDim.DimPk, ParentID1, intIC)
						PCON1 = api.Entity.PercentConsolidation(intIC, ParentID1)
						PCON = PCON1 
						PConlist.add(PCON)
						ICCount = ICCount -1
						If ICCount = 0 Or pCon = 1
							Exit For
						End If
					Else
						Dim ParentmemMemList2 As List(Of Member) = api.Members.GetParents(entityDimPk, ParentID1, False)
						Dim ParentmemidList2 As New List(Of Integer)
						For Each ParentMem2 As Member In ParentmemMemList2
							If DescendantsofParentIDList.CONTAINS(ParentMem2.MemberId)
								ParentmemidList2.Add(ParentMem2.MemberId)
							End If
						Next
						For Each ParentID2 As Integer In ParentmemidList2
							If api.Members.IsDescendant(api.Pov.EntityDim.DimPk, ParentID2, intIC)
								PCON1 = api.Entity.PercentConsolidation(intIC, ParentID1)
								PCON2 = api.Entity.PercentConsolidation(ParentID1, ParentID2)
								PCON = (PCON1*PCON2)
								PConlist.add(PCON)

								ICCount = ICCount -1
								If ICCount = 0 Or 
								pCon = 1
									Exit For
								End If 
							Else
								Dim ParentmemMemList3 As List(Of Member) = api.Members.GetParents(entityDimPk, ParentID2, False)
								Dim ParentmemidList3 As New List(Of Integer)
								For Each ParentMem3 As Member In ParentmemMemList3
									If DescendantsofParentIDList.CONTAINS(ParentMem3.MemberId)
										ParentmemidList3.Add(ParentMem3.MemberId)
									End If
								Next
								For Each ParentID3 As Integer In ParentmemidList3
									If api.Members.IsDescendant(api.Pov.EntityDim.DimPk, ParentID3, intIC)
										PCON1 = api.Entity.PercentConsolidation(intIC, ParentID1)
										PCON2 = api.Entity.PercentConsolidation(ParentID1, ParentID2)
										PCON3 = api.Entity.PercentConsolidation(ParentID2, ParentID3)
										PCON = (PCON1*PCON2*PCON3)
										PConlist.add(PCON)
										ICCount = ICCount -1
										If ICCount = 0 Or 
										pCon = 1
											Exit For
										End If 
									Else
										Dim ParentmemMemList4 As List(Of Member) = api.Members.GetParents(entityDimPk, ParentID3, False)
										Dim ParentmemidList4 As New List(Of Integer)
										For Each ParentMem4 As Member In ParentmemMemList4
											If DescendantsofParentIDList.CONTAINS(ParentMem4.MemberId)
												ParentmemidList4.Add(ParentMem4.MemberId)
											End If
										Next
										For Each ParentID4 As Integer In ParentmemidList4
											If api.Members.IsDescendant(api.Pov.EntityDim.DimPk, ParentID4, intIC)
												PCON1 = api.Entity.PercentConsolidation(intIC, ParentID1)
												PCON2 = api.Entity.PercentConsolidation(ParentID1, ParentID2)
												PCON3 = api.Entity.PercentConsolidation(ParentID2, ParentID3)	
												PCON4 = api.Entity.PercentConsolidation(ParentID3, ParentID4)
												PCON = (PCON1*PCON2*PCON3*PCON4)
												PConlist.add(PCON)
												ICCount = ICCount -1
												If ICCount = 0 Or 
												pCon = 1
													Exit For
												End If 
											Else
												Dim ParentmemMemList5 As List(Of Member) = api.Members.GetParents(entityDimPk, ParentID4, False)
												Dim ParentmemidList5 As New List(Of Integer)
												For Each ParentMem5 As Member In ParentmemMemList5
													If DescendantsofParentIDList.CONTAINS(ParentMem5.MemberId)
														ParentmemidList5.Add(ParentMem5.MemberId)
													End If
												Next
												For Each ParentID5 As Integer In ParentmemidList5
													If api.Members.IsDescendant(api.Pov.EntityDim.DimPk, ParentID5, intIC)
														PCON1 = api.Entity.PercentConsolidation(intIC, ParentID1)
														PCON2 = api.Entity.PercentConsolidation(ParentID1, ParentID2)
														PCON3 = api.Entity.PercentConsolidation(ParentID2, ParentID3)	
														PCON4 = api.Entity.PercentConsolidation(ParentID3, ParentID4)
														PCON5 = api.Entity.PercentConsolidation(ParentID4, ParentID5)
														PCON = (PCON1*PCON2*PCON3*PCON4*PCON5)
														PConlist.add(PCON)
														ICCount = ICCount -1
														If ICCount = 0 Or 
														PCON = 1
															Exit For
														End If
														
													End If
												Next
											End If 
										Next
									End If 
								Next
							End If 
						Next 
					End If
				Next 
			End If
			If PConlist.Count>0
				PCON = PConlist.Max
			End If 
			Return PCON


		End Function
		#End Region
		
#Region "GetUltimatePCON_relationshiptxt"
		Private Function GetUltimatePCON_relationshiptxt(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal intIC As Integer, ByVal Reclass As String, ByVal intTime As Integer)
			Dim entityDimPk As DimPk = api.Pov.EntityDim.DimPk
			Dim ASG_CY_CONS As String = "ASG_CY_CONS" 'LegalStructure
'			Dim intIC As Integer = api.Pov.Entity.MemberId
			'Dim parentId As Integer = api.Pov.Parent.MemberId
			Dim scenarioTypeId As Integer = api.Pov.ScenarioTypeId
			Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId	

			Dim myScenarioTypeMember As Integer = api.Pov.ScenarioTypeId
			Dim LegalStructure As Integer = api.Members.GetMemberid(DimType.Entity.Id, ASG_CY_CONS)
			Dim boolLS As Boolean = True 
			Dim PConlist As New List(Of Decimal)
			Dim indirectper As Decimal = 0
			'Added For setup PCON, ConsMethod In seperate Cube.	
			Dim PCON As Decimal = 0
			Dim PCON1 As Decimal = 0
			Dim PCON2 As Decimal = 0
			Dim PCON3 As Decimal = 0
			Dim PCON4 As Decimal = 0
			Dim PCON5 As Decimal = 0
			Dim PCON6 As Decimal = 0
			Dim PCON7 As Decimal = 0
			Dim PCON8 As Decimal = 0
			Dim PCON9 As Decimal = 0
			Dim PCON10 As Decimal = 0
			Dim PCON11 As Decimal = 0
'			Dim Reclass As String = "RECLASS_FROM"
			'Getting list of all entities below legal
			Dim DescendantsofParentmemlist As List(Of Member) = api.Members.GetDescendants(entityDimPk,api.Pov.Parent.MemberId)
			Dim DescendantsofParentIDList As New List(Of Integer)
			For Each DescendantsofParentmem As Member In DescendantsofParentmemlist
				DescendantsofParentIDList.Add(DescendantsofParentmem.MemberId)
			Next
			If api.Pov.Entity.Name.XFEqualsIgnoreCase(ASG_CY_CONS)
				DescendantsofParentIDList.add(api.Pov.Entity.MemberId)
			Else
				DescendantsofParentIDList.add(api.Pov.Parent.MemberId)
			End If 
			'Getting the count of all the possible parents of the entity
			Dim ICCount As Integer = DescendantsofParentIDList.FindAll(Function(value As Integer) Value = intIC).Count
			Dim ICParentlist As List(Of Member) = api.Members.GetParents(entityDimPk, intIC, False)
			'Check if the entity has a parent and if the entity is a is descendant of LegalStructure
			If ICParentlist Is Nothing Or 
			ICParentlist.Count = 0 Or 
			Not api.Members.IsDescendant(entityDimPk, LegalStructure, intIC)
				boolLS = False
			End If
			If boolLS
				Dim ParentmemidList1 As New List(Of Integer)
				For Each possibleEntityParentmem1 As Member In ICParentlist
					If DescendantsofParentIDList.CONTAINS(possibleEntityParentmem1.MemberId)
						ParentmemidList1.Add(possibleEntityParentmem1.MemberId)
					End If
				Next
				PCONlist.Add(0)
				For Each ParentID1 As Integer In ParentmemidList1
					If api.Members.IsDescendant(api.Pov.EntityDim.DimPk, ParentID1, intIC)
						If api.Entity.RelationshipText(intIC, ParentID1, 3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON1 = 0 Else PCON1 = 1
						PCON = PCON1 
						PConlist.add(PCON)
						ICCount = ICCount -1
						If ICCount = 0 Or pCon = 1
							Exit For
						End If
					Else
						Dim ParentmemMemList2 As List(Of Member) = api.Members.GetParents(entityDimPk, ParentID1, False)
						Dim ParentmemidList2 As New List(Of Integer)
						For Each ParentMem2 As Member In ParentmemMemList2
							If DescendantsofParentIDList.CONTAINS(ParentMem2.MemberId)
								ParentmemidList2.Add(ParentMem2.MemberId)
							End If
						Next
						For Each ParentID2 As Integer In ParentmemidList2
							If api.Members.IsDescendant(api.Pov.EntityDim.DimPk, ParentID2, intIC)
								If api.Entity.RelationshipText(intIC, ParentID1, 3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON1 = 0 Else PCON1 = 1
								If api.Entity.RelationshipText(ParentID1, ParentID2, 3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON2 = 0 Else PCON2 = 1
								PCON = (PCON1*PCON2)
								PConlist.add(PCON)
								ICCount = ICCount -1
								If ICCount = 0 Or 
								pCon = 1
									Exit For
								End If 
							Else
								Dim ParentmemMemList3 As List(Of Member) = api.Members.GetParents(entityDimPk, ParentID2, False)
								Dim ParentmemidList3 As New List(Of Integer)
								For Each ParentMem3 As Member In ParentmemMemList3
									If DescendantsofParentIDList.CONTAINS(ParentMem3.MemberId)
										ParentmemidList3.Add(ParentMem3.MemberId)
									End If
								Next
								For Each ParentID3 As Integer In ParentmemidList3
									If api.Members.IsDescendant(api.Pov.EntityDim.DimPk, ParentID3, intIC)
										If api.Entity.RelationshipText(intIC, ParentID1, 3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON1 = 0 Else PCON1 = 1
										If api.Entity.RelationshipText(ParentID1, ParentID2, 3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON2 = 0 Else PCON2 = 1
										If api.Entity.RelationshipText(ParentID2, ParentID3, 3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON3 = 0 Else PCON3 = 1
										PCON = (PCON1*PCON2*PCON3)
										PConlist.add(PCON)
										ICCount = ICCount -1
										If ICCount = 0 Or 
										pCon = 1
											Exit For
										End If 
									Else
										Dim ParentmemMemList4 As List(Of Member) = api.Members.GetParents(entityDimPk, ParentID3, False)
										Dim ParentmemidList4 As New List(Of Integer)
										For Each ParentMem4 As Member In ParentmemMemList4
											If DescendantsofParentIDList.CONTAINS(ParentMem4.MemberId)
												ParentmemidList4.Add(ParentMem4.MemberId)
											End If
										Next
										For Each ParentID4 As Integer In ParentmemidList4
											If api.Members.IsDescendant(api.Pov.EntityDim.DimPk, ParentID4, intIC)
												If api.Entity.RelationshipText(intIC, ParentID1, 3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON1 = 0 Else PCON1 = 1
												If api.Entity.RelationshipText(ParentID1, ParentID2, 3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON2 = 0 Else PCON2 = 1
												If api.Entity.RelationshipText(ParentID2, ParentID3, 3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON3 = 0 Else PCON3 = 1
												If api.Entity.RelationshipText(ParentID3, ParentID4, 3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON4 = 0 Else PCON4 = 1
												PCON = (PCON1*PCON2*PCON3*PCON4)
												PConlist.add(PCON)
												ICCount = ICCount -1
												If ICCount = 0 Or 
												pCon = 1
													Exit For
												End If 
											Else
												Dim ParentmemMemList5 As List(Of Member) = api.Members.GetParents(entityDimPk, ParentID4, False)
												Dim ParentmemidList5 As New List(Of Integer)
												For Each ParentMem5 As Member In ParentmemMemList5
													If DescendantsofParentIDList.CONTAINS(ParentMem5.MemberId)
														ParentmemidList5.Add(ParentMem5.MemberId)
													End If
												Next
												For Each ParentID5 As Integer In ParentmemidList5
													If api.Members.IsDescendant(api.Pov.EntityDim.DimPk, ParentID5, intIC)
														If api.Entity.RelationshipText(intIC, ParentID1,3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON3 = 0 Else PCON3 = 1
														If api.Entity.RelationshipText(ParentID1, ParentID2,3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON3 = 0 Else PCON3 = 1
														If api.Entity.RelationshipText(ParentID2, ParentID3,3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON3 = 0 Else PCON3 = 1
														If api.Entity.RelationshipText(ParentID3, ParentID4,3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON4 = 0 Else PCON5 = 1
														If api.Entity.RelationshipText(ParentID4, ParentID5,3,,intTime).XFEqualsIgnoreCase(Reclass) Then PCON6 = 0 Else PCON6 = 1
														PCON = (PCON1*PCON2*PCON3*PCON4*PCON5)
														PConlist.add(PCON)
														ICCount = ICCount -1
														
														If ICCount = 0 Or 
														PCON = 1
															Exit For
														End If
														
													End If
												Next
											End If 
										Next
									End If 
								Next
							End If 
						Next 
					End If
				Next 
			End If
			If PConlist.Count>0
				PCON = PConlist.Max
			End If 
			Return PCON


		End Function
		#End Region

#Region "BufName"
Public Function BufName(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal cell As DataBufferCell, ByVal Dimension As String)
	Try
		Dim membername As String =""
		Select Case Dimension
		Case "A#" 
			membername = "A#" & api.Members.GetMemberName(DimType.Account.Id, cell.DataBufferCellPk.AccountId)
		Case "F#" 
			membername = "F#" & api.Members.GetMemberName(DimType.Flow.Id, cell.DataBufferCellPk.FlowId)
		Case "O#" 
			membername = "O#" & api.Members.GetMemberName(DimType.Origin.Id, cell.DataBufferCellPk.OriginId)
		Case "IC#" 
			membername = "I#" & api.Members.GetMemberName(DimType.IC.Id, cell.DataBufferCellPk.ICId)
		Case "I#" 
			membername = "I#" & api.Members.GetMemberName(DimType.IC.Id, cell.DataBufferCellPk.ICId)
		Case "U1#" 
			membername = "U1#" & api.Members.GetMemberName(DimType.UD1.Id, cell.DataBufferCellPk.UD1Id)
		Case "U2#" 
			membername = "U2#" & api.Members.GetMemberName(DimType.UD2.Id, cell.DataBufferCellPk.UD2Id)
		Case "U3#" 
			membername = "U3#" & api.Members.GetMemberName(DimType.UD3.Id, cell.DataBufferCellPk.UD3Id)
		Case "U4#" 
			membername = "U4#" & api.Members.GetMemberName(DimType.UD4.Id, cell.DataBufferCellPk.UD4Id)
		Case "U5#" 
			membername = "U5#" & api.Members.GetMemberName(DimType.UD5.Id, cell.DataBufferCellPk.UD5Id)
		Case "U6#" 
			membername = "U6#" & api.Members.GetMemberName(DimType.ud6.Id, cell.DataBufferCellPk.UD6Id)
		Case "U7#" 
			membername = "U7#" & api.Members.GetMemberName(DimType.UD7.Id, cell.DataBufferCellPk.UD7Id)
		Case "U8#" 
			membername = "U8#" & api.Members.GetMemberName(DimType.ud8.Id, cell.DataBufferCellPk.ud8id)
		Case "A" 
			membername = "A#" & api.Members.GetMemberName(DimType.Account.Id, cell.DataBufferCellPk.AccountId)
		Case "F" 
			membername = "F#" & api.Members.GetMemberName(DimType.Flow.Id, cell.DataBufferCellPk.FlowId)
		Case "O" 
			membername = "O#" & api.Members.GetMemberName(DimType.Origin.Id, cell.DataBufferCellPk.OriginId)
		Case "I" 
			membername = "I#" & api.Members.GetMemberName(DimType.IC.Id, cell.DataBufferCellPk.ICId)
		Case "IC" 
			membername = "I#" & api.Members.GetMemberName(DimType.IC.Id, cell.DataBufferCellPk.ICId)
		Case "U1" 
			membername = "U1#" & api.Members.GetMemberName(DimType.UD1.Id, cell.DataBufferCellPk.UD1Id)
		Case "U2" 
			membername = "U2#" & api.Members.GetMemberName(DimType.UD2.Id, cell.DataBufferCellPk.UD2Id)
		Case "U3" 
			membername = "U3#" & api.Members.GetMemberName(DimType.UD3.Id, cell.DataBufferCellPk.UD3Id)
		Case "U4" 
			membername = "U4#" & api.Members.GetMemberName(DimType.UD4.Id, cell.DataBufferCellPk.UD4Id)
		Case "U5" 
			membername = "U5#" & api.Members.GetMemberName(DimType.UD5.Id, cell.DataBufferCellPk.UD5Id)
		Case "U6" 
			membername = "U6#" & api.Members.GetMemberName(DimType.ud6.Id, cell.DataBufferCellPk.UD6Id)
		Case "U7" 
			membername = "U7#" & api.Members.GetMemberName(DimType.UD7.Id, cell.DataBufferCellPk.UD7Id)
		Case "U8" 
			membername = "U8#" & api.Members.GetMemberName(DimType.ud8.Id, cell.DataBufferCellPk.ud8id)		
		Case "UD1" 
			membername = "UD1#" & api.Members.GetMemberName(DimType.UD1.Id, cell.DataBufferCellPk.UD1Id)
		Case "UD2" 
			membername = "UD2#" & api.Members.GetMemberName(DimType.UD2.Id, cell.DataBufferCellPk.UD2Id)
		Case "UD3" 
			membername = "UD3#" & api.Members.GetMemberName(DimType.UD3.Id, cell.DataBufferCellPk.UD3Id)
		Case "UD4" 
			membername = "UD4#" & api.Members.GetMemberName(DimType.UD4.Id, cell.DataBufferCellPk.UD4Id)
		Case "UD5" 
			membername = "UD5#" & api.Members.GetMemberName(DimType.UD5.Id, cell.DataBufferCellPk.UD5Id)
		Case "UD6" 
			membername = "UD6#" & api.Members.GetMemberName(DimType.ud6.Id, cell.DataBufferCellPk.UD6Id)
		Case "UD7" 
			membername = "UD7#" & api.Members.GetMemberName(DimType.UD7.Id, cell.DataBufferCellPk.UD7Id)
		Case "UD8" 
			membername = "UD8#" & api.Members.GetMemberName(DimType.ud8.Id, cell.DataBufferCellPk.ud8id)	
		End Select
		Return membername
	Catch ex As Exception
	Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region

	End Class
End Namespace