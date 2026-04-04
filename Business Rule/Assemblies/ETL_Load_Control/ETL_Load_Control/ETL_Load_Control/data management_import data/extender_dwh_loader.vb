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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_dwh_loader
	''' <summary>
	''' DWH Loader Extender - Executes Data Warehouse loads from Data Management
	''' 
	''' Usage from Data Management Step:
	''' 1. Single load by LoadId:
	'''    p_function=ExecuteDWHLoad&amp;p_load_id=1
	''' 
	''' 2. Execute all enabled loads:
	'''    p_function=ExecuteAllDWHLoads
	''' 
	''' 3. Override period range (ignores control table values):
	'''    p_function=ExecuteDWHLoad&amp;p_load_id=1&amp;p_from=2025M1&amp;p_to=2025M6
	''' 
	''' Period format: yyyyMm (e.g., 2025M1, 2025M11)
	''' Load types: 'Single' (one period), 'Range' (from-to)
	''' </summary>
	Public Class MainClass
		
		'Reference LOG table class for concurrency control
		Dim LogTable As New Workspace.__WsNamespacePrefix.__WsAssemblyName.STG_ETL_LOAD_LOG()
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						#Region "Execute DWH Load"
						
						'Get function parameter to determine which function to execute
						Dim functionName As String = args.NameValuePairs.XFGetValue("p_function")
						
						If functionName.XFEqualsIgnoreCase("ExecuteDWHLoad") Then
							'Execute single DWH load by LoadId
							Dim loadIdStr As String = args.NameValuePairs.XFGetValue("p_load_id")
							Dim loadId As Integer = 0
							
							If String.IsNullOrEmpty(loadIdStr) OrElse Not Integer.TryParse(loadIdStr, loadId) Then
								Throw New XFException(si, New Exception("Parameter 'p_load_id' is required and must be a valid integer"))
							End If
							
							'Check for optional period overrides
							Dim fromOverride As String = args.NameValuePairs.XFGetValue("p_from")
							Dim toOverride As String = args.NameValuePairs.XFGetValue("p_to")
							
							If Not String.IsNullOrEmpty(fromOverride) Then
								'Execute with period overrides
								Me.ExecuteDWHLoadWithOverride(si, loadId, fromOverride, toOverride)
							Else
								'Execute using control table configuration
								Me.ExecuteDWHLoad(si, loadId)
							End If
							
							Return Nothing
						End If
						
						If functionName.XFEqualsIgnoreCase("ExecuteAllDWHLoads") Then
							'Execute all enabled DWH loads
							Me.ExecuteAllDWHLoads(si)
							
							Return Nothing
						End If
						
						#End Region
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						
				End Select
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "DWH Load Functions"
		
		''' <summary>
		''' Executes a single DWH load using configuration from DWH_LOAD_CONTROL table
		''' </summary>
		''' <param name="loadId">LoadId from XFC_DWH_LOAD_CONTROL</param>
		Private Sub ExecuteDWHLoad(ByVal si As SessionInfo, ByVal loadId As Integer)
			Try
				BRApi.ErrorLog.LogMessage(si, $"[INFO] Starting DWH load for LoadId: {loadId}")
				
				'Get configuration from control table
				Dim config As HelperFunctions.DwhLoadConfig = HelperFunctions.GetDwhLoadConfigById(si, loadId)
				
				If config Is Nothing Then
					Throw New XFException(si, New Exception($"No enabled configuration found in XFC_DWH_LOAD_CONTROL for LoadId: {loadId}"))
				End If
				
				'Execute using the appropriate loader based on target table
				ExecuteDWHLoadInternal(si, config)
				
				BRApi.ErrorLog.LogMessage(si, $"[INFO] DWH load completed for LoadId: {loadId}")
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		''' <summary>
		''' Executes a DWH load with period overrides (ignores control table FromTime/ToTime)
		''' </summary>
		''' <param name="loadId">LoadId from XFC_DWH_LOAD_CONTROL (for table names)</param>
		''' <param name="fromTime">Override FromTime in OneStream format (e.g., 2025M1)</param>
		''' <param name="toTime">Override ToTime in OneStream format (e.g., 2025M6). Empty = current period</param>
		Private Sub ExecuteDWHLoadWithOverride(
			ByVal si As SessionInfo,
			ByVal loadId As Integer,
			ByVal fromTime As String,
			ByVal toTime As String
		)
			Try
				BRApi.ErrorLog.LogMessage(si, $"[INFO] Starting DWH load with override for LoadId: {loadId}, From: {fromTime}, To: {If(String.IsNullOrEmpty(toTime), "current", toTime)}")
				
				'Get configuration from control table
				Dim config As HelperFunctions.DwhLoadConfig = HelperFunctions.GetDwhLoadConfigById(si, loadId)
				
				If config Is Nothing Then
					Throw New XFException(si, New Exception($"No enabled configuration found in XFC_DWH_LOAD_CONTROL for LoadId: {loadId}"))
				End If
				
				'Override period values
				config.FromTime = fromTime
				config.ToTime = toTime
				
				'If fromTime is provided and toTime is empty, determine load type
				If Not String.IsNullOrEmpty(fromTime) AndAlso String.IsNullOrEmpty(toTime) Then
					'Could be Single or Range to current - keep original LoadType from config
				End If
				
				'Execute using the appropriate loader
				ExecuteDWHLoadInternal(si, config)
				
				BRApi.ErrorLog.LogMessage(si, $"[INFO] DWH load with override completed for LoadId: {loadId}")
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		''' <summary>
		''' Executes all enabled DWH loads from control table
		''' </summary>
		Private Sub ExecuteAllDWHLoads(ByVal si As SessionInfo)
			Try
				BRApi.ErrorLog.LogMessage(si, "[DEBUG] ExecuteAllDWHLoads - Starting...")
				
				'Get all enabled configurations
				Dim configs As List(Of HelperFunctions.DwhLoadConfig) = HelperFunctions.GetAllEnabledDwhLoadConfigs(si)
				
				BRApi.ErrorLog.LogMessage(si, $"[DEBUG] Found {If(configs Is Nothing, "NULL", configs.Count.ToString())} configurations")
				
				If configs Is Nothing OrElse configs.Count = 0 Then
					BRApi.ErrorLog.LogMessage(si, "[WARNING] No enabled configurations found in XFC_DWH_LOAD_CONTROL")
					Return
				End If
				
				For Each config As HelperFunctions.DwhLoadConfig In configs
					BRApi.ErrorLog.LogMessage(si, $"[DEBUG] Config found - LoadId: {config.LoadId}, Source: {config.SourceTable}, Target: {config.TargetTable}, From: {config.FromTime}, To: {config.ToTime}")
				Next
				
				Dim successCount As Integer = 0
				Dim errorCount As Integer = 0
				
				For Each config As HelperFunctions.DwhLoadConfig In configs
					Try
						BRApi.ErrorLog.LogMessage(si, $"[INFO] Processing DWH load for LoadId: {config.LoadId}")
						ExecuteDWHLoadInternal(si, config)
						successCount += 1
					Catch ex As Exception
						errorCount += 1
						BRApi.ErrorLog.LogMessage(si, $"[ERROR] Failed to execute DWH load for LoadId {config.LoadId}: {ex.Message}")
						'Continue with next load
					End Try
				Next
				
				BRApi.ErrorLog.LogMessage(si, $"[INFO] All DWH loads completed. Success: {successCount}, Errors: {errorCount}")
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		''' <summary>
		''' Internal method that routes to the appropriate DWH loader based on table name
		''' </summary>
		Private Sub ExecuteDWHLoadInternal(ByVal si As SessionInfo, ByVal config As HelperFunctions.DwhLoadConfig)
			Try
				'Validate configuration
				If String.IsNullOrEmpty(config.SourceTable) OrElse String.IsNullOrEmpty(config.TargetTable) Then
					Throw New XFException(si, New Exception($"SourceTable or TargetTable is empty for LoadId: {config.LoadId}"))
				End If
				
				If String.IsNullOrEmpty(config.FromTime) Then
					Throw New XFException(si, New Exception($"FromTime is required for LoadId: {config.LoadId}"))
				End If
				
				'Check for running process using LogTable class
				If LogTable.HasRunningProcess(si, config.LoadId, config.SourceTable, config.TargetTable) Then
					BRApi.ErrorLog.LogMessage(si, $"[WARNING] DWH load skipped for LoadId: {config.LoadId}. A previous instance is still RUNNING.")
					Return
				End If
				
				'Route to appropriate loader based on base table name
				Dim baseTableName As String = HelperFunctions.GetBaseTableName(config.TargetTable)
				
				Select Case baseTableName.ToUpper()
					Case "FIN_MIX"
						Dim dwhFinMix As New dwh_fin_mix()
						dwhFinMix.PopulateDWHTable(
							si,
							config.SourceTable,
							config.TargetTable,
							config.FromTime,
							config.ToTime,
							config.LoadId,
							config.LoadType
						)
						
					'Add more cases for other DWH tables as needed
					'Case "OTHER_TABLE"
					'    Dim dwhOther As New dwh_other_table()
					'    dwhOther.PopulateDWHTable(...)
					
					Case Else
						'Generic handler - attempt to use dwh_fin_mix as template
						BRApi.ErrorLog.LogMessage(si, $"[WARNING] No specific handler for table {baseTableName}. Using generic loader.")
						Dim dwhGeneric As New dwh_fin_mix()
						dwhGeneric.PopulateDWHTable(
							si,
							config.SourceTable,
							config.TargetTable,
							config.FromTime,
							config.ToTime,
							config.LoadId,
							config.LoadType
						)
				End Select
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		#End Region
		
	End Class
End Namespace
