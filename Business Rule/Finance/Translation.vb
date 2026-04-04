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

Namespace OneStream.BusinessRule.Finance.Translation
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					Case Is = FinanceFunctionType.Translate
'						If api.Time.GetYearFromId < 2024 And 

'Included by DAGO on 20250224
							Dim turningYear As Integer = 2024
							Dim turningMonth As Integer = 10
							Dim currentYear As Integer = api.Time.GetYearFromId
							Dim currentMonth As Integer = api.Time.GetPeriodNumFromId
'							If (currentYear > turningYear) Or (currentYear = turningYear And currentMonth >= turningMonth) Then
'End of included by DAGO on 20250224

					If api.Pov.Scenario.Name.XFEqualsIgnoreCase("Actual_Dummy") And api.Time.GetYearFromId =2024 
'						api.ExecuteDefaultTranslation
						Translation_NEW(si, api)
'					ElseIf api.Pov.Scenario.Name.XFEqualsIgnoreCase("Actual") And api.Time.GetYearFromId >=2025 'Commented out by DAGO on 20250224
					ElseIf api.Pov.Scenario.Name.XFEqualsIgnoreCase("Actual") And ((currentYear > turningYear) Or (currentYear = turningYear And currentMonth >= turningMonth))
'					  api.LogMessage("AN " & api.Pov.Entity.Name) 
					  Translation_NEW(si, api)
					 ElseIf (api.Time.GetYearFromId <= 2024 And 
						Not api.Pov.Scenario.Name.XFEqualsIgnoreCase("Actual_Dummy") And 
						Not api.Pov.Scenario.Name.XFContainsIgnoreCase("Budget") And
						Not api.Pov.Scenario.Name.XFContainsIgnoreCase("Q_FCST") And
						Not api.Pov.Scenario.Name.XFContainsIgnoreCase("5YP") And
						Not api.Pov.Scenario.Name.XFContainsIgnoreCase("FC_") And
						Not api.Pov.Scenario.Name.XFContainsIgnoreCase("Actual_OP")And
						Not api.Pov.Scenario.Name.XFContainsIgnoreCase("Actual_at_OP")) Or
						    api.Pov.Scenario.Name.XFContainsIgnoreCase("ESG") 
							api.ExecuteDefaultTranslation
						Else
'							If api.Pov.Entity.Name.XFEqualsIgnoreCase("L_DEL")
'								api.ExecuteDefaultTranslation
'							Else 
							Translation(si, api)
'							End If
						End If
						
					End Select
					
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Translation"
		Private Sub Translation(ByVal si As SessionInfo, ByVal api As FinanceRulesApi)
			Try
				'-- Clear all previously Translated data from the translated member.
				api.Data.ClearCalculatedData(False, True, False)
				Dim ActiveEntity As Boolean = api.Entity.InUse()
				If ActiveEntity And api.Cons.IsForeignCurrencyForEntity
					'Getting rates
					Dim RateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability '"ClosingRate_PLAN"
					Dim CloRate As Decimal =  api.FxRates.GetCalculatedFxRate(Ratetypeclo, Api.Pov.Time.MemberId) '"ClosingRate_PLAN"
					Dim CloRateLY As Decimal =  api.FxRates.GetCalculatedFxRate(Ratetypeclo, API.Time.GetLastPeriodInPriorYear).XFToStringForFormula '"ClosingRate_PLAN"
					Dim RateTypeAVG As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp
					Dim AVGrate As Decimal =  api.FxRates.GetCalculatedFxRate(RateTypeAVG, Api.Pov.Time.MemberId)
					Dim strTime As String = api.Time.GetYearFromId
					Dim SpotRate_BS As Decimal =  api.Data.GetDataCell("C#Local:A#SpotRate_BS:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
					If SpotRate_BS = 0
						SpotRate_BS = CloRate
					End If 
					Dim Rate As Decimal = Nothing
					
					Dim FlowDimPk As DimPk = api.Pov.FlowDim.DimPk
					Dim F_Accrued_interest_Mov_3 As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_Accrued_interest_Mov_3")
					Dim F_CLO As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_CLO")
					Dim F_Accrued_interest_Leases_Mov_3 As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_Accrued_interest_Leases_Mov_3")
'api.LogMessage("AN: " & CloRate & " : " & CloRateLY)

					Dim Years_LC As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"Years_LC")
					Dim Years As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"Years")
					Dim F_Retained_Earnings_Mov_1 As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_Retained_Earnings_Mov_1")
					Dim A970000 As Integer = api.Members.GetMemberId(dimtype.Account.Id,"970000")
					Dim stracctxt3 As String = ""
					Dim txt1Scen As String = api.Scenario.Text(1)
					Dim LastindexScen As Integer = txt1Scen.count(Function(x) x = "#")
					'Get the Databuffer
					Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#YTD")
					Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, "C#Local:V#YTD", destinationInfo)
'					If RateTypeClo.ToString.XFContainsIgnoreCase("PLAN")
'						If LastindexScen > 0
'							Dim Lastperiod As String = txt1Scen.Split("#")(LastindexScen)
'							If Lastperiod.Contains("M")
'api.LogMessage("AN: " & Lastperiod  & " Year " & Lastperiod.Split("M")(0) & " Month " & Lastperiod.Split("M")(1))
''								Lastperiod= Lastperiod.Split("M")(1)
'								If api.Time.GetPeriodNumFromId <=  Lastperiod.Split("M")(1) And 
'								api.Time.GetYearFromId =api.Time.GetYearFromId(si.WorkflowClusterPk.TimeKey)
'								 	RateTypeAVG = api.FxRates.GetFxRateType("AverageRate")
'								 	RateTypeClo = api.FxRates.GetFxRateType("ClosingRate")
'									strTime = api.Pov.Time.MemberId
'							 	End If
'						 	End If
'						 End If

							
'						CloRate = api.FxRates.GetCalculatedFxRate(RateTypeClo.ToString, strTime, api.Entity.GetLocalCurrency.Id, api.Cons.GetCurrency("EUR").Id)
'						AVGrate =  api.FxRates.GetCalculatedFxRate(RateTypeAVG.ToString, strTime, api.Entity.GetLocalCurrency.Id, api.Cons.GetCurrency("EUR").Id)
''						brapi.ErrorLog.LogMessage(SI, "CloRate1 " & CloRate.ToString & " GetYearFromId " &  Api.Time.GetYearFromId & " GetLocalCurrency " & api.Entity.GetLocalCurrency.Name & " EUR " &  api.Cons.GetCurrency("EUR").Name)
'					End If
					If RateTypeClo.ToString.XFContainsIgnoreCase("PLAN")
						CloRate = api.FxRates.GetCalculatedFxRate(RateTypeClo.ToString, api.Members.GetMemberId(dimtype.Time.Id, Api.Time.GetYearFromId), api.Entity.GetLocalCurrency.Id, api.Cons.GetCurrency("EUR").Id)
						AVGrate =  api.FxRates.GetCalculatedFxRate(RateTypeAVG.ToString, api.Members.GetMemberId(dimtype.Time.Id, Api.Time.GetYearFromId), api.Entity.GetLocalCurrency.Id, api.Cons.GetCurrency("EUR").Id)
'						brapi.ErrorLog.LogMessage(SI, "CloRate1 " & CloRate.ToString & " GetYearFromId " &  Api.Time.GetYearFromId & " GetLocalCurrency " & api.Entity.GetLocalCurrency.Name & " EUR " &  api.Cons.GetCurrency("EUR").Name)
					End If
					
					
					If Not sourceDataBuffer Is Nothing Then
						Dim resultDataBuffer As DataBuffer = New DataBuffer()
						For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
							If Not cell.CellStatus.IsNoData Then
								Dim Cell2 As New DataBufferCell(cell)
								Dim AccountType As Integer = api.Account.GetAccountType(Cell.DataBufferCellPk.accountid).Id
								Dim FlowType As Boolean = api.Flow.SwitchType(cell.DataBufferCellPk.Flowid)
								Dim strFlowtxt8_SpotRate_BS As Boolean = api.Flow.Text(cell.DataBufferCellPk.FlowId,8).XFContainsIgnoreCase("SpotRate_BS")
								stracctxt3 = api.Account.Text(cell.DataBufferCellPk.AccountId,3)
'								If cell.DataBufferCellPk.FlowId <> api.Members.GetMemberId(dimtype.Flow.Id, "F_OPE") And 
								If cell.DataBufferCellPk.FlowId <> api.Members.GetMemberId(dimtype.Flow.Id, "EUR_Override") And 
								cell.DataBufferCellPk.FlowId <> api.Members.GetMemberId(dimtype.Flow.Id, "F_GL_Load")' And
								'Not (stracctxt3.XFContainsIgnoreCase("CTA") And 
								'cell.DataBufferCellPk.FlowId = api.Members.GetMemberId(dimtype.Flow.Id, "F_CLO"))
								If cell.DataBufferCellPk.OriginId = DimConstants.AdjInput
									cell.DataBufferCellPk.OriginId = DimConstants.AdjConsolidated
								End If 
									If cell.DataBufferCellPk.FlowId = DimConstants.None Or
									api.Members.IsBase(FlowDimPk, Years_LC, cell.DataBufferCellPk.FlowId) Or
									api.Members.IsBase(FlowDimPk, Years, cell.DataBufferCellPk.FlowId) Or
									cell.DataBufferCellPk.FlowId = F_Retained_Earnings_Mov_1  Or 
									(cell.DataBufferCellPk.FlowId = F_CLO  And cell.DataBufferCellPk.AccountId = A970000)
										rate = AVGrate
									ElseIf strFlowtxt8_SpotRate_BS
										rate = SpotRate_BS
									ElseIf api.Pov.Scenario.Name.XFContainsIgnoreCase("Actual") And 
									cell.DataBufferCellPk.FlowId = api.Members.GetMemberId(dimtype.Flow.Id, "F_OPE")
										rate = CloRateLY
									Else 
										rate = CloRate
									End If
									If rate <> 0
										Cell2.CellAmount  = Cell2.CellAmount * rate
										resultDataBuffer.SetCell(api.DbConnApp.SI, Cell2, True)
									End If 
								End If
							End If
						Next
							api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
					End If
				
					Dim destinationInfo2 As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
					Dim sourceDataBuffer2 As DataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, "C#Local:V#Periodic", destinationInfo)
					If Not sourceDataBuffer2 Is Nothing Then
						Dim resultDataBuffer As DataBuffer = New DataBuffer()
						For Each cell As DataBufferCell In sourceDataBuffer2.DataBufferCells.Values
							If Not cell.CellStatus.IsNoData Then
								If cell.DataBufferCellPk.OriginId = DimConstants.AdjInput
									cell.DataBufferCellPk.OriginId = DimConstants.AdjConsolidated
								End If 
								Dim Cell2 As New DataBufferCell(cell)
								Dim AccountType As Integer = api.Account.GetAccountType(Cell.DataBufferCellPk.accountid).Id
								Dim Defaultcurrency As Boolean = True
								Dim FlowType As Boolean = api.Flow.SwitchType(cell.DataBufferCellPk.Flowid)
								Dim strFlowtxt8_SpotRate_BS As Boolean = api.Flow.Text(cell.DataBufferCellPk.FlowId,8).XFContainsIgnoreCase("SpotRate_BS")
								If Defaultcurrency And strFlowtxt8_SpotRate_BS
									rate = SpotRate_BS
									Cell2.CellAmount  = Cell2.CellAmount * rate
									resultDataBuffer.SetCell(api.DbConnApp.SI, Cell2, True)
								End If
							End If
						Next
							api.Data.SetDataBuffer(resultDataBuffer, destinationInfo2)
						End If
					End If 
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	
		#End Region

		#Region "Translation_NEW"
		Private Sub Translation_NEW(ByVal si As SessionInfo, ByVal api As FinanceRulesApi)
			Try
				'-- Clear all previously Translated data from the translated member.
'				api.Data.ClearCalculatedData(False, True, False)
				Dim ActiveEntity As Boolean = api.Entity.InUse()
				If ActiveEntity And api.Cons.IsForeignCurrencyForEntity
					'Getting rates
					Dim RateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability '"ClosingRate_PLAN"
					Dim CloRate As Decimal =  api.FxRates.GetCalculatedFxRate(Ratetypeclo, Api.Pov.Time.MemberId) '"ClosingRate_PLAN"
					Dim CloRateLY As Decimal =  api.FxRates.GetCalculatedFxRate(Ratetypeclo, API.Time.GetLastPeriodInPriorYear).XFToStringForFormula '"ClosingRate_PLAN"
					Dim RateTypeAVG As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp
					Dim AVGrate As Decimal =  api.FxRates.GetCalculatedFxRate(RateTypeAVG, Api.Pov.Time.MemberId)
					Dim strTime As String = api.Time.GetYearFromId
					Dim SpotRate_BS As Decimal =  api.Data.GetDataCell("C#Local:A#SpotRate_BS:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
					If SpotRate_BS = 0
						SpotRate_BS = CloRate
					End If 
					Dim Rate As Decimal = Nothing
					
					Dim FlowDimPk As DimPk = api.Pov.FlowDim.DimPk
					Dim F_Accrued_interest_Mov_3 As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_Accrued_interest_Mov_3")
					Dim F_CLO As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_CLO")
					Dim F_Accrued_interest_Leases_Mov_3 As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_Accrued_interest_Leases_Mov_3")
'api.LogMessage("AN: " & CloRate & " : " & CloRateLY)

					Dim Years_LC As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"Years_LC")
					Dim Years As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"Years")
					Dim F_Retained_Earnings_Mov_1 As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_Retained_Earnings_Mov_1")
					Dim A970000 As Integer = api.Members.GetMemberId(dimtype.Account.Id,"970000")
					Dim stracctxt3 As String = ""
					Dim txt1Scen As String = api.Scenario.Text(1)
					Dim LastindexScen As Integer = txt1Scen.count(Function(x) x = "#")
					'Get the Databuffer
					Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
					Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, "C#Local:V#Periodic", destinationInfo)

					If RateTypeClo.ToString.XFContainsIgnoreCase("PLAN")
						CloRate = api.FxRates.GetCalculatedFxRate(RateTypeClo.ToString, api.Members.GetMemberId(dimtype.Time.Id, Api.Time.GetYearFromId), api.Entity.GetLocalCurrency.Id, api.Cons.GetCurrency("EUR").Id)
						AVGrate =  api.FxRates.GetCalculatedFxRate(RateTypeAVG.ToString, api.Members.GetMemberId(dimtype.Time.Id, Api.Time.GetYearFromId), api.Entity.GetLocalCurrency.Id, api.Cons.GetCurrency("EUR").Id)
'						brapi.ErrorLog.LogMessage(SI, "CloRate1 " & CloRate.ToString & " GetYearFromId " &  Api.Time.GetYearFromId & " GetLocalCurrency " & api.Entity.GetLocalCurrency.Name & " EUR " &  api.Cons.GetCurrency("EUR").Name)
					End If
					
					
					If Not sourceDataBuffer Is Nothing Then
						Dim resultDataBuffer As DataBuffer = New DataBuffer()
						For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
							If Not cell.CellStatus.IsNoData Then
								Dim Cell2 As New DataBufferCell(cell)
								Dim AccountType As Integer = api.Account.GetAccountType(Cell.DataBufferCellPk.accountid).Id
								Dim FlowType As Boolean = api.Flow.SwitchType(cell.DataBufferCellPk.Flowid)
								Dim strFlowtxt8_SpotRate_BS As Boolean = api.Flow.Text(cell.DataBufferCellPk.FlowId,8).XFContainsIgnoreCase("SpotRate_BS")
								stracctxt3 = api.Account.Text(cell.DataBufferCellPk.AccountId,3)
'								If cell.DataBufferCellPk.FlowId <> api.Members.GetMemberId(dimtype.Flow.Id, "F_OPE") And 
								If cell.DataBufferCellPk.FlowId <> api.Members.GetMemberId(dimtype.Flow.Id, "EUR_Override") And 
								cell.DataBufferCellPk.FlowId <> api.Members.GetMemberId(dimtype.Flow.Id, "F_GL_Load") And
								cell.DataBufferCellPk.FlowId <> api.Members.GetMemberId(dimtype.Flow.Id, "F_Acquisitions") 'Added by DAGO on 20250424 so F_Acquisitions is not duplicated also in below row 306
								If cell.DataBufferCellPk.OriginId = DimConstants.AdjInput
									cell.DataBufferCellPk.OriginId = DimConstants.AdjConsolidated
								End If 
								If cell.DataBufferCellPk.FlowId = DimConstants.None Or
									api.Members.IsBase(FlowDimPk, Years_LC, cell.DataBufferCellPk.FlowId) Or
									api.Members.IsBase(FlowDimPk, Years, cell.DataBufferCellPk.FlowId) Or
									cell.DataBufferCellPk.FlowId = F_Retained_Earnings_Mov_1  Or 
									(cell.DataBufferCellPk.FlowId = F_CLO  And cell.DataBufferCellPk.AccountId = A970000) 
									
										rate = AVGrate
									ElseIf strFlowtxt8_SpotRate_BS
										rate = SpotRate_BS
									ElseIf cell.DataBufferCellPk.FlowId = api.Members.GetMemberId(dimtype.Flow.Id, "F_OPE")
										rate = CloRateLY
									ElseIf cell.DataBufferCellPk.FlowId = F_CLO 'And 
'									Not api.Members.IsBase(API.Pov.AccountDim.DimPk, API.Members.GetMemberId(DIMTYPE.Account.Id,"0"),CELL.DataBufferCellPk.AccountId)
										rate =CloRate 
									Else 
										rate = AVGrate
									End If
									If api.Account.GetAccountType(cell.DataBufferCellPk.AccountId).Name.XFEqualsIgnoreCase("NonFinancial") Or
									api.Members.GetMemberName(dimtype.UD8.Id, cell.DataBufferCellPk.UD8Id).XFContainsIgnoreCase("Volume") Or
									api.Account.GetAccountType(cell.DataBufferCellPk.AccountId).Name.XFEqualsIgnoreCase("Flow")
										rate = 1
									End If
									If rate <> 0
										Cell2.CellAmount  = Cell2.CellAmount * rate
										resultDataBuffer.SetCell(api.DbConnApp.SI, Cell2, True)
									End If 
								End If
							End If
						Next
							api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
					End If
				
					Dim destinationInfo2 As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
					Dim sourceDataBuffer2 As DataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, "C#Local:V#Periodic", destinationInfo)
					If Not sourceDataBuffer2 Is Nothing Then
						Dim resultDataBuffer As DataBuffer = New DataBuffer()
						For Each cell As DataBufferCell In sourceDataBuffer2.DataBufferCells.Values
							If Not cell.CellStatus.IsNoData Then
								If cell.DataBufferCellPk.OriginId = DimConstants.AdjInput
									cell.DataBufferCellPk.OriginId = DimConstants.AdjConsolidated
								End If 
								Dim Cell2 As New DataBufferCell(cell)
								Dim AccountType As Integer = api.Account.GetAccountType(Cell.DataBufferCellPk.accountid).Id
								Dim Defaultcurrency As Boolean = True
								Dim FlowType As Boolean = api.Flow.SwitchType(cell.DataBufferCellPk.Flowid)
								Dim strFlowtxt8_SpotRate_BS As Boolean = api.Flow.Text(cell.DataBufferCellPk.FlowId,8).XFContainsIgnoreCase("SpotRate_BS")
								If Defaultcurrency And strFlowtxt8_SpotRate_BS
									rate = SpotRate_BS
									Cell2.CellAmount  = Cell2.CellAmount * rate
									resultDataBuffer.SetCell(api.DbConnApp.SI, Cell2, True)
								End If
							End If
						Next
							api.Data.SetDataBuffer(resultDataBuffer, destinationInfo2)
						End If
					End If 
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	
		#End Region

		#Region "Translation_Periodic"
		Private Sub Translation_Periodic(ByVal si As SessionInfo, ByVal api As FinanceRulesApi)
			Try
				'-- Clear all previously Translated data from the translated member.
				api.Data.ClearCalculatedData(False, True, False)
				Dim ActiveEntity As Boolean = api.Entity.InUse()
				If ActiveEntity And api.Cons.IsForeignCurrencyForEntity
					'Getting rates
					Dim RateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability '"ClosingRate_PLAN"
					Dim CloRate As Decimal =  api.FxRates.GetCalculatedFxRate(Ratetypeclo, Api.Pov.Time.MemberId) '"ClosingRate_PLAN"
					Dim CloRateLY As Decimal =  api.FxRates.GetCalculatedFxRate(Ratetypeclo, API.Time.GetLastPeriodInPriorYear).XFToStringForFormula '"ClosingRate_PLAN"
					Dim RateTypeAVG As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp
					Dim AVGrate As Decimal =  api.FxRates.GetCalculatedFxRate(RateTypeAVG, Api.Pov.Time.MemberId)
					Dim strTime As String = api.Time.GetYearFromId
					Dim SpotRate_BS As Decimal =  api.Data.GetDataCell("C#Local:A#SpotRate_BS:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
					If SpotRate_BS = 0
						SpotRate_BS = CloRate
					End If 
					Dim Rate As Decimal = Nothing
					
					Dim FlowDimPk As DimPk = api.Pov.FlowDim.DimPk
					Dim F_Accrued_interest_Mov_3 As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_Accrued_interest_Mov_3")
					Dim F_CLO As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_CLO")
					Dim F_Accrued_interest_Leases_Mov_3 As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_Accrued_interest_Leases_Mov_3")
'api.LogMessage("AN: " & CloRate & " : " & CloRateLY)

					Dim Years_LC As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"Years_LC")
					Dim Years As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"Years")
					Dim F_Retained_Earnings_Mov_1 As Integer = api.Members.GetMemberId(dimtype.Flow.Id,"F_Retained_Earnings_Mov_1")
					Dim A970000 As Integer = api.Members.GetMemberId(dimtype.Account.Id,"970000")
					Dim stracctxt3 As String = ""
					Dim txt1Scen As String = api.Scenario.Text(1)
					Dim LastindexScen As Integer = txt1Scen.count(Function(x) x = "#")
					'Get the Databuffer
					Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
					Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, "C#Local:V#Periodic", destinationInfo)

					If RateTypeClo.ToString.XFContainsIgnoreCase("PLAN")
						CloRate = api.FxRates.GetCalculatedFxRate(RateTypeClo.ToString, api.Members.GetMemberId(dimtype.Time.Id, Api.Time.GetYearFromId), api.Entity.GetLocalCurrency.Id, api.Cons.GetCurrency("EUR").Id)
						AVGrate =  api.FxRates.GetCalculatedFxRate(RateTypeAVG.ToString, api.Members.GetMemberId(dimtype.Time.Id, Api.Time.GetYearFromId), api.Entity.GetLocalCurrency.Id, api.Cons.GetCurrency("EUR").Id)
'						brapi.ErrorLog.LogMessage(SI, "CloRate1 " & CloRate.ToString & " GetYearFromId " &  Api.Time.GetYearFromId & " GetLocalCurrency " & api.Entity.GetLocalCurrency.Name & " EUR " &  api.Cons.GetCurrency("EUR").Name)
					End If
					
					
					If Not sourceDataBuffer Is Nothing Then
						Dim resultDataBuffer As DataBuffer = New DataBuffer()
						For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
							If Not cell.CellStatus.IsNoData Then
								Dim Cell2 As New DataBufferCell(cell)
								Dim AccountType As Integer = api.Account.GetAccountType(Cell.DataBufferCellPk.accountid).Id
								Dim FlowType As Boolean = api.Flow.SwitchType(cell.DataBufferCellPk.Flowid)
								Dim strFlowtxt8_SpotRate_BS As Boolean = api.Flow.Text(cell.DataBufferCellPk.FlowId,8).XFContainsIgnoreCase("SpotRate_BS")
								stracctxt3 = api.Account.Text(cell.DataBufferCellPk.AccountId,3)
'								If cell.DataBufferCellPk.FlowId <> api.Members.GetMemberId(dimtype.Flow.Id, "F_OPE") And 
								If cell.DataBufferCellPk.FlowId <> api.Members.GetMemberId(dimtype.Flow.Id, "EUR_Override") And 
								cell.DataBufferCellPk.FlowId <> api.Members.GetMemberId(dimtype.Flow.Id, "F_GL_Load")								
									If cell.DataBufferCellPk.OriginId = DimConstants.AdjInput
										cell.DataBufferCellPk.OriginId = DimConstants.AdjConsolidated
									End If 
									If cell.DataBufferCellPk.FlowId = DimConstants.None Or
									api.Members.IsBase(FlowDimPk, Years_LC, cell.DataBufferCellPk.FlowId) Or
									api.Members.IsBase(FlowDimPk, Years, cell.DataBufferCellPk.FlowId) Or
									cell.DataBufferCellPk.FlowId = F_Retained_Earnings_Mov_1  Or 
									(cell.DataBufferCellPk.FlowId = F_CLO  And cell.DataBufferCellPk.AccountId = A970000) 
									
										rate = AVGrate
									ElseIf strFlowtxt8_SpotRate_BS
										rate = SpotRate_BS
									ElseIf cell.DataBufferCellPk.FlowId = api.Members.GetMemberId(dimtype.Flow.Id, "F_OPE")
										rate = CloRateLY
									ElseIf cell.DataBufferCellPk.FlowId = F_CLO 'And 
'									Not api.Members.IsBase(API.Pov.AccountDim.DimPk, API.Members.GetMemberId(DIMTYPE.Account.Id,"0"),CELL.DataBufferCellPk.AccountId)
										rate =CloRate 
									Else 
										rate = AVGrate
									End If
									If rate <> 0
										Cell2.CellAmount  = Cell2.CellAmount * rate
										resultDataBuffer.SetCell(api.DbConnApp.SI, Cell2, True)
									End If 
								End If
							End If
						Next
							api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
					End If
				
					Dim destinationInfo2 As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
					Dim sourceDataBuffer2 As DataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, "C#Local:V#Periodic", destinationInfo)
					If Not sourceDataBuffer2 Is Nothing Then
						Dim resultDataBuffer As DataBuffer = New DataBuffer()
						For Each cell As DataBufferCell In sourceDataBuffer2.DataBufferCells.Values
							If Not cell.CellStatus.IsNoData Then
								Dim Cell2 As New DataBufferCell(cell)
								Dim AccountType As Integer = api.Account.GetAccountType(Cell.DataBufferCellPk.accountid).Id
								Dim Defaultcurrency As Boolean = True
								Dim FlowType As Boolean = api.Flow.SwitchType(cell.DataBufferCellPk.Flowid)
								Dim strFlowtxt8_SpotRate_BS As Boolean = api.Flow.Text(cell.DataBufferCellPk.FlowId,8).XFContainsIgnoreCase("SpotRate_BS")
								If Defaultcurrency And strFlowtxt8_SpotRate_BS
									rate = SpotRate_BS
									Cell2.CellAmount  = Cell2.CellAmount * rate
									resultDataBuffer.SetCell(api.DbConnApp.SI, Cell2, True)
								End If
							End If
						Next
							api.Data.SetDataBuffer(resultDataBuffer, destinationInfo2)
						End If
					End If 
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	
		#End Region

	End Class
End Namespace