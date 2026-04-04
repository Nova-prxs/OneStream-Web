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

Namespace OneStream.BusinessRule.Extender.test_dt
	Public Class MainClass
		Public Shared Function Main(ByVal si As SessionInfo, ByVal params As BRParams) As Object

        ' Obtener el WorkflowClusterPk del contexto actual o como parámetro
        Dim wfClusterPk As WorkflowClusterPk = si.WorkflowClusterPk

        ' Alternativamente: permitir pasarlo desde params si es necesario
        If params.ContainsKey("WorkflowClusterPk") Then
            wfClusterPk = CType(params("WorkflowClusterPk"), WorkflowClusterPk)
        End If

        ' Obtener los datos validados de staging (StageTargetData)
        Dim stageData As List(Of StagingCellData) = BRApi.Stage.GetStageCellDataList(si, wfClusterPk)

        ' Crear la tabla en memoria que coincida con tu custom table
        Dim dt As New DataTable("Custom_StagingAudit")

        ' Define las columnas que tiene tu tabla personalizada
        dt.Columns.Add("Entity", GetType(String))
        dt.Columns.Add("Account", GetType(String))
        dt.Columns.Add("Scenario", GetType(String))
        dt.Columns.Add("Time", GetType(String))
        dt.Columns.Add("View", GetType(String))
        dt.Columns.Add("UD1", GetType(String))
        dt.Columns.Add("UD2", GetType(String))
        dt.Columns.Add("UD3", GetType(String))
        dt.Columns.Add("Amount", GetType(Decimal))

        ' Rellenar la DataTable con los datos del stage
        For Each cell As StagingCellData In stageData
            Dim row As DataRow = dt.NewRow()
            row("Entity") = cell.EntityName
            row("Account") = cell.AccountName
            row("Scenario") = cell.ScenarioName
            row("Time") = cell.TimeName
            row("View") = cell.ViewName
            row("UD1") = cell.UD1Name
            row("UD2") = cell.UD2Name
            row("UD3") = cell.UD3Name
            row("Amount") = cell.Amount
            dt.Rows.Add(row)
        Next

        ' Guardar en la custom table
        BRApi.Database.SaveCustomDataTable(si, "Custom_StagingAudit", dt)

        ' Mensaje de éxito
        Return String.Format("Se insertaron {0} filas en la tabla Custom_StagingAudit.", dt.Rows.Count)

    End Function
	End Class
End Namespace