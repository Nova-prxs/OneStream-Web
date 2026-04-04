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

Namespace OneStream.BusinessRule.Extender.UTI_TableFromFile3
	
	Public Class MainClass
		
#Region "Main"
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
				
				Try
					
					Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)					
					
					' Personalizar para cada subida
					'----------------------------------------------------------------------------------
'					Dim filePath As String = "\Documents\Users\Nova2\OS_RESPONSABLE_CEBES.csv"		
'					Dim tableName As String = "XFC_Responsible_CEBES"
'					Dim fieldTokens As New List(Of String)		
						
'						fieldTokens.Add("xfText#:[cebe]")
'						fieldTokens.Add("xfText#:[type]")
'						fieldTokens.Add("xfText#:[ph_am]")
'						fieldTokens.Add("xfText#:[area_manager]")
'						fieldTokens.Add("xfText#:[ph_ubication]")
'						fieldTokens.Add("xfText#:[ubication]")
'						fieldTokens.Add("xfText#:[ph_director]")
'						fieldTokens.Add("xfText#:[director]")
'						fieldTokens.Add("xfText#:[ph_organization]")
'						fieldTokens.Add("xfText#:[region]")
'						fieldTokens.Add("xfText#:[ph_consultantfq]")
'						fieldTokens.Add("xfText#:[consultant]")
'						fieldTokens.Add("xfText#:[ph_geographic]")
'						fieldTokens.Add("xfText#:[regional]")
'						fieldTokens.Add("xfText#:[ph_business]")
'						fieldTokens.Add("xfText#:[business]")
'						fieldTokens.Add("xfText#:[ph_ginvips]")
'						fieldTokens.Add("xfText#:[ginvips]")
						
						
'					Dim filePath As String = "\Documents\Users\Nova2\OS_COMPARABILIDAD_CEBES_DESC_2024.csv"		
'					Dim tableName As String = "XFC_ComparativeCEBES"
'					Dim fieldTokens As New List(Of String)		
						
'						fieldTokens.Add("xfText#:[cebe]")
'						fieldTokens.Add("xfText#:[date]")
'						fieldTokens.Add("xfText#:[weeklycomparability]")
'						fieldTokens.Add("xfText#:[annualcomparability]")
'						fieldTokens.Add("xfText#:[desc_weeklycomparability]")
'						fieldTokens.Add("xfText#:[desc_annualcomparability]")
						
						
						
						
'					Dim filePath As String = "\Documents\Users\Nova2\OS_COMPARABILIDAD_DESC2025.csv"		
'					Dim tableName As String = "XFC_ComparativeCEBES"
'					Dim fieldTokens As New List(Of String)		
						
'						fieldTokens.Add("xfText#:[cebe]")
'						fieldTokens.Add("xfText#:[date]")
'						fieldTokens.Add("xfText#:[weeklycomparability]")
'						fieldTokens.Add("xfText#:[annualcomparability]")
'						fieldTokens.Add("xfText#:[desc_weeklycomparability]")
'						fieldTokens.Add("xfText#:[desc_annualcomparability]")
						
						
'					Dim filePath As String = "\Documents\Users\Nova2\OS_REFORMA_CEBESADJUSTED.csv"		
'					Dim tableName As String = "XFC_Renovation_CEBES"
'					Dim fieldTokens As New List(Of String)		
						
'						fieldTokens.Add("xfInt#:[id]")
'						fieldTokens.Add("xfText#:[cebe]")
'						fieldTokens.Add("xfText#:[renovation_start_date]")
'						fieldTokens.Add("xfText#:[renovation_end_date]")


'					Dim filePath As String = "\Documents\Users\Nova2\OS_VENTAS_DIARIAS_ADJUSTED.csv"		
'					Dim tableName As String = "XFC_RawSales"	
'					Dim fieldTokens As New List(Of String)						
						
'						fieldTokens.Add("xfText#:[date]")
'						fieldTokens.Add("xfText#:[unit_code]")
'						fieldTokens.Add("xfText#:[cod_channel3]")
'						fieldTokens.Add("xfDbl#:[sales_net]")
'						fieldTokens.Add("xfDbl#:[sales_gross]")						
'						fieldTokens.Add("xfDbl#:[customers]")
'						fieldTokens.Add("xfDbl#:[num_ticket]")
'						fieldTokens.Add("xfDbl#:[AT]")
'						fieldTokens.Add("xfDbl#:[avg_invoice]")
						
						
'					Dim filePath As String = "\Documents\Users\Nova2\OS_MAESTRO_CUENTASADJUSTED.csv"		
'					Dim tableName As String = "XFC_Accounts"	
'					Dim fieldTokens As New List(Of String)						
						
'						fieldTokens.Add("xfText#:[account_number]")
'						fieldTokens.Add("xfText#:[descriptive]")
'						fieldTokens.Add("xfText#:[name]")
'						fieldTokens.Add("xfText#:[plan_account]")
'						fieldTokens.Add("xfText#:[balance_account]")						
				
						
					Dim filePath As String = "\Documents\Users\Nova2\OS_COSTES_PERSONAL_CONCAT.csv"		
					Dim tableName As String = "XFC_PersonnelCost"	
					Dim fieldTokens As New List(Of String)						
						
						fieldTokens.Add("xfText#:[year_month]")
						fieldTokens.Add("xfText#:[cebe]")
						fieldTokens.Add("xfText#:[employee_category]")
						fieldTokens.Add("xfDbl#:[night_hs]")
						fieldTokens.Add("xfDbl#:[supplementary_hs]")						
						fieldTokens.Add("xfDbl#:[productive_hs_total]")
						fieldTokens.Add("xfDbl#:[regular_hs]")
						fieldTokens.Add("xfDbl#:[holidays]")
						fieldTokens.Add("xfDbl#:[overtime]")
						fieldTokens.Add("xfDbl#:[night_hs]")
						fieldTokens.Add("xfDbl#:[paid_leave]")						
						fieldTokens.Add("xfDbl#:[unpaid_leave]")
						fieldTokens.Add("xfDbl#:[IT_long_term_leave]")
						fieldTokens.Add("xfDbl#:[training]")
						fieldTokens.Add("xfDbl#:[absances_sanctions]")
						fieldTokens.Add("xfDbl#:[absences]")

						
						
'					Dim filePath As String = "\Documents\Users\Nova2\OS_COSTES_TEORICOS_CONCAT.csv"		
'					Dim tableName As String = "XFC_TheoreticalCosts"	
'					Dim fieldTokens As New List(Of String)						
						
'						fieldTokens.Add("xfText#:[date]")
'						fieldTokens.Add("xfText#:[cebe]")
'						fieldTokens.Add("xfText#:[channel3]")
'						fieldTokens.Add("xfDbl#:[theo_cost_sales_units]")
						
						
						
'					Dim filePath As String = "\Documents\Users\Nova2\OS_COSTES_TEORICOS_DESPERDICIO_PERSONAL_CONCAT.csv"		
'					Dim tableName As String = "XFC_TheoreticalWastePersonnelCosts"
'					Dim fieldTokens As New List(Of String)						
						
'						fieldTokens.Add("xfText#:[date]")
'						fieldTokens.Add("xfText#:[cebe]")
'						fieldTokens.Add("xfDbl#:[waste]")
'						fieldTokens.Add("xfDbl#:[personnel_cost]")

						
'					Dim filePath As String = "\Documents\Users\Nova2\OS_VENTAS_DIARIAS_2023y2024ADJUSTED.csv"		
'					Dim tableName As String = "XFC_RawSales"	
'					Dim fieldTokens As New List(Of String)						
						
'						fieldTokens.Add("xfText#:[date]")
'						fieldTokens.Add("xfText#:[unit_code]")
'						fieldTokens.Add("xfText#:[cod_channel3]")
'						fieldTokens.Add("xfDbl#:[sales_net]")
'						fieldTokens.Add("xfDbl#:[sales_gross]")						
'						fieldTokens.Add("xfDbl#:[customers]")
'						fieldTokens.Add("xfDbl#:[num_ticket]")
'						fieldTokens.Add("xfDbl#:[AT]")
'						fieldTokens.Add("xfDbl#:[avg_invoice]")

					
						
'					Dim filePath As String = "\Documents\Users\Nova2\OS_P&L.csv"		
'					Dim tableName As String = "XFC_PL"	
'					Dim fieldTokens As New List(Of String)						
						
'						fieldTokens.Add("xfText#:[scenario]")
'						fieldTokens.Add("xfText#:[period]")
'						fieldTokens.Add("xfText#:[company]")
'						fieldTokens.Add("xfText#:[accounting_account]")
'						fieldTokens.Add("xfText#:[ceco]")						
'						fieldTokens.Add("xfDbl#:[accumulated_balance]")
'						fieldTokens.Add("xfText#:[currency]")
'						fieldTokens.Add("xfText#:[accounts_chart]")
						
						
						
						
'					Dim filePath As String = "\Documents\Users\Nova2\OS_MAESTRO_CUENTAS.csv"		
'					Dim tableName As String = "XFC_Accounts"	
'					Dim fieldTokens As New List(Of String)						
						
'						fieldTokens.Add("xfText#:[account_number]")
'						fieldTokens.Add("xfText#:[descriptive]")
'						fieldTokens.Add("xfText#:[name]")
'						fieldTokens.Add("xfText#:[plan_account]")				
'						fieldTokens.Add("xfText#:[balance_account]")

'					Dim filePath As String = "\Documents\Users\Nova2\OS_JERARQUIA_SALAS.csv"		
'					Dim tableName As String = "XFC_ChannelHierarchy"	
'					Dim fieldTokens As New List(Of String)						
						
'						fieldTokens.Add("xfText#:[cod_channel1]")
'						fieldTokens.Add("xfText#:[desc_channel1]")
'						fieldTokens.Add("xfText#:[cod_channel2]")
'						fieldTokens.Add("xfText#:[desc_channel2]")
'						fieldTokens.Add("xfText#:[cod_channel3]")						
'						fieldTokens.Add("xfText#:[desc_channel3]")

'					Dim filePath As String = "\Documents\Users\Nova\Efecto Calendario.csv"		
'					Dim tableName As String = "XFC_ComparativeDates"	
'					Dim fieldTokens As New List(Of String)						
						
'						fieldTokens.Add("xfText#:[date]")
'						fieldTokens.Add("xfText#:[date_comparable]")
													
					'-----------------------------------------------------------------------------------						
					
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
						
					Dim delimitedLoadResult As TableRangeContent = BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ",", dbLocation, tableName, loadMethod, fieldTokens, True)
					
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