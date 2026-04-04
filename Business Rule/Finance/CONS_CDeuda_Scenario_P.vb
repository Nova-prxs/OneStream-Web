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

Namespace OneStream.BusinessRule.Finance.CONS_CDeuda_Scenario_P
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				
				Select Case api.FunctionType
																				
					Case Is = FinanceFunctionType.MemberList
						
						'Cadena que va en el Cube View
						'S#Root.CustomMemberList(BRName=CONS_CDeuda_Scenario_P, MemberListName=ScenarioTime):T#WFLastInYear	
						
						Dim sScenario As String = String.Empty									
																
						Dim DatoP11 = api.Data.GetDataCell("A#CR_Z:S#P11:T#POVLastInYear:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount
						Dim DatoP09 = api.Data.GetDataCell("A#CR_Z:S#P09:T#POVLastInYear:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount
						Dim DatoP06 = api.Data.GetDataCell("A#CR_Z:S#P06:T#POVLastInYear:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount
						Dim DatoP03 = api.Data.GetDataCell("A#CR_Z:S#P03:T#POVLastInYear:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount
		
						
						If DatoP11 <> 0 Then
							sScenario = "P11"
						Else If DatoP09 <> 0 Then
							sScenario = "P09"
						Else If DatoP06 <> 0 Then
							sScenario = "P06"
						Else 
							sScenario = "P03"
						End If																	
							
						sScenario = "S#" & sScenario
						
						Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)														
						Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, sScenario, Nothing)							
						Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)
							
						Return objMemberList
						
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
	End Class
	
End Namespace