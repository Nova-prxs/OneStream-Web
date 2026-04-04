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
'------------------------------------------------------------------------------------------------------------
'Reference Code: 	OP_Entity_MemberList
'
'Description:		Use a business rule to return Operative of first and second year Entities for Opening process
'
'Usage:				Use the following on the cube view or parameter  E#Member.[Name of Business Rule, Name of List in Business Rule]
'
'Created By:
'
'Date Created:		25/07/2024
'------------------------------------------------------------------------------------------------------------	

Namespace OneStream.BusinessRule.Finance.OP_Entity_MemberList
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try				
		
				Select Case api.FunctionType 
							
					Case Is = FinanceFunctionType.MemberListHeaders
						Dim objMemberListHeaders As New List(Of MemberListHeader)
						objMemberListHeaders.Add(New MemberListHeader("OP_List"))
						objMemberListHeaders.Add(New MemberListHeader("OP_Mirrors"))
						Return objMemberListHeaders
						
					Case Is = FinanceFunctionType.MemberList
						
						
			            If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("OP_List")Then

							Dim sEntityPK As DimPk = api.Pov.EntityDim.DimPk
							Dim lstResults As New List(Of MemberInfo)
		                    Dim objMemberListHeader = New MemberListHeader(args.MemberListArgs.MemberListName)
		                	Dim objMembers As MemberInfo = Nothing
							Dim oEntityDimPK As DimPk = api.Pov.EntityDim.DimPk
							Dim sBrand As String = args.MemberListArgs.NameValuePairs("Op_Brand")
							Dim sScenario As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "OP_prm_CurrScenario") 
							Dim sYear As String = args.MemberListArgs.NameValuePairs("Op_Year")
							BRApi.ErrorLog.LogMessage(si, sYear)
'							Get parameters
'							If sScenario.XFEqualsIgnoreCase("Forecast") Then
'								sYear = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Actual_Year")
'							Else
'								sYear = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Budget_Year")
'							End If
'							brapi.ErrorLog.LogMessage(si, "sScenario: " & sScenario)
							Dim lstBaseEntMemInfo As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(oEntityDimPK, $"E#{sBrand}.Base", Nothing)
							Dim iTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id, sYear)
							
							 For Each objEntity As MemberInfo In lstBaseEntMemInfo
								Dim sEntText1 As String = BRApi.Finance.Entity.Text(si, objEntity.Member.MemberId,1,0,iTimeId)
'								If (objEntity.Member.Name.Equals("SF224635"))
'		                    		brapi.ErrorLog.LogMessage(si, "Entidad " & sEntText1)
'								End If
								If sEntText1.XFEqualsIgnoreCase ("Operativas Segundo Año") Or sEntText1.XFEqualsIgnoreCase ("Operativas Primer Año") Then
									
									objMembers = New MemberInfo (api.Members.GetMember(DimType.Entity.Id, objEntity.Member.MemberId))
									lstResults.Add(Objmembers)
				                    'End If
								End If
			                    
							Next

							Dim objMemberList As New MemberList(objMemberListHeader, lstResults)
							Return objMemberList
			            
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("OP_Mirrors") Then
							
							Dim oMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim sBrand As String = args.MemberListArgs.NameValuePairs("Op_Brand")
							Dim sYear As String = args.MemberListArgs.NameValuePairs("Op_Year")
							Dim oEntDimPK As DimPk = api.Dimensions.GetDim("Entities").DimPk
							Dim oEntMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(oEntDimPK, $"E#{sBrand}.Base", Nothing)
							Dim lstMemberInfos As New List(Of MemberInfo)
							
							Dim iTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id, sYear)
							
							Dim miEntOp As MemberInfo
							For Each miEntity As MemberInfo In oEntMemberInfos
								Dim sEntText1 As String = BRApi.Finance.Entity.Text(si, miEntity.Member.MemberId,1,0,iTimeId)
								
								If(sEntText1.XFEqualsIgnoreCase ("Comparables") Or sEntText1.XFEqualsIgnoreCase ("Reformas") Or sEntText1.XFEqualsIgnoreCase ("Reformas Año Anterior")) Then	
									miEntOp = New MemberInfo(api.Members.GetMember(dimtype.Entity.Id, miEntity.Member.MemberId))
									lstMemberInfos.Add(miEntOp)
									'BRApi.ErrorLog.LogMessage(si,"Entity: " & miEntityComp.Member.Name)
								End If
								
							Next
							
							Dim oMemberList As New MemberList(oMemberListHeader, lstMemberInfos)
							Return oMemberList
						
						End If
						
						
				End Select
				Return Nothing

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace