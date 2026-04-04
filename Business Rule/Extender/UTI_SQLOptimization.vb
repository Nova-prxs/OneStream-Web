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

Namespace OneStream.BusinessRule.Extender.UTI_SQLOptimization
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						Select args.NameValuePairs("p_function")
						Case "RebuildAllIndexes"
							' Get all the tables with fragmentation over 5%
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim dt = BRApi.Database.ExecuteSql(
									dbConn,
									"
										DECLARE @DatabaseID int;
										SET @DatabaseID = DB_ID();
										
										SELECT
										    objects.[name] AS table_name
										FROM sys.dm_db_index_physical_stats (@DatabaseID, NULL, NULL, NULL, 'LIMITED') dm_db_index_physical_stats
										INNER JOIN sys.indexes indexes ON dm_db_index_physical_stats.[object_id] = indexes.[object_id] 
										    AND dm_db_index_physical_stats.index_id = indexes.index_id
										INNER JOIN sys.objects objects ON indexes.[object_id] = objects.[object_id]
										INNER JOIN sys.schemas schemas ON objects.[schema_id] = schemas.[schema_id]
										WHERE objects.[type] IN ('U','V') -- tablas y vistas
										    AND objects.is_ms_shipped = 0
										    AND indexes.[type] IN (1,2,3,4) -- tipos de índices válidos
										    AND indexes.is_disabled = 0
										    AND indexes.is_hypothetical = 0
										    AND dm_db_index_physical_stats.alloc_unit_type_desc = 'IN_ROW_DATA'
										    AND dm_db_index_physical_stats.index_level = 0
											AND dm_db_index_physical_stats.avg_fragmentation_in_percent > 5
											AND objects.[name] LIKE 'XFC_%'
									",
									False
								)
								If dt Is Nothing OrElse dt.Rows.Count = 0 Then Return Nothing
								' Rebuild all indexes per table
								For Each row In dt.Rows
									BRApi.Database.ExecuteSql(
									dbConn,
									$"
										ALTER INDEX ALL
										ON dbo.{row("table_name")}
										REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON);
									",
									False
								)
								Next
							End Using
						End Select
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace