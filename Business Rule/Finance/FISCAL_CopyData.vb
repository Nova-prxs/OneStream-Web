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

Namespace OneStream.BusinessRule.Finance.FISCAL_CopyData
	Public Class MainClass
		
		'Instantiate shared functions BR
		Dim UTISharedFunctions As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass()
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					Case Is = FinanceFunctionType.CustomCalculate
						
						#Region "Copy Data"
						
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyData") Then
							'Declare first period of data to copy reserves
							Dim firstCopyDataPeriod As String = "2025M1"
							Dim isFirstCopyDataPeriod As Boolean = api.Pov.Time.Name = firstCopyDataPeriod
							'Get if first calendar and fiscal month to control calculations and logic
							Dim isFirstCalendarMonth As Boolean = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Time", $"T#WFPrior2", True)(0).Member.Name.EndsWith("M1")
							Dim isFirstFiscalMonth As Boolean = api.Pov.Time.Name.EndsWith("M1")
							
							'Clear target data buffer
							Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{api.Pov.Scenario.Name})")
							UTISharedFunctions.ClearDataBuffer(si, api, targetDataBuffer)
							
							'Get reserve accounts if it's not first copy data period, bs and bs result accounts
							Dim bsAccounts As List(Of String) = Me.GetAccountNameList(si, "BS")
							Dim bsResultAccounts As List(Of String) = Me.GetAccountNameList(si, "BSP_A1_VII")
							'Get BS accounts for PnL
							Dim bsAccountsForPnL As List(Of String) = bsAccounts.Except(bsResultAccounts).ToList()
							'Periodic reserves must be copied if not first copy data period
							Dim reserveAccounts As New List(Of String)
							If Not isFirstCopyDataPeriod Then
								reserveAccounts = Me.GetAccountNameList(si, "BSP_A1_III")
								reserveAccounts.AddRange(Me.GetAccountNameList(si, "BSP_A1_V"))
								bsAccountsForPnL = bsAccountsForPnL.Except(reserveAccounts).ToList()
							End If
							
							'---------------------------------------------------------- BS COPY -----------------------------------------------------------
							'Get source data buffer and declare expression destination info
							Dim BSPrior2SourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#CONSO:S#R:T#WFPrior2,A#BS.Base)")
							Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#YTD")
							
							'Create a new target data buffer to filter out reserve accounts
							targetDataBuffer = New DataBuffer()
							'Loop through all the source data buffer cells
							For Each sourceCell As DataBufferCell In BSPrior2SourceDataBuffer.DataBufferCells.Values
								'Filter out reserve accounts, BS result accounts and mov and DBE flow
								If (
										isFirstCalendarMonth _
										OrElse Not reserveAccounts.Contains(api.Members.GetMember(dimTypeId.Account, sourceCell.DataBufferCellPk.AccountId).Name)
									) _
									AndAlso Not bsResultAccounts.Contains(api.Members.GetMember(dimTypeId.Account, sourceCell.DataBufferCellPk.AccountId).Name) _
									AndAlso api.Members.GetMember(dimTypeId.Flow, sourceCell.DataBufferCellPk.FlowId).Name <> "MOV" _
									AndAlso (
										api.Members.GetMember(dimTypeId.Flow, sourceCell.DataBufferCellPk.FlowId).Name <> "DBE" _
										OrElse api.Members.GetMember(dimTypeId.Origin, sourceCell.DataBufferCellPk.OriginId).Name = "AdjInput"
									)
									'Set cell to the target data buffer
									targetDataBuffer.SetCell(si, sourceCell, True)
								End If
							Next
							'Set target Data Buffer
							api.Data.SetDataBuffer(targetDataBuffer, expDestInfo,,,,,,,,,,,,,True)
							
							'---------------------------------------------------------- PnL & RESERVES COPY -----------------------------------------------------------
							
							'Get prior 2 and 1 source data buffer; and declare periodic and target data buffers
							Dim prior2SourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#CONSO:S#R:T#WFPrior2)")
							Dim prior1TargetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(T#WFPrior1)")
							Dim periodicTargetDataBuffer As New DataBuffer()
							Dim targetFullDataBuffer As New DataBuffer()
							
							'Control periodic data buffer
							'If it's first calendar month, periodic is just the prior 2
							If isFirstCalendarMonth Then
								periodicTargetDataBuffer = prior2SourceDataBuffer
							Else
								'Get prior 3 source data buffer and calculate periodic target data buffer
								Dim prior3SourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#CONSO:S#R:T#WFPrior3)")
								periodicTargetDataBuffer = prior2SourceDataBuffer - prior3SourceDataBuffer
							End If
							
							'Control target full data buffer
							'If it's first fiscal month, target full data buffer doesn't use prior 1 target data buffer
							If isFirstFiscalMonth Then	
								targetFullDataBuffer = periodicTargetDataBuffer
							Else
								targetFullDataBuffer = prior1targetDataBuffer + periodicTargetDataBuffer
							End If
							
							'Declare target data buffer to filter out BS accounts (reserves must be copied too)
							targetDataBuffer = New DataBuffer()
							'Loop through all the target full data buffer cells
							For Each sourceCell As DataBufferCell In targetFullDataBuffer.DataBufferCells.Values
								'Filter out BS accounts for PnL and mov and DBE flow
								If (
										Not isFirstCalendarMonth _
										OrElse Not reserveAccounts.Contains(api.Members.GetMember(dimTypeId.Account, sourceCell.DataBufferCellPk.AccountId).Name)
									) _
									AndAlso Not bsAccountsForPnL.Contains(api.Members.GetMember(dimTypeId.Account, sourceCell.DataBufferCellPk.AccountId).Name) _
									AndAlso api.Members.GetMember(dimTypeId.Flow, sourceCell.DataBufferCellPk.FlowId).Name <> "MOV" _
									AndAlso (
										api.Members.GetMember(dimTypeId.Flow, sourceCell.DataBufferCellPk.FlowId).Name <> "DBE" _
										OrElse api.Members.GetMember(dimTypeId.Origin, sourceCell.DataBufferCellPk.OriginId).Name = "AdjInput"
									)
									'Set cell to the target data buffer
									targetDataBuffer.SetCell(si, sourceCell, True)
								End If
							Next
							'Set target Data Buffer
							api.Data.SetDataBuffer(targetDataBuffer, expDestInfo,,,,,,,,,,,,,True)
							
							'---------------------------------------------------------- PERIMETER COPY -----------------------------------------------------------
							'Get source data buffer
							Dim PerimeterPrior2SourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#CONSO:S#R:T#WFPrior2,A#PERIMETRO.Base)")
							'Set target Data Buffer
							api.Data.SetDataBuffer(PerimeterPrior2SourceDataBuffer, expDestInfo,,,,,,,,,,,,,True)
							
						#End Region
						
						End If
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Functions"
		
		#Region "Helper Functions"
		
		#Region "Get Reserve Accounts"
		
		Private Function GetAccountNameList(ByVal si As SessionInfo, ByVal ancestorMemberName As String) As List(Of String)
			'Get reserves accounts
			Dim accountList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Accounts", $"A#{ancestorMemberName}.Base", True)
			'Create a string
			Dim nameList As List(Of String) = accountList.Select(Function(account) account.Member.Name).ToList()
			
			Return nameList
			
		End Function
		
		#End Region
		
		#End Region
		
		#End Region
		
	End Class
End Namespace