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

Imports System.Collections.Concurrent

Namespace OneStream.BusinessRule.Finance.XFW_FSK_NoInput
	
#Region "Main Class"		

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
						
'						Dim flow As Object = Me.InitFlowClass(si, api, globals)
'						Dim acct As Object = Me.InitAccountClass(si, api, globals)

'						Dim IsAdmin As Boolean = BRApi.Security.Authorization.IsUserInGroup(si, "Administrators")	
						
						'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX					
						'--------------------- NO INPUT - PARCHIS --------------------------------------------------------------------						
	
'						Dim sAccount As String = api.Pov.Account.Name				
												
''						If (bAplica)	
'						If (Not api.Entity.HasChildren())
							
'							Dim sEntity As String = api.Pov.Entity.Name
'							Dim sICP As String = api.Pov.IC.Name
'							Dim sUD1 As String = api.Pov.UD1.Name
'							Dim sUD2 As String = api.Pov.UD2.Name
'							Dim sUD7 As String = api.Pov.UD7.Name					
'							Dim sScenario As String = api.Pov.Scenario.Name
'							Dim sTime As String = api.Pov.Time.Name
'							Dim iYear As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Year
							
							
'							'CUENTAS CDR
'							Dim sAccountsRRHH As List(Of String) = New List(Of String) ({"RH001","RH024","RH007","RH007S","RH007U","RH007US"})
'							Dim sAccountsCR As List(Of String) = New List(Of String) ({"CR_A9","CR_D9","CR_E9"})
							
'							If (sAccount.StartsWith("6") _
'							Or sAccount.StartsWith("7") _
'							Or sAccountsRRHH.Contains(sAccount) _
'							Or sAccountsCR.Contains(sAccount))
							
'								'VALIDACIÓN 0 - Tiene que informarse el SITE a partir de 2023
'								If sUD7.Equals("None")
'									If(iYear >= 2023)									
'										Return 	ConditionalInputResultType.NoInput
'									Else ' Antes de 2023 se trata el none como Site General
'										sUD7 = "S101"
'									End If																		
'								End If
															
'								'VALIDACIÓN 1 - Parchis Entity-Site			
'								Dim Parchis_Entity_Site As Decimal = api.Data.GetDataCell("E#" & sEntity & ":S#" & sScenario & ":T#" & sTime & ":A#Parchis_Entity_Site:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#" & sUD7 & ":U8#None").CellAmount
								
'								If (Parchis_Entity_Site = 0) Then
'									Return 	ConditionalInputResultType.NoInput
'								Else
									
'									'VALIDACIÓN 2 - Parchis Site-BU			
'									Dim Parchis_Site_BU = api.Data.GetDataCell("E#" & sEntity & ":S#" & sScenario & ":T#" & sTime & ":A#Parchis_Site_BU:C#Local:V#YTD:F#None:O#Forms:I#None:U1#" & sUD1 & ":U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#" & sUD7 & ":U8#None").CellAmount

'									If (Parchis_Site_BU = 0) Then
'										Return 	ConditionalInputResultType.NoInput
'									Else
										
'										'VALIDACIÓN 3 - Parchis Site-BU (Para ICP y UD2)
'										Dim Parchis_Site_BU_ICP = api.Data.GetDataCell("E#" & sICP & ":S#" & sScenario & ":T#" & sTime & ":A#Parchis_Site_BU:C#Local:V#YTD:F#None:O#Forms:I#None:U1#" & sUD2 & ":U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#Top:U8#None").CellAmount
										
'										If (Parchis_Site_BU_ICP = 0 And sICP <> "None") Then
'											Return 	ConditionalInputResultType.NoInput									
'										End If
										
'									End If
									
'								End If	
				
'							'CUENTAS BALANCE Y RESTO DE CUENTAS
'							Else If (sAccount.StartsWith("1") _
'							Or sAccount.StartsWith("2") _
'							Or sAccount.StartsWith("3") _
'							Or sAccount.StartsWith("4") _
'							Or sAccount.StartsWith("5") _
'							Or sAccount.StartsWith("RH") _
'							Or sAccount.StartsWith("CD") _
'							Or sAccount.StartsWith("CR_") _
'							Or sAccount.StartsWith("CTRL") _
'							Or sAccount.StartsWith("CV") _
'							Or sAccount.StartsWith("INM"))
									
'								If IsAdmin = False Then	
									
'									'Si es vacío se comporta como el Site Gral
'									If sUD7.Equals("None")
'										sUD7 = "S101"
'									End If
									
'									'VALIDACIÓN 1 - Parchis Site-BU			
'									Dim Parchis_Site_BU = api.Data.GetDataCell("E#" & sEntity & ":S#" & sScenario & ":T#" & sTime & ":A#Parchis_Site_BU:C#Local:V#YTD:F#None:O#Forms:I#None:U1#" & sUD1 & ":U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#" & sUD7 & ":U8#None").CellAmount

'									If (Parchis_Site_BU = 0) Then
'										Return 	ConditionalInputResultType.NoInput
'									Else
										
'										'VALIDACIÓN 2 - Parchis Site-BU (Para ICP y UD2)
'										Dim Parchis_Site_BU_ICP = api.Data.GetDataCell("E#" & sICP & ":S#" & sScenario & ":T#" & sTime & ":A#Parchis_Site_BU:C#Local:V#YTD:F#None:O#Forms:I#None:U1#" & sUD2 & ":U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#Top:U8#None").CellAmount
										
'										If (Parchis_Site_BU_ICP = 0 And sICP <> "None") Then
'											Return 	ConditionalInputResultType.NoInput									
'										End If
										
'									End If
									
'								End If
								
'							End If	
						
'						End If
						
'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX					
'--------------------- NO INPUT - PARCHIS --------------------------------------------------------------------						

'		Dim IsAdmin As Boolean = BRApi.Security.Authorization.IsUserInGroup(si, "Administrators")

		
'		Dim sEntity As String = api.Pov.Entity.Name
'		Dim sICP As String = api.Pov.IC.Name
'		Dim sUD1 As String = api.Pov.UD1.Name
'		Dim sUD2 As String = api.Pov.UD2.Name
'		Dim sScenario As String = api.Pov.Scenario.Name
'		Dim sTime As String = api.Pov.Time.Name
		
'		If IsAdmin = False
		
'			'Generate intersections for Entity - UD1 combinations			
'			Dim Parchis_Entity = api.Data.GetDataCell("E#" & sEntity & ":S#" & sScenario & ":T#" & sTime & ":A#Parchis:C#Local:V#YTD:F#None:O#Forms:I#None:U1#" & sUD1 & ":U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount

'			'Generate intersections for ICP - UD2 combinations
'			Dim Parchis_ICP = api.Data.GetDataCell("E#" & sICP & ":S#" & sScenario & ":T#" & sTime & ":A#Parchis:C#Local:V#YTD:F#None:O#Forms:I#None:U1#" & sUD2 & ":U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount

'			If (Parchis_Entity = 0) Or (Parchis_ICP = 0) Then
'			'If (Parchis_Entity = 0) Then
				
'				Return 	ConditionalInputResultType.NoInput
				
'			Else
				
'				Return 	ConditionalInputResultType.Default
			
'			End If
		
'		End If


'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
'-------------------------------------------------------------------------------------------------------------						
						
'----------------------NO INPUT - BLOCK USERS -----------------------------------------------------------------						
							
'		Dim POVEntity As String = api.Pov.Entity.Name
'		Dim POVScenario As String = api.Pov.Scenario.name
'		Dim POVTime As String = api.Pov.Time.Name
				
				
'		Dim Block_Status = api.Data.GetDataCell("E#" & POVEntity & ":S#" & POVScenario & ":T#" & POVTime & ":A#Status_Bloqueo:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount				
'		Dim Block_Pack = api.Data.GetDataCell("E#" & POVEntity & ":S#" & POVScenario & ":T#" & POVTime & ":A#Status_Pack:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#Pack:U5#None:U6#None:U7#None:U8#None").CellAmount				

			
			
'				If IsAdmin = False Then							
'	'----------------------------------------------------------------------------					 
''Si los usuarios están bloqueados para carga normal

'					If Block_Status = 1 Then	
						
'						'Si el pack está bloqueado
'						If Block_Pack = 1 Then	
																																																																												
'							If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then												
'								Return 	ConditionalInputResultType.NoInput	
								
'							End If
							
						
'						'Si el pack NO está bloqueado
'						Else If Block_Pack = 0 Then
							
'							If Not api.Pov.UD4.Name.XFEqualsIgnoreCase("Pack")
							
'								If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then															
'									Return 	ConditionalInputResultType.NoInput	
									
'								End If
								
'							End If
							
'						End If
''----------------------------------------------------------------------------
''Si los usuarios NO están bloqueados para carga normal	

'					Else If Block_Status = 0 Then
						
'						'Si el pack está bloqueado
'						If Block_Pack = 1 Then	
							
'							If api.Pov.UD4.Name.XFEqualsIgnoreCase("Pack")
							
'								If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then
'									Return 	ConditionalInputResultType.NoInput
									
'								End If
								
'							End If						
						
'						End If																					
													
'					End If																																																	
'				End If					
						
				
'----------------------NO INPUT FORM & IMPORT -----------------------------------------------------------------
					
						'If api.pov.Origin.name.XFEqualsIgnoreCase("BeforeAdj") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then
						
							'No Input on Flow MVT ECF ECO ECR VARO VARC
							'If api.Pov.Flow.MemberId.Equals(flow.None.MemberId) Or api.Pov.Flow.MemberId.Equals(flow.ECF.MemberId) Or api.Pov.Flow.MemberId.Equals(flow.ECO.MemberId) _
								'Or api.Pov.Flow.MemberId.Equals(flow.ECR.MemberId) Or api.Pov.Flow.MemberId.Equals(flow.VARO.MemberId) Or api.Pov.Flow.MemberId.Equals(flow.VARC.MemberId) Then																	
								
								'Return ConditionalInputResultType.NoInput
					
								'End If	
								
								

'--------------------------XXXX Block load data in 6 & 7 Account groups if UD1 = UD2 = UD3 = None XXXX--------------------------------
		
'			Dim Time As String = api.Pov.Time.Name
'			Dim Year As Integer = Time.Substring(0,4)
			
'			'brapi.ErrorLog.LogMessage(si, Year)

'			If Year > 2020 Then
'				If ((api.Pov.Account.Name.StartsWith("6")) Or (api.Pov.Account.Name.StartsWith("7"))) Then
'					If (api.Pov.UD1.name.XFEqualsIgnoreCase("None") Or api.Pov.UD2.name.XFEqualsIgnoreCase("None")) Then					
'					Return ConditionalInputResultType.NoInput
'					End If
'				End If
'			End If


'--------------------------XXXX BLOCK CP_CG AND CID  XXXX--------------------------

'		Dim CP_CG_Base As List(Of member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk,api.Members.GetMemberId(dimtype.Account.Id,"CP_CG"))
'		Dim CID_Base As List(Of member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk,api.Members.GetMemberId(dimtype.Account.Id,"CID"))
'		'getAllmembers para la lista? (mirar la de entity)
'		'Dim IsAdminUser As Boolean = BRApi.Security.Authorization.IsUserInGroup(si, "Administrators")

		
'		'CP_CG
'		For Each CPCG_BaseAccount As Member In CP_CG_Base
									
'			'If api.Pov.Account.MemberId.Equals(CPCG_BaseAccount) Then
'			If api.Pov.Account.name.Equals(CPCG_BaseAccount) Then
																																											
'				If api.Pov.UD3.Name.XFEqualsIgnoreCase("None") Then
														
'					Return ConditionalInputResultType.NoInput
				
'				End If
				
'			End If	
			
'		Next
				
'		'CID
'		For Each CID_BaseAccount As Member In CID_Base
			
'			If api.Pov.Account.name.Equals(CID_BaseAccount) Then
													
'				If (Not api.Pov.UD3.Name.XFEqualsIgnoreCase("8080")) Then
					
'					brapi.ErrorLog.LogMessage(si, "entro5")
					
'					Return ConditionalInputResultType.NoInput
					
'				End If	
			
'			End If
																														
'		Next
			
'---------------------------- TEST TIME----------------------------

'		Dim IsAdmin As Boolean = BRApi.Security.Authorization.IsUserInGroup(si, "Administrators")
'		Dim EntityList As List(Of member) = api.Members.GetAllMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"LEGAL"))	
'		'Dim TimeParam As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "CONS_Time")
		
'		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
'		Dim myWorkflowUnitPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
'		Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, myWorkflowUnitPk.TimeKey)
'		Dim objTimeMemberSubComponents As TimeMemberSubComponents = BRApi.Finance.Time.GetSubComponentsFromName(si, wfTime)
'		Dim wfYear As String = objTimeMemberSubComponents.Year.ToString
'		Dim wfMes As String = objTimeMemberSubComponents.Month.ToString
'		Dim wfClusterPK As WorkflowUnitClusterPk = si.WorkflowClusterPk
'		Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
'		Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromID(si, si.WorkflowClusterPk.ScenarioKey)
'		Dim wfEntityName As String = BRApi.Workflow.Metadata.GetProfileEntities(si, wfClusterPK.ProfileKey)(0).EntityName
		
'			'brapi.ErrorLog.LogMessage(si, wfTime)
'			'brapi.ErrorLog.LogMessage(si, wfScenarioName)
				
		
'				For Each Entity As Member In EntityList
'					Dim Block_StatusR = api.Data.GetDataCell("E#" & Entity.Name.ToString & ":S#" & wfScenarioName & ":T#" & wfTime & ":A#Status_Bloqueo:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount									
					
'					'REAL
'				If IsAdmin = False Then
					
'					If ((api.Pov.Scenario.Name = wfScenarioName) And (api.Pov.Time.Name = wfTime))  Then
																																																																		
'							If Block_StatusR = 1 Then
																						
'								If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then
																																		
'									Return 	ConditionalInputResultType.NoInput																			
									
'								End If							
'							End If	
'						End If
					
'				End If
				
				
'				Next
					
																												
'--------------------------XXXX NOT USED - Block Users to avoid modify data in WF(Import/Form/Adj) when Admin is reviewing ---XXXX-----------------------------	

'		Dim IsAdmin As Boolean = BRApi.Security.Authorization.IsUserInGroup(si, "Administrators")
'		Dim EntityList As List(Of member) = api.Members.GetAllMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"LEGAL"))
		
		
'				For Each Entity As Member In EntityList
'					Dim Block_StatusR = api.Data.GetDataCell("E#" & Entity.Name.ToString & ":S#R:T#2015M12:A#Status_Bloqueo:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount				
'					Dim Block_StatusS = api.Data.GetDataCell("E#" & Entity.Name.ToString & ":S#S:T#2015M12:A#Status_Bloqueo:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
'					Dim Block_StatusO1 = api.Data.GetDataCell("E#" & Entity.Name.ToString & ":S#O1:T#2015M12:A#Status_Bloqueo:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
'					Dim Block_StatusO2 = api.Data.GetDataCell("E#" & Entity.Name.ToString & ":S#O2:T#2015M12:A#Status_Bloqueo:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
'					Dim Block_StatusO3 = api.Data.GetDataCell("E#" & Entity.Name.ToString & ":S#O3:T#2015M12:A#Status_Bloqueo:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
'					Dim Block_StatusP03 = api.Data.GetDataCell("E#" & Entity.Name.ToString & ":S#P03:T#2015M12:A#Status_Bloqueo:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
'					Dim Block_StatusP06 = api.Data.GetDataCell("E#" & Entity.Name.ToString & ":S#P06:T#2015M12:A#Status_Bloqueo:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
'					Dim Block_StatusP09 = api.Data.GetDataCell("E#" & Entity.Name.ToString & ":S#P09:T#2015M12:A#Status_Bloqueo:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
'					Dim Block_StatusP11 = api.Data.GetDataCell("E#" & Entity.Name.ToString & ":S#P11:T#2015M12:A#Status_Bloqueo:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
					
'					'REAL
'				If IsAdmin = False Then
'					If api.Pov.Scenario.Name = "R"
																													
'							If Block_StatusR = 1 Then
																						
'								If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then
																																		
'									Return 	ConditionalInputResultType.NoInput																			
									
'								End If							
'							End If
																										
'					'OBJETIVO
'					Else If api.Pov.Scenario.Name = "O1"							
																			
'							If Block_StatusO1 = 1 Then
																						
'								If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then
																																		
'									Return 	ConditionalInputResultType.NoInput																			
									
'								End If							
'							End If
												
						
'					Else If api.Pov.Scenario.Name = "O2"	
																									
'							If Block_StatusO2 = 1 Then
																						
'								If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then
																																		
'									Return 	ConditionalInputResultType.NoInput																			
									
'								End If							
'							End If
						
						
'					Else If api.Pov.Scenario.Name = "O3"	
																									
'							If Block_StatusO3 = 1 Then
																						
'								If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then
																																		
'									Return 	ConditionalInputResultType.NoInput																			
									
'								End If							
'							End If
																
					
'					'PREVISTO
'					Else If api.Pov.Scenario.Name = "P03"	
																									
'							If Block_StatusP03 = 1 Then
																						
'								If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then
																																		
'									Return 	ConditionalInputResultType.NoInput																			
									
'								End If							
'							End If						
						
'					Else If api.Pov.Scenario.Name = "P06"	
																									
'							If Block_StatusP06 = 1 Then
																						
'								If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then
																																		
'									Return 	ConditionalInputResultType.NoInput	
'								End If							
'							End If						
						
'					Else If api.Pov.Scenario.Name = "P09"							
																			
'							If Block_StatusP09 = 1 Then
																						
'								If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then
																																		
'									Return 	ConditionalInputResultType.NoInput
						
'								End If							
'							End If						
						
'					Else If api.Pov.Scenario.Name = "P11"							
																			
'							If Block_StatusP11 = 1 Then
																						
'								If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("Import") Or api.pov.Origin.name.XFEqualsIgnoreCase("Forms") Then
																																		
'									Return 	ConditionalInputResultType.NoInput		
'								End If							
'							End If
									
'					End If
'				End If
																											
'				Next				
											
'--------------------------XXXX-------------------------------------------------------------------XXXX-----------------------------									
								
'							'No Input and all Equity Group and Mino Accounts (10600T and 1060MT) 
'							If api.Members.IsBase(api.Pov.AccountDim.DimPk,acct.TopEQG.MemberId, api.Pov.Account.MemberId) Or api.Members.IsBase(api.Pov.AccountDim.DimPk,acct.TopEQM.MemberId, api.Pov.Account.MemberId)
'								Return ConditionalInputResultType.NoInput
							
'							End If
'							'No Input on flow RES and AFF for restult and retained earnings (120000.ENT,120000.RES, 120000.AFF 110000.AFF) 
'							If ((api.Pov.Flow.MemberId.Equals(flow.ENT.MemberId) Or api.Pov.Flow.MemberId.Equals(flow.AFF.MemberId) Or api.Pov.Flow.MemberId.Equals(flow.RES.MemberId)) And api.Pov.Account.MemberId.Equals(Acct.ResEx.MemberId)) _
'								Or (api.Pov.Flow.MemberId.Equals(flow.AFF.MemberId) And api.Pov.Account.MemberId.Equals(Acct.RetEarn.MemberId))
'								Return ConditionalInputResultType.NoInput
							
'							End If
							
'							'No input on Equity accounts calculated in Elimination: 261EQUI,2904EQUI,2040EQUI,880EQUI,PSTKEQUI
'							If api.Pov.Account.MemberId.Equals(Acct.MEE.MemberId) Or api.Pov.Account.MemberId.Equals(Acct.GWAMEE.MemberId) Or api.Pov.Account.MemberId.Equals(Acct.GWMEE.MemberId) _
'								Or api.Pov.Account.MemberId.Equals(Acct.ResultatMEE.MemberId) Or api.Pov.Account.MemberId.Equals(Acct.PSTKEQUI.MemberId)
																				
'								Return ConditionalInputResultType.NoInput
'							End If
						
							
'							'No input On all PlugAccounts used In P&L Or Balance sheet
										
'							Dim PlugAccNoinput As Integer
							
'							For Each PlugAccNoinput In Acct.PlugAccounts 'Public list of plug accounts
								
'								If api.Pov.Account.MemberId.Equals(PlugAccNoinput)																										
'									Return ConditionalInputResultType.NoInput
'								End If
'							Next
							
							
'							'No Input on any other UD4 than None 
'							If Not api.Pov.UD4.name.XFEqualsIgnoreCase("None") Then
'									Return ConditionalInputResultType.NoInput
'								End If	
								
						
'					'----------------------NO INPUT ADJINPUT excluding OWNERPOSTADJ ----------------------------------------------------------------	
						
						
'						If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") And Not api.pov.Cons.name.XFEqualsIgnoreCase("OwnerPostAdj") 
'							'Adjinput are Authorized on UD4 RETMAN only 
'							If Not api.Members.IsBase(api.Pov.UD4Dim.DimPk,api.Members.GetMemberId(Dimtype.UD4.Id,"RETMAN"),api.Pov.UD4.MemberId)
'								Return ConditionalInputResultType.NoInput
'							End If
									
									
'							'No Input on flow RES and AFF for restult and retained earnings (120000.ENT,120000.RES, 120000.AFF 110000.AFF) 
'							If ((api.Pov.Flow.MemberId.Equals(flow.ENT.MemberId) Or api.Pov.Flow.MemberId.Equals(flow.AFF.MemberId) Or api.Pov.Flow.MemberId.Equals(flow.RES.MemberId)) And api.Pov.Account.MemberId.Equals(Acct.ResEx.MemberId)) _
'								Or (api.Pov.Flow.MemberId.Equals(flow.AFF.MemberId) And api.Pov.Account.MemberId.Equals(Acct.RetEarn.MemberId))
'								Return ConditionalInputResultType.NoInput
							
'							End If					
								
							
''							'No Input and all Equity Group and Mino Accounts (10600T and 1060MT) 
							
''							If api.Members.IsBase(api.Pov.AccountDim.DimPk,acct.TopEQG.MemberId, api.Pov.Account.MemberId) Or api.Members.IsBase(api.Pov.AccountDim.DimPk,acct.TopEQM.MemberId, api.Pov.Account.MemberId)
''								Return ConditionalInputResultType.NoInput
							
''							End If
							
							
							
'						End If	
						
'					'----------------------NO INPUT ALL ADJUSTMENTS Levels----------------------------------------------------------	
						
'						If api.pov.Origin.name.XFEqualsIgnoreCase("AdjInput") Or api.pov.Origin.name.XFEqualsIgnoreCase("AdjConsolidated")
'							'CLO Closing flow
'							If api.Pov.Flow.MemberId.Equals(flow.Clo.MemberId) Then
'								Return ConditionalInputResultType.NoInput
'							End If		
							
'							'UD4 = None
'							If api.Pov.UD4.name.XFEqualsIgnoreCase("None") Then
'								Return ConditionalInputResultType.NoInput
'							End If		
							
'						End If
						
						
						
												
						Return ConditionalInputResultType.Default
						
					Case Is = FinanceFunctionType.CustomCalculate
						'If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Test") Then
						'api.Data.Calculate("A#Profit = A#Sales - A#Costs")
						'End If
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
	Sub CustomBlockAccess (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)
	
	
			Dim AdminGroup As Boolean = BRApi.Security.Authorization.IsUserInAdminGroup(si)					
			
					
				If AdminGroup = False Then
						
				'	Return ConditionalInputResultType.NoInput
										
				End If
	End Sub
	
#End Region

#Region "Helper Functions"				

		Private Function InitFlowClass (ByRef Si As SessionInfo, ByRef Api As FinanceRulesApi, ByVal Globals As BRGlobals) As Object
			Try
				Dim sGName As String = "FSKFlows"
				Dim Flow As Object = globals.GetObject(sGName)
				If Not Flow Is Nothing Then
					Return Flow
				Else
					Dim FSKM As New OneStream.BusinessRule.Finance.XFW_FSK_ConsolidationRules.MainClass	
					Flow = FSKM.InitFlowClass(Si, Api,Globals)
					'Flow = New FlowClass (Si, Api)
					globals.SetObject(sGName, Flow)
					System.Threading.Thread.Sleep(50)
					Return globals.GetObject(sGName)
				End If

			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function InitAccountClass (ByRef Si As SessionInfo, ByRef Api As FinanceRulesApi, ByVal Globals As BRGlobals) As Object
			Try
				Dim sGName As String = "FSKAccounts"
				Dim Account As Object = globals.GetObject(sGName)
				If Not Account Is Nothing Then
					Return Account
				Else
					Dim FSKM As New OneStream.BusinessRule.Finance.XFW_FSK_ConsolidationRules.MainClass	
					Account = FSKM.InitAccountClass(Si, Api,Globals)
					Return Account
'					Account = New AccountClass (Si, Api)
'					globals.SetObject(sGName, Account)
'					System.Threading.Thread.Sleep(50)
'					Return globals.GetObject(sGName)
				End If
				
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
	End Class
#End Region

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

End Namespace