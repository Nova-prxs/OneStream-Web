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



 Dim prm_Id As String = args.NameValuePairs.Keys(0)
	                Dim selectedDate As String = args.NameValuePairs.XFGetValue(prm_Id)	
					
		            Dim selectedYear As Integer = CInt(Left(selectedDate, 4))
		            Dim selectedMonth As Integer



Namespace OneStream.BusinessRule.DashboardStringFunction.Book_Param_Helper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("GetTimeDescription") Then
					 Dim param_timeName As Integer = args.NameValuePairs.XFGetValue("param_timeName")
					 
					 BRApi.ErrorLog.LogMessage(si,"param_timeName: " & param_timeName)
					 
					 Dim test As String = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Time, param_timeName).Description
					 
					 Dim test2 As String =  BRApi.Finance.Members.GetMember(si, dimTypeId.Time, param_timeName).Description
					 
					 BRApi.ErrorLog.LogMessage(si,"test: " & test)
					 
					 BRApi.ErrorLog.LogMessage(si,"test2: " & test2)
					 
					 Return BRApi.Finance.Metadata.GetMember(si, dimTypeId.Time, param_timeName).Description
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace