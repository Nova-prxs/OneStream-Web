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

Namespace OneStream.BusinessRule.DashboardStringFunction.AQS_FPA_ParamHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
					If args.FunctionName.XFEqualsIgnoreCase("DynamicMemberlistUD4") Then
						'Example call in Cube View - XFBR(AQS_FPA_ParamHelper, DynamicMemberlistUD4, selectedEntity=|!INP_EntitySelection!|)		
						'Grab the selectedEntity and pick up the Parent
						Dim selectedEntity As String = args.NameValuePairs.XFGetValue("selectedEntity")
						Dim countryName As String() = selectedEntity.Split("_")
						
						'Create empty list
						Dim listUD4 As String = Nothing

						'Get UD4 List 
						Dim ud4Memberlist = $"U4#{countryName(0)}.base"	
						Dim ud4DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"UD4_200_{countryName(0)}")
						Dim ud4List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, ud4DimPk, ud4Memberlist, True)
						Dim ud4Memberinfo As MemberInfo
					 		
						'Loop through the members of the list
						For Each ud4Memberinfo In ud4List	
							'Grab the UD4 name	
							Dim ud4Name As String = ud4Memberinfo.Member.Name
							'For each UD4 create the concatenation with the UD1 member based on the first four characters
							listUD4 = listUD4 & "U4#"& ud4Name & ":U1#"&ud4Name.Substring(0,4) & ":U2#"& ud4Name.Substring(4) &  ","
						Next
						
						'Return the list and remove the last comma by using the TRIM function
						Return listUD4.Trim().Remove(listUD4.Length - 1)
						
					End If					
					
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace