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
 
Namespace OneStream.BusinessRule.Finance.CopyScenario
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Dim sFunction As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Function")
					If sFunction = "CopyP1X_All" Then	
						Me.Copiado_P1X_All(si, api, globals, args)
					Else If sFunction = "ClearData" Then	
						Me.ClearData(si, api, globals, args)
					End If
 
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	Sub ClearData(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		'Get parameters
		Dim targetScenario As String = api.Pov.Scenario.Name
		If targetScenario.ToUpper = "STATUTORY" Then
			'-------------------------------------------------- CLEAR DATA ------------------------------------
			'Clear target data buffer
			Dim targetDataBufferclear As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Horse:S#{targetScenario})")
			Me.ClearDataBuffer(si, api, targetDataBufferclear)
		End If
	End Sub
	'--------------------------------------------------------------------------------------
	Sub Copiado_P1X_All(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		'Get parameters
		Dim targetScenario As String = api.Pov.Scenario.Name
		If targetScenario.ToUpper = "STATUTORY" Then
			'-------------------------------------------------- CLEAR DATA ------------------------------------
			'Clear target data buffer
			Dim targetDataBufferclear As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Horse:S#{targetScenario}, [U4#Root.TreeDescendantsInclusive.Remove(U4#PACKN1, U4#PACKN2, U4#PACKN4)])") 
			If targetDataBufferclear.DataBufferCells.Count > 0 Then
					For Each targetCell In targetDataBufferclear.DataBufferCells.Values
						Dim UD4MemberType As String = api.Members.GetMemberName(dimtypeid.UD4, targetCell.DataBufferCellPk.UD4Id)
						'Del escenario reporting no borramos el contenido de UD4 AC
						
						If (UD4MemberType <> "PACKN1" And UD4MemberType <> "PACKN2" And UD4MemberType <> "PACKN4")  Then
							Me.ClearDataBuffer(si, api, targetDataBufferclear)
						End If 
					Next
				End If

			'-------------------------------------------------- PERIM COPY -----------------------------------
			'Get source data buffer And Declare destination info for SCOPE ACCOUNTS
			Dim sourceDataBufferScope As DataBuffer = api.Data.GetDataBufferUsingFormula($"RemoveZeros(FilterMembers(Cb#Horse:S#Actual, A#PerimeterAcc.base))")
			Dim expDestInfoScope As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
			'Convert dimensionality and set data buffer
			api.Data.SetDataBuffer(sourceDataBufferScope, expDestInfoScope,,,,,,,,,,,,, True)

			'-------------------------------------------------- GENERAL COPY ------------------------------------
			'Get source data buffer And Declare destination info
			Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"RemoveZeros(FilterMembers(Cb#Horse:S#Actual,O#Import,O#Forms))")
			Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
			'Declare new data buffer
			Dim targetDataBuffer As New DataBuffer()
			'Return if data buffer has no cells
			If sourceDataBuffer.DataBufferCells.Count > 0 Then
			'Loop through all the data buffer cells
				For Each sourceCell In sourceDataBuffer.DataBufferCells.Values
					Dim flowName As String = api.Members.GetMemberName(dimtypeid.Flow, sourceCell.DataBufferCellPk.FlowId)
					'Filter out MOV
					If (flowName <> "F14" Or flowName <> "F80C" Or flowName <> "F80M")  Then
						'Declare result cell from source cell and convert U3 to None and map account
						Dim resultCell As New DataBufferCell(sourceCell)
						'Set cell
						targetDataBuffer.SetCell(si, resultCell, True)
					End If
				Next
			End If
			'Convert dimensionality and set data buffer
			api.Data.SetDataBuffer(targetDataBuffer, expDestInfo,,,,,,,,,,,,, True)
		End If

	End Sub
	Public Sub ClearDataBuffer(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal targetDataBuffer As DataBuffer)
			'Build clean data buffer
			Dim cleanDataBuffer As New DataBuffer
			Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
			For Each targetDataBufferCell In targetDataBuffer.DataBufferCells.Values
				If Not targetDataBufferCell.CellStatus.IsNoData Then
					targetDataBufferCell.CellAmount = 0
					targetDataBufferCell.CellStatus = New DataCellStatus(targetDataBufferCell.CellStatus, DataCellExistenceType.NoData)
					cleanDataBuffer.SetCell(si, targetDataBufferCell)
				End If
			Next
			'Clean data buffer
			api.Data.SetDataBuffer(cleanDataBuffer, cleanExpDestInfo,,,,,,,,,,,,,True)
		End Sub
	End Class
End Namespace