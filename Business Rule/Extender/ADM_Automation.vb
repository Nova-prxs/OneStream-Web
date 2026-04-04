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

namespace OneStream.BusinessRule.Extender.ADM_Automation
{
	public class MainClass
	{
		public object Main(SessionInfo si, BRGlobals globals, object api, ExtenderArgs args)
		{
			try
			{
				switch (args.FunctionType)
				{
				case ExtenderFunctionType.Unknown:
					break;
				case ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep:
					string paramFunction = args.NameValuePairs.XFGetValue("p_function");
					switch (paramFunction) {
					case "UpdateMetadata":
						string[] endpoints = {
							"ZFI_0005", "ZFI_0004", "ZFI_0012", "ZFI_0013", "ZFI_0006", "ZFI_0018", "ZFI_0002", "ZFI_0007", "ZFI_0010"
						};
						foreach (string endpoint in endpoints) {
							BRApi.Utilities.QueueDataMgmtSequence(
								si,
								BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, false, "AdminDataManagement"),
								"AdminDataManagement_ImportData_RESTAPI",
								new Dictionary<string, string>{
									{"p_endpoint", endpoint},
									{"p_custom_params", ""}
								}
							);
						}
						break;
					}
					break;
				case ExtenderFunctionType.ExecuteExternalDimensionSource:
					// Add External Members
					var externalMembers = new List<NameValuePair>();
					externalMembers.Add(new NameValuePair("YourMember1Name", "YourMember1Value"));
					externalMembers.Add(new NameValuePair("YourMember2Name", "YourMember2Value"));
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