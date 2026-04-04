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

Namespace OneStream.BusinessRule.Finance.OP_Recalculate
    	
    Public Class MainClass
    		
		Dim SharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		
    		
        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
            Try
                Select Case api.FunctionType					
    						
                    Case Is = FinanceFunctionType.CustomCalculate			
    							
                        If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("MappingBrand") Then
    						
                            Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")			
                            Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")							
                            Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))							
                            Dim sBrandText As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
    							
                            Dim brandGroupDict As New Dictionary(Of String, String) From {
    																{"Burger King", "P_BK"},
    																{"Fridays", "P_FR"},
    																{"Domino''s Pizza", "P_DP"},
    																{"Foster''s Hollywood", "P_FH"},
    																{"Foster''s Hollywood Valencia", "P_FHV"},
    																{"Starbucks", "P_SB"},
    																{"Starbucks Portugal", "P_SB_PT"},
    																{"Starbucks Bélgica", "P_SB_BEL"},
    																{"Starbucks Francia", "P_SB_PT"},
    																{"Starbucks Holanda", "P_SB_HO"},
    																{"Ginos", "P_GI"},
    																{"Ginos Portugal", "P_GI_PT"},
    																{"VIPS", "P_VI_Aux"}
    																}
    							
                            Dim sBrand As String = brandGroupDict(sBrandText)																
    							
                            'Build a dictionary to send the parameters to a DM
                            Dim customSubstVars As New Dictionary(Of String, String) From {
    																			{"p_scenario", sScenario},																
    																			{"p_year", sYear},																															
    																			{"p_brand", sBrand},																
    																			{"p_forecast_month", iForecastMonth}
    																		}
    																		
                            Dim customSubstVarsVipsSmart As New Dictionary(Of String, String) From {
    																			{"p_scenario", sScenario},																
    																			{"p_year", sYear},																															
    																			{"p_brand", "P_VS"},																
    																			{"p_forecast_month", iForecastMonth}
    																		}	
    															
                            Dim sBrandAgg As String = If(sBrand.Equals("P_FHV"), "M_FH", sBrand.Replace("P_", "M_"))						
                            Dim customSubstVarsAgg As New Dictionary(Of String, String) From {
    																			{"p_scenario", sScenario},																
    																			{"p_year", sYear},																															
    																			{"p_brand", sBrandAgg.Replace("_Aux", "")},																
    																			{"p_forecast_month", iForecastMonth}
    																		}																		
    																																			
    																		
                            BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "OP_Recalculate", customSubstVars)															
                            BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "ALL_Aggregate", customSubstVarsAgg)
    							
                        ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Recalculate") Then									
    									
                            Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
                            Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
                            Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
                            Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")				
    							
                            Dim sActualYear As String = If(sScenario = "Budget", (Integer.Parse(sYear) - 1).ToString(), sYear)
    							
                            Dim iTime As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Time.Id, sYear)
                            Dim sText1 As String = BRApi.Finance.Entity.Text(si, Api.Pov.Entity.MemberId, 2, 0, iTime)		
    							
                            Dim sEntity As String = Api.Pov.Entity.Description
                            Dim sTime As String = Api.Pov.Time.Description
                            'BRAPI.ErrorLog.LogMessage(si, "Registro: " & sEntity & " - " & sTime)
    							
                            If Not (sText1.Equals("Comparables") _
                            Or sText1.Equals("Cerradas") _
                            Or sText1.Equals("Reformas") _
                            Or sText1.Equals("Reformas Año Anterior")) Then		
    						 	
                                'Get OP Calculates Main Class
	                             Dim OP_Calculates As New OneStream.BusinessRule.Finance.OP_Calculates.MainClass
	    								
	                                'Execute distribute for that opening entity
	                             OP_Calculates.ExecuteDistribute(si, api.Pov.Entity.Name, sScenario, sYear, "Own", iForecastMonth)
    								
                            End If
    
                        End If
    						
                End Select
    
                Return Nothing
    				
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
    			
        End Function
    
    End Class
    	
End Namespace