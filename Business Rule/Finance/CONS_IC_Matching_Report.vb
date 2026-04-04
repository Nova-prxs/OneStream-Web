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

Namespace OneStream.BusinessRule.Finance.CONS_IC_Matching_Report
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
				
					Case Is = FinanceFunctionType.CustomCalculate
						'If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Test") Then
						'api.Data.Calculate("A#Profit = A#Sales - A#Costs")
						'End If
						
						'Execute Method Query for Workflow PK and use for specific Plug account filter
		Dim methodQuery As String = "{|WFProfile|}{|WFScenario|}{|WFTime|}{920L,921L,922L}{}{}{}{0}{}{}{F#Top:O#Top:U1#Top:U2#Top:U3#Top:U4#Legal:U5#None:U6#None:U7#Top:U8#None}{}"

		Dim objDataSet As DataSet = Nothing 
		Dim objDataSet2 As DataSet = Nothing 
		Dim objDataSet3 As DataSet = Nothing
		Dim objDataSet4 As DataSet = Nothing
		Dim objDataSet5 As DataSet = Nothing
		Dim objDataSet6 As DataSet = Nothing 
		Dim isDifference As Boolean = False
		Dim rptCurrAmount As Double = 0.00
		Dim rptCurrTotal As Double = 0.00
		Dim isWarning As Boolean = False
		Dim info1 As String = ""
		Dim info2 As String = ""
		Dim isDifferentCount As Integer = 0

		'Dim primEntity As String = api.Pov.Entity.Name
		Dim primEntity As String = "E122"
		
		'brapi.ErrorLog.LogMessage(si, "1")
		
		Using dbConnApp As DBConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
			objDataSet = BRApi.Database.ExecuteMethodCommand(dbConnApp, XFCommandMethodTypeId.ICMatchingForWorkflowUnitMultiPlug, methodQuery, "IC", Nothing)			
				
				'Create the data table to return
				Dim dt As New DataTable("Partners")	
				
				Dim objCol = New DataColumn
	            objCol.ColumnName = "Partner"
	            objCol.DataType = GetType(String)
	            objCol.DefaultValue = ""
	            objCol.AllowDBNull = False
	            dt.Columns.Add(objCol)					
				
				objCol = New DataColumn
				objCol.ColumnName = "RptCurrAmount"
				objCol.DataType = GetType(Double)
	            objCol.DefaultValue = 0.00
	            objCol.AllowDBNull = True
	            dt.Columns.Add(objCol)
				
				'brapi.ErrorLog.LogMessage(si, "2")								
			
			For Each table As DataTable In objDataSet.Tables
				
				If table.TableName = "IC" Then 'other table is "IC_TransStatus	
					
					'brapi.ErrorLog.LogMessage(si, table.TableName)
					
					For Each row As DataRow In table.Rows
											
						Dim primaryEntity As String = row.Item("PrimaryEntity")
						Dim entityParts As List(Of String) = StringHelper.SplitString(primaryEntity,"-")
						Dim primary As String = entityParts(0)
						Dim partnerEntity As String = row.Item("PartnerEntity")
						Dim partnerParts As List(Of String) = StringHelper.SplitString(partnerEntity,"-")
						Dim partner As String = partnerParts(0)					

						Dim existPartner As DataRow() = dt.Select("[Partner] = '" & partner & "'")
						
						'brapi.ErrorLog.LogMessage(si, existPartner.Length.ToString())
						
						isDifference = False
						isDifference = row("IsDifference")
						
						If isDifference = True Then
						
						If (primEntity = row.Item("PrimaryEntity"))
							
							If (existPartner.Length > 0) Then
								'brapi.ErrorLog.LogMessage(si, "2.Modifica partner")
								'brapi.ErrorLog.LogMessage(si, "2b:" & partner)	
								'brapi.ErrorLog.LogMessage(si, "2c:" & row(7).ToString())							
								'brapi.ErrorLog.LogMessage(si, "2d:" & row("rptCurrAmount").ToString())						
								dt.Select("[Partner] = '" & partner & "'")(0)("RptCurrAmount") = dt.Select("[Partner] = '" & partner & "'")(0)("RptCurrAmount") + row("rptCurrAmount")                	
							Else
								Dim NewRow As DataRow = dt.NewRow()	
								'brapi.ErrorLog.LogMessage(si, "1.Registra partner")
								'brapi.ErrorLog.LogMessage(si, "1b:" & partner)	
								'brapi.ErrorLog.LogMessage(si, "1c:" & row(7).ToString())
								'brapi.ErrorLog.LogMessage(si, "1d:" & row("rptCurrAmount").ToString())

								NewRow("Partner") = partner
								NewRow("RptCurrAmount") = row("rptCurrAmount") 
															
								dt.Rows.Add(NewRow)
							End If
						End If
						
						End If
					Next row
					
				End If
			Next table
			
			For Each row As DataRow In dt.Rows
				If Math.Abs(row("rptCurrAmount")) > 25000

					info1 +=  primEntity & " - " & row("Partner") & " has difference of: " & formatCurrency(row("rptCurrAmount"),2).Replace("$","") & Chr(13) & ""
					rptCurrTotal = rptCurrTotal + row("rptCurrAmount")
					
					isWarning = True
	            	
				End If
			Next row
			
		End Using
																								
							
		End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace