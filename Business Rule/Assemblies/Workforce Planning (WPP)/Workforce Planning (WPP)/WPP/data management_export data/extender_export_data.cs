using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_export_data
{
	public class MainClass
	{
		//Reference "UTI_SharedFunctionsCS" Business Rule
		OneStream.BusinessRule.Finance.UTI_SharedFunctionsCS.MainClass UTISharedFunctionsBR = new OneStream.BusinessRule.Finance.UTI_SharedFunctionsCS.MainClass();
		
		public object Main(SessionInfo si, BRGlobals globals, object api, ExtenderArgs args)
		{
			try
			{
				switch (args.FunctionType)
				{
					case ExtenderFunctionType.Unknown:
						break;
						
					case ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep:
						//Get parameters and create full path and declare column mapping dictionary
						string sheetName = args.NameValuePairs["p_sheet"];
						string parentPath = $"Documents/Users/{si.UserName}";
						string childPath = "Exports";
						string fullPath = $"{parentPath}/{childPath}";
						string queryParams = args.NameValuePairs["p_query_params"];
						string WSPrefix = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, Guid.Empty, "WPP.prm_WPP_Workspace_Prefix");
						
						//Create folder if necessary
						BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, parentPath, childPath);
						
						//Create data table depending on the table parameter
						DataTable dt = UTISharedFunctionsBR.CreateDataTableFromSheetName(si, sheetName, queryParams, WSPrefix);
						
						//Create file
						UTISharedFunctionsBR.GenerateExcelFileFromDataTable(si, dt, sheetName, FileSystemLocation.ApplicationDatabase, fullPath);
						break;
						
					case ExtenderFunctionType.ExecuteExternalDimensionSource:
						//Add External Members
						var externalMembers = new List<NameValuePair>();
						externalMembers.Add(new NameValuePair("YourMember1Name","YourMember1Value"));
						externalMembers.Add(new NameValuePair("YourMember2Name","YourMember2Value"));
						return externalMembers;
				}

				return null;
			}
			catch (Exception ex)
			{
				throw ErrorHandler.LogWrite(si, new XFException(si, ex));
			}
		}
		
	}
}
