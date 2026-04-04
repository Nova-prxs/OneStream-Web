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

Namespace OneStream.BusinessRule.Extender.UTI_SQLClearMasterOfContracts
    Public Class MainClass
        ' accion: "defineIntersecciones" | "eliminaIntersecciones"
        ' Public Function Main(si As SessionInfo, accion As String) As Object
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
            Dim accion As String = "eliminaintersecciones"
			Try
                Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)

                    Select Case accion.ToLowerInvariant()
                        Case "defineintersecciones"
                            DefineIntersecciones(si, dbConn)
                        Case "eliminaintersecciones"
                            EliminaIntersecciones(si, dbConn)
                        Case Else
                            Throw New ApplicationException("Acción no reconocida. Usa 'defineIntersecciones' o 'eliminaIntersecciones'.")
                    End Select

                End Using
                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        '--------------------------------------------------------------------------------
        ' 1) Crear tabla ESPEJO + cargarla con las filas completas de la original
        '    que correspondan a tus (entity,id) pegados abajo.
        '--------------------------------------------------------------------------------
        Private Sub DefineIntersecciones(si As SessionInfo, dbConn As DbConnInfo)
            Dim targetTable As String = "XFC_RES_MASTER_work_order"               ' tabla original
            Dim backupTable As String = "XFC_RES_MASTER_work_order_to_delete"     ' espejo/backup

            ' 1. Crear tabla espejo si no existe (mismo esquema que la original)
            Dim createSql As String = $"
			IF OBJECT_ID(N'{backupTable}', N'U') IS NULL
			BEGIN
			    CREATE TABLE {backupTable} (
			        entity VARCHAR(255),
			        id VARCHAR(255),
			        [description] VARCHAR(255),
			        project_id VARCHAR(255),
			        project_description VARCHAR(255),
			        technology VARCHAR(255),
			        client VARCHAR(255),
			        rev_type VARCHAR(255),
			        serv_type VARCHAR(255),
			        phase VARCHAR(255),
			        start_date DATE,
			        break_clause_date_1 DATE,
			        break_clause_date_2 DATE,
			        break_clause_date_3 DATE,
			        end_date DATE,
			        mwh DECIMAL(18, 2),
			        as_sold_contract_value DECIMAL(18, 2),
			        as_sold_direct_cost DECIMAL(18, 2),
			        contract_backlog_value DECIMAL(18, 2),
			        [comment] VARCHAR(MAX),
			        CONSTRAINT PK_{backupTable.Replace(".", "_")} PRIMARY KEY (entity, id)
			    );
			
			    CREATE INDEX idx_{backupTable.Replace(".", "_")}_entity ON {backupTable}(entity);
			    CREATE INDEX idx_{backupTable.Replace(".", "_")}_client ON {backupTable}(client);
			    CREATE INDEX idx_{backupTable.Replace(".", "_")}_break1 ON {backupTable}(break_clause_date_1);
			    CREATE INDEX idx_{backupTable.Replace(".", "_")}_end ON {backupTable}(end_date);
			END;
			"
            BRApi.Database.ExecuteSql(dbConn, createSql, False)

            ' 2. Vaciar backup antes de cargar
            BRApi.Database.ExecuteSql(dbConn, $"TRUNCATE TABLE {backupTable};", False)

            ' 3. Tus PARES (entity,id) -> pégalos aquí (1 línea por par)
            Dim pares As New List(Of Tuple(Of String, String)) From {
                Tuple.Create("E107","07.SP-0030-03"),
                Tuple.Create("E141","WP-0021-02")
            }

            ' 4. Construir VALUES (...) para cruzar y copiar filas COMPLETAS desde la original
            '    de ese modo el backup contiene exactamente lo que se va a borrar.
            If pares.Count = 0 Then
                BRApi.ErrorLog.LogMessage(si, "[defineIntersecciones] No se han definido pares (entity,id).")
                Return
            End If

            Dim batchSize As Integer = 800   ' margen seguro para tamaño de comando
            Dim batch As New List(Of String)(batchSize)
            Dim totalCopiadas As Integer = 0

            For Each p In pares
                Dim e As String = "N'" & p.Item1.Replace("'", "''") & "'"
                Dim i As String = "N'" & p.Item2.Replace("'", "''") & "'"
                batch.Add($"({e},{i})")

                If batch.Count >= batchSize Then
                    CopiarAlBackup(dbConn, targetTable, backupTable, batch)
                    totalCopiadas += batch.Count
                    batch.Clear()
                End If
            Next

            If batch.Count > 0 Then
                CopiarAlBackup(dbConn, targetTable, backupTable, batch)
                totalCopiadas += batch.Count
            End If

            BRApi.ErrorLog.LogMessage(si, $"[defineIntersecciones] Copiadas {totalCopiadas} claves al backup {backupTable} desde {targetTable}.")
        End Sub

        ' Copia filas completas de la original al backup uniendo con una tabla derivada (VALUES ...)
        Private Sub CopiarAlBackup(dbConn As DbConnInfo, targetTable As String, backupTable As String, batch As List(Of String))
            Dim valuesBlock As String = String.Join(",", batch)
            Dim insertSql As String = $"
			;WITH v(entity,id) AS (
			    SELECT v.entity, v.id
			    FROM (VALUES {valuesBlock}) AS v(entity,id)
			)
			INSERT INTO {backupTable} (
			    entity, id, [description], project_id, project_description, technology, client,
			    rev_type, serv_type, phase, start_date, break_clause_date_1, break_clause_date_2,
			    break_clause_date_3, end_date, mwh, as_sold_contract_value, as_sold_direct_cost,
			    contract_backlog_value, [comment]
			)
			SELECT
			    t.entity, t.id, t.[description], t.project_id, t.project_description, t.technology, t.client,
			    t.rev_type, t.serv_type, t.phase, t.start_date, t.break_clause_date_1, t.break_clause_date_2,
			    t.break_clause_date_3, t.end_date, t.mwh, t.as_sold_contract_value, t.as_sold_direct_cost,
			    t.contract_backlog_value, t.[comment]
			FROM {targetTable} AS t
			JOIN v ON t.entity = v.entity AND t.id = v.id;
			"
            BRApi.Database.ExecuteSql(dbConn, insertSql, False)
        End Sub

        '--------------------------------------------------------------------------------
        ' 2) Borrar de la original CRUZANDO con el backup (misma tabla espejo)
        '--------------------------------------------------------------------------------
        Private Sub EliminaIntersecciones(si As SessionInfo, dbConn As DbConnInfo)
            Dim targetTable As String = "XFC_RES_MASTER_work_order"
            Dim backupTable As String = "XFC_RES_MASTER_work_order_to_delete"

            Dim sql As String = $"
			DELETE t
			FROM {targetTable} AS t
			JOIN {backupTable} AS b
			  ON t.entity = b.entity
			 AND t.id     = b.id;"
            BRApi.Database.ExecuteSql(dbConn, sql, False)

            BRApi.ErrorLog.LogMessage(si, $"[eliminaIntersecciones] DELETE ejecutado: {targetTable} cruzado con {backupTable}.")
        End Sub

    End Class
End Namespace