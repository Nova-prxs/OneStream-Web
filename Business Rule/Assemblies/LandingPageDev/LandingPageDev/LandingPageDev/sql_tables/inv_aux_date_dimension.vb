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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class inv_aux_date_dimension
		
		'Declare table name
		Dim tableName As String = "XFC_INV_AUX_date_dimension"

		#Region "Get Migration Query"
		
        Public Function GetMigrationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
					IF OBJECT_ID(N'{tableName}', N'U') IS NULL
					BEGIN
						CREATE TABLE {tableName} (
						    date_key INT PRIMARY KEY,
						    date DATE NOT NULL,
						    day TINYINT NOT NULL,
						    day_of_week TINYINT NOT NULL,
						    day_name VARCHAR(10) NOT NULL,
						    day_of_year SMALLINT NOT NULL,
						    week_of_year TINYINT NOT NULL,
						    month TINYINT NOT NULL,
						    month_name VARCHAR(10) NOT NULL,
						    quarter TINYINT NOT NULL,
						    year SMALLINT NOT NULL,
						    is_weekday BIT NOT NULL,
						    is_holiday BIT NOT NULL
						);
					END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP TABLE IF EXISTS {tableName};
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

		#Region "Get Population Query"
		
        Public Function GetPopulationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Population type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
				 	DECLARE @StartDate DATE = '2020-01-01';
					DECLARE @EndDate DATE = '2040-12-31';
					
					WITH DateCTE AS (
					    SELECT @StartDate AS date
					    UNION ALL
					    SELECT DATEADD(DAY, 1, date)
					    FROM DateCTE
					    WHERE date < @EndDate
					)
					MERGE INTO {tableName} AS target
					USING (
						SELECT 
						    CONVERT(INT, CONVERT(VARCHAR, date, 112)) AS date_key,
						    date,
						    DAY(date) AS day,
						    DATEPART(WEEKDAY, date) AS day_of_week,
						    DATENAME(WEEKDAY, date) AS day_name,
						    DATEPART(DAYOFYEAR, date) AS day_of_year,
						    DATEPART(WEEK, date) AS week_of_year,
						    MONTH(date) AS month,
						    DATENAME(MONTH, date) AS month_name,
						    DATEPART(QUARTER, date) AS quarter,
						    YEAR(date) AS year,
						    CASE WHEN DATEPART(WEEKDAY, date) BETWEEN 2 AND 6 THEN 1 ELSE 0 END AS is_weekday,
						    0 AS is_holiday -- You can update this later for specific holidays
						FROM DateCTE
					) AS source
					ON target.date_key = source.date_key
					WHEN NOT MATCHED THEN
					INSERT (
					    date_key, date, day, day_of_week, day_name, day_of_year, week_of_year,
					    month, month_name, quarter, year, is_weekday, is_holiday
					)
					VALUES (
					    source.date_key, source.date, source.day, source.day_of_week,
						source.day_name, source.day_of_year, source.week_of_year,
						source. month, source.month_name, source.quarter, source.year,
						source.is_weekday, source.is_holiday
					)
					OPTION (MAXRECURSION 0);
				"
				
				'Down
				Dim downQuery As String = $"
					
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

	End Class
End Namespace
