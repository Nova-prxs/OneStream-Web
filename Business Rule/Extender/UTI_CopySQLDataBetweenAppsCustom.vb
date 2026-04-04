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

namespace OneStream.BusinessRule.Extender.UTI_CopySQLDataBetweenAppsCustom
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
				case ExtenderFunctionType.Unknown:
					// Get parameters
					string paramSourceApp = "Horse_Development";
					if (string.IsNullOrEmpty(paramSourceApp)) {
						throw new Exception("error copying data: no source app has been provided");
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
						BRApi.Database.ExecuteSql(
							internalDbConn,
							$@"
							DELETE pc
							FROM XFC_INV_FACT_project_cash pc
							WHERE EXISTS (
								SELECT 1
								FROM XFC_INV_MASTER_project p
								WHERE p.company_id = 671
									AND pc.project_id = p.project
							);
							",
							false
						);
						DataTable dt = BRApi.Database.ExecuteSql(
							externalDbConn,
							$@"
							SELECT *
							FROM XFC_INV_FACT_project_cash pc
							WHERE EXISTS (
								SELECT 1
								FROM XFC_INV_MASTER_project p
								WHERE p.company_id = 671
									AND pc.project_id = p.project
							);
							",
							false
						);
						if (dt == null || dt.Rows.Count == 0) {return null;}
						UTIBusinessRule.LoadDataTableToCustomTable(si, "XFC_INV_FACT_project_cash", dt, "merge");
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