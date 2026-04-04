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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.actions
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType

					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
													
						Dim FunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.functions
						
						Dim dateMod As String 	= DateTime.Today
						Dim sql As String 		= "SELECT 'Build query' AS Message"
						Dim sqlVal As String 	= "SELECT 'Build query' AS Message"
						Dim dtVal As DataTable 	= Nothing
						Dim sMailDictionary As New Dictionary(Of String, String) From {
							{"R0671", "gomes.santos@horse.tech, miguel.castejon@nova-praxis.com, santos.liliana@horse.tech, carolina.c.loureiro-extern@horse.tech"}							
						}
						
						If args.FunctionName.XFEqualsIgnoreCase("FunctionName") Then
							
						#Region "User"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("Edit_Times") Then
							
							Dim sFactory As String = 		args.NameValuePairs.GetValueOrEmpty("factory")
							Dim sProduct As String = 		args.NameValuePairs.GetValueOrEmpty("product")
							Dim sGM As String = 			args.NameValuePairs.GetValueOrEmpty("gm")
							Dim sCC As String = 			args.NameValuePairs.GetValueOrEmpty("cc")
							Dim sTime As String = 			args.NameValuePairs.GetValueOrEmpty("time")
							Dim sTimeUnit As String = 		args.NameValuePairs.GetValueOrEmpty("time_unit")
							Dim sStartDate As String = 		args.NameValuePairs.GetValueOrEmpty("start_date")
							Dim sEndDateOption As String = 	args.NameValuePairs.GetValueOrEmpty("end_date_option")
							Dim sNewValue As String = 		args.NameValuePairs.GetValueOrEmpty("new_value").Replace(",",".")
							Dim sComment As String = 		args.NameValuePairs.GetValueOrEmpty("comment") & $"- Modified by {si.UserName} ({dateMod})"
							
							#Region "Mod. Validation"
							
							sqlVal = $"
							DECLARE 
							    @id_factory      nvarchar(20) = N'{sFactory}',
							    @id_product      nvarchar(50) = N'{sProduct}',
							    @id_averagegroup nvarchar(50) = N'{sGM}';
							
							SELECT COUNT(*) + 1 AS Cnt
							FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
							WHERE t.id_factory      = @id_factory
							  AND t.id_product      = @id_product
							  AND t.id_averagegroup = @id_averagegroup
							  AND t.uotype IN (N'UO1', N'UO2');

							"
							 Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                				dtVal = BRAPi.Database.ExecuteSql(dbConnApp, sqlVal, True)
            				End Using  
							
							If dtVal.Rows.Count > 0 And Convert.ToInt32(dtVal.Rows(0)("Cnt")) = 1 Then Return MessagePopUp("Create the Time")							   
							
							#End Region
							
							#Region "Modify"
							
							sql = $"	
							
					BEGIN TRY
					    BEGIN TRANSACTION;
							
					-- 0) Dclaracion de variables
                            DECLARE
							    @id_factory      		nvarchar(20)  	= N'{sFactory}',
							    @id_product      		nvarchar(50)  	= N'{sProduct}',
							    @id_costcenter 		    nvarchar(50)  	= N'{sCC}',
							    -- @id_averagegroup 		nvarchar(50)  	= N'{sGM}',
							    @uotype          		nvarchar(10)  	= N'{sTime}',
							    @new_value       		decimal(18,8) 	= {sNewValue},     
							    @new_start_date  		date          	= '{sStartDate}',
							    @new_end_date_option  	nvarchar(50)	= N'{sEndDateOption}',
							    @comment         		nvarchar(250) 	= N'{sComment}',
							    @value_unit      		nvarchar(10)  	= N'{sTimeUnit}';    
							
							/* --- convertir SIEMPRE a horas --- */
							DECLARE @new_value_hours decimal(18,8);
							
							IF UPPER(@value_unit) IN (N'H', N'HORA', N'HORAS', N'HR', N'HRS')
							    SET @new_value_hours = @new_value;                  -- ya viene en horas
							ELSE IF UPPER(@value_unit) IN (N'MIN', N'MINS', N'MINUTO', N'MINUTOS')
							    SET @new_value_hours = @new_value / 60.0;           -- convertir de minutos a horas
							ELSE
							BEGIN
							    RAISERROR('Unidad no válida: use H o MIN.', 16, 1);
							    RETURN;
							END
							
							-- Tomamos averagegropu/workcenter del dato actual si existe (para reusarlos en el INSERT)
							DECLARE @id_averagegroup nvarchar(50), @id_workcenter int;
							
							SELECT TOP (1)
							       @id_averagegroup = ISNULL(id_averagegroup, '-1'),
							       @id_workcenter = -1
                            FROM XFC_PLT_MASTER_CostCenter_Hist mc                            
							WHERE mc.id_factory      = @id_factory
							  AND mc.id              = @id_costcenter
							  AND CAST(@new_start_date AS datetime) BETWEEN mc.start_date AND mc.end_date
							ORDER BY mc.start_date DESC;

					-- 1) Si hay una fila que cubre @new_start_date, acotamos su end_date al día anterior
						    UPDATE t
						       SET end_date = DATEADD(DAY, -1, CAST(@new_start_date AS datetime))
						    FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
						    WHERE t.id_factory      = @id_factory
						      AND t.id_product      = @id_product
						      AND t.id_costcenter   = @id_costcenter
						      AND t.uotype          = @uotype
						      AND CAST(@new_start_date AS datetime) BETWEEN t.start_date AND t.end_date;
						
					-- 2) Si la nueva fila NO es 'next_period' (queda abierta a 9999-12-31),
						    --    invalidamos (eliminamos) TODAS las filas futuras/solapadas desde @new_start_date en adelante
						    IF UPPER(@new_end_date_option) <> N'NEXT_PERIOD'
						    BEGIN
						        DELETE t
						        FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
						        WHERE t.id_factory      = @id_factory
						          AND t.id_product      = @id_product
						          AND t.id_costcenter   = @id_costcenter
						          AND t.uotype          = @uotype
						          AND t.start_date >= CAST(@new_start_date AS datetime);
						    END;
						
					-- 3) Calcular end_date de la nueva fila
						    DECLARE @next_start datetime, @new_end_date datetime;
							
						    IF UPPER(@new_end_date_option) = N'NEXT_PERIOD'
						    BEGIN
						        SELECT @next_start = MIN(t.start_date)
						        FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
						        WHERE t.id_factory      = @id_factory
						          AND t.id_product      = @id_product
						          AND t.id_costcenter   = @id_costcenter
						          AND t.uotype          = @uotype
						          AND t.start_date > CAST(@new_start_date AS datetime);
							
						        SET @new_end_date = CASE WHEN @next_start IS NOT NULL
						                                 THEN DATEADD(DAY, -1, @next_start)
						                                 ELSE CAST('9999-12-31T00:00:00' AS datetime)
						                            END;
						    END
						    ELSE
						    BEGIN
						        SET @new_end_date = CAST('9999-12-31T00:00:00' AS datetime);
						    END;
							
						    -- Valores por defecto si no había una fila vigente previa
						    IF @id_averagegroup IS NULL SET @id_costcenter = N'-1';
						    IF @id_workcenter   IS NULL SET @id_workcenter = -1;
							
					-- 4) Insertar el nuevo tiempo
						    INSERT INTO dbo.XFC_PLT_AUX_Production_Actual_Times
						    (
						        id_factory, start_date, end_date, id_product,
						        id_costcenter, id_averagegroup, id_workcenter,
						        uotype, value, comment
						    )
						    VALUES
						    (
						        @id_factory, CAST(@new_start_date AS datetime), @new_end_date, @id_product,
						        @id_costcenter, @id_averagegroup, @id_workcenter,
						        @uotype, @new_value_hours, @comment
						    );
							
					-- 5) Limpieza: Eliminar cualquier registro con start_date > end_date (misma clave)						    
							DELETE t
						    FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
						    WHERE t.start_date > t.end_date
						      AND t.id_factory      = @id_factory
						      AND t.id_product      = @id_product
						      AND t.id_costcenter   = @id_costcenter
						      AND t.uotype          = @uotype;			

					-- 6) Homogeneizar si hay dos registros consecutivos con el mismo valor y un día de diferencia
						UPDATE t
						SET t.start_date = p.start_date_1, 
						    t.end_date = p.end_date_2
						FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
						JOIN (
						    SELECT 
						        t1.id_factory, 
						        t1.id_product, 
						        t1.id_costcenter, 
						        t1.uotype, 
						        t1.value,
						        t1.start_date AS start_date_1, 
						        t1.end_date AS end_date_1, 
						        t2.start_date AS start_date_2, 
						        t2.end_date AS end_date_2
						    FROM dbo.XFC_PLT_AUX_Production_Actual_Times t1
						    JOIN dbo.XFC_PLT_AUX_Production_Actual_Times t2
						        ON t1.id_factory = t2.id_factory
						        AND t1.id_product = t2.id_product
						        AND t1.id_costcenter = t2.id_costcenter
						        AND t1.uotype = t2.uotype
						        AND t1.value = t2.value
						        AND DATEDIFF(DAY, t1.end_date, t2.start_date) = 1
						) p
						    ON t.id_factory = p.id_factory
						    AND t.id_product = p.id_product
						    AND t.id_costcenter = p.id_costcenter
						    AND t.uotype = p.uotype
						    AND t.value = p.value
						    AND t.start_date = p.start_date_2
						    AND t.end_date = p.end_date_2;
						
						-- Eliminar el registro duplicado (el segundo registro que se combinó)
						DELETE t
						FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
						JOIN (
						    SELECT 
						        t1.id_factory, 
						        t1.id_product, 
						        t1.id_costcenter, 
						        t1.uotype, 
						        t1.value,
						        t1.start_date AS start_date_1, 
						        t1.end_date AS end_date_1, 
						        t2.start_date AS start_date_2, 
						        t2.end_date AS end_date_2
						    FROM dbo.XFC_PLT_AUX_Production_Actual_Times t1
						    JOIN dbo.XFC_PLT_AUX_Production_Actual_Times t2
						        ON t1.id_factory = t2.id_factory
						        AND t1.id_product = t2.id_product
						        AND t1.id_costcenter = t2.id_costcenter
						        AND t1.uotype = t2.uotype
						        AND t1.value = t2.value
						        AND DATEDIFF(DAY, t1.end_date, t2.start_date) = 1
						) p
						    ON t.id_factory = p.id_factory
						    AND t.id_product = p.id_product
						    AND t.id_costcenter = p.id_costcenter
						    AND t.uotype = p.uotype
						    AND t.value = p.value
						    AND t.start_date = p.start_date_2
						    AND t.end_date = p.end_date_2;
						
						DELETE t1
						    
							FROM dbo.XFC_PLT_AUX_Production_Actual_Times t1
							INNER JOIN dbo.XFC_PLT_AUX_Production_Actual_Times t2
							    ON t1.id_factory = t2.id_factory
							    AND t1.id_product = t2.id_product
							    AND t1.id_costcenter = t2.id_costcenter
							    AND t1.uotype = t2.uotype
							    AND t1.value = t2.value
							    AND DATEDIFF(DAY, t1.start_date, t2.start_date) IN (0,1) 
								AND t1.end_date < t2.end_date
							
							WHERE 1=1
						      AND t1.id_factory      = @id_factory
						      AND t1.id_product      = @id_product
						      AND t1.id_costcenter   = @id_costcenter
						      AND t1.uotype          = @uotype;	
							
						COMMIT;
					END TRY
					BEGIN CATCH
					    ROLLBACK;
					END CATCH;
							"
							
							#End Region
							
							ExecuteSql(si, sql)
							
							Dim message As String = $"{vbCrLf}------------ Time Modified ------------{vbCrLf}PRODUCT: {sProduct}{vbCrLf}CC: {sCC} || GM: {sGM}{vbCrLf}TIME: {sTime} ({sTimeUnit}){vbCrLf}START DATE: {sStartDate}{vbCrLf}NEW VALUE: {sNewValue}{vbCrLf}COMMENT: {sComment}{vbCrLf}----------------------------------------"						
							args.NameValuePairs.Add("message", message)
							args.NameValuePairs.Add("mailList", sMailDictionary.GetValueOrDefault(sFactory,"NotRecived"))
							brapi.Utilities.ExecuteDataMgmtSequence(si, "dmSq_PLTTimes_SendMail", args.NameValuePairs)
							
							Return MessagePopUp(message)
							
						Else If args.FunctionName.XFEqualsIgnoreCase("Delete_Times") Then

							Dim sFactory As String = 	args.NameValuePairs.GetValueOrEmpty("factory")
							Dim sProduct As String = 	args.NameValuePairs.GetValueOrEmpty("product")
							Dim sGM As String = 		args.NameValuePairs.GetValueOrEmpty("gm")
							Dim sCC As String = 		args.NameValuePairs.GetValueOrEmpty("cc")
							Dim sEndDate As String = 	args.NameValuePairs.GetValueOrEmpty("end_date")
							Dim sComment As String = 	args.NameValuePairs.GetValueOrEmpty("end_comment") & $" - Deleted by {si.UserName} ({dateMod})"
							
							#Region "Del. Validation"
							
							sqlVal = $"
							DECLARE 
							    @id_factory      nvarchar(20) = N'{sFactory}',
							    @id_product      nvarchar(50) = N'{sProduct}',
							    @id_averagegroup nvarchar(50) = N'{sGM}';
							
							SELECT COUNT(*) + 1 AS Cnt
							FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
							WHERE t.id_factory      = @id_factory
							  AND t.id_product      = @id_product
							  AND t.id_averagegroup = @id_averagegroup
							  AND t.uotype IN (N'UO1', N'UO2');

							"
							 Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                				dtVal = BRAPi.Database.ExecuteSql(dbConnApp, sqlVal, True)
            				End Using  
							
							If dtVal.Rows.Count > 0 And Convert.ToInt32(dtVal.Rows(0)("Cnt")) = 1 Then Return MessagePopUp("Product Not Created")							   
							
							#End Region
							
							#Region "Delete"
							sql = $"
							DECLARE
							    @id_factory      nvarchar(20)  = N'{sFactory}',
							    @id_product      nvarchar(50)  = N'{sProduct}',
							    @id_costcenter 	nvarchar(50)  = N'{sCC}',
							    -- @id_averagegroup nvarchar(50)  = N'{sGM}',
							    @sEndDate        date          = '{sEndDate}',
							    @comment         nvarchar(250) = N'{sComment}';

							--------------------------------------------------------------------------------
							-- 1) Cerrar vigencia actual que cubre @sEndDate (todas los UOTypes)
							--------------------------------------------------------------------------------
							UPDATE t
							   	SET end_date = DATEADD(DAY, -1, CAST(@sEndDate AS datetime))
								 	, comment = CONCAT(comment, @comment)
							FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
							WHERE t.id_factory      = @id_factory
							  AND t.id_product      = @id_product
							  AND t.id_costcenter = @id_costcenter
							  AND t.start_date <  CAST(@sEndDate AS datetime)
							  AND t.end_date   >= CAST(@sEndDate AS datetime);
							
							-- --------------------------------------------------------------------------------
							-- -- 2) Desde @sEndDate en adelante, valor = 0 (todas las filas futuras)
							-- --------------------------------------------------------------------------------
							-- UPDATE t
							--    SET value = 0
							--      -- , comment = COALESCE(NULLIF(@comment, N''), t.comment)  -- opcional
							-- FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
							-- WHERE t.id_factory      = @id_factory
							--   AND t.id_product      = @id_product
							--   AND t.id_costcenter 	= @id_costcenter
							--   AND t.start_date >= CAST(@sEndDate AS datetime);
							-- 
							-- --------------------------------------------------------------------------------
							-- -- 3) Insertar fila puente de 0 si no existe nada desde @sEndDate
							-- --    para cada (uotype, id_costcenter, id_workcenter).
							-- --------------------------------------------------------------------------------
							-- ;WITH combos AS (
							--     SELECT DISTINCT uotype, id_costcenter, id_workcenter
							--     FROM dbo.XFC_PLT_AUX_Production_Actual_Times
							--     WHERE id_factory 	= @id_factory
							--       AND id_product 	= @id_product
							--       AND id_costcenter = @id_costcenter
							-- ),
							-- siguientes AS (
							--     SELECT
							--         c.uotype, c.id_costcenter, c.id_workcenter,
							--         MIN(CASE WHEN t.start_date >= CAST(@sEndDate AS datetime)
							--                  THEN t.start_date END) AS next_start
							--     FROM combos c
							--     LEFT JOIN dbo.XFC_PLT_AUX_Production_Actual_Times t
							--         ON t.id_factory      	= @id_factory
							--        AND t.id_product      	= @id_product
							--        AND t.id_costcenter 		= @id_costcenter
							--        AND t.uotype         	= c.uotype
							--        AND t.id_averagegroup   	= c.id_averagegroup
							--        AND t.id_workcenter   	= c.id_workcenter
							--     GROUP BY c.uotype, c.id_averagegroup, c.id_workcenter
							-- ),
							-- a_insertar AS (
							--     SELECT
							--         @id_factory                                  AS id_factory,
							--         CAST(@sEndDate AS datetime)                  AS start_date,
							--         CASE WHEN s.next_start IS NOT NULL
							--                   AND s.next_start > CAST(@sEndDate AS datetime)
							--              THEN DATEADD(DAY, -1, s.next_start)
							--              ELSE CAST('9999-12-31' AS datetime)
							--         END                                          AS end_date,
							--         @id_product                                  AS id_product,
							--         s.id_averagegroup                            AS id_averagegroup,
							--         @id_costcenter                             	 AS id_costcenter,
							--         s.id_workcenter                              AS id_workcenter,
							--         s.uotype                                     AS uotype,
							--         CAST(0 AS decimal(18,8))                     AS value,
							--         @comment                                     AS comment
							--     FROM siguientes s
							--     -- Insertamos solo si NO existe ya una fila que empiece exactamente el @sEndDate
							--     WHERE NOT EXISTS (
							--         SELECT 1
							--         FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
							--         WHERE t.id_factory      = @id_factory
							--           AND t.id_product      = @id_product
							--           AND t.id_costcenter 	= @id_costcenter
							--           AND t.uotype          = s.uotype
							--           AND t.id_averagegroup = s.id_averagegroup
							--           AND t.id_workcenter   = s.id_workcenter
							--           AND t.start_date      = CAST(@sEndDate AS datetime)
							--     )
							-- )
							-- INSERT INTO dbo.XFC_PLT_AUX_Production_Actual_Times
							-- (
							--     id_factory, start_date, end_date, id_product,
							--     id_costcenter, id_averagegroup, id_workcenter,
							--     uotype, value, comment
							-- )
							-- SELECT
							--     id_factory, start_date, end_date, id_product,
							--     id_costcenter, id_averagegroup, id_workcenter,
							--     uotype, value, comment
							-- FROM a_insertar;						
							"
							#End Region
							
							ExecuteSql(si, sql)
							
							Dim message As String = $"{vbCrLf}------------ Time Deleted ------------{vbCrLf}PRODUCT: {sProduct}{vbCrLf}GM: {sGM}{vbCrLf}END DATE: {sEndDate}{vbCrLf}COMMENT: {sComment}{vbCrLf}----------------------------------------"
							args.NameValuePairs.Add("message", message)
							brapi.Utilities.ExecuteDataMgmtSequence(si, "dmSq_PLTTimes_SendMail", args.NameValuePairs)
							
							Return MessagePopUp(message)
							
						Else If args.FunctionName.XFEqualsIgnoreCase("Create_Times") Then
							
							
							Dim sFactory As String = 		args.NameValuePairs.GetValueOrEmpty("factory")
							Dim sProduct As String = 		args.NameValuePairs.GetValueOrEmpty("product")
							Dim sGM As String = 			args.NameValuePairs.GetValueOrEmpty("gm")
							Dim sCC As String = 			args.NameValuePairs.GetValueOrEmpty("cc")
							Dim sTimeUnit As String = 		args.NameValuePairs.GetValueOrEmpty("time_unit")
							Dim sStartDate As String = 		args.NameValuePairs.GetValueOrEmpty("new_date")
							Dim sNewValueUO1 As String = 	args.NameValuePairs.GetValueOrEmpty("new_value_UO1")
							Dim sNewValueUO2 As String = 	args.NameValuePairs.GetValueOrEmpty("new_value_UO2")
							Dim sComment As String = 		args.NameValuePairs.GetValueOrEmpty("new_comment") & $"- Created by {si.UserName} ({dateMod})"
							
							#Region "Cre. Validation"
							
							sqlVal = $"
							DECLARE 
							    @id_factory      nvarchar(20) = N'{sFactory}',
							    @id_product      nvarchar(50) = N'{sProduct}',
							    @id_averagegroup nvarchar(50) = N'{sGM}';
							
							SELECT COUNT(*) AS Cnt
							FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
							WHERE t.id_factory      = @id_factory
							  AND t.id_product      = @id_product
							  AND t.id_averagegroup = @id_averagegroup
							  AND t.uotype IN (N'UO1', N'UO2');

							"
							 Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                				dtVal = BRAPi.Database.ExecuteSql(dbConnApp, sqlVal, True)
            				End Using  
							
							If dtVal.Rows.Count > 0 And Convert.ToInt32(dtVal.Rows(0)("Cnt")) > 0 Then Return MessagePopUp("Not Valid Time")							   
							
							#End Region
								
							#Region "Create"
							sql = $"
							/* === Insertar dos nuevas líneas (UO1 y UO2) === */
							DECLARE
							    @id_factory       nvarchar(20)  = N'{sFactory}',
							    @id_product       nvarchar(50)  = N'{sProduct}',
							    @id_costcenter  nvarchar(50)  = N'{sCC}',
							    -- @id_averagegroup  nvarchar(50)  = N'{sGM}',
							    @new_start_date   date          = '{sStartDate}',
							    @comment          nvarchar(250) = N'{sComment}',
							
							    -- Valores introducidos por el usuario (pueden venir en horas o minutos)
							    @uo1_value        decimal(18,8) = {sNewValueUO1},
							    @uo2_value        decimal(18,8) = {sNewValueUO2},
							    @value_unit       nvarchar(10)  = N'{sTimeUnit}';
							
							/* --- convertir SIEMPRE a horas --- */
							DECLARE @uo1_hours decimal(18,8), @uo2_hours decimal(18,8);
							
							IF UPPER(@value_unit) IN (N'H', N'HORA', N'HORAS', N'HR', N'HRS')
							BEGIN
							    SET @uo1_hours = @uo1_value;
							    SET @uo2_hours = @uo2_value;
							END
							ELSE IF UPPER(@value_unit) IN (N'MIN', N'MINS', N'MINUTO', N'MINUTOS')
							BEGIN
							    SET @uo1_hours = @uo1_value / 60.0;
							    SET @uo2_hours = @uo2_value / 60.0;
							END
							ELSE
							BEGIN
							    RAISERROR('Unidad no válida: use H o MIN.', 16, 1);
							    RETURN;
							END
							
							-- Tomamos averagegropu/workcenter del dato actual si existe (para reusarlos en el INSERT)
							DECLARE @id_averagegroup nvarchar(50), @id_workcenter int;
							
							SELECT TOP (1)
							       @id_averagegroup = id_averagegroup,
							       @id_workcenter = -1
                            FROM XFC_PLT_MASTER_CostCenter_Hist mc                            
							WHERE mc.id_factory      = @id_factory
							  AND mc.id              = @id_costcenter
							  AND CAST(@new_start_date AS datetime) BETWEEN mc.start_date AND mc.end_date
							ORDER BY mc.start_date DESC;

							IF @id_workcenter   IS NULL SET @id_workcenter = -1;
							IF @id_averagegroup IS NULL SET @id_costcenter = -1;
							
							/* --- fecha fin fija --- */
							DECLARE @new_end_date datetime = '9999-12-31T00:00:00';
							
							BEGIN TRAN;
							
							-- 1) Cerrar la vigencia que cubre la nueva fecha (si existe) para cada UO
							UPDATE t
							   SET t.end_date = DATEADD(DAY, -1, CAST(@new_start_date AS datetime))
							FROM dbo.XFC_PLT_AUX_Production_Actual_Times t
							JOIN (VALUES (N'UO1', @uo1_hours), (N'UO2', @uo2_hours)) AS p(uotype, val_hours)
							  ON p.uotype = t.uotype
							WHERE t.id_factory      = @id_factory
							  AND t.id_product      = @id_product
							  AND t.id_costcenter   = @id_costcenter
							  AND CAST(@new_start_date AS datetime) BETWEEN t.start_date AND t.end_date;
							
							-- 2) Insertar las dos nuevas vigencias (UO1 y UO2)
							INSERT INTO dbo.XFC_PLT_AUX_Production_Actual_Times
							(
							    id_factory, start_date, end_date, id_product,
							    id_costcenter, id_averagegroup, id_workcenter,
							    uotype, value, comment
							)
							SELECT
							    @id_factory,
							    CAST(@new_start_date AS datetime),
							    @new_end_date,
							    @id_product,
							    @id_costcenter,
							    @id_averagegroup,
							    @id_workcenter,
							    p.uotype,
							    p.val_hours,
							    @comment
							FROM (VALUES (N'UO1', @uo1_hours), (N'UO2', @uo2_hours)) AS p(uotype, val_hours);
							
							COMMIT TRAN;
							"
							#End Region
							
							ExecuteSql(si, sql)
							Dim message As String =$"{vbCrLf}------------ Time Created ------------{vbCrLf}PRODUCT: {sProduct}{vbCrLf}GM: {sGM}{vbCrLf}TIME UO1: {sNewValueUO1} ({sTimeUnit}){vbCrLf}TIME UO2: {sNewValueUO2} ({sTimeUnit}){vbCrLf}START DATE: {sStartDate}{vbCrLf}COMMENT: {sComment} {vbCrLf}----------------------------------------"
							args.NameValuePairs.Add("message", message)
							args.NameValuePairs.Add("Comment", args.NameValuePairs.GetValueOrEmpty("new_comment"))
							args.NameValuePairs.Add("start_date", sStartDate)
							args.NameValuePairs.Add("mailList", sMailDictionary.GetValueOrDefault(sFactory,"NotRecived"))
							brapi.Utilities.ExecuteDataMgmtSequence(si, "dmSq_PLTTimes_SendMail", args.NameValuePairs)
													
							Return MessagePopUp(message)
							
						#End Region
						
						#Region "Admin"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("Upload_Historical_Data") Then
							' Variables
							Dim factory As String = args.NameValuePairs("factory")
							
							' FOLDER PATH
							Dim originPath As String 	= $"/Documents/Public/Times/Admin/Loaded"
							Dim destPath As String 		= $"/Documents/Public/Times/Admin/Loaded"
							Dim fileName As String = String.Empty
							' Recorriendo los fiecheros de las carpetas				
							Dim fileExtensions As New List(Of String) From {"csv", "txt", "xlsx"}
							Dim filesFolder As List(Of XFFileInfoEx) = BRApi.FileSystem.GetFilesInFolder(si, FileSystemLocation.ApplicationDatabase, originPath, XFFileType.All, fileExtensions)
	
							For Each files In filesFolder
								
								fileName = files.XFFileInfo.Name
								WriteFileToTable(si, originPath, destPath, fileName, factory)
								
								Dim infoFile As XFFileInfo = BRApi.FileSystem.GetFileInfo(si,FileSystemLocation.ApplicationDatabase,$"{originPath}/{fileName}",True).XFFileInfo
								Dim file As XFFile = BRApi.FileSystem.GetFile(si,FileSystemLocation.ApplicationDatabase, $"{originPath}/{fileName}",True, True).XFFile
							
								Dim fileName0 As String = fileName.Split(".")(0)
								Dim fileExt As String = fileName.Split(".")(1)
								file.FileInfo.Name = $"{fileName0}_{factory}.{fileExt}"
								file.FileInfo.Description = $"Modify by {si.username}"
								
								Brapi.FileSystem.InsertOrUpdateFile(si, file)
								
								#Region "Copiado del fichero"
								
'								' 5. Movemos el Fichero a cargas
'								Dim filePath As String = $"{originPath}/{fileName}"
'								'  5.1- Recogemos la información del fichero cargado				
'								Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase,filePath, True, True)
'								Dim folderName As String = fileInfo.XFFile.FileInfo.FolderFullName 
'								Dim bytesOfFile As Byte() = fileInfo.XFFile.ContentFileBytes
								
'								' 5.2- Creación de un archivo igual con el nombre deseado
'								Dim fileName0 As String = fileName.Split(".")(0)
'								Dim fileExt As String = fileName.Split(".")(1)				
'								Dim newFileName As String = $"{fileName0}_{factory}.{fileExt}"
				
'								Dim copyFileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, newFileName, destPath)
							
'								'Aditional detail
'								copyFileInfo.ContentFileExtension = "xlsx"
'								copyFileInfo.ContentFileContainsData = True
'								copyFileInfo.XFFileType = True
							
'								'Execute copy
'								Dim copyFile As XFFile = New XFFile(copyFileInfo, String.Empty, bytesOfFile)
'								BRApi.FileSystem.InsertOrUpdateFile(si, copyFile)
								
'								' 5.3- Eliminamos el fichero original Cargado
'								BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, filePath)

								#End Region
								
							Next		
							Return MessagePopUp("Fichero Cargado Correctamente")
						
						#End Region
						
						End If
						
					Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						
						#Region "Update Times"						
						If args.FunctionName.XFEqualsIgnoreCase("UpdateTimes") Then
							' Variables
							Dim i As Integer = 0
							Dim sqlAction As String = String.Empty
							Dim sqlInsert As String = String.Empty
							Dim sqlInsertColumns As String = String.Empty
							Dim sqlInsertValues As String = String.Empty
							Dim sqlUpdate As String = String.Empty
							Dim sqlUpdateFilters As String = String.Empty
							Dim sqlUpdateValues As String = String.Empty
							Dim sqlDelete As String = String.Empty
							Dim sqlDeleteValues As String = String.Empty
							
							Dim value As String = String.Empty
							Dim userCulture As String = si.Culture
							
							' Save Data Result
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()						
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = True
							saveDataTaskResult.Message = ""
							saveDataTaskResult.CancelDefaultSave = True
							'Get save data task info
							Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							
							For Each rowInfo In saveDataTaskInfo.EditedDataRows
								
								Dim actions As Integer = rowInfo.InsertUpdateOrDelete
								saveDataTaskResult.Message = $"{saveDataTaskResult.Message}{vbCrLf}"
								
								If actions = 0 Then
									
									'Crear
									saveDataTaskResult.Message = $"{saveDataTaskResult.Message}CREATED{vbCrLf}"
									For Each modDataRowKey As String In rowInfo.ModifiedDataRow.Items.Keys
										saveDataTaskResult.Message = $"{saveDataTaskResult.Message}{rowInfo.ModifiedDataRow.Item(modDataRowKey)}, "
										value = formatValue(modDataRowKey, rowInfo.ModifiedDataRow.Item(modDataRowKey))
										If i=0 Then
											sqlInsertColumns = $"{modDataRowKey}" 
											sqlInsertValues = $" {value}"
											
										Else
											
											sqlInsertColumns = $"{sqlInsertColumns}, {modDataRowKey}" 
											sqlInsertValues = $"{sqlInsertValues},{value}"
										End If
										i += 1
									Next
									i = 0 
									sqlInsertColumns = $"{sqlInsertColumns}, id_workcenter" 
									sqlInsertValues = $"{sqlInsertValues}, '-1'"
									sqlInsert = $"
										INSERT INTO XFC_PLT_AUX_Production_Actual_Times ({sqlInsertColumns}) VALUES ({sqlInsertValues});
										{sqlInsert}
									"
									sqlInsertColumns = String.Empty
									sqlInsertValues = String.Empty
									
								Else If actions = 1
									saveDataTaskResult.Message = $"{saveDataTaskResult.Message}MODIFIED{vbCrLf}"
									' Modificar
									For Each modDataRowKey As String In rowInfo.ModifiedDataRow.Items.Keys
										saveDataTaskResult.Message = $"{saveDataTaskResult.Message}{rowInfo.ModifiedDataRow.Item(modDataRowKey)}, "
										value = formatValue(modDataRowKey, rowInfo.ModifiedDataRow.Item(modDataRowKey))
										If rowInfo.ModifiedDataRow.Item(modDataRowKey) = rowInfo.OriginalDataRow.Item(modDataRowKey) Then
											sqlUpdateFilters = $"
												{sqlUpdateFilters}
												AND {modDataRowKey}={value}"
										Else 
											If i = 0 Then
												sqlUpdateValues = $"
													{modDataRowKey}={value}
												"
												i += 1
											Else
												sqlUpdateValues = $"
													{sqlUpdateValues}
													, {modDataRowKey}={value}
												"
											End If
										End If
										
									Next
									
									sqlUpdate = $"
											{sqlUpdate}
											UPDATE XFC_PLT_AUX_Production_Actual_Times SET {sqlUpdateValues} WHERE 1=1 {sqlUpdateFilters}
										"
									i=0
									
									
									sqlUpdateFilters = String.Empty
									sqlUpdateValues = String.Empty
									
								Else If actions = 2
									
									saveDataTaskResult.Message = $"{saveDataTaskResult.Message}DELETED{vbCrLf}"
									' Eliminar
									For Each modDataRowKey As String In rowInfo.OriginalDataRow.Items.Keys
										
										saveDataTaskResult.Message = $"{saveDataTaskResult.Message}{rowInfo.OriginalDataRow.Item(modDataRowKey)}, "									
										value = formatValue(modDataRowKey, rowInfo.OriginalDataRow.Item(modDataRowKey))
										
										sqlDeleteValues = $" {sqlDeleteValues} AND {modDataRowKey}={value}"
										
									Next
									
									sqlDelete = $"
										{sqlDelete}
										DELETE FROM XFC_PLT_AUX_Production_Actual_Times WHERE 1=1 {sqlDeleteValues};
										"
									sqlDeleteValues = String.Empty
								End If

							Next
							
							sqlAction = $"
															
								BEGIN TRY
								    BEGIN TRANSACTION;
										{sqlDelete}			
										{sqlUpdate}
										{sqlInsert}							
									COMMIT;
								END TRY
								BEGIN CATCH
								    ROLLBACK;
								END CATCH;	
							"
							BRAPI.ErrorLog.LogMessage(SI, sqlAction)
							ExecuteSql(si, sqlAction)
							
							Return saveDataTaskResult		
						
						End If		
						#End Region		
							
					End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
				
		
		#Region "ETL"		
		
		Public Function WriteFileToTable(
										ByVal si As SessionInfo, _ 
										ByVal originPath As String, _
										ByVal destinationPath As String, _
										ByVal fileName As String, _
										Optional inputFactroy As String = ""
										) As List(Of TableRangeContent)		
		
			Try
				
				Dim loadResults As New List(Of TableRangeContent)
				Dim filePath As String = $"{originPath}/{fileName}"
				
				' Get the upload file from the database file system
'				Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase,filePath, True, False, False, SharedConstants.Unknown, Nothing, True)
				Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase,filePath, True, True)
				
				' Tables and Queries
				Dim tableOrgn As String = String.Empty
				Dim tableDest As String = String.Empty
				Dim sqlRaw As String = String.Empty		
				Dim sqlMap As String = String.Empty		
				
				Dim factoryList As New List(Of String) From {"R0671", "R0045106", "R0483003", "R0529002", "R0548913", "R0548914", "R0585", "R0592"}
				Dim factory As String = String.Empty
				
			#Region "Uploads and Mappings"

				#Region "Historical Actual Data"
		
				If fileName = "XFC_TIMES_HistoricalData.xlsx" Then
										
					For Each factory In factoryList
						tableOrgn = $"XFC_PLT_RAW_Production_Actual_Times_{factory}"
						tableDest = $"XFC_PLT_AUX_Production_Actual_Times"
						
						sqlRaw = $"
						
								{sqlRaw}
						
								DROP TABLE IF EXISTS {tableOrgn};
								CREATE TABLE {tableOrgn} 
								 	(
									[id_costcenter] 	varchar(50) ,
									[id_reference] 		varchar(50) ,
									[uo_type] 			varchar(50) ,
									[taj_tso_indicator] varchar(50) ,
									[start_date] 		varchar(50) ,
									[end_date] 			varchar(50) ,
									[value] 			decimal(18,2) ,
									[comment] 			varchar(max)
								 	); "				
						
						sqlMap = $"	
									{sqlMap}
						
									DELETE FROM {tableDest}
									WHERE 1=1
										AND '{factory}'='{inputFactroy}'
										AND id_factory = '{factory}';	
									
									INSERT INTO {tableDest} (id_factory, id_product, id_costcenter, value, comment, start_date, end_date, uotype, id_averagegroup, id_workcenter)
						
									SELECT 
										'{factory}' 																	as id_factory
										, R.id_reference															as id_product
										, R.id_costcenter 															as id_costcenter
										, R.value																	as value
										, R.comment																	as comment					
										, CONVERT(DATE, R.start_date, 112)											as start_date
										, CONVERT(DATE, ISNULL(R.end_date,'99991231'), 112)							as end_date
										, REPLACE(SUBSTRING(taj_tso_indicator, LEN(taj_tso_indicator) - 3),'E','')	as uotype
										, ISNULL(GM.id_averagegroup, '#')											as id_averagegroup
										, -1 																		as id_workcenter
										
						
									FROM {tableOrgn} R
						
									LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist GM
										ON 	GM.id = R.id_costcenter
										AND CONVERT(DATE, R.start_date, 112) BETWEEN GM.start_date AND GM.end_date
										AND GM.id_factory = '{factory}'
						
									WHERE 1=1
										AND taj_tso_indicator LIKE 'TAJ%'
										AND '{factory}'='{inputFactroy}';
								"
					Next
					
					sqlRaw = $"
						BEGIN TRY
						    BEGIN TRANSACTION;
						
						    {sqlRaw}
						
						    COMMIT;
						END TRY
						BEGIN CATCH
						    ROLLBACK;
						END CATCH;"
					
					sqlMap = $"
						BEGIN TRY
						    BEGIN TRANSACTION;
						
						    {sqlMap}
						
						    COMMIT;
						END TRY
						BEGIN CATCH
						    ROLLBACK;
						END CATCH;"
				#End Region		

				End If
				
			#End Region
	
			#Region "Ejecuciones Finales"	
			
				If sqlRaw <> String.Empty Then 
					
					' 1. Creación de tabla temporal RAW
					ExecuteSql(si, "DROP TABLE IF EXISTS "& tableOrgn)					
					ExecuteSql(si, sqlRaw) 
					' 2. Carga de datos en tabla temporal RAW					
					BRapi.Utilities.LoadCustomTableUsingExcel(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes)											
				
				End If
				' 3. Insertar los datos mapeados en la tabla FINAL
				If sqlMap <> String.Empty Then ExecuteSql(si, sqlMap)					
				
				' 4. Eliminar la tabla temporal RAW
				If sqlRaw <> String.Empty Then ExecuteSql(si, "DROP TABLE " & tableOrgn)											
				
'				' 5. Movemos el Fichero a cargas
'				'  5.1- Recogemos la información del fichero cargado				
'				Dim folderName As String = fileInfo.XFFile.FileInfo.FolderFullName 
'				Dim bytesOfFile As Byte() = fileInfo.XFFile.ContentFileBytes
				
'				' 5.2- Creación de un archivo igual con el nombre deseado
'				Dim fileName0 As String = fileName.Split(".")(0)
'				Dim fileExt As String = fileName.Split(".")(1)				
'				Dim newFileName As String = $"{fileName0}_{factory}.{fileExt}"

'				Dim copyFileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, newFileName, originPath)
			
'				'Aditional detail
'				copyFileInfo.ContentFileExtension = "xlsx"
'				copyFileInfo.ContentFileContainsData = True
'				copyFileInfo.XFFileType = True
			
'				'Execute copy
'				Dim copyFile As XFFile = New XFFile(copyFileInfo, String.Empty, bytesOfFile)
'				BRApi.FileSystem.InsertOrUpdateFile(si, copyFile)
				
'				' 5.3- Eliminamos el fichero original Cargado
'				BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, filePath)
						
				Return loadResults
		#End Region
	
			Catch ex As Exception
				Dim filePath As String = $"{originPath}/{fileName}"
				BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, filePath)
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function			
		
		#End Region
		
		#Region "Helper Functions"
		
        Private Sub ExecuteSql(ByVal si As SessionInfo, ByVal sqlCmd As String)        
		
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                               
                BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, True)
				
            End Using   
				
        End Sub	
		
		Private Function MessagePopUp(ByVal message As String)
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = message
			Return selectionChangedTaskResult				
		End Function
		
		Private Function formatValue(ByVal columnName As String, ByVal columnValue As String)
			
			If columnName = "start_date" Or columnName = "end_date" Then
				' 0 - 1 - 2				 1 - 0 - 2 
				' 19/12/2025 00:00:00 -- 12/19/2025 00:00:00 --
				Dim sDate As String = columnValue.Split(" ")(0)
				Dim sReverseDate As String ="'"& sDate.Split("/")(2) &"-"& sDate.Split("/")(1) &"-"& sDate.Split("/")(0)&"T00:00:00'"
				columnValue = sReverseDate
				
			Else If columnName = "value"
				columnValue=columnValue.Replace(",",".")
			Else 
				columnValue=$"'{columnValue}'"
			End If
			
			Return columnValue
		End Function
		
		#End Region
		
	End Class
End Namespace
