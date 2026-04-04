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

Namespace OneStream.BusinessRule.Extender.UTI_FxTranslationSystem
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Get function name
						Dim paramFunction As String = args.NameValuePairs("p_function")
						
						'Control function
						
						#Region "Set Fx Rates"
						
						If paramFunction = "SetFxRates" Then
							'Get parameters
							Dim paramYear As Integer = args.NameValuePairs("p_year")
							Dim fxRateTypeList As New List(Of String) From {"AverageRate", "OpeningRate", "ClosingRate", "HistoricalRate"}
							Dim paramDefaultCurrency As String = args.NameValuePairs("p_default_currency")
							
							'Declare culture info to control decimal separator
							Dim nfi As NumberFormatInfo = CultureInfo.InvariantCulture.NumberFormat
							
							'Get currencies from properties
							Dim objAppProperties As AppProperties = BRApi.Utilities.GetApplicationProperties(si)
							Dim currencyArray As String() = objAppProperties.CurrencyFilter.Split(",")
							
							'Create data table to populate the custom table
							Dim fxRatesDataTable As New DataTable()
							fxRatesDataTable.Columns.Add("year")
							fxRatesDataTable.Columns.Add("month")
							fxRatesDataTable.Columns.Add("type")
							fxRatesDataTable.Columns.Add("source")
							fxRatesDataTable.Columns.Add("target")
							fxRatesDataTable.Columns.Add("rate")
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								'Get all fx rate types (TEMPORAL SOLUTION) and add them to the fx rate type array
								Dim fxRateTypesDataTable As DataTable = BRApi.Database.ExecuteSql(
									dbConn,
									"
										SELECT DISTINCT Name
										FROM FxRateType
									",
									False
								)
								If fxRateTypesDataTable.Rows.Count > 0 Then
									For Each row As DataRow In fxRateTypesDataTable.Rows
										fxRateTypeList.Add(row("Name"))
									Next
								End If
								
								'Add fx rates to data table per period and currency
								For Each fxRateType As String In fxRateTypeList
									For i As Integer = 1 To 12
										Dim period As String = $"{paramYear}M{i}"
										For Each currency As String In currencyArray
											'Get fx rate for the period
											Dim fxRatePkUsingNames As New FxRatePkUsingNames(fxRateType, period, currency, paramDefaultCurrency)
											Dim objFxRateUsingNames As FxRateUsingNames = BRApi.Finance.Data.GetStoredFxRate(si, fxRatePkUsingNames)
											'Get rate
											Dim rate As Decimal = objFxRateUsingNames.Amount
											'Only save if it has an amount
											If rate > 0 Then
												'Create new row
												Dim row As DataRow = fxRatesDataTable.NewRow()
												row("year") = paramYear
												row("month") = i
												row("type") = fxRateType
												row("source") = currency.Trim()
												row("target") = paramDefaultCurrency
												row("rate") = rate.ToString(nfi)
												fxRatesDataTable.Rows.Add(row)
											End If
										Next
									Next
								Next
								
								If fxRatesDataTable.Rows.Count > 0 Then
									'Load data table to raw custom table
									UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_MAIN_RAW_fxrate", fxRatesDataTable, "replace")
									'Perform a merge to the aux custom table
									BRApi.Database.ExecuteSql(
										dbConn,
										"
											MERGE INTO XFC_MAIN_AUX_fxrate AS target
											USING (
												SELECT 
													year,
													month,
													type,
													source,
													target,
													rate
												FROM XFC_MAIN_RAW_fxrate
											) AS source
											ON target.year = source.year
											AND target.month = source.month
											AND target.type = source.type
											AND target.source = source.source
											AND target.target = source.target
											WHEN MATCHED THEN
											    UPDATE SET
											        rate = source.rate
											WHEN NOT MATCHED THEN
											    INSERT (
											        year, month, type, source, target, rate
											    )
											    VALUES (
											        source.year, source.month, source.type, source.source, source.target, rate
											    );
										",
										False
									)
								End If
							End Using
							
						End If
						
						#End Region
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						'Add External Members
						Dim externalMembers As New List(Of NameValuePair)
						externalMembers.Add(New NameValuePair("YourMember1Name","YourMember1Value"))
						externalMembers.Add(New NameValuePair("YourMember2Name","YourMember2Value"))
						Return externalMembers
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace