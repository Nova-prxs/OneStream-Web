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

Namespace OneStream.BusinessRule.Extender.PLT_ExecuteSQL_3
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
			Dim sQuery As String = String.Empty
			Dim shared_BiBlend As New OneStream.BusinessRule.Extender.UTI_SharedBiBlend.shared_BiBlend						
			Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries	
			
				#Region "Product Type"
'				sQuery = "
'				UPDATE XFC_PLT_MASTER_Product
'				SET type = CASE
'				    WHEN id = '100010003R' THEN 'MOTEUR'
'				    WHEN id = '100010248R' THEN 'MOTEUR'
'				    WHEN id = '100010718R' THEN 'MOTEUR'
'				    WHEN id = '100010899R' THEN 'MOTEUR'
'				    WHEN id = '100010920R' THEN 'MOTEUR'
'				    WHEN id = '100010941R' THEN 'MOTEUR'
'				    WHEN id = '100010976R' THEN 'MOTEUR'
'				    WHEN id = '100012067R' THEN 'MOTEUR'
'				    WHEN id = '100012619R' THEN 'MOTEUR'
'				    WHEN id = '100012886R' THEN 'MOTEUR'
'				    WHEN id = '100012926R' THEN 'MOTEUR'
'				    WHEN id = '100012964R' THEN 'MOTEUR'
'				    WHEN id = '100013321R' THEN 'MOTEUR'
'				    WHEN id = '100013728R' THEN 'MOTEUR'
'				    WHEN id = '100014079R' THEN 'MOTEUR'
'				    WHEN id = '100014767R' THEN 'MOTEUR'
'				    WHEN id = '100015108R' THEN 'MOTEUR'
'				    WHEN id = '100015230R' THEN 'MOTEUR'
'				    WHEN id = '100015331R' THEN 'MOTEUR'
'				    WHEN id = '100015839R' THEN 'MOTEUR'
'				    WHEN id = '100016418R' THEN 'MOTEUR'
'				    WHEN id = '100017354R' THEN 'MOTEUR'
'				    WHEN id = '100017434R' THEN 'MOTEUR'
'				    WHEN id = '100017722R' THEN 'MOTEUR'
'				    WHEN id = '100017724R' THEN 'MOTEUR'
'				    WHEN id = '100017756R' THEN 'MOTEUR'
'				    WHEN id = '100017940R' THEN 'MOTEUR'
'				    WHEN id = '100018004R' THEN 'MOTEUR'
'				    WHEN id = '100018452R' THEN 'MOTEUR'
'				    WHEN id = '100018721R' THEN 'MOTEUR'
'				    WHEN id = '100018726R' THEN 'MOTEUR'
'				    WHEN id = '100018867R' THEN 'MOTEUR'
'				    WHEN id = '100019144R' THEN 'MOTEUR'
'				    WHEN id = '100019603R' THEN 'MOTEUR'
'				    WHEN id = '111105173R' THEN 'AUTRES'
'				    WHEN id = '111109158R' THEN 'AUTRES'
'				    WHEN id = '122820799R' THEN 'AUTRES'
'				    WHEN id = '124106897R' THEN 'AUTRES'
'				    WHEN id = '124108205R' THEN 'AUTRES'
'				    WHEN id = '132648303R' THEN 'AUTRES'
'				    WHEN id = '135023545R' THEN 'AUTRES'
'				    WHEN id = '150100347R' THEN 'AUTRES'
'				    WHEN id = '150100565R' THEN 'AUTRES'
'				    WHEN id = '150102617R' THEN 'AUTRES'
'				    WHEN id = '231C19341R' THEN '#'
'				    WHEN id = '290A04PM0E' THEN 'AUTRES'
'				    WHEN id = '290H29130R' THEN '#'
'				    WHEN id = '291166DB45' THEN 'AUTRES'
'				    WHEN id = '291167DB45' THEN 'AUTRES'
'				    WHEN id = '291A13414R' THEN '#'
'				    WHEN id = '291A16645R' THEN '#'
'				    WHEN id = '304011184R' THEN 'AUTRES'
'				    WHEN id = '320100313R' THEN 'BOITE'
'				    WHEN id = '320100534R' THEN 'BOITE'
'				    WHEN id = '320100724R' THEN 'BOITE'
'				    WHEN id = '320100777R' THEN 'BOITE'
'				    WHEN id = '320100993R' THEN 'BOITE'
'				    WHEN id = '320101889R' THEN 'BOITE'
'				    WHEN id = '320101994R' THEN 'BOITE'
'				    WHEN id = '320102278R' THEN 'BOITE'
'				    WHEN id = '320102693R' THEN 'BOITE'
'				    WHEN id = '320102703R' THEN 'BOITE'
'				    WHEN id = '320102944R' THEN 'BOITE'
'				    WHEN id = '320103122R' THEN 'BOITE'
'				    WHEN id = '320103557R' THEN 'BOITE'
'				    WHEN id = '320104034R' THEN 'BOITE'
'				    WHEN id = '320104472R' THEN 'BOITE'
'				    WHEN id = '320104597R' THEN 'BOITE'
'				    WHEN id = '320104615R' THEN 'BOITE'
'				    WHEN id = '320104843R' THEN 'BOITE'
'				    WHEN id = '320104860R' THEN 'BOITE'
'				    WHEN id = '320105444R' THEN 'BOITE'
'				    WHEN id = '320105452R' THEN 'BOITE'
'				    WHEN id = '320105520R' THEN 'BOITE'
'				    WHEN id = '320106168R' THEN 'BOITE'
'				    WHEN id = '320106170R' THEN 'BOITE'
'				    WHEN id = '320106554R' THEN 'BOITE'
'				    WHEN id = '320106838R' THEN 'BOITE'
'				    WHEN id = '320106969R' THEN 'BOITE'
'				    WHEN id = '320106980R' THEN 'BOITE'
'				    WHEN id = '320107114R' THEN 'BOITE'
'				    WHEN id = '320107376R' THEN 'BOITE'
'				    WHEN id = '320107504R' THEN 'BOITE'
'				    WHEN id = '320107606R' THEN 'BOITE'
'				    WHEN id = '320108230R' THEN 'BOITE'
'				    WHEN id = '320108263R' THEN 'BOITE'
'				    WHEN id = '320108381R' THEN 'BOITE'
'				    WHEN id = '320108664R' THEN 'BOITE'
'				    WHEN id = '320108786R' THEN 'BOITE'
'				    WHEN id = '320109354R' THEN 'BOITE'
'				    WHEN id = '320109588R' THEN 'BOITE'
'				    WHEN id = '321011553R' THEN '#'
'				    WHEN id = '321018540R' THEN 'AUTRES'
'				    WHEN id = '331001830R' THEN 'ORGANE'
'				    WHEN id = '331007830R' THEN 'ORGANE'
'				    WHEN id = '383000649R' THEN 'ORGANE'
'				    WHEN id = '383007672R' THEN 'ORGANE'
'				    WHEN id = '384213925R' THEN 'AUTRES'
'				    WHEN id = '384214352R' THEN 'AUTRES'
'				    WHEN id = '384219005R' THEN 'AUTRES'
'				    WHEN id = '7701717807' THEN 'AUTRES'
'				    WHEN id = '8200444619' THEN 'AUTRES'
'				    WHEN id = '8201345025' THEN 'MOTEUR'
'				    WHEN id = '8201589950' THEN 'MOTEUR'
'				    WHEN id = '8201602778' THEN 'MOTEUR'
'				    WHEN id = '8201660055' THEN 'AUTRES'
'				    WHEN id = '8201710314' THEN 'MOTEUR'
'				    WHEN id = '8201720530' THEN 'MOTEUR'
'				    WHEN id = '8201729176' THEN '#'
'				    WHEN id = '8201729852' THEN 'MOTEUR'
'				    WHEN id = '8201730203' THEN 'MOTEUR'
'				    WHEN id = '8201752491' THEN 'AUTRES'
'				    WHEN id = '8201752501' THEN 'AUTRES'
'				    WHEN id = 'B323705660' THEN '#'
'				    WHEN id = 'B323710676' THEN '#'
'				    WHEN id = 'B326062475' THEN '#'
'				    WHEN id = 'BR10HS' THEN 'MOTEUR'
'				    WHEN id = 'BR10LS' THEN 'MOTEUR'
'				    WHEN id = 'D323710676' THEN '#'
'				    WHEN id = 'H4DI' THEN 'MOTEUR'
'				    WHEN id = 'H5DI' THEN 'MOTEUR'
'				    WHEN id = 'H5HI' THEN 'MOTEUR'
'				    WHEN id = 'HR10' THEN 'MOTEUR'
'				    WHEN id = 'HR12XDV' THEN 'MOTEUR'
'				    WHEN id = 'HR13' THEN 'MOTEUR'
'				    WHEN id = 'HR16' THEN 'MOTEUR'
'				    WHEN id = 'HR18DDH' THEN 'MOTEUR'
'				    WHEN id = 'HR18XDV' THEN 'MOTEUR'
'				    WHEN id = 'K9K836' THEN 'MOTEUR'
'				    WHEN id = 'K9KGF GEN8' THEN 'MOTEUR'
'				    WHEN id = 'K9KGFGEN56' THEN 'MOTEUR'
'				    WHEN id = 'T323705660' THEN '#'
'				    WHEN id = 'T323710676' THEN '#'
'				    WHEN id = 'T326062475' THEN '#'
'				    WHEN id = 'TONNE_ALU' THEN 'AUTRES'
'				    WHEN id = 'TONNEALU' THEN 'AUTRES'
'				    WHEN id = 'TONNES' THEN 'AUTRES'
'				    WHEN id = '320105415R' THEN 'BOITE'
'				    WHEN id = '320106758R' THEN 'BOITE'
'				    WHEN id = '100010270R' THEN 'MOTEUR'
'				    WHEN id = '100014595R' THEN 'MOTEUR'
'				    WHEN id = '8201733029' THEN 'MOTEUR'
'				    WHEN id = '8201733030' THEN 'MOTEUR'
'				    WHEN id = '8201757508' THEN 'MOTEUR'
'				    WHEN id = '320100456R' THEN 'BOITE'
'				    WHEN id = '320104414R' THEN 'BOITE'
'				    WHEN id = '100015133R' THEN 'MOTEUR'
'				    WHEN id = '100019660R' THEN 'MOTEUR'
'				    WHEN id = '8201599803' THEN 'MOTEUR'
'				    WHEN id = '8201729829' THEN 'MOTEUR'
'				    WHEN id = '320101210R' THEN 'BOITE'
'				    WHEN id = '100010008R' THEN 'MOTEUR'
'				    ELSE type
'				END
'				WHERE id IN (
'				    '100010003R', '100010248R', '100010718R', '100010899R', '100010920R', 
'				    '100010941R', '100010976R', '100012067R', '100012619R', '100012886R',
'				    '100012926R', '100012964R', '100013321R', '100013728R', '100014079R',
'				    '100014767R', '100015108R', '100015230R', '100015331R', '100015839R',
'				    '100016418R', '100017354R', '100017434R', '100017722R', '100017724R',
'				    '100017756R', '100017940R', '100018004R', '100018452R', '100018721R',
'				    '100018726R', '100018867R', '100019144R', '100019603R', '111105173R',
'				    '111109158R', '122820799R', '124106897R', '124108205R', '132648303R',
'				    '135023545R', '150100347R', '150100565R', '150102617R', '231C19341R',
'				    '290A04PM0E', '290H29130R', '291166DB45', '291167DB45', '291A13414R',
'				    '291A16645R', '304011184R', '320100313R', '320100534R', '320100724R',
'				    '320100777R', '320100993R', '320101889R', '320101994R', '320102278R',
'				    '320102693R', '320102703R', '320102944R', '320103122R', '320103557R',
'				    '320104034R', '320104472R', '320104597R', '320104615R', '320104843R',
'				    '320104860R', '320105444R', '320105452R', '320105520R', '320106168R',
'				    '320106170R', '320106554R', '320106838R', '320106969R', '320106980R',
'				    '320107114R', '320107376R', '320107504R', '320107606R', '320108230R',
'				    '320108263R', '320108381R', '320108664R', '320108786R', '320109354R',
'				    '320109588R', '321011553R', '321018540R', '331001830R', '331007830R',
'				    '383000649R', '383007672R', '384213925R', '384214352R', '384219005R',
'				    '7701717807', '8200444619', '8201345025', '8201589950', '8201602778',
'				    '8201660055', '8201710314', '8201720530', '8201729176', '8201729852',
'				    '8201730203', '8201752491', '8201752501', 'B323705660', 'B323710676',
'				    'B326062475', 'BR10HS', 'BR10LS', 'D323710676', 'H4DI', 'H5DI', 'H5HI',
'				    'HR10', 'HR12XDV', 'HR13', 'HR16', 'HR18DDH', 'HR18XDV', 'K9K836',
'				    'K9KGF GEN8', 'K9KGFGEN56', 'T323705660', 'T323710676', 'T326062475',
'				    'TONNE_ALU', 'TONNEALU', 'TONNES', '320105415R', '320106758R', '100010270R',
'				    '100014595R', '8201733029', '8201733030', '8201757508', '320100456R',
'				    '320104414R', '100015133R', '100019660R', '8201599803', '8201729829',
'				    '320101210R', '100010008R'
'				);
'				"
				#End Region
				
				#Region "Energy Variance"
'				sQuery = "
'				INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-2-01', 'E', 'Price', 'Budget_V4', 784.22); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-2-01', 'E', 'Consumption', 'Budget_V4', 9203.44);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-3-01', 'E', 'Price', 'Budget_V4', 751.68); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-3-01', 'E', 'Consumption', 'Budget_V4', 9990.01);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-4-01', 'E', 'Price', 'Budget_V4', 746.85); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-4-01', 'E', 'Consumption', 'Budget_V4', 10805.23);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-5-01', 'E', 'Price', 'Budget_V4', 753.26); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-5-01', 'E', 'Consumption', 'Budget_V4', 10342.37);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-6-01', 'E', 'Price', 'Budget_V4', 850.15); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-6-01', 'E', 'Consumption', 'Budget_V4', 12320.21);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-7-01', 'E', 'Price', 'Budget_V4', 850.54); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-7-01', 'E', 'Consumption', 'Budget_V4', 11609.92);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-8-01', 'E', 'Price', 'Budget_V4', 827.24); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-8-01', 'E', 'Consumption', 'Budget_V4', 6418.97);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-9-01', 'E', 'Price', 'Budget_V4', 857.97); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-9-01', 'E', 'Consumption', 'Budget_V4', 11087.27);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-10-01', 'E', 'Price', 'Budget_V4', 850.38); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-10-01', 'E', 'Consumption', 'Budget_V4', 12191.78);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-11-01', 'E', 'Price', 'Budget_V4', 861.86); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-11-01', 'E', 'Consumption', 'Budget_V4', 9329.07);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-12-01', 'E', 'Price', 'Budget_V4', 849.18); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-12-01', 'E', 'Consumption', 'Budget_V4', 8273.78);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-1-01', 'G', 'Price', 'Budget_V4', 305.92); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-1-01', 'G', 'Consumption', 'Budget_V4', 11086.57);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-2-01', 'G', 'Price', 'Budget_V4', 95.99); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-2-01', 'G', 'Consumption', 'Budget_V4', 6288.41);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-3-01', 'G', 'Price', 'Budget_V4', 225.48); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-3-01', 'G', 'Consumption', 'Budget_V4', 6788.59);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-4-01', 'G', 'Price', 'Budget_V4', 198.65); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-4-01', 'G', 'Consumption', 'Budget_V4', 4422.48);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-5-01', 'G', 'Price', 'Budget_V4', 194.39); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-5-01', 'G', 'Consumption', 'Budget_V4', 3883.09);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-6-01', 'G', 'Price', 'Budget_V4', 193.67); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-6-01', 'G', 'Consumption', 'Budget_V4', 3653.23);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-7-01', 'G', 'Price', 'Budget_V4', 194.08); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-7-01', 'G', 'Consumption', 'Budget_V4', 3994.4);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-8-01', 'G', 'Price', 'Budget_V4', 191.71); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-8-01', 'G', 'Consumption', 'Budget_V4', 1734.27);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-9-01', 'G', 'Price', 'Budget_V4', 194.46); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-9-01', 'G', 'Consumption', 'Budget_V4', 4584.46);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-10-01', 'G', 'Price', 'Budget_V4', 228.18); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-10-01', 'G', 'Consumption', 'Budget_V4', 7081.49);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-11-01', 'G', 'Price', 'Budget_V4', 239.47); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-11-01', 'G', 'Consumption', 'Budget_V4', 7118.73);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-12-01', 'G', 'Price', 'Budget_V4', 251.17); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0045106', '2025-12-01', 'G', 'Consumption', 'Budget_V4', 8704.09);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-1-01', 'E', 'Price', 'Budget_V4', 281.09); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-1-01', 'E', 'Consumption', 'Budget_V4', 2577.96);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-2-01', 'E', 'Price', 'Budget_V4', 283.82); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-2-01', 'E', 'Consumption', 'Budget_V4', 3029.83);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-3-01', 'E', 'Price', 'Budget_V4', 283.82); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-3-01', 'E', 'Consumption', 'Budget_V4', 2898.9);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-4-01', 'E', 'Price', 'Budget_V4', 283.82); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-4-01', 'E', 'Consumption', 'Budget_V4', 3102.8);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-5-01', 'E', 'Price', 'Budget_V4', 283.82); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-5-01', 'E', 'Consumption', 'Budget_V4', 3081.92);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-6-01', 'E', 'Price', 'Budget_V4', 283.82); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-6-01', 'E', 'Consumption', 'Budget_V4', 3019.19);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-7-01', 'E', 'Price', 'Budget_V4', 286.67); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-7-01', 'E', 'Consumption', 'Budget_V4', 2875.19);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-8-01', 'E', 'Price', 'Budget_V4', 298.02); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-8-01', 'E', 'Consumption', 'Budget_V4', 3249.14);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-9-01', 'E', 'Price', 'Budget_V4', 298.02); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-9-01', 'E', 'Consumption', 'Budget_V4', 3296.26);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-10-01', 'E', 'Price', 'Budget_V4', 298.02); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-10-01', 'E', 'Consumption', 'Budget_V4', 3424.26);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-11-01', 'E', 'Price', 'Budget_V4', 298.02); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-11-01', 'E', 'Consumption', 'Budget_V4', 2938.12);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-12-01', 'E', 'Price', 'Budget_V4', 298.02); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-12-01', 'E', 'Consumption', 'Budget_V4', 2613.62);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-1-01', 'G', 'Price', 'Budget_V4', 300.6); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-1-01', 'G', 'Consumption', 'Budget_V4', 1029.43);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-2-01', 'G', 'Price', 'Budget_V4', 321.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-2-01', 'G', 'Consumption', 'Budget_V4', 1449.62);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-3-01', 'G', 'Price', 'Budget_V4', 321.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-3-01', 'G', 'Consumption', 'Budget_V4', 1546.08);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-4-01', 'G', 'Price', 'Budget_V4', 321.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-4-01', 'G', 'Consumption', 'Budget_V4', 1406.12);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-5-01', 'G', 'Price', 'Budget_V4', 321.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-5-01', 'G', 'Consumption', 'Budget_V4', 1705.59);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-6-01', 'G', 'Price', 'Budget_V4', 321.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-6-01', 'G', 'Consumption', 'Budget_V4', 1524.23);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-7-01', 'G', 'Price', 'Budget_V4', 321.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-7-01', 'G', 'Consumption', 'Budget_V4', 1317.28);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-8-01', 'G', 'Price', 'Budget_V4', 321.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-8-01', 'G', 'Consumption', 'Budget_V4', 1560.25);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-9-01', 'G', 'Price', 'Budget_V4', 321.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-9-01', 'G', 'Consumption', 'Budget_V4', 1403.75);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-10-01', 'G', 'Price', 'Budget_V4', 321.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-10-01', 'G', 'Consumption', 'Budget_V4', 1386.48);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-11-01', 'G', 'Price', 'Budget_V4', 321.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-11-01', 'G', 'Consumption', 'Budget_V4', 1169.81);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-12-01', 'G', 'Price', 'Budget_V4', 321.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0483003', '2025-12-01', 'G', 'Consumption', 'Budget_V4', 1044.23);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-1-01', 'E', 'Price', 'Budget_V4', 3138.16); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-1-01', 'E', 'Consumption', 'Budget_V4', 3496.22);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-2-01', 'E', 'Price', 'Budget_V4', 3778); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-2-01', 'E', 'Consumption', 'Budget_V4', 3577.64);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-3-01', 'E', 'Price', 'Budget_V4', 3515); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-3-01', 'E', 'Consumption', 'Budget_V4', 3659.45);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-4-01', 'E', 'Price', 'Budget_V4', 3714); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-4-01', 'E', 'Consumption', 'Budget_V4', 3787.57);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-5-01', 'E', 'Price', 'Budget_V4', 3760); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-5-01', 'E', 'Consumption', 'Budget_V4', 3841.14);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-6-01', 'E', 'Price', 'Budget_V4', 3881); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-6-01', 'E', 'Consumption', 'Budget_V4', 3620.16);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-7-01', 'E', 'Price', 'Budget_V4', 4312); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-7-01', 'E', 'Consumption', 'Budget_V4', 4017.61);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-8-01', 'E', 'Price', 'Budget_V4', 4473); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-8-01', 'E', 'Consumption', 'Budget_V4', 2030.45);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-9-01', 'E', 'Price', 'Budget_V4', 4216); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-9-01', 'E', 'Consumption', 'Budget_V4', 3670.76);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-10-01', 'E', 'Price', 'Budget_V4', 4386); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-10-01', 'E', 'Consumption', 'Budget_V4', 3538.65);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-11-01', 'E', 'Price', 'Budget_V4', 4348); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-11-01', 'E', 'Consumption', 'Budget_V4', 3131.83);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-12-01', 'E', 'Price', 'Budget_V4', 4433); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-12-01', 'E', 'Consumption', 'Budget_V4', 2394.14);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-1-01', 'G', 'Price', 'Budget_V4', 1106.52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-1-01', 'G', 'Consumption', 'Budget_V4', 1723.65);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-2-01', 'G', 'Price', 'Budget_V4', 2099); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-2-01', 'G', 'Consumption', 'Budget_V4', 1944.91);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-3-01', 'G', 'Price', 'Budget_V4', 2099); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-3-01', 'G', 'Consumption', 'Budget_V4', 1661.06);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-4-01', 'G', 'Price', 'Budget_V4', 2099); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-4-01', 'G', 'Consumption', 'Budget_V4', 1154.94);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-5-01', 'G', 'Price', 'Budget_V4', 2099); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-5-01', 'G', 'Consumption', 'Budget_V4', 1085.44);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-6-01', 'G', 'Price', 'Budget_V4', 2099); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-6-01', 'G', 'Consumption', 'Budget_V4', 888.53);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-7-01', 'G', 'Price', 'Budget_V4', 2099); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-7-01', 'G', 'Consumption', 'Budget_V4', 964.72);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-8-01', 'G', 'Price', 'Budget_V4', 2099); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-8-01', 'G', 'Consumption', 'Budget_V4', 491.72);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-9-01', 'G', 'Price', 'Budget_V4', 2414); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-9-01', 'G', 'Consumption', 'Budget_V4', 724.68);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-10-01', 'G', 'Price', 'Budget_V4', 2414); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-10-01', 'G', 'Consumption', 'Budget_V4', 824.77);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-11-01', 'G', 'Price', 'Budget_V4', 2414); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-11-01', 'G', 'Consumption', 'Budget_V4', 1233.01);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-12-01', 'G', 'Price', 'Budget_V4', 2414); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0529002', '2025-12-01', 'G', 'Consumption', 'Budget_V4', 1267.28);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-1-01', 'E', 'Price', 'Budget_V4', 96.83); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-1-01', 'E', 'Consumption', 'Budget_V4', 4450.44);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-2-01', 'E', 'Price', 'Budget_V4', 97); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-2-01', 'E', 'Consumption', 'Budget_V4', 4294.98);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-3-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-3-01', 'E', 'Consumption', 'Budget_V4', 4375.38);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-4-01', 'E', 'Price', 'Budget_V4', 82); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-4-01', 'E', 'Consumption', 'Budget_V4', 3970.3);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-5-01', 'E', 'Price', 'Budget_V4', 82); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-5-01', 'E', 'Consumption', 'Budget_V4', 4554.11);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-6-01', 'E', 'Price', 'Budget_V4', 85); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-6-01', 'E', 'Consumption', 'Budget_V4', 6076.2);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-7-01', 'E', 'Price', 'Budget_V4', 97); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-7-01', 'E', 'Consumption', 'Budget_V4', 6407.28);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-8-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-8-01', 'E', 'Consumption', 'Budget_V4', 2695.99);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-9-01', 'E', 'Price', 'Budget_V4', 85); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-9-01', 'E', 'Consumption', 'Budget_V4', 4203.67);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-10-01', 'E', 'Price', 'Budget_V4', 82); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-10-01', 'E', 'Consumption', 'Budget_V4', 3230.62);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-11-01', 'E', 'Price', 'Budget_V4', 85); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-11-01', 'E', 'Consumption', 'Budget_V4', 3724.91);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-12-01', 'E', 'Price', 'Budget_V4', 93); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-12-01', 'E', 'Consumption', 'Budget_V4', 2957.81);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-1-01', 'G', 'Price', 'Budget_V4', 31.36); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-1-01', 'G', 'Consumption', 'Budget_V4', 3068.5);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-2-01', 'G', 'Price', 'Budget_V4', 50); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-2-01', 'G', 'Consumption', 'Budget_V4', 2184.11);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-3-01', 'G', 'Price', 'Budget_V4', 50); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-3-01', 'G', 'Consumption', 'Budget_V4', 1814.4);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-4-01', 'G', 'Price', 'Budget_V4', 50); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-4-01', 'G', 'Consumption', 'Budget_V4', 1145.78);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-5-01', 'G', 'Price', 'Budget_V4', 50); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-5-01', 'G', 'Consumption', 'Budget_V4', 1028.82);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-6-01', 'G', 'Price', 'Budget_V4', 50); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-6-01', 'G', 'Consumption', 'Budget_V4', 1111.03);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-7-01', 'G', 'Price', 'Budget_V4', 50); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-7-01', 'G', 'Consumption', 'Budget_V4', 998.05);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-8-01', 'G', 'Price', 'Budget_V4', 50); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-8-01', 'G', 'Consumption', 'Budget_V4', 655.81);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-9-01', 'G', 'Price', 'Budget_V4', 50); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-9-01', 'G', 'Consumption', 'Budget_V4', 1157.82);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-10-01', 'G', 'Price', 'Budget_V4', 50); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-10-01', 'G', 'Consumption', 'Budget_V4', 1163.33);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-11-01', 'G', 'Price', 'Budget_V4', 50); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-11-01', 'G', 'Consumption', 'Budget_V4', 1688.54);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-12-01', 'G', 'Price', 'Budget_V4', 50); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548913', '2025-12-01', 'G', 'Consumption', 'Budget_V4', 2609.6);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-1-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-1-01', 'E', 'Consumption', 'Budget_V4', 10511.58);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-2-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-2-01', 'E', 'Consumption', 'Budget_V4', 10654);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-3-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-3-01', 'E', 'Consumption', 'Budget_V4', 11341.57);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-4-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-4-01', 'E', 'Consumption', 'Budget_V4', 10697.69);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-5-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-5-01', 'E', 'Consumption', 'Budget_V4', 10781.18);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-6-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-6-01', 'E', 'Consumption', 'Budget_V4', 12002.92);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-7-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-7-01', 'E', 'Consumption', 'Budget_V4', 12803.24);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-8-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-8-01', 'E', 'Consumption', 'Budget_V4', 6858.6);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-9-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-9-01', 'E', 'Consumption', 'Budget_V4', 13703.27);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-10-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-10-01', 'E', 'Consumption', 'Budget_V4', 15115.8);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-11-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-11-01', 'E', 'Consumption', 'Budget_V4', 12269.42);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-12-01', 'E', 'Price', 'Budget_V4', 90); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-12-01', 'E', 'Consumption', 'Budget_V4', 8321.26);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-1-01', 'G', 'Price', 'Budget_V4', 52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-1-01', 'G', 'Consumption', 'Budget_V4', 8597.88);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-2-01', 'G', 'Price', 'Budget_V4', 52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-2-01', 'G', 'Consumption', 'Budget_V4', 7472.02);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-3-01', 'G', 'Price', 'Budget_V4', 52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-3-01', 'G', 'Consumption', 'Budget_V4', 5475.53);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-4-01', 'G', 'Price', 'Budget_V4', 52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-4-01', 'G', 'Consumption', 'Budget_V4', 3980.89);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-5-01', 'G', 'Price', 'Budget_V4', 52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-5-01', 'G', 'Consumption', 'Budget_V4', 3294.82);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-6-01', 'G', 'Price', 'Budget_V4', 52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-6-01', 'G', 'Consumption', 'Budget_V4', 3271.81);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-7-01', 'G', 'Price', 'Budget_V4', 52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-7-01', 'G', 'Consumption', 'Budget_V4', 3326.64);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-8-01', 'G', 'Price', 'Budget_V4', 52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-8-01', 'G', 'Consumption', 'Budget_V4', 1816.87);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-9-01', 'G', 'Price', 'Budget_V4', 52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-9-01', 'G', 'Consumption', 'Budget_V4', 3930.08);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-10-01', 'G', 'Price', 'Budget_V4', 52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-10-01', 'G', 'Consumption', 'Budget_V4', 4558.4);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-11-01', 'G', 'Price', 'Budget_V4', 52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-11-01', 'G', 'Consumption', 'Budget_V4', 7434.74);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-12-01', 'G', 'Price', 'Budget_V4', 52); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0548914', '2025-12-01', 'G', 'Consumption', 'Budget_V4', 6807.42);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-1-01', 'E', 'Price', 'Budget_V4', 214800.71); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-1-01', 'E', 'Consumption', 'Budget_V4', 146.11);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-2-01', 'E', 'Price', 'Budget_V4', 152314.27); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-2-01', 'E', 'Consumption', 'Budget_V4', 312.51);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-3-01', 'E', 'Price', 'Budget_V4', 158582.22); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-3-01', 'E', 'Consumption', 'Budget_V4', 308.54);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-4-01', 'E', 'Price', 'Budget_V4', 160174.23); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-4-01', 'E', 'Consumption', 'Budget_V4', 326.44);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-5-01', 'E', 'Price', 'Budget_V4', 166236.4); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-5-01', 'E', 'Consumption', 'Budget_V4', 325.16);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-6-01', 'E', 'Price', 'Budget_V4', 164320.03); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-6-01', 'E', 'Consumption', 'Budget_V4', 367.69);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-7-01', 'E', 'Price', 'Budget_V4', 170015.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-7-01', 'E', 'Consumption', 'Budget_V4', 367.47);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-8-01', 'E', 'Price', 'Budget_V4', 173205.11); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-8-01', 'E', 'Consumption', 'Budget_V4', 382.26);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-9-01', 'E', 'Price', 'Budget_V4', 197675.79); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-9-01', 'E', 'Consumption', 'Budget_V4', 294.39);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-10-01', 'E', 'Price', 'Budget_V4', 182242.95); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-10-01', 'E', 'Consumption', 'Budget_V4', 394.39);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-11-01', 'E', 'Price', 'Budget_V4', 199048.58); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-11-01', 'E', 'Consumption', 'Budget_V4', 335.99);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-12-01', 'E', 'Price', 'Budget_V4', 273561.77); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-12-01', 'E', 'Consumption', 'Budget_V4', 179.95);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-1-01', 'G', 'Price', 'Budget_V4', 23989.79); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-1-01', 'G', 'Consumption', 'Budget_V4', 360.13);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-2-01', 'G', 'Price', 'Budget_V4', 25855.49); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-2-01', 'G', 'Consumption', 'Budget_V4', 801.3);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-3-01', 'G', 'Price', 'Budget_V4', 26677.15); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-3-01', 'G', 'Consumption', 'Budget_V4', 784.15);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-4-01', 'G', 'Price', 'Budget_V4', 32074.4); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-4-01', 'G', 'Consumption', 'Budget_V4', 835.47);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-5-01', 'G', 'Price', 'Budget_V4', 32568.5); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-5-01', 'G', 'Consumption', 'Budget_V4', 829.85);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-6-01', 'G', 'Price', 'Budget_V4', 33504.17); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-6-01', 'G', 'Consumption', 'Budget_V4', 948.89);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-7-01', 'G', 'Price', 'Budget_V4', 34527.69); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-7-01', 'G', 'Consumption', 'Budget_V4', 946.23);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-8-01', 'G', 'Price', 'Budget_V4', 35529.96); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-8-01', 'G', 'Consumption', 'Budget_V4', 986.89);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-9-01', 'G', 'Price', 'Budget_V4', 36732.15); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-9-01', 'G', 'Consumption', 'Budget_V4', 747.32);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-10-01', 'G', 'Price', 'Budget_V4', 37558.49); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-10-01', 'G', 'Consumption', 'Budget_V4', 1020.25);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-11-01', 'G', 'Price', 'Budget_V4', 38702.55); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-11-01', 'G', 'Consumption', 'Budget_V4', 861.71);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-12-01', 'G', 'Price', 'Budget_V4', 40498.86); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0592', '2025-12-01', 'G', 'Consumption', 'Budget_V4', 430.52);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-1-01', 'E', 'Price', 'Budget_V4', 180089); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-1-01', 'E', 'Consumption', 'Budget_V4', 744.83);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-2-01', 'E', 'Price', 'Budget_V4', 134641); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-2-01', 'E', 'Consumption', 'Budget_V4', 1286.38);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-3-01', 'E', 'Price', 'Budget_V4', 134641); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-3-01', 'E', 'Consumption', 'Budget_V4', 1285.81);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-4-01', 'E', 'Price', 'Budget_V4', 134641); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-4-01', 'E', 'Consumption', 'Budget_V4', 1446.95);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-5-01', 'E', 'Price', 'Budget_V4', 134641); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-5-01', 'E', 'Consumption', 'Budget_V4', 1205.32);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-6-01', 'E', 'Price', 'Budget_V4', 134641); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-6-01', 'E', 'Consumption', 'Budget_V4', 1251.15);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-7-01', 'E', 'Price', 'Budget_V4', 134641); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-7-01', 'E', 'Consumption', 'Budget_V4', 1570.07);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-8-01', 'E', 'Price', 'Budget_V4', 134641); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-8-01', 'E', 'Consumption', 'Budget_V4', 1251.09);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-9-01', 'E', 'Price', 'Budget_V4', 140026); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-9-01', 'E', 'Consumption', 'Budget_V4', 1089.91);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-10-01', 'E', 'Price', 'Budget_V4', 140026); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-10-01', 'E', 'Consumption', 'Budget_V4', 1457.34);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-11-01', 'E', 'Price', 'Budget_V4', 140026); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-11-01', 'E', 'Consumption', 'Budget_V4', 1164.77);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-12-01', 'E', 'Price', 'Budget_V4', 140026); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-12-01', 'E', 'Consumption', 'Budget_V4', 776.35);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-1-01', 'G', 'Price', 'Budget_V4', 69585); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-1-01', 'G', 'Consumption', 'Budget_V4', 203.57);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-2-01', 'G', 'Price', 'Budget_V4', 51942); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-2-01', 'G', 'Consumption', 'Budget_V4', 516.27);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-3-01', 'G', 'Price', 'Budget_V4', 52086); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-3-01', 'G', 'Consumption', 'Budget_V4', 531.5);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-4-01', 'G', 'Price', 'Budget_V4', 52231); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-4-01', 'G', 'Consumption', 'Budget_V4', 537.45);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-5-01', 'G', 'Price', 'Budget_V4', 52377); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-5-01', 'G', 'Consumption', 'Budget_V4', 491.87);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-6-01', 'G', 'Price', 'Budget_V4', 52523); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-6-01', 'G', 'Consumption', 'Budget_V4', 541.09);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-7-01', 'G', 'Price', 'Budget_V4', 52669); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-7-01', 'G', 'Consumption', 'Budget_V4', 559.01);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-8-01', 'G', 'Price', 'Budget_V4', 52816); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-8-01', 'G', 'Consumption', 'Budget_V4', 514.05);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-9-01', 'G', 'Price', 'Budget_V4', 52963); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-9-01', 'G', 'Consumption', 'Budget_V4', 517.77);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-10-01', 'G', 'Price', 'Budget_V4', 53110); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-10-01', 'G', 'Consumption', 'Budget_V4', 517.56);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-11-01', 'G', 'Price', 'Budget_V4', 53258); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-11-01', 'G', 'Consumption', 'Budget_V4', 461.78);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-12-01', 'G', 'Price', 'Budget_V4', 53407); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0585', '2025-12-01', 'G', 'Consumption', 'Budget_V4', 211.2);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-1-01', 'E', 'Price', 'Budget_V4', 124.44); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-1-01', 'E', 'Consumption', 'Budget_V4', 5429.85);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-2-01', 'E', 'Price', 'Budget_V4', 123.3); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-2-01', 'E', 'Consumption', 'Budget_V4', 4728.79);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-3-01', 'E', 'Price', 'Budget_V4', 123.31); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-3-01', 'E', 'Consumption', 'Budget_V4', 5096.14);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-4-01', 'E', 'Price', 'Budget_V4', 123.33); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-4-01', 'E', 'Consumption', 'Budget_V4', 5384);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-5-01', 'E', 'Price', 'Budget_V4', 123.35); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-5-01', 'E', 'Consumption', 'Budget_V4', 5588.22);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-6-01', 'E', 'Price', 'Budget_V4', 123.34); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-6-01', 'E', 'Consumption', 'Budget_V4', 5383.6);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-7-01', 'E', 'Price', 'Budget_V4', 123.32); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-7-01', 'E', 'Consumption', 'Budget_V4', 5494.14);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-8-01', 'E', 'Price', 'Budget_V4', 123.44); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-8-01', 'E', 'Consumption', 'Budget_V4', 4910.78);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-9-01', 'E', 'Price', 'Budget_V4', 123.32); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-9-01', 'E', 'Consumption', 'Budget_V4', 5631.49);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-10-01', 'E', 'Price', 'Budget_V4', 123.34); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-10-01', 'E', 'Consumption', 'Budget_V4', 6128.33);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-11-01', 'E', 'Price', 'Budget_V4', 123.35); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-11-01', 'E', 'Consumption', 'Budget_V4', 5878.69);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-12-01', 'E', 'Price', 'Budget_V4', 123.4); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-12-01', 'E', 'Consumption', 'Budget_V4', 5534.56);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-1-01', 'G', 'Price', 'Budget_V4', 83.08); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-1-01', 'G', 'Consumption', 'Budget_V4', 1231);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-2-01', 'G', 'Price', 'Budget_V4', 103.88); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-2-01', 'G', 'Consumption', 'Budget_V4', 1063.5);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-3-01', 'G', 'Price', 'Budget_V4', 115.03); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-3-01', 'G', 'Consumption', 'Budget_V4', 859.41);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-4-01', 'G', 'Price', 'Budget_V4', 117.02); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-4-01', 'G', 'Consumption', 'Budget_V4', 795.63);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-5-01', 'G', 'Price', 'Budget_V4', 135.24); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-5-01', 'G', 'Consumption', 'Budget_V4', 607.84);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-6-01', 'G', 'Price', 'Budget_V4', 135.46); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-6-01', 'G', 'Consumption', 'Budget_V4', 551.02);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-7-01', 'G', 'Price', 'Budget_V4', 168.35); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-7-01', 'G', 'Consumption', 'Budget_V4', 491);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-8-01', 'G', 'Price', 'Budget_V4', 93.09); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-8-01', 'G', 'Consumption', 'Budget_V4', 345.28);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-9-01', 'G', 'Price', 'Budget_V4', 141.4); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-9-01', 'G', 'Consumption', 'Budget_V4', 621.89);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-10-01', 'G', 'Price', 'Budget_V4', 130.93); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-10-01', 'G', 'Consumption', 'Budget_V4', 749.96);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-11-01', 'G', 'Price', 'Budget_V4', 103); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-11-01', 'G', 'Consumption', 'Budget_V4', 1065.9);
'INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-12-01', 'G', 'Price', 'Budget_V4', 88.83); INSERT INTO XFC_PLT_AUX_EnergyVariance (id_factory, date, energy_type, indicator, scenario, value) VALUES ('R0671', '2025-12-01', 'G', 'Consumption', 'Budget_V4', 1014.51);
'				"
			#End Region
			
				#Region "ARG - Key Allocation BUD_V4"
'				sQuery = "
'				DELETE FROM XFC_PLT_AUX_AllocationKeys
'				WHERE 1=1
'					AND id_factory = 'R0592'
'					AND scenario = 'Budget_V4'
				
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-01-01', 'GM05011', 'HN05014', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-02-01', 'GM05011', 'HN05014', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-03-01', 'GM05011', 'HN05014', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-04-01', 'GM05011', 'HN05014', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-05-01', 'GM05011', 'HN05014', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-06-01', 'GM05011', 'HN05014', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-07-01', 'GM05011', 'HN05014', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-08-01', 'GM05011', 'HN05014', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-09-01', 'GM05011', 'HN05014', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-10-01', 'GM05011', 'HN05014', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-11-01', 'GM05011', 'HN05014', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-12-01', 'GM05011', 'HN05014', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-01-01', 'GM05011', 'HN05014', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-02-01', 'GM05011', 'HN05014', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-03-01', 'GM05011', 'HN05014', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-04-01', 'GM05011', 'HN05014', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-05-01', 'GM05011', 'HN05014', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-06-01', 'GM05011', 'HN05014', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-07-01', 'GM05011', 'HN05014', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-08-01', 'GM05011', 'HN05014', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-09-01', 'GM05011', 'HN05014', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-10-01', 'GM05011', 'HN05014', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-11-01', 'GM05011', 'HN05014', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-12-01', 'GM05011', 'HN05014', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-01-01', 'GM05011', 'HN05014', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-02-01', 'GM05011', 'HN05014', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-03-01', 'GM05011', 'HN05014', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-04-01', 'GM05011', 'HN05014', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-05-01', 'GM05011', 'HN05014', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-06-01', 'GM05011', 'HN05014', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-07-01', 'GM05011', 'HN05014', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-08-01', 'GM05011', 'HN05014', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-09-01', 'GM05011', 'HN05014', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-10-01', 'GM05011', 'HN05014', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-11-01', 'GM05011', 'HN05014', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-12-01', 'GM05011', 'HN05014', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-01-01', 'GM05011', 'HN05014', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-02-01', 'GM05011', 'HN05014', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-03-01', 'GM05011', 'HN05014', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-04-01', 'GM05011', 'HN05014', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-05-01', 'GM05011', 'HN05014', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-06-01', 'GM05011', 'HN05014', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-07-01', 'GM05011', 'HN05014', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-08-01', 'GM05011', 'HN05014', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-09-01', 'GM05011', 'HN05014', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-10-01', 'GM05011', 'HN05014', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-11-01', 'GM05011', 'HN05014', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-12-01', 'GM05011', 'HN05014', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-01-01', 'GM05011', 'HN05014', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-02-01', 'GM05011', 'HN05014', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-03-01', 'GM05011', 'HN05014', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-04-01', 'GM05011', 'HN05014', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-05-01', 'GM05011', 'HN05014', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-06-01', 'GM05011', 'HN05014', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-07-01', 'GM05011', 'HN05014', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-08-01', 'GM05011', 'HN05014', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-09-01', 'GM05011', 'HN05014', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-10-01', 'GM05011', 'HN05014', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-11-01', 'GM05011', 'HN05014', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-12-01', 'GM05011', 'HN05014', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-01-01', 'GM05012', 'HN05014', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-02-01', 'GM05012', 'HN05014', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-03-01', 'GM05012', 'HN05014', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-04-01', 'GM05012', 'HN05014', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-05-01', 'GM05012', 'HN05014', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-06-01', 'GM05012', 'HN05014', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-07-01', 'GM05012', 'HN05014', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-08-01', 'GM05012', 'HN05014', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-09-01', 'GM05012', 'HN05014', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-10-01', 'GM05012', 'HN05014', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-11-01', 'GM05012', 'HN05014', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-12-01', 'GM05012', 'HN05014', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-01-01', 'GM05012', 'HN05014', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-02-01', 'GM05012', 'HN05014', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-03-01', 'GM05012', 'HN05014', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-04-01', 'GM05012', 'HN05014', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-05-01', 'GM05012', 'HN05014', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-06-01', 'GM05012', 'HN05014', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-07-01', 'GM05012', 'HN05014', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-08-01', 'GM05012', 'HN05014', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-09-01', 'GM05012', 'HN05014', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-10-01', 'GM05012', 'HN05014', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-11-01', 'GM05012', 'HN05014', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-12-01', 'GM05012', 'HN05014', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-01-01', 'GM05012', 'HN05014', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-02-01', 'GM05012', 'HN05014', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-03-01', 'GM05012', 'HN05014', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-04-01', 'GM05012', 'HN05014', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-05-01', 'GM05012', 'HN05014', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-06-01', 'GM05012', 'HN05014', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-07-01', 'GM05012', 'HN05014', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-08-01', 'GM05012', 'HN05014', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-09-01', 'GM05012', 'HN05014', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-10-01', 'GM05012', 'HN05014', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-11-01', 'GM05012', 'HN05014', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-12-01', 'GM05012', 'HN05014', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-01-01', 'GM05012', 'HN05014', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-02-01', 'GM05012', 'HN05014', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-03-01', 'GM05012', 'HN05014', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-04-01', 'GM05012', 'HN05014', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-05-01', 'GM05012', 'HN05014', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-06-01', 'GM05012', 'HN05014', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-07-01', 'GM05012', 'HN05014', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-08-01', 'GM05012', 'HN05014', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-09-01', 'GM05012', 'HN05014', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-10-01', 'GM05012', 'HN05014', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-11-01', 'GM05012', 'HN05014', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-12-01', 'GM05012', 'HN05014', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-01-01', 'GM05012', 'HN05014', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-02-01', 'GM05012', 'HN05014', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-03-01', 'GM05012', 'HN05014', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-04-01', 'GM05012', 'HN05014', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-05-01', 'GM05012', 'HN05014', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-06-01', 'GM05012', 'HN05014', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-07-01', 'GM05012', 'HN05014', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-08-01', 'GM05012', 'HN05014', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-09-01', 'GM05012', 'HN05014', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-10-01', 'GM05012', 'HN05014', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-11-01', 'GM05012', 'HN05014', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-12-01', 'GM05012', 'HN05014', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-01-01', 'GM05013', 'HN05014', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-02-01', 'GM05013', 'HN05014', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-03-01', 'GM05013', 'HN05014', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-04-01', 'GM05013', 'HN05014', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-05-01', 'GM05013', 'HN05014', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-06-01', 'GM05013', 'HN05014', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-07-01', 'GM05013', 'HN05014', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-08-01', 'GM05013', 'HN05014', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-09-01', 'GM05013', 'HN05014', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-10-01', 'GM05013', 'HN05014', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-11-01', 'GM05013', 'HN05014', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-12-01', 'GM05013', 'HN05014', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-01-01', 'GM05013', 'HN05014', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-02-01', 'GM05013', 'HN05014', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-03-01', 'GM05013', 'HN05014', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-04-01', 'GM05013', 'HN05014', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-05-01', 'GM05013', 'HN05014', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-06-01', 'GM05013', 'HN05014', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-07-01', 'GM05013', 'HN05014', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-08-01', 'GM05013', 'HN05014', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-09-01', 'GM05013', 'HN05014', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-10-01', 'GM05013', 'HN05014', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-11-01', 'GM05013', 'HN05014', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-12-01', 'GM05013', 'HN05014', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-01-01', 'GM05013', 'HN05014', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-02-01', 'GM05013', 'HN05014', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-03-01', 'GM05013', 'HN05014', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-04-01', 'GM05013', 'HN05014', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-05-01', 'GM05013', 'HN05014', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-06-01', 'GM05013', 'HN05014', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-07-01', 'GM05013', 'HN05014', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-08-01', 'GM05013', 'HN05014', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-09-01', 'GM05013', 'HN05014', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-10-01', 'GM05013', 'HN05014', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-11-01', 'GM05013', 'HN05014', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-12-01', 'GM05013', 'HN05014', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-01-01', 'GM05013', 'HN05014', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-02-01', 'GM05013', 'HN05014', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-03-01', 'GM05013', 'HN05014', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-04-01', 'GM05013', 'HN05014', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-05-01', 'GM05013', 'HN05014', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-06-01', 'GM05013', 'HN05014', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-07-01', 'GM05013', 'HN05014', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-08-01', 'GM05013', 'HN05014', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-09-01', 'GM05013', 'HN05014', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-10-01', 'GM05013', 'HN05014', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-11-01', 'GM05013', 'HN05014', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-12-01', 'GM05013', 'HN05014', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-01-01', 'GM05013', 'HN05014', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-02-01', 'GM05013', 'HN05014', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-03-01', 'GM05013', 'HN05014', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-04-01', 'GM05013', 'HN05014', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-05-01', 'GM05013', 'HN05014', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-06-01', 'GM05013', 'HN05014', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-07-01', 'GM05013', 'HN05014', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-08-01', 'GM05013', 'HN05014', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-09-01', 'GM05013', 'HN05014', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-10-01', 'GM05013', 'HN05014', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-11-01', 'GM05013', 'HN05014', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-12-01', 'GM05013', 'HN05014', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-01-01', 'GM05011', 'HN05015', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-02-01', 'GM05011', 'HN05015', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-03-01', 'GM05011', 'HN05015', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-04-01', 'GM05011', 'HN05015', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-05-01', 'GM05011', 'HN05015', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-06-01', 'GM05011', 'HN05015', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-07-01', 'GM05011', 'HN05015', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-08-01', 'GM05011', 'HN05015', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-09-01', 'GM05011', 'HN05015', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-10-01', 'GM05011', 'HN05015', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-11-01', 'GM05011', 'HN05015', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-12-01', 'GM05011', 'HN05015', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-01-01', 'GM05011', 'HN05015', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-02-01', 'GM05011', 'HN05015', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-03-01', 'GM05011', 'HN05015', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-04-01', 'GM05011', 'HN05015', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-05-01', 'GM05011', 'HN05015', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-06-01', 'GM05011', 'HN05015', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-07-01', 'GM05011', 'HN05015', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-08-01', 'GM05011', 'HN05015', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-09-01', 'GM05011', 'HN05015', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-10-01', 'GM05011', 'HN05015', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-11-01', 'GM05011', 'HN05015', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-12-01', 'GM05011', 'HN05015', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-01-01', 'GM05011', 'HN05015', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-02-01', 'GM05011', 'HN05015', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-03-01', 'GM05011', 'HN05015', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-04-01', 'GM05011', 'HN05015', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-05-01', 'GM05011', 'HN05015', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-06-01', 'GM05011', 'HN05015', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-07-01', 'GM05011', 'HN05015', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-08-01', 'GM05011', 'HN05015', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-09-01', 'GM05011', 'HN05015', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-10-01', 'GM05011', 'HN05015', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-11-01', 'GM05011', 'HN05015', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-12-01', 'GM05011', 'HN05015', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-01-01', 'GM05011', 'HN05015', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-02-01', 'GM05011', 'HN05015', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-03-01', 'GM05011', 'HN05015', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-04-01', 'GM05011', 'HN05015', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-05-01', 'GM05011', 'HN05015', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-06-01', 'GM05011', 'HN05015', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-07-01', 'GM05011', 'HN05015', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-08-01', 'GM05011', 'HN05015', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-09-01', 'GM05011', 'HN05015', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-10-01', 'GM05011', 'HN05015', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-11-01', 'GM05011', 'HN05015', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-12-01', 'GM05011', 'HN05015', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-01-01', 'GM05011', 'HN05015', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-02-01', 'GM05011', 'HN05015', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-03-01', 'GM05011', 'HN05015', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-04-01', 'GM05011', 'HN05015', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-05-01', 'GM05011', 'HN05015', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-06-01', 'GM05011', 'HN05015', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-07-01', 'GM05011', 'HN05015', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-08-01', 'GM05011', 'HN05015', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-09-01', 'GM05011', 'HN05015', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-10-01', 'GM05011', 'HN05015', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-11-01', 'GM05011', 'HN05015', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-12-01', 'GM05011', 'HN05015', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-01-01', 'GM05012', 'HN05015', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-02-01', 'GM05012', 'HN05015', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-03-01', 'GM05012', 'HN05015', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-04-01', 'GM05012', 'HN05015', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-05-01', 'GM05012', 'HN05015', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-06-01', 'GM05012', 'HN05015', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-07-01', 'GM05012', 'HN05015', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-08-01', 'GM05012', 'HN05015', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-09-01', 'GM05012', 'HN05015', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-10-01', 'GM05012', 'HN05015', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-11-01', 'GM05012', 'HN05015', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-12-01', 'GM05012', 'HN05015', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-01-01', 'GM05012', 'HN05015', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-02-01', 'GM05012', 'HN05015', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-03-01', 'GM05012', 'HN05015', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-04-01', 'GM05012', 'HN05015', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-05-01', 'GM05012', 'HN05015', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-06-01', 'GM05012', 'HN05015', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-07-01', 'GM05012', 'HN05015', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-08-01', 'GM05012', 'HN05015', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-09-01', 'GM05012', 'HN05015', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-10-01', 'GM05012', 'HN05015', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-11-01', 'GM05012', 'HN05015', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-12-01', 'GM05012', 'HN05015', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-01-01', 'GM05012', 'HN05015', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-02-01', 'GM05012', 'HN05015', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-03-01', 'GM05012', 'HN05015', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-04-01', 'GM05012', 'HN05015', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-05-01', 'GM05012', 'HN05015', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-06-01', 'GM05012', 'HN05015', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-07-01', 'GM05012', 'HN05015', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-08-01', 'GM05012', 'HN05015', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-09-01', 'GM05012', 'HN05015', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-10-01', 'GM05012', 'HN05015', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-11-01', 'GM05012', 'HN05015', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-12-01', 'GM05012', 'HN05015', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-01-01', 'GM05012', 'HN05015', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-02-01', 'GM05012', 'HN05015', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-03-01', 'GM05012', 'HN05015', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-04-01', 'GM05012', 'HN05015', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-05-01', 'GM05012', 'HN05015', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-06-01', 'GM05012', 'HN05015', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-07-01', 'GM05012', 'HN05015', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-08-01', 'GM05012', 'HN05015', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-09-01', 'GM05012', 'HN05015', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-10-01', 'GM05012', 'HN05015', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-11-01', 'GM05012', 'HN05015', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-12-01', 'GM05012', 'HN05015', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-01-01', 'GM05012', 'HN05015', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-02-01', 'GM05012', 'HN05015', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-03-01', 'GM05012', 'HN05015', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-04-01', 'GM05012', 'HN05015', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-05-01', 'GM05012', 'HN05015', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-06-01', 'GM05012', 'HN05015', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-07-01', 'GM05012', 'HN05015', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-08-01', 'GM05012', 'HN05015', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-09-01', 'GM05012', 'HN05015', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-10-01', 'GM05012', 'HN05015', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-11-01', 'GM05012', 'HN05015', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-12-01', 'GM05012', 'HN05015', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-01-01', 'GM05013', 'HN05015', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-02-01', 'GM05013', 'HN05015', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-03-01', 'GM05013', 'HN05015', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-04-01', 'GM05013', 'HN05015', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-05-01', 'GM05013', 'HN05015', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-06-01', 'GM05013', 'HN05015', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-07-01', 'GM05013', 'HN05015', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-08-01', 'GM05013', 'HN05015', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-09-01', 'GM05013', 'HN05015', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-10-01', 'GM05013', 'HN05015', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-11-01', 'GM05013', 'HN05015', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-12-01', 'GM05013', 'HN05015', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-01-01', 'GM05013', 'HN05015', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-02-01', 'GM05013', 'HN05015', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-03-01', 'GM05013', 'HN05015', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-04-01', 'GM05013', 'HN05015', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-05-01', 'GM05013', 'HN05015', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-06-01', 'GM05013', 'HN05015', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-07-01', 'GM05013', 'HN05015', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-08-01', 'GM05013', 'HN05015', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-09-01', 'GM05013', 'HN05015', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-10-01', 'GM05013', 'HN05015', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-11-01', 'GM05013', 'HN05015', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-12-01', 'GM05013', 'HN05015', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-01-01', 'GM05013', 'HN05015', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-02-01', 'GM05013', 'HN05015', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-03-01', 'GM05013', 'HN05015', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-04-01', 'GM05013', 'HN05015', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-05-01', 'GM05013', 'HN05015', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-06-01', 'GM05013', 'HN05015', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-07-01', 'GM05013', 'HN05015', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-08-01', 'GM05013', 'HN05015', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-09-01', 'GM05013', 'HN05015', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-10-01', 'GM05013', 'HN05015', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-11-01', 'GM05013', 'HN05015', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-12-01', 'GM05013', 'HN05015', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-01-01', 'GM05013', 'HN05015', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-02-01', 'GM05013', 'HN05015', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-03-01', 'GM05013', 'HN05015', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-04-01', 'GM05013', 'HN05015', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-05-01', 'GM05013', 'HN05015', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-06-01', 'GM05013', 'HN05015', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-07-01', 'GM05013', 'HN05015', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-08-01', 'GM05013', 'HN05015', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-09-01', 'GM05013', 'HN05015', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-10-01', 'GM05013', 'HN05015', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-11-01', 'GM05013', 'HN05015', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-12-01', 'GM05013', 'HN05015', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-01-01', 'GM05013', 'HN05015', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-02-01', 'GM05013', 'HN05015', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-03-01', 'GM05013', 'HN05015', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-04-01', 'GM05013', 'HN05015', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-05-01', 'GM05013', 'HN05015', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-06-01', 'GM05013', 'HN05015', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-07-01', 'GM05013', 'HN05015', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-08-01', 'GM05013', 'HN05015', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-09-01', 'GM05013', 'HN05015', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-10-01', 'GM05013', 'HN05015', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-11-01', 'GM05013', 'HN05015', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-12-01', 'GM05013', 'HN05015', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-01-01', 'GM05011', 'HN05016', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-02-01', 'GM05011', 'HN05016', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-03-01', 'GM05011', 'HN05016', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-04-01', 'GM05011', 'HN05016', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-05-01', 'GM05011', 'HN05016', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-06-01', 'GM05011', 'HN05016', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-07-01', 'GM05011', 'HN05016', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-08-01', 'GM05011', 'HN05016', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-09-01', 'GM05011', 'HN05016', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-10-01', 'GM05011', 'HN05016', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-11-01', 'GM05011', 'HN05016', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-12-01', 'GM05011', 'HN05016', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-01-01', 'GM05011', 'HN05016', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-02-01', 'GM05011', 'HN05016', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-03-01', 'GM05011', 'HN05016', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-04-01', 'GM05011', 'HN05016', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-05-01', 'GM05011', 'HN05016', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-06-01', 'GM05011', 'HN05016', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-07-01', 'GM05011', 'HN05016', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-08-01', 'GM05011', 'HN05016', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-09-01', 'GM05011', 'HN05016', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-10-01', 'GM05011', 'HN05016', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-11-01', 'GM05011', 'HN05016', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-12-01', 'GM05011', 'HN05016', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-01-01', 'GM05011', 'HN05016', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-02-01', 'GM05011', 'HN05016', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-03-01', 'GM05011', 'HN05016', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-04-01', 'GM05011', 'HN05016', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-05-01', 'GM05011', 'HN05016', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-06-01', 'GM05011', 'HN05016', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-07-01', 'GM05011', 'HN05016', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-08-01', 'GM05011', 'HN05016', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-09-01', 'GM05011', 'HN05016', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-10-01', 'GM05011', 'HN05016', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-11-01', 'GM05011', 'HN05016', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-12-01', 'GM05011', 'HN05016', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-01-01', 'GM05011', 'HN05016', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-02-01', 'GM05011', 'HN05016', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-03-01', 'GM05011', 'HN05016', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-04-01', 'GM05011', 'HN05016', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-05-01', 'GM05011', 'HN05016', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-06-01', 'GM05011', 'HN05016', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-07-01', 'GM05011', 'HN05016', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-08-01', 'GM05011', 'HN05016', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-09-01', 'GM05011', 'HN05016', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-10-01', 'GM05011', 'HN05016', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-11-01', 'GM05011', 'HN05016', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-12-01', 'GM05011', 'HN05016', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-01-01', 'GM05011', 'HN05016', 'R0592', 0.322916362948455, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-02-01', 'GM05011', 'HN05016', 'R0592', 0.27971446324255, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-03-01', 'GM05011', 'HN05016', 'R0592', 0.28983355711085, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-04-01', 'GM05011', 'HN05016', 'R0592', 0.276411879084083, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-05-01', 'GM05011', 'HN05016', 'R0592', 0.30989878219692, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-06-01', 'GM05011', 'HN05016', 'R0592', 0.287290654081895, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-07-01', 'GM05011', 'HN05016', 'R0592', 0.289401303962656, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-08-01', 'GM05011', 'HN05016', 'R0592', 0.316016988055124, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-09-01', 'GM05011', 'HN05016', 'R0592', 0.324679280848115, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-10-01', 'GM05011', 'HN05016', 'R0592', 0.283777388858501, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-11-01', 'GM05011', 'HN05016', 'R0592', 0.286624942785605, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-12-01', 'GM05011', 'HN05016', 'R0592', 0.31616688312453, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-01-01', 'GM05012', 'HN05016', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-02-01', 'GM05012', 'HN05016', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-03-01', 'GM05012', 'HN05016', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-04-01', 'GM05012', 'HN05016', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-05-01', 'GM05012', 'HN05016', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-06-01', 'GM05012', 'HN05016', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-07-01', 'GM05012', 'HN05016', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-08-01', 'GM05012', 'HN05016', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-09-01', 'GM05012', 'HN05016', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-10-01', 'GM05012', 'HN05016', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-11-01', 'GM05012', 'HN05016', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-12-01', 'GM05012', 'HN05016', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-01-01', 'GM05012', 'HN05016', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-02-01', 'GM05012', 'HN05016', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-03-01', 'GM05012', 'HN05016', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-04-01', 'GM05012', 'HN05016', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-05-01', 'GM05012', 'HN05016', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-06-01', 'GM05012', 'HN05016', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-07-01', 'GM05012', 'HN05016', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-08-01', 'GM05012', 'HN05016', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-09-01', 'GM05012', 'HN05016', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-10-01', 'GM05012', 'HN05016', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-11-01', 'GM05012', 'HN05016', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-12-01', 'GM05012', 'HN05016', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-01-01', 'GM05012', 'HN05016', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-02-01', 'GM05012', 'HN05016', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-03-01', 'GM05012', 'HN05016', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-04-01', 'GM05012', 'HN05016', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-05-01', 'GM05012', 'HN05016', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-06-01', 'GM05012', 'HN05016', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-07-01', 'GM05012', 'HN05016', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-08-01', 'GM05012', 'HN05016', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-09-01', 'GM05012', 'HN05016', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-10-01', 'GM05012', 'HN05016', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-11-01', 'GM05012', 'HN05016', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-12-01', 'GM05012', 'HN05016', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-01-01', 'GM05012', 'HN05016', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-02-01', 'GM05012', 'HN05016', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-03-01', 'GM05012', 'HN05016', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-04-01', 'GM05012', 'HN05016', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-05-01', 'GM05012', 'HN05016', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-06-01', 'GM05012', 'HN05016', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-07-01', 'GM05012', 'HN05016', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-08-01', 'GM05012', 'HN05016', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-09-01', 'GM05012', 'HN05016', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-10-01', 'GM05012', 'HN05016', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-11-01', 'GM05012', 'HN05016', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-12-01', 'GM05012', 'HN05016', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-01-01', 'GM05012', 'HN05016', 'R0592', 0.491110046486174, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-02-01', 'GM05012', 'HN05016', 'R0592', 0.509445220434101, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-03-01', 'GM05012', 'HN05016', 'R0592', 0.50967089479616, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-04-01', 'GM05012', 'HN05016', 'R0592', 0.509151967129888, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-05-01', 'GM05012', 'HN05016', 'R0592', 0.532373368050385, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-06-01', 'GM05012', 'HN05016', 'R0592', 0.519602517627664, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-07-01', 'GM05012', 'HN05016', 'R0592', 0.50782341025306, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-08-01', 'GM05012', 'HN05016', 'R0592', 0.503181359455506, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-09-01', 'GM05012', 'HN05016', 'R0592', 0.520479231640077, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-10-01', 'GM05012', 'HN05016', 'R0592', 0.522640708279234, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-11-01', 'GM05012', 'HN05016', 'R0592', 0.511975931963858, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-12-01', 'GM05012', 'HN05016', 'R0592', 0.521689242957802, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-01-01', 'GM05013', 'HN05016', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-02-01', 'GM05013', 'HN05016', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-03-01', 'GM05013', 'HN05016', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-04-01', 'GM05013', 'HN05016', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-05-01', 'GM05013', 'HN05016', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-06-01', 'GM05013', 'HN05016', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-07-01', 'GM05013', 'HN05016', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-08-01', 'GM05013', 'HN05016', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-09-01', 'GM05013', 'HN05016', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-10-01', 'GM05013', 'HN05016', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-11-01', 'GM05013', 'HN05016', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('1', '2025-12-01', 'GM05013', 'HN05016', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-01-01', 'GM05013', 'HN05016', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-02-01', 'GM05013', 'HN05016', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-03-01', 'GM05013', 'HN05016', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-04-01', 'GM05013', 'HN05016', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-05-01', 'GM05013', 'HN05016', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-06-01', 'GM05013', 'HN05016', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-07-01', 'GM05013', 'HN05016', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-08-01', 'GM05013', 'HN05016', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-09-01', 'GM05013', 'HN05016', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-10-01', 'GM05013', 'HN05016', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-11-01', 'GM05013', 'HN05016', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('2', '2025-12-01', 'GM05013', 'HN05016', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-01-01', 'GM05013', 'HN05016', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-02-01', 'GM05013', 'HN05016', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-03-01', 'GM05013', 'HN05016', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-04-01', 'GM05013', 'HN05016', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-05-01', 'GM05013', 'HN05016', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-06-01', 'GM05013', 'HN05016', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-07-01', 'GM05013', 'HN05016', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-08-01', 'GM05013', 'HN05016', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-09-01', 'GM05013', 'HN05016', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-10-01', 'GM05013', 'HN05016', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-11-01', 'GM05013', 'HN05016', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('3', '2025-12-01', 'GM05013', 'HN05016', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-01-01', 'GM05013', 'HN05016', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-02-01', 'GM05013', 'HN05016', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-03-01', 'GM05013', 'HN05016', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-04-01', 'GM05013', 'HN05016', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-05-01', 'GM05013', 'HN05016', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-06-01', 'GM05013', 'HN05016', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-07-01', 'GM05013', 'HN05016', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-08-01', 'GM05013', 'HN05016', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-09-01', 'GM05013', 'HN05016', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-10-01', 'GM05013', 'HN05016', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-11-01', 'GM05013', 'HN05016', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('4', '2025-12-01', 'GM05013', 'HN05016', 'R0592', 0.162143873917668, 'Budget_V4');
'INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-01-01', 'GM05013', 'HN05016', 'R0592', 0.185973590565371, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-02-01', 'GM05013', 'HN05016', 'R0592', 0.210840316323349, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-03-01', 'GM05013', 'HN05016', 'R0592', 0.20049554809299, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-04-01', 'GM05013', 'HN05016', 'R0592', 0.214436153786029, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-05-01', 'GM05013', 'HN05016', 'R0592', 0.157727849752694, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-06-01', 'GM05013', 'HN05016', 'R0592', 0.193106828290441, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-07-01', 'GM05013', 'HN05016', 'R0592', 0.202775285784284, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-08-01', 'GM05013', 'HN05016', 'R0592', 0.18080165248937, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-09-01', 'GM05013', 'HN05016', 'R0592', 0.154841487511808, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-10-01', 'GM05013', 'HN05016', 'R0592', 0.193581902862265, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-11-01', 'GM05013', 'HN05016', 'R0592', 0.201399125250537, 'Budget_V4');INSERT INTO XFC_PLT_AUX_AllocationKeys (costnature, date, id_averagegroup, id_costcenter, id_factory, percentage, scenario) VALUES ('5', '2025-12-01', 'GM05013', 'HN05016', 'R0592', 0.162143873917668, 'Budget_V4');

'				"
				#End Region
				
				#Region "Master Product descriptions"
				sQuery = "			
				
				UPDATE XFC_PLT_MASTER_Product SET description = 'Timming cover HR16', new_product = 0 WHERE id = '135028729R';
				UPDATE XFC_PLT_MASTER_Product SET description = 'Timming cover HR10', new_product = 0 WHERE id = '135028243R';
				UPDATE XFC_PLT_MASTER_Product SET description = 'Timming cover HR13', new_product = 0 WHERE id = '135026774R';
				UPDATE XFC_PLT_MASTER_Product SET description = 'Ladder frame HR10', new_product = 0 WHERE id = '110171810R';
				UPDATE XFC_PLT_MASTER_Product SET description = 'Ladder frame HR13', new_product = 0 WHERE id = '110177181R';
				UPDATE XFC_PLT_MASTER_Product SET description = 'CARTER EMBRAYAGE JR5', new_product = 0 WHERE id = '304019713R';
				UPDATE XFC_PLT_MASTER_Product SET description = 'CARTER MECANISME  JR5', new_product = 0 WHERE id = '321014602R';
				UPDATE XFC_PLT_MASTER_Product SET description = 'CARTER MECANISME JH3', new_product = 0 WHERE id = '8200478267';
				UPDATE XFC_PLT_MASTER_Product SET description = 'CARTER EMBRAYAGE JH3', new_product = 0 WHERE id = '8200478266';
				UPDATE XFC_PLT_MASTER_Product SET description = 'CARTER EMBRAYAGE JH3 B4D', new_product = 0 WHERE id = '304016091R';
				UPDATE XFC_PLT_MASTER_Product SET description = 'CARTER EMBRAYAGE JH3 HX6', new_product = 0 WHERE id = '304016666R';
				UPDATE XFC_PLT_MASTER_Product SET description = 'CARTER EMBRAYAGE JH3 PF K', new_product = 0 WHERE id = '304017639R';
					"
				#End Region
				
				#Region "Drop raw tables"
				sQuery = "
					DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_R0041606			;
					DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_R00450106		;
					DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_R0045106			;
					DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_R0483003			;
					DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_R0529002			;
					DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_R0548913			;
					DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_R0548914			;
					DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_R054913			;
					DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_R0585			;
					DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_R0592			;
					DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_R0671			;
																						;
					DROP TABLE IF EXISTS XFC_PLT_FACT_CostsDistribution_Raw_R0045106	;
					DROP TABLE IF EXISTS XFC_PLT_FACT_CostsDistribution_Raw_R0483003	;
					DROP TABLE IF EXISTS XFC_PLT_FACT_CostsDistribution_Raw_R0529002	;
					DROP TABLE IF EXISTS XFC_PLT_FACT_CostsDistribution_Raw_R0548913	;
					DROP TABLE IF EXISTS XFC_PLT_FACT_CostsDistribution_Raw_R0548914	;
					DROP TABLE IF EXISTS XFC_PLT_FACT_CostsDistribution_Raw_R0585		;
					DROP TABLE IF EXISTS XFC_PLT_FACT_CostsDistribution_Raw_R0592		;
					DROP TABLE IF EXISTS XFC_PLT_FACT_CostsDistribution_Raw_R0671		;
				
					DROP TABLE IF EXISTS XFC_PLT_RAW_Production_Actual_Times_R0045106	;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Production_Actual_Times_R0483003	;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Production_Actual_Times_R0529002	;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Production_Actual_Times_R0548913	;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Production_Actual_Times_R0548914	;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Production_Actual_Times_R0585		;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Production_Actual_Times_R0592		;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Production_Actual_Times_R0671		;
				
					DROP TABLE IF EXISTS XFC_PLT_RAW_Nomenclature						;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Nomenclature_Date					;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Nomenclature_Date_Planning			;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Production							;
					DROP TABLE IF EXISTS XFC_PLT_RAW_DailyHours							;
					DROP TABLE IF EXISTS XFC_PLT_RAW_EffectsAnalysis					;
					DROP TABLE IF EXISTS XFC_PLT_RAW_EnergyVariance						;
					DROP TABLE IF EXISTS XFC_PLT_RAW_KSB1								;
					DROP TABLE IF EXISTS XFC_PLT_RAW_KSB1_Raw							;
					DROP TABLE IF EXISTS XFC_PLT_RAW_StartUpCosts						;
					DROP TABLE IF EXISTS XFC_PLT_RAW_RampUpCosts						;
					DROP TABLE IF EXISTS XFC_PLT_RAW_TimePresence						;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Workforce							;
					DROP TABLE IF EXISTS XFC_PLT_RAW_CostsAllocations					;
					DROP TABLE IF EXISTS XFC_PLT_RAW_CostsFixedVariable					;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Calendar							;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Balancing							;
					DROP TABLE IF EXISTS XFC_PLT_RAW_CostsInput							;
					DROP TABLE IF EXISTS XFC_PLT_RAW_DailyHours_Stored					;
					DROP TABLE IF EXISTS XFC_PLT_RAW_HRAdditionalInfo					;
					DROP TABLE IF EXISTS XFC_PLT_RAW_CostMonthlyPRE						;
				
					DROP TABLE IF EXISTS XFC_PLT_RAW_NewProducts_Simulation				;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Production_Planning_Times			;
					DROP TABLE IF EXISTS XFC_PLT_RAW_Production_Planning_Volumes_Dist	;
				
					-- DROP TABLE IF EXISTS XFC_PLT_HIER_Nomenclature_Date_BackUp			;
					-- DROP TABLE IF EXISTS XFC_PLT_HIER_Nomenclature_Date_BackUp_July		;
					-- DROP TABLE IF EXISTS XFC_PLT_AUX_FixedVariableCosts_BackUp			;
				
					
					
				
				"
				#End Region						
												
				#Region "MASTER - Product Factory"
				
				sQuery = "
					TRUNCATE TABLE XFC_PLT_MASTER_Product_Factory;
				
					WITH Reference as (
						SELECT DISTINCT id_factory, id_product
						FROM XFC_PLT_HIER_Nomenclature_Date
					)
					
					, Component as (
						SELECT DISTINCT id_factory, id_component
						FROM XFC_PLT_HIER_Nomenclature_Date
					)
					
					, FinalClasification as (
					SELECT 
						COALESCE(A.id_factory, B.id_factory) as id_factory
						, COALESCE(A.id_product, B.id_component) as id_product
						, CASE 
							WHEN A.id_product IS NULL THEN 'RMat'
							WHEN B.id_component IS NULL THEN 'Fin'
							ELSE 'WIP'
						END AS product_level
					
					FROM Reference A
					
					FULL OUTER JOIN Component B
						ON  B.id_factory = A.id_factory
						AND B.id_component = A.id_product
					
					)

					INSERT INTO XFC_PLT_MASTER_Product_Factory (id_factory, id_product, level, type, organ_name)
					SELECT 
					    A.id_factory,
					    A.id_product,
					    A.product_level,
						COALESCE(NULLIF(B.type,''), '#') AS Type,
						'#' as organ_name
				
					FROM FinalClasification A
					LEFT JOIN XFC_PLT_MASTER_Product B
						ON B.id = A.id_product
					;
				"
				
				#End Region
								
				#Region "Activity TSO correction"
'					sQuery = "
'						SELECT * INTO XFC_PLT_AUX_Production_Planning_Times_BackUp
'						FROM XFC_PLT_AUX_Production_Planning_Times;
'					"
					
					sQuery = "
						UPDATE XFC_PLT_AUX_Production_Planning_Times
						SET value = CAST(value/60 AS DECIMAL(18,6))
						WHERE 1=1
							AND (scenario = 'RF09' OR scenario = 'RF09_Ref')
							AND MONTH(date) BETWEEN 9 AND 12
							AND id_factory='R0529002'
							
					"
				#End Region
								
				#Region "Master Cost Center"
					
				sQuery = "
				
					TRUNCATE TABLE XFC_PLT_MASTER_CostCenter_Nature
					INSERT INTO XFC_PLT_MASTER_CostCenter_Nature (
					    scenario,
					    id_factory,
					    id_costcenter,
					    start_date,
					    end_date,
					    nature,
						id_averagegroup
					)
					SELECT
					    scenario,
					    id_factory,
					    id,
					    start_date,
					    end_date,
					    nature,
						id_averagegroup
					FROM XFC_PLT_MASTER_CostCenter_Hist;
				"
				
				sQuery = "
				DROP TABLE IF EXISTS XFC_PLT_MASTER_CostCenter_Hist_Testing
				SELECT *, CAST('2025-01-01' AS DATE) as start_date_sap, CAST('2025-01-01' AS DATE) as end_date_sap INTO XFC_PLT_MASTER_CostCenter_Hist_Testing
				FROM XFC_PLT_MASTER_CostCenter_Hist
				WHERE 1=0
				;
				"
				
				#End Region
				
				#Region "Master Nomenclature"
				
				sQuery = "					
					SELECT * INTO XFC_PLT_HIER_Nomenclature_Date_BackUp_July
					FROM XFC_PLT_HIER_Nomenclature_Date;
				
					SELECT * INTO XFC_PLT_MASTER_CostCenter_Hist_BackUp_July
					FROM XFC_PLT_MASTER_CostCenter_Hist;
				"
				
				#End Region
				
				#Region "Rubrica en COST DISTRIBUTION"
					sQuery = "
						-- INSERT INTO XFW_TDM_TablesDefinition 
						-- VALUES ('XFC_PLT_FACT_CostsDistribution', 'id_rubric', 'varchar',50,0,0,1,'',0,'2025-09-02','DEV1_c001406a_DEV_PCT_20250609.084100',9);
						
						-- ALTER TABLE XFC_PLT_FACT_CostsDistribution
						-- ADD id_rubric VARCHAR(50);
						
						UPDATE t
						SET t.id_rubric = s.id_rubric
						FROM XFC_PLT_FACT_CostsDistribution t
						INNER JOIN XFC_PLT_FACT_Costs s
						    ON t.scenario     = s.scenario
						   AND t.date         = s.date
						   AND t.id_factory   = s.id_factory
						   AND t.id_account   = s.id_account
						   AND t.id_costcenter= s.id_costcenter;
						
					"
				
				
				
				#End Region
				
				#Region "Function en Maestro GM"
					sQuery = "
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Autres pièces moteur' WHERE id_function = 'MUAP';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Usinage fonderie' WHERE id_function = 'MUBF';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Culasses' WHERE id_function = 'MUCU';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Vilebrequin' WHERE id_function = 'MUVI';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Boîtier différentiel' WHERE id_function = 'MBBD';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Assemblage Moteur' WHERE id_function = 'MAMT';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Carter Cylindres' WHERE id_function = 'MUCC';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Bielles' WHERE id_function = 'MUBI';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Carter de mécanisme' WHERE id_function = 'MBCM';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Arbre à Cames' WHERE id_function = 'MUAC';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Traitement thermique' WHERE id_function = 'MBTT';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Autres pièces BV' WHERE id_function = 'MBAP';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Packaging' WHERE id_function = 'PACK';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Assemblage BV' WHERE id_function = 'MABV';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Synchro (moyeux, baladeurs...)' WHERE id_function = 'MBSY';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Fonderie Alu' WHERE id_function = 'MFAL';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Soudure Châssis' WHERE id_function = 'MCHS';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Usinage Châssis' WHERE id_function = 'MUBC';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Autre Chassis' WHERE id_function = 'MCHO';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Emboutissage' WHERE id_function = 'MEMB';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Soudure/Assemblage châssis' WHERE id_function = 'MCHA';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Arbres' WHERE id_function = 'MBAR';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Pignons' WHERE id_function = 'MBPI';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Mecanique' WHERE id_function = 'FDIM';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'MPR' WHERE id_function = 'MMPR';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'CKD' WHERE id_function = 'CKD';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Assemblage Batterie' WHERE id_function = 'FBAT';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Peinture Châssis' WHERE id_function = 'MCHP';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Bancs et Tests' WHERE id_function = 'MMBT';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Carter d''embrayage' WHERE id_function = 'MBCE';
						UPDATE XFC_PLT_MASTER_AverageGroup SET function_description = 'Moteur Electrique' WHERE id_function = 'GMPE';
					"
				#End Region
				
				#Region "Nomenclature - PLANNING"
				
				sQuery = "					
					
				
					INSERT INTO XFC_PLT_HIER_Nomenclature_Date_Planning (
						scenario
						, id_factory
						, id_product
						, id_component
						, start_date
						, end_date
						, coefficient
						, prorata
					)
				
					SELECT
						'Budget_v1' as scenario
						, id_factory
						, id_product
						, id_component
						, start_date
						, end_date
						, coefficient
						, prorata
				
					FROM XFC_PLT_HIER_Nomenclature_Date
				
					WHERE 1=1
						AND 2026 BETWEEN YEAR(start_date) AND YEAR(end_date)  
				
				"
				
				#End Region
				
				#Region "UPDATE o Insert - Master CostCenters (GM, Nature)"
				
				sQuery = $"
				
					-- UPDATE XFC_PLT_MASTER_CostCenter_Nature
					-- SET end_date = '9999-12-31T00:00:00'
					-- WHERE 1=1
					-- 	AND scenario = 'Actual'
					-- 	AND id_factory = 'R0483003'
					-- 	AND id_costcenter = 'TZ14811';
				
					-- INSERT INTO dbo.[XFC_PLT_MASTER_CostCenter_Nature] 
					-- (
					--     [scenario],
					--     [id_factory],
					--     [id_costcenter],
					--     [start_date],
					--     [end_date],
					--     [nature],
					--     [id_averagegroup]
					-- )
					-- VALUES
					--     (N'Actual', N'R0483003', N'TZ14811', '2025-10-01T00:00:00', '2025-12-31T00:00:00', N'CP',  N'GM182784'),
					--     (N'Actual', N'R0483003', N'TZ14811', '2026-01-01T00:00:00', '9999-12-31T00:00:00', N'CP',  N'#');
					-- 
					-- 
					-- INSERT INTO dbo.[XFC_PLT_MASTER_CostCenter_Hist] 
					-- (
					--     [id],
					--     [id_factory],
					--     [description],
					--     [scenario],
					--     [start_date],
					--     [end_date],
					--     [type],
					--     [nature],
					--     [id_averagegroup],
					--     [function],
					--     [id_parent]
					-- )
					-- VALUES
					--     (N'TM5560', N'R0483003', 'Calidad Sev1', N'Actual', '2026-01-01T00:00:00', '9999-12-31T00:00:00', N'A', N'CAT', N'#', -1, -1),
					--     (N'TM5561', N'R0483003', 'Calidad Sev1', N'Actual', '2026-01-01T00:00:00', '9999-12-31T00:00:00', N'A', N'CAT', N'#', -1, -1),
					--     (N'TM5531', N'R0483003', 'Calidad Sev1', N'Actual', '2026-01-01T00:00:00', '9999-12-31T00:00:00', N'A', N'CAT', N'#', -1, -1),
					--     (N'TM5533', N'R0483003', 'Calidad Sev1', N'Actual', '2026-01-01T00:00:00', '9999-12-31T00:00:00', N'A', N'CAT', N'#', -1, -1);
					
					UPDATE XFC_PLT_MASTER_CostCenter_Hist	
					SET id_factory = 'R0548913'
					WHERE 1=1
						AND scenario = 'Actual'
						AND start_date = '2026-01-01T00:00:00'
						AND end_date = '9999-12-31T00:00:00'
						AND id_factory = 'R0483003'
						AND id IN ('TM5560', 'TM5561', 'TM5531', 'TM5533');
				"				
				#End Region
								
				#Region "Actualización Maestro Centros de Coste - SEPT"
				
				sQuery = $"
				
					/* 0. CREACIÓN TABLA NATURALEZA - GM */
					
						-- TRUNCATE TABLE XFC_PLT_MASTER_CostCenter_Nature;
						-- INSERT INTO XFC_PLT_MASTER_CostCenter_Nature (
						--     scenario,
						--     id_factory,
						--     id_costcenter,
						--     start_date,
						--     end_date,
						--     nature,
						-- 	id_averagegroup
						-- )
						-- SELECT
						--     scenario,
						--     id_factory,
						--     id,
						--     start_date,
						--     end_date,
						--     nature,
						-- 	id_averagegroup
						-- FROM XFC_PLT_MASTER_CostCenter_Hist;
				
						DROP TABLE IF EXISTS XFC_PLT_MASTER_CostCenter_Nature_BackUp_October;
						
						SELECT * INTO XFC_PLT_MASTER_CostCenter_Nature_BackUp_October
						FROM XFC_PLT_MASTER_CostCenter_Nature;
						
				
					/* 1. CREACIÓN DEL HISTÓRICO */
				
						DROP TABLE IF EXISTS XFC_PLT_MASTER_CostCenter_Hist_BackUp_October;
						
						SELECT * INTO XFC_PLT_MASTER_CostCenter_Hist_BackUp_October
						FROM XFC_PLT_MASTER_CostCenter_Hist;
				
					/* 2. ACTUALIZAR LOS CENTROS DE COSTE DE SAP */
	
				
					/* 3. COMPROBACIÓN DE REGISTROS DIFERENTES */
						/* 
						-- 3.1 Nuevos registros
				
						SELECT n.*
						FROM XFC_PLT_MASTER_CostCenter_Hist n
						LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist_BackUp b
						    ON n.id = b.id
						   AND n.id_factory = b.id_factory
						   -- (añadir más claves si hacen falta)
						WHERE b.id IS NULL;
					
						-- 3.2 Registros con cambios
				
						SELECT n.*, b.*
						FROM XFC_PLT_MASTER_CostCenter_Hist n
						JOIN XFC_PLT_MASTER_CostCenter_Hist_BackUp b
						    ON n.id = b.id
						   AND n.id_factory = b.id_factory
						WHERE 
						    ISNULL(n.description, '') <> ISNULL(b.description, '')
						 OR ISNULL(n.start_date, '19000101') <> ISNULL(b.start_date, '19000101')
						 OR ISNULL(n.end_date,   '99991231') <> ISNULL(b.end_date, '99991231')
						 OR ISNULL(n.type, '') <> ISNULL(b.type, '')
						 OR ISNULL(n.nature, '') <> ISNULL(b.nature, '')
						 OR ISNULL(n.id_averagegroup, '') <> ISNULL(b.id_averagegroup, '')
						 OR ISNULL(n.function, -1) <> ISNULL(b.function, -1)
						 OR ISNULL(n.id_parent, -1) <> ISNULL(b.id_parent, -1);
					
						-- 3.3 Registros borrados
				
						SELECT b.*
						FROM XFC_PLT_MASTER_CostCenter_Hist_BackUp b
						LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist n
						    ON b.id = n.id
						   AND b.id_factory = n.id_factory
						WHERE n.id IS NULL;
						*/
				"
				
				#End Region
				
				#Region "Delete de duplicados"
					sQuery = $"
						WITH d AS (
						    SELECT *,
						           ROW_NUMBER() OVER (
						             PARTITION BY [date], [id_account], [id_factory], [scenario]
						             ORDER BY (SELECT 0)                  -- o por una columna identidad si la tienes
						           ) AS rn
						    FROM dbo.XFC_PLT_AUX_FixedVarialeCosts
						)
						DELETE FROM d
						WHERE rn > 1;
					"
					
					sQuery = "
											
						-- SELECT * INTO XFC_PLT_AUX_FixedVariableCosts_BackUp
						-- FROM XFC_PLT_AUX_FixedVariableCosts;
					
						TRUNCATE TABLE XFC_PLT_AUX_FixedVariableCosts;
						
						INSERT INTO XFC_PLT_AUX_FixedVariableCosts
						SELECT DISTINCT *
						FROM XFC_PLT_AUX_FixedVariableCosts_BackUp;
					"
				#End Region				
				
				#Region "Actualizacion Maestros - Centros de Coste y Nomenclatura"
				
				sQuery = $"
				BEGIN TRY
					BEGIN TRANSACTION
								
					/* 0. CREACIÓN DE HISTÓRICOS - Cost Center - NATURE Y GM */
				
						DROP TABLE IF EXISTS XFC_PLT_MASTER_CostCenter_Nature_BackUp_October;
						
						SELECT * INTO XFC_PLT_MASTER_CostCenter_Nature_BackUp_October
						FROM XFC_PLT_MASTER_CostCenter_Nature;
										
					/* 1. CREACIÓN DEL HISTÓRICO - Cost Center - HIST */
				
						DROP TABLE IF EXISTS XFC_PLT_MASTER_CostCenter_Hist_BackUp_October;
						
						SELECT * INTO XFC_PLT_MASTER_CostCenter_Hist_BackUp_October
						FROM XFC_PLT_MASTER_CostCenter_Hist;
				
					/* 2. CREACIÓN DE HISTÓRICOS - Nomenclatuere - HIER Y REPORT */
				
						DROP TABLE IF EXISTS XFC_PLT_HIER_Nomenclature_Date_BackUp_October;
						
						SELECT * INTO XFC_PLT_HIER_Nomenclature_Date_BackUp_October
						FROM XFC_PLT_HIER_Nomenclature_Date;
				
						DROP TABLE IF EXISTS XFC_PLT_HIER_Nomenclature_Date_Report_BackUp_October;
						
						SELECT * INTO XFC_PLT_HIER_Nomenclature_Date_Report_BackUp_October
						FROM XFC_PLT_HIER_Nomenclature_Date_Report;
				
					COMMIT TRANSACTION;
				END TRY
				
				BEGIN CATCH
					ROLLBACK TRANSACTION;
				END CATCH;
				
				"
				#End Region
								
				#Region "Nomenclature Copy"

''				'2. Initialization NOMENCLATURE_REPORT
''				shared_queries.update_Nomenclature_Report(si, "Actual", "2025", "10",,"")
''				shared_BiBlend.CopyTable(si, "XFC_PLT_HIER_Nomenclature_Date_Report", "", "", "2025", "Actual")

''				Dim factoryList As New List(Of String) From {"R0045106","R0483003","R0529002","R0548913","R0548914","R0585","R0592","R0671"}

''				For sMonth As Integer = 1 To 8
''					For Each sFactory As String In factoryList
''						shared_queries.update_FactVTU_Report_NewTables(si, "Actual", "2025", sMonth.ToString, sFactory)
''					Next
''				Next	

				#End Region	

				#Region "Times View"
				sQuery = "
					CREATE OR ALTER VIEW VIEW_PLT_AUX_Production_Actual_Times As
				
				    SELECT 
						id_factory			as id_factory
						, id_product		as id_product
						, id_costcenter 	as id_costcenter
						, value				as value
						, comment			as comment			
						, start_date		as start_date
						, end_date			as end_date
						, uotype			as uotype
						, id_averagegroup	as id_averagegroup
						, id_workcenter		as id_workcenter			
				
				    FROM XFC_PLT_AUX_Production_Actual_Times
				"
				#End Region
				
				#Region "Products Allocaitons Mapping"
				
				sQuery = "
					-- ============================================================================
					-- BACKUP: BackUp de XFC_PLT_MASTER_Product
					-- ============================================================================
					SELECT * INTO XFC_PLT_MASTER_Product_BAK_20240626
					FROM XFC_PLT_MASTER_Product;
				"
							
				sQuery = "
					-- ============================================================================
					-- INSERT BAKUP: de nuevos productos en XFC_PLT_MASTER_Product
					-- ============================================================================
					INSERT INTO XFC_PLT_MASTER_Product (id, [type], [description], [new_product])
					SELECT P.id, P.[type], P.[description], P.[new_product]
					FROM XFC_PLT_MASTER_Product_BAK_20240626 P; 
				"
					
				sQuery = "
					-- ============================================================================
					-- MERGE: Populate XFC_PLT_MASTER_Product from VIEW_PLT_MASTER_Product_Properties
					-- ============================================================================
					;MERGE INTO XFC_PLT_MASTER_Product AS Target
					USING (
					    SELECT 
					        DATA.id_product
					        , DATA.desc_product
					        , DATA.organ_type
					        , DATA.family
					        , DATA.[index]
					        , DATA.[family_index]
					        , COALESCE(N1.organ_name, N2.organ_name, DATA.organ_type) AS organ_name
					
					    FROM (
					
					        SELECT 
					            P.id AS id_product
					            , P.description AS desc_product
					            , ISNULL(F1.organ_type, F2.organ_type) AS organ_type
					        
					            , ISNULL(F1.family, CASE WHEN ISNULL(F1.organ_type, F2.organ_type) IN ('GearBox','Engine') THEN LEFT(RIGHT(P.description,7),3) END) AS family
					        
					            , ISNULL(F1.[index], CASE WHEN ISNULL(F1.organ_type, F2.organ_type) IN ('GearBox','Engine') THEN RIGHT(P.description,3) END) AS [index]
					        
					            , ISNULL(F1.family, CASE WHEN ISNULL(F1.organ_type, F2.organ_type) IN ('GearBox','Engine') THEN LEFT(RIGHT(P.description,7),3) END)
					            + ' ' + ISNULL(F1.[index], CASE WHEN ISNULL(F1.organ_type, F2.organ_type) IN ('GearBox','Engine') THEN RIGHT(P.description,3) END) AS [family_index]
					        
					        FROM XFC_PLT_MASTER_Product P
					        
					        LEFT JOIN ( SELECT id_reference, MAX(id) AS id
					                    FROM XFC_PLT_AUX_Product_OldFiveDigits
					                    GROUP BY id_reference) F0
					            ON P.id = F0.id_reference
					        
					        LEFT JOIN XFC_PLT_AUX_Product_OldFiveDigits F1
					            ON F0.id_reference = F1.id_reference
					            AND F0.id = F1.id
					        
					        LEFT JOIN XFC_PLT_AUX_Product_FiveDigits F2
					            ON LEFT(P.id,5) = F2.id
					
					    ) DATA
					
					    LEFT JOIN (	SELECT family_index, MAX(organ_name) AS organ_name 
					                FROM XFC_PLT_AUX_Product_NamesList 
					                GROUP BY family_index) N1
					        ON DATA.family_index = N1.family_index
					
					    LEFT JOIN ( SELECT family, MAX(organ_name) AS organ_name 
					                FROM XFC_PLT_AUX_Product_NamesList 
					                GROUP BY family) N2
					        ON DATA.family = N2.family
					
					) AS Source
					ON Target.id = Source.id_product
					
					WHEN MATCHED THEN
					    UPDATE SET
					        [organ_type] = Source.organ_type
					        , [description] = Source.desc_product
					        , [organ_name] = Source.organ_name
					        , [family] = Source.family
					        , [index] = Source.[index]
					        , [family_index] = Source.[family_index]
					
					WHEN NOT MATCHED BY TARGET THEN
					    INSERT (id, [organ_type], [description], [organ_name], [family], [index], [family_index])
					    VALUES (
					        Source.id_product
					        , Source.organ_type
					        , Source.desc_product
					        , Source.organ_name
					        , Source.family
					        , Source.[index]
					        , Source.[family_index]
					    );
				"
				
				sQuery = "
					-- ============================================================================
					-- DELETE: Eliminar la tabla de BackUp
					-- ============================================================================
					DROP TABLE XFC_PLT_MASTER_Product_BAK_20240626;    
				"
				
				sQuery = "
					-- ============================================================================
					-- VIEW: VIEW_PLT_MASTER_Product_by_Factory
					-- Description: Products master data linked to production facts by factory
					-- ============================================================================
					
					CREATE OR ALTER VIEW VIEW_PLT_MASTER_Product_by_Factory AS
					
					SELECT 
					    FP.id_factory
					    , MP.id AS id
					    , MP.description
					    , MP.organ_name
					    , MP.family
					    , MP.[index]    
					    , MP.family_index
					    , MP.organ_type
					
					FROM (SELECT DISTINCT id_factory, id_product FROM XFC_PLT_FACT_Production) FP
					INNER JOIN XFC_PLT_MASTER_Product MP
					    ON FP.id_product = MP.id;				
				"

				#End Region
				
'				ExecuteSql(si, sQuery)

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

        Private Sub ExecuteSql(ByVal si As SessionInfo, ByVal sqlCmd As String)        
		
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                               
                BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, True)
				
            End Using   
				
        End Sub			
		
        Private Function ExecuteSqlOtherAPP(ByVal si As SessionInfo, ByVal sqlCmd As String, ByVal OtherApp As String) As DataTable
		
			Dim oar As New OpenAppResult
			Dim osi As SessionInfo = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, OtherApp, oar)
		
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(osi)
                               
                Dim dt As DataTable = BRAPi.Database.ExecuteSql(dbConnApp, sqlCmd, True)
				Return dt
				
            End Using   
			
			Return Nothing
				
        End Function					
				
		Private Sub ExecuteActionSQLOnBIBlend(ByVal si As SessionInfo, ByVal sqlCmd As String)
				'Use the name of the database, used in OneStream Server Configuration Utility >> App Server Config File >> Database Server Connections
				Dim extConnName As String = "OneStream BI Blend" 
                Using dbConnApp As DBConnInfo = BRApi.Database.CreateExternalDbConnInfo(si, extConnName)          
					BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, False, True)
                End Using                                                                               
        End Sub
		
	End Class
	
End Namespace