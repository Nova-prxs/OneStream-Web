Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database

Namespace OneStream.BusinessRule.Finance.CONS_PACK_BORRAR
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				
				Dim IsAdmin As Boolean = BRApi.Security.Authorization.IsUserInGroup(si, "Administrators")						
		
				Dim PovEntity As String = api.Pov.Entity.Name
				Dim PovScenario As String = api.Pov.Scenario.name
				Dim PovTime As String = api.Pov.Time.Name
		
				Dim Block_Pack = api.Data.GetDataCell("E#" & PovEntity & ":S#" & PovScenario & ":T#" & PovTime & ":A#Status_Pack:C#Local:V#YTD:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#Pack:U5#None:U6#None:U7#None:U8#None").CellAmount				
			
									
					If (Block_Pack <> 1 And IsAdmin = False) Or (IsAdmin = True) Then		
				
'------------ DELETE DATA -----------------------------------------------------------------------------------------------------------

						'We pass the Param
						Dim sEntity As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Entity","")
						
						Dim destination As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
										
						'retrieve the data buffer to clear, loop through the cells, and set the cell amounts to zero and the cell status to NoData

						Dim sourceBufferToClear As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(O#Forms:UD4#Pack, [A#Root.Base.Remove(Status_Pack)])", DataApiScriptMethodType.Calculate, False, destination)

						If (Not sourceBufferToClear Is Nothing) Then

						     Dim resultBufferToClear As New DataBuffer()

						        For Each sourceCell As DataBufferCell In sourceBufferToClear.DataBufferCells.Values

						            Dim resultCell As New DataBufferCell(sourceCell)

						               resultCell.CellAmount = 0

						               resultCell.CellStatus = DataCellStatus.CreateDataCellStatus(True, False)

						               resultBufferToClear.SetCell(si, resultCell)

						            Next

						            'clear out the cells
						            api.Data.SetDataBuffer(resultBufferToClear, destination)							

						End If
				
'------------ DELETE ANNOTATION -----------------------------------------------------------------------------------------------------------			
								
									
					'get annotation data table sql statement
					Dim sql As New Text.StringBuilder()
					sql.Append("UPDATE DataAttachment ")
					sql.Append("SET text = '' ")
					sql.Append("WHERE Time = '"& povTime & "' ")
					sql.Append("AND ")
					sql.Append("Scenario = 'R' ")
					sql.Append("AND ")
					sql.Append("Entity = '"& povEntity & "' ")
					sql.Append("AND ")
					sql.Append("Flow = 'PAK' ")
					
					Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)				
						Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sql.ToString, True)												
					End Using				
																
					Return Nothing
				
				Else
					
					Dim sMensajeError As String = "Los datos no se han podido borrar. La entidad seleccionada está bloqueada // Data could not be deleted. The selected Entity is blocked"
								
						Throw New Exception(sMensajeError)	
										
				End If
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace