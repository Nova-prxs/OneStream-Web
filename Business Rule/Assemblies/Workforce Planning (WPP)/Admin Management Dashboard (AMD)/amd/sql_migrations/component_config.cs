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
	public class component_config : IMigration
	{
		string tableName = "XFC_HR_AMD_ComponentConfig";

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
					    id                  INT IDENTITY(1,1) NOT NULL,
					    dashboard_context   VARCHAR(50)   NOT NULL,
					    dashboard_name      VARCHAR(255)  NOT NULL,
					    sort_order          INT           NOT NULL DEFAULT 0,
					    template_name_suffix VARCHAR(255) NOT NULL,
					    p_public_name       VARCHAR(255)  NULL,
					    p_import_endpoint   VARCHAR(255)  NULL,
					    p_table_name        VARCHAR(255)  NULL,
					    p_import_table_name VARCHAR(255)  NULL,
					    p_import_folder     VARCHAR(500)  NULL,
					    p_import_files_type VARCHAR(50)   NULL,
					    p_import_method     VARCHAR(50)   NULL,
					    p_import_delimiter  VARCHAR(50)   NULL,
					    p_has_file          VARCHAR(10)   NULL,
					    is_active           BIT           NOT NULL DEFAULT 1,
					    CONSTRAINT PK_{tableName} PRIMARY KEY (id)
					)
				END;
				";

				string downQuery = $@"
					DROP TABLE IF EXISTS {tableName};
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
					    VALUES
					        -- 01_amd_Metadata_Repeater (4 items)
					        ('AMD', '01_amd_Metadata_Repeater', 1, 'Master CECO',
					         'Master CECO', '', 'XFC_MAIN_MASTER_Companies', 'XFC_MAIN_MASTER_Companies',
					         'Documents/Public/Admin Data Management/Import data', 'delimited', 'replace', ',', '0'),

					        ('AMD', '01_amd_Metadata_Repeater', 2, 'Mapeo Cuentas Contables',
					         'Mapeo Cuentas Contables', '', 'XFC_MAIN_MASTER_Companies', 'XFC_MAIN_MASTER_Companies',
					         'Documents/Public/Admin Data Management/Import data', 'delimited', 'replace', ',', '0'),

					        ('AMD', '01_amd_Metadata_Repeater', 3, 'Master Conceptos',
					         'Master Conceptos', '', 'XFC_MAIN_MASTER_Companies', 'XFC_MAIN_MASTER_Companies',
					         'Documents/Public/Admin Data Management/Import data', 'delimited', 'replace', ',', '0'),

					        ('AMD', '01_amd_Metadata_Repeater', 4, 'Master Estructura O.',
					         'Master Estructura O.', '', 'XFC_MAIN_MASTER_Companies', 'XFC_MAIN_MASTER_Companies',
					         'Documents/Public/Admin Data Management/Import data', 'delimited', 'replace', ',', '0'),

					        -- 01_amd_Data_Repeater (5 items)
					        ('AMD', '01_amd_Data_Repeater', 1, 'Nomina Real',
					         'Nomina Real', '', 'XFC_HR_RAW_GLB_NominaReal', 'XFC_HR_RAW_GLB_NominaReal',
					         'Documents/Public/Admin Data Management/Import data/XFC_HR_RAW_GLB_NominaReal', 'delimited', 'replace', 'pipe', '0'),

					        ('AMD', '01_amd_Data_Repeater', 2, 'Distribucion CECO',
					         'Distribucion CECO', '', 'XFC_HR_RAW_GLB_DistribCECO', 'XFC_HR_RAW_GLB_DistribCECO',
					         'Documents/Public/Admin Data Management/Import data/XFC_HR_RAW_GLB_DistribCECO', 'delimited', 'replace', 'pipe', '0'),

					        ('AMD', '01_amd_Data_Repeater', 3, 'Master Empleados',
					         'Master Empleados', '', 'XFC_HR_MASTER_GLB_Empleados', 'XFC_HR_MASTER_GLB_Empleados',
					         'Documents/Public/Admin Data Management/Import data/XFC_HR_MASTER_GLB_Empleados', 'delimited', 'replace', 'pipe', '0'),

					        ('AMD', '01_amd_Data_Repeater', 4, 'Nomina Teorica',
					         'Nomina Teorica', '', 'XFC_HR_RAW_GLB_NominaTeorica', 'XFC_HR_RAW_GLB_NominaTeorica',
					         'Documents/Public/Admin Data Management/Import data/XFC_HR_RAW_GLB_NominaTeorica', 'delimited', 'replace', 'pipe', '0'),

					        ('AMD', '01_amd_Data_Repeater', 5, 'Absentismo',
					         'Absentismo', '', 'XFC_HR_RAW_GLB_Absentismo', 'XFC_HR_RAW_GLB_Absentismo',
					         'Documents/Public/Admin Data Management/Import data/XFC_HR_RAW_GLB_Absentismo', 'delimited', 'replace', 'pipe', '0')
					) AS source (
					    dashboard_context, dashboard_name, sort_order, template_name_suffix,
					    p_public_name, p_import_endpoint, p_table_name, p_import_table_name,
					    p_import_folder, p_import_files_type, p_import_method, p_import_delimiter, p_has_file
					)
					ON  target.dashboard_context   = source.dashboard_context
					AND target.dashboard_name      = source.dashboard_name
					AND target.template_name_suffix = source.template_name_suffix
					WHEN MATCHED THEN
					    UPDATE SET
					        sort_order          = source.sort_order,
					        p_public_name       = source.p_public_name,
					        p_import_endpoint   = source.p_import_endpoint,
					        p_table_name        = source.p_table_name,
					        p_import_table_name = source.p_import_table_name,
					        p_import_folder     = source.p_import_folder,
					        p_import_files_type = source.p_import_files_type,
					        p_import_method     = source.p_import_method,
					        p_import_delimiter  = source.p_import_delimiter,
					        p_has_file          = source.p_has_file
					WHEN NOT MATCHED THEN
					    INSERT (
					        dashboard_context, dashboard_name, sort_order, template_name_suffix,
					        p_public_name, p_import_endpoint, p_table_name, p_import_table_name,
					        p_import_folder, p_import_files_type, p_import_method, p_import_delimiter, p_has_file
					    )
					    VALUES (
					        source.dashboard_context, source.dashboard_name, source.sort_order, source.template_name_suffix,
					        source.p_public_name, source.p_import_endpoint, source.p_table_name, source.p_import_table_name,
					        source.p_import_folder, source.p_import_files_type, source.p_import_method, source.p_import_delimiter, source.p_has_file
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
