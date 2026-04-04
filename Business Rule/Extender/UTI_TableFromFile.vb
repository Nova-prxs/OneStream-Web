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

Namespace OneStream.BusinessRule.Extender.UTI_TableFromFile
	
	Public Class MainClass
		
#Region "Main"
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
				
				Try
					
					Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)					
					
					' Personalizar para cada subida
					'----------------------------------------------------------------------------------
'					Dim filePath As String = "\Documents\Users\Nova\POC Ventas_07_05_2024.csv"		
'					Dim tableName As String = "XFC_ActualSales"	
'					Dim fieldTokens As New List(Of String)						
						
'						fieldTokens.Add("xfText#:[date]")
'						fieldTokens.Add("xfText#:[week_day]")
'						fieldTokens.Add("xfText#:[brand]")
'						fieldTokens.Add("xfText#:[unit_code]")
'						fieldTokens.Add("xfText#:[unit_description]")
'						fieldTokens.Add("xfText#:[location]")
'						fieldTokens.Add("xfText#:[weekly_comparability]")
'						fieldTokens.Add("xfText#:[unit_type]")
'						fieldTokens.Add("xfText#:[state]")						
'						fieldTokens.Add("xfText#:[region]")
'						fieldTokens.Add("xfText#:[channel]")
'						fieldTokens.Add("xfText#:[indicator]")						
'						fieldTokens.Add("xfDbl#:[sales]")
'						fieldTokens.Add("xfDbl#:[sales_ppto]")
'						fieldTokens.Add("xfDbl#:[sales_aa]")
'						fieldTokens.Add("xfDbl#:[customers]")
'						fieldTokens.Add("xfDbl#:[customers_ppto]")
'						fieldTokens.Add("xfDbl#:[customers_aa]")						
'						fieldTokens.Add("xfDbl#:[AT]")		
'						fieldTokens.Add("xfDbl#:[AT_ppto]")		
'						fieldTokens.Add("xfDbl#:[AT_aa]")	
						
					Dim filePath As String = "\Documents\Users\Nova\Efecto Calendario 2025.csv"		
					Dim tableName As String = "XFC_ComparativeDates"	
					Dim fieldTokens As New List(Of String)						
						
						fieldTokens.Add("xfText#:[date]")
						fieldTokens.Add("xfText#:[date_comparable]")
						fieldTokens.Add("xfText#:[date_comparable_2]")
						fieldTokens.Add("xfText#:[date_comparable_3]")
						fieldTokens.Add("xfText#:[date_comparable_4]")
						fieldTokens.Add("xfText#:[date_comparable_5]")
						fieldTokens.Add("xfText#:[date_comparable_6]")
						
'					Dim filePath As String = "\Documents\Users\Nova2\OS_COSTES_TEORICOS_2023y2024.csv"		
'					Dim tableName As String = "XFC_TheoreticalCosts"	
'					Dim fieldTokens As New List(Of String)						
						
'						fieldTokens.Add("xfText#:[date]")
'						fieldTokens.Add("xfText#:[cebe]")
'						fieldTokens.Add("xfText#:[channel3]")
'						fieldTokens.Add("xfDbl#:[theo_cost_sales_units]")
'						fieldTokens.Add("xfDbl#:[waste]")						
'						fieldTokens.Add("xfDbl#:[personnel_cost]")						
													
'					'-----------------------------------------------------------------------------------						
					
					Dim loadResults As List(Of TableRangeContent) = Me.WriteFileToTable(si, filePath, tableName, fieldTokens)
							
'					Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						
'					Dim InsertQuery As String = "UPDATE XFC_EventEffects
'												 SET ef_year_var = 0"
					
'					Dim truncate As DataTable = BRApi.Database.ExecuteSql(dbcon,InsertQuery,False)
								
'					End Using	
	
					Return Nothing
					
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				End Try
			End Function
			
#End Region

#Region "File Load Helper"

		Private Function WriteFileToTable(ByVal si As SessionInfo, ByVal filePath As String, ByVal tableName As String, ByVal fieldTokens As List(Of String)) As List(Of TableRangeContent)
			Try
				Dim loadResults As New List(Of TableRangeContent)
				
				'Get the upload "TEMP" file from the database file system
				Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath, True, False, False, SharedConstants.Unknown, Nothing, True)				
							
				'Determine what type of file we are loading and execute the load
				If filePath.XFContainsIgnoreCase("xlsx") Then
					
					'Load Excel File
					'Note: dbLocation, TableName, LoadMethod & Field names defined in header of each excel "xft" range.
					loadResults = BRApi.Utilities.LoadCustomTableUsingExcel(si, SourceDataOriginTypes.FromFileUpload, "Temp.xlsx", fileInfo.XFFile.ContentFileBytes)

				Else
									
					'Load Delimited File
					'Note: dbLocation, TableName, LoadMethod & Field names must be defined as variables and passed into the load method
					Dim dbLocation As String = "Application" ' "Application"								
					Dim loadMethod As String = "Merge"	 ' Use an expression for partial replace -->  Merge:[(ProductName = 'SomeProduct')] 															
						
					Dim delimitedLoadResult As TableRangeContent = BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, "Temp.csv", fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
					
					'BRAPI.ErrorLog.LogMessage(si, "2")
					
				End If	
				
				'Delete the temp file
				'BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, filePath)
				
				Return loadResults
								
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
        Private Sub ExecuteSql(ByVal si As SessionInfo, ByVal script As List(Of String))
            If Not script.Count = 0                                                                                   
                Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                    
                    'delete duplicated lines of code
                    script = script.Distinct().ToList
        
                    'trigger only one sql cmd
                    Dim sqlCmd As String = String.join(vbnewline,script)
                    
                    BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, True)
                End Using                                                                               
            End If  
        End Sub		
		
#End Region
		
	End Class
End Namespace