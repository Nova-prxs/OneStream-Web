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

Namespace OneStream.BusinessRule.Extender.CFX_CreateMappingTable
	Public Class MainClass
'------------------------------------------------------------------------------------------------------------
'Reference Code: 		CFX_CreateMappingTable
'
'Description:			Used to create or delete the cash flow (CFX) mapping table 
'
'Use Examples:	        Uncomment the needed rows (CREATE TABLE / DROP TABLE) and run the rule from the top ribbon button ("Execute Extender")
'						CAUTION: Only use "DROP TABLE" when you are aware of the consequences (the table will be deleted, as will all the mappings!)
'
'Created By:			Henning Windhagen
'
'Date Created:			21-03-2023
'------------------------------------------------------------------------------------------------------------	
		
		Private ReadOnly tableName As String = "CashFlowMapping"
		Private ReadOnly tableNameToDelete As String = "CashFlowMapping"
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
'					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
'						'Possibly use this to execute this table creation rule from a dashboard
											
					Case Is = ExtenderFunctionType.Unknown
						
						'Define SQL script to create table 
						Dim sqlScript As New List(Of String)
						
						'Create table
						sqlScript.Add($"CREATE TABLE {tableName}( " _ 
							& "SourceAccount [NVARCHAR](50) , " _ 
							& "SourceAccountID [BIGINT] , " _ 
							& "SourceAccountDescription [NVARCHAR](100) , " _ 
							& "SourceAccountGroup [NVARCHAR](2) , " _ 
							& "ActiveAccount [BIT] , " _ 
							& "SourceFlow [NVARCHAR](50) , " _ 
							& "SourceFlowID [BIGINT] , " _ 
							& "SourceFlowDescription [NVARCHAR](100) , " _ 
							& "ActiveFlow [BIT] , " _ 
							& "FlowInAccountConstraint [BIT] , " _ 
							& "TargetCashFlow [NVARCHAR](50) , " _ 
							& "TargetCashFlowID [BIGINT] , " _ 
							& "Signage [NVARCHAR](20), " _ 
							& "PRIMARY KEY CLUSTERED " _ 
							& "( " _ 
							& "[SourceAccount] ASC, " _ 
							& "[SourceFlow] ASC " _ 
							& ") " _ 
							& ") " )

						'New script adding a new column with GUID as PK for increased compatibility with Spreadsheet BRs
'						sqlScript.Add($"CREATE TABLE {tableName}( " _ 
'							& "PK UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID() , " _ 	
'							& "SourceAccount [NVARCHAR](50) , " _ 
'							& "SourceAccountID [BIGINT] , " _ 
'							& "SourceAccountDescription [NVARCHAR](100) , " _ 
'							& "SourceAccountGroup [NVARCHAR](2) , " _ 
'							& "ActiveAccount [BIT] , " _ 
'							& "SourceFlow [NVARCHAR](50) , " _ 
'							& "SourceFlowID [BIGINT] , " _ 
'							& "SourceFlowDescription [NVARCHAR](100) , " _ 
'							& "ActiveFlow [BIT] , " _ 
'							& "FlowInAccountConstraint [BIT] , " _ 
'							& "TargetCashFlow [NVARCHAR](50) , " _ 
'							& "TargetCashFlowID [BIGINT] , " _ 
'							& "Signage [NVARCHAR](20)) " ) '_ 
						'Alter table to sort by column(s) --> not working when tested
'						sqlScript.Add($"ALTER TABLE {tableName} " _ 
'							& "ORDER BY " _ 
'							& "[SourceAccount] ASC, " _ 
'							& "[SourceFlow] ASC " _ 
'							& " " )
							
'						Delete table
'						sqlScript.Add($"DROP TABLE [{tableNameToDelete}]")

						'Execute SQL command
						Me.ExecuteSql(si, sqlScript)
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		' ### Helper Subs ###
        Private Sub ExecuteSql(ByVal si As SessionInfo, ByVal script As List(Of String))
            If Not script.Count = 0                                                                                   
                Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                            
                    'trigger only one sql cmd
                    Dim sqlCmd As String = String.join(vbnewline,script)
                    
                    BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, True)
					
                End Using                                                                               
            End If  
        End Sub
		
	End Class
End Namespace