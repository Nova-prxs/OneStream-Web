# Seccion 5: Tools (9% del examen)

## Objetivos del Examen

- **201.5.1**: Demostrar comprension de los diferentes tipos de Data Management Steps y sus casos de uso apropiados
- **201.5.2**: Demostrar comprension de la funcionalidad del Excel Add-in
- **201.5.3**: Demostrar comprension de Application Properties (global, Currency, etc.)
- **201.5.4**: Demostrar como crear una scheduled task (tarea programada)
- **201.5.5**: Demostrar comprension del uso correcto de load/extract
- **201.5.6**: Demostrar comprension de la diferencia entre las herramientas de load/extract a nivel de aplicacion y a nivel de sistema

---

## Conceptos Clave

### Objetivo 201.5.1: Data Management Steps y sus casos de uso

Data Management es el modulo que permite copiar datos, limpiar datos y ejecutar procesos para un Cube, Scenario, Entity y Time. La estructura es jerarquica y se accede desde **Application > Tools > Data Management**.

#### Jerarquia de Data Management

- **Data Management Profile**: Organiza los Groups para su presentacion y acceso por parte de los usuarios.
- **Data Management Group**: Contenedor de nivel superior que agrupa Sequences y Steps. Un Group puede asignarse a multiples Profiles.
- **Data Management Sequence**: Serie ordenada de uno o mas Steps que se ejecutan en el orden en que estan organizados.
- **Data Management Step**: Unidad individual de trabajo con un tipo especifico.

![Interfaz de Data Management mostrando la jerarquia de Groups, Sequences y Steps](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1673-6426.png)

> **Punto clave para el examen**: Los Steps se crean primero y luego se asignan a Sequences. Las Sequences se organizan dentro de Groups, y los Groups se exponen a traves de Profiles.

#### Busqueda en Data Management

Se puede buscar rapidamente cualquier objeto de Data Management usando el icono de binoculares:
1. Seleccionar el tipo de objeto: All Items, Sequence, o Step.
2. Escribir texto de busqueda y hacer clic en Search.
3. Opcionalmente usar "View in Hierarchy" para ver los resultados en contexto jerarquico.

#### Renombrar, Copiar y Pegar

Las Sequences y Steps existentes pueden ser renombrados, copiados y pegados usando la barra de herramientas. Los **Data Management Groups y Profiles NO pueden copiarse ni pegarse**. Al pegar una Sequence o Step en la misma jerarquia, el nombre se sufija con "_copy".

---

#### Propiedades de Sequences

##### General

| Propiedad | Descripcion |
|-----------|-------------|
| **Name** | Nombre de la Sequence |
| **Description** | Descripcion opcional |
| **Data Management Group** | Group al que pertenece |
| **Application Server** | Permite asignar un servidor especifico si la Sequence tiene ejecucion prolongada |

##### Notification (Notificacion por email)

![Configuracion de Notification en una Sequence](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1675-6432.png)

| Propiedad | Descripcion |
|-----------|-------------|
| **Enable Email Notification** | Default False. Cuando es True, se pueden configurar notificaciones por usuario/grupo |
| **Notification Connection** | Conexion al servidor de email (se identifica automaticamente) |
| **Notification User and Groups** | Usuarios y grupos que recibiran la notificacion |
| **Notification Event Type** | Not Used, On Success, On Failure, On Success or Failure |

> **Importante**: Si no hay servidor de email disponible, esta funcionalidad se deshabilita. El Error Log captura informacion sobre usuarios con cuentas de email invalidas o desactivadas.

![Ejemplo de email de notificacion de Data Management](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1677-6437.png)

##### Queueing (Control de cola de ejecucion)

| Propiedad | Valor Default | Descripcion |
|-----------|---------------|-------------|
| **Use Queueing** | True | Controla la utilizacion del CPU del servidor |
| **Maximum % CPU Utilization** | 70% | Maximo CPU permitido antes de que la tarea pase de Queued a Running. No poner menos de 10 o la tarea puede nunca iniciar |
| **Maximum Queued Time Before Canceling** | 20 minutos | Tiempo maximo en cola antes de cancelar automaticamente |

> **Nota para el examen**: El procesamiento en cola de batch processing override otros ajustes de cola del Workflow. La cola de batch processing no aplica a batch script, solo a la pantalla Batch en el Workflow.

##### Parameter Substitutions 1-8

Permiten pasar variables a la Sequence. Cada parametro tiene Name (nombre) y Value (valor). Estos parametros se muestran automaticamente si estan configurados cuando se selecciona la Sequence en Task Scheduler.

---

#### Los 6 tipos de Data Management Steps integrados

##### 1. Calculate

Ejecuta calculos/consolidaciones integrados. Se crea un Step con Step Type = Calculate.

**Tipos de calculo disponibles:**

| Tipo de Calculo | Descripcion |
|----------------|-------------|
| **Calculate** | Ejecuta calculo a nivel de Entity dentro del miembro Local de la Consolidation Dimension, sin traducir ni consolidar |
| **Translate** | Ejecuta Calculate y luego traduce datos dentro del miembro Translated para cada relacion aplicable |
| **Consolidate** | Ejecuta Calculate, Translate y completa calculos a traves de toda la Consolidation Dimension |
| **Force Calculate/Translate/Consolidate** | Ejecuta como si cada celda necesitara ser recalculada. Ignora el estado de calculo actual |
| **Con Logging** | Activa logging detallado visible en Task Activity. Se puede hacer drill en el log para ver el tiempo y detalle de cada calculo |
| **Force con Logging** | Combina Force con Logging detallado |

**Parametros del Step Calculate:**

| Parametro | Descripcion |
|-----------|-------------|
| **Cube** | Cube donde se ejecutara la consolidacion/calculo |
| **Entity Filter** | Entidad(es) o combinaciones de jerarquias de Entity |
| **Parent Filter** | Para jerarquias alternativas, especifica el Parent |
| **Consolidated Filter** | Miembro(s) de Consolidation a incluir |
| **Scenario Filter** | Miembro(s) de Scenario a incluir |
| **Time Filter** | Miembro(s) de Time a incluir |

##### 2. Clear Data

Limpia datos de un Cube/Scenario/Entity/Time especifico.

| Propiedad | Descripcion |
|-----------|-------------|
| **Use Detailed Logging** | Si True, proporciona detalle adicional en Task Activity Log |
| **Cube** | Cube donde se limpiaran los datos |
| **Entity Filter** | Filtro de Entity |
| **Scenario Filter** | Filtro de Scenario |
| **Time Filter** | Filtro de Time |
| **Clear Imported Data** | Limpia el miembro Import de la Origin Dimension (True/False) |
| **Clear Forms Data** | Limpia el miembro Forms de la Origin Dimension (True/False) |
| **Clear Adjustment Data and Delete Journals** | Limpia miembros de Adjustment y elimina Journals (True/False) |
| **Clear Data Cell Attachments** | Limpia Data Cell Attachments |

##### 3. Copy Data

Copia datos entre combinaciones de Source y Destination. Permite copiar entre diferentes Cubes, Entities, Scenarios, Time periods y Views.

| Propiedad | Descripcion |
|-----------|-------------|
| **Source Cube / Destination Cube** | Cubes de origen y destino |
| **Source Entity Filter / Destination Entity Filter** | Filtros de Entity de origen y destino |
| **Source Scenario / Destination Scenario** | Scenarios de origen y destino |
| **Source Time Filter / Destination Time Filter** | Filtros de Time de origen y destino |
| **Source View / Destination View** | Permite copiar datos YTD a Periodic y viceversa. Si se deja en blanco, se copia al mismo View Member |
| **Copy Imported Data** | True/False para copiar O#Import |
| **Copy Forms Data** | True/False para copiar O#Forms |
| **Copy Adjustment Data** | True/False para copiar O#Adjustments |
| **Copy Data Cell Attachments** | Incluir Data Cell Attachments en la copia |

> **Punto clave**: Si los campos Source View y Destination View se dejan en blanco, los datos se copian al mismo View Member (YTD copia a YTD, Periodic copia a Periodic).

##### 4. Custom Calculate

El uso tipico es la **velocidad de calculos durante data entry** y la **flexibilidad para analisis What-if**. En lugar de ejecutar un Calculate o Consolidation completo, el Custom Calculate ejecuta un calculo sobre un slice de datos dentro de uno o mas Data Units.

**Caracteristicas clave:**

- **No crea informacion de auditoria** para cada celda afectada, por lo que es mas rapido que Copy Data.
- El usuario debe ser miembro del **Manage Data Group** del Scenario, o el Step fallara.
- Soporta **Durable Storage**: Datos guardados con Durable no se borran durante Calculate normal (solo con ClearCalculated explicito o Force).
- Acepta Parameters en formato: `Name1=Frankfurt, Name2=[Houston Heights]`
- Soporta Custom Parameters con sintaxis `Name3=|!myParam!|` que solicitan al usuario en tiempo de ejecucion.

**Propiedades especificas de Custom Calculate:**

| Propiedad | Descripcion |
|-----------|-------------|
| **Data Units** | Define Cube, Scenario (unicos), mas filtros de Entity, Parent, Consolidation y Time. La Business Rule se ejecuta una vez por cada Data Unit |
| **Point of View** | Entradas de dimensiones individuales no en el Data Unit (View, Account, Flow, Origin, IC, UD1-UD8). Se referencian desde la Business Rule |
| **Business Rule / Function Name** | Nombre de la Finance-Type Business Rule y funcion contenida |

> **Concepto critico de Durable Storage**: Cuando se ejecuta un Calculation o Consolidation despues de un Custom Calculate, los datos calculados por el Step se borran a menos que se hayan guardado con Storage Type de Durable. ClearCalculatedData es el primer paso en la secuencia de calculo estandar. Los datos Durable se ignoran incluso durante Force Calculate/Consolidate, a menos que se use una funcion ClearCalculated dentro de la Business Rule.

##### 5. Execute Business Rule

Ejecuta una Extensibility Business Rule seleccionada.

| Propiedad | Descripcion |
|-----------|-------------|
| **Business Rule** | Seleccion de las Business Rules disponibles de la aplicacion |
| **Parameters** | Campo opcional para pasar parametros o variables a la Business Rule |
| **Use Detailed Logging** | Si True, genera log detallado en Task Activity |

##### 6. Export Data

Exporta datos del Cube a un archivo en el File Share.

| Propiedad | Descripcion |
|-----------|-------------|
| **File Share File Name** | Nombre del archivo exportado |
| **Include Cube/Entity/Parent/Cons/Scenario/Time in File Name** | Opciones para incluir cada dimension en el nombre |
| **Overwrite Existing Files** | True para sobreescribir archivos existentes |
| **Include Zeroes** | Incluir registros con monto cero |
| **Include Member Descriptions** | Incluir descripciones de miembros |
| **Include Cell Annotations** | Incluir anotaciones de celdas |
| **Include Input Data / Calculated Data** | Incluir datos de tipo input y/o calculados |
| **Filtros de datos** | Account, Flow, Origin, IC, UD1-8 Filter (usar #All para todos los datos base almacenados) |
| **Specific Data Filters** | Para control granular sobre intersecciones especificas |

> **Ubicacion de archivos exportados**: Al ejecutar una Sequence desde Data Management, los archivos se encuentran en System > Tools > File Explorer > File Share > Applications > [Aplicacion] > DataManagement > Export > [Username] > [Carpeta mas reciente].

![Boton para ejecutar una Sequence o Step](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1694-6475.png)

---

#### Steps adicionales

##### Export File
Exporta un Extensible Document o Report Book al File Share de OneStream.

| Propiedad | Descripcion |
|-----------|-------------|
| **File Share Folder** | Carpeta de destino en File Share |
| **File Name Suffix** | Sufijo para el nombre del archivo |
| **Overwrite Existing Files** | True para sobreescribir |
| **File Source Type** | URL, Application/System Dashboard File, Application/System Database File, File Share File |
| **Process Extensible Document** | Si True, procesa el documento; si False, muestra sin procesar (para testing) |
| **Parameters** | Lista separada por comas de pares nombre-valor |

> **Nota**: Un Extensible Document es un archivo Text, Word, PowerPoint o Excel que usa Parameters en su contenido. El nombre del archivo debe contener `.xfDoc` antes de la extension (ejemplo: `StatusReport.xfDoc.docx`).

##### Export Report
Exporta Dashboards como PDFs.

| Propiedad | Descripcion |
|-----------|-------------|
| **Report File Type** | **PDFs in Zip File** (individuales en ZIP) o **Combined PDF File** (un solo PDF combinado) |
| **Object Type** | Dashboard o Dashboard Profile |
| **Object Name** | Nombre del Dashboard o Dashboard Profile |
| **Object Parameters** | Parametros opcionales usados al generar el reporte |

##### Reset Scenario

Similar a Clear Data pero mucho mas agresivo. Limpia datos (incluyendo datos de parent Entity), informacion de auditoria, Workflow Status y Calculation Status como si nunca hubieran existido.

**Diferencias clave con Clear Data:**

| Aspecto | Clear Data | Reset Scenario |
|---------|-----------|----------------|
| Limpia datos | Si | Si (incluyendo parent Entity data) |
| Limpia auditoria | No | Si |
| Limpia Workflow Status | No | Si |
| Limpia Calculation Status | No | Si |
| Crea auditoria del proceso | Si | No (mas rapido) |
| Limpia Durable data | No | Si |
| Requiere Manage Data Group | No | Si |

> **Mejores practicas para Reset Scenario**: Solo cambiar Manage Data Group de Nobody a un User Group exclusivo antes de ejecutar, y luego cambiar de vuelta a Nobody. **Siempre hacer backup de la base de datos** antes de ejecutar Reset Scenario.

| Propiedad | Descripcion |
|-----------|-------------|
| **Scenario** | Un Scenario a resetear |
| **Reset All Time Periods** | Default True. Si False, se habilitan Start Year/End Year |
| **Start Year / End Year** | Primer y ultimo ano a limpiar |
| **Start Time Period in First Year** | Periodo de tiempo opcional en el primer ano |

##### Execute Scenario Hybrid Source Data Copy
Copia datos base del Cube desde un Source Scenario usando la configuracion Hybrid Source Data. La copia ocurre cuando se ejecuta un calculo estandar en el Scenario destino.

---

### Objetivo 201.5.2: Funcionalidad del Excel Add-in

El Excel Add-In es una forma alternativa de ingresar, actualizar, gestionar, consultar y analizar datos de la aplicacion usando hojas de calculo Excel. Se puede lanzar desde multiples ubicaciones: OnePlace, Application Tab, System Tab, Workflow forms, Cell/Data Unit attachments, Dashboards, Cube Views y File Explorer.

#### Conexion del Excel Add-In

##### Agregar el Ribbon de OneStream

1. En Excel: File > Options > Add-ins
2. En Manage, seleccionar **COM Add-ins** y clic en Go
3. Marcar **OneStreamExcelAddIn** y clic en OK

![Seleccion de COM Add-ins en Excel Options](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2000-7515.png)

![Activacion del OneStreamExcelAddIn](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2000-7516.png)

##### Login al Excel Add-In

1. Desde el ribbon de OneStream en Excel, clic en **Log In**
2. En Server Address, usar el boton de elipsis para agregar o seleccionar la URL del servidor
3. Seleccionar una conexion disponible o agregar una nueva (URL + descripcion + Add)
4. Clic en **Connect** para autenticar
5. Ingresar username y password, clic en **Logon**
6. Seleccionar la aplicacion y clic en **Open Application**

![Ribbon de OneStream despues del login mostrando usuario y aplicacion](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2002-7525.png)

##### Client Updater

El Client Updater recupera software actualizado del servidor cuando la version del Excel Add-In no coincide con la version del servidor. El usuario necesita permisos de escritura en la carpeta de instalacion. Si las versiones no coinciden, se hace clic en Update y luego OK. Se debe cerrar Excel antes de proceder.

> **Nota**: Se crea automaticamente una carpeta de backup con los archivos de la version anterior.

#### Tres componentes principales del Excel Add-In

##### 1. Task Pane (Panel de tareas)

![Task Pane del Excel Add-In mostrando POV, Quick Views y Documents](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2004-7533.png)

| Tab | Descripcion |
|-----|-------------|
| **POV** | Muestra Global POV, Workflow POV y Cube POV. Vinculado al POV de la aplicacion |
| **Quick Views** | Construir y editar Quick Views |
| **Documents** | Documentos publicos o especificos del usuario. Acceso y gestion con File Explorer |

##### 2. OneStream Ribbon (Cinta)

El ribbon contiene las siguientes categorias:

![Ribbon de OneStream en Excel](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2001-7519.png)

**a) OneStream**: Muestra usuario actual y aplicacion. Login/Logoff.

**b) Explore**: Acceso rapido a Quick Views, Cube Views y Table Views.

![Seccion Explore del ribbon](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2006-7549.png)

**c) Analysis**: Data Attachments, Cell Detail, Drill Down, Copy POV, Convert to XFGetCells. La funcion "Copy POV from Data Cell" captura el POV de la celda seleccionada y permite pegarlo como XFGetCell.

**d) Refresh**:

| Accion | Refresh Sheet | Refresh Workbook |
|--------|---------------|------------------|
| **Funcion** | Refresca solo la hoja activa | Refresca todas las hojas del workbook |
| **Dirty Cells** | Limpia dirty cells solo en la hoja seleccionada | Limpia dirty cells en todas las hojas |
| **Parameters (CV)** | Muestra parameters de la hoja actual | Muestra todos los parameters del workbook |

**e) Calculation**: Consolidate/Translate/Calculate (si se tiene permiso).

**f) Submit**:

| Accion | Submit Sheet | Submit Workbook |
|--------|-------------|-----------------|
| **Funcion** | Identifica cambios en la hoja activa y los guarda en BD | Identifica cambios en todas las hojas y los guarda en BD |
| **Datos** | Solo la hoja seleccionada | Todas las hojas |
| **Parameters** | Sin prompts | Sin prompts |

**g) Spreading**: Distribucion de datos sobre celdas seleccionadas.

![Opciones de Spreading en el ribbon](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2011-7564.png)

| Tipo de Spreading | Descripcion |
|-------------------|-------------|
| **Fill** | Llena cada celda seleccionada con el valor en Amount to Spread |
| **Clear Data** | Limpia todos los datos en las celdas seleccionadas |
| **Factor** | Multiplica todas las celdas por el rate especificado |
| **Accumulate** | Toma el valor de la primera celda, lo multiplica por el rate, y coloca el resultado en la segunda celda, y asi sucesivamente |
| **Even Distribution** | Distribuye el Amount to Spread uniformemente entre las celdas seleccionadas |
| **445 Distribution** | Peso 4-4-5 entre las tres celdas seleccionadas |
| **454 Distribution** | Peso 4-5-4 entre las tres celdas seleccionadas |
| **544 Distribution** | Peso 5-4-4 entre las tres celdas seleccionadas |
| **Proportional Distribution** | Multiplica el valor de cada celda por el Amount to Spread y lo divide por la suma total. Si todas tienen valor cero, se comporta como Even Distribution |

**Propiedades de Spreading:**

| Propiedad | Descripcion |
|-----------|-------------|
| **Amount to Spread** | Valor a distribuir (default: ultima celda seleccionada) |
| **Rate** | Solo para Factor y Accumulate |
| **Retain Amount in Flagged Input Cells** | Si True, no aplica spreading a celdas marcadas |
| **Include Flagged Readonly Cells in Totals** | Si True, incluye celdas bloqueadas en el total (default True) |
| **Flag Selected Cells** | Marca celdas para retener su valor durante spreading |
| **Clear Flags** | Limpia marcas de celdas |

![Ejemplo de Accumulate Spreading con rate 1.5](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2012-7568.png)

**h) File Operations**: File Explorer, Save to Server, Save Offline Copy.

**i) General**: Object Lookup, In-Sheet Actions, Parameters, Select Member, Preferences.

**j) Tasks**: Task Activity.

##### 3. Error Logs

Almacenados localmente en la carpeta AppData. Se eliminan automaticamente despues de **60 dias**. Utiles para troubleshooting de problemas del Add-In.

---

#### Preferencias del Excel Add-In

![Panel de Preferences del Excel Add-In](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2018-7595.png)

| Preferencia | Descripcion |
|-------------|-------------|
| **Enable Macros for OneStream Event Processing** | Habilita macros de Excel para llamadas API de OneStream (default False) |
| **Invalidate Old Data When Workbook is Opened** | Fuerza data refresh al abrir (default False) |
| **Use Minimal Calculation for Refresh** | Solo calcula formulas en la hoja activa (default True). Solo Excel Add-In |
| **Use Multithreading for Cube View Workbook Refresh** | Procesamiento concurrente para multiples hojas (default depende de version) |
| **Retain All Formatting when Saving Offline** | Default False. Si True, obtiene todo el formato caracter por caracter |
| **Preserve Hidden Rows and Columns** | Activo solo cuando Retain All Formatting es True |

> **Recomendacion critica para el examen**: Se recomienda configurar Excel en **Manual Calculation Mode** para mejor rendimiento cuando se usa con OneStream.

---

#### Funciones de Retrieve (XF Functions)

| Funcion | Descripcion | Ejemplo |
|---------|-------------|---------|
| **XFGetCell** | Recupera datos con 20 parametros de dimension | `XFGetCell(NoDataAsZero, Cube, Entity, Parent, Cons, Scenario, Time, View, Account, Flow, Origin, IC, UD1-UD8)` |
| **XFGetCell5** | Igual que XFGetCell pero limita UD a 5 | Misma estructura con UD1-UD5 |
| **XFSetCell** | Guarda datos segun parametros | `XFSetCell(CellValue, StoreZeroAsNoData, Cube, ...)` |
| **XFGetFXRate** | Recupera tasas de cambio | `XFGetFXRate(DisplayNoDataAsZero, FXRateType, Time, SourceCurrency, DestCurrency)` |
| **XFSetFXRate** | Guarda tasas de cambio | `XFSetFXRate(Value, StoreZeroAsNoData, FXRateType, Time, Source, Dest)` |
| **XFGetMemberProperty** | Recupera propiedades de miembros de dimension | `XFGetMemberProperty("Entity","Houston","Currency","","","")` |
| **XFGetRelationshipProperty** | Recupera propiedades de relaciones | `XFGetRelationshipProperty("Entity","Houston","South Houston","PercentConsolidation","","2015M7")` |
| **XFGetHierarchyProperty** | Determina si una dimension tiene hijos (True/False) | `XFGetHierarchyProperty("entity","HoustonEntities","Houston Heights","HasChildren","Houston","Actual",FALSE)` |
| **XFGetCellUsingScript** | Usa Member Script en vez de parametros individuales | `XFGetCellUsingScript(TRUE,"GolfStream","E#Frankfurt:C#Local:S#Actual:T#2022M1:V#YTD:A#10100:...","","")` |
| **XFGetDashboardParameterValue** | Obtiene valor de parametro de Dashboard | `XFGetDashboardParameterValue("myParamName","DefaultValue")` |

> **Para el examen**: Si un campo dentro de la funcion no se necesita, se ingresa una comilla doble `""` para ignorarlo. Si se escribe mal el nombre de un miembro, la celda retorna un error indicando el primer nombre mal escrito.

##### Funciones con sufijo Volatile

Funciones como `XFGetCellVolatile`, `XFSetCellVolatile`, etc. son necesarias en casos donde Excel requiere una funcion volatil para refrescar correctamente (por ejemplo, algunos graficos de Excel que referencian celdas calculadas).

##### Funciones con sufijo UsingScript

Las funciones `XFGetCellUsingScript`, `XFSetCellUsingScript`, etc. permiten usar Member Script (ej. `A#Sales:E#Texas`) en lugar de parametros individuales. Multiples parametros de Member Script se combinan en un solo script.

---

#### Quick Views

Reportes ad hoc que permiten pivotar, drill back, crear datasets y disenar workbooks.

##### Creacion de Quick Views

1. Ingresar miembros con formato **DimensionToken#MemberName** (ej. `A#IncomeStatement`, `T#2018M1`)
2. Seleccionar el area y clic en **Create Quick View**

![Creacion de Quick View ingresando dimension tokens](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2046-7691.png)

##### Funciones de expansion de miembros

| Funcion | Descripcion |
|---------|-------------|
| `Root.Children` | Hijos directos del Root |
| `Root.List(60999,64000,63000)` | Lista especifica de miembros |
| `.Base` | Miembros de nivel base |
| `.Descendants` | Todos los descendientes |
| `2018.Base` | Periodos base del ano 2018 |

##### Opciones de Quick View (Double-Click Behavior)

El comportamiento de doble clic se configura en Preferences > Quick View Double-Click Behavior. Las opciones incluyen: NextLevel (default), TreeDescendants, TreeDescendantsR (orden inverso), AllBase, Children, etc.

##### Conversion a XFGetCells

Se pueden convertir Quick Views existentes a formulas XFGetCell usando **Convert to XFGetCells** en el ribbon. Esto elimina la definicion del Quick View y crea formulas individuales. **No se puede revertir** la conversion.

---

#### Cube Views en Excel

##### Cube View Connection Settings

![Configuracion de Cube View Connection en Excel](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2026-8036.png)

| Propiedad | Descripcion |
|-----------|-------------|
| **Name** | Se auto-crea al seleccionar el Cube View |
| **Refers To** | Celda o rango de inicio en la hoja |
| **Cube View** | Seleccionar via Object Lookup |
| **Resize Initial Column Widths** | Auto-ajustar ancho segun configuracion del Cube View (default activo) |
| **Insert/Delete Rows When Resizing** | Para stacking vertical. Agrega/elimina filas automaticamente |
| **Insert/Delete Columns When Resizing** | Para stacking horizontal. Agrega/elimina columnas automaticamente |
| **Include Cube View Header** | Incluir encabezado del Cube View |
| **Retain Formulas in Cube View Content** | Retener formulas Excel en el Cube View tras Submit y Refresh |
| **Dynamically Evaluate Highlighted Cells** | Solo disponible si Retain Formulas esta activo. Resalta celdas con valores diferentes sin necesidad de Refresh |
| **Preserve Excel Format** | Preservar formato nativo de Excel en el Cube View |
| **Add Parameter Selectors to Sheet** | Genera selectores de parametros para el Cube View. Debe seleccionarse durante la creacion |

##### Retain Formulas in Cube View Content

Permite formar grids de datos en Excel usando Cube Views que pueden vincularse a otros modelos de Excel para enviar datos a OneStream. Las formulas en celdas escribibles se retienen al hacer Submit y Refresh. Si el valor de la formula difiere del valor en la BD, la celda se convierte en "dirty cell" (amarilla).

![Configuracion de Retain Formulas y Dynamically Highlight](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2029-8037.png)

**Mejores practicas para Retain Formulas:**
- Usar "Well-Formed Grid" (Root.List o lista separada por comas) en Cube Views
- Las formulas con VLOOKUP referenciando texto funcionan mejor cuando se mueven miembros
- Deseleccionar Retain Formulas eliminara todas las formulas existentes en el grid
- Pivotar dimensiones existentes rompera las formulas
- Cambiar la estructura del grid (reordenar dimensiones en filas/columnas) rompera las formulas

##### Named Regions

Al traer un Cube View a Excel se crean Named Regions automaticamente para: el Cube View completo, column headers, row headers y secciones de datos. Estas regiones se pueden usar con Styles para formato diferenciado.

![Ejemplo de Named Regions y Styles combinados en un reporte](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2038-7649.png)

---

#### Table Views

- Definidos mediante Business Rules de tipo **Spreadsheet**
- Diseñados para recopilar registros de tablas relacionales
- El administrador controla seguridad y write-back via Business Rules (usando funciones BRAPI Authorization)
- Un workbook puede contener multiples Table Views (misma hoja o diferentes hojas)
- Limite de rendimiento: ~**8000 KB** de datos por Table View
- **Solo actualizan registros existentes; NO realizan inserts**
- No soportan paging; todas las filas deben retornarse al mismo tiempo

---

#### In-Sheet Actions

Botones en la hoja para ejecutar acciones directamente sin salir de Excel.

| Propiedad | Descripcion |
|-----------|-------------|
| **Label** | Texto del boton |
| **Refers To** | Celda o rango donde aparece el boton |
| **Submit** | Sheet, Workbook, o Nothing |
| **Data Management Sequence** | Sequence a ejecutar al hacer clic |
| **Parameters** | Valores explicitos o dejar en blanco para resolver por selecciones de la hoja. Formato: `myparam=value1, myparam2=[South Houston]` |
| **Do not wait for task to finish** | Permite continuar trabajando mientras se ejecuta la Sequence |
| **Refresh** | Sheet, Workbook, o Nothing |
| **Text Color / Background Color** | Colores personalizados (codigo hex de 8 caracteres o selector) |

> **Importante**: Los parameters solo pueden usarse si han sido resueltos en un Cube View o Table View existente en la hoja o workbook. In-Sheet Actions no solicitan parametros no resueltos.

---

#### Spreadsheet (aplicacion Windows)

Funcionalidad similar al Excel Add-In pero integrada en la OneStream Windows App, sin necesitar Excel instalado.

**Limitaciones frente a Excel:**
- No soporta Macros, Solver, Document Properties
- No permite Undo/Redo
- No permite referenciar workbooks externos
- Algunos tipos de graficos 3D no disponibles (Stacked 3-D columns, 3-D Line, Stacked 3-D bars, Cylinder/Cone/Pyramid charts)
- Copiar Quick Views solo dentro del mismo workbook

---

### Objetivo 201.5.3: Application Properties

Accesible via **Application > Tools > Application Properties**. Aqui se configuran propiedades predeterminadas de la aplicacion.

#### General Properties

##### Global Point of View

| Propiedad | Descripcion |
|-----------|-------------|
| **Global Scenario** | Scenario predeterminado que los usuarios veran en Workflow. Debe configurarse un valor inicial incluso si la configuracion de Transformation no se usa |
| **Global Time** | Periodo de tiempo predeterminado que los usuarios veran en Workflow |

##### Company Information

| Propiedad | Descripcion |
|-----------|-------------|
| **Company Name** | Aparece en reportes generados automaticamente desde Cube Views |
| **Logo File** | Formato PNG, ~50 pixeles de alto. Aparece en Cube Views y Reports |

##### Workflow Channels

**UD Dimension Type for Workflow Channels**: La Origin Dimension controla la carga de datos, pero en algunos casos otras dimensiones User Defined requieren su propia capa de locking. Ejemplo: una empresa planifica por Entity por Product. Una Entity puede tener cinco productos gestionados por diferentes personas. Cada canal puede bloquearse separadamente.

##### Formatting

**Number Format**: Formato numerico global que puede ser sobreescrito por Cube Views.

| Formato | Resultado con 10000.001 |
|---------|------------------------|
| N0 | Sin decimales |
| N1-N6 | De 1 a 6 decimales (N2 = dos decimales, etc.) |
| #,###,0 | 10,000 y -10,000 |
| #,###,0;(#,###,0);0 | 10,000 y (10,000) |
| #,###,0.00 | 10,000.00 y -10,000.00 |
| #,###,0.00;(#,###,0.00);0.00 | 10,000.00 y (10,000.00) |
| #,###,0% | 10,000% y -10,000% |

> **Truco para el examen**: Para alinear verticalmente numeros positivos y negativos en reportes con parentesis, incluir espacios finales en el formato positivo. Ejemplo: `#,###,0.00 ;(#,###,0.00);0.00` (espacio despues de .00 en la seccion positiva).

> **La N en los formatos indica que son internacionales.**

##### Currencies

Todas las monedas usadas deben estar listadas aqui para ser usadas en Entity, traduccion de moneda o ingreso de tasas. Incluye monedas pre-Euro y descontinuadas para datos historicos. Si se necesita una moneda no listada, contactar a OneStream Support.

##### Transformation

![Icono de Enforce Global POV en Import Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1628-6298.png)

| Propiedad | Descripcion |
|-----------|-------------|
| **Enforce Global POV** | Si True, fuerza Global Scenario y Time para todos los usuarios. Muestra icono especial durante Import en Workflow |
| **Allow Loads Before Workflow View Year** | Si True, permite cargar datos a periodos anteriores al ano actual del Workflow |
| **Allow Loads After Workflow View Year** | Si True, permite cargar datos a periodos posteriores al ano actual del Workflow |

##### Certification

| Propiedad | Descripcion |
|-----------|-------------|
| **Lock after Certify** | Si True, bloquea automaticamente despues de la certificacion en Workflow |

#### Dimension Properties

![Dimension Properties mostrando Time y User Defined Dimensions](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1629-6303.png)

| Propiedad | Descripcion |
|-----------|-------------|
| **Start Year / End Year** | Define el rango de anos de la aplicacion |
| **UD1-8 Description** | Descripcion personalizada para cada dimension User Defined. Visible en: POV, Dimension Library, Cube View Member Filters, Drill Down headers, Excel Add-in/Spreadsheet, Journals |

> **Punto clave**: La descripcion aplica al **tipo de dimension**, no a cada dimension individual.

#### Standard Reports Properties

Estas configuraciones se aplican al auto-generar un reporte desde un Cube View.

| Seccion | Propiedades |
|---------|-------------|
| **Logo** | Height, Bottom Margin |
| **Title** | Top Margin, Font Family, Font Size, Bold, Italic, Text Color |
| **Header Labels** | Top/Bottom Margin, Font Family, Font Size, Bold, Italic, Text Color |
| **Header Bar** | Background Color, Line Color |
| **Footer** | Text, Font Family, Font Size, Show Line, Show Date, Show Page Numbers, Line Color, Text Color |

---

### Objetivo 201.5.4: Crear una Scheduled Task

Task Scheduler permite programar Data Management Sequences para ejecucion automatica. Acceso: **Application > Tools > Task Scheduler**.

![Pantalla principal de Task Scheduler](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1708-6518.png)

#### Vistas disponibles

##### Grid View

Vista tabular con columnas filtrables y agrupables. Se pueden filtrar multiples selecciones y agrupar por columna arrastrando al header bar.

![Grid View del Task Scheduler con tareas programadas](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1709-6521.png)

| Campo | Definicion |
|-------|-----------|
| **User Name** | Usuario que creo la tarea |
| **Name** | Nombre de la tarea |
| **Description** | Descripcion |
| **Sequence** | Data Management Sequence ejecutada |
| **Schedule** | Frecuencia implementada |
| **Next Start Time** | Proxima ejecucion programada |
| **Last Start Time** | Ultima ejecucion |
| **Expire Date/Time** | Cuando la tarea deja de ejecutarse |
| **State** | Enabled o Disabled |
| **Count** | Numero de veces que se ha ejecutado |
| **Invalidate Date/Time** | Fecha/hora de suspension (si esta habilitada por admin) |
| **Validate Task** | Validar tarea para mantenerla activa |

##### Calendar View

Vista calendario con tareas codificadas por colores por usuario. Soporta vistas: Today, Work Week, Week, Month, Timeline, Agenda. Se puede agrupar por user name, date, o sin grupo.

![Calendar View con tareas codificadas por color](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1716-6544.png)

> La vista que se use se recuerda para la proxima visita a la pagina.

#### Procedimiento paso a paso: Crear una nueva tarea

![Dialogo de nueva tarea - Tab Task](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1728-6587.png)

**Paso 1 - Tab Task:**
1. Clic en **Create Scheduled Task**
2. **Name**: Nombre de la tarea
3. **Description**: Descripcion
4. **Start Date/Time**: Fecha y hora de inicio (tiempo local del usuario). Se guarda en UTC. Si no se especifica, default a fecha/hora actual
5. **Sequence**: Seleccionar la Data Management Sequence (filtrar por nombre)
6. **Parameters**: Se muestran automaticamente si estan configurados en la Sequence

![Seleccion de Sequence con parametros](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1730-6595.png)

**Paso 2 - Tab Schedule:**

![Dialogo de Schedule mostrando opciones de frecuencia](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1731-6598.png)

| Frecuencia | Detalle |
|------------|---------|
| **One Time** | Ejecucion unica basada en Start Date/Time |
| **Minutes** | Recurrente cada 5-180 minutos. Se puede limitar con Time From/To |
| **Daily** | Configurar frecuencia de recurrencia |
| **Weekly** | Seleccionar dias y frecuencia. Ejemplo: Recur Every 2 weeks, On: Sunday, Monday, Friday |
| **Monthly** | Seleccionar dias y frecuencia |

Propiedades adicionales:
- **Expire Date/Time**: Cuando la tarea deja de ejecutarse
- **Enabled**: Habilitar/deshabilitar
- **Administration Enabled (Enabled by Manager)**: Solo el Administrator puede cambiar

> **Nota sobre Minutes**: Las entradas del calendario se crean aunque caigan fuera del rango de tiempo seleccionado. Ejemplo: una tarea cada 30 minutos entre 2:00pm y 5:00pm mostrara entradas todo el dia cada 30 minutos en el Calendar View.

**Paso 3 - Tab Advanced:**

![Tab Advanced mostrando numero de reintentos](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1732-6601.png)

Numero de reintentos si falla: **maximo 3**.

**Paso 4**: Clic en OK. La nueva tarea aparece en Grid View y Calendar View.

#### Ejemplo de logica de ejecucion semanal

Start Date/Time: 5/7/2024 11:55 AM (miercoles), Recur Every: 2 weeks, On: Sunday, Monday, Friday

| Fecha | Resultado |
|-------|-----------|
| Friday 5/9 | Run |
| Sunday 5/11 | Run |
| Monday 5/12 | Run |
| Friday 5/16 | Skip (segunda semana) |
| Sunday 5/18 | Skip |
| Friday 5/23 | Run (tercera semana = recurrencia) |
| Sunday 5/25 | Run |
| Monday 5/26 | Run |

#### Monitoreo de tareas

- **Task Activity**: El Task Type se muestra como "**Data Management Scheduled Task**". La descripcion es el nombre de la tarea separado por un guion seguido de la sequence.

![Task Activity mostrando tarea programada ejecutada](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1719-6557.png)

- **Logon Activity**: En System > Logon Activity, el Client Module aparece como "**Scheduler**".

![Logon Activity mostrando login via Scheduler](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1720-6560.png)

#### Roles de seguridad para Task Scheduler

![Roles de Application Security para Task Scheduler](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1722-6567.png)

| Rol | Tipo | Permisos |
|-----|------|----------|
| **TaskSchedulerPage** | Application UI Role | Ver la pagina de Task Scheduler. Ver tareas de todos |
| **TaskScheduler** | Application Security Role | Crear tareas, editar propias, validar tareas, ver todas. **No puede** hacer load/extract ni cambiar nombre de tarea |
| **ManageTaskScheduler** | Application Security Role | Crear, ver, editar y eliminar todas las tareas. **Puede** hacer load/extract. No puede cambiar nombre de tarea |

#### Load/Extract de tareas programadas

Si se tiene el rol ManageTaskScheduler, se pueden cargar y extraer archivos de Task Scheduler:
- **Load**: Application > Tools > Load/Extract > seleccionar archivo > clic Load
- **Extract**: Application > Tools > Load/Extract > clic Extract > File Type "Task Scheduler" > seleccionar tarea > Extract

---

### Objetivo 201.5.5: Uso correcto de Load/Extract

Load/Extract permite importar y exportar secciones de la aplicacion o sistema usando formato XML.

#### Conceptos generales

- **Load**: Importar un archivo XML con definiciones de artefactos
- **Extract**: Exportar artefactos a un archivo XML
- **Extract and Edit**: Extraer y editar el XML inmediatamente
- Solo **Administrators** tienen acceso por defecto
- Se usa para mover configuraciones entre entornos (Development > Test > Production)

![Interfaz de Load/Extract con opciones de Extract, Load y Extract and Edit](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1764-8030.png)

#### Application Load/Extract (Application > Tools > Load/Extract)

| Artefacto | Descripcion | Ubicacion en la app |
|-----------|-------------|---------------------|
| **Application Zip File** | Todo excepto Data y FX Rates. Crea una copia completa | Toda la aplicacion |
| **Application Security Roles** | Roles de seguridad de aplicacion | Application > Tools > Security Roles |
| **Application Properties** | Propiedades de la aplicacion | Application > Tools > Application Properties |
| **Workflow Channels** | Canales de Workflow | Application > Workflow > Workflow Channels |
| **Metadata** | Business Rules, Time Dimension Profiles, Dimensions, Cubes. Soporta busqueda de cambios por username y timestamp | Application > Cube > Dimension Library |
| **Cube Views** | Groups y Profiles | Application > Presentation > Cube Views |
| **Data Management** | Groups y Profiles | Application > Tools > Data Management |
| **Application Workspaces** | Maintenance Units y Profiles (Groups, Components, Adapters, Parameters) | Application > Presentation > Workspaces |
| **Confirmation Rules** | Groups y Profiles | Application > Workflow > Confirmation Rules |
| **Certification Questions** | Groups y Profiles | Application > Workflow > Certification Questions |
| **Transformation Rules** | Business Rules, Groups y Profiles | Application > Data Collection > Transformation Rules |
| **Data Sources** | Business Rules y Data Sources | Application > Data Collection > Data Sources |
| **Form Templates** | Groups y Profiles | Application > Data Collection > Form Templates |
| **Journal Templates** | Groups y Profiles | Application > Data Collection > Journal Templates |
| **Workflow Profiles** | Profiles y Templates. Al cargar, se limpian propiedades antiguas si no estan en el XML | Application > Workflow > Workflow Profiles |
| **Extensibility Rules** | Solo Extensibility Rules (otros tipos se exportan con su objeto asociado) | Application > Tools > Business Rules |
| **FX Rates** | Por FX Rate Type y Time Period | Application > Cube > FX Rates |
| **FX Rate CSV** | Exportacion CSV: FxRateType, Time, SourceCurrency, DestCurrency, Amount, HasData | Application > Cube > FX Rates |
| **Task Scheduler** | Tareas programadas | Application > Tools > Task Scheduler |

> **Punto clave sobre Workflow Profiles**: Al cargar via XML, el proceso de Load limpia configuraciones de propiedades antiguas que no estan especificadas en el XML cargado. Esto asegura que las ediciones hechas al Profile antes de la extraccion se respeten al recargar.

---

### Objetivo 201.5.6: Diferencia entre Application y System Load/Extract

#### System Load/Extract (System > Tools > Load/Extract)

![Interfaz de System Load/Extract](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1909-7196.png)

| Artefacto | Descripcion |
|-----------|-------------|
| **Security** | System Roles, Users, Security Groups, Exclusion Groups. Opcion **Extract Unique IDs** (desmarcar si se carga en otro entorno para evitar errores con IDs duplicados). Permite seleccionar Items To Extract especificos o secciones on/off |
| **System Dashboards** | Maintenance Units y Profiles (Groups, Components, Adapters, Parameters). Items To Extract con seleccion granular |
| **Error Log** | Con rango de Start/End Time. Checkbox "Extract All Items" para todo |
| **Task Activity** | Con rango de Start/End Time. Checkbox "Extract All Items" |
| **Logon Activity** | Con rango de Start/End Time. Checkbox "Extract All Items" |

#### Tabla comparativa completa

| Aspecto | Application Load/Extract | System Load/Extract |
|---------|--------------------------|---------------------|
| **Ubicacion** | Application > Tools > Load/Extract | System > Tools > Load/Extract |
| **Artefactos principales** | Metadata, Rules, Cube Views, Data Mgmt, Workflows, Dashboards, FX Rates, Task Scheduler | Security (Users, Groups, Roles), System Dashboards, Logs (Error, Task Activity, Logon) |
| **Base de datos** | Application Database | Framework (System) Database |
| **Alcance** | Una aplicacion especifica | Todo el entorno/sistema (compartido entre aplicaciones) |
| **Security Role (UI)** | ApplicationLoadExtractPage | SystemLoadExtractPage |
| **Security Roles adicionales** | Administrator o equivalente | ManageSystemSecurityUsers, ManageSystemSecurityGroups, ManageSystemSecurityRoles |
| **Formato** | XML (o ZIP para Application Zip File) | XML |
| **Opcion Extract Unique IDs** | No aplica | Si, desmarcar al mover a otro entorno |

#### Mejores practicas para Load/Extract

1. **Siempre desplegar y probar cambios primero en un entorno de desarrollo**
2. Considerar hacer una copia de la BD de produccion para probar cambios
3. Extraer cambios del entorno de desarrollo y evaluar en un entorno de test antes de produccion
4. **Evitar desplegar durante periodos de alta carga o actividad**
5. Para Security XML loads por usuarios no-Administrator: la seguridad debe preexistir en el entorno destino
6. Al mover Security entre entornos, **desmarcar Extract Unique IDs** para evitar conflictos
7. Para Workflow Profiles, tener en cuenta que el Load limpia propiedades no incluidas en el XML

---

## Herramientas de Sistema Adicionales (Contexto para el examen)

### File Explorer y File Share

**File Explorer** (System > Tools > File Explorer) permite gestionar documentos almacenados en la Application Database, System Database y File Share.

**File Share** es un directorio self-service que soporta almacenamiento de archivos externo a las bases de datos de OneStream.

#### Estructura de carpetas del File Share

| Carpeta | Proposito |
|---------|-----------|
| **Batch / Harvest** | Automatizacion de Connector Data Loads. Harvest se limpia automaticamente al cargar |
| **Content** | Almacenamiento seguro para archivos >300 MB (hasta 2 GB) |
| **Data Management** | Carpeta default para exportaciones de Data Management |
| **Groups** | Para componentes de dashboard con File Source Type de File Share, segurizado por grupo |
| **Incoming** | Archivos fuente para importar a Stage |
| **Internal** | Contenido de Application/System Database files |
| **Outgoing** | Disponible para procesos personalizados |

#### Permisos de File Share

- **Administrator** y **ManageFileShare** tienen derechos completos
- Non-Administrators pueden recibir derechos para modificar, acceso completo o limitado a carpetas Content
- Para acceder a File Share export/import: se necesitan roles **SystemPane** y **FileExplorerPage**

### Environment Monitoring

Accesible via **System > Tools > Environment** (solo Windows App). Permite monitorear Web Servers, Mobile Web Servers, Application Server Sets y Database Servers.

### Profiler

Herramienta para desarrolladores que captura cada evento procesado en una sesion de usuario (Business Rules, Formulas, Workspace Assemblies). Vinculado a actividad de usuario, no a nivel de aplicacion.

| Propiedad | Descripcion |
|-----------|-------------|
| **Number of Minutes to Run** | Default 20, max 60 |
| **Number of Hours to Retain** | Default 24, max 168 |
| **Include Top Level Methods** | Captura entry points de alto nivel |
| **Include Adapters** | Incluye llamadas a Data Adapter |
| **Include Business Rules** | Incluye Business Rules en el output |

Roles requeridos: **ManageProfiler** (ejecutar sesiones) y **ProfilerPage** (ver la pagina).

---

## Puntos Criticos a Memorizar

### Data Management:
- Los 6 tipos de Steps: **Calculate, Clear Data, Copy Data, Custom Calculate, Execute Business Rule, Export Data**
- Steps adicionales: **Export File, Export Report, Reset Scenario, Execute Scenario Hybrid Source Data Copy**
- Estructura: **Profile > Group > Sequence > Step**. Los Steps se asignan a Sequences. Las Sequences se organizan en Groups. Los Groups se exponen en Profiles
- **Custom Calculate** no crea auditoria, es mas rapido; requiere Manage Data Group; soporta Durable Storage
- **Reset Scenario** limpia datos, auditoria, Workflow Status y Calculation Status. Mas agresivo que Clear Data. Incluso limpia datos Durable
- La **Notification** por email se configura en la **Sequence** (no en el Step). Opciones: Not Used, On Success, On Failure, On Success or Failure
- El **Queueing** tiene como predeterminado 70% CPU y 20 minutos de espera. No poner menos de 10% CPU
- **Durable Storage**: Datos que persisten durante Calculate normal; solo se eliminan con ClearCalculated, Force con ClearCalculated, o Reset Scenario
- **Groups y Profiles NO se pueden copiar/pegar**; solo Sequences y Steps

### Excel Add-in:
- **Tres componentes**: Task Pane, OneStream Ribbon, Error Logs
- **Refresh Sheet** vs **Refresh Workbook**: Sheet solo la hoja activa; Workbook todo el archivo
- **Submit Sheet** vs **Submit Workbook**: Misma logica que Refresh
- Funciones clave: **XFGetCell** (20 params), **XFSetCell**, **XFGetCellUsingScript** (Member Script), **XFGetMemberProperty**
- **Retain Formulas in Cube View Content**: Permite mantener formulas Excel en Cube Views. Dirty cells en amarillo
- **Dynamically Highlight Evaluated Cells**: Actualiza celdas sin necesidad de Refresh. Solo disponible si Retain Formulas esta activo
- Se recomienda **Calculation Mode Manual** en Excel para mejor rendimiento
- El Add-In se registra como **COM Add-in**
- **In-Sheet Actions**: Botones para Submit, Refresh y Data Management. Parameters requieren estar resueltos en un CV/TV existente
- **Table Views**: Solo updates, no inserts. Limite ~8000 KB. Definidos por Business Rules tipo Spreadsheet
- **Error Logs**: Se eliminan automaticamente despues de 60 dias
- **Save Offline Copy**: Guarda copia sin funciones para usuarios sin Add-In
- **Add Parameter Selectors to Sheet**: Se debe seleccionar durante la creacion del Cube View Connection, no retroactivamente

### Application Properties:
- **Global Scenario y Global Time** se configuran en General Properties
- **Enforce Global POV**: Fuerza el Scenario y Time global para todos los usuarios
- **Lock after Certify**: Auto-bloqueo post-certificacion
- **Number Format**: N0-N6 para decimales; formatos personalizados con tres secciones (positivos;negativos;ceros)
- **Currencies**: Deben estar listadas para ser usadas en Entity, traduccion o tasas
- **UD1-8 Description**: Aplica al tipo de dimension, no a cada dimension individual

### Task Scheduler:
- Programa **Data Management Sequences** (no Steps individuales)
- Si la Sequence no tiene un Step, el job **fallara**
- **Frecuencias**: One Time, Minutes (5-180), Daily, Weekly, Monthly
- Roles: **TaskSchedulerPage** (ver pagina) + **TaskScheduler** (crear/editar propias) vs **ManageTaskScheduler** (administrar todas + load/extract)
- Se guarda en **UTC**; se muestra en zona horaria local
- Maximo **3 reintentos** si falla
- **No se puede cambiar el nombre** de una tarea existente (ni con TaskScheduler ni ManageTaskScheduler)
- **Administration Enabled (Enabled by Manager)**: Solo cambiable por Administrator
- En Task Activity: Task Type = "Data Management Scheduled Task"
- En Logon Activity: Client Module = "Scheduler"

### Load/Extract:
- **Application**: Metadata, Rules, Dashboards, FX Rates, Task Scheduler, etc. Es especifico de una aplicacion
- **System**: Security (Users, Groups, Roles), System Dashboards, Logs. Es a nivel de framework
- **Extract Unique IDs** en Security: **Desmarcar** al mover a otro entorno para evitar conflictos de IDs
- **Application Zip File**: Exporta todo excepto datos y FX Rates
- **Workflow Profiles Load**: Limpia propiedades antiguas no incluidas en el XML
- **Extensibility Rules**: Se exportan separadamente; otros tipos de Business Rules se exportan con su objeto asociado

---

## Escenarios de Examen Basados en Practica

### Escenario 1: Seleccion del Step correcto
**Pregunta**: Un usuario necesita ejecutar un calculo rapido tipo What-if sobre un subconjunto de datos sin generar auditoria. Cual es el tipo de Step mas apropiado?
**Respuesta**: **Custom Calculate**. Es mas rapido que Copy Data porque no genera auditoria, y permite calculos sobre un slice de datos.

### Escenario 2: Datos que desaparecen despues de Consolidate
**Pregunta**: Un Custom Calculate Step guardo datos correctamente, pero despues de ejecutar Consolidate, los datos desaparecieron. Que se debe hacer?
**Respuesta**: Asegurar que la Business Rule del Custom Calculate use **Durable Storage** (`api.Data.Calculate("formula", True)`). Sin Durable, ClearCalculatedData al inicio del Consolidate eliminara los datos.

### Escenario 3: Configuracion de Task Scheduler
**Pregunta**: Un usuario tiene el rol TaskScheduler pero no puede extraer tareas programadas via Load/Extract. Que rol necesita?
**Respuesta**: Necesita el rol **ManageTaskScheduler**, que es el unico que permite load/extract de tareas programadas.

### Escenario 4: Mover seguridad entre entornos
**Pregunta**: Al cargar Security XML de un entorno de desarrollo a produccion, se producen errores de IDs duplicados. Como se soluciona?
**Respuesta**: Al extraer Security, **desmarcar "Extract Unique IDs"** para que los IDs unicos no se incluyan en el XML. Asi, el entorno destino asigna sus propios IDs.

### Escenario 5: Formulas que se rompen en Excel
**Pregunta**: Un usuario configuro formulas en un Cube View Connection con Retain Formulas habilitado, pero las formulas se rompieron. Que pudo causar esto?
**Respuesta**: Pivotar las dimensiones existentes del Cube View o cambiar la estructura del grid (reordenar dimensiones en filas/columnas) rompe las formulas. Se debe usar un "Well-Formed Grid" con Root.List o listas separadas por comas.

### Escenario 6: Reset Scenario vs Clear Data
**Pregunta**: Cual es la diferencia principal entre Reset Scenario y Clear Data, y cuando usar cada uno?
**Respuesta**: **Clear Data** solo limpia datos especificos por Origin type (Import, Forms, Adjustments). **Reset Scenario** limpia ademas auditoria, Workflow Status, Calculation Status, y datos Durable. Usar Reset Scenario cuando se necesita un "clean slate" completo; usar Clear Data para limpieza selectiva.

---

## Mapeo de Fuentes

| Objetivo | Libro/Capitulo |
|----------|---------------|
| 201.5.1 | Design Reference Guide, Chapter 13 - Application Tools (Data Management) |
| 201.5.2 | Design Reference Guide, Chapter 24 - Excel Add-In; Chapter 13 - Application Tools (Spreadsheet) |
| 201.5.3 | Design Reference Guide, Chapter 13 - Application Tools (Application Properties) |
| 201.5.4 | Design Reference Guide, Chapter 13 - Application Tools (Task Scheduler) |
| 201.5.5 | Design Reference Guide, Chapter 13 - Application Tools (Load/Extract Application Artifacts); Chapter 19 - System Tools (Load/Extract System Artifacts) |
| 201.5.6 | Design Reference Guide, Chapter 13 - Application Tools (Load/Extract Application); Chapter 19 - System Tools (Load/Extract System) |
