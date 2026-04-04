using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace OneStream.BusinessRule.Extender.UTI_CopySQLDataBetweenApps
{
	public class MainClass
	{
		public OneStream.BusinessRule.Finance.UTI_SharedFunctions_CS.MainClass UTIBusinessRule = new OneStream.BusinessRule.Finance.UTI_SharedFunctions_CS.MainClass();
		
		public object Main(SessionInfo si, BRGlobals globals, object api, ExtenderArgs args)
		{
			try
			{
				switch (args.FunctionType)
				{
				case ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep:
					// Get parameters
					string paramSourceApp = args.NameValuePairs.XFGetValue("p_source_app");
					string[] paramTablesToCopy = args.NameValuePairs.XFGetValue("p_tables_to_copy").Split(",");
					if (string.IsNullOrEmpty(paramSourceApp) || paramTablesToCopy.Length == 0) {
						throw new Exception("error copying data: no source app or tables to copy have been provided");
					}
					
					// Create another app session info
					OpenAppResult openAppResult = new OpenAppResult();
					SessionInfo externalSi = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, paramSourceApp, out openAppResult);
					if (openAppResult != OpenAppResult.Success) {
						throw new Exception($"error opening source app: {openAppResult.ToString()}");
					}
					
					// Copy data between apps
					using (
						DbConnInfoApp internalDbConn = BRApi.Database.CreateApplicationDbConnInfo(si),
						externalDbConn = BRApi.Database.CreateApplicationDbConnInfo(externalSi)
					) {
						foreach (string tableName in paramTablesToCopy) {
							DataTable dt = BRApi.Database.ExecuteSql(
								externalDbConn,
								$@"
								SELECT *
								FROM {tableName};
								",
								false
							);
							if (dt == null || dt.Rows.Count == 0) {continue;}
							UTIBusinessRule.LoadDataTableToCustomTable(si, tableName, dt, "replace");
						}
					}
					break;
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