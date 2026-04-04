using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
	public class mapping : IMigration
	{
		string tableName = "XFC_HR_MASTER_Mapping";

		#region "Get Migration Query"

		public string GetMigrationQuery(SessionInfo si, string type)
		{
			try
			{
				if (type.ToLower() != "up" && type.ToLower() != "down")
					throw new XFException(si, new Exception("Migration type must be 'up' or 'down'"));

				string upQuery = $@"
				IF OBJECT_ID(N'{tableName}', N'U') IS NULL
				BEGIN
					CREATE TABLE {tableName} (
					    table_name     VARCHAR(255) NOT NULL,
					    source_column  VARCHAR(255) NOT NULL,
					    target_column  VARCHAR(255) NOT NULL,
					    display        VARCHAR(255) NULL,
					    CONSTRAINT PK_{tableName} PRIMARY KEY (table_name, source_column)
					)
				END;

				IF OBJECT_ID(N'{tableName}', N'U') IS NOT NULL
				AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'{tableName}') AND name = 'display')
				BEGIN
				    ALTER TABLE {tableName} ADD display VARCHAR(255) NULL;
				END;
				";

				string downQuery = $@"
					DROP TABLE {tableName};
				";

				return type.ToLower() == "up" ? upQuery : downQuery;
			}
			catch (Exception ex)
			{
				throw new XFException(si, ex);
			}
		}

		#endregion

		#region "Get Population Query"

		public string GetPopulationQuery(SessionInfo si, string type)
		{
			try
			{
				if (type.ToLower() != "up" && type.ToLower() != "down")
					throw new XFException(si, new Exception("Population type must be 'up' or 'down'"));

				string upQuery = $@"
					MERGE INTO {tableName} AS target
					USING (
					    SELECT 
					        table_name,
					        source_column,
					        target_column
					    FROM XFC_HR_RAW_Mapping
					) AS source
					ON  target.table_name    = source.table_name
					AND target.source_column = source.source_column
					WHEN MATCHED THEN
					    UPDATE SET
					        target_column = source.target_column
					WHEN NOT MATCHED THEN
					    INSERT (
					        table_name,
					        source_column,
					        target_column
					    )
					    VALUES (
					        source.table_name,
					        source.source_column,
					        source.target_column
					    );
				";

				string downQuery = $@"
					TRUNCATE TABLE {tableName};
				";

				return type.ToLower() == "up" ? upQuery : downQuery;
			}
			catch (Exception ex)
			{
				throw new XFException(si, ex);
			}
		}

		#endregion

	}
}