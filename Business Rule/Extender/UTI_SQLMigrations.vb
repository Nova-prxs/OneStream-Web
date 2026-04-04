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

Namespace OneStream.BusinessRule.Extender.UTI_SQLMigrations
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
					
				Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)						
					
					Dim Query As String =  "
					IF OBJECT_ID(N'XFC_RAW_TrendAdjustmentsV2', N'U') IS NULL
					BEGIN
						CREATE TABLE XFC_RAW_TrendAdjustmentsV2 (	
							unit_code VARCHAR(255),
							type VARCHAR(50),
							channel VARCHAR(50),
							year INTEGER,
							w20 DEC(18,2),
						    w21 DEC(18,2),
						    w22 DEC(18,2),
						    w23 DEC(18,2),
						    w24 DEC(18,2),
						    w25 DEC(18,2),
						    w26 DEC(18,2),
						    w27 DEC(18,2),
						    w28 DEC(18,2),
						    w29 DEC(18,2),
						    w30 DEC(18,2),
						    w31 DEC(18,2),
						    w32 DEC(18,2),
						    w33 DEC(18,2),
						    w34 DEC(18,2),
						    w35 DEC(18,2),
						    w36 DEC(18,2),
						    w37 DEC(18,2),
						    w38 DEC(18,2),
						    w39 DEC(18,2),
						    w40 DEC(18,2),
						    w41 DEC(18,2),
						    w42 DEC(18,2),
						    w43 DEC(18,2),
						    w44 DEC(18,2),
						    w45 DEC(18,2),
						    w46 DEC(18,2),
						    w47 DEC(18,2),
						    w48 DEC(18,2),
						    w49 DEC(18,2),
						    w50 DEC(18,2),
						    w51 DEC(18,2),
						    w52 DEC(18,2),
						    w53 DEC(18,2)
						);
					END;
					
					IF OBJECT_ID(N'XFC_TrendAdjustmentsV2', N'U') IS NULL
					BEGIN
						CREATE TABLE XFC_TrendAdjustmentsV2 (	
							unit_code VARCHAR(255),
							type VARCHAR(50),
							channel VARCHAR(50),
							year INTEGER,
							week INTEGER,
							amount_perc DEC(18,2),
							CONSTRAINT PK_XFC_TrendAdjustmentsV2 PRIMARY KEY (unit_code, type, channel, year, week)
						);
					END;
					
					CREATE INDEX idx_unit_code ON XFC_TrendAdjustmentsV2 (unit_code);
					CREATE INDEX idx_unit_code_year ON XFC_TrendAdjustmentsV2 (unit_code, year);
					
					IF OBJECT_ID(N'XFC_VIEW_TrendAdjustmentsV2', N'V') IS NULL
					BEGIN
					EXEC('
						CREATE VIEW XFC_VIEW_TrendAdjustmentsV2 AS
						SELECT 
						    unit_code,
							c.description,
							c.sales_brand AS brand,
						    PivotTable.type,
						    PivotTable.channel,
						    PivotTable.year,
						    [20] AS [Week 20],
						    [21] AS [Week 21],
						    [22] AS [Week 22],
						    [23] AS [Week 23],
						    [24] AS [Week 24],
						    [25] AS [Week 25],
						    [26] AS [Week 26],
						    [27] AS [Week 27],
						    [28] AS [Week 28],
						    [29] AS [Week 29],
						    [30] AS [Week 30],
						    [31] AS [Week 31],
						    [32] AS [Week 32],
						    [33] AS [Week 33],
						    [34] AS [Week 34],
						    [35] AS [Week 35],
						    [36] AS [Week 36],
						    [37] AS [Week 37],
						    [38] AS [Week 38],
						    [39] AS [Week 39],
						    [40] AS [Week 40],
						    [41] AS [Week 41],
						    [42] AS [Week 42],
						    [43] AS [Week 43],
						    [44] AS [Week 44],
						    [45] AS [Week 45],
						    [46] AS [Week 46],
						    [47] AS [Week 47],
						    [48] AS [Week 48],
						    [49] AS [Week 49],
						    [50] AS [Week 50],
						    [51] AS [Week 51],
						    [52] AS [Week 52],
						    [53] AS [Week 53]
						FROM 
						    XFC_TrendAdjustmentsV2
						PIVOT (
						    MAX(amount_perc)
						    FOR week IN (
						        [20], [21], [22], [23], [24], [25], [26], [27], [28], [29], [30],
						        [31], [32], [33], [34], [35], [36], [37], [38], [39], [40], [41],
						        [42], [43], [44], [45], [46], [47], [48], [49], [50], [51], [52], [53]
						    )
						) AS PivotTable
						INNER JOIN XFC_CEBES c ON PivotTable.unit_code = c.cebe;
					')
					END;
					"
					
					BRApi.Database.ExecuteSql(dbcon,Query,False)


				End Using					

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
	End Class
End Namespace