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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.AQS_CNT_DashboardExtender
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType

					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						
						Dim workspaceName As String = "AQS_CNT_Connectors"
						Dim isSystemLevel As Boolean = False
						Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, isSystemLevel, workspaceName)
							
						If 	args.FunctionName.XFEqualsIgnoreCase("SaveParameters_iScala") Then
							brapi.ErrorLog.LogMessage(si, "ASSEMBLIES IScala sListConnectors > " )
							'Save Admin parameters.
							'Get parameter values.
							Dim sListConnectors As String = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("MS_BL_ListConnectors_iScala_AQS_CNT")
							Dim sColumnNames As String = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("iV_ColumNames_iScala_AQS_CNT")
							Dim sSQL As String = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("iV_SQL_iScala_AQS_CNT")
'							Dim sActivateiScalaLog As String = args.NameValuePairs.XFGetValue("MS_DL_ActivateLog_iScala_AQS_CNT")
							
							brapi.ErrorLog.LogMessage(si, "ASSEMBLIES IScala sListConnectors > " & sListConnectors & "sColumnNames > " & sColumnNames &  "sSQL > " & sSQL)
'							brapi.ErrorLog.LogMessage(si, "ASSEMBLIESsActivateiScalaLog > " & sActivateiScalaLog)
							
							'Save parameters.
'							If sListConnectors.Length <> 0 Then BRAPi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "MS_BL_ListConnectors_AQS_CNT", sListConnectors)
'							If sColumnNames.Length <> 0 Then BRAPi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "IV_ColumNames_AQS_CNT", sColumnNames)
'							If sSQL.Length <> 0 Then BRAPi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "IV_SQL_AQS_CNT", sSQL)
							
							If sListConnectors.Length <> 0 Then BRAPi.Dashboards.Parameters.SetLiteralParameterValue(si, isSystemLevel, workspaceID,"MS_BL_ListConnectors_AQS_CNT", sListConnectors)
							If sColumnNames.Length <> 0 Then BRAPi.Dashboards.Parameters.SetLiteralParameterValue(si, isSystemLevel, workspaceID,"IV_ColumNames_AQS_CNT", sColumnNames)
							If sSQL.Length <> 0 Then BRAPi.Dashboards.Parameters.SetLiteralParameterValue(si, isSystemLevel, workspaceID,"IV_SQL_AQS_CNT", sSQL)
						
						
							
						ElseIf 	args.FunctionName.XFEqualsIgnoreCase("SaveParameters_SAP") Then
							'Save Admin parameters.
							'Get parameter values.
							Dim sListConnectors As String = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("MS_BL_ListConnectors_SAP_AQS_CNT")
							Dim sColumnNames As String = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("iV_ColumNames_SAP_AQS_CNT")
							Dim sSQL As String = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("iV_SQL_SAP_AQS_CNT")
							Dim sActivateSAPLog As String = args.NameValuePairs.XFGetValue("MS_DL_ActivateLog_SAP_AQS_CNT")							
							
							brapi.ErrorLog.LogMessage(si, "ASSEMBLIES SAP sListConnectors > " & sListConnectors & "sColumnNames > " & sColumnNames &  "sSQL > " & sSQL)
							
							'Save parameters.
							If sListConnectors.Length <> 0 Then BRAPi.Dashboards.Parameters.SetLiteralParameterValue(si, isSystemLevel, workspaceID, "MS_BL_ListConnectors_SAP_AQS_CNT", sListConnectors)
							If sColumnNames.Length <> 0 Then BRAPi.Dashboards.Parameters.SetLiteralParameterValue(si, isSystemLevel, workspaceID, "IV_ColumNames_SAP_AQS_CNT", sColumnNames)
							If sSQL.Length <> 0 Then BRAPi.Dashboards.Parameters.SetLiteralParameterValue(si, isSystemLevel, workspaceID, "IV_SQL_SAP_AQS_CNT", sSQL)	
							
						End If
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
