Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database

Namespace OneStream.BusinessRule.Finance.Consolidate_V2
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					'The following consolidation methods are defined:
					'--- * Full = FULLCONSOLIDATION
					'--- * Holding = HOLDING
					'--- * Equity = EQUITY
					'--- * JV: relationshiptext1 = JV focus on the Joint venture with the consolidation to the Investments into associates accounts
					'--- * NCI = NONCONTROLLINGINTEREST
					'--- * Merged Entities = CUSTOM1
					'--- * Method or percentage change during the year = CUSTOM5
				
					Case Is = FinanceFunctionType.ConsolidateShare	
						Dim ImprovePerformance As Boolean = True
						#Region "ConsolidateShare"
						If Not api.Entity.InUse(api.Pov.Entity.MemberId, api.Pov.Scenario.MemberId, api.Pov.Time.MemberId) Then Exit Function
						api.Data.ClearCalculatedData(True, True, True, True)
						Dim Method As String = UCase(api.Entity.OwnershipType(api.Pov.Entity.MemberId,api.Pov.Parent.MemberId,api.Pov.Scenario.MemberId,api.Pov.Time.MemberId).ToString)
						Dim strEnt_Text1 As String = api.Entity.Text(1)
						Dim strEnt_Text3 As String = api.Entity.Text(3)
'						If Method.Equals("CUSTOM5", StringComparison.InvariantCultureIgnoreCase) And
'						strEnt_Text3.XFEqualsIgnoreCase("DISPOSED")	

''							ConsolidateDisposed(si, api)
'						ElseIf Method.Equals("EQUITY", StringComparison.InvariantCultureIgnoreCase) And
'						strEnt_Text1.XFEqualsIgnoreCase("JV")	
''							ConsolidateJV(si, api)
'						Else
							ConsolidateShareNormal(si, api, ImprovePerformance)
'						End If 
						#End Region
					Case Is = FinanceFunctionType.ConsolidateElimination
						If Not api.Entity.InUse(api.Pov.Entity.MemberId, api.Pov.Scenario.MemberId, api.Pov.Time.MemberId) Then Exit Function
						Dim onlyfrom2023dataelim As Boolean = True
						#Region "ConsolidateElimination"
						api.Data.ClearCalculatedData(True,True,True,True)
'						If onlyfrom2023dataelim 
'							If api.Time.GetYearFromId <2023
'								onlyfrom2023dataelim =True 'False
'							End If 
'						Else
'							onlyfrom2023dataelim = True
'						End If
						Dim Method As String = UCase(api.Entity.OwnershipType(api.Pov.Entity.MemberId,api.Pov.Parent.MemberId,api.Pov.Scenario.MemberId,api.Pov.Time.MemberId).ToString)
						Dim strEnt_Text1 As String = api.Entity.Text(1)
						Dim strEnt_Text3 As String = api.Entity.Text(3)
						If Not Method.Equals("CUSTOM5", StringComparison.InvariantCultureIgnoreCase) And
						Not strEnt_Text3.Equals("DISPOSED", StringComparison.InvariantCultureIgnoreCase)
							IntercompanyElimination(si, api, onlyfrom2023dataelim)
						End If

						#End Region
					End Select
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "ConsolidateShareNormal"
		Private Sub ConsolidateShareNormal(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal ImprovePerformance As Boolean)
			Try
		' 	Clear all previously calculated data from the Share consolidation member.
			Dim myTimeMember As Integer = api.Pov.Time.MemberPk.MemberId
			Dim myTimeMemberLY As Integer = api.Time.GetLastPeriodInPriorYear(myTimeMember)
			Dim myScenarioTypeMember As Integer = api.Pov.ScenarioTypeId
			Dim Method As String = UCase(api.Entity.OwnershipType(api.Pov.Entity.MemberId,api.Pov.Parent.MemberId,myScenarioTypeMember,myTimeMember).ToString)
			Dim MethodLY As String = UCase(api.Entity.OwnershipType(api.Pov.Entity.MemberId,api.Pov.Parent.MemberId,myScenarioTypeMember,myTimeMemberLY).ToString)				
			Dim ActiveEntity As Boolean = api.Entity.InUse(api.Pov.Entity.MemberId,myScenarioTypeMember,myTimeMember)
			Dim PCon As Decimal = (1/100)*api.Entity.PercentConsolidation(api.Pov.Entity.MemberId,api.Pov.Parent.MemberId,myScenarioTypeMember,myTimeMember)
			Dim PConLY As Decimal = (1/100)*api.Entity.PercentConsolidation(api.Pov.Entity.MemberId,api.Pov.Parent.MemberId,myScenarioTypeMember,myTimeMemberLY)
			Dim PCONDeltaLYCY  As Decimal = PCon-PConLY	
			Dim BOOLMETHODCONS As Boolean = (Method = MethodLY)	
			Dim F_OPE As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_OPE")
			Dim AccountPk As DimPk = api.Pov.AccountDim.DimPk		

			'-- Only consolidate active entities
			If ActiveEntity Then				
				'-- Get Data Buffer (YTD), Required for SWITCH TYPE on FLOW members.
				Dim sourceDataBuffer As DataBuffer = API.Data.GetDataBufferForCustomShareCalculation(viewid:=DIMCONSTANTS.Periodic)
				If Not sourceDataBuffer Is Nothing And PCon <> 0 Then								
					Dim resultDataBuffer As DataBuffer = New DataBuffer()
					For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values	
						If cell.CellAmount <> 0 And Not cell.CellStatus.IsNoData 
							Dim accountId As Integer = cell.DataBufferCellPk.AccountId
							Dim shareCell As New DataBufferCell(cell)
							If api.Pov.Parent.Name.XFEqualsIgnoreCase("SubCons_Affidea_Group_BV") And ImprovePerformance
'								If  api.Members.IsBase(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id, "Years_LC"), cell.DataBufferCellPk.FlowId)
''									sharecell.DataBufferCellPk.FlowId =  DimConstants.None
'								End If
								sharecell.DataBufferCellPk.UD2Id =  DimConstants.None
								sharecell.DataBufferCellPk.UD3Id =  DimConstants.None
								sharecell.DataBufferCellPk.UD5Id =  DimConstants.None
								sharecell.DataBufferCellPk.UD6Id =  DimConstants.None
								sharecell.DataBufferCellPk.UD7Id =  DimConstants.None
							End If 
							If sharecell.DataBufferCellPk.FlowId = F_OPE
								If PConLY <> 0
									Sharecell.CellAmount = cell.CellAmount * PConLY
								End If
							Else 
								Sharecell.CellAmount = Cell.CellAmount * PCon
							End If 
							resultDataBuffer.SetCell(api.SI, sharecell, True)
						End If 
					Next cell
				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")	
				api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)	
				End If
			End If
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	

		#End Region
		
		#Region "IntercompanyElimination"
		Private Sub IntercompanyElimination(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal onlyfrom2023dataelim As Boolean)
			Try	
				Dim entityDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Ent_100_Group")
				Dim entityId As Integer = api.Pov.Entity.MemberId
				Dim parentId As Integer = api.Pov.Parent.MemberId
				Dim scenarioTypeId As Integer = api.Pov.ScenarioTypeId
				Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
				Dim myTimeMember As Integer = api.Pov.Time.MemberId
				Dim myTimeMemberLY As Integer = api.Time.GetLastPeriodInPriorYear(myTimeMember)
				Dim F_OPE As Integer = api.Members.GetMemberId(dimtype.Flow.Id, "F_OPE")
				
				Dim intUD8_OB As Integer = api.Members.GetMemberId(dimtype.UD8.Id, "OB")
				Dim IS_IC As Boolean = True
				Dim strPlugtext1 As String = ""
				Dim intPlugtext1 As Integer = -1
				
				Dim strtext2 As String = ""
				Dim PCON As Decimal = api.Entity.PercentConsolidation*(1/100)
				Dim POWN As Decimal = api.Entity.PercentOwnership*(1/100)
				Dim POWNLY As Decimal = api.Entity.PercentOwnership(api.Pov.Entity.MemberId,,,api.Time.GetLastPeriodInPriorYear)*(1/100)
				Dim plugAccountMemID As Integer = -1
				Dim intTxt2Ac As Integer =  -1
				
				'pull the data source, this is including I#None data, to determine the NCI.
				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferForCustomElimCalculation(
				includeICNone := True, _
				includeICPartners := True, _ 
				combineImportFormsAndAdjConsolidatedIntoElim := True, _
				viewID := DimConstants.YTD)
				If sourceDataBuffer Is Nothing Then Exit Sub
				Dim resultDataBuffer As DataBuffer = New DataBuffer()
				For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
					If cell.CellAmount <> 0 And Not cell.CellStatus.IsNoData And PCon <> 0
						Dim accountId As Integer = cell.DataBufferCellPk.AccountId
						Dim icEntityId As Integer = cell.DataBufferCellPk.ICId
						Dim plugAccount As Member = api.Account.GetPlugAccount(accountId)
						strPlugtext1 = api.Account.Text(accountId,1)
						plugAccountMemID = plugAccount.MemberId
						Dim amount As Decimal = cell.CellAmount
						IS_IC = True 
						'
						'I#Inter_Country, I#Intra_Country
						If cell.DataBufferCellPk.ICId = api.Members.GetMemberId(dimtype.IC.Id, "Inter_Country") And 
						Not api.Entity.HasChildren
							IS_IC = True 
						ElseIf cell.DataBufferCellPk.ICId = api.Members.GetMemberId(dimtype.IC.Id, "Intra_Country") And
						api.Entity.HasChildren
							IS_IC = True
						End If 

						
						
						'___NEW DETERMINE normal elimination condition (i.e. is there a counterparty, is there a plug?).
						If api.Account.IsIC(accountId) <> 1 
							IS_IC = False 
						ElseIf cell.DataBufferCellPk.ICId = DimConstants.None
							IS_IC = False
						ElseIf Not api.Account.IsICMemberValidForAccount(accountId, icEntityId) 
							IS_IC = False
						ElseIf plugAccount Is Nothing Or plugAccount.IsUnknown()
							IS_IC = False	
						ElseIf Not api.Members.IsDescendant(entityDimPk, api.Pov.Parent.MemberId, cell.DataBufferCellPk.ICId)
							'__________added specific for PLAN__________
							If cell.DataBufferCellPk.ICId = api.Members.GetMemberId(dimtype.IC.Id, "Inter_Country") And 
							Not api.Entity.HasChildren
								IS_IC = True 
							ElseIf cell.DataBufferCellPk.ICId = api.Members.GetMemberId(dimtype.IC.Id, "Intra_Country") And
							api.Entity.HasChildren
								IS_IC = True
							Else
								IS_IC = False
							End If 
							'__________added specific for PLAN__________
'							IS_IC = False
						End If
						'___NEW DETERMINE normal elimination condition (i.e. is there a counterparty, is there a plug?).
						
						intPlugtext1 = -1
						strtext2 = api.Account.Text(accountId, 2)	
						intTxt2Ac = -1
'						If cell.DataBufferCellPk.ICId = 80740359
'							BRAPI.ErrorLog.LogMessage(si, " IS_IC " & IS_IC & " cell.DataBufferCellPk.ICId " & cell.DataBufferCellPk.ICId & " " & cell.CellAmount)
'						End If  
						
						'___NEW DETERMINE IF GOODWILL PLUG IS APPLICABLE
						If intUD8_OB =cell.DataBufferCellPk.UD8Id
							If strPlugtext1.XFContainsIgnoreCase("#") AndAlso
							strPlugtext1.XFContainsIgnoreCase("Goodwill")
								If api.Members.GetMemberId(dimtype.Account.Id, strPlugtext1.Split("#")(1)) <> -1
									intPlugtext1 = api.Members.GetMemberId(dimtype.Account.Id, strPlugtext1.Split("#")(1))
								End If
							ElseIf api.Members.IsBase(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "0"),cell.DataBufferCellPk.AccountId)
								intPlugtext1 = api.Members.GetMemberId(dimtype.Account.Id, "630000")
							End If
						End If 
						
						If intPlugtext1 <> -1
							plugAccountMemID = intPlugtext1
						End If 
					
						'___NEW DETERMINE IF GOODWILL PLUG IS APPLICABLE
						
						'______Disabled NOT needed anymore, indirect plug
'						If intPlugtext1 <> -1 And 
'						Not api.Entity.HasChildren And
'						Not api.Members.IsChild(api.Pov.EntityDim.DimPk, api.Pov.Parent.MemberId, cell.DataBufferCellPk.ICId) And 
'						(api.Time.GetYearFromId >= 2024 Or api.Pov.Scenario.Name.XFEqualsIgnoreCase("Actual_Dummy"))
'							plugAccountMemID = intPlugtext1
'						End If 
						
						'______Disabled NOT needed anymore, indirect plug
						
						'______Disabled NOT needed anymore, indirect plug
'						strPlugtext1 = api.Account.Text(plugAccount.MemberId, 1)
						'______Disabled NOT needed anymore, indirect plug
					
						'Update 20102023 update for indirect elim. 
						'Check if the text1 of the PLUG has a text and if this text is an account name. 
						'Example text: Indirect Plug: A#673000
						'If there is no indirect plug the full ic amount will be eliminated towards the direct plug account (assigned as plug account) 
						'In specific instances there can also be an elimination towards the NCI account. 
						
						'______Disabled NOT needed anymore, indirect plug
'						If strPlugtext1.XFContainsIgnoreCase("#") AndAlso 
'						strPlugtext1.XFContainsIgnoreCase("PLUG")
'							If api.Members.GetMemberId(dimtype.Account.Id, strPlugtext1.Split("#")(1)) <> -1
'								intPlugtext1 = api.Members.GetMemberId(dimtype.Account.Id, strPlugtext1.Split("#")(1))
'							End If 
'						End If
						'______Disabled NOT needed anymore, indirect plug
						
						'Based on the counterparty determine if the IC entity is within the same country. 
						'Entity should be base, parent is country
						'The IC member should be a child of this parent. 
						'Default the memberid is the plug account based on the above mentioned conditions
						'Here it Is determined what the Plug should be (direct or indirect)
						
						'______Disabled NOT needed anymore, indirect plug
'						If intPlugtext1 <> -1 And 
'						Not api.Entity.HasChildren And
'						Not api.Members.IsChild(api.Pov.EntityDim.DimPk, api.Pov.Parent.MemberId, cell.DataBufferCellPk.ICId) And 
'						(api.Time.GetYearFromId >= 2024 Or api.Pov.Scenario.Name.XFEqualsIgnoreCase("Actual_Dummy"))
'							plugAccountMemID = intPlugtext1
'						End If 
						'______Disabled NOT needed anymore, indirect plug
						
						'End update 20102023 update for indirect elim.
						'Determine the account for NCI Elim (Plug)
						'Example text1: NCI_ELIM: A#940000
						
						If strtext2.XFContainsIgnoreCase("#") AndAlso strtext2.XFContainsIgnoreCase("NCI")
							If api.Members.GetMemberId(dimtype.Account.Id, strtext2.Split("#")(1)) <> -1
								intTxt2Ac =	api.Members.GetMemberId(dimtype.Account.Id, strtext2.Split("#")(1))
							End If 
						End If
						
						'____1. NCI NON CONTROLLING ELIMINATIONS____
						'This ifstatement checks if there is a difference between the percentage consolidation and percentage ownership
						'AND next it checks if there is a NCI account determined.
						'If both apply so differences between the percentages And there Is a NCI account it will Continue For this ifstatement.
						If PCON <> POWN And intTxt2Ac <> -1 And onlyfrom2023dataelim
							If cell.DataBufferCellPk.FlowId = F_OPE
							'Specific for the opening balance, based on the POWNLY (POWN Last Year)
							'Specific for the non-controlling part (100-90): 10% for last year (PCON-POWNLY)
								SetDataToDataBuffer(si, api, resultDataBuffer, New DataBufferCell(cell), -cell.CellAmount*(PCON-POWNLY), cell.DataBufferCellPk.AccountId, False)
								SetDataToDataBuffer(si, api, resultDataBuffer, New DataBufferCell(cell), cell.CellAmount*(PCON-POWNLY), intTxt2Ac, True, False)
								If POWNLY-POWN <> 0
								'Specific for the differences between the current pown and the pown of last year M12
								'This will be eliminated towards a specific flow: F_NCI_Delta.
								'This is deteremined in the following private sub: NCI_SetDataToDataBuffer.
									NCI_SetDataToDataBuffer(si, api, resultDataBuffer, New DataBufferCell(cell), -cell.CellAmount*(POWNLY-POWN), cell.DataBufferCellPk.AccountId, False)
									NCI_SetDataToDataBuffer(si, api, resultDataBuffer, New DataBufferCell(cell), cell.CellAmount*(POWNLY-POWN), intTxt2Ac, True)
								End If
							Else
								'Applicable for every flow except for the opening balance. 
								SetDataToDataBuffer(si, api, resultDataBuffer, New DataBufferCell(cell), -cell.CellAmount*(PCON-POWN), cell.DataBufferCellPk.AccountId, False)
								SetDataToDataBuffer(si, api, resultDataBuffer, New DataBufferCell(cell), cell.CellAmount*(PCON-POWN), intTxt2Ac, True, False)
							End If
							'____2. INTERCOMPANY ELIMINATIONS WITH NCI LOGIC____
							'The check below is if the value is a valid IC and should therefore be eliminated.
							
							'___NEW DETERMINE normal elim
							If Not IS_IC
								Continue For
							End If 
							'___NEW DETERMINE normal elim
						
							'______Disabled NOT needed anymore, indirect plug
'							If cell.CellStatus.IsNoData Or amount = 0
'								Continue For
'							ElseIf api.Account.IsIC(accountId) <> 1 
'								Continue For
'							ElseIf icEntityId = DimConstants.None
'								Continue For
'							ElseIf Not api.Account.IsICMemberValidForAccount(accountId, icEntityId) 
'								Continue For
'							ElseIf plugAccount Is Nothing Or plugAccount.IsUnknown()
'								Continue For
'							End If
							'______Disabled NOT needed anymore, indirect plug
							
							'IC elim specific controlling part in the NCI (100-10): 90%. (1-(PCON-POWNLY)
							If cell.DataBufferCellPk.FlowId = F_OPE
							'Specific for the opening balance, based on the POWNLY (POWN Last Year)
								SetDataToDataBuffer(si, api, resultDataBuffer, New DataBufferCell(cell), -cell.CellAmount*(1-(PCON-POWNLY)), cell.DataBufferCellPk.AccountId, False)
								SetDataToDataBuffer(si, api, resultDataBuffer, New DataBufferCell(cell), cell.CellAmount*(1-(PCON-POWNLY)), plugAccountMemID, True)
							Else
							'Applicable for every flow except for the opening balance. 
								SetDataToDataBuffer(si, api, resultDataBuffer, New DataBufferCell(cell), -cell.CellAmount*(1-(PCON-POWN)), cell.DataBufferCellPk.AccountId, False)
								SetDataToDataBuffer(si, api, resultDataBuffer, New DataBufferCell(cell), cell.CellAmount*(1-(PCON-POWN)), plugAccountMemID, True)
							End If
						ElseIf plugAccountMemID <> -1

						'____3.INTERCOMPANY ELIMINATIONS WITHOUT NCI LOGIC____
						'If there is no NCI account and there is no difference between the PCON and POWN
						'No percentage is taken into account, the amount is fully eliminated. 
						'The check below is if the value is a valid IC and should therefore be eliminated. 
							'___NEW DETERMINE normal elim
							If Not IS_IC
								Continue For
							End If
							'___NEW DETERMINE normal elim
							
							'______Disabled NOT needed anymore, indirect plug
'							If cell.CellStatus.IsNoData Or amount = 0
'								Continue For
'							ElseIf api.Account.IsIC(accountId) <> 1 
'								Continue For
'							ElseIf icEntityId = DimConstants.None
'								Continue For
'							ElseIf Not api.Account.IsICMemberValidForAccount(accountId, icEntityId) 
'								Continue For
'							ElseIf plugAccount Is Nothing Or plugAccount.IsUnknown()
'								Continue For
'							End If
							'______Disabled NOT needed anymore, indirect plug
							
							SetDataToDataBuffer(si, api, resultDataBuffer, New DataBufferCell(cell), -cell.CellAmount, cell.DataBufferCellPk.AccountId, False)
							SetDataToDataBuffer(si, api, resultDataBuffer, New DataBufferCell(cell), cell.CellAmount, plugAccountMemID, True)
						End If
					End If 
				Next cell
				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
				api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	
		#End Region
		
		#Region "SetDataToDataBuffer"
		Private Sub SetDataToDataBuffer(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByRef dataBuffer As DataBuffer, _
				ByVal cell As DataBufferCell, ByVal amount As Decimal, ByVal accountid As Integer,  _
				Optional ByVal setToNone As Boolean = False, Optional ByVal ICtoEntity As Boolean = False)
				Dim intEntity As Integer = api.Pov.Entity.MemberId
				cell.DataBufferCellPk.AccountId = accountid
				cell.CellAmount = amount
				If setToNone
					cell.DataBufferCellPk.UD1Id = DimConstants.None
					cell.DataBufferCellPk.UD2Id = DimConstants.None
					cell.DataBufferCellPk.UD3Id = DimConstants.None
					cell.DataBufferCellPk.UD4Id = DimConstants.None
					cell.DataBufferCellPk.UD5Id = DimConstants.None
					cell.DataBufferCellPk.UD6Id = DimConstants.None
					cell.DataBufferCellPk.UD7Id = DimConstants.None
'					cell.DataBufferCellPk.UD8Id = DimConstants.None
				End If
'				If ICtoEntity
'					If api.Entity.IsIC(intEntity)
'						cell.DataBufferCellPk.ICId = intEntity
'					End If
'				End If 
				
				dataBuffer.SetCell(api.si, cell, True)
		End Sub
		#End Region
		
		#Region "NCI_SetDataToDataBuffer"
		Private Sub NCI_SetDataToDataBuffer(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByRef dataBuffer As DataBuffer, _
				ByVal cell As DataBufferCell, ByVal amount As Decimal, ByVal accountid As Integer,  _
				Optional ByVal setToNone As Boolean = False, Optional ByVal ICtoEntity As Boolean = False)
				Dim intEntity As Integer = api.Pov.Entity.MemberId
				cell.DataBufferCellPk.AccountId = accountid
				cell.CellAmount = amount
				cell.DataBufferCellPk.FlowId = api.Members.GetMemberId(dimtype.Flow.Id,"F_NCI_Delta")
				If setToNone
					cell.DataBufferCellPk.UD1Id = DimConstants.None
					cell.DataBufferCellPk.UD2Id = DimConstants.None
					cell.DataBufferCellPk.UD3Id = DimConstants.None
					cell.DataBufferCellPk.UD4Id = DimConstants.None
					cell.DataBufferCellPk.UD5Id = DimConstants.None
					cell.DataBufferCellPk.UD6Id = DimConstants.None
					cell.DataBufferCellPk.UD7Id = DimConstants.None
'					cell.DataBufferCellPk.UD8Id = DimConstants.None
				End If
'				If ICtoEntity
'					If api.Entity.IsIC(intEntity)
'						cell.DataBufferCellPk.ICId = intEntity
'					End If
'				End If 
				
				dataBuffer.SetCell(api.si, cell, True)
		End Sub
		#End Region

	End Class
End Namespace