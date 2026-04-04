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

Namespace OneStream.BusinessRule.Finance.VariableRepository
	Public Class MainClass
'------------------------------------------------------------------------------------------------------------
'Reference Code: 		VariableRepository
'
'Description:			Public variables used as referable repository for other Business Rules
'						 --> This is to ease the process of transfering BRs between applications to provide a single point where all variables are stored
'
'Use Examples:	        First initialize "Dim rep As New OneStream.BusinessRule.Finance.VariableRepository.MainClass" then call variables with "rep.mainReportingCube"
'
'Created By:			Henning Windhagen
'
'Date Created:			28-08-2020
'------------------------------------------------------------------------------------------------------------			
		
		'GENERAL
		Public mainReportingCube As String = "Horse"	
		Public mainScenarioDimName As String = "Scenarios"	
		Public mainEntityDimName As String = "Entities"
		Public mainGroupEntity As String = "Horse_Group"
		Public mainAccountDimName As String = "Accounts"	
		Public topBSAccount As String = "Balance"
		Public topISAccount As String = "P&L"
		Public mainFlowDimName As String = "Flows"	
		Public mainMovementMemberName As String = "F99F"
		Public openingBalanceMember As String = "F00"
		Public closingBalanceMember As String = "F99"
		
		'TOP MEMBERS FOR MAIN REPORTING CUBE
		Public topUD1 As String = "Top_Region_Customer" '"None" if not in use
		Public topUD2 As String = "Top_Product" '"None" if not in use
		Public topUD3 As String = "Top" '"None" if not in use
		Public topUD4 As String = "Top" '"None" if not in use
		Public topUD5 As String = "None" '"None" if not in use
		Public topUD6 As String = "None" '"None" if not in use
		Public topUD7 As String = "None" '"None" if not in use
		Public topUD8 As String = "None" '"None" if not in use
		
		'CFX (Cash Flow)
'		Public U1CashFlowDimName As String = "MainUD1"
'		Public CFTopAccount As String = "CashFlow" 'Top account above IS and BS that includes CFAdjsAccount for manual adjustments
'		Public CFAdjsAccount As String = "CFXAdjs"
'		Public CFTopFlow As String = "CashFlow" 'Top flow above all movements and None
'		Public CFTopMember As String = "GRCASHFLOW" 'total CF, ie. net cash flow minus net CF in BS
'		Public netCFTopMember As String = "NETCF" 'net CF
'		Public netCFinBSTopMember As String = "CLOASS" 'net CF in balance sheet
'		Public CFCashOpening As String = "OPEASS" 	
'		Public CFCashMovement As String = "CHinLASS" 
'		Public targetOriginForMemberID As String = "Import"
'		Public sourceOriginForMemberName As String = "BeforeAdj"

		Public U1CashFlowDimName As String = "Customer"
		Public CFTopAccount As String = "None" 'Top account above IS and BS that includes CFAdjsAccount for manual adjustments
		Public CFAdjsAccount As String = "None"
		Public CFTopFlow As String = "None" 'Top flow above all movements and None
		Public CFTopMember As String = "None" 'total CF, ie. net cash flow minus net CF in BS
		Public netCFTopMember As String = "None" 'net CF
		Public netCFinBSTopMember As String = "None" 'net CF in balance sheet
		Public CFCashOpening As String = "None" 	
		Public CFCashMovement As String = "None" 
		Public targetOriginForMemberID As String = "Import"
		Public sourceOriginForMemberName As String = "BeforeAdj"


			
	End Class
End Namespace