Imports System.IO
Imports System.Data
Imports System.Linq
Imports System.Text
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
Imports Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.main_solution_helper

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class main_master_cost_center_group
		
		Private tableName As String = "XFC_MAIN_MASTER_cost_center_group"

		#Region "Get Migration Query"
		
        Public Function GetMigrationQuery(ByVal si As SessionInfo, type As String) As String
            Try
                'Handle type input
                If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then
                    Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
                End If

                'Up Query: Script para crear la tabla de Cost Center Group
                Dim upQuery As String = $"
                    IF OBJECT_ID(N'{tableName}', N'U') IS NULL
                    BEGIN
                        CREATE TABLE {tableName} (
                            CostCenter NVARCHAR(255) NOT NULL,
                            Description NVARCHAR(500) NULL,
                            CostCenterGroup NVARCHAR(255) NOT NULL,
                            entity_id NVARCHAR(255) NOT NULL
                        );
                    END;
                "
                
                'Down Query: Elimina la tabla.
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
		
        Public Function GetPopulationQuery(ByVal si As SessionInfo, type As String) As List(Of String)
            Dim queries As New List(Of String)
            Try
                If type.ToLower = "down" Then
                    ' Para 'down', solo limpiamos la tabla.
                    queries.Add($"TRUNCATE TABLE {tableName};")
                    Return queries
                ElseIf type.ToLower <> "up" Then
                    Throw New XFException(si, New Exception("Population type must be 'up' or 'down'"))
                End If

                ' Lógica de población para 'up'
                
                ' 1. Añadimos el TRUNCATE como primer paso para limpiar la tabla.
                queries.Add($"TRUNCATE TABLE {tableName};")

                Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    ' 2. Obtener el diccionario de mapeos
                    Dim transformationRules As Dictionary(Of String, String) = MainSolutionHelper.GetTransformationRulesMapping(si)
                    
                    ' 3. Leer los datos de la tabla de inventario
                    Dim selectSql As String = "SELECT id, entity_member FROM XFC_INV_MASTER_cost_center"
                    Dim inventoryData As DataTable = BRApi.Database.ExecuteSql(dbConn, selectSql, True)
                    
                    ' 4. Construir la LISTA de inserciones
                    If inventoryData IsNot Nothing AndAlso inventoryData.Rows.Count > 0 Then
                        For Each row As DataRow In inventoryData.Rows
                            Dim costCenter As String = row("id")?.ToString()
                            Dim entityId As String = row("entity_member")?.ToString()
                            
                            If Not String.IsNullOrEmpty(costCenter) AndAlso Not String.IsNullOrEmpty(entityId) Then
                                Dim costCenterGroup As String = "None"
                                If transformationRules.ContainsKey(costCenter) Then
                                    costCenterGroup = transformationRules(costCenter)
                                End If
                                
                                ' Generar CADA comando INSERT y añadirlo a la lista
                                Dim insertSql As String = $"INSERT INTO {tableName} (CostCenter, Description, CostCenterGroup, entity_id) VALUES ('{costCenter.Replace("'", "''")}', '', '{costCenterGroup.Replace("'", "''")}', '{entityId.Replace("'", "''")}');"
                                queries.Add(insertSql)
                            End If
                        Next
                    End If
                End Using

                Return queries
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
		
		#End Region

	End Class
End Namespace