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

Namespace OneStream.BusinessRule.Extender.XPBI_ExportMetadataTable
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
						
				ExportDataToTable(si)
				
				ExportDataToTable_Dynamics(si)

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
		Public Sub ExportDataToTable (ByVal si As SessionInfo)
			
			Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
					
			Dim fieldTokens As New List(Of String)	
			fieldTokens.Add("xfText#:[Cube]")
			fieldTokens.Add("xfText#:[Entity]")
			fieldTokens.Add("xfText#:[Parent]")
			fieldTokens.Add("xfText#:[Cons]")
			fieldTokens.Add("xfText#:[Scenario]")
			fieldTokens.Add("xfText#:[Time]")						
			fieldTokens.Add("xfText#:[View]")
			fieldTokens.Add("xfText#:[Account]")	
			fieldTokens.Add("xfText#:[Flow]")						
			fieldTokens.Add("xfText#:[Origin]")
			fieldTokens.Add("xfText#:[IC]")
			fieldTokens.Add("xfText#:[UD1]")
			fieldTokens.Add("xfText#:[UD2]")
			fieldTokens.Add("xfText#:[UD3]")
			fieldTokens.Add("xfText#:[UD4]")
			fieldTokens.Add("xfText#:[UD5]")
			fieldTokens.Add("xfText#:[UD6]")
			fieldTokens.Add("xfText#:[UD7]")
			fieldTokens.Add("xfText#:[UD8]")
			fieldTokens.Add("xfDec#:[Amount]")
			fieldTokens.Add("xfText#:[HasData]") 
			fieldTokens.Add("xfText#:[Annotation]")
			fieldTokens.Add("xfText#:[Assumptions]")
			fieldTokens.Add("xfText#:[AuditComment]")
			fieldTokens.Add("xfText#:[Footnote]")
			fieldTokens.Add("xfText#:[VarianceExplanation]")
			
			Dim fileName As String = String.Empty
			
			'YTD
			fileName = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName) & "\PowerBI\CONS_PowerBI_ExportData.csv"
			Brapi.ErrorLog.LogMessage(si, "fileNameYTD: " & fileName)
			
			BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileShare, fileName, File.ReadAllBytes(fileName), ",", "Application", "XPBI_Data", "Replace", fieldTokens, True)

			'Periodic
			fileName = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName) & "\PowerBI\CONS_PowerBI_ExportData_Periodic.csv"
			Brapi.ErrorLog.LogMessage(si, "fileNamePeriodic: " & fileName)	
			
			BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileShare, fileName, File.ReadAllBytes(fileName), ",", "Application", "XPBI_Data_Periodic", "Replace", fieldTokens, True)			
			
		End Sub
		
		Public Sub ExportDataToTable_Dynamics (ByVal si As SessionInfo)
			
			Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
					
			Dim fieldTokens As New List(Of String)	
			fieldTokens.Add("xfText#:[Cube]")
			fieldTokens.Add("xfText#:[Entity]")
			fieldTokens.Add("xfText#:[Cons]")
			fieldTokens.Add("xfText#:[Scenario]")
			fieldTokens.Add("xfText#:[Time]")						
			fieldTokens.Add("xfText#:[View]")
			fieldTokens.Add("xfText#:[AuxCol1]")			
			fieldTokens.Add("xfText#:[Flow]")						
			fieldTokens.Add("xfText#:[Origin]")
			fieldTokens.Add("xfText#:[IC]")
			fieldTokens.Add("xfText#:[AuxCol2]")
			fieldTokens.Add("xfText#:[UD2]")
			fieldTokens.Add("xfText#:[UD3]")
			fieldTokens.Add("xfText#:[UD4]")
			fieldTokens.Add("xfText#:[UD5]")
			fieldTokens.Add("xfText#:[UD6]")
			fieldTokens.Add("xfText#:[UD7]")
			fieldTokens.Add("xfText#:[UD8]")
			fieldTokens.Add("xfText#:[Account]")	
			fieldTokens.Add("xfText#:[UD1]")			
			fieldTokens.Add("xfDec#:[Amount]")
			fieldTokens.Add("xfText#:[HasData]") 
			fieldTokens.Add("xfText#:[Annotation]")
			fieldTokens.Add("xfText#:[Assumptions]")
			fieldTokens.Add("xfText#:[AuditComment]")
			fieldTokens.Add("xfText#:[Footnote]")
			fieldTokens.Add("xfText#:[VarianceExplanation]")
			
			'Dynamics
			Dim fileName As String = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName) & "\PowerBI\CONS_PowerBI_ExportData_Dynamics.csv"
			Brapi.ErrorLog.LogMessage(si, "fileNameDynamics: " & fileName)
			
			BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileShare, fileName, File.ReadAllBytes(fileName), ",", "Application", "XPBI_Data_Dynamics", "Replace", fieldTokens, False)

		End Sub				
		
	End Class
	
End Namespace