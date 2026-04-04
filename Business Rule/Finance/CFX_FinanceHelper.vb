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

Namespace OneStream.BusinessRule.Finance.CFX_FinanceHelper
	Public Class MainClass
'------------------------------------------------------------------------------------------------------------
'Reference Code: 		CFX_FinanceHelper
'
'Description:			Used for custom functions required for the CFX module, in particular to return the result for the dynamic cash flow
'
'Use Examples:	        First initialize "Dim CashFlowHelper As New OneStream.BusinessRule.Finance.CFX_FinanceHelper.MainClass" 
'						Then Return value Using "Return CashFlowHelper.CashFlowCalc(si, globals, api, args)" on the dynamic member
'
'Created By:			Henning Windhagen
'
'Date Created:			28-08-2020
'------------------------------------------------------------------------------------------------------------	
		#Region "Main (used for memberlists)"
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				
				'initialize VariableRepository
				Dim rep As New OneStream.BusinessRule.Finance.VariableRepository.MainClass

				Select Case api.FunctionType					

					'MEMBER LISTS
					Case Is = FinanceFunctionType.MemberList
										
						#Region "Non-Extended Accounts Base"
						'Call example: A#root.CustomMemberList(BRName=CFX_FinanceHelper, MemberListName=MainAccDimBase, TopAccount=|!topBSAccount!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("MainAccDimBase") Then
							
							Dim sTopAcc As String = args.MemberListArgs.NameValuePairs("TopAccount") 
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim accountDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, rep.mainAccountDimName)							
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(accountDimPk, "A#[" & sTopAcc & "].Base", Nothing)
							
							'Sort alphabetically
							Dim objSortedMembers As List(Of Member) = Nothing							
							If Not objMemberInfos Is Nothing Then
								objSortedMembers = (From memberInfo In objMemberInfos Order By MemberInfo.Member.Name Ascending Select MemberInfo.Member).ToList()
							End If
							Dim objMemberlist As New MemberList(objMemberListHeader, objSortedMembers)
							
							Return objMemberlist 
							
						#End Region
						
						#Region "Entities in alphabetical order"
							
						'Call example: E#root.CustomMemberList(BRName=CFX_FinanceHelper, MemberListName=Entities_sorted_alphabet, TopEntity=|!mainGroupEntity!|)
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("Entities_sorted_alphabet") Then
							
							Dim sTopEnt As String = args.MemberListArgs.NameValuePairs("TopEntity") 

							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim entityDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, rep.mainEntityDimName)
							Dim objMemberInfos As List(Of MemberInfo) = brapi.Finance.Members.GetMembersUsingFilter(si, entityDimPk, "E#[" & sTopEnt & "].DescendantsInclusive", True)

							'Sort alphabetically
							Dim objSortedMembers As List(Of Member) = Nothing							
							If Not objMemberInfos Is Nothing Then
								objSortedMembers = (From memberInfo In objMemberInfos Order By MemberInfo.Member.Name Ascending Select MemberInfo.Member).ToList()
							End If
							Dim objMemberlist As New MemberList(objMemberListHeader, objSortedMembers)
							
							Return objMemberlist 
						End If
						#End Region
														
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		#End Region 

		#Region "Cash Flow Calc"
		
		Public Function CashFlowCalc(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Decimal
					
			Try
					
				'initialize VariableRepository
				Dim rep As New OneStream.BusinessRule.Finance.VariableRepository.MainClass

				'Helper method to dynamically calculate Cash Flow for non-matrix analysis
				'------------------------------------------------
				If api.Pov.Entity.memberid.Equals(dimconstants.Unknown) Then
					Return 0
				Else	
					'STEP 1: Create mapping dictionary			
					Dim oCFMap As Object = Me.mappingDictionary(si, globals, api, args, rep) 

					'STEP 2: Set Global CF data object, if not existing yet
					Dim dCFValue As Decimal	
					Dim oCFData As Dictionary(Of String, Decimal) 'dictionary that holds the return key and value
					Dim sGlobalObjectName_Data As String = "CFData_" & api.Pov.Entity.Name & "_" & api.Pov.Scenario.Name & "_" & api.Pov.Time.Name & "_" & api.Pov.Cons.Name & "_" & api.Pov.View.Name 
					Dim sQuery As String = "I#Top:U1#" & rep.topUD1 & ":U2#" & rep.topUD2 & ":U3#" & rep.topUD3 & ":U4#" & rep.topUD4 & ":U5#" & rep.topUD5 & ":U6#" & rep.topUD6 & ":U8#None" 'basis for data buffer to retrieve all data from BS and IS accounts to be used to calculate CF
					oCFData = globals.GetObject(sGlobalObjectName_Data)

					If oCFData Is Nothing Then 
						'get CF results and set as global object
						oCFData = Me.resultDictionary(si, globals, api, rep, sQuery, oCFData, sGlobalObjectName_Data, oCFMap, "Standard") 
						globals.SetObject(sGlobalObjectName_Data, oCFData)
					End If	
					
					'STEP 3: Retrieve final CF value from global object and return to dynamic member
					Dim result As Decimal = Decimal.Zero				
					For Each cfBaseMember As Member In api.Members.GetBaseMembers(api.Pov.UD1Dim.DimPk, api.Pov.UD1.MemberId)
						If oCFData.TryGetValue(cfBaseMember.MemberId, dCFValue) Then
							Result = result + dCFValue
						End If	
					Next

					If Not result = Decimal.Zero Then Return result 'to improve drill down
					
				End If
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		#End Region 
		
		#Region "Cash Flow Calc Matrix"
		
		Public Function CashFlowCalcMatrix(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Decimal
					
			Try
			
				'initialize VariableRepository
				Dim rep As New OneStream.BusinessRule.Finance.VariableRepository.MainClass
					
				'Helper method to dynamically calculate Cash Flow for matrix analysis
				'------------------------------------------------
				If api.Pov.Entity.memberid.Equals(dimconstants.Unknown) Then
					Return 0
				Else
					'STEP 1: Create mapping dictionary
					Dim oCFMap As Object = Me.mappingDictionary(si, globals, api, args, rep)

					'STEP 2: Set Global CF data object, if not existing yet
					Dim dCFValue As Decimal	
					Dim oCFData As Dictionary(Of String, Decimal) 'dictionary that holds the return key and value
					Dim sGlobalObjectName_MatrixData As String = "CFMatrixData_" & api.Pov.Entity.Name & "_" & api.Pov.Scenario.Name & "_" & api.Pov.Time.Name & "_" & api.Pov.Cons.Name & "_" & api.Pov.View.Name 
					Dim sQuery As String = "I#Top:U2#" & rep.topUD2 & ":U3#" & rep.topUD3 & ":U4#" & rep.topUD4 & ":U5#" & rep.topUD5 & ":U6#" & rep.topUD6 & ":U7#" & rep.topUD7 & ":U8#None" 'basis for data buffer to retrieve all data from BS and IS accounts to be used to calculate CF
					SyncLock globals
						oCFData = globals.GetObject(sGlobalObjectName_MatrixData)
						If oCFData Is Nothing Then 
							'get CF results and set as global object
							oCFData = Me.resultDictionary(si, globals, api, rep, sQuery, oCFData, sGlobalObjectName_MatrixData, oCFMap, "Matrix") 
							globals.SetObject(sGlobalObjectName_MatrixData, oCFData)
						End If
					End SyncLock

					'STEP 3: Retrieve final CF value from global object and return to dynamic member
					Dim result As Decimal = Decimal.Zero				
					For Each cfBaseMember As Member In api.Members.GetBaseMembers(api.Pov.UD1Dim.DimPk, api.Pov.UD1.MemberId)
						If oCFData.TryGetValue(cfBaseMember.MemberId.ToString & "_" & api.Pov.Account.memberID.ToString & "_" & api.Pov.Flow.memberID.ToString, dCFValue) Then
							Result = result + dCFValue
						End If	
					Next
					If Not result = Decimal.Zero Then Return result 'to improve drill down
					
				End If
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#End Region 

		#Region "Cash Flow Calc Stored"
		
		Public Sub CashFlowCalcStored(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sourceOrigin As String)
					
			Try
					
				'initialize VariableRepository
				Dim rep As New OneStream.BusinessRule.Finance.VariableRepository.MainClass

				'STEP 1: Create mapping dictionary	
				Dim oCFMap As Object = Me.mappingDictionary(si, globals, api, args, rep)
					
				'STEP 2: Get data buffer with all IS and BS data to loop through and check against CF mapping
'				Dim sQueryData As String = "O#" & sourceOrigin & ":I#Top:U7#None:U8#None, A#" & rep.CFTopAccount & ".base" 'POV for source data '--> changed for HUH
				Dim sQueryData As String = "O#" & sourceOrigin & ":I#Top:U8#None, A#" & rep.CFTopAccount & ".base" 'POV for source data
				Dim oMySourceDataBuffer As DataBuffer = api.data.GetDataBufferUsingFormula("FilterMembers(A#All:" & sQueryData & ", A#" & rep.CFTopAccount & ".base)") 'data buffer with all CF source data
'				oMySourceDataBuffer.LogDataBuffer(api,"Lookup (Stored - Source Data)",1000)
				
				'STEP 3: Create data buffer with data using source data buffer and mapping
				Dim destinationOrigin As String 
				If sourceOrigin = "Top" Then
					destinationOrigin = "Import"
				Else
					destinationOrigin = sourceOrigin
				End If
					
				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("O#" & destinationOrigin & ":I#None:U8#CashFlowCalc_Stored")
				Dim iMappingSourceSign As Integer 

				If (Not oMySourceDataBuffer Is Nothing) Or (Not oCFMap Is Nothing) Then
					Dim oMyResultDataBuffer As DataBuffer = New DataBuffer()

					For Each cell As DataBufferCell In oMySourceDataBuffer.DataBufferCells.Values
						If (Not Cell.CellStatus.IsNoData) Then
							Dim iAccountId As Integer = cell.DataBufferCellPk.AccountId
							Dim iFlowId As Integer = cell.DataBufferCellPk.FlowId
							Dim sourceDataCombination As String = iAccountId.ToString & "_" & iFlowId.ToString
							
							If oCFMap.ContainsKey(sourceDataCombination) Then
								iMappingSourceSign = math.Sign(oCFMap.item(sourceDataCombination))
								cell.DataBufferCellPk.UD1Id = math.Abs(oCFMap.item(sourceDataCombination))
								cell.CellAmount = cell.CellAmount * iMappingSourceSign
								oMyResultDataBuffer.SetCell(api.DbConnApp.SI, cell)
							End If
						End If
					Next
					
					api.Data.SetDataBuffer(oMyResultDataBuffer, destinationInfo)
				End If 

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub
		
		#End Region 
		
		#Region "Cash Flow Drill Down"
		
		Public Function CashFlowDrillDown(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
			Try

				'initialize VariableRepository
				Dim rep As New OneStream.BusinessRule.Finance.VariableRepository.MainClass
					
				'Helper method to retrieve drill down from CF settings 
				'------------------------------------------------
				
				'STEP 1: Create mapping dictionary			
				Dim oCFMap As Object = Me.mappingDictionary(si, globals, api, args, rep) 
				
				'STEP 2: Loop through cash flow members, accounts and flows to return the correct combinations for drill down
				'Set POV for CF settings
				Dim iTargetID As Integer = api.Pov.UD1.MemberId
				Dim oDataCellPk As DataCellPk = api.Pov.GetDataCellPk
				oDataCellPK.UD8Id = api.members.GetMemberId(dimtype.UD8.Id, "None")
				
				'General declarations
				Dim oOptions As New DataCellDisplayOptions(False, False)
				Dim sSave As String = String.Empty
				Dim result As New DrillDownFormulaResult()
				Dim iSourceSign As Integer				
			
				'get annotation data table sql statement to loop through each mapped item
				Dim sql As New Text.StringBuilder()
				sql.AppendLine("SELECT SourceAccount, SourceAccountID, ActiveAccount, SourceFlow, SourceFlowID, ActiveFlow, FlowInAccountConstraint, TargetCashFlow, TargetCashFlowID, Signage ")
				sql.AppendLine("FROM CashFlowMapping ")
				sql.AppendLine("WITH (NOLOCK) ")
				sql.AppendLine("WHERE ActiveAccount = 'TRUE' ")
				sql.AppendLine("AND ")
				sql.AppendLine("ActiveFlow = 'TRUE' ")
				sql.AppendLine("AND ")
				sql.AppendLine("FlowInAccountConstraint = 'TRUE' ")
				sql.AppendLine("AND ")
				sql.AppendLine("LEN(TargetCashFlow ) > 0 ") 'IS NOT NULL returned blank cells with data in the Signage column

				'define member script variables
				Dim sSourceAccount As String = String.Empty
				Dim sSourceFlow As String = String.Empty
				Dim sCashFlowTarget As String = String.Empty
				Dim sSignage As String = String.Empty
				
				' ### Loop through SQL rows and add each mapping entry to drill down result

				Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					'execute SQL to create a data attachment data table for the POV data unit
					Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sql.ToString, True)
					If Not dt Is Nothing Then
						
						'Loop through cash flow members in the drill down POV (usually only 1)
						For Each cfMember As Member In api.Members.GetBaseMembers(api.Pov.UD1Dim.DimPk, iTargetID)
			
							'loop through the records of the data table 
							For Each row As DataRow In dt.Rows
								If iTargetID = CInt(row("TargetCashFlowID")) Then
									sSignage = row("Signage") & "1" 'assume +1 for non-entry
									iSourceSign = math.Sign(CInt(sSignage))

									oDataCellPK.AccountId = CInt(row("SourceAccountID"))
									oDataCellPK.FlowId = CInt(row("SourceFlowID"))
									'add string to explanation
									sSave = sSave & IIf(iSourceSign < 0, " - ", " + ") & _
										"A#" & api.Members.GetMemberName(dimtype.Account.Id, odatacellpk.AccountId) & _
										":F#" & api.Members.GetMemberName(dimtype.Flow.Id, odatacellpk.FlowId)
									'add source data cells for drill down result
									result.SourceDataCells.Add("A#" & api.Members.GetMemberName(dimtype.Account.Id, odatacellpk.AccountId) & _
										":F#" & api.Members.GetMemberName(dimtype.Flow.Id, odatacellpk.FlowId) & ":O#Top:I#" & _
										oDataCellPk.GetICName(api) & ":U1#Top:U2#" & oDataCellPk.GetUD2Name(api) & _
										":U3#" & oDataCellPk.GetUD3Name(api) & ":U4#" & oDataCellPk.GetUD4Name(api) & ":U5#" & oDataCellPk.GetUD5Name(api) & _
										":U6#" & oDataCellPk.GetUD6Name(api) & ":U7#" & oDataCellPk.GetUD7Name(api) &":U8#None") '--> still to be adjusted for HUH
								End If
							Next 
						Next
					End If
				End Using					
						
				result.Explanation = api.Pov.UD1.Name & " = " & sSave
				
				Return result
					
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Function	
		
	#End Region

		#Region "Helper Functions"

		Public Function mappingDictionary(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal rep As Object) As Object
			Try
				Dim sGlobalObjectName_Mapping As String = "CFMap" 
				Dim oCFMap As Dictionary(Of String, Integer) 'dictionary that holds the return key and value
				Dim dCFTargetID As Integer
				
				SyncLock globals
					oCFMap = globals.GetObject(sGlobalObjectName_Mapping)
					
					If oCFMap Is Nothing Then
						oCFMap = New Dictionary(Of String, Integer)

						Dim iTargetMember As Integer 
						Dim iSourceSign As Integer 
						Dim targetCombination As String		
	
						'### Retrieve rows from SQL annotation table ("CashFlowMapping") ###

						'get mapping table sql statement
						Dim sql As New Text.StringBuilder()
						sql.AppendLine("SELECT SourceAccount, SourceAccountID, ActiveAccount, SourceFlow, SourceFlowID, ActiveFlow, FlowInAccountConstraint, TargetCashFlow, TargetCashFlowID, Signage ")
						sql.AppendLine("FROM CashFlowMapping ")
						sql.AppendLine("WITH (NOLOCK) ")
						sql.AppendLine("WHERE ActiveAccount = 'TRUE' ")
						sql.AppendLine("AND ")
						sql.AppendLine("ActiveFlow = 'TRUE' ")
						sql.AppendLine("AND ")
						sql.AppendLine("FlowInAccountConstraint = 'TRUE' ")
						sql.AppendLine("AND ")
						sql.AppendLine("LEN(TargetCashFlow ) > 0 ") 'IS NOT NULL returned blank cells with data in the Signage column
						
						'define member script variables
						Dim sSourceAccount As String = String.Empty
						Dim sSourceFlow As String = String.Empty
						Dim sCashFlowTarget As String = String.Empty
						Dim sSignage As String = String.Empty
						Dim iCashFlowTargetID As Integer
						
						' ### Loop through SQL rows and add each mapping entry to dictionary

						Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						'execute SQL to create a data attachment data table for the POV data unit
							Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sql.ToString, True)
							If Not dt Is Nothing Then
									'loop through the records of the data table 
								For Each row As DataRow In dt.Rows
									iCashFlowTargetID = row("TargetCashFlowID")
									iTargetMember = CInt(row("TargetCashFlowID"))
									sSignage = row("Signage") & "1" 'assume +1 for non-entry
									iSourceSign = math.Sign(CInt(sSignage))
									targetCombination = row("SourceAccountID").ToString & "_" & row("SourceFlowID").ToString
									
									'Add the result and target combination to the mapping dictionary
									If oCFMap.TryGetValue(targetCombination, dCFTargetID) Then
										oCFMap.item(targetCombination) = iSourceSign * iTargetMember
									Else 
										oCFMap.Add(targetCombination,  iSourceSign * iTargetMember)	
									End If
								Next 
							End If
						End Using								
					End If

					globals.SetObject(sGlobalObjectName_Mapping, oCFMap)
				End SyncLock
				
				Return oCFMap
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
				
		Public Function resultDictionary(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal rep As Object, ByVal sQuery As String, ByVal oCFData As Object, ByVal sGlobalObjectName_Data As Object, ByVal oCFMap As Object, ByVal AnalysisType As String) As Object
			Try
				
				SyncLock globals 'With this Synclock only data buffers with data on them are opened and all empty ones are skipped
						
					'general declarations
					Dim dCFValue As Decimal	
					Dim iTargetMember As Integer
					Dim sTargetMembers As String = String.Empty
					Dim iSourceSign As Integer	
					oCFData = New Dictionary(Of String, Decimal)

					'define analysis type
					Dim bMatrixAnalysis As Boolean = False
					If AnalysisType = "Matrix" Then
						bMatrixAnalysis = True
					End If
	
					Dim oOptions As New DataCellDisplayOptions(False, False)
					Dim oMyDataBuffer As DataBuffer = api.data.GetDataBufferUsingFormula("FilterMembers(A#All:" & sQuery & ", A#" & rep.CFTopAccount & ".base)") 'data buffer with BS and IS data
'					oMyDataBuffer.LogDataBuffer(api,"Lookup (Stored - Source Data)",1000)

					For Each oMyDataBufferCell As DataBufferCell In oMyDataBuffer.DataBufferCells().Values
						If (Not oMyDataBufferCell.CellStatus.IsNoData) Then
							
							'get target member ID from data cell
							Dim targetID As Integer
							If Not bMatrixAnalysis Then
								If oCFMap.TryGetValue(oMyDataBufferCell.DataBufferCellPk.AccountId & "_" & oMyDataBufferCell.DataBufferCellPk.FlowId, targetID) Then
									iTargetMember = math.abs(targetID)
								End If
								If iTargetMember <> 0 Then 								
									iSourceSign = math.Sign(targetID) 'get signage

									'### Add value to target cash flow member ###
									If oMyDataBufferCell.CellStatus.IsRealData Then 'only add real data in order to have a correct CF also in planning scenarios
										If oCFData.TryGetValue(iTargetMember, dCFValue) Then
											oCFData.item(iTargetMember) = iSourceSign * oMyDataBufferCell.CellAmount + dCFValue
										Else
											oCFData.Add(iTargetMember,  iSourceSign * oMyDataBufferCell.CellAmount)
										End If
									End If
								End If
							Else 'matrix analysis
								If oCFMap.TryGetValue(oMyDataBufferCell.DataBufferCellPk.AccountId & "_" & oMyDataBufferCell.DataBufferCellPk.FlowId, targetID) Then
									sTargetMembers = math.abs(targetID) & "_" & oMyDataBufferCell.DataBufferCellPk.AccountId & "_" & oMyDataBufferCell.DataBufferCellPk.FlowId
								End If
								If Not sTargetMembers Is Nothing Then 								
									iSourceSign = math.Sign(targetID) 'get signage

									'### Add value to target cash flow member ###
									If oMyDataBufferCell.CellStatus.IsRealData Then 'only add real data in order to have a correct CF also in planning scenarios
										If oCFData.TryGetValue(sTargetMembers, dCFValue) Then
											oCFData.item(sTargetMembers) = iSourceSign * oMyDataBufferCell.CellAmount + dCFValue
										Else
											oCFData.Add(sTargetMembers,  iSourceSign * oMyDataBufferCell.CellAmount)
										End If
									End If
								End If	
							End If
						End If
					Next
				
				End SyncLock

				Return oCFData
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
				
		#End Region
	
	End Class
End Namespace