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

Namespace OneStream.BusinessRule.Extender.UTI_TableFromFile2
	
	Public Class MainClass
		
#Region "Main"
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
				
				Try
					
					Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)					
					
					' Personalizar para cada subida
					'----------------------------------------------------------------------------------
'					Dim filePath As String = "\Documents\Users\Nova2\OS_MAESTRO_CEBES_ADJUSTED.csv"		
'					Dim tableName As String = "XFC_CEBES"
'					Dim fieldTokens As New List(Of String)		
						
'						fieldTokens.Add("xfText#:[cebe]")
'						fieldTokens.Add("xfText#:[description]")
'						fieldTokens.Add("xfText#:[open_date]")
'						fieldTokens.Add("xfText#:[close_date]")
'						fieldTokens.Add("xfText#:[brand]")
'						fieldTokens.Add("xfText#:[location]")
'						fieldTokens.Add("xfText#:[city]")
'						fieldTokens.Add("xfText#:[state]")
'						fieldTokens.Add("xfText#:[region]")
'						fieldTokens.Add("xfText#:[country]")
'						fieldTokens.Add("xfText#:[postal_code]")
'						fieldTokens.Add("xfText#:[unit_type]")
'						fieldTokens.Add("xfText#:[company]")

'					Dim filePath As String = "\Documents\Users\Nova\OS_MAESTRO_FECHA_ADJUSTED_OK2.csv"		
'					Dim tableName As String = "XFC_ComparativeDates"
'					Dim fieldTokens As New List(Of String)		
						
'						fieldTokens.Add("xfText#:[date]")
'						fieldTokens.Add("xfText#:[date_comparable]")
'						fieldTokens.Add("xfText#:[date_comparable_2]")
'						fieldTokens.Add("xfText#:[date_comparable_3]")
'                        fieldTokens.Add("xfText#:[date_comparable_4]")
'						fieldTokens.Add("xfText#:[date_comparable_5]")
'						fieldTokens.Add("xfText#:[date_comparable_6]")
						
'					Dim filePath As String = "\Documents\Users\Nova2\OS_JERARQUIA_CEBES.csv"		
'					Dim tableName As String = "XFC_CEBESHierarchy"
'					Dim fieldTokens As New List(Of String)		
						
'						fieldTokens.Add("xfText#:[hierarchy]")
'						fieldTokens.Add("xfInt#:[id]")
'						fieldTokens.Add("xfText#:[node]")
'						fieldTokens.Add("xfInt#:[father_id]")
'						fieldTokens.Add("xfText#:[cebe]")
'						fieldTokens.Add("xfText#:[description]")

'					Dim filePath As String = "\Documents\Users\Nova\Efecto Calendario.csv"		
'					Dim tableName As String = "XFC_ComparativeDates"	
'					Dim fieldTokens As New List(Of String)						
						
'						fieldTokens.Add("xfText#:[date]")
'						fieldTokens.Add("xfText#:[date_comparable]")

'					Dim filePath As String = "\Documents\Users\Nova2\OS_DESPERDICIO_2023y2024ok.csv"		
'					Dim tableName As String = "XFC_Waste"
'					Dim fieldTokens As New List(Of String)
						
'						fieldTokens.Add("xfText#:[date]")
'						fieldTokens.Add("xfText#:[cebe]")
'						fieldTokens.Add("xfDbl#:[amount]")
						
					Dim filePath As String = "\Documents\Users\Nova2\OS_JERARQUIA_CUENTAS.csv"		
					Dim tableName As String = "XFC_AccountHierarchy"
					Dim fieldTokens As New List(Of String)
						
						fieldTokens.Add("xfText#:[hierarchy]")
						fieldTokens.Add("xfInt#:[id]")
						fieldTokens.Add("xfText#:[node]")
						fieldTokens.Add("xfInt#:[father_id]")
						fieldTokens.Add("xfText#:[account_number]")
						fieldTokens.Add("xfText#:[description]")
						
'					Dim filePath As String = "\Documents\Users\Nova2\OS_ZPARAMCARGAFI_ADJUSTED.csv"		
'					Dim tableName As String = "XFC_FILoadZParam"
'					Dim fieldTokens As New List(Of String)
						
'						fieldTokens.Add("xfInt#:[mandt]")
'						fieldTokens.Add("xfText#:[parameter]")
'						fieldTokens.Add("xfText#:[value]")
'						fieldTokens.Add("xfText#:[beginning_date]")
'						fieldTokens.Add("xfText#:[end_date]")
'						fieldTokens.Add("xfText#:[modification_user]")
'						fieldTokens.Add("xfText#:[modification_date]")
'						fieldTokens.Add("xfText#:[active]")
'						fieldTokens.Add("xfText#:[description]")
'						fieldTokens.Add("xfText#:[modification_hour]")
													
					'-----------------------------------------------------------------------------------						

'					Using dbConn As DbConnInfo = BRApi.Database.CreateDbConnInfo(si, DbLocation.External, "SICPreSQL")
					
'						Dim selectQuery As String = "	SELECT *
'														FROM XFC_RawSales"
						
'						Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
						
'						BRApi.ErrorLog.LogMessage(si, dt.Rows.Count().ToString)
					
'					End Using
					
					Dim loadResults As List(Of TableRangeContent) = Me.WriteFileToTable(si, filePath, tableName, fieldTokens)
	
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
					loadResults = BRApi.Utilities.LoadCustomTableUsingExcel(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes)

				Else
									
					'Load Delimited File
					'Note: dbLocation, TableName, LoadMethod & Field names must be defined as variables and passed into the load method
					Dim dbLocation As String = "Application" ' "Application"								
					Dim loadMethod As String = "Replace"	 ' Use an expression for partial replace -->  Merge:[(ProductName = 'SomeProduct')] 															
						
					Dim delimitedLoadResult As TableRangeContent = BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
					
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