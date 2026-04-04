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

Namespace OneStream.BusinessRule.Finance.XFW_FSK_MemberFormulas_20221114
	Public Class MainClass
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 		XFW_FSK_MemberFormulas
		'
		'Description:			Member formulas based on the Financial Starter Kit
		'
		'Usage:					The functions located in this business rules should be called from the memberformulas
		'						on the dimension member.
		'						The following formula can be use to call the function:
		'
		'						Dim FSK As New OneStream.BusinessRule.Finance.XFW_FSK_MemberFormulas.MainClass
		'						FSK.NameOfTheFunction(si, api, globals, args)
		'
		'Exemple :				Dim FSK As New OneStream.BusinessRule.Finance.XFW_FSK_MemberFormulas.MainClass
		'						FSK.FlowCarryForward(si, api, globals, args)
		'
		'Created By:			OneStream
		'Version: 				V1.0
		'Date Created:			01 June 2020
		'------------------------------------------------------------------------------------------------------------	
		
#Region "Main"
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object

			Try

				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.MemberList
						
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("ICList") Then
							
							Dim oMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim sQuery As String = args.MemberListArgs.NameValuePairs("Query")
							Dim oEntDimPK As DimPk = api.Pov.Entitydim.DimPk 
							Dim oEntMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(oEntDimPK, sQuery, Nothing)
							Dim oICMemberInfos As New List(Of MemberInfo)
							
							Dim entityMember As MemberInfo
							For Each ICMember As MemberInfo In oEntMemberInfos
							
								entityMember = New MemberInfo(api.Members.GetMember(dimtype.IC.Id, ICMember.Member.MemberId))
								oICMemberInfos.Add(entityMember)
								
							Next
							
							Dim oMemberList As New MemberList(oMemberListHeader, oICMemberInfos)
							Return oMemberList
						End If
						
					Case Is = FinanceFunctionType.Calculate
				
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "Member Formulas for Ownership Parameters"
	'Dim FSK As New OneStream.BusinessRule.Finance.XFW_FSK_MemberFormulas.MainClass
	'FSK.AcctParameter(si, api, globals, args, "PctCon")

	Sub AcctParameter (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, ByRef account As String, Optional ByRef number As Decimal = 100)
	
		If Not api.Members.GetMemberId(dimtypeid.Account, account).Equals(DimConstants.Unknown) Then
			If api.Entity.HasChildren Then
				api.Data.Calculate("V#YTD:A#" & account & ":F#None:O#Import:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = " & number.ToString(CultureInfo.InvariantCulture))
				
				For Each oParentMember As Member In api.Members.GetParents(api.Pov.EntityDim.DimPk, api.Pov.Entity.MemberId, False)
					If api.Entity.IsIC(oParentMember.MemberId) Then
						api.Data.Calculate("V#YTD:A#" & account & ":F#None:O#Import:I#" & oParentMember.Name & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = " & number.ToString(CultureInfo.InvariantCulture))
					End If	
				Next
	
			End If	
		End If
	End Sub

	'Dim FSK As New OneStream.BusinessRule.Finance.XFW_FSK_MemberFormulas.MainClass
	'FSK.AcctMinParameter(si, api, globals, args, "PctMin", "PctCon", "PctOwn")
	
	Sub AcctMinParameter (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, ByRef minAccount As String, ByRef conAccount As String, ByRef ownAccount As String)
		If Not (api.Members.GetMemberId(dimtypeid.Account, minAccount).Equals(DimConstants.Unknown) _
		 OrElse api.Members.GetMemberId(dimtypeid.Account, conAccount).Equals(DimConstants.Unknown)  _
		 OrElse api.Members.GetMemberId(dimtypeid.Account, ownAccount).Equals(DimConstants.Unknown)) Then
		api.data.calculate("A#" & minAccount & "=A#" & conAccount & "-A#" & ownAccount)
		End If
	End Sub
	
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

#Region "Member Formulas for Flows"
	'Dim FSK As New OneStream.BusinessRule.Finance.XFW_FSK_MemberFormulas.MainClass
	'FSK.FlowXXX(si, api, globals, args)

	Sub FlowCarryForward (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		'Calculate the Openings = CLO N-1 - Carry forward

		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		
			api.Data.ClearCalculatedData("F#" & Flow.Ouv.Name & "",True,True,True)
			api.Data.Calculate("F#" & Flow.Ouv.Name & " = F#" & Flow.Clo.Name & ":T#PovPriorYearM12")
			

	End Sub
	
	Sub FlowCarryForwardAPV (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		'Calculate the Openings = CLO N-1 - Carry forward

		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim CuentasCV As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "CV"))
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
		'Dim PriorYear As Integer = api.Time.GetYearFromId(timeId) - 1
		Dim Scenario As String = api.Pov.Scenario.Name
	 	Dim FX As New GlobalClass(si, api,args)
		
		Dim periodNumber As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Month
		'Adaptamos el multiplicador para P03, aplica desde Abril
		
		'Dimensiones para calcular el escenario Objetivo
		Dim DatoRealUlt = api.Data.GetDataCell("A#CR_Z:S#R:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
		Dim DatoPrevUlt11 = api.Data.GetDataCell("A#CR_Z:S#P11:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
		Dim DatoPrevUlt09 = api.Data.GetDataCell("A#CR_Z:S#P09:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
		Dim DatoPrevUlt06 = api.Data.GetDataCell("A#CR_Z:S#P06:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
		Dim DatoPrevUlt03 = api.Data.GetDataCell("A#CR_Z:S#P03:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
				
		
		If CurYear > 2019 Then
'				api.Data.ClearCalculatedData("F#" & Flow.Ouv.Name & "",True,True,True)
'				api.Data.Calculate("F#" & Flow.Ouv.Name & " = F#" & Flow.Clo.Name & ":T#PovPriorYearM12")
			    		
			If api.Cons.IsLocalCurrencyforEntity() Then
			' Calculamos APE CV y CF para Real (O;R;P;E)
				If Scenario = "R" Then 
					If 	Not api.Entity.HasChildren() Then
						'Dim DatoReal = api.Data.GetDataCell("A#CR_Z:S#R:T#Pov:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
						'If DatoReal > 0 Or DatoReal < 0 Then
							' Cartera de Ventas
							api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#R",True,True,True)
							api.Data.Calculate("F#APV:A#CV_Ventas:S#R = F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#R")	
							
							' Cartera de Facturación
							api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#R",True,True,True)
							api.Data.Calculate("F#APV:A#CV_Fact:S#R = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#R")
						'End If
					
					Else If api.Entity.HasChildren() Then
						api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#R",True,True,True)
						api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#R = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
						
						api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#R",True,True,True)
						api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#R = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
					End If
					
				'De momento solo hay dato en M12, hay que ver que se calcule en el resto de meses cuando mensualice		
				Else If Scenario = "P03" Then
					
					If periodNumber > 11 Then
					
						If 	Not api.Entity.HasChildren() Then
							'Dim DatoPrev3 = api.Data.GetDataCell("A#CR_Z:S#P03:T#Pov:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
							'If DatoPrev3 <> 0 Then
								' Cartera de Ventas
								api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#P03",True,True,True)
								api.Data.Calculate("F#APV:A#CV_Ventas:S#P03 = F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#R")	
									
								' Cartera de Facturación
								api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#P03",True,True,True)
								api.Data.Calculate("F#APV:A#CV_Fact:S#P03 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#R")
							'End If
						Else If api.Entity.HasChildren() Then
							api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#P03",True,True,True)
							api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#P03 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
							
							api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#P03",True,True,True)
							api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#P03 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
						End If
					
					End If
				
				Else If Scenario = "P06" Then
					
					If periodNumber > 11 Then
					
						If 	Not api.Entity.HasChildren() Then
							'Dim DatoPrev6 = api.Data.GetDataCell("A#CR_Z:S#P06:T#Pov:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
							'If DatoPrev6 <> 0 Then
								' Cartera de Ventas
								api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#P06",True,True,True)
								api.Data.Calculate("F#APV:A#CV_Ventas:S#P06 = F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#R")	
									
								' Cartera de Facturación
								api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#P06",True,True,True)
								api.Data.Calculate("F#APV:A#CV_Fact:S#P06 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#R")
							'End If
						Else If api.Entity.HasChildren() Then
							api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#P06",True,True,True)
							api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#P06 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
							
							api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#P06",True,True,True)
							api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#P06 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
						End If
					
					End If
				
				Else If Scenario = "P09" Then
					
					If periodNumber > 11 Then
					
							If 	Not api.Entity.HasChildren() Then
								'Dim DatoPrev9 = api.Data.GetDataCell("A#CR_Z:S#P09:T#Pov:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
								'If DatoPrev9 <> 0 Then
									' Cartera de Ventas
									api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#P09",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Ventas:S#P09 = F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#R")	
										
									' Cartera de Facturación
									api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#P09",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Fact:S#P09 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#R")
								'End If
							Else If api.Entity.HasChildren() Then
								api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#P09",True,True,True)
								api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#P09 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
								
								api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#P09",True,True,True)
								api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#P09 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
						End If
					End If
					
				Else If Scenario = "P11" Then
					
					If periodNumber > 11 Then
					
						If 	Not api.Entity.HasChildren() Then
							'Dim DatoPrev11 = api.Data.GetDataCell("A#CR_Z:S#P11:T#Pov:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
							'If DatoPrev11 <> 0 Then
								' Cartera de Ventas
								api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#P11",True,True,True)
								api.Data.Calculate("F#APV:A#CV_Ventas:S#P11 = F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#R")	
									
								' Cartera de Facturación
								api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#P11",True,True,True)
								api.Data.Calculate("F#APV:A#CV_Fact:S#P11 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#R")
							'End If
						Else If api.Entity.HasChildren() Then
							api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#P11",True,True,True)
							api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#P11 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
							
							api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#P11",True,True,True)
							api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#P11 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
						End If
					
					End If
					
				
				'De momento solo hay dato en M12, hay que ver que se calcule en el resto de meses cuando mensualice	
				' Si el ultimo mes real con dato es M12 y si hay dato en la CRZ de objetivo		
				Else If Scenario = "O1" Then
						'Dim DatoObj1 = api.Data.GetDataCell("A#CR_Z:S#O1:T#Pov:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
							'If DatoObj1 <> 0 Then
							If periodNumber > 11 Then
								If  DatoRealUlt <> 0 Then
									If 	Not api.Entity.HasChildren() Then
										' Cartera de Ventas
										api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O1",True,True,True)
										api.Data.Calculate("F#APV:A#CV_Ventas:S#O1= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#R")	
										
										' Cartera de Facturación
										api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O1",True,True,True)
										api.Data.Calculate("F#APV:A#CV_Fact:S#O1 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#R")
									Else If api.Entity.HasChildren() Then
										api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O1",True,True,True)
										api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O1 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
										
										api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O1",True,True,True)
										api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O1 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
									End If
								
								Else If DatoPrevUlt11 <> 0 Then
									If 	Not api.Entity.HasChildren() Then
										' Cartera de Ventas
										api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O1",True,True,True)
										api.Data.Calculate("F#APV:A#CV_Ventas:S#O1= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#P11")	
										
										' Cartera de Facturación
										api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O1",True,True,True)
										api.Data.Calculate("F#APV:A#CV_Fact:S#O1 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#P11")
									Else If api.Entity.HasChildren() Then
										api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O1",True,True,True)
										api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O1 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#P11")
										
										api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O1",True,True,True)
										api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O1 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#P11")
									End If
									
								Else If DatoPrevUlt09 <> 0 Then
									If 	Not api.Entity.HasChildren() Then
										' Cartera de Ventas
										api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O1",True,True,True)
										api.Data.Calculate("F#APV:A#CV_Ventas:S#O1= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#P09")	
										
										' Cartera de Facturación
										api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O1",True,True,True)
										api.Data.Calculate("F#APV:A#CV_Fact:S#O1 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#P09")
									Else If api.Entity.HasChildren() Then
										api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O1",True,True,True)
										api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O1 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#P09")
										
										api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O1",True,True,True)
										api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O1 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#P09")
									End If
									
								Else If DatoPrevUlt06 <> 0 Then
									If 	Not api.Entity.HasChildren() Then
										' Cartera de Ventas
										api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O1",True,True,True)
										api.Data.Calculate("F#APV:A#CV_Ventas:S#O1= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#P06")	
										
										' Cartera de Facturación
										api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O1",True,True,True)
										api.Data.Calculate("F#APV:A#CV_Fact:S#O1 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#P06")
									Else If api.Entity.HasChildren() Then
										api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O1",True,True,True)
										api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O1 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#P06")
										
										api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O1",True,True,True)
										api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O1 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#P06")
									End If
									
								Else If DatoPrevUlt03 <> 0 Then
									If 	Not api.Entity.HasChildren() Then
										' Cartera de Ventas
										api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O1",True,True,True)
										api.Data.Calculate("F#APV:A#CV_Ventas:S#O1= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#P03")	
										
										' Cartera de Facturación
										api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O1",True,True,True)
										api.Data.Calculate("F#APV:A#CV_Fact:S#O1 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#P03")
									Else If api.Entity.HasChildren() Then
										api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O1",True,True,True)
										api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O1 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#P03")
										
										api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O1",True,True,True)
										api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O1 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#P03")
									End If
								End If
							End If
				
				Else If Scenario = "O2" Then
					'Dim DatoObj2 = api.Data.GetDataCell("A#CR_Z:S#O2:T#Pov:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
						'If DatoObj2 <> 0 Then
						If periodNumber > 11 Then
							If  DatoRealUlt <> 0 Then
								If 	Not api.Entity.HasChildren() Then
									' Cartera de Ventas
									api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O2",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Ventas:S#O2= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#R")	
									
									' Cartera de Facturación
									api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O2",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Fact:S#O2 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#R")
								Else If api.Entity.HasChildren() Then
									api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O2",True,True,True)
									api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O2 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
										
									api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O2",True,True,True)
									api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O2 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
								End If
							
							Else If DatoPrevUlt11 <> 0 Then
								If 	Not api.Entity.HasChildren() Then
									' Cartera de Ventas
									api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O2",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Ventas:S#O2= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#P11")	
									
									' Cartera de Facturación
									api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O2",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Fact:S#O2 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#P11")
								Else If api.Entity.HasChildren() Then
									api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O2",True,True,True)
									api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O2 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#P11")
										
									api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O2",True,True,True)
									api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O2 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#P11")
								End If
								
							Else If DatoPrevUlt09 <> 0 Then
								If 	Not api.Entity.HasChildren() Then
									' Cartera de Ventas
									api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O2",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Ventas:S#O2= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#P09")	
									
									' Cartera de Facturación
									api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O2",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Fact:S#O2 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#P09")
								Else If api.Entity.HasChildren() Then
									api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O2",True,True,True)
									api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O2 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#P09")
										
									api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O2",True,True,True)
									api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O2 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#P09")
								End If
								
							Else If DatoPrevUlt06 <> 0 Then
								If 	Not api.Entity.HasChildren() Then
									' Cartera de Ventas
									api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O2",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Ventas:S#O2= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#P06")	
									
									' Cartera de Facturación
									api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O2",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Fact:S#O2 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#P06")
								Else If api.Entity.HasChildren() Then
									api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O2",True,True,True)
									api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O2 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#P06")
										
									api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O2",True,True,True)
									api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O2 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#P06")
								End If
								
							Else If DatoPrevUlt03 <> 0 Then
								If 	Not api.Entity.HasChildren() Then
									' Cartera de Ventas
									api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O2",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Ventas:S#O2= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#P03")	
									
									' Cartera de Facturación
									api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O2",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Fact:S#O2 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#P03")
								Else If api.Entity.HasChildren() Then
									api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O2",True,True,True)
									api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O2 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#P03")
										
									api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O2",True,True,True)
									api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O2 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#P03")
								End If
							End If
						End If
				Else If Scenario = "O3" Then
					'Dim DatoObj3 = api.Data.GetDataCell("A#CR_Z:S#O3:T#Pov:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
						'If DatoObj3 <> 0 Then
						If periodNumber > 11 Then
							If  DatoRealUlt <> 0 Then
								If 	Not api.Entity.HasChildren() Then
									' Cartera de Ventas
									api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O3",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Ventas:S#O3= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#R")	
									
									' Cartera de Facturación
									api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O3",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Fact:S#O3 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#R")
								Else If api.Entity.HasChildren() Then
									api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O3",True,True,True)
									api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O3 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
										
									api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O3",True,True,True)
									api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O3 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#R")
								End If
							
							Else If DatoPrevUlt11 <> 0 Then
								If 	Not api.Entity.HasChildren() Then
									' Cartera de Ventas
									api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O3",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Ventas:S#O3= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#P11")	
									
									' Cartera de Facturación
									api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O3",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Fact:S#O3 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#P11")
								Else If api.Entity.HasChildren() Then
									api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O3",True,True,True)
									api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O3 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#P11")
										
									api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O3",True,True,True)
									api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O3 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#P11")
								End If
								
							Else If DatoPrevUlt09 <> 0 Then
								If 	Not api.Entity.HasChildren() Then
									' Cartera de Ventas
									api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O3",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Ventas:S#O3= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#P09")	
									
									' Cartera de Facturación
									api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O3",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Fact:S#O3 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#P09")
								Else If api.Entity.HasChildren() Then
									api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O3",True,True,True)
									api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O3 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#P09")
										
									api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O3",True,True,True)
									api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O3 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#P09")
								End If
								
							Else If DatoPrevUlt06 <> 0 Then
								If 	Not api.Entity.HasChildren() Then
									' Cartera de Ventas
									api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O3",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Ventas:S#O3= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#P06")	
									
									' Cartera de Facturación
									api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O3",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Fact:S#O3 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#P06")
								Else If api.Entity.HasChildren() Then
									api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O3",True,True,True)
									api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O3 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#P06")
										
									api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O3",True,True,True)
									api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O3 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#P06")
								End If
								
							Else If DatoPrevUlt03 <> 0 Then
								If 	Not api.Entity.HasChildren() Then
									' Cartera de Ventas
									api.Data.ClearCalculatedData("F#APV:A#CV_Ventas:S#O3",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Ventas:S#O3= F#SDC:A#CV_Ventas:T#PovPriorYearM12:S#P03")	
									
									' Cartera de Facturación
									api.Data.ClearCalculatedData("F#APV:A#CV_Fact:S#O3",True,True,True)
									api.Data.Calculate("F#APV:A#CV_Fact:S#O3 = F#SDC:A#CV_Fact:T#PovPriorYearM12:S#P03")
								Else If api.Entity.HasChildren() Then
									api.Data.ClearCalculatedData("A#CV001:F#None:O#AdjInput:S#O3",True,True,True)
									api.Data.Calculate("A#CV001:F#None:O#AdjInput:S#O3 = A#CV004:F#None:O#AdjInput:T#PovPriorYearM12:S#P03")
										
									api.Data.ClearCalculatedData("A#CV006:F#None:O#AdjInput:S#O3",True,True,True)
									api.Data.Calculate("A#CV006:F#None:O#AdjInput:S#O3 = A#CV010:F#None:O#AdjInput:T#PovPriorYearM12:S#P03")
								End If
							End If
						End If
					
				'Else If Scenario = "E" Then
				End If 
									
						
				
			End If
		
'				api.Data.ClearCalculatedData("F#APV:A#CV_Ventas",True,True,True)
'				api.Data.Calculate("F#APV:A#CV_Ventas = F#SDC:A#CV_Ventas:T#PovPriorYearM12")
				
'				api.Data.ClearCalculatedData("F#APV:A#CV_Fact",True,True,True)
'				api.Data.Calculate("F#APV:A#CV_Fact = F#SDC:A#CV_Fact:T#PovPriorYearM12")
		
		'cuando hay asientos en "Legal" también se deben arrastrar
		
		End If
	
			
		If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
			If Scenario = "R" Or Scenario = "R_Extrapolado" Then 
				Dim sExpression As String = "F#APV{0}=F#APV:C#Local{1} * " & FX.closingRatePriorYEtoText
				api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
				api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			Else If Scenario = "P03" Or Scenario = "P06" Or Scenario = "P09" Or Scenario = "P11" Then
				'En PXX, el TC de APE es el cierre real de M12 del año anterior
				Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
				Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
				Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
				Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
				Dim scenariobase As String = api.pov.scenario.name
					
				Dim sExpression As String = "F#APV{0}=F#APV:S#" & scenariobase & ":C#Local{1} * " & closingRatePriorRealToText
				api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
				api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			Else If Scenario = "O1" Or Scenario = "O2" Or Scenario = "O3" Then
				'En OX, el TC de APE depende del dato de partida
				If  DatoRealUlt <> 0 Then
					Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
					Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
					Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
					Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
					Dim scenariobase As String = api.pov.scenario.name
					
					Dim sExpression As String = "F#APV{0}=F#APV:S#" & scenariobase & ":C#Local{1} * " & closingRatePriorRealToText
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				Else If DatoPrevUlt11 <> 0 Then
					Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
					Dim rateTypeCloP11 As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242899) 'CubeId (CONSO), ScenarioId (Real)
					Dim closingRateP11 As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeCloP11,timeyearend)
					Dim closingRatePriorP11ToText As String = closingRateP11.ToString(CultureInfo.InvariantCulture)
					Dim scenariobase As String = api.pov.scenario.name
					
					Dim sExpression As String = "F#APV{0}=F#APV:S#" & scenariobase & ":C#Local{1} * " & closingRatePriorP11ToText
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				Else If DatoPrevUlt09 <> 0 Then
					Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
					Dim rateTypeCloP09 As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242898) 'CubeId (CONSO), ScenarioId (Real)
					Dim closingRateP09 As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeCloP09,timeyearend)
					Dim closingRatePriorP09ToText As String = closingRateP09.ToString(CultureInfo.InvariantCulture)
					Dim scenariobase As String = api.pov.scenario.name
					
					Dim sExpression As String = "F#APV{0}=F#APV:S#" & scenariobase & ":C#Local{1} * " & closingRatePriorP09ToText
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				Else If DatoPrevUlt06 <> 0 Then
					Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
					Dim rateTypeCloP06 As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242897) 'CubeId (CONSO), ScenarioId (Real)
					Dim closingRateP06 As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeCloP06,timeyearend)
					Dim closingRatePriorP06ToText As String = closingRateP06.ToString(CultureInfo.InvariantCulture)
					Dim scenariobase As String = api.pov.scenario.name
					
					Dim sExpression As String = "F#APV{0}=F#APV:S#" & scenariobase & ":C#Local{1} * " & closingRatePriorP06ToText
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				Else If DatoPrevUlt03 <> 0 Then
					Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
					Dim rateTypeCloP03 As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242907) 'CubeId (CONSO), ScenarioId (Real)
					Dim closingRateP03 As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeCloP03,timeyearend)
					Dim closingRatePriorP03ToText As String = closingRateP03.ToString(CultureInfo.InvariantCulture)
					Dim scenariobase As String = api.pov.scenario.name
					
					Dim sExpression As String = "F#APV{0}=F#APV:S#" & scenariobase & ":C#Local{1} * " & closingRatePriorP03ToText
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				End If
			End If
		End If

	End Sub
	
	Sub APC (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
		Dim FX As New GlobalClass(si, api,args)

		If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
			If api.Pov.Scenario.Name = "R" Then 
				Dim sExpression As String = "F#APC{0}=F#APC:C#Local{1} * " & FX.closingRatePriorYEtoText
				api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
				api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				
			End If
		End If
		
	End Sub

	Function FlowCarryForwardDDP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult

		Dim Flow As Object = Me.InitFlowClass(si, api, globals)

		'Simple formula drill down
		Dim result As New DrillDownFormulaResult()

		'Define Explanation to be shown in drill title bar
		result.Explanation = "Carry Forward"

		'Add a row below per each drill result to be shown 
		result.SourceDataCells.Add("F#" & Flow.Clo.Name & ":T#PovPriorYearM12")
		
		Return result
	End Function
	
	Sub FlowCalcError (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		'Calculate the error flow - Calculation of ERR flow

		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		
		If api.Cons.IsLocalCurrencyforEntity() And Not api.Entity.HasChildren() Then
			api.Data.Calculate("F#" & Flow.ERR.Name & " = F#" & Flow.Clo.Name & " - F#" & Flow.TF.Name)
		End If
	End Sub

	Function FlowCalcErrorDDP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)

		'Simple formula drill down
		Dim result As New DrillDownFormulaResult()

		'Define Explanation to be shown in drill title bar
		result.Explanation = "Calc Error"

		'Add a row below per each drill result to be shown 
		result.SourceDataCells.Add("F#" & Flow.Clo.Name)
		result.SourceDataCells.Add("F#" & Flow.TF.Name)
		
		Return result
	End Function

	Sub flowAff(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		Dim CurrTime As String = api.Pov.Time.Name
		Dim CurrYear As Integer = TimeDimHelper.GetSubComponentsFromName(CurrTime).Year
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim FX As New GlobalClass(si, api,args)

		If Not api.Entity.HasChildren() Then
			If api.Cons.IsLocalCurrencyforEntity() Then
				'fs for accounts with Text1=DBE_AUT, movement is calculated in F#DBE (any movement is conisdered as profit distribtion to be translated at prior year Avg rate)
				Dim CuentasDBE As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "Capital"))
				Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
				Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
	 			If CurYear > 2019 Then
					For Each Account As Member In CuentasDBE 
						If api.Account.Text(Account.MemberId, 1) = "DBE_AUT" Then

							api.Data.ClearCalculatedData("A#"+Account.Name.ToString+":O#Forms:F#" & flow.Mvt.Name & "",True,True,True)
							api.Data.ClearCalculatedData("A#"+Account.Name.ToString+":O#Import:F#" & flow.Mvt.Name & "",True,True,True)

							api.Data.ClearCalculatedData("A#"+Account.Name.ToString+":O#Forms:F#" & flow.Aff.Name & "",True,True,True)
						 	api.Data.Calculate("A#"+Account.Name.ToString+":O#Forms:F#" & flow.Aff.Name & "=RemoveZeros(A#"+Account.Name.ToString+":O#Forms:F#" & flow.CLO.Name & ")-RemoveZeros(A#"+Account.Name.ToString+":O#Forms:F#" & flow.TF.Name & ")" & "")
							api.Data.ClearCalculatedData("A#"+Account.Name.ToString+":O#Import:F#" & flow.Aff.Name & "",True,True,True)
						 	api.Data.Calculate("A#"+Account.Name.ToString+":O#Import:F#" & flow.Aff.Name & "=RemoveZeros(A#"+Account.Name.ToString+":O#Import:F#" & flow.CLO.Name & ")-RemoveZeros(A#"+Account.Name.ToString+":O#Import:F#" & flow.TF.Name & ")" & "")
						End If
					Next
			 	End If
			ElseIf (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
				Dim sExpression As String = "F#" & Flow.Aff.Name & "{0}=F#" & Flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText
				api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
				api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				
			ElseIf  (api.Pov.Cons.Name = "OwnerPostAdj") Then
		
				Dim Account As Object = Me.InitAccountClass(si,api, globals)

				'APE reclass to DBE and moved to 120AA at OwnerPostAdj level- fs20201218
				api.Data.ClearCalculatedData("A#" & Account.ResG.name & ":O#AdjInput:F#" & Flow.Aff.Name & "",True,True,True)
				api.Data.Calculate("A#" & Account.ResG.name & ":O#AdjInput:F#" & Flow.Aff.Name & " = -A#" & Account.ResG.name & ":O#AdjInput:F#" & Flow.Ouv.Name)
				api.Data.ClearCalculatedData("A#" & Account.ResM.name & ":O#AdjInput:F#" & Flow.Aff.Name & "",True,True,True)
				api.Data.Calculate("A#" & Account.ResM.name & ":O#AdjInput:F#" & Flow.Aff.Name & " = -A#" & Account.ResM.name & ":O#AdjInput:F#" & Flow.Ouv.Name)
				api.Data.ClearCalculatedData("A#" & Account.RsvG.name & ":O#AdjInput:F#" & Flow.Aff.Name & "",True,True,True)
				api.Data.Calculate("A#" & Account.RsvG.name & ":O#AdjInput:F#" & Flow.Aff.Name & " = -A#" & Account.ResG.name & ":O#AdjInput:F#" & Flow.Aff.Name)
				api.Data.ClearCalculatedData("A#" & Account.RsvM.name & ":O#AdjInput:F#" & Flow.Aff.Name & "",True,True,True)
				api.Data.Calculate("A#" & Account.RsvM.name & ":O#AdjInput:F#" & Flow.Aff.Name & " = -A#" & Account.ResM.name & ":O#AdjInput:F#" & Flow.Aff.Name)
			End If
		End If
	End Sub

	Function FlowAffDDP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim FX As New GlobalClass(si, api,args)
		Dim result As New DrillDownFormulaResult()

		If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
			Return result
		ElseIf (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
				
			'Define Explanation to be shown in drill title bar
			result.Explanation = "Calc AFF with average Rate Prior Year End: "  & FX.averageRatePriorYEtoText

			'Add a row below per each drill result to be shown 
			result.SourceDataCells.Add("F#" & Flow.Aff.Name & ":C#Local")
			Return result
		Else
			Return result			
		End If
	End Function

	Sub flowDiv(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim FX As New GlobalClass(si, api,args)
		If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
				   
	    'Translate AFF flow at Prior Year End Average Rate
		'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
		'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"
		Dim sExpression As String = "F#" & flow.Div.Name & "{0}=F#" & flow.Div.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
		
		End If
	End Sub

	Function flowDivDDP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim FX As New GlobalClass(si, api,args)

		Dim result As New DrillDownFormulaResult()
		If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then

			'Define Explanation to be shown in drill title bar
			result.Explanation = "Calc Div with average Rate: "  & FX.averageRatePriorYEtoText

			'Add a row below per each drill result to be shown 
			result.SourceDataCells.Add("F#" & Flow.Div.Name & ":C#Local")
			
			Return result
		End If
		Return result
	End Function


	Sub flowRes(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		Dim Account As Object = Me.InitAccountClass(si,api, globals)
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)

		If Not api.Entity.HasChildren() Then
			If  api.Cons.IsLocalCurrencyforEntity() Then
				'fs se calcula en la cuenta 129000, delimitada por las dim UD1-UD3 (none en destino, Top en origen)
				'api.Data.Calculate("A#" & Account.ResEx.name & ":F#" & Flow.Res.Name & "= A#" & Account.TopIS.Name & ":F#" & Flow.None.Name & "")	
				api.Data.Calculate("A#" & Account.ResR.name & ":O#AdjInput:F#" & Flow.Res.Name & ":I#None=RemoveZeros(A#" & Account.TopIS.Name & ":O#AdjInput:F#" & Flow.None.Name & ":I#Top)")
			'ElseIf  (api.Pov.Cons.Name = "OwnerPostAdj") Or (api.Pov.Cons.Name = "OwnerPreAdj") Or api.cons.IsForeignCurrencyForEntity Then
			ElseIf  (api.Pov.Cons.Name = "OwnerPreAdj") Or api.cons.IsForeignCurrencyForEntity Then
				api.Data.ClearCalculatedData("A#" & Account.ResR.name & ":O#AdjInput:F#" & Flow.Res.Name & "",True,True,True)
				api.Data.Calculate("A#" & Account.ResR.name & ":O#AdjInput:F#" & Flow.Res.Name & ":I#None=RemoveZeros(A#" & Account.TopIS.Name & ":O#AdjInput:F#" & Flow.None.Name & ":I#Top)")
			ElseIf  (api.Pov.Cons.Name = "OwnerPostAdj") Then

'				Dim Entity As String = api.Pov.Entity.Name
				Dim Grp As String = api.Pov.Parent.Name
				'Dim POwn As Decimal = api.Data.GetDataCell("C#Local:V#YTD:A#PctOWN:F#None:O#BeforeAdj:I#" & Grp & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(CultureInfo.InvariantCulture) / 100
				'Dim PMin As Decimal = api.Data.GetDataCell("C#Local:V#YTD:A#PctMIN:F#None:O#BeforeAdj:I#" & Grp & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(CultureInfo.InvariantCulture) / 100
				Dim Own As OwnershipClass
				'Dim Own As New OwnershipClass(si,api,globals,Entity,,,, OpeScenario)
				Own = New OwnershipClass(si,api,globals,api.Pov.entity.Name,,,, api.Pov.Scenario.Name)
				
				Dim PGrupos As String = Own.Pown.ToString("G",CultureInfo.InvariantCulture)
				Dim PMinoritarioss As String = Own.Pmin.ToString("G",CultureInfo.InvariantCulture)
				Dim PGrupo As Decimal
				Dim PMinoritarios As Decimal
				PGrupo = PGrupos.XFConvertToDecimal(0)
				PMinoritarios = PMinoritarioss.XFConvertToDecimal(0)
				
				'brapi.errorlog.LogMessage(si, "Parent - " & api.Pov.Parent.name & " - Entity - " & api.Pov.Entity.name & " - Pown - " & PGrupo & " - Pmin - "& PMinoritarios & " - Period - "& api.Pov.Time.Name)
				


				
				api.Data.ClearCalculatedData("A#" & Account.ResG.name & ":O#AdjInput:F#" & Flow.Res.Name & "",True,True,True)
				'api.Data.Calculate("A#" & Account.ResG.name & ":O#AdjInput:F#" & Flow.Res.Name & ":I#None=RemoveZeros(A#" & Account.TopIS.Name & ":O#AdjInput:F#" & Flow.None.Name & ":I#Top) * " & pOwn & "")
				api.Data.Calculate("A#" & Account.ResG.name & ":O#AdjInput:F#" & Flow.Res.Name & ":I#None=RemoveZeros(A#PG_A5:O#AdjInput:F#" & Flow.None.Name & ":I#Top) * "& PGrupos &"")
				api.Data.ClearCalculatedData("A#" & Account.ResM.name & ":O#AdjInput:F#" & Flow.Res.Name & "",True,True,True)
				'api.Data.Calculate("A#" & Account.ResM.name & ":O#AdjInput:F#" & Flow.Res.Name & ":I#None=RemoveZeros(A#" & Account.TopIS.Name & ":O#AdjInput:F#" & Flow.None.Name & ":I#Top) * " & pMin & "")
				api.Data.Calculate("A#" & Account.ResM.name & ":O#AdjInput:F#" & Flow.Res.Name & ":I#None=RemoveZeros(A#PG_A5:O#AdjInput:F#" & Flow.None.Name & ":I#Top) * "& PMinoritarioss &"")
				api.Data.ClearCalculatedData("A#" & Account.MinInt.name & ":O#AdjInput:F#None:I#None",True,True,True)
				api.Data.Calculate("A#" & Account.MinInt.name & ":O#AdjInput:F#None:I#None=-A#" & Account.ResM.name & ":O#AdjInput:F#" & Flow.Res.Name)

				'brapi.errorlog.LogMessage(si, "Parent - " & api.Pov.Parent.name & " - Entity - " & api.Pov.Entity.name & " - 2AGU - " & Account.ResG.name & " - 2ATU - "& Account.ResM.name & " - Period - "& api.Pov.Time.Name)

				
				
			End If
		End If

	End Sub

	Function flowResDDP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Account As Object = Me.InitAccountClass(si, api, globals)
		
		Dim result As New DrillDownFormulaResult()
		If (api.Cons.IsLocalCurrencyforEntity() And Not api.Entity.HasChildren()) Then

			'Define Explanation to be shown in drill title bar
			result.Explanation = "Flow Res"

			'Add a row below per each drill result to be shown 
			result.SourceDataCells.Add("A#" & Account.TopIS.Name & ":F#" & Flow.None.Name)
			
			Return result
		End If
		Return result
	End Function	
	
	Sub flowMVT(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		If (api.Cons.IsLocalCurrencyforEntity() And Not api.Entity.HasChildren()) Then
				   
	    'Translate AFF flow at Prior Year End Average Rate      
		api.Data.ClearCalculatedData("O#Forms:F#" & flow.MVT.Name, True, True, True)
		api.Data.ClearCalculatedData("O#Import:F#" & flow.MVT.Name, True, True, True)
		api.Data.Calculate("O#Forms:F#" & flow.MVT.Name & "=RemoveZeros(O#Forms:F#" & flow.CLO.Name & ")-RemoveZeros(O#Forms:F#" & flow.TF.Name & ")" & "")
		api.Data.Calculate("O#Import:F#" & flow.MVT.Name & "=RemoveZeros(O#Import:F#" & flow.CLO.Name & ")-RemoveZeros(O#Import:F#" & flow.TF.Name & ")" & "")
 
		End If
	End Sub

	Function flowMVTDDP(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult

		Dim result As New DrillDownFormulaResult()

		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		If (api.Cons.IsLocalCurrencyforEntity() And Not api.Entity.HasChildren()) Then

			'Define Explanation to be shown in drill title bar
			result.Explanation = "Flow MOV "

			'Add a row below per each drill result to be shown 
			result.SourceDataCells.Add("F#" & flow.SDC.Name)
			result.SourceDataCells.Add("F#" & flow.TF.Name)
			
		End If
		Return result
	End Function
	
	Sub flowECO(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
		Dim Time As String = api.Pov.Time.Name
		Dim Year As Integer = Time.Substring(0, 4)
		Dim Cube As String = api.Pov.Cube.Name
		
	If Cube = "CONSO" Then
		
		If Year > 2019 Then
		
			If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
				Dim Scenario As String = api.pov.scenario.name

				If Not(Scenario.StartsWith("O")) Then
					
					Dim Flow As Object = Me.InitFlowClass(si, api, globals)
					Dim Account As Object = Me.InitAccountClass(si, api, globals)
					Dim FX As New GlobalClass(si, api,args)
					
					'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
					Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
					
					'Variable APV para el escenario R y PX
					Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
					Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
					Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
					
					Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
				'fs	Dim ENT_TC_TCO As String = "(F#" & flow.Ent.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ent.Name & "{0})"
				'fs	Dim SOR_TC_TCO As String = "(F#" & flow.Sor.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Sor.Name & "{0})"
					Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
					'Dim FTA_TC_TCO As String = "(F#" & flow.FPA.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FPA.Name & "{0})"
					Dim FTA_TC_TCO As String = "(F#APM:C#Local{1} * " & FX.closingRateToText & "-F#APM:C#Local{1} * " & closingRatePriorRealToText & ")"
					Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
					'Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & FX.closingRatePriorYEtoText & ")"
					'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
					Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"
					
					
					Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorRealToText & ")"
					
					'Calculate FX on Opening: OUV Local * (Closing Rate - opening Rate) and same for ENT and FTA

					'AdjInput needs to be the source for AdjCondolidated to avoid double counting
					'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
					'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"			
		'fs			Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & ENT_TC_TCO  & "+" & SOR_TC_TCO  & "+" & FUS_TC_TCO & "+" & FTA_TC_TCO & "+" & AFF_TC_TCO
					Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO & "+" & FTA_TC_TCO & ""
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
					
					api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
				
				ElseIf Scenario.StartsWith("O") Then
					'Dimensiones para calcular el escenario Objetivo
					Dim DatoRealUlt = api.Data.GetDataCell("A#CR_Z:S#R:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
					Dim DatoPrevUlt11 = api.Data.GetDataCell("A#CR_Z:S#P11:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
					Dim DatoPrevUlt09 = api.Data.GetDataCell("A#CR_Z:S#P09:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
					Dim DatoPrevUlt06 = api.Data.GetDataCell("A#CR_Z:S#P06:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
					Dim DatoPrevUlt03 = api.Data.GetDataCell("A#CR_Z:S#P03:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
					
					If DatoRealUlt<> 0 Then
						Dim Flow As Object = Me.InitFlowClass(si, api, globals)
						Dim Account As Object = Me.InitAccountClass(si, api, globals)
						Dim FX As New GlobalClass(si, api,args)
						
						'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
						Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
						
						'Variable APV para el escenario R y PX
						Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
						Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
						
						
						Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
						Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
						Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
						Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"
						Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorRealToText & ")"
						
						Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO &""
						api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
						api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
						
						api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
						
					ElseIf DatoPrevUlt11<> 0 Then
						Dim Flow As Object = Me.InitFlowClass(si, api, globals)
						Dim Account As Object = Me.InitAccountClass(si, api, globals)
						Dim FX As New GlobalClass(si, api,args)
						
						'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
						Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
						
						'Variable APC
						Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
						Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
						
						'P11
						Dim rateTypeCloP11 As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242899) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRateP11 As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeCloP11,timeyearend)
						Dim closingRatePriorP11ToText As String = closingRateP11.ToString(CultureInfo.InvariantCulture)
						
						
						Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
						Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
						Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
						Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"
						Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorP11ToText & ")"
						
						Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO &""
						api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
						api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
						
						api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
						
					ElseIf DatoPrevUlt09<> 0 Then
						Dim Flow As Object = Me.InitFlowClass(si, api, globals)
						Dim Account As Object = Me.InitAccountClass(si, api, globals)
						Dim FX As New GlobalClass(si, api,args)
						
						'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
						Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
						
						'Variable APC
						Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
						Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
						
						'P09
						Dim rateTypeCloP09 As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242898) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRateP09 As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeCloP09,timeyearend)
						Dim closingRatePriorP09ToText As String = closingRateP09.ToString(CultureInfo.InvariantCulture)
						
						
						Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
						Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
						Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
						Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"
						Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorP09ToText & ")"
						
						Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO &""
						api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
						api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
						
						api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
						
					ElseIf DatoPrevUlt06<> 0 Then
						Dim Flow As Object = Me.InitFlowClass(si, api, globals)
						Dim Account As Object = Me.InitAccountClass(si, api, globals)
						Dim FX As New GlobalClass(si, api,args)
						
						'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
						Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
						
						'Variable APC
						Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
						Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
						
						'P06	
						Dim rateTypeCloP06 As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242897) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRateP06 As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeCloP06,timeyearend)
						Dim closingRatePriorP06ToText As String = closingRateP06.ToString(CultureInfo.InvariantCulture)
						
						
						Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
						Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
						Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
						Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"
						Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorP06ToText & ")"
						
						Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO &""
						api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
						api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
						
						api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
					
					ElseIf DatoPrevUlt03<> 0 Then
						Dim Flow As Object = Me.InitFlowClass(si, api, globals)
						Dim Account As Object = Me.InitAccountClass(si, api, globals)
						Dim FX As New GlobalClass(si, api,args)
						
						'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
						Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
						
						'Variable APC
						Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
						Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
						
						'P03	
						Dim rateTypeCloP03 As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242907) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRateP03 As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeCloP03,timeyearend)
						Dim closingRatePriorP03ToText As String = closingRateP03.ToString(CultureInfo.InvariantCulture)
						
						
						Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
						Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
						Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
						Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"
						Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorP03ToText & ")"
						
						Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO &""
						api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
						api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
						
						api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
					
					End If
					End If
				End If
				'> a 2019
				Else
					
					If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
				Dim Scenario As String = api.pov.scenario.name

				If Not(Scenario.StartsWith("O")) Then	   
					Dim Flow As Object = Me.InitFlowClass(si, api, globals)
					Dim Account As Object = Me.InitAccountClass(si, api, globals)
					Dim FX As New GlobalClass(si, api,args)
					
					'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
					Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
					
					'Variable APV para el escenario R y PX
					Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
					Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
					Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
					
					Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
				'fs	Dim ENT_TC_TCO As String = "(F#" & flow.Ent.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ent.Name & "{0})"
				'fs	Dim SOR_TC_TCO As String = "(F#" & flow.Sor.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Sor.Name & "{0})"
					Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
					'Dim FTA_TC_TCO As String = "(F#" & flow.FPA.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FPA.Name & "{0})"
					Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
					'Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & FX.closingRatePriorYEtoText & ")"
					'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
					Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"
					
					
					Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorRealToText & ")"
					
					'Calculate FX on Opening: OUV Local * (Closing Rate - opening Rate) and same for ENT and FTA

					'AdjInput needs to be the source for AdjCondolidated to avoid double counting
					'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
					'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"			
		'fs			Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & ENT_TC_TCO  & "+" & SOR_TC_TCO  & "+" & FUS_TC_TCO & "+" & FTA_TC_TCO & "+" & AFF_TC_TCO
					Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO &""
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
					
					api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
				
				ElseIf Scenario.StartsWith("O") Then
					'Dimensiones para calcular el escenario Objetivo
					Dim DatoRealUlt = api.Data.GetDataCell("A#CR_Z:S#R:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
					Dim DatoPrevUlt11 = api.Data.GetDataCell("A#CR_Z:S#P11:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
					Dim DatoPrevUlt09 = api.Data.GetDataCell("A#CR_Z:S#P09:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
					Dim DatoPrevUlt06 = api.Data.GetDataCell("A#CR_Z:S#P06:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
					Dim DatoPrevUlt03 = api.Data.GetDataCell("A#CR_Z:S#P03:T#PovPriorYearM12:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#top:U2#top:U3#top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
					
					If DatoRealUlt<> 0 Then
						Dim Flow As Object = Me.InitFlowClass(si, api, globals)
						Dim Account As Object = Me.InitAccountClass(si, api, globals)
						Dim FX As New GlobalClass(si, api,args)
						
						'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
						Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
						
						'Variable APV para el escenario R y PX
						Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
						Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
						
						
						Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
						Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
						Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
						Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"
						Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorRealToText & ")"
						
						Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO &""
						api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
						api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
						
						api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
						
					ElseIf DatoPrevUlt11<> 0 Then
						Dim Flow As Object = Me.InitFlowClass(si, api, globals)
						Dim Account As Object = Me.InitAccountClass(si, api, globals)
						Dim FX As New GlobalClass(si, api,args)
						
						'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
						Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
						
						'Variable APC
						Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
						Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
						
						'P11
						Dim rateTypeCloP11 As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242899) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRateP11 As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeCloP11,timeyearend)
						Dim closingRatePriorP11ToText As String = closingRateP11.ToString(CultureInfo.InvariantCulture)
						
						
						Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
						Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
						Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
						Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"
						Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorP11ToText & ")"
						
						Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO &""
						api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
						api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
						
						api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
						
					ElseIf DatoPrevUlt09<> 0 Then
						Dim Flow As Object = Me.InitFlowClass(si, api, globals)
						Dim Account As Object = Me.InitAccountClass(si, api, globals)
						Dim FX As New GlobalClass(si, api,args)
						
						'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
						Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
						
						'Variable APC
						Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
						Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
						
						'P09
						Dim rateTypeCloP09 As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242898) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRateP09 As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeCloP09,timeyearend)
						Dim closingRatePriorP09ToText As String = closingRateP09.ToString(CultureInfo.InvariantCulture)
						
						
						Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
						Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
						Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
						Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"
						Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorP09ToText & ")"
						
						Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO &""
						api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
						api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
						
						api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
						
					ElseIf DatoPrevUlt06<> 0 Then
						Dim Flow As Object = Me.InitFlowClass(si, api, globals)
						Dim Account As Object = Me.InitAccountClass(si, api, globals)
						Dim FX As New GlobalClass(si, api,args)
						
						'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
						Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
						
						'Variable APC
						Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
						Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
						
						'P06	
						Dim rateTypeCloP06 As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242897) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRateP06 As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeCloP06,timeyearend)
						Dim closingRatePriorP06ToText As String = closingRateP06.ToString(CultureInfo.InvariantCulture)
						
						
						Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
						Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
						Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
						Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"
						Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorP06ToText & ")"
						
						Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO &""
						api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
						api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
						
						api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
					
					ElseIf DatoPrevUlt03<> 0 Then
						Dim Flow As Object = Me.InitFlowClass(si, api, globals)
						Dim Account As Object = Me.InitAccountClass(si, api, globals)
						Dim FX As New GlobalClass(si, api,args)
						
						'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
						Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
						
						'Variable APC
						Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
						Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
						
						'P03	
						Dim rateTypeCloP03 As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242907) 'CubeId (CONSO), ScenarioId (Real)
						Dim closingRateP03 As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeCloP03,timeyearend)
						Dim closingRatePriorP03ToText As String = closingRateP03.ToString(CultureInfo.InvariantCulture)
						
						
						Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
						Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
						Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
						Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"
						Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorP03ToText & ")"
						
						Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO &""
						api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
						api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
						
						api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
					
					End If
					
					
				
				End If
			End If
		End If
	'Cubo Order y Contorlling
	Else
		If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
					Dim Flow As Object = Me.InitFlowClass(si, api, globals)
					Dim Account As Object = Me.InitAccountClass(si, api, globals)
					Dim FX As New GlobalClass(si, api,args)
					
					'SB - Añadido para que todas las aperturas (APV) conviertan al tipo de cierre REAL
					Dim timeyearend As Integer = api.Time.GetLastPeriodInPriorYear
					
					'Variable APV para el escenario R y PX
					Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability(0, 5242902) 'CubeId (CONSO), ScenarioId (Real)
					Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeyearend)
					Dim closingRatePriorRealToText As String = closingRate.ToString(CultureInfo.InvariantCulture)
					
					Dim OUV_TC_TCO As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Ouv.Name & "{0})"
				
					Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
				
					Dim FTA_TC_TCO As String = "(F#APM:C#Local{1} * " & FX.closingRateToText & "-F#APM:C#Local{1} * " & closingRatePriorRealToText & ")"
					Dim AFF_TC_TCO As String = "(F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Aff.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
					Dim APC_TC_TCO As String = "(F#APC:C#Local{1} * " & FX.closingRateToText & "-F#APC:C#Local{1} * " & closingRatePriorRealToText & ")"					
					
					Dim APV_TC_TCO As String = "(F#APV:C#Local{1} * " & FX.closingRateToText & "-F#APV:C#Local{1} * " & closingRatePriorRealToText & ")"
					
					Dim sExpression As String = "F#" & Flow.ECO.Name & "{0}=" & OUV_TC_TCO & "+" & FUS_TC_TCO & "+" & AFF_TC_TCO & "+" & APV_TC_TCO & "+" & APC_TC_TCO & "+" & FTA_TC_TCO & ""
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
					
					api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#" & Flow.ECO.Name,True,True,True)
		End If
	End If	
	End Sub

	Function flowECODDP(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
		Dim result As New DrillDownFormulaResult()

		If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
			Dim Flow As Object = Me.InitFlowClass(si, api, globals)
			Dim Account As Object = Me.InitAccountClass(si, api, globals)
			Dim FX As New GlobalClass(si, api,args)

			'Define Explanation to be shown in drill title bar
			result.Explanation = "Flow ECO delta to Closing Rate: " & fx.closingRateToText

			Dim OUV_LC_TCO As String = "F#" & flow.Ouv.Name & ":C#Local"
		'fs	Dim ENT_LC_TCO As String = "F#" & flow.Ent.Name & ":C#Local"
		'fs	Dim FTA_LC_TCO As String = "F#" & flow.FPA.Name & ":C#Local"
			Dim AFF_LC_TCO As String = "F#" & flow.Aff.Name & ":C#Local"
		'fs	Dim SOR_LC_TCO As String = "F#" & flow.Sor.Name & ":C#Local"
			Dim FUS_LC_TCO As String = "F#" & flow.FUS.Name & ":C#Local"

			Dim OUV_AC_TCO As String = "F#" & flow.Ouv.Name
		'fs	Dim ENT_AC_TCO As String = "F#" & flow.Ent.Name
		'fs	Dim FTA_AC_TCO As String = "F#" & flow.FPA.Name
			Dim AFF_AC_TCO As String = "F#" & flow.Aff.Name
		'fs	Dim SOR_AC_TCO As String = "F#" & flow.Sor.Name
			Dim FUS_AC_TCO As String = "F#" & flow.FUS.Name

			Dim sOrigin As String
			If api.Pov.Origin.MemberId.Equals(dimconstants.AdjConsolidated) Then
				sOrigin = ":O#AdjInput"
			Else	
				sOrigin = ":O#" & api.Pov.Origin.Name
			End If	
			'Add a row below per each drill result to be shown 
			result.SourceDataCells.Add(OUV_LC_TCO & sOrigin)
		'fs	result.SourceDataCells.Add(ENT_LC_TCO & sOrigin)
		'fs	result.SourceDataCells.Add(FTA_LC_TCO & sOrigin)
			result.SourceDataCells.Add(AFF_LC_TCO & sOrigin)
		'fs	result.SourceDataCells.Add(SOR_LC_TCO & sOrigin)
			result.SourceDataCells.Add(FUS_LC_TCO & sOrigin)
			
			result.SourceDataCells.Add(OUV_AC_TCO)
		'fs	result.SourceDataCells.Add(ENT_AC_TCO)
		'fs	result.SourceDataCells.Add(FTA_AC_TCO)
			result.SourceDataCells.Add(AFF_AC_TCO)
		'fs	result.SourceDataCells.Add(SOR_AC_TCO)
			result.SourceDataCells.Add(FUS_AC_TCO)

		End If
		Return result
	End Function
	
	Sub flowECF(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
			Dim Flow As Object = Me.InitFlowClass(si, api, globals)
			Dim Account As Object = Me.InitAccountClass(si, api, globals)
			Dim FX As New GlobalClass(si, api,args)
					   
			Dim CLO_TC_TM As String = "(F#" & flow.Clo.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.Clo.Name & ":C#Local{1} * " & FX.averageRateToText & ")"
			Dim OUV_TM_TC As String = "(F#" & flow.Ouv.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Ouv.Name & ":C#Local{1} * " & fx.closingRateToText & ")"
		'fs	Dim ENT_TM_TC As String = "(F#" & flow.Ent.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Ent.Name & ":C#Local{1} * " & fx.closingRateToText & ")"
		'fs	Dim SOR_TM_TC As String = "(F#" & flow.Sor.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Sor.Name & ":C#Local{1} * " & fx.closingRateToText & ")"
			Dim FUS_TM_TC As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & ")"
			Dim FTA_TM_TC As String = "(F#" & flow.FPA.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.FPA.Name & ":C#Local{1} * " & fx.closingRateToText & ")"
			Dim DIV_TM_TM1 As String = "(F#" & flow.Div.Name & ":C#Local{1} * " & FX.averageRateToText & "-F#" & flow.Div.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
			Dim APV_TM_TC As String = "(F#APV:C#Local{1} * " & FX.averageRateToText & "-F#APV:C#Local{1} * " & FX.closingRateToText & ")"
			Dim APC_TM_TC As String = "(F#APC:C#Local{1} * " & FX.averageRateToText & "-F#APC:C#Local{1} * " & FX.closingRateToText & ")"
			
			
		    'Calculate FX on Activity: CLO Local * (Closing Rate - Average Rate) and Reverse the impact from OUV, ENT, FTA, AFF and DIV (which are included in CLO balance)
'fs			Dim sExpression As String = "F#" & flow.ECF.Name & "{0}= " & CLO_TC_TM & "+" & OUV_TM_TC & "+" & ENT_TM_TC & "+" & SOR_TM_TC & "+" & FUS_TM_TC & "+" & FTA_TM_TC & "+" & DIV_TM_TM1
			Dim sExpression As String = "F#" & flow.ECF.Name & "{0}= " & CLO_TC_TM & "+" & OUV_TM_TC & "+" & FUS_TM_TC & "+" & DIV_TM_TM1 & "+" & APV_TM_TC & "+" & APC_TM_TC & "+" & FTA_TM_TC & ""
			'AdjInput needs to be the source for AdjCondolidated to avoid double counting
			'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
			'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

		End If
	End Sub
	
	Function flowECFDDP(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
		Dim result As New DrillDownFormulaResult()

		If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
			Dim Flow As Object = Me.InitFlowClass(si, api, globals)
			Dim Account As Object = Me.InitAccountClass(si, api, globals)
			Dim FX As New GlobalClass(si, api,args)

			'Define Explanation to be shown in drill title bar
			result.Explanation = "Flow ECF delta to Closing Rate: " & fx.closingRateToText & " with Average Rate: " & FX.averageRateToText & " and prev. Years Average Rate: " & FX.averageRatePriorYEtoText

			Dim CLO_LC As String = "F#" & flow.Clo.Name & ":C#Local"
			Dim OUV_LC As String = "F#" & flow.Ouv.Name & ":C#Local"
		'fs	Dim ENT_LC As String = "F#" & flow.Ent.Name & ":C#Local"
		'fs	Dim FTA_LC As String = "F#" & flow.FPA.Name & ":C#Local"
			Dim DIV_LC As String = "F#" & flow.Div.Name & ":C#Local"
		'fs	Dim SOR_LC As String = "F#" & flow.SOR.Name & ":C#Local"
			Dim FUS_LC As String = "F#" & flow.FUS.Name & ":C#Local"

			Dim sOrigin As String
			If api.Pov.Origin.MemberId.Equals(dimconstants.AdjConsolidated) Then
				sOrigin = ":O#AdjInput"
			Else	
				sOrigin = ":O#" & api.Pov.Origin.Name
			End If	

			'Add a row below per each drill result to be shown 
			result.SourceDataCells.Add(CLO_LC & sOrigin)
			result.SourceDataCells.Add(OUV_LC & sOrigin)
		'fs	result.SourceDataCells.Add(ENT_LC & sOrigin)
		'fs	result.SourceDataCells.Add(FTA_LC & sOrigin)
			result.SourceDataCells.Add(DIV_LC & sOrigin)
		'fs	result.SourceDataCells.Add(SOR_LC & sOrigin)
			result.SourceDataCells.Add(FUS_LC & sOrigin)
		End If
		Return result
	End Function
	

	
	Sub flowCLO(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Account As Object = Me.InitAccountClass(si, api, globals)

		If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
	'	'Calcul du flux de cloture pour les PMV Cession Immos
	
			api.Data.ClearCalculatedData(False,True,False,"A#CESSIONSIMMO.Base.Where(Name DoesnotContain CF)")'HC
			api.data.calculate("F#" & flow.Clo.name & "=F#" & flow.TF.name,"A#CESSIONSIMMO.Base.Where(Name DoesnotContain CF)")
			'fs CLO calculated for Local adjs
			api.Data.ClearCalculatedData("O#AdjInput:F#" & flow.Clo.name,False,True,False) 
			api.data.calculate("O#AdjInput:F#" & flow.Clo.name & "=O#AdjInput:F#" & flow.TF.name)

		Else	
		    'Calculate CLO: CLO = Sum of all Flows
			api.Data.ClearCalculatedData("F#" & flow.Clo.name,False,True,False) 'Should not have to clear local data or consolidated data, only translated as translated is calculated first by default engine.
			api.data.calculate("F#" & flow.Clo.name & "=F#" & flow.TF.name)
			'Recalcul du flux d'erreur pour 120C/106C
			'Calculate Error flow for 120C/106C
			api.data.calculate("A#" & Account.RsvC.Name & ":F#" & flow.ERR.name & "=A#" & Account.RsvC.Name & ":F#" & flow.Clo.name & "-A#" & Account.RsvC.Name & ":F#" & flow.TF.name)
			api.data.calculate("A#" & Account.RsvCG.Name & ":F#" & flow.ERR.name & "=A#" & Account.RsvCG.Name & ":F#" & flow.Clo.name & "-A#" & Account.RsvCG.Name & ":F#" & flow.TF.name)
			api.data.calculate("A#" & Account.RsvCM.Name & ":F#" & flow.ERR.name & "=A#" & Account.RsvCM.Name & ":F#" & flow.Clo.name & "-A#" & Account.RsvCM.Name & ":F#" & flow.TF.name)
			api.data.calculate("A#" & Account.ResC.Name & ":F#" & flow.ERR.name & "=A#" & Account.ResC.Name & ":F#" & flow.Clo.name & "-A#" & Account.ResC.Name & ":F#" & flow.TF.name)
			api.data.calculate("A#" & Account.ResCG.Name & ":F#" & flow.ERR.name & "=A#" & Account.ResCG.Name & ":F#" & flow.Clo.name & "-A#" & Account.ResCG.Name & ":F#" & flow.TF.name)
			api.data.calculate("A#" & Account.ResCM.Name & ":F#" & flow.ERR.name & "=A#" & Account.ResCM.Name & ":F#" & flow.Clo.name & "-A#" & Account.ResCM.Name & ":F#" & flow.TF.name)
			End If
	End Sub	
	
	Function flowCLODDP(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
		Dim result As New DrillDownFormulaResult()

		If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
		Else	
			Dim Flow As Object = Me.InitFlowClass(si, api, globals)
			'Define Explanation to be shown in drill title bar
			result.Explanation = "Flow CLO"
			'Add a row below per each drill result to be shown 
			result.SourceDataCells.Add("F#" & flow.TF.name)

		End If
		Return result
	End Function
	
	Sub flowHIST(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, ByVal sflowName As String)
		If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
			Dim FX As New GlobalClass(si, api,args)
		   
			Dim Flow As Object = Me.InitFlowClass(si, api, globals)
			Dim HistFlow As Member = flow.getMember(sFlowName)'Me.InitFlowClass(si, api, globals)
		 
		    'Translate ENT flow at Opening Rate      
			Dim TauxEntree As Decimal = 0
			If Histflow.MemberId = flow.Ent.MemberId Then
				If api.Entity.IsIC(api.Pov.Parent.MemberId) Then
					TauxEntree = api.Data.GetDataCell("A#TauxEntreePerimetre:C#Local:O#BeforeAdj:I#" & api.Pov.Parent.Name & ":F#None:U1#None:U2#None:U3#None:U4#None").CellAmount
				End If	
				If TauxEntree = 0 Then
					TauxEntree = api.Data.GetDataCell("A#TauxEntreePerimetre:C#Local:O#BeforeAdj:I#None:F#None:U1#None:U2#None:U3#None:U4#None").CellAmount
				End If	
			Else	
				If api.Entity.IsIC(api.Pov.Parent.MemberId) Then
					TauxEntree = api.Data.GetDataCell("A#TauxSortiePerimetre:C#Local:O#BeforeAdj:I#" & api.Pov.Parent.Name & ":F#None:U1#None:U2#None:U3#None:U4#None").CellAmount
				End If	
				If TauxEntree = 0 Then
					TauxEntree = api.Data.GetDataCell("A#TauxSortiePerimetre:C#Local:O#BeforeAdj:I#None:F#None:U1#None:U2#None:U3#None:U4#None").CellAmount
				End If	
			End If	
			If TauxEntree = 0 Then TauxEntree = FX.averageRateToText
			api.Data.ClearCalculatedData("F#" & HistFlow.Name,True,True,True)
			'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
			'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"			
			Dim sExpression As String = "F#" & HistFlow.Name & "{0}=F#" & HistFlow.Name & ":C#Local{1} *" & TauxEntree
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

		End If
	End Sub

	Function flowHISTDDP(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, ByVal sflowName As String) As DrillDownFormulaResult
		Dim result As New DrillDownFormulaResult()

		If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
		Else	
			Dim Flow As Object = Me.InitFlowClass(si, api, globals)
			Dim HistFlow As Member = flow.getMember(sFlowName)'Me.InitFlowClass(si, api, globals)

			Dim sTauxScript As String = ""
			If Histflow.MemberId = flow.Ent.MemberId Then
				sTauxScript = "A#TauxEntreePerimetre:C#Local:O#BeforeAdj:I#None:F#None:U1#None:U2#None:U3#None:U4#None"
			Else
				sTauxScript = "A#TauxSortiePerimetre:C#Local:O#BeforeAdj:I#None:F#None:U1#None:U2#None:U3#None:U4#None"
			End If
			
			Dim Taux As Decimal = api.Data.GetDataCell(sTauxScript).CellAmount

			'Define Explanation to be shown in drill title bar
			result.Explanation = "Flow " & HistFlow.Name & " with historical exchange rate: " & sTauxScript
			'Add a row below per each drill result to be shown 
			result.SourceDataCells.Add("F#" & HistFlow.Name & ":C#Local")
			result.SourceDataCells.Add(sTauxScript)

		End If
		Return result
	End Function
	
	Sub flowFPA (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
		Dim Time As String = api.Pov.Time.Name
		Dim Year As Integer = Time.Substring(0, 4)
		
		If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
			
			If Year < 2020 Then
						   
				'Dim FX As New GlobalClass(si, api,args)
				Dim Flow As Object = Me.InitFlowClass(si, api, globals)
				Dim FX As New GlobalClass(si, api,args)
			 
			    'Translate ENT flow at Opening Rate
				'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
				'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"			
				Dim sExpression As String = "F#" & flow.FPA.Name & "{0}=(F#" & flow.FPA.Name & ":C#Local{1} * " & fx.averageRateToText & ")"
				api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
				api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				
			Else
				
				'Dim FX As New GlobalClass(si, api,args)
				Dim Flow As Object = Me.InitFlowClass(si, api, globals)
				Dim FX As New GlobalClass(si, api,args)
			 
			    'Translate ENT flow at Opening Rate
				'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
				'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"			
				Dim sExpression As String = "F#" & flow.FPA.Name & "{0}=(F#" & flow.FPA.Name & ":C#Local{1} * " & fx.closingRatePriorYEtoText & ")"
				api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
				api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			
			End If
			
		End If
	End Sub

	Function flowFPADDP(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
		Dim result As New DrillDownFormulaResult()

		If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
			Dim FX As New GlobalClass(si, api,args)
			Dim Flow As Object = Me.InitFlowClass(si, api, globals)

			'Define Explanation to be shown in drill title bar
			result.Explanation = "Flow FPA with exchange rate closing rate prior year: " & FX.closingRatePriorYEtoText
			'Add a row below per each drill result to be shown 
			result.SourceDataCells.Add("F#" & flow.FPA.Name & ":C#Local")

		End If
		Return result
	End Function
	
	Sub Calc_CVentas (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
	Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
	Dim FX As New GlobalClass(si, api,args)
		
		If CurYear = 2019 Then
			If Not api.Entity.HasChildren() Then
				If api.Cons.IsLocalCurrencyforEntity() Then
				
				'En 2019 tengo que llamar a u4#gestion porque el dato tiene como origen ECT. En 2020 no hara falta.
				api.Data.ClearCalculatedData("A#CV_Ventas:F#DIS",True,True,True)
				api.Data.Calculate("A#CV_Ventas:F#DIS:U4#None:O#Import=A#CV003:F#None:U4#Gestion:O#Top * (-1)")
				
				' La cuenta CV_A_FCT solo se carga en ADJINPUT
				api.Data.ClearCalculatedData("A#CV_Ventas:F#OMO:O#AdjInput",True,True,True)
				api.Data.Calculate("A#CV_Ventas:F#OMO:O#AdjInput=A#CV_A_FCT:F#None:O#AdjInput")
				
				api.Data.ClearCalculatedData("A#CV_Ventas:F#SDC",True,True,True)
				api.Data.Calculate("A#CV_Ventas:F#SDC=A#CV_Ventas:F#APV + A#CV_Ventas:F#INC + A#CV_Ventas:F#DIS + A#CV_Ventas:F#OMO")
				
				End If
			End If
		ElseIf CurYear > 2019 Then
			If Not api.Entity.HasChildren() Then
				If api.Cons.IsLocalCurrencyforEntity() Then
				
				'A partir de 2020 los cruces de U4 son Base
				api.Data.ClearCalculatedData("A#CV_Ventas:O#AdjInput:F#INC",True,True,True)
				api.Data.Calculate("A#CV_Ventas:O#AdjInput:F#INC=A#CV002:O#AdjInput:F#None")
				
				api.Data.ClearCalculatedData("A#CV_Ventas:F#DIS",True,True,True)
				api.Data.Calculate("A#CV_Ventas:F#DIS=A#CV003:F#None*(-1)")
				
				api.Data.ClearCalculatedData("A#CV_Ventas:F#OMO",True,True,True)
				api.Data.Calculate("A#CV_Ventas:F#OMO=A#CV_A_FCT:F#None")
				
				api.Data.ClearCalculatedData("A#CV_Ventas:F#SDC",True,True,True)
				api.Data.Calculate("A#CV_Ventas:F#SDC=A#CV_Ventas:F#APV + A#CV_Ventas:F#INC + A#CV_Ventas:F#DIS + A#CV_Ventas:F#OMO")
				
				ElseIf (api.Pov.Cons.Name ="OwnerPreAdj") Or (api.Pov.Cons.Name ="OwnerPostAdj") Then
					api.Data.ClearCalculatedData("A#CV_Ventas:F#OMO",True,True,True)
					api.Data.Calculate("A#CV_Ventas:F#OMO=A#CV_A_FCT:F#OMO + A#CV_A_FCT:F#None")
					
					api.Data.ClearCalculatedData("A#CV_Ventas:O#AdjInput:F#INC",True,True,True)
					api.Data.Calculate("A#CV_Ventas:O#AdjInput:F#INC=A#CV002:O#AdjInput:F#None")
					
				End If
			End If
		End If
	End Sub
	
	Sub Calc_CV00X (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
	Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
	Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
	
		If Not api.Entity.HasChildren() Then
			If api.Cons.IsLocalCurrencyforEntity() Or (api.Pov.Cons.Name = "EUR") Then
				
			api.Data.ClearCalculatedData("A#CV001:F#None",True,True,True)
			api.Data.Calculate("A#CV001:F#None=A#CV_Ventas:F#APV")
			
			'detallado O#import porque CV002 tiene asientos en None y se eliminan
			api.Data.ClearCalculatedData("A#CV002:O#Import:F#None",True,True,True)
			api.Data.Calculate("A#CV002:O#Import:F#None=A#CV_Ventas:O#Import:F#INC")
				
			api.Data.ClearCalculatedData("A#CV005:F#None",True,True,True)
			api.Data.Calculate("A#CV005:F#None=A#CV_Ventas:F#DCA + A#CV_Ventas:F#DCM")
				
			api.Data.ClearCalculatedData("A#CV004:F#None",True,True,True)
			api.Data.Calculate("A#CV004:F#None=A#CV_Ventas:F#SDC")
				
				If CurYear = 2019 Then 'porque en 2019 hay datos de asientos cargados por fichero
					
						api.Data.ClearCalculatedData("A#CV_A_FCT:F#OMO:O#Import",True,True,True)
						api.Data.ClearCalculatedData("A#CV_A_FCT:F#None:O#Import",True,True,True)
						api.Data.Calculate("A#CV_A_FCT:F#None:O#Import=A#CV_A_FCT:F#None:O#Import + A#CV_Ventas:F#OMO:O#Import")
					
				End If
				
			Else If (api.Pov.Cons.Name ="OwnerPreAdj" Or api.Pov.Cons.Name ="OwnerPostAdj") Then
				api.Data.ClearCalculatedData("A#CV004:F#None",True,True,True)
				api.Data.Calculate("A#CV004:F#None=A#CV_A_FCT:F#None")
			
			
			End If
		Else If api.Entity.HasChildren() Then
		api.Data.ClearCalculatedData("A#CV004:O#AdjInput:F#None",True,True,True)
		api.Data.Calculate("A#CV004:O#AdjInput:F#None=A#CV001:O#AdjInput:F#None + A#CV_A_FCT:O#AdjInput:F#None + A#CV002:O#AdjInput:F#None")
		End If
	End Sub
	
	Sub Calc_CFact (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
	Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
	Dim FX As New GlobalClass(si, api,args)
		
		If CurYear = 2019 Then
			If Not api.Entity.HasChildren() Then
				If api.Cons.IsLocalCurrencyforEntity() Then
					
				api.Data.ClearCalculatedData("A#CV_Fact:F#SDC",True,True,True)
				api.Data.Calculate("A#CV_Fact:F#SDC=A#CV_Fact:F#APV + A#CV_Fact:F#INC + A#CV_Fact:F#DIS + A#CV_Fact:F#OMO")
				
				End If
			End If
		ElseIf CurYear > 2019 Then
			If Not api.Entity.HasChildren() Then
				If api.Cons.IsLocalCurrencyforEntity() Then
				
				api.Data.ClearCalculatedData("A#CV_Fact:O#AdjInput:F#INC",True,True,True)
				api.Data.Calculate("A#CV_Fact:O#AdjInput:F#INC=A#CV007:O#AdjInput:F#None")
				
				api.Data.ClearCalculatedData("A#CV_Fact:O#AdjInput:F#DIS",True,True,True)
				api.Data.Calculate("A#CV_Fact:O#AdjInput:F#DIS=A#CV008:O#AdjInput:F#None * (-1)")
				
				api.Data.ClearCalculatedData("A#CV_Fact:F#OMO",True,True,True)
				api.Data.Calculate("A#CV_Fact:F#OMO=A#CV_B_FCT:F#None")
					
				api.Data.ClearCalculatedData("A#CV_Fact:F#SDC",True,True,True)
				api.Data.Calculate("A#CV_Fact:F#SDC=A#CV_Fact:F#APV + A#CV_Fact:F#INC + A#CV_Fact:F#DIS + A#CV_Fact:F#OMO")
				
				ElseIf (api.Pov.Cons.Name ="OwnerPreAdj") Or (api.Pov.Cons.Name ="OwnerPostAdj") Then
					api.Data.ClearCalculatedData("A#CV_Fact:F#OMO",True,True,True)
					api.Data.Calculate("A#CV_Fact:F#OMO=A#CV_B_FCT:F#None")
					
					api.Data.ClearCalculatedData("A#CV_Fact:O#AdjInput:F#INC",True,True,True)
					api.Data.Calculate("A#CV_Fact:O#AdjInput:F#INC=A#CV007:O#AdjInput:F#None")
					
					api.Data.ClearCalculatedData("A#CV_Fact:O#AdjInput:F#DIS",True,True,True)
					api.Data.Calculate("A#CV_Fact:O#AdjInput:F#DIS=A#CV008:O#AdjInput:F#None * (-1)")
				
				End If
			End If
		End If
		
	End Sub
	
	Sub Calc_CF00X (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
	Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
	Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
		
		If Not api.Entity.HasChildren() Then
			If api.Cons.IsLocalCurrencyforEntity() Or (api.Pov.Cons.Name = "EUR") Then
				
			api.Data.ClearCalculatedData("A#CV006:F#None",True,True,True)
			api.Data.Calculate("A#CV006:F#None=A#CV_Fact:F#APV")
				
			'detallado O#import porque tiene asientos en None y se eliminan				
			api.Data.ClearCalculatedData("A#CV007:O#Import:F#None",True,True,True)
			api.Data.Calculate("A#CV007:O#Import:F#None=A#CV_Fact:O#Import:F#INC")
			
			'detallado O#import porque tiene asientos en None y se eliminan			
			api.Data.ClearCalculatedData("A#CV008:O#Import:F#None",True,True,True)
			api.Data.Calculate("A#CV008:O#Import:F#None=A#CV_Fact:O#Import:F#DIS * (-1)")
				
			api.Data.ClearCalculatedData("A#CV009:F#None",True,True,True)
			api.Data.Calculate("A#CV009:F#None=A#CV_Fact:F#DCA + A#CV_Fact:F#DCM")
				
			api.Data.ClearCalculatedData("A#CV010:F#None",True,True,True)
			api.Data.Calculate("A#CV010:F#None=A#CV_Fact:F#SDC")
				
				If CurYear = 2019 Then
				api.Data.ClearCalculatedData("A#CV_B_FCT:F#OMO",True,True,True)
				api.Data.ClearCalculatedData("A#CV_B_FCT:F#None",True,True,True)
				api.Data.Calculate("A#CV_B_FCT:F#None=A#CV_Fact:F#OMO")
				End If
			
			
			Else If (api.Pov.Cons.Name ="OwnerPreAdj" Or api.Pov.Cons.Name ="OwnerPostAdj") Then
				api.Data.ClearCalculatedData("A#CV010:F#None",True,True,True)
				api.Data.Calculate("A#CV010:F#None=A#CV_B_FCT:F#None")
				
			End If
		Else If api.Entity.HasChildren() Then
		api.Data.ClearCalculatedData("A#CV010:O#AdjInput:F#None",True,True,True)
		api.Data.Calculate("A#CV010:O#AdjInput:F#None=A#CV006:O#AdjInput:F#None + A#CV_B_FCT:O#AdjInput:F#None + A#CV007:O#AdjInput:F#None + A#CV008:O#AdjInput:F#None")
		End If
	End Sub
	
#End Region

#Region "Member Formulas For Is Accounts"
	'Dim FSK As New OneStream.BusinessRule.Finance.XFW_FSK_MemberFormulas.MainClass
	'FSK.AcctXXX(si, api, globals, args)

	Sub AcctMinInt (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		
'		api.Data.Calculate("A#INTMIN:F#None=A#120M:F#CLO")
		api.Data.Calculate("A#" & Acct.MinInt.Name & ":F#None=A#" & Acct.ResM.Name & ":F#" & flow.Clo.Name)

	End Sub
	
	Function AcctMinIntDDP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult

		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)

		'Simple formula drill down
		Dim result As New DrillDownFormulaResult()

		'Define Explanation to be shown in drill title bar
		result.Explanation = "Int Min"

		'Add a row below per each drill result to be shown 
		result.SourceDataCells.Add("A#" & Acct.ResM.Name & ":F#" & flow.Clo.Name)
		
		Return result
	End Function
	
		'Añadido por SB
	Sub RDOMIN (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)

		If (Not api.Entity.HasChildren() And ((api.Pov.Cons.Name = "Elimination") Or (api.Pov.Cons.Name = "OwnerPostAdj"))) Then
			
			api.Data.Calculate("A#RDOMIN:F#None:I#None=-A#2ATUG:F#SDC:I#Top")
		
		End If
	End Sub

#End Region

#Region "Member Formulas for BS Accounts"
	'Dim FSK As New OneStream.BusinessRule.Finance.XFW_FSK_MemberFormulas.MainClass
	'FSK.AcctXXX(si, api, globals, args)

	Sub AcctResEx (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		
		'Calc result of the year & retained earnings
		If Not api.Entity.HasChildren() Then
			If api.Cons.IsLocalCurrencyforEntity() Then

				'BS Result Current & Prior 12900
				api.Data.Calculate("A#" & Acct.ResEx.Name & ":F#" & Flow.Aff.Name & "=-A#" & Acct.ResEx.Name & ":F#" & Flow.Ouv.Name & "")
				api.Data.Calculate("A#" & Acct.ResEx.Name & ":O#AdjInput:F#" & Flow.Aff.Name & "=-A#" & Acct.ResEx.Name & ":O#AdjInput:F#APM")

				api.Data.Calculate("A#" & Acct.ResEx.Name & ":O#Forms:F#" & Flow.Res.Name & ":I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None=A#" & Acct.TopIS.Name & ":O#Forms:F#None:I#Top:U3#Top:U4#Legal:U5#None:U6#None:U7#None:U8#None")
				api.Data.Calculate("A#" & Acct.ResEx.Name & ":O#Forms:F#" & Flow.Clo.Name & "=A#" & Acct.ResEx.Name & ":O#Forms:F#" & Flow.Res.Name & "")
				api.Data.Calculate("A#" & Acct.ResEx.Name & ":O#Import:F#" & Flow.Res.Name & ":I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None=A#" & Acct.TopIS.Name & ":O#Import:F#None:I#Top:U3#Top:U4#Legal:U5#None:U6#None:U7#None:U8#None")
				api.Data.Calculate("A#" & Acct.ResEx.Name & ":O#Import:F#" & Flow.Clo.Name & "=A#" & Acct.ResEx.Name & ":O#Import:F#" & Flow.Res.Name & "")

				'Prior Year Result for Adjustments 12000AA
				api.Data.Calculate("A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.Aff.Name & "= - 1* A#" & Acct.ResEx.Name & ":O#AdjInput:F#" & Flow.Aff.Name & "")
				api.data.ClearCalculatedData ("A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.Aff.Name & ":U4#RetSoc1_PL", True, True, True)
				api.Data.Calculate("A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.Clo.Name & "= A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.TF.Name & "")

				
			ElseIf  (api.Pov.Cons.Name = "OwnerPostAdj") Or (api.Pov.Cons.Name = "OwnerPreAdj") Or api.cons.IsForeignCurrencyForEntity Then
			'only for AdjInput
			
				'BS Result Current & Prior 12900
				api.Data.Calculate("A#" & Acct.ResEx.Name & ":O#AdjInput:F#" & Flow.Aff.Name & "=-A#" & Acct.ResEx.Name & ":O#AdjInput:F#APM -A#" & Acct.ResEx.Name & ":O#AdjInput:F#APE")

				'Prior Year Result 12000AA
				api.Data.Calculate("A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.Aff.Name & "= - 1* A#" & Acct.ResEx.Name & ":O#AdjInput:F#" & Flow.Aff.Name & "")
				api.data.ClearCalculatedData ("A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.Aff.Name & ":U4#RetSoc1_PL", True, True, True) '20220412 NP Lu
				api.Data.Calculate("A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.Clo.Name & "= A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.TF.Name & "")

			End If
						
		End If
	End Sub
	
	Sub AcctRsvC (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim CurrTime As String = api.Pov.Time.Name
		Dim CurrYear As Integer = TimeDimHelper.GetSubComponentsFromName(CurrTime).Year

		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim FX As New GlobalClass(si, api,args)

		If Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
			Dim ComptesBilan As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, acct.TopBS.MemberId)
			
			api.Data.ClearCalculatedData("A#" & Acct.RsvC.name & ":F#" & Flow.ECO.name,True,True,True)
			api.Data.ClearCalculatedData("A#" & Acct.RsvC.name & ":F#" & Flow.ECF.name,True,True,True)
			
			'--------------------------------------------------------
		       		'II-4-2- Conversion du capital - Equity translation
		    '--------------------------------------------------------
			'Capitaux Propres hors Resultat
			Dim CPOUV_TC_TCO As String = "(A#Capital:F#" & Flow.OUV.name & ":C#Local{1} * " & fx.closingRateToText & "-A#Capital:F#" & Flow.OUV.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & ")"
'fs			Dim CPENT_TC_TCO As String = "(A#Capital:F#" & Flow.ENT.name & ":C#Local{1} * " & fx.closingRateToText & "-A#Capital:F#ENT{0})"
'fs			Dim CPFPA_TC_TCO As String = "(A#Capital:F#" & Flow.FPA.name & ":C#Local{1} * " & fx.closingRateToText & "-A#Capital:F#" & Flow.FPA.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & ")"
			Dim CPAFF_TC_TM1 As String = "(A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & fx.closingRatetoText & "-A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
			Dim CPAFF_TM_TM1 As String = "(A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
			Dim CPAFF_TMC_TM1 As String = "(A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & FX.averageRateToText & "-A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
			'Dim CPAFF_TM_TM1_SPL As String = "(A#Capital:F#" & Flow.AFF.name & ":U4#RetSoc1_PL:C#Local{1} * " & FX.closingRatetoText & "-A#Capital:F#" & Flow.AFF.name & ":U4#RetSoc1_PL:C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
			
			'brapi.errorlog.LogMessage(si, "APE - " & CPOUV_TC_TCO)
			
			
			'Calculate FX on Opening: CLO Local * (Closing Rate - Opening Rate) and same for ENT and FTA
			'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
			'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"
'fs			Dim sExpression As String = "A#" & Acct.RsvC.name & ":I#None:F#" & Flow.ECO.name & "{0}=" & CPOUV_TC_TCO & "+" & CPENT_TC_TCO & "+" & CPFPA_TC_TCO  & "+" & CPAFF_TC_TM1  & "-" & CPAFF_TM_TM1
			Dim sExpression As String = "A#" & Acct.RsvC.name & ":I#None:F#" & Flow.ECO.name & "{0}=" & CPOUV_TC_TCO & "+" & CPAFF_TC_TM1'  & "+" & CPAFF_TM_TM1   & "-" & CPAFF_TMC_TM1
			'Dim sExpression As String = "A#" & Acct.RsvC.name & ":I#None:F#" & Flow.ECO.name & "{0}=" & CPOUV_TC_TCO & "-" & CPAFF_TM_TM1
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				
			Dim CPCLO_TC_TM As String = "(A#Capital:F#" & Flow.CLO.name & ":C#Local{1} * " & fx.closingRateToText & "-A#Capital:F#" & Flow.CLO.name & ":C#Local{1} * " & FX.averageRateToText & ")"
			Dim CPOUV_TM_TC As String = "(A#Capital:F#" & Flow.OUV.name & ":C#Local{1} * " & FX.averageRateToText & "-A#Capital:F#" & Flow.OUV.name & ":C#Local{1} * " & fx.closingRateToText & ")"
'fs			Dim CPENT_TM_TC As String = "(A#Capital:F#" & Flow.ENT.name & ":C#Local{1} * " & FX.averageRateToText & "-A#Capital:F#" & Flow.ENT.name & ":C#Local{1} * " & fx.closingRateToText & ")"
'fs			Dim CPFPA_TM_TC As String = "(A#Capital:F#" & Flow.FPA.name & ":C#Local{1} * " & FX.averageRateToText & "-A#Capital:F#" & Flow.FPA.name & ":C#Local{1} * " & fx.closingRateToText & ")"
			Dim CPAFF_TM_TC As String = "(A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & FX.averageRateToText & "-A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & fx.closingRateToText & ")"
			Dim CPAFF_TM_TC1 As String = "(A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & FX.averageRateToText & "-A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & fx.closingRatePriorYEtoText & ")"
			Dim CPDIV_TM_TM1 As String = "(A#Capital:F#" & Flow.DIV.name & ":C#Local{1} * " & FX.averageRateToText & "-A#Capital:F#" & Flow.DIV.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
			'fs Calculation of CTA for flow entered in EUR to override the translated amount
			'Dim CPFPE_CLO_TC As String = "(A#Capital:F#" & Flow.CLO.name & ":C#Local{1} * " & FX.averageRateToText & "-A#Capital:F#FPE:C#Local{1} * 1)"
			Dim CPFPE_FPL_TC As String = "(A#Capital:F#FPL:C#Local{1}) * " & FX.averageRateToText & " - A#Capital:F#FPE:C#Local{1}) * 1)"
'			Dim CPAFF_TMC_TM1 As String = "(A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & FX.averageRateToText & "-A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"

			
'fs			sExpression = "A#" & Acct.RsvC.name & ":I#None:F#" & Flow.ECF.name & "{0}=" & CPCLO_TC_TM & "+" & CPOUV_TM_TC & "+" & CPENT_TM_TC & "+" & CPFPA_TM_TC  & "+" & CPAFF_TM_TC & "-" & CPDIV_TM_TM1
			sExpression = "A#" & Acct.RsvC.name & ":I#None:F#" & Flow.ECF.name & "{0}=" & CPCLO_TC_TM & "+" & CPOUV_TM_TC & "+" & CPAFF_TM_TC & "-" & CPDIV_TM_TM1 & "+" & CPFPE_FPL_TC & "+" & CPAFF_TMC_TM1 '& "+" & CPAFF_TM_TC1
			'sExpression = "A#" & Acct.RsvC.name & ":I#None:F#" & Flow.ECF.name & "{0}=" & CPCLO_TC_TM & "+" & CPOUV_TM_TC & "-" & CPAFF_TM_TC & "-" & CPDIV_TM_TM1 & "+" & CPFPE_FPL_TC
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

			sExpression = "A#" & Acct.RsvC.name & ":I#None:F#" & Flow.AFF.name & "{0}=A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText
			'api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			'api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

			
			'--------------------------------------------------------
					'II-4-1- Conversion du résultat - Net income translation
			'--------------------------------------------------------

			'Dim DC_AFF_129_TC1_TM1 As String = "(A#Resultats:F#" & Flow.AFF.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#Resultats:F#" & Flow.AFF.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
			'Dim DC_AFF_129RvPL_TC1_TM As String = "(A#Resultats:F#" & Flow.AFF.name & ":U4#RetSoc1_PL:C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#Resultats:F#" & Flow.AFF.name & ":U4#RetSoc1_PL:C#Local{1} * " & fx.averageRateToText & ")"
			
			sExpression = "A#" & Acct.ResC.name & ":I#None:F#" & Flow.AFF.name & "{0}=A#Resultats:F#" & Flow.AFF.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#Resultats:F#" & Flow.AFF.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText' & "+A#Resultats:F#" & Flow.AFF.name & ":U4#RetSoc1_PL:C#Local{1} * " & fx.closingRateToText & "-A#Resultats:F#" & Flow.AFF.name & ":U4#RetSoc1_PL:C#Local{1} * " & fx.averageRatePriorYEtoText
			'sExpression = "A#" & Acct.ResC.name & ":I#None:F#" & Flow.AFF.name & "{0}=" & DC_AFF_129_TC1_TM1 & "+" & DC_AFF_129RvPL_TC1_TM 
'fs			sExpression = "A#" & Acct.ResC.name & ":I#None:F#" & Flow.AFF.name & "{0}=A#129000:F#" & Flow.AFF.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#129000:F#" & Flow.AFF.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			
			'fs adjustment in the CTA for the DBE flow after clearing data from A#129000:U4#RetSoc1_PL:F#DBE in Sub AcctResEx 'FS Commented 1/12/2020 to validate 2020m3
'			sExpression = "A#" & Acct.ResC.name & ":U4#RetSoc1_PL:I#None:F#" & Flow.AFF.name & "{0}=A#Resultats:F#" & Flow.AFF.name & ":U4#RetSoc1_PL:C#Local{1} * " & FX.closingRateToText & "-A#Resultats:F#" & Flow.AFF.name & ":U4#RetSoc1_PL:C#Local{1} * " & fx.averageRatePriorYEtoText &"-A#Resultats:F#" & Flow.AFF.name & ":U4#RetSoc1_PL:C#Local{1} * " & FX.closingRateToText & "+A#Resultats:F#" & Flow.AFF.name & ":U4#RetSoc1_PL:C#Local{1} * " & fx.averageRateToText
'			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
'			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

'			sExpression = "A#" & Acct.ResC.name & ":U4#RetSoc1_PL:I#None:F#" & Flow.AFF.name & "{0}=+A#Resultats:F#" & Flow.AFF.name & ":U4#RetSoc1_PL:C#Local{1} * " & FX.closingRateToText & "-A#Resultats:F#" & Flow.AFF.name & ":U4#RetSoc1_PL:C#Local{1} * " & fx.averageRateToText
'			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
'			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			
			sExpression = "A#" & Acct.ResC.name & ":I#None:F#" & Flow.ECF.name & "{0}=A#Resultats:F#" & Flow.CLO.name & ":C#Local{1} * " & fx.closingRateToText & "-A#Resultats:F#" & Flow.CLO.name & ":C#Local{1} * " & FX.averageRateToText
'			sExpression = "A#" & Acct.ResC.name & ":I#None:F#" & Flow.ECF.name & "{0}=A#" & Acct.ResEx.Name & ":F#" & Flow.CLO.name & ":C#Local{1} * " & fx.closingRateToText & "-A#" & Acct.ResEx.Name & ":F#" & Flow.CLO.name & ":C#Local{1} * " & FX.averageRateToText
'			sExpression = "A#" & Acct.ResC.name & ":I#None:F#" & Flow.ECF.name & "{0}= 1"
			'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
			'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"			
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			
'fs disabled next section, as in Ingeteam Investment in subs must be translated like any other BS acct
			For Each Account As Member In ComptesBilan
				Dim RegleConversion As String = api.Account.Text(Account.MemberId, 2)
				
					
				If	RegleConversion="HIST_TIT" Then											
					'----------------------------------------------------------
			        'II-4-3- Calcul des Ecarts de conversion sur titres - Participating interests translation
			        '----------------------------------------------------------
					Dim OUV_TC_TCO As String = "(A#"+ Account.Name +":F#" & Flow.OUV.name & ":C#Local{1} * " & fx.closingRateToText & "-A#"+ Account.Name +":F#" & Flow.OUV.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & ")"
					Dim ENT_TC_TCO As String = "(A#"+ Account.Name +":F#" & Flow.ENT.name & ":C#Local{1} * " & fx.closingRateToText & "-A#"+ Account.Name +":F#ENT{0})"
					Dim FPA_TC_TCO As String = "(A#"+ Account.Name +":F#" & Flow.FPA.name & ":C#Local{1} * " & fx.closingRateToText & "-A#"+ Account.Name +":F#" & Flow.FPA.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & ")"
					
					'Calculate FX on Opening: CLO Local * (Closing Rate - Opening Rate) and same for ENT and FTA
					'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
					'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"					
					sExpression = "A#"+ Account.Name +"EH:F#" & Flow.ECO.name & "{0}=" & OUV_TC_TCO & "+" & ENT_TC_TCO & "+" & FPA_TC_TCO
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
					
					Dim CLO_TC_TM As String = "(A#"+ Account.Name +":F#" & Flow.CLO.name & ":C#Local{1} * " & fx.closingRateToText & "-A#"+ Account.Name +":F#" & Flow.CLO.name & ":C#Local{1} * " & FX.averageRateToText & ")"
					Dim OUV_TM_TC As String = "(A#"+ Account.Name +":F#" & Flow.OUV.name & ":C#Local{1} * " & FX.averageRateToText & "-A#"+ Account.Name +":F#" & Flow.OUV.name & ":C#Local{1} * " & fx.closingRateToText & ")"
					Dim ENT_TM_TC As String = "(A#"+ Account.Name +":F#" & Flow.ENT.name & ":C#Local{1} * " & FX.averageRateToText & "-A#"+ Account.Name +":F#" & Flow.ENT.name & ":C#Local{1} * " & fx.closingRateToText & ")"
					Dim FPA_TM_TC As String = "(A#"+ Account.Name +":F#" & Flow.FPA.name & ":C#Local{1} * " & FX.averageRateToText & "-A#"+ Account.Name +":F#" & Flow.FPA.name & ":C#Local{1} * " & fx.closingRateToText & ")"
					
				    'Calculate FX on Activity: CLO Local * (Closing Rate - Average Rate) and Reverse the impact from OUV, ENT, FTA, AFF and DIV (which are included in CLO balance)
					sExpression = "A#"+ Account.Name +"EH:F#" & Flow.ECF.name & "{0}=" & CLO_TC_TM & "+" & OUV_TM_TC & "+" & ENT_TM_TC & "+" & FPA_TM_TC
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
													
				ElseIf RegleConversion="HIST_PR" Then	
					'-----------------------------------------------------------
		        	'II-4-6- Traitement spécifiques de conversion provision - Provisions translation
		        	'-----------------------------------------------------------
					Dim OUV_TC_TCO As String = "(A#"+ Account.Name +":F#" & Flow.OUV.name & ":C#Local{1} * " & fx.closingRateToText & "-A#"+ Account.Name +":F#" & Flow.OUV.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & ")"
					Dim ENT_TC_TCO As String = "(A#"+ Account.Name +":F#" & Flow.ENT.name & ":C#Local{1} * " & fx.closingRateToText & "-A#"+ Account.Name +":F#ENT{0})"
					Dim FPA_TC_TCO As String = "(A#"+ Account.Name +":F#" & Flow.FPA.name & ":C#Local{1} * " & fx.closingRateToText & "-A#"+ Account.Name +":F#" & Flow.FPA.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & ")"
					
					'Calculate FX on Opening: CLO Local * (Closing Rate - Opening Rate) and same for ENT and FTA
					'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
					'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"
					sExpression = "A#"+ Account.Name +"EH:F#" & Flow.ECO.name & "{0}=" & OUV_TC_TCO & "+" & ENT_TC_TCO & "+" & FPA_TC_TCO
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
					
					Dim CLO_TC_TM As String = "(A#"+ Account.Name +":F#" & Flow.CLO.name & ":C#Local{1} * " & fx.closingRateToText & "-A#"+ Account.Name +":F#" & Flow.CLO.name & ":C#Local{1} * " & FX.averageRateToText & ")"
					Dim OUV_TM_TC As String = "(A#"+ Account.Name +":F#" & Flow.OUV.name & ":C#Local{1} * " & FX.averageRateToText & "-A#"+ Account.Name +":F#" & Flow.OUV.name & ":C#Local{1} * " & fx.closingRateToText & ")"
					Dim ENT_TM_TC As String = "(A#"+ Account.Name +":F#" & Flow.ENT.name & ":C#Local{1} * " & FX.averageRateToText & "-A#"+ Account.Name +":F#" & Flow.ENT.name & ":C#Local{1} * " & fx.closingRateToText & ")"
					Dim FPA_TM_TC As String = "(A#"+ Account.Name +":F#" & Flow.FPA.name & ":C#Local{1} * " & FX.averageRateToText & "-A#"+ Account.Name +":F#" & Flow.FPA.name & ":C#Local{1} * " & fx.closingRateToText & ")"
					Dim AFF_TM_TM1 As String = "(A#"+ Account.Name +":F#" & Flow.AFF.name & ":C#Local{1} * " & FX.averageRateToText & "-A#"+ Account.Name +":F#" & Flow.AFF.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
					Dim DIV_TM_TM1 As String = "(A#"+ Account.Name +":F#" & Flow.DIV.name & ":C#Local{1} * " & FX.averageRateToText & "-A#"+ Account.Name +":F#" & Flow.DIV.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
					Dim ODEP_TM_TC As String = "(A#"+ Account.Name +":F#" & Flow.DEXP.name & ":C#Local{1} * " & FX.averageRateToText & "-A#"+ Account.Name +":F#" & Flow.DEXP.name & ":C#Local{1} * " & fx.closingRateToText & ")"
					Dim FDEP_TM_TC As String = "(A#"+ Account.Name +":F#" & Flow.DFIN.name & ":C#Local{1} * " & FX.averageRateToText & "-A#"+ Account.Name +":F#" & Flow.DFIN.name & ":C#Local{1} * " & fx.closingRateToText & ")"
					Dim OREV_TM_TC As String = "(A#"+ Account.Name +":F#" & Flow.REXP.name & ":C#Local{1} * " & FX.averageRateToText & "-A#"+ Account.Name +":F#" & Flow.REXP.name & ":C#Local{1} * " & fx.closingRateToText & ")"
					Dim FREV_TM_TC As String = "(A#"+ Account.Name +":F#" & Flow.RFIN.name & ":C#Local{1} * " & FX.averageRateToText & "-A#"+ Account.Name +":F#" & Flow.RFIN.name & ":C#Local{1} * " & fx.closingRateToText & ")"
					Dim ODEP_TC_TM As String = "(A#"+ Account.Name +":F#" & Flow.DEXP.name & ":C#Local{1} * " & fx.closingRateToText & "-A#"+ Account.Name +":F#" & Flow.DEXP.name & ":C#Local{1} * " & FX.averageRateToText & ")"
					Dim FDEP_TC_TM As String = "(A#"+ Account.Name +":F#" & Flow.DFIN.name & ":C#Local{1} * " & fx.closingRateToText & "-A#"+ Account.Name +":F#" & Flow.DFIN.name & ":C#Local{1} * " & FX.averageRateToText & ")"
					Dim OREV_TC_TM As String = "(A#"+ Account.Name +":F#" & Flow.REXP.name & ":C#Local{1} * " & fx.closingRateToText & "-A#"+ Account.Name +":F#" & Flow.REXP.name & ":C#Local{1} * " & FX.averageRateToText & ")"
					Dim FREV_TC_TM As String = "(A#"+ Account.Name +":F#" & Flow.RFIN.name & ":C#Local{1} * " & fx.closingRateToText & "-A#"+ Account.Name +":F#" & Flow.RFIN.name & ":C#Local{1} * " & FX.averageRateToText & ")"
					
				    'Calculate FX on Activity: CLO Local * (Closing Rate - Average Rate) and Reverse the impact from OUV, ENT, FTA, AFF and DIV (which are included in CLO balance)
					'remove ODEP, FDEP, OREV, FREV, included in ECR
					'ECF flow for EH accounts
					'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
					'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"
					sExpression = "A#"+ Account.Name +"EH:F#" & Flow.ECF.name & "{0}=" & CLO_TC_TM & "+" & OUV_TM_TC & "+" & ENT_TM_TC & "+" & FPA_TM_TC & "+" & AFF_TM_TM1 & "+" & DIV_TM_TM1 & "+" & ODEP_TM_TC & "+" & FDEP_TM_TC & "-" & OREV_TM_TC & "-" & FREV_TM_TC
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
					'ECR flow for EH accounts
					sExpression = "A#"+ Account.Name +"EH:F#" & Flow.ECR.name & "{0}=" & ODEP_TC_TM & "+" & FDEP_TC_TM & "-" & OREV_TC_TM & "-" & FREV_TC_TM
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
					
																							
				End If
			Next
'fs disabled above section, as in Ingeteam Investment in subs must be translated like any other BS acct

		End If
	End Sub
	
	Sub SUPERAVIT (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		'Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		'Dim Acct As Object = Me.InitAccountClass(si, api, globals)

		If (Not api.Entity.HasChildren() And (api.Cons.IsLocalCurrencyforEntity())) Then
			'api.Data.Calculate("A#SUPERAVIT:F#SDC:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None=0")
			'api.Data.Calculate("A#SUPERAVIT:F#SDC:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None=-A#BSA:F#SDC:I#Top:U1#Top:U2#Top:U3#Top:U4#Legal:U5#None:U6#None:U7#None:U8#None+A#BSP:F#SDC:I#Top:U1#Top:U2#Top:U3#Top:U4#Legal:U5#None:U6#None:U7#None:U8#None")
			api.Data.ClearCalculatedData("A#SUPERAVIT:O#Import:I#None:U1#None:U2#None:U3#None:U5#None:U6#None:U7#None:U8#None", True, True, True)
			api.Data.ClearCalculatedData("A#SUPERAVIT:O#Forms:I#None:U1#None:U2#None:U3#None:U5#None:U6#None:U7#None:U8#None", True, True, True)
			
			api.Data.Calculate("A#SUPERAVIT:O#Import:F#MOV:I#None:U1#None:U2#None:U3#None:U5#None:U6#None:U7#None:U8#None = -A#BSA:O#Import:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U7#None:U8#None + A#BSP:O#Import:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U7#None:U8#None")
			api.Data.Calculate("A#SUPERAVIT:O#Import:F#SDC:I#None:U1#None:U2#None:U3#None:U5#None:U6#None:U7#None:U8#None = A#SUPERAVIT:O#Import:F#MOV:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U7#None:U8#None")
			api.Data.Calculate("A#SUPERAVIT:O#Forms:F#MOV:I#None:U1#None:U2#None:U3#None:U5#None:U6#None:U7#None:U8#None = -A#BSA:O#Forms:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U7#None:U8#None + A#BSP:O#Forms:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U7#None:U8#None")
			api.Data.Calculate("A#SUPERAVIT:O#Forms:F#SDC:I#None:U1#None:U2#None:U3#None:U5#None:U6#None:U7#None:U8#None = A#SUPERAVIT:O#Forms:F#MOV:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U7#None:U8#None")

		'SB "A#SUPERAVIT:F#SDC=#missing"
		'SB "A#SUPERAVIT:F#MOV=#missing"
		'SB "A#SUPERAVIT:F#MOV=A#SUPERAVIT:F#SDC-A#SUPERAVIT:F#APE"
		
		End If

	End Sub

	Sub AcctADJS (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	
		If (Not api.Entity.HasChildren() And (api.Cons.IsLocalCurrencyforEntity() Or (api.Pov.Cons.Name = "OwnerPostAdj") Or (api.Pov.Cons.Name = "OwnerPreAdj") Or api.cons.IsForeignCurrencyForEntity)) Then
				
			api.Data.Calculate("A#ADJS:O#AdjInput:F#REC:I#None:U1#None:U2#None:U3#None:U5#None:U6#None:U7#None:U8#None = -A#BSA:O#AdjInput:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U7#None:U8#None + A#BSP:O#AdjInput:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U7#None:U8#None")
			api.Data.Calculate("A#ADJS:O#AdjInput:F#SDC:I#None:U1#None:U2#None:U3#None:U5#None:U6#None:U7#None:U8#None = A#ADJS:O#AdjInput:F#REC:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U7#None:U8#None")
			
		End If
		
'		Comentado en 06/2021 por descuadre en IPT
'		If api.Pov.entity.Name = "IPT" Then
			
'			api.Data.Calculate("A#ADJS:O#AdjInput:F#REC:I#None:U1#None:U2#None:U3#None:U5#None:U6#None:U7#None:U8#None = -A#BSA:O#AdjInput:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U7#None:U8#None + A#BSP:O#AdjInput:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U7#None:U8#None")
'			api.Data.Calculate("A#ADJS:O#AdjInput:F#SDC:I#None:U1#None:U2#None:U3#None:U5#None:U6#None:U7#None:U8#None = A#ADJS:O#AdjInput:F#REC:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U7#None:U8#None")	
			
'		End If
		

	End Sub
	

#End Region

#Region "Member Formulas for Cash Flow"

Sub AcctTFTRESULTAT (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTRESULTAT:F#None" & UDDest & "=A#Resultats:F#" & Flow.RES.name & UDSource & "+A#69000T:F#None" & UDSource)
End Sub


Sub AcctTFTPVMVCessions (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTPVMVCessions:F#None" & UDDest & "=-A#PVMVCessions:F#None" & UDSource)
End Sub

Sub AcctTFTPROVISIONSCORPORELLESA (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTPROVISIONSCORPORELLESA:F#None" & UDDest & "=A#ProvisionsCorporelles:F#" & Flow.AUG.name & UDSource & "+A#ProvisionsCorporelles:F#" & Flow.REXP.name & UDSource)
End Sub


Sub AcctTFTPROVISIONSCORPORELLESD (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTPROVISIONSCORPORELLESD:F#None" & UDDest & "=A#ProvisionsCorporelles:F#" & Flow.DEPA.name & UDSource & "+A#ProvisionsCorporelles:F#" & Flow.DEPD.name & UDSource)
End Sub


Sub AcctTFTPROVISIONSINCORPORELLESA (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTPROVISIONSINCORPORELLESA:F#None" & UDDest & "=A#ProvisionsIncorporelles:F#" & Flow.AUG.name & UDSource & "+A#ProvisionsIncorporelles:F#" & Flow.REXP.name & UDSource)
End Sub


Sub AcctTFTPROVISIONSINCORPORELLESD (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTPROVISIONSINCORPORELLESD:F#None" & UDDest & "=A#ProvisionsIncorporelles:F#" & Flow.DEPA.name & UDSource & "+A#ProvisionsIncorporelles:F#" & Flow.DEPD.name & UDSource)
End Sub


Sub AcctTFTDEPRECIATIONECARTACQUISITION (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTDEPRECIATIONECARTACQUISITION:F#None" & UDDest & "=A#DepreciationEcartAcquisition:F#" & Flow.DEPA.name & UDSource)
End Sub


Sub AcctTFTAUTRESPROVISIONS (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim APAug As String
	Dim APODEP As String
	Dim APDim As String
	Dim APFDEP As String
	Dim APOREV As String
	Dim APFREV As String
	Dim APIIMP As String
	Dim APDIMP As String

	APAug="A#AutresProvisions:F#" & Flow.AUG.name & UDSource
	APODEP="A#AutresProvisions:F#" & Flow.DEXP.name & UDSource
	APDim="A#AutresProvisions:F#" & Flow.Dimi.name & UDSource
	APFDEP="A#AutresProvisions:F#" & Flow.DFIN.name & UDSource
	APOREV="A#AutresProvisions:F#" & Flow.REXP.name & UDSource
	APFREV="A#AutresProvisions:F#" & Flow.RFIN.name & UDSource
	APIIMP="A#AutresProvisions:F#" & Flow.DEPA.name & UDSource
	APDIMP="A#AutresProvisions:F#" & Flow.DEPD.name & UDSource

	api.Data.Calculate("A#TFTAUTRESPROVISIONS:F#None" & UDDest & "=" & APAug & "+" & APODEP & "+" & APDIM & "+" & APFDEP & "+" & APOREV & "+" & APFREV & "+" & APIIMP & "+" & APDIMP)
End Sub


Sub AcctTFTIMPOTSDIFFERES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim IDMvt As String
	Dim IDIDEF As String
	Dim IDDDEF As String
	Dim IDODEP As String
	Dim IDFDEP As String
	Dim IDOREV As String
	Dim IDFREV As String
	Dim CAIDEF As String
	Dim CADDEF As String
	Dim REIDEF As String
	Dim REDDEF As String

	IDMvt="A#ImpotsDifferes:F#" & Flow.MVT.name & UDSource
	IDIDEF="A#ImpotsDifferes:F#" & Flow.IDEF.name & UDSource
	IDDDEF="A#ImpotsDifferes:F#" & Flow.DDEF.name & UDSource
	IDODEP="A#ImpotsDifferes:F#" & Flow.DEXP.name & UDSource
	IDFDEP="A#ImpotsDifferes:F#" & Flow.DFIN.name & UDSource
	IDOREV="A#ImpotsDifferes:F#" & Flow.REXP.name & UDSource
	IDFREV="A#ImpotsDifferes:F#" & Flow.RFIN.name & UDSource
	CAIDEF="A#Capital:F#" & Flow.IDEF.name & UDSource
	CADDEF="A#Capital:F#" & Flow.DDEF.name & UDSource
	REIDEF="A#Reserves:F#" & Flow.IDEF.name & UDSource
	REDDEF="A#Reserves:F#" & Flow.DDEF.name & UDSource

	api.Data.Calculate("A#TFTIMPOTSDIFFERES:F#None" & UDDest & "=" & IDMvt & "+" & IDIDEF & "+" & IDDDEF & "+" & IDODEP & "+" & IDFDEP & "+" & IDOREV & "+" & IDFREV & "+" & CAIDEF & "+" & CADDEF & "+" & REIDEF & "+" & REDDEF)
End Sub


Sub AcctTFTIMPOTS (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTIMPOTS:F#None" & UDDest & "=-A#69000T:F#None" & UDSource)
End Sub


Sub AcctTFTINTERETSCOURUS (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim ICMvt As String
	Dim ICAug As String
	Dim ICDim As String

	ICMvt="A#InteretsCourus:F#" & Flow.MVT.name & UDSource
	ICAug="A#InteretsCourus:F#" & Flow.AUG.name & UDSource
	ICDim="A#InteretsCourus:F#" & Flow.Dimi.name & UDSource

	api.Data.Calculate("A#TFTINTERETSCOURUS:F#None" & UDDest & "=" & ICMvt & "+" & ICAug & "+" & ICDim)
End Sub

Sub AcctTFTSTOCKS (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim STMvt As String
	Dim STAug As String
	Dim STDim As String
	Dim STODEP As String
	Dim STOREV As String

	STMvt="A#Stocks:F#" & Flow.MVT.name & UDSource
	STAug="A#Stocks:F#" & Flow.AUG.name & UDSource
	STDim="A#Stocks:F#" & Flow.Dimi.name & UDSource
	STODEP="A#Stocks:F#" & Flow.DEXP.name & UDSource
	STOREV="A#Stocks:F#" & Flow.REXP.name & UDSource

	api.Data.Calculate("A#TFTSTOCKS:F#None" & UDDest & "=-" & STMvt & "-" & STAug & "+" & STDim & "+" & STODEP & "+" & STOREV)
End Sub


Sub AcctTFTCREANCESCLIENTS (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim CCMvt As String
	Dim CCODEP As String
	Dim CCOREV As String

	CCMvt="A#CreancesClients:F#" & Flow.MVT.name & UDSource
	CCODEP="A#CreancesClients:F#" & Flow.DEXP.name & UDSource
	CCOREV="A#CreancesClients:F#" & Flow.REXP.name & UDSource

	api.Data.Calculate("A#TFTCREANCESCLIENTS:F#None" & UDDest & "=-" & CCMvt & "-" & CCODEP & "-" & CCOREV)
End Sub


Sub AcctTFTFOURNISSEURS (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTFOURNISSEURS:F#None" & UDDest & "=A#Fournisseurs:F#" & Flow.MVT.name & UDSource)
End Sub


Sub AcctTFTAUTRESBFROPERATIONNEL (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim BOMvt As String
	Dim BOODEP As String
	Dim BOOREV As String

	BOMvt="A#AutresBFROperationnel:F#" & Flow.MVT.name & UDSource
	BOODEP="A#AutresBFROperationnel:F#" & Flow.DEXP.name & UDSource
	BOOREV="A#AutresBFROperationnel:F#" & Flow.REXP.name & UDSource

	api.Data.Calculate("A#TFTAUTRESBFROPERATIONNEL:F#None" & UDDest & "=-" & BOMvt & "+" & BOODEP & "+" & BOOREV)
End Sub


Sub AcctTFTAUTRESBFRNONOPERATIONNEL (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim BNMvt As String
	Dim BNAug As String
	Dim BNDim As String

	BNMvt="A#AutresBFRNonOperationnel:F#" & Flow.MVT.name & UDSource
	BNAug="A#AutresBFRNonOperationnel:F#" & Flow.AUG.name & UDSource
	BNDim="A#AutresBFRNonOperationnel:F#" & Flow.Dimi.name & UDSource

	api.Data.Calculate("A#TFTAUTRESBFRNONOPERATIONNEL:F#None" & UDDest & "=-" & BNMvt & "-" & BNAug & "+" & BNDim)
End Sub


Sub AcctTFTCOMPTESLIAISON (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim CLMvt As String
	Dim CLAug As String
	Dim CLDim As String

	CLMvt="A#ComptesLiaison:F#" & Flow.MVT.name & UDSource
	CLAug="A#ComptesLiaison:F#" & Flow.AUG.name & UDSource
	CLDim="A#ComptesLiaison:F#" & Flow.Dimi.name & UDSource

	api.Data.Calculate("A#TFTCOMPTESLIAISON:F#None" & UDDest & "=" & CLMvt & "+" & CLAug & "+" & CLDim)
End Sub


Sub AcctTFTINSTRUMENTSFINDERIVES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim DIAug As String
	Dim DIDim As String
	Dim DIIREVA As String
	Dim DIDREVA As String
	Dim RVIREVA As String
	Dim RVDREVA As String

	DIAug="A#InstrumentsFinanciersDerives:F#" & Flow.Aug.name & UDSource
	DIDim="A#InstrumentsFinanciersDerives:F#" & Flow.Dimi.name & UDSource
	DIIREVA="A#InstrumentsFinanciersDerives:F#" & Flow.IREVA.name & UDSource
	DIDREVA="A#InstrumentsFinanciersDerives:F#" & Flow.DREVA.name & UDSource
	RVIREVA="A#107100:F#" & Flow.IREVA.name & UDSource
	RVDREVA="A#107100:F#" & Flow.DREVA.name & UDSource

	api.Data.Calculate("A#TFTINSTRUMENTSFINDERIVES:F#None" & UDDest & "=" & DIAug & "+" & DIDim & "+" & DIIREVA & "+" & DIDREVA & "+" & RVIREVA & "+" & RVDREVA)
End Sub


Sub AcctTFTAUTRESACTIFSFIN (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim RVIREVA As String
	Dim RVDREVA As String

	RVIREVA="A#107200:F#" & Flow.IREVA.name & UDSource
	RVDREVA="A#107200:F#" & Flow.DREVA.name & UDSource

	api.Data.Calculate("A#TFTAUTRESACTIFSFIN:F#None" & UDDest & "=" & RVIREVA & "+" & RVDREVA)
End Sub


Sub AcctTFTREEVALUATIONIMMOS (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim IIIREVA As String
	Dim IIDREVA As String
	Dim ICIREVA As String
	Dim ICDREVA As String
	Dim RVIREVA As String
	Dim RVDREVA As String

	IIIREVA="A#ImmobilisationsIncorporelles:F#" & Flow.IREVA.name & UDSource
	IIDREVA="A#ImmobilisationsIncorporelles:F#" & Flow.DREVA.name & UDSource
	ICIREVA="A#ImmobilisationsCorporelles:F#" & Flow.IREVA.name & UDSource
	ICDREVA="A#ImmobilisationsCorporelles:F#" & Flow.DREVA.name & UDSource
	RVIREVA="A#107300:F#" & Flow.IREVA.name & UDSource
	RVDREVA="A#107300:F#" & Flow.DREVA.name & UDSource

	api.Data.Calculate("A#TFTREEVALUATIONIMMOS:F#None" & UDDest & "=" & IIIREVA & "+" & IIDREVA & "+" & ICIREVA & "+" & ICDREVA & "+" & RVIREVA & "+" & RVDREVA)
End Sub


Sub AcctTFTREEVALUATIONRESERVES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim REIREVA As String
	Dim REDREVA As String

	REIREVA="A#Reserves:F#" & Flow.IREVA.name & UDSource
	REDREVA="A#Reserves:F#" & Flow.DREVA.name & UDSource

	api.Data.Calculate("A#TFTREEVALUATIONRESERVES:F#None" & UDDest & "=" & REIREVA & "+" & REDREVA)
End Sub

Sub AcctTFTACQIMMOSCORPORELLES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim ICAug As String

	ICAug="A#ImmobilisationsCorporelles:F#" & Flow.AUG.name & UDSource

	api.Data.Calculate("A#TFTACQIMMOSCORPORELLES:F#None" & UDDest & "=-" & ICAug)
End Sub


Sub AcctTFTACQIMMOSINCORPORELLES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim IIAug As String

	IIAug="A#ImmobilisationsIncorporelles:F#" & Flow.AUG.name & UDSource

	api.Data.Calculate("A#TFTACQIMMOSINCORPORELLES:F#None" & UDDest & "=-" & IIAug)
End Sub


Sub AcctTFTACQIMMOSFINANCIERES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim IFAug As String

	IFAug="A#ImmobilisationsFinancieres:F#" & Flow.AUG.name & UDSource

	api.Data.Calculate("A#TFTACQIMMOSFINANCIERES:F#None" & UDDest & "=-" & IFAug)
End Sub


Sub AcctTFTACQTITRES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim TIAug As String
	Dim TICAPINC As String
	Dim ACAug As String

	TIAug="A#Titres:F#" & Flow.AUG.name & UDSource
	TICAPINC="A#Titres:F#" & Flow.AUGCAP.name & UDSource
	ACAug="A#AutoControle:F#" & Flow.AUG.name & UDSource

	api.Data.Calculate("A#TFTACQTITRES:F#None" & UDDest & "=-" & TIAug & "-" & TICAPINC & "-" & ACAug)
End Sub


Sub AcctTFTPRETS (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim PRAug As String
	Dim PRDim As String

	PRAug="A#Prets:F#" & Flow.AUG.name & UDSource
	PRDim="A#Prets:F#" & Flow.Dimi.name & UDSource

	api.Data.Calculate("A#TFTPRETS:F#None" & UDDest & "=" & PRAug & "+" & PRDim)
End Sub


Sub AcctTFTINTCOURUSCREANCES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim IRMvt As String
	Dim IRAug As String
	Dim IRDim As String

	IRMvt="A#InteretsCourusCreances:F#" & Flow.MVT.name & UDSource
	IRAug="A#InteretsCourusCreances:F#" & Flow.AUG.name & UDSource
	IRDim="A#InteretsCourusCreances:F#" & Flow.Dimi.name & UDSource

	api.Data.Calculate("A#TFTINTCOURUSCREANCES:F#None" & UDDest & "=" & IRMvt & "+" & IRAug & "+" & IRDim)
End Sub


Sub AcctTFTSUBVENTIONSINVEST (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim SIAug As String
	Dim SIDim As String

	SIAug="A#SubventionsInvestissement:F#" & Flow.Aug.name & UDSource
	SIDim="A#SubventionsInvestissement:F#" & Flow.Dimi.name & UDSource

	api.Data.Calculate("A#TFTSUBVENTIONSINVEST:F#None" & UDDest & "=" & SIAug & "+" & SIDim)
End Sub


Sub AcctTFTCESSIMMOSCORPORELLES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim ICDim As String
	Dim PIDim As String
	Dim PVMVCorp As String

	ICDim="A#ImmobilisationsCorporelles:F#" & Flow.Dimi.name & UDSource
	PIDim="A#ProvisionsCorporelles:F#" & Flow.Dimi.name & UDSource
	PVMVCorp="A#PVMVCorporelles:F#None" & UDSource

	api.Data.Calculate("A#TFTCESSIMMOSCORPORELLES:F#None" & UDDest & "=-" & ICDim & "+" & PIDim & "+" & PVMVCorp)
End Sub


Sub AcctTFTCESSIMMOSINCORPORELLES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim IIDim As String
	Dim PIDim As String

	IIDim="A#ImmobilisationsIncorporelles:F#" & Flow.Dimi.name & UDSource
	PIDim="A#ProvisionsIncorporelles:F#" & Flow.Dimi.name & UDSource

	api.Data.Calculate("A#TFTCESSIMMOSINCORPORELLES:F#None" & UDDest & "=" & IIDim & "+" & PIDim)
End Sub


Sub AcctTFTCESSIMMOSFINANCIERES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim TIDim As String
	Dim TICAPDEC As String
	Dim IFDim As String
	Dim ACDim As String

	TIDim="A#Titres:F#" & Flow.Dimi.name & UDSource
	TICAPDEC="A#Titres:F#" & Flow.DIMCAP.name & UDSource
	IFDim="A#ImmobilisationsFinancieres:F#" & Flow.Dimi.name & UDSource
	ACDim="A#AutoControle:F#" & Flow.Dimi.name & UDSource

	api.Data.Calculate("A#TFTCESSIMMOSFINANCIERES:F#None" & UDDest & "=" & TIDim & "+" & TICAPDEC & "+" & IFDim & "+" & ACDim)
End Sub


Sub AcctTFTRESULTATSMEE (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim TINin As String

	TINin="A#Titres:F#" & Flow.RES.name & UDSource

	api.Data.Calculate("A#TFTRESULTATSMEE:F#None" & UDDest & "=" & TINin)
End Sub


Sub AcctTFTAUGMENTATIONCAPITAL (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim BOAug As String
	Dim BODim As String
	Dim RSAug As String
	Dim RSDim As String
	Dim RSCAPINC As String
	Dim RSCAPDEC As String
	Dim CAAug As String
	Dim CADim As String

	BOAug="A#AutresBFROperationnel:F#" & Flow.Aug.name & UDSource
	BODim="A#AutresBFROperationnel:F#" & Flow.Dimi.name & UDSource
	RSAug="A#Reserves:F#" & Flow.Aug.name & UDSource
	RSDim="A#Reserves:F#" & Flow.Dimi.name & UDSource
	RSCAPINC="A#Reserves:F#" & Flow.AUGCAP.name & UDSource
	RSCAPDEC="A#Reserves:F#" & Flow.DIMCAP.name & UDSource
	CAAug="A#Capital:F#" & Flow.Aug.name & UDSource
	CADim="A#Capital:F#" & Flow.Dimi.name & UDSource

	api.Data.Calculate("A#TFTAUGMENTATIONCAPITAL:F#None" & UDDest & "=" & BOAug & "+" & BODim & "+" & RSAug & "+" & RSDim & "+" & RSCAPINC & "+" & RSCAPDEC & "+" & CAAug & "+" & CADim)
End Sub


Sub AcctTFTEMISSIONEMPRUNTS (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim EMAug As String

	EMAug="A#Emprunts:F#" & Flow.AUG.name & UDSource

	api.Data.Calculate("A#TFTEMISSIONEMPRUNTS:F#None" & UDDest & "=" & EMAug)
End Sub


Sub AcctTFTREMBOURSEMENTEMPRUNTS (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim EMDim As String

	EMDim="A#Emprunts:F#" & Flow.Dimi.name & UDSource

	api.Data.Calculate("A#TFTREMBOURSEMENTEMPRUNTS:F#None" & UDDest & "=" & EMDim)
End Sub


Sub AcctTFTAUTRESDETTESFINNC (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim DNAug As String
	Dim DNDim As String
	Dim DNMvt As String

	DNAug="A#DettesFinNonCourantes:F#" & Flow.AUG.name & UDSource
	DNDim="A#DettesFinNonCourantes:F#" & Flow.Dimi.name & UDSource
	DNMvt="A#DettesFinNonCourantes:F#" & Flow.MVT.name & UDSource

	api.Data.Calculate("A#TFTAUTRESDETTESFINNC:F#None" & UDDest & "=" & DNAug & "-" & DNDim & "+" & DNMvt)
End Sub


Sub AcctTFTAUTRESDETTESFINC (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim DCAug As String
	Dim DCDim As String
	Dim DCMvt As String

	DCAug="A#DettesFinCourantes:F#" & Flow.AUG.name & UDSource
	DCDim="A#DettesFinCourantes:F#" & Flow.Dimi.name & UDSource
	DCMvt="A#DettesFinCourantes:F#" & Flow.MVT.name & UDSource

	api.Data.Calculate("A#TFTAUTRESDETTESFINC:F#None" & UDDest & "=" & DCAug & "-" & DCDim & "+" & DCMvt)
End Sub


Sub AcctTFTDIVIDENDESVERSES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	Dim TIAff As String
	Dim TIDiv As String
	Dim TIIDIV As String
	Dim CAAff As String
	Dim CADiv As String
	Dim CAIDIV As String
	Dim ACAff As String
	Dim ACDiv As String
	Dim ACIDIV As String
	Dim REAff As String
	Dim REDiv As String
	Dim REIDIV As String
	Dim RTAff As String

	TIAff="A#Titres:F#" & Flow.AFF.name & UDSource
	TIDiv="A#Titres:F#" & Flow.DIV.name & UDSource
	TIIDIV="A#Titres:F#" & Flow.IDiv.name & UDSource
	CAAff="A#Capital:F#" & Flow.AFF.name & UDSource
	CADiv="A#Capital:F#" & Flow.DIV.name & UDSource
	CAIDIV="A#Capital:F#" & Flow.IDiv.name & UDSource
	ACAff="A#AutoControle:F#" & Flow.AFF.name & UDSource
	ACDiv="A#AutoControle:F#" & Flow.DIV.name & UDSource
	ACIDIV="A#AutoControle:F#" & Flow.IDiv.name & UDSource
	REAff="A#Reserves:F#" & Flow.AFF.name & UDSource
	REDiv="A#Reserves:F#" & Flow.DIV.name & UDSource
	REIDIV="A#Reserves:F#" & Flow.IDiv.name & UDSource
	RTAff="A#Resultats:F#" & Flow.AFF.name & UDSource

	api.Data.Calculate("A#TFTDIVIDENDESVERSES:F#None" & UDDest & "=" & TIAff & "+" & TIDiv & "+" & TIIDIV & "+" & CAAff & "+" & CADiv & "+" & CAIDIV & "+" & ACAff & "+" & ACDiv & "+" & ACIDIV & "+" & REAff & "+" & REDiv & "+" & REIDIV & "+" & RTAff)
End Sub


Sub AcctTFTECARTCONV (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTCONV:F#None" & UDDest & "=-A#" & acct.TopBS.name & ":F#" & Flow.ECO.name & UDSource & "-A#" & acct.TopBS.name & ":F#" & Flow.ECF.name & UDSource & "-A#" & acct.TopBS.name & ":F#" & Flow.ECR.name & UDSource & "+A#" & Acct.NetCash.name & ":F#" & Flow.ECO.name & UDSource & "+A#" & Acct.NetCash.name & ":F#" & Flow.ECF.name & UDSource & "+A#" & Acct.NetCash.name & ":F#" & Flow.ECR.name & UDSource)
End Sub


Sub AcctTFTVARPER (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTVARPER:F#None" & UDDest & "=A#" & acct.TopBS.name & ":F#" & Flow.VAR.name & UDSource & "-A#" & Acct.NetCash.name & ":F#" & Flow.VAR.name & UDSource)
End Sub


Sub AcctTFTECARTREC (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTREC:F#None" & UDDest & "=A#" & acct.TopBS.name & ":F#" & Flow.REC.name & UDSource & "-A#" & Acct.NetCash.name & ":F#" & Flow.REC.name & UDSource)
End Sub


Sub AcctTFTECARTFUS (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTFUS:F#None" & UDDest & "=-A#" & acct.TopBS.name & ":F#" & Flow.FUS.name & UDSource & "+A#" & Acct.NetCash.name & ":F#" & Flow.FUS.name & UDSource)
End Sub


Sub AcctTFTECARTENT (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTENT:F#None" & UDDest & "=A#" & acct.TopBS.name & ":F#" & Flow.ENT.name & UDSource & "-A#" & Acct.NetCash.name & ":F#" & Flow.ENT.name & UDSource)
End Sub


Sub AcctTFTECARTERR (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTERR:F#None" & UDDest & "=A#" & acct.TopBS.name & ":F#" & Flow.ERR.name & UDSource & "-A#" & Acct.NetCash.name & ":F#" & Flow.ERR.name & UDSource)
End Sub


Sub AcctTFTECARTSOR (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTSOR:F#None" & UDDest & "=A#" & acct.TopBS.name & ":F#" & Flow.SOR.name & UDSource & "-A#" & Acct.NetCash.name & ":F#" & Flow.SOR.name & UDSource)
End Sub


Sub AcctTFTECARTFPA (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTFPA:F#None" & UDDest & "=A#" & acct.TopBS.name & ":F#" & Flow.FPA.name & UDSource & "-A#" & Acct.NetCash.name & ":F#" & Flow.FPA.name & UDSource)
End Sub


Sub AcctTFTCLO (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTCLO:F#None" & UDDest & "=A#" & Acct.NetCash.name & ":F#" & Flow.CLO.name & UDSource)
End Sub


Sub AcctTFTOUV (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTOUV:F#None" & UDDest & "=A#" & Acct.NetCash.name & ":F#" & Flow.OUV.name & UDSource)
End Sub


Sub AcctTFTECARTCONVTRESORERIE (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTCONVTRESORERIE:F#None" & UDDest & "=A#" & Acct.NetCash.name & ":F#" & Flow.ECO.name & UDSource & "+A#" & Acct.NetCash.name & ":F#" & Flow.ECF.name & UDSource & "+A#" & Acct.NetCash.name & ":F#" & Flow.ECR.name & UDSource)
End Sub


Sub AcctTFTECARTVARPERTRESORERIE (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTVARPERTRESORERIE:F#None" & UDDest & "=A#" & Acct.NetCash.name & ":F#" & Flow.VAR.name & UDSource)
End Sub


Sub AcctTFTECARTRECTRESORERIE (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTRECTRESORERIE:F#None" & UDDest & "=A#" & Acct.NetCash.name & ":F#" & Flow.REC.name & UDSource)
End Sub


Sub AcctTFTECARTFUSTRESORERIE (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTFUSTRESORERIE:F#None" & UDDest & "=A#" & Acct.NetCash.name & ":F#" & Flow.FUS.name & UDSource)
End Sub


Sub AcctTFTECARTENTTRESORERIE (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTENTTRESORERIE:F#None" & UDDest & "=A#" & Acct.NetCash.name & ":F#" & Flow.ENT.name & UDSource)
End Sub


Sub AcctTFTECARTERRTRESORERIE (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTERRTRESORERIE:F#None" & UDDest & "=A#" & Acct.NetCash.name & ":F#" & Flow.ERR.name & UDSource)
End Sub


Sub AcctTFTECARTSORTRESORERIE (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTSORTRESORERIE:F#None" & UDDest & "=A#" & Acct.NetCash.name & ":F#" & Flow.SOR.name & UDSource)
End Sub


Sub AcctTFTECARTFPATRESORERIE (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	Dim Acct As Object = Me.InitAccountClass(si, api, globals)
	Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
	Dim UDSource As String 
	Dim UDDest As String

	UDSource=":I#None:UD1#TOTUD1:UD2#TOTUD2:UD3#TOTUD3:" & Nat.NatDimType.Abbrev & "#NATURES"
	UDDest=":I#None:UD1#None:UD2#None:UD3#None:" & Nat.NatDimType.Abbrev & "#None"

	api.Data.Calculate("A#TFTECARTFPATRESORERIE:F#None" & UDDest & "=A#" & Acct.NetCash.name & ":F#" & Flow.FPA.name & UDSource)
End Sub

#End Region

#Region "Member Formulas for Tech Accounts"

Sub AcctEA (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	If Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
		
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim timeprioryearend As Integer = api.Time.GetLastPeriodInPriorYear
		Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability()
		Dim rateTypeAve As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp()
		Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
		Dim closingRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeClo,timeprioryearend)
		Dim AverageRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
		Dim AverageRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeAve,timeprioryearend)
				
		'----------------------------------------------------------
		'II-4-4- Calcul des Ecarts d'aquisition positifs - Goodwill translation
		'----------------------------------------------------------		
		Dim OUV_TC_TCO As String = "(A#EA:F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#EA:F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRatePriorYE) & ")"'HC
		Dim ENT_TC_TCO As String = "(A#EA:F#" & Flow.ENT.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#EA:F#ENT{0})"
		Dim FTA_TC_TCO As String = "(A#EA:F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#EA:F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRatePriorYE) & ")"
		
		'Calculate FX on Opening: CLO Local * (Closing Rate - Opening Rate) and same for ENT and FTA
		Dim sExpression As String = "A#EAEH:F#" & Flow.ECO.name & "{0}=" & OUV_TC_TCO & "+" & ENT_TC_TCO & "+" & FTA_TC_TCO
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
		
		Dim CLO_TC_TM As String = "(A#EA:F#" & Flow.CLO.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#EA:F#" & Flow.CLO.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & ")"
		Dim OUV_TM_TC As String = "(A#EA:F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#EA:F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
		Dim ENT_TM_TC As String = "(A#EA:F#" & Flow.ENT.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#EA:F#" & Flow.ENT.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
		Dim FTA_TM_TC As String = "(A#EA:F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#EA:F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
		
		'Calculate FX on Activity: CLO Local * (Closing Rate - Average Rate) and Reverse the impact from OUV, ENT, FTA, AFF and DIV (which are included in CLO balance)
		sExpression = "A#EAEH:F#" & Flow.ECF.name & "{0}=" & CLO_TC_TM & "+" & OUV_TM_TC & "+" & ENT_TM_TC & "+" & FTA_TM_TC
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
		
	End If
End Sub

Sub AcctAEA (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	If Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
		
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim timeprioryearend As Integer = api.Time.GetLastPeriodInPriorYear
		Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability()
		Dim rateTypeAve As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp()
		Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
		Dim closingRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeClo,timeprioryearend)
		Dim AverageRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
		Dim AverageRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeAve,timeprioryearend)

		'----------------------------------------------------------
		'II-4-5- Traitement spécifiques de conversion amortist Ecart d'acq. - Goodwill depreciation translation
		'----------------------------------------------------------
		Dim OUV_TC_TCO As String = "(A#AEA:F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#AEA:F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRatePriorYE) & ")"
		Dim ENT_TC_TCO As String = "(A#AEA:F#" & Flow.ENT.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#AEA:F#ENT{0})"
		Dim FTA_TC_TCO As String = "(A#AEA:F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#AEA:F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRatePriorYE) & ")"
		
		'Calculate FX on Opening: CLO Local * (Closing Rate - Opening Rate) and same for ENT and FTA
		Dim sExpression As String = "A#AEAEH:F#" & Flow.ECO.name & "{0}=" & OUV_TC_TCO & "+" & ENT_TC_TCO & "+" & FTA_TC_TCO
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
		
		Dim CLO_TC_TM As String = "(A#AEA:F#" & Flow.CLO.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#AEA:F#" & Flow.CLO.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & ")"
		Dim OUV_TM_TC As String = "(A#AEA:F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#AEA:F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
		Dim ENT_TM_TC As String = "(A#AEA:F#" & Flow.ENT.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#AEA:F#" & Flow.ENT.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
		Dim FTA_TM_TC As String = "(A#AEA:F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#AEA:F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
		Dim IIMP_TM_TC As String = "(A#AEA:F#" & Flow.DEPA.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#AEA:F#" & Flow.DEPA.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
		
		sExpression = "A#AEAEH:F#" & Flow.ECF.name & "{0}=" & CLO_TC_TM & "+" & OUV_TM_TC & "+" & ENT_TM_TC & "+" & FTA_TM_TC & "+" & IIMP_TM_TC
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

		sExpression = "A#AEAEH:F#" & Flow.ECR.name & "{0}=A#AEA:F#" & Flow.DEPA.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#AEA:F#" & Flow.DEPA.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate)
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

	End If
End Sub

Sub AcctMS310000 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	If Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
		Dim ComptesMargeStock As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "MSTOCKS"))
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim timeprioryearend As Integer = api.Time.GetLastPeriodInPriorYear
		Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability()
		Dim rateTypeAve As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp()
		Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
		Dim closingRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeClo,timeprioryearend)
		Dim AverageRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
		Dim AverageRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeAve,timeprioryearend)
		
		For Each Account As Member In ComptesMargeStock		
			'-----------------------------------------------------------
			'II-4-7- Traitement spécifiques de conversion Marge sur Stock - Profit on stock translation
			'-----------------------------------------------------------
			Dim OUV_TC_TCO As String = "(A#"+Account.Name.ToString+":F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#"+Account.Name.ToString+":F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRatePriorYE) & ")"
			Dim ENT_TC_TCO As String = "(A#"+Account.Name.ToString+":F#" & Flow.ENT.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#"+Account.Name.ToString+":F#ENT{1})"
			Dim FTA_TC_TCO As String = "(A#"+Account.Name.ToString+":F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#"+Account.Name.ToString+":F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRatePriorYE) & ")"
			
			'Calculate FX on Opening: CLO Local * (Closing Rate - Opening Rate) and same for ENT and FTA
			Dim sExpression As String = "A#"+Account.Name.ToString+"EH:F#" & Flow.ECO.name & "{0}=" & OUV_TC_TCO & "+" & ENT_TC_TCO & "+" & FTA_TC_TCO
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			
			Dim CLO_TC_TM As String = "(A#"+Account.Name.ToString+":F#" & Flow.CLO.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#"+Account.Name.ToString+":F#" & Flow.CLO.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & ")"
			Dim OUV_TM_TC As String = "(A#"+Account.Name.ToString+":F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#"+Account.Name.ToString+":F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
			Dim ENT_TM_TC As String = "(A#"+Account.Name.ToString+":F#" & Flow.ENT.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#"+Account.Name.ToString+":F#" & Flow.ENT.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
			Dim FTA_TM_TC As String = "(A#"+Account.Name.ToString+":F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#"+Account.Name.ToString+":F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
			
			'Calculate FX on Activity: CLO Local * (Closing Rate - Average Rate) and Reverse the impact from OUV, ENT, FTA, AFF and DIV (which are included in CLO balance)
			sExpression = "A#"+Account.Name.ToString+"EH:F#" & Flow.ECF.name & "{0}=" & CLO_TC_TM & "+" & OUV_TM_TC & "+" & ENT_TM_TC & "+" & FTA_TM_TC
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			
			'le flux MVT ont un impact Ecart de Change sur le Ecart Change résultat
			'Movement has an impact on translation adjustment - Result (ECR)
			sExpression = "A#"+Account.Name.ToString+"EH:F#" & Flow.ECR.name & "{0}=A#"+Account.Name.ToString+":F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#"+Account.Name.ToString+":F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate)
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			
			'il faut dont annuler son impact déjà pris en compte sur l'écart de change sur flux
			'it is necessary to cancel its impact already entered on translation adjustment - Flow (ECF)
			sExpression = "A#"+Account.Name.ToString+"EH:F#" & Flow.ECF.name & "{0}=A#"+Account.Name.ToString+"EH:F#" & Flow.ECF.name & "+A#"+Account.Name.ToString+":F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#"+Account.Name.ToString+":F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate)
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

		Next	
	End If
End Sub

Sub AcctDIVV (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	If Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
		
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim timeprioryearend As Integer = api.Time.GetLastPeriodInPriorYear
		Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability()
		Dim rateTypeAve As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp()
		Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
		Dim closingRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeClo,timeprioryearend)
		Dim AverageRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
		Dim AverageRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeAve,timeprioryearend)

		'-----------------------------------------------------------
		'II-4-8- Conversion des dividendes versés - Paid dividends translation
		'-----------------------------------------------------------		
		Dim sExpression As String = "A#DIVV:F#" & Flow.None.name & "{0}=A#DIVV:F#" & Flow.None.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRatePriorYE)
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
		
	End If
End Sub

Sub AcctDIVEQUI (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	If Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
		
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim timeprioryearend As Integer = api.Time.GetLastPeriodInPriorYear
		Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability()
		Dim rateTypeAve As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp()
		Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
		Dim closingRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeClo,timeprioryearend)
		Dim AverageRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
		Dim AverageRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeAve,timeprioryearend)

		'-----------------------------------------------------------
		'II-4-8- Conversion des dividendes versés - Paid dividends translation
		'-----------------------------------------------------------														
		Dim sExpression As String = "A#DIVEQUI:F#" & Flow.DIV.name & "{0}=A#DIVEQUI:F#" & Flow.DIV.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRatePriorYE)
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

	End If
End Sub

Sub AcctFLUXDIVV (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	If Not api.Entity.HasChildren() And (api.Cons.IsLocalCurrencyforEntity()) Then
		
		api.Data.ClearCalculatedData("A#FLUXDIVV:F#" & Flow.MVT.name & "",True,True,True)
		api.Data.Calculate("A#FLUXDIVV:F#" & Flow.MVT.name & "=A#DIVV:F#[None]")
		api.Data.ClearCalculatedData("A#FLUXDIVV:F#" & Flow.CLO.name & "",True,True,True)
		api.Data.Calculate("A#FLUXDIVV:F#" & Flow.CLO.name & "=A#FLUXDIVV:F#" & Flow.TF.name & "")
		api.Data.ClearCalculatedData("A#FLUXDIVV:I#None",True,True,True)

	ElseIf Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
		
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim timeprioryearend As Integer = api.Time.GetLastPeriodInPriorYear
		Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability()
		Dim rateTypeAve As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp()
		Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
		Dim closingRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeClo,timeprioryearend)
		Dim AverageRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
		Dim AverageRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeAve,timeprioryearend)
		
		'-----------------------------------------------------------
		'II-4-8- Conversion des dividendes versés - Paid dividends translation
		'-----------------------------------------------------------																		
		Dim sExpression As String = "A#FLUXDIVV:F#" & Flow.MVT.name & "{0}=A#FLUXDIVV:F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRatePriorYE)
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

		api.Data.ClearCalculatedData("A#FLUXDIVV:F#" & Flow.ECF.name & "",True,True,True)
		api.Data.ClearCalculatedData("A#FLUXDIVV:F#" & Flow.ECO.name & "",True,True,True)
	End If
End Sub

Sub AcctFLUXRASD (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	If Not api.Entity.HasChildren() And (api.Cons.IsLocalCurrencyforEntity()) Then
		
		api.Data.ClearCalculatedData("A#FLUXRASD:F#" & Flow.MVT.name & "",True,True,True)
		api.Data.Calculate("A#FLUXRASD:F#" & Flow.MVT.name & "=A#RASD:F#[None]")
		api.Data.ClearCalculatedData("A#FLUXRASD:F#" & Flow.CLO.name & "",True,True,True)
		api.Data.Calculate("A#FLUXRASD:F#" & Flow.CLO.name & "=A#FLUXRASD:F#" & Flow.TF.name & "")
		api.Data.ClearCalculatedData("A#FLUXRASD:I#None",True,True,True)

	ElseIf Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
		
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim timeprioryearend As Integer = api.Time.GetLastPeriodInPriorYear
		Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability()
		Dim rateTypeAve As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp()
		Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
		Dim closingRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeClo,timeprioryearend)
		Dim AverageRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
		Dim AverageRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeAve,timeprioryearend)
		
		'-----------------------------------------------------------
		'II-4-8- Conversion des dividendes versés - Paid dividends translation
		'-----------------------------------------------------------							
		api.Data.ClearCalculatedData("A#FLUXRASD:F#" & Flow.ECF.name & "",True,True,True)
		api.Data.ClearCalculatedData("A#FLUXRASD:F#" & Flow.ECO.name & "",True,True,True)

		Dim sExpression As String = "A#FLUXRASDEH:F#" & Flow.ECF.name & "{0}=A#FLUXRASD:F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & "-A#FLUXRASD:F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate)
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

	End If
End Sub

Sub AcctFLUXDIVR (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	If Not api.Entity.HasChildren() And (api.Cons.IsLocalCurrencyforEntity()) Then
		
		api.Data.ClearCalculatedData("A#FLUXDIVR:F#" & Flow.MVT.name & "",True,True,True)
		api.Data.Calculate("A#FLUXDIVR:F#" & Flow.MVT.name & "=A#761100:F#[None]")
		api.Data.ClearCalculatedData("A#FLUXDIVR:F#" & Flow.CLO.name & "",True,True,True)
		api.Data.Calculate("A#FLUXDIVR:F#" & Flow.CLO.name & "=A#FLUXDIVR:F#" & Flow.TF.name & "")
		api.Data.ClearCalculatedData("A#FLUXDIVR:I#None",True,True,True)

	ElseIf Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
		
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim timeprioryearend As Integer = api.Time.GetLastPeriodInPriorYear
		Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability()
		Dim rateTypeAve As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp()
		Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
		Dim closingRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeClo,timeprioryearend)
		Dim AverageRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
		Dim AverageRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeAve,timeprioryearend)
		
		'-----------------------------------------------------------
		'II-4-8- Conversion des dividendes versés - Paid dividends translation
		'-----------------------------------------------------------							
		api.Data.ClearCalculatedData("A#FLUXDIVR:F#" & Flow.ECF.name & "",True,True,True)
		api.Data.ClearCalculatedData("A#FLUXDIVR:F#" & Flow.ECO.name & "",True,True,True)

		Dim sExpression As String = "A#FLUXDIVREH:F#" & Flow.ECF.name & "{0}=A#FLUXDIVR:F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & "-A#FLUXDIVR:F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate)
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

	End If
End Sub

Sub AcctFLUXADIVV (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	If Not api.Entity.HasChildren() And (api.Cons.IsLocalCurrencyforEntity()) Then
		
		api.Data.ClearCalculatedData("A#FLUXADIVV:F#" & Flow.MVT.name & "",True,True,True)
		api.Data.Calculate("A#FLUXADIVV:F#" & Flow.MVT.name & "=A#ADIVV:F#[None]")
		api.Data.ClearCalculatedData("A#FLUXADIVV:F#" & Flow.CLO.name & "",True,True,True)
		api.Data.Calculate("A#FLUXADIVV:F#" & Flow.CLO.name & "=A#FLUXADIVV:F#" & Flow.TF.name & "")
		api.Data.ClearCalculatedData("A#FLUXADIVV:I#None",True,True,True)

	ElseIf Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
		
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim timeprioryearend As Integer = api.Time.GetLastPeriodInPriorYear
		Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability()
		Dim rateTypeAve As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp()
		Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
		Dim closingRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeClo,timeprioryearend)
		Dim AverageRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
		Dim AverageRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeAve,timeprioryearend)
		
		'-----------------------------------------------------------
		'II-4-8- Conversion des dividendes versés - Paid dividends translation
		'-----------------------------------------------------------
		api.Data.ClearCalculatedData("A#FLUXADIVV:F#" & Flow.ECF.name & "",True,True,True)
		api.Data.ClearCalculatedData("A#FLUXADIVV:F#" & Flow.ECO.name & "",True,True,True)
	End If
End Sub

Sub AcctFLUXRASAD (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	If Not api.Entity.HasChildren() And (api.Cons.IsLocalCurrencyforEntity()) Then
		
		api.Data.ClearCalculatedData("A#FLUXRASAD:F#" & Flow.MVT.name & "",True,True,True)
		api.Data.Calculate("A#FLUXRASAD:F#" & Flow.MVT.name & "=A#RASAD:F#[None]")
		api.Data.ClearCalculatedData("A#FLUXRASAD:F#" & Flow.CLO.name & "",True,True,True)
		api.Data.Calculate("A#FLUXRASAD:F#" & Flow.CLO.name & "=A#FLUXRASAD:F#" & Flow.TF.name & "")
		api.Data.ClearCalculatedData("A#FLUXRASAD:I#None",True,True,True)

	ElseIf Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
		
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim timeprioryearend As Integer = api.Time.GetLastPeriodInPriorYear
		Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability()
		Dim rateTypeAve As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp()
		Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
		Dim closingRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeClo,timeprioryearend)
		Dim AverageRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
		Dim AverageRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeAve,timeprioryearend)
		
		'-----------------------------------------------------------
		'II-4-8- Conversion des dividendes versés - Paid dividends translation
		'-----------------------------------------------------------							
		api.Data.ClearCalculatedData("A#FLUXRASAD:F#" & Flow.ECF.name & "",True,True,True)
		api.Data.ClearCalculatedData("A#FLUXRASAD:F#" & Flow.ECO.name & "",True,True,True)

		Dim sExpression As String = "A#FLUXRASADEH:F#" & Flow.ECF.name & "{0}=A#FLUXRASAD:F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & "-A#FLUXRASAD:F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate)
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

	End If
End Sub

Sub AcctFLUXADIVR (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	If Not api.Entity.HasChildren() And (api.Cons.IsLocalCurrencyforEntity()) Then
		
		api.Data.ClearCalculatedData("A#FLUXADIVR:F#" & Flow.MVT.name & "",True,True,True)
		api.Data.Calculate("A#FLUXADIVR:F#" & Flow.MVT.name & "=A#761200:F#[None]")
		api.Data.ClearCalculatedData("A#FLUXADIVR:F#" & Flow.CLO.name & "",True,True,True)
		api.Data.Calculate("A#FLUXADIVR:F#" & Flow.CLO.name & "=A#FLUXADIVR:F#" & Flow.TF.name & "")
		api.Data.ClearCalculatedData("A#FLUXADIVR:I#None",True,True,True)

	ElseIf Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
		
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim timeprioryearend As Integer = api.Time.GetLastPeriodInPriorYear
		Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability()
		Dim rateTypeAve As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp()
		Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
		Dim closingRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeClo,timeprioryearend)
		Dim AverageRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
		Dim AverageRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeAve,timeprioryearend)
		
		'-----------------------------------------------------------
		'II-4-8- Conversion des dividendes versés - Paid dividends translation
		'-----------------------------------------------------------							
		api.Data.ClearCalculatedData("A#FLUXADIVR:F#" & Flow.ECF.name & "",True,True,True)
		api.Data.ClearCalculatedData("A#FLUXADIVR:F#" & Flow.ECO.name & "",True,True,True)

		Dim sExpression As String = "A#FLUXADIVREH:F#" & Flow.ECF.name & "{0}=A#FLUXADIVR:F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & "-A#FLUXADIVR:F#" & Flow.MVT.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate)
		api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
		api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

	End If
End Sub

Sub Acct201000PMV (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	Dim Flow As Object = Me.InitFlowClass(si, api, globals)
	If Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
		Dim ComptesPVCession As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "CESSIONSIMMO"))
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim timeprioryearend As Integer = api.Time.GetLastPeriodInPriorYear
		Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability()
		Dim rateTypeAve As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp()
		Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
		Dim closingRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeClo,timeprioryearend)
		Dim AverageRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
		Dim AverageRatePriorYE As Decimal = api.FxRates.GetCalculatedFxRate(rateTypeAve,timeprioryearend)
		
		For Each Account As Member In ComptesPVCession
			If Left(Account.Name.ToString,2)<>"CF" Then
				'-----------------------------------------------------------
				'II-4-9- Traitement spécifiques de plus-value de cession - Disposals translation
				'-----------------------------------------------------------
				Dim OUV_TC_TCO As String = "(A#"+Account.Name.ToString+":F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#"+Account.Name.ToString+":F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRatePriorYE) & ")"
				Dim ENT_TC_TCO As String = "(A#"+Account.Name.ToString+":F#" & Flow.ENT.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#"+Account.Name.ToString+":F#ENT)"
				Dim FTA_TC_TCO As String = "(A#"+Account.Name.ToString+":F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#"+Account.Name.ToString+":F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRatePriorYE) & ")"
				
				'Calculate FX on Opening: CLO Local * (Closing Rate - Opening Rate) and same for ENT and FPA
				Dim sExpression As String = "A#"+Account.Name.ToString+"EH:F#" & Flow.ECO.name & "{0}=" & OUV_TC_TCO & "+" & ENT_TC_TCO & "+" & FTA_TC_TCO
				api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
				api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				
				Dim CLO_TC_TM As String = "(A#"+Account.Name.ToString+":F#" & Flow.CLO.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#"+Account.Name.ToString+":F#" & Flow.CLO.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & ")"
				Dim OUV_TM_TC As String = "(A#"+Account.Name.ToString+":F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#"+Account.Name.ToString+":F#" & Flow.OUV.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
				Dim ENT_TM_TC As String = "(A#"+Account.Name.ToString+":F#" & Flow.ENT.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#"+Account.Name.ToString+":F#" & Flow.ENT.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
				Dim FTA_TM_TC As String = "(A#"+Account.Name.ToString+":F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#"+Account.Name.ToString+":F#" & Flow.FPA.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate) & ")"
				
				'Calculate FX on Activity: CLO Local * (Closing Rate - Average Rate) and Reverse the impact from OUV, ENT, FPA, AFF and DIV (which are included in CLO balance)
				sExpression = "A#"+Account.Name.ToString+"EH:F#" & Flow.ECF.name & "{0}=" & CLO_TC_TM & "+" & OUV_TM_TC & "+" & ENT_TM_TC & "+" & FTA_TM_TC
				api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
				api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				
				'les flux AUG et DIM ont un impact Ecart de Change sur le Ecart Change résultat
				'Increase and Decrease have an impact on translation adjustment - Result

				sExpression = "A#"+Account.Name.ToString+"EH:F#" & Flow.ECR.name & "{0}=A#"+Account.Name.ToString+":F#" & Flow.AUG.name & ":C#Local{1} * " & api.Data.DecimalToText(closingRate) & "-A#"+Account.Name.ToString+":F#" & Flow.AUG.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate)
				api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
				api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				
				'il faut dont annuler leur impact déjà pris en compte sur l'écart de change sur flux
				'it is necessary to cancel their impact already entered on translation adjustment - Flow
				sExpression = "A#"+Account.Name.ToString+"EH:F#" & Flow.ECF.name & "{0}=A#"+Account.Name.ToString+"EH:F#" & Flow.ECF.name & "+A#"+Account.Name.ToString+":F#" & Flow.AUG.name & ":C#Local{1} * " & api.Data.DecimalToText(AverageRate) & "-A#"+Account.Name.ToString+":F#" & Flow.AUG.name & ":C#Local{1} * " & api.Data.DecimalToText(ClosingRate)
				api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
				api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

			End If        
		Next
	End If
End Sub
	
	Sub INM2XX (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		'Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		'Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		
		'Affectation du résultat - Appropriation of N-1 income / loss
		If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
		
		api.Data.ClearCalculatedData("A#INM200:F#INC",True,True,True)
		api.Data.ClearCalculatedData("A#INM280:F#INC",True,True,True)
		api.Data.ClearCalculatedData("A#INM210:F#INC",True,True,True)
		api.Data.ClearCalculatedData("A#INM281:F#INC",True,True,True)
		api.Data.ClearCalculatedData("A#INM230:F#INC",True,True,True)
		api.Data.ClearCalculatedData("A#INM283:F#INC",True,True,True)
		
		api.Data.Calculate("A#INM200:F#INC=A#INM200:F#SDC")
		api.Data.Calculate("A#INM280:F#INC=A#INM280:F#SDC")
		api.Data.Calculate("A#INM210:F#INC=A#INM210:F#SDC")
		api.Data.Calculate("A#INM281:F#INC=A#INM281:F#SDC")
		api.Data.Calculate("A#INM230:F#INC=A#INM230:F#SDC")
		api.Data.Calculate("A#INM283:F#INC=A#INM283:F#SDC")
		End If
	
	End Sub
	
	Sub CalcTAM_CD001 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
	api.Data.ClearCalculatedData("A#TAM_CD001",True,True,True)
		
	'Cubo + Parent + Entity + Cons + Scenario + Origin + U5 a U8	
	'Acc + ICP + Flow + U1 a U4 + Time + View 
	'En funcion de la cuenta, U4 será legal o gestion.
	api.Data.Calculate("A#TAM_CD001:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
	A#CD001:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
	A#CD001:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
	A#CD001:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
	A#CD001:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
	A#CD001:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
	A#CD001:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
	A#CD001:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
	A#CD001:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
	A#CD001:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
	A#CD001:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
	A#CD001:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
	A#CD001:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")
	
	End Sub

	Sub CalcTAM_CD002 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
	api.Data.ClearCalculatedData("A#TAM_CD002",True,True,True)
		
	api.Data.Calculate("A#TAM_CD002:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
	A#CD002:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
	A#CD002:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
	A#CD002:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
	A#CD002:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
	A#CD002:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
	A#CD002:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
	A#CD002:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
	A#CD002:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
	A#CD002:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
	A#CD002:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
	A#CD002:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
	A#CD002:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")
	
	End Sub

	Sub CalcTAM_CD003 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CD003",True,True,True)
		
		api.Data.Calculate("A#TAM_CD003:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CD003:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CD003:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CD003:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CD003:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CD003:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CD003:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CD003:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CD003:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CD003:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CD003:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CD003:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CD003:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CD004 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CD004",True,True,True)
		
		api.Data.Calculate("A#TAM_CD004:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CD004:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CD004:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CD004:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CD004:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CD004:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CD004:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CD004:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CD004:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CD004:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CD004:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CD004:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CD004:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CV008 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CV008",True,True,True)
		
		api.Data.Calculate("A#TAM_CV008:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CV008:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CV008:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CV008:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CV008:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CV008:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CV008:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CV008:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CV008:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CV008:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CV008:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CV008:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CV008:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CTRL_477 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_477",True,True,True)
		
		api.Data.Calculate("A#TAM_477:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CTRL_477:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POV:V#Periodic + 
		A#CTRL_477:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior1:V#Periodic +
		A#CTRL_477:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior2:V#Periodic +
		A#CTRL_477:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior3:V#Periodic +
		A#CTRL_477:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior4:V#Periodic +
		A#CTRL_477:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior5:V#Periodic +
		A#CTRL_477:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior6:V#Periodic +
		A#CTRL_477:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior7:V#Periodic +
		A#CTRL_477:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior8:V#Periodic +
		A#CTRL_477:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior9:V#Periodic +
		A#CTRL_477:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior10:V#Periodic +
		A#CTRL_477:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CTRL_472 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_472",True,True,True)
		
		api.Data.Calculate("A#TAM_472:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CTRL_472:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POV:V#Periodic + 
		A#CTRL_472:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior1:V#Periodic +
		A#CTRL_472:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior2:V#Periodic +
		A#CTRL_472:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior3:V#Periodic +
		A#CTRL_472:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior4:V#Periodic +
		A#CTRL_472:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior5:V#Periodic +
		A#CTRL_472:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior6:V#Periodic +
		A#CTRL_472:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior7:V#Periodic +
		A#CTRL_472:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior8:V#Periodic +
		A#CTRL_472:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior9:V#Periodic +
		A#CTRL_472:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior10:V#Periodic +
		A#CTRL_472:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_BSP_C_V_1 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_BSP_C_V_1",True,True,True)
		
		api.Data.Calculate("A#TAM_BSP_C_V_1:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#BSP_C_V_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POV:V#Periodic + 
		A#BSP_C_V_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior1:V#Periodic +
		A#BSP_C_V_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior2:V#Periodic +
		A#BSP_C_V_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior3:V#Periodic +
		A#BSP_C_V_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior4:V#Periodic +
		A#BSP_C_V_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior5:V#Periodic +
		A#BSP_C_V_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior6:V#Periodic +
		A#BSP_C_V_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior7:V#Periodic +
		A#BSP_C_V_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior8:V#Periodic +
		A#BSP_C_V_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior9:V#Periodic +
		A#BSP_C_V_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior10:V#Periodic +
		A#BSP_C_V_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_BSP_C_V_2 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_BSP_C_V_2",True,True,True)
		
		api.Data.Calculate("A#TAM_BSP_C_V_2:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#BSP_C_V_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POV:V#Periodic + 
		A#BSP_C_V_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior1:V#Periodic +
		A#BSP_C_V_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior2:V#Periodic +
		A#BSP_C_V_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior3:V#Periodic +
		A#BSP_C_V_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior4:V#Periodic +
		A#BSP_C_V_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior5:V#Periodic +
		A#BSP_C_V_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior6:V#Periodic +
		A#BSP_C_V_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior7:V#Periodic +
		A#BSP_C_V_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior8:V#Periodic +
		A#BSP_C_V_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior9:V#Periodic +
		A#BSP_C_V_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior10:V#Periodic +
		A#BSP_C_V_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_BSP_C_V_3 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_BSP_C_V_3",True,True,True)
		
		api.Data.Calculate("A#TAM_BSP_C_V_3:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#BSP_C_V_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POV:V#Periodic + 
		A#BSP_C_V_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior1:V#Periodic +
		A#BSP_C_V_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior2:V#Periodic +
		A#BSP_C_V_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior3:V#Periodic +
		A#BSP_C_V_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior4:V#Periodic +
		A#BSP_C_V_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior5:V#Periodic +
		A#BSP_C_V_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior6:V#Periodic +
		A#BSP_C_V_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior7:V#Periodic +
		A#BSP_C_V_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior8:V#Periodic +
		A#BSP_C_V_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior9:V#Periodic +
		A#BSP_C_V_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior10:V#Periodic +
		A#BSP_C_V_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_BSA_B_II_1 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_BSA_B_II_1",True,True,True)
		
		api.Data.Calculate("A#TAM_BSA_B_II_1:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#BSA_B_II_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POV:V#Periodic + 
		A#BSA_B_II_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior1:V#Periodic +
		A#BSA_B_II_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior2:V#Periodic +
		A#BSA_B_II_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior3:V#Periodic +
		A#BSA_B_II_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior4:V#Periodic +
		A#BSA_B_II_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior5:V#Periodic +
		A#BSA_B_II_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior6:V#Periodic +
		A#BSA_B_II_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior7:V#Periodic +
		A#BSA_B_II_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior8:V#Periodic +
		A#BSA_B_II_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior9:V#Periodic +
		A#BSA_B_II_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior10:V#Periodic +
		A#BSA_B_II_1:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_BSA_B_II_2 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_BSA_B_II_2",True,True,True)
		
		api.Data.Calculate("A#TAM_BSA_B_II_2:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#BSA_B_II_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POV:V#Periodic + 
		A#BSA_B_II_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior1:V#Periodic +
		A#BSA_B_II_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior2:V#Periodic +
		A#BSA_B_II_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior3:V#Periodic +
		A#BSA_B_II_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior4:V#Periodic +
		A#BSA_B_II_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior5:V#Periodic +
		A#BSA_B_II_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior6:V#Periodic +
		A#BSA_B_II_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior7:V#Periodic +
		A#BSA_B_II_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior8:V#Periodic +
		A#BSA_B_II_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior9:V#Periodic +
		A#BSA_B_II_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior10:V#Periodic +
		A#BSA_B_II_2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_BSA_B_II_3 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_BSA_B_II_3",True,True,True)
		
		api.Data.Calculate("A#TAM_BSA_B_II_3:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#BSA_B_II_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POV:V#Periodic + 
		A#BSA_B_II_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior1:V#Periodic +
		A#BSA_B_II_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior2:V#Periodic +
		A#BSA_B_II_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior3:V#Periodic +
		A#BSA_B_II_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior4:V#Periodic +
		A#BSA_B_II_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior5:V#Periodic +
		A#BSA_B_II_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior6:V#Periodic +
		A#BSA_B_II_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior7:V#Periodic +
		A#BSA_B_II_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior8:V#Periodic +
		A#BSA_B_II_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior9:V#Periodic +
		A#BSA_B_II_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior10:V#Periodic +
		A#BSA_B_II_3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_BSA_B_II_4 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_BSA_B_II_4",True,True,True)
		
		api.Data.Calculate("A#TAM_BSA_B_II_4:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#BSA_B_II_4:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POV:V#Periodic + 
		A#BSA_B_II_4:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior1:V#Periodic +
		A#BSA_B_II_4:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior2:V#Periodic +
		A#BSA_B_II_4:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior3:V#Periodic +
		A#BSA_B_II_4:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior4:V#Periodic +
		A#BSA_B_II_4:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior5:V#Periodic +
		A#BSA_B_II_4:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior6:V#Periodic +
		A#BSA_B_II_4:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior7:V#Periodic +
		A#BSA_B_II_4:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior8:V#Periodic +
		A#BSA_B_II_4:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior9:V#Periodic +
		A#BSA_B_II_4:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior10:V#Periodic +
		A#BSA_B_II_4:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_BSA_B_II_5 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_BSA_B_II_5",True,True,True)
		
		api.Data.Calculate("A#TAM_BSA_B_II_5:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#BSA_B_II_5:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POV:V#Periodic + 
		A#BSA_B_II_5:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior1:V#Periodic +
		A#BSA_B_II_5:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior2:V#Periodic +
		A#BSA_B_II_5:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior3:V#Periodic +
		A#BSA_B_II_5:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior4:V#Periodic +
		A#BSA_B_II_5:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior5:V#Periodic +
		A#BSA_B_II_5:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior6:V#Periodic +
		A#BSA_B_II_5:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior7:V#Periodic +
		A#BSA_B_II_5:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior8:V#Periodic +
		A#BSA_B_II_5:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior9:V#Periodic +
		A#BSA_B_II_5:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior10:V#Periodic +
		A#BSA_B_II_5:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_BSA_B_II_6 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_BSA_B_II_6",True,True,True)
		
		api.Data.Calculate("A#TAM_BSA_B_II_6:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#BSA_B_II_6:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POV:V#Periodic + 
		A#BSA_B_II_6:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior1:V#Periodic +
		A#BSA_B_II_6:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior2:V#Periodic +
		A#BSA_B_II_6:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior3:V#Periodic +
		A#BSA_B_II_6:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior4:V#Periodic +
		A#BSA_B_II_6:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior5:V#Periodic +
		A#BSA_B_II_6:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior6:V#Periodic +
		A#BSA_B_II_6:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior7:V#Periodic +
		A#BSA_B_II_6:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior8:V#Periodic +
		A#BSA_B_II_6:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior9:V#Periodic +
		A#BSA_B_II_6:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior10:V#Periodic +
		A#BSA_B_II_6:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Legal:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CR_B (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CR_B",True,True,True)
		
		api.Data.Calculate("A#TAM_CR_B:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CR_B:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CR_B:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CR_B:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CR_B:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CR_B:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CR_B:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CR_B:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CR_B:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CR_B:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CR_B:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CR_B:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CR_B:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CR_D2 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CR_D2",True,True,True)
		
		api.Data.Calculate("A#TAM_CR_D2:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CR_D2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CR_D2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CR_D2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CR_D2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CR_D2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CR_D2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CR_D2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CR_D2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CR_D2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CR_D2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CR_D2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CR_D2:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CR_D3 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CR_D3",True,True,True)
		
		api.Data.Calculate("A#TAM_CR_D3:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CR_D3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CR_D3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CR_D3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CR_D3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CR_D3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CR_D3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CR_D3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CR_D3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CR_D3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CR_D3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CR_D3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CR_D3:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CR_E (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CR_E",True,True,True)
		
		api.Data.Calculate("A#TAM_CR_E:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CR_E:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CR_E:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CR_E:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CR_E:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CR_E:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CR_E:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CR_E:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CR_E:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CR_E:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CR_E:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CR_E:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CR_E:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CR_G (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CR_G",True,True,True)
		
		api.Data.Calculate("A#TAM_CR_G:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CR_G:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CR_G:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CR_G:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CR_G:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CR_G:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CR_G:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CR_G:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CR_G:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CR_G:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CR_G:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CR_G:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CR_G:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CR_Q11 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CR_Q11",True,True,True)
		
		api.Data.Calculate("A#TAM_CR_Q11:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CR_Q11:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CR_Q11:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CR_Q11:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CR_Q11:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CR_Q11:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CR_Q11:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CR_Q11:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CR_Q11:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CR_Q11:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CR_Q11:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CR_Q11:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CR_Q11:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CR_Q22 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CR_Q22",True,True,True)
				
		api.Data.Calculate("A#TAM_CR_Q22:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CR_Q22:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CR_Q22:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CR_Q22:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CR_Q22:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CR_Q22:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CR_Q22:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CR_Q22:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CR_Q22:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CR_Q22:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CR_Q22:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CR_Q22:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CR_Q22:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CR_Q23 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CR_Q23",True,True,True)
		
		api.Data.Calculate("A#TAM_CR_Q23:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CR_Q23:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CR_Q23:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CR_Q23:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CR_Q23:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CR_Q23:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CR_Q23:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CR_Q23:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CR_Q23:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CR_Q23:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CR_Q23:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CR_Q23:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CR_Q23:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CG (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CG",True,True,True)
		
		api.Data.Calculate("A#TAM_CG:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CG:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CG:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CG:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CG:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CG:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CG:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CG:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CG:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CG:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CG:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CG:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CG:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CG_13 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CG_13",True,True,True)
		
		api.Data.Calculate("A#TAM_CG_13:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CG_13:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CG_13:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CG_13:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CG_13:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CG_13:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CG_13:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CG_13:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CG_13:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CG_13:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CG_13:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CG_13:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CG_13:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_CG_14 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_CG_14",True,True,True)
		
		api.Data.Calculate("A#TAM_CG_14:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#CG_14:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#CG_14:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#CG_14:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#CG_14:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#CG_14:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#CG_14:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#CG_14:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#CG_14:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#CG_14:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#CG_14:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#CG_14:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#CG_14:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub

	Sub CalcTAM_640200 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_640200",True,True,True)		
			
		api.Data.Calculate("A#TAM_640200:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#640200:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#640200:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#640200:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#640200:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#640200:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#640200:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#640200:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#640200:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#640200:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#640200:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#640200:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#640200:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub
	
	Sub CalcTAM_6692 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		api.Data.ClearCalculatedData("A#TAM_6692",True,True,True)		
			
		api.Data.Calculate("A#TAM_6692:I#None:F#None:U1#None:U2#None:U3#None:U4#None = 
		A#6692:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POV:V#Periodic + 
		A#6692:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior1:V#Periodic +
		A#6692:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior2:V#Periodic +
		A#6692:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior3:V#Periodic +
		A#6692:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior4:V#Periodic +
		A#6692:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior5:V#Periodic +
		A#6692:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior6:V#Periodic +
		A#6692:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior7:V#Periodic +
		A#6692:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior8:V#Periodic +
		A#6692:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior9:V#Periodic +
		A#6692:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior10:V#Periodic +
		A#6692:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:T#POVPrior11:V#Periodic")

	End Sub
	
	Sub Calc_SSGG (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim myWorkflowUnitPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
		Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, myWorkflowUnitPk.TimeKey)
		Dim objTimeMemberSubComponents As TimeMemberSubComponents = BRApi.Finance.Time.GetSubComponentsFromName(si, wfTime)
		Dim wfYear As String = objTimeMemberSubComponents.Year.ToString
		Dim wfMes As String = objTimeMemberSubComponents.Month.ToString
		Dim wfClusterPK As WorkflowUnitClusterPk = si.WorkflowClusterPk
		Dim CurYear As Integer = api.Time.GetYearFromId(timeId)	
		Dim POVEntity As String = api.pov.entity.name
		Dim TimeBase As String = wfYear & "M12"
		Dim ScenarioBase As String
		Dim Account_INV As String
		Dim periodNumber As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Month
		
		If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then			
		
					If api.Pov.Scenario.Name = "R" Then	
						If CurYear > 2020 Then
					
							If (periodNumber = "1" Or periodNumber = "2" Or periodNumber = "3" Or periodNumber = "4" Or periodNumber = "5") Then
								ScenarioBase = "O1"
								Account_INV = "Pres_Aprob_Año_N"
							Else If (periodNumber = "6" Or periodNumber = "7" Or periodNumber = "8" Or periodNumber = "9" Or periodNumber = "10" Or periodNumber = "11") Then
								ScenarioBase = "P06"
								Account_INV = "Pres_Aprob_Año_N"
							Else
								ScenarioBase = "R"
								Account_INV = "Prev_Inv"
							End If
							Me.SSGG_Reparto(si, api, globals, args, ScenarioBase, Account_INV)
						End If
					
					Else If api.Pov.Scenario.Name = "P03" Then
						If CurYear > 2020 Then
							If api.Pov.Time.Name = CurYear & "M12" Then 
								ScenarioBase = "O1"
								Account_INV = "Pres_Aprob_Año_N"
								Me.SSGG_Reparto(si, api, globals, args, ScenarioBase, Account_INV)
							End If
						End If
					Else If (api.Pov.Scenario.Name = "P06" Or api.Pov.Scenario.Name = "P09" Or api.Pov.Scenario.Name = "P11") Then
						If CurYear > 2020 Then
							If api.Pov.Time.Name = CurYear & "M12" Then 
								ScenarioBase = "P06"
								Account_INV = "Pres_Aprob_Año_N"
								Me.SSGG_Reparto(si, api, globals, args, ScenarioBase, Account_INV)
							End If
						End If
					Else If (api.Pov.Scenario.Name = "O1" Or api.Pov.Scenario.Name = "O2" Or api.Pov.Scenario.Name = "O3") Then
						If CurYear > 2021 Then
							If api.Pov.Time.Name = CurYear & "M12" Then 
								ScenarioBase = api.Pov.Scenario.Name
								Account_INV = "Pres_Aprob_Año_N"
								Me.SSGG_Reparto(si, api, globals, args, ScenarioBase, Account_INV)
							End If
						End If
					End If
		End If
			
	End Sub
	
	Sub SSGG_Reparto (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, ScenarioBase As String, Account_INV As String)
		
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim myWorkflowUnitPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
		Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, myWorkflowUnitPk.TimeKey)
		Dim objTimeMemberSubComponents As TimeMemberSubComponents = BRApi.Finance.Time.GetSubComponentsFromName(si, wfTime)
		Dim wfYear As String = objTimeMemberSubComponents.Year.ToString
		Dim wfMes As String = objTimeMemberSubComponents.Month.ToString
		Dim wfClusterPK As WorkflowUnitClusterPk = si.WorkflowClusterPk
		Dim CurYear As Integer = api.Time.GetYearFromId(timeId)	
		Dim POVEntity As String = api.pov.entity.name
		Dim TimeBase As String = wfYear & "M12"
		
		Dim DatoHolding = api.Data.GetDataCell("A#N_UP:S#" & ScenarioBase & ":T#" & TimeBase & ":C#Local:O#Forms:I#None:V#YTD:F#None:U1#HD01:U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
				
		If DatoHolding > 0 Then
			'Copiamos A#P_UP
			api.Data.Calculate("A#P_UP = A#P_UP:E#E999:S#" & ScenarioBase & ":T#" & TimeBase & "") 
			'Copiamos A#P_Ventas_Inversion			
			api.Data.Calculate("A#P_Ventas_Inversion = A#P_Ventas_Inversion:E#E999:S#" & ScenarioBase & ":T#" & TimeBase & "")												
			'Copiamos A#P_RH			
			api.Data.Calculate("A#P_RH = A#P_RH:E#E999:S#" & ScenarioBase & ":T#" & TimeBase & "")
					
			Dim ListaUd1 As List(Of member) = api.Members.GetBaseMembers(api.Pov.UD1Dim.DimPk,api.Members.GetMemberId(dimtype.UD1.Id,"DN"))
			For Each UD1M As member In ListaUd1
			Dim DatoUP = api.Data.GetDataCell("A#N_UP:S#" & ScenarioBase & ":T#" & TimeBase & ":C#Local:O#Forms:I#None:V#YTD:F#None:U1#" & UD1M.Name.ToString & ":U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
				If DatoUP > 0 Then
									
					api.Data.ClearCalculatedData("Cb#CONSO:A#Total_Repartir:U1#" & UD1M.Name.ToString & "",True,True,True)	
									
					Dim Ptg_UP = api.Data.GetDataCell("E#E999:A#P_UP:S#" & ScenarioBase & ":T#" & TimeBase & ":F#None:C#Local:O#Forms:I#None:U1#HD01:U2#HD01:U3#None:U4#None:U5#None / 100").CellAmount.ToString(cultureinfo.InvariantCulture)
					Dim Ptg_VI = api.Data.GetDataCell("E#E999:A#P_Ventas_Inversion:S#" & ScenarioBase & ":T#" & TimeBase & ":F#None:C#Local:O#Forms:I#None:U1#HD01:U2#HD01:U3#None:U4#None:U5#None / 100").CellAmount.ToString(cultureinfo.InvariantCulture)
					Dim Ptg_RH = api.Data.GetDataCell("E#E999:A#P_RH:S#" & ScenarioBase & ":T#" & TimeBase & ":F#None:C#Local:O#Forms:I#None:U1#HD01:U2#HD01:U3#None:U4#None:U5#None / 100").CellAmount.ToString(cultureinfo.InvariantCulture)
								
					Dim sCalculo As New Text.StringBuilder()
					sCalculo.Append("A#Total_Repartir:F#None:O#Forms:I#None:U1#" & UD1M.Name.ToString & ":U2#HD01:U3#None:U4#None:U5#None = (")
					'Calculo UP
					sCalculo.Append("(A#N_UP:S#" & ScenarioBase & ":T#" & TimeBase & ":F#None:O#Forms:I#None:U1#" & UD1M.Name.ToString & ":U2#HD01:U3#None:U4#None:U5#None / (A#N_UP:S#" & ScenarioBase & ":T#" & TimeBase & ":F#None:O#forms:I#None:U1#DN:U2#HD01:U3#None:U4#None:U5#None - A#N_UP:S#" & ScenarioBase & ":T#" & TimeBase & ":F#None:O#forms:I#None:U1#HD01:U2#HD01:U3#None:U4#None:U5#None) * " & Ptg_UP & ") +")
					'Calculo Ventas + Inversion
					sCalculo.Append("(((A#CR_A:S#" & ScenarioBase & ":T#" & TimeBase & ":F#Top:O#Top:I#Top:U1#" & UD1M.Name.ToString & ":U2#Top:U3#Top:U4#Gestion:U5#None + Cb#CONTR:A#" & Account_INV & ":S#" & ScenarioBase & ":T#" & TimeBase & ":F#Top:O#Top:I#Top:U1#" & UD1M.Name.ToString & ":U2#Top:U3#[Total Clientes]:U4#None:U5#None)*")
					sCalculo.Append("1/((A#CR_A:S#" & ScenarioBase & ":T#" & TimeBase & ":F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None - A#CR_A:S#" & ScenarioBase & ":T#" & TimeBase & ":F#Top:O#Top:I#Top:U1#HD01:U2#Top:U3#Top:U4#Gestion:U5#None - A#CR_A:S#" & ScenarioBase & ":T#" & TimeBase & ":F#Top:O#Top:I#Top:U1#9909:U2#Top:U3#Top:U4#Gestion:U5#None) + (Cb#CONTR:A#" & Account_INV & ":S#" & ScenarioBase & ":T#" & TimeBase & ":F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None - Cb#CONTR:A#" & Account_INV & ":S#" & ScenarioBase & ":T#" & TimeBase & ":F#Top:O#Top:I#Top:U1#HD01:U2#Top:U3#[Total Clientes]:U4#None:U5#None)))* " & Ptg_VI & ") +")
					'Calculo RH
					sCalculo.Append("(A#RH024:S#" & ScenarioBase & ":T#" & TimeBase & ":F#Top:O#Top:I#Top:U1#" & UD1M.Name.ToString & ":U2#Top:U3#Top:U4#Gestion:U5#None / (A#RH024:S#" & ScenarioBase & ":T#" & TimeBase & ":F#Top:O#Top:I#Top:U1#DN:U2#Top:U3#Top:U4#Gestion:U5#None - A#RH024:S#" & ScenarioBase & ":T#" & TimeBase & ":F#Top:O#Top:I#Top:U1#HD01:U2#Top:U3#Top:U4#Gestion:U5#None) * " & Ptg_RH.ToString & ") ) ")
					api.Data.Calculate(sCalculo.ToString())

					api.Data.ClearCalculatedData("A#Total_Repartir:U1#HD01",True,True,True)
									
				End If
							
			Next
							
			'Borrado Repartir_SSGG
			api.Data.ClearCalculatedData("A#CR_W1:U4#Repartir_SSGG",True,True,True)
			api.Data.ClearCalculatedData("A#CR_W2J9:U4#Repartir_SSGG",True,True,True)
			api.Data.ClearCalculatedData("A#623200:U4#Repartir_SSGG",True,True,True)
						
			For Each UD1M As member In ListaUd1 
				If api.Data.GetDataCell("A#Total_Repartir:F#None:O#Forms:I#None:U1#" & UD1M.Name.ToString & ":U2#HD01").CellAmount <> 0 Then
							
					'Calculo Peso
					api.Data.ClearCalculatedData("A#Peso_Final:U1#" & UD1M.Name.ToString & "",True,True,True)
					api.Data.Calculate("A#Peso_Final:F#None:O#Forms:I#None:U1#" & UD1M.Name.ToString & ":U2#HD01:U3#None:U4#None:U5#None = A#Total_Repartir:F#None:O#Forms:I#None:U1#" & UD1M.Name.ToString & ":U2#HD01:U3#None:U4#None:U5#None / A#Total_Repartir:F#None:O#Forms:I#None:U1#DN:U2#HD01:U3#None:U4#None:U5#None")
								
					Dim PesoFinal=api.Data.GetDataCell("A#Peso_Final:F#None:O#Forms:I#None:U1#" & UD1M.Name.ToString & ":U2#HD01:U3#None:U4#None:U5#None").CellAmount.ToString(cultureinfo.InvariantCulture)
								
					'Reparto CR_V a CR_W2J9
		
					'NEW Reparto
					Dim sCalculo2 As New Text.StringBuilder()
					sCalculo2.Append("A#CR_W2J9:F#None:I#" & POVEntity & ":U1#" & UD1M.Name.ToString & ":U2#HD01:U3#All:U4#Repartir_SSGG:U5#All =")
					'Calculo CR_V
					sCalculo2.Append("A#CR_V:F#Top:I#Top:U1#HD01:U2#Top:U3#All:U4#Gestion:U5#All * " & PesoFinal & " * (-1)")
					'Calculo 623200 con ICP E502
					sCalculo2.Append("+(A#623200:F#Top:I#E502:U1#HD01:U2#Top:U3#All:U4#Gestion:U5#All * " & PesoFinal & ")")
					api.Data.Calculate(sCalculo2.ToString())
								
					'NEW Neteo
					api.Data.Calculate("A#CR_W2J9:F#None:I#" & POVEntity & ":U1#9909:U2#9909:U4#Repartir_SSGG = A#CR_V:F#Top:I#Top:U1#HD01:U2#Top:U4#Gestion - A#623200:F#Top:I#E502:U1#HD01:U2#Top:U4#Gestion")
								
					'NEW Ingreso CR_W1
					api.Data.Calculate("A#CR_W1:F#None:I#" & POVEntity & ":U1#HD01:U2#" & UD1M.Name.ToString & ":U3#All:U4#Repartir_SSGG:U5#All = A#CR_W1:F#None:I#" & POVEntity & ":U1#HD01:U2#" & UD1M.Name.ToString & ":U3#All:U4#Repartir_SSGG:U5#All + A#CR_W2J9:F#None:I#" & POVEntity & ":U1#" & UD1M.Name.ToString & ":U2#HD01:U3#All:U4#Repartir_SSGG:U5#All")
							
					'Reparto 623200 IC E101
									
					'NEW IC E101
					api.Data.Calculate("A#623200:F#None:I#" & POVEntity & ":U1#" & UD1M.Name.ToString & ":U2#HD01:U3#All:U4#Repartir_SSGG:U5#All = A#623200:F#Top:I#E101:U1#HD01:U2#Top:U3#All:U4#Gestion:U5#All * " & PesoFinal & "")
									
					'NEW Neteo
					api.Data.Calculate("A#623200:F#None:I#" & POVEntity & ":U1#9909:U2#9909:U4#Repartir_SSGG = -A#623200:F#Top:I#E101:U1#HD01:U2#Top:U4#Gestion")
								
					'NEW Ingreso CR_W1
					api.Data.Calculate("A#CR_W1:F#None:I#" & POVEntity & ":U1#HD01:U2#" & UD1M.Name.ToString & ":U3#All:U4#Repartir_SSGG:U5#All = A#CR_W1:F#None:I#" & POVEntity & ":U1#HD01:U2#" & UD1M.Name.ToString & ":U3#All:U4#Repartir_SSGG:U5#All + A#623200:F#None:I#" & POVEntity & ":U1#" & UD1M.Name.ToString & ":U2#HD01:U3#All:U4#Repartir_SSGG:U5#All")
									
								
					'Vaciado de CR_W1
					api.Data.Calculate("A#CR_W1:F#None:I#" & POVEntity & ":U1#9909:U2#9909:U4#Repartir_SSGG = A#CR_V:F#None:I#Top:U1#HD01:U2#Top:U4#Gestion - A#623200:F#None:I#E101:U1#HD01:U2#Top:U4#Gestion - A#623200:F#None:I#E502:U1#HD01:U2#Top:U4#Gestion")
				End If	
			Next
		End If
	End Sub


	
	
Sub Calc_Mensualizacion_CORP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

	Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim CurYear As Integer = api.Time.GetYearFromId(timeId)		
		Dim ObjetivoM12 As String = (CurYear & "M12")
		Dim PriorYear As Integer = CurYear -1
				
		'Definimos el multiplicador de meses
		Dim periodMultiplier As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Month
		'Adaptamos el multiplicador para P03, aplica desde Abril
		Dim P03Multiplier As Integer = (periodMultiplier - 3)
		'Adaptamos el multiplicador para P06, aplica desde Julio
		Dim P06Multiplier As Integer = (periodMultiplier - 6)
		'Adaptamos el multiplicador para P09, aplica desde Octubre
		Dim P09Multiplier As Integer = (periodMultiplier - 9)
		
			
		'If api.Entity.HasChildren() Then
			'Or (Not(api.Entity.HasChildren())And (api.Pov.Cons.Name = "OwnerPreAdj" Or api.Pov.Cons.Name = "OwnerPostAdj")) Then
			 
			If (api.Pov.Scenario.Name = "O1" Or api.Pov.Scenario.Name = "O2" Or api.Pov.Scenario.Name = "O3" Or api.Pov.Scenario.Name = "P03" Or api.Pov.Scenario.Name = "P06" Or api.Pov.Scenario.Name = "P09" Or api.Pov.Scenario.Name = "P11") Then
										
				Dim DatoRealUltimo = api.Data.GetDataCell("A#CR_Z:S#R:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
				Dim DatoP11Ultimo = api.Data.GetDataCell("A#CR_Z:S#P11:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
				Dim DatoP09Ultimo = api.Data.GetDataCell("A#CR_Z:S#P09:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
				Dim DatoP06Ultimo = api.Data.GetDataCell("A#CR_Z:S#P06:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
				
	
				Dim ScenarioBase As String
				
					If DatoRealUltimo <> 0 Then
						ScenarioBase = "R"
						
					Else If DatoP11Ultimo <> 0 Then
						ScenarioBase = "P11"
						
					Else If DatoP09Ultimo <> 0 Then
						ScenarioBase = "P09"
						
					Else If DatoP06Ultimo <> 0 Then
						ScenarioBase = "P06"
						
					Else 
						ScenarioBase = "P03"
						
					End If
										
''xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx				
		'OBJETIVO: O1, O2 or O3 CDR, ANX, INVER_DESINVER, INMO_GESTION, RH, CV y CD

				If Not api.Pov.Time.Name = CurYear.ToString() & "M12" Then		
																																								
					'CDR					
					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#CDR.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
					'ANX
					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#ANX.Base",, "O#AdjInput",,,,,,,,,,,, True)

					'129000
					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#129000",, "O#AdjInput",,,,,,,,,,,, True)
					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#129000",, "O#Forms",,,,,,,,,,,, True)
					
					'77E
					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#77E",, "O#Forms",,,,,,,,,,,, True)

					'INVER_DESINVER
					api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#INVER_DESINV.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
					'INMO_GESTION
					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#INMO_GESTION.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
					'RH - FOTO
					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None + ((( T#" & ObjetivoM12 & ":UD4#None -  S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None) / 12) * " & periodMultiplier & ")", "A#RH.Base.Where(Text2 Contains Foto)",, "O#AdjInput",,,,,,,,,,,, True)
															
					'RH - NO FOTO
					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  UD4#None:T#" & ObjetivoM12 & " /12 * " & periodMultiplier & "", "A#RH.Base.Where(Text2 DoesNotContain Foto).Remove(RH021)",, "O#AdjInput",,,,,,,,,,,, True)
					
					'CV
					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  UD4#Gestion:T#" & ObjetivoM12 & " /12 * " & periodMultiplier & "", "A#CV.Base.Where(Text1 Contains Mensualizacion)",, "O#AdjInput",,,,,,,,,,,, True)
					
						'CV expepcion CV_CF
						api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "", "A#CV_Ventas","F#APV","O#AdjInput",,,,,,,,,,,, True)
						api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "", "A#CV_FACT","F#APV","O#AdjInput",,,,,,,,,,,, True)
						
						'CV expepcion CVXXX
						api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "", "A#CV001",,"O#AdjInput",,,,,,,,,,,, True)
						api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "", "A#CV006",,"O#AdjInput",,,,,,,,,,,, True)
										
					'CD003B
					'api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None + ((( T#" & ObjetivoM12 & ":UD4#None -  S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None) / 12) * " & periodMultiplier & ")", "A#CD003B.Base",, "O#AdjInput",,,,,,,,,,,, True)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Legal + (((T#" & ObjetivoM12 & ":UD4#Gestion - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Legal) / 12) * " & periodMultiplier & ")", "A#CD003B.Base",,"O#AdjInput",,,,,,,,,,,, True)
					
					'CD004B
					'api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None + ((( T#" & ObjetivoM12 & ":UD4#None -  S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None) / 12) * " & periodMultiplier & ")", "A#CD004B.Base",, "O#AdjInput",,,,,,,,,,,, True)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Legal + (((T#" & ObjetivoM12 & ":UD4#Gestion - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Legal) / 12) * " & periodMultiplier & ")", "A#CD004B.Base",,"O#AdjInput",,,,,,,,,,,, True)
					
					'CD005B
					'api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None + ((( T#" & ObjetivoM12 & ":UD4#None -  S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None) / 12) * " & periodMultiplier & ")", "A#CD005B.Base",, "O#AdjInput",,,,,,,,,,,, True)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Legal + (((T#" & ObjetivoM12 & ":UD4#Gestion - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Legal) / 12) * " & periodMultiplier & ")", "A#CD005B.Base",,"O#AdjInput",,,,,,,,,,,, True)
					
					'CD
					'api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None + ((( T#" & ObjetivoM12 & ":UD4#None -  S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None) / 12) * " & periodMultiplier & ")", "A#CD.Base",, "O#AdjInput",,,,,,,,,,,, True)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Gestion + (((T#" & ObjetivoM12 & ":UD4#Gestion - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Gestion) / 12) * " & periodMultiplier & ")", "A#CD.Base",,"O#AdjInput",,,,,,,,,,,, True)
					
																																																							
				End If																					
										

'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx	
		'PREVISTO P03 
		
'			Else If api.Pov.Scenario.Name = "P03" Then
				
												
'				If api.Pov.Time.Name = CurYear.ToString() & "M3" Then
					
'					'CDR
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#CDR.Base",, "O#AdjInput",,,,,,,,,,,, True)					
					
'					'ANX
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#ANX.Base",, "O#AdjInput",,,,,,,,,,,, True)												
					
'					'129000
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#129000",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'INVE_DESINVE
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#INVER_DESINV.Base",, "O#AdjInput",,,,,,,,,,,, True)						
										
'					'INMO_GESTION
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#INMO_GESTION.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'RH
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#RH.Base.Remove(RH021)",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CV
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#CV.Base.Where(Text1 Contains Mensualizacion)",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD003B
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#CD003B.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD004B
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#CD004B.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD005B
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#CD005B.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#CD.Base",, "O#AdjInput",,,,,,,,,,,, True)						
				
	
'				'De Abril a Noviembre
'				Else If periodMultiplier > 3 And periodMultiplier < 12 Then
					
'					'CDR
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion = (( S#P03:T#" & CurYear.ToString & "M12:UD4#Gestion -  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion) / 9 * " & P03Multiplier & ") +  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#CDR.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'ANX
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion = (( S#P03:T#" & CurYear.ToString & "M12:UD4#Gestion -  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion) / 9 * " & P03Multiplier & ") +  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#ANX.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'129000
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion = (( S#P03:T#" & CurYear.ToString & "M12:UD4#Gestion -  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion) / 9 * " & P03Multiplier & ") +  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#129000",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'INVE_DESINVE
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion = (( S#P03:T#" & CurYear.ToString & "M12:UD4#Gestion -  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion) / 9 * " & P03Multiplier & ") +  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#INVER_DESINV.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'INMO_GESTION
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion = (( S#P03:T#" & CurYear.ToString & "M12:UD4#Gestion -  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion) / 9 * " & P03Multiplier & ") +  S#R:T#" & CurYear.ToString & "M3:UD4#Gestion", "A#INMO_GESTION.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'RH
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#P03:T#" & CurYear.ToString & "M3:UD4#Repartir_Mensualizacion + (( S#P03:T#" & CurYear.ToString & "M12:U4#Gestion -  S#R:T#" & CurYear.ToString & "M3:U4#Gestion) / 9 * " & P03Multiplier & ")", "A#RH.Base.Remove(RH021)",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CV
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#P03:T#" & CurYear.ToString & "M3:UD4#Repartir_Mensualizacion + (( S#P03:T#" & CurYear.ToString & "M12:U4#Gestion -  S#R:T#" & CurYear.ToString & "M3:U4#Gestion) / 9 * " & P03Multiplier & ")", "A#CV.Base.Where(Text1 Contains Mensualizacion)",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CD003B
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#P03:T#" & CurYear.ToString & "M3:UD4#Repartir_Mensualizacion + (( S#P03:T#" & CurYear.ToString & "M12:U4#Gestion -  S#R:T#" & CurYear.ToString & "M3:U4#Gestion) / 9 * " & P03Multiplier & ")", "A#CD003B.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CD004B
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#P03:T#" & CurYear.ToString & "M3:UD4#Repartir_Mensualizacion + (( S#P03:T#" & CurYear.ToString & "M12:U4#Gestion -  S#R:T#" & CurYear.ToString & "M3:U4#Gestion) / 9 * " & P03Multiplier & ")", "A#CD004B.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CD005B
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#P03:T#" & CurYear.ToString & "M3:UD4#Repartir_Mensualizacion + (( S#P03:T#" & CurYear.ToString & "M12:U4#Gestion -  S#R:T#" & CurYear.ToString & "M3:U4#Gestion) / 9 * " & P03Multiplier & ")", "A#CD005B.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CD
'					api.Data.Calculate(" UD4#Repartir_Mensualizacion =  S#P03:T#" & CurYear.ToString & "M3:UD4#Repartir_Mensualizacion + (( S#P03:T#" & CurYear.ToString & "M12:U4#Gestion -  S#R:T#" & CurYear.ToString & "M3:U4#Gestion) / 9 * " & P03Multiplier & ")", "A#CD.Base",, "O#AdjInput",,,,,,,,,,,, True)
																												
'				End If	
																					
''xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx	
'		'PREVISTO P06

'			Else If api.Pov.Scenario.Name = "P06" Then
				
'				If api.Pov.Time.Name = CurYear.ToString() & "M6" Then
					
'					'api.Data.ClearCalculatedData("S#P06:UD4#Repartir_Mensualizacion",True,True,True,True)
					
'					'CDR
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#CDR.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'ANX
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#ANX.Base",, "O#AdjInput",,,,,,,,,,,, True)												
					
'					'129000
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#129000",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'INVE_DESINVE
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#INVER_DESINV.Base",, "O#AdjInput",,,,,,,,,,,, True)						
										
'					'INMO_GESTION
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#INMO_GESTION.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'RH
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#RH.Base.Remove(RH021)",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CV
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#CV.Base.Where(Text1 Contains Mensualizacion)",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD003B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#CD003B.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD004B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#CD004B.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD005B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#CD005B.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#CD.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
														
'				'De julio a Noviembre
'				Else If periodMultiplier > 6 And periodMultiplier < 12 Then
					
'					'api.Data.ClearCalculatedData("S#P06:UD4#Repartir_Mensualizacion",True,True,True,True)
					
'					'CDR
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = ((S#P06:T#" & CurYear.ToString & "M12:UD4#Gestion - S#R:T#" & CurYear.ToString & "M6:UD4#Gestion) / 6 * " & P06Multiplier & ") + S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#CDR.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'ANX
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = ((S#P06:T#" & CurYear.ToString & "M12:UD4#Gestion - S#R:T#" & CurYear.ToString & "M6:UD4#Gestion) / 6 * " & P06Multiplier & ") + S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#ANX.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'129000
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = ((S#P06:T#" & CurYear.ToString & "M12:UD4#Gestion - S#R:T#" & CurYear.ToString & "M6:UD4#Gestion) / 6 * " & P06Multiplier & ") + S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#129000",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'INVE_DESINVE
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = ((S#P06:T#" & CurYear.ToString & "M12:UD4#Gestion - S#R:T#" & CurYear.ToString & "M6:UD4#Gestion) / 6 * " & P06Multiplier & ") + S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#INVER_DESINV.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'INMO_GESTION
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = ((S#P06:T#" & CurYear.ToString & "M12:UD4#Gestion - S#R:T#" & CurYear.ToString & "M6:UD4#Gestion) / 6 * " & P06Multiplier & ") + S#R:T#" & CurYear.ToString & "M6:UD4#Gestion", "A#INMO_GESTION.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'RH
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#P06:T#" & CurYear.ToString & "M6:UD4#Repartir_Mensualizacion + ((S#P06:T#" & CurYear.ToString & "M12:U4#Gestion - S#R:T#" & CurYear.ToString & "M6:U4#Gestion) / 6 * " & P06Multiplier & ")", "A#RH.Base.Remove(RH021)",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CV
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#P06:T#" & CurYear.ToString & "M6:UD4#Repartir_Mensualizacion + ((S#P06:T#" & CurYear.ToString & "M12:U4#Gestion - S#R:T#" & CurYear.ToString & "M6:U4#Gestion) / 6 * " & P06Multiplier & ")", "A#CV.Base.Where(Text1 Contains Mensualizacion)",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CD003B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#P06:T#" & CurYear.ToString & "M6:UD4#Repartir_Mensualizacion + ((S#P06:T#" & CurYear.ToString & "M12:U4#Gestion - S#R:T#" & CurYear.ToString & "M6:U4#Gestion) / 6 * " & P06Multiplier & ")", "A#CD003B.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CD004B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#P06:T#" & CurYear.ToString & "M6:UD4#Repartir_Mensualizacion + ((S#P06:T#" & CurYear.ToString & "M12:U4#Gestion - S#R:T#" & CurYear.ToString & "M6:U4#Gestion) / 6 * " & P06Multiplier & ")", "A#CD004B.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CD005B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#P06:T#" & CurYear.ToString & "M6:UD4#Repartir_Mensualizacion + ((S#P06:T#" & CurYear.ToString & "M12:U4#Gestion - S#R:T#" & CurYear.ToString & "M6:U4#Gestion) / 6 * " & P06Multiplier & ")", "A#CD005B.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CD
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#P06:T#" & CurYear.ToString & "M6:UD4#Repartir_Mensualizacion + ((S#P06:T#" & CurYear.ToString & "M12:U4#Gestion - S#R:T#" & CurYear.ToString & "M6:U4#Gestion) / 6 * " & P06Multiplier & ")", "A#CD.Base",, "O#AdjInput",,,,,,,,,,,, True)
				
				
'				End If
				

'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx	
		'PREVISTO P09
			
'			Else If api.Pov.Scenario.Name = "P09" Then
				
'				If api.Pov.Time.Name = CurYear.ToString() & "M9" Then
					
'					'CDR
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#CDR.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'ANX
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#ANX.Base",, "O#AdjInput",,,,,,,,,,,, True)												
					
'					'129000
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#129000",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'INVE_DESINVE
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#INVER_DESINV.Base",, "O#AdjInput",,,,,,,,,,,, True)						
										
'					'INMO_GESTION
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#INMO_GESTION.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'RH
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#RH.Base.Remove(RH021)",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CV
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#CV.Base.Where(Text1 Contains Mensualizacion)",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD003B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#CD003B.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD004B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#CD004B.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD005B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#CD005B.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#CD.Base",, "O#AdjInput",,,,,,,,,,,, True)						

					
'				'De Octubre a Noviembre	
'				Else If periodMultiplier > 9 And periodMultiplier < 12 Then
					
'					'CDR
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = ((S#P09:T#" & CurYear.ToString & "M12:UD4#Gestion - S#R:T#" & CurYear.ToString & "M9:UD4#Gestion) / 3 * " & P09Multiplier & ") + S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#CDR.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'ANX
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = ((S#P09:T#" & CurYear.ToString & "M12:UD4#Gestion - S#R:T#" & CurYear.ToString & "M9:UD4#Gestion) / 3 * " & P09Multiplier & ") + S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#ANX.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'129000
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = ((S#P09:T#" & CurYear.ToString & "M12:UD4#Gestion - S#R:T#" & CurYear.ToString & "M9:UD4#Gestion) / 3 * " & P09Multiplier & ") + S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#129000",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'INVE_DESINVE
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = ((S#P09:T#" & CurYear.ToString & "M12:UD4#Gestion - S#R:T#" & CurYear.ToString & "M9:UD4#Gestion) / 3 * " & P09Multiplier & ") + S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#INVER_DESINV.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'INMO_GESTION
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = ((S#P09:T#" & CurYear.ToString & "M12:UD4#Gestion - S#R:T#" & CurYear.ToString & "M9:UD4#Gestion) / 3 * " & P09Multiplier & ") + S#R:T#" & CurYear.ToString & "M9:UD4#Gestion", "A#INMO_GESTION.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'RH
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#P09:T#" & CurYear.ToString & "M9:UD4#Repartir_Mensualizacion + ((S#P09:T#" & CurYear.ToString & "M12:U4#Gestion - S#R:T#" & CurYear.ToString & "M9:U4#Gestion) / 3 * " & P09Multiplier & ")", "A#RH.Base.Remove(RH021)",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CV
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#P09:T#" & CurYear.ToString & "M9:UD4#Repartir_Mensualizacion + ((S#P09:T#" & CurYear.ToString & "M12:U4#Gestion - S#R:T#" & CurYear.ToString & "M9:U4#Gestion) / 3 * " & P09Multiplier & ")", "A#CV.Base.Where(Text1 Contains Mensualizacion)",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CD003B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#P09:T#" & CurYear.ToString & "M9:UD4#Repartir_Mensualizacion + ((S#P09:T#" & CurYear.ToString & "M12:U4#Gestion - S#R:T#" & CurYear.ToString & "M9:U4#Gestion) / 3 * " & P09Multiplier & ")", "A#CD003B.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CD004B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#P09:T#" & CurYear.ToString & "M9:UD4#Repartir_Mensualizacion + ((S#P09:T#" & CurYear.ToString & "M12:U4#Gestion - S#R:T#" & CurYear.ToString & "M9:U4#Gestion) / 3 * " & P09Multiplier & ")", "A#CD004B.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CD005B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#P09:T#" & CurYear.ToString & "M9:UD4#Repartir_Mensualizacion + ((S#P09:T#" & CurYear.ToString & "M12:U4#Gestion - S#R:T#" & CurYear.ToString & "M9:U4#Gestion) / 3 * " & P09Multiplier & ")", "A#CD005B.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'CD
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#P09:T#" & CurYear.ToString & "M9:UD4#Repartir_Mensualizacion + ((S#P09:T#" & CurYear.ToString & "M12:U4#Gestion - S#R:T#" & CurYear.ToString & "M9:U4#Gestion) / 3 * " & P09Multiplier & ")", "A#CD.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
					
'				End If
		
		
		
'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx	
		'PREVISTO P11
		
'			Else If api.Pov.Scenario.Name = "P11" Then
'				'Seleccionamos Octubre de base
'				If api.Pov.Time.Name = CurYear.ToString() & "M11" Then
					
'					'CDR
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M10:UD4#Gestion", "A#CDR.Base",, "O#AdjInput",,,,,,,,,,,, True)
					
'					'ANX
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M10:UD4#Gestion", "A#ANX.Base",, "O#AdjInput",,,,,,,,,,,, True)												
					
'					'129000
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M10:UD4#Gestion", "A#129000",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'INVE_DESINVE
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M10:UD4#Gestion", "A#INVER_DESINV.Base",, "O#AdjInput",,,,,,,,,,,, True)						
										
'					'INMO_GESTION
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M10:UD4#Gestion", "A#INMO_GESTION.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'RH
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M10:UD4#Gestion", "A#RH.Base.Remove(RH021)",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CV
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M10:UD4#Gestion", "A#CV.Base.Where(Text1 Contains Mensualizacion)",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD003B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M10:UD4#Gestion", "A#CD003B.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD004B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M10:UD4#Gestion", "A#CD004B.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD005B
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M10:UD4#Gestion", "A#CD005B.Base",, "O#AdjInput",,,,,,,,,,,, True)						
					
'					'CD
'					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#R:T#" & CurYear.ToString & "M10:UD4#Gestion", "A#CD.Base",, "O#AdjInput",,,,,,,,,,,, True)						

										
'			End If
		End If
	'End If

End Sub

	
	
#End Region

#Region "Member Formulas for CDR Accounts"

'	Sub AgrupAmortx (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
'		'Dim Flow As Object = Me.InitFlowClass(si, api, globals)
'		'Dim Acct As Object = Me.InitAccountClass(si, api, globals)
			
'		If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
'		api.Data.Calculate("A#CR_Q13B:F#None:I#None:U3#8080:U4#none:U5#None:U6#None:U7#None:U8#None=A#681100:F#None:I#None:U3#8080:U4#None:U5#None:U6#None:U7#None:U8#None+A#680100:F#None:I#None:U3#8080:U4#None:U5#None:U6#None:U7#None:U8#None+A#682100:F#None:I#None:U3#8080:U4#None:U5#None:U6#None:U7#None:U8#None")
'		End If

'	End Sub
	
	Sub AccCRw2 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		'Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		'Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		
		'Affectation du résultat - Appropriation of N-1 income / loss
		If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
		api.Data.Calculate("A#CR_W2:U4#inputgestion=A#623200:U4#inputlegal")
		End If
	
	End Sub
	
	Sub Calc_RH021 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
				
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim CurYear As Integer = api.Time.GetYearFromId(timeId)	
		
		If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
			If CurYear > 2019 Then
				If api.Pov.Scenario.Name.StartsWith("O") Then	
					If api.Pov.Scenario.Name="O1" Then 
					api.Data.ClearCalculatedData("A#RH021:S#O1",True,True,True)
					api.Data.Calculate("A#RH021:S#O1:F#OTROS:U1#HD01:U2#HD01=A#RH021:S#R:F#OTROS:U1#HD01:U2#HD01")
					Else If api.Pov.Scenario.Name="O2" Then
					api.Data.ClearCalculatedData("A#RH021:S#O2",True,True,True)
					api.Data.Calculate("A#RH021:S#O2:F#OTROS:U1#HD01:U2#HD01=A#RH021:S#R:F#OTROS:U1#HD01:U2#HD01")
					Else If api.Pov.Scenario.Name="O3" Then
					api.Data.ClearCalculatedData("A#RH021:S#O3",True,True,True)
					api.Data.Calculate("A#RH021:S#O3:F#OTROS:U1#HD01:U2#HD01=A#RH021:S#R:F#OTROS:U1#HD01:U2#HD01")
					End If
				Else If api.Pov.Scenario.Name.StartsWith("P") Then
					
					If api.Pov.Scenario.Name="P03" Then 
					api.Data.ClearCalculatedData("A#RH021:S#P03",True,True,True)
					api.Data.Calculate("A#RH021:S#P03:F#OTROS:U1#HD01:U2#HD01=A#RH021:S#R:F#OTROS:U1#HD01:U2#HD01")
					
					Else If api.Pov.Scenario.Name="P06" Then
					api.Data.ClearCalculatedData("A#RH021:S#P06",True,True,True)
					api.Data.Calculate("A#RH021:S#P06:F#OTROS:U1#HD01:U2#HD01=A#RH021:S#R:F#OTROS:U1#HD01:U2#HD01")
					
					Else If api.Pov.Scenario.Name="P09" Then
					api.Data.ClearCalculatedData("A#RH021:S#P09",True,True,True)
					api.Data.Calculate("A#RH021:S#P09:F#OTROS:U1#HD01:U2#HD01=A#RH021:S#R:F#OTROS:U1#HD01:U2#HD01")
					
					Else If api.Pov.Scenario.Name="P11" Then
					api.Data.ClearCalculatedData("A#RH021:S#P11",True,True,True)
					api.Data.Calculate("A#RH021:S#P11:F#OTROS:U1#HD01:U2#HD01=A#RH021:S#R:F#OTROS:U1#HD01:U2#HD01")
					
					End If
					
'				Else If api.Pov.Scenario.Name.StartsWith("R_Extrapolado") Then
										
'					api.Data.ClearCalculatedData("A#RH021:S#R_Extrapolado",True,True,True)
'					api.Data.Calculate("A#RH021:S#R_Extrapolado:F#OTROS:U1#HD01:U2#HD01=A#RH021:S#R:F#OTROS:U1#HD01:U2#HD01")
							
					
										
				End If	
			End If
		
		
		End If
		
	End Sub

	Sub Calc_RH028_RH029 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
	
	If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
		
		
		
		Dim DatoRealUltimo = api.Data.GetDataCell("A#CR_Z:S#R:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
		Dim DatoP11Ultimo = api.Data.GetDataCell("A#CR_Z:S#P11:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
		Dim DatoP09Ultimo = api.Data.GetDataCell("A#CR_Z:S#P09:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
		Dim DatoP06Ultimo = api.Data.GetDataCell("A#CR_Z:S#P06:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
		Dim DatoP03Ultimo = api.Data.GetDataCell("A#CR_Z:S#P03:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
		
		'REAL
		If api.Pov.Scenario.Name = "R" Then
					
				Dim DatoReal = api.Data.GetDataCell("A#CR_Z:S#R:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
					
				If DatoReal <> 0 Then
					
					'Borramos RH028 y RH029
					api.Data.ClearCalculatedData("A#RH028:S#R",True,True,True)
					api.Data.ClearCalculatedData("A#RH029:S#R",True,True,True)
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#R = A#RH001:S#R - A#RH001:S#R:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#R = A#RH001:S#R - A#RH001:S#R:T#POVPriorYearM12 - A#RH027:S#R")
						
				End If
									
		'PREVISTO
						
		Else If api.Pov.Scenario.Name = "P03" Then	
					
				Dim DatoPrevistoP03 = api.Data.GetDataCell("A#CR_Z:S#P03:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
				
				If DatoPrevistoP03 <> 0 Then
										
				'Borramos RH028 y RH029
				api.Data.ClearCalculatedData("A#RH029:S#P03",True,True,True)
				api.Data.ClearCalculatedData("A#RH028:S#P03",True,True,True)
				'Calculamos RH028 y RH029
				api.Data.Calculate("A#RH029:S#P03 = A#RH001:S#P03 - A#RH001:S#R:T#POVPriorYearM12")
				api.Data.Calculate("A#RH028:S#P03 = A#RH001:S#P03 - A#RH001:S#R:T#POVPriorYearM12 - A#RH027:S#P03")																	
				
				
				End If
		
		Else If api.Pov.Scenario.Name = "P06" Then	
					
				Dim DatoPrevistoP06 = api.Data.GetDataCell("A#CR_Z:S#P06:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
				
				If DatoPrevistoP06 <> 0 Then
										
				'Borramos RH028 y RH029
				api.Data.ClearCalculatedData("A#RH029:S#P06",True,True,True)
				api.Data.ClearCalculatedData("A#RH028:S#P06",True,True,True)
				'Calculamos RH028 y RH029
				api.Data.Calculate("A#RH029:S#P06 = A#RH001:S#P06 - A#RH001:S#R:T#POVPriorYearM12")
				api.Data.Calculate("A#RH028:S#P06 = A#RH001:S#P06 - A#RH001:S#R:T#POVPriorYearM12 - A#RH027:S#P06")																	
				
				
				End If
				
		Else If api.Pov.Scenario.Name = "P09" Then	
					
				Dim DatoPrevistoP09 = api.Data.GetDataCell("A#CR_Z:S#P09:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
				
				If DatoPrevistoP09 <> 0 Then
										
				'Borramos RH028 y RH029
				api.Data.ClearCalculatedData("A#RH029:S#P09",True,True,True)
				api.Data.ClearCalculatedData("A#RH028:S#P09",True,True,True)
				'Calculamos RH028 y RH029
				api.Data.Calculate("A#RH029:S#P09 = A#RH001:S#P09 - A#RH001:S#R:T#POVPriorYearM12")
				api.Data.Calculate("A#RH028:S#P09 = A#RH001:S#P09 - A#RH001:S#R:T#POVPriorYearM12 - A#RH027:S#P09")																	
				
				
				End If
				
		Else If api.Pov.Scenario.Name = "P11" Then	
					
				Dim DatoPrevistoP11 = api.Data.GetDataCell("A#CR_Z:S#P11:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
				
				If DatoPrevistoP11 <> 0 Then
										
				'Borramos RH028 y RH029
				api.Data.ClearCalculatedData("A#RH029:S#P11",True,True,True)
				api.Data.ClearCalculatedData("A#RH028:S#P11",True,True,True)
				'Calculamos RH028 y RH029
				api.Data.Calculate("A#RH029:S#P11 = A#RH001:S#P11 - A#RH001:S#R:T#POVPriorYearM12")
				api.Data.Calculate("A#RH028:S#P11 = A#RH001:S#P11 - A#RH001:S#R:T#POVPriorYearM12 - A#RH027:S#P11")																	
				
				
				End If
				
		Else If api.Pov.Scenario.Name = "O1" Then	
						
					
				Dim DatoPrevistoO1 = api.Data.GetDataCell("A#CR_Z:S#O1:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
								
				
				If DatoPrevistoO1 <> 0 Then
														
					
					'Borramos RH028 y RH029	
					api.Data.ClearCalculatedData("A#RH029:S#O1",True,True,True)
					api.Data.ClearCalculatedData("A#RH028:S#O1",True,True,True)
					
					If DatoRealUltimo <> 0 Then	
					
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O1 = A#RH001:S#O1 - A#RH001:S#R:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O1 = A#RH001:S#O1 - A#RH001:S#R:T#POVPriorYearM12 - A#RH027:S#O1")
					
					Else If DatoP11Ultimo <> 0 Then																	
				
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O1 = A#RH001:S#O1 - A#RH001:S#P11:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O1 = A#RH001:S#O1 - A#RH001:S#P11:T#POVPriorYearM12 - A#RH027:S#O1")
				
					Else If DatoP09Ultimo <> 0 Then						
				
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O1 = A#RH001:S#O1 - A#RH001:S#P09:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O1 = A#RH001:S#O1 - A#RH001:S#P09:T#POVPriorYearM12 - A#RH027:S#O1")
				
					Else If DatoP06Ultimo <> 0	Then						
				
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O1 = A#RH001:S#O1 - A#RH001:S#P06:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O1 = A#RH001:S#O1 - A#RH001:S#P06:T#POVPriorYearM12 - A#RH027:S#O1")
				
					Else If DatoP03Ultimo <> 0	Then						
				
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O1 = A#RH001:S#O1 - A#RH001:S#P03:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O1 = A#RH001:S#O1 - A#RH001:S#P03:T#POVPriorYearM12 - A#RH027:S#O1")
					
					End If
				End If
				
		Else If api.Pov.Scenario.Name = "O2" Then	
					
				Dim DatoPrevistoO2 = api.Data.GetDataCell("A#CR_Z:S#O2:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
				
				If DatoPrevistoO2 <> 0 Then
				
					'Borramos RH028 y RH029
					api.Data.ClearCalculatedData("A#RH029:S#O2",True,True,True)
					api.Data.ClearCalculatedData("A#RH028:S#O2",True,True,True)
					
					If DatoRealUltimo <> 0 Then	
					
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O2 = A#RH001:S#O2 - A#RH001:S#R:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O2 = A#RH001:S#O2 - A#RH001:S#R:T#POVPriorYearM12 - A#RH027:S#O2")					
					
					Else If DatoP11Ultimo <> 0	Then						
				
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O2 = A#RH001:S#O2 - A#RH001:S#P11:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O2 = A#RH001:S#O2 - A#RH001:S#P11:T#POVPriorYearM12 - A#RH027:S#O2")
				
					Else If DatoP09Ultimo <> 0	Then						
				
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O2 = A#RH001:S#O2 - A#RH001:S#P09:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O2 = A#RH001:S#O2 - A#RH001:S#P09:T#POVPriorYearM12 - A#RH027:S#O2")
				
					Else If DatoP06Ultimo <> 0	Then						
				
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O2 = A#RH001:S#O2 - A#RH001:S#P06:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O2 = A#RH001:S#O2 - A#RH001:S#P06:T#POVPriorYearM12 - A#RH027:S#O2")
				
					Else If DatoP03Ultimo <> 0	Then						
				
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O2 = A#RH001:S#O2 - A#RH001:S#P03:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O2 = A#RH001:S#O2 - A#RH001:S#P03:T#POVPriorYearM12 - A#RH027:S#O2")
					
					End If
				End If		
				
		Else If api.Pov.Scenario.Name = "O3" Then	
					
				Dim DatoPrevistoO3 = api.Data.GetDataCell("A#CR_Z:S#O3:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
				
				If DatoPrevistoO3 <> 0 Then
				
					'Borramos RH028 y RH029
					api.Data.ClearCalculatedData("A#RH029:S#O3",True,True,True)
					api.Data.ClearCalculatedData("A#RH028:S#O3",True,True,True)
						
					If DatoRealUltimo <> 0 Then	
						
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O3 = A#RH001:S#O3 - A#RH001:S#R:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O3 = A#RH001:S#O3 - A#RH001:S#R:T#POVPriorYearM12 - A#RH027:S#O3")
					
					Else If DatoP11Ultimo <> 0	Then						
				
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O3 = A#RH001:S#O3 - A#RH001:S#P11:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O3 = A#RH001:S#O3 - A#RH001:S#P11:T#POVPriorYearM12 - A#RH027:S#O3")
				
					Else If DatoP09Ultimo <> 0	Then						
				
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O3 = A#RH001:S#O3 - A#RH001:S#P09:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O3 = A#RH001:S#O3 - A#RH001:S#P09:T#POVPriorYearM12 - A#RH027:S#O3")
				
					Else If DatoP06Ultimo <> 0	Then						
				
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O3 = A#RH001:S#O3 - A#RH001:S#P06:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O3 = A#RH001:S#O3 - A#RH001:S#P06:T#POVPriorYearM12 - A#RH027:S#O3")
				
					Else If DatoP03Ultimo <> 0	Then						
				
					'Calculamos RH028 y RH029
					api.Data.Calculate("A#RH029:S#O3 = A#RH001:S#O3 - A#RH001:S#P03:T#POVPriorYearM12")
					api.Data.Calculate("A#RH028:S#O3 = A#RH001:S#O3 - A#RH001:S#P03:T#POVPriorYearM12 - A#RH027:S#O3")
					
					End If
				End If				
															
				
				
'		Else If api.Pov.Scenario.Name = "E1" Then	
					
'				Dim DatoPrevistoO1 = api.Data.GetDataCell("A#CR_Z:S#O1:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
				
'				If DatoPrevistoO1 <> 0 Then
				
'					'Borramos RH028 y RH029
'					api.Data.ClearCalculatedData("A#RH029:S#O1",True,True,True)
'					api.Data.ClearCalculatedData("A#RH028:S#O1",True,True,True)
					
'					If DatoRealUltimo <> 0 Then										
				
'					'Calculamos RH028 y RH029
'					api.Data.Calculate("A#RH029:S#O1 = A#RH001:S#O1 - A#RH001:S#R:T#POVPriorYearM12")
'					api.Data.Calculate("A#RH028:S#O1 = A#RH001:S#O1 - A#RH001:S#R:T#POVPriorYearM12 - A#RH027:S#O1")
					
'					Else If DatoP11Ultimo <> 0							
				
'					'Calculamos RH028 y RH029
'					api.Data.Calculate("A#RH029:S#O1 = A#RH001:S#O1 - A#RH001:S#P11:T#POVPriorYearM12")
'					api.Data.Calculate("A#RH028:S#O1 = A#RH001:S#O1 - A#RH001:S#P11:T#POVPriorYearM12 - A#RH027:S#O1")
				
'					Else If DatoP09Ultimo <> 0							
				
'					'Calculamos RH028 y RH029
'					api.Data.Calculate("A#RH029:S#O1 = A#RH001:S#O1 - A#RH001:S#P09:T#POVPriorYearM12")
'					api.Data.Calculate("A#RH028:S#O1 = A#RH001:S#O1 - A#RH001:S#P09:T#POVPriorYearM12 - A#RH027:S#O1")
				
'					Else If DatoP06Ultimo <> 0							
				
'					'Calculamos RH028 y RH029
'					api.Data.Calculate("A#RH029:S#O1 = A#RH001:S#O1 - A#RH001:S#P06:T#POVPriorYearM12")
'					api.Data.Calculate("A#RH028:S#O1 = A#RH001:S#O1 - A#RH001:S#P06:T#POVPriorYearM12 - A#RH027:S#O1")
				
'					Else If DatoP03Ultimo <> 0							
				
'					'Calculamos RH028 y RH029
'					api.Data.Calculate("A#RH029:S#O1 = A#RH001:S#O1 - A#RH001:S#P03:T#POVPriorYearM12")
'					api.Data.Calculate("A#RH028:S#O1 = A#RH001:S#O1 - A#RH001:S#P03:T#POVPriorYearM12 - A#RH027:S#O1")
					
'					End If
'				End If

		End If
	End If
	

	End Sub
	
	
	Sub Calc_RH015 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
			If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
				
				api.Data.ClearCalculatedData("A#RH015",True,True,True)
				
'				api.Data.Calculate("A#RH015:F#FIJOS:O#Import:I#ALL:U1#ALL:U2#ALL:U3#ALL:U4#ALL=A#RH024:F#FIJOS:O#Import:I#ALL:U1#ALL:U2#ALL:U3#ALL:U4#ALL*A#RH021:F#OTROS:O#Forms:I#None:U1#HD01:U2#HD01:U3#None:U4#None")
'				api.Data.Calculate("A#RH015:F#EVENTUALES:O#Import:I#ALL:U1#ALL:U2#ALL:U3#ALL:U4#ALL=A#RH024:F#EVENTUALES:O#Import:I#ALL:U1#ALL:U2#ALL:U3#ALL:U4#ALL*A#RH021:F#OTROS:O#Forms:I#None:U1#HD01:U2#HD01:U3#None:U4#None")
'				api.Data.Calculate("A#RH015:F#OTROS:O#Import:I#ALL:U1#ALL:U2#ALL:U3#ALL:U4#ALL=A#RH024:F#OTROS:O#Import:I#ALL:U1#ALL:U2#ALL:U3#ALL:U4#ALL*A#RH021:F#OTROS:O#Forms:I#None:U1#HD01:U2#HD01:U3#None:U4#None")
				
				api.Data.Calculate("A#RH015:F#FIJOS:O#Import = MultiplyUnbalanced(A#RH024:F#FIJOS:O#Import, A#RH021:F#OTROS:O#Forms:I#None:U1#HD01:U2#HD01:U3#None:U4#None, I#None:U1#HD01:U2#HD01:U3#None:U4#None)")
				api.Data.Calculate("A#RH015:F#EVENTUALES:O#Import = MultiplyUnbalanced(A#RH024:F#EVENTUALES:O#Import, A#RH021:F#OTROS:O#Forms:I#None:U1#HD01:U2#HD01:U3#None:U4#None, I#None:U1#HD01:U2#HD01:U3#None:U4#None)")
				api.Data.Calculate("A#RH015:F#OTROS:O#Import = MultiplyUnbalanced(A#RH024:F#OTROS:O#Import, A#RH021:F#OTROS:O#Forms:I#None:U1#HD01:U2#HD01:U3#None:U4#None, I#None:U1#HD01:U2#HD01:U3#None:U4#None)")
			
			End If	
		
	End Sub
	
	Sub GtosGrales_CRJ1 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
				
			'If (Not api.Entity.HasChildren()) Then
			
			api.Data.ClearCalculatedData("A#CR_J1",True,True,True)
			api.Data.Calculate("A#CR_J1:U3#1010=A#Calc_CRJ_1010:U3#1010 * (-1)")
			api.Data.Calculate("A#CR_J1:U3#1011=A#Calc_CRJ:U3#1011 * (-1)")
			api.Data.Calculate("A#CR_J1:U3#1099=A#Calc_CRJ:U3#1099 * (-1)")
				
			'End If
		
	End Sub
		
	Sub GtosGrales_CRJ2 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
		'If (Not api.Entity.HasChildren()) Then
			
			api.Data.ClearCalculatedData("A#CR_J2",True,True,True)	
			api.Data.Calculate("A#CR_J2:U3#2021=A#Calc_CRJ:U3#2021 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2022=A#Calc_CRJ:U3#2022 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2023=A#Calc_CRJ:U3#2023 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2024=A#Calc_CRJ:U3#2024 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2025=A#Calc_CRJ:U3#2025 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2026=A#Calc_CRJ:U3#2026 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2027=A#Calc_CRJ:U3#2027 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2028=A#Calc_CRJ:U3#2028 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2029=A#Calc_CRJ:U3#2029 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2030=A#Calc_CRJ:U3#2030 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2031=A#Calc_CRJ:U3#2031 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2050=A#Calc_CRJ:U3#2050 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2099=A#Calc_CRJ:U3#2099 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#2999=A#Calc_CRJ:U3#2999 * (-1)")
			api.Data.Calculate("A#CR_J2:U3#None=A#Calc_CRJ:U3#None * (-1)")
			
		'End If

	End Sub

	Sub GtosGrales_CRJ3 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
		'If (Not api.Entity.HasChildren()) Then
			
			api.Data.ClearCalculatedData("A#CR_J3",True,True,True)
			api.Data.Calculate("A#CR_J3:U3#3030=A#Calc_CRJ:U3#3030 * (-1)")
			
		'End If

	End Sub

	Sub GtosGrales_CRJ4 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
		'If (Not api.Entity.HasChildren()) Then
			
			api.Data.ClearCalculatedData("A#CR_J4",True,True,True)
			api.Data.Calculate("A#CR_J4:U3#4040=A#Calc_CRJ:U3#4040 * (-1)")
			
		'End If

	End Sub

	Sub GtosGrales_CRJ5 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
		'If (Not api.Entity.HasChildren()) Then
			
			api.Data.ClearCalculatedData("A#CR_J5",True,True,True)
			api.Data.Calculate("A#CR_J5:U3#9099=A#623200:U3#9099")
			
		'End If

	End Sub

	Sub GtosGrales_CRG1 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
		'If (Not api.Entity.HasChildren()) Then
			
			api.Data.ClearCalculatedData("A#CR_G1",True,True,True)
			api.Data.Calculate("A#CR_G1:U3#1010=A#CRG1_Calc:U3#1010 * (-1)")
		
		'End If

	End Sub

	Sub GtosGrales_CRG2 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		'If (Not api.Entity.HasChildren()) Then
			
			api.Data.ClearCalculatedData("A#CR_G2",True,True,True)
			api.Data.Calculate("A#CR_G2:U3#1010=A#CRG2_Calc:U3#1010 * (-1)")
		
		'End If

	End Sub

	Sub GtosGrales_CRG3 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		'If (Not api.Entity.HasChildren()) Then
			
			api.Data.ClearCalculatedData("A#CR_G3",True,True,True)
			api.Data.Calculate("A#CR_G3:U3#1010=A#CRG3_Calc:U3#1010 * (-1)")
		'
		'End If

	End Sub

	Sub GtosGrales_Q21 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
		'If (Not api.Entity.HasChildren()) Then
		
		'Calc Q21 original
		api.Data.ClearCalculatedData("A#CR_Q21",True,True,True)
		api.Data.Calculate("A#CR_Q21:U3#8080=(A#Calc_CRJ:U3#8080 * (-1)) - A#623200:U3#8080 + A#682100:U3#8080")
		api.Data.Calculate("A#CR_Q21:U3#8080:I#None=A#CR_Q21:U3#8080:I#None + A#681100:U3#8080:I#None + A#680100:U3#8080:I#None + A#682100:U3#8080:I#None")
		

		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
		
		If CurYear > 2020 Then
			If api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity() Then
			'Solo revierte cuentas base de la CR_Q	
			Dim CuentasQ2 As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "CR_Q2"))
			For Each Acc As Member In CuentasQ2 
			api.Data.Calculate("A#" & Acc.Name.ToString & ":U3#8080:U4#ELCE_REV = - A#" & Acc.Name.ToString & ":U3#8080:U4#ICP_Gestion")
			Next
	
			'Reversion en vaciado de las plug con ceco 8080 en elce_ges
				api.Data.Calculate("A#607010:U4#ELCE_REV = A#CR_Q23:U4#ELCE_REV * (-1)")
				api.Data.Calculate("A#601010:U4#ELCE_REV = A#CR_Q22:U4#ELCE_REV * (-1)")
				api.Data.Calculate("A#640210:U3#2099:U4#ELCE_REV = A#CR_Q21:U3#8080:U4#ELCE_REV * (-1)")
			End If
		End If

	End Sub
	
	Sub Calc_BAL01 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
		'If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
			If api.Pov.Scenario.Name = "R" Then
				
				api.Data.ClearCalculatedData("A#BAL01_N",True,True,True)
				api.Data.ClearCalculatedData("A#BAL01_D",True,True,True)
				
				api.Data.Calculate("A#BAL01_N = A#BSA_B")
				api.Data.Calculate("A#BAL01_D = A#BSP_C")
				
			'Cuando el escenario NO es Real	el dato se imputa				
			
			End If
		'End If

	End Sub
	
	Sub Calc_BAL02 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
		'If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
			If api.Pov.Scenario.Name = "R" Then
				
				api.Data.ClearCalculatedData("A#BAL02_N",True,True,True)
				api.Data.ClearCalculatedData("A#BAL02_D",True,True,True)
				
				api.Data.Calculate("A#BAL02_N = A#BSP_A")
				api.Data.Calculate("A#BAL02_D = A#BSA")
				
			'Cuando el escenario NO es Real	el dato se imputa				
			End If
		'End If

	End Sub
	
	Sub Calc_BAL03 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
		'If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
			If api.Pov.Scenario.Name = "R" Then
				
				api.Data.ClearCalculatedData("A#BAL03_N",True,True,True)
				api.Data.Calculate("A#BAL03_N = (A#BSA_B - A#BSP_C)")
				
			'Cuando el escenario NO es Real	el dato se imputa			
				
			End If
		'End If

	End Sub
	
	Sub Calc_PM01 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
		'If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
			If api.Pov.Scenario.Name = "R" Then
				
				api.Data.ClearCalculatedData("A#PM01_N",True,True,True)
				api.Data.ClearCalculatedData("A#PM01_D",True,True,True)
				api.Data.Calculate("A#PM01_N = (A#TAM_CD001+A#TAM_CD002+A#TAM_CD004)/11*365")
				api.Data.Calculate("A#PM01_D = (A#TAM_CV008+A#TAM_477)")
				
			'Cuando el escenario NO es Real	el dato se imputa			
				
			End If
		'End If

	End Sub
	
	Sub Calc_PM02 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
		'If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
			If api.Pov.Scenario.Name = "R" Then
				
				api.Data.ClearCalculatedData("A#PM02_N",True,True,True)
				api.Data.ClearCalculatedData("A#PM02_D",True,True,True)
				api.Data.Calculate("A#PM02_N = (A#TAM_BSP_C_V_1 + A#TAM_BSP_C_V_2 + A#TAM_BSP_C_V_3 - A#TAM_BSA_B_II_6)/11*365")
				api.Data.Calculate("A#PM02_D = A#TAM_CR_B - A#TAM_640200 - A#TAM_6692 + A#TAM_CR_D2 + A#TAM_CR_D3 + A#TAM_CR_E + A#TAM_CR_Q22 + A#TAM_CR_Q23 + A#TAM_CG - A#TAM_CG_13 - A#TAM_CG_14 + A#TAM_472 + (A#400900:T#POVPrior12 + A#403900:T#POVPrior12 +	A#410900:T#POVPrior12 +A#410930:T#POVPrior12 +A#410940:T#POVPrior12 +A#410950:T#POVPrior12) -(A#400900 + A#403900 +A#410900 +A#410930 +A#410940 + A#410950)")
			'Cuando el escenario NO es Real	el dato se imputa			
				
			End If
		'End If

	End Sub
	
	Sub Calc_PM03 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
		'If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
			If api.Pov.Scenario.Name = "R" Then
				
				api.Data.ClearCalculatedData("A#PM03_N",True,True,True)
				api.Data.ClearCalculatedData("A#PM03_D",True,True,True)
				api.Data.Calculate("A#PM03_N = ((A#TAM_BSA_B_II_1 + A#TAM_BSA_B_II_2)/11*365)")
				api.Data.Calculate("A#PM03_D = (A#TAM_CR_D2 +A#TAM_CR_D3)")
				
			'Cuando el escenario NO es Real	el dato se imputa			
				
			End If
		'End If

	End Sub
	
	Sub Calc_PM04 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
		'If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
			If api.Pov.Scenario.Name = "R" Then
				
				api.Data.ClearCalculatedData("A#PM04_N",True,True,True)
				api.Data.ClearCalculatedData("A#PM04_D",True,True,True)
				api.Data.Calculate("A#PM04_N = (A#TAM_BSA_B_II_3)/11*365")
				api.Data.Calculate("A#PM04_D = A#TAM_CR_B + A#TAM_CR_D2 + A#TAM_CR_D3 + A#TAM_CR_E + A#TAM_CR_G + A#TAM_CR_Q11")
				
			'Cuando el escenario NO es Real	el dato se imputa			
				
			End If
		'End If

	End Sub
	
	Sub Calc_PM05 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
		'If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
			If api.Pov.Scenario.Name = "R" Then
				
				api.Data.ClearCalculatedData("A#PM05_N",True,True,True)
				api.Data.ClearCalculatedData("A#PM05_D",True,True,True)
				api.Data.Calculate("A#PM05_N = (A#TAM_BSA_B_II_4 + A#TAM_BSA_B_II_5)/11*365")
				api.Data.Calculate("A#PM05_D = A#TAM_CR_B + A#TAM_CR_D2 + A#TAM_CR_D3 + A#TAM_CR_E + A#TAM_CR_G + A#TAM_CR_Q11")
				
			'Cuando el escenario NO es Real	el dato se imputa			
				
			End If
		'End If

	End Sub
	
	Sub Calc_PM06 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
		'If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
			If api.Pov.Scenario.Name = "R" Then
				
				api.Data.ClearCalculatedData("A#PM06_N",True,True,True)
				api.Data.ClearCalculatedData("A#PM06_D",True,True,True)
				api.Data.Calculate("A#PM06_N = (A#TAM_CD003)/11*365")
				api.Data.Calculate("A#PM06_D = A#TAM_CV008")
				
			'Cuando el escenario NO es Real	el dato se imputa			
				
			End If
		'End If

	End Sub
	
	Sub Cartera_Deuda (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
				
		api.Data.ClearCalculatedData("A#CD003",True,True,True)
		api.Data.ClearCalculatedData("A#CD003BC",True,True,True)
			
		api.Data.ClearCalculatedData("A#CD004",True,True,True)
		api.Data.ClearCalculatedData("A#CD004BC",True,True,True)
			
		api.Data.ClearCalculatedData("A#CD005",True,True,True)
		api.Data.ClearCalculatedData("A#CD005BC",True,True,True)
		
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
		
		
		If CurYear < 2021 Then
			api.Data.Calculate("A#CD003BC = A#CD003B")
			api.Data.Calculate("A#CD003 = A#CD003BC + A#CD003M")
				
			api.Data.Calculate("A#CD004BC = A#CD004B")
			api.Data.Calculate("A#CD004 = A#CD004BC + A#CD004M")
				
			api.Data.Calculate("A#CD005BC = A#CD005B")
			api.Data.Calculate("A#CD005 = A#CD005BC + A#CD005M")
			
			api.Data.ClearCalculatedData("A#CD001",True,True,True)
			api.Data.Calculate("A#CD001=A#CD005-A#CD004-A#CD003-A#CD002")
		
		Else If CurYear > 2020 And CurYear < 2022 Then
				api.Data.Calculate("A#CD003BC = A#CD003B")
				api.Data.Calculate("A#CD003 = A#CD003BC + A#CD003M")
					
				'No queremos en el 2021 la reversion de BS en CD del 2020 -> empezamos de 0 -> para que cuadre la Calidad de Deuda
				api.Data.ClearCalculatedData("A#CD003BC:U4#RetSoc_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD003:U4#RetSoc_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD003BC:U4#RetSocG_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD003:U4#RetSocG_R2:F#APE",True,True,True)
						
				api.Data.Calculate("A#CD004BC = A#CD004B")
				api.Data.Calculate("A#CD004 = A#CD004BC + A#CD004M")
				
				'No queremos en el 2021 la reversion de BS en CD del 2020 -> empezamos de 0 -> para que cuadre la Calidad de Deuda
				api.Data.ClearCalculatedData("A#CD004BC:U4#RetSoc_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD004:U4#RetSoc_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD004BC:U4#RetSocG_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD004:U4#RetSocG_R2:F#APE",True,True,True)
						
				api.Data.Calculate("A#CD005BC = A#CD005B")
				api.Data.Calculate("A#CD005 = A#CD005BC + A#CD005M")
				
				'No queremos en el 2021 la reversion de BS en CD del 2020 -> empezamos de 0 -> para que cuadre la Calidad de Deuda
				api.Data.ClearCalculatedData("A#CD005BC:U4#RetSoc_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD005:U4#RetSoc_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD005BC:U4#RetSocG_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD005:U4#RetSocG_R2:F#APE",True,True,True)
							
				If api.Pov.Scenario.Name <> "O1" Then 'La CD001 se ha cargado en O1 de 2021 y se duplica el dato
					api.Data.ClearCalculatedData("A#CD001",True,True,True)
					api.Data.Calculate("A#CD001=A#CD005-A#CD004-A#CD003-A#CD002")
				Else
					If api.Entity.HasChildren() Then
						api.Data.ClearCalculatedData("A#CD001",True,True,True)
						api.Data.Calculate("A#CD001=A#CD005-A#CD004-A#CD003-A#CD002")
					End If
				End If
		
		Else If CurYear > 2021 Then
			api.Data.Calculate("A#CD003BC = A#CD003B")
			api.Data.Calculate("A#CD003 = A#CD003BC + A#CD003M")
				
			api.Data.Calculate("A#CD004BC = A#CD004B")
			api.Data.Calculate("A#CD004 = A#CD004BC + A#CD004M")
				
			api.Data.Calculate("A#CD005BC = A#CD005B")
			api.Data.Calculate("A#CD005 = A#CD005BC + A#CD005M")
			
			api.Data.ClearCalculatedData("A#CD001",True,True,True)
			api.Data.Calculate("A#CD001=A#CD005-A#CD004-A#CD003-A#CD002")
			
			If Not(api.Pov.Scenario.Name.StartsWith("O")) Then
				'Parece que sí queremos en el 2021 la reversion de BS en CD del 2020 -> empezamos de 0 -> para que cuadre la Calidad de Deuda
				api.Data.ClearCalculatedData("A#CD003BC:U1#None:U4#RetSoc_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD003:U1#None:U4#RetSoc_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD003BC:U1#None:U4#RetSocG_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD003:U1#None:U4#RetSocG_R2:F#APE",True,True,True)
				
				api.Data.ClearCalculatedData("A#CD003BC:U1#None:U4#RetSoc_R2:F#DCA",True,True,True)
				api.Data.ClearCalculatedData("A#CD003:U1#None:U4#RetSoc_R2:F#DCA",True,True,True)
				api.Data.ClearCalculatedData("A#CD003BC:U1#None:U4#RetSocG_R2:F#DCA",True,True,True)
				api.Data.ClearCalculatedData("A#CD003:U1#None:U4#RetSocG_R2:F#DCA",True,True,True)
					
				'No queremos en el 2021 la reversion de BS en CD del 2020 -> empezamos de 0 -> para que cuadre la Calidad de Deuda
				api.Data.ClearCalculatedData("A#CD004BC:U1#None:U4#RetSoc_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD004:U1#None:U4#RetSoc_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD004BC:U1#None:U4#RetSocG_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD004:U1#None:U4#RetSocG_R2:F#APE",True,True,True)
				
				api.Data.ClearCalculatedData("A#CD004BC:U1#None:U4#RetSoc_R2:F#DCA",True,True,True)
				api.Data.ClearCalculatedData("A#CD004:U1#None:U4#RetSoc_R2:F#DCA",True,True,True)
				api.Data.ClearCalculatedData("A#CD004BC:U1#None:U4#RetSocG_R2:F#DCA",True,True,True)
				api.Data.ClearCalculatedData("A#CD004:U1#None:U4#RetSocG_R2:F#DCA",True,True,True)
					
				'No queremos en el 2021 la reversion de BS en CD del 2020 -> empezamos de 0 -> para que cuadre la Calidad de Deuda
				api.Data.ClearCalculatedData("A#CD005BC:U1#None:U4#RetSoc_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD005:U1#None:U4#RetSoc_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD005BC:U1#None:U4#RetSocG_R2:F#APE",True,True,True)
				api.Data.ClearCalculatedData("A#CD005:U1#None:U4#RetSocG_R2:F#APE",True,True,True)
				
				api.Data.ClearCalculatedData("A#CD005BC:U1#None:U4#RetSoc_R2:F#DCA",True,True,True)
				api.Data.ClearCalculatedData("A#CD005:U1#None:U4#RetSoc_R2:F#DCA",True,True,True)
				api.Data.ClearCalculatedData("A#CD005BC:U1#None:U4#RetSocG_R2:F#DCA",True,True,True)
				api.Data.ClearCalculatedData("A#CD005:U1#None:U4#RetSocG_R2:F#DCA",True,True,True)
				
				api.Data.ClearCalculatedData("A#CD001",True,True,True)
				api.Data.Calculate("A#CD001=A#CD005-A#CD004-A#CD003-A#CD002")
				
				End If
		
		End If
		

	End Sub
	
	Sub Cartera_Ventas (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		'SB. En HFM no distingue si el entity tiene o no hijos	
		'SB. Se calcula para Local, Ajustes y parece que para Share y Elimination también
		'If (Not api.Entity.HasChildren() ) Then
			'SB. Se calcula CV001 con un IF para todos los escenarios R, P, O, S, E
'		If (Not api.Entity.HasChildren() And (api.Pov.Cons.Name = "Share" Or api.Pov.Cons.Name ="Elimination")) Then
'			'SB. La cuenta CV004 es calculada siempre
'			api.Data.ClearCalculatedData("A#CV004",True,True,True)
'			api.Data.Calculate("A#CV004 = A#CV001+A#CV002+A#CV005-A#CV003+A#CV_A_FCT")
			
'			api.Data.ClearCalculatedData("A#CV010",True,True,True)
'			api.Data.Calculate("A#CV010 = A#CV006+A#CV007+A#CV009-A#CV008+A#CV_B_FCT")
'		Else If api.Entity.HasChildren() Then
'			api.Data.ClearCalculatedData("A#CV004",True,True,True)
'			api.Data.Calculate("A#CV004 = A#CV001+A#CV002+A#CV005-A#CV003+A#CV_A_FCT")
			
'			api.Data.ClearCalculatedData("A#CV010",True,True,True)
'			api.Data.Calculate("A#CV010 = A#CV006+A#CV007+A#CV009-A#CV008+A#CV_B_FCT")
'		End If		

	End Sub
	
	Sub CalcAjus1 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
		'If ((api.Pov.Entity.Name="LEGAL" Or api.Pov.Entity.Name="FISCAL" Or api.Pov.Entity.Name="IPT" Or api.Pov.Entity.Name="IPC") And api.Cons.IsLocalCurrencyforEntity()) Then
		If api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity() Then
		
		api.Data.Calculate("A#730000:I#None:F#None:UD3#None:UD4#Ajus1= - A#200XXX:I#Top:F#INC:UD3#Top:UD4#ICP_Legal - A#200XXX:I#Top:F#DIS:UD3#Top:UD4#ICP_Legal - A#200XXX:I#Top:F#None:UD3#Top:UD4#ICP_Legal")
		api.Data.Calculate("A#731000:I#None:F#None:UD3#None:UD4#Ajus1= - A#210XXX:I#Top:F#INC:UD3#Top:UD4#ICP_Legal - A#210XXX:I#Top:F#DIS:UD3#Top:UD4#ICP_Legal - A#210XXX:I#Top:F#None:UD3#Top:UD4#ICP_Legal")
		api.Data.Calculate("A#733000:I#None:F#None:UD3#None:UD4#Ajus1= - A#230XXX:I#Top:F#INC:UD3#Top:UD4#ICP_Legal - A#230XXX:I#Top:F#DIS:UD3#Top:UD4#ICP_Legal - A#230XXX:I#Top:F#None:UD3#Top:UD4#ICP_Legal")
		
		api.Data.Calculate("A#730000:F#None:I#None:UD3#None:UD4#Ajus2= - A#200XXX:F#INC:I#Top:UD3#Top:UD4#ICP_Gestion - A#200XXX:F#DIS:I#Top:UD3#Top:UD4#ICP_Gestion - A#200XXX:I#Top:F#None:UD3#Top:UD4#ICP_Gestion")
		api.Data.Calculate("A#731000:F#None:I#None:UD3#None:UD4#Ajus2= - A#210XXX:F#INC:I#Top:UD3#Top:UD4#ICP_Gestion - A#210XXX:F#DIS:I#Top:UD3#Top:UD4#ICP_Gestion - A#210XXX:I#Top:F#None:UD3#Top:UD4#ICP_Gestion")
		api.Data.Calculate("A#733000:F#None:I#None:UD3#None:UD4#Ajus2= - A#230XXX:F#INC:I#Top:UD3#Top:UD4#ICP_Gestion - A#230XXX:F#DIS:I#Top:UD3#Top:UD4#ICP_Gestion - A#230XXX:I#Top:F#None:UD3#Top:UD4#ICP_Gestion")
		
		'api.Data.Calculate("A#CV002:UD4#Ajus1=A#700000:UD4#Ajus1")
		End If

	End Sub

	
#End Region

#Region "Member Formulas for UD4"

	Sub Autoreversal (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		'If (Not api.Entity.HasChildren() And (api.Cons.IsLocalCurrencyforEntity() Or (api.Pov.Cons.Name = "OwnerPostAdj") Or (api.Pov.Cons.Name = "OwnerPreAdj") Or api.cons.IsForeignCurrencyForEntity)) Then		
		If api.Cons.IsLocalCurrencyforEntity()  Then
			api.Data.Calculate("U4#RetSoc1_SR:O#AdjInput = -U4#RetSoc1_SI:O#AdjInput:T#PovPriorYearM12")
			Dim CuentasDBE As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "Capital"))
				For Each Account As Member In CuentasDBE 
					'api.Data.ClearCalculatedData("A#"+Account.Name.ToString+":U4#RetSoc1_SR:O#AdjInput:F#DBE",True,True,True)
					'api.Data.ClearCalculatedData("A#"+Account.Name.ToString+":U4#RetSoc1_SR:O#AdjInput:F#OMO",True,True,True)
				 	api.Data.Calculate("A#"+Account.Name.ToString+":U4#RetSoc1_SR:O#AdjInput:F#DBE = RemoveZeros(A#"+Account.Name.ToString+":U4#RetSoc1_SR:O#AdjInput:F#OMO)")
					api.Data.ClearCalculatedData("A#"+Account.Name.ToString+":U4#RetSoc1_SR:O#AdjInput:F#OMO",True,True,True)
				Next
		ElseIf (api.Pov.Cons.Name = "OwnerPostAdj") Or (api.Pov.Cons.Name = "OwnerPreAdj") Or api.cons.IsForeignCurrencyForEntity Then		
			api.Data.Calculate("U4#RetSoc1_SR:O#AdjInput = -U4#RetSoc1_SI:O#AdjInput:T#PovPriorYearM12")
		End If
	End Sub
	
	Sub Ajus1 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		'If api.pov.Entity.Name = "LEGAL" And api.Cons.IsLocalCurrencyforEntity() Then
		If api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity() Then
			If api.Data.GetDataCell("A#920L:C#Local:O#Top:I#Top:F#TF:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
				api.Data.Calculate("A#252000:F#REC:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = A#920L:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
				api.Data.Calculate("A#252000:F#SDC:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = A#920L:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
			Else
				api.Data.Calculate("A#171000:F#REC:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = -A#920L:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
				api.Data.Calculate("A#171000:F#SDC:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = -A#920L:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
			End If
			api.Data.Calculate("A#920L:U4#Ajus1 = -A#920L:U4#RetAut_Legal")
			
			If api.Data.GetDataCell("A#921L:C#Local:O#Top:I#Top:F#TF:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
				api.Data.Calculate("A#430000:F#REC:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = A#921L:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
				api.Data.Calculate("A#430000:F#SDC:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = A#921L:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
			Else
				api.Data.Calculate("A#400000:F#REC:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = -A#921L:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
				api.Data.Calculate("A#400000:F#SDC:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = -A#921L:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
			End If
			api.Data.Calculate("A#921L:U4#Ajus1 = -A#921L:U4#RetAut_Legal")

			If api.Data.GetDataCell("A#922L:C#Local:O#Top:I#Top:F#TF:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
				api.Data.Calculate("A#542000:F#REC:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = A#922L:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
				api.Data.Calculate("A#542000:F#SDC:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = A#922L:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
			Else
				api.Data.Calculate("A#521000:F#REC:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = -A#922L:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
				api.Data.Calculate("A#521000:F#SDC:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = -A#922L:F#TF:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
			End If
			api.Data.Calculate("A#922L:U4#Ajus1 = -A#922L:U4#RetAut_Legal")
			
			If api.Data.GetDataCell("A#940L:C#Local:O#Top:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
				api.Data.Calculate("A#768000:F#None:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = A#940L:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
			Else
				api.Data.Calculate("A#668000:F#None:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = -A#940L:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None")
			End If
			api.Data.Calculate("A#940L:U4#Ajus1 = -A#940L:U4#RetAut_Legal")
			
'			Dim InvDesinv As String = "(A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & FX.averageRateToText & "-A#Capital:F#" & Flow.AFF.name & ":C#Local{1} * " & fx.closingRatePriorYEtoText & ")"
'
'			 (A#INVER_DESINV:C#Local:O#Top:I#Top:F#INC:U1#Top:U2#Top:U3#Top:U4#Agregated_Data:U5#None:U6#None:U7#None:U8#None - A#INVER_DESINV:C#Local:O#Top:I#Top:F#DIS:U1#Top:U2#Top:U3#Top:U4#Agregated_Data:U5#None:U6#None:U7#None:U8#None)")
			
			If api.Data.GetDataCell("A#950L:C#Local:O#Top:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None").CellAmount + api.Data.GetDataCell("A#INVER_DESINV:C#Local:O#Top:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
				api.Data.Calculate("A#768000:F#None:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = A#768000:F#None:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None + A#950L:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None + (A#INVER_DESINV:I#Top:F#INC:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None - A#INVER_DESINV:I#Top:F#DIS:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None + A#INVER_DESINV:I#Top:F#None:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None)")
			Else
				api.Data.Calculate("A#668000:F#None:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None = A#668000:F#None:I#None:U1#None:U2#None:U3#None:U4#Ajus1:U5#None:U6#None:U7#None:U8#None - A#950L:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None - (A#INVER_DESINV:I#Top:F#INC:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None - A#INVER_DESINV:I#Top:F#DIS:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None + A#INVER_DESINV:I#Top:F#None:U1#Top:U2#Top:U3#Top:U4#RetAut_Legal:U5#None:U6#None:U7#None:U8#None)")
			End If
			api.Data.Calculate("A#951L:U4#Ajus1 = -A#951L:U4#RetAut_Legal")
			api.Data.Calculate("A#952L:U4#Ajus1 = -A#952L:U4#RetAut_Legal")
			api.Data.Calculate("A#953L:U4#Ajus1 = -A#953L:U4#RetAut_Legal")
			
			'Calculo APE despues de realizar para ajus1
			'Recalcula MOV desde de recalcular APE, ya que TF se modifica.
			'Cuentas incluidas en este calculo 252000, 171000, 430000, 400000, 542000, 521000
			api.Data.ClearCalculatedData("F#APE:U4#Ajus1",True,True,True)
			api.Data.Calculate("F#APE:U4#Ajus1 = F#SDC:U4#Ajus1:T#PovPriorYearM12")	
			
			api.Data.ClearCalculatedData("A#252000:F#DIS:U4#Ajus1",True,True,True)
			api.Data.Calculate("A#252000:F#DIS:U4#Ajus1 = - A#252000:F#SDC:U4#Ajus1:T#PovPriorYearM12")	

			api.Data.ClearCalculatedData("A#171000:F#DIS:U4#Ajus1",True,True,True)
			api.Data.Calculate("A#171000:F#DIS:U4#Ajus1 = - A#171000:F#SDC:U4#Ajus1:T#PovPriorYearM12")	

			api.Data.ClearCalculatedData("A#430000:F#DIS:U4#Ajus1",True,True,True)
			api.Data.Calculate("A#430000:F#DIS:U4#Ajus1 = - A#430000:F#SDC:U4#Ajus1:T#PovPriorYearM12")	

			api.Data.ClearCalculatedData("A#400000:F#DIS:U4#Ajus1",True,True,True)
			api.Data.Calculate("A#400000:F#DIS:U4#Ajus1 = - A#400000:F#SDC:U4#Ajus1:T#PovPriorYearM12")	

			api.Data.ClearCalculatedData("A#542000:F#DIS:U4#Ajus1",True,True,True)
			api.Data.Calculate("A#542000:F#DIS:U4#Ajus1 = - A#542000:F#SDC:U4#Ajus1:T#PovPriorYearM12")	

			api.Data.ClearCalculatedData("A#521000:F#DIS:U4#Ajus1",True,True,True)
			api.Data.Calculate("A#521000:F#DIS:U4#Ajus1 = - A#521000:F#SDC:U4#Ajus1:T#PovPriorYearM12")	
			
'			api.Data.ClearCalculatedData("F#MOV:U4#Ajus1",True,True,True)
'			api.Data.Calculate("F#MOV:U4#Ajus1 = F#SDC:U4#Ajus1-F#TF:U4#Ajus1")	
			
		End If
	End Sub

	Sub Ajus2 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		'If api.pov.Entity.Name = "LEGAL" And api.Cons.IsLocalCurrencyforEntity() Then
		If api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity() Then

			Dim UNBase As List(Of Member) = api.Members.GetBaseMembers(api.Pov.UD1Dim.DimPk, api.Members.GetMemberId(dimtype.UD1.Id, "Top"))
			For Each UN As Member In UNBase

				If Not UN.Name.ToString.substring(0,4).equals("Elim") Then
					If api.Data.GetDataCell("A#940L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#940L:F#Top:I#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
					Else
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = -A#940L:F#Top:I#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
					End If
					api.Data.Calculate("A#940L:U4#Ajus2:U1#"+UN.Name.ToString+" = -A#940L:U4#Elim_G:U1#"+UN.Name.ToString+"")

					If api.Data.GetDataCell("A#952L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount  > 0 Then
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None + A#952L:F#Top:I#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
					Else
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None -A#952L:F#Top:I#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
					End If
					api.Data.Calculate("A#952L:U4#Ajus2:U1#"+UN.Name.ToString+" = -A#952L:U4#Elim_G:U1#"+UN.Name.ToString+"")

				Else 'If UN starts with Elim
					If api.Data.GetDataCell("A#940L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#940L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					Else
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = -A#940L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					End If
					api.Data.Calculate("A#940L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#940L:U4#Elim_G:U1#"+UN.Name.ToString+"")

					If api.Data.GetDataCell("A#952L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
						'api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None =  A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None + A#952L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None =  A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None + A#952L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
						'api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					Else
						'api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None -A#952L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None -A#952L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
						'api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					End If
					api.Data.Calculate("A#952L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#952L:U4#Elim_G:U1#"+UN.Name.ToString+"")
				End	If	

				'951L + 953L
				If Not UN.Name.ToString.equals("Elim_DN") And Not UN.Name.ToString.equals("Elim_Top") Then
					If Not UN.Name.ToString.substring(0,4).equals("Elim") Then
						If api.Data.GetDataCell("A#951L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
							'api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#951L:F#Top:I#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
							api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#951L:F#Top:I#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None + A#953L:F#INC:I#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None + A#953L:F#None:I#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
						Else
							'api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = -A#951L:F#Top:I#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
							api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = -A#951L:F#Top:I#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None -A#953L:F#INC:I#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None -A#953L:F#None:I#Top:U1#"+UN.Name.ToString+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
						End If
						api.Data.Calculate("A#951L:U4#Ajus2:U1#"+UN.Name.ToString+" = -A#951L:U4#Elim_G:U1#"+UN.Name.ToString+"")
						api.Data.Calculate("A#953L:U4#Ajus2:U1#"+UN.Name.ToString+" = -A#953L:U4#Elim_G:U1#"+UN.Name.ToString+"")
					Else 'If UN starts with Elim
						If api.Data.GetDataCell("A#951L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
							'api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#700000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
							api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None + A#953L:F#INC:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None + A#953L:F#None:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#700000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
							api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#600000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						Else
							'api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = -A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#600000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
							api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = -A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None -A#953L:F#INC:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None -A#953L:F#None:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#600000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
							api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#700000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						End If
						api.Data.Calculate("A#951L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#951L:U4#Elim_G:U1#"+UN.Name.ToString+"")
						api.Data.Calculate("A#953L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#953L:U4#Elim_G:U1#"+UN.Name.ToString+"")
					End	If	
				Else 'if UN = Elim_DN
					If api.Data.GetDataCell("A#951L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
						'api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None =  A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None + A#952L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						'api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None =  A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None + A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None =  A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None + A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None + A#953L:F#INC:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None + A#953L:F#None:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#600000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#700000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					Else
						'api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None -A#952L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						'api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None -A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None -A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None -A#953L:F#INC:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None -A#953L:F#None:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#700000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#600000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					End If
					api.Data.Calculate("A#951L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#951L:U4#Elim_G:U1#"+UN.Name.ToString+"")
					api.Data.Calculate("A#953L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#953L:U4#Elim_G:U1#"+UN.Name.ToString+"")
				End If	
			Next
			
	'xxxxxxxx
			'Entes XXX
			Dim UNBaseENTE As List(Of Member) = api.Members.GetBaseMembers(api.Pov.UD1Dim.DimPk, api.Members.GetMemberId(dimtype.UD1.Id, "ENTES"))
			For Each UN As Member In UNBaseENTE

				If UN.Name.ToString.substring(0,4).equals("Elim") Then
					If api.Data.GetDataCell("A#940L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#940L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					Else
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = -A#940L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					End If
					api.Data.Calculate("A#940L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#940L:U4#Elim_G:U1#"+UN.Name.ToString+"")

					If api.Data.GetDataCell("A#952L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
					'xx	api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None =  A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None + A#952L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None =  A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None + A#952L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
					'xx	api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					Else
					'xx	api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None -A#952L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None -A#952L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
					'xx	api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					End If
					api.Data.Calculate("A#952L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#952L:U4#Elim_G:U1#"+UN.Name.ToString+"")
				End	If	

			'	951L + 953L
				If Not UN.Name.ToString.equals("Elim_ENTES") Then
					If UN.Name.ToString.substring(0,4).equals("Elim") Then
						If api.Data.GetDataCell("A#951L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
						'xx	api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#700000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
							api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None + A#953L:F#INC:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None + A#953L:F#None:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#700000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
							api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#600000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						Else
						'xx	api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = -A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#600000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
							api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = -A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None -A#953L:F#INC:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None -A#953L:F#None:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#600000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
							api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#700000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						End If
						api.Data.Calculate("A#951L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#951L:U4#Elim_G:U1#"+UN.Name.ToString+"")
						api.Data.Calculate("A#953L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#953L:U4#Elim_G:U1#"+UN.Name.ToString+"")
					End	If	
				End If	
			Next
			'XXXXXXX
			
			'MERCADO XXX
			Dim UNBaseMC As List(Of Member) = api.Members.GetBaseMembers(api.Pov.UD1Dim.DimPk, api.Members.GetMemberId(dimtype.UD1.Id, "MC"))
			For Each UN As Member In UNBaseMC
				
				'940L + 952L
				If UN.Name.ToString.substring(0,4).equals("Elim") Then  'If UN starts with Elim
					If api.Data.GetDataCell("A#940L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#940L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					Else
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = -A#940L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					End If
					api.Data.Calculate("A#940L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#940L:U4#Elim_G:U1#"+UN.Name.ToString+"")

					If api.Data.GetDataCell("A#952L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
					
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None =  A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None + A#952L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
						
					Else
						
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None -A#952L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
						
					End If
					api.Data.Calculate("A#952L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#952L:U4#Elim_G:U1#"+UN.Name.ToString+"")
				End	If	

				'951L + 953L
				If Not UN.Name.ToString.equals("Elim_MC") Then
					If UN.Name.ToString.substring(0,4).equals("Elim")  Then
						If api.Data.GetDataCell("A#951L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
								
							api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None + A#953L:F#INC:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None + A#953L:F#None:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#700000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
							api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#600000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						
						Else
								
							api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = -A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None -A#953L:F#INC:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None -A#953L:F#None:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None - A#600000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
							api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#700000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						End If
					api.Data.Calculate("A#951L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#951L:U4#Elim_G:U1#"+UN.Name.ToString+"")
					api.Data.Calculate("A#953L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#953L:U4#Elim_G:U1#"+UN.Name.ToString+"")
					End If
				Else 'if UN = Elim_MC
					If api.Data.GetDataCell("A#951L:C#Local:O#Top:I#Top:F#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None").CellAmount > 0 Then
						
						api.Data.Calculate("A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None =  A#CR_768000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None + A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None + A#953L:F#INC:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None + A#953L:F#None:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#600000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#700000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					Else
						
						api.Data.Calculate("A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = A#CR_668000:F#None:I#None:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None -A#951L:F#Top:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None -A#953L:F#INC:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None -A#953L:F#None:I#Top:U1#"+UN.Name.ToString.substring(5)+":U2#Top:U3#Top:U4#Elim_G:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#700000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#700000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#600000:F#None:I#E999:U1#"+UN.Name.ToString+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None = - A#600000:F#None:I#E999:U1#"+UN.Name.ToString.substring(5)+":U2#None:U3#None:U4#Ajus2:U5#None:U6#None:U7#None:U8#None")
					End If
					api.Data.Calculate("A#951L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#951L:U4#Elim_G:U1#"+UN.Name.ToString+"")
					api.Data.Calculate("A#953L:U4#Ajus2:U1#"+UN.Name.ToString+" = - A#953L:U4#Elim_G:U1#"+UN.Name.ToString+"")
				End If	
			Next
	'xxxxxxxx	
			
		api.Data.Calculate("A#CV002:UD4#Ajus2=A#700000:UD4#Ajus2")	
		End If
	End Sub		
#End Region

#Region "Member Formulas for French Gaap -> IFRS"
'	Dim FSK As New OneStream.BusinessRule.Finance.XFW_FSK_MemberFormulas.MainClass
'	FSK.AcctXXX(si, api, globals, args)
	
	Sub RETAUTO01 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)

		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO01"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity() Then
			api.Data.ClearCalculatedData("A#" & Acct.ResR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			api.Data.Calculate("A#201000:O#AdjInput" & UDDest & "=-A#201000:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.REC.name & ":O#AdjInput" & UDDest & "=-A#201000:F#" & Flow.REC.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.FUS.name & ":O#AdjInput" & UDDest & "=-A#201000:F#" & Flow.FUS.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#ENT:O#AdjInput" & UDDest & "=-A#201000:F#ENT:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.ERR.name & ":O#AdjInput" & UDDest & "=-A#201000:F#" & Flow.ERR.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#IREVA:O#AdjInput" & UDDest & "=-A#201000:F#IREVA:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#DREVA:O#AdjInput" & UDDest & "=-A#201000:F#DREVA:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#658000:F#None:I#None:O#AdjInput" & UDDest & "=A#201000:F#" & Flow.AUG.name & ":O#BeforeAdj" & UDSource & "-A#201000:F#" & Flow.DIMI.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.OUV.name & "" & UDDest & "-A#" & Acct.ResR.name & ":F#ENT" & UDDest)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.RES.name & ":I#None" & UDDest & "=A#RESULTAT:F#None:I#None" & UDDest)
			api.Data.Calculate("F#" & Flow.CLO.name & "" & UDDest & "=F#" & Flow.TF.name & "" & UDDest)

		End If
	End Sub

	Sub RETAUTO02 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)

		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO02"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity()  Then
			api.Data.ClearCalculatedData("A#" & Acct.ResR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			api.Data.ClearCalculatedData("A#" & Acct.RsvR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			
			api.Data.Calculate("A#280100:O#AdjInput" & UDDest & "=-A#280100:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.REC.name & ":O#AdjInput" & UDDest & "=-A#280100:F#" & Flow.REC.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.FUS.name & ":O#AdjInput" & UDDest & "=-A#280100:F#" & Flow.FUS.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#ENT:O#AdjInput" & UDDest & "=-A#280100:F#ENT:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.ERR.name & ":O#AdjInput" & UDDest & "=A#280100:F#" & Flow.ERR.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#681110:F#None:O#AdjInput" & UDDest & "=-A#280100:F#" & Flow.AUG.name & ":O#BeforeAdj" & UDSource & "+A#280100:F#" & Flow.DIMI.name & ":O#BeforeAdj" & UDSource & "+A#280100:F#REXP:O#BeforeAdj" & UDSource & "-A#280100:F#DEPA:O#BeforeAdj" & UDSource & "+A#280100:F#DEPD:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.OUV.name & "" & UDDest & "-A#" & Acct.ResR.name & ":F#ENT" & UDDest)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.RES.name & ":I#None" & UDDest & "=A#RESULTAT:F#None:I#None" & UDDest)
			api.Data.Calculate("F#" & Flow.CLO.name & "" & UDDest & "=F#" & Flow.TF.name & "" & UDDest)
		End If	
	End Sub
	
	Sub RETAUTO03 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)

		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO03"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity()  Then
			api.Data.ClearCalculatedData("A#" & Acct.ResR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			api.Data.ClearCalculatedData("A#" & Acct.RsvR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			api.Data.Calculate("A#476000:O#AdjInput" & UDDest & "=-A#476000:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.REC.name & ":O#AdjInput" & UDDest & "=-A#476000:F#" & Flow.REC.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.FUS.name & ":O#AdjInput" & UDDest & "=-A#476000:F#" & Flow.FUS.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#ENT:O#AdjInput" & UDDest & "=-A#476000:F#ENT:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#666000:F#None:O#AdjInput" & UDDest & "=A#476000:F#MVT:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.OUV.name & "" & UDDest & "-A#" & Acct.ResR.name & ":F#ENT" & UDDest)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.RES.name & ":I#None" & UDDest & "=A#RESULTAT:F#None:I#None" & UDDest)
			api.Data.Calculate("F#" & Flow.CLO.name & "" & UDDest & "=F#" & Flow.TF.name & "" & UDDest)

		End If
	End Sub

	Sub RETAUTO04 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)

		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO04"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity()  Then
			api.Data.ClearCalculatedData("A#" & Acct.ResR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			api.Data.Calculate("A#142000:O#AdjInput" & UDDest & "=-A#142000:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.REC.name & ":O#AdjInput" & UDDest & "=A#142000:F#" & Flow.REC.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.FUS.name & ":O#AdjInput" & UDDest & "=A#142000:F#" & Flow.FUS.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#ENT:O#AdjInput" & UDDest & "=A#142000:F#ENT:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.ERR.name & ":O#AdjInput" & UDDest & "=A#142000:F#" & Flow.ERR.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#787200:F#None:O#AdjInput" & UDDest & "=-A#142000:F#" & Flow.DIMI.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#687200:F#None:O#AdjInput" & UDDest & "=-A#142000:F#" & Flow.AUG.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.OUV.name & "" & UDDest & "-A#" & Acct.ResR.name & ":F#ENT" & UDDest)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.RES.name & ":I#None" & UDDest & "=A#RESULTAT:F#None:I#None" & UDDest)
			api.Data.Calculate("F#" & Flow.CLO.name & "" & UDDest & "=F#" & Flow.TF.name & "" & UDDest)
		End If
	End Sub

	Sub RETAUTO05 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)

		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO05"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity()  Then
			api.Data.ClearCalculatedData("A#" & Acct.ResR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			api.Data.Calculate("A#143100:O#AdjInput" & UDDest & "=-A#143100:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.REC.name & ":O#AdjInput" & UDDest & "=A#143100:F#" & Flow.REC.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.FUS.name & ":O#AdjInput" & UDDest & "=A#143100:F#" & Flow.FUS.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#ENT:O#AdjInput" & UDDest & "=A#143100:F#ENT:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.ERR.name & ":O#AdjInput" & UDDest & "=A#143100:F#" & Flow.ERR.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#787200:F#None:O#AdjInput" & UDDest & "=-A#143100:F#" & Flow.DIMI.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#687200:F#None:O#AdjInput" & UDDest & "=-A#143100:F#" & Flow.AUG.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.OUV.name & "" & UDDest & "-A#" & Acct.ResR.name & ":F#ENT" & UDDest)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.RES.name & ":I#None" & UDDest & "=A#RESULTAT:F#None:I#None" & UDDest)
			api.Data.Calculate("F#" & Flow.CLO.name & "" & UDDest & "=F#" & Flow.TF.name & "" & UDDest)
		End If
	End Sub	

	Sub RETAUTO06 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)

		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO06"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity()  Then
			api.Data.ClearCalculatedData("A#" & Acct.ResR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			api.Data.Calculate("A#145000:O#AdjInput" & UDDest & "=-A#145000:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.REC.name & ":O#AdjInput" & UDDest & "=A#145000:F#" & Flow.REC.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.FUS.name & ":O#AdjInput" & UDDest & "=A#145000:F#" & Flow.FUS.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#ENT:O#AdjInput" & UDDest & "=A#145000:F#ENT:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.ERR.name & ":O#AdjInput" & UDDest & "=A#145000:F#" & Flow.ERR.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#787200:F#None:O#AdjInput" & UDDest & "=-A#145000:F#" & Flow.DIMI.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#687200:F#None:O#AdjInput" & UDDest & "=-A#145000:F#" & Flow.AUG.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.OUV.name & "" & UDDest & "-A#" & Acct.ResR.name & ":F#ENT" & UDDest)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.RES.name & ":I#None" & UDDest & "=A#RESULTAT:F#None:I#None" & UDDest)
			api.Data.Calculate("F#" & Flow.CLO.name & "" & UDDest & "=F#" & Flow.TF.name & "" & UDDest)
		End If
	End Sub

	Sub RETAUTO07 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)

		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO07"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity()  Then
			api.Data.ClearCalculatedData("A#" & Acct.ResR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			api.Data.Calculate("A#146000:O#AdjInput" & UDDest & "=-A#146000:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.REC.name & ":O#AdjInput" & UDDest & "=A#146000:F#" & Flow.REC.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.FUS.name & ":O#AdjInput" & UDDest & "=A#146000:F#" & Flow.FUS.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#ENT:O#AdjInput" & UDDest & "=A#146000:F#ENT:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.ERR.name & ":O#AdjInput" & UDDest & "=A#146000:F#" & Flow.ERR.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#787200:F#None:O#AdjInput" & UDDest & "=-A#146000:F#" & Flow.DIMI.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#687200:F#None:O#AdjInput" & UDDest & "=-A#146000:F#" & Flow.AUG.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.OUV.name & "" & UDDest & "-A#" & Acct.ResR.name & ":F#ENT" & UDDest)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.RES.name & ":I#None" & UDDest & "=A#RESULTAT:F#None:I#None" & UDDest)
			api.Data.Calculate("F#" & Flow.CLO.name & "" & UDDest & "=F#" & Flow.TF.name & "" & UDDest)
		End If
	End Sub

	Sub RETAUTO08 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)

		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO08"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity()  Then
			api.Data.ClearCalculatedData("A#" & Acct.ResR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			api.Data.Calculate("A#147000:O#AdjInput" & UDDest & "=-A#147000:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.REC.name & ":O#AdjInput" & UDDest & "=A#147000:F#" & Flow.REC.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.FUS.name & ":O#AdjInput" & UDDest & "=A#147000:F#" & Flow.FUS.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#ENT:O#AdjInput" & UDDest & "=A#147000:F#ENT:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.ERR.name & ":O#AdjInput" & UDDest & "=A#147000:F#" & Flow.ERR.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#787200:F#None:O#AdjInput" & UDDest & "=-A#147000:F#" & Flow.DIMI.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#687200:F#None:O#AdjInput" & UDDest & "=-A#147000:F#" & Flow.AUG.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.OUV.name & "" & UDDest & "-A#" & Acct.ResR.name & ":F#ENT" & UDDest)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.RES.name & ":I#None" & UDDest & "=A#RESULTAT:F#None:I#None" & UDDest)
			api.Data.Calculate("F#" & Flow.CLO.name & "" & UDDest & "=F#" & Flow.TF.name & "" & UDDest)
		End If
	End Sub

	Sub RETAUTO09 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)

		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO09"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity()  Then
			api.Data.ClearCalculatedData("A#" & Acct.ResR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			api.Data.Calculate("A#148100:O#AdjInput" & UDDest & "=-A#148100:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.REC.name & ":O#AdjInput" & UDDest & "=A#148100:F#" & Flow.REC.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.FUS.name & ":O#AdjInput" & UDDest & "=A#148100:F#" & Flow.FUS.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#ENT:O#AdjInput" & UDDest & "=A#148100:F#ENT:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.ERR.name & ":O#AdjInput" & UDDest & "=A#148100:F#" & Flow.ERR.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#787200:F#None:O#AdjInput" & UDDest & "=-A#148100:F#" & Flow.DIMI.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#687200:F#None:O#AdjInput" & UDDest & "=-A#148100:F#" & Flow.AUG.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.OUV.name & "" & UDDest & "-A#" & Acct.ResR.name & ":F#ENT" & UDDest)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.RES.name & ":I#None" & UDDest & "=A#RESULTAT:F#None:I#None" & UDDest)
			api.Data.Calculate("F#" & Flow.CLO.name & "" & UDDest & "=F#" & Flow.TF.name & "" & UDDest)
		End If
	End Sub

	Sub RETAUTO10 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)

		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO10"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity()  Then
			api.Data.ClearCalculatedData("A#" & Acct.ResR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			api.Data.Calculate("A#148200:O#AdjInput" & UDDest & "=-A#148200:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.REC.name & ":O#AdjInput" & UDDest & "=A#148200:F#" & Flow.REC.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.FUS.name & ":O#AdjInput" & UDDest & "=A#148200:F#" & Flow.FUS.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#ENT:O#AdjInput" & UDDest & "=A#148200:F#ENT:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.ERR.name & ":O#AdjInput" & UDDest & "=A#148200:F#" & Flow.ERR.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#787200:F#None:O#AdjInput" & UDDest & "=-A#148200:F#" & Flow.DIMI.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#687200:F#None:O#AdjInput" & UDDest & "=-A#148200:F#" & Flow.AUG.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.OUV.name & "" & UDDest & "-A#" & Acct.ResR.name & ":F#ENT" & UDDest)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.RES.name & ":I#None" & UDDest & "=A#RESULTAT:F#None:I#None" & UDDest)
			api.Data.Calculate("F#" & Flow.CLO.name & "" & UDDest & "=F#" & Flow.TF.name & "" & UDDest)
		End If
	End Sub

	Sub RETAUTO11 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)

		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO11"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity()  Then
			api.Data.ClearCalculatedData("A#" & Acct.ResR.name & ":F#" & Flow.ERR.name & "" & UDDest,True,True,True)
			api.Data.Calculate("A#151500:O#AdjInput" & UDDest & "=-A#151500:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.REC.name & ":O#AdjInput" & UDDest & "=A#151500:F#" & Flow.REC.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.FUS.name & ":O#AdjInput" & UDDest & "=A#151500:F#" & Flow.FUS.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#ENT:O#AdjInput" & UDDest & "=A#151500:F#ENT:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.ERR.name & ":O#AdjInput" & UDDest & "=A#151500:F#" & Flow.ERR.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#786510:F#None:O#AdjInput" & UDDest & "=-A#151500:F#RFIN:O#BeforeAdj" & UDSource & "-A#151500:F#REXP:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#686510:F#None:O#AdjInput" & UDDest & "=-A#151500:F#DFIN:O#BeforeAdj" & UDSource & "-A#151500:F#DEXP:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.OUV.name & "" & UDDest & "-A#" & Acct.ResR.name & ":F#ENT" & UDDest)
			api.Data.Calculate("A#" & Acct.RsvR.name & ":F#" & Flow.AFF.name & "" & UDDest & "=-A#" & Acct.ResR.name & ":F#" & Flow.AFF.name & "" & UDDest)
			api.Data.Calculate("A#" & Acct.ResR.name & ":F#" & Flow.RES.name & ":I#None" & UDDest & "=A#RESULTAT:F#None:I#None" & UDDest)
			api.Data.Calculate("F#" & Flow.CLO.name & "" & UDDest & "=F#" & Flow.TF.name & "" & UDDest)
		End If
	End Sub

	Sub RETAUTO12_1 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)

		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO12"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity() Then
			api.Data.Calculate("A#130001:O#AdjInput" & UDDest & "=-A#130001:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#487001:I#None:O#AdjInput" & UDDest & "=A#130001:O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#487001:F#MVT:I#None:O#AdjInput" & UDDest & "=A#130001:F#" & Flow.AUG.name & ":O#BeforeAdj" & UDSource & "-A#130001:F#" & Flow.DIMI.name & ":O#BeforeAdj" & UDSource & "+A#130001:F#" & Flow.ERR.name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("F#" & Flow.CLO.name & "" & UDDest & "=F#" & Flow.TF.name & "" & UDDest)
		End If
	End Sub

	Sub RETAUTO12_0 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
		
		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO12"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity() Then
			api.Data.Calculate("A#" & Acct.SubInvNC.Name & ":O#AdjInput" & UDDest & "=-A#" & Acct.SubInvNC.Name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.DefIncNC.Name & ":I#None:O#AdjInput" & UDDest & "=A#" & Acct.SubInvNC.Name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.DefIncNC.Name & ":F#" & Flow.MVT.Name & ":I#None:O#AdjInput" & UDDest & "=A#" & Acct.SubInvNC.Name & ":F#" & Flow.Aug.Name & ":O#BeforeAdj" & UDSource & "-A#" & Acct.SubInvNC.Name & ":F#" & Flow.Dimi.Name & ":O#BeforeAdj" & UDSource & "+A#" & Acct.SubInvNC.Name & ":F#" & Flow.ERR.Name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("F#" & Flow.Clo.Name & "" & UDDest & "=F#" & Flow.TF.Name & "" & UDDest)
		End If
		
	End Sub

	Sub RETAUTO13 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim UDSource As String 
		Dim UDDest As String
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		Dim Nat As Object = Me.InitNatureClass(si, api, globals, dimtype.UD4)
		
		UDSource=":" & Nat.NatDimType.Abbrev & "#None"
		UDDest=":" & Nat.NatDimType.Abbrev & "#RETAUTO13"

		If Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyForEntity() Then
			api.Data.ClearCalculatedData("A#" & Acct.ResR.Name & ":F#" & Flow.ERR.Name & "" & UDDest,True,True,True)
			api.Data.ClearCalculatedData("A#" & Acct.RsvR.Name & ":F#" & Flow.ERR.Name & "" & UDDest,True,True,True)
			api.Data.Calculate("A#" & Acct.AdjCT.Name & ":O#AdjInput" & UDDest & "=-A#" & Acct.AdjCT.Name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.Name & ":F#" & Flow.Rec.Name & ":O#AdjInput" & UDDest & "=A#" & Acct.AdjCT.Name & ":F#" & Flow.Rec.Name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.Name & ":F#" & Flow.FUS.Name & ":O#AdjInput" & UDDest & "=A#" & Acct.AdjCT.Name & ":F#" & Flow.FUS.Name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.RsvR.Name & ":F#" & Flow.Ent.Name & ":O#AdjInput" & UDDest & "=A#" & Acct.AdjCT.Name & ":F#" & Flow.Ent.Name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ProfitCT.Name & ":F#None:O#AdjInput" & UDDest & "=A#" & Acct.AdjCT.Name & ":F#" & Flow.MVT.Name & ":O#BeforeAdj" & UDSource)
			api.Data.Calculate("A#" & Acct.ResR.Name & ":F#" & Flow.Aff.Name & "" & UDDest & "=-A#" & Acct.ResR.Name & ":F#" & Flow.Ouv.Name & UDDest & "-A#" & Acct.ResR.Name & ":F#" & Flow.Ent.Name & "" & UDDest)
			api.Data.Calculate("A#" & Acct.RsvR.Name & ":F#" & Flow.Aff.Name & "" & UDDest & "=-A#" & Acct.ResR.Name & ":F#" & Flow.Aff.Name & UDDest)
			api.Data.Calculate("A#" & Acct.ResR.Name & ":F#" & Flow.Res.Name & ":I#None" & UDDest & "=A#RESULTAT:F#None:I#None" & UDDest)
			api.Data.Calculate("F#" & Flow.Clo.Name & UDDest & "=F#" & Flow.TF.Name & UDDest)
		End If
	End Sub	
	
#End Region


#Region "Other Calcs"
	Sub ExitedEntityRollForward (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals)

		If Not(api.Pov.Cons.MemberId = ConsMemberId.Elimination Or api.Pov.Cons.MemberId = ConsMemberId.Share) Then
			' Roll forward the deconsolidation data
			Dim Entity As String = api.Pov.Entity.Name
			Dim Period As String = api.Pov.Time.Name
			Dim methodD As DataCell = api.Data.GetDataCell("E#" & Entity & ":C#Local:T#" &  Period  & ":V#YTD:A#Methode:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None")
			Dim method As Integer = methodD.CellAmount
			If method = FSKMethod.SORTANTE And Not ( api.Time.IsFirstPeriodInYear) Then
				Dim methodPPD As DataCell = api.Data.GetDataCell("E#" & Entity & ":C#Local:T#PovPrior1:V#YTD:A#Methode:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None")
				Dim methodPP As Integer = methodPPD.CellAmount
				If methodPP = FSKMethod.SORTANTE Then
					api.Data.Calculate("A#All:T#" & Period & " = A#All:T#" & api.Time.GetPriorPeriod())
				End If
			End If	
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
					Return Flow
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

				End If
				
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Function
		
		Private Function InitNatureClass (ByRef Si As SessionInfo, ByRef Api As FinanceRulesApi, ByVal Globals As BRGlobals, ByVal oDimType As DimType) As Object
			Try
				Dim sGName As String = "FSKNature"
				Dim Nature As Object = globals.GetObject(sGName)
				If Not Nature Is Nothing Then
					Return Nature
				Else
					Dim FSKM As New OneStream.BusinessRule.Finance.XFW_FSK_ConsolidationRules.MainClass	
					Nature = FSKM.InitNatureClass(Si, Api,Globals, api.Pov.UD4Dim.DimPk)
					Return Nature
				End If
				
			Catch ex As Exception
				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Function

#End Region

	End Class

#Region "FXRate Class"
	Public Class globalClass
		
		Public closingRateToText As String
		Public averageRateToText As String
		Public averageRatePriorYEtoText As String
		Public closingRatePriorYEtoText As String
		Public closingRatePriorYEtoText2 As String
		Public OpeScenario As String
		Sub New (ByRef oSi As SessionInfo, ByRef oApi As FinanceRulesApi, ByVal oArgs As FinanceRulesArgs)
			Dim referenceBR As New OneStream.BusinessRule.Finance.XFW_FSK_OpeningScenario.MainClass	
			Dim opeScenario As String = referenceBR.fctOpeningScenario(oSi, oApi, oArgs)
			Dim timeId As Integer = oapi.Pov.Time.MemberPk.MemberId
			Dim timeprioryearend As Integer = oapi.Time.GetLastPeriodInPriorYear
			Dim timeprioryearend2 As Integer = oapi.Time.GetLastPeriodInPriorYear(timeprioryearend)
		    Dim rateTypeClo As FxRateType = oapi.FxRates.GetFxRateTypeForAssetLiability()
			Dim rateTypeAve As FxRateType = oapi.FxRates.GetFxRateTypeForRevenueExp()
		    Dim closingRate As Decimal =  oapi.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)			
			Dim AverageRate As Decimal =  oapi.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
			Dim openingScenarioId As Integer = oapi.Members.GetMember(DimType.Scenario.Id, OpeScenario).MemberId
			Dim ClosingRateTypePriorYE As FxRateType = oapi.Scenario.GetFxRateTypeForAssetLiability(openingScenarioId)
			Dim AverageRateTypePriorYE As FxRateType = oapi.Scenario.GetFxRateTypeForRevenueExpense(openingScenarioId)		
			Dim closingRatePriorYE As Decimal = oapi.FxRates.GetCalculatedFxRate(ClosingRateTypePriorYE,timeprioryearend)
			Dim AverageRatePriorYE As Decimal = oapi.FxRates.GetCalculatedFxRate(AverageRateTypePriorYE,timeprioryearend)
			Dim closingRatePriorYE2 As Decimal = oapi.FxRates.GetCalculatedFxRate(ClosingRateTypePriorYE,timeprioryearend2)
			
			closingRateToText = closingRate.ToString("G",CultureInfo.InvariantCulture)
			averageRateToText = AverageRate.ToString("G",CultureInfo.InvariantCulture)	
			averageRatePriorYEtoText = AverageRatePriorYE.ToString("G",CultureInfo.InvariantCulture)
			closingRatePriorYEtoText = ClosingRatePriorYE.ToString("G",CultureInfo.InvariantCulture)
			closingRatePriorYEtoText2 = ClosingRatePriorYE2.ToString("G",CultureInfo.InvariantCulture)
		End Sub
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



End Namespace