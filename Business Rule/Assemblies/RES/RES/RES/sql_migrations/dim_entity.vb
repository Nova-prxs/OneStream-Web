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
    Public Class dim_entity

        'Declare table name
        Dim tableName As String = "XFC_RES_DIM_Entity"

		#Region "Get Migration Query"
		
        Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
            Try
                'Handle type input
                If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
                    Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))

                'Up
                ' Step 1: Create table if it does not exist
                Dim upQuery As String = $"
                    IF OBJECT_ID(N'{tableName}', N'U') IS NULL
                    BEGIN
                        CREATE TABLE XFC_RES_DIM_Entity (
                            entity VARCHAR(100),
                            entityDescription VARCHAR(255),
                            currency VARCHAR(50),
                            country VARCHAR(100),
                            countryDescription VARCHAR(255),
                            region VARCHAR(100),
                            regionDescription VARCHAR(255),
                            LastUpdated DATETIME
                        );
                    END"
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
		
		        Public Function GetPopulationQuery(ByVal si As SessionInfo, ByVal type As String) As String
		            Try
		                If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
		                    Throw New XFException(si, New Exception("Population type must be 'up' or 'down'"))
		
		                If type.ToLower = "down" Then
		                    Return "" ' No acción en 'down'
		                End If
		
		                Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
		
		                    ' Obtener entidades base bajo RES_SER
		                    Dim baseEntities As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(
		                        si,
		                        "Entities",
		                        "E#RES_SER.Base",
		                        True,
		                        Nothing,
		                        Nothing
		                    )
		
		                    If baseEntities Is Nothing OrElse baseEntities.Count = 0 Then
		                        Throw New XFException(si, New Exception("No se encontraron entidades bajo RES_SER."))
		                    End If
		
		                    ' Construir mapa entidad -> moneda
		                    Dim currencyMap As New Dictionary(Of String, String)
		                    For Each baseEntity In baseEntities
		                        Dim name = baseEntity.Member.Name
		                        Dim currency = BRApi.Finance.Entity.GetLocalCurrency(si, baseEntity.Member.MemberId).Name
		                        currencyMap(name) = currency
		                    Next
		
		                    ' Crear CASE WHEN para moneda
		                    Dim currencyCase As String = "CASE m.Name" & vbCrLf
		                    For Each kvp In currencyMap
		                        currencyCase &= $"    WHEN '{kvp.Key.Replace("'", "''")}' THEN '{kvp.Value}'" & vbCrLf
		                    Next
		                    currencyCase &= "    ELSE NULL END"
		
		                    ' Construir cláusula IN
		                    Dim entityNames As String = String.Join(",", currencyMap.Keys.Select(Function(e) $"'{e.Replace("'", "''")}'"))
		
		                    ' Crear query final
		                    Dim insertSql As String = $"
		                        INSERT INTO XFC_RES_DIM_Entity (
		                            entity, entityDescription, currency, country, countryDescription, region, regionDescription, LastUpdated
		                        )
		                        SELECT 
		                            m.Name,
		                            m.Description,
		                            {currencyCase} AS currency,
		                            mp.TextValue AS country,
		                            country.Description AS countryDescription,
		                            region.Name AS region,
		                            region.Description AS regionDescription,
		                            GETDATE() AS LastUpdated
		                        FROM 
		                            Member m
		                        LEFT JOIN 
		                            MemberProperty mp ON m.MemberID = mp.MemberID AND mp.PropertyID = 900300
		                        LEFT JOIN 
		                            Member country ON country.Name = mp.TextValue AND country.DimID = 43
		                        LEFT JOIN 
		                            Relationship r ON r.ChildId = country.MemberID
		                        LEFT JOIN 
		                            Member region ON region.MemberID = r.ParentId
		                        WHERE 
		                            m.DimID = 32
		                            AND m.Name IN ({entityNames});
		                    "
		
		                    Return insertSql
		
		                End Using
		            Catch ex As Exception
		                Throw New XFException(si, ex)
		            End Try
		        End Function
		
		#End Region

    End Class
End Namespace
