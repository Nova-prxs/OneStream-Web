' Metadata_Data.vb — METADATA_DATASET assembly, OCE workspace
' Receives QueryType and optional dimension parameters via
' CustomSubstVarsAsCommaSeparatedPairs substvar.
'
' SubstVar format: "QueryType=GetDimensionList" or
'                  "QueryType=GetDimensionMembers,DimName=Entity,ExpandLevel=1,IncludeRoot=true"
'
' Supported QueryTypes:
'   GetDimensionList         -> Returns all dimension metadata rows
'   GetDimensionMembers      -> Returns dimension member hierarchy rows
'   GetMemberProperties      -> Returns member property rows (DimTypeId, MemberPk required)
'   SearchMembers            -> Returns matching member rows (DimTypeId, SearchTerm required)
'   GetTimeMembers           -> Returns time period member rows
'   GetMemberFormulas        -> Returns formula variation rows (DimTypeId, MemberPk required)
'   GetMemberAllProperties   -> Returns all member property rows (DimTypeId, MemberPk required)
'   GetMembersWithFormulas   -> Returns all formula members across dimensions
'   GetMembersWithTextFields -> Returns all text-field members across dimensions
'   GetScenarioTypes         -> Returns scenario type rows
'   GetDimTypes              -> Returns dimension type rows
'   GetFormulaTypes          -> Returns formula type rows
'
' Returns a DataTable named METADATA_RESULTS.
' The client (dimensionService.ts) uses extractDataFromResponse which falls back
' to Tables[0].Rows when the named key is not found, so METADATA_RESULTS is
' the single declared result table name for all queries.
'
' Implementation uses SQL queries against the application database tables
' (Dim, Member, MemberRel, TimeMember, ScenarioType, etc.) via
' BRApi.Database.CreateApplicationDbConnInfo + ExecuteSql.
' The Dim/Member classes use binary serialization so their properties are
' not reliably accessible from BRApi; direct SQL is the correct approach.

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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.Metadata_Data

    Public Class MainClass

        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
            ' --- FunctionType dispatch (matches proven OneStream workspace assembly pattern) ---
            ' The framework calls Main with args.FunctionType to either list dataset names
            ' or execute a specific dataset. args.DataSetName is populated from the second
            ' part of the adapter's methodQuery (e.g., {MetadataQuery}).
            Select Case args.FunctionType
                Case Is = DashboardDataSetFunctionType.GetDataSetNames
                    ' Return the dataset names this assembly handles
                    Return New List(Of String) From {"MetadataQuery"}

                Case Is = DashboardDataSetFunctionType.GetDataSet
                    ' Fall through to the data retrieval logic below
            End Select

            Dim resultDs As New DataSet()
            Dim dt As New DataTable("METADATA_RESULTS")
            resultDs.Tables.Add(dt)
            Dim dbConn As DbConnInfo = Nothing

            Try
                ' --- Step 1: Read both dictionaries from DashboardDataSetArgs ---
                ' The REST API sends CustomSubstVarsAsCommaSeparatedPairs as a string.
                ' The framework may populate CustomSubstVars, NameValuePairs, or both.
                ' We check both and merge them so dispatch works regardless.
                Dim csvars As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

                ' Try CustomSubstVars first
                If args IsNot Nothing AndAlso args.CustomSubstVars IsNot Nothing Then
                    For Each kvp In args.CustomSubstVars
                        csvars(kvp.Key) = kvp.Value
                    Next
                End If

                ' Also merge NameValuePairs (framework may put values here instead)
                If args IsNot Nothing AndAlso args.NameValuePairs IsNot Nothing Then
                    For Each kvp In args.NameValuePairs
                        If Not csvars.ContainsKey(kvp.Key) Then
                            csvars(kvp.Key) = kvp.Value
                        End If
                    Next
                End If

                ' --- Step 2: Read QueryType ---
                Dim queryType As String = csvars.XFGetValue("QueryType")
                If String.IsNullOrWhiteSpace(queryType) Then
                    Throw New Exception("Metadata_Data: QueryType not found in CustomSubstVars")
                End If

                ' --- Step 3: Get application DB connection for SQL queries ---
                dbConn = BRApi.Database.CreateApplicationDbConnInfo(si)

                ' --- Step 4: Dispatch on QueryType ---
                Select Case queryType

                    Case "GetDimensionList"
                        ' Returns all dimension definitions via SQL against the Dim table.
                        ' Each row in Dim represents a dimension; DimTypeId identifies the
                        ' standard type (0=Account,1=Entity,...,15=UD8), DimId is unique per dim.
                        dt.Columns.Add("DimPk", GetType(String))
                        dt.Columns.Add("Name", GetType(String))
                        dt.Columns.Add("Description", GetType(String))
                        dt.Columns.Add("DimType", GetType(String))
                        dt.Columns.Add("DimGroup", GetType(String))
                        dt.Columns.Add("MemberCount", GetType(Integer))
                        dt.Columns.Add("IsStandard", GetType(Boolean))
                        dt.Columns.Add("InheritedFromDimPk", GetType(String))
                        dt.Columns.Add("InheritedFromName", GetType(String))

                        Dim dimSql As String = "SELECT d.DimId, d.DimTypeId, d.Name, d.Description, " &
                            "(SELECT COUNT(*) FROM Member m WHERE m.DimId = d.DimId) AS MemberCount " &
                            "FROM Dim d " &
                            "ORDER BY d.DimTypeId, d.Name"
                        Dim dimResult As DataTable = BRApi.Database.ExecuteSql(dbConn, dimSql, False)
                        If dimResult IsNot Nothing Then
                            For Each dr As DataRow In dimResult.Rows
                                Dim row As DataRow = dt.NewRow()
                                Dim dimId As Integer = Convert.ToInt32(dr("DimId"))
                                Dim dimTypeId As Integer = Convert.ToInt32(dr("DimTypeId"))
                                row("DimPk") = dimTypeId.ToString() & "_" & dimId.ToString()
                                row("Name") = SafeStr(dr("Name"))
                                row("Description") = SafeStr(dr("Description"))
                                row("DimType") = dimTypeId.ToString()
                                row("DimGroup") = GetDimGroup(dimTypeId)
                                row("MemberCount") = If(IsDBNull(dr("MemberCount")), 0, Convert.ToInt32(dr("MemberCount")))
                                row("IsStandard") = (dimTypeId <= 8)
                                row("InheritedFromDimPk") = DBNull.Value
                                row("InheritedFromName") = DBNull.Value
                                dt.Rows.Add(row)
                            Next
                        End If

                    Case "GetDimensionMembers"
                        ' Returns hierarchical members for a dimension via SQL.
                        ' Joins Member and Relationship (ParentId/ChildId) for parent-child.
                        Dim dimName As String = If(csvars IsNot Nothing, csvars.XFGetValue("DimName"), String.Empty)
                        Dim expandLevel As Integer = 1
                        Dim expandStr As String = If(csvars IsNot Nothing, csvars.XFGetValue("ExpandLevel"), String.Empty)
                        If Not String.IsNullOrEmpty(expandStr) Then Integer.TryParse(expandStr, expandLevel)

                        dt.Columns.Add("MemberPk", GetType(String))
                        dt.Columns.Add("Name", GetType(String))
                        dt.Columns.Add("Description", GetType(String))
                        dt.Columns.Add("ParentMemberPk", GetType(String))
                        dt.Columns.Add("ParentName", GetType(String))
                        dt.Columns.Add("Level", GetType(Integer))
                        dt.Columns.Add("IsBase", GetType(Boolean))
                        dt.Columns.Add("HasChildren", GetType(Boolean))
                        dt.Columns.Add("ChildCount", GetType(Integer))
                        dt.Columns.Add("MemberIndex", GetType(Integer))
                        dt.Columns.Add("IsCalculated", GetType(Boolean))
                        dt.Columns.Add("FormulaType", GetType(String))

                        If Not String.IsNullOrEmpty(dimName) Then
                            ' Sanitize dimName for SQL (replace single quotes)
                            Dim safeDimName As String = dimName.Replace("'", "''")
                            Dim memberSql As String =
                                "SELECT m.MemberId, m.Name, m.Description, " &
                                "r.ParentId, p.Name AS ParentName, " &
                                "r.SiblingSortOrder " &
                                "FROM Dim d " &
                                "INNER JOIN Member m ON m.DimId = d.DimId " &
                                "LEFT JOIN Relationship r ON r.DimId = d.DimId AND r.ChildId = m.MemberId " &
                                "LEFT JOIN Member p ON p.DimId = d.DimId AND p.MemberId = r.ParentId " &
                                "WHERE d.Name = '" & safeDimName & "' " &
                                "ORDER BY r.SiblingSortOrder"
                            Dim memberResult As DataTable = BRApi.Database.ExecuteSql(dbConn, memberSql, False)
                            If memberResult IsNot Nothing Then
                                ' Build a set of parent IDs to determine HasChildren
                                Dim parentIds As New HashSet(Of Integer)
                                For Each dr As DataRow In memberResult.Rows
                                    Dim pid As Object = dr("ParentId")
                                    If Not IsDBNull(pid) Then
                                        Dim pidVal As Integer = Convert.ToInt32(pid)
                                        If pidVal >= 0 Then parentIds.Add(pidVal)
                                    End If
                                Next
                                For Each dr As DataRow In memberResult.Rows
                                    Dim row As DataRow = dt.NewRow()
                                    Dim memberId As Integer = Convert.ToInt32(dr("MemberId"))
                                    row("MemberPk") = memberId.ToString()
                                    row("Name") = SafeStr(dr("Name"))
                                    row("Description") = SafeStr(dr("Description"))
                                    Dim parentId As Object = dr("ParentId")
                                    If IsDBNull(parentId) OrElse Convert.ToInt32(parentId) < 0 Then
                                        row("ParentMemberPk") = DBNull.Value
                                        row("ParentName") = String.Empty
                                    Else
                                        row("ParentMemberPk") = Convert.ToInt32(parentId).ToString()
                                        row("ParentName") = SafeStr(dr("ParentName"))
                                    End If
                                    row("Level") = 0
                                    Dim hasKids As Boolean = parentIds.Contains(memberId)
                                    row("IsBase") = Not hasKids
                                    row("HasChildren") = hasKids
                                    row("ChildCount") = 0
                                    row("MemberIndex") = If(IsDBNull(dr("SiblingSortOrder")), 0, CInt(Convert.ToInt64(dr("SiblingSortOrder")) Mod 2147483647))
                                    row("IsCalculated") = False
                                    row("FormulaType") = String.Empty
                                    dt.Rows.Add(row)
                                Next
                            End If
                        End If

                    Case "GetMemberProperties"
                        ' Returns named properties for a specific member via
                        ' BRApi.Finance.Metadata.GetMember with includeProperties=True.
                        Dim dimTypeId As Integer = 0
                        Dim dimTypeStr As String = If(csvars IsNot Nothing, csvars.XFGetValue("DimTypeId"), String.Empty)
                        If Not String.IsNullOrEmpty(dimTypeStr) Then Integer.TryParse(dimTypeStr, dimTypeId)
                        Dim memberPkStr As String = If(csvars IsNot Nothing, csvars.XFGetValue("MemberPk"), String.Empty)
                        Dim memberId As Integer = 0
                        Integer.TryParse(memberPkStr, memberId)

                        dt.Columns.Add("PropertyName", GetType(String))
                        dt.Columns.Add("PropertyValue", GetType(String))
                        dt.Columns.Add("PropertyType", GetType(String))
                        dt.Columns.Add("IsStandard", GetType(Boolean))

                        ' Use BRApi.Finance.Metadata.GetMember to get MemberInfo with properties
                        If memberId > 0 Then
                            Dim mi As MemberInfo = BRApi.Finance.Metadata.GetMember(si, dimTypeId, memberId, True)
                            If mi IsNot Nothing AndAlso mi.Member IsNot Nothing Then
                                Dim m As Member = mi.Member
                                ' Add standard member fields as properties
                                AddPropertyRow(dt, "Name", m.Name, "String", True)
                                AddPropertyRow(dt, "MemberId", m.MemberId.ToString(), "Int32", True)
                                AddPropertyRow(dt, "Description", If(mi.LocalDescription IsNot Nothing, mi.LocalDescription.Description, String.Empty), "String", True)
                            End If
                        End If

                    Case "SearchMembers"
                        ' Returns members matching search term via SQL LIKE query.
                        Dim searchDimTypeId As Integer = 0
                        Dim searchDimTypeStr As String = If(csvars IsNot Nothing, csvars.XFGetValue("DimTypeId"), String.Empty)
                        If Not String.IsNullOrEmpty(searchDimTypeStr) Then Integer.TryParse(searchDimTypeStr, searchDimTypeId)
                        Dim searchTerm As String = If(csvars IsNot Nothing, csvars.XFGetValue("SearchTerm"), String.Empty)
                        ' URL-decode the search term (client encodes it)
                        If Not String.IsNullOrEmpty(searchTerm) Then
                            searchTerm = System.Net.WebUtility.UrlDecode(searchTerm)
                        End If
                        Dim maxResults As Integer = 100
                        Dim maxStr As String = If(csvars IsNot Nothing, csvars.XFGetValue("MaxResults"), String.Empty)
                        If Not String.IsNullOrEmpty(maxStr) Then Integer.TryParse(maxStr, maxResults)

                        dt.Columns.Add("MemberPk", GetType(String))
                        dt.Columns.Add("Name", GetType(String))
                        dt.Columns.Add("Description", GetType(String))
                        dt.Columns.Add("ParentMemberPk", GetType(String))
                        dt.Columns.Add("ParentName", GetType(String))
                        dt.Columns.Add("Level", GetType(Integer))
                        dt.Columns.Add("IsBase", GetType(Boolean))
                        dt.Columns.Add("HasChildren", GetType(Boolean))
                        dt.Columns.Add("ChildCount", GetType(Integer))
                        dt.Columns.Add("MemberIndex", GetType(Integer))
                        dt.Columns.Add("IsCalculated", GetType(Boolean))
                        dt.Columns.Add("FormulaType", GetType(String))
                        dt.Columns.Add("MatchType", GetType(String))

                        If Not String.IsNullOrEmpty(searchTerm) Then
                            Dim safeTerm As String = searchTerm.Replace("'", "''")
                            Dim searchSql As String =
                                "SELECT TOP " & maxResults.ToString() & " m.MemberId, m.Name, m.Description, " &
                                "r.ParentId, p.Name AS ParentName, r.SiblingSortOrder " &
                                "FROM Dim d " &
                                "INNER JOIN Member m ON m.DimId = d.DimId " &
                                "LEFT JOIN Relationship r ON r.DimId = d.DimId AND r.ChildId = m.MemberId " &
                                "LEFT JOIN Member p ON p.DimId = d.DimId AND p.MemberId = r.ParentId " &
                                "WHERE d.DimTypeId = " & searchDimTypeId.ToString() & " " &
                                "AND (m.Name LIKE '%" & safeTerm & "%' OR m.Description LIKE '%" & safeTerm & "%') " &
                                "ORDER BY m.Name"
                            Dim searchResult As DataTable = BRApi.Database.ExecuteSql(dbConn, searchSql, False)
                            If searchResult IsNot Nothing Then
                                For Each dr As DataRow In searchResult.Rows
                                    Dim row As DataRow = dt.NewRow()
                                    Dim mId As Integer = Convert.ToInt32(dr("MemberId"))
                                    row("MemberPk") = mId.ToString()
                                    Dim mName As String = SafeStr(dr("Name"))
                                    row("Name") = mName
                                    Dim mDesc As String = SafeStr(dr("Description"))
                                    row("Description") = mDesc
                                    Dim parentId As Object = dr("ParentId")
                                    If IsDBNull(parentId) Then
                                        row("ParentMemberPk") = DBNull.Value
                                        row("ParentName") = String.Empty
                                    Else
                                        row("ParentMemberPk") = Convert.ToInt32(parentId).ToString()
                                        row("ParentName") = SafeStr(dr("ParentName"))
                                    End If
                                    row("Level") = 0
                                    row("IsBase") = True
                                    row("HasChildren") = False
                                    row("ChildCount") = 0
                                    row("MemberIndex") = If(IsDBNull(dr("SiblingSortOrder")), 0, CInt(Convert.ToInt64(dr("SiblingSortOrder")) Mod 2147483647))
                                    row("IsCalculated") = False
                                    row("FormulaType") = String.Empty
                                    ' Determine match type: name or description
                                    If mName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 Then
                                        row("MatchType") = "Name"
                                    Else
                                        row("MatchType") = "Description"
                                    End If
                                    dt.Rows.Add(row)
                                Next
                            End If
                        End If

                    Case "GetTimeMembers"
                        ' Returns time period members from the Time dimension via SQL.
                        dt.Columns.Add("MemberPk", GetType(Integer))
                        dt.Columns.Add("Name", GetType(String))
                        dt.Columns.Add("Description", GetType(String))

                        ' Query the Member table for the Time dimension (DimTypeId=3)
                        Dim timeSql As String =
                            "SELECT m.MemberId, m.Name, m.Description " &
                            "FROM Dim d " &
                            "INNER JOIN Member m ON m.DimId = d.DimId " &
                            "WHERE d.DimTypeId = 3 " &
                            "ORDER BY m.MemberId"
                        Dim timeResult As DataTable = BRApi.Database.ExecuteSql(dbConn, timeSql, False)
                        If timeResult IsNot Nothing Then
                            For Each dr As DataRow In timeResult.Rows
                                Dim row As DataRow = dt.NewRow()
                                row("MemberPk") = Convert.ToInt32(dr("MemberId"))
                                row("Name") = SafeStr(dr("Name"))
                                row("Description") = SafeStr(dr("Description"))
                                dt.Rows.Add(row)
                            Next
                        End If

                    Case "GetMemberFormulas"
                        ' Returns formula variations for a member.
                        ' Uses BRApi.Finance.Metadata.GetMember with includeProperties to
                        ' extract formula info from VaryingMemberProperties.
                        Dim fmlDimTypeId As Integer = 0
                        Dim fmlDimTypeStr As String = If(csvars IsNot Nothing, csvars.XFGetValue("DimTypeId"), String.Empty)
                        If Not String.IsNullOrEmpty(fmlDimTypeStr) Then Integer.TryParse(fmlDimTypeStr, fmlDimTypeId)
                        Dim fmlMemberPkStr As String = If(csvars IsNot Nothing, csvars.XFGetValue("MemberPk"), String.Empty)
                        Dim fmlMemberId As Integer = 0
                        Integer.TryParse(fmlMemberPkStr, fmlMemberId)

                        dt.Columns.Add("ScenarioTypeId", GetType(Integer))
                        dt.Columns.Add("ScenarioTypeName", GetType(String))
                        dt.Columns.Add("TimeId", GetType(Integer))
                        dt.Columns.Add("TimeName", GetType(String))
                        dt.Columns.Add("Formula", GetType(String))
                        dt.Columns.Add("FormulaType", GetType(String))

                        ' TODO: Populate from VaryingMemberProperties when property structure
                        ' is confirmed. For now returns column schema only; client handles empty.

                    Case "GetMemberAllProperties"
                        ' Returns all property variations for a member.
                        Dim allPropDimTypeId As Integer = 0
                        Dim allPropDimTypeStr As String = If(csvars IsNot Nothing, csvars.XFGetValue("DimTypeId"), String.Empty)
                        If Not String.IsNullOrEmpty(allPropDimTypeStr) Then Integer.TryParse(allPropDimTypeStr, allPropDimTypeId)
                        Dim allPropMemberPkStr As String = If(csvars IsNot Nothing, csvars.XFGetValue("MemberPk"), String.Empty)
                        Dim allPropMemberId As Integer = 0
                        Integer.TryParse(allPropMemberPkStr, allPropMemberId)

                        dt.Columns.Add("PropertyName", GetType(String))
                        dt.Columns.Add("PropertyType", GetType(String))
                        dt.Columns.Add("DataType", GetType(String))
                        dt.Columns.Add("ScenarioTypeId", GetType(Integer))
                        dt.Columns.Add("ScenarioTypeName", GetType(String))
                        dt.Columns.Add("TimeId", GetType(Integer))
                        dt.Columns.Add("TimeName", GetType(String))
                        dt.Columns.Add("CubeType", GetType(String))
                        dt.Columns.Add("Value", GetType(String))
                        dt.Columns.Add("EnumOptions", GetType(String))

                        ' Use BRApi.Finance.Metadata.GetMember with includeProperties=True
                        If allPropMemberId > 0 Then
                            Dim allMi As MemberInfo = BRApi.Finance.Metadata.GetMember(si, allPropDimTypeId, allPropMemberId, True)
                            If allMi IsNot Nothing AndAlso allMi.Member IsNot Nothing Then
                                Dim am As Member = allMi.Member
                                AddAllPropertyRow(dt, "Name", "Static", "String", 0, "", 0, "", "", am.Name)
                                AddAllPropertyRow(dt, "MemberId", "Static", "Int32", 0, "", 0, "", "", am.MemberId.ToString())
                                If allMi.LocalDescription IsNot Nothing Then
                                    AddAllPropertyRow(dt, "Description", "Static", "String", 0, "", 0, "", "", allMi.LocalDescription.Description)
                                End If
                            End If
                        End If

                    Case "GetMembersWithFormulas"
                        ' Returns all formula members across all dimensions.
                        ' TODO: Requires iterating dimensions and extracting formula properties.
                        ' For now returns column schema only; client handles empty.
                        dt.Columns.Add("DimName", GetType(String))
                        dt.Columns.Add("DimTypeId", GetType(Integer))
                        dt.Columns.Add("DimTypeName", GetType(String))
                        dt.Columns.Add("MemberId", GetType(Integer))
                        dt.Columns.Add("MemberPk", GetType(String))
                        dt.Columns.Add("Name", GetType(String))
                        dt.Columns.Add("Description", GetType(String))
                        dt.Columns.Add("FormulaType", GetType(String))
                        dt.Columns.Add("ScenarioTypeId", GetType(Integer))
                        dt.Columns.Add("ScenarioTypeName", GetType(String))
                        dt.Columns.Add("TimeId", GetType(Integer))
                        dt.Columns.Add("TimeName", GetType(String))
                        dt.Columns.Add("Formula", GetType(String))

                    Case "GetMembersWithTextFields"
                        ' Returns all text-field members across all dimensions.
                        ' TODO: Requires iterating dimensions and extracting text properties.
                        ' For now returns column schema only; client handles empty.
                        dt.Columns.Add("DimName", GetType(String))
                        dt.Columns.Add("DimTypeId", GetType(Integer))
                        dt.Columns.Add("DimTypeName", GetType(String))
                        dt.Columns.Add("MemberId", GetType(Integer))
                        dt.Columns.Add("MemberPk", GetType(String))
                        dt.Columns.Add("Name", GetType(String))
                        dt.Columns.Add("Description", GetType(String))
                        dt.Columns.Add("TextField", GetType(String))
                        dt.Columns.Add("TextValue", GetType(String))
                        dt.Columns.Add("ScenarioTypeId", GetType(Integer))
                        dt.Columns.Add("ScenarioTypeName", GetType(String))
                        dt.Columns.Add("TimeId", GetType(Integer))
                        dt.Columns.Add("TimeName", GetType(String))

                    Case "GetScenarioTypes"
                        ' Returns all scenario type definitions via SQL.
                        dt.Columns.Add("ScenarioTypeId", GetType(Integer))
                        dt.Columns.Add("ScenarioTypeName", GetType(String))

                        Dim stSql As String =
                            "SELECT ScenarioTypeId, Name FROM ScenarioType ORDER BY ScenarioTypeId"
                        Dim stResult As DataTable = BRApi.Database.ExecuteSql(dbConn, stSql, False)
                        If stResult IsNot Nothing Then
                            For Each dr As DataRow In stResult.Rows
                                Dim row As DataRow = dt.NewRow()
                                row("ScenarioTypeId") = Convert.ToInt32(dr("ScenarioTypeId"))
                                row("ScenarioTypeName") = SafeStr(dr("Name"))
                                dt.Rows.Add(row)
                            Next
                        End If

                    Case "GetDimTypes"
                        ' Returns all dimension type definitions.
                        ' DimType is an enum-like class: 0=Account,1=Entity,...,15=UD8
                        dt.Columns.Add("DimTypeId", GetType(Integer))
                        dt.Columns.Add("DimTypeName", GetType(String))

                        ' Query distinct DimTypeId values from the Dim table
                        Dim dtSql As String =
                            "SELECT DISTINCT DimTypeId FROM Dim ORDER BY DimTypeId"
                        Dim dtResult As DataTable = BRApi.Database.ExecuteSql(dbConn, dtSql, False)
                        If dtResult IsNot Nothing Then
                            For Each dr As DataRow In dtResult.Rows
                                Dim dtId As Integer = Convert.ToInt32(dr("DimTypeId"))
                                Dim row As DataRow = dt.NewRow()
                                row("DimTypeId") = dtId
                                row("DimTypeName") = GetDimTypeName(dtId)
                                dt.Rows.Add(row)
                            Next
                        End If

                    Case "GetFormulaTypes"
                        ' Returns all formula type definitions.
                        ' FormulaType is an enum: 0=NoFormula,1=Expression,2=Percent,...
                        dt.Columns.Add("FormulaTypeId", GetType(Integer))
                        dt.Columns.Add("FormulaTypeName", GetType(String))

                        ' Return the known FormulaType enum values
                        AddFormulaTypeRow(dt, 0, "NoFormula")
                        AddFormulaTypeRow(dt, 1, "Expression")
                        AddFormulaTypeRow(dt, 2, "Percent")
                        AddFormulaTypeRow(dt, 3, "Share")
                        AddFormulaTypeRow(dt, 4, "Custom")

                    Case "DebugRelationship"
                        ' Diagnostic: run the EXACT GetDimensionMembers SQL for Scenarios and show results
                        dt.Columns.Add("Source", GetType(String))
                        dt.Columns.Add("Col1", GetType(String))
                        dt.Columns.Add("Col2", GetType(String))
                        dt.Columns.Add("Col3", GetType(String))
                        dt.Columns.Add("Col4", GetType(String))
                        dt.Columns.Add("Col5", GetType(String))

                        ' 1) Run the exact member query for Scenarios
                        Dim dbgMemberSql As String =
                            "SELECT TOP 10 m.MemberId, m.Name, m.Description, " &
                            "r.ParentId, p.Name AS ParentName, " &
                            "r.SiblingSortOrder " &
                            "FROM Dim d " &
                            "INNER JOIN Member m ON m.DimId = d.DimId " &
                            "LEFT JOIN Relationship r ON r.DimId = d.DimId AND r.ChildId = m.MemberId " &
                            "LEFT JOIN Member p ON p.DimId = d.DimId AND p.MemberId = r.ParentId " &
                            "WHERE d.Name = 'Scenarios' " &
                            "ORDER BY r.SiblingSortOrder"
                        Try
                            Dim dbgMemberResult As DataTable = BRApi.Database.ExecuteSql(dbConn, dbgMemberSql, False)
                            Dim infoRow As DataRow = dt.NewRow()
                            infoRow("Source") = "MemberQuery"
                            infoRow("Col1") = "RowCount=" & If(dbgMemberResult IsNot Nothing, dbgMemberResult.Rows.Count.ToString(), "NULL")
                            infoRow("Col2") = "ColCount=" & If(dbgMemberResult IsNot Nothing, dbgMemberResult.Columns.Count.ToString(), "0")
                            infoRow("Col3") = ""
                            infoRow("Col4") = ""
                            infoRow("Col5") = ""
                            dt.Rows.Add(infoRow)

                            If dbgMemberResult IsNot Nothing Then
                                For Each dr As DataRow In dbgMemberResult.Rows
                                    Dim row As DataRow = dt.NewRow()
                                    row("Source") = "Member"
                                    row("Col1") = "Id=" & SafeStr(dr("MemberId"))
                                    row("Col2") = "Name=" & SafeStr(dr("Name"))
                                    row("Col3") = "ParentId=" & SafeStr(dr("ParentId"))
                                    row("Col4") = "ParentName=" & SafeStr(dr("ParentName"))
                                    row("Col5") = "Sort=" & SafeStr(dr("SiblingSortOrder"))
                                    dt.Rows.Add(row)
                                Next
                            End If
                        Catch exMember As Exception
                            Dim memberErrRow As DataRow = dt.NewRow()
                            memberErrRow("Source") = "MemberQuery_Error"
                            memberErrRow("Col1") = exMember.Message
                            memberErrRow("Col2") = ""
                            memberErrRow("Col3") = ""
                            memberErrRow("Col4") = ""
                            memberErrRow("Col5") = ""
                            dt.Rows.Add(memberErrRow)
                        End Try

                        ' 2) Also try a simple member-only query (no Relationship JOIN)
                        Dim dbgSimpleSql As String =
                            "SELECT TOP 5 m.MemberId, m.Name FROM Member m " &
                            "INNER JOIN Dim d ON m.DimId = d.DimId " &
                            "WHERE d.Name = 'Scenarios' ORDER BY m.MemberId"
                        Try
                            Dim dbgSimpleResult As DataTable = BRApi.Database.ExecuteSql(dbConn, dbgSimpleSql, False)
                            Dim simpleInfoRow As DataRow = dt.NewRow()
                            simpleInfoRow("Source") = "SimpleQuery"
                            simpleInfoRow("Col1") = "RowCount=" & If(dbgSimpleResult IsNot Nothing, dbgSimpleResult.Rows.Count.ToString(), "NULL")
                            simpleInfoRow("Col2") = ""
                            simpleInfoRow("Col3") = ""
                            simpleInfoRow("Col4") = ""
                            simpleInfoRow("Col5") = ""
                            dt.Rows.Add(simpleInfoRow)

                            If dbgSimpleResult IsNot Nothing Then
                                For Each dr As DataRow In dbgSimpleResult.Rows
                                    Dim row As DataRow = dt.NewRow()
                                    row("Source") = "Simple"
                                    row("Col1") = "Id=" & SafeStr(dr("MemberId"))
                                    row("Col2") = "Name=" & SafeStr(dr("Name"))
                                    row("Col3") = ""
                                    row("Col4") = ""
                                    row("Col5") = ""
                                    dt.Rows.Add(row)
                                Next
                            End If
                        Catch exSimple As Exception
                            Dim simpleErrRow As DataRow = dt.NewRow()
                            simpleErrRow("Source") = "SimpleQuery_Error"
                            simpleErrRow("Col1") = exSimple.Message
                            simpleErrRow("Col2") = ""
                            simpleErrRow("Col3") = ""
                            simpleErrRow("Col4") = ""
                            simpleErrRow("Col5") = ""
                            dt.Rows.Add(simpleErrRow)
                        End Try

                        ' 3) Check Relationship data for Scenarios (DimId=0)
                        Dim dbgRelForScenarios As String =
                            "SELECT TOP 5 r.DimId, r.DimTypeId, r.ParentId, r.ChildId " &
                            "FROM Relationship r WHERE r.DimId = 0 ORDER BY r.ChildId"
                        Try
                            Dim dbgRelResult As DataTable = BRApi.Database.ExecuteSql(dbConn, dbgRelForScenarios, False)
                            Dim relInfoRow As DataRow = dt.NewRow()
                            relInfoRow("Source") = "RelForScen"
                            relInfoRow("Col1") = "RowCount=" & If(dbgRelResult IsNot Nothing, dbgRelResult.Rows.Count.ToString(), "NULL")
                            relInfoRow("Col2") = ""
                            relInfoRow("Col3") = ""
                            relInfoRow("Col4") = ""
                            relInfoRow("Col5") = ""
                            dt.Rows.Add(relInfoRow)

                            If dbgRelResult IsNot Nothing Then
                                For Each dr As DataRow In dbgRelResult.Rows
                                    Dim row As DataRow = dt.NewRow()
                                    row("Source") = "RelScen"
                                    row("Col1") = "DimId=" & SafeStr(dr("DimId"))
                                    row("Col2") = "DimTypeId=" & SafeStr(dr("DimTypeId"))
                                    row("Col3") = "ParentId=" & SafeStr(dr("ParentId"))
                                    row("Col4") = "ChildId=" & SafeStr(dr("ChildId"))
                                    row("Col5") = ""
                                    dt.Rows.Add(row)
                                Next
                            End If
                        Catch exRel As Exception
                            Dim relErrRow As DataRow = dt.NewRow()
                            relErrRow("Source") = "RelForScen_Error"
                            relErrRow("Col1") = exRel.Message
                            relErrRow("Col2") = ""
                            relErrRow("Col3") = ""
                            relErrRow("Col4") = ""
                            relErrRow("Col5") = ""
                            dt.Rows.Add(relErrRow)
                        End Try

                    Case Else
                        dt.Columns.Add("Error", GetType(String))
                        Dim errRow As DataRow = dt.NewRow()
                        errRow("Error") = "Metadata_Data: Unknown QueryType: " & queryType
                        dt.Rows.Add(errRow)

                End Select

                ' Close the DB connection to prevent DbConnInfo finalizer errors
                If dbConn IsNot Nothing Then
                    dbConn.Close()
                End If

            Catch ex As Exception
                ' Ensure connection is closed even on error
                If dbConn IsNot Nothing Then
                    Try
                        dbConn.Close()
                    Catch
                    End Try
                End If
                dt.Columns.Clear()
                dt.Columns.Add("Error", GetType(String))
                Dim catchRow As DataRow = dt.NewRow()
                catchRow("Error") = "Metadata_Data error: " & ex.Message
                dt.Rows.Add(catchRow)
            End Try

            Return resultDs
        End Function

        ''' <summary>Returns a safe string from a DataRow column value, handling DBNull.</summary>
        Private Function SafeStr(value As Object) As String
            If value Is Nothing OrElse IsDBNull(value) Then Return String.Empty
            Return value.ToString()
        End Function

        ''' <summary>Returns the standard dim type name for a given DimTypeId.</summary>
        Private Function GetDimTypeName(dimTypeId As Integer) As String
            Select Case dimTypeId
                Case 0 : Return "Account"
                Case 1 : Return "Entity"
                Case 2 : Return "Scenario"
                Case 3 : Return "Time"
                Case 4 : Return "View"
                Case 5 : Return "Flow"
                Case 6 : Return "Origin"
                Case 7 : Return "IC"
                Case 8 : Return "UD1"
                Case 9 : Return "UD2"
                Case 10 : Return "UD3"
                Case 11 : Return "UD4"
                Case 12 : Return "UD5"
                Case 13 : Return "UD6"
                Case 14 : Return "UD7"
                Case 15 : Return "UD8"
                Case Else : Return "Unknown_" & dimTypeId.ToString()
            End Select
        End Function

        ''' <summary>Returns the dimension group name for the webview dropdown filter.</summary>
        Private Function GetDimGroup(dimTypeId As Integer) As String
            Select Case dimTypeId
                Case 0 : Return "Entity"
                Case 1 : Return "Parent"
                Case 2 : Return "Scenario"
                Case 3 : Return "Time"
                Case 4 : Return "View"
                Case 5 : Return "Account"
                Case 6 : Return "Flow"
                Case 7 : Return "Origin"
                Case 8 : Return "IC"
                Case 9 : Return "UD1"
                Case 10 : Return "UD2"
                Case 11 : Return "UD3"
                Case 12 : Return "UD4"
                Case 13 : Return "UD5"
                Case 14 : Return "UD6"
                Case 15 : Return "UD7"
                Case 16 : Return "UD8"
                Case Else : Return "Other"
            End Select
        End Function

        ''' <summary>Adds a property row to GetMemberProperties result table.</summary>
        Private Sub AddPropertyRow(dt As DataTable, name As String, value As String, propType As String, isStandard As Boolean)
            Dim row As DataRow = dt.NewRow()
            row("PropertyName") = name
            row("PropertyValue") = If(value, String.Empty)
            row("PropertyType") = propType
            row("IsStandard") = isStandard
            dt.Rows.Add(row)
        End Sub

        ''' <summary>Adds a row to GetMemberAllProperties result table.</summary>
        Private Sub AddAllPropertyRow(dt As DataTable, name As String, propType As String,
                                      dataType As String, scenarioTypeId As Integer,
                                      scenarioTypeName As String, timeId As Integer,
                                      timeName As String, cubeType As String, value As String)
            Dim row As DataRow = dt.NewRow()
            row("PropertyName") = name
            row("PropertyType") = propType
            row("DataType") = dataType
            row("ScenarioTypeId") = scenarioTypeId
            row("ScenarioTypeName") = If(scenarioTypeName, String.Empty)
            row("TimeId") = timeId
            row("TimeName") = If(timeName, String.Empty)
            row("CubeType") = If(cubeType, String.Empty)
            row("Value") = If(value, String.Empty)
            row("EnumOptions") = String.Empty
            dt.Rows.Add(row)
        End Sub

        ''' <summary>Adds a formula type row.</summary>
        Private Sub AddFormulaTypeRow(dt As DataTable, id As Integer, name As String)
            Dim row As DataRow = dt.NewRow()
            row("FormulaTypeId") = id
            row("FormulaTypeName") = name
            dt.Rows.Add(row)
        End Sub

    End Class

End Namespace
