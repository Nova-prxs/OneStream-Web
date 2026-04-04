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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.RES_extensibility_TemplateUploadMessage
	Public Class MainClass

		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType

					Case Is = ExtenderFunctionType.Unknown

					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						' Leer el parámetro paramFunction
						Dim paramFunction As String = args.NameValuePairs("p_function")
						Select Case paramFunction
							Case "ImportWorkOrders"
								Return Me.BuildWorkOrderWarningMessage(si, args)
							Case "ImportProjectPlanningTemplate"
								Return Me.BuildTemplateWarningMessage(si, args, "project")
							Case "ImportStructurePlanningTemplate"
								Return Me.BuildTemplateWarningMessage(si, args, "structure")
							' Aquí se pueden añadir más casos para otros tipos de importación
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

		Private Function BuildTemplateWarningMessage(ByVal si As SessionInfo, ByVal args As ExtenderArgs, ByVal importType As String) As String
			Try
				'Parse query params
				Dim paramQueryParams As String = args.NameValuePairs("p_query_params")
				Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, paramQueryParams)
				Dim paramEntity As String = parameterDict("paramEntity")
				Dim paramScenario As String = parameterDict("paramScenario")
				Dim paramYear As Integer = CInt(parameterDict("paramYear"))

				'Count warnings for this import
				Dim sql As String = "
					SELECT COUNT(*) AS warning_count
					FROM XFC_RES_AUX_template_warning
					WHERE entity = @paramEntity
					  AND scenario = @paramScenario
					  AND year = @paramYear
					  AND import_type = @importType"

				Dim dbParams As New List(Of DbParamInfo) From {
					New DbParamInfo("paramEntity", paramEntity),
					New DbParamInfo("paramScenario", paramScenario),
					New DbParamInfo("paramYear", paramYear),
					New DbParamInfo("importType", importType)
				}

				Dim dt As DataTable
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					dt = BRApi.Database.ExecuteSql(dbConn, sql, dbParams, False)
				End Using

				Dim warningCount As Integer = 0
				If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
					warningCount = CInt(dt.Rows(0)("warning_count"))
				End If

				If warningCount > 0 Then
					Return $"{warningCount} rows with warning please check Warning Table"
				End If

				Return "Import completed successfully"
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		Private Function BuildWorkOrderWarningMessage(ByVal si As SessionInfo, ByVal args As ExtenderArgs) As String
			Try
				'Parse query params
				Dim paramQueryParams As String = args.NameValuePairs("p_query_params")
				Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, paramQueryParams)
				Dim paramEntity As String = parameterDict("paramEntity")
				Dim paramYear As Integer = CInt(parameterDict("paramYear"))

				'Count warnings for this import
				Dim sql As String = "
					SELECT COUNT(*) AS warning_count
					FROM XFC_RES_AUX_template_warning
					WHERE entity = @paramEntity
					  AND year = @paramYear
					  AND import_type = 'work_order'"

				Dim dbParams As New List(Of DbParamInfo) From {
					New DbParamInfo("paramEntity", paramEntity),
					New DbParamInfo("paramYear", paramYear)
				}

				Dim dt As DataTable
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					dt = BRApi.Database.ExecuteSql(dbConn, sql, dbParams, False)
				End Using

				Dim warningCount As Integer = 0
				If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
					warningCount = CInt(dt.Rows(0)("warning_count"))
				End If

				If warningCount > 0 Then
					Return $"{warningCount} rows with warning please check Warning Table"
				End If

				Return "Import completed successfully"
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

	End Class
End Namespace
