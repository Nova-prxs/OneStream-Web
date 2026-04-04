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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.tables_data
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						Dim sqlMasterFunction = $"
							INSERT INTO XFC_HR_MASTER_Function (id, function_lvl_one, function_group) VALUES 
								('Accounting', 'FINANCE & CONTROL', 'FINANCE & CONTROL'),
								('After Sales Quality', 'Quality', 'QUALITY'),
								('BOP Purchases', 'PROCUREMENT', 'PROCUREMENT'),
								('Central Management', 'CEO Office', 'MANAGEMENT'),
								('COMMUNICATION', 'COMMUNICATION', 'COMMUNICATION'),
								('Control', 'FINANCE & CONTROL', 'FINANCE & CONTROL'),
								('Factories Manangement', 'MANAGEMENT', 'MANAGEMENT'),
								('Factories Mang. Support Services', 'MANAGEMENT', 'MANAGEMENT'),
								('Hygiene and Saftey', 'HSE', 'HSE'),
								('IS IT', 'IS IT', 'IS IT'),
								('LEGAL', 'Legal', 'Legal'),
								('Logistic', 'SUPPLYCHAIN / LOGISTIC', 'SUPPLYCHAIN / LOGISTIC'),
								('Mass Production', 'Production', 'PRODUCTION'),
								('Meta & Perf. Industrial Excellence', 'Meta & Perf. Industrial Excellence', 'MANAGEMENT'),
								('None', 'NONE', 'OTHERS'),
								('OTHERS', 'OTHERS', 'OTHERS'),
								('Payroll and HR Support', 'HR', 'PEOPLE & ORGANIZATION'),
								('PHF Purchases', 'PROCUREMENT', 'PROCUREMENT'),
								('Process Engineering', 'Process Engineering', 'ENGINEERING'),
								('Product Engineering', 'Product Engineering', 'ENGINEERING'),
								('Production Others', 'Production', 'PRODUCTION'),
								('Production Quality', 'Quality', 'QUALITY'),
								('PROJETS', 'PROJETS', 'PROJETS'),
								('Prototypes Production', 'Production', 'PRODUCTION'),
								('Purchases Quality', 'Quality', 'QUALITY'),
								('Purchasing G&A', 'PROCUREMENT', 'PROCUREMENT'),
								('Real Estate & Facilities', 'Real Estate & Facilities', 'Real Estate & Facilities'),
								('SALES', 'SALES', 'SALES'),
								('Supply Chain', 'SUPPLYCHAIN / LOGISTIC', 'SUPPLYCHAIN / LOGISTIC'),
								('Taxes', 'FINANCE & CONTROL', 'FINANCE & CONTROL');
						
						"
						ExecuteSQL(si, sqlMasterFunction)
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
		
				
		Private Sub ExecuteActionSQL(ByVal si As SessionInfo, ByVal sqlCmd As String)        
			Dim dt As DataTable = Nothing
	        Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)                           
	            BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, False)			
	        End Using   
	    End Sub
		
		Private Function ExecuteSQL(ByVal si As SessionInfo, ByVal sqlCmd As String)        
			Dim dt As DataTable = Nothing
	        Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)                           
	            dt = BRAPi.Database.ExecuteSql(dbConnApp, sqlCmd, True)			
	        End Using   
			Return dt	
	    End Function
		
	End Class
End Namespace
