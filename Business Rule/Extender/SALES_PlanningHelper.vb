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

Namespace OneStream.BusinessRule.Extender.SALES_PlanningHelper
	Public Class MainClass
		
		'Get SALES_SharedFunctions business rule
		Dim SalesSharedFunctionsBR As New OneStream.BusinessRule.Finance.SALES_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try

				'Get Workflow Cluster Pk to check if it's completed
				Dim isWfChecked As Boolean = IIf(BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk).GetOverallStatus.ToString = "Completed", True, False)
				
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						' Get function name
						Dim paramFunction As String = args.NameValuePairs.XFGetValue("p_function")
						
						Select paramFunction
						Case "ExecuteCalendarEffect"
						
							Dim sScenario As String = args.NameValuePairs.XFGetValue("p_scenario")
							Dim sYear As String = args.NameValuePairs.XFGetValue("p_year")
							Dim sBrand As String = args.NameValuePairs.XFGetValue("p_brand")
							Dim paramStep As String = args.NameValuePairs.XFGetValue("p_step")
							
							If Not Me.BrandSecurityAuthorization(si, sBrand) Then
								'Create a pop up to let the user know that he doesn's have access.
								Throw New Exception("You don't have access.")
							End If
							
							'Check if workflow is checked
							If isWfChecked Then
								'Create a pop up to let the user know that he doesn's have access.
								Throw New Exception("You can't modify the data, workflow is already checked.")
							End If
							
							SalesSharedFunctionsBR.ExecuteCalendarEffect(si, sScenario, sYear, sBrand, paramStep)
							
						Case Else
							Throw New Exception("The function doesn't exist.")
						End Select
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						'Add External Members
						Dim externalMembers As New List(Of NameValuePair)
						externalMembers.Add(New NameValuePair("YourMember1Name","YourMember1Value"))
						externalMembers.Add(New NameValuePair("YourMember2Name","YourMember2Value"))
						Return externalMembers
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Brand Security Authorization"
		
		Public Function BrandSecurityAuthorization(ByVal si As SessionInfo, ByVal ParamBrand As String) As Boolean
		
			Dim brandGroupDict As New Dictionary(Of String, String) From {
																			{"Burger King", "Burger King"},
																			{"Fridays", "Fridays"},
																			{"Domino''s Pizza", "Domino''s Pizza"},
																			{"Foster''s Hollywood", "Foster''s Hollywood"},
																			{"Foster''s Hollywood Valencia", "Foster''s Hollywood"},
																			{"Starbucks", "Starbucks España y Portugal"},
																			{"Starbucks Portugal", "Starbucks España y Portugal"},
																			{"Starbucks Bélgica", "Starbucks Francia, Holanda y Bélgica"},
																			{"Starbucks Francia", "Starbucks Francia, Holanda y Bélgica"},
																			{"Starbucks Holanda", "Starbucks Francia, Holanda y Bélgica"},
																			{"Ginos", "Ginos"},
																			{"Ginos Portugal", "Ginos"},
																			{"VIPS", "VIPS"},
																			{"All", "Global"}
																			}
			
			Return (BRApi.Security.Authorization.IsUserInGroup(si, si.UserName, brandGroupDict(ParamBrand), False) Or BRApi.Security.Authorization.IsUserInGroup(si, si.UserName, "Global", False))
			
		End Function
		
		#End Region
	End Class
End Namespace