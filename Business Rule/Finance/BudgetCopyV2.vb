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

Namespace OneStream.BusinessRule.Finance.BudgetCopyV2
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			
			Try
				
				Dim sFunction As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Function")
				
				If (Not api.Entity.HasChildren) And api.Cons.IsLocalCurrencyForEntity() Then

					If sFunction = "CopyScenario" Then
						
						Me.CopyScenario(si, api, globals, args)
						
					End If
					
				End If
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
		Sub CopyScenario (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		

			Dim results As New DataBuffer	
			Dim dataToCopy As DataBuffer = api.Data.GetDataBufferUsingFormula("S#Budget_V1:O#Import")
	
			For Each db1Cell As DataBufferCell In dataToCopy.DataBufferCells.Values
				Dim resultCell As New DataBufferCell(db1Cell)				
				results.SetCell(si, resultCell, False)			

			Next
	
			Dim exprDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("S#Budget_V2:O#Forms")
			api.Data.SetDataBuffer(results, exprDestInfo,,,,,,,,,,,,,True)
	
		End Sub

	End Class
	
End Namespace