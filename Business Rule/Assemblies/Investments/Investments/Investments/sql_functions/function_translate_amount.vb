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
	Public Class function_translate_amount
		
		'Declare table name
		Dim functionName As String = "TranslateAmount"

		#Region "Get Migration Query"
		
        Public Function GetMigrationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
					IF OBJECT_ID(N'dbo.{functionName}', N'FN') IS NULL
					BEGIN
						EXECUTE('
							CREATE FUNCTION dbo.{functionName}
							(
							    @Year INT,
							    @Month INT,
							    @Type VARCHAR(50),
							    @Source VARCHAR(3),
							    @Target VARCHAR(3),
							    @Amount DECIMAL(18,2)
							)
							RETURNS DECIMAL(18,2)
							AS
							BEGIN
								-- If source and target are the same, return amount
								IF @Source = @Target
									RETURN @Amount
				
							    DECLARE @Rate DECIMAL(18,6)
							    DECLARE @Result DECIMAL(18,2)
							
							    -- Get the exchange rate from the table
							    SELECT @Rate = rate
							    FROM XFC_MAIN_AUX_fxrate
							    WHERE year = @Year
							      AND month = @Month
							      AND type = @Type
							      AND source = @Source
							      AND target = @Target
							
							    -- If no rate found, look for the inverse, else return NULL
							    IF @Rate IS NULL
								BEGIN
									SELECT @Rate = rate
								    FROM XFC_MAIN_AUX_fxrate
								    WHERE year = @Year
								      AND month = @Month
								      AND type = @Type
								      AND source = @Target
								      AND target = @Source
								
									IF @Rate IS NULL
							        	RETURN NULL
				
									SET @Result = @Amount / @Rate
				
									RETURN @Result
								END
							
							    -- Calculate the translated amount
							    SET @Result = @Amount * @Rate
							
							    RETURN @Result
							END
						')
					END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP FUNCTION IF EXISTS dbo.{functionName};
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
