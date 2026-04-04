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
Imports System.Threading.Tasks

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.production_data_import
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				
				Dim tableSelected As String = args.NameValuePairs.GetValueOrDefault("table","All")
				Dim listTables As New List(Of String) From {
															"XFC_MAIN_AUX_fxrate",
															"XFC_PLT_FACT_CostsDistribution",
															"XFC_PLT_FACT_Costs_VTU_Final",
															"XFC_PLT_FACT_Costs_VTU_Final_Local",
															"XFC_PLT_FACT_Costs_VTU_Report_Account",
															"XFC_PLT_FACT_Costs_VTU_Report_Account_Local",
															"XFC_PLT_HIER_Nomenclature_Date_Report",
															"XFC_PLT_MASTER_Factory",
															"XFC_PLT_MASTER_Product",
															"XFC_PLT_MASTER_Account",
															"XFC_PLT_MASTER_CostCenter_Hist",
															"XFC_PLT_MASTER_AverageGroup",
															"XFC_PLT_MASTER_NatureCost"															
															}
															
	
	            Dim listTables2 As List(Of String) = If(tableSelected = "All", listTables.Where(Function(t) Not t.Trim().StartsWith("--")).ToList(),New List(Of String) From {tableSelected})
	
	            Dim totalTables As Integer = listTables2.Count
	            Dim counter As Integer = 0
	
	            ' --- Procesamiento paralelo con límite de hilos ---
	            Parallel.ForEach(
	                listTables2,
	                New ParallelOptions With {.MaxDegreeOfParallelism = 3},  ' cambia 3 por la cantidad deseada de hilos
	                Sub(table)
	
	                    Try
	                        Dim localNum As Integer
	
	                        SyncLock GetType(MainClass)
	                            counter += 1
	                            localNum = counter
	                        End SyncLock

	                        ' 1️ Obtener datos desde la otra app
	                        Dim sGetData As String = $"
	                            SELECT * 
	                            FROM {table}
	                            WHERE 1=1
	                        "
	
	                        Dim dt As DataTable = ExecuteSqlOtherAPP(si, sGetData, "Production")
	
	                        ' 2️ Truncar destino
	                        Dim sTruncate As String = $"TRUNCATE TABLE {table}"
	                        ExecuteActionSQLOnBIBlend(si, sTruncate)
	
	                        ' 3️ Insertar en destino
	                        brapi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, $"INSERT {localNum}/{totalTables} - {table}", 0)
	                        BRApi.Database.SaveCustomDataTable(si, "OneStream BI Blend", table, dt, True)
	
	
	                    Catch ex As Exception
	                        ErrorHandler.LogWrite(si, New XFException(si, ex))
	                    End Try
	
	                End Sub
	            )
										

				#Region "Non Paralel Version"
				
'				' Generar la segunda lista (solo las que no están comentadas)
'				Dim listTables2 As List(Of String) = If(tableSelected="All", listTables.Where(Function(t) Not t.Trim().StartsWith("--")).ToList(), New List(Of String) From {tableSelected})											

'				Dim num As Integer = 0

'				For Each table As String In listTables2
					
'					num += 1
'					Dim dt As New DataTable		
					
'					Dim sGetData As String = $"
'						SELECT * 
'						FROM {table}
'						WHERE 1=1
'					"
'					dt = ExecuteSqlOtherAPP(si, sGetData,"Production")
					
'					Dim sTuncate As String = $"TRUNCATE TABLE {table}"
					
'					ExecuteActionSQLOnBIBlend(si, sTuncate)				
										
'					brapi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, $"INSERT {num}/{listTables2.Count}", num*10)
'					BRApi.Database.SaveCustomDataTable(si, "OneStream BI Blend" , table, dt, True)

'				Next	
				
				#End Region
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Sub ExecuteSql(ByVal si As SessionInfo, ByVal sqlCmd As String)        
		
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                               
                BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, True)
				
            End Using   
				
        End Sub	
		
		Private Function ExecuteSqlOtherAPP(ByVal si As SessionInfo, ByVal sqlCmd As String, ByVal OtherApp As String) As DataTable
		
			Dim oar As New OpenAppResult
			Dim osi As SessionInfo = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, OtherApp, oar)
		
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(osi)
                               
                Dim dt As DataTable = BRAPi.Database.ExecuteSql(dbConnApp, sqlCmd, True)
				Return dt
				
            End Using   
			
			Return Nothing
				
        End Function
		
		Private Sub ExecuteActionSQLOnBIBlend(ByVal si As SessionInfo, ByVal sqlCmd As String)
				'Use the name of the database, used in OneStream Server Configuration Utility >> App Server Config File >> Database Server Connections
				Dim extConnName As String = "OneStream BI Blend" 
                Using dbConnApp As DBConnInfo = BRApi.Database.CreateExternalDbConnInfo(si, extConnName)          
					BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, False, True)
                End Using                                                                               
        End Sub
		
		Private Function ExecuteSQLBIBlend(ByVal si As SessionInfo, ByVal sqlCmd As String)
				Dim dt As DataTable = Nothing
				'Use the name of the database, used in OneStream Server Configuration Utility >> App Server Config File >> Database Server Connections
				Dim extConnName As String = "OneStream BI Blend"
                Using dbConnApp As DBConnInfo = BRApi.Database.CreateExternalDbConnInfo(si, extConnName) 
					dt = BRAPi.Database.ExecuteSql(dbConnApp, sqlCmd, True)
                End Using				
				Return dt
        End Function	
		
		Private Function BuildInsert (ByVal si As SessionInfo, ByVal dt As DataTable, ByVal tableName As String)
		
			Dim insertStatements As New List(Of String)()
			Dim batchSize As Integer = 1000
		    ' Recuperar los tipos de datos de las columnas desde la base de datos
		    Dim columnTypes As Dictionary(Of String, String) = GetColumnTypesFromDatabase(si, tableName)
		
		    ' Verificar si se recuperaron los tipos de datos
		    If columnTypes Is Nothing OrElse columnTypes.Count = 0 Then
		        Throw New Exception("No se pudieron obtener los tipos de columna de la base de datos.")
		    End If
		
		    ' Crear la parte común del INSERT (columnas)
		    Dim columnsPart As New StringBuilder()
		    columnsPart.Append("INSERT INTO " & tableName & " (")
		
		    For Each column As DataColumn In dt.Columns
		        columnsPart.Append(column.ColumnName & ", ")
		    Next
		
		    ' Eliminar la última coma y espacio
		    columnsPart.Length -= 2
		    columnsPart.Append(") VALUES ")
		
		    ' Crear los valores para cada lote
		    Dim currentBatch As New StringBuilder()
		    Dim rowCount As Integer = 0
		
		    For Each row As DataRow In dt.Rows
		        currentBatch.Append("(")
		
		        For Each column As DataColumn In dt.Columns
		            Dim value As Object = row(column)
		            Dim columnName As String = column.ColumnName
		
		            ' Verificar si el tipo de columna está disponible
		            If columnTypes.ContainsKey(columnName) Then
		                Dim columnType As String = columnTypes(columnName)
		
		                ' Verificar el tipo de dato y formatear el valor
		                If value Is DBNull.Value Then
		                    currentBatch.Append("NULL, ")
		                ElseIf columnType = "nvarchar" OrElse columnType = "varchar" Then
		                    ' Para cadenas, escapamos las comillas simples
		                    currentBatch.Append("'" & value.ToString().Replace("'", "''") & "', ")
		                ElseIf columnType = "datetime" Or columnType = "date" Then
		                    ' Para fechas, usamos el formato adecuado de SQL
		                    currentBatch.Append("'" & CType(value, DateTime).ToString("yyyy-MM-dd HH:mm:ss") & "', ")
		                ElseIf columnType = "decimal" Then
		                    ' Para decimales, simplemente convertimos el valor
		                    currentBatch.Append(value.ToString().Replace(",", ".") & ", ")
		                ElseIf columnType = "bit" Then
		                    ' Para valores booleanos (bit), usamos 1 o 0
		                    currentBatch.Append(If(CBool(value), "1", "0") & ", ")
		                Else
		                    ' Otros tipos de datos
		                    currentBatch.Append(value.ToString() & ", ")
		                End If
		            End If
		        Next
		
		        ' Eliminar la última coma y espacio
		        currentBatch.Length -= 2
		        currentBatch.Append("), ")
		
		        rowCount += 1
		
		        ' Si alcanzamos el tamaño del lote, añadir el lote actual a la lista de sentencias y reiniciar
		        If rowCount >= batchSize Then
		            insertStatements.Add(columnsPart.ToString() & currentBatch.ToString().TrimEnd(" "c).TrimEnd(","c)&";")
		            currentBatch.Clear()
		            rowCount = 0
		        End If
		    Next
		
		    ' Si hay un lote que no se ha añadido (menos de batchSize registros restantes), agregarlo
		    If currentBatch.Length > 0 Then
		        insertStatements.Add(columnsPart.ToString() & currentBatch.ToString().TrimEnd(" "c).TrimEnd(","c)&";")
		    End If
		
		    Return insertStatements

		End Function
		
		' Función para recuperar los tipos de columna de una tabla desde la base de datos
		Private Function GetColumnTypesFromDatabase(ByVal si As SessionInfo, ByVal tableName As String) As Dictionary(Of String, String)
		    Dim columnTypes As New Dictionary(Of String, String)()
		
		    
		        Dim query As String = $"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'"
				
				Dim dtInfo As DataTable = ExecuteSQLBIBlend(si, query)
				
				For Each reader In dtInfo.Rows		     
                    Dim columnName As String = reader("COLUMN_NAME").ToString()
                    Dim dataType As String = reader("DATA_TYPE").ToString().ToLower() ' Normalizar tipo de datos a minúsculas
                    columnTypes(columnName) = dataType
				Next
				
		    Return columnTypes
			
		End Function
		
	End Class
End Namespace
