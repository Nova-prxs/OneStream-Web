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

Namespace OneStream.BusinessRule.Finance.ADMIN_Calculates
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.CustomCalculate
						
						#Region "Copy Actual to Forecast"
						
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyActualToForecast") Then
							
							'Clear target data buffer
							Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Forecast,F#None,O#Import)")
							Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							Me.ClearDataBuffer(si, api, targetDataBuffer, cleanExpDestInfo)
							
							'Get source data buffer and declare destination info
							Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Actual,F#None,O#Import)")
							Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							
							'Convert dimensionality and set data buffer
							Dim convertedDataBuffer As DataBuffer = api.Data.ConvertDataBufferExtendedMembers("Alsea", "Actual", sourceDataBuffer)
							api.Data.SetDataBuffer(convertedDataBuffer, expDestInfo,,,,,,,,,,,,, True)
						
						#End Region
						
						#Region "Copy Actual To Forecast Version"
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyActualToForecastVersion") Then
						    'Get parameters
						    Dim ParamTargetScenario As String = args.CustomCalculateArgs.NameValuePairs("p_TargetScenario")
						    Dim ParamSourceScenario As String = args.CustomCalculateArgs.NameValuePairs("p_SourceScenario")
						    
						    Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{ParamTargetScenario},F#None)")
						    Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo($"S#{ParamTargetScenario}")
						    Me.ClearDataBuffer(si, api, targetDataBuffer, cleanExpDestInfo)
						    
						    Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{ParamSourceScenario},F#None)")
						    
						    Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo($"S#{ParamTargetScenario}")
						    
						    'Convert dimensionality and set data buffer - CORREGIDO: quité comillas de la variable
						    Dim convertedDataBuffer As DataBuffer = api.Data.ConvertDataBufferExtendedMembers("Alsea", ParamSourceScenario, sourceDataBuffer)
						    api.Data.SetDataBuffer(convertedDataBuffer, expDestInfo,,,,,,,,,,,,, True)
						#End Region
																		
						#Region "Copy CECOS"
						
						ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Fusion_Cecos") Then
							
							
'							api.Data.ClearCalculatedData($"S#Budget:E#M002698:O#Import",True,True,True,True)
							api.Data.Calculate($"S#Budget:E#M002699:O#Import = 0 + S#Budget:E#M004699:O#Import",True)
						    api.Data.Calculate($"S#Budget:E#M002699:O#Forms = 0 + S#Budget:E#M004699::O#Forms",True)
							
						#End Region
						
						#Region "Copy Actual to Forecast (ECI)"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyActualToForecastSalesECI") Then							
							
							'Get source data buffer and declare destination info
							Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Actual,F#None,A#70010000,A#90000170,A#90000171)")
							Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							
							'Convert dimensionality and set data buffer
							Dim convertedDataBuffer As DataBuffer = api.Data.ConvertDataBufferExtendedMembers("Alsea", "Actual", sourceDataBuffer)
							api.Data.SetDataBuffer(convertedDataBuffer, expDestInfo,,,,,,,,,,,,, True)
						
						#End Region		
						
						#Region "Copy Actual to Forecast Manual"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyActualToForecastManual") Then							
							
							'Get source data buffer and declare destination info
							Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Actual,F#None,O#Import,A#Customers)")
							Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							
							'Convert dimensionality and set data buffer
							Dim convertedDataBuffer As DataBuffer = api.Data.ConvertDataBufferExtendedMembers("Alsea", "Actual", sourceDataBuffer)
							api.Data.SetDataBuffer(convertedDataBuffer, expDestInfo,,,,,,,,,,,,, True)
						
						#End Region								
					
						#Region "Copy Actual to Forecast Sales"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyActualToForecastSales") Then							
							
							'Get source data buffer and declare destination info
							Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Actual,F#None,O#Import,A#Customers)")
							Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							
							'Convert dimensionality and set data buffer
							Dim convertedDataBuffer As DataBuffer = api.Data.ConvertDataBufferExtendedMembers("Alsea", "Actual", sourceDataBuffer)
							api.Data.SetDataBuffer(convertedDataBuffer, expDestInfo,,,,,,,,,,,,, True)
						
						#End Region							
						
						#Region "Copy Historical to Budget"
						
					   Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyHistoricalToBudget2024Cube") Then
						   
						   'Clear target data buffer
							Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Budget,F#None,O#Import)")
							Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							Me.ClearDataBuffer(si, api, targetDataBuffer, cleanExpDestInfo)
							
							'Get source data buffer and declare destination info
							Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Historical_Data,F#None,O#Import)")
							Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							
							'Convert dimensionality and set data buffer
							Dim convertedDataBuffer As DataBuffer = api.Data.ConvertDataBufferExtendedMembers("Alsea", "Historical_Data", sourceDataBuffer)
							api.Data.SetDataBuffer(convertedDataBuffer, expDestInfo,,,,,,,,,,,,, True)
						
						#End Region
						
						#Region "Save Scenario Version"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("SaveScenarioVersion") Then
							
							'Get scenario parameter
							Dim ParamScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							
							'Clear target data buffer
							Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{api.Pov.Scenario.Name})",,False,,,)
							Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							Me.ClearDataBuffer(si, api, targetDataBuffer, cleanExpDestInfo)
							
							'Get source data buffer and declare destination info
							Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{ParamScenario})")
							Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							
							'Set Data buffer
							api.Data.SetDataBuffer(sourceDataBuffer, expDestInfo,,,,,,,,,,,,, True)
						
						#End Region
						
						#Region "Copy RDI to Mexico"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyRDIToMexico") Then
							
							'Get parameters
							Dim ParamScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							
							'Get source data buffer and declare destination info
							Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{ParamScenario},F#None,U1#None,U2#Top.Base,A#AL_RDI.Base)")
							Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							Dim targetDataBuffer As New DataBuffer
							'Process each data buffer cell to change the account member
							For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
								Dim targetCell As New DataBufferCell(cell)
								Dim cellAccountName As String = api.Members.GetMemberName(dimType.Account.Id, targetCell.DataBufferCellPk.AccountId)
								Dim cellAccountMapped As String = BRApi.Utilities.TransformText(si, cellAccountName, "PLANNING_Mexico_Account_Mapping", False)
								If Not String.IsNullOrEmpty(cellAccountMapped) Then
									targetCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account.Id, cellAccountMapped)
									targetDataBuffer.SetCell(si, targetCell, True)
								End If
							Next
							'Set data buffer
							api.Data.SetDataBuffer(targetDataBuffer, expDestInfo,,,,,,,,,,,,, True)
						#End Region
						
						#Region "Clear Data Buffer"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("ClearDataBuffer") Then
							
							'Clear target data buffer
							Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Forecast:A#XXX,F#None,O#Forms)",,False,,,)
							Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							Me.ClearDataBuffer(si, api, targetDataBuffer, cleanExpDestInfo)
						
						#End Region		
						
						#Region "Clear Data"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("ClearData") Then
							
							'Clear target data buffer
							Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Budget)",,False,,,)
							Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							Me.ClearDataBuffer(si, api, targetDataBuffer, cleanExpDestInfo)
						
						#End Region						
						
						#Region "Seed Reporting Cube"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("SeedReportingCube") Then
							
							'Declare parameters and relevant variables
							Dim povYear As Integer = CInt(api.Pov.Time.Name.Split("M")(0))
							Dim maxYear As Integer = povYear + 2
							Dim povEntityName As String = api.Pov.Entity.Name
							Dim povEntityID As Integer = api.Pov.Entity.MemberId
							Dim dt As New DataTable
							dt.Columns.Add("year")
							dt.Columns.Add("desc_annualcomparability")
							dt.Columns.Add("scenario")
							
							'Build a dictionary to match table comparability to dimension comparability
							Dim comparabilityDictionary As New Dictionary(Of String, String) From {
								{"Comparables", "Comparables_N"},
								{"Cerradas", "Closings_N"},
								{"Reformas", "Reformas_N"},
								{"Reformas Año Anterior", "Reformas_AA_N"},
								{"Operativas Primer Año", "Openings_CY_N"},
								{"Operativas Segundo Año", "Openings_PY_N"},
								{"Otros Estados No Comparab", "Other_States_N"}
							}
							'Clear target data buffer
							Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{api.Pov.Scenario.Name})")
							Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							Me.ClearDataBuffer(si, api, targetDataBuffer, cleanExpDestInfo)
							
							'Select cebe comparability for current and next years
							For iYear As Integer = povYear To maxYear
								' Actual row with text 1
								Dim row = dt.NewRow()
								row("year") = iYear
								row("scenario") = "Actual"
								row("desc_annualcomparability") = BRApi.Finance.Entity.Text(
									si,
									povEntityID, 1, 0,
									api.Members.GetMemberId(DimType.Time.Id, iYear)
								)
								dt.Rows.Add(row)
								
								' Planning row with text 2
								row = dt.NewRow()
								row("year") = iYear
								row("scenario") = "Planning"
								row("desc_annualcomparability") = BRApi.Finance.Entity.Text(
									si,
									povEntityID, 2, 0,
									api.Members.GetMemberId(DimType.Time.Id, iYear)
								)
								dt.Rows.Add(row)
							Next
							
'							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'								Dim selectQuery As String = $"
'									WITH RealComparability AS (
'									    SELECT 
'									        cebe,
'									        date,
'									        desc_annualcomparability,
'									        ROW_NUMBER() OVER (PARTITION BY cebe, YEAR(date) ORDER BY date) AS rn
'									    FROM 
'									        XFC_ComparativeCEBES WITH(NOLOCK)
'										WHERE
'											YEAR(date) BETWEEN {povYear} AND {maxYear}
'											AND cebe = '{povEntityName}'
'									)
'									SELECT 
'									    YEAR(date) AS year,
'									    desc_annualcomparability,
'										'Actual' AS scenario
'									FROM 
'									    RealComparability
'									WHERE 
'									    rn = 1
								
'									UNION ALL
								
'									SELECT year, desc_annualcomparability, 'Planning' AS scenario
'									FROM XFC_ComparativeCEBESAux WITH(NOLOCK)
'									WHERE
'										year BETWEEN {povYear} AND {maxYear}
'										AND cebe = '{povEntityName}'
'								"
'								dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
'							End Using
							
							'Build new data buffer
							Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Alsea,S#{api.Pov.Scenario.Name},F#None,U1#Top.Base,U1#None,U2#None,U2#Top.Base,A#AL_RDI.Base,A#Sales_Customers_Aux.Base,A#Personnel_Hours.Base,A#Cost_of_Sales_Total.Base)")
							Dim newDataBuffer As New DataBuffer
							'Loop through each data buffer cell
							For Each sourceDataBufferCell In sourceDataBuffer.DataBufferCells.Values
								'Filter out no data cells and 0's
								If Not sourceDataBufferCell.CellStatus.IsNoData AndAlso sourceDataBufferCell.CellAmount <> 0 Then
									If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
										'Loop through each comparability year to add data to it's corresponding UD3, UD6, UD7 members and origin import
										For Each row As DataRow In dt.Rows
											Dim i As Integer = row("year") - povYear
											Dim targetCell As New DataBufferCell(sourceDataBufferCell)
											Dim UD3MemberName As String = comparabilityDictionary(row("desc_annualcomparability")) 'Comparability
											Dim UD6MemberName As String = $"N{i}" 'Years of difference with the reporting year
											Dim UD7MemberName As String = $"Rptg_{row("scenario")}" 'Scenario: Planning or Actual
											targetCell.DataBufferCellPk.UD3Id = api.Members.GetMemberId(DimType.UD3.Id, UD3MemberName)
											targetCell.DataBufferCellPk.UD6Id = api.Members.GetMemberId(DimType.UD6.Id, UD6MemberName)
											targetCell.DataBufferCellPk.UD7Id = api.Members.GetMemberId(DimType.UD7.Id, UD7MemberName)
											targetCell.DataBufferCellPk.OriginId = api.Members.GetMemberId(DimType.Origin.Id, "Import")
											newDataBuffer.SetCell(si, targetCell, True)
											'Add a cell converted to mexico reporting if it has a mapping account
											Dim cellAccountName As String = targetCell.GetAccountName(api) ' api.Members.GetMemberName(dimType.Account.Id, targetCell.DataBufferCellPk.AccountId)
											Dim cellAccountMapped As String = BRApi.Utilities.TransformText(si, cellAccountName, "PLANNING_Mexico_Account_Mapping", False)
											If Not String.IsNullOrEmpty(cellAccountMapped) Then
												Dim mexicoTargetCell As New DataBufferCell(targetCell)
												mexicoTargetCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account.Id, cellAccountMapped)
												newDataBuffer.SetCell(si, mexicoTargetCell, True)
											End If
										Next
									End If
								End If
							Next
							'Seed new data buffer
							api.Data.SetDataBuffer(newDataBuffer, cleanExpDestInfo,,,,,,,,,,,,,True)
						
						#End Region
						
						#Region "Seed Reporting Cube"

						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("SeedReportingCube_Forecast") Then
						    
						    
						    Dim ParamSourceScenario As String = args.CustomCalculateArgs.NameValuePairs("pSourceScenario")
						    
						    
						    If String.IsNullOrEmpty(ParamSourceScenario) Then
						        ParamSourceScenario = api.Pov.Scenario.Name
						    End If
						    
						    'Declare parameters and relevant variables
						    Dim povYear As Integer = CInt(api.Pov.Time.Name.Split("M")(0))
						    Dim maxYear As Integer = povYear + 2
						    Dim povEntityName As String = api.Pov.Entity.Name
						    Dim povEntityID As Integer = api.Pov.Entity.MemberId
						    Dim dt As New DataTable
						    dt.Columns.Add("year")
						    dt.Columns.Add("desc_annualcomparability")
						    dt.Columns.Add("scenario")
						    
						    'Build a dictionary to match table comparability to dimension comparability
						    Dim comparabilityDictionary As New Dictionary(Of String, String) From {
						        {"Comparables", "Comparables_N"},
						        {"Cerradas", "Closings_N"},
						        {"Reformas", "Reformas_N"},
						        {"Reformas Año Anterior", "Reformas_AA_N"},
						        {"Operativas Primer Año", "Openings_CY_N"},
						        {"Operativas Segundo Año", "Openings_PY_N"},
						        {"Otros Estados No Comparab", "Other_States_N"}
						    }
						    
						    'Clear target data buffer
						    Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{api.Pov.Scenario.Name})")
						    Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
						    Me.ClearDataBuffer(si, api, targetDataBuffer, cleanExpDestInfo)
						    
						    'Select cebe comparability for current and next years
						    For iYear As Integer = povYear To maxYear
						        ' Actual row with text 1
						        Dim row = dt.NewRow()
						        row("year") = iYear
						        row("scenario") = "Actual"
						        row("desc_annualcomparability") = BRApi.Finance.Entity.Text(
						            si,
						            povEntityID, 1, 0,
						            api.Members.GetMemberId(DimType.Time.Id, iYear)
						        )
						        dt.Rows.Add(row)
						        
						        ' Planning row with text 2
						        row = dt.NewRow()
						        row("year") = iYear
						        row("scenario") = "Planning"
						        row("desc_annualcomparability") = BRApi.Finance.Entity.Text(
						            si,
						            povEntityID, 2, 0,
						            api.Members.GetMemberId(DimType.Time.Id, iYear)
						        )
						        dt.Rows.Add(row)
						    Next
						    
						    'Build new data buffer - AQUÍ ESTÁ EL CAMBIO PRINCIPAL
						    Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Alsea,S#{ParamSourceScenario},F#None,U1#Top.Base,U1#None,U2#None,U2#Top.Base,A#AL_RDI.Base,A#Sales_Customers_Aux.Base,A#Personnel_Hours.Base,A#Cost_of_Sales_Total.Base)")
						    Dim newDataBuffer As New DataBuffer
						    
						    'Loop through each data buffer cell
						    For Each sourceDataBufferCell In sourceDataBuffer.DataBufferCells.Values
						        'Filter out no data cells and 0's
						        If Not sourceDataBufferCell.CellStatus.IsNoData AndAlso sourceDataBufferCell.CellAmount <> 0 Then
						            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						                'Loop through each comparability year to add data to it's corresponding UD3, UD6, UD7 members and origin import
						                For Each row As DataRow In dt.Rows
						                    Dim i As Integer = row("year") - povYear
						                    Dim targetCell As New DataBufferCell(sourceDataBufferCell)
						                    Dim UD3MemberName As String = comparabilityDictionary(row("desc_annualcomparability")) 'Comparability
						                    Dim UD6MemberName As String = $"N{i}" 'Years of difference with the reporting year
						                    Dim UD7MemberName As String = $"Rptg_{row("scenario")}" 'Scenario: Planning or Actual
						                    targetCell.DataBufferCellPk.UD3Id = api.Members.GetMemberId(DimType.UD3.Id, UD3MemberName)
						                    targetCell.DataBufferCellPk.UD6Id = api.Members.GetMemberId(DimType.UD6.Id, UD6MemberName)
						                    targetCell.DataBufferCellPk.UD7Id = api.Members.GetMemberId(DimType.UD7.Id, UD7MemberName)
						                    targetCell.DataBufferCellPk.OriginId = api.Members.GetMemberId(DimType.Origin.Id, "Import")
						                    newDataBuffer.SetCell(si, targetCell, True)
						                    'Add a cell converted to mexico reporting if it has a mapping account
						                    Dim cellAccountName As String = targetCell.GetAccountName(api)
						                    Dim cellAccountMapped As String = BRApi.Utilities.TransformText(si, cellAccountName, "PLANNING_Mexico_Account_Mapping", False)
						                    If Not String.IsNullOrEmpty(cellAccountMapped) Then
						                        Dim mexicoTargetCell As New DataBufferCell(targetCell)
						                        mexicoTargetCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account.Id, cellAccountMapped)
						                        newDataBuffer.SetCell(si, mexicoTargetCell, True)
						                    End If
						                Next
						            End If
						        End If
						    Next
						    'Seed new data buffer
						    api.Data.SetDataBuffer(newDataBuffer, cleanExpDestInfo,,,,,,,,,,,,,True)
						
						#End Region
							
						End If
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Functions"
		
		#Region "Helpers"
		
		Public Sub ClearDataBuffer(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal targetDataBuffer As DataBuffer, ByVal cleanExpDestInfo As ExpressionDestinationInfo)
			'Build clean data buffer
			Dim cleanDataBuffer As New DataBuffer
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
		
		#End Region
		
		#End Region
	End Class
End Namespace