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

Namespace OneStream.BusinessRule.DashboardStringFunction.CashFlow_Maps
	Public Class MainClass
'------------------------------------------------------------------------------------------------------------
'Reference Code: 		CashFlow_Maps
'
'Description:			Used to return the cube view rows based on one Cash flow account. 
'
'Use Examples:	        XFBR(CashFlow_Maps, CF_Map, CFAccount=|!DrillAccount!|)
'
'Created By:			Marcelo Castro
'
'Date Created:			24-06-2024
'------------------------------------------------------------------------------------------------------------	

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("CF_Map") Then
					'Query DB for list of members using that plug account
							'Using dbcApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim sbSQLAcc As New Text.StringBuilder()					 
								Dim sOAcc As String 
								Dim sOFlow As String 
								Dim sCFAcc As String = args.NameValuePairs("CFAccount")
								Dim sResult As String =""
								Dim iCount As Integer = 0
								Dim sComma As String =""
								
								BRAPI.ErrorLog.LogMessage (si, "CF account " & sCFAcc)
								'Select all the Balance accounts and Flows defined in the mapping table for one CF account.
								sbSQLAcc.Append(" SELECT Origin_Account, Origin_Flow")			
								sbSQLAcc.Append(" FROM XFC_CASHFLOW_TABLE where Destination_Account = '" & sCFAcc & "'")																				
								
								Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
									Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQLAcc.ToString,False)											
											
										For Each dr As DataRow In dt.Rows
																					
											sOAcc = dr("Origin_Account").toString
											sOFlow = dr("Origin_Flow").toString
											
											'brapi.ErrorLog.LogMessage(si, sCFAcc)
											If iCount > 0 Then sComma = ","	
											
											sResult = sResult & sComma & "A#"& sOAcc & ":F#"& sOFlow & ":Name(" & sOAcc &" - "& sOFlow &")" 
											iCount = iCount + 1
										Next				
									End Using
								End Using
'								BRAPI.ErrorLog.LogMessage (si, "Result " & sResult)
					Return sResult '"A#1101000C:F#F20:Name(1101000C-F20),A#ASSETS:F#F01:Name(Assets-F10)"
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace