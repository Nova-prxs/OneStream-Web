using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
	public class ExportTables
	{
		#region Execute All
		public void ExecuteFullProcess(
			SessionInfo si,
			string tablePrefix = "XFW_TCR",
			string destinationFolder = "Documents/Users/sbioque/Exports",
			string originPath = "Documents/Users/nova/Imports"
		)
		{
			ExportTablesToInsertFiles(si, tablePrefix, destinationFolder);
			ExecuteInsertFilesFromFolder(si, originPath);
		}
		#endregion
		
		#region "Create INSERTS"

		#region "Export Tabbles to Insert"

		public void ExportTablesToInsertFiles(
			SessionInfo si,
			string tablePrefix,
			string destinationFolder
		)
		{
			CreateClean_TXT(
				si: si,
				tablePrefix: tablePrefix,
				folderPath: destinationFolder
			);

			List<string> allTables = GetTablesByPrefix(si, tablePrefix);

			List<string> orderedTablesFK = GetTablesOrderedForInsert(si, tablePrefix);

			List<string> independentTables =
				allTables.Except(orderedTablesFK).OrderBy(t => t).ToList();

			List<string> finalInsertOrder = new List<string>();
			finalInsertOrder.AddRange(orderedTablesFK);
			finalInsertOrder.AddRange(independentTables);

			int orderIndex = 1;

			foreach (string tableName in finalInsertOrder)
			{
				DataTable dt = GetTableData(si, tableName);
				if (dt == null || dt.Rows.Count == 0) continue;

				CreateInsert_TXT(
					si: si,
					dt: dt,
					tableName: tableName,
					folderPath: destinationFolder,
					fileName: $"{orderIndex}_{tableName}_Insert.txt"
				);

				orderIndex += 1;
			}
		}

		#endregion

		#region "Get Tables Info - By Prefix and Data"

		public List<string> GetTablesByPrefix(
			SessionInfo si,
			string tablePrefix
		)
		{
			List<string> result = new List<string>();

			string sql =
				$@"SELECT TABLE_NAME
			      FROM INFORMATION_SCHEMA.TABLES
			      WHERE TABLE_TYPE = 'BASE TABLE'
			      AND TABLE_NAME LIKE '{tablePrefix}%'";

			DataTable dt = ExecuteSqlData(si, sql);

			foreach (DataRow row in dt.Rows)
			{
				result.Add(row["TABLE_NAME"].ToString());
			}

			return result;
		}

		public DataTable GetTableData(
			SessionInfo si,
			string tableName
		)
		{
			string sql = $"SELECT * FROM {tableName}";
			return ExecuteSqlData(si, sql);
		}

		#endregion

		#region "Get Tables Ordered For Insert"

		public List<string> GetTablesOrderedForInsert(
			SessionInfo si,
			string tablePrefix
		)
		{
			DataTable dependencies = GetForeignKeyDependencies(si);
			Dictionary<string, List<string>> graph =
				BuildDependencyGraph(dependencies, tablePrefix);

			return TopologicalSort(graph);
		}

		#endregion

		#region "Create Insert TXT"

		public void CreateInsert_TXT(
			SessionInfo si,
			DataTable dt,
			string tableName,
			string columnMapping = null,
			string folderPath = "Documents/Users/sbioque",
			string fileName = "TableName_Insert.txt"
		)
		{
			XFFolderEx folderEX = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, $"{folderPath}");

			List<string> insertStatements = BuildInsert(si, dt, tableName, columnMapping);

			StringBuilder fileContent = new StringBuilder();

			foreach (string insertStmt in insertStatements)
			{
				fileContent.AppendLine(insertStmt);
				fileContent.AppendLine();
			}

			byte[] fileAsByte = System.Text.Encoding.UTF8.GetBytes(fileContent.ToString());

			XFFileInfo fileDataInfo = new XFFileInfo(FileSystemLocation.ApplicationDatabase, $"{fileName}", folderEX.XFFolder.FullName);
			XFFile fileData = new XFFile(fileDataInfo, string.Empty, fileAsByte);
			fileDataInfo.ContentFileExtension = "txt";
			BRApi.FileSystem.InsertOrUpdateFile(si, fileData);
		}

		public void CreateClean_TXT(
			SessionInfo si,
			string tablePrefix,
			string folderPath
		)
		{
			List<string> deleteStatements =
				GenerateCleanStatements(si, tablePrefix);

			StringBuilder content = new StringBuilder();

			content.AppendLine("-- AUTO-GENERATED CLEAN SCRIPT");
			content.AppendLine("-- Order respects FK dependencies");
			content.AppendLine();

			foreach (string stmt in deleteStatements)
			{
				content.AppendLine(stmt);
			}

			byte[] bytes =
				Encoding.UTF8.GetBytes(content.ToString());

			XFFolderEx folderEX =
				BRApi.FileSystem.GetFolder(
					si,
					FileSystemLocation.ApplicationDatabase,
					folderPath
				);

			XFFileInfo fileInfo = new XFFileInfo(
				FileSystemLocation.ApplicationDatabase,
				$"0_Clean_{tablePrefix}.sql",
				folderEX.XFFolder.FullName
			);

			fileInfo.ContentFileExtension = "sql";

			XFFile file = new XFFile(fileInfo, string.Empty, bytes);

			BRApi.FileSystem.InsertOrUpdateFile(si, file);
		}

		#endregion

		#region "Build Insert"

		public List<string> BuildInsert(SessionInfo si, DataTable dt, string tableName, object columnMapping = null)
		{
			List<string> insertStatements = new List<string>();
			int batchSize = 1000;
			Dictionary<string, string> columnTypes = GetColumnTypesFromDatabase(si, tableName);

			if (columnTypes == null || columnTypes.Count == 0)
			{
				throw new Exception("No se pudieron obtener los tipos de columna de la base de datos.");
			}

			StringBuilder columnsPart = new StringBuilder();
			columnsPart.Append("INSERT INTO " + tableName + " (");

			if (columnMapping == null)
			{
				foreach (DataColumn column in dt.Columns)
				{
					columnsPart.Append(column.ColumnName + ", ");
				}
			}
			else
			{
				foreach (DataColumn column in dt.Columns)
				{
					columnsPart.Append(((dynamic)columnMapping)[column.ColumnName] + ", ");
				}
			}

			columnsPart.Length -= 2;
			columnsPart.Append(") VALUES ");

			StringBuilder currentBatch = new StringBuilder();
			int rowCount = 0;
			foreach (DataRow row in dt.Rows)
			{
				currentBatch.Append("(");

				foreach (DataColumn column in dt.Columns)
				{
					object value = row[column];
					string columnName = columnMapping == null ? column.ColumnName : ((dynamic)columnMapping)[column.ColumnName];

					if (columnTypes.ContainsKey(columnName))
					{
						string columnType = columnTypes[columnName];

						if (value == DBNull.Value)
						{
							currentBatch.Append("NULL, ");
						}
						else if (columnType == "nvarchar" || columnType == "varchar")
						{
							currentBatch.Append("'" + value.ToString().Replace("'", "''").Replace(";", "-") + "', ");
						}
						else if (columnType == "datetime" || columnType == "date")
						{
							currentBatch.Append("'" + ((DateTime)value).ToString("yyyyMMdd") + "', ");
						}
						else if (columnType == "decimal")
						{
							currentBatch.Append(Convert.ToDecimal(value).ToString(System.Globalization.CultureInfo.InvariantCulture) + ", ");
						}
						else if (columnType == "bit")
						{
							currentBatch.Append((Convert.ToBoolean(value) ? "1" : "0") + ", ");
						}
						else
						{
							currentBatch.Append("'" + value.ToString().Replace("'", "''") + "', ");
						}
					}
				}

				currentBatch.Length -= 2;
				currentBatch.Append("), ");

				rowCount += 1;

				if (rowCount >= batchSize)
				{
					insertStatements.Add(columnsPart.ToString() + currentBatch.ToString().TrimEnd(' ').TrimEnd(',') + ";");
					currentBatch.Clear();
					rowCount = 0;
				}
			}

			if (currentBatch.Length > 0)
			{
				insertStatements.Add(columnsPart.ToString() + currentBatch.ToString().TrimEnd(' ').TrimEnd(',') + ";");
			}

			return insertStatements;
		}

		public Dictionary<string, string> GetColumnTypesFromDatabase(SessionInfo si, string tableName)
		{
			Dictionary<string, string> columnTypes = new Dictionary<string, string>();
			string query = $"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'";
			DataTable dtInfo = ExecuteSqlData(si, query);

			foreach (DataRow reader in dtInfo.Rows)
			{
				string columnName = reader["COLUMN_NAME"].ToString();
				string dataType = reader["DATA_TYPE"].ToString().ToLower();
				columnTypes[columnName] = dataType;
			}

			return columnTypes;
		}

		#endregion

		#region "Create Clean TXT"

		public DataTable GetForeignKeyDependencies(SessionInfo si)
		{
			string sql = @"
		        SELECT
		            OBJECT_NAME(fk.parent_object_id)     AS ChildTable,
		            OBJECT_NAME(fk.referenced_object_id) AS ParentTable
		        FROM sys.foreign_keys fk
		    ";

			return ExecuteSqlData(si, sql);
		}

		public Dictionary<string, List<string>> BuildDependencyGraph(
			DataTable dependencies,
			string tablePrefix
		)
		{
			Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>();

			foreach (DataRow row in dependencies.Rows)
			{
				string child = row["ChildTable"].ToString();
				string parent = row["ParentTable"].ToString();

				if (!child.StartsWith(tablePrefix) ||
				    !parent.StartsWith(tablePrefix)) continue;

				if (!graph.ContainsKey(child))
				{
					graph[child] = new List<string>();
				}

				graph[child].Add(parent);

				if (!graph.ContainsKey(parent))
				{
					graph[parent] = new List<string>();
				}
			}

			return graph;
		}

		public List<string> TopologicalSort(
			Dictionary<string, List<string>> graph
		)
		{
			List<string> result = new List<string>();
			HashSet<string> visited = new HashSet<string>();

			foreach (var node in graph.Keys)
			{
				VisitNode(node, graph, visited, result);
			}

			return result;
		}

		public void VisitNode(
			string node,
			Dictionary<string, List<string>> graph,
			HashSet<string> visited,
			List<string> result
		)
		{
			if (visited.Contains(node)) return;

			visited.Add(node);

			foreach (var parent in graph[node])
			{
				VisitNode(parent, graph, visited, result);
			}

			result.Add(node);
		}

		public List<string> GenerateCleanStatements(
			SessionInfo si,
			string tablePrefix
		)
		{
			List<string> allTables = GetTablesByPrefix(si, tablePrefix);

			DataTable dependencies = GetForeignKeyDependencies(si);
			Dictionary<string, List<string>> graph =
				BuildDependencyGraph(dependencies, tablePrefix);

			List<string> orderedTablesFK =
				TopologicalSort(graph);

			List<string> orderedForDelete = new List<string>(orderedTablesFK);
			orderedForDelete.Reverse();

			List<string> independentTables =
				allTables.Except(orderedTablesFK).OrderBy(t => t).ToList();

			List<string> finalCleanOrder = new List<string>();
			finalCleanOrder.AddRange(orderedForDelete);
			finalCleanOrder.AddRange(independentTables);

			List<string> deletes = new List<string>();
			foreach (string tableName in finalCleanOrder)
			{
				deletes.Add($"DELETE FROM {tableName};");
			}

			return deletes;
		}

		#endregion

		#endregion

		#region "Execute INSERTS"

		public void ExecuteInsertFilesFromFolder(
			SessionInfo si,
			string originPath
		)
		{
			List<string> fileExtensions = new List<string> { "sql", "txt" };

			List<XFFileInfoEx> filesFolder =
				BRApi.FileSystem.GetFilesInFolder(
					si,
					FileSystemLocation.ApplicationDatabase,
					originPath,
					XFFileType.All,
					fileExtensions
				);

			List<XFFileInfoEx> orderedFiles =
				filesFolder.OrderBy(f => Convert.ToInt32(f.XFFileInfo.Name.Split('_')[0])).ToList();

			foreach (XFFileInfoEx fileEx in orderedFiles)
			{
				string fileName = fileEx.XFFileInfo.Name;
				string filePath = $"{originPath}/{fileName}";
				
				XFFileEx file =
					BRApi.FileSystem.GetFile(
						si,
						FileSystemLocation.ApplicationDatabase,
						filePath,
						true,
						true
					);

				string sqlText =
					Encoding.UTF8.GetString(file.XFFile.ContentFileBytes);

				string[] statements = sqlText.Split(';');

				foreach (string stmt in statements)
				{
					string sql = stmt.Trim();

					if (sql == string.Empty) continue;

					BRApi.ErrorLog.LogMessage(si, sql);
					ExecuteSql(si, sql);
				}
			}
		}

		#endregion

		#region "Exequte SQL"

		public void ExecuteSql(SessionInfo si, string sqlCmd)
		{
			using (DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
			{
				BRApi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, true, true);
			}
		}

		public DataTable ExecuteSqlData(SessionInfo si, string sqlCmd)
		{
			DataTable dt = null;
			using (DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
			{
				dt = BRApi.Database.ExecuteSql(dbConnApp, sqlCmd, true);
			}
			return dt;
		}

		#endregion
	}
	
	public class ReadFiles
	{
		
	}
}
