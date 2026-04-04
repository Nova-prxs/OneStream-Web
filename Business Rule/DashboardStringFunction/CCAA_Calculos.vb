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

Namespace OneStream.BusinessRule.DashboardStringFunction.CCAA_Calculos
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				
				If args.FunctionName.XFEqualsIgnoreCase("sum17B") Then
					
					Dim sName As String = "Cash Breakdown"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma17B As String = "
										  (E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1681002:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round) 
										+ (E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1681004:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+ (E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1681022:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+ (E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1681023:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+ (E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1681001:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+ (E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1681021:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+ (E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1681000CF:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+ (E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1681020CF:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma17B &"):Name(""" & sName & """)"
				Else If args.FunctionName.XFEqualsIgnoreCase("sum16A") Then
					
					Dim sName As String = "Other liablities"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2901720:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round) 
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2901710:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2901730:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2901100:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2901800:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2941100L:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2901000CF:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2801600:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2701000C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2301720:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2301710:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2301730:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2301100:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2301000C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2201000C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2201300:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
				
				Else If args.FunctionName.XFEqualsIgnoreCase("sum8Db") Then
					
					Dim sName As String = "Deferred tax assets"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#FA1_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round) 
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#IM1_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#RU_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#PP1_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#PRP_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#POth_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#OTy_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#Loss_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#UNF_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#UTx_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#OthU_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
				
				Else If args.FunctionName.XFEqualsIgnoreCase("sum18A_current") Then
					
					Dim sName As String = "Current liabilities 18A"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569920:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round) 
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531010:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631300C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631310C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2561010C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2561020C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569910C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569930C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2941200L:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
				
				Else If args.FunctionName.XFEqualsIgnoreCase("sum18D_current") Then
					
					Dim sName As String = "Current liabilities 18D"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569920C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round) 
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569921C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531010C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531015C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531020C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531025C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631300CF:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631320:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631310C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2561010C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2561020C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2581010C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2581020C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569910C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569930C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2941200L:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
				
				Else If args.FunctionName.XFEqualsIgnoreCase("sum61a") Then
					
					Dim sName As String = "Revenue 61a"
					'Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#R0671:P#Horse_Group:C#Top:S#Actual:T#"& sTime &":V#YTD:A#541000:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R0611:P#Horse_Group:C#Top:S#Actual:T#"& sTime &":V#YTD:A#541000:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R1302:P#Horse_Group:C#Top:S#Actual:T#"& sTime &":V#YTD:A#541000:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R0592:P#Horse_Group:C#Top:S#Actual:T#"& sTime &":V#YTD:A#541000:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R0585:P#Horse_Group:C#Top:S#Actual:T#"& sTime &":V#YTD:A#541000:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R1303:P#Horse_Group:C#Top:S#Actual:T#"& sTime &":V#YTD:A#541000:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R1300:P#Horse_Group:C#Top:S#Actual:T#"& sTime &":V#YTD:A#541000:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R1301:P#Horse_Group:C#Top:S#Actual:T#"& sTime &":V#YTD:A#541000:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum4") Then
					
					Dim sName As String = "Revenue 4"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541101:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round) 
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541102:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541107:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541112:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541119:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541128:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541141:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541137:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541127:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541109:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541114:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541103:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541105:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541111:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541115:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541132:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#541140:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"
																								
					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum8Db") Then
					
					Dim sName As String = "Deferred Tax Assets 8Db"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#FA1_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#IM1_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#RU_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#PP1_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#PRP_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#POth_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#OTy_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#Loss_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#UNF_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#UTx_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#OthU_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"
																								
					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
					
				Else If args.FunctionName.XFEqualsIgnoreCase("dif3") Then
					
					Dim sName As String = "Deferred Tax Assets - Liabilities in Balance Sheet (3)"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC05:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#04PNC01:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"
																								
					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum18A") Then
					
					Dim sName As String = "Total liabilities 18A"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2149920:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2131010:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2151300C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2151310C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2141010C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2141020C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2149910C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569920:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531010:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631300C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631310C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2561010C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2561020C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569910C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569930C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2941200L:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum18B") Then
					
					Dim sName As String = "Total liabilities 18B"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#LOANCR_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#LLIAB_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#DERIV_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#OVL_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#OOFL_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
				
				Else If args.FunctionName.XFEqualsIgnoreCase("sum9A1") Then
					
					Dim sName As String = "Intangible Assets 9A1"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1021010C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1009100:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#Othint_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1021020C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1021004C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1009104:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#Othint1_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1021010L:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum9A1_flow") Then
					
					Dim sName As String = "Intangible Assets Flow 9A1"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					Dim sAcc As String = args.NameValuePairs("Account")
					'Dim sAcc As String = api.Data.GetDataCell("Account")
					'brapi.ErrorLog.LogMessage(si, $"{sAcc}")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F20:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F14:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F16:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F30:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F80C:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F80M:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F50:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F70:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"
					
						
					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum9B_flow") Then
					
					Dim sName As String = "Property plant and equipment Flow 9B"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					Dim sAcc As String = args.NameValuePairs("Account")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F20:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F14:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F16:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F30:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F80C:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F80M:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F58:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F50:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F70:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#"& sAcc &":F#F57:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum9B") Then
					
					Dim sName As String = "Property plant and equipment 9B"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1049000:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1049100:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1049200:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#INDMA_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TangA_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1031000C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#CP_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1049004:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1049104:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1049204:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#INDMAa:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TangD_AC:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1031004C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum18D_noncurrent") Then
					
					Dim sName As String = "NonCurrent liabilities 18D"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2149920:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round) 
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2131010C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2131015C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2131020C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2131025C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2151300C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2151310C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2141010C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2141020C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2149910C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum61a_ppeia") Then
					
					Dim sName As String = "Property plant and equipment and intangible assets 61a"
					'Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#R0671:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC01:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R0611:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC01:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R1302:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC01:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R0592:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC01:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R0585:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC01:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R1303:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC01:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R1300:P#Horse_Group:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC01:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R1301:P#Horse_Group:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC01:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R0671:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC02:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R0611:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC02:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R1302:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC02:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R0592:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC02:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R0585:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC02:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R1303:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC02:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R1300:P#Horse_Group:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC02:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#R1301:P#Horse_Group:C#Local:S#Actual:T#"& sTime &":V#YTD:A#01ANC02:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum18D_flows") Then
					
					Dim sName As String = "Current liabilities flow 18D"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569920C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round) 
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569921C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531010C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531015C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531020C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531025C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631300CF:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631320:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631310C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2561010C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2561020C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2581010C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2581020C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569910C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569930C:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2941200L:F#FSM1_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569920C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round) 
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569921C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531010C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531015C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531020C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531025C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631300CF:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631320:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631310C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2561010C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2561020C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2581010C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2581020C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569910C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569930C:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2941200L:F#FRM_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569920C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round) 
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569921C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531010C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531015C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531020C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2531025C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631300CF:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631320:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2631310C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2561010C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2561020C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2581010C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2581020C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569910C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2569930C:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2941200L:F#FRM3_AC:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
				
				Else If args.FunctionName.XFEqualsIgnoreCase("sum10") Then
					
					Dim sName As String = "Inventories 10"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501210:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501220:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501009L:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501400:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501610:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501800:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501620:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501214:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501224:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501404:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501244:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501614:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum20B") Then
					
					Dim sName As String = "Working Capital 20B"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFEX01:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFEX02:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFEX03:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFEX04:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFEX05:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum18A_noncurrent") Then
					
					Dim sName As String = "NonCurrent liabilities 18A"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2149920:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2131010:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2151300C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2151310C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2141010C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2141020C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2149910C:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum20A") Then
					
					Dim sName As String = "Working Capital 20A"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TCAF04:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round) 
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2061210:F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2521210:F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#572123:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2521620:F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2521000CF:F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2061620:F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2061210:F#F35:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2521210:F#F35:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2521620:F#F35:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2521000CF:F#F35:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2061620:F#F35:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2061000CF:F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2061000CF:F#F35:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2521330:F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2521330:F#F35:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2521430:F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2521430:F#F35:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501004C:F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1541004C:F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1541004C:F#F37:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501004C:F#F35:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1541004C:F#F35:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2041000C:F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#2041000C:F#F35:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#1501009L:F#F25:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#581114R:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#4911:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#4912:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#4927:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#581108R:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#4930:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#4933:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#4928:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)
										-(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#4929:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top_AC:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
				Else If args.FunctionName.XFEqualsIgnoreCase("sum20C") Then
					
					Dim sName As String = "Working Capital 20C"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFIN_G2:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFIN03:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
										+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFIN11:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
				
				Else If args.FunctionName.XFEqualsIgnoreCase("sum5") Then
					
					Dim sName As String = "Cash flow 5"
					Dim sEntity  As String = args.NameValuePairs("Entity")
					Dim sTime As String = args.NameValuePairs("Time")
					
					Dim suma As String = "
									 (E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#02AC07:F#F00:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round) 
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFIN07:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFIN_G3:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TUNB:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFEX:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TCAF01:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TCAF02:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TCAF_G:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TBFR:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFEX07:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFEX_G2:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TFEX09:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TINV02FRD1:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TINV02PAI2:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TINV023_G:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TINV_G1:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TINV_G2:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TINV_G4:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
									+(E#"& sEntity &":C#Local:S#Actual:T#"& sTime &":V#YTD:A#TINV_G3:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#None:U8#AC_Round)
										"

					Return $"GetDataCell("& suma &"):Name(""" & sName & """)"
					
				Else If args.FunctionName.XFEqualsIgnoreCase("SumOfPackage")
					
					Dim sOutput As String = String.Empty
					Dim sEntity As String = args.NameValuePairs("Entity")
					
					Dim sChildrenList As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, brapi.Finance.Dim.GetDimPk(si, "Entities"), $"E#{sEntity}.Children",True)

					For Each Entity_Children In sChildrenList
						Dim sChildName = Entity_Children.Member.Name
						'brapi.ErrorLog.LogMessage(si, sChildName)
						Dim cruce As String = $" A#EBT:E#{sChildName}:U4#Top:I#Top:U1#Top:U2#Top:U3#Top:O#BeforeAdj +"
						sOutput += cruce 
					Next
					sOutput = sOutput.TrimEnd("+"c)
					Return $"GETDATACELL({sOutput}):Name(INCOME BEFORE TAXES )"
				
				
				Else If args.FunctionName.XFEqualsIgnoreCase("SumOfPackage2")
					
					Dim sOutput As String = String.Empty
					Dim sEntity As String = args.NameValuePairs("Entity")
					
					Dim sChildrenList As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, brapi.Finance.Dim.GetDimPk(si, "Entities"), $"E#{sEntity}.Children",True)

					For Each Entity_Children In sChildrenList
						Dim sChildName = Entity_Children.Member.Name
						'brapi.ErrorLog.LogMessage(si, sChildName)
						Dim cruce As String = $" A#411:E#{sChildName}:U4#Top:I#Top:U1#Top:U2#Top:U3#Top:O#BeforeAdj +"
						sOutput += cruce 
					Next
					sOutput = sOutput.TrimEnd("+"c)
					Return $"GETDATACELL({sOutput}):Name(Current and deferred taxes)"
					
				End If
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace