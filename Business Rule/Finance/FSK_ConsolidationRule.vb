#Region "Imports"

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


'Additional Imports
Imports System.Collections.Concurrent

#End Region
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 		FSK_ConsolidationRule
		'
		'Description:			Consolidation Engine based on the Financial Starter Kit
		'
		'Usage:					Used to provide an accelerator for complex consolidation. 
		'						To be put in the cube's formula with custom consolidation settings
		'						Is used by MemberFormulas and No.Input Rules
		'						Please refer to documentation
		'
		'Created By: OneStream FSK rule
		'Customized By:			Nova
		'Version: 				V1.0
		'Date Created:			January 2024
		'------------------------------------------------------------------------------------------------------------	
#Region "Change History"
'VERSION WITH BOOK FUNCTION & ID & UPDATE FLOW VARIABLES
'Plus SQL Table to track relationship for Pelim
'Perimeter without mention of parents / scenario & with opening scenario function

'CWe 2020-4-8 Simplified class initialization  with globals
'Replaced
' 	Dim Flow As FlowClass = globals.GetObject("FSKFlows")
'	If Flow Is Nothing Then
'		Flow = New FlowClass(si, api)
'		globals.SetObject("FSKFlows", Flow)
'	End If
'With
'   Dim Flow As FlowClass = Me.InitFlowClass(si, api, globals)
'By creating two functions In the Main Class to initalize Flow and Acct 

'CWe 2020-4-8 Dynamic and Simplified ownership handling (Ownership by Parent
'Replaced old ownership variables with a ownership class
'Class is retrieveing the ownerships from the databody based on entity and parent combinations

'CWe 2020-4-20 IFRS5
'Creating a new method CalculateIFRS5 to book ifrs 5 adjustments for shares and elimination. It will called in the calculate section of the Main class.
'Changed the allocation of target accounts and movements
'Accounts that are under TopAss will go to assets held for sale, accounts under TopLiab will go to liabilities helf for sale
'The var movements will be eliminated like a other movements against the DOP movement

'I also added a new column to the log table showing the actual business rule executing the transaction (no line numbers yet)

'CWe 2020-4-24 EXITM and Merge
'Created two methods to eliminate the balance sheet. They will called in the calculate section of the Main class
'-to eliminate the entity itself
'-to eliminate all intercompany relationships with the exiting company

'CWe 2020-4-25 Change the order of ExitIC and IFRS5 consolidaton 

'CWe 2020-4-30 Fix the AUG or DIM problem for titre by splitting the sections and turning the signs

'FSM 2020-7-14 Commented lines 129-134; 3470-3475; Region IFRS and Region EXIT

#End Region

Namespace OneStream.BusinessRule.Finance.FSK_ConsolidationRule
		
#Region "Main Class"		
Public Class MainClass
		Private bDoLog As Boolean = False
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				
				'Enable And disable line item logging
'XX				bDoLog = globals.GetBoolValue("Log_CHP", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Log_CHP").XFConvertToBool)
'XX				globals.SetBoolValue("Log_CHP", bDoLog)

				Dim IsInitialized As Boolean = globals.GetBoolValue("FSK_Init", False)
				
				If Not IsInitialized Then
					SyncLock Globals
						Dim sGName As String = "FSKNature"
						globals.SetObject(sGName, Nothing)
						sGName = "FSKAccounts"
						globals.SetObject(sGName, Nothing)
						sGName = "FSKFlows"
						globals.SetObject(sGName, Nothing)
						globals.SetBoolValue("FSK_Init", True)
					End SyncLock

				End If
				
				Select Case api.FunctionType
					Case Is = FinanceFunctionType.ConsolidateShare
						ConsolidateShare(si, api, globals, args)
					Case Is = FinanceFunctionType.ConsolidateElimination
						CalculateElim(si, api, globals, args)
				End Select
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
'		Public Function GetUDasICId(ByVal api As FinanceRulesApi, ByVal oAcell As DataBufferCell, ByVal iICUD As Integer, ByVal iUD As Integer) As Integer
'			Select Case iICUD 
'				Case dimtype.UD1.Id
'					Return api.members.GetMemberId(iUD, oAcell.DataBufferCellPk.GetUD1Name(api))
'				Case dimtype.UD2.Id
'					Return api.members.GetMemberId(iUD, oAcell.DataBufferCellPk.GetUD2Name(api))
'			End Select
'		End Function											

'		Public Function GetUDId(ByVal oAcell As DataBufferCell, ByVal iUD As Integer) As Integer
'			Select Case iUD 
'				Case dimtype.UD1.Id
'					Return oAcell.DataBufferCellPk.UD1Id
'				Case dimtype.UD2.Id
'					Return oAcell.DataBufferCellPk.UD2Id
'			End Select
'		End Function		

'		Public Function GetUDName(ByVal api As FinanceRulesApi, ByVal oAcell As DataBufferCell, ByVal sUD As String) As String
'			Select Case sUD 
			
'				Case "UD1"
'					Return oAcell.DataBufferCellPk.GetUD1Name(api)
'				Case "UD2"
'					Return oAcell.DataBufferCellPk.GetUD2Name(api)
'			End Select
'		End Function
		
		
#End Region

'Consolidation Business Rules
#Region "Share Calculation"
   
		Private Sub ConsolidateShare(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			Try
					'Clear all previously calculated data from the Share consolidation member.
					api.Data.ClearCalculatedData(True, True, True)
					
					Dim Entity As String = api.Pov.Entity.Name
					Dim EntityMember As Member = api.Pov.Entity
					Dim refOwnTime As String = api.Pov.Time.Name
					
					Dim Period As String = api.Pov.Time.Name
					Dim GlobalClass As New GlobalClass(si,api,args)
					
					Dim Opescenario As String = GlobalClass.OpeScenario				
					Dim Nat As NatureClass = Me.InitNatureClass(si, api, globals, api.Pov.UD4Dim.DimPk)
					Dim Flow As FlowClass = Me.InitFlowClass(si, api, globals)
					Dim Acct As AccountClass = Me.InitAccountClass(si, api, globals)
					
					
					
					Dim accountDimPk As DimPk = api.Pov.AccountDim.DimPk
					
					'Get the percent consolidation.				
					Dim Own As New OwnershipClass(si,api,globals,Entity,,,, OpeScenario)
					
					Dim method As Integer = Own.method
					Dim scope As Integer = Own.scope
				
					'Read the source data from the Translated and OwnerPreAdj consolidation members.
					Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
					Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferForCustomShareCalculation()
					
					'Multiply each cell by the percent consolidation.
					If Not sourceDataBuffer Is Nothing Then
						Dim resultDataBuffer As DataBuffer = New DataBuffer()
						Dim share As New ConsClass(si, globals, api, resultDataBuffer, bDoLog) 'The object for the book method

						For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
							If (Not cell.CellStatus.IsNoData) Then
								Dim flowMember As Member = api.Members.GetMember(DimType.flow.Id, cell.DataBufferCellPk.FlowId)
								Dim flowDestMember As Member
								'Opening flow consolidated in flow VARC
								If flowMember.MemberId = flow.OPE.MemberId Then
									flowDestMember = flow.VarC
								Else
									flowDestMember = flowMember
								End If
								
								Dim accountId As Integer = cell.DataBufferCellPk.AccountId
								Dim acctMember As Member = api.Members.GetMember(DimType.Account.Id, cell.DataBufferCellPk.AccountId)
								Dim NatureSource As Member = Nat.GetMember(cell.DataBufferCellPk.Item(nat.NatDimType.Id))
								
								Dim text3Acct As String = api.Account.Text(accountId, 3)
								Dim CaseElim As String							
								Dim UndSco As Integer = InStr(text3Acct, "_")
	        					If UndSco > 0 Then CaseElim=Left(text3Acct, UndSco - 1) Else CaseElim=""
								
								Dim Nature As String
									If NatureSource.Name = "Top" Then 
										Nature = "Top" 
									ElseIf Len(text3Acct) = UndSco Then 
										Nature = "None"  
									Else 
										Nature = Right(text3Acct, Len(text3Acct) - UndSco)	
									End If
								Dim NatureDest As Member = Nat.getMember(Nature)'api.Members.GetMember(DimType.UD4.Id, Nature)
								
								
								'----------------------------------------------------
								' Default consolidation
					            '----------------------------------------------------
								'If the flow is Not CLO
								Dim sDFRule As String = "DEFAULT"
								If flowMember.MemberId <> flow.Clo.MemberId Then
									
									'Scope = ENTRY Then Flow = ENT
									If scope = FSKScope.ENTRY Then
										If flowMember.MemberId <> flow.OPE.MemberId And flowMember.MemberId <> flow.RES.MemberId And flowMember.MemberId <> flow.None.MemberId Then
											share.Book(cell,,flow.ENT.MemberId,,,,,,,,,,,Own.pCon ,,sDFRule ,10005)
										Else
											share.Book(cell,,flowDestMember.MemberId,,,,,,,,,,,Own.pCon ,,sDFRule ,10005)
										End If
									
									ElseIf method <> FSKMethod.MEE And scope <> FSKScope.EXITM And method <> FSKMethod.NOCONSO Then
										'If the flow is DBE
										If flowMember.MemberId = flow.RPY.MemberId Then
											'Consolidate the data at the % of consolidation of N-1
											share.Book(cell,,flowDestMember.MemberId,,,,,,,,,,,Own.pCon_1 ,,sDFRule ,10001)
											'Consolidate the data on the VARC flow at the change in % of consolidation
											share.Book(cell,,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,sDFRule ,10002)
										'DIV flows	
										ElseIf flowMember.MemberId = flow.Div.MemberId Then
											'Consolidate the data at the % of consolidation of N-1
											share.Book(cell,,flowDestMember.MemberId,,,,,,,,,,,Own.pCon_1 ,,sDFRule ,10003)
											'Consolidate the data on the VARC flow at the change in % of consolidation
											share.Book(cell,,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,sDFRule ,10004)
										
										'For other flows	
										Else
											'Consolidate the data at the % of consolidation of N
											share.Book(cell,,flowDestMember.MemberId,,,,,,,,,,,Own.pCon ,,sDFRule ,10005)
										End If	
									End If
								End If
								
							
								'If the consolidation method is not Exit/Not Consolidated
								If scope <> FSKScope.EXITM And method <> FSKMethod.NOCONSO Then
									'If the consolidation rule is not Blank and the flow is not CLO
									If CaseElim<>"" And flowMember.MemberId <> flow.Clo.MemberId Then
										
					                    '----------------------------------------------------
										' Equity accounts
					                    '----------------------------------------------------
										If CaseElim="FFPP" Then
												
											If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											'If the flow is DBE/DIV	
											If flowMember.MemberId = flow.RPY.MemberId Then
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Then
													'Consolidate the data at the % of consolidation of N-1
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon_1 ,,CaseElim,10006)
													'Consolidate the data on the VARC flow at the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,CaseElim,10007)
													'Trigger the account Investments in equity affiliates at the % of consolidation of N-1
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon_1 ,,CaseElim,10008)
													'Trigger the account Investments in equity affiliates on the VARC flow at the change in % of consolidation
													share.Book(cell,acct.MEE.MemberId,flow.VarC.MemberId,,,,,,NatureDest.MemberId,,,,,Own.VpCon ,,CaseElim,10009)
												End If
												
												'If the entity is not consolidated in N-1
												If Own.pCon_1 = 0 Then
													'Consolidate the data on the VARC flow at - the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,-Own.VpCon,,CaseElim,10010)
													'Consolidate the data on the ENT flow at the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.Ent.MemberId,,,,,,,,,,,Own.VpCon ,,CaseElim,10011)
												End If
												
											ElseIf flowMember.MemberId = flow.Div.MemberId Then
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Then
													'Consolidate the data at the % of consolidation of N-1
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,-Own.pCon_1 ,,CaseElim,10012)
													'Consolidate the data on the VARC flow at the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,CaseElim,10013)
													'Trigger the account Investments in equity affiliates at the % of consolidation of N-1
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,-Own.pCon_1 ,,CaseElim,10014)
													'Trigger the account Investments in equity affiliates on the VARC flow at the change in % of consolidation
													share.Book(cell,acct.MEE.MemberId,flow.VarC.MemberId,,,,,,NatureDest.MemberId,,,,,Own.VpCon ,,CaseElim,10015)
												End If
												
												'If the entity is not consolidated in N-1
												If Own.pCon_1 = 0 Then
													'Consolidate the data on the VARC flow at - the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,-Own.VpCon,,CaseElim,10016)
													'Consolidate the data on the ENT flow at the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.Ent.MemberId,,,,,,,,,,,Own.VpCon ,,CaseElim,10017)
												End If
												
											End If

											
										'----------------------------------------------------
										' Account: Current Year Earnings
					                    '----------------------------------------------------
										ElseIf CaseElim="UTIL" Then
											If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											
											'If the flow is DBE		
											If flowMember.MemberId = flow.RPY.MemberId Then										
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Then
													'Consolidate the data at the % of consolidation of N-1
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon_1 ,,CaseElim,10026)
													'Consolidate the data on the VARC flow at the change in % of consolidation
													'novashare.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,CaseElim,10027)
													'Trigger the account Investments in equity affiliates at the % of consolidation of N-1
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon_1 ,,CaseElim,10028)
													'Trigger the account Investments in equity affiliates on the VARC flow at the change in % of consolidation
													'novashare.Book(cell,acct.MEE.MemberId,flow.VarC.MemberId,,,,,,NatureDest.MemberId,,,,,Own.VpCon ,,CaseElim,10029)
												End If
											
											'For other flows	
											Else
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Then
													'Consolidate the data at the % of consolidation of N
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon ,,CaseElim,10030)
													'Trigger the account Investments in equity affiliates at the % of consolidation
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10031)
													
													'If the flow is RES
													If flowMember.MemberId = flow.Res.MemberId Then 
														'Trigger the account Net income from equity companies on the None flow at the % of consolidation
														share.Book(cell,acct.ResMEE.MemberId,flow.None.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10032)
													End If
												End If
											End If
											
										'----------------------------------------------------
										' Account 120C: CTA from Current Year Earnings
					                    '----------------------------------------------------	
										ElseIf CaseElim="UTILC" Then										
											If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											
												'If the flow is DBE		
											If flowMember.MemberId = flow.RPY.MemberId Then										
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Then
													'Consolidate the data at the % of consolidation of N-1
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon_1 ,,CaseElim,10033)
													'Consolidate the data on the VARC flow at the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,CaseElim,10034)
													'Trigger the account Investments in equity affiliates at the % of consolidation of N-1
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon_1 ,,CaseElim,10035)
													'Trigger the account Investments in equity affiliates on the VARC flow at the change in % of consolidation
													share.Book(cell,acct.MEE.MemberId,flow.VarC.MemberId,,,,,,NatureDest.MemberId,,,,,Own.VpCon ,,CaseElim,10036)
												End If
												
											'For other flows	
											Else
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Then
													'Consolidate the data at the % of consolidation of N
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon ,,CaseElim,10037)
													'Trigger the account Investments in equity affiliates at the % of consolidation
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10038)
												End If
											End If
										End If	
									End If
								End If	

							End If
						Next
						
						'----------------------------------------------------
						' Consolidated Openings
					    '----------------------------------------------------
						Dim PriorPer As String = api.Time.GetNameFromId(api.Time.GetLastPeriodInPriorYear)
						Dim sourceScript As String = "F#" & flow.Clo.Name & ":C#SHARE:T#" & PriorPer & ":S#" & Opescenario
						Dim sourceDataBuffer2 As DataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, sourceScript, destinationInfo)

						Dim sPPRule As String = "PRIORYEAR"
						If scope <> FSKScope.EXITM And method <> FSKMethod.NOCONSO Then							
							
							If Not sourceDataBuffer2 Is Nothing Then
								For Each cell As DataBufferCell In sourceDataBuffer2.DataBufferCells.Values
									
									If (Not cell.CellStatus.IsNoData) Then
										'Trigger the VARC flow at -100%
										share.Book(cell,,flow.VarO.MemberId,,,,,,,,,,,-1,,sPPRule,10039)
									End If
								Next
							End If

						ElseIf scope = FSKScope.EXITM Then
							If Not sourceDataBuffer2 Is Nothing Then
								For Each cell As DataBufferCell In sourceDataBuffer2.DataBufferCells.Values
									If (Not cell.CellStatus.IsNoData) Then
										'Trigger the SOR flow at -100%
										share.Book(cell,,flow.EXI.MemberId,,,,,,,,,,,-1,,sPPRule,10040)
									End If
								Next
							End If
							
						End If	
						'Store the results for the Share consolidation member that is currently being executed.
						api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
						share.WriteLogToTable
					End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub				
#End Region

#Region "Elimination"		
		Private Sub CalculateElim(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			Try
				'If Not api.Entity.HasChildren() Then
				Dim Parent As String = api.Pov.Parent.Name
				Dim Entity As String = api.Pov.Entity.Name
			    Dim entityMember As Member = api.Pov.Entity
				Dim hasChildren As Boolean = api.Entity.HasChildren
			    Dim Scenario As String = api.Pov.Scenario.Name
				Dim CurYear As String = Left (api.Pov.Time.Name, 4)
				Dim PerNo As String = Right (api.Pov.Time.Name, Len(api.Pov.Time.Name)-5)
				Dim Period As String = api.Pov.Time.Name
				
				'Get the percent consolidation.				
				Dim Own As OwnershipClass
				Dim method As Integer
				Dim scope As Integer

				Dim GlobalClass As New GlobalClass(si,api,args)
				Dim Opescenario As String = GlobalClass.OpeScenario
				'xxIf Not api.Entity.HasChildren() Then
					'Get the percent consolidation.				
					Own = New OwnershipClass(si,api,globals,Entity,,,, OpeScenario)
					method = Own.method
					scope = Own.scope
					
				'Else	
					'method = 2 and 100% Ownership
				'xx	Own = New OwnershipClass(si,1,1)
				'xx	method = FSKMethod.IG
				'xx	scope = FSKScope.DefaultScope
					
				'xxEnd If

					'Variables that are confimed used
					Dim accountDimPk As DimPk = api.Pov.AccountDim.DimPk
					
					Dim Acct As AccountClass = Me.InitAccountClass(si, api, globals)
					Dim Flow As FlowClass = Me.InitFlowClass(si, api, globals)
					Dim Nat As NatureClass = Me.InitNatureClass(si, api, globals, api.Pov.UD4Dim.DimPk)

					Dim NatureDimPropU As Member = Nat.PropU
					Dim NatureDimPropE As Member = Nat.PropE
					Dim NatureDimMinos As Member = Nat.Minoritaires
					Dim NatureDimEquity As Member = Nat.Equity
					Dim icNoneMember As Member = api.Members.GetMember(DimType.Entity.Id, DimConstants.None)
					Dim UD2NoneMember As Member = api.Members.GetMember(DimType.UD2.Id, "None")
					Dim text3Parent As String = api.Entity.Text(api.Pov.Parent.MemberId,3)
					Dim UD8Dest As Member = api.Members.GetMember(DimType.UD8.Id, "None")
					
					Dim refOwnTime As String = api.Pov.Time.Name
			
					Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
					Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferForCustomShareCalculation()
					
						Call GlobalClass.ClearPelimTable(si,api)						
									
			        If Not sourceDataBuffer Is Nothing Then
						Dim resultDataBuffer As DataBuffer = New DataBuffer()
						Dim cons As New ConsClass(si, globals, api, resultDataBuffer, bDoLog) 'The object for the book method
						
						'sourceDataBuffer.LogDataBuffer(api,"sourcedatabufferlog",1000)
						
						For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
						
					'	resultDataBuffer.LogDataBuffer(api,api.Pov.Entity.Name &"-"& api.Pov.Time.Name &"-"& api.Pov.Cons.Name & ":Target Data Buffer", 1000)
					'	sourceDataBuffer.LogDataBuffer(api,api.Pov.Entity.Name &"-"& api.Pov.Time.Name &"-"& api.Pov.Cons.Name & ":Source Data Buffer", 1000)
						
			                If (Not cell.CellStatus.IsNoData) And scope <> FSKScope.EXITM And method <> FSKMethod.NOCONSO Then								
								Dim flowMember As Member = api.Members.GetMember(DimType.flow.Id, cell.DataBufferCellPk.FlowId)
								Dim flowDestMember As Member
								
								'If the source flow is OPE
								If flowMember.MemberId = flow.OPE.MemberId Then
									'the destination flow is VARC
									flowDestMember = flow.VarC
								'For other flows	
								Else
									'the destination flow is the source flow
									flowDestMember = flowMember
								End If	
									
								Dim bDoEliminate As Boolean = True
								Dim acctMember As Member = api.Members.GetMember(DimType.Account.Id, cell.DataBufferCellPk.AccountId)
								Dim NatureSource As Member = Nat.getMember(cell.DataBufferCellPk.Item(nat.NatDimType.Id))'api.Members.GetMember(DimType.UD4.Id, cell.DataBufferCellPk.UD4Id)
								Dim accountId As Integer = cell.DataBufferCellPk.AccountId
								
								'NOVA
								Dim iCid As Integer = cell.DataBufferCellPk.ICId
								Dim IntercoMember As Member = api.Members.GetMember(DimType.IC.Id, cell.DataBufferCellPk.ICId)
								
'								Dim IntercoDestMember As Member
'								If Not api.Entity.HasChildren() Then
'									IntercoDestMember = api.Pov.Entity
'								Else 
'									IntercoDestMember = IntercoMember
'								End If
								
								Dim IntercoDestMember As Member = api.Pov.Entity

								'Determine consolidation rule and UD4 nature from account text3
								'For Example: Account 101000 text3 is CAPI_CAP, rule is CAPI and UD4 is CAP								
								Dim text3Acct As String = api.Account.Text(accountId, 3)
								Dim CaseElim As String							
								Dim UndSco As Integer = InStr(text3Acct, "_")
	        					If UndSco > 0 Then CaseElim=Left(text3Acct, UndSco - 1) Else CaseElim=""
								Dim Nature As String

								If Len(text3Acct) = UndSco Then 
									Nature = "None" 
								
								'Member condition to keep the original memeber value, do not generate audit trail values for automatic consolidation. 
								'EXAMPLE 
								'ElseIf NatureSource.Name = "RetSocG_R1" Or NatureSource.Name = "RetSocG_R2" Or NatureSource.Name = "RetSocG1_PL" Then
								'	Nature = "ELCE_GES"
								
								Else 
									'Audit trail value in UD4 based on Right Text3 after "_"
									Nature = Right(text3Acct, Len(text3Acct) - UndSco)							
								End If
								Dim NatureDest As Member = Nat.getMember(Nature)'api.Members.GetMember(DimType.NatureDim.Id, Nature)
								Dim icMember As Member = api.Members.GetMember(DimType.IC.Id, cell.DataBufferCellPk.ICId)
								Dim icName As String = icMember.Name
								Dim partnerMember As Member = api.Members.GetMember(DimType.IC.Id, cell.DataBufferCellPk.ICId)
								Dim partnerName As String = partnerMember.Name
								
								Dim EntityMethod As Integer = OwnershipClass.GetOwnershipInfoByName(si, api, globals, "method",api.Pov.Entity.Name)
								Dim partnerMethod As Integer = OwnershipClass.GetOwnershipInfoByName(si, api, globals, "method",partnerName)
								Dim icMethod As Integer  = OwnershipClass.GetOwnershipInfoByName(si, api, globals, "method", icName)
								Dim icScope As Integer  = OwnershipClass.GetOwnershipInfoByName(si, api, globals, "scope", icName)
								Dim icIsSplit As Boolean = False 
								Dim CanEliminate As Boolean = False
								Dim icMethod1245 As Boolean = False
								Dim icMethod12345 As Boolean = False
								Dim icRef As String
								icRef=icName
								
								'xx Dim isICPartnerADescendantOfParentEnt = api.Members.IsDescendant(api.Pov.EntityDim.DimPk, api.Pov.Parent.MemberId, cell.DataBufferCellPk.ICId, Nothing)
								'xx Dim isICPartnerADescendantOfParentEnt = api.Members.IsBase(api.Pov.EntityDim.DimPk, api.Pov.Entity.MemberId, cell.DataBufferCellPk.ICId, Nothing)
								Dim isICPartnerADescendantOfParentEnt = api.Members.IsDescendant(api.Pov.EntityDim.DimPk, api.Pov.Parent.MemberId, cell.DataBufferCellPk.ICId, Nothing)
								'								If api.Pov.Entity.Name = "E148" And icName.Equals("E163") Then
'									brapi.ErrorLog.LogMessage(si, "SR" & isICPartnerADescendantOfParentEnt.ToString)
'								End If
								'xx validate orgbyperiod change in pcon not =0
									
								If isICPartnerADescendantOfParentEnt = True Then
									If api.Pov.Entity.Name = "E148" And icName.Equals("E163") Then
'										brapi.ErrorLog.LogMessage(si, "SR1" & api.Pov.Entity.Name & " - " & icName & " - " & icMethod.ToString & " - " & api.Pov.Parent.Name)
										'brapi.ErrorLog.LogMessage(si, "1 - " & api.Pov.Entity.Name & " - " & icName & " - " & icpown.method & " - " & icMethod12345)
									End If
									'CanEliminate = True
									If icMethod = FSKMethod.HOLDING Or icMethod = FSKMethod.IG Or icMethod = FSKMethod.IP Then
										icMethod1245 = True
										icMethod12345 = True
									ElseIf icMethod	= FSKMethod.MEE Then
										icMethod12345 = True
									End If	
								End If
								
								'brapi.ErrorLog.LogMessage(si, "err99: entidad " & api.Pov.Entity.Name & " padre " & api.Pov.Parent.Name & icName & " metodo " & icMethod.tostring)
				
								
								If CaseElim<>"" And flowMember.MemberId <> flow.Clo.MemberId Then
									
									'******************************************************************************************
											'Matrix consolidation by BU (UD1) to be included in IC elim

'										Dim sUD As String = "UD1"
'										Dim sICUD As String = "UD2"
'										Dim iUD As String = DimType.UD1.Id
'										Dim iICUD As String = DimType.UD2.Id
'										Dim oUDDim As DimPk = api.Pov.UD1Dim.DimPk
'										Dim oICUDDim As DimPk = api.Pov.UD2Dim.DimPk
'										Dim iUDTop As Integer = api.Members.GetMemberId(dimtype.ud1.Id, "Top")
'										Dim iUDTopMC As Integer = api.Members.GetMemberId(dimtype.ud1.Id, "MC")
'										Dim iUDTopENTE As Integer = api.Members.GetMemberId(dimtype.ud1.Id, "ENTES")
'										Dim elimMember As String = "None"
'										Dim elimMemberMC As String = "None"
'										Dim elimMemberENTE As String = "None"
'										Dim ParentAccountPL As Integer = api.Members.GetMemberId(dimtypeid.Account, "PG")
'										Dim ParentAccountCDR As Integer = api.Members.GetMemberId(dimtypeid.Account, "CDR")
'										Dim ParentAccountInvDesinv As Integer = api.Members.GetMemberId(dimtypeid.Account, "INVER_DESINV")
'										Dim ParentAccountCDCalc As Integer = api.Members.GetMemberId(dimtypeid.Account, "CTAS_CALC")
'										Dim ParentAccountCV As Integer = api.Members.GetMemberId(dimtypeid.Account, "CV_A")
'										Dim ParentAccountBS As Integer = api.Members.GetMemberId(dimtypeid.Account, "BS")
'										Dim	elimMemberId = api.Members.GetMemberId(dimTypeId.UD1, "None")
'										Dim	elimMemberIdMC = api.Members.GetMemberId(dimTypeId.UD1, "None")
'										Dim	elimMemberIdENTE = api.Members.GetMemberId(dimTypeId.UD1, "None")
'										Dim destAccountId As Integer = api.Members.GetMemberId(dimtypeid.Account,cell.DataBufferCellPk.GetAccountName(api))
'									
'										If Not api.Members.IsDescendant(accountDimPk, ParentAccountPL, destAccountId) _ 
'											And Not api.Members.IsDescendant(accountDimPk, ParentAccountCDR, destAccountId) _ 
'											And Not api.Members.IsDescendant(accountDimPk, ParentAccountInvDesinv, destAccountId) _ 
'											And Not api.Members.IsDescendant(accountDimPk, ParentAccountCDCalc, destAccountId) _ 
'											And Not api.Members.IsDescendant(accountDimPk, ParentAccountCV, destAccountId) _ 
'											And Not api.Members.IsDescendant(accountDimPk, ParentAccountBS, destAccountId) _ 
'											Or api.Pov.UD1.Name.Equals("None") Or api.Pov.UD2.Name.Equals("None") Then								
'											elimMember = "None"
'										Else
'											If cell.DataBufferCellPk.GetUD1Name(api) = cell.DataBufferCellPk.GetUD2Name(api) Then
'												elimMember = cell.DataBufferCellPk.GetUD1Name(api)
'											Else
'												If cell.DataBufferCellPk.GetUD1Name(api).Equals(cell.DataBufferCellPk.Item(sUD)) Then
												
'												Else
'														elimMember = "Elim_" & api.Members.GetFirstCommonParent(oUDDim, iUDTop, _ 
'																														  Me.GetUDId(cell, iUD), _
'																														  Me.GetUDasICId(api, cell, iICUD, iUD)).Name
'														elimMemberMC = "Elim_" & api.Members.GetFirstCommonParent(oUDDim, iUDTopMC, _ 
'																														  Me.GetUDId(cell, iUD), _
'																														  Me.GetUDasICId(api, cell, iICUD, iUD)).Name
'														elimMemberENTE = "Elim_" & api.Members.GetFirstCommonParent(oUDDim, iUDTopENTE, _ 
'																														  Me.GetUDId(cell, iUD), _
'																														  Me.GetUDasICId(api, cell, iICUD, iUD)).Name
'											    End If																	 
'											End If								
'										End If	
'										elimMemberId = api.Members.GetMemberId(dimTypeId.UD1, elimMember)																	  
'										elimMemberIdMC = api.Members.GetMemberId(dimTypeId.UD1, elimMemberMC)																	  
'										elimMemberIdENTE = api.Members.GetMemberId(dimTypeId.UD1, elimMemberENTE)	
										

										'fs end of U1#Elim_XXX calcuation
										'******************************************************************************************

									'----------------------------------------------------
					                ' Capitaux Propres
									' Equity accounts
					                '----------------------------------------------------
									
									If CaseElim="FFPP" Then
										'If consolidation method is not Holding ou (consolidation method is Holding and account is 106R)
										If method<>FSKMethod.HOLDING Then
											'fs disabled to use the UD4 member defined in text3 for account
											'If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											'If flow is DBE or DIV
											If flowMember.MemberId = flow.RPY.MemberId Or flowMember.MemberId = flow.Div.MemberId Then
												'If the entity is not consolidated in N-1
												If Own.pCon_1 = 0 Then	'Mehtode ENTRANTES							            													
													'Eliminate the Source account on the ENT flow at Own.PCON
													Cons.Book(cell,acctMember.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10041)
													'Trigger the account 106G on the flow ENT at Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flow.Ent.MemberId,IntercoDestMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10042)

													'Trigger the account 106M on the flow ENT at Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flow.Ent.MemberId,IntercoDestMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10043)
												
												'If the entity is consolidated in N-1	
												Else
													'If the consolidation method is Not Equity Pickup
													If method<>FSKMethod.MEE Then
														'Eliminate the source account on the source flow at Own.PCON of N-1
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon_1,,CaseElim,10044)
														
														'Trigger the source account on the VARC flow at change in Own.PCON
														Cons.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.VpCon,,CaseElim,10045)
														
														'Trigger the account 106G on the source flow at Own.POWN N-1
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,IntercoDestMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn_1 ,,CaseElim,10046)
														
														'Trigger the account 106G on the VARC flow at change in Own.POWN
														Cons.Book(cell,acct.RsvG.MemberId,flow.VarC.MemberId,IntercoDestMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpOwn ,,CaseElim,10047)
														
														'Trigger the account 106M on the source flow at Own.PMIN N-1
														Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,IntercoDestMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin_1 ,,CaseElim,10048)
														
														'Trigger the account 106G on the VARC flow at change in Own.PMIN
														Cons.Book(cell,acct.RsvM.MemberId,flow.VarC.MemberId,IntercoDestMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpMin ,,CaseElim,10049)
													'If consolidation method is Equity Pickup	
													Else
														'Eliminate the source account on the source flow (at 100%)													
														'fsm no lo eliminamos porque lo hemos desactivado de Shared
														'Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10051)
													
														'Trigger the account 106G on the source flow (at 100%)
														'Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10053)
														Cons.Book(cell,acct.RsvEq.MemberId,flowDestMember.MemberId,IntercoDestMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10053)
														'fsm hacemos la contrapartida con la cuenta 1BEQ
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,CaseElim,10059)
													End If	
												End If
											'Other Flows
											Else
												'If consolidation method is Not Equity Pickup
												If method<>FSKMethod.MEE Then
													'Eliminate the source account on the Destination flow at Own.PCON
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10055)
													'Trigger account 106G on Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,IntercoDestMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10056)
													'Trigger account 106M on Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,IntercoDestMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10057)
												'If consolidation method is Equity Pickup	
												Else
													'Eliminate the source account on the Destination flow (at 100%)
													'fsm no lo eliminamos porque lo hemos desactivado de Shared
													'Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.Pcon,,CaseElim,10058)
													'Trigger account 106G on the Destination flow (at 100%)
													'Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,CaseElim,10059)
													Cons.Book(cell,acct.RsvEq.MemberId,flowDestMember.MemberId,IntercoDestMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,CaseElim,10059)
													'fsm hacemos la contrapartida con la cuenta 1BEQ
													Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,CaseElim,10059)
												End If	
											End If
			                            End If

									'----------------------------------------------------
									' Cuentas de Subvenciones y cambios de Valor - fs
					                '----------------------------------------------------	
									
									ElseIf CaseElim="RIS" Then
										'If consolidation method is not Holding
										If method<>FSKMethod.HOLDING Then
											'fs disabled to use the UD4 member defined in text3 for account
											'If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											
											'If flow is DBE or DIV
											If flowMember.MemberId = flow.TDO.MemberId Or flowMember.MemberId = flow.TDM.MemberId Then
'											'Other Flows
												'If consolidation method is Not Equity Pickup
												If method<>FSKMethod.MEE Then
													'Elimina la cuenta de origen por el % de minoritarios
													'Eliminate the source account on the Destination flow at Own.PMIN 
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10055)
													
													'Genera la cuenta de intereses minoritarios al % Own.PMIN
													'Trigger account 2ATR on Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10057)
												End If	

											ElseIf flowMember.MemberId <> flow.Clo.MemberId Then
'											'Other Flows
												'If consolidation method is Not Equity Pickup
												If method<>FSKMethod.MEE Then
													'Elimina la cuenta de origen por el % de minoritarios
													'Eliminate the source account on the Destination flow at Own.PMIN 
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10055)
													
													'Genera la cuenta de intereses minoritarios al % Own.PMIN
													'Trigger account 2ATR on Destination flow at Own.PMIN
													Cons.Book(cell,acct.RisM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10057)
												Else
													'Elimina la cuenta de origen por el PCON
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,CaseElim,10059)
													'fsm hacemos la contrapartida con la cuenta 1BEQ
													Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,CaseElim,10059)
												End If	
											End If
			                            End If		
										
										ElseIf CaseElim="RIS1" Then
										'If consolidation method is not Holding
										If method<>FSKMethod.HOLDING Then
											'fs disabled to use the UD4 member defined in text3 for account
											'If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											
											'If flow is DBE or DIV
											If flowMember.MemberId = flow.TDO.MemberId Or flowMember.MemberId = flow.TDM.MemberId Then
'											'Other Flows
												'If consolidation method is Not Equity Pickup
												If method<>FSKMethod.MEE Then
													'Elimina la cuenta de origen por el % de minoritarios
													'Eliminate the source account on the Destination flow at Own.PMIN 
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10055)
													
													'Genera la cuenta de intereses minoritarios al % Own.PMIN
													'Trigger account 2ATR on Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10057)
												End If	

											ElseIf flowMember.MemberId <> flow.Clo.MemberId Then
'											'Other Flows
												'If consolidation method is Not Equity Pickup
												If method<>FSKMethod.MEE Then
													'Elimina la cuenta de origen por el % de minoritarios
													'Eliminate the source account on the Destination flow at Own.PMIN 
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10055)
													
													'Genera la cuenta de intereses minoritarios al % Own.PMIN
													'Trigger account 2ATR on Destination flow at Own.PMIN
													Cons.Book(cell,acct.Ris1M.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10057)
												Else
													'Elimina la cuenta de origen por el PCON
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,CaseElim,10059)
													'fsm hacemos la contrapartida con la cuenta 1BEQ
													Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,CaseElim,10059)
												End If	
											End If
			                            End If		
										
										ElseIf CaseElim="RIS2" Then
										'If consolidation method is not Holding
										If method<>FSKMethod.HOLDING Then
											'fs disabled to use the UD4 member defined in text3 for account
											'If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											
											'If flow is DBE or DIV
											If flowMember.MemberId = flow.TDO.MemberId Or flowMember.MemberId = flow.TDM.MemberId Then
'											'Other Flows
												'If consolidation method is Not Equity Pickup
												If method<>FSKMethod.MEE Then
													'Elimina la cuenta de origen por el % de minoritarios
													'Eliminate the source account on the Destination flow at Own.PMIN 
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10055)
													
													'Genera la cuenta de intereses minoritarios al % Own.PMIN
													'Trigger account 2ATR on Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10057)
												End If	

											ElseIf flowMember.MemberId <> flow.Clo.MemberId Then
'											'Other Flows
												'If consolidation method is Not Equity Pickup
												If method<>FSKMethod.MEE Then
													'Elimina la cuenta de origen por el % de minoritarios
													'Eliminate the source account on the Destination flow at Own.PMIN 
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10055)
													
													'Genera la cuenta de intereses minoritarios al % Own.PMIN
													'Trigger account 2ATR on Destination flow at Own.PMIN
													Cons.Book(cell,acct.Ris2M.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10057)
												Else
													'Elimina la cuenta de origen por el PCON
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,CaseElim,10059)
													'fsm hacemos la contrapartida con la cuenta 1BEQ
													Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,CaseElim,10059)
												End If	
											End If
			                            End If		
										
										ElseIf CaseElim="RIS3" Then
										'If consolidation method is not Holding
										If method<>FSKMethod.HOLDING Then
											'fs disabled to use the UD4 member defined in text3 for account
											'If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											
											'If flow is DBE or DIV
											If flowMember.MemberId = flow.TDO.MemberId Or flowMember.MemberId = flow.TDM.MemberId Then
'											'Other Flows
												'If consolidation method is Not Equity Pickup
												If method<>FSKMethod.MEE Then
													'Elimina la cuenta de origen por el % de minoritarios
													'Eliminate the source account on the Destination flow at Own.PMIN 
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10055)
													
													'Genera la cuenta de intereses minoritarios al % Own.PMIN
													'Trigger account 2ATR on Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10057)
												End If	

											ElseIf flowMember.MemberId <> flow.Clo.MemberId Then
'											'Other Flows
												'If consolidation method is Not Equity Pickup
												If method<>FSKMethod.MEE Then
													'Elimina la cuenta de origen por el % de minoritarios
													'Eliminate the source account on the Destination flow at Own.PMIN 
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10055)
													
													'Genera la cuenta de intereses minoritarios al % Own.PMIN
													'Trigger account 2ATR on Destination flow at Own.PMIN
													Cons.Book(cell,acct.Ris3M.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10057)
												Else
													'Elimina la cuenta de origen por el PCON
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,CaseElim,10059)
													'fsm hacemos la contrapartida con la cuenta 1BEQ
													Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,CaseElim,10059)
												End If	
											End If
			                            End If									
										
									'----------------------------------------------------
					                ' Account 120000: Current Year Earnings
					                '----------------------------------------------------	
									ElseIf CaseElim="UTIL" Then
										'fs disabled to use the UD4 member defined in text3 for account
										'If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
										
										'If flow is DBE
										If flowMember.MemberId = flow.RPY.MemberId Then
											'If consolidation method is Equity Pickup
											If method=FSKMethod.MEE Then
												'Eliminate the source account on the source flow (at 100%)
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.PCon,,CaseElim,10081)

												'Trigger account 120G on the source flow (at 100%)
'												Cons.Book(cell,acct.ResG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,CaseElim,10083) 'fs Account 2AGUE para P.Equiv
												Cons.Book(cell,acct.ResEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,CaseElim,10083)
		
											ElseIf method=FSKMethod.Holding Then
												'Eliminate the source account on the source flow at Own.PCON N-1
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon_1,,CaseElim,10075)
												
												'Eliminate the source account on the VARC flow at change in Own.PCON
												Cons.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.VpCon,,CaseElim,10076)
												
												'Trigger account 120G on the source flow at Own.POWN N-1
												Cons.Book(cell,acct.ResH.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn_1 ,,CaseElim,10077)
												
												'Trigger account 120G on the VARC flow at change in Own.POWN
												Cons.Book(cell,acct.ResH.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpOwn ,,CaseElim,10078)
												
												'Trigger account 120M on the source flow at Own.PMIN N-1
												Cons.Book(cell,acct.ResM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin_1 ,,CaseElim,10079)
												
												'Trigger account 120M on the VARC flow at change in Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpMin ,,CaseElim,10080)
												
											Else	
												'Eliminate the source account on the source flow at Own.PCON N-1
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon_1,,CaseElim,10075)
												
												'Eliminate the source account on the VARC flow at change in Own.PCON
												Cons.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.VpCon,,CaseElim,10076)
												
												'Trigger account 120G on the source flow at Own.POWN N-1
												Cons.Book(cell,acct.ResG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn_1 ,,CaseElim,10077)
												
												'Trigger account 120G on the VARC flow at change in Own.POWN
												Cons.Book(cell,acct.ResG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpOwn ,,CaseElim,10078)
												
												'Trigger account 120M on the source flow at Own.PMIN N-1
												Cons.Book(cell,acct.ResM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin_1 ,,CaseElim,10079)
												
												'Trigger account 120M on the VARC flow at change in Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpMin ,,CaseElim,10080)
											End If
								
											
											
										'Other Flows	
										Else
											'If consolidation method is Equity Pickup	
											If method=FSKMethod.MEE Then
												'Eliminate the source account on the Destination flow (at 100%)
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.PCon,,CaseElim,10088)

												'Trigger account 120G on the Destination flow (at 100%)
'												Cons.Book(cell,acct.ResG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,CaseElim,10089)
												Cons.Book(cell,acct.ResEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,CaseElim,10089)

											ElseIf method=FSKMethod.Holding Then
												
												'Eliminate the source account on the Destination flow at Own.PCON
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10085)

												'Trigger account 120G on the Destination flow at Own.POWN
												Cons.Book(cell,acct.ResH.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10086)
												
												'Trigger account 120M on the Destination flow at Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10087)
											Else
												'Eliminate the source account on the Destination flow at Own.PCON
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10085)

												'Trigger account 120G on the Destination flow at Own.POWN
												Cons.Book(cell,acct.ResG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10086)
												
												'Trigger account 120M on the Destination flow at Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10087)
											End If	
											
										End If
									'----------------------------------------------------
					                ' Account 129C / DIFCON Result
					                '----------------------------------------------------	
									
									ElseIf CaseElim="UTILC" Then										
										'fs disabled to use the UD4 member defined in text3 for account
										'If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
										
										'If flow is DBE	
										If flowMember.MemberId = flow.RPY.MemberId Then
											'If consolidation method is NOT Equity Pickup
											If method<>FSKMethod.MEE Then
												'Eliminate the source account on the source flow at Own.PCON N-1
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon_1,,CaseElim,10090)
												
												'Eliminate the source account on the VARC flow at change in Own.PCON
												Cons.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.VpCon,,CaseElim,10091)
												
												'Trigger account 120CG on the source flow at Own.POWN N-1
												Cons.Book(cell,acct.ResCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn_1 ,,CaseElim,10092)
												
												'Trigger account 120CG on the VARC flow at change in Own.POWN
												Cons.Book(cell,acct.ResCG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpOwn ,,CaseElim,10093)
												
												'Trigger account 120CM on the source flow at Own.PMIN N-1
												Cons.Book(cell,acct.ResCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin_1 ,,CaseElim,10094)
												
												'Trigger account 120CM on the VARC flow at change in Own.PMIN
												Cons.Book(cell,acct.ResCM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpMin ,,CaseElim,10095)
											'If consolidation method is Equity Pickup
											Else
												'Eliminate the source account on the source flow (at 100%)
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.PCon,,CaseElim,10096)

												'Trigger account 120CG on the source flow (at 100%)
												Cons.Book(cell,acct.ResCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,CaseElim,10098)

											End If
										'Other Flows	
										Else
											'If consolidation method is NOT Equity Pickup
											If method<>FSKMethod.MEE Then
												'Eliminate the source account on the Destination flowe at Own.PCON
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10100)

												'Trigger account 120CG on Destination flow at Own.POWN
												Cons.Book(cell,acct.ResCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10101)
												
												'Trigger account 120CM on Destination flow at Own.PMIN
												Cons.Book(cell,acct.ResCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10102)
											'If consolidation method is Equity Pickup	
											Else
												'Eliminate source account on Destination flow (at 100%)
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.PCon,,CaseElim,10103)

												'Trigger account 120CG on Destination flow (at 100%)
												Cons.Book(cell,acct.ResCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,CaseElim,10104)
											End If	
										End If	
				                    '----------------------------------------------------
				                    ' Compte EA: compte technique des Ecarts d'acquisition
									' Account EA: technical account for Goodwill
				                    '----------------------------------------------------
									
									ElseIf CaseElim="EA" Then
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											Dim acctGW As Member = api.Account.GetPlugAccount(cell.DataBufferCellPk.AccountId)
											
											'If consolidation method is Equity Pickup or source nature is Equity
											'If method=3 Or NatureSource.Name="Equity" Then acctGW = acct.GWMEE
											If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then acctGW = acct.GWMEE
											
											'If flows is INC
											If flowMember.MemberId = flow.INC.MemberId Then
												'If the entity is not consolidated in N-1
												If Own.pCon_1 = 0 Then
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then
														
														'Trigger account 204000 on flow ENT at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10106)
														
														'Trigger account 106G on flow ENT at Own.POWN
														Cons.Book(cell,acct.RsvG.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10107)
														
		                        						'Trigger account 106M on flow ENT at Own.PMIN
														Cons.Book(cell,acct.RsvM.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10108)
													'If Nature is Minoritaires (NatureDim)	
													Else
													
														'Trigger account 106G on flow ENT (at -100%)
														Cons.Book(cell,acct.RsvG.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10110)
														
		                        						'Trigger account 106M on flow ENT (at 100%)
														Cons.Book(cell,acct.RsvM.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10111)
													End If
												'If the entity is consolidated in N-1	
												Else
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then
														
														'Trigger account 204000 on flow VARC at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10113)
														
														'Trigger account 106G on flow VARC at Own.POWN
														Cons.Book(cell,acct.RsvG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10114)
														
		                        						'Trigger account 106M on flow VARC at Own.PMIN
														Cons.Book(cell,acct.RsvM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10115)
													'If Nature is Minoritaires (NatureDim)	
													Else
													
		                        						'Trigger account 106G on flow VARC at -100%
														Cons.Book(cell,acct.RsvG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10117)
														
		                        						'Trigger account 106M on flow VARC at 100%
														Cons.Book(cell,acct.RsvM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10118)
													End If	
												End If
											'If flows is DIS
											ElseIf flowMember.MemberId = flow.DEC.MemberId Then
												'If the entity is not consolidated in N-1
												If Own.pCon_1 = 0 Then
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then
													
														'Trigger account 204000 on flow ENT at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10120)
														
														'Trigger account 106G on flow ENT at Own.POWN
														Cons.Book(cell,acct.RsvG.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,CaseElim,10121)
														
		                        						'Trigger account 106M on flow ENT at Own.PMIN
														Cons.Book(cell,acct.RsvM.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin ,,CaseElim,10122)
													'If Nature is Minoritaires (NatureDim)	
													Else
														'Eliminate source record for Stage consolidations (at 100%)						
														'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,CaseElim,10123)
													
														'Trigger account 106G on flow ENT (at -100%)
														Cons.Book(cell,acct.RsvG.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,CaseElim,10124)
														
		                        						'Genere le compte 106M sur le flux ENT (a 100%)
														'Trigger account 106M on flow ENT (at 100%)
														Cons.Book(cell,acct.RsvM.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,CaseElim,10125)
													End If
												'Si l'entite est consolidee en N-1
												'If the entity is consolidated in N-1	
												Else
													'Si la nature (NatureDim) n'est pas Minoritaires
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then
														'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
														'Eliminate source record for Stage consolidations (at 100%)						
														'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,,,,,,-1,,CaseElim,10126)
														
														'Genere le compte 204000 sur le flux VARC a Own.PCON
														'Trigger account 204000 on flow VARC at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10127)
														
														'Genere le compte 106G sur le flux VARC a Own.POWN
														'Trigger account 106G on flow VARC at Own.POWN
														Cons.Book(cell,acct.RsvG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,CaseElim,10128)
														
		                        						'Genere le compte 106M sur le flux VARC a Own.PMIN
														'Trigger account 106M on flow VARC at Own.PMIN
														Cons.Book(cell,acct.RsvM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin ,,CaseElim,10129)
													'Pour la Nature Minoritaires (NatureDim)
													'If Nature is Minoritaires (NatureDim)	
													Else
												
		                        						'Genere le compte 106G sur le flux VARC a -100%
														'Trigger account 106G on flow VARC at -100%
														Cons.Book(cell,acct.RsvG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,CaseElim,10131)
														
		                        						'Genere le compte 106M sur le flux VARC a 100%
														'Trigger account 106M on flow VARC at 100%
														Cons.Book(cell,acct.RsvM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,CaseElim,10132)
													End If	
												End If
											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF	
											ElseIf flowMember.MemberId = flow.TDO.MemberId Or flowMember.MemberId = flow.TDM.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
														
													'Genere le compte 204000 sur le flux Destination a Own.PCON
													'Trigger account 204000 on the Destination flow at Own.PCON
													Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10134)
														
													'Genere le compte 106CG sur le flux de Destination a Own.POWN
													'Trigger account 106CG on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10135)
													
	                        						'Genere le compte 106CM sur le flux de Destination a Own.PMIN
													'Trigger account 106CM on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10136)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
												
	                        						'Genere le compte 106CG sur le flux de Destination a -100%
													'Trigger account 106CG on the Destination flow at -100%
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10138)
													
	                        						'Genere le compte 106CG sur le flux de Destination a 100%
													'Trigger account 106CG on the Destination flow at 100%
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10139)
												End If																																		
											'Pour les autres flux
											'For other flows
											Else
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'brapi.errorlog.LogMessage(si, "Account GW " & acctGW.Name)	
													'Genere le compte 204000 sur le flux Destination a Own.PCON
													'Trigger account 204000 on the Destination flow at Own.PCON
													Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10141)
													'Cons.Book(cell,"A#2FCC_CO",flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10141)
														
													'Genere le compte 106G sur le flux Destination a Own.POWN
													'Trigger account 106G on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10142)
													
	                        						'Genere le compte 106G sur le flux Destination a Own.PMIN
													'Trigger account 106G on the Destination flow at Own.PMIN	
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10143)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
													
	                        						'Genere le compte 106G sur le flux Destination (a -100%)
													'Trigger account 106G on the Destination flow (at -100%)
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10145)
													
	                        						'Genere le compte 106M sur le flux Destination (a 100%)
													'Trigger account 106M on the Destination flow (at 100%)
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10146)
												End If	
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the Minority Interest portion on the Minoritaires Nature 
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10147)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10148)
											End If	
			                            End If	'Can Eliminate
									'----------------------------------------------------
					                ' Compte EAEH: Cumul des ecarts de conversion sur Ecarts d'acquisition
									' Account EAEH: CTA on Goodwill
					                '----------------------------------------------------	
									ElseIf CaseElim="EAH" Then
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si le flux est OUV/VARC/SOR
											'If flow is OUV/VARC/SOR
	                						If flowMember.MemberId = flow.OPE.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.EXI.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
	                    						If NatureSource.Name<>"Minoritaires" Then
	                        						'Genere le compte 106G sur le flux Destination a -Own.POWN
													'Trigger account 106G on the Destination flow at -Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10150)
													
													'Genere le compte 106CG sur le flux Destination a Own.POWN
													'Trigger account 106CG on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10151)
													
													'Genere le compte 106CG sur le flux Destination a -Own.PMIN
													'Trigger account 106CG on the Destination flow at -Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10152)
													
													'Genere le compte 106CM sur le flux Destination a Own.PMIN
													'Trigger account 106CM on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10153)
	                    						'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)
												Else													
													'Genere le compte 106G sur le flux Destination (a 100%)
													'Trigger account 106G on the Destination flow (at 100%)
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10154)
													
													'Genere le compte 106CG sur le flux Destination (a -100%)
													'Trigger account 106CG on the Destination flow (at -100%)
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10155)
													
													'Genere le compte 106M sur le flux Destination (a -100%)
													'Trigger account 106M on the Destination flow (at -100%)
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10156)
													
													'Genere le compte 106CM sur le flux Destination (a 100%)
													'Trigger account 106CM on the Destination flow (at 100%)
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10157)
													
	                    						End If
											End If	'CurCustom4<>Mino
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10158)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1- Own.pCon),,CaseElim,10159)
											End If	
										End If
									'----------------------------------------------------
					                ' Compte AEA: Amortissement des ecarts d'acquisition
									' Account AEA: Goodwill amortization
					                '----------------------------------------------------	
									ElseIf CaseElim="AEA" Then
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											Dim acctGWA As Member = api.Account.GetPlugAccount(cell.DataBufferCellPk.AccountId)
											Dim aDAmEA As Member = api.Members.GetMember(DimType.Account.Id, "680400")
											
											'Si la methode est Mise en Equivalence ou la nature source est Equity
											'If consolidation method is Equity Pickup or source nature is Equity
											'If method=3 Or NatureSource.Name="Equity" Then acctGWA = acct.GWAMEE	
											If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then acctGWA = acct.GWAMEE	
											
											'If flows is ECO or ECF	
	                						If flowMember.MemberId = flow.TDO.MemberId Or flowMember.MemberId = flow.TDM.MemberId Then
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Eliminate source record for Stage consolidations at Own.PCON 						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-Own.pCon,,CaseElim,10170)
														
													'Trigger account 2904000/2904EQUI on the Destination flow at Own.PCON
													Cons.Book(cell,acctGWA.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10171)
												End If	
											Else
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Eliminate source record for Stage consolidations at Own.PCON						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-Own.pCon,,CaseElim,10172)

													If method=FSKMethod.HOLDING Then													
														Cons.Book(cell,acct.RsvH.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,CaseElim,10173)
													Else	
														'Trigger account 106G on the Destination flow at Own.POWN
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,CaseElim,10173)
														
														'Trigger account 106M on the Destination flow at Own.PMIN
														Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,CaseElim,10174)
													End If
													
													'Trigger account 2904000/2904EQUI on the Destination flow at Own.PCON
													Cons.Book(cell,acctGWA.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10175)
												'If Nature is Minoritaires (NatureDim)
												Else
													'Trigger account 106G on the Destination flow at 100%
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10177)
													'Trigger account 106M on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10178)
												End If	
	                						End If

										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10179)
											
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1-Own.pCon),,CaseElim,10180)
											End If	
										End If	'Can Eliminate
									'----------------------------------------------------
					                ' Compte AEAEH: Cumul des ecarts de conversion sur amortissement des ecarts d'acquisition
									' Account AEAEH: CTA on Goodwill amortization
					                '----------------------------------------------------	
									
									ElseIf CaseElim="AEAH" Then
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
											'Eliminate source record for Stage consolidations (at 100%)						
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,CaseElim,10181)
											
											'Si le flux est OUV/VARC/SOR
											'If flow is OUV/VARC/SOR
											If flowMember.MemberId = flow.OPE.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.EXI.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
	                    						If NatureSource.Name<>"Minoritaires" Then
	                        						'Genere le compte 106G sur le flux Destination a Own.POWN
													'Trigger account 106G on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10182)
													
													'Genere le compte 106CG sur le flux Destination a -Own.POWN
													'Trigger account 106CG on the Destination flow at -Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10183)
													
													'Genere le compte 106M sur le flux Destination a Own.PMIN
													'Trigger account 106M on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10184)
													
													'Genere le compte 106CM sur le flux Destination a -Own.PMIN
													'Trigger account 106CM on the Destination flow at -Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10185)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)
	                    						Else
													'Genere le compte 106G sur le flux Destination a -100%
													'Trigger account 106G on the Destination flow at -100%
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10186)
													
													'Genere le compte 106CG sur le flux Destination a 100%
													'Trigger account 106CG on the Destination flow at 100%
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10187)
													
													'Genere le compte 106M sur le flux Destination a 100%
													'Trigger account 106M on the Destination flow at 100%
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10188)
													
													'Genere le compte 106CG sur le flux Destination a -100%
													'Trigger account 106CG on the Destination flow at -100%
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10189)
													
	                    						End If 'CurCustom4<>Mino
											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF	
											ElseIf flowMember.MemberId = flow.TDO.MemberId Or flowMember.MemberId = flow.TDM.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													
								                    'Genere le compte 106CG sur le flux Destination a -Own.POWN
													'Trigger account 106CG on the Destination flow at -Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10190)
													
								                    'Genere le compte 106CM sur le flux Destination a -Own.PMIN
													'Trigger account 106CM on the Destination flow at -Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10191)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
								                    'Genere le compte 106CG sur le flux Destination a 100%
													'Trigger account 106CG on the Destination flow at 100%
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10192)
													
								                    'Genere le compte 106CG sur le flux Destination a -100%
													'Trigger account 106CG on the Destination flow at -100%
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10193)
													
												End If	'CurCustom4<>Mino
											
											'Si le flux est ECR
											'If the flow is ECR	
											ElseIf flowMember.MemberId = flow.TDR.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
								                    'Genere le compte 120CG sur le flux ECF a -Own.POWN
													'Trigger account 120CG on the ECF flow at -Own.POWN
													Cons.Book(cell,acct.ResCG.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10194)
													
								                    'Genere le compte 120CM sur le flux ECF a -Own.PMIN
													'Trigger account 120CM on the ECF flow at -Own.PMIN
													Cons.Book(cell,acct.ResCM.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10195)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else																			
								                    'Genere le compte 120CG sur le flux ECF a 100%
													'Trigger account 120CG on the ECF flow at 100%
													Cons.Book(cell,acct.ResCG.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10196)
													
								                    'Genere le compte 120CM sur le flux ECF a -100%
													'Trigger account 120CM on the ECF flow at -100%
													Cons.Book(cell,acct.ResCM.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10197)
													
												End If	'CurCustom4<>Mino
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10198)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1-Own.pCon),,CaseElim,10199)
											End If	
										End If
									'----------------------------------------------------
					                ' Comptes de provisions intragroupe
									' Intercompany allowances accounts
					                '----------------------------------------------------	
									ElseIf CaseElim="DETP" Then
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'If the flow is DEXP/DFIN
											If flowMember.MemberId = flow.TDO.MemberId Or flowMember.MemberId = flow.TDM.MemberId Then
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
														'Trigger account 261EQUI on the Destination flow at Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10242)
													
													Else
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10243)
														Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,CaseElim,10244)
														Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin ,,CaseElim,10245)
													
													End If
												End If	
											Else
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													If method=FSKMethod.HOLDING Then
														Cons.Book(cell,acct.RsvH.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,CaseElim,10244)
													Else
														'Trigger account 106G on the Destination flow at Own.POWN
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,CaseElim,10244)
														
														'Trigger account 106M on the Destination flow at Own.PMIN
														Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin ,,CaseElim,10245)
													End If
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
														'Trigger account 261EQUI on the Destination flow at Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10247)
													
													Else
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10248)
													End If	
												'If Nature is Minoritaires (NatureDim)										                    
												Else

													'Trigger account 106G on the Destination flow at -100%
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10250)
													
													'Trigger account 106M on the Destination flow at 100%
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10251)
												End If	'CurCustom4<>Mino
											End If
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10253)
											End If	
										End If	'Can Eliminate
										
									'----------------------------------------------------
					                ' Cuentas de PyG de dotación de provisiones cartera- Rutinas ProvInc y ProvDis
									' PnL accounts for investment impairments
					                '----------------------------------------------------	
										
									ElseIf CaseElim="DETPDIS" Then

										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si la nature (NatureDim) n'est pas Minoritaires
											'If nature (NatureDim) is not Minoritaires
											If NatureSource.Name<>"Minoritaires" Then
												'Genere le compte 120G sur le flux RES a -Own.POWN
												'Trigger account 120G on the RES flow at -Own.POWN
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10282)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10282)
												End If	
							                    'Genere le compte 120M sur le flux RES a -Own.PMIN
												'Trigger account 120M on the RES flow at -Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10283)
												
												If method=FSKMethod.HOLDING Then													
													Cons.Book(cell,acct.RsvH.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,CaseElim,10284)
												Else
													'Genere le compte 106CG sur le flux DIV a -Own.POWN
													'Trigger account 106CG on the DIV flow at -Own.POWN
	'FS												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,CaseElim,10284)
													Cons.Book(cell,acct.RsvG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,CaseElim,10284)
													
													'Genere le compte 106CM sur le flux DIV a -Own.PMIN
													'Trigger account 106CM on the DIV flow at -Own.PMIN
	'FS												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,CaseElim,10285)
													Cons.Book(cell,acct.RsvM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,CaseElim,10285)
												End If
												
												'Si la methode est Mise en Equivalence ou la nature source est Equity
												'If consolidation method is Equity Pickup or source nature is Equity
												'If method=3 Or NatureSource.Name="Equity" Then
												If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
													'Genere le compte 880EQUI sur le flux None a -Own.PCON
													'Trigger account 880EQUI on the None flow at -Own.PCON
													
													Cons.Book(cell,acct.ResMEE.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10287)
	
														
			                    					'Genere le compte 261EQUI sur le flux RES a -Own.PCON
													'Trigger account 261EQUI on the RES flow at -Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10288)
													
			                    					'Genere le compte 261EQUI sur le flux REC a Own.PCON
													'Trigger account 261EQUI on the REC flow at Own.PCON
													'Cons.Book(cell,acct.MEE.MemberId,flow.Rec.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10289)
													'NOVA Añadido para que lleve el decremento del deterioro a reservas al flujo F35
													Cons.Book(cell,acct.MEE.MemberId,flow.DETPDIS.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10289)
												
												Else
													'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
													'Eliminate source record For Stage consolidations at Own.PCON
													
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10290)

												End If
											'Pour la Nature Minoritaires (NatureDim)
											'If Nature is Minoritaires (NatureDim)	
											Else

												'Genere le compte 120G sur le flux RES a 100%
												'Trigger account 120G on the RES flow at 100%
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10292)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10292)
												End If	
							                    'Genere le compte 120M sur le flux RES a -100%
												'Trigger account 120M on the RES flow at -100%
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10293)
												
												'Genere le compte 106CG sur le flux DIV a 100%
												'Trigger account 106CG on the DIV flow at 100%
												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,CaseElim,10294)
												
												'Genere le compte 106CG sur le flux DIV a -100%
												'Trigger account 106CG on the DIV flow at -100%
												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,CaseElim,10295)
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10296)
												
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10297)
										End If
										End If											
									
									ElseIf CaseElim="DETPINC" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										Dim icpOwn As New OwnershipClass(si,api,globals, icName,api.Pov.Parent.Name,,,OpeScenario)
 
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
												'Rsult impact on BS												
												Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10282)
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10283)
 
												'Cons.Book(cell,acct.RsvG.MemberId,flow.Rec.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,CaseElim,10284)
												'Cons.Book(cell,acct.RsvM.MemberId,flow.Rec.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,CaseElim,10285)
												'NOVA Añadido para que lleve el INC del deterioro a reservas al flujo F25
												Cons.Book(cell,acct.RsvG.MemberId,flow.DETPINC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,CaseElim,10284)
												Cons.Book(cell,acct.RsvM.MemberId,flow.DETPINC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,CaseElim,10285)

 
												'If consolidation method is Equity Pickup or source nature is Equity
												If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
													Cons.Book(cell,acct.ResMEE.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,CaseElim,10287)
													Cons.Book(cell,acct.MEE.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,CaseElim,10288)
													'Cons.Book(cell,acct.MEE.MemberId,flow.Rec.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10289)
													Cons.Book(cell,acct.MEE.MemberId,flow.DETPINC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10289)
												
												Else
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10290)
												End If
 
 
											'Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10296)
											Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10296)
 
											If method=FSKMethod.MEE Then
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10297)
											End If
										End If	
									'----------------------------------------------------
									' CTA on intercompany Allowance accounts
					                '----------------------------------------------------	
									ElseIf CaseElim="PROVH" Then		
			
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'If flow is OUV/VARC/SOR
											If flowMember.MemberId = flow.OPE.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.EXI.MemberId Then
												'If nature (NatureDim) is not Minoritaires
		                						If NatureSource.Name<>"Minoritaires" Then
													
													'Trigger account 106G on the Destination flow at -Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10255)
													
													'Trigger account 106CG on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10256)
													
													'Trigger account 106M on the Destination flow at -Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10257)
													
													'Trigger account 106CM on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10258)

												'If Nature is Minoritaires (NatureDim)	
												Else
												
													'Trigger account 106G on the Destination flow at 100%
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10260)
													
													'Trigger account 106CG on the Destination flow at -100%
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10261)
													
													'Trigger account 106M on the Destination flow at -100%
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10262)
													
													'Trigger account 106CM on the Destination flow at 100%
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10263)
													
		                						End If 'CurCustom4<>Mino
											'If flows is ECO or ECF	
											ElseIf flowMember.MemberId = flow.TDO.MemberId Or flowMember.MemberId = flow.TDM.MemberId Then
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													
													'Trigger account 106CG on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10265)

													'Trigger account 106CG on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10266)

												'If Nature is Minoritaires (NatureDim)	
												Else
												
													'Trigger account 106CG on the Destination flow at -100%
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10268)
													
													'Trigger account 106CM on the Destination flow at 100%
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10269)
													
												End If	'CurCustom4<>Mino
											'If the flow is ECR
											ElseIf flowMember.MemberId = flow.TDR.MemberId Then
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
												
													'Trigger account 120CG on the ECF flow at Own.POWN
													Cons.Book(cell,acct.ResCG.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10271)
													
													'Trigger account 120CM on the ECF flow at Own.PMIN
													Cons.Book(cell,acct.ResCM.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10272)
												'If Nature is Minoritaires (NatureDim)														
												Else
													'Trigger account 120CG on the ECF flow at -100%
													Cons.Book(cell,acct.ResCG.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10274)

													'Trigger account 120CM on the ECF flow at 100%
													Cons.Book(cell,acct.ResCM.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10275)
													
												End If	'CurCustom4<>Mino
											End If

										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10276)
											
											'If consolidation method is Equity Pickup
											If method=FSKMethod.MEE Then
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10277)
											End If	
										End If	'Can Eliminate
										
									'fs DIVA works like DIVR but uses EnlaDiv instead of conso reserves	
									ElseIf CaseElim="DIVA" Then

										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'If nature (NatureDim) is not Minoritaires
											If NatureSource.Name<>"Minoritaires" Then
												'Trigger account 120G on the RES flow at -Own.POWN
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10282)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10282)
												End If	
												
												'Trigger account 120M on the RES flow at -Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10283)
												Cons.Book(cell,acct.EnlaDiv.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,CaseElim,10284)
												
												Cons.Book(cell,acct.RsvM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,CaseElim,10285)
												
												'If consolidation method is Equity Pickup or source nature is Equity
												If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
													'Trigger account 880EQUI on the None flow at -Own.PCON
													Cons.Book(cell,acct.ResMEE.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10287)
												
													'Trigger account 261EQUI on the RES flow at -Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10288)
													
													'Trigger account 261EQUI on the REC flow at Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Rec.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10289)
												Else
													'Eliminate source record for Stage consolidations at Own.PCON
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10290)
												End If

											'If Nature is Minoritaires (NatureDim)	
											Else

												'Trigger account 120G on the RES flow at 100%
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10292)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10292)
												End If	
												'Trigger account 120M on the RES flow at -100%
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10293)
												
												'Trigger account 106CG on the DIV flow at 100%
												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,CaseElim,10294)
												
												'Trigger account 106CG on the DIV flow at -100%
												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,CaseElim,10295)
											End If
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10296)

											'If consolidation method is Equity Pickup
											If method=FSKMethod.MEE Then
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10297)
												End If
										End If	
									
									ElseIf CaseElim="DIVR" Then
									'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si la nature (NatureDim) n'est pas Minoritaires
											'If nature (NatureDim) is not Minoritaires
											If NatureSource.Name<>"Minoritaires" Then
												'Genere le compte 120G sur le flux RES a -Own.POWN
												'Trigger account 120G on the RES flow at -Own.POWN
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10282)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10282)
												End If	
												
							                    'Genere le compte 120M sur le flux RES a -Own.PMIN
												'Trigger account 120M on the RES flow at -Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10283)
												
												'Genere le compte 106CG sur le flux DIV a -Own.POWN
												'Trigger account 106CG on the DIV flow at -Own.POWN
												'FSCons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,CaseElim,10284)
												If method=FSKMethod.Holding Then												
													Cons.Book(cell,acct.RsvH.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,CaseElim,10284)
												Else
													Cons.Book(cell,acct.RsvG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,CaseElim,10284)
												End If
												
												'Trigger account 106CM on the DIV flow at -Own.PMIN
												Cons.Book(cell,acct.RsvM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,CaseElim,10285)
												
												'Si la methode est Mise en Equivalence ou la nature source est Equity
												'If consolidation method is Equity Pickup or source nature is Equity
												'If method=3 Or NatureSource.Name="Equity" Then
												If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
													'Genere le compte 880EQUI sur le flux None a -Own.PCON
													'Trigger account 880EQUI on the None flow at -Own.PCON
													Cons.Book(cell,acct.ResMEE.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10287)
				                    					'Genere le compte 261EQUI sur le flux RES a -Own.PCON
													'Trigger account 261EQUI on the RES flow at -Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10288)
													
			                    					'Genere le compte 261EQUI sur le flux REC a Own.PCON
													'Trigger account 261EQUI on the REC flow at Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Rec.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10289)
												Else
													'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
													'Eliminate source record for Stage consolidations at Own.PCON
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10290)
												End If
											'Pour la Nature Minoritaires (NatureDim)
											'If Nature is Minoritaires (NatureDim)	
											Else

												'Genere le compte 120G sur le flux RES a 100%
												'Trigger account 120G on the RES flow at 100%
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10292)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10292)
												End If	
							                    'Genere le compte 120M sur le flux RES a -100%
												'Trigger account 120M on the RES flow at -100%
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10293)
												
												'Genere le compte 106CG sur le flux DIV a 100%
												'Trigger account 106CG on the DIV flow at 100%
												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,CaseElim,10294)
												
												'Genere le compte 106CG sur le flux DIV a -100%
												'Trigger account 106CG on the DIV flow at -100%
												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,CaseElim,10295)
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10296)
												
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10297)
											End If
										End If	
									ElseIf CaseElim="DIVRH" Then
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si le flux est ECF
											'If the flow is ECF
											If flowMember.MemberId = flow.TDM.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then

													'Genere le compte 120CG sur le flux ECF a -Own.POWN
													'Trigger account 120CG on the ECF flow at -Own.POWN
													Cons.Book(cell,acct.ResCG.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10299)
													
								                    'Genere le compte 120CM sur le flux ECF a -Own.PMIN
													'Trigger account 120CM on the ECF flow at -Own.PMIN
													Cons.Book(cell,acct.ResCM.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10300)
													
													'Genere le compte 106CG sur le flux ECF a Own.POWN
													'Trigger account 106CG on the ECF flow at Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10301)
													
													'Genere le compte 106CM sur le flux ECF a Own.PMIN
													'Trigger account 106CM on the ECF flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10302)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
												
													'Genere le compte 120CG sur le flux ECF a 100%
													'Trigger account 120CG on the ECF flow at 100%
													Cons.Book(cell,acct.ResCG.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10304)
													
								                    'Genere le compte 120CM sur le flux ECF a -100%
													'Trigger account 120CM on the ECF flow at -100%
													Cons.Book(cell,acct.ResCM.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10305)
													
													'Genere le compte 106CG sur le flux ECF a -100%
													'Trigger account 106CG on the ECF flow at -100%
													Cons.Book(cell,acct.RsvCG.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10306)
													
													'Genere le compte 106CM sur le flux ECF a 100%
													'Trigger account 106CM on the ECF flow at 100%
													Cons.Book(cell,acct.RsvCM.MemberId,flow.TDM.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10307)
												End If
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10308)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10309)
											End If
												
										End If	
									ElseIf CaseElim="DIVRVAR" Then
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si le flux est OUV/VARC/SOR
											'If flow is OUV/VARC/SOR		
											If flowMember.MemberId = flow.OPE.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.EXI.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Genere le compte 106G sur le flux Destination a -Own.POWN
													'Trigger account 106G on the Destination flow at -Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10311)
													
								                    'Genere le compte 106M sur le flux Destination a Own.PMIN
													'Trigger account 106M on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,CaseElim,10312)
													
													'Genere le compte 106CG sur le flux Destination a Own.POWN
													'Trigger account 106CG on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,CaseElim,10313)
													
													'Genere le compte 106CM sur le flux Destination a Own.PMIN
													'Trigger account 106CM on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10314)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
													'Genere le compte 106G sur le flux Destination a 100%
													'Trigger account 106G on the Destination flow at 100%
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10315)
													
								                    'Genere le compte 106M sur le flux Destination a -100%
													'Trigger account 106M on the Destination flow at -100%
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10316)
													
													'Genere le compte 106CG sur le flux Destination a -100%
													'Trigger account 106CG on the Destination flow at -100%
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10317)
													
													'Genere le compte 106CM sur le flux Destination a 100%
													'Trigger account 106CM on the Destination flow at 100%
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,CaseElim,10318)
												End If
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10319)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10320)
											End If
												
										End If	
									
									ElseIf CaseElim="ADIVV" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											
											'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
											'Eliminate source record for Stage consolidations (at 100%)
											Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,,,,,,-1,,CaseElim,10321)
																							
										End If
										
									ElseIf CaseElim="ADIVR" Then
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si la nature (NatureDim) n'est pas Minoritaires
											'If nature (NatureDim) is not Minoritaires
											If NatureSource.Name<>"Minoritaires" Then
												'Trigger account 120G on the RES flow at -Own.POWN
												Cons.Book(cell,acct.EnlaDiv.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon,,CaseElim,10322) 'NPXX POwn -> PCon
												
												'Trigger account 120M on the RES flow at -Own.PMIN
												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10324) 'NPXX POwn -> PCon
												
												'Trigger account 106CM on the ADIV flow at -Own.PMIN
												'If consolidation method is Equity Pickup or source nature is Equity
												'If method=3 Or NatureSource.Name="Equity" Then
												If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
													'Trigger account 880EQUI on the None flow at -Own.PCON
													Cons.Book(cell,acct.ResMEE.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10327)
													
													'Trigger account 261EQUI on the RES flow at -Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10328)
													
													'Trigger account 261EQUI on the REC flow at Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Rec.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,CaseElim,10329)
												Else
													'Eliminate source record for Stage consolidations at Own.PCON
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10330)
												End If

											'If Nature is Minoritaires (NatureDim)	
											Else
												
												'Trigger account 120G on the RES flow at 100%
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10282)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,CaseElim,10282)
												End If	
												
												'Trigger account 120M on the RES flow at -100%
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10333)
												
												'Trigger account 106CG on the ADIV flow at 100%
												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10334)
												
												'Trigger account 106CM on the ADIV flow at -100%
												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,CaseElim,10335)
											End If

										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10336)
											
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10337)
											End If
										End If
										
									ElseIf CaseElim="RAS" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
																					
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature					
										End If
									ElseIf CaseElim="DIVVH" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
					
										End If

									'----------------------------------------------------
									' Goodwill (impact the subsidiary entity reserves)
					                '----------------------------------------------------	
									ElseIf CaseElim="GW" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										Dim icpOwn As New OwnershipClass(si,api,globals, icName,,,,OpeScenario)
										Dim acctGW As Member = api.Account.GetPlugAccount(cell.DataBufferCellPk.AccountId)
										
										
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'If flow is AUG or DIM (not a problem target flow is always flowDestMember)
											If flowMember.MemberId = flow.INC.MemberId Or flowMember.MemberId = flow.DEC.MemberId  Then
												'If the ICP is not consolidated in N-1
												If ICPOwn.pCon_1 = 0 Then
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then												
												
														'If consolidation method is Equity Pickup or source nature is Equity
														If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
															'Trigger account 261EQUI on the Destination flow at -Own.PCON
															Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,CaseElim,10350)
															Cons.Book(cell,acct.RsvEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10348)
														'If the consolidation method is not Equity Pickup or the nature is not Equity	
														Else
															'Eliminate source record for Stage consolidations at Own.PCON
															Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,CaseElim,10351)
															Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10348)
														End If
													'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding
													'ElseIf icMethod <> 1 Then
													ElseIf icMethod <> FSKMethod.HOLDING Then
														'Eliminate source record for Stage consolidations (at 100%)						
														Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,1,,CaseElim,10352)
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10348)
													End If
												'If the ICP is consolidated in N-1	
												Else
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then												
																												
														'If consolidation method is Equity Pickup or source nature is Equity
														If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														
															'Trigger account 261EQUI on the Destination flow at -Own.PCON
															Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,CaseElim,10355)
															Cons.Book(cell,acct.RsvEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10353)

														'If the consolidation method is not Equity Pickup or the nature is not Equity	
														Else
															'Eliminate source record for Stage consolidations at Own.PCON
															Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,CaseElim,10356)
															Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10353)
														End If
													'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding	
													ElseIf icMethod <> FSKMethod.HOLDING Then
														'Eliminate source record for Stage consolidations (at 100%)						
														Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,1,,CaseElim,10357)
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10353)
														
													End If
												End If
											
											'If flows is ECO or ECF
											ElseIf flowMember.MemberId = flow.TDO.MemberId Or flowMember.MemberId = flow.TDM.MemberId Then
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													
													'If consolidation method is Equity Pickup or source nature is Equity
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														'Trigger account 261EQUI on the Destination flow at -Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,CaseElim,10370)
														Cons.Book(cell,acct.RsvEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10368)
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,CaseElim,10371)
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10368)
													End If
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding															
												'ElseIf icMethod <> 1 Then
												ElseIf icMethod <> FSKMethod.HOLDING Then
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,1,,CaseElim,10372)
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10368)
														
												End If
											
											'For other flows	
											Else
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then

													'If consolidation method is Equity Pickup or source nature is Equity
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then													
														'Trigger account 261EQUI on the Destination flow at -Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,CaseElim,10375)
														Cons.Book(cell,acct.RsvEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10373)
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,CaseElim,10376)
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10373)
													End If
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding															
												'ElseIf icMethod <> 1 Then	
												ElseIf icMethod <> FSKMethod.HOLDING Then
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,1,,CaseElim,10377)
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10373)
														
												End If
												
											End If
											
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin ,,CaseElim,10378)
											
											'If consolidation method is Equity Pickup
											If method=FSKMethod.MEE Then
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10379)
											End If	
										End If	

									'----------------------------------------------------
					                ' Titres de participation
									' Investment in Subs
					                '----------------------------------------------------	
									ElseIf CaseElim="PART" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										Dim icpOwn As New OwnershipClass(si,api,globals, icName,api.Pov.Parent.Name,,,OpeScenario)
										'Dim PartnerOwn As New OwnershipClass(si,api,globals, partnerName, api.Pov.Parent.Name,,, OpeScenario)		
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'xxIf icMethod12345 And PartnerMethod <> FSKMethod.NOCONSO And EntityMethod <> FSKMethod.NOCONSO Then
											'brapi.ErrorLog.LogMessage(si, "err1" & api.Pov.Entity.Name)
											'If flow is AUG or DIM (not a problem target flow is always flowDestMember)
											If flowMember.MemberId = flow.INC.MemberId Or flowMember.MemberId = flow.DEC.MemberId  Then
												'If the ICP is not consolidated in N-1
												If ICPOwn.pCon_1 = 0 Then
												'xx If PartnerOwn.PCon_1 = 0 Then
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then												
														
														'If consolidation method is Equity Pickup or source nature is Equity
														If icMethod=FSKMethod.MEE Or NatureSource.Name="Equity" Then
															'Trigger account 261EQUI on the Destination flow at -Own.PCON
															Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10350)
															Cons.Book(cell,acct.RsvEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10348)
														'If the consolidation method is not Equity Pickup or the nature is not Equity	
														Else
															'Eliminate source record for Stage consolidations at Own.PCON
															Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10351)
															Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10348)
														End If
													'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding
													'ElseIf icMethod <> 1 Then
													'xxElseIf icMethod <> FSKMethod.HOLDING Then
													ElseIf icMethod <> FSKMethod.HOLDING Then
														'Eliminate source record for Stage consolidations (at 100%)						
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,CaseElim,10352)
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10348)
												
													End If
												'If the ICP is consolidated in N-1	
												Else
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then												
														'If consolidation method is Equity Pickup or source nature is Equity
														'If method=3 Or NatureSource.Name="Equity" Then
														If icMethod=FSKMethod.MEE Or NatureSource.Name="Equity" Then
													
															'Trigger account 261EQUI on the Destination flow at -Own.PCON
															Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10355)
															Cons.Book(cell,acct.RsvEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10353)
														'If the consolidation method is not Equity Pickup or the nature is not Equity	
														Else
															'Eliminate source record for Stage consolidations at Own.PCON
															Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10356)
															Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10353)
														End If
													'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding	
													'ElseIf icMethod <> 1 Then	
													ElseIf icMethod <> FSKMethod.HOLDING Then
														'Eliminate source record for Stage consolidations (at 100%)						
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,CaseElim,10357)
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10353)
														
													End If
												End If

											'If flows is ECO or ECF
											ElseIf flowMember.MemberId = flow.TDO.MemberId Or flowMember.MemberId = flow.TDM.MemberId Then
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
												
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If icMethod=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														'Trigger account 261EQUI on the Destination flow at -Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10370)
														Cons.Book(cell,acct.RsvEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10368)
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10371)
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10368)
													End If
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding															
												'ElseIf icMethod <> 1 Then
												ElseIf icMethod <> FSKMethod.HOLDING Then
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,CaseElim,10372)
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10368)
														
												End If
											
											'For other flows	
											Else
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If icMethod=FSKMethod.MEE Or NatureSource.Name="Equity" Then													
														'Trigger account 261EQUI on the Destination flow at -Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10375)
														Cons.Book(cell,acct.RsvEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10373)
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,CaseElim,10376)
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,CaseElim,10373)
													End If
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding															
												'ElseIf icMethod <> 1 Then	
												ElseIf icMethod <> FSKMethod.HOLDING Then
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,CaseElim,10377)
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10373)
												End If
											End If
											
'										'If the ICP is not under the current parent and is different from None	
'										ElseIf icMember.MemberID<>DimConstants.None Then
'											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
'											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10378)
'											'If consolidation method is Equity Pickup
'											'If method=3 Then
'											If method=FSKMethod.MEE Then
'												'Archive the proportionalized amount on nature Equity to trigger the elimination later
'												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10379)
'											End If	
										End If	
										
									'----------------------------------------------------
					                ' Titres de participation: cumul des ecarts de conversion
									' CTA on Investment in Subs
					                '----------------------------------------------------
									ElseIf CaseElim="PARTH" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then	
											'If flow is OUV/VARC/SOR
											If flowMember.MemberId = flow.OPE.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.EXI.MemberId Then
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then

												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding
												'ElseIf icMethod <> 1 Then
												ElseIf icMethod <> FSKMethod.HOLDING Then
													
												End If	'CurCustom4<>Mino
												
											End If
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,CaseElim,10383)
											
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,CaseElim,10384)
											End If	
										End If	

									'----------------------------------------------------
					                ' Elimination reciproques
									' Reciprocal eliminations
					                '----------------------------------------------------	
									ElseIf CaseElim="ELIM" Then
										'If the consolidation method of the entity is not Equity Pickup
										If method <> FSKMethod.MEE Then
											'If ICP consolidation method is 1,2,3,4,5
											If icMethod1245 Then
												Dim MinpCon As Decimal
												
												Dim icpOwn As New OwnershipClass(si,api,globals, icName,,,,Opescenario)
												If Own.pCon >  ICPOwn.pCon Then
	    											MinpCon = ICPOwn.pCon
												Else
	    											MinpCon = Own.pCon
												End If
												
												Dim plugElim As Member = api.Account.GetPlugAccount(cell.DataBufferCellPk.AccountId)												
												Dim acctAccountType As AccountType = api.Account.GetAccountType(acctMember.MemberId)
												Dim plugAccountType As AccountType = api.Account.GetAccountType(plugElim.MemberId)

												If NatureSource.MemberId=Nat.PropE.MemberId Then												
													'HS.Con aCompte & C1Dest & c4Nature & vElim,-1,Audit
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,CaseElim,10340)
													'HS.Con aCompte & C1Dest & vElim,PCon*(-1),Audit
													Cons.Book(cell,acctMember.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pcon,,CaseElim,10341)
							                        'HS.Con aCompte & C1Dest & c4PropU & vElim,-1,Audit
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimPropU.MemberId,,,,,-1,,CaseElim,10342)
													
									        	ElseIf NatureSource.MemberId=Nat.PropU.MemberId Then
							                        'HS.Con aCompte & C1Dest & c4Nature & vElim,Min,Audit
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,MinpCon ,,CaseElim,10343)
													'HS.Con aCompte & C1Dest & c4PropU & vElim,(PCon-Min)*(-1),Audit
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimPropU.MemberId,,,,,-(own.pCon-MinpCon),,CaseElim,10344)
							                    
												Else
													'Eliminate source record at the minimum % ownership of the entity or the ICP
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-MinpCon,,CaseElim,10345)
													
													'If the account type of the source account = the account type of the plug account (ex: ASSET and ASSET)
													If acctAccountType = plugAccountType Then													
														'Trigger the plug account on the Destination flow at the minimum % ownership of the entity or the ICP
														Cons.Book(cell,plugElim.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,MinpCon ,,CaseElim,10346)
													'If the account type of the source account is <> from the account type of the plug account (ex: ASSET and LIABILITY)
													Else
														'Trigger the plug account on the Destination flow at the minimum % ownership of the entity or the ICP (reverse sign)
														Cons.Book(cell,plugElim.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-MinpCon,,CaseElim,10347)
													End If	
												End If
												
											End If
										End If
									'----------------------------------------------------
					                ' Marge en Stock
									' Inventory profits
					                '----------------------------------------------------				
									ElseIf CaseElim="MSTK" Then

									'----------------------------------------------------
					                ' Marge en Stock: cumul des ecarts de conversion
									' CTA on Inventory profits
					                '----------------------------------------------------
									ElseIf CaseElim="MSTKH" Then

									'----------------------------------------------------
					                ' PV/MV de cession d'immobilisations intragroupe
									' Gain/Loss on interco sale of fixed assets
					                '----------------------------------------------------	
									ElseIf CaseElim="PMV" Then

									'----------------------------------------------------
					                ' PV/MV de cession d'immobilisations intragroupe: cumul des ecarts de conversion
									' CTA on Gain/Loss on interco sale of fixed assets
					                '----------------------------------------------------	
									ElseIf CaseElim="PMVH" Then


									End If	'CaseElim=
								End If 'If bDoEliminate Then
							End If 'If (Not cell.CellStatus.IsNoData) Then
			            Next

						'----------------------------------------------------
					    ' Ouvertures consolidees - Eliminations 
						' Consolidated Openings - Eliminations
					    '----------------------------------------------------
						Dim PriorPer As String = api.Time.GetNameFromId(api.Time.GetLastPeriodInPriorYear)
						Dim sPPRule As String = "PRIORYEAR"
'						Dim sourceScript As String = "F#" & flow.CLO.name & ":T#" & PriorPer
						Dim sourceScript As String = "S#" & Opescenario & ":F#" & flow.CLO.name & ":T#" & PriorPer
						Dim sourceDataBuffer2 As DataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, sourceScript, False, destinationInfo)

						If Not sourceDataBuffer2 Is Nothing Then
							For Each cell As DataBufferCell In sourceDataBuffer2.DataBufferCells.Values
								If (Not cell.CellStatus.IsNoData) Then
									
									Dim acctMember As Member = api.Members.GetMember(DimType.Account.Id, cell.DataBufferCellPk.AccountId)
									Dim NatureSource As Member = Nat.getMember(cell.DataBufferCellPk.Item(nat.NatDimType.Id))'api.Members.GetMember(DimType.UD4.Id, cell.DataBufferCellPk.UD4Id)	
									
									'If the consolidation method is not Exit
									'If method<>6 Then
									If scope<>FSKScope.EXITM Then
										'For accounts 120G/120M/120CG/120CM
'										If acctMember.Name="120G" Or acctMember.Name="120M" Or acctMember.Name="120CG" Or acctMember.Name="120CM" Then
										If acctMember.MemberId=Acct.ResG.MemberId Or acctMember.MemberId=Acct.ResM.MemberId Or _
										acctMember.MemberId=Acct.ResCG.MemberId Or acctMember.MemberId=Acct.ResCM.MemberId Then
		   
											'Dim AutoAdj As Member = nat.AutoAdj 'api.Members.GetMember(DimType.ud4.Id, "AutoAdj")
											'Dim ManAdj As Member =  nat.ManAdj 'api.Members.GetMember(DimType.ud4.Id, "ManAdj")
											'estas dos son las que dan error
											Dim isNatureBaseAutoAdj = api.Members.IsDescendant(Nat.NatDimPK, Nat.AutoAdj.MemberId, cell.DataBufferCellPk.item(nat.NatDimType.Id), Nothing)
											Dim isNatureBaseManAdj = api.Members.IsDescendant(Nat.NatDimPK, Nat.ManAdj.MemberId, cell.DataBufferCellPk.item(nat.NatDimType.Id), Nothing)
											
											'If the nature is base of RETAUTO/RETMAN or CAP (NatureDim)
											If isNatureBaseAutoAdj Or isNatureBaseManAdj Or NatureSource.Name="CAPI" Or NatureSource.Name="RESE" Then 'HC
												'Trigger the VARC flow at -100%
												Cons.Book(cell,,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10480)
											'For other natures (NatureDim)
											ElseIf NatureSource.Name="DIVI" Or NatureSource.Name="DETP" Then
												Cons.Book(cell,,flow.RPY.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10480)
												Cons.Book(cell,acct.RsvG.MemberId,flow.RPY.MemberId,,DimConstants.Elimination,,,,,,,,,1,,sPPRule,10480)
											Else
												'Trigger the DBE flow at -100% 
		                    					'novacons.Book(cell,,flow.RPY.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10481)
												'If the account is 120G
												If acctMember.MemberId=Acct.ResG.MemberId Then '"120G" Then
													'Trigger the account 106G DBE flow at 100%
													'novaCons.Book(cell,acct.RsvG.MemberId,flow.RPY.MemberId,,DimConstants.Elimination,,,,,,,,,1,,sPPRule,10482)
													'Trigger the account 106G VARC flow at -100%
													Cons.Book(cell,acct.RsvG.MemberId,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10483)
												'If the account is 120M	
												ElseIf acctMember.MemberId=Acct.ResM.MemberId Then
													'Trigger the account 106M DBE flow at 100%
													'novaCons.Book(cell,acct.RsvM.MemberId,flow.RPY.MemberId,,DimConstants.Elimination,,,,,,,,,1,,sPPRule ,10484)
													'Trigger the account 106M VARC flow at -100%
													Cons.Book(cell,acct.RsvM.MemberId,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10485)
												'If the account is 120CG	
												ElseIf acctMember.MemberId=Acct.ResCG.MemberId Then
													'Trigger the account 106CG DBE flow at 100%
													'novaCons.Book(cell,acct.RsvCG.MemberId,flow.RPY.MemberId,,DimConstants.Elimination,,,,,,,,,1,,sPPRule ,10486)
													'Trigger the account 106CG VARC flow at -100%
													Cons.Book(cell,acct.RsvCG.MemberId,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10487)
												'If the account is 120CM	
												ElseIf acctMember.MemberId=Acct.ResCM.MemberId Then
													'Trigger the account 106CM DBE flow at 100%
													'novaCons.Book(cell,acct.RsvCM.MemberId,flow.RPY.MemberId,,DimConstants.Elimination,,,,,,,,,1,,sPPRule,10488)
													'Trigger the account 106CM VARC flow at -100%
													Cons.Book(cell,acct.RsvCM.MemberId,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10489)
												End If	
													
											End If	
										'For other accounts (not 120G/120M/120CG/120CM)
										Else
											'Trigger the VARC flow at -100%
											cons.Book(cell,,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10490)
										End If
									'If the consolidation method Is Exit	
									Else
										'Trigger the OUV flow at -100%
										cons.Book(cell,,flow.OPE.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10491)
										'Trigger the SOR flow at -100%
										cons.Book(cell,,flow.EXI.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10492)
									End If	
								End If	'CellStatus is Not NoData
							Next
						End If	'If Not sourceDataBuffer2 Is Nothing Then

						api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
						cons.WriteLogToTable
						
					End If 'If Not sourceDataBuffer Is Nothing Then
			
		    Catch ex As Exception
		        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		    End Try
		End Sub
#End Region


'Special Scopes
#Region "IFRS5"
#End Region

#Region "Exit"
#End Region

'Support Function

#Region "Helper Functions"				

		Private Function GetExitList (ByRef Si As SessionInfo, ByRef Api As FinanceRulesApi, ByVal Globals As BRGlobals) As  ConcurrentDictionary(Of Integer, member)
			Try
				Dim sGName As String = "FSKExitList" & Api.Pov.Scenario.Name & Api.Pov.Time.Name
				Dim ExitList As ConcurrentDictionary(Of Integer, member) = globals.GetObject(sGName)
				If Not ExitList Is Nothing Then
					Return ExitList
				Else
					ExitList = New ConcurrentDictionary(Of Integer, member) 
					globals.SetObject(sGName, ExitList)
					System.Threading.Thread.Sleep(50)
					Return globals.GetObject(sGName)
				End If
				
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Function
#End Region

#Region "Init Functions"
		Public Function InitFlowClass (ByRef Si As SessionInfo, ByRef Api As FinanceRulesApi, ByVal Globals As BRGlobals, Optional ByVal bForced As Boolean = False) As FlowClass
			Try
				Dim sGName As String = "FSKFlows"
				Dim Flow As Object

				SyncLock Globals
					Flow = TryCast(globals.GetObject(sGName), FlowClass)
					If Flow Is Nothing OrElse bForced Then
						Flow = New FlowClass(Si, Api)
						globals.SetObject(sGName, Flow)
					End If
				End SyncLock	

				Return Flow
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function InitAccountClass (ByRef Si As SessionInfo, ByRef Api As FinanceRulesApi, ByVal Globals As BRGlobals, Optional ByVal bForced As Boolean = False) As AccountClass
			Try
				Dim sGName As String = "FSKAccounts"
				Dim Account As Object
				SyncLock Globals
					Account = TryCast(globals.GetObject(sGName), AccountClass)
					If Account Is Nothing OrElse bForced Then
						Account = New AccountClass(Si, Api)
						globals.SetObject(sGName, Account)
					End If
				End SyncLock					
					
				Return Account
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Function

		Public Function InitNatureClass (ByRef Si As SessionInfo, ByRef Api As FinanceRulesApi, ByVal Globals As BRGlobals, ByVal oDimPk As Dimpk, Optional ByVal bForced As Boolean = False) As NatureClass
			Try
				Dim sGName As String = "FSKNature"
				Dim Nature As Object
				SyncLock Globals
					Nature = TryCast(globals.GetObject(sGName), NatureClass)
					If Nature Is Nothing OrElse bForced Then
						Nature = New NatureClass (Si, Api, oDimPK)
						globals.SetObject(sGName, Nature)
					End If
				End SyncLock				
				Return Nature
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Function
	End Class	
#End Region

'Support Classes
#Region "Consolidation Helper Classes"			
	Public Class ConsClass
		Private si As SessionInfo
		Private api As FinanceRulesApi
		Private globals As BRGlobals
		Private args As FinanceRulesArgs
		Private TargetSpace As DataBuffer
		Private log As DataTable
		Private iTopIS As Integer
		Private iTopBS As Integer
		
		Public sourceEntity As Member
		
		Public Sub New (ByVal oSi As SessionInfo, ByVal oGlobals As BRGlobals, ByVal oApi As FinanceRulesApi, ByRef oTargetSpace As DataBuffer, ByVal Optional bLog As Boolean = True, ByVal Optional bDeleteRows As Boolean = True)
			Try
				si = oSi
				api = oApi
				globals = oGlobals
				TargetSpace = oTargetSpace
				sourceEntity = api.Pov.Entity

				Dim Acct As AccountClass = globals.GetObject("FSKAccounts")
				If Acct Is Nothing Then
'					Me.iTopBS = api.Members.GetMemberID(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopBS", "XFW_FSK_Accounts", True))
'					Me.iTopIS = api.Members.GetMemberID(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopIS", "XFW_FSK_Accounts", True))
					Me.iTopBS = api.Members.GetMemberID(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopBS", "FSKAccounts", True))
					Me.iTopIS = api.Members.GetMemberID(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopIS", "FSKAccounts", True))
				Else		
					Me.iTopBS = Acct.TopBS.MemberId
					Me.iTopIS = Acct.TopIS.MemberId
				End If	
			
				
				If bLog Then 
					'VK log = getLogTable(bDeleteRows)
				Else
					'VK deleteLogTable()
					log = Nothing
				End If
			
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Sub

		Protected Overrides Sub Finalize()
			si = Nothing
			api = Nothing
			globals = Nothing
			args = Nothing
			log = Nothing

        End Sub
		
		Public Sub Book (ByRef oSourceCell As DataBufferCell, Optional ByVal iAccount As Integer = DimConstants.All, Optional ByVal iFlow As Integer = DimConstants.All, Optional ByVal iIC As Integer = DimConstants.All, Optional ByVal iOrigin As Integer = DimConstants.All, Optional ByVal iUD1 As Integer = DimConstants.All, Optional ByVal iUD2 As Integer = DimConstants.All, Optional ByVal iUD3 As Integer = DimConstants.All, Optional ByVal iUD4 As Integer = DimConstants.All, Optional ByVal iUD5 As Integer = DimConstants.All, Optional ByVal iUD6 As Integer = DimConstants.All, Optional ByVal iUD7 As Integer = DimConstants.All, Optional ByVal iUD8 As Integer = DimConstants.All, Optional ByVal iFactor As Decimal = 1, Optional ByVal sComment As String = "", Optional ByVal sRule As String = "", Optional ByVal iLine As Integer = 0) 
			Try		
				Dim oTargetCell As New DataBufferCell(oSourceCell)
				
				With oTargetCell.DataBufferCellPk
					
					If Not iAccount = DimConstants.All Then .AccountId = iAccount
					If Not iFlow = DimConstants.All Then .FlowId = iFlow
					If Not iIC = DimConstants.All Then .ICId = iIC
					If Not iOrigin = DimConstants.All Then .OriginId = iOrigin
					If Not iUD1 = DimConstants.All Then .UD1Id = iUD1
					If Not iUD2 = DimConstants.All Then .UD2Id = iUD2
					If Not iUD3 = DimConstants.All Then .UD3Id = iUD3
					If Not iUD4 = DimConstants.All Then .UD4Id = iUD4
					If Not iUD5 = DimConstants.All Then .UD5Id = iUD5
					If Not iUD6 = DimConstants.All Then .UD6Id = iUD6
					If Not iUD7 = DimConstants.All Then .UD7Id = iUD7
					If Not iUD8 = DimConstants.All Then .UD8Id = iUD8
						
				End With

				oTargetCell.CellAmount = iFactor * oTargetCell.CellAmount
				
				TargetSpace.SetCell(api.SI, oTargetCell, True)
				LogElimInTable(oSourceCell, oTargetCell, iFactor, sComment, sRule, iLine)
		
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Sub
	
		Public Sub Book (ByRef oSourceCell As DataBufferCell, Optional ByVal oAccount As member = Nothing, Optional ByVal oFlow As member = Nothing, Optional ByVal oIC As member = Nothing, Optional ByVal oOrigin As member = Nothing, Optional ByVal oUD1 As member = Nothing, Optional ByVal oUD2 As member = Nothing, Optional ByVal oUD3 As member = Nothing, Optional ByVal oUD4 As member = Nothing, Optional ByVal oUD5 As member = Nothing, Optional ByVal oUD6 As member = Nothing, Optional ByVal oUD7 As member = Nothing, Optional ByVal oUD8 As member = Nothing, Optional ByVal iFactor As Decimal = 1, Optional ByVal sComment As String = "", Optional ByVal sRule As String = "", Optional ByVal iLine As Integer = 0) 
		
			Try
				Dim oTargetCell As New DataBufferCell(oSourceCell)
			
				With oTargetCell.DataBufferCellPk
					
					If Not oAccount Is Nothing Then .AccountId = oAccount.MemberId
					If Not oFlow Is Nothing Then .FlowId = oFlow.MemberId
					If Not oIC Is Nothing Then .ICId = oIC.MemberId
					If Not oOrigin Is Nothing Then .OriginId = oOrigin.MemberId
					If Not oUD1 Is Nothing Then .UD1Id = oUD1.MemberId
					If Not oUD2 Is Nothing Then .UD2Id = oUD2.MemberId
					If Not oUD3 Is Nothing Then .UD3Id = oUD3.MemberId
					If Not oUD4 Is Nothing Then .UD4Id = oUD4.MemberId
					If Not oUD5 Is Nothing Then .UD5Id = oUD5.MemberId
					If Not oUD6 Is Nothing Then .UD6Id = oUD6.MemberId
					If Not oUD7 Is Nothing Then .UD7Id = oUD7.MemberId
					If Not oUD8 Is Nothing Then .UD8Id = oUD8.MemberId
						
				End With

				oTargetCell.CellAmount = iFactor * oTargetCell.CellAmount
				
				TargetSpace.SetCell(api.SI, oTargetCell, True)
				LogElimInTable(oSourceCell, oTargetCell, iFactor, sComment, sRule, iLine)
		
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Sub
	
		Public Sub Book (ByRef oSourceCell As DataBufferCell, Optional ByVal sAccount As String = "", Optional ByVal sFlow As String = "", Optional ByVal sIC As String = "", Optional ByVal sOrigin As String = "", Optional ByVal sUD1 As String = "", Optional ByVal sUD2 As String = "", Optional ByVal sUD3 As String = "", Optional ByVal sUD4 As String = "", Optional ByVal sUD5 As String = "", Optional ByVal sUD6 As String = "", Optional ByVal sUD7 As String = "", Optional ByVal sUD8 As String = "", Optional ByVal iFactor As Decimal = 1, Optional ByVal sComment As String = "", Optional ByVal sRule As String = "", Optional ByVal iLine As Integer = 0) 
		
			Try
				Dim oTargetCell As New DataBufferCell(oSourceCell)
				
				With oTargetCell.DataBufferCellPk
					
					If Not sAccount = "" Then .AccountId = api.Members.GetMemberId(dimtypeid.Account, sAccount)
					If Not sFlow = "" Then .FlowId = api.Members.GetMemberId(dimtypeid.Flow, sFlow)
					If Not sIC = "" Then .ICId = api.Members.GetMemberId(dimtypeid.IC, sIC)
					If Not sOrigin = "" Then .OriginId = api.Members.GetMemberId(dimtypeid.Origin, sOrigin)
					If Not sUD1 = "" Then .UD1Id = api.Members.GetMemberId(dimtypeid.UD1, sUD1)
					If Not sUD2 = "" Then .UD2Id = api.Members.GetMemberId(dimtypeid.UD2, sUD2)
					If Not sUD3 = "" Then .UD3Id = api.Members.GetMemberId(dimtypeid.UD3, sUD3)
					If Not sUD4 = "" Then .UD4Id = api.Members.GetMemberId(dimtypeid.UD4, sUD4)
					If Not sUD5 = "" Then .UD5Id = api.Members.GetMemberId(dimtypeid.UD5, sUD5)
					If Not sUD6 = "" Then .UD6Id = api.Members.GetMemberId(dimtypeid.UD6, sUD6)
					If Not sUD7 = "" Then .UD7Id = api.Members.GetMemberId(dimtypeid.UD7, sUD7)
					If Not sUD8 = "" Then .UD8Id = api.Members.GetMemberId(dimtypeid.UD8, sUD8)
						
				End With
				
				oTargetCell.CellAmount = iFactor * oTargetCell.CellAmount
				
				TargetSpace.SetCell(api.SI, oTargetCell, True)
				LogElimInTable(oSourceCell, oTargetCell, iFactor, sComment, sRule, iLine)

			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Sub

		Public Sub Book (ByRef oSourceCell As DataBufferCell, Optional ByVal sScript As String = "", Optional ByVal iFactor As Decimal = 1, Optional ByVal sComment As String = "", Optional ByVal sRule As String = "", Optional ByVal iLine As Integer = 0) 
			Try
				Dim oTargetCell As New DataBufferCell(oSourceCell)
				Dim oScript As New MemberScriptBuilder(sScript)

				With oTargetCell.DataBufferCellPk
					
					If Not oScript.Account = "" Then .AccountId = api.Members.GetMemberId(dimtypeid.Account, oScript.Account)
					If Not oScript.Flow = "" Then .FlowId = api.Members.GetMemberId(dimtypeid.Flow, oScript.Flow)
					If Not oScript.IC = "" Then .ICId = api.Members.GetMemberId(dimtypeid.IC, oScript.IC)
					If Not oScript.Origin = "" Then .OriginId = api.Members.GetMemberId(dimtypeid.Origin, oScript.Origin)
					If Not oScript.UD1 = "" Then .UD1Id = api.Members.GetMemberId(dimtypeid.UD1, oScript.UD1)
					If Not oScript.UD2 = "" Then .UD2Id = api.Members.GetMemberId(dimtypeid.UD2, oScript.UD2)
					If Not oScript.UD3 = "" Then .UD3Id = api.Members.GetMemberId(dimtypeid.UD3, oScript.UD3)
					If Not oScript.UD4 = "" Then .UD4Id = api.Members.GetMemberId(dimtypeid.UD4, oScript.UD4)
					If Not oScript.UD5 = "" Then .UD5Id = api.Members.GetMemberId(dimtypeid.UD5, oScript.UD5)
					If Not oScript.UD6 = "" Then .UD6Id = api.Members.GetMemberId(dimtypeid.UD6, oScript.UD6)
					If Not oScript.UD7 = "" Then .UD7Id = api.Members.GetMemberId(dimtypeid.UD7, oScript.UD7)
					If Not oScript.UD8 = "" Then .UD8Id = api.Members.GetMemberId(dimtypeid.UD8, oScript.UD8)
						
				End With
				
				oTargetCell.CellAmount = iFactor * oTargetCell.CellAmount
				
				TargetSpace.SetCell(api.SI, oTargetCell, True)
				LogElimInTable(oSourceCell, oTargetCell, iFactor, sComment, sRule, iLine)
			
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Sub

		Private Sub LogElimInTable(ByRef oSourceCell As DataBufferCell, ByRef oTargetCell As DataBufferCell, Optional ByVal iFactor As Decimal = 1, Optional ByVal sComment As String = "", Optional ByVal sRule As String = "", Optional ByVal iLine As Integer = 0)
			
			Try
				If Not log Is Nothing Then
					Dim factor As Decimal = iFactor
					Dim amount As Decimal = oTargetCell.CellAmount
					Dim comment As String = sComment			

					Dim line As datarow = log.NewRow()
					line("UniqueID") = guid.NewGuid
					line("SortOrder") = 1
					line("CubeID") = api.Pov.GetDataUnitPk.CubeId
					line("BRLine") = iLine 'sf.GetFileLineNumber()'api.Pov.Origin.MemberId
					line("IsIS") = api.Members.IsBase(api.Pov.AccountDim.DimPk, Me.iTopIS, otargetCell.DataBufferCellPk.AccountId)
					line("IsBS") = api.Members.IsBase(api.Pov.AccountDim.DimPk, Me.iTopBS, otargetCell.DataBufferCellPk.AccountId)
					line("ConsID") = api.Pov.GetDataUnitPk.ConsId
					line("EntityID") = api.Pov.GetDataUnitPk.EntityId
					line("ParentID") = api.Pov.GetDataUnitPk.ParentID
					line("ScenarioID") = api.Pov.GetDataUnitPk.ScenarioId
					line("TimeID") = api.Pov.GetDataUnitPk.TimeId
					
					line("EliminationID") = "0".XFConvertToGuid(Guid.Empty)
					For Each oDimtype In dimtype.GetAllDimTypes
						If oDimtype.Id.Equals(dimtypeId.Entity) Or _
							 oDimtype.Id.Equals(dimtypeId.Consolidation) Or _
							 oDimtype.Id.Equals(dimtypeId.Origin) Or _
							 oDimtype.Id.Equals(dimtypeId.View) Or _
							 oDimtype.Id.Equals(dimtypeId.Scenario) Or _
							 oDimtype.Id.Equals(dimtypeId.Time) Then
						
							 Else
							line(oDimtype.Name & "ID") = otargetCell.DataBufferCellPk.item(oDimtype.Id)
						End If
					Next

					line("Amount") = math.Abs(Amount)
									
					line("Description") = comment & IIf(comment.Length>0, ": ", "") & factor.XFToString("0.######") & " * " & Amount.XFToString("N2") & " (" & sourceEntity.Name & " - " &  oSourceCell.DataBufferCellPk.GetAccountName(api) & " - " & oSourceCell.DataBufferCellPk.GetICName(api) & " - " & oSourceCell.DataBufferCellPk.GetFlowName(api) & ")" 
					Line("ConsRule") = sRule
					Dim oAccounttypeTarget As AccountType = api.Account.GetAccountType(oTargetCell.DataBufferCellPk.AccountId)
				
					Dim bDebitCreditTarget As Boolean = oAccounttypeTarget.Equals(accounttype.Asset) Or oAccounttypeTarget.Equals(accounttype.Expense)
					Dim bDebitCredit As Boolean = bDebitCreditTarget				

					If amount < 0 Then
						bDebitCredit = Not(bDebitCredit)
					End If
					
					line("LineItemType") = IIf(bDebitCredit, 0, 1)
					
					log.Rows.Add(line)
				End If

			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

			
		End Sub
		
		Public Sub PElimPartnerTable(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, icEntity As Member)	

			Try
				Dim sID As Integer = api.Pov.Scenario.MemberId
				Dim eID As Integer = api.Pov.Entity.MemberId
				Dim iID As Integer = icEntity.MemberId
				Dim tID As Integer = api.Pov.Time.MemberId
				Dim cID As Integer = api.pov.Cube.CubeId
				
				Dim mySql_FindIfRecordExist As New Text.StringBuilder	
				mySql_FindIfRecordExist.AppendLine("SELECT * FROM [XFC_FSK_PelimPartners] WHERE (EntityIDforElim = '" & eID & "' AND ScenarioID = '" & sID & "' AND TimeID = '" & tID & "' AND ICIDforElim = '" & iID & "')")
				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
				
				If BRApi.Database.ExecuteSql(dbConnApp,mySql_FindIfRecordExist.ToString, True).Rows.Count = 0 Then		

					Dim mySql_insertRecord As New Text.StringBuilder	
					mySql_insertRecord.AppendLine( "INSERT INTO XFC_FSK_PelimPartners (UniqueID, ScenarioID, TimeId, CubeId, EntityIDforElim, ICIDforElim, EntityNameforElim, ICNameforElim)")
					mySql_insertRecord.AppendLine("VALUES (NEWID(), " & sid & "," & tid & ", " & cID & ", " & eID & ", " & iID & ", '" & api.Pov.Entity.Name & "', '" & icEntity.Name  & "')")
					BRApi.Database.ExecuteSql(dbConnApp,mySql_insertRecord.ToString, True)

						Dim icAncestors As List (Of Member) = api.Members.GetAncestors(api.Pov.EntityDim.DimPk, icEntity.MemberId, False)
						For Each icAncestor In icAncestors
								Dim mySql_FindIfRecordExist2 As New Text.StringBuilder

								mySql_FindIfRecordExist2.AppendLine("SELECT * FROM [XFC_FSK_PelimPartners] WHERE (EntityIDforElim = '" & eID & "' AND ScenarioID = '" & sID & "' AND TimeID = '" & tID & "' AND ICIDforElim = '" & icAncestor.MemberId & "')")
								If BRApi.Database.ExecuteSql(dbConnApp,mySql_FindIfRecordExist2.ToString, True).Rows.Count = 0 Then		
									
								Dim mySql_insertRecord2 As New Text.StringBuilder
									mySql_insertRecord2.AppendLine( "INSERT INTO XFC_FSK_PelimPartners (UniqueID, ScenarioID, TimeId, CubeId, EntityIDforElim, ICIDforElim, EntityNameforElim, ICNameforElim)")
									mySql_insertRecord2.AppendLine(" VALUES (NEWID(), " & sid & ", " & tid & ", " & cID & ", " & eID & ", " & icAncestor.MemberId & ", '" & api.Pov.Entity.Name & "', '" & icAncestor.Name  & "')")
									BRApi.Database.ExecuteSql(dbConnApp,mySql_insertRecord2.ToString, True)									
								End If
						
					Next icAncestor
					
				End If
			End Using
			
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Sub

		Public Function pullICPelimMembers(ByVal si As SessionInfo, ByVal api As FinanceRulesApi)	

			Try
				'Build SQL String To Lookup Value In Custom Table to pull the parent memeber for the base GL
					Dim mysql As New Text.StringBuilder
					Dim sID As Integer = api.Pov.Scenario.MemberId
					Dim eID As Integer = api.Pov.Entity.MemberId
					Dim tID As Integer = api.Pov.Time.MemberId
					Dim cID As Integer = api.pov.Cube.CubeId
					'Create SQL Statement
					Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					mysql.AppendLine("SELECT [EntityIDforElim] FROM XFC_FSK_PelimPartners Where ([ScenarioId] = '" & sID & "' AND [TimeId] = '" & tID & "' AND [ICIDforElim] = '" & eID &"')")

					'SQL call to pull from the Table
				
					Dim entityList As New List(Of Member)				
					For Each row As DataRow In BRApi.Database.ExecuteSql(dbconnapp,mysql.ToString, True).Rows
						Dim linkedEntity As Integer
						linkedEntity = row(0)
						entityList.Add(BRApi.Finance.Members.GetMember(si,dimTypeId.Entity, linkedEntity))
					Next row

					Return entityList
				End Using
				
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Function		
			
		Private Function deleteLogTable () As Boolean
			Try		
				Dim LineItemName As String = "XFW_FSK_EliminationLineItem"
				
				Dim sql As  New Text.StringBuilder
				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					If (DBSql.DoesTableExist(dbConnApp, LineItemName)) Then
						sql.Append("delete from [" & LineItemName & "] ")
						sql.Append("where ")
						sql.Append("EntityID = '" & api.Pov.GetDataUnitPk.EntityID & "' and ")
						sql.Append("ParentID = '" & api.Pov.GetDataUnitPk.ParentID & "' and ")
						sql.Append("ScenarioId = '" & api.Pov.GetDataUnitPk.ScenarioId & "' and ")
						sql.Append("TimeId = '" & api.Pov.GetDataUnitPk.TimeId & "' and ")
						sql.Append("CubeId = '" & api.Pov.GetDataUnitPk.CubeId & "' and ")
						sql.Append("ConsId = '" & api.Pov.GetDataUnitPk.ConsId & "'")
						SyncLock dbConnApp
							brapi.Database.ExecuteSql(dbconnapp, sql.ToString, True)'brapi.Database.get.GetCustomDataTable(si, dblocation.Application, sLineItemName, "")					
						End SyncLock
						Return True
					Else
						Return True
					End If
				End Using
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function	
		
		Private Function getLogTable(ByVal Optional bDeleteRows As Boolean = True) As DataTable
			Try
				Dim LineItemName As String = "XFW_FSK_EliminationLineItem"
				Dim oADatatable As DataTable = Nothing
				
				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					If log Is Nothing Then

						Dim sql As  New Text.StringBuilder

						If bDeleteRows Then
							Me.deleteLogTable()
						End If	
						sql.clear
						sql.Append("select Top 1 * from [" & LineItemName & "] ")
						oADatatable = brapi.Database.ExecuteSql(dbconnapp, sql.ToString, True)'brapi.Database.get.GetCustomDataTable(si, dblocation.Application, sLineItemName, "")
						oADatatable.clear
					End If
				End Using				
				Return oADatatable
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Sub WriteLogToTable()
			Try

					If Not log Is Nothing Then

						If log.Rows.Count > 0 Then

								Using log			
									BRApi.Database.SaveCustomDataTable(si, "App", log.TableName, log, True)
								End Using

						End If
						log.Rows.Clear
					End If
		
				Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub
		

	End Class
	
	Public Class GlobalClass
		Private si As SessionInfo
		Private api As FinanceRulesApi
		Private globals As BRGlobals
		Private args As FinanceRulesArgs
		Private referenceBR As New OneStream.BusinessRule.Finance.FSK_OpeningScenario.MainClass
		Public OpeScenario As String
		
		Public Sub New (ByVal oSi As SessionInfo, ByVal oApi As FinanceRulesApi, ByVal oargs As FinanceRulesArgs)
			Try			
				If OpeScenario Is Nothing Then
					 OpeScenario = referenceBR.fctOpeningScenario(osi, oapi, oargs)
				End If
			
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
	
		End Sub

		Protected Overrides Sub Finalize()
			si = Nothing
			api = Nothing
			globals = Nothing
			args = Nothing
        End Sub
		
		Public Sub ClearPelimTable(ByVal si As SessionInfo, ByVal api As FinanceRulesApi)
			Try
				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					Dim sID As Integer = api.Pov.Scenario.MemberId
					Dim eID As Integer = api.Pov.Entity.MemberId
					Dim tID As Integer = api.Pov.Time.MemberId
					Dim cID As Integer = api.pov.Cube.CubeId
					Dim mySql_DeleteRecordsforPovEntity As New Text.StringBuilder	'Clearing first all record for current entity
					mySql_DeleteRecordsforPovEntity.AppendLine("DELETE FROM [XFC_FSK_PelimPartners] WHERE (EntityIDForElim = '" & eID & "' AND ScenarioID = '" & sID & "' AND TimeID = '" & tID & "')")
					BRApi.Database.ExecuteSql(dbConnApp,mySql_DeleteRecordsforPovEntity.ToString, True)		
				End Using

			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		
		End Sub	
	End Class
#End Region

#Region "Account Class"			
	
	Public Class AccountClass

		Public GWAMEE As Member
		Public GWMEE As Member
		Public MEE As Member
		Public MinInt As Member
		Public ResC As Member
		Public ResCG As Member
		Public ResCM As Member
		Public ResEx As Member
		Public ResG As Member
		Public ResM As Member
		Public ResR As Member
		Public ResMEE As Member
		Public RsvC As Member
		Public RsvCG As Member
		Public RsvCM As Member
		Public RsvG As Member
		Public RsvM As Member
		Public RsvR As Member
		Public TopBS As Member
		Public TopIS As Member
		Public ResEq As Member
		Public RsvEq As Member
		Public RsvCEq As Member
		Public RsvH As Member
		Public ResH As Member
		Public EnlaDiv As Member
		Public RisM As Member
		Public Ris1M As Member
		Public Ris2M As Member
		Public Ris3M As Member
		
		Private _PlugAccounts As List(Of Integer)
		
		Public  ReadOnly Property PlugAccounts() As List(Of Integer)
			Get 
				Return _PlugAccounts
			End Get			
		End Property

		Public  ReadOnly Property MinAccounts() As List(Of Integer)
			Get 
				Return New List(Of Integer) From {MinInt.MemberId, ResCM.MemberId, ResM.MemberId, RsvCM.MemberId, RsvM.MemberId}
			End Get			
		End Property
		
		Public  ReadOnly Property GroupAccounts() As List(Of Integer)
			Get 
				Return New List(Of Integer) From {MinInt.MemberId, ResCM.MemberId, ResM.MemberId, RsvCM.MemberId, RsvM.MemberId,RisM.MemberId, Ris1M.MemberId, Ris2M.MemberId, Ris3M.MemberId}
			End Get			
		End Property

		Private si As SessionInfo
		Private api As FinanceRulesApi

		Sub New (ByRef oSi As SessionInfo, ByRef oApi As FinanceRulesApi)
			Try
				api = oApi
				si = oSi
				'FSKAccounts is the name of the Transformation Rule Profile (lookup) for accounts
				GWAMEE = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "GWAMEE", "FSKAccounts", True))
				GWMEE = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "GWMEE", "FSKAccounts", True))
				MEE = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "MEE", "FSKAccounts", True))
				MinInt = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "MinInt", "FSKAccounts", True))
				ResC = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResC", "FSKAccounts", True))
				ResCG = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResCG", "FSKAccounts", True))
				ResCM = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResCM", "FSKAccounts", True))
				ResEx = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResEx", "FSKAccounts", True))
				ResG = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResG", "FSKAccounts", True))
				ResM = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResM", "FSKAccounts", True))
				ResR = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResR", "FSKAccounts", True))
				ResMEE = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResMEE", "FSKAccounts", True))
				RsvC = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvC", "FSKAccounts", True))
				RsvCG = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvCG", "FSKAccounts", True))
				RsvCM = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvCM", "FSKAccounts", True))
				RsvG = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvG", "FSKAccounts", True))
				RsvM = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvM", "FSKAccounts", True))
				RsvR = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvR", "FSKAccounts", True))
				TopBS = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopBS", "FSKAccounts", True))
				TopIS = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopIS", "FSKAccounts", True))
				ResEq = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResEq", "FSKAccounts", True))
				RsvEq = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvEq", "FSKAccounts", True))
				RsvCEq = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvCEq", "FSKAccounts", True))
				RsvH = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvH", "FSKAccounts", True))
				ResH = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResH", "FSKAccounts", True))
				EnlaDiv = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "EnlaDiv", "FSKAccounts", True))
				RisM = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RisM", "FSKAccounts", True))
				Ris1M = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "Ris1M", "FSKAccounts", True))
				Ris2M = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "Ris2M", "FSKAccounts", True))
				Ris3M = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "Ris3M", "FSKAccounts", True))
				
				_PlugAccounts = New List(Of Integer)

				Dim AccBSbase As Integer = Me.TopBS.MemberId 'api.Members.GetMemberId(DimType.Account.Id, "BILAN")
				Dim AccPLbase As Integer = Me.TopIS.MemberId 'api.Members.GetMemberId(DimType.Account.Id, "RESULTATG")
				
				For Each oAccount As Member In api.Members.GetBaseMembers(api.Pov.accountdim.DimPk, AccBSbase)
					Dim plugAcct As Member = api.account.GetPlugAccount(oAccount.MemberId)					
								
						If Not plugAcct.MemberId.Equals(dimconstants.Unknown) AndAlso Not PlugAccounts.Contains(plugAcct.MemberId) Then
							_PlugAccounts.Add(plugAcct.MemberId)
						End If
				Next
				
				For Each oAccount As Member In api.Members.GetBaseMembers(api.Pov.accountdim.DimPk, AccPLbase)
					Dim plugAcct As Member = api.account.GetPlugAccount(oAccount.MemberId)					
								
						If Not plugAcct.MemberId.Equals(dimconstants.Unknown) AndAlso Not PlugAccounts.Contains(plugAcct.MemberId) Then
							_PlugAccounts.Add(plugAcct.MemberId)
						End If
				Next

			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Sub	

		Protected Overrides Sub Finalize()
			si = Nothing
			api = Nothing
		End Sub
		
		Function getName(iID As Integer)
			Return api.Members.GetMemberName(dimtypeid.Account, iID)
		End Function 	
		
		Function getID(sName As String)
			Return api.Members.GetMemberID(dimtypeid.Account, sName)
		End Function 	

		Function getMember(iID As Integer)
			Return api.Members.GetMember(dimtypeid.Account, iID)
		End Function 	

		Function getMember(sName As String)
			Return api.Members.GetMember(dimtypeid.Account, sName)
		End Function 	
	
	End Class

#End Region

#Region "Flow Class"			
	
	Public Class FlowClass 

		Public NONE As Member
		Public CLO As Member
		Public DEC As Member
		Public DIV As Member
		Public ENT As Member
		Public EXI As Member
		Public INC As Member
		Public OPE As Member
		Public REC As Member
		Public RES As Member 
		Public RPY As Member 
		Public TDM As Member 
		Public TDO As Member 
		Public TDR As Member		
		Public TF As Member 
		Public VARC As Member 
		Public VARO As Member
		Public VARP As Member
		Public INCCAP As Member
		Public DIMCAP As Member
		Public ERR As Member
		Public ELC As Member
		Public ECE As Member
		Public DETPINC As Member
		Public DETPDIS As Member
		
		Private si As SessionInfo
		Private api As FinanceRulesApi

		Sub New (ByRef oSi As SessionInfo, ByRef oApi As FinanceRulesApi)
			Try
				api = oApi
				si = oSi
				'FSKFlows is the name of the Transformation Rule Profile (lookup) for Flows
				NONE = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "NONE", "FSKFlows", True)) 
				CLO = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "CLO", "FSKFlows", True)) 			
				DEC = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DEC", "FSKFlows", True))
				DIV = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DIV", "FSKFlows", True))
				ENT = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "ENT", "FSKFlows", True))
				EXI = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "EXI", "FSKFlows", True))
				INC = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "INC", "FSKFlows", True))
				OPE = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "OPE", "FSKFlows", True))
				REC = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "REC", "FSKFlows", True))
				RES = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "RES", "FSKFlows", True))
				RPY = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "RPY", "FSKFlows", True))
				TDM = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "TDM", "FSKFlows", True))			
				TDO = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "TDO", "FSKFlows", True))			
				TDR = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "TDR", "FSKFlows", True))
				TF = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "TF", "FSKFlows", True))
				VARC = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "VARC", "FSKFlows", True))
				VARO = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "VARO", "FSKFlows", True))
				VARP = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "VARP", "FSKFlows", True))
				INCCAP = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "INCCAP", "FSKFlows", True))
				DIMCAP = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DIMCAP", "FSKFlows", True))
				ERR = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "ERR", "FSKFlows", True))
				ELC = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "ELC", "FSKFlows", True))
				ECE = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "ECE", "FSKFlows", True))
				DETPINC = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DETPINC", "FSKFlows", True))
				DETPDIS = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DETPDIS", "FSKFlows", True))
				
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
	
		End Sub					

		Protected Overrides Sub Finalize()
			si = Nothing
			api = Nothing
		End Sub		
		
		Function getName(iID As Integer) As String
			Return api.Members.GetMemberName(dimtypeid.Flow, iID)
		End Function 	
		
		Function getID(sName As String) As Integer
			Return api.Members.GetMemberID(dimtypeid.Flow, sName)
		End Function 	

		Function getMember(iID As Integer) As Member
			Return api.Members.GetMember(dimtypeid.Flow, iID)
		End Function 	

		Function getMember(sName As String) As Member
			Return api.Members.GetMember(dimtypeid.Flow, sName)
		End Function 	
				
End Class
#End Region

#Region "Nature Class"			
	
	Public Class NatureClass 

		Public ACIR As Member
		Public PropU As Member
		Public PropE As Member
		Public Minoritaires As Member
		Public Equity As Member
		Public AutoAdj As Member
		Public ManAdj As Member
		
		Private si As SessionInfo
		Private api As FinanceRulesApi
		Public NatDimType As DimType
		Public NatDimPK As DimPk
		Public NatDimTypeID As Integer
		
		Sub New (ByRef oSi As SessionInfo, ByRef oApi As FinanceRulesApi, oDimPK As DimPK)
			Try
				api = oApi
				si = oSi
				NatDimPk = oDimPK
				NatDimTypeID = NatDimPk.DimTypeId
				NatDimType = DimType.GetItem(NatDimTypeID)
				'FSKNature is the name of the Transformation Rule Profile (lookup) for UD4
				
					PropU = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PropU", "FSKNature", True))
					PropE = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PropE", "FSKNature", True))
					Equity = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "Equity", "FSKNature", True))
					Minoritaires = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "Minoritaires", "FSKNature", True))
					AutoAdj = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "AUTOADJ", "FSKNature", True))
					ManAdj = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "MANADJ", "FSKNature", True))
					
				Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
	
		End Sub					
		
		Protected Overrides Sub Finalize()
			si = Nothing
			api = Nothing
		End Sub
				
		Function getName(iID As Integer)
			Return api.Members.GetMemberName(NatDimType.Id, iID)
		End Function 	
		
		Function getID(sName As String)
			Return api.Members.GetMemberID(NatDimType.Id, sName)
		End Function 	

		Function getMember(iID As Integer)
			Return api.Members.GetMember(NatDimType.Id, iID)
		End Function 	

		Function getMember(sName As String)
			Return api.Members.GetMember(NatDimType.Id, sName)
		End Function
		
End Class
#End Region

#Region "Ownership Class"			

Public Class OwnershipClass
		
	Private dicOwnershipCells As ConcurrentDictionary(Of String, datacell)
	
	Private pConD As DataCell
	Private pCon_1D As DataCell
	Private pOwnD As DataCell
	Private pOwn_1D As DataCell
	Private methodD As DataCell
	Private method_1D As DataCell
	Private scopeD As DataCell
	Private discontinuedD As DataCell
	Private discontinued_1D As DataCell
	
	Public pCon As Decimal
	Public pCon_1 As Decimal
	Public pOwn As Decimal
	Public pOwn_1 As Decimal
	Public VpCon As Decimal
	Public VpOwn As Decimal
	Public pMin As Decimal
	Public pMin_1 As Decimal
	Public VpMin As Decimal
	Public method As Decimal
	Public method_1 As Decimal
	
	Public scope As Decimal
	Public discontinued As Boolean
	Public discontinued_1 As Boolean

	Private si As SessionInfo
	Private api As FinanceRulesApi
	
	Private Entity As String
	Private Parent As String
	Private PerScenario As String
	Private Time As String
	Private TimePY As String
	Private OpnScenario As String

	Sub New (ByRef Si As SessionInfo, ByRef percentOwn As Decimal, Optional ByRef percentCon As Decimal = 1)

		Try
			pConD   = Nothing
			pCon_1D = Nothing
			pOwnD   = Nothing
			pOwn_1D = Nothing

			pCon = percentCon
			pOwn = percentOwn
			pMin = pCon - pOwn
			pCon_1 = percentCon
			pOwn_1 = percentOwn
			pMin_1 = pCon - pOwn
			VpCon = 0.0
			VpOwn = 0.0
			VpMin = 0.0
			methodD = Nothing
			method_1D = Nothing
			
			scope = 0.0
			discontinued = 0.0
			discontinued = 0.0
			
			method = FSKMethod.HOLDING
			method_1 = FSKMethod.HOLDING
		Catch ex As Exception
			
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try

	End Sub
	
	Sub New (ByRef oSi As SessionInfo, ByRef oApi As FinanceRulesApi, ByVal Globals As BRGlobals, Optional ByRef sEntity As String = "", Optional ByRef sParent As String = "", Optional ByRef sScenario As String = "", Optional ByRef sTime As String = "", Optional ByRef sOpnScenario As String = "")
		Try
			api = oApi
			si = oSi

			If sEntity.Equals("") Then Entity = api.Pov.Entity.Name Else Entity = sEntity
			Dim iEntity As Integer = api.Members.GetMemberId(dimtypeid.Entity, Entity)
			
			If sParent.Equals("") Then Parent = api.Pov.Parent.Name Else Parent = sParent
			Dim iParent As Integer = api.Members.GetMemberId(dimtypeid.Entity, Parent)
				
				If sTime.Equals("") Then Time = api.Pov.Time.Name Else Time = sTime
				Dim iTime As Integer = api.Members.GetMemberId(dimtypeid.Time, Time)
                Dim iTimePY As Integer = api.Time.GetLastPeriodInPriorYear(iTime)
                TimePY = api.Members.GetMemberName(dimtypeid.Time, iTimePY)
				
				If sScenario.Equals("") Then PerScenario = api.Pov.Scenario.Name Else PerScenario = sScenario
				If sOpnScenario.Equals("") Then OpnScenario = api.Pov.Scenario.Name Else OpnScenario = sOpnScenario
				
				SyncLock Globals.LockObjectForInitialization
					If dicOwnershipCells Is Nothing Then
						dicOwnershipCells = Globals.GetObject("dicOwnershipCells")
						If dicOwnershipCells Is Nothing Then
							dicOwnershipCells = New ConcurrentDictionary(Of String, datacell)
							globals.SetObject("dicOwnershipCells", dicOwnershipCells)
							System.Threading.Thread.Sleep(50)
							dicOwnershipCells = Globals.GetObject("dicOwnershipCells")
						End If
					End If
				 End SyncLock
												
				Dim sStdPov As String = ":V#YTD:C#Local:F#None:O#Top:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim sPerPov As String = ""
				If api.Entity.IsIC(iParent) Then
					sPerPov = ":E#" & Entity & ":I#" & Parent & ":S#" & PerScenario & ":T#" & Time & sStdPov						
				Else	
					sPerPov = ":E#" & Entity & ":I#None:S#" & PerScenario & ":T#" & Time & sStdPov
				End If	
				
				Dim sOpnPov As String = ""
				If api.Entity.IsIC(api.Pov.Parent.MemberId) Then
					sOpnPov= ":E#" & Entity & ":I#" & Parent & ":S#" & OpnScenario & ":T#PovPriorYearM12" & sStdPov
				Else	
					sOpnPov= ":E#" & Entity & ":I#None:S#" & OpnScenario & ":T#PovPriorYearM12" & sStdPov
				End If

																																																  
																																															  
																																																 
																																														   
																																																								

																																																
																																																
																																																	  
																																																									  

				If Not api.Members.IsChild(api.Pov.EntityDim.DimPk, iParent, iEntity) Then
	
                    If Not dicOwnershipCells.TryGetValue("A#PctCon" & sPerPov, pConD) Then

                        pConD = New datacell (api.Data.GetDataCell("A#PctCon" & sPerPov).DataCellPk, GetIndirectRelationship(si, api, dicOwnershipCells, api.Pov.EntityDim.DimPk, Entity, Parent, True, sTime),datacellstatus.CreateDataCellStatus(False, False))
                        dicOwnershipCells.TryAdd("A#PctCon" & sPerPov, pConD)
                    End If  
                    If Not dicOwnershipCells.TryGetValue("A#PctOwn" & sPerPov, pOwnD) Then
                        pOwnD   = New datacell (api.Data.GetDataCell("A#PctOwn" & sPerPov).DataCellPk, GetIndirectRelationship(si, api, dicOwnershipCells, api.Pov.EntityDim.DimPk, Entity, Parent, False, sTime),datacellstatus.CreateDataCellStatus(False, False))
						dicOwnershipCells.TryAdd("A#PctOwn" & sPerPov, pOwnD)

                    End If  

					If Not dicOwnershipCells.TryGetValue("A#Method" & sPerPov, methodD) Then
                        methodD = New datacell (api.Data.GetDataCell("A#Method" & sPerPov).DataCellPk, GetIndirectMethod(si, api, dicOwnershipCells, Entity, Parent, sTime),datacellstatus.CreateDataCellStatus(False, False))
						dicOwnershipCells.TryAdd("A#Method" & sPerPov, methodD)
                    End If
                Else    
                    If Not dicOwnershipCells.TryGetValue("A#PctCon" & sPerPov, pConD)       Then pConD   = api.Data.GetDataCell("A#PctCon" & sPerPov)   : dicOwnershipCells.TryAdd("A#PctCon" & sPerPov, pConD)             
                    If Not dicOwnershipCells.TryGetValue("A#PctOwn" & sPerPov, pOwnD)       Then pOwnD   = api.Data.GetDataCell("A#PctOwn" & sPerPov)   : dicOwnershipCells.TryAdd("A#PctOwn" & sPerPov, pOwnD)
                    If Not dicOwnershipCells.TryGetValue("A#Method" & sPerPov, methodD)     Then methodD = api.Data.GetDataCell("A#Method" & sPerPov)   : dicOwnershipCells.TryAdd("A#Method" & sPerPov, methodD)               
                End If
                
                
               	If Not dicOwnershipCells.TryGetValue("A#Scope" & sPerPov, scopeD)       Then scopeD = api.Data.GetDataCell("A#Scope" & sPerPov)   : dicOwnershipCells.TryAdd("A#Scope" & sPerPov, scopeD)
                If Not dicOwnershipCells.TryGetValue("A#Discontinued" & sPerPov, discontinuedD) Then discontinuedD = api.Data.GetDataCell("A#Discontinued" & sPerPov)   : dicOwnershipCells.TryAdd("A#Discontinued" & sPerPov, discontinuedD)
                If Not dicOwnershipCells.TryGetValue("A#PctCon" & sOpnPov, pCon_1D)     Then pCon_1D = api.Data.GetDataCell("A#PctCon" & sOpnPov)   : dicOwnershipCells.TryAdd("A#PctCon" & sOpnPov, pCon_1D)
                If Not dicOwnershipCells.TryGetValue("A#PctOwn" & sOpnPov, pOwn_1D)     Then pOwn_1D = api.Data.GetDataCell("A#PctOwn" & sOpnPov)   : dicOwnershipCells.TryAdd("A#PctOwn" & sOpnPov, pOwn_1D)
                If Not dicOwnershipCells.TryGetValue("A#Method" & sOpnPov, method_1D)   Then method_1D = api.Data.GetDataCell("A#Method" & sOpnPov) : dicOwnershipCells.TryAdd("A#Method" & sOpnPov, method_1D)         
                If Not dicOwnershipCells.TryGetValue("A#Discontinued" & sOpnPov, discontinued_1D) Then discontinued_1D = api.Data.GetDataCell("A#Discontinued" & sOpnPov)   : dicOwnershipCells.TryAdd("A#Discontinued" & sOpnPov, discontinued_1D)


				pCon    = pConD.CellAmount/100
				pCon_1  = pCon_1D.CellAmount/100
				pOwn    = pOwnD.CellAmount/100
				pOwn_1  = pOwn_1D.CellAmount/100
				VpCon   = pCon - pCon_1
				VpOwn   = pOwn - pOwn_1
				pMin    = pCon - pOwn
				pMin_1  = pCon_1 - pOwn_1
				VpMin   = pMin - pMin_1
				
				method = methodD.CellAmount
				method_1 = method_1D.CellAmount
				scope = scopeD.CellAmount
				discontinued = discontinuedD.CellAmount
				discontinued_1 = discontinued_1D.CellAmount

		Catch ex As Exception
			
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try

	End Sub

	Protected Overrides Sub Finalize()
		si = Nothing
		api = Nothing
		dicOwnershipCells = Nothing
	End Sub
	
Shared Public Function GetOwnershipInfoByName(si As SessionInfo, ByRef api As FinanceRulesApi, ByVal Globals As BRGlobals, ByRef sName As String, Optional ByRef sEntity As String = "", Optional ByRef sParent As String = "", Optional ByRef sScenario As String = "", Optional ByRef sTime As String = "") As Decimal
    
        Try
            If sEntity.Equals("") Then sEntity = api.Pov.Entity.Name
            If sParent.Equals("") Then sParent = api.Pov.Parent.Name
            If sTime.Equals("") Then sTime = api.Pov.Time.Name
            If sScenario.Equals("") Then sScenario = api.Pov.Scenario.Name
            
            Dim pConD As DataCell = Nothing
            Dim pOwnD As DataCell = Nothing
            Dim methodD As DataCell = Nothing
            Dim scopeD As DataCell = Nothing
            Dim discontinuedD As DataCell = Nothing
            Dim dicOwnershipCells As ConcurrentDictionary(Of String, datacell) = Nothing
            
			SyncLock Globals.LockObjectForInitialization
	            dicOwnershipCells = Globals.GetObject("dicOwnershipCells")
	            If dicOwnershipCells Is Nothing Then
	                dicOwnershipCells = New ConcurrentDictionary(Of String, datacell)
	                globals.SetObject("dicOwnershipCells", dicOwnershipCells)
	            End If
			End SyncLock
		
            Dim sStdPov As String = ":V#YTD:C#Local:F#None:O#Top:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim sPerPov As String = ":E#" & sEntity & ":I#" & sParent & ":S#" & sScenario & ":T#" & sTime & sStdPov
            
            Dim iEntity As Integer = api.Members.GetMemberId(dimtype.entity.Id, sEntity)
            Dim iParent As Integer = api.Members.GetMemberId(dimtype.entity.Id, sParent)
			
            If Not iParent.Equals(dimconstants.Unknown) AndAlso Not iEntity.Equals(dimconstants.Unknown) AndAlso Not api.Members.IsChild(api.Pov.EntityDim.DimPk, iParent, iEntity) Then

				If sName.XFEqualsIgnoreCase("pCon") Or sName.XFEqualsIgnoreCase("pMin") Then
                    If Not dicOwnershipCells.TryGetValue("A#PctCon" & sPerPov, pConD) Then
                        pConD = New datacell (api.Data.GetDataCell("A#PctCon" & sPerPov).DataCellPk, OwnershipClass.GetIndirectRelationship(si, api, dicOwnershipCells, api.Pov.EntityDim.DimPk, sEntity, sParent, True, sTime),datacellstatus.CreateDataCellStatus(False, False))
                        dicOwnershipCells.TryAdd("A#PctCon" & sPerPov, pConD)
                    End If
				End If
				
	            If sName.XFEqualsIgnoreCase("pOwn") Or sName.XFEqualsIgnoreCase("pMin") Then
                    If Not dicOwnershipCells.TryGetValue("A#PctOwn" & sPerPov, pOwnD) Then
                        pOwnD   = New datacell (api.Data.GetDataCell("A#PctOwn" & sPerPov).DataCellPk, OwnershipClass.GetIndirectRelationship(si, api, dicOwnershipCells, api.Pov.EntityDim.DimPk, sEntity, sParent, False, sTime),datacellstatus.CreateDataCellStatus(False, False))
						dicOwnershipCells.TryAdd("A#PctOwn" & sPerPov, pOwnD)
                    End If  
	            End If  
	           
	            If sName.XFEqualsIgnoreCase("method") Then
					If Not dicOwnershipCells.TryGetValue("A#Method" & sPerPov, methodD) Then
                       methodD = New datacell (api.Data.GetDataCell("A#Method" & sPerPov).DataCellPk, OwnershipClass.GetIndirectMethod(si, api, dicOwnershipCells, sEntity, sParent, sTime),datacellstatus.CreateDataCellStatus(False, False))
						dicOwnershipCells.TryAdd("A#Method" & sPerPov, methodD)
                    End If
				End If	
			End If   
			
				
				If sName.XFEqualsIgnoreCase("pCon") Or sName.XFEqualsIgnoreCase("pMin") Then
	                If Not dicOwnershipCells.TryGetValue("A#PctCon" & sPerPov, pConD) Then pConD   = api.Data.GetDataCell("A#PctCon" & sPerPov) : dicOwnershipCells.TryAdd("A#PctCon" & sPerPov, pConD)             
	            End If
	            
	            If sName.XFEqualsIgnoreCase("pOwn") Or sName.XFEqualsIgnoreCase("pMin") Then
	                If Not dicOwnershipCells.TryGetValue("A#PctOwn" & sPerPov, pOwnD)      Then pOwnD   = api.Data.GetDataCell("A#PctOwn" & sPerPov)    : dicOwnershipCells.TryAdd("A#PctOwn" & sPerPov, pOwnD)
	            End If  
	           
	            If sName.XFEqualsIgnoreCase("method") Then
	                If Not dicOwnershipCells.TryGetValue("A#Method" & sPerPov, methodD)   Then methodD = api.Data.GetDataCell("A#Method" & sPerPov)   : dicOwnershipCells.TryAdd("A#Method" & sPerPov, methodD)	            		
				End If
	        
	            If sName.XFEqualsIgnoreCase("scope") Then
	                If Not dicOwnershipCells.TryGetValue("A#Scope" & sPerPov, scopeD)   Then scopeD = api.Data.GetDataCell("A#Scope" & sPerPov)   : dicOwnershipCells.TryAdd("A#Scope" & sPerPov, scopeD)
	            End If
			
	            If sName.XFEqualsIgnoreCase("discontinued") Then
	                If Not dicOwnershipCells.TryGetValue("A#Discontinued" & sPerPov, discontinuedD)   Then discontinuedD = api.Data.GetDataCell("A#Discontinued" & sPerPov)   : dicOwnershipCells.TryAdd("A#Discontinued" & sPerPov, discontinuedD)
	            End If

			
			If sName.XFEqualsIgnoreCase("pCon")
                Return pConD.CellAmount/100
            ElseIf sName.XFEqualsIgnoreCase("pOwn")
                Return pOwnD.CellAmount/100
            ElseIf sName.XFEqualsIgnoreCase("pMin") 
                Return pConD.CellAmount/100 - pOwnD.CellAmount/100
            ElseIf sName.XFEqualsIgnoreCase("method")
				If sEntity.Equals("E163") And sparent.Equals("P148") Then
					api.LogMessage("K25 - " & methodD.CellAmountAsText)
				End If
                Return methodD.CellAmount
            ElseIf sName.XFEqualsIgnoreCase("scope")
                Return scopeD.CellAmount
            ElseIf sName.XFEqualsIgnoreCase("discontinued")
                Return discontinuedD.CellAmount
            Else
                Dim myEx As New exception("Incorrect Name: " & sName)
                'brapi.ErrorLog.LogError(si, myEx)
                Return 0
            End If
        Catch ex As Exception
            
            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
        End Try
    End Function
		
	Shared Public Function GetIndirectRelationship(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal dicOwnershipCells As ConcurrentDictionary(Of String, datacell), ByVal oDimpk As DimPk, ByVal sEntity As String, ByVal sParent As String, Optional ByVal isCons As Boolean = True, Optional ByVal sTime As String = "") As Decimal
        Try
            Dim dResult As Decimal = 0
            If sTime = "" Then sTime = api.Pov.Time.Name
            Dim iEntity As Integer = api.Members.GetMemberid(dimtypeid.Entity, sEntity)
            Dim iParent As Integer = api.Members.GetMemberid(dimtypeid.Entity, sParent)
            Dim iTime As Integer = api.Members.GetMemberid(dimtypeid.Time, sTime)
        
            If api.Members.IsDescendant(oDimpk, iParent, iEntity) Then
                dResult += GetIndirectRelationshipRec(si, api, dicOwnershipCells, oDimpk, iEntity, iParent, IIf(isCons, "PctCon", "PctOwn"), iTime)
			End If
            
            Return dResult
        Catch ex As Exception
            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
        End Try
        
    End Function

	Shared Public Function GetIndirectRelationship(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal dicOwnershipCells As ConcurrentDictionary(Of String, datacell), ByVal oDimpk As DimPk, ByVal iEntity As Integer, ByVal iParent As Integer, Optional ByVal isCons As Boolean = True, Optional ByVal iTime As Integer = 0) As Decimal
        Try
            Dim dResult As Decimal = 0
            If iTime = 0 Then iTime = api.Pov.Time.MemberId
            If api.Members.IsDescendant(oDimpk, iParent, iEntity) Then
                dResult += GetIndirectRelationshipRec(si, api, dicOwnershipCells, oDimpk, iEntity, iParent,  IIf(isCons, "PctCon", "PctOwn"), iTime)
            End If
            
            Return dResult
        Catch ex As Exception
            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
        End Try
        
    End Function
        
    Shared Public Function GetIndirectRelationshipRec(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal dicOwnershipCells As ConcurrentDictionary(Of String, datacell), ByVal oDimpk As DimPk, ByVal iEntity As Integer, ByVal iParent As Integer, ByVal sIsCons As String, ByVal iTime As Integer) As Decimal
        Try
            Dim dResult As Decimal = 0
            Dim sParent As String = api.Members.GetMemberName(dimtypeid.Entity, iParent)
            Dim sTime As String = api.Members.GetMemberName(dimtypeid.Time, iTime)
			
            Dim sStdPov As String = $":S#{api.pov.scenario.name}:T#{api.pov.time.name}:V#YTD:C#Local:F#None:O#Top:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim oConOrOwn As datacell = Nothing

            For Each oChild As Member In api.Members.GetChildren(oDimpk, iParent)
				
				Dim sTest As String = $"A#{sIsCons}:E#{oChild.Name}:I#{sParent}" & sStdPov
				If Not dicOwnershipCells.TryGetValue(sTest, oConOrOwn) Then oConOrOwn = api.Data.GetDataCell(sTest) : dicOwnershipCells.TryAdd(sTest, oConOrOwn)             
				Dim dConOrOwn As Decimal = oConOrOwn.CellAmount/100
                If oChild.MemberId = iEntity Then
                    dResult = dResult + dConOrOwn
                ElseIf api.Members.IsDescendant(oDimpk, oChild.MemberId, iEntity) Then
                    dResult = dResult + dConOrOwn * (GetIndirectRelationshipRec(si, api, dicOwnershipCells, oDimpk, iEntity, oChild.MemberId, sIsCons,iTime) /100)
                End If
            Next
            Return dResult*100
        Catch ex As Exception
            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
        End Try        
    End Function
	
    Shared Public Function GetIndirectMethod(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal dicOwnershipCells As ConcurrentDictionary(Of String, datacell), ByVal sEntity As String, ByVal sParent As String, Optional ByVal sTime As String = "") As Integer
        Try
            Dim iEntity As Integer = api.Members.GetMemberId(dimtype.entity.Id, sEntity)
            Dim iParent As Integer = api.Members.GetMemberId(dimtype.entity.Id, sParent)
            If sTime = "" Then sTime = api.Pov.Time.Name
            Dim iTime As Integer = api.Members.GetMemberid(dimtypeid.Time, sTime)
               
            Return GetIndirectMethod(si, api, dicOwnershipCells, iEntity, iParent, iTime)
        Catch ex As Exception
            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
        End Try
            
    End Function

    Shared Public Function GetIndirectMethod(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal dicOwnershipCells As ConcurrentDictionary(Of String, datacell), ByVal iEntity As Integer, ByVal iParent As Integer, Optional ByVal iTime As Integer = 0) As Integer
        Try         
            Dim oDimpk As DimPk = api.Pov.EntityDim.DimPk
            Dim oDirectMethod As Integer = FSKMethod.NOCONSO
            Dim oMethod As Integer = FSKMethod.NOCONSO
            If iTime = 0 Then iTime = api.Pov.Time.MemberId
            Dim sEntity As String = api.Members.GetMemberName(dimtypeid.Entity, iEntity)
            Dim sParent As String = api.Members.GetMemberName(dimtypeid.Entity, iParent)
            Dim sTime As String = api.Members.GetMemberName(dimtypeid.Time, iTime)

            Dim sStdPov As String = $":S#{api.pov.scenario.name}:T#{api.pov.time.name}:V#YTD:C#Local:F#None:O#Top:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim oMethodDataCell As datacell = Nothing	
			   
            If api.Members.IsDescendant(oDimpk,iParent,iEntity) Then
                If api.Members.IsChild(oDimpk, iParent, iEntity) Then  
																												
																   
					Dim sTest As String = $"A#Method:E#{sEntity}:I#{sParent}" & sStdPov
					If Not dicOwnershipCells.TryGetValue(sTest, oMethodDataCell) Then oMethodDataCell = api.Data.GetDataCell(sTest) : dicOwnershipCells.TryAdd(sTest, oMethodDataCell)             
					oDirectMethod = oMethodDataCell.CellAmount 'api.Data.GetDataCell($"A#Method:E#{sEntity}:I#{sParent}" & sStdPov).CellAmount
					If oDirectMethod = 0 Then oDirectMethod = FSKMethod.NOCONSO
																												
                    If  oDirectMethod.equals(FSKMethod.HOLDING) Or _
                        oDirectMethod.equals(FSKMethod.IG) Or _
                        oDirectMethod.equals(FSKMethod.IP) Then
						Return oDirectMethod
                    End If
                End If												   
            
               For Each oChild As Member In api.Members.GetChildren(oDimpk, iParent)
																																		 
																	   
					Dim sTest As String = $"A#Method:E#{oChild.Name}:I#{sParent}" & sStdPov
					If Not dicOwnershipCells.TryGetValue(sTest, oMethodDataCell) Then oMethodDataCell = api.Data.GetDataCell(sTest) : dicOwnershipCells.TryAdd(sTest, oMethodDataCell)    
                    Dim oParentChildMethod As Integer = oMethodDataCell.CellAmount 
					If oParentChildMethod = 0 Then oParentChildMethod = FSKMethod.NOCONSO
                    Dim oChildMethod As Integer = GetIndirectMethod(si, API, dicOwnershipCells, iEntity, oChild.MemberId,iTime)
																								   
                   oMethod = GetMaxMethod(si, oMethod, GetMinMethod(si, oParentChildMethod, oChildMethod))       
                Next
								
				Return GetMaxMethod(si, oMethod, oDirectMethod)
				
            Else
				Return FSKMethod.NOCONSO
            End If
            
        Catch ex As Exception
            
            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
        End Try
    End Function
    
    Shared Public Function GetMaxMethod(ByVal si As SessionInfo, ByVal oMethod1 As Integer, ByVal oMethod2 As Integer) As Integer
        Try
        
            If oMethod1.Equals(oMethod2) Then
                Return oMethod1
            ElseIf oMethod1.Equals(FSKMethod.HOLDING) Or oMethod2.Equals(FSKMethod.HOLDING) Then
                Return FSKMethod.Holding
            ElseIf oMethod1.Equals(FSKMethod.IG) Or oMethod2.Equals(FSKMethod.IG) Then
                Return FSKMethod.IG
            ElseIf oMethod1.Equals(FSKMethod.IP) Or oMethod2.Equals(FSKMethod.IP) Then
                Return FSKMethod.IP
            ElseIf oMethod1.Equals(FSKMethod.MEE) Or oMethod2.Equals(FSKMethod.MEE) Then
                Return FSKMethod.MEE
            Else
                Return FSKMethod.NOCONSO
            End If  
                
        Catch ex As Exception
            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
        End Try
    End Function        
    
    Shared Public Function GetMinMethod(ByVal si As SessionInfo, ByVal oMethod1 As Integer, ByVal oMethod2 As Integer) As Integer
        Try
        
            If oMethod1.Equals(oMethod2) Then
                Return oMethod1
            ElseIf oMethod1.Equals(FSKMethod.NOCONSO) Or oMethod2.Equals(FSKMethod.NOCONSO) Then
                Return FSKMethod.NOCONSO
            ElseIf oMethod1.Equals(FSKMethod.MEE) Or oMethod2.Equals(FSKMethod.MEE) Then
                Return FSKMethod.MEE
            ElseIf oMethod1.Equals(FSKMethod.IP) Or oMethod2.Equals(FSKMethod.IP) Then
                Return FSKMethod.IP
            ElseIf oMethod1.Equals(FSKMethod.IG) Or oMethod2.Equals(FSKMethod.IG) Then
                Return FSKMethod.IG
            Else
                Return FSKMethod.HOLDING
            End If  
                
        Catch ex As Exception
            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
        End Try
    End Function        
	
																																																																														 
 
	 
														   
														   
													 
																 
   
								  
								  
									
								   
										  
																			   
   
															 
									   
																	 
															 
									 
															  
		 
	  
																														
																										  

																			   
																																														   
		 
   
																			   
																																															
		  
   
											 
																																															   
		 
  
											
																																														 
		 

												   
																																																								   
		 

									  
							   
										  
							   
										   
													  
											
							 
										   
							
												  
								   
	   
														 
									 
			
		 
					   
   
														   
		 

			 
End Class

#End Region

'Support Constants
#Region "Methods"
' (1,2,3,4,5,6,7,8,9,10,11) 1 = HOLDING, 2 = IG, 3 = MEE, 4 = PROPORTIONNELLE, 5 = SORTANTE, 6 = EXITM, 7 = NOCONSO, 8 = Discontinued, 9 = MERGED, 10 = EXITING EQUITY, 11 = EXITING DISCONTINUED
	Public Enum FSKMethod
		HOLDING = 1
		IG = 2
		MEE = 3
		IP = 4
		NOCONSO = 7
	End Enum
#End Region

#Region "Scope"
'ENTRY, MERGE, EXITM, SORTANTE 10,20,30,40
	Public Enum FSKScope
		DefaultScope = 0
		Entry = 10
		Merge = 20
		EXITM = 30
	End Enum
#End Region

End Namespace