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

Namespace OneStream.BusinessRule.Finance.Conditional_Input
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.ConditionalInput
					Dim boolinputactive As Boolean = True
					
					If api.Members.IsDescendant(api.Pov.ScenarioDim.DimPk, api.Members.GetMemberId(dimtype.Scenario.Id, "BUDGET_GROUP"), api.Pov.Scenario.MemberId) Or 
					api.Members.IsDescendant(api.Pov.ScenarioDim.DimPk, api.Members.GetMemberId(dimtype.Scenario.Id, "Q_FCST_GROUP"), api.Pov.Scenario.MemberId) Or 
					api.Members.IsDescendant(api.Pov.ScenarioDim.DimPk, api.Members.GetMemberId(dimtype.Scenario.Id, "APPROVED_BM_GROUP"), api.Pov.Scenario.MemberId) Or 
					api.Members.IsDescendant(api.Pov.ScenarioDim.DimPk, api.Members.GetMemberId(dimtype.Scenario.Id, "5YP"), api.Pov.Scenario.MemberId)
					
					
						If boolinputactive And
						api.Time.GetYearFromId >=2025 And 
						Not api.Entity.HasChildren And 
						api.Cons.IsLocalCurrencyForEntity And						
						(Not api.Members.IsDescendant(api.Pov.EntityDim.DimPk, api.Members.GetMemberId(dimtype.Entity.Id, "Switzerland"), api.Pov.Entity.MemberId) And 
						Not api.Pov.Entity.Name.XFContainsIgnoreCase("Switzerland"))
							'Text ACCOUNTS 1-8
'						(api.pov.Origin.Name.XFEqualsIgnoreCase("Forms") Or
'						api.pov.Origin.Name.XFEqualsIgnoreCase("Import")) And
							'OPTION 1:
'							Text1: U1#Top.Base.Where(Text3 <> Headquarters) 
'							Text1: Headquarter => Not FALSE => TRUE
'							Text1: Headquarter = "" => True
'							Text2: U2#Top.base.Remove(U2#19, U2#CCNF, U2#DINF, U2#TRNF, U2#TMNF, U2#LANF, U2#NMNF, U2#OUNF)
'							Text3: U3#UD3_DummyBucket, U3#None, U3#QA, U3#BL, U3#RM

							'OPTION 2a:
'							Text4: U1#Top.Base.Where(Text3 = Headquarters)
'							Text5: U2#19, U2#CCNF, U2#DINF, U2#TRNF, U2#TMNF, U2#LANF, U2#NMNF, U2#OUNF
'							Text6: U3#Top.Base.Remove(U3#None)

							'OPTION 2b:
'							Text7: U2#Top.base.Remove(U2#19, U2#CCNF, U2#DINF, U2#TRNF, U2#TMNF, U2#LANF, U2#NMNF, U2#OUNF)
'							Text8: U3#Top.Base.Remove(U3#NF)

'							Headquarters
							'U1#MB02:U2#02:U3#ST:U4#None:U5#UD1Default:U6#None:U7#UD1Default:U8#Pre_IFRS16_Plan
							Dim intACCmemid As Integer = api.Pov.Account.MemberId
							
'							Determine the current POV
							Dim intud1memid As Integer = api.Pov.UD1.MemberId
							Dim intud2memid As Integer = api.Pov.UD2.MemberId
							Dim intud3memid As Integer = api.Pov.UD3.MemberId
							
							Dim txt1UD1Headquarters As String = api.Account.Text(intACCmemid, 1)
							Dim txt2UD2Headquarters As String = api.Account.Text(intACCmemid, 2)
							Dim txt3UD3Headquarters As String = api.Account.Text(intACCmemid, 3)
							
							Dim txt4UD1MedCenter As String = api.Account.Text(intACCmemid, 4)
							Dim txt5UD2MedCenter As String = api.Account.Text(intACCmemid, 5)
							Dim txt6UD3MedCenter As String = api.Account.Text(intACCmemid, 6)
							Dim txt7UD2MedCenter As String = api.Account.Text(intACCmemid, 7)
							Dim txt8UD3MedCenter As String = api.Account.Text(intACCmemid, 8)
							
							'If there is NO UD1 TEXT3 (i.e. medical center or Headquarters) if it is empty no restrictions are applied.
							If api.UD1.Text(api.Pov.UD1.MemberId, 3) = Nothing Or 
							(txt1UD1Headquarters = Nothing  And 
							(txt4UD1MedCenter = Nothing Or txt4UD1MedCenter.XFEqualsIgnoreCase("PLAN_MOV") ))
'							(txt1UD1Headquarters = Nothing  And 
'							txt4UD1MedCenter = Nothing )
							
								Return ConditionalInputResultType.Default
								
							ElseIf api.Members.IsBase(api.Pov.AccountDim.DimPk,api.Members.GetMemberId(dimtype.Account.Id,"Non_Financial"),intACCmemid)
							
								Return ConditionalInputResultType.Default
								
							ElseIf api.Pov.IC.MemberId <> DimConstants.None

								Return ConditionalInputResultType.Default
								
							End If

							Dim txt1UD1HeadquartersList As List(Of Integer) =  BRApi.Finance.Members.GetMembersUsingFilter(si, api.Pov.UD1Dim.DimPk, txt1UD1Headquarters, True).Select(Function(X) X.Member.MemberId).tolist()
							Dim txt2UD2HeadquartersList As List(Of Integer) =  BRApi.Finance.Members.GetMembersUsingFilter(si, api.Pov.UD2Dim.DimPk, txt2UD2Headquarters, True).Select(Function(X) X.Member.MemberId).tolist()
							Dim txt3UD3HeadquartersList As List(Of Integer) =  BRApi.Finance.Members.GetMembersUsingFilter(si, api.Pov.UD3Dim.DimPk, txt3UD3Headquarters, True).Select(Function(X) X.Member.MemberId).tolist()

							Dim txt4UD1MedCenterList As List(Of Integer) =  BRApi.Finance.Members.GetMembersUsingFilter(si, api.Pov.UD1Dim.DimPk, txt4UD1MedCenter, True).Select(Function(X) X.Member.MemberId).tolist()
							Dim txt5UD2MedCenterList As List(Of Integer) =  BRApi.Finance.Members.GetMembersUsingFilter(si, api.Pov.UD2Dim.DimPk, txt5UD2MedCenter, True).Select(Function(X) X.Member.MemberId).tolist()
							Dim txt6UD3MedCenterList As List(Of Integer) =  BRApi.Finance.Members.GetMembersUsingFilter(si, api.Pov.UD3Dim.DimPk, txt6UD3MedCenter, True).Select(Function(X) X.Member.MemberId).tolist()

							Dim txt7UD2MedCenterList As List(Of Integer) =  BRApi.Finance.Members.GetMembersUsingFilter(si, api.Pov.UD2Dim.DimPk, txt7UD2MedCenter, True).Select(Function(X) X.Member.MemberId).tolist()
							Dim txt8UD3MedCenterList As List(Of Integer) =  BRApi.Finance.Members.GetMembersUsingFilter(si, api.Pov.UD3Dim.DimPk, txt8UD3MedCenter, True).Select(Function(X) X.Member.MemberId).tolist()

							Dim booltxt1UD1Headquarters As Boolean =  txt1UD1HeadquartersList.Find(Function(n) n = intud1memid)
							Dim booltxt2UD2Headquarters As Boolean =  txt2UD2HeadquartersList.Find(Function(n) n = intud2memid)
							Dim booltxt3UD3Headquarters As Boolean =  txt3UD3HeadquartersList.Find(Function(n) n = intud3memid)

							Dim booltxt4UD1MedCenter As Boolean =  txt4UD1MedCenterList.Find(Function(n) n = intud1memid)
							Dim booltxt5UD2MedCenter As Boolean =  txt5UD2MedCenterList.Find(Function(n) n = intud2memid)
							Dim booltxt6UD3MedCenter As Boolean =  txt6UD3MedCenterList.Find(Function(n) n = intud3memid)
							
							Dim booltxt7UD2MedCenter As Boolean =  txt7UD2MedCenterList.Find(Function(n) n = intud2memid)
							Dim booltxt8UD3MedCenter As Boolean =  txt8UD3MedCenterList.Find(Function(n) n = intud3memid)
''''							If api.Pov.UD1.Name.XFEqualsIgnoreCase("VL01") And 
''''							api.Pov.UD2.Name.XFEqualsIgnoreCase("19") And 
''''							api.Pov.UD3.Name.XFEqualsIgnoreCase("CA")
''''							 Api.LogMessage("Conditional Input: headquarters: " & booltxt1UD1Headquarters & " UD2: " & booltxt2UD2Headquarters & " UD3: " & booltxt3UD3Headquarters )
''''							End If
''''							Dim BoolOptions As Boolean =  (booltxt1UD1Headquarters And booltxt2UD2Headquarters And booltxt3UD3Headquarters) Or (booltxt4UD1MedCenter And booltxt5UD2MedCenter And booltxt6UD3MedCenter) Or (booltxt4UD1MedCenter And booltxt7UD2MedCenter And booltxt8UD3MedCenter)
							If Not booltxt1UD1Headquarters And 
							Not txt1UD1Headquarters.XFEqualsIgnoreCase("")
								If txt2UD2Headquarters.XFEqualsIgnoreCase("") And
							 	txt3UD3Headquarters.XFEqualsIgnoreCase("")
									Return ConditionalInputResultType.NoInput
								End If 
								If booltxt2UD2Headquarters Or
								booltxt3UD3Headquarters
									Return ConditionalInputResultType.NoInput
								Else
									Return ConditionalInputResultType.Default
								End If 
							ElseIf Not booltxt4UD1MedCenter And
							Not txt4UD1MedCenter.XFEqualsIgnoreCase("")
								'CHECK IF it is within the filter for the MEDICAL CENTER for both Text5 (UD2) or Text 
								If txt5UD2MedCenter.XFEqualsIgnoreCase("") And
							 	txt6UD3MedCenter.XFEqualsIgnoreCase("")
									Return ConditionalInputResultType.NoInput
								End If 
								If booltxt5UD2MedCenter Or 
								booltxt6UD3MedCenter
'''''									If booltxt7UD2MedCenter Or booltxt8UD3MedCenter
										Return ConditionalInputResultType.NoInput
'''''									End If
'''''								ElseIf txt5UD2MedCenter.XFEqualsIgnoreCase("") Or 
'''''								txt6UD3MedCenter.XFEqualsIgnoreCase("") 
'''''									If booltxt7UD2MedCenter Or 
'''''									booltxt8UD3MedCenter
'''''										Return ConditionalInputResultType.NoInput
'''''									End If
'''''									Return ConditionalInputResultType.Default
								End If
'								Return ConditionalInputResultType.Default
							End If
							Return ConditionalInputResultType.Default
						End If
					End If
				End Select

				Return ConditionalInputResultType.Default
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace