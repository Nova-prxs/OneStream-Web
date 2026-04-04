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

Namespace OneStream.BusinessRule.Extender.JLM_Solution
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						#Region "Delete all journals in current workflow unit"						
						' --- For testing only, Delete all journals in current workflow unit ---
						Dim jtw As JournalsAndTemplatesForWorkflow = brapi.Journals.Metadata.GetJournalsAndTemplates(si, si.WorkflowClusterPk, False)
'						brapi.ErrorLog.LogMessage(si, "JRL COUNT: " & jtw.Journals.Count)
						For Each jsi As JournalSummaryInfo In jtw.Journals
'							If jsi.JournalStatus = JournalStatus.Posted Then
								brapi.Journals.Process.ExecuteUnpost(si, jsi.UniqueID)
''								brapi.Journals.Process.ExecuteReject(si, jsi.UniqueID)
'							Else
'							ElseIf jsi.JournalStatus = JournalStatus.Approved OrElse jsi.JournalStatus = JournalStatus.Submitted Then
								brapi.Journals.Process.ExecuteReject(si, jsi.UniqueID)
								brapi.Journals.Metadata.DeleteJournalOrTemplate(si, jsi.UniqueID)
'							End If
'							brapi.Journals.Metadata.DeleteJournalOrTemplate(si, jsi.UniqueID)
						Next
						Dim sbSQLLineItems As New Text.StringBuilder()
						
						Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
							sbSQLLineItems.AppendLine("Delete FROM [JLM_ImportLineItem] ")
							BRApi.Database.ExecuteSql(dbConnApp, sbSQLLineItems.ToString(), False)
						End Using
						
'						Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
'							sbSQLLineItems.AppendLine("Delete FROM [JLM_ImportHeader] ")
'							BRApi.Database.ExecuteSql(dbConnApp, sbSQLLineItems.ToString(), False)
'						End Using
						#End Region


					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
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
	End Class
End Namespace