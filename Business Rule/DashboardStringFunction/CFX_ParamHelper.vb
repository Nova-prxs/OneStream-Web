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

Namespace OneStream.BusinessRule.DashboardStringFunction.CFX_ParamHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				
				'XFBR(CFX_ParamHelper, timeName, A=[|!param_POV_Time!|])

				 If args.FunctionName.XFEqualsIgnoreCase("timeName") Then
					If args.NameValuePairs.Count < 1 Then Return "Error"											
					
					' Obtener el ID y valor del parámetro
	                Dim prm_Id As String = args.NameValuePairs.Keys(0)
	                Dim selectedDate As String = args.NameValuePairs.XFGetValue(prm_Id)	
					
					
					Dim TimeYear As String =  selectedDate.Substring(0, 4)
					
					Dim TimeMonth As Integer
		
		            If Len(selectedDate) = 6 Then
		                ' Meses de enero a septiembre
		                TimeMonth = CInt(Right(selectedDate, 1))
		            Else
		                ' Meses de octubre a diciembre
		                TimeMonth = CInt(Right(selectedDate, 2))
		            End If
					
					Dim monthAbbreviation As String 
					
					Select Case TimeMonth   
						Case 1    
							monthAbbreviation = "January" 
						Case 2     
							monthAbbreviation = "February"   
						Case 3     
							monthAbbreviation = "March"  
						Case 4    
							monthAbbreviation = "April"   
						Case 5     
							monthAbbreviation = "May"    
						Case 6      
							monthAbbreviation = "June"  
						Case 7    
							monthAbbreviation = "July"  
						Case 8     
							monthAbbreviation = "August"   
						Case 9     
							monthAbbreviation = "September"   
						Case 10      
							monthAbbreviation = "Octuber"  
						Case 11      
							monthAbbreviation = "November"  
						Case 12      
							monthAbbreviation = "December"   
						Case Else         
							monthAbbreviation = "" 
					End Select
				
               		Return monthAbbreviation & " " & TimeYear	
				
				End If
				

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace