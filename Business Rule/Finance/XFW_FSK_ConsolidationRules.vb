#Region "Imports"

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

'Additional Imports
Imports System.Collections.Concurrent

#End Region
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 		XFW_FSK_ConsolidationRules
		'
		'Description:			Consolidation Engine based on the Financial Starter Kit
		'
		'Usage:					Used to provide an accelerator for complex consolidation. 
		'						To be put in the cube's formula with custom consolidation settings
		'						Is used by MemberFormulas and No.Input Rules
		'						Please refer to documentation
		'
		'
		'Created By:			OneStream
		'Version: 				V1.0
		'Date Created:			01 June 2020
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

'CWe 2020-4-24 Exit and Merge
'Created two methods to eliminate the balance sheet. They will called in the calculate section of the Main class
'-to eliminate the entity itself
'-to eliminate all intercompany relationships with the exiting company

'CWe 2020-4-25 Change the order of ExitIC and IFRS5 consolidaton 

'CWe 2020-4-30 Fix the AUG or DIM problem for titre by splitting the sections and turning the signs

'FSM 2020-7-14 Commented lines 129-134; 3470-3475; Region IFRS and Region EXIT

#End Region

Namespace OneStream.BusinessRule.Finance.XFW_FSK_ConsolidationRules
		
#Region "Main Class"		
	Public Class MainClass
		Private bDoLog As Boolean = False
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				
				'Enable And disable line item logging
				bDoLog = globals.GetBoolValue("Log_CHP", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Log_CHP").XFConvertToBool)
				globals.SetBoolValue("Log_CHP", bDoLog)

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
					Case Is = FinanceFunctionType.Calculate
						
						Me.CalculateElimP2(si, api, globals, args)
						'Consolidate the IC with exiting companies
		'fs				Me.CalculateExitIC(si, api, globals, args)
						'Consolidate the IFRS5
		'fs				Me.CalculateIFRS5(si, api, globals, args)
						'Consolidate the Exit or Merge of a comopany
		'fs				Me.CalculateExit(si, api, globals, args)
						
					Case Is = FinanceFunctionType.ConsolidateShare
						ConsolidateShare(si, api, globals, args)

					Case Is = FinanceFunctionType.ConsolidateElimination
						CalculateElim(si, api, globals, args)
				End Select

				'Dim Nat As NatureClass = Me.InitNatureClass(si, api, globals, api.Pov.UD4Dim.DimPk)'HC
'FS 20200626 Disabled lines for accounts FLUXDIVX
'				api.Data.ClearCalculatedData("A#FLUXDIVR:I#None",True,True,True)
'				api.Data.ClearCalculatedData("A#FLUXDIVREH:I#None",True,True,True)
'				api.Data.ClearCalculatedData("A#FLUXADIVR:I#None",True,True,True)
'				api.Data.ClearCalculatedData("A#FLUXADIVREH:I#None",True,True,True)
'				api.Data.ClearCalculatedData("I#None:" & nat.NatDimType.Abbrev & "#" & Nat.Minoritaires.Name,True,True,True)
'				api.Data.ClearCalculatedData("I#None:" & nat.NatDimType.Abbrev & "#" & Nat.Equity.Name,True,True,True)
					
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function GetUDasICId(ByVal api As FinanceRulesApi, ByVal oAcell As DataBufferCell, ByVal iICUD As Integer, ByVal iUD As Integer) As Integer
			Select Case iICUD 
			
				Case dimtype.UD1.Id
					Return api.members.GetMemberId(iUD, oAcell.DataBufferCellPk.GetUD1Name(api))
				Case dimtype.UD2.Id
					Return api.members.GetMemberId(iUD, oAcell.DataBufferCellPk.GetUD2Name(api))
			End Select
		End Function											

		Public Function GetUDId(ByVal oAcell As DataBufferCell, ByVal iUD As Integer) As Integer
			Select Case iUD 
			
				Case dimtype.UD1.Id
					Return oAcell.DataBufferCellPk.UD1Id
				Case dimtype.UD2.Id
					Return oAcell.DataBufferCellPk.UD2Id
			End Select
		End Function		

		Public Function GetUDName(ByVal api As FinanceRulesApi, ByVal oAcell As DataBufferCell, ByVal sUD As String) As String
			Select Case sUD 
			
				Case "UD1"
					Return oAcell.DataBufferCellPk.GetUD1Name(api)
				Case "UD2"
					Return oAcell.DataBufferCellPk.GetUD2Name(api)
			End Select
		End Function
		
		
#End Region

'Consolidation Business Rules
#Region "Share Calculation"
   
		Private Sub ConsolidateShare(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			Try
				If api.Entity.HasChildren() Then
					api.ExecuteDefaultShare()
				Else
					'Clear all previously calculated data from the Share consolidation member.
					api.Data.ClearCalculatedData(True, True, True)
					
					Dim Entity As String = api.Pov.Entity.Name
					Dim EntityMember As Member = api.Pov.Entity
					Dim refOwnTime As String = api.Pov.Time.Name
					Dim Period As String = api.Pov.Time.Name
					Dim GlobalClass As New GlobalClass(si,api,args)
					Dim Opescenario As String = GlobalClass.OpeScenario
					Dim Acct As AccountClass = Me.InitAccountClass(si, api, globals)
					Dim Flow As FlowClass = Me.InitFlowClass(si, api, globals)
					Dim Nat As NatureClass = Me.InitNatureClass(si, api, globals, api.Pov.UD4Dim.DimPk)
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
								If flowMember.MemberId = flow.Ouv.MemberId Then
									flowDestMember = flow.VarC
								Else
									flowDestMember = flowMember
								End If
								
								Dim accountId As Integer = cell.DataBufferCellPk.AccountId
								Dim acctMember As Member = api.Members.GetMember(DimType.Account.Id, cell.DataBufferCellPk.AccountId)
								Dim NatureSource As Member = Nat.GetMember(cell.DataBufferCellPk.Item(nat.NatDimType.Id))
								
								Dim text3Acct As String = api.Account.Text(accountId, 3)
								Dim RegleElim As String							
								Dim UndSco As Integer = InStr(text3Acct, "_")
	        					If UndSco > 0 Then RegleElim = Left(text3Acct, UndSco - 1) Else RegleElim = ""
								
								Dim Nature As String
								If NatureSource.Name = "Pack" Then 
									Nature = "Pack" 
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
									'If the consolidation method is not Equity Pickup/Exit/Non Consolidated
										'If method <> FSKMethod.MEE And method <> FSKMethod.SORTIE And method <> FSKMethod.NOCONSO And method <> FSKMethod.EXMEE Then 
								'SBM Nueva seccion para entrada de perimetro
									If scope = FSKScope.ENTRY Then
										If flowMember.MemberId <> flow.OUV.MemberId Then
											share.Book(cell,,flow.VarC.MemberId,,,,,,,,,,,Own.pCon ,,sDFRule ,900)
										Else
											share.Book(cell,,flow.ENT.MemberId,,,,,,,,,,,Own.pCon ,,sDFRule ,901)
										End If
														  
																													  
			  
																																																														  
				
		 
											 
														  
																																																															
		  
			  
																																																															
																																																												

				
		  
		  
									ElseIf method <> FSKMethod.MEE And scope <> FSKScope.SORTIE And method <> FSKMethod.NOCONSO Then
										'If the flow is AFF/DIV
										If flowMember.MemberId = flow.Aff.MemberId Then
											'Consolidate the data at the % of consolidation of N-1
											share.Book(cell,,flowDestMember.MemberId,,,,,,,,,,,Own.pCon_1 ,,sDFRule ,10001)
											'Consolidate the data on the VARC flow at the change in % of consolidation
											share.Book(cell,,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,sDFRule ,10002)
										'For other flows	
										ElseIf flowMember.MemberId = flow.Div.MemberId Then
											'Consolidate the data at the % of consolidation of N-1
											share.Book(cell,,flowDestMember.MemberId,,,,,,,,,,,Own.pCon_1 ,,sDFRule ,10003)
											'Consolidate the data on the VARC flow at the change in % of consolidation
											share.Book(cell,,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,sDFRule ,10004)
		  
										'For other flows	
										Else
											'If the consolidation method is not Discontinued and not Exiting discontinued
											If method <> FSKMethod.DIS And method <> FSKMethod.EXDIS Then
												'Consolidate the data at the % of consolidation of N
												share.Book(cell,,flowDestMember.MemberId,,,,,,,,,,,Own.pCon ,,sDFRule ,10005)
											End If
										End If	
									End If
								End If
								
								
								
								'If the consolidation method is not Exit/Not Consolidated
'								If method <> FSKMethod.SORTIE And method <> FSKMethod.NOCONSO Then
								If scope <> FSKScope.SORTIE And method <> FSKMethod.NOCONSO Then
									'If the consolidation rule is not Blank and the flow is not CLO
									If RegleElim<>"" And flowMember.MemberId <> flow.Clo.MemberId Then
										
					                    '----------------------------------------------------
										' Equity accounts
					                    '----------------------------------------------------
										If RegleElim="CAPI" Then
												
											If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											'If the flow is AFF/DIV	
											If flowMember.MemberId = flow.Div.MemberId Then
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Or method=FSKMethod.EXMEE Then
													'Consolidate the data at the % of consolidation of N-1
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon_1 ,,RegleElim,10006)
													'Consolidate the data on the VARC flow at the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,RegleElim,10007)
													'Trigger the account Investments in equity affiliates at the % of consolidation of N-1
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon_1 ,,RegleElim,10008)
													'Trigger the account Investments in equity affiliates on the VARC flow at the change in % of consolidation
													share.Book(cell,acct.MEE.MemberId,flow.VarC.MemberId,,,,,,NatureDest.MemberId,,,,,Own.VpCon ,,RegleElim,10009)
												End If
												
												'If the entity is not consolidated in N-1
												If Own.pCon_1 = 0 Then
													'Consolidate the data on the VARC flow at - the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,-Own.VpCon,,RegleElim,10010)
													'Consolidate the data on the ENT flow at the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.Ent.MemberId,,,,,,,,,,,Own.VpCon ,,RegleElim,10011)
												End If
												
											ElseIf flowMember.MemberId = flow.Div.MemberId Then
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Or method=FSKMethod.EXMEE Then
													'Consolidate the data at the % of consolidation of N-1
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,-Own.pCon_1 ,,RegleElim,10012)
													'Consolidate the data on the VARC flow at the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,RegleElim,10013)
													'Trigger the account Investments in equity affiliates at the % of consolidation of N-1
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,-Own.pCon_1 ,,RegleElim,10014)
													'Trigger the account Investments in equity affiliates on the VARC flow at the change in % of consolidation
													share.Book(cell,acct.MEE.MemberId,flow.VarC.MemberId,,,,,,NatureDest.MemberId,,,,,Own.VpCon ,,RegleElim,10015)
												End If
												
												'If the entity is not consolidated in N-1
												If Own.pCon_1 = 0 Then
													'Consolidate the data on the VARC flow at - the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,-Own.VpCon,,RegleElim,10016)
													'Consolidate the data on the ENT flow at the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.Ent.MemberId,,,,,,,,,,,Own.VpCon ,,RegleElim,10017)
												End If
												
											'For other flows	
											Else
												'If the consolidation method is Equity Pickup / not exiting equity
												'fsm desactivado esta parte, para que no escriba en Shared. Se actualiza la parte de elimination
'												If method=FSKMethod.MEE Or  method=FSKMethod.EXMEE Then
'													'Consolidate the data at the % of consolidation
'													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon ,,RegleElim,10018)
'													'Trigger the account Investments in equity affiliates at the % of consolidation
'													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10019)
'												End If
											End If

											
										'----------------------------------------------------
										' Account Currency Translation Adjustments: CTA
					                    '----------------------------------------------------
										ElseIf RegleElim="CAPIC" Then
											
											'If the flow is AFF
											If flowMember.MemberId = flow.Aff.MemberId Then												
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Or method=FSKMethod.EXMEE Then	
													'Consolidate the data at the % of consolidation of N-1
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon_1 ,,RegleElim,10020)
													'Consolidate the data on the VARC flow at the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,RegleElim,10021)
													'Trigger the account Investments in equity affiliates at the % of consolidation of N-1
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon_1 ,,RegleElim,10022)
													'Trigger the account Investments in equity affiliates on the VARC flow at the change in % of consolidation
													share.Book(cell,acct.MEE.MemberId,flow.VarC.MemberId,,,,,,NatureDest.MemberId,,,,,Own.VpCon ,,RegleElim,10023)
												End If
												
											'For other flows														
											Else																						
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Or method=FSKMethod.EXMEE Then
													'Consolidate the data at the % of consolidation of N
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon ,,RegleElim,10024)
													'Trigger the account Investments in equity affiliates at the % of consolidation
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10025)
												End If
											End If
											
										'----------------------------------------------------
										' Account: Current Year Earnings
					                    '----------------------------------------------------
										ElseIf RegleElim="RESU" Then
											If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											
											'If the flow is AFF		
											If flowMember.MemberId = flow.Aff.MemberId Then										
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Or method=FSKMethod.EXMEE Then
													'Consolidate the data at the % of consolidation of N-1
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon_1 ,,RegleElim,10026)
													'Consolidate the data on the VARC flow at the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,RegleElim,10027)
													'Trigger the account Investments in equity affiliates at the % of consolidation of N-1
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon_1 ,,RegleElim,10028)
													'Trigger the account Investments in equity affiliates on the VARC flow at the change in % of consolidation
													share.Book(cell,acct.MEE.MemberId,flow.VarC.MemberId,,,,,,NatureDest.MemberId,,,,,Own.VpCon ,,RegleElim,10029)
												End If
											
											'For other flows	
											Else
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Or method=FSKMethod.EXMEE Then
													'Consolidate the data at the % of consolidation of N
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon ,,RegleElim,10030)
													'Trigger the account Investments in equity affiliates at the % of consolidation
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10031)
													
													'If the flow is RES
													If flowMember.MemberId = flow.Res.MemberId Then 
														'Trigger the account Net income from equity companies on the None flow at the % of consolidation
														share.Book(cell,acct.ResultatMEE.MemberId,flow.None.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10032)
													End If
												End If
											End If
											
										'----------------------------------------------------
										' Account 120C: CTA from Current Year Earnings
					                    '----------------------------------------------------	
										ElseIf RegleElim="RESUC" Then										
											If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											
												'If the flow is AFF		
											If flowMember.MemberId = flow.Aff.MemberId Then										
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Or method=FSKMethod.EXMEE Then
													'Consolidate the data at the % of consolidation of N-1
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon_1 ,,RegleElim,10033)
													'Consolidate the data on the VARC flow at the change in % of consolidation
													share.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,,,,,,,,,,Own.VpCon ,,RegleElim,10034)
													'Trigger the account Investments in equity affiliates at the % of consolidation of N-1
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon_1 ,,RegleElim,10035)
													'Trigger the account Investments in equity affiliates on the VARC flow at the change in % of consolidation
													share.Book(cell,acct.MEE.MemberId,flow.VarC.MemberId,,,,,,NatureDest.MemberId,,,,,Own.VpCon ,,RegleElim,10036)
												End If
												
											'For other flows	
											Else
												'If the consolidation method is Equity Pickup / not exiting equity
												If method=FSKMethod.MEE Or method=FSKMethod.EXMEE Then
													'Consolidate the data at the % of consolidation of N
													share.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,,,,,,,,,,Own.pCon ,,RegleElim,10037)
													'Trigger the account Investments in equity affiliates at the % of consolidation
													share.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10038)
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
						'If the consolidation method is not EXIT/Non Consolidated/Discontinued/exiting discontinued
'						If method <> FSKMethod.SORTIE And method <> FSKMethod.NOCONSO And method <> FSKMethod.DIS And method <> FSKMethod.EXDIS Then							
						If scope <> FSKScope.SORTIE And method <> FSKMethod.NOCONSO And method <> FSKMethod.DIS Then							
							
							If Not sourceDataBuffer2 Is Nothing Then
								For Each cell As DataBufferCell In sourceDataBuffer2.DataBufferCells.Values
									
									If (Not cell.CellStatus.IsNoData) Then
										'Trigger the VARC flow at -100%
										share.Book(cell,,flow.VarO.MemberId,,,,,,,,,,,-1,,sPPRule,10039)
									End If
								Next
							End If

						'If the consolidation method is EXIT
'						ElseIf method = FSKMethod.SORTIE Then
						ElseIf scope = FSKScope.SORTIE Then
							If Not sourceDataBuffer2 Is Nothing Then
								For Each cell As DataBufferCell In sourceDataBuffer2.DataBufferCells.Values
									If (Not cell.CellStatus.IsNoData) Then
										'Trigger the SOR flow at -100%
										share.Book(cell,,flow.Sor.MemberId,,,,,,,,,,,-1,,sPPRule,10040)
									End If
								Next
							End If
							
						ElseIf method = FSKMethod.DIS Then
							
						ElseIf method = FSKMethod.EXDIS Then
							
						End If	
						'Store the results for the Share consolidation member that is currently being executed.
						api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
						share.WriteLogToTable
					End If

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
				If Not api.Entity.HasChildren() Then
					
					'Get the percent consolidation.				
					Own = New OwnershipClass(si,api,globals,Entity,,,, OpeScenario)
					method = Own.method
					scope = Own.scope
					
				Else	
					'method = 2 and 100% Ownership
					Own = New OwnershipClass(si,1,1)
					method = FSKMethod.IG
					scope = FSKScope.DefaultScope
					
				End If

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
						For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
						
					'	resultDataBuffer.LogDataBuffer(api,api.Pov.Entity.Name &"-"& api.Pov.Time.Name &"-"& api.Pov.Cons.Name & ":Target Data Buffer", 1000)
					'	sourceDataBuffer.LogDataBuffer(api,api.Pov.Entity.Name &"-"& api.Pov.Time.Name &"-"& api.Pov.Cons.Name & ":Source Data Buffer", 1000)
							
							'If (Not cell.CellStatus.IsNoData) And scope <> FSKScope.SORTIE And method <> FSKMethod.NOCONSO Then	SBM 
			                If (Not cell.CellStatus.IsNoData) And method <> FSKMethod.NOCONSO Then	
								Dim flowMember As Member = api.Members.GetMember(DimType.flow.Id, cell.DataBufferCellPk.FlowId)
								Dim flowDestMember As Member
								
								'Si le flux source est OUV 
								'If the source flow is OUV
								If flowMember.MemberId = flow.Ouv.MemberId And scope <> FSKScope.SORTIE And scope <> FSKScope.ENTRY Then 'Luxx SBM
									'le flux de destination est VARC
									'the destination flow is VARC
									flowDestMember = flow.VarC
								'Pour les autres flux
								'For other flows	
								Else
									'le flux de destination est le flux source
									'the destination flow is the source flow
									flowDestMember = flowMember
								End If	
									
								Dim bDoEliminate As Boolean = True
								Dim acctMember As Member = api.Members.GetMember(DimType.Account.Id, cell.DataBufferCellPk.AccountId)
								Dim NatureSource As Member = Nat.getMember(cell.DataBufferCellPk.Item(nat.NatDimType.Id))'api.Members.GetMember(DimType.UD4.Id, cell.DataBufferCellPk.UD4Id)
								Dim accountId As Integer = cell.DataBufferCellPk.AccountId

								'Determine la regle de consolidation et la Nature de destination a partir du text3 du compte
								'Exemple: le compte 101000 a le text3 suivant: CAPI_CAP
								'La regle de consolidation est donc CAPI et la nature de destination est CAP
								
								'Determine consolidation rule and UD4 nature from account text3
								'For Example: Account 101000 text3 is CAPI_CAP, rule is CAPI and UD4 is CAP								
								Dim text3Acct As String = api.Account.Text(accountId, 3)
								Dim RegleElim As String							
								Dim UndSco As Integer = InStr(text3Acct, "_")
	        					If UndSco > 0 Then RegleElim = Left(text3Acct, UndSco - 1) Else RegleElim = ""
								Dim Nature As String
	        					'If Len(text3Acct) = UndSco Then Nature = "None" Else Nature = Right(text3Acct, Len(text3Acct) - UndSco)							
								'fs Jornals "Gestion" in the source to be eliminated on nature ELCE_GES 
								If Len(text3Acct) = UndSco Then 
									Nature = "None" 
								ElseIf NatureSource.Name = "Pack" Then 
									Nature = "Pack"
								ElseIf NatureSource.Name = "RetSocG_R1" Or NatureSource.Name = "RetSocG_R2" Or NatureSource.Name = "RetSocG1_PL" Then
									Nature = "ELCE_GES"
								Else 
									Nature = Right(text3Acct, Len(text3Acct) - UndSco)							
								End If
								Dim NatureDest As Member = Nat.getMember(Nature)'api.Members.GetMember(DimType.NatureDim.Id, Nature)
								Dim icMember As Member = api.Members.GetMember(DimType.IC.Id, cell.DataBufferCellPk.ICId)
								Dim icName As String = icMember.Name

								Dim icMethod As Integer  = OwnershipClass.GetOwnershipInfoByName(si, api, globals, "method", icName)
								Dim icScope As Integer  = OwnershipClass.GetOwnershipInfoByName(si, api, globals, "scope", icName)
								Dim icIsSplit As Boolean = False 
								Dim CanEliminate As Boolean = False
								Dim icMethod1245 As Boolean = False
								Dim icMethod12345 As Boolean = False
								Dim icRef As String
								icRef=icName
								
								Dim isICPartnerADescendantOfParentEnt = api.Members.IsDescendant(api.Pov.EntityDim.DimPk, api.Pov.Parent.MemberId, cell.DataBufferCellPk.ICId, Nothing)
									
								If isICPartnerADescendantOfParentEnt = True Then
									'CanEliminate = True
									If icMethod = FSKMethod.HOLDING Or icMethod = FSKMethod.IG Or icMethod = 4 Or icScope = FSKScope.SORTANTE Then
										icMethod1245 = True
										icMethod12345 = True
									ElseIf icMethod	= FSKMethod.MEE Then
										icMethod12345 = True
									End If	
								End If
				
								
								If RegleElim<>"" And flowMember.MemberId <> flow.Clo.MemberId Then
									'----------------------------------------------------
					                ' Capitaux Propres
									' Equity accounts
					                '----------------------------------------------------
									
									'******************************************************************************************
										'fs Matrix consolidation by BU (UD1) to be included in IC elim

										Dim sUD As String = "UD1"
										Dim sICUD As String = "UD2"
										Dim iUD As String = DimType.UD1.Id
										Dim iICUD As String = DimType.UD2.Id
										Dim oUDDim As DimPk = api.Pov.UD1Dim.DimPk
										Dim oICUDDim As DimPk = api.Pov.UD2Dim.DimPk
										Dim iUDTop As Integer = api.Members.GetMemberId(dimtype.ud1.Id, "Top")
										Dim iUDTopMC As Integer = api.Members.GetMemberId(dimtype.ud1.Id, "MC")
										Dim iUDTopENTE As Integer = api.Members.GetMemberId(dimtype.ud1.Id, "ENTES")
										'Dim iUDTop As Integer = api.Members.GetMemberId(dimtype.ud1.Id, "DN")
										Dim elimMember As String = "None"
										Dim elimMemberMC As String = "None"
										Dim elimMemberENTE As String = "None"
										Dim ParentAccountPL As Integer = api.Members.GetMemberId(dimtypeid.Account, "PG")
										Dim ParentAccountCDR As Integer = api.Members.GetMemberId(dimtypeid.Account, "CDR")
										Dim ParentAccountInvDesinv As Integer = api.Members.GetMemberId(dimtypeid.Account, "INVER_DESINV")
										Dim ParentAccountCDCalc As Integer = api.Members.GetMemberId(dimtypeid.Account, "CTAS_CALC")
										Dim ParentAccountCV As Integer = api.Members.GetMemberId(dimtypeid.Account, "CV_A")
										Dim ParentAccountBS As Integer = api.Members.GetMemberId(dimtypeid.Account, "BS")
										Dim	elimMemberId = api.Members.GetMemberId(dimTypeId.UD1, "None")
										Dim	elimMemberIdMC = api.Members.GetMemberId(dimTypeId.UD1, "None")
										Dim	elimMemberIdENTE = api.Members.GetMemberId(dimTypeId.UD1, "None")
										Dim destAccountId As Integer = api.Members.GetMemberId(dimtypeid.Account,cell.DataBufferCellPk.GetAccountName(api))
										
										If Not api.Members.IsDescendant(accountDimPk, ParentAccountPL, destAccountId) _ 
											And Not api.Members.IsDescendant(accountDimPk, ParentAccountCDR, destAccountId) _ 
											And Not api.Members.IsDescendant(accountDimPk, ParentAccountInvDesinv, destAccountId) _ 
											And Not api.Members.IsDescendant(accountDimPk, ParentAccountCDCalc, destAccountId) _ 
											And Not api.Members.IsDescendant(accountDimPk, ParentAccountCV, destAccountId) _ 
											And Not api.Members.IsDescendant(accountDimPk, ParentAccountBS, destAccountId) _ 
											Or api.Pov.UD1.Name.Equals("None") Or api.Pov.UD2.Name.Equals("None") Then								
											elimMember = "None"
										Else
											If cell.DataBufferCellPk.GetUD1Name(api) = cell.DataBufferCellPk.GetUD2Name(api) Then
												elimMember = cell.DataBufferCellPk.GetUD1Name(api)
											Else
												If cell.DataBufferCellPk.GetUD1Name(api).Equals(cell.DataBufferCellPk.Item(sUD)) Then
												Else
														elimMember = "Elim_" & api.Members.GetFirstCommonParent(oUDDim, iUDTop, _ 
																														  Me.GetUDId(cell, iUD), _
																														  Me.GetUDasICId(api, cell, iICUD, iUD)).Name
														elimMemberMC = "Elim_" & api.Members.GetFirstCommonParent(oUDDim, iUDTopMC, _ 
																														  Me.GetUDId(cell, iUD), _
																														  Me.GetUDasICId(api, cell, iICUD, iUD)).Name
														elimMemberENTE = "Elim_" & api.Members.GetFirstCommonParent(oUDDim, iUDTopENTE, _ 
																														  Me.GetUDId(cell, iUD), _
																														  Me.GetUDasICId(api, cell, iICUD, iUD)).Name
											    End If																	 
											End If								
										End If	
										elimMemberId = api.Members.GetMemberId(dimTypeId.UD1, elimMember)																	  
										elimMemberIdMC = api.Members.GetMemberId(dimTypeId.UD1, elimMemberMC)																	  
										elimMemberIdENTE = api.Members.GetMemberId(dimTypeId.UD1, elimMemberENTE)	
										
										

										'fs end of U1#Elim_XXX calcuation
										'******************************************************************************************

									If RegleElim="CAPI" Then
										'Si la methode de consolidation n'est pas Holding ou (methode est Holding et compte est 106R)
										'If consolidation method is not Holding ou (consolidation method is Holding and account is 106R)
										'If method<>FSKMethod.HOLDING Or (method=FSKMethod.HOLDING And acctMember.MemberId = acct.RsvR.MemberId) Then
										If method<>FSKMethod.HOLDING Then
											'fs disabled to use the UD4 member defined in text3 for account
											'If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											
											'Si le flux est AFF or DIV
											'If flow is AFF or DIV
											If flowMember.MemberId = flow.Aff.MemberId Or flowMember.MemberId = flow.Div.MemberId Then
												'Si l'entite n'est pas consolidee en N-1
												'If the entity is not consolidated in N-1
												'If Own.pCon_1 = 0 Then	'Mehtode ENTRANTES	
												If scope = FSKScope.ENTRY Then	'Scope Entry SBM							        												
											
							                        'Elimine le compte Source sur le flux ENT a Own.PCON
													'Eliminate the Source account on the ENT flow at Own.PCON
													Cons.Book(cell,acctMember.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10041)
													
							                        'Genere le compte 106G sur le flux ENT a Own.POWN
													'Trigger the account 106G on the flow ENT at Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10042)
													
							                        'Genere le compte 106M sur le flux ENT a Own.PMIN
													'Trigger the account 106M on the flow ENT at Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10043)
												
												'Si l'entite est consolidee en N-1
												'If the entity is consolidated in N-1	
												Else
													'Si la methode de consolidation n'est pas Mise en Equivalence
													'If the consolidation method is Not Equity Pickup
													'If method<>3 Then
													If method<>FSKMethod.MEE Then
														'Elimine le compte Source et Flux Source a Own.PCON de N-1
														'Eliminate the source account on the source flow at Own.PCON of N-1
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon_1,,RegleElim,10044)
														
														'Genere sur le compte source le flux VARC a la variation de Own.PCON
														'Trigger the source account on the VARC flow at change in Own.PCON
														Cons.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.VpCon,,RegleElim,10045)
														
														'Genere le compte 106G sur le flux source a Own.POWN de N-1
														'Trigger the account 106G on the source flow at Own.POWN N-1
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn_1 ,,RegleElim,10046)
														
														'Genere le compte 106G sur le flux VARC a la variation de Own.POWN
														'Trigger the account 106G on the VARC flow at change in Own.POWN
														Cons.Book(cell,acct.RsvG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpOwn ,,RegleElim,10047)
														
														'Genere le compte 106M sur le flux source a Own.PMIN de N-1
														'Trigger the account 106M on the source flow at Own.PMIN N-1
														Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin_1 ,,RegleElim,10048)
														
														'Genere le compte 106M sur le flux VARC a la variation de Own.PMIN
														'Trigger the account 106G on the VARC flow at change in Own.PMIN
														Cons.Book(cell,acct.RsvM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpMin ,,RegleElim,10049)
													'Si la methode de consolidation est Mise en Equivalence
													'If consolidation method is Equity Pickup	
													Else
														'Elimine le compte Source sur le flux Source (a 100%)
														'Eliminate the source account on the source flow (at 100%)													
														'fsm no lo eliminamos porque lo hemos desactivado de Shared
														'Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10051)
													
														'genere le compte 106G sur le flux source (a 100%)
														'Trigger the account 106G on the source flow (at 100%)
														'Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10053)
														Cons.Book(cell,acct.RsvEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10053)
														'fsm hacemos la contrapartida con la cuenta 1BEQ
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,RegleElim,10059)
													End If	
												End If
											'Autres flux
											'Other Flows
											Else
												'Si la methode n'est pas Mise en Equivalence
												'If consolidation method is Not Equity Pickup
												'If method<>3 Then
												If method<>FSKMethod.MEE Then
													'Elimine le compte Source sur le flux de Destination a Own.PCON
													'Eliminate the source account on the Destination flow at Own.PCON
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10055)

													'Genere le compte 106G sur le flux de destination a Own.POWN
													'Trigger account 106G on Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10056)
													
													'Genere le compte 106M sur le flux de destination a Own.PMIN
													'Trigger account 106M on Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10057)
												'Si la methode est Mise en Equivalence
												'If consolidation method is Equity Pickup	
												Else
													'Elimine le compte Source sur le flux de Destination (a 100%)
													'Eliminate the source account on the Destination flow (at 100%)
													'fsm no lo eliminamos porque lo hemos desactivado de Shared
													'Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.Pcon,,RegleElim,10058)

													'Genere le compte 106G sur le flux de Destination (a 100%)
													'Trigger account 106G on the Destination flow (at 100%)
													'Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,RegleElim,10059)
													Cons.Book(cell,acct.RsvEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,RegleElim,10059)
													'fsm hacemos la contrapartida con la cuenta 1BEQ
													Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,RegleElim,10059)
												End If	
											End If
			                            End If
									'----------------------------------------------------
					                ' Compte 106C
									' Account 106C
					                '----------------------------------------------------	
									ElseIf RegleElim="CAPIC" Then								

										'fs disabled to use the UD4 member defined in text3 for account
										'If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource										
										
										'Si le flux source est AFF
										'If source flow is AFF
										If flowMember.MemberId = flow.Aff.MemberId Then
											'Si la methode n'est pas Mise en Equivalence
											'If consolidation method is Not Equity Pickup
											'If method<>3 Then
											If method<>FSKMethod.MEE Then
												'Elimine le compte source sur le flux Source a Own.PCON de N-1
												'Eliminate the source account on the source flow at Own.PCON N-1
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon_1,,RegleElim,10060)
												
												'Genere le compte source sur le flux VARC a la variation de Own.PCON
												'Trigger the source account on the VARC flow at change in Own.PCON
												Cons.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.VpCon,,RegleElim,10061)
												
												'Genere le compte 106CG sur le flux Source a Own.POWN de N-1
												'Trigger the account 106CG on the source flow at Own.POWN N-1
												Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn_1 ,,RegleElim,10062)
												
												'Genere le compte 106CG sur le flux VARC a la variation de Own.POWN
												'Trigger the account 106CG on the VARC flow at change in Own.POWN
												Cons.Book(cell,acct.RsvCG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpOwn ,,RegleElim,10063)
												
												'Genere le compte 106CM sur le flux Source a Own.PMIN de N-1
												'Trigger the account 106CM on the source flow at Own.PMIN N-1
												Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin_1 ,,RegleElim,10064)
												
												'Genere le compte 106CM sur le flux VARC a la variation de Own.PMIN
												'Trigger the account 106CM on the VARC flow at change in Own.PMIN
												Cons.Book(cell,acct.RsvCM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpMin ,,RegleElim,10065)
											'Si la methode est Mise en Equivalence
											'If consolidation method is Equity Pickup
											Else
												'Elimine le compte Source sur le flux Source (a 100%)
												'Eliminate the source account on the source flow (at 100%)
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.Pcon,,RegleElim,10066)
												
												'Genere le compte 106CG sur le flux Source (a 100%)
												'Trigger the account 106CG on the source flow (at 100%)
												Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,RegleElim,10068)
												
											End If	
										'Autre flux
										'Other flows
										Else
											'Si la methode n'est pas Mise en Equivalence
											'If consolidation is Not Equity Pickup
											'If method<>3 Then
											If method<>FSKMethod.MEE Then
												'Elimine le compte Source sur le flux de Destination a Own.PCON
												'Eliminate the source account on the Destination flow at Own.PCON
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10070)

												'Genere le compte 106CG sur le flux de Destination a Own.POWN
												'Trigger the account 106CG on the Destination flow at Own.POWN
												Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10071)
												
												'Genere le compte 106CM sur le flux de Destination a Own.PMIN
												'Trigger the account 106CM on the Destination flow at Own.PMIN
												Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10072)
											'Si la methode est Mise en Equivalence
											'If consolidation method is Equity Pickup	
											Else
												'Elimine le compte Source sur le flux de Destination (a 100%)
												'Eliminate the Source account on the Destination flow (at 100%)
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.Pcon,,RegleElim,10073)

												'Genere le compte 106CG sur le flux de Destination (a 100%)
												'Trigger the account 106CG on the Destination flow (at 100%)
												Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,RegleElim,10074)
											End If	
										End If

									'----------------------------------------------------
									' Cuentas de Subvenciones y cambios de Valor - fs
					                '----------------------------------------------------	
										
									ElseIf RegleElim="RISI" Then
										'Si el método de consolidación es distinto de HOLDING
										'If consolidation method is not Holding
										If method<>FSKMethod.HOLDING Then
											'fs disabled to use the UD4 member defined in text3 for account
											'If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
											
											'Si le flux est AFF or DIV
											'If flow is AFF or DIV

											If flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
'											'Autres flux
											'Other Flows
												'Si la methode n'est pas Mise en Equivalence
												'If consolidation method is Not Equity Pickup
												'If method<>3 Then
												If method<>FSKMethod.MEE Then
													'Elimina la cuenta de origen por el % de minoritarios
													'Eliminate the source account on the Destination flow at Own.PMIN 
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10055)
													
													'Genera la cuenta de intereses minoritarios al % Own.PMIN
													'Trigger account 2ATR on Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10057)
												End If	

											ElseIf flowMember.MemberId <> flow.Clo.MemberId Then
'											'Autres flux
											'Other Flows
												'Si la methode n'est pas Mise en Equivalence
												'If consolidation method is Not Equity Pickup
												'If method<>3 Then
												If method<>FSKMethod.MEE Then
													'Elimina la cuenta de origen por el % de minoritarios
													'Eliminate the source account on the Destination flow at Own.PMIN 
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10055)
													
													'Genera la cuenta de intereses minoritarios al % Own.PMIN
													'Trigger account 2ATR on Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10057)
												Else
													'Elimina la cuenta de origen por el PCON
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,RegleElim,10059)
													'fsm hacemos la contrapartida con la cuenta 1BEQ
													Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.Pcon ,,RegleElim,10059)
												End If	
											End If
			                            End If										
										
									'----------------------------------------------------
					                ' Compte 120000: Resultat de l'exercice
									' Account 120000: Current Year Earnings
					                '----------------------------------------------------	
									ElseIf RegleElim="RESU" Then
										'fs disabled to use the UD4 member defined in text3 for account
										'If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
										
										'Si le flux est AFF	
										'If flow is AFF
										If flowMember.MemberId = flow.Aff.MemberId Then
											'Si le methode n'est pas Mise en Equivalence
											'If consolidation method is Not Equity Pickup
											'If method<>3 Then
											
'											If method<>FSKMethod.MEE Then
'												'Elimine le compte Source sur le flux Source a Own.PCON N-1
'												'Eliminate the source account on the source flow at Own.PCON N-1
'												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon_1,,RegleElim,10075)
												
'												'Elimine le compte Source sur le flux VARC a la variation de Own.PCON
'												'Eliminate the source account on the VARC flow at change in Own.PCON
'												Cons.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.VpCon,,RegleElim,10076)
												
'												'Genere le compte 120G sur le flux Source a Own.POWN N-1
'												'Trigger account 120G on the source flow at Own.POWN N-1
'												Cons.Book(cell,acct.ResG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn_1 ,,RegleElim,10077)
												
'												'Genere le compte 120G sur le flux VARC a la variation de Own.POWN
'												'Trigger account 120G on the VARC flow at change in Own.POWN
'												Cons.Book(cell,acct.ResG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpOwn ,,RegleElim,10078)
												
'												'Genere le compte 120M sur le flux Source a Own.PMIN N-1
'												'Trigger account 120M on the source flow at Own.PMIN N-1
'												Cons.Book(cell,acct.ResM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin_1 ,,RegleElim,10079)
												
'												'Genere le compte 120M sur le flux VARC a la variation de Own.PMIN
'												'Trigger account 120M on the VARC flow at change in Own.PMIN
'												Cons.Book(cell,acct.ResM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpMin ,,RegleElim,10080)
'											'Si la methode est Mise en Equivalence
'											'If consolidation method is Equity Pickup
'											Else
'												'Elimine le compte Source sur le flux Source (a 100%)
'												'Eliminate the source account on the source flow (at 100%)
'												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.PCon,,RegleElim,10081)

'												'Genere le compte 120G sur le flux Source (a 100%)
'												'Trigger account 120G on the source flow (at 100%)
''												Cons.Book(cell,acct.ResG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,RegleElim,10083) 'fs Account 2AGUE para P.Equiv
'												Cons.Book(cell,acct.ResEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,RegleElim,10083)

'											End If
											
											'fs including specific calc for Holding method
											'Si la methode est Mise en Equivalence
											'If consolidation method is Equity Pickup
											If method=FSKMethod.MEE Then
												'Elimine le compte Source sur le flux Source (a 100%)
												'Eliminate the source account on the source flow (at 100%)
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.PCon,,RegleElim,10081)

												'Genere le compte 120G sur le flux Source (a 100%)
												'Trigger account 120G on the source flow (at 100%)
'												Cons.Book(cell,acct.ResG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,RegleElim,10083) 'fs Account 2AGUE para P.Equiv
												Cons.Book(cell,acct.ResEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,RegleElim,10083)
		
											ElseIf method=FSKMethod.Holding Then
												'Elimine le compte Source sur le flux Source a Own.PCON N-1
												'Eliminate the source account on the source flow at Own.PCON N-1
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon_1,,RegleElim,10075)
												
												'Elimine le compte Source sur le flux VARC a la variation de Own.PCON
												'Eliminate the source account on the VARC flow at change in Own.PCON
												Cons.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.VpCon,,RegleElim,10076)
												
												'Genere le compte 120G sur le flux Source a Own.POWN N-1
												'Trigger account 120G on the source flow at Own.POWN N-1
												Cons.Book(cell,acct.ResH.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn_1 ,,RegleElim,10077)
												
												'Genere le compte 120G sur le flux VARC a la variation de Own.POWN
												'Trigger account 120G on the VARC flow at change in Own.POWN
												Cons.Book(cell,acct.ResH.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpOwn ,,RegleElim,10078)
												
												'Genere le compte 120M sur le flux Source a Own.PMIN N-1
												'Trigger account 120M on the source flow at Own.PMIN N-1
												Cons.Book(cell,acct.ResM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin_1 ,,RegleElim,10079)
												
												'Genere le compte 120M sur le flux VARC a la variation de Own.PMIN
												'Trigger account 120M on the VARC flow at change in Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpMin ,,RegleElim,10080)
												
											Else	
												'Elimine le compte Source sur le flux Source a Own.PCON N-1
												'Eliminate the source account on the source flow at Own.PCON N-1
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon_1,,RegleElim,10075)
												
												'Elimine le compte Source sur le flux VARC a la variation de Own.PCON
												'Eliminate the source account on the VARC flow at change in Own.PCON
												Cons.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.VpCon,,RegleElim,10076)
												
												'Genere le compte 120G sur le flux Source a Own.POWN N-1
												'Trigger account 120G on the source flow at Own.POWN N-1
												Cons.Book(cell,acct.ResG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn_1 ,,RegleElim,10077)
												
												'Genere le compte 120G sur le flux VARC a la variation de Own.POWN
												'Trigger account 120G on the VARC flow at change in Own.POWN
												Cons.Book(cell,acct.ResG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpOwn ,,RegleElim,10078)
												
												'Genere le compte 120M sur le flux Source a Own.PMIN N-1
												'Trigger account 120M on the source flow at Own.PMIN N-1
												Cons.Book(cell,acct.ResM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin_1 ,,RegleElim,10079)
												
												'Genere le compte 120M sur le flux VARC a la variation de Own.PMIN
												'Trigger account 120M on the VARC flow at change in Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpMin ,,RegleElim,10080)
											End If
								
											
											
										'Pour les autres flux
										'Other Flows	
										Else
											'Si la methode n'est pas Mise en Equivalence
											'If consolidation method is NOT Equity Pikcup
											'If method<>3 Then
'											If method<>FSKMethod.MEE Then
'												'Elimine le compte Source sur le flux Destination a Own.PCON
'												'Eliminate the source account on the Destination flow at Own.PCON
'												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10085)

'												'Genere le compte 120G sur le flux Destination a Own.POWN
'												'Trigger account 120G on the Destination flow at Own.POWN
'												Cons.Book(cell,acct.ResG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10086)
												
'												'Genere le compte 120M sur le flux Destination a Own.PMIN
'												'Trigger account 120M on the Destination flow at Own.PMIN
'												Cons.Book(cell,acct.ResM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10087)
'											'Si la methode est Mise en Equivalence
'											'If consolidation method is Equity Pickup	
'											Else
'												'Elimine le compte Source sur le flux Destination (a 100%)
'												'Eliminate the source account on the Destination flow (at 100%)
'												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.PCon,,RegleElim,10088)

'												'Genere le compte 120G sur le flux Destination (a 100%)
'												'Trigger account 120G on the Destination flow (at 100%)
''												Cons.Book(cell,acct.ResG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,RegleElim,10089)
'												Cons.Book(cell,acct.ResEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,RegleElim,10089)
'											End If	

											'Si la methode est Mise en Equivalence
											'If consolidation method is Equity Pickup	
											If method=FSKMethod.MEE Then
												'Elimine le compte Source sur le flux Destination (a 100%)
												'Eliminate the source account on the Destination flow (at 100%)
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.PCon,,RegleElim,10088)

												'Genere le compte 120G sur le flux Destination (a 100%)
												'Trigger account 120G on the Destination flow (at 100%)
'												Cons.Book(cell,acct.ResG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,RegleElim,10089)
												Cons.Book(cell,acct.ResEq.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,RegleElim,10089)

											ElseIf method=FSKMethod.Holding Then
												
												'Elimine le compte Source sur le flux Destination a Own.PCON
												'Eliminate the source account on the Destination flow at Own.PCON
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10085)

												'Genere le compte 120G sur le flux Destination a Own.POWN
												'Trigger account 120G on the Destination flow at Own.POWN
												Cons.Book(cell,acct.ResH.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10086)
												
												'Genere le compte 120M sur le flux Destination a Own.PMIN
												'Trigger account 120M on the Destination flow at Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10087)
											Else
												'Elimine le compte Source sur le flux Destination a Own.PCON
												'Eliminate the source account on the Destination flow at Own.PCON
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10085)

												'Genere le compte 120G sur le flux Destination a Own.POWN
												'Trigger account 120G on the Destination flow at Own.POWN
												Cons.Book(cell,acct.ResG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10086)
												
												'Genere le compte 120M sur le flux Destination a Own.PMIN
												'Trigger account 120M on the Destination flow at Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10087)
											End If	
											
											
											
										End If
									'----------------------------------------------------
					                ' Compte 120C
									' Account 120C
					                '----------------------------------------------------	
									ElseIf RegleElim="RESUC" Then										
										'fs disabled to use the UD4 member defined in text3 for account
										'If NatureSource.MemberId <> Dimconstants.None Then NatureDest = NatureSource
										
										'Si le flux est AFF
										'If low is AFF	
										If flowMember.MemberId = flow.Aff.MemberId Then
											'Si la methode n'est pas Mise en Equivalence
											'If consolidation method is NOT Equity Pickup
											'If method<>3 Then
											If method<>FSKMethod.MEE Then
												'Elimine le compte Source sur le flux Source a Own.PCON N-1
												'Eliminate the source account on the source flow at Own.PCON N-1
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon_1,,RegleElim,10090)
												
												'Elimine le compte Source sur le flux VARC a la variation de Own.PCON
												'Eliminate the source account on the VARC flow at change in Own.PCON
												Cons.Book(cell,acctMember.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.VpCon,,RegleElim,10091)
												
												'Genere le compte 120CG sur le flux Source a Own.POWN N-1
												'Trigger account 120CG on the source flow at Own.POWN N-1
												Cons.Book(cell,acct.ResCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn_1 ,,RegleElim,10092)
												
												'Genere le compte 120CG sur le flux VARC a la variation de Own.POWN
												'Trigger account 120CG on the VARC flow at change in Own.POWN
												Cons.Book(cell,acct.ResCG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpOwn ,,RegleElim,10093)
												
												'Genere le compte 120CM sur le flux Source a Own.PMIN N-1
												'Trigger account 120CM on the source flow at Own.PMIN N-1
												Cons.Book(cell,acct.ResCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin_1 ,,RegleElim,10094)
												
												'Genere le compte 120CM sur le flux VARC a la variation de Own.PMIN
												'Trigger account 120CM on the VARC flow at change in Own.PMIN
												Cons.Book(cell,acct.ResCM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.VpMin ,,RegleElim,10095)
											'Si la methode est Mise en Equivalence
											'If consolidation method is Equity Pickup
											Else
												'Elimine le compte Source sur le flux Source (a 100%)
												'Eliminate the source account on the source flow (at 100%)
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.PCon,,RegleElim,10096)

												'Genere le compte 120CG sur le flux Source (a 100%)
												'Trigger account 120CG on the source flow (at 100%)
												Cons.Book(cell,acct.ResCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,RegleElim,10098)

											End If
										'Autres flux
										'Other Flows	
										Else
											'Si la methode n'est pas Mise en Equivalence
											'If consolidation method is NOT Equity Pickup
											'If method<>3 Then
											If method<>FSKMethod.MEE Then
												'Elimine le compte Source sur le flux Destination a Own.PCON
												'Eliminate the source account on the Destination flowe at Own.PCON
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10100)

												'Genere le compte 120CG sur le flux Destination a Own.POWN
												'Trigger account 120CG on Destination flow at Own.POWN
												Cons.Book(cell,acct.ResCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10101)
												
												'Genere le compte 120CM sur le flux Destination a Own.PMIN
												'Trigger account 120CM on Destination flow at Own.PMIN
												Cons.Book(cell,acct.ResCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10102)
											'Si la methode est Mise en Equivalence
											'If consolidation method is Equity Pickup	
											Else
												'Elimine le compte Source sur le flux Destination (a 100%)
												'Eliminate source account on Destination flow (at 100%)
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.PCon,,RegleElim,10103)

												'Genere le compte 120CG sur le flux Destination (a 100%)
												'Trigger account 120CG on Destination flow (at 100%)
												Cons.Book(cell,acct.ResCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon ,,RegleElim,10104)
											End If	
										End If	
				                    '----------------------------------------------------
				                    ' Compte EA: compte technique des Ecarts d'acquisition
									' Account EA: technical account for Goodwill
				                    '----------------------------------------------------
									ElseIf RegleElim="EA" Then
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											Dim acctGW As Member = api.Account.GetPlugAccount(cell.DataBufferCellPk.AccountId)
											
											'Si la methode est Mise en Equivalence ou la nature source est Equity
											'If consolidation method is Equity Pickup or source nature is Equity
											'If method=3 Or NatureSource.Name="Equity" Then acctGW = acct.GWMEE
											If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then acctGW = acct.GWMEE
											
											'Si le flux est AUG
											'If flows is AUG
											If flowMember.MemberId = flow.Aug.MemberId Then
												'Si l'entite n'est pas consolidee en N-1
												'If the entity is not consolidated in N-1
												If Own.pCon_1 = 0 Then
													'Si la nature (NatureDim) n'est pas Minoritaires
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then
														
														'Genere le compte 204000 sur le flux ENT a Own.PCON
														'Trigger account 204000 on flow ENT at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10106)
														
														'Genere le compte 106G sur le flux ENT a Own.POWN
														'Trigger account 106G on flow ENT at Own.POWN
														Cons.Book(cell,acct.RsvG.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10107)
														
		                        						'Genere le compte 106M sur le flux ENT a Own.PMIN
														'Trigger account 106M on flow ENT at Own.PMIN
														Cons.Book(cell,acct.RsvM.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10108)
													'Pour la Nature Minoritaires (NatureDim)
													'If Nature is Minoritaires (NatureDim)	
													Else
													
														'Genere le compte 106G sur le flux ENT (a -100%)
														'Trigger account 106G on flow ENT (at -100%)
														Cons.Book(cell,acct.RsvG.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10110)
														
		                        						'Genere le compte 106M sur le flux ENT (a 100%)
														'Trigger account 106M on flow ENT (at 100%)
														Cons.Book(cell,acct.RsvM.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10111)
													End If
												'Si l'entite est consolidee en N-1
												'If the entity is consolidated in N-1	
												Else
													'Si la nature (NatureDim) n'est pas Minoritaires
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then
														
														'Genere le compte 204000 sur le flux VARC a Own.PCON
														'Trigger account 204000 on flow VARC at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10113)
														
														'Genere le compte 106G sur le flux VARC a Own.POWN
														'Trigger account 106G on flow VARC at Own.POWN
														Cons.Book(cell,acct.RsvG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10114)
														
		                        						'Genere le compte 106M sur le flux VARC a Own.PMIN
														'Trigger account 106M on flow VARC at Own.PMIN
														Cons.Book(cell,acct.RsvM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10115)
													'Pour la Nature Minoritaires (NatureDim)
													'If Nature is Minoritaires (NatureDim)	
													Else
													
		                        						'Genere le compte 106G sur le flux VARC a -100%
														'Trigger account 106G on flow VARC at -100%
														Cons.Book(cell,acct.RsvG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10117)
														
		                        						'Genere le compte 106M sur le flux VARC a 100%
														'Trigger account 106M on flow VARC at 100%
														Cons.Book(cell,acct.RsvM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10118)
													End If	
												End If
											'Si le flux est DIM
											'If flows is DIM
											ElseIf flowMember.MemberId = flow.Dimi.MemberId Then
												'Si l'entite n'est pas consolidee en N-1
												'If the entity is not consolidated in N-1
												If Own.pCon_1 = 0 Then
													'Si la nature (NatureDim) n'est pas Minoritaires
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then
													
														'Genere le compte 204000 sur le flux ENT a Own.PCON
														'Trigger account 204000 on flow ENT at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10120)
														
														'Genere le compte 106G sur le flux ENT a Own.POWN
														'Trigger account 106G on flow ENT at Own.POWN
														Cons.Book(cell,acct.RsvG.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,RegleElim,10121)
														
		                        						'Genere le compte 106M sur le flux ENT a Own.PMIN
														'Trigger account 106M on flow ENT at Own.PMIN
														Cons.Book(cell,acct.RsvM.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin ,,RegleElim,10122)
													'Pour la Nature Minoritaires (NatureDim)
													'If Nature is Minoritaires (NatureDim)	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
														'Eliminate source record for Stage consolidations (at 100%)						
														'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10123)
													
														'Genere le compte 106G sur le flux ENT (a -100%)
														'Trigger account 106G on flow ENT (at -100%)
														Cons.Book(cell,acct.RsvG.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,RegleElim,10124)
														
		                        						'Genere le compte 106M sur le flux ENT (a 100%)
														'Trigger account 106M on flow ENT (at 100%)
														Cons.Book(cell,acct.RsvM.MemberId,flow.Ent.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,RegleElim,10125)
													End If
												'Si l'entite est consolidee en N-1
												'If the entity is consolidated in N-1	
												Else
													'Si la nature (NatureDim) n'est pas Minoritaires
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then
														'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
														'Eliminate source record for Stage consolidations (at 100%)						
														'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10126)
														
														'Genere le compte 204000 sur le flux VARC a Own.PCON
														'Trigger account 204000 on flow VARC at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10127)
														
														'Genere le compte 106G sur le flux VARC a Own.POWN
														'Trigger account 106G on flow VARC at Own.POWN
														Cons.Book(cell,acct.RsvG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,RegleElim,10128)
														
		                        						'Genere le compte 106M sur le flux VARC a Own.PMIN
														'Trigger account 106M on flow VARC at Own.PMIN
														Cons.Book(cell,acct.RsvM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin ,,RegleElim,10129)
													'Pour la Nature Minoritaires (NatureDim)
													'If Nature is Minoritaires (NatureDim)	
													Else
												
		                        						'Genere le compte 106G sur le flux VARC a -100%
														'Trigger account 106G on flow VARC at -100%
														Cons.Book(cell,acct.RsvG.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,RegleElim,10131)
														
		                        						'Genere le compte 106M sur le flux VARC a 100%
														'Trigger account 106M on flow VARC at 100%
														Cons.Book(cell,acct.RsvM.MemberId,flow.VarC.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,RegleElim,10132)
													End If	
												End If
											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF	
											ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
														
													'Genere le compte 204000 sur le flux Destination a Own.PCON
													'Trigger account 204000 on the Destination flow at Own.PCON
													Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10134)
														
													'Genere le compte 106CG sur le flux de Destination a Own.POWN
													'Trigger account 106CG on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10135)
													
	                        						'Genere le compte 106CM sur le flux de Destination a Own.PMIN
													'Trigger account 106CM on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10136)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
												
	                        						'Genere le compte 106CG sur le flux de Destination a -100%
													'Trigger account 106CG on the Destination flow at -100%
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10138)
													
	                        						'Genere le compte 106CG sur le flux de Destination a 100%
													'Trigger account 106CG on the Destination flow at 100%
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10139)
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
													Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10141)
													'Cons.Book(cell,"A#2FCC_CO",flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10141)
														
													'Genere le compte 106G sur le flux Destination a Own.POWN
													'Trigger account 106G on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10142)
													
	                        						'Genere le compte 106G sur le flux Destination a Own.PMIN
													'Trigger account 106G on the Destination flow at Own.PMIN	
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10143)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
													
	                        						'Genere le compte 106G sur le flux Destination (a -100%)
													'Trigger account 106G on the Destination flow (at -100%)
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10145)
													
	                        						'Genere le compte 106M sur le flux Destination (a 100%)
													'Trigger account 106M on the Destination flow (at 100%)
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10146)
												End If	
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the Minority Interest portion on the Minoritaires Nature 
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10147)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10148)
											End If	
			                            End If	'Can Eliminate
									'----------------------------------------------------
					                ' Compte EAEH: Cumul des ecarts de conversion sur Ecarts d'acquisition
									' Account EAEH: CTA on Goodwill
					                '----------------------------------------------------	
									ElseIf RegleElim="EAH" Then
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si le flux est OUV/VARC/SOR
											'If flow is OUV/VARC/SOR
	                						If flowMember.MemberId = flow.Ouv.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.Sor.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
	                    						If NatureSource.Name<>"Minoritaires" Then
	                        						'Genere le compte 106G sur le flux Destination a -Own.POWN
													'Trigger account 106G on the Destination flow at -Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10150)
													
													'Genere le compte 106CG sur le flux Destination a Own.POWN
													'Trigger account 106CG on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10151)
													
													'Genere le compte 106CG sur le flux Destination a -Own.PMIN
													'Trigger account 106CG on the Destination flow at -Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10152)
													
													'Genere le compte 106CM sur le flux Destination a Own.PMIN
													'Trigger account 106CM on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10153)
	                    						'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)
												Else													
													'Genere le compte 106G sur le flux Destination (a 100%)
													'Trigger account 106G on the Destination flow (at 100%)
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10154)
													
													'Genere le compte 106CG sur le flux Destination (a -100%)
													'Trigger account 106CG on the Destination flow (at -100%)
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10155)
													
													'Genere le compte 106M sur le flux Destination (a -100%)
													'Trigger account 106M on the Destination flow (at -100%)
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10156)
													
													'Genere le compte 106CM sur le flux Destination (a 100%)
													'Trigger account 106CM on the Destination flow (at 100%)
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10157)
													
	                    						End If
											End If	'CurCustom4<>Mino
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10158)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1- Own.pCon),,RegleElim,10159)
											End If	
										End If
									'----------------------------------------------------
					                ' Compte AEA: Amortissement des ecarts d'acquisition
									' Account AEA: Goodwill amortization
					                '----------------------------------------------------	
									ElseIf RegleElim="AEA" Then
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											Dim acctGWA As Member = api.Account.GetPlugAccount(cell.DataBufferCellPk.AccountId)
											Dim aDAmEA As Member = api.Members.GetMember(DimType.Account.Id, "680400")
											
											'Si la methode est Mise en Equivalence ou la nature source est Equity
											'If consolidation method is Equity Pickup or source nature is Equity
											'If method=3 Or NatureSource.Name="Equity" Then acctGWA = acct.GWAMEE	
											If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then acctGWA = acct.GWAMEE	
											
											'Si le flux est DEPA
											'If the flow is DEPA	
	                						If flowMember.MemberId = flow.DepA.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
													'Eliminate source record for Stage consolidations at Own.PCON						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-Own.pCon,,RegleElim,10160)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														'Genere le compte 2904EQUI sur le flux RES a Own.PCON
														'Trigger account 2904EQUI on the RES flow at Own.PCON
														Cons.Book(cell,acctGWA.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10161)
														
														'Genere le compte 880EQUI sur le flux None a -Own.PCON
														'Trigger account 880EQUI on the None flow at -Own.PCON													
														Cons.Book(cell,acct.ResultatMEE.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10162)
													Else	
														'Genere le compte 290400 sur le flux Destination a Own.PCON
														'Trigger account 290400 on the Destination flow at Own.PCON
														Cons.Book(cell,acctGWA.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10163)
														
														'Genere le compte 680400 sur le flux None a Own.PCON
														'Trigger account 680400 on the None flow at Own.PCON													
														Cons.Book(cell,aDAmEA.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10164)
													End If
													
													'Genere le compte 120G sur le flux RES a -Own.POWN
													'Trigger account 120G on the RES flow at -Own.POWN
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10165)
													
													'Genere le compte 120M sur le flux RES a -Own.PMIN
													'Trigger account 120M on the RES flow at-Own.PMIN
													Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10166)
		                    					'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
																								
													'Genere le compte 120G sur le flux Destination a 100%
													'Trigger account 120G on the Destination flow at 100%
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10168)
													
		                    						'Genere le compte 120M sur le flux Destination a -100%
													'Trigger account 120M on the Destination flow at -100%
													Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10169)
												End If
											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF	
	                						ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
													'Eliminate source record for Stage consolidations at Own.PCON 						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-Own.pCon,,RegleElim,10170)
														
													'Genere le compte 2904000/2904EQUI sur le flux Destination a Own.PCON
													'Trigger account 2904000/2904EQUI on the Destination flow at Own.PCON
													Cons.Book(cell,acctGWA.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10171)
												End If	
											Else
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
													'Eliminate source record for Stage consolidations at Own.PCON						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-Own.pCon,,RegleElim,10172)

													If method=FSKMethod.HOLDING Then													
														Cons.Book(cell,acct.RsvH.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10173)
													Else	
														'Genere le compte 106G sur le flux Destination a Own.POWN
														'Trigger account 106G on the Destination flow at Own.POWN
														'Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10173)
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10173)
														
														'Genere le compte 106M sur le flux Destination a Own.PMIN
														'Trigger account 106M on the Destination flow at Own.PMIN
														'Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10174)
														Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,RegleElim,10174)
													End If
													
	                        						'Genere le compte 2904000/2904EQUI sur le flux Destination a Own.PCON
													'Trigger account 2904000/2904EQUI on the Destination flow at Own.PCON
													Cons.Book(cell,acctGWA.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10175)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)
												Else
														
													'Genere le compte 106G sur le flux Destination a 100%
													'Trigger account 106G on the Destination flow at 100%
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10177)
													
		                    						'Genere le compte 106M sur le flux Destination a Own.PMIN
													'Trigger account 106M on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10178)
												End If	
	                						End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10179)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1-Own.pCon),,RegleElim,10180)
											End If	
										End If	'Can Eliminate
									'----------------------------------------------------
					                ' Compte AEAEH: Cumul des ecarts de conversion sur amortissement des ecarts d'acquisition
									' Account AEAEH: CTA on Goodwill amortization
					                '----------------------------------------------------	
									ElseIf RegleElim="AEAH" Then
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
											'Eliminate source record for Stage consolidations (at 100%)						
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10181)
											
											'Si le flux est OUV/VARC/SOR
											'If flow is OUV/VARC/SOR
											If flowMember.MemberId = flow.Ouv.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.Sor.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
	                    						If NatureSource.Name<>"Minoritaires" Then
	                        						'Genere le compte 106G sur le flux Destination a Own.POWN
													'Trigger account 106G on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10182)
													
													'Genere le compte 106CG sur le flux Destination a -Own.POWN
													'Trigger account 106CG on the Destination flow at -Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10183)
													
													'Genere le compte 106M sur le flux Destination a Own.PMIN
													'Trigger account 106M on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10184)
													
													'Genere le compte 106CM sur le flux Destination a -Own.PMIN
													'Trigger account 106CM on the Destination flow at -Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10185)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)
	                    						Else
													'Genere le compte 106G sur le flux Destination a -100%
													'Trigger account 106G on the Destination flow at -100%
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10186)
													
													'Genere le compte 106CG sur le flux Destination a 100%
													'Trigger account 106CG on the Destination flow at 100%
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10187)
													
													'Genere le compte 106M sur le flux Destination a 100%
													'Trigger account 106M on the Destination flow at 100%
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10188)
													
													'Genere le compte 106CG sur le flux Destination a -100%
													'Trigger account 106CG on the Destination flow at -100%
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10189)
													
	                    						End If 'CurCustom4<>Mino
											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF	
											ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													
								                    'Genere le compte 106CG sur le flux Destination a -Own.POWN
													'Trigger account 106CG on the Destination flow at -Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10190)
													
								                    'Genere le compte 106CM sur le flux Destination a -Own.PMIN
													'Trigger account 106CM on the Destination flow at -Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10191)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
								                    'Genere le compte 106CG sur le flux Destination a 100%
													'Trigger account 106CG on the Destination flow at 100%
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10192)
													
								                    'Genere le compte 106CG sur le flux Destination a -100%
													'Trigger account 106CG on the Destination flow at -100%
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10193)
													
												End If	'CurCustom4<>Mino
											
											'Si le flux est ECR
											'If the flow is ECR	
											ElseIf flowMember.MemberId = flow.Ecr.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
								                    'Genere le compte 120CG sur le flux ECF a -Own.POWN
													'Trigger account 120CG on the ECF flow at -Own.POWN
													Cons.Book(cell,acct.ResCG.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10194)
													
								                    'Genere le compte 120CM sur le flux ECF a -Own.PMIN
													'Trigger account 120CM on the ECF flow at -Own.PMIN
													Cons.Book(cell,acct.ResCM.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10195)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else																			
								                    'Genere le compte 120CG sur le flux ECF a 100%
													'Trigger account 120CG on the ECF flow at 100%
													Cons.Book(cell,acct.ResCG.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10196)
													
								                    'Genere le compte 120CM sur le flux ECF a -100%
													'Trigger account 120CM on the ECF flow at -100%
													Cons.Book(cell,acct.ResCM.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10197)
													
												End If	'CurCustom4<>Mino
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10198)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1-Own.pCon),,RegleElim,10199)
											End If	
										End If
									'----------------------------------------------------
					                ' Comptes de provisions intragroupe
									' Intercompany allowances accounts
					                '----------------------------------------------------	
									ElseIf RegleElim="PROV" Then
										'brapi.errorlog.LogMessage(si, "If method - " & acctMember.MemberId)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si le flux est DEXP/DFIN
											'If the flow is DEXP/DFIN
											If flowMember.MemberId = flow.Dexp.MemberId Or flowMember.MemberId = flow.Dfin.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Genere le compte 120G sur le flux RES a Own.POWN
													'Trigger account 120G on the RES flow at Own.POWN
													If method=FSKMethod.Holding Then
														Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10200)
		                    						Else
														Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10200)
													End If	
														'Genere le compte 120M sur le flux RES a Own.PMIN
													'Trigger account 120M on the RES flow at Own.PMIN
													Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10201)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
														'Eliminate source record for Stage consolidations (at 100%)						
														'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10202)
													
														'Genere le compte 261EQUI sur le flux RES a Own.PCON
														'Trigger account 261EQUI on the RES flow at Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10203)
													
														'Genere le compte 880EQUI sur le flux None a Own.PCON
														'Trigger account 880EQUI on the None flow at Own.PCON
														Cons.Book(cell,acct.ResultatMEE.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10204)
													
													'Si la methode n'est pas Mise en Equivalence ou la nature source n'est pas Equity
													'If consolidation method is not Equity Pickup or source nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10205)
												
														'Si le flux est DEXP
														'If flow is DEXP
'														If flowMember.MemberId = flow.Dexp.MemberId Then
'															'Si la nature de destination est PTRA
'															'If Destination nature is PTRA
'															If Nature="PTRA" Then
'																'Genere le compte 681720 sur le flux None a -Own.PCON
'																'Trigger account 681720 on the None flow at -Own.PCON
'																Dim aODotProv1 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aODotProv1.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10206)
															
'															'Si la nature de destination est POTR
'															'If Destination nature is POTR	
'															ElseIf Nature="POTR" Then
'								                            	'Genere le compte 681730 sur le flux None a -Own.PCON
'																'Trigger account 681730 on the None flow at -Own.PCON
'																Dim aODotProv2 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aODotProv2.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10207)
															
'															'Si la nature de destination est PFIA
'															'If Destination nature is PFIA
'															ElseIf Nature="PFIA" Then
'								                            	'Genere le compte 681720 sur le flux None a -Own.PCON
'																'Trigger account 681720 on the None flow at -Own.PCON
'																Dim aODotProv3 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aODotProv3.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10208)
																
'															'Si la nature de destination est PARC
'															'If Destination nature is PARC	
'															ElseIf Nature="PARC" Then
'								                            	'Genere le compte 681510 sur le flux None a -Own.PCON
'																'Trigger account 681510 on the None flow at -Own.PCON
'																Dim aODotProv4 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aODotProv4.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10209)
															
'															'Si la nature de destination est PSRI
'															'If Destination nature is PSRI	
'															ElseIf Nature="PSRI" Then
'								                            	'Genere le compte 681510 sur le flux None a -Own.PCON
'																'Trigger account 681510 on the None flow at -Own.PCON
'																Dim aODotProv5 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aODotProv5.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10210)
'															End If
															
														'Si le flux est DFIN
														'If flow is DFIN
														If flowMember.MemberId = flow.Dfin.MemberId Then
															'Si la nature de destination est PFIA
															'If Destination nature is PFIA
'															If Nature="PFIA" Then
'								                            	'Genere le compte 686620 sur le flux None a -Own.PCON
'																'Trigger account 686620 on the None flow at -Own.PCON
'																Dim aFDotProv3 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aFDotProv3.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10211)
																
'															'Si la nature de destination est PARC
'															'If Destination nature is PARC	
'															ElseIf Nature="PARC" Then
'								                            	'Genere le compte 686510 sur le flux None a -Own.PCON
'																'Trigger account 686510 on the None flow at -Own.PCON
'																Dim aFDotProv4 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aFDotProv4.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10212)
															
'															'Si la nature de destination est PSRI
'															'If Destination nature is PSRI	
'															ElseIf Nature="PSRI" Then
'								                            	'Genere le compte 686520 sur le flux None a -Own.PCON
'																'Trigger account 686520 on the None flow at -Own.PCON
'																Dim aFDotProv5 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aFDotProv5.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10213)
															
'															'Si la nature de destination est PINV
'															'If Destination nature is PINV	
'															ElseIf Nature="PINV" Then
'								                            	'Genere le compte 686621 sur le flux None a -Own.PCON
'																'Trigger account 686621 on the None flow at -Own.PCON
'																Dim aFDotProv6 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aFDotProv6.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10214)
															
															'fs Ingeteam	
															'Si la naturaleza es FOND
															'If Destination nature is FOND	
															If Nature="FOND" Then
								                            	'Generar la cuenta 696000 con el flujo None a -Own.PCON
																'Trigger account 696000 on the None flow at -Own.PCON
																Dim aFDotProv7 As Member = api.Members.GetMember(DimType.Account.Id, "696000") 
																Cons.Book(cell,aFDotProv7.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10214)
															End If
														End If	
													End If
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else	
														
													'Genere le compte 120G sur le flux RES a -100%
													'Trigger account 120G on the RES flow at -100%
													If method=FSKMethod.Holding Then
														Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10216)
													Else
														Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10216)														
													End If	
		                    						'Genere le compte 120M sur le flux RES a Own.PMIN
													'Trigger account 120M on the RES flow at Own.PMIN
													Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10217)
													
												End If
											'Si le flux est REXP ou RFIN
											'If flows is REXP or RFIN	
											ElseIf flowMember.MemberId = flow.Rexp.MemberId Or flowMember.MemberId = flow.Rfin.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Genere le compte 120G sur le flux RES a Own.POWN
													'Trigger account 120G on the RES flow at Own.POWN
													If method=FSKMethod.Holding Then
														Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,RegleElim,10218)
													Else
														Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,RegleElim,10218)														
													End If	
		                    						'Genere le compte 120M sur le flux RES a Own.PMIN
													'Trigger account 120M on the RES flow at Own.PMIN
													Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin ,,RegleElim,10219)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
													
														'Genere le compte 261EQUI sur le flux RES a Own.PCON
														'Trigger account 261EQUI on the RES flow at Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10221)
													
														'Genere le compte 880EQUI sur le flux None a Own.PCON
														'Trigger account 880EQUI on the None flow at Own.PCON
														Cons.Book(cell,acct.ResultatMEE.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10222)
													
													'Si la methode n'est pas Mise en Equivalence ou la nature source n'est pas Equity
													'If consolidation method is not Equity Pickup or source nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10223)
												
														'Si le flux est DEXP
														'If flow is DEXP
'														If flowMember.MemberId = flow.Dexp.MemberId Then
'															'Si la nature de destination est PTRA
'															'If Destination nature is PTRA
'															If Nature="PTRA" Then
'																'Genere le compte 681720 sur le flux None a -Own.PCON
'																'Trigger account 681720 on the None flow at -Own.PCON
'																Dim aODotProv1 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aODotProv1.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10224)
															
'															'Si la nature de destination est POTR
'															'If Destination nature is POTR	
'															ElseIf Nature="POTR" Then
'								                            	'Genere le compte 681730 sur le flux None a -Own.PCON
'																'Trigger account 681730 on the None flow at -Own.PCON
'																Dim aODotProv2 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aODotProv2.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10225)
															
'															'Si la nature de destination est PFIA
'															'If Destination nature is PFIA
'															ElseIf Nature="PFIA" Then
'								                            	'Genere le compte 681720 sur le flux None a -Own.PCON
'																'Trigger account 681720 on the None flow at -Own.PCON
'																Dim aODotProv3 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aODotProv3.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10226)
																
'															'Si la nature de destination est PARC
'															'If Destination nature is PARC	
'															ElseIf Nature="PARC" Then
'								                            	'Genere le compte 681510 sur le flux None a -Own.PCON
'																'Trigger account 681510 on the None flow at -Own.PCON
'																Dim aODotProv4 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aODotProv4.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10227)
															
'															'Si la nature de destination est PSRI
'															'If Destination nature is PSRI	
'															ElseIf Nature="PSRI" Then
'								                            	'Genere le compte 681510 sur le flux None a -Own.PCON
'																'Trigger account 681510 on the None flow at -Own.PCON
'																Dim aODotProv5 As Member = api.Members.GetMember(DimType.Account.Id, "696100") 
'																Cons.Book(cell,aODotProv5.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10228)
'															End If
															
														'Si le flux est DFIN
														'If flow is DFIN
														'If flowMember.MemberId = flow.Rexp.MemberId Then
'															'Si la nature de destination est PTRA
'															'If Destination nature is PTRA
'															If Nature="PTRA" Then
'																'Genere le compte 781720 sur le flux None a Own.PCON
'																'Trigger account 781720 on the None flow at Own.PCON
'																Dim aORepProv1 As Member = api.Members.GetMember(DimType.Account.Id, "796100") 
'																Cons.Book(cell,aORepProv1.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10229)
															
'															'Si la nature de destination est POTR
'															'If Destination nature is POTR	
'															ElseIf Nature="POTR" Then
'								                            	'Genere le compte 781730 sur le flux None a Own.PCON
'																'Trigger account 781730 on the None flow at Own.PCON
'																Dim aORepProv2 As Member = api.Members.GetMember(DimType.Account.Id, "796100") 
'																Cons.Book(cell,aORepProv2.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10230)
															
'															'Si la nature de destination est PFIA
'															'If Destination nature is PFIA	
'															ElseIf Nature="PFIA" Then
'								                            	'Genere le compte 781720 sur le flux None a Own.PCON
'																'Trigger account 781720 on the None flow at Own.PCON
'																Dim aORepProv3 As Member = api.Members.GetMember(DimType.Account.Id, "796100") 
'																Cons.Book(cell,aORepProv3.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10231)
															
'															'Si la nature de destination est PARC
'															'If Destination nature is PARC	
'															ElseIf Nature="PARC" Then
'								                            	'Genere le compte 781510 sur le flux None a Own.PCON
'																'Trigger account 781510 on the None flow at Own.PCON
'																Dim aORepProv4 As Member = api.Members.GetMember(DimType.Account.Id, "796100") 
'																Cons.Book(cell,aORepProv4.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10232)
															
'															'Si la nature de destination est PSRI
'															'If Destination nature is PSRI	
'															ElseIf Nature="PSRI" Then
'								                            	'Genere le compte 781510 sur le flux None a Own.PCON
'																'Trigger account 781510 on the None flow at Own.PCON
'																Dim aORepProv5 As Member = api.Members.GetMember(DimType.Account.Id, "796100") 
'																Cons.Book(cell,aORepProv5.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10233)
'															End If
'														'Si le flux est RFIN
'														'If flow is RFIN	
														If flowMember.MemberId = flow.Rfin.MemberId Then
'															'Si la nature de destination est PFIA
'															'If Destination nature is PFIA
'															If Nature="PFIA" Then
'								                            	'Genere le compte 786620 sur le flux None a Own.PCON
'																'Trigger account 786620 on the None flow at Own.PCON
'																Dim aFRepProv3 As Member = api.Members.GetMember(DimType.Account.Id, "796100") 
'																Cons.Book(cell,aFRepProv3.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10234)
															
'															'Si la nature de destination est PARC
'															'If Destination nature is PARC	
'															ElseIf Nature="PARC" Then
'								                            	'Genere le compte 786510 sur le flux None a Own.PCON
'																'Trigger account 786510 on the None flow at Own.PCON
'																Dim aFRepProv4 As Member = api.Members.GetMember(DimType.Account.Id, "796100") 
'																Cons.Book(cell,aFRepProv4.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10235)
															
'															'Si la nature de destination est PSRI
'															'If Destination nature is PSRI	
'															ElseIf Nature="PSRI" Then
'								                            	'Genere le compte 786520 sur le flux None a Own.PCON
'																'Trigger account 786520 on the None flow at Own.PCON
'																Dim aFRepProv5 As Member = api.Members.GetMember(DimType.Account.Id, "796100") 
'																Cons.Book(cell,aFRepProv5.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10236)
															
'															'Si la nature de destination est PINV
'															'If Destination nature is PINV	
'															ElseIf Nature="PINV" Then
'								                            	'Genere le compte 786621 sur le flux None a Own.PCON
'																'Trigger account 786621 on the None flow at Own.PCON
'																Dim aFRepProv6 As Member = api.Members.GetMember(DimType.Account.Id, "796100") 
'																Cons.Book(cell,aFRepProv6.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10237)
															'fs Ingeteam
															'Si la naturaleza destino es FOND
															'If Destination nature is FOND	
															If Nature="FOND" Then
								                            	'Generar la cuenta 796000 con el flujo None a Own.PCON
																'Trigger account 796000 on the None flow at Own.PCON
																Dim aFRepProv7 As Member = api.Members.GetMember(DimType.Account.Id, "796000") 
																Cons.Book(cell,aFRepProv7.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10237)
																End If														
														End If	
													End If
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else	
														
													'Genere le compte 120G sur le flux RES a -100%
													'Trigger account 120G on the RES flow at -100%
													If method=FSKMethod.Holding Then
														Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,RegleElim,10239)
													Else
														Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,RegleElim,10239)
													End If	
		                    						'Genere le compte 120M sur le flux RES a Own.PMIN
													'Trigger account 120M on the RES flow at Own.PMIN
													Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,RegleElim,10240)
													
												End If

											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF	
											ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
																										
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
														'Genere le compte 261EQUI sur le flux Destination a Own.PCON
														'Trigger account 261EQUI on the Destination flow at Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10242)
													
'xx													ElseIf method=FSKMethod.HOLDING Then
'xx														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10243)
														'fsm 20200901 añadidas las dos lineas siguientes como contrapartida en reservas a la elim del flujo DCM/DCA
'xx														Cons.Book(cell,acct.RsvH.MemberId,flow.MVT.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,RegleElim,10244)
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10243)
														'fsm 20200901 añadidas las dos lineas siguientes como contrapartida en reservas a la elim del flujo DCM/DCA
														Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,RegleElim,10244)
														Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin ,,RegleElim,10245)
													
													End If
												End If	
											Else
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													If method=FSKMethod.HOLDING Then
														Cons.Book(cell,acct.RsvH.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,RegleElim,10244)
													Else
														'Genere le compte 106G sur le flux Destination a Own.POWN
														'Trigger account 106G on the Destination flow at Own.POWN
	'													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10244)
														Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,RegleElim,10244)
														
									                    'Genere le compte 106M sur le flux Destination a Own.PMIN
														'Trigger account 106M on the Destination flow at Own.PMIN
	'													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10245)
														Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin ,,RegleElim,10245)
													End If
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
														'Genere le compte 261EQUI sur le flux Destination a Own.PCON
														'Trigger account 261EQUI on the Destination flow at Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10247)
													
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10248)
													End If	
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)										                    
												Else
													
								                    'Genere le compte 106G sur le flux Destination a -100%
													'Trigger account 106G on the Destination flow at -100%
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10250)
													
								                    'Genere le compte 106M sur le flux Destination a 100%
													'Trigger account 106M on the Destination flow at 100%
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10251)
												End If	'CurCustom4<>Mino
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
						'fsm				Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10252)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10253)
											End If	
										End If	'Can Eliminate
										
									'----------------------------------------------------
					                ' Cuentas de PyG de dotación de provisiones cartera- Rutinas ProvInc y ProvDis
									' PnL accounts for investment impairments
					                '----------------------------------------------------	
										
									ElseIf RegleElim = "PROVDIS" Then

										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si la nature (NatureDim) n'est pas Minoritaires
											'If nature (NatureDim) is not Minoritaires
											If NatureSource.Name<>"Minoritaires" Then
												'Genere le compte 120G sur le flux RES a -Own.POWN
												'Trigger account 120G on the RES flow at -Own.POWN
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10282)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10282)
												End If	
							                    'Genere le compte 120M sur le flux RES a -Own.PMIN
												'Trigger account 120M on the RES flow at -Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10283)
												
												If method=FSKMethod.HOLDING Then													
													Cons.Book(cell,acct.RsvH.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10284)
												Else
													'Genere le compte 106CG sur le flux DIV a -Own.POWN
													'Trigger account 106CG on the DIV flow at -Own.POWN
	'FS												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10284)
													Cons.Book(cell,acct.RsvG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10284)
													
													'Genere le compte 106CM sur le flux DIV a -Own.PMIN
													'Trigger account 106CM on the DIV flow at -Own.PMIN
	'FS												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10285)
													Cons.Book(cell,acct.RsvM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10285)
												End If
												
												'Si la methode est Mise en Equivalence ou la nature source est Equity
												'If consolidation method is Equity Pickup or source nature is Equity
												'If method=3 Or NatureSource.Name="Equity" Then
												If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
													'Genere le compte 880EQUI sur le flux None a -Own.PCON
													'Trigger account 880EQUI on the None flow at -Own.PCON
													Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10287)
														If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then
															
															If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
															Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,elimMemberIdMC,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10287)
															End If
															
															If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
															
															brapi.errorlog.logmessage(si, "1"&elimMemberIdENTE.ToString)
															brapi.errorlog.logmessage(si, "1"&iUDTopENTE.ToString)
															
															Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10287)
															End If
														
														End If
														
			                    					'Genere le compte 261EQUI sur le flux RES a -Own.PCON
													'Trigger account 261EQUI on the RES flow at -Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10288)
													
			                    					'Genere le compte 261EQUI sur le flux REC a Own.PCON
													'Trigger account 261EQUI on the REC flow at Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Rec.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10289)
												Else
													'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
													'Eliminate source record for Stage consolidations at Own.PCON
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10290)
														If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then
															
															If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
															Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdMC,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10290)
															End If
															
															If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
															brapi.errorlog.logmessage(si, "2"&elimMemberIdENTE.ToString)
															brapi.errorlog.logmessage(si, "2"&iUDTopENTE.ToString)
															Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10290)
															End If
															
														End If
												End If
											'Pour la Nature Minoritaires (NatureDim)
											'If Nature is Minoritaires (NatureDim)	
											Else

												'Genere le compte 120G sur le flux RES a 100%
												'Trigger account 120G on the RES flow at 100%
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10292)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10292)
												End If	
							                    'Genere le compte 120M sur le flux RES a -100%
												'Trigger account 120M on the RES flow at -100%
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10293)
												
												'Genere le compte 106CG sur le flux DIV a 100%
												'Trigger account 106CG on the DIV flow at 100%
												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10294)
												
												'Genere le compte 106CG sur le flux DIV a -100%
												'Trigger account 106CG on the DIV flow at -100%
												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10295)
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberId,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10296)
												If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then

													If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdMC,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10296)
													End If
													
													If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10296)
													End If
													
												End If
												
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberId,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10297)
													If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then

														If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdMC,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10297)
														End If
													
														If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10297)
														End If
														
													End If
											End If
										End If											
										
									ElseIf RegleElim = "PROVINC" Then

										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si la nature (NatureDim) n'est pas Minoritaires
											'If nature (NatureDim) is not Minoritaires
											If NatureSource.Name<>"Minoritaires" Then
												'Genere le compte 120G sur le flux RES a -Own.POWN
												'Trigger account 120G on the RES flow at -Own.POWN
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10282)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10282)
												End If	
							                    'Genere le compte 120M sur le flux RES a -Own.PMIN
												'Trigger account 120M on the RES flow at -Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,RegleElim,10283)

												If method=FSKMethod.HOLDING Then	
													'vk
													Cons.Book(cell,acct.RsvH.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10284)
												Else
													'Genere le compte 106CG sur le flux DIV a -Own.POWN
													'Trigger account 106CG on the DIV flow at -Own.POWN
	'FS												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10284)
													Cons.Book(cell,acct.RsvG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10284)
													
													'Genere le compte 106CM sur le flux DIV a -Own.PMIN
													'Trigger account 106CM on the DIV flow at -Own.PMIN
	'FS												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10285)
													Cons.Book(cell,acct.RsvM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,RegleElim,10285)
												End If
												
												'Si la methode est Mise en Equivalence ou la nature source est Equity
												'If consolidation method is Equity Pickup or source nature is Equity
												'If method=3 Or NatureSource.Name="Equity" Then
												If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
													'Genere le compte 880EQUI sur le flux None a -Own.PCON
													'Trigger account 880EQUI on the None flow at -Own.PCON
													Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10287)
													If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then
														
														If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
														Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,elimMemberIdMC,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10287)
														End If
														
														If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
														Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10287)
														End If
														
													End If
													
			                    					'Genere le compte 261EQUI sur le flux RES a -Own.PCON
													'Trigger account 261EQUI on the RES flow at -Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10288)
													
			                    					'Genere le compte 261EQUI sur le flux REC a Own.PCON
													'Trigger account 261EQUI on the REC flow at Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Rec.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10289)
												Else
													'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
													'Eliminate source record for Stage consolidations at Own.PCON
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10290)
													If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then 
														
														If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
														Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdMC,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10290)
														End If
													
														If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
														Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10290)
														End If
														
													End If
												End If
											'Pour la Nature Minoritaires (NatureDim)
											'If Nature is Minoritaires (NatureDim)	
											Else

												'Genere le compte 120G sur le flux RES a 100%
												'Trigger account 120G on the RES flow at 100%
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10292)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10292)
												End If	
							                    'Genere le compte 120M sur le flux RES a -100%
												'Trigger account 120M on the RES flow at -100%
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10293)
												
												'Genere le compte 106CG sur le flux DIV a 100%
												'Trigger account 106CG on the DIV flow at 100%
												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10294)
												
												'Genere le compte 106CG sur le flux DIV a -100%
												'Trigger account 106CG on the DIV flow at -100%
												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10295)
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberId,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10296)
												If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then

													If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdMC,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10296)
													End If
													
													If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10296)
													End If
													
												End If
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberId,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10297)
												If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then

													If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdMC,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10297)
													End If
													
													If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10297)
													End If
													
												End If											
											End If
										End If											
										
										
									'----------------------------------------------------
					                ' Comptes de Provision intragroupe: cumul des ecarts de conversion
									' CTA on intercompany Allowance accounts
					                '----------------------------------------------------	
									ElseIf RegleElim="PROVH" Then		
			
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si le flux est OUV/VARC/SOR
											'If flow is OUV/VARC/SOR
											If flowMember.MemberId = flow.Ouv.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.Sor.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
		                						If NatureSource.Name<>"Minoritaires" Then
													
		                    						'Genere le compte 106G sur le flux Destination a -Own.POWN
													'Trigger account 106G on the Destination flow at -Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10255)
													
													'Genere le compte 106CG sur le flux Destination a Own.POWN
													'Trigger account 106CG on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10256)
													
													'Genere le compte 106M sur le flux Destination a -Own.PMIN
													'Trigger account 106M on the Destination flow at -Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10257)
													
													'Genere le compte 106CM sur le flux Destination a Own.PMIN
													'Trigger account 106CM on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10258)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
												
													'Genere le compte 106G sur le flux Destination a 100%
													'Trigger account 106G on the Destination flow at 100%
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10260)
													
													'Genere le compte 106CG sur le flux Destination a -100%
													'Trigger account 106CG on the Destination flow at -100%
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10261)
													
													'Genere le compte 106M sur le flux Destination a -100%
													'Trigger account 106M on the Destination flow at -100%
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10262)
													
													'Genere le compte 106CM sur le flux Destination a 100%
													'Trigger account 106CM on the Destination flow at 100%
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10263)
													
		                						End If 'CurCustom4<>Mino
											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF	
											ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													
								                    'Genere le compte 106CG sur le flux Destination a Own.POWN
													'Trigger account 106CG on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10265)
													
								                    'Genere le compte 106CG sur le flux Destination a Own.PMIN
													'Trigger account 106CG on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10266)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
												
								                    'Genere le compte 106CG sur le flux Destination a -100%
													'Trigger account 106CG on the Destination flow at -100%
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10268)
													
								                    'Genere le compte 106CM sur le flux Destination a 100%
													'Trigger account 106CM on the Destination flow at 100%
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10269)
													
												End If	'CurCustom4<>Mino
											'Si le flux est ECR
											'If the flow is ECR
											ElseIf flowMember.MemberId = flow.Ecr.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
												
								                    'Genere le compte 120CG sur le flux ECF a Own.POWN
													'Trigger account 120CG on the ECF flow at Own.POWN
													Cons.Book(cell,acct.ResCG.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10271)
													
								                    'Genere le compte 120CM sur le flux ECF a Own.PMIN
													'Trigger account 120CM on the ECF flow at Own.PMIN
													Cons.Book(cell,acct.ResCM.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10272)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)														
												Else
												
								                    'Genere le compte 120CG sur le flux ECF a -100%
													'Trigger account 120CG on the ECF flow at -100%
													Cons.Book(cell,acct.ResCG.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10274)
													
								                    'Genere le compte 120CM sur le flux ECF a 100%
													'Trigger account 120CM on the ECF flow at 100%
													Cons.Book(cell,acct.ResCM.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10275)
													
												End If	'CurCustom4<>Mino
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10276)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10277)
											End If	
										End If	'Can Eliminate
										
									ElseIf RegleElim = "DIVVVAR" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature	
											'Not needed
					
										End If
									ElseIf RegleElim = "DIVVVAR2" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											
											'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
											'Eliminate source record for Stage consolidations (at 100%)
											'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10279)
											
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature	
											'Not needed
					
										End If
									ElseIf RegleElim = "DIVV" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											
											'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
											'Eliminate source record for Stage consolidations (at 100%)
											'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10280)
																								
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature					
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then

											ElseIf Own.pCon<1 Then
												'Deactivated - Can be removed
												'Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,(1-Own.pCon),,RegleElim,10281)
											End If
					
										End If	
									
									'fs DIVA works like DIVR but uses EnlaDiv instead of conso reserves	
									ElseIf RegleElim = "DIVA" Then

										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si la nature (NatureDim) n'est pas Minoritaires
											'If nature (NatureDim) is not Minoritaires
											If NatureSource.Name<>"Minoritaires" Then
												'Genere le compte 120G sur le flux RES a -Own.POWN
												'Trigger account 120G on the RES flow at -Own.POWN
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10282)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10282)
												End If	
												
							                    'Genere le compte 120M sur le flux RES a -Own.PMIN
												'Trigger account 120M on the RES flow at -Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10283)
												
												'Genere le compte 106CG sur le flux DIV a -Own.POWN
												'Trigger account 106CG on the DIV flow at -Own.POWN
'FS												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10284)
												'fs impact ENLADIV instead of Reserves
												'Cons.Book(cell,acct.RsvG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10284)
												Cons.Book(cell,acct.EnlaDiv.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10284)
												
												'Genere le compte 106CM sur le flux DIV a -Own.PMIN
												'Trigger account 106CM on the DIV flow at -Own.PMIN
'FS												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10285)
												Cons.Book(cell,acct.RsvM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10285)
												
												'Si la methode est Mise en Equivalence ou la nature source est Equity
												'If consolidation method is Equity Pickup or source nature is Equity
												'If method=3 Or NatureSource.Name="Equity" Then
												If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
													'Genere le compte 880EQUI sur le flux None a -Own.PCON
													'Trigger account 880EQUI on the None flow at -Own.PCON
													Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10287)
													If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then
														
														If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
														Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,elimMemberIdMC,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10287)
														End If
													
														If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
														Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10287)
														End If
														
													End If
													
			                    					'Genere le compte 261EQUI sur le flux RES a -Own.PCON
													'Trigger account 261EQUI on the RES flow at -Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10288)
													
			                    					'Genere le compte 261EQUI sur le flux REC a Own.PCON
													'Trigger account 261EQUI on the REC flow at Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Rec.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10289)
												Else
													'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
													'Eliminate source record for Stage consolidations at Own.PCON
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10290)
													If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then 		
													
														If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
														Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdMC,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10290)
														End If
														
														If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
														Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10290)
														End If
														
													End If
												End If
											'Pour la Nature Minoritaires (NatureDim)
											'If Nature is Minoritaires (NatureDim)	
											Else

												'Genere le compte 120G sur le flux RES a 100%
												'Trigger account 120G on the RES flow at 100%
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10292)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10292)
												End If	
							                    'Genere le compte 120M sur le flux RES a -100%
												'Trigger account 120M on the RES flow at -100%
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10293)
												
												'Genere le compte 106CG sur le flux DIV a 100%
												'Trigger account 106CG on the DIV flow at 100%
												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10294)
												
												'Genere le compte 106CG sur le flux DIV a -100%
												'Trigger account 106CG on the DIV flow at -100%
												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10295)
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberId,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10296)
											If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then 
											
												If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
												Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdMC,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10296)
												End If
											
												If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
												Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10296)
												End If
												
											End If
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberId,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10297)
												If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then

													If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdMC,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10297)
													End If
													
													If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10297)
													End If
													
												End If
											End If
										End If	
									
									ElseIf RegleElim = "DIVR" Then
									'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si la nature (NatureDim) n'est pas Minoritaires
											'If nature (NatureDim) is not Minoritaires
											If NatureSource.Name<>"Minoritaires" Then
												'Genere le compte 120G sur le flux RES a -Own.POWN
												'Trigger account 120G on the RES flow at -Own.POWN
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10282)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10282)
												End If	
												
							                    'Genere le compte 120M sur le flux RES a -Own.PMIN
												'Trigger account 120M on the RES flow at -Own.PMIN
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10283)
												
												'Genere le compte 106CG sur le flux DIV a -Own.POWN
												'Trigger account 106CG on the DIV flow at -Own.POWN
'FS												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10284)
If method=FSKMethod.Holding Then												
Cons.Book(cell,acct.RsvH.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10284)
Else
Cons.Book(cell,acct.RsvG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10284)
End If
												
												'Genere le compte 106CM sur le flux DIV a -Own.PMIN
												'Trigger account 106CM on the DIV flow at -Own.PMIN
'FS												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10285)
												Cons.Book(cell,acct.RsvM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10285)
												
												'Si la methode est Mise en Equivalence ou la nature source est Equity
												'If consolidation method is Equity Pickup or source nature is Equity
												'If method=3 Or NatureSource.Name="Equity" Then
												If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
													'Genere le compte 880EQUI sur le flux None a -Own.PCON
													'Trigger account 880EQUI on the None flow at -Own.PCON
													Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10287)
													If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then 
													
														If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
														Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,elimMemberIdMC,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10287)
														End If
													
														If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
														Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10287)
														End If
														
													End If
													
			                    					'Genere le compte 261EQUI sur le flux RES a -Own.PCON
													'Trigger account 261EQUI on the RES flow at -Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10288)
													
			                    					'Genere le compte 261EQUI sur le flux REC a Own.PCON
													'Trigger account 261EQUI on the REC flow at Own.PCON
													Cons.Book(cell,acct.MEE.MemberId,flow.Rec.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10289)
												Else
													'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
													'Eliminate source record for Stage consolidations at Own.PCON
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10290)
													If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then 
														
														If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
														Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdMC,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10290)
														End If
														
														If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
														Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10290)
														End If
														
													End If
												End If
											'Pour la Nature Minoritaires (NatureDim)
											'If Nature is Minoritaires (NatureDim)	
											Else

												'Genere le compte 120G sur le flux RES a 100%
												'Trigger account 120G on the RES flow at 100%
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10292)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10292)
												End If	
							                    'Genere le compte 120M sur le flux RES a -100%
												'Trigger account 120M on the RES flow at -100%
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10293)
												
												'Genere le compte 106CG sur le flux DIV a 100%
												'Trigger account 106CG on the DIV flow at 100%
												Cons.Book(cell,acct.RsvCG.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10294)
												
												'Genere le compte 106CG sur le flux DIV a -100%
												'Trigger account 106CG on the DIV flow at -100%
												Cons.Book(cell,acct.RsvCM.MemberId,flow.Div.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10295)
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberId,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10296)
												If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then 
													
													If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdMC,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10296)
													End If
													
													If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10296)
													End If
													
												End If
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberId,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10297)
												If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") Then 	
													
													If api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdMC,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10297)
													End If
												
													If api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10297)
													End If
													
												End If
											End If
										End If	
									ElseIf RegleElim = "DIVRH" Then
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si le flux est ECF
											'If the flow is ECF
											If flowMember.MemberId = flow.Ecf.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then

													'Genere le compte 120CG sur le flux ECF a -Own.POWN
													'Trigger account 120CG on the ECF flow at -Own.POWN
													Cons.Book(cell,acct.ResCG.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10299)
													
								                    'Genere le compte 120CM sur le flux ECF a -Own.PMIN
													'Trigger account 120CM on the ECF flow at -Own.PMIN
													Cons.Book(cell,acct.ResCM.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10300)
													
													'Genere le compte 106CG sur le flux ECF a Own.POWN
													'Trigger account 106CG on the ECF flow at Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10301)
													
													'Genere le compte 106CM sur le flux ECF a Own.PMIN
													'Trigger account 106CM on the ECF flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10302)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
												
													'Genere le compte 120CG sur le flux ECF a 100%
													'Trigger account 120CG on the ECF flow at 100%
													Cons.Book(cell,acct.ResCG.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10304)
													
								                    'Genere le compte 120CM sur le flux ECF a -100%
													'Trigger account 120CM on the ECF flow at -100%
													Cons.Book(cell,acct.ResCM.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10305)
													
													'Genere le compte 106CG sur le flux ECF a -100%
													'Trigger account 106CG on the ECF flow at -100%
													Cons.Book(cell,acct.RsvCG.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10306)
													
													'Genere le compte 106CM sur le flux ECF a 100%
													'Trigger account 106CM on the ECF flow at 100%
													Cons.Book(cell,acct.RsvCM.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10307)
												End If
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10308)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10309)
											End If
												
										End If	
									ElseIf RegleElim = "DIVRVAR" Then
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si le flux est OUV/VARC/SOR
											'If flow is OUV/VARC/SOR		
											If flowMember.MemberId = flow.Ouv.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.Sor.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Genere le compte 106G sur le flux Destination a -Own.POWN
													'Trigger account 106G on the Destination flow at -Own.POWN
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10311)
													
								                    'Genere le compte 106M sur le flux Destination a Own.PMIN
													'Trigger account 106M on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10312)
													
													'Genere le compte 106CG sur le flux Destination a Own.POWN
													'Trigger account 106CG on the Destination flow at Own.POWN
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10313)
													
													'Genere le compte 106CM sur le flux Destination a Own.PMIN
													'Trigger account 106CM on the Destination flow at Own.PMIN
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10314)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
													'Genere le compte 106G sur le flux Destination a 100%
													'Trigger account 106G on the Destination flow at 100%
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10315)
													
								                    'Genere le compte 106M sur le flux Destination a -100%
													'Trigger account 106M on the Destination flow at -100%
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10316)
													
													'Genere le compte 106CG sur le flux Destination a -100%
													'Trigger account 106CG on the Destination flow at -100%
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10317)
													
													'Genere le compte 106CM sur le flux Destination a 100%
													'Trigger account 106CM on the Destination flow at 100%
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10318)
												End If
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10319)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10320)
											End If
												
										End If	
									
									ElseIf RegleElim = "ADIVV" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											
											'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
											'Eliminate source record for Stage consolidations (at 100%)
											Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10321)
																							
										End If
										
									ElseIf RegleElim = "ADIVR" Then
										If icMethod12345 Then
											If NatureSource.Name<>"Minoritaires" Then
												Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.PCon,,RegleElim,10322) 'NPXX POwn -> PCon
												Cons.Book(cell,acct.RsvCG.MemberId,flow.IDiv.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10324) 'NPXX POwn -> PCon

												If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
													Cons.Book(cell,acct.ResultatMEE.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10327)
													Cons.Book(cell,acct.MEE.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10328)
													Cons.Book(cell,acct.MEE.MemberId,flow.Rec.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10329)
												Else
													Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10330)
												End If

											Else
												
												If method=FSKMethod.Holding Then
													Cons.Book(cell,acct.ResH.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10282)
												Else
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10282)
												End If	
												
												Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10333)
												Cons.Book(cell,acct.RsvCG.MemberId,flow.IDiv.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,RegleElim,10334)
												
												Cons.Book(cell,acct.RsvCM.MemberId,flow.IDiv.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10335)
											End If

										ElseIf icMember.MemberID<>DimConstants.None Then	'Donnees techniques pour les paliers
											Cons.Book(cell,acctMember.MemberId,,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10336)
											
											If method=FSKMethod.MEE Then
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10337)
											End If
										End If
									ElseIf RegleElim = "RAS" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
																					
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature					
												
							
										End If
									ElseIf RegleElim = "DIVVH" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											
											'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
											'Eliminate source record for Stage consolidations (at 100%)
											'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10339)
											
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature					
												

					
										End If
									'----------------------------------------------------
					                ' Elimination reciproques
									' Reciprocal eliminations
					                '----------------------------------------------------	
									ElseIf RegleElim="ELIM" Then
										'Si la methode de l'entite n'est pas Mise en Equivalence
										'If the consolidation method of the entity is not Equity Pickup
										'If method <> 3 Then
										
										If method <> FSKMethod.MEE Then
											'Si la methode du partenaire est 1,2,3,4,5
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
							'------> fs Elim standard (DN)			
'												If NatureSource.Name="PropE" Then												
												If NatureSource.MemberId=Nat.PropE.MemberId Then												
													'HS.Con aCompte & C1Dest & c4Nature & vElim,-1,Audit
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,-1,,RegleElim,10340)
											
													'HS.Con aCompte & C1Dest & vElim,PCon*(-1),Audit
													Cons.Book(cell,acctMember.MemberId,flow.Res.MemberId,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,-Own.pcon,,RegleElim,10341)
											
							                        'HS.Con aCompte & C1Dest & c4PropU & vElim,-1,Audit
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberId,,,NatureDimPropU.MemberId,,,,,-1,,RegleElim,10342)
													

'									        	ElseIf NatureSource.Name="PropU" Then																	'
									        	ElseIf NatureSource.MemberId=Nat.PropU.MemberId Then																	'
							                        'HS.Con aCompte & C1Dest & c4Nature & vElim,Min,Audit
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,MinpCon ,,RegleElim,10343)
													
													'HS.Con aCompte & C1Dest & c4PropU & vElim,(PCon-Min)*(-1),Audit
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberId,,,NatureDimPropU.MemberId,,,,,-(own.pCon-MinpCon),,RegleElim,10344)
													
							                    
												Else
													'Elimine la donnee source au minimum de % de l'entite ou du partenaire
													'Eliminate source record at the minimum % ownership of the entity or the ICP
													'Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-MinpCon,,RegleElim,10345)
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,-MinpCon,,RegleElim,10345)
													'brapi.errorlog.LogMessage(si, "ElimMemberUD1id " & elimMemberid & " ElimMemberUD1 " & elimMember & " Account " & cell.DataBufferCellPk.GetAccountName(api))	
													'resultDataBuffer.LogDataBuffer(api,api.Pov.Entity.Name & api.Pov.Time.Name & api.Pov.Cons.Name & ":Target Data Buffer", 1000)
													
													'Si le type de compte du compte source = le type de compte du compte de liaison (ex: ASSET et ASSET)
													'If the account type of the source account = the account type of the plug account (ex: ASSET and ASSET)
													If acctAccountType = plugAccountType Then													
														'Genere le compte de liaison sur le flux de Destination au minimum du % de l'entite ou du partenaire
														'Trigger the plug account on the Destination flow at the minimum % ownership of the entity or the ICP
														'Cons.Book(cell,plugElim.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,MinpCon ,,RegleElim,10346)
														Cons.Book(cell,plugElim.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,MinpCon ,,RegleElim,10346)
													'Si le type de compte du compte source est <> du type de compte du compte de liaison (ex: ASSET et LIABILITY)
													'If the account type of the source account is <> from the account type of the plug account (ex: ASSET and LIABILITY)
													Else
														'Genere le compte de liaison sur le flux de Destination au minimum du % de l'entite ou du partenaire (signe inverse)
														'Trigger the plug account on the Destination flow at the minimum % ownership of the entity or the ICP (reverse sign)
														'Cons.Book(cell,plugElim.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-MinpCon,,RegleElim,10347)
														Cons.Book(cell,plugElim.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberId,,,NatureDest.MemberId,,,,,-MinpCon,,RegleElim,10347)
														'Cons.Book(cell,plugElim.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,MinpCon,,RegleElim,10347)
													End If	
												End If

							'------> fs Elim MERCADO				

												If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") And api.Members.IsDescendant(oUDDim, iUDTopMC, elimMemberIdMC) Then

													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdMC,,,NatureDest.MemberId,,,,,-MinpCon,,RegleElim,10345)
													If acctAccountType = plugAccountType Then													
														Cons.Book(cell,plugElim.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdMC,,,NatureDest.MemberId,,,,,MinpCon ,,RegleElim,10346)
													Else
														Cons.Book(cell,plugElim.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdMC,,,NatureDest.MemberId,,,,,-MinpCon,,RegleElim,10347)
													End If	
													'resultDataBuffer.LogDataBuffer(api,api.Pov.Entity.Name & api.Pov.Time.Name & api.Pov.Cons.Name & ":Target Data Buffer", 1000)

												End If
'							'------> fs Elim ENTES				
												If (cell.DataBufferCellPk.GetUD1Name(api) <> cell.DataBufferCellPk.GetUD2Name(api)) And (cell.DataBufferCellPk.GetUD1Name(api)<>"None") And api.Members.IsDescendant(oUDDim, iUDTopENTE, elimMemberIdENTE) Then

													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDest.MemberId,,,,,-MinpCon,,RegleElim,10345)
													If acctAccountType = plugAccountType Then													
														Cons.Book(cell,plugElim.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDest.MemberId,,,,,MinpCon ,,RegleElim,10346)
													Else
														Cons.Book(cell,plugElim.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,elimMemberIdENTE,,,NatureDest.MemberId,,,,,-MinpCon,,RegleElim,10347)
													End If	
													'resultDataBuffer.LogDataBuffer(api,api.Pov.Entity.Name & api.Pov.Time.Name & api.Pov.Cons.Name & ":Target Data Buffer", 1000)

												End If

												
											End If
										End If
									'----------------------------------------------------
					                ' Fondo de Comercio de Consolidación (con impacto en reservas de la participada)
									' Goodwill (impact the subsidiary entity reserves)
					                '----------------------------------------------------	
									ElseIf RegleElim="GW" Then
										Call cons.PElimPartnerTable(si,api,icMember)
'										Dim aLTIT As Member = api.Members.GetMember(DimType.Account.Id, "18L")
										Dim aLTIT As Member = api.Members.GetMember(DimType.Account.Id, "1LEG")
										Dim icpOwn As New OwnershipClass(si,api,globals, icName,,,,OpeScenario)
										Dim acctGW As Member = api.Account.GetPlugAccount(cell.DataBufferCellPk.AccountId)
										
										
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si le flux est AUG or DIM
											'If flow is AUG or DIM (not a problem target flow is always flowDestMember)
											If flowMember.MemberId = flow.Aug.MemberId Or flowMember.MemberId = flow.Dimi.MemberId  Then
												'Si le partenaire n'est pas consolide en N-1
												'If the ICP is not consolidated in N-1
												If ICPOwn.pCon_1 = 0 Then
													'Si la nature (NatureDim) n'est pas Minoritaires
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then												
								                        'Genere le compte 18L sur le flux Destination a Own.PCON 
														'Trigger account 18L on the Destination flow at Own.PCON								                        
														Cons.Book(cell,aLTIT.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10348)
														
														'Si la methode est Mise en Equivalence ou la nature source est Equity
														'If consolidation method is Equity Pickup or source nature is Equity
														'If method=3 Or NatureSource.Name="Equity" Then
														If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														
															'Genere le compte 261EQUI sur le flux Destination a -Own.PCON
															'Trigger account 261EQUI on the Destination flow at -Own.PCON
															Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10350)
														'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
														'If the consolidation method is not Equity Pickup or the nature is not Equity	
														Else
															'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
															'Eliminate source record for Stage consolidations at Own.PCON
															Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10351)
														End If
													'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
													'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding
													'ElseIf icMethod <> 1 Then
													ElseIf icMethod <> FSKMethod.HOLDING Then
														'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
														'Eliminate source record for Stage consolidations (at 100%)						
														Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,1,,RegleElim,10352)
												
													End If
												'Si le partenaire est consolide en N-1
												'If the ICP is consolidated in N-1	
												Else
													'Si la nature (NatureDim) n'est pas Minoritaires
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then												
								                        'Genere le compte 18L sur le flux source a Own.PCON 
														'Trigger account 18L on the source flow at Own.PCON								                        
														Cons.Book(cell,aLTIT.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10353)
																												
														'Si la methode est Mise en Equivalence ou la nature source est Equity
														'If consolidation method is Equity Pickup or source nature is Equity
														'If method=3 Or NatureSource.Name="Equity" Then
														If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														
															'Genere le compte 261EQUI sur le flux Destination a -Own.PCON
															'Trigger account 261EQUI on the Destination flow at -Own.PCON
															Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10355)
														'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
														'If the consolidation method is not Equity Pickup or the nature is not Equity	
														Else
															'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
															'Eliminate source record for Stage consolidations at Own.PCON
															Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10356)
														End If
													'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
													'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding	
													'ElseIf icMethod <> 1 Then	
													ElseIf icMethod <> FSKMethod.HOLDING Then
														'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
														'Eliminate source record for Stage consolidations (at 100%)						
														Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,1,,RegleElim,10357)
														
													End If
												End If
											
											'Si le flux est AUGCAP
											'If the flow is AUGCAP	
											ElseIf flowMember.MemberId = flow.AUGCAP.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
							                        'Genere le compte 18L sur le flux AUG a Own.PCON 
													'Trigger account 18L on the AUG flow at Own.PCON 								                        
													Cons.Book(cell,aLTIT.MemberId,flow.Aug.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10358)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
													
														'Genere le compte 261EQUI sur le flux AUG a -Own.PCON
														'Trigger account 261EQUI on the AUG flow at -Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flow.Aug.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10360)
													'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10361)
													End If
												'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding		
												'ElseIf icMethod <> 1 Then
												ElseIf icMethod <> FSKMethod.HOLDING Then
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,1,,RegleElim,10362)
														
												End If
											'Si le flux est DIMCAP
											'If the flow is DIMCAP	
											ElseIf flowMember.MemberId = flow.DIMCAP.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
							                        'Genere le compte 18L sur le flux DIM a Own.PCON 
													'Trigger account 18L on the DIM flow at Own.PCON								                        
													Cons.Book(cell,aLTIT.MemberId,flow.Di.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10363)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then												
														'Genere le compte 261EQUI sur le flux DIM a -Own.PCON
														'Trigger account 261EQUI on the DIM flow at -Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flow.Di.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10365)
													'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10366)
													End If
												'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding	
												'ElseIf icMethod <> 1 Then
												ElseIf icMethod <> FSKMethod.HOLDING Then

													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,1,,RegleElim,10367)

												End If
											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF
											ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
							                        'Genere le compte 18L sur le flux Destination a Own.PCON
													'Trigger account 18L on the Destination flow at Own.PCON								                        
													Cons.Book(cell,aLTIT.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10368)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
														'Genere le compte 261EQUI sur le flux Destination a -Own.PCON
														'Trigger account 261EQUI on the Destination flow at -Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10370)
													'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10371)
													End If
												'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding															
												'ElseIf icMethod <> 1 Then
												ElseIf icMethod <> FSKMethod.HOLDING Then
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,1,,RegleElim,10372)
														
												End If
											
											'Pour les autres flux
											'For other flows	
											Else
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
							                        'Genere le compte 18L sur le flux Destination a Own.PCON
													'Trigger account 18L on the Destination flow at Own.PCON								                        
													Cons.Book(cell,aLTIT.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon ,,RegleElim,10373)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then													
														'Genere le compte 261EQUI sur le flux Destination a -Own.PCON
														'Trigger account 261EQUI on the Destination flow at -Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10375)
													'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10376)
													End If
												'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding															
												'ElseIf icMethod <> 1 Then	
												ElseIf icMethod <> FSKMethod.HOLDING Then
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,1,,RegleElim,10377)
														
												End If
												
											End If
											
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,-Own.pMin ,,RegleElim,10378)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctGW.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10379)
											End If	
										End If	

									'----------------------------------------------------
					                ' Titres de participation
									' Investment in Subs
					                '----------------------------------------------------	
									ElseIf RegleElim="TITRES" Then
										Call cons.PElimPartnerTable(si,api,icMember)
'										Dim aLTIT As Member = api.Members.GetMember(DimType.Account.Id, "18L")
										Dim aLTIT As Member = api.Members.GetMember(DimType.Account.Id, "1LEG")
										Dim icpOwn As New OwnershipClass(si,api,globals, icName,,,,OpeScenario)
				'		resultDataBuffer.LogDataBuffer(api,api.Pov.Entity.Name &"-"& api.Pov.Time.Name &"-"& api.Pov.Cons.Name & ":Target Data Buffer", 1000)
				'		sourceDataBuffer.LogDataBuffer(api,api.Pov.Entity.Name &"-"& api.Pov.Time.Name &"-"& api.Pov.Cons.Name & ":Source Data Buffer", 1000)
										
				'		brapi.errorlog.LogMessage(si, "Entity p1 " & api.Pov.Entity.Name & " IC p1 " & ICName)	
					
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											'Si le flux est AUG or DIM
											'If flow is AUG or DIM (not a problem target flow is always flowDestMember)
											If flowMember.MemberId = flow.Aug.MemberId Or flowMember.MemberId = flow.Dimi.MemberId  Then
												'Si le partenaire n'est pas consolide en N-1
												'If the ICP is not consolidated in N-1
												If ICPOwn.pCon_1 = 0 Then
													'Si la nature (NatureDim) n'est pas Minoritaires
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then												
								                        'Genere le compte 18L sur le flux Destination a Own.PCON 
														'Trigger account 18L on the Destination flow at Own.PCON								                        
														Cons.Book(cell,aLTIT.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10348)
														
														'Si la methode est Mise en Equivalence ou la nature source est Equity
														'If consolidation method is Equity Pickup or source nature is Equity
														'If method=3 Or NatureSource.Name="Equity" Then
														If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														
															'Genere le compte 261EQUI sur le flux Destination a -Own.PCON
															'Trigger account 261EQUI on the Destination flow at -Own.PCON
															Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10350)
														'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
														'If the consolidation method is not Equity Pickup or the nature is not Equity	
														Else
															'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
															'Eliminate source record for Stage consolidations at Own.PCON
															Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10351)
														End If
													'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
													'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding
													'ElseIf icMethod <> 1 Then
													ElseIf icMethod <> FSKMethod.HOLDING Then
														'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
														'Eliminate source record for Stage consolidations (at 100%)						
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10352)
												
													End If
												'Si le partenaire est consolide en N-1
												'If the ICP is consolidated in N-1	
												Else
													'Si la nature (NatureDim) n'est pas Minoritaires
													'If nature (NatureDim) is not Minoritaires
													If NatureSource.Name<>"Minoritaires" Then												
								                        'Genere le compte 18L sur le flux source a Own.PCON 
														'Trigger account 18L on the source flow at Own.PCON								                        
														Cons.Book(cell,aLTIT.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10353)
																												
														'Si la methode est Mise en Equivalence ou la nature source est Equity
														'If consolidation method is Equity Pickup or source nature is Equity
														'If method=3 Or NatureSource.Name="Equity" Then
														If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														
															'Genere le compte 261EQUI sur le flux Destination a -Own.PCON
															'Trigger account 261EQUI on the Destination flow at -Own.PCON
															Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10355)
														'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
														'If the consolidation method is not Equity Pickup or the nature is not Equity	
														Else
															'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
															'Eliminate source record for Stage consolidations at Own.PCON
															Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10356)
														End If
													'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
													'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding	
													'ElseIf icMethod <> 1 Then	
													ElseIf icMethod <> FSKMethod.HOLDING Then
														'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
														'Eliminate source record for Stage consolidations (at 100%)						
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10357)
														
													End If
												End If
											
											'Si le flux est AUGCAP
											'If the flow is AUGCAP	
											ElseIf flowMember.MemberId = flow.AUGCAP.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
							                        'Genere le compte 18L sur le flux AUG a Own.PCON 
													'Trigger account 18L on the AUG flow at Own.PCON 								                        
													Cons.Book(cell,aLTIT.MemberId,flow.Aug.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10358)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
													
														'Genere le compte 261EQUI sur le flux AUG a -Own.PCON
														'Trigger account 261EQUI on the AUG flow at -Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flow.Aug.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10360)
													'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10361)
													End If
												'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding		
												'ElseIf icMethod <> 1 Then
												ElseIf icMethod <> FSKMethod.HOLDING Then
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10362)
														
												End If
											'Si le flux est DIMCAP
											'If the flow is DIMCAP	
											ElseIf flowMember.MemberId = flow.DIMCAP.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
							                        'Genere le compte 18L sur le flux DIM a Own.PCON 
													'Trigger account 18L on the DIM flow at Own.PCON								                        
													Cons.Book(cell,aLTIT.MemberId,flow.Di.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10363)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then												
														'Genere le compte 261EQUI sur le flux DIM a -Own.PCON
														'Trigger account 261EQUI on the DIM flow at -Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flow.Di.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10365)
													'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10366)
													End If
												'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding	
												'ElseIf icMethod <> 1 Then
												ElseIf icMethod <> FSKMethod.HOLDING Then

													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10367)

												End If
											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF
											ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
							                        'Genere le compte 18L sur le flux Destination a Own.PCON
													'Trigger account 18L on the Destination flow at Own.PCON								                        
													Cons.Book(cell,aLTIT.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10368)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
												
														'Genere le compte 261EQUI sur le flux Destination a -Own.PCON
														'Trigger account 261EQUI on the Destination flow at -Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10370)
													'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10371)
													End If
												'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding															
												'ElseIf icMethod <> 1 Then
												ElseIf icMethod <> FSKMethod.HOLDING Then
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10372)
														
												End If
											
											'Pour les autres flux
											'For other flows	
											Else
			'			resultDataBuffer.LogDataBuffer(api,api.Pov.Entity.Name &"-"& api.Pov.Time.Name &"-"& api.Pov.Cons.Name & ":Target Data Buffer", 1000)
			'			sourceDataBuffer.LogDataBuffer(api,api.Pov.Entity.Name &"-"& api.Pov.Time.Name &"-"& api.Pov.Cons.Name & ":Source Data Buffer", 1000)

												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
							                        'Genere le compte 18L sur le flux Destination a Own.PCON
													'Trigger account 18L on the Destination flow at Own.PCON								                        
													Cons.Book(cell,aLTIT.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10373)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then													
														'Genere le compte 261EQUI sur le flux Destination a -Own.PCON
														'Trigger account 261EQUI on the Destination flow at -Own.PCON
														Cons.Book(cell,acct.MEE.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10375)
													'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10376)
													End If
												'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding															
												'ElseIf icMethod <> 1 Then	
												ElseIf icMethod <> FSKMethod.HOLDING Then
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10377)
		
												End If
												
											End If
											
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10378)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10379)
											End If	
										End If	
										
									'----------------------------------------------------
					                ' Titres de participation: cumul des ecarts de conversion
									' CTA on Investment in Subs
					                '----------------------------------------------------
									ElseIf RegleElim="TITRESH" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then	
											'Si le flux est OUV/VARC/SOR
											'If flow is OUV/VARC/SOR
											If flowMember.MemberId = flow.Ouv.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.Sor.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then

												'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
												'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding
												'ElseIf icMethod <> 1 Then
												ElseIf icMethod <> FSKMethod.HOLDING Then
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)
													'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10382)
													
												End If	'CurCustom4<>Mino
												
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10383)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												'SDL not balanced entry Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10384)
											End If	
										End If	
									'----------------------------------------------------
					                ' Marge en Stock
									' Inventory profits
					                '----------------------------------------------------				
									ElseIf RegleElim="MSTK" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											Dim CompteStock As Member = api.Account.GetPlugAccount(cell.DataBufferCellPk.AccountId)
											Dim plugResultat As Member = api.Members.GetMember(DimType.Account.Id, "65L")
											Dim plugBilan As Member = api.Members.GetMember(DimType.Account.Id, "40L")
											Dim icpOwn As New OwnershipClass(si,api,globals, icName,,,,OpeScenario)
											
											'Si le flux est MVT
											'If the flow is MVT
											If flowMember.MemberId = flow.MVT.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'----------- Impact chez la vendeuse - Seller-------------------------
								                    'Genere le compte 120G sur le flux RES a -Own.POWN * Own.PCON du partenaire
													'Trigger account 120G on the RES flow at -Own.POWN * Own.PCON of ICP
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pOwn * ICPOwn.pCon),,RegleElim,10385)
													
								                    'Genere le compte 120M sur le flux RES a -Own.PMIN * Own.PCON du partenaire
													'Trigger account 120M on the RES flow at -Own.PMIN * Own.PCON of ICP
													Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pMin * ICPOwn.pcon),,RegleElim,10386)
														
								                    'Genere le compte 40L sur le flux Destination a -Own.PCON * Own.PCON du partenaire
													'Trigger account 40L on the Destination flow at -Own.PCON * Own.PCON of ICP
													Cons.Book(cell,plugBilan.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.pCon),,RegleElim,10387)
														
								                    'Genere le compte 65L sur le flux None a -Own.PCON * Own.PCON du partenaire
													'Trigger account 65L on the None flow at -Own.PCON * Own.PCON of ICP
													Cons.Book(cell,plugResultat.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pCon * ICPOwn.pCon),,RegleElim,10388)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
														'Eliminate source record for Stage consolidations (at 100%)						
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10389)
													'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
													'If the consolidation method is not Equity Pickup or the nature is not Equity
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON						
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10390)
													End If
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10391)
													
													'Genere le compte 120G sur le flux RES a Own.PCON du partenaire
													'Trigger account 120G on the RES flow at Own.PCON of ICP
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(1 * ICPOwn.pCon),,RegleElim,10392)
													
		                    						'Genere le compte 120M sur le flux RES a -Own.PCON du partenaire
													'Trigger account 120M on the RES flow at -Own.PCON of ICP
													Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(1 * ICPOwn.pCon),,RegleElim,10393)
												End If
											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF
											ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
												'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
												'Eliminate source record for Stage consolidations (at 100%)						
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10394)
											'Pour les autres flux
											'For other flows	
											Else
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then													
													'----------- Impact chez la vendeuse - Seller-------------------------
								                    'Genere le compte 106G sur le flux Destination a -Own.POWN * Own.PCON du partenaire
													'Trigger account 106G on the Destination flow at -Own.POWN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pOwn * ICPOwn.pCon),,RegleElim,10395)
													
								                    'Genere le compte 106M sur le flux Destination a -Own.PMIN * Own.PCON du partenaire
													'Trigger account 106M on the Destination flow at -Own.PMIN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pMin * ICPOwn.pCon),,RegleElim,10396)
													
								                    'Genere le compte 40L sur le flux Destination a -Own.PCON * Own.PCON du partenaire et UD7 est l'ICP
													'Trigger the account 40L on the Destination flow at -Own.PCON * Own.PCON of ICP and the UD7 is the ICP
													Cons.Book(cell,plugBilan.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.pCon),,RegleElim,10397)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
														'Eliminate source record for Stage consolidations (at 100%)						
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10398)
													'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON						
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10399)
													End If
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10400)
													
								                    'Genere le compte 106G sur le flux Destination a Own.PCON du partenaire
													'Trigger account 106G on the Destination flow at Own.PCON of ICP
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.pCon ,,RegleElim,10401)
													
								                    'Genere le compte 106M sur le flux Destination a -Own.PCON du partenaire
													'Trigger account 106M on the Destination flow at -Own.PCON of ICP
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.pCon,,RegleElim,10402)
													
												End If
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the Minority Interests portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10403)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10404)
											End If
													
										End If	'If Can Eliminate
									'----------------------------------------------------
					                ' Marge en Stock: cumul des ecarts de conversion
									' CTA on Inventory profits
					                '----------------------------------------------------
									ElseIf RegleElim="MSTKH" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											Dim CompteStock As Member = api.Account.GetPlugAccount(cell.DataBufferCellPk.AccountId)
											Dim plugResultat As Member = api.Members.GetMember(DimType.Account.Id, "65L")
											Dim plugBilan As Member = api.Members.GetMember(DimType.Account.Id, "40L")
											Dim icpOwn As New OwnershipClass(si,api,globals, icName,,,,OpeScenario)
											
											'Si le flux est OUV/VARC/SOR
											'If flow is OUV/VARC/SOR
											If flowMember.MemberId = flow.Ouv.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.Sor.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10405)
														
												    'Genere le compte 106G sur le flux Destination a Own.POWN * Own.PCON du partenaire
													'Trigger account 106G on the Destination flow at Own.POWN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pOwn * ICPOwn.pCon),,RegleElim,10406)
													
								                    'Genere le compte 106CG sur le flux Destination a -Own.POWN * Own.PCON du partenaire
													'Trigger account 106CG on the Destination flow at -Own.POWN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pOwn * ICPOwn.pCon),,RegleElim,10407)
													
								                    'Genere le compte 106M sur le flux Destination a Own.PMIN * Own.PCON du partenaire
													'Trigger account 106M on the Destination flow at Own.PMIN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pMin * ICPOwn.pCon),,RegleElim,10408)
													
								                    'Genere le compte 106CM sur le flux Destination a -Own.PMIN * Own.PCON du partenaire
													'Trigger account 106CM on the Destination flow at -Own.PMIN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pMin * ICPOwn.pCon),,RegleElim,10409)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)
												Else
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10410)
														
								                    'Genere le compte 106G sur le flux Destination a -Own.PCON du partenaire
													'Trigger account 106G on the Destination flow at -Own.PCON of ICP
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.pCon,,RegleElim,10411)
													
								                    'Genere le compte 106CG sur le flux Destination a Own.PCON du partenaire
													'Trigger account 106CG on the Destination flow at Own.PCON of ICP
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.pCon ,,RegleElim,10412)
													
								                    'Genere le compte 106M sur le flux Destination a Own.PCON du partenaire
													'Trigger account 106M on the Destination flow at Own.PCON of ICP
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.pCon ,,RegleElim,10413)
													
								                    'Genere le compte 106CM sur le flux Destination a -Own.PCON du partenaire
													'Trigger account 106CM on the Destination flow at -Own.PCON of ICP
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.pCon,,RegleElim,10414)
																									
												End If
											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF	
											ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10415)
														
													'----------- Impact chez la vendeuse - Seller-------------------------					                    
								                    'Genere le compte 106CG sur le flux Destination a -Own.POWN * Own.PCON du partenaire
													'Trigger account 106CG on the Destination flow at -Own.POWN * Own.PCON of ICP
								                    Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pOwn * ICPOwn.pCon),,RegleElim,10416)
													
													'Genere le compte 106CM sur le flux Destination a -Own.PMIN * Own.PCON du partenaire
													'Trigger account 106CM on the Destination flow at -Own.PMIN * Own.PCON of ICP
								                    Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pMin * ICPOwn.pCon),,RegleElim,10417)
													
													'Genere le compte 40L sur le flux Destination a -Own.PCON * Own.PCON du partenaire
													'Trigger the account 40L on the Destination flow at -Own.PCON * Own.PCON of ICP
													Cons.Book(cell,plugBilan.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.pCon),,RegleElim,10418)
													
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)
												Else
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10419)
													
								                    'Genere le compte 106CG sur le flux Destination a Own.PCON du partenaire
													'Trigger account 106CG on the Destination flow at Own.PCON of ICP
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.pCon ,,RegleElim,10420)
													
								                    'Genere le compte 106CM sur le flux Destination a -Own.PCON du partenaire
													'Trigger account 106CM on the Destination flow at -Own.PCON of ICP
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.pCon,,RegleElim,10421)
													
												End If
											'Si le flux est ECR
											'If the flow is ECR	
											ElseIf flowMember.MemberId = flow.Ecr.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10422)
														
													'----------- Impact chez la vendeuse -------------------------
								                    'Genere le compte 120CG sur le flux ECF a -Own.POWN * Own.PCON du partenaire
													'Trigger account 120CG on the ECF flow at -Own.POWN * Own.PCON of ICP
													Cons.Book(cell,acct.ResCG.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pOwn * ICPOwn.pCon),,RegleElim,10423)
													
								                    'Genere le compte 120CM sur le flux ECF a -Own.PMIN du partenaire
													'Trigger account 120CM on the ECF flow at -Own.PMIN of ICP
													Cons.Book(cell,acct.ResCM.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pMin * ICPOwn.pCon),,RegleElim,10424)
													
								                    'Genere le compte 40L sur le flux ECF a -Own.PCON * Own.PCON du partenaire
													'Trigger the account 40L on the ECF flow at -Own.PCON * Own.PCON of ICP
													Cons.Book(cell,plugBilan.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.pCon),,RegleElim,10425)
													
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)
												Else
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10426)
														
								                    'Genere le compte 120CG sur le flux ECF a Own.PCON du partenaire
													'Trigger account 120CG on the ECF flow at Own.PCON of ICP
													Cons.Book(cell,acct.ResCG.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.pCon ,,RegleElim,10427)
													
								                    'Genere le compte 120CM sur le flux ECF a -Own.PCON du partenaire
													'Trigger account 120CM on the ECF flow at -Own.PCON of ICP
													Cons.Book(cell,acct.ResCM.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.pCon,,RegleElim,10428)
												End If	
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10429)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10430)
											End If
	
										End If
									'----------------------------------------------------
					                ' PV/MV de cession d'immobilisations intragroupe
									' Gain/Loss on interco sale of fixed assets
					                '----------------------------------------------------	
									ElseIf RegleElim="PMV" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
																					
											Dim CompteImmo As String = Left(acctMember.Name,6)
											Dim CompteImmoDest As Member = api.Members.GetMember(DimType.Account.Id, CompteImmo)
											Dim plugBilan As Member = api.Members.GetMember(DimType.Account.Id, "16L")
											Dim icpOwn As New OwnershipClass(si,api,globals, icName,,,,OpeScenario)
											
											Dim plugResultat As Member = api.Account.GetPlugAccount(cell.DataBufferCellPk.AccountId)
											
											'Si le flux est AUG
											'If the flow is AUG
											If flowMember.MemberId = flow.Aug.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'----------- Impact chez la vendeuse - Seller -------------------------
								                	'Genere le compte 120G sur le flux RES a Own.POWN * Own.PCON du partenaire
													'Trigger account 120G on the RES flow at Own.POWN * Own.PCON of ICP
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pOwn * ICPOwn.pCon),,RegleElim,10431)
													
								                	'Genere le compte 120M sur le flux RES a -Own.PMIN * Own.PCON du partenaire
													'Trigger account 120M on the RES flow at -Own.PMIN * Own.PCON of ICP
													Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pMin * ICPOwn.pCon),,RegleElim,10432)
													
								                	'Genere le compte 16L sur le flux AUG a -Own.PCON * Own.PCON du partenaire
													'Trigger the account 16L on the AUG flow at -Own.PCON * Own.PCON of ICP
													Cons.Book(cell,plugBilan.MemberId,flow.Aug.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.pCon),,RegleElim,10433)
													
								                	'Genere le compte de liaison (Plug Account du compte source) sur le flux None a -Own.PCON * Own.PCON du partenaire et UD7 est l'ICP
													'Trigger the plug account (Plug Account on the source account) on the None flow at -Own.PCON * Own.PCON of ICP and the UD7 is the ICP
													Cons.Book(cell,plugResultat.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.pCon),,RegleElim,10434)
																										
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
														'Eliminate source record for Stage consolidations (at 100%)						
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10435)
													'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON						
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10436)
													End If
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)
												Else
													
								                	'Genere le compte 120G sur le flux RES a Own.PCON du partenaire
													'Trigger account 120G on the RES flow at Own.PCON of ICP
													Cons.Book(cell,acct.ResG.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.pCon ,,RegleElim,10438)
													
								                	'Genere le compte 120M sur le flux RES a -Own.PCON du partenaire
													'Trigger account 120M on the RES flow at -Own.PCON of ICP
													Cons.Book(cell,acct.ResM.MemberId,flow.Res.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.pCon,,RegleElim,10439)
													
												End If
											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF	
											ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
											'Pour les autres flux different de NBV
											'For other flows different from NBV	
'											ElseIf flowMember.MemberId = flow.NBV.MemberId Then  CWe and FSM 2020-05-08 needs to be not equal 
											ElseIf flowMember.MemberId <> flow.NBV.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													'----------- Impact chez la vendeuse - Seller -------------------------
								                	'Genere le compte 106G sur le flux Destination a -Own.POWN * Own.PCON du partenaire
													'Trigger account 106G on the Destination flow at -Own.POWN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pOwn * ICPOwn.pCon),,RegleElim,10441)
													
								                	'Genere le compte 106M sur le flux Destination a -Own.PMIN * Own.PCON du partenaire
													'Trigger account 106M on the Destination flow at -Own.PMIN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pMin * ICPOwn.pCon),,RegleElim,10442)
													
								                	'Genere le compte 16L sur le flux Destination a Own.PCON * Own.PCON du partenaire
													'Trigger the account 16L on the Destination flow at Own.PCON * Own.PCON of ICP
													Cons.Book(cell,plugBilan.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.pCon),,RegleElim,10443)
													
													'Si la methode est Mise en Equivalence ou la nature source est Equity
													'If consolidation method is Equity Pickup or source nature is Equity
													'If method=3 Or NatureSource.Name="Equity" Then
													If method=FSKMethod.MEE Or NatureSource.Name="Equity" Then
														'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
														'Eliminate source record for Stage consolidations (at 100%)						
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10444)
													'Si la methode n'est pas Mise en Equivalence ou la nature n'est pas Equity
													'If the consolidation method is not Equity Pickup or the nature is not Equity	
													Else
														'Elimine la donnee source pour les Consolidations en Paliers a Own.PCON
														'Eliminate source record for Stage consolidations at Own.PCON						
														Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10445)
													End If
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10446)
													
								                	'Genere le compte 106G sur le flux Destination a Own.PCON du partenaire
													'Trigger account 106G on the Destination flow at Own.PCON of ICP
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.pCon ,,RegleElim,10447)
													
								                	'Genere le compte 106M sur le flux Destination a -Own.PCON du partenaire
													'Trigger account 106M on the Destination flow at -Own.PCON of ICP
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.pCon,,RegleElim,10448)
												End If
											'Si le flux est NBV
											'If the flow is NBV	
											Else
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then											     		
													'Si la nature de destination est INTANGGL
													'If the destination nature is INTANGGL
													If NatureDest.Name = "INTANGGL" Then'HC
														Dim CFDISPINT As Member = api.Members.GetMember(DimType.Account.Id, "CFDISP_INTANG")
														Dim CFPURCHINT As Member = api.Members.GetMember(DimType.Account.Id, "CFPURCH_INTANG")
								               	    	
														'Genere le compte CFDISP_INTANG sur le flux None a -Own.PCON * Own.PCON du partenaire
														'Trigger the account CFDISP_INTANG on the None flow at -Own.PCON * Own.PCON of ICP																									
														Cons.Book(cell,CFDISPINT.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.pCon),,RegleElim,10449)
														
													'Si la nature de destination est TANGGL
													'If the destination nature is TANGGL	
													ElseIf NatureDest.Name = "TANGGL" Then
														Dim CFDISPTAN As Member = api.Members.GetMember(DimType.Account.Id, "CFDISP_TANG")
														Dim CFPURCHTAN As Member = api.Members.GetMember(DimType.Account.Id, "CFPURCH_TANG")
														
								               	    	'Genere le compte CFDISP_TANG sur le flux None a -Own.PCON * Own.PCON du partenaire
														'Trigger the account CFDISP_TANG on the None flow at -Own.PCON * Own.PCON of ICP
														Cons.Book(cell,CFDISPTAN.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.pCon),,RegleElim,10450)
														
													'Si la nature de destination est FINGL
													'If the destination nature is FINGL	
													ElseIf NatureDest.Name = "FINGL" Then
														Dim CFDISPFIN As Member = api.Members.GetMember(DimType.Account.Id, "CFDISP_FIN")
														Dim CFPURCHFIN As Member = api.Members.GetMember(DimType.Account.Id, "CFPURCH_FIN")
														
								               	    	'Genere le compte CFDISP_FIN sur le flux None a -Own.PCON * Own.PCON du partenaire
														'Trigger the account CFDISP_FIN on the None flow at -Own.PCON * Own.PCON of ICP
														Cons.Book(cell,CFDISPFIN.MemberId,flow.None.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.pCon),,RegleElim,10451)
														
													End If
												End If
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then											
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10452)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10453)
											End If
										End If
									'----------------------------------------------------
					                ' PV/MV de cession d'immobilisations intragroupe: cumul des ecarts de conversion
									' CTA on Gain/Loss on interco sale of fixed assets
					                '----------------------------------------------------	
									ElseIf RegleElim="PMVH" Then
										Call cons.PElimPartnerTable(si,api,icMember)
										'Si la methode du partenaire est 1,2,3,4,5
										'If ICP consolidation method is 1,2,3,4,5
										If icMethod12345 Then
											
											Dim CompteImmo As String = Left(acctMember.Name,6)
											Dim CompteImmoDest As Member = api.Members.GetMember(DimType.Account.Id, CompteImmo)
											Dim plugBilan As Member = api.Members.GetMember(DimType.Account.Id, "16L")
											Dim icpOwn As New OwnershipClass(si,api,globals, icName,,,,OpeScenario)
											
											'Si le flux est OUV/VARC/SOR
											'If flow is OUV/VARC/SOR
											If flowMember.MemberId = flow.Ouv.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.Sor.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													
													'----------- Impact chez la vendeuse - Seller -------------------------
								                    'Genere le compte 106CG sur le flux Destination a -Own.POWN * Own.PCON du partenaire
													'Trigger account 106CG on the Destination flow at -Own.POWN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pOwn * ICPOwn.pCon),,RegleElim,10455)
													
								                    'Genere le compte 106CM sur le flux Destination a -Own.PMIN * Own.PCON du partenaire
													'Trigger account 106CM on the Destination flow at -Own.PMIN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pMin * ICPOwn.pCon),,RegleElim,10456)
													
													'Genere le compte 106G sur le flux Destination a Own.POWN * Own.PCON du partenaire
													'Trigger account 106G on the Destination flow at Own.POWN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pOwn * ICPOwn.pCon),,RegleElim,10457)
													
								                    'Genere le compte 106M sur le flux Destination a Own.PMIN * Own.PCON du partenaire
													'Trigger account 106M on the Destination flow at Own.PMIN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pMin * ICPOwn.pCon),,RegleElim,10458)
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)
												Else
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10459)
													
								                    'Genere le compte 106CG sur le flux Destination a Own.PCON du partenaire
													'Trigger account 106CG on the Destination flow at Own.PCON of ICP
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.pCon ,,RegleElim,10460)
													
								                    'Genere le compte 106CM sur le flux Destination a -Own.PCON du partenaire
													'Trigger account 106CM on the Destination flow at -Own.PCON of ICP
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.pCon,,RegleElim,10461)
													
													'Genere le compte 106G sur le flux Destination a -Own.PCON du partenaire
													'Trigger account 106G on the Destination flow at -Own.PCON of ICP
													Cons.Book(cell,acct.RsvG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.pCon,,RegleElim,10462)
													
								                    'Genere le compte 106M sur le flux Destination a Own.PCON du partenaire
													'Trigger account 106M on the Destination flow at Own.PCON of ICP
													Cons.Book(cell,acct.RsvM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.pCon ,,RegleElim,10463)
												End If
											'Si le flux est ECO ou ECF
											'If flows is ECO or ECF	
											ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													
													'----------- Impact chez la vendeuse - Seller -------------------------
								                   	'Genere le compte 106CG sur le flux Destination a -Own.POWN * Own.PCON du partenaire
													'Trigger account 106CG on the Destination flow at -Own.POWN * Own.PCON of ICP
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pOwn * ICPOwn.pCon),,RegleElim,10465)
													
								                   	'Genere le compte 106CM sur le flux Destination a -Own.PMIN * Own.PCON du partenaire
													'Trigger account 106CM on the Destination flow at -Own.PMIN of ICP
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pMin * ICPOwn.pCon),,RegleElim,10466)
													
								                   	'Genere le compte 16L sur le flux Destination a -Own.PCON * Own.PCON du partenaire et UD7 est l'ICP
													'Trigger the account 16L on the Destination flow at -Own.PCON * Own.PCON of ICP and the UD7 is the ICP
													Cons.Book(cell,plugBilan.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.pCon),,RegleElim,10467)

												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)												
												Else
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10468)
													
								                   	'Genere le compte 106CG sur le flux Destination a Own.PCON du partenaire
													'Trigger account 106CG on the Destination flow at Own.PCON of ICP
													Cons.Book(cell,acct.RsvCG.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.pCon ,,RegleElim,10469)
													
								                   	'Genere le compte 106CM sur le flux Destination a -Own.PCON du partenaire
													'Trigger account 106CM on the Destination flow at -Own.PCON of ICP
													Cons.Book(cell,acct.RsvCM.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.pCon,,RegleElim,10470)
												End If
											'Si le flux est ECR
											'If the flow is ECR
											ElseIf flowMember.MemberId = flow.Ecr.MemberId Then
												'Si la nature (NatureDim) n'est pas Minoritaires
												'If nature (NatureDim) is not Minoritaires
												If NatureSource.Name<>"Minoritaires" Then
													
													'----------- Impact chez la vendeuse - Seller -------------------------
								                   	'Genere le compte 120CG sur le flux ECF a -Own.POWN * Own.PCON du partenaire
													'Trigger account 120CG on the ECF flow at -Own.POWN * Own.PCON of ICP
													Cons.Book(cell,acct.ResCG.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pOwn * ICPOwn.pCon),,RegleElim,10472)
													
								                   	'Genere le compte 120CM sur le flux ECF a -Own.PMIN * Own.PCON du partenaire
													'Trigger account 120CM on the ECF flow at -Own.PMIN * Own.PCON of ICP
													Cons.Book(cell,acct.ResCM.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pMin * ICPOwn.pCon),,RegleElim,10473)
													
								                   	'Genere le compte 16L sur le flux ECF a -Own.PCON * Own.PCON du partenaire et UD7 est l'ICP
													'Trigger the account 16L on the ECF flow at -Own.PCON * Own.PCON of ICP and the UD7 is the ICP
													Cons.Book(cell,plugBilan.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.pCon),,RegleElim,10474)
																									
												'Pour la Nature Minoritaires (NatureDim)
												'If Nature is Minoritaires (NatureDim)	
												Else
													'Elimine la donnee source pour les Consolidations en Paliers (a 100%)
													'Eliminate source record for Stage consolidations (at 100%)						
													Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,RegleElim,10475)
													
								                   	'Genere le compte 120CG sur le flux ECF a Own.PCON du partenaire
													'Trigger account 120CG on the ECF flow at Own.PCON of ICP
													Cons.Book(cell,acct.ResCG.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.pCon ,,RegleElim,10476)
													
								                   	'Genere le compte 120CM sur le flux ECF a -Own.PCON du partenaire
													'Trigger account 120CM on the ECF flow at -Own.PCON of ICP
													Cons.Book(cell,acct.ResCM.MemberId,flow.ECF.MemberId,,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.pCon,,RegleElim,10477)
												End If	
											End If
										'Si le partenaire n'est pas sous le parent et est different de None
										'If the ICP is not under the current parent and is different from None	
										ElseIf icMember.MemberID<>DimConstants.None Then											
											'Donnees techniques pour les paliers, store la portion minoritaire sur la Nature Minoritaires
											'Technical data for stage hierarchies, archive the minority portion on the Minoritaires Nature
											Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimMinos.MemberId,,,,,Own.pMin ,,RegleElim,10478)
											
											'Si la methode de consolidation est Mise en Equivalence
											'If consolidation method is Equity Pickup
											'If method=3 Then
											If method=FSKMethod.MEE Then
												'Store le montant proportionalise sur la nature Equity pour declencher l'elimination ulterieurement
												'Archive the proportionalized amount on nature Equity to trigger the elimination later
												Cons.Book(cell,acctMember.MemberId,flowDestMember.MemberId,,DimConstants.Elimination,,,,NatureDimEquity.MemberId,,,,,-(1 - Own.pCon),,RegleElim,10479)
											End If
										End If	
									End If	'RegleElim=
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
									
									'Si la methode de consolidation n'est pas Sortie
									'If the consolidation method is not Exit
									'If method<>6 Then
'									If method<>FSKMethod.SORTIE Then
									If scope<>FSKScope.SORTIE Then
										'Pour les compte 120G/120M/120CG/120CM
										'For accounts 120G/120M/120CG/120CM
'										If acctMember.Name="120G" Or acctMember.Name="120M" Or acctMember.Name="120CG" Or acctMember.Name="120CM" Then
										If acctMember.MemberId=Acct.ResG.MemberId Or acctMember.MemberId=Acct.ResM.MemberId Or _
										acctMember.MemberId=Acct.ResCG.MemberId Or acctMember.MemberId=Acct.ResCM.MemberId Then
											
											'Dim AutoAdj As Member = nat.AutoAdj 'api.Members.GetMember(DimType.ud4.Id, "AutoAdj")
											'Dim ManAdj As Member =  nat.ManAdj 'api.Members.GetMember(DimType.ud4.Id, "ManAdj")
											Dim isNatureBaseAutoAdj = api.Members.IsDescendant(Nat.NatDimPK, Nat.AutoAdj.MemberId, cell.DataBufferCellPk.item(nat.NatDimType.Id), Nothing)
											Dim isNatureBaseManAdj = api.Members.IsDescendant(Nat.NatDimPK, Nat.ManAdj.MemberId, cell.DataBufferCellPk.item(nat.NatDimType.Id), Nothing)
											'Dim isNatureBaseAutoAdj = api.Members.IsDescendant(Nat.NatDimPK, Nat.RetSoc.MemberId, cell.DataBufferCellPk.item(nat.NatDimType.Id), Nothing)
											'Dim isNatureBaseManAdj = api.Members.IsDescendant(Nat.NatDimPK, Nat.RetCon.MemberId, cell.DataBufferCellPk.item(nat.NatDimType.Id), Nothing)
											
											'Si la nature appartient RETAUTO/RETMAN ou CAP (NatureDim)
											'If the nature is base of RETAUTO/RETMAN or CAP (NatureDim)
											If isNatureBaseAutoAdj Or isNatureBaseManAdj Or NatureSource.Name="CAP" Then 'HC
											'If isNatureBaseAutoAdj Or isNatureBaseManAdj Or NatureSource.Name="CAPI" Or NatureSource.Name="RESE" Or NatureSource.Name="UTIL" Or NatureSource.Name="OTRO" Then 'HC
												'Genere le flux VARC a -100%
												'Trigger the VARC flow at -100%
												Cons.Book(cell,,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10480)
												'Cons.Book(cell,acctMember.MemberId,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10480)
											'Pour les autres natures (NatureDim)
											'For other natures (NatureDim)	 
											Else
												'Genere le flux AFF a -100%
												'Trigger the AFF flow at -100% 
		                    					cons.Book(cell,,flow.Aff.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10481)
		                    					'cons.Book(cell,acctMember.MemberId,flow.Aff.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10481)
												
												'Si le compte est 120G
												'If the account is 120G
												If acctMember.MemberId=Acct.ResG.MemberId Then '"120G" Then
													'Genere le compte 106G flux AFF a 100%
													'Trigger the account 106G AFF flow at 100%
													Cons.Book(cell,acct.RsvG.MemberId,flow.Aff.MemberId,,DimConstants.Elimination,,,,,,,,,1,,sPPRule,10482)
													
			                						'Genere le compte 106G flux VARC a -100%
													'Trigger the account 106G VARC flow at -100%
													Cons.Book(cell,acct.RsvG.MemberId,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10483)
												'Si le compte est 120M
												'If the account is 120M	
												ElseIf acctMember.MemberId=Acct.ResM.MemberId Then
													'Genere le compte 106M flux AFF a 100%
													'Trigger the account 106M AFF flow at 100%
													Cons.Book(cell,acct.RsvM.MemberId,flow.Aff.MemberId,,DimConstants.Elimination,,,,,,,,,1,,sPPRule ,10484)
													
			                						'Genere le compte 106M flux VARC a -100%
													'Trigger the account 106M VARC flow at -100%
													Cons.Book(cell,acct.RsvM.MemberId,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10485)
												'Si le compte est 120CG
												'If the account is 120CG	
												ElseIf acctMember.MemberId=Acct.ResCG.MemberId Then
													'Genere le compte 106CG flux AFF a 100%
													'Trigger the account 106CG AFF flow at 100%
													Cons.Book(cell,acct.RsvCG.MemberId,flow.Aff.MemberId,,DimConstants.Elimination,,,,,,,,,1,,sPPRule ,10486)
													
			                						'Genere le compte 106CG flux VARC a -100%
													'Trigger the account 106CG VARC flow at -100%
													Cons.Book(cell,acct.RsvCG.MemberId,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10487)
												'Si le compte est 120CM
												'If the account is 120CM	
												ElseIf acctMember.MemberId=Acct.ResCM.MemberId Then
													'Genere le compte 106CM flux AFF a 100%
													'Trigger the account 106CM AFF flow at 100%
													Cons.Book(cell,acct.RsvCM.MemberId,flow.Aff.MemberId,,DimConstants.Elimination,,,,,,,,,1,,sPPRule,10488)
													
			                						'Genere le compte 106CM flux VARC a -100%
													'Trigger the account 106CM VARC flow at -100%
													Cons.Book(cell,acct.RsvCM.MemberId,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10489)
													
												End If	
													
											End If	
										'Pour les autres comptes (autre que 120G/120M/120CG/120CM)
										'For other accounts (not 120G/120M/120CG/120CM)
										Else
											'Genere le flux VARC a -100%
											'Trigger the VARC flow at -100%
											cons.Book(cell,,flow.VarO.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10490)
										End If
									'Si la methode de consolidation est Sortie
									'If the consolidation method is EXIT	
									Else
										'Genere le flux OUV a -100%
										'Trigger the OUV flow at -100%
										cons.Book(cell,,flow.Ouv.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10491)
										
										'Genere le flux SOR a -100%
										'Trigger the SOR flow at -100%
										cons.Book(cell,,flow.Sor.MemberId,,DimConstants.Elimination,,,,,,,,,-1,,sPPRule,10492)
									End If	
								End If	'CellStatus is Not NoData
							Next
						End If	'If Not sourceDataBuffer2 Is Nothing Then

						api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
						cons.WriteLogToTable
						
					End If 'If Not sourceDataBuffer Is Nothing Then
'FS 20200626 Disabled lines for accounts FLUXDIVX
'					api.Data.ClearCalculatedData("A#FLUXDIVR:I#None",True,True,True)'HC
'					api.Data.ClearCalculatedData("A#FLUXDIVREH:I#None",True,True,True)
'					api.Data.ClearCalculatedData("A#FLUXADIVR:I#None",True,True,True)
'					api.Data.ClearCalculatedData("A#FLUXADIVREH:I#None",True,True,True)
'					api.Data.ClearCalculatedData("I#None:" & nat.NatDimType.Abbrev & "#" & Nat.Minoritaires.Name,True,True,True)
'					api.Data.ClearCalculatedData("I#None:" & nat.NatDimType.Abbrev & "#" & Nat.Equity.Name,True,True,True)
			
		    Catch ex As Exception
		        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		    End Try
		End Sub
#End Region

#Region "Second Pass Elimnations"

		Private Sub CalculateElimP2(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			Try
				
				If (api.Pov.Cons.MemberId = DimConstants.ConsElimination) And (args.CalculateArgs.IsSecondPassEliminationCalc) Then
					Dim GlobalClass As New GlobalClass(si,api,args)
					Dim Opescenario As String = GlobalClass.OpeScenario
					Dim Entity As String = api.Pov.Entity.Name
					Dim EntityMember As Member = api.Pov.Entity
					Dim resultDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("C#Elimination")
'					resultDataBuffer.LogDataBuffer(api,"Start", 1000)
					Dim PCons As New ConsClass(si, globals, api, resultDataBuffer, bDoLog, False) 'The object for the book method
					
					Dim Acct As AccountClass = Me.InitAccountClass(si, api, globals)										
					Dim Flow As FlowClass = Me.InitFlowClass(si, api, globals)
					Dim Nat As NatureClass = Me.InitNatureClass(si, api, globals, api.Pov.UD4Dim.DimPk)

					Dim Period As String = api.Pov.Time.Name
					Dim icNoneMember As member = api.Members.GetMember(DimType.Entity.Id, DimConstants.None)
					
					Dim parentId As Integer = api.Pov.Parent.MemberId
					Dim childEntities As List(Of Member) = PCons.pullICPelimMembers(si,api)				
				
					Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
					
					If Not childEntities Is Nothing Then
						For Each childEntity In childEntities
							
							If api.Members.IsChild(api.Pov.EntityDim.DimPk, api.Pov.Parent.MemberId,childentity.MemberId) Then
								pcons.sourceEntity = childEntity
								
	                            Dim parentcubeid As Integer = api.Cubes.GetCubeInfo().ParentCubeId
								
								If api.Cubes.GetCubeInfo(parentcubeid) Is Nothing Then parentcubeid = api.Pov.Cube.CubeId
								
								Dim childDataBuffer As Databuffer = api.Data.GetDataBufferForCustomShareCalculation(parentcubeid, childEntity.Memberid,,,,)
																		
								If Not childDataBuffer Is Nothing Then
									
									For Each sourceCell As DataBufferCell In childDataBuffer.DataBufferCells.Values
										
										Dim SourceEntityName As String = childEntity.Name
										Dim SourceEntityId As Integer = childEntity.MemberId
										Dim accountId As Integer = sourcecell.DataBufferCellPk.AccountId
										Dim SourceicId As Integer = sourcecell.DataBufferCellPk.ICId
										Dim acctMember As Member = api.Members.GetMember(DimType.Account.Id, sourcecell.DataBufferCellPk.AccountId)
										Dim icMember As Member = api.Members.GetMember(DimType.IC.Id, sourcecell.DataBufferCellPk.ICId)
										Dim icName As String = icMember.Name
										Dim NatureSource As Member = Nat.getMember(sourceCell.DataBufferCellPk.Item(nat.NatDimType.Id))'api.Members.GetMember(DimType.UD4.Id, sourcecell.DataBufferCellPk.UD4Id)

										Dim icMethod As Integer = OwnershipClass.GetOwnershipInfoByName(si, api, globals, "method",icName)
										Dim icScope As Integer = OwnershipClass.GetOwnershipInfoByName(si, api, globals, "method",icName)
										Dim icMethod1245 As Boolean = False
										Dim icMethod12345 As Boolean = False
										Dim icRef As String
										icRef=childEntity.Name
																						
										Dim isICPartnerADescendantOfParentEnt = api.Members.IsDescendant(api.Pov.EntityDim.DimPk, api.Pov.Parent.MemberId, sourcecell.DataBufferCellPk.ICId, Nothing)
											
										If isICPartnerADescendantOfParentEnt = True Then

											If icMethod = FSKMethod.HOLDING Or icMethod = FSKMethod.IG Or icMethod = 4 Or icScope = FSKScope.SORTANTE Then
												icMethod1245 = True
												icMethod12345 = True
											ElseIf icMethod	= FSKMethod.MEE Then
												icMethod12345 = True
											End If	
										End If
										
										Dim flowMember As Member = api.Members.GetMember(DimType.flow.Id, sourcecell.DataBufferCellPk.FlowId)
										Dim flowDestMember As Member
									
										If flowMember.MemberId = flow.Ouv.MemberId Then
											flowDestMember = flow.VarC
										Else
											flowDestMember = flowMember
										End If
										
										Dim text3Acct As String = api.Account.Text(accountId, 3)
										Dim RegleElim As String							
										Dim UndSco As Integer = InStr(text3Acct, "_")
			        					If UndSco > 0 Then RegleElim = Left(text3Acct, UndSco - 1) Else RegleElim = ""
										
										Dim Nature As String
			        					If NatureSource.Name = "Pack" Then 
											Nature = "Pack" 
										ElseIf Len(text3Acct) = UndSco Then 
											Nature = "None" 
										Else Nature = Right(text3Acct, Len(text3Acct) - UndSco)	
										End If
										Dim NatureDest As Member = nat.getMember(Nature)'api.Members.GetMember(DimType.UD4.Id, Nature)
						
										'Get the percent consolidation.
										
										Dim Own As New OwnershipClass(si,api,globals, ChildEntity.Name, api.Pov.Parent.Name,,, OpeScenario)
										
										Dim isICPartnerADescendantOfCurrentEnt = api.Members.IsDescendant(api.Pov.EntityDim.DimPk, api.Pov.Entity.MemberId, sourcecell.DataBufferCellPk.ICId, Nothing)
										
										If icName=api.Pov.Entity.Name Or isICPartnerADescendantOfCurrentEnt = True Then

											'----------------------------------------------------
							                ' Fondo de Comercio de Consolidación (con impacto en reservas de la participada)
											' Goodwill (impact the subsidiary entity reserves)
							                '----------------------------------------------------	
											If RegleElim="GW" Then
												
												Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)
											
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then
													
													'Si le flux est AUG
													'If flow is AUG
													If flowMember.MemberId = flow.Aug.MemberId  Then
														'Si le partenaire n'est pas consolide en N-1
														'If the ICP is not consolidated in N-1
														If ICPOwn.PCon_1 = 0 Then
															'Si la nature (NatureDim) n'est pas Minoritaires
															'If nature (NatureDim) is not Minoritaires
															If NatureSource.Name<>"Minoritaires" Then																										
					                        					'Genere le compte 18LP sur le flux ENT a -Own.PCON
																'Trigger account 18LP on the ENT flow at -Own.PCON
																PCons.Book(sourcecell,acct.LPTIT.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10493)
																
																'Si la methode du partenaire est Holding
																'If the consolidation of ICP is Holding
																'If icMethod = 1 Then
																If icMethod = FSKMethod.HOLDING Then
																	'Genere le compte 106A sur le flux ENT a -Own.PCON
																	'Trigger account 106A on the ENT flow at -Own.PCON
																	PCons.Book(sourcecell,acct.AutoCtrl.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10494)
																'Pour les autres methodes
																'For other consolidation methods	
										                        Else
										                            'Genere le compte 106G sur le flux ENT a -Own.POWN
																	'Trigger account 106G on the ENT flow at -Own.POWN
																	PCons.Book(sourcecell,acct.RsvG.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10495)
																	
										                            'Genere le compte 106M sur le flux ENT a -Own.PMIN
																	'Trigger account 106M on the ENT flow at -Own.PMIN
																	PCons.Book(sourcecell,acct.RsvM.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,RegleElim,10496)
																	
										                        End If 'If ICPMethod=MethodHolding
															'ElseIf icMethod <> 1 Then													
															ElseIf icMethod <> FSKMethod.HOLDING Then													
				                        						'Genere le compte 106G sur le flux VARC a 100%
																'Trigger account 106G on the VARC flow at 100%
																PCons.Book(sourcecell,acct.RsvG.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,RegleElim,10497)
																
				                        						'Genere le compte 106M sur le flux VARC a -100%
																'Trigger account 106M on the VARC flow at -100%
																PCons.Book(sourcecell,acct.RsvM.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,RegleElim,10498)
															End If	'If nature (NatureDim) is not Minoritaires
														'Si le partenaire est consolide en N-1
														'If the ICP is consolidated in N-1	
														Else
															'Si la nature (NatureDim) n'est pas Minoritaires
															'If nature (NatureDim) is not Minoritaires
															If NatureSource.Name<>"Minoritaires" Then												
																
					                        					'Genere le compte 18LP sur le flux source a -Own.PCON
																'Trigger account 18LP on the source flow at -Own.PCON
																PCons.Book(sourcecell,acct.LPTIT.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10499)

																'Si la methode du partenaire est Holding
																'If the ICP consolidation method is Holding
										                        'If icMethod = 1 Then
										                        If icMethod = FSKMethod.HOLDING Then
																	
										                            'Genere le compte 106A sur le flux VARC a -Own.PCON
																	'Trigger account 106A on the VARC flow at -Own.PCON
																	PCons.Book(sourcecell,acct.AutoCtrl.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10500)
																'Pour les autres methodes
																'For other consolidation methods	
										                        Else
										                            'Genere le compte 106G sur le flux VARC a -Own.POWN
																	'Trigger account 106G on the VARC flow at -Own.POWN
																	PCons.Book(sourcecell,acct.RsvG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10501)
																	
										                            'Genere le compte 106M sur le flux VARC a -Own.PMIN
																	'Trigger account 106M on the VARC flow at -Own.PMIN
																	PCons.Book(sourcecell,acct.RsvM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,RegleElim,10502)
										                        End If 'If ICPMethod=MethodHolding

															'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
															'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding	
															'ElseIf icMethod <> 1 Then	
															ElseIf icMethod <> FSKMethod.HOLDING Then	
																
																'Genere le compte 106G sur le flux VARC a 100%
																'Trigger account 106G on the VARC flow at 100%
																PCons.Book(sourcecell,acct.RsvG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,RegleElim,10503)
																
				                        						'Genere le compte 106M sur le flux VARC a -100%
																'Trigger account 106M on the VARC flow at -100%
																PCons.Book(sourcecell,acct.RsvM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,RegleElim,10504)
															End If	'If nature (NatureDim) is not Minoritaires
														End If	'If the ICP is not consolidated in N-1
													'Si le flux et DIM
													'If flow is DIM
													ElseIf flowMember.MemberId = flow.DIMI.MemberId  Then
														'Si le partenaire n'est pas consolide en N-1
														'If the ICP is not consolidated in N-1
														If ICPOwn.PCon_1 = 0 Then
															'Si la nature (NatureDim) n'est pas Minoritaires
															'If nature (NatureDim) is not Minoritaires
															If NatureSource.Name<>"Minoritaires" Then																										
					                        					'Genere le compte 18LP sur le flux ENT a -Own.PCON
																'Trigger account 18LP on the ENT flow at -Own.PCON
																PCons.Book(sourcecell,acct.LPTIT.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10505)
																
																'Si la methode du partenaire est Holding
																'If the consolidation of ICP is Holding
																'If icMethod = 1 Then
																If icMethod = FSKMethod.HOLDING Then
																	'Genere le compte 106A sur le flux ENT a -Own.PCON
																	'Trigger account 106A on the ENT flow at -Own.PCON
																	PCons.Book(sourcecell,acct.AutoCtrl.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10506)
																'Pour les autres methodes
																'For other consolidation methods	
										                        Else
										                            'Genere le compte 106G sur le flux ENT a -Own.POWN
																	'Trigger account 106G on the ENT flow at -Own.POWN
																	PCons.Book(sourcecell,acct.RsvG.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10507)
																	
										                            'Genere le compte 106M sur le flux ENT a -Own.PMIN
																	'Trigger account 106M on the ENT flow at -Own.PMIN
																	PCons.Book(sourcecell,acct.RsvM.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10508)
																	
										                        End If 'If ICPMethod=MethodHolding
															'ElseIf icMethod <> 1 Then													
															ElseIf icMethod <> FSKMethod.HOLDING Then													
				                        						'Genere le compte 106G sur le flux VARC a 100%
																'Trigger account 106G on the VARC flow at 100%
																PCons.Book(sourcecell,acct.RsvG.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10509)
																
				                        						'Genere le compte 106M sur le flux VARC a -100%
																'Trigger account 106M on the VARC flow at -100%
																PCons.Book(sourcecell,acct.RsvM.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10510)
															End If	'If nature (NatureDim) is not Minoritaires
														'Si le partenaire est consolide en N-1
														'If the ICP is consolidated in N-1	
														Else
															'Si la nature (NatureDim) n'est pas Minoritaires
															'If nature (NatureDim) is not Minoritaires
															If NatureSource.Name<>"Minoritaires" Then												
																
					                        					'Genere le compte 18LP sur le flux source a -Own.PCON
																'Trigger account 18LP on the source flow at -Own.PCON
																PCons.Book(sourcecell,acct.LPTIT.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10511)

																'Si la methode du partenaire est Holding
																'If the ICP consolidation method is Holding
										                        'If icMethod = 1 Then
										                        If icMethod = FSKMethod.HOLDING Then
																	
										                            'Genere le compte 106A sur le flux VARC a -Own.PCON
																	'Trigger account 106A on the VARC flow at -Own.PCON
																	PCons.Book(sourcecell,acct.AutoCtrl.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10512)
																'Pour les autres methodes
																'For other consolidation methods	
										                        Else
										                            'Genere le compte 106G sur le flux VARC a -Own.POWN
																	'Trigger account 106G on the VARC flow at -Own.POWN
																	PCons.Book(sourcecell,acct.RsvG.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10513)
																	
										                            'Genere le compte 106M sur le flux VARC a -Own.PMIN
																	'Trigger account 106M on the VARC flow at -Own.PMIN
																	PCons.Book(sourcecell,acct.RsvM.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10514)
										                        End If 'If ICPMethod=MethodHolding

															'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
															'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding	
															'ElseIf icMethod <> 1 Then	
															ElseIf icMethod <> FSKMethod.HOLDING Then	
																
																'Genere le compte 106G sur le flux VARC a 100%
																'Trigger account 106G on the VARC flow at 100%
																PCons.Book(sourcecell,acct.RsvG.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10515)
																
				                        						'Genere le compte 106M sur le flux VARC a -100%
																'Trigger account 106M on the VARC flow at -100%
																PCons.Book(sourcecell,acct.RsvM.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10516)
															End If	'If nature (NatureDim) is not Minoritaires
														End If	'If the ICP is not consolidated in N-1
													
													'Si le flux est AUGCAP
													'If the flow is AUGCAP	
													ElseIf flowMember.MemberId = flow.AUGCAP.MemberId Then
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															
				                        					'Genere le compte 18LP sur le flux AUG a -Own.PCON
															'Trigger account 18LP on the AUG flow at -Own.PCON
															PCons.Book(sourcecell,acct.LPTIT.MemberId,flow.Aug.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10517)

															'Si la methode du partenaire est Holding
															'If the consolidation method of the ICP is Holding
									                        'If icMethod = 1 Then
									                        If icMethod = FSKMethod.HOLDING Then
									                            'Genere le compte 106A sur le flux AUG a -Own.PCON
																'Trigger account 106A on the AUG flow at -Own.PCON
																PCons.Book(sourcecell,acct.AutoCtrl.MemberId,flow.Aug.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10518)
															'Pour les autres methodes
															'For other consolidation methods	
									                        Else
									                            'Genere le compte 106G sur le flux AUG a -Own.POWN
																'Trigger account 106G on the AUG flow at -Own.POWN
																PCons.Book(sourcecell,acct.RsvG.MemberId,flow.Aug.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10519)
																
									                            'Genere le compte 106M sur le flux AUG a -Own.PMIN
																'Trigger account 106M on the AUG flow at -Own.PMIN
																PCons.Book(sourcecell,acct.RsvM.MemberId,flow.Aug.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,RegleElim,10520)
									                        End If 'If ICPMethod=MethodHolding	
															

														'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
														'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding		
														'ElseIf icMethod <> 1 Then	
														ElseIf icMethod <> FSKMethod.HOLDING Then	
																
			                        						'Genere le compte 106G sur le flux AUG a 100%
															'Trigger account 106G on the AUG flow at 100%
															PCons.Book(sourcecell,acct.RsvG.MemberId,flow.Aug.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,RegleElim,10521)
															
			                        						'Genere le compte 106M sur le flux AUG a -100%
															'Trigger account 106M on the AUG flow at -100%
															PCons.Book(sourcecell,acct.RsvM.MemberId,flow.Aug.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,RegleElim,10522)
														End If	'If nature (NatureDim) is not Minoritaires
													'Si le flux est DIMCAP
													'If the flow is DIMCAP	
													ElseIf flowMember.MemberId = flow.DIMCAP.MemberId Then
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															
				                        					'Genere le compte 18LP sur le flux DIM a -Own.PCON
															'Trigger account 18LP on the DIM flow at -Own.PCON
															PCons.Book(sourcecell,acct.LPTIT.MemberId,flow.Di.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10523)

															'Si la methode du partenaire est Holding
															'If the consolidation method of the ICP is Holding
									                        'If icMethod = 1 Then
									                        If icMethod = FSKMethod.HOLDING Then
									                            'Genere le compte 106A sur le flux DIM a -Own.PCON
																'Trigger account 106A on the DIM flow at -Own.PCON
																PCons.Book(sourcecell,acct.AutoCtrl.MemberId,flow.Di.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10524)
															'Pour les autres methodes
															'For other consolidation methods
									                        Else
									                            'Genere le compte 106G sur le flux DIM a -Own.POWN
																'Trigger account 106G on the DIM flow at -Own.POWN
																PCons.Book(sourcecell,acct.RsvG.MemberId,flow.Di.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10525)
																
									                            'Genere le compte 106M sur le flux DIM a -Own.PMIN
																'Trigger account 106M on the DIM flow at -Own.PMIN
																PCons.Book(sourcecell,acct.RsvM.MemberId,flow.Di.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,RegleElim,10526)
									                        End If 'If ICPMethod=MethodHolding
															
														'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
														'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding	
														'ElseIf icMethod <> 1 Then	
														ElseIf icMethod <> FSKMethod.HOLDING Then	
																
			                        						'Genere le compte 106G sur le flux DIM a 100%
															'Trigger account 106G on the DIM flow at 100%
															PCons.Book(sourcecell,acct.RsvG.MemberId,flow.Di.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,RegleElim,10527)
															
			                        						'Genere le compte 106M sur le flux DIM a -Own.PMIN
															'Trigger account 106M on the DIM flow at -Own.PMIN
															PCons.Book(sourcecell,acct.RsvM.MemberId,flow.Di.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,RegleElim,10528)
														End If	'If nature (NatureDim) is not Minoritaires
													'Si le flux est ECO ou ECF
													'If flows is ECO or ECF
													ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															
				                        					'Genere le compte 18LP sur le flux Destination a -Own.PCON
															'Trigger account 18LP on the Destination flow at -Own.PCON
															PCons.Book(sourcecell,acct.LPTIT.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10529)

															'Si la methode du partenaire est Holding
															'If the consolidation method of the ICP is Holding
									                        'If icMethod = 1 Then
									                        If icMethod = FSKMethod.HOLDING Then
									                            'Genere le compte 106CG sur le flux Destination a -Own.PCON
																'Trigger account 106CG on the Destination flow at -Own.PCON
																PCons.Book(sourcecell,acct.RsvCG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10530)
															'Pour les autres methodes
															'For other consolidation methods
									                        Else
									                            'Genere le compte 106CG sur le flux Destination a -Own.POWN
																'Trigger account 106CG on the Destination flow at -Own.POWN
																PCons.Book(sourcecell,acct.RsvCG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10531)
																
									                            'Genere le compte 106CM sur le flux Destination a -Own.PMIN
																'Trigger account 106CM on the Destination flow at -Own.PMIN
																PCons.Book(sourcecell,acct.RsvCM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,RegleElim,10532)
									                        End If 'If ICPMethod=MethodHolding
															
														'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
														'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding															
														'ElseIf icMethod <> 1 Then
														ElseIf icMethod <> FSKMethod.HOLDING Then
																
			                        						'Genere le compte 106CG sur le flux Destination a 100%
															'Trigger account 106CG on the Destination flow at 100%
															PCons.Book(sourcecell,acct.RsvCG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,RegleElim,10533)
															
			                        						'Genere le compte 106CM sur le flux Destination a -100%
															'Trigger account 106CM on the Destination flow at -100%
															PCons.Book(sourcecell,acct.RsvCM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,RegleElim,10534)
														End If	'If nature (NatureDim) is not Minoritaires
													
													'Pour les autres flux
													'For other flows	
													Else
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															
				                        					'Genere le compte 18LP sur le flux Destination a -Own.PCON
															'Trigger account 18LP on the Destination flow at -Own.PCON
															PCons.Book(sourcecell,acct.LPTIT.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10535)

															'Si la methode du partenaire est Holding
															'If the ICP consolidation  method is Holding
									                        'If icMethod = 1 Then
									                        If icMethod = FSKMethod.HOLDING Then
									                            'Genere le compte 106A sur le flux Destination a -Own.PCON
																'Trigger account 106A on the Destination flow at -Own.PCON
																PCons.Book(sourcecell,acct.AutoCtrl.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon,,RegleElim,10536)
															'Pour les autres methodes
															'For other consolidation methods	
															ElseIf icMethod <> FSKMethod.MEE Then  
									                            'Genere le compte 106G sur le flux Destination a -Own.POWN
																'Trigger account 106G on the Destination flow at -Own.POWN
																PCons.Book(sourcecell,acct.RsvG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10537)
																
									                            'Genere le compte 106M sur le flux Destination a -Own.PMIN
																'Trigger account 106M on the Destination flow at -Own.PMIN
																PCons.Book(sourcecell,acct.RsvM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin,,RegleElim,10538)
									                        Else
																PCons.Book(sourcecell,acct.RsvEq.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn,,RegleElim,10537)
																
															End If 'If ICPMethod=MethodHolding
															
														'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
														'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding															
														'ElseIf icMethod <> 1 Then	
														ElseIf icMethod <> FSKMethod.HOLDING Then	
																
			                        						'Genere le compte 106G sur le flux Destination a 100%
															'Trigger account 106G on the Destination flow at 100%
															PCons.Book(sourcecell,acct.RsvG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1 ,,RegleElim,10539)
															
			                        						'Genere le compte 106M sur le flux Destination a -100%
															'Trigger account 106M on the Destination flow at -100%
															PCons.Book(sourcecell,acct.RsvM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1,,RegleElim,10540)
														End If	'If nature (NatureDim) is not Minoritaires
														
													End If	'Test on Flows
												End If	'If ICPMethod = 12345 
											
											
											ElseIf RegleElim="TITRES" Then
											
			'	brapi.errorlog.LogMessage(si, "Entity p2 " & api.Pov.Entity.Name & "IC p2 " & api.Pov.IC.Name)	
											
												Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)
											
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then
													
													'Si le flux est AUG
													'If flow is INC y DIS
													If flowMember.MemberId = flow.Aug.MemberId Or flowMember.MemberId = flow.Dimi.MemberId  Then
														If ICPOwn.PCon_1 = 0 Then
															If NatureSource.Name<>"Minoritaires" Then																										
																PCons.Book(sourcecell,acct.LPTIT.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10493)
																If icMethod = FSKMethod.HOLDING Then
																	PCons.Book(sourcecell,acct.AutoCtrl.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10494)
																ElseIf icMethod <> FSKMethod.MEE Then 'xxxLu aqui falta la condición para que haga para MEE
																	PCons.Book(sourcecell,acct.RsvG.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10495)
																	PCons.Book(sourcecell,acct.RsvM.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10496)
																Else 'xxxnew Equity secction NP20211006
																	PCons.Book(sourcecell,acct.RsvEq.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10537)
																End If 'If ICPMethod=MethodHolding

															ElseIf icMethod <> FSKMethod.HOLDING Then													
																PCons.Book(sourcecell,acct.RsvG.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10497)
																PCons.Book(sourcecell,acct.RsvM.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10498)
															End If	'If nature (NatureDim) is not Minoritaires

														Else 'PCON = 1
															If NatureSource.Name<>"Minoritaires" Then												
																PCons.Book(sourcecell,acct.LPTIT.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10499)
																If icMethod = FSKMethod.HOLDING Then
																	PCons.Book(sourcecell,acct.AutoCtrl.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10500)
																ElseIf icMethod <> FSKMethod.MEE Then 'xxxLu aqui falta la condición para que haga para MEE
																	PCons.Book(sourcecell,acct.RsvG.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10513)
																	PCons.Book(sourcecell,acct.RsvM.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10514)
																Else 'xxxnew Equity secction NP20211006
																	PCons.Book(sourcecell,acct.RsvEq.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10537)
																End If 'If ICPMethod=MethodHolding

															ElseIf icMethod <> FSKMethod.HOLDING Then	
																
																PCons.Book(sourcecell,acct.RsvG.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn ,,RegleElim,10503)
																PCons.Book(sourcecell,acct.RsvM.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10504)

															End If	'If nature (NatureDim) is not Minoritaires
														End If	'If the ICP is not consolidated in N-1
														
													'Si le flux est ECO ou ECF
													'If flows is ECO or ECF
													ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															
				                        					'Genere le compte 18LP sur le flux Destination a -Own.PCON
															'Trigger account 18LP on the Destination flow at -Own.PCON
															PCons.Book(sourcecell,acct.LPTIT.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10529)

															'Si la methode du partenaire est Holding
															'If the consolidation method of the ICP is Holding
									                        'If icMethod = 1 Then
									                        If icMethod = FSKMethod.HOLDING Then
									                            'Genere le compte 106CG sur le flux Destination a -Own.PCON
																'Trigger account 106CG on the Destination flow at -Own.PCON
																PCons.Book(sourcecell,acct.RsvCG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10530)
															'Pour les autres methodes
															'For other consolidation methods
									                        Else
									                            'Genere le compte 106CG sur le flux Destination a -Own.POWN
																'Trigger account 106CG on the Destination flow at -Own.POWN
																PCons.Book(sourcecell,acct.RsvCG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10531)
																
									                            'Genere le compte 106CM sur le flux Destination a -Own.PMIN
																'Trigger account 106CM on the Destination flow at -Own.PMIN
																PCons.Book(sourcecell,acct.RsvCM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10532)
									                        End If 'If ICPMethod=MethodHolding
															
														'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
														'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding															
														'ElseIf icMethod <> 1 Then
														ElseIf icMethod <> FSKMethod.HOLDING Then
																
			                        						'Genere le compte 106CG sur le flux Destination a 100%
															'Trigger account 106CG on the Destination flow at 100%
															PCons.Book(sourcecell,acct.RsvCG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10533)
															
			                        						'Genere le compte 106CM sur le flux Destination a -100%
															'Trigger account 106CM on the Destination flow at -100%
															PCons.Book(sourcecell,acct.RsvCM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10534)
														End If	'If nature (NatureDim) is not Minoritaires
													
													'Pour les autres flux
													'For other flows	
													Else 'xxxLu seccion correcta para resto de flujos
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															
				                        					'Genere le compte 18LP sur le flux Destination a -Own.PCON
															'Trigger account 18LP on the Destination flow at -Own.PCON
															PCons.Book(sourcecell,acct.LPTIT.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10535)
															
															'Si la methode du partenaire est Holding
															'If the ICP consolidation  method is Holding
									                        'If icMethod = 1 Then
									                        If icMethod = FSKMethod.HOLDING Then
									                            'Genere le compte 106A sur le flux Destination a -Own.PCON
																'Trigger account 106A on the Destination flow at -Own.PCON
																PCons.Book(sourcecell,acct.AutoCtrl.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10536)
															'Pour les autres methodes
															'For other consolidation methods	
															ElseIf icMethod <> FSKMethod.MEE Then  
									                            'Genere le compte 106G sur le flux Destination a -Own.POWN
																'Trigger account 106G on the Destination flow at -Own.POWN
																PCons.Book(sourcecell,acct.RsvG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10537)
																
									                            'Genere le compte 106M sur le flux Destination a -Own.PMIN
																'Trigger account 106M on the Destination flow at -Own.PMIN
																PCons.Book(sourcecell,acct.RsvM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10538)
									                       
														   Else 'xxxLu condición a replicar arriba
																PCons.Book(sourcecell,acct.RsvEq.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10537)
															'brapi.errorlog.LogMessage(si, " Account " & acctMember.Name)							
															End If 'If ICPMethod=MethodHolding
															
														'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
														'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding															
														'ElseIf icMethod <> 1 Then	
														ElseIf icMethod <> FSKMethod.HOLDING Then	
																
			                        						'Genere le compte 106G sur le flux Destination a 100%
															'Trigger account 106G on the Destination flow at 100%
															PCons.Book(sourcecell,acct.RsvG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10539)
															
			                        						'Genere le compte 106M sur le flux Destination a -100%
															'Trigger account 106M on the Destination flow at -100%
															PCons.Book(sourcecell,acct.RsvM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10540)
														End If	'If nature (NatureDim) is not Minoritaires
														
													End If	'Test on Flows
												End If	'If ICPMethod = 12345 

											'----------------------------------------------------
							                ' Titres de participation: cumul des ecarts de conversion
											' CTA on Investment in Subs
							                '----------------------------------------------------
											ElseIf RegleElim="TITRESH" Then
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then

													Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)
												
													'Si le flux est OUV/VARC/SOR
													'If flow is OUV/VARC/SOR
													If flowMember.MemberId = flow.Ouv.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.Sor.MemberId Then
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															'If icMethod = 1 Then
															If icMethod = FSKMethod.HOLDING Then
															
																'Genere le compte 106A sur le flux Destination a Own.PCON
																'Trigger account 106A on the Destination flow at Own.PCON
																PCons.Book(sourcecell,acct.AutoCtrl.MemberId,flowDestmember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pCon ,,RegleElim,10541)
																
										                        'Genere le compte 106CG sur le flux Destination a -Own.PCON
																'Trigger account 106CG on the Destination flow at -Own.PCON
																PCons.Book(sourcecell,acct.RsvCG.MemberId,flowDestmember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pCon,,RegleElim,10542)
										                    Else
															
										                        'Genere le compte 106G sur le flux Destination a Own.POWN
																'Trigger account 106G on the Destination flow at Own.POWN
																PCons.Book(sourcecell,acct.RsvG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pOwn ,,RegleElim,10543)
																
										                        'Genere le compte 106CG sur le flux Destination a -Own.POWN
																'Trigger account 106CG on the Destination flow at -Own.POWN
																PCons.Book(sourcecell,acct.RsvCG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pOwn,,RegleElim,10544)
																
										                        'Genere le compte 106M sur le flux Destination a Own.PMIN
																'Trigger account 106M on the Destination flow at Own.PMIN
																PCons.Book(sourcecell,acct.RsvM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,Own.pMin ,,RegleElim,10545)
																
										                        'Genere le compte 106CM sur le flux Destination a -Own.PMIN
																'Trigger account 106CM on the Destination flow at -Own.PMIN
																PCons.Book(sourcecell,acct.RsvCM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-Own.pMin,,RegleElim,10546)
																
										                    End If
														'Pour la Nature Minoritaires (NatureDim) et si la methode du partenaire n'est pas Holding
														'If Nature is Minoritaires (NatureDim) and ICP Method is not Holding
														'ElseIf icMethod <> 1 Then
														ElseIf icMethod <> FSKMethod.HOLDING Then
															
									                        'Genere le compte 106G sur le flux Destination a -100%
															'Trigger account 106G on the Destination flow at -100%
															PCons.Book(sourcecell,acct.RsvG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10547)
																
									                        'Genere le compte 106CG sur le flux Destination a 100%
															'Trigger account 106CG on the Destination flow at 100%
															PCons.Book(sourcecell,acct.RsvCG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10548)
																
									                        'Genere le compte 106M sur le flux Destination a 100%
															'Trigger account 106M on the Destination flow at 100%
															PCons.Book(sourcecell,acct.RsvM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,1 ,,RegleElim,10549)
															
									                        'Genere le compte 106CM sur le flux Destination a -100%
															'Trigger account 106CM on the Destination flow at -100%
															PCons.Book(sourcecell,acct.RsvCM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-1,,RegleElim,10550)
															
														End If	'CurCustom4<>Mino															
													End If	
												End If
											
											'----------------------------------------------------
							                ' Marge en Stock
											' Inventory profits
							                '----------------------------------------------------				
											ElseIf RegleElim="MSTK" And childEntity.MemberId <> api.Pov.Entity.MemberId Then 
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then
													Dim CompteStock As Member = api.Account.GetPlugAccount(sourcecell.DataBufferCellPk.AccountId)
													Dim plugResultat As Member = api.Members.GetMember(DimType.Account.Id, "65L")
													Dim plugBilan As Member = api.Members.GetMember(DimType.Account.Id, "40L")
													Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)

													'Si le flux est MVT
													'If the flow is MVT
													If flowMember.MemberId = flow.MVT.MemberId Then
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															
															'----------- Impact chez l'acheteuse - Buyer -------------------------
															'Genere le compte 40L sur le flux Destination a Own.PCON * Own.PCON du partenaire
															'Trigger account 40L on the Destination flow at Own.PCON * Own.PCON of ICP
															PCons.Book(sourcecell,plugBilan.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pCon * ICPOwn.PCon),,RegleElim,10551)
																								
										                    'Si la methode du partenaire n'est pas Mise en Equivalence
															'If the consolidation method of the ICP is Not Equity Pickup
										                    'If icMethod <> 3 Then
										                    If icMethod <> FSKMethod.MEE Then
										                        'Genere le compte de Stock (Plug Account du compte source) sur le flux Destination a -Own.PCON * Own.PCON du partenaire
																'Trigger the inventory account (Plug Account on the source account) on the Destination flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,CompteStock.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10552)
															'Si la methode du partenaire est Mise en Equivalence
															'If the consolidation method of the ICP is Equity Pickup	
										                    Else
										                        'Genere le compte 264EQUI sur le flux RES a -Own.PCON * Own.PCON du partenaire
																'Trigger account 264EQUI on the RES flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,acct.MEE.MemberId,flow.Res.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10553)
																
										                    End If
															
															'Si la methode du partenaire n'est pas Mise en Equivalence
															'If the ICP consolidation method is Not Equity Pickup
										                    'If icMethod <> 3 Then
										                    If icMethod <> FSKMethod.MEE Then
																'Si la nature de Destination est MPIF/MECS/MECB (NatureDim
																'If Destination nature is MPIF/MECS/MECB (NatureDim)
										                        If Nature = "MPIF" Or Nature = "MECS" Or Nature = "MECB" Then															
										                          	'Genere le compte 713500 sur le flux None a -Own.PCON * Own.PCON du partenaire
																	'Trigger account 713500 on the None flow at -Own.PCON * Own.PCON of ICP
																	Dim VarStkPF As Member = api.Members.GetMember(DimType.Account.Id, "713500")														
																	PCons.Book(sourcecell,VarStkPF.MemberId,flow.None.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10554)
																'Si la nature de Destination est MMAT (NatureDim
																'If Destination nature is MMAT (NatureDim)	
										                        ElseIf Nature = "MMAT" Then														
										                            'Genere le compte 603000 sur le flux None a -Own.PCON * Own.PCON du partenaire
																	'Trigger account 603000 on the None flow at -Own.PCON * Own.PCON of ICP
																	Dim VarStkMP As Member = api.Members.GetMember(DimType.Account.Id, "603000")
																	PCons.Book(sourcecell,VarStkMP.MemberId,flow.None.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pcon * ICPOwn.PCon),,RegleElim,10555)
																'Si la nature de Destination est MMAR (NatureDim
																'If Destination nature is MMAR (NatureDim)	
										                        ElseIf Nature = "MMAR" Then
										                            'Genere le compte 603700 sur le flux None a -Own.PCON * Own.PCON du partenaire
																	'Trigger account 603700 on the None flow at -Own.PCON * Own.PCON of ICP
																	Dim VarStkMAR As Member = api.Members.GetMember(DimType.Account.Id, "603700")
																	PCons.Book(sourcecell,VarStkMAR.MemberId,flow.None.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pCon * ICPOwn.PCon),,RegleElim,10556)
										                        End If
															'Si la methode du partenaire est Mise en Equivalence
															'If the consolidation method of the ICP is Equity Pickup	
										                    Else
										                        'Genere le compte 880EQUI sur le flux None a -Own.PCON * Own.PCON du partenaire
																'Trigger account 880EQUI on the None flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,acct.ResultatMEE.MemberId,flow.None.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10557)
										                    End If
										                    
															'Genere le compte 65L sur le flux None a -Own.PCON * Own.PCON du partenaire
															'Trigger the account 65L on the None flow at -Own.PCON * Own.PCON of ICP
															PCons.Book(sourcecell,plugResultat.MemberId,flow.None.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10558)
																																
														End If
													'Si le flux est ECO ou ECF
													'If flows is ECO or ECF
													ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then

													'Pour les autres flux
													'For other flows	
													Else
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then													

															'Genere le compte 40L sur le flux Destination a Own.PCON * Own.PCON du partenaire
															'Trigger the account 40L on the Destination flow at Own.PCON * Own.PCON of ICP
															PCons.Book(sourcecell,plugBilan.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pCon * ICPOwn.PCon),,RegleElim,10559)
															
										                    '----------- Impact chez l'acheteuse - Buyer -------------------------
										                    'Si la methode du partenaire n'est pas Mise en Equivalence
															'If the consolidation method of the ICP is Not Equity Pickup
										                    'If icMethod <> 3 Then
										                    If icMethod <> FSKMethod.MEE Then
										                        'Genere le compte de Stock (Plug Account du compte source) sur le flux Destination a -Own.PCON * Own.PCON du partenaire
																'Trigger the inventory account (Plug Account on the source account) on the Destination flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,CompteStock.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10560)
															'Si la methode du partenaire est Mise en Equivalence
															'If the consolidation method of the ICP is Equity Pickup	
										                    Else
										                        'Genere le compte 261EQUI sur le flux Destination a -Own.PCON * Own.PCON du partenaire
																'Trigger the account 261EQUI on the Destination flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,acct.MEE.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10561)
										                    End If
																															
														End If
													End If
												End If	'If Can Eliminate
											'----------------------------------------------------
							                ' Marge en Stock: cumul des ecarts de conversion
											' CTA on Inventory profits
							                '----------------------------------------------------
											ElseIf RegleElim="MSTKH" And childEntity.MemberId <> api.Pov.Entity.MemberId Then ' CWE & SDL 2020-05-12
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then
													Dim CompteStock As Member = api.Account.GetPlugAccount(sourcecell.DataBufferCellPk.AccountId)
													Dim plugResultat As Member = api.Members.GetMember(DimType.Account.Id, "65L")
													Dim plugBilan As Member = api.Members.GetMember(DimType.Account.Id, "40L")
													Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)
													
													'Si le flux est OUV/VARC/SOR
													'If flow is OUV/VARC/SOR
													If flowMember.MemberId = flow.ECO.MemberId Then 'flow.Ouv.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.Sor.MemberId Then
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															
															'----------- Impact chez l'acheteuse - Buyer -------------------------
															'Genere le compte 40L sur le flux Destination a Own.PCON * Own.PCON du partenaire
															'Trigger the account 40L on the Destination flow at Own.PCON * Own.PCON of ICP
															PCons.Book(sourcecell,plugBilan.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pCon * ICPOwn.PCon),,RegleElim,10562)

															'Si la methode du partenaire n'est pas Mise en Equivalence
															'If the consolidation method of the ICP is Not Equity Pickup
										                    'If icMethod <> 3 Then
										                    If icMethod <> FSKMethod.MEE Then
										                        'Genere le compte de Stock (Plug Account du compte source) sur le flux Destination a -Own.PCON * Own.PCON du partenaire
																'Trigger the inventory account (Plug Account on the source account) on the Destination flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,CompteStock.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10563)
															'Si la methode du partenaire est Mise en Equivalence
															'If the consolidation method of the ICP is Equity Pickup	
										                    Else
										                        'Genere le compte 261EQUI sur le flux Destination a -Own.PCON * Own.PCON du partenaire
																'Trigger the account 261EQUI on the Destination flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,acct.MEE.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10564)
										                    End If
															
														End If
													'Si le flux est ECR
													'If the flow is ECR	
													ElseIf flowMember.MemberId = flow.Ecr.MemberId Then
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															
															'Genere le compte 40L sur le flux ECF a Own.PCON * Own.PCON du partenaire
															'Trigger the account 40L on the ECF flow at Own.PCON * Own.PCON of ICP
															PCons.Book(sourcecell,plugBilan.MemberId,flow.ECF.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pCon * ICPOwn.PCon),,RegleElim,10565)
															
										                    '----------- Impact chez l'acheteuse -------------------------
															'Si la methode du partenaire n'est pas Mise en Equivalence
															'If the consolidation method of the ICP is Not Equity Pickup
										                    'If icMethod <> 3 Then
										                    If icMethod <> FSKMethod.MEE Then
																'Genere le compte de Stock (Plug Account du compte source) sur le flux ECF a -Own.PCON * Own.PCON du partenaire
																'Trigger the inventory account (Plug Account on the source account) on the ECF flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,CompteStock.MemberId,flow.ECF.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10566)
															'Si la methode du partenaire est Mise en Equivalence
															'If the consolidation method of the ICP is Equity Pickup	
										                    Else
										                        'Genere le compte 261EQUI sur le flux ECF a -Own.PCON * Own.PCON du partenaire
																'Trigger the account 261EQUI on the ECF flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,acct.MEE.MemberId,flow.ECF.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10567)
										                    End If

														End If	
													End If
			
												End If
											'----------------------------------------------------
							                ' PV/MV de cession d'immobilisations intragroupe
											' Gain/Loss on interco sale of fixed assets
							                '----------------------------------------------------	
											ElseIf RegleElim="PMV" And childEntity.MemberId <> api.Pov.Entity.MemberId Then ' CWE & SDL 2020-05-12
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then
																							
													Dim CompteImmo As String = Left(acctMember.Name,6)
													Dim CompteImmoDest As Member = api.Members.GetMember(DimType.Account.Id, CompteImmo)
													Dim plugBilan As Member = api.Members.GetMember(DimType.Account.Id, "16L")
													Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)
													
													Dim plugResultat As Member = api.Account.GetPlugAccount(sourcecell.DataBufferCellPk.AccountId)
													
													'Si le flux est AUG
													'If the flow is AUG
													If flowMember.MemberId = flow.Aug.MemberId Then
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															
										                	'----------- Impact chez l'acheteuse - Buyer -------------------------
															'Genere le compte 16L sur le flux AUG a Own.PCON * Own.PCON du partenaire
															'Trigger the account 16L on the AUG flow at Own.PCON * Own.PCON of ICP
															PCons.Book(sourcecell,plugBilan.MemberId,flow.Aug.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pCon * ICPOwn.PCon),,RegleElim,10568)
															
															'Si la methode du partenaire n'est pas Mise en Equivalence
															'If the consolidation method of the ICP is Not Equity Pickup 
										                	'If icMethod <> 3 Then
										                	If icMethod <> FSKMethod.MEE Then
										                	    'Genere le compte d'immobilisation sur le flux AUG a -Own.PCON * Own.PCON du partenaire
																'Trigger the fixed asset account on the AUG flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,CompteImmoDest.MemberId,flow.Aug.MemberId,ICNoneMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10569)
															'Si la methode du partenaire est Mise en Equivalence
															'If the consolidation method of the ICP is Equity Pickup	
										                	Else
										                	    'Genere le compte 261EQUI sur le flux AUG a -Own.PCON * Own.PCON du partenaire
																'Trigger the 261EQUI account on the AUG flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,acct.MEE.MemberId,flow.Aug.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10570)
										                	End If
																															
														End If
													'Si le flux est ECO ou ECF
													'If flows is ECO or ECF	
													ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then

													'Pour les autres flux different de NBV
													'For other flows different from NBV	
													ElseIf flowMember.MemberId <> flow.NBV.MemberId Then
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															
									                    	'----------- Impact chez l'acheteuse - Buyer -------------------------
															'Genere le compte 16L sur le flux Destination a -Own.PCON * Own.PCON du partenaire
															'Trigger the account 16L on the Destination flow at -Own.PCON * Own.PCON of ICP
															PCons.Book(sourcecell,plugBilan.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pCon * ICPOwn.PCon),,RegleElim,10571)
															
										                	'Si la methode du partenaire n'est pas Mise en Equivalence
															'If the consolidation method of the ICP is Not Equity Pickup
										                	'If icMethod <> 3 Then
										                	If icMethod <> FSKMethod.MEE Then
										                	    'Genere le compte d'immobilisation sur le flux Destination a -Own.PCON * Own.PCON du partenaire
																'Trigger the fixed asset account on the Destination flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,CompteImmoDest.MemberId,flowDestMember.MemberId,ICNoneMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10572)
															'Si la methode du partenaire est Mise en Equivalence
															'If the consolidation method of the ICP is Equity Pickup	
										                	Else
										                	    'Genere le compte 261EQUI sur le flux Destination a -Own.PCON * Own.PCON du partenaire
																'Trigger the account 261EQUI on the Destination flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,acct.MEE.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10573)
										                	End If

														End If
													'Si le flux est NBV
													'If the flow is NBV	
													Else
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then											     		
															'Si la nature de destination est INTANGGL
															'If the destination nature is INTANGGL
															If NatureDest.Name = "INTANGGL" Then'HC
																Dim CFDISPINT As Member = api.Members.GetMember(DimType.Account.Id, "CFDISP_INTANG")
																Dim CFPURCHINT As Member = api.Members.GetMember(DimType.Account.Id, "CFPURCH_INTANG")
																
										               	    	'Genere le compte CFPURCH_INTANG sur le flux None a -Own.PCON * Own.PCON du partenaire
																'Trigger the account CFPURCH_INTANG on the None flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,CFPURCHINT.MemberId,flow.None.MemberId,ICNoneMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10574)
															'Si la nature de destination est TANGGL
															'If the destination nature is TANGGL	
															ElseIf NatureDest.Name = "TANGGL" Then
																Dim CFDISPTAN As Member = api.Members.GetMember(DimType.Account.Id, "CFDISP_TANG")
																Dim CFPURCHTAN As Member = api.Members.GetMember(DimType.Account.Id, "CFPURCH_TANG")
																
										               	    	'Genere le compte CFPURCH_TANG sur le flux None a -Own.PCON * Own.PCON du partenaire
																'Trigger the account CFPURCH_TANG on the None flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,CFPURCHTAN.MemberId,flow.None.MemberId,ICNoneMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10575)
															'Si la nature de destination est FINGL
															'If the destination nature is FINGL	
															ElseIf NatureDest.Name = "FINGL" Then
																Dim CFDISPFIN As Member = api.Members.GetMember(DimType.Account.Id, "CFDISP_FIN")
																Dim CFPURCHFIN As Member = api.Members.GetMember(DimType.Account.Id, "CFPURCH_FIN")
																
										               	    	'Genere le compte CFPURCH_FIN sur le flux None a -Own.PCON * Own.PCON du partenaire
																'Trigger the account CFPURCH_FIN on the None flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,CFPURCHFIN.MemberId,flow.None.MemberId,ICNoneMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10576)
															End If
														End If
													End If

												End If

											'----------------------------------------------------
							                ' PV/MV de cession d'immobilisations intragroupe: cumul des ecarts de conversion
											' CTA on Gain/Loss on interco sale of fixed assets
							                '----------------------------------------------------	
											ElseIf RegleElim="PMVH" And childEntity.MemberId <> api.Pov.Entity.MemberId Then 
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then
													
													Dim CompteImmo As String = Left(acctMember.Name,6)
													Dim CompteImmoDest As Member = api.Members.GetMember(DimType.Account.Id, CompteImmo)
													Dim plugBilan As Member = api.Members.GetMember(DimType.Account.Id, "16L")
													Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)
													
													'Si le flux est OUV/VARC/SOR
													'If flow is OUV/VARC/SOR
													If flowMember.MemberId = flow.Ouv.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.Sor.MemberId Then

													'Si le flux est ECO ou ECF
													'If flows is ECO or ECF	
													ElseIf flowMember.MemberId = flow.Eco.MemberId Or flowMember.MemberId = flow.Ecf.MemberId Then
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then

										                   	'----------- Impact chez l'acheteuse - Buyer -------------------------
															'Genere le compte 16L sur le flux Destination a Own.PCON * Own.PCON du partenaire
															'Trigger the account 16L on the Destination flow at Own.PCON * Own.PCON of ICP
															PCons.Book(sourcecell,plugBilan.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pCon * ICPOwn.PCon),,RegleElim,10577)
															
															'Si la methode du partenaire n'est pas Mise en Equivalence
															'If the consolidation method of the ICP is Not Equity Pickup
										                   	'If icMethod <> 3 Then
										                   	If icMethod <> FSKMethod.MEE Then
										                   	    'Genere le compte d'immobilisation sur le flux Destination a -Own.PCON * Own.PCON du partenaire
																'Trigger the fixed asset account on the Destination flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,CompteImmoDest.MemberId,flowDestMember.MemberId,ICNoneMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10578)
															'Si la methode du partenaire est Mise en Equivalence
															'If the consolidation method of the ICP is Equity Pickup	
										                   	Else
										                   	    'Genere le compte 261EQUI sur le flux Destination a -Own.PCON * Own.PCON du partenaire
																'Trigger the 261EQUI account on the Destination flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,acct.MEE.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10579)
										                   	End If

														End If
													'Si le flux est ECR
													'If the flow is ECR
													ElseIf flowMember.MemberId = flow.Ecr.MemberId Then
														'Si la nature (NatureDim) n'est pas Minoritaires
														'If nature (NatureDim) is not Minoritaires
														If NatureSource.Name<>"Minoritaires" Then
															
										                   	'----------- Impact chez l'acheteuse - Buyer -------------------------
															'Genere le compte 16L sur le flux ECF a -Own.PCON * Own.PCON du partenaire
															'Trigger the account 16L on the ECF flow at -Own.PCON * Own.PCON of ICP
															PCons.Book(sourcecell,plugBilan.MemberId,flow.ECF.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,(Own.pCon * ICPOwn.PCon),,RegleElim,10580)
															
															'Si la methode du partenaire n'est pas Mise en Equivalence
															'If the consolidation method of the ICP is Not Equity Pickup
										                   	'If icMethod <> 3 Then
										                   	If icMethod <> FSKMethod.MEE Then
										                   	    'Genere le compte d'immobilisation sur le flux ECF a -Own.PCON * Own.PCON du partenaire
																'Trigger the fixed asset account on the ECF flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,CompteImmoDest.MemberId,flow.ECF.MemberId,icNoneMember.MemberId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10581)
															'Si la methode du partenaire est Mise en Equivalence
															'If the consolidation method of the ICP is Equity Pickup	
										                   	Else
										                   	    'Genere le compte 261EQUI sur le flux ECF a -Own.PCON * Own.PCON du partenaire
																'Trigger the 261EQUI account on the ECF flow at -Own.PCON * Own.PCON of ICP
																PCons.Book(sourcecell,acct.MEE.MemberId,flow.ECF.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-(Own.pCon * ICPOwn.PCon),,RegleElim,10582)
										                   	End If												

														End If	
													End If

												End If
												
											ElseIf RegleElim = "DIVVVAR" And childEntity.MemberId <> api.Pov.Entity.MemberId Then ' CWE & SDL 2020-05-12
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then
													Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)
													
													'Si le flux est OUV/VARC/SOR
													'If flow is OUV/VARC/SOR
													If flowMember.MemberId = flow.Ouv.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.Sor.MemberId Then
														
														'------------------ Note, must be this way for Tech_WTAXD ---------------------
														'------------------ Otherwise, VARPER, recheck for Tech_DIV -------------------
												
															'Genere le compte 106G sur le flux Destination a -Own.POWN du partenaire
															'Trigger account 106G on the Destination flow at -Own.POWN of the ICP
															PCons.Book(sourcecell,acct.RsvG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.POwn ,,RegleElim,10583)
															
										                    'Genere le compte 106M sur le flux Destination a -Own.PMIN du partenaire
															'Trigger account 106M on the Destination flow at -Own.PMIN of the ICP
															PCons.Book(sourcecell,acct.RsvM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.PMin ,,RegleElim,10584)
															
															'Genere le compte 106CG sur le flux Destination a Own.POWN du partenaire
															'Trigger account 106CG on the Destination flow at Own.POWN of the ICP
															PCons.Book(sourcecell,acct.RsvCG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.POwn,,RegleElim,10585)
															
															'Genere le compte 106CM sur le flux Destination a Own.PMIN du partenaire
															'Trigger account 106CM on the Destination flow at Own.PMIN of the ICP
															PCons.Book(sourcecell,acct.RsvCM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.PMin,,RegleElim,10586)
														
													
													End If								
												End If
												
											ElseIf RegleElim = "DIVVVAR2" And childEntity.MemberId <> api.Pov.Entity.MemberId Then ' CWE & SDL 2020-05-12
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then
													Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)
													
													'Si le flux est OUV/VARC/SOR
													'If flow is OUV/VARC/SOR
													If flowMember.MemberId = flow.Ouv.MemberId Or flowMember.MemberId = flow.VarC.MemberId Or flowMember.MemberId = flow.Sor.MemberId Then
														
														'------------------ Note, must be this way for Tech_WTAXD ---------------------
														'------------------ Otherwise, VARPER, recheck for Tech_DIV -------------------
								
															'Genere le compte 106G sur le flux Destination a -Own.POWN du partenaire
															'Trigger account 106G on the Destination flow at -Own.POWN of the ICP
															PCons.Book(sourcecell,acct.RsvG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.POwn,,RegleElim,10587)
															
										                    'Genere le compte 106M sur le flux Destination a -Own.PMIN du partenaire
															'Trigger account 106M on the Destination flow at -Own.PMIN of the ICP
															PCons.Book(sourcecell,acct.RsvM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.PMin,,RegleElim,10588)
															
															'Genere le compte 106CG sur le flux Destination a Own.POWN du partenaire
															'Trigger account 106CG on the Destination flow at Own.POWN of the ICP
															PCons.Book(sourcecell,acct.RsvCG.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.POwn ,,RegleElim,10589)
															
															'Genere le compte 106CM sur le flux Destination a Own.PMIN du partenaire
															'Trigger account 106CM on the Destination flow at Own.PMIN of the ICP
															PCons.Book(sourcecell,acct.RsvCM.MemberId,flowDestMember.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.PMin ,,RegleElim,10590)
														
												
													End If	
												End If
												
											ElseIf RegleElim = "DIVV" And childEntity.MemberId <> api.Pov.Entity.MemberId Then ' CWE & SDL 2020-05-12
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then
													Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)
													
													'Genere le compte 106CG sur le flux DIV a -Own.POWN du partenaire
													'Trigger account 106CG on the DIV flow at -Own.POWN of the ICP
													PCons.Book(sourcecell,acct.RsvCG.MemberId,flow.Div.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.POwn ,,RegleElim,10591)
													
								                    'Genere le compte 106CM sur le flux DIV a Own.PMIN du partenaire
													'Trigger account 106CM on the DIV flow at Own.PMIN of the ICP
													PCons.Book(sourcecell,acct.RsvCM.MemberId,flow.Div.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.PMin ,,RegleElim,10592)
													
													'Si l'entite et le partenaire sont consolides en N-1
													'If the entity and the ICP are consolidated in N-1
													If Own.pCon_1 > 0 And ICPOwn.PCon_1 > 0 Then
														'Genere le compte 106G sur le flux DIV a Own.POWN N-1 du partenaire
														'Trigger account 106G on the DIV flow at Own.POWN N-1 of the ICP
														PCons.Book(sourcecell,acct.RsvG.MemberId,flow.Div.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.POwn_1,,RegleElim,10593)
														
									                    'Genere le compte 106G sur le flux VARC a la variation de Own.POWN du partenaire
														'Trigger account 106G on the VARC flow at the change in Own.POWN of the ICP
														PCons.Book(sourcecell,acct.RsvG.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.VPOwn ,,RegleElim,10594)
														
														'Genere le compte 106M sur le flux DIV a Own.PMIN N-1 du partenaire
														'Trigger account 106M on the DIV flow at Own.PMIN N-1 of the ICP
														PCons.Book(sourcecell,acct.RsvM.MemberId,flow.Div.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.PMin_1,,RegleElim,10595)
														
														'Genere le compte 106M sur le flux VARC a la variation de Own.POWN du partenaire
														'Trigger account 106M on the VARC flow at the change in Own.POWN of the ICP
														PCons.Book(sourcecell,acct.RsvM.MemberId,flow.VarC.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.VPMin ,,RegleElim,10596)
													
													'Si	l'entite est consolidee en N-1 mais le partenaire n'est pas consolide en N-1
													'If the entity is consolidated in N-1 but the ICP is not consolidated in N-1
													ElseIf Own.pCon_1 > 0 And ICPOwn.PCon_1 = 0 Then
			                    						'Genere le compte 106G sur le flux DIV a Own.POWN du partenaire
														'Trigger account 106G on the DIV flow at Own.POWN of the ICP
														PCons.Book(sourcecell,acct.RsvG.MemberId,flow.Div.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.POwn,,RegleElim,10597)
														
									                    'Genere le compte 106M sur le flux DIV a Own.PMIN du partenaire
														'Trigger account 106M on the DIV flow at Own.PMIN of the ICP
														PCons.Book(sourcecell,acct.RsvM.MemberId,flow.Div.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.PMin,,RegleElim,10598)
													'Si l'entite et le partenaire ne sont pas consolides en N-1
													'If the entity and the ICP are not consolidated in N-1
													Else
			                    						'Genere le compte 106G sur le flux ENT a Own.POWN du partenaire
														'Trigger account 106G on the ENT flow at Own.POWN of the ICP
														PCons.Book(sourcecell,acct.RsvG.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.POwn,,RegleElim,10599)
														
									                    'Genere le compte 106M sur le flux ENT a Own.PMIN du partenaire
														'Trigger account 106M on the ENT flow at Own.PMIN of the ICP
														PCons.Book(sourcecell,acct.RsvM.MemberId,flow.Ent.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.PMin,,RegleElim,10600)
																			
													End If
												End If
											
											ElseIf RegleElim = "ADIVV" And childEntity.MemberId <> api.Pov.Entity.MemberId Then ' CWE & SDL 2020-05-12
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then
													Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)
													
													'Genere le compte 106G sur le flux ADIV a -Own.POWN du partenaire
													'Trigger account 106G on the ADIV flow at -Own.POWN of the ICP
													PCons.Book(sourcecell,acct.RsvG.MemberId,flow.IDiv.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.POwn,,RegleElim,10601)
													
								                    'Genere le compte 106M sur le flux ADIV a -Own.PMIN du partenaire
													'Trigger account 106M on the ADIV flow at -Own.PMIN of the ICP
													PCons.Book(sourcecell,acct.RsvM.MemberId,flow.IDiv.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.PMin,,RegleElim,10602)
																							
													'Genere le compte 106CG sur le flux ADIV a Own.POWN du partenaire
													'Trigger account 106CG on the ADIV flow at Own.POWN of the ICP
													PCons.Book(sourcecell,acct.RsvCG.MemberId,flow.IDiv.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.POwn ,,RegleElim,10603)
													
								                    'Genere le compte 106CM sur le flux ADIV a Own.PMIN du partenaire
													'Trigger account 106CM on the ADIV flow at Own.PMIN of the ICP
													PCons.Book(sourcecell,acct.RsvCM.MemberId,flow.IDiv.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.PMin ,,RegleElim,10604)
														
												End If	
											
											ElseIf RegleElim = "RAS" And childEntity.MemberId <> api.Pov.Entity.MemberId Then ' CWE & SDL 2020-05-12
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then
													Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)
													
													'Genere le compte 695000 sur le flux None a Own.PCON du partenaire
													'Trigger account 695000 on the None flow at Own.PCON of the ICP				                                        
									                PCons.Book(sourcecell,acct.Impots.MemberId,,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.PCon ,,RegleElim,10605)
							
													'Genere le compte 120G sur le flux RES a -Own.POWN du partenaire
													'Trigger account 120G on the RES flow at -Own.POWN of the ICP
													PCons.Book(sourcecell,acct.ResG.MemberId,flow.Res.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.POwn,,RegleElim,10606)
													
								                    'Genere le compte 120M sur le flux RES a -Own.PMIN du partenaire
													'Trigger account 120M on the RES flow at -Own.PMIN of the ICP
													PCons.Book(sourcecell,acct.ResM.MemberId,flow.Res.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.PMin,,RegleElim,10607)
																							
													'Genere le compte 106CGG sur le flux DIV a -Own.POWN du partenaire
													'Trigger account 106CGG on the DIV flow at -Own.POWN of the ICP
													PCons.Book(sourcecell,acct.RsvCG.MemberId,flow.Div.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.POwn,,RegleElim,10608)
													
								                    'Genere le compte 106CM sur le flux DIV a -Own.PMIN du partenaire
													'Trigger account 106CM on the DIV flow at -Own.PMIN of the ICP
													PCons.Book(sourcecell,acct.RsvCM.MemberId,flow.Div.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.PMin,,RegleElim,10609)
													
												End If
											ElseIf RegleElim = "DIVVH" And childEntity.MemberId <> api.Pov.Entity.MemberId Then ' CWE & SDL 2020-05-12
												'Si la methode du partenaire est 1,2,3,4,5
												'If ICP consolidation method is 1,2,3,4,5
												If icMethod12345 Then
													Dim icpOwn As New OwnershipClass(si,api,globals, icName, api.Pov.Parent.Name,,, OpeScenario)
													
													'Si le flux est ECF
													'If the flow is ECF
													If flowMember.MemberId = flow.Ecf.MemberId Then     	    
														'Genere le compte 120CG sur le flux ECF a -Own.POWN du partenaire
														'Trigger account 120CG on the ECF flow at -Own.POWN of the ICP
														PCons.Book(sourcecell,acct.ResCG.MemberId,flow.ECF.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.POwn,,RegleElim,10610)
														
									                    'Genere le compte 120CM sur le flux ECF a -Own.PMIN du partenaire
														'Trigger account 120CM on the ECF flow at -Own.PMIN of the ICP
														PCons.Book(sourcecell,acct.ResCM.MemberId,flow.ECF.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,-ICPOwn.PMin,,RegleElim,10611)
														
														'Genere le compte 106CG sur le flux ECF a Own.POWN du partenaire
														'Trigger account 106CG on the ECF flow at Own.POWN of the ICP
														PCons.Book(sourcecell,acct.RsvCG.MemberId,flow.ECF.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.POwn ,,RegleElim,10612)
														
														'Genere le compte 106CM sur le flux ECF a Own.PMIN du partenaire
														'Trigger account 106CM on the ECF flow at Own.PMIN of the ICP
														PCons.Book(sourcecell,acct.RsvCM.MemberId,flow.ECF.MemberId,SourceEntityId,DimConstants.Elimination,,,,NatureDest.MemberId,,,,,ICPOwn.PMin ,,RegleElim,10613)
													
													End If
							
												End If	
												
											End If	'End If RegleElim=
										End If	'icName=api.pov.Entity.Name
									Next	'Each sourceCell As DataBufferCell In childDataBuffer.DataBufferCells.Values
								End If	'If Not childDataBuffer Is Nothing Then										
							End If	'If childEntity.MemberId <> api.Pov.Entity.MemberId Then
						Next	'For Each childEntity In childEntities						
					End If	'If Not childEntities Is Nothing Then


					api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
					PCons.WriteLogToTable
					
					api.data.calculate("F#" & Flow.Clo.Name & "= F#" & Flow.TF.Name)
					
				End If	'If (api.Pov.Cons.MemberId = DimConstants.ConsElimination) And (args.CalculateArgs.IsSecondPassEliminationCalc) Then
			
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Sub
#End Region

#Region "Vaciado Plug"

	
#End Region

'Special Scopes
#Region "IFRS5"

'		Private Sub CalculateIFRS5(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
'			Try
'				If Not (OwnershipClass.GetOwnershipInfoByName(si, api, globals, "Discontinued") = 1.0) Then Exit Sub

'				'Initialize the defined member classes
'				Dim Acct As AccountClass = Me.InitAccountClass(si, api, globals)
'				Dim Flow As FlowClass = Me.InitFlowClass(si, api, globals)
'				Dim Nat As NatureClass = Me.InitNatureClass(si, api, globals, api.Pov.UD4Dim.DimPk)
'				Dim sPY As String = api.Members.GetMemberName(dimtypeid.Time, api.Time.GetLastPeriodInPriorYear)
'				Dim bDiscPY As Boolean = OwnershipClass.GetOwnershipInfoByName(si, api, globals, "Discontinued",,,,sPY) = 1.0
'				Dim resultDataBuffer As DataBuffer
'				Dim originMember As Integer
'				Dim sI5Rule As String = "IFRS5"
				
'				'get the member for the nature element
'				Dim NatureDest As Member = Nat.DiscOp 'api.Members.GetMember(DimType.UD4.Id, "DiscOp")
				
'				'Check, if it is for elimination (needs to run after the second pass) or the share
'				If (api.Pov.Cons.MemberId = DimConstants.ConsElimination) And args.CalculateArgs.IsSecondPassEliminationCalc Then
'					'Get the databuffer to not delete existing eliminations
'					resultDataBuffer = api.Data.GetDataBufferUsingFormula("C#Elimination")
'					'The origin dimension always needs to be elimination 
'					originMember = DimConstants.Elimination 
'				ElseIf (api.Pov.Cons.MemberId = DimConstants.Share) Then
'					'Get the databuffer to not delete existing eliminations
'				 	resultDataBuffer = api.Data.GetDataBufferUsingFormula("C#Share")
'					'No specific origin dimension needed. .all will be replaced by the actual one
'					originMember = DimConstants.All
'				Else
'					'exit if none of these cases happens
'					Exit Sub
'				End If
				
'				'For IFRS5 the source databuffer is the same like the result databuffer
'				Dim sourceDataBuffer As New DataBuffer(resultDataBuffer)
				
'				'DestinationInfo for later
'				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
				
'				If Not sourceDataBuffer Is Nothing Then
'					Dim I5Cons As New ConsClass(si, globals, api, resultDataBuffer, bDoLog, False) 'The object for the book method
'					'Loop the Databuffer
'					For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
'						'In some cases we need to switch the sign
'						Dim signA As Integer = 1
'						Dim signM As Integer = 1
'						'Initialize the target flow member object
'						Dim flowMember As Member = api.Members.GetMember(DimType.flow.Id, cell.DataBufferCellPk.FlowId)
'						Dim flowDestMember1 As Member = Nothing
'						Dim flowDestMember2 As Member = Nothing
								
'						'Si le flux source est CLO rien de faire
'						'If the source flow is CLO do nothing					
'						If flowMember.Equals(flow.CLO) Then 
'							Continue For
'						'Si le flux source n'est pas NONE 
'						'If the source flow is not none
'						ElseIf flowMember.MemberId.Equals(DimConstants.None) Then
'							flowDestMember1 = flowMember
'							flowDestMember2 = flowMember
'						'If the source flow is part of Total Flows	
'						ElseIf api.Members.IsBase(api.Pov.FlowDim.DimPk, flow.TF.MemberId, flowMember.MemberId)
'							flowDestMember1 = flow.DisOp
'							flowDestMember2 = flow.DisOp
'							'Switch sign variable in case the switch sign property is set
'							If api.Flow.SwitchSign(flowMember.MemberId) Then
'								signM *= -1
'							End If
'						Else
'							'Do nothing
'							Continue For
'						End If

'						'Initialize the target account member object
'						Dim acctMember As Member = api.Members.GetMember(DimType.Account.Id, cell.DataBufferCellPk.AccountId)
'						Dim acctType As AccountType = api.Account.GetAccountType(cell.DataBufferCellPk.AccountId)
'						Dim acctDestMember As Member = Nothing
'						'Si le conte source est ...
'						'If the source account is ...
'						'Do nothing for equtity accounts

'						'Do nothing for equtity accounts
'						If api.Members.IsBase(api.Pov.AccountDim.DimPk, acct.TopEQ.MemberId, acctMember.MemberId) Then
'							 Continue For
'						'Do Nothing For plug accounts
'						ElseIf acct.PlugAccounts.Contains(acctMember.MemberId) Then
'							 Continue For
'						'Do nothing if the account is not part of the Balance sheet and not part of the Income statement
'						ElseIf Not api.Members.IsBase(api.Pov.AccountDim.DimPk, acct.TopBS.MemberId, acctMember.MemberId) And _
'						Not api.Members.IsBase(api.Pov.AccountDim.DimPk, acct.TopIS.MemberId, acctMember.MemberId)Then
'							 Continue For
'						'Select the Tax target
'						ElseIf acctMember.Equals(acct.Tax) Then 
'							acctDestMember = acct.DiscTax
'						'Select the Asset target
'						ElseIf api.Members.IsBase(api.Pov.AccountDim.DimPk, acct.TopAss.MemberId, acctMember.MemberId) Then
'							acctDestMember = acct.AssHeldfSale
'							If acctType.Equals(accounttype.Liability) Then signA *= -1 'if account is liability sign needs to change
'						'Select the Liability target
'						ElseIf api.Members.IsBase(api.Pov.AccountDim.DimPk, acct.TopLiab.MemberId, acctMember.MemberId) Then
'							acctDestMember = acct.LiabHeldfSale
'							If acctType.Equals(accounttype.Asset) Then signA *= -1 'if account is asset sign needs to change
'						ElseIf acctType.Equals(AccountType.Expense) Then
'							acctDestMember = acct.DiscProfit
'						 	'If the source is an expense swith sighn
'							signA *= -1
'						ElseIf acctType.Equals(AccountType.Revenue) Then
'							acctDestMember = acct.DiscProfit
'						Else
'							Continue For 
'						End If

'						If bDiscPY AndAlso cell.DataBufferCellPk.item(nat.NatDimType.Id).Equals(NatureDest.MemberId) Then
'							If (flowMember.MemberId = flow.Ouv.MemberId) Then
'								Continue For
'							ElseIf (flowMember.MemberId = flow.VarO.MemberId) Then
'									flowDestMember1 = flowMember
'									flowDestMember2 = flow.DisOp
'									acctDestMember = acctMember
'									signA = 1
'							End If	
'						End If

'						'Double sided booking to adjust the discontinued operations
'						I5Cons.Book(cell, acctMember.MemberId,     flowDestMember1.MemberId,, originMember,,,, NatureDest.MemberId,,,,,-1*signM,,sI5Rule,10614)
'						I5Cons.Book(cell, acctDestMember.MemberId, flowDestMember2.MemberId,, originMember,,,, NatureDest.MemberId,,,,, signM*signA,,sI5Rule,10615)

'					Next	'Each sourceCell As DataBufferCell In childDataBuffer.DataBufferCells.Values
'					'Write the data back to the cube
'					api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
'					'Write the logged transactio to the sql database
'					I5Cons.WriteLogToTable
'				End If

'				'Copy the TF flow over the clo flow
'				api.data.calculate("F#" & Flow.Clo.Name & "= F#" & Flow.TF.Name)
				
'			Catch ex As Exception
				
'				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'			End Try
'End Sub

#End Region

#Region "Exit"

'		Private Sub CalculateExit(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
'			Try
'				Dim iScope As Integer = OwnershipClass.GetOwnershipInfoByName(si, api, globals, "Scope")
'				Dim flowDestMember As Member = Nothing

'				'Initialize the defined member classes
'				Dim Acct As AccountClass = Me.InitAccountClass(si, api, globals)
'				Dim Flow As FlowClass = Me.InitFlowClass(si, api, globals)				
''				brapi.errorlog.LogMessage(si, "scope " & iScope)
'				If iScope = FSKScope.SORTIE Or iScope = FSKScope.SORTANTE Then
'					flowDestMember = flow.Sor
'					globals.SetBoolValue("Exit", True)
'					Me.GetExitList(si,api,globals).TryAdd(api.Pov.Entity.MemberId, api.Members.GetMember(dimtypeid.Entity,api.Pov.Entity.MemberId))
'				ElseIf iScope = FSKScope.Merge Then
'					flowDestMember = flow.FUS
'				Else
'					Exit Sub
'				End If	
				
'				Dim Own As New OwnershipClass(si, api,globals)
''				Dim sPY As String = api.Members.GetMemberName(dimtypeid.Time, api.Time.GetLastPeriodInPriorYear)
				
'				Dim resultDataBuffer As DataBuffer
'				Dim sourceDataBuffer As DataBuffer
				
'				Dim originMember As Integer
'				Dim sExitRule As String = "Exit"
				
'				'Check, if it is for elimination (needs to run after the second pass) or the share
'				If (api.Pov.Cons.MemberId = DimConstants.ConsElimination) And (args.CalculateArgs.IsSecondPassEliminationCalc) Then
'					'Get the databuffer to delete existing contributions
'					sourceDataBuffer = api.Data.GetDataBufferUsingFormula("C#Share:F#" & Flow.TF.Name & " + C#Elimination:F#" & Flow.TF.Name)
'					resultDataBuffer = api.Data.GetDataBufferUsingFormula("C#Elimination")
'					'The origin dimension always needs to be elimination 
'					originMember = DimConstants.Elimination 
'				ElseIf (api.Pov.Cons.MemberId = DimConstants.Share) Then
'					'Get the databuffer to delete existing shares
'				 	sourceDataBuffer = api.Data.GetDataBufferUsingFormula("C#Share:F#" & Flow.TF.Name)
'					resultDataBuffer = api.Data.GetDataBufferUsingFormula("C#Share")
'					'No specific origin dimension needed. DimConstants.all will be replaced by the actual one
'					originMember = DimConstants.All
'				Else
'					'exit if none of these cases happens
'					Exit Sub
'				End If
				
'				'For IFRS5 the source databuffer is the same like the result databuffer
''				sourcedatabuffer.LogDataBuffer(api, "Hoi", 1000)
'				'DestinationInfo for later
'				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
				
'				If Not sourceDataBuffer Is Nothing Then
'					Dim ExCons As New ConsClass(si, globals, api, resultDataBuffer, bDoLog, False) 'The object for the book method
'					'Loop the Databuffer
'					For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
		
'						'Initialize the target account member object
'						Dim acctMember As Member = api.Members.GetMember(DimType.Account.Id, cell.DataBufferCellPk.AccountId)
'						Dim acctType As AccountType = api.Account.GetAccountType(cell.DataBufferCellPk.AccountId)
'						Dim acctDestMember As Member = Nothing

'						'Only eliminate if the account is a Balance Sheet account
'						If Not api.Members.IsBase(api.Pov.AccountDim.DimPk, acct.TopBS.MemberId, acctMember.MemberId) Then
'							Continue For 
'						End If
						
'						'In some cases we need to switch the sign
'						Dim signA As Decimal = 1
'						'Initialize the target flow member object
'						Dim flowMember As Member = api.Members.GetMember(DimType.flow.Id, cell.DataBufferCellPk.FlowId)
								
'						'Si le conte source est ...
'						'If the source account is ...

'						If acctType.Equals(accounttype.Asset) Then
'							signA *= -1 'if account is asset sign needs to change
'						End If
						
'						If (api.Pov.Cons.MemberId = DimConstants.Share) Then
'							If acctMember.MemberId.Equals(acct.RetEarn.MemberId) Or acctMember.MemberId.Equals(acct.ResEx.MemberId) Then
'							'Do nothing for profit and retained earnings  accounts
'								Continue For
'							ElseIf  acctMember.MemberId.Equals(acct.ResR.MemberId) Then 	
'								'Do not eliminate the Adjustments on Profit, but correct the Adjustments on Reserves
'								ExCons.Book(cell, acct.RsvR.MemberId, flowdestmember.MemberId,, originMember,,,,,,,,,-1,,sExitRule,10616)
'							ElseIf	acctMember.MemberId.Equals(acct.ResC.MemberId) Then
'								'Do not eliminate the CTA on Profit, but correct the CTA on Reserves
'								ExCons.Book(cell, acct.RsvC.MemberId, flowdestmember.MemberId,, originMember,,,,,,,,,-1,,sExitRule,10617)
'							Else
'								'Just eliminate the account
'								ExCons.Book(cell, acctMember.MemberId, flowdestmember.MemberId,, originMember,,,,,,,,,-1,,sExitRule,10618)
'							End If
'							'Use the Retained Earnings as plug account
'							ExCons.Book(cell, acct.RetEarn.MemberId, flowdestmember.MemberId,, originMember,,,,,,,,,signA,,sExitRule,10619)
'						ElseIf (api.Pov.Cons.MemberId = DimConstants.ConsElimination) Then
'							If acctMember.MemberId.Equals(acct.RsvG.MemberId) Or acctMember.MemberId.Equals(acct.ResG.MemberId) Then
'							'Do nothing for profit and retained earnings  accounts
'								Continue For
'							ElseIf acctMember.MemberId.Equals(acct.ResR.MemberId) Then	
'								'Do not eliminate the Adjustments on Profit, but correct the Adjustments on Reserves
'								ExCons.Book(cell, acct.RsvR.MemberId, flowdestmember.MemberId,, originMember,,,,,,,,,-1,,sExitRule,10620)
'							ElseIf acctMember.MemberId.Equals(acct.ResCG.MemberId) Then
'								'Do not eliminate the CTA on Profit, but correct the CTA on Reserves
'								ExCons.Book(cell, acct.RsvCG.MemberId, flowdestmember.MemberId,, originMember,,,,,,,,,-1,,sExitRule,10621)
'							ElseIf acctMember.MemberId.Equals(acct.ResM.MemberId) Then
'								'Do not eliminate the Min on Profit, but correct the Min on Reserves
'								ExCons.Book(cell, acct.RsvM.MemberId, flowdestmember.MemberId,, originMember,,,,,,,,,-1,,sExitRule,10622)
'							ElseIf acctMember.MemberId.Equals(acct.ResCM.MemberId)  Then
'								'Do not eliminate the Min CTA on Profit, but correct the Min CTA on Reserves
'								ExCons.Book(cell, acct.RsvCM.MemberId, flowdestmember.MemberId,, originMember,,,,,,,,,-1,,sExitRule,10623)
'							Else
'								'Just eliminate the account
'								ExCons.Book(cell, acctMember.MemberId, flowdestmember.MemberId,, originMember,,,,,,,,,-1,,sExitRule,10624)
'							End If	
'							'Use the Reserves Group as plug account
'							ExCons.Book(cell, acct.RsvG.MemberId, flowdestmember.MemberId,, originMember,,,,,,,,,signA,,sExitRule,10625)
'						End If
'					Next	'Each sourceCell As DataBufferCell In childDataBuffer.DataBufferCells.Values
'					'resultDataBuffer.LogDataBuffer(api, api.Pov.Entity.Name & api.Pov.Time.Name & api.Pov.Cons.Name & ": Manno", 1000)
'					'Write the data back to the cube
'					api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
'					'Write the logged transactio to the sql database
'					ExCons.WriteLogToTable
'				End If
				
'				'Copy the TF flow over the clo flow
'				api.data.calculate("F#" & Flow.Clo.Name & "= F#" & Flow.TF.Name)
				
'			Catch ex As Exception
				
'				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'			End Try
'End Sub
		
'		Private Sub CalculateExitIC(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
'			Try
'				Dim flowDestMember As Member = Nothing

'				'Initialize the defined member classes
'				Dim Acct As AccountClass = Me.InitAccountClass(si, api, globals)
'				Dim Flow As FlowClass = Me.InitFlowClass(si, api, globals)				

'				Dim resultDataBuffer As DataBuffer
'				Dim sourceDataBuffer As DataBuffer
				
'				Dim originMember As Integer
'				Dim sExitRule As String = "ExitIC"

'				If Not ((api.Pov.Cons.MemberId = DimConstants.ConsElimination) And (args.CalculateArgs.IsSecondPassEliminationCalc)) Then
'					Exit Sub
'				ElseIf Not globals.GetBoolValue("Exit", False) Then
'					Exit Sub
'				End If
'				Dim iclist As ConcurrentDictionary(Of Integer, member) = Me.GetExitList(si, api, globals)
				
'				resultDataBuffer = api.Data.GetDataBufferUsingFormula("C#Elimination")
'				Dim ExCons As New ConsClass(si, globals, api, resultDataBuffer, bDoLog, False) 'The object for the book method
'				For Each iCMember As Member In iclist.Values

'					Dim sExDesc As String = "Exiting company = " & ICMember.name

'					If OwnershipClass.GetOwnershipInfoByName(si, api, globals, "scope", iCMember.Name) = FSKScope.Merge Then 
'						flowdestmember = Flow.FUS
'					Else
'						flowdestmember = Flow.Sor
'					End If	
					
'					'Check, if it is for elimination (needs to run after the second pass) or the share
'					'Get the databuffer to delete existing contributions
'					sourceDataBuffer = api.Data.GetDataBufferUsingFormula("C#Elimination:I#" & ICMember.name)
'					'The origin dimension always needs to be elimination 
'					originMember = DimConstants.Elimination 
				 
'					'For IFRS5 the source databuffer is the same like the result databuffer
'	'				sourcedatabuffer.LogDataBuffer(api, "Hoi", 1000)
'					'DestinationInfo for later
'					Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
					
'					If Not sourceDataBuffer Is Nothing Then
'						'Loop the Databuffer
'						For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
			
'							'Initialize the target account member object
'							Dim acctMember As Member = api.Members.GetMember(DimType.Account.Id, cell.DataBufferCellPk.AccountId)
'							Dim acctType As AccountType = api.Account.GetAccountType(cell.DataBufferCellPk.AccountId)
'							Dim acctDestMember As Member = api.Account.GetPlugAccount(acctMember.MemberId)
'							If acctDestMember Is Nothing OrElse acctDestMember.MemberId = DimConstants.Unknown Then
'								Continue For
'							End If	

'							'Only eliminate if the account is a Balance Sheet account
'							If Not api.Members.IsBase(api.Pov.AccountDim.DimPk, acct.TopBS.MemberId, acctMember.MemberId) Then
'								Continue For 
'							End If
							
'							'In some cases we need to switch the sign
'							Dim signA As Decimal = 1
'							'Initialize the target flow member object
'							Dim flowMember As Member = api.Members.GetMember(DimType.flow.Id, cell.DataBufferCellPk.FlowId)
									
'							'Si le conte source est ...
'							'If the source account is ...

'							If acctType.Equals(accounttype.Asset) Then
'								signA *= -1 'if account is asset sign needs to change
'							End If
							
'							ExCons.Book(cell, acctMember.MemberId, flowdestmember.MemberId,iCMember.MemberId, originMember,,,,,,,,,-1,sExDesc,sExitRule,10626)
'							ExCons.Book(cell, acctDestMember.MemberId, flowdestmember.MemberId,dimconstants.None, originMember,,,,,,,,,signA,,sExitRule,10627)

'						Next	'Each sourceCell As DataBufferCell In childDataBuffer.DataBufferCells.Values
'						'Write the data back to the cube
'						api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
'						'Write the logged transactio to the sql database
'					End If
'				Next
'				ExCons.WriteLogToTable
				
'				'Copy the TF flow over the clo flow
'				api.data.calculate("F#" & Flow.Clo.Name & "= F#" & Flow.TF.Name)
				
'			Catch ex As Exception
				
'				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'			End Try
'End Sub

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
						Flow = New FlowClass (Si, Api)
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
						Account = New AccountClass (Si, Api)

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
					Me.iTopBS = api.Members.GetMemberID(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopBS", "XFW_FSK_Accounts", True))
					Me.iTopIS = api.Members.GetMemberID(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopIS", "XFW_FSK_Accounts", True))
				Else		
					Me.iTopBS = Acct.TopBS.MemberId
					Me.iTopIS = Acct.TopIS.MemberId
				End If	
			
				
				If bLog Then 
					'SB log = getLogTable(bDeleteRows)
				Else
					' SB deleteLogTable()
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
							'SB Me.deleteLogTable()
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
		Private referenceBR As New OneStream.BusinessRule.Finance.XFW_FSK_OpeningScenario.MainClass
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
		'Public Resultat As Member
		Public AdjCT As Member
		Public AssHeldfSale As Member
		Public AutoCtrl As Member
		Public DIVMEE As Member
		Public DefIncNC As Member
		Public DiscProfit As Member
		Public DiscTax As Member
		Public GWAMEE As Member
		Public GWMEE As Member
		Public Impots As Member
		Public LPTIT As Member
		Public LiabHeldfSale As Member
		Public MEE As Member
		Public MinInt As Member
		Public NetCash As Member
		Public PSTKEQUI As Member
		Public ProfitCT As Member
		Public ResC As Member
		Public ResCG As Member
		Public ResCM As Member
		Public ResEx As Member
		Public ResG As Member
		Public ResM As Member
		Public ResR As Member
		Public ResultatMEE As Member
		Public RetEarn As Member
		Public RsvC As Member
		Public RsvCG As Member
		Public RsvCM As Member
		Public RsvG As Member
		Public RsvM As Member
		Public RsvR As Member
		Public SubInvNC As Member
		Public Tax As Member
		Public TopAss As Member
		Public TopBS As Member
		Public TopEQ As Member
		Public TopEQG As Member
		Public TopEQM As Member
		Public TopIS As Member
		Public TopLiab As Member
		Public ResEq As Member
		Public RsvEq As Member
		Public RsvCEq As Member
		Public RsvH As Member
		Public ResH As Member
		Public EnlaDiv As Member
		
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
				Return New List(Of Integer) From {ResCG.MemberId, ResG.MemberId, RsvCG.MemberId, RsvG.MemberId}
			End Get			
		End Property

		Private si As SessionInfo
		Private api As FinanceRulesApi

		Sub New (ByRef oSi As SessionInfo, ByRef oApi As FinanceRulesApi)
			Try
				api = oApi
				si = oSi
			
				AdjCT = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "AdjCT", "XFW_FSK_Accounts", True))
				AssHeldfSale = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "AssHeldfSale", "XFW_FSK_Accounts", True))
				AutoCtrl = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "AutoCtrl", "XFW_FSK_Accounts", True))
				DIVMEE = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "DIVMEE", "XFW_FSK_Accounts", True))
				DefIncNC = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "DefIncNC", "XFW_FSK_Accounts", True))
				DiscProfit = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "DiscProfit", "XFW_FSK_Accounts", True))
				DiscTax = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "DiscTax", "XFW_FSK_Accounts", True))
				GWAMEE = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "GWAMEE", "XFW_FSK_Accounts", True))
				GWMEE = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "GWMEE", "XFW_FSK_Accounts", True))
				Impots = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "Impots", "XFW_FSK_Accounts", True))
				LPTIT = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "LPTIT", "XFW_FSK_Accounts", True))
				LiabHeldfSale = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "LiabHeldfSale", "XFW_FSK_Accounts", True))
				MEE = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "MEE", "XFW_FSK_Accounts", True))
				MinInt = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "MinInt", "XFW_FSK_Accounts", True))
				NetCash = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "NetCash", "XFW_FSK_Accounts", True))
				PSTKEQUI = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "PSTKEQUI", "XFW_FSK_Accounts", True))
				ProfitCT = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ProfitCT", "XFW_FSK_Accounts", True))
				ResC = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResC", "XFW_FSK_Accounts", True))
				ResCG = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResCG", "XFW_FSK_Accounts", True))
				ResCM = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResCM", "XFW_FSK_Accounts", True))
				ResEx = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResEx", "XFW_FSK_Accounts", True))
				ResG = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResG", "XFW_FSK_Accounts", True))
				ResM = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResM", "XFW_FSK_Accounts", True))
				ResR = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResR", "XFW_FSK_Accounts", True))
				ResultatMEE = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResultatMEE", "XFW_FSK_Accounts", True))
				RetEarn = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RetEarn", "XFW_FSK_Accounts", True))
				RsvC = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvC", "XFW_FSK_Accounts", True))
				RsvCG = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvCG", "XFW_FSK_Accounts", True))
				RsvCM = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvCM", "XFW_FSK_Accounts", True))
				RsvG = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvG", "XFW_FSK_Accounts", True))
				RsvM = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvM", "XFW_FSK_Accounts", True))
				RsvR = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvR", "XFW_FSK_Accounts", True))
				SubInvNC = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "SubInvNC", "XFW_FSK_Accounts", True))
				Tax = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "Tax", "XFW_FSK_Accounts", True))
				TopAss = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopAss", "XFW_FSK_Accounts", True))
				TopBS = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopBS", "XFW_FSK_Accounts", True))
				TopEQ = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopEQ", "XFW_FSK_Accounts", True))
				TopEQG = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopEQG", "XFW_FSK_Accounts", True))
				TopEQM = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopEQM", "XFW_FSK_Accounts", True))
				TopIS = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopIS", "XFW_FSK_Accounts", True))
				TopLiab = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "TopLiab", "XFW_FSK_Accounts", True))
				ResEq = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResEq", "XFW_FSK_Accounts", True))
				RsvEq = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvEq", "XFW_FSK_Accounts", True))
				RsvCEq = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvCEq", "XFW_FSK_Accounts", True))
				RsvH = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "RsvH", "XFW_FSK_Accounts", True))
				ResH = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "ResH", "XFW_FSK_Accounts", True))
				EnlaDiv = api.Members.GetMember(DimType.Account.Id, BRApi.Utilities.TransformText(si, "EnlaDiv", "XFW_FSK_Accounts", True))
				
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

		Public AFF As Member 
		Public AUG As Member 
		Public AUGCAP As Member
		Public CLO As Member
		Public DDEF As Member
		Public DEPA As Member
		Public DEPD As Member
		Public DEXP As Member
		Public DFIN As Member
		Public DI As Member
		Public DIMCAP As Member
		Public DIMI As Member 
		Public DISOP As Member 
		Public DIV As Member 
		Public DREVA As Member		
		Public ECF As Member 
		Public ECO As Member 
		Public ECR As Member
		Public ENT As Member 
		Public ERR As Member		
		Public FPA As Member		
		Public FUS As Member		
		Public IDEF As Member
		Public IDIV As Member
		Public IREVA As Member		
		Public MVT As Member
		Public NBV As Member		
		Public NONE As Member
		Public OUV As Member 
		Public REC As Member
		Public RES As Member 
		Public REXP As Member
		Public RFIN As Member
		Public SOR As Member 
		Public TF As Member
		Public VAR As Member
		Public VARC As Member 
		Public VARO As Member
		Public APE_TOT As Member
		
		Private si As SessionInfo
		Private api As FinanceRulesApi

		Sub New (ByRef oSi As SessionInfo, ByRef oApi As FinanceRulesApi)
			Try
				api = oApi
				si = oSi
				
				AFF = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "AFF", "XFW_FSK_FLOWS", True))
				AUG = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "AUG", "XFW_FSK_FLOWS", True))
				AUGCAP = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "AUGCAP", "XFW_FSK_FLOWS", True))
				CLO = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "CLO", "XFW_FSK_FLOWS", True))
				DDEF = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DDEF", "XFW_FSK_FLOWS", True))
				DEPA = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DEPA", "XFW_FSK_FLOWS", True))
				DEPD = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DEPD", "XFW_FSK_FLOWS", True))
				DEXP = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DEXP", "XFW_FSK_FLOWS", True))
				DFIN = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DFIN", "XFW_FSK_FLOWS", True))
				DI = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "PUBLIC", "XFW_FSK_FLOWS", True))
				DIMCAP = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DIMCAP", "XFW_FSK_FLOWS", True))
				DIMI = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DIM", "XFW_FSK_FLOWS", True))
				DISOP = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DISOP", "XFW_FSK_FLOWS", True))
				DIV = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DIV", "XFW_FSK_FLOWS", True))
				DREVA = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "DREVA", "XFW_FSK_FLOWS", True))
				ECF = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "ECF", "XFW_FSK_FLOWS", True))
				ECO = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "ECO", "XFW_FSK_FLOWS", True))
				ECR = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "ECR", "XFW_FSK_FLOWS", True))
				ENT = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "ENT", "XFW_FSK_FLOWS", True))
				ERR = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "ERR", "XFW_FSK_FLOWS", True))
				FPA = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "FPA", "XFW_FSK_FLOWS", True))
				FUS = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "FUS", "XFW_FSK_FLOWS", True))
				IDEF = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "IDEF", "XFW_FSK_FLOWS", True))
				IDIV = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "ADIV", "XFW_FSK_FLOWS", True))
				IREVA = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "IREVA", "XFW_FSK_FLOWS", True))
				MVT = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "MVT", "XFW_FSK_FLOWS", True))
				NBV = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "NBV", "XFW_FSK_FLOWS", True))
				NONE = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "NONE", "XFW_FSK_FLOWS", True))
				OUV = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "OUV", "XFW_FSK_FLOWS", True))
				REC = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "REC", "XFW_FSK_FLOWS", True))
				RES = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "RES", "XFW_FSK_FLOWS", True))
				REXP = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "REXP", "XFW_FSK_FLOWS", True))
				RFIN = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "RFIN", "XFW_FSK_FLOWS", True))
				SOR = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "SOR", "XFW_FSK_FLOWS", True))
				TF = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "TF", "XFW_FSK_FLOWS", True))
				VAR = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "VAR", "XFW_FSK_FLOWS", True))
				VARC = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "VARC", "XFW_FSK_FLOWS", True))
				VARO = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "VARO", "XFW_FSK_FLOWS", True))
				APE_TOT = api.Members.GetMember(DimType.flow.Id, BRApi.Utilities.TransformText(si, "APE_TOT", "XFW_FSK_FLOWS", True))
				
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
		Public ADIV As Member
		Public AEA As Member
		Public AUTOADJ As Member
		Public CAP As Member
		Public CAPFTA As Member
		Public CAPREV01 As Member
		Public CAPREV02 As Member
		Public CAPREV03 As Member
		Public DISCOP As Member
		Public DIV As Member
		Public EA As Member
		Public EQUITY As Member
		Public FIN As Member
		Public INDEB As Member
		Public INVP As Member
		Public MANADJ As Member
		Public MECB As Member
		Public MECS As Member
		Public MINORITAIRES As Member
		Public MMAR As Member
		Public MMAT As Member
		Public MPIF As Member
		Public OPE As Member
		Public PARC As Member
		Public PFIA As Member
		Public PINV As Member
		Public PMVCONV As Member
		Public PMVIC As Member
		Public PMVIF As Member
		Public PMVII As Member
		Public POTR As Member
		Public PROPE As Member
		Public PROPU As Member
		Public PSRI As Member
		Public PTRA As Member
		Public SUBCAP As Member
		Public TIT As Member
		
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
				
				ACIR = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "ACIR", "XFW_FSK_NATURE", True))
				ADIV = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "ADIV", "XFW_FSK_NATURE", True))
				AEA = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "AEA", "XFW_FSK_NATURE", True))
				AUTOADJ = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "AUTOADJ", "XFW_FSK_NATURE", True))
				CAP = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "CAP", "XFW_FSK_NATURE", True))
				CAPFTA = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "CAPFTA", "XFW_FSK_NATURE", True))
				CAPREV01 = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "CAPREV01", "XFW_FSK_NATURE", True))
				CAPREV02 = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "CAPREV02", "XFW_FSK_NATURE", True))
				CAPREV03 = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "CAPREV03", "XFW_FSK_NATURE", True))
				DISCOP = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "DISCOP", "XFW_FSK_NATURE", True))
				DIV = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "DIV", "XFW_FSK_NATURE", True))
				EA = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "EA", "XFW_FSK_NATURE", True))
				EQUITY = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "EQUITY", "XFW_FSK_NATURE", True))		
				FIN = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "FIN", "XFW_FSK_NATURE", True))
				INDEB = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "INDEB", "XFW_FSK_NATURE", True))
				INVP = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "INVP", "XFW_FSK_NATURE", True))
				MANADJ = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "MANADJ", "XFW_FSK_NATURE", True))
				MECB = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "MECB", "XFW_FSK_NATURE", True))
				MECS = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "MECS", "XFW_FSK_NATURE", True))
				MINORITAIRES = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "MINORITAIRES", "XFW_FSK_NATURE", True))		
				MMAR = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "MMAR", "XFW_FSK_NATURE", True))
				MMAT = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "MMAT", "XFW_FSK_NATURE", True))
				MPIF = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "MPIF", "XFW_FSK_NATURE", True))
				OPE = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "OPE", "XFW_FSK_NATURE", True))
				PARC = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PARC", "XFW_FSK_NATURE", True))
				PFIA = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PFIA", "XFW_FSK_NATURE", True))
				PINV = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PINV", "XFW_FSK_NATURE", True))
				PMVCONV = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PMVCONV", "XFW_FSK_NATURE", True))
				PMVIC = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PMVIC", "XFW_FSK_NATURE", True))
				PMVIF = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PMVIF", "XFW_FSK_NATURE", True))
				PMVII = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PMVII", "XFW_FSK_NATURE", True))
				POTR = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "POTR", "XFW_FSK_NATURE", True))
				PROPE = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PROPE", "XFW_FSK_NATURE", True))		
				PROPU = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PROPU", "XFW_FSK_NATURE", True))		
				PSRI = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PSRI", "XFW_FSK_NATURE", True))
				PTRA = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "PTRA", "XFW_FSK_NATURE", True))
				SUBCAP = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "SUBCAP", "XFW_FSK_NATURE", True))
				TIT = api.Members.GetMember(NatDimType.Id, BRApi.Utilities.TransformText(si, "TIT", "XFW_FSK_NATURE", True))
				
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
			
			If api.Members.HasChildren(api.pov.EntityDim.DimPk, iEntity)
				pConD   = Nothing
				pCon_1D = Nothing
				pOwnD   = Nothing
				pOwn_1D = Nothing

				pCon = 1
				pOwn = 1
				pMin = 0
				pCon_1 = 1
				pOwn_1 = 1
				pMin_1 = 0
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
			Else
				If sParent.Equals("") Then Parent = api.Pov.Parent.Name Else Parent = sParent
				Dim iParent As Integer = api.Members.GetMemberId(dimtypeid.Entity, Parent)
				
				If sTime.Equals("") Then Time = api.Pov.Time.Name Else Time = sTime
				If sScenario.Equals("") Then PerScenario = api.Pov.Scenario.Name Else PerScenario = sScenario
				If sOpnScenario.Equals("") Then OpnScenario = api.Pov.Scenario.Name Else OpnScenario = sOpnScenario
				
				If dicOwnershipCells Is Nothing Then
					dicOwnershipCells = Globals.GetObject("dicOwnershipCells")
					If dicOwnershipCells Is Nothing Then
						dicOwnershipCells = New ConcurrentDictionary(Of String, datacell)
						globals.SetObject("dicOwnershipCells", dicOwnershipCells)
						System.Threading.Thread.Sleep(50)
						dicOwnershipCells = Globals.GetObject("dicOwnershipCells")
					End If
				End If
							
				Dim sStdPov As String = ":V#YTD:C#Local:F#None:O#BeforeAdj:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
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
				
				If Not dicOwnershipCells.TryGetValue("A#PctCon" & sPerPov, pConD)      	Then pConD   = api.Data.GetDataCell("A#PctCon" & sPerPov)	: dicOwnershipCells.TryAdd("A#PctCon" & sPerPov, pConD)				
				If Not dicOwnershipCells.TryGetValue("A#PctOwn" & sPerPov, pOwnD)      	Then pOwnD   = api.Data.GetDataCell("A#PctOwn" & sPerPov)	: dicOwnershipCells.TryAdd("A#PctOwn" & sPerPov, pOwnD)
				If Not dicOwnershipCells.TryGetValue("A#Methode" & sPerPov, methodD)   	Then methodD = api.Data.GetDataCell("A#Methode" & sPerPov)   : dicOwnershipCells.TryAdd("A#Methode" & sPerPov, methodD)
				If Not dicOwnershipCells.TryGetValue("A#Scope" & sPerPov, scopeD)   	Then scopeD = api.Data.GetDataCell("A#Scope" & sPerPov)   : dicOwnershipCells.TryAdd("A#Scope" & sPerPov, scopeD)
				If Not dicOwnershipCells.TryGetValue("A#Discontinued" & sPerPov, discontinuedD) Then discontinuedD = api.Data.GetDataCell("A#Discontinued" & sPerPov)	: dicOwnershipCells.TryAdd("A#Discontinued" & sPerPov, discontinuedD)

				If Not dicOwnershipCells.TryGetValue("A#PctCon" & sOpnPov, pCon_1D)    	Then pCon_1D = api.Data.GetDataCell("A#PctCon" & sOpnPov)	: dicOwnershipCells.TryAdd("A#PctCon" & sOpnPov, pCon_1D)
				If Not dicOwnershipCells.TryGetValue("A#PctOwn" & sOpnPov, pOwn_1D)    	Then pOwn_1D = api.Data.GetDataCell("A#PctOwn" & sOpnPov)	: dicOwnershipCells.TryAdd("A#PctOwn" & sOpnPov, pOwn_1D)
				If Not dicOwnershipCells.TryGetValue("A#Methode" & sOpnPov, method_1D) 	Then method_1D = api.Data.GetDataCell("A#Methode" & sOpnPov)	: dicOwnershipCells.TryAdd("A#Methode" & sOpnPov, method_1D)			
				If Not dicOwnershipCells.TryGetValue("A#Discontinued" & sOpnPov, discontinued_1D) Then discontinued_1D = api.Data.GetDataCell("A#Discontinued" & sOpnPov)	: dicOwnershipCells.TryAdd("A#Discontinued" & sOpnPov, discontinued_1D)

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
			End If
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
			
			dicOwnershipCells = Globals.GetObject("dicOwnershipCells")
			If dicOwnershipCells Is Nothing Then
				dicOwnershipCells = New ConcurrentDictionary(Of String, datacell)
				globals.SetObject("dicOwnershipCells", dicOwnershipCells)
				System.Threading.Thread.Sleep(50)
				dicOwnershipCells = Globals.GetObject("dicOwnershipCells")
			End If
						
			Dim sStdPov As String = ":V#YTD:C#Local:F#None:O#BeforeAdj:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sPerPov As String = ":E#" & sEntity & ":I#" & sParent & ":S#" & sScenario & ":T#" & sTime & sStdPov

			If sName.XFEqualsIgnoreCase("pCon") Or sName.XFEqualsIgnoreCase("pMin") Then
				If Not dicOwnershipCells.TryGetValue("A#PctCon" & sPerPov, pConD) Then pConD   = api.Data.GetDataCell("A#PctCon" & sPerPov)	: dicOwnershipCells.TryAdd("A#PctCon" & sPerPov, pConD)				
			End If
			
			If sName.XFEqualsIgnoreCase("pOwn") Or sName.XFEqualsIgnoreCase("pMin") Then
				If Not dicOwnershipCells.TryGetValue("A#PctOwn" & sPerPov, pOwnD)      Then pOwnD   = api.Data.GetDataCell("A#PctOwn" & sPerPov)	: dicOwnershipCells.TryAdd("A#PctOwn" & sPerPov, pOwnD)
			End If	
			
			If sName.XFEqualsIgnoreCase("method") Then
				If Not dicOwnershipCells.TryGetValue("A#Methode" & sPerPov, methodD)   Then methodD = api.Data.GetDataCell("A#Methode" & sPerPov)   : dicOwnershipCells.TryAdd("A#Methode" & sPerPov, methodD)
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
				Return methodD.CellAmount
			ElseIf sName.XFEqualsIgnoreCase("scope")
				Return scopeD.CellAmount
			ElseIf sName.XFEqualsIgnoreCase("discontinued")
				Return discontinuedD.CellAmount
			Else
				Dim myEx As New exception("Incorrect Name: " & sName)
				brapi.ErrorLog.LogError(si, myEx)
				Return 0
			End If
		Catch ex As Exception
			
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try

	End Function
End Class

#End Region

'Support Constants
#Region "Methods"
' (1,2,3,4,5,6,7,8,9,10,11) 1 = HOLDING, 2 = IG, 3 = MEE, 4 = PROPORTIONNELLE, 5 = SORTANTE, 6 = SORTIE, 7 = NOCONSO, 8 = Discontinued, 9 = MERGED, 10 = EXITING EQUITY, 11 = EXITING DISCONTINUED
	Public Enum FSKMethod
		HOLDING = 1
		IG = 2
		MEE = 3
		IP = 4
		SORTANTE = 5
		SORTIE = 6
		NOCONSO = 7
		DIS = 8
		MER = 9
		EXMEE = 10
		EXDIS = 11
	End Enum
#End Region

#Region "Scope"
'ENTRY, MERGE, SORTIE, SORTANTE 10,20,30,40
	Public Enum FSKScope
		DefaultScope = 0
		Entry = 10
		Merge = 20
		SORTIE = 30
		SORTANTE = 40
	End Enum
#End Region

End Namespace