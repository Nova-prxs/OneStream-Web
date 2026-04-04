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

Namespace OneStream.BusinessRule.Finance.CopyRF06_temp
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				
				Me.CopyRF06_temp(si, api, globals, args)	
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Sub CopyRF06_temp (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			'Clear scenario calculated data
			api.Data.ClearCalculatedData($"S#{api.Pov.Scenario.Name}",True,True,True,True)
			'Declare target and source data buffers
			Dim siDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
			Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"RemoveZeros(FilterMembers(T#2024M{api.Pov.Time.Name.Split("M")(1)}:S#RF09, A#PL.Base))")
'			For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
'				If api.Pov.Entity.Name = "R1301001" And api.Pov.Time.Name.Split("M")(1) = "1" Then
'					BRApi.ErrorLog.LogMessage(si, api.Members.GetMemberName(dimType.Account.Id, cell.DataBufferCellPk.AccountId))
'					BRApi.ErrorLog.LogMessage(si, cell.CellAmount)
'				End If
'			Next
			'Process data buffer
			api.Data.SetDataBuffer(sourceDataBuffer,siDestinationInfo,,,,,,,,,,,,,True)
	
			'BS copy
'			Dim siDestinationInfoBS As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
'			Dim sourceDataBufferBS As DataBuffer = api.Data.GetDataBufferUsingFormula($"RemoveZeros(FilterMembers(T#2024M{api.Pov.Time.Name.Split("M")(1)}:S#RF06, A#BS.Base))")

'			'Process data buffer
'			api.Data.SetDataBuffer(sourceDataBufferBS,siDestinationInfoBS,,,,,,,,,,,,,True)			
			
		End Sub
		
		
		
'		Sub CopyDataWithBufferFcstToBgt (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)	
		
'		' Leer los parámetros SourceCopyScenario, TargetCopyScenario y TimeCopy
'		Dim sourceCopyScenario As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("SourceScenario")
'		Dim targetCopyScenario As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("DestinationScenario")
				
		
'		' Construir la expresión de cálculo dinámica
'		Dim calculationExpression As String = "S#" & targetCopyScenario  & "=S#" & sourceCopyScenario 
		
'		' Ejecutar el cálculo
'		api.Data.Calculate(calculationExpression, True)
	
'		End Sub		
			
	End Class
End Namespace