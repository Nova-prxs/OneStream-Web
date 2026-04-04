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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.datamanagement
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						Dim FunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.functions
						Dim QueriesBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.all_queries
						Dim functionName = args.NameValuePairs("FunctionName")		
						
						If functionName = "CreateExcel"

							brapi.ErrorLog.LogMessage(si,"Entra 2: " & functionName)
							Dim queryName As String = args.NameValuePairs("queryName")
							Dim parameters As New Dictionary(Of String, String)  
							
							parameters.Add("Factory", args.NameValuePairs("factory"))
							
							Dim sql As String = QueriesBR.Query(queryName, parameters)
							brapi.ErrorLog.LogMessage(si,"Entra 3: " & sql)
							FunctionsBR.CreateExcel(si, sql, args.NameValuePairs("factory"))
							brapi.ErrorLog.LogMessage(si,"Excel Creado 4: " & args.NameValuePairs("factory"))
							Return Nothing
							
						Else If functionName = "CreateHistoricalData"
							
							Dim sql As String = QueriesBR.Query("CreateHistoricalData")
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Brapi.Database.ExecuteActionQuery(dbConn, sql, True, False)
							End Using
							
							Return Nothing
							
						End If
						
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
