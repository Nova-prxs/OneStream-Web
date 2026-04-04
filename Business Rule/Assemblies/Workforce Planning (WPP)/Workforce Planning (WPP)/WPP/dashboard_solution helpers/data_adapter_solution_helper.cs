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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.data_adapter_solution_helper
{
	public class MainClass
	{
		public object Main(SessionInfo si, BRGlobals globals, object api, DashboardDataSetArgs args)
		{
			try
			{
				switch (args.FunctionType)
				{
					case DashboardDataSetFunctionType.GetDataSetNames:
						List<string> names = new List<string>();
						names.Add("TestFunction");
						return names;
					
					case DashboardDataSetFunctionType.GetDataSet:
						
						if (args.DataSetName.XFEqualsIgnoreCase("TestFunction"))
						{
							
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
