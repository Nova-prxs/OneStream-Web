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
Imports Workspace.RESTAPIFramework.RESTAPIRequestManager.Classes

Namespace OneStream.BusinessRule.Connector.CON_S4BSLoad
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		' Reference "RESTAPIFramework_SharedFunctions" and "RESTAPIRequestManager"
		Dim RESTAPISharedFunctionsBR As New OneStream.BusinessRule.Finance.RESTAPIFramework_SharedFunctions.MainClass
		Dim RESTAPIRequestManagerBR As New Workspace.RESTAPIFramework.RESTAPIRequestManager.Utils()
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As Object
			Try
				
				'Get the Field name list or load the data    
                Select Case args.ActionType
                Case Is = ConnectorActionTypes.GetFieldList
						
                    'Return Field Name List
					'In case we use OneStream tables
					Dim fieldList As New List(Of String)
					fieldList.Add("CompanyCode")
					fieldList.Add("FiscalYear")
					fieldList.Add("FiscalPeriod")
					fieldList.Add("GLAccount")
					fieldList.Add("CostCenter")
					fieldList.Add("ProfitCenter")
					fieldList.Add("AmountInCompanyCodeCurrency")
					fieldList.Add("CompanyCodeCurrency")
					fieldList.Add("FinancialTransactionType")
					fieldList.Add("WBSElementInternalID")
					fieldList.Add("OrderID")
					fieldList.Add("Supplier")
					fieldList.Add("Customer")
					fieldList.Add("PartnerCompany")
					fieldList.Add("Product")
					
					Return fieldList
                        
                Case Is = ConnectorActionTypes.GetData
				
					' Get Workflow information
					Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, api.WorkflowUnitPk.TimeKey)
					Dim oTime As TimeMemberSubComponents = BRApi.Finance.Time.GetSubComponentsFromName(si, wfTime)
					Dim wfYear As Integer = oTime.Year
					Dim wfMonth As Integer = oTime.Month
					
					' Get profit center first letters based on the parent workflow description
					Dim profitCenterBeginning As String = BRApi.Workflow.Metadata.GetProfile(si, api.WorkflowProfile.ParentProfileKey).Description
					
					' Get company id based on entity id
					Dim companyDt As New DataTable
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						companyDt = BRApi.Database.ExecuteSql(
							dbConn,
							"
							SELECT TOP 1 company_id
							FROM XFC_MAIN_MASTER_ProfitCenters WITH(NOLOCK)
							WHERE LEFT(entity_id, 5) = @profitCenterBeginning
							",
							New List(Of DbParamInfo) From {
								New DbParamInfo("profitCenterBeginning", profitCenterBeginning)
							},
							False
						)
					End Using
					
					' Validate output
					If companyDt.Rows.Count = 0 Then Throw New Exception(
						$"error getting company code by entity id: no company id registered for entity id '{profitCenterBeginning}'"
					)
					Dim companyId = companyDt.Rows(0)("company_id")
					If String.IsNullOrEmpty(companyId) Then Throw New Exception(
						$"error getting company code by entity id: no company id registered for entity id '{profitCenterBeginning}'"
					)
					
					' Declare and populate parameter dict
					Dim parameterDict As New Dictionary(Of String, String)
					parameterDict("year") = wfYear.ToString
					parameterDict("month") = wfMonth.ToString.PadLeft(3, "0"c)
					parameterDict("company_code") = companyId
					parameterDict("account_filter") = "and (GLAccount ge '10000000' and GLAccount le '59999999')"
					
					' Get data
					Dim dt As New DataTable
					Dim dataRequest As RESTAPIRequest = RESTAPIRequestManagerBR.GetSavedRequest(si, "ZFI_BALANCE_YTD")
					dt = dataRequest.GetDataTable(, parameterDict)
					
					' Create parsedDt schema same as dt
					Dim parsedDt As New DataTable()
					For Each column As DataColumn In dt.Columns
					    parsedDt.Columns.Add(column.ColumnName, column.DataType)
					Next
					
					If dt.Rows.Count > 0 Then
					    For Each row As DataRow In dt.Rows
					        Dim newRow As DataRow = parsedDt.NewRow()
					
					        ' Copy all columns values from dt row to newRow
					        For Each col As DataColumn In dt.Columns
					            newRow(col.ColumnName) = row(col.ColumnName)
					        Next
							' If PartnerCompany is empty or whitespace, assign customer or supplier
							Dim partnerCompanyValue As String = If(row.IsNull("PartnerCompany"), "", row("PartnerCompany").ToString().Trim())
					        If String.IsNullOrEmpty(partnerCompanyValue) Then
					            partnerCompanyValue = If(row.IsNull("Customer"), "", row("Customer").ToString().Trim())
					        End If
					        ' If partnerCompanyValue is empty or whitespace, assign Supplier value
					        If String.IsNullOrEmpty(partnerCompanyValue) Then
					            partnerCompanyValue = If(row.IsNull("Supplier"), "", row("Supplier"))
					        End If
							newRow("Customer") = partnerCompanyValue
					
					        parsedDt.Rows.Add(newRow)
					    Next
					End If
					
					api.Parser.ProcessDataTable(si, parsedDt, False, api.ProcessInfo)
					
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace