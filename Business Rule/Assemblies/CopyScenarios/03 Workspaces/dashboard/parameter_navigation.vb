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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.parameter_navigation
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		'Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					

					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						If args.FunctionName.XFEqualsIgnoreCase("LoadDashboard") Then
							
							
							
							Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
							loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = True
							

							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize _
								And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CopyScenario_Time") = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "prm_CopyScenario_Time")
							End If
							
							
							Return loadDashboardTaskResult
							
						End If
						


					
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		

		
	End Class
End Namespace
