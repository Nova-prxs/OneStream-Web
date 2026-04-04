# Seccion 4: Presentation (14% del examen)

## Objetivos del Examen

- **201.4.1:** Demostrar comprension de las configuraciones de Cube View
- **201.4.2:** Demostrar comprension del alcance (scope) de Workspaces
- **201.4.3:** Describir los pasos para crear un Report Book

---

## Conceptos Clave

### 201.4.1: Configuraciones de Cube View

Un Cube View se utiliza para consultar datos del cubo y presentarlos al usuario de diversas formas. Pueden ser de solo lectura, usados para edicion de datos, y utilizados como Data Source para diferentes mecanismos de visualizacion. Son los **"building blocks of reporting"** (bloques de construccion de reportes).

#### Donde se accede a los Cube Views

- Application > Presentation > Cube Views (pagina de Cube Views)
- Workflow Task (formularios)
- Workflow Analysis (seccion de analisis)
- OnePlace > Cube Views
- Spreadsheet / Excel Add-In
- Dentro de Workspaces (en Maintenance Units)

Los Cube Views se pueden incorporar en: Report Books, Dashboards, Form Templates, Spreadsheets y Extensible Documents.

#### Estructura organizativa de Cube Views

1. **Cube View Groups:** Organizan los Cube Views. Se debe crear un grupo antes de crear un Cube View.
2. **Cube View Profiles:** Organizan los grupos. Se asignan a areas de la aplicacion (ej. Workflow Profile). Un grupo puede asignarse a multiples profiles.
   - **Visibility:** Controla donde se ven los Cube Views (Always, ubicacion especifica, o Never - solo visible en la pagina Cube Views de Application tab).

**Propiedades del Cube View Group:**
- Name, Description, Workspace, Maintenance Unit
- Security: Access Group y Maintenance Group

![Cube View Toolbar con iconos de gestion](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1054-4415.png)

#### Toolbar del Cube View (Application Tab)

| Icono | Funcion |
|-------|---------|
| **Create Group** | Organizar Cube Views en grupos |
| **Create Profile** | Organizar grupos en profiles |
| **Manage Profile Members** | Asignar grupos a profiles |
| **Create Cube View** | Crear un nuevo Cube View dentro de un grupo |
| **Delete Selected Item** | Eliminar un Cube View o grupo seleccionado |
| **Rename Selected Item** | Renombrar un item seleccionado |
| **Cancel All Changes** | Cancelar cambios no guardados |
| **Save** | Guardar cambios |
| **Copy Selected Cube View** | Copiar un Cube View como template |
| **Search** | Buscar Cube Views |
| **Open Data Explorer** | Ejecutar y ver el Cube View en Data Explorer |
| **Show Objects That Reference** | Ver donde se usa el item seleccionado |
| **Object Lookup** | Buscar y seleccionar un objeto para copiar |

#### Componentes principales de un Cube View

| Componente | Descripcion |
|-----------|------------|
| **POV (Point of View)** | Define el cubo y todas las dimensiones usadas. No es necesario seleccionar dimensiones usadas en filas o columnas. Toma prioridad sobre el POV Pane. |
| **General Settings** | Row/column templates, common settings, headings, formatting, navigation links. |
| **Rows and Columns** | Determina el contenido de filas y columnas. Hasta 4 dimensiones anidadas en filas y 2 en columnas. |
| **Member Filters** | Seleccion de miembros, parametros, variables, expansiones, expresiones y business rules. |
| **Formatting** | Formato de encabezados y celdas (Data Explorer, Excel, Report). |
| **Report Header/Footer** | Controla lo que se muestra en encabezado/pie al ejecutar como Data Explorer Report. |

![Propiedades del Cube View en Application Tab](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1053-4412.png)

#### Configuracion del POV

Cada Cube View necesita una configuracion de POV para recuperar datos. Se puede:
- Ingresar informacion individualmente por campo
- Copiar el Cube POV completo (Right-click > Copy Cube POV > Paste POV)
- Arrastrar y soltar desde el POV Pane
- **Importante:** La informacion en el POV slider tiene prioridad sobre el POV Pane.

![Configuracion del POV en el Cube View editor](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1059-4443.png)

**Pasos para configurar el POV:**
1. En la pagina Cube Views, bajo Cube View Groups, seleccionar un Cube View.
2. Seleccionar el POV slider.
3. Ingresar informacion individualmente o mover el Cube POV completo.
4. Para mover todo el Cube POV: Right-click Cube POV > Copy Cube POV > Right-click header > Paste POV.

#### Filas y Columnas (Rows and Columns)

- Usar `+` para agregar y `-` para eliminar filas/columnas.
- Se pueden aplicar Member Filters, formatting, data suppression y overrides a nivel de fila y columna.
- **Sharing de filas/columnas:** Se pueden compartir todas o filas/columnas especificas desde otro Cube View (General Settings > Sharing). Los cambios se enlazan automaticamente.

![Grid de preview de Rows and Columns](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1060-4446.png)

**Opciones de Sharing de Rows/Columns:**
- **All Rows/Columns:** Ingresar el nombre del Cube View fuente cuyos rows/columns se compartiran.
- **Specified Rows/Columns:** Referenciar un solo row o column del Cube View fuente.

**Cube View Templates:** Crear un grupo especifico para templates (ej. "Templates_Columns") y copiar Cube Views para reutilizar formato y propiedades. Cada Cube View debe tener un nombre unico.

#### Propiedades de Data en Rows/Columns

| Propiedad | Descripcion |
|----------|------------|
| **Can Modify Data** | Si False, filas/columnas son read-only. Si True, el setting de False del Cube View lo sobreescribe. |
| **Text Box** | Valor numerico por defecto para celdas de datos |
| **Combo Box** | Usa un List Parameter para generar dropdown en la celda |
| **Date** | Habilita calendario en la celda para seleccionar fecha |
| **Date Time** | Habilita calendario y hora en la celda |
| **List Parameter** | Nombre del parametro que genera la lista dropdown |
| **Enable Report Navigation Link** | (Solo filas) Habilita Navigation Link Drill Downs en dashboards |
| **Dashboard to Open in Dialog** | (Solo filas) Nombre del dashboard o parametro para el Navigation Link |
| **Linked Cube Views** | Lista de Cube Views accesibles via right-click en las celdas |

#### Member Filter Builder

Herramienta que simplifica la construccion de Member Filters complejos. Se accede desde Rows and Columns > seleccionar fila/columna > Member Filters tab.

![Member Filter Builder dialog completo](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1064-4458.png)

**Componentes del Member Filter Builder:**

| Pestana | Funcion |
|---------|---------|
| **Member Filter** | Campo donde se construye el filtro |
| **Dimension Tokens** | Boton por cada dimension que lanza el dialogo de seleccion |
| **Member Expansion Functions** | Doble clic para agregar (ej. Children, Descendants, TreeDescendants) |
| **Where Clause** | Propiedades para completar la expresion (ej. Name Contains, HasChildren) |
| **Time Functions** | Solo para dimension Time (ej. `T#POVPrior1`). Tipos: POV, WF, Global, General |
| **Variables** | Substitution Variables del sistema (ej. `|POVTime|`, `|WFScenario|`, `|UserName|`) |
| **Samples** | Sintaxis de ejemplo para consultas complejas |
| **Expansion** | Expansiones comunes agregadas al final de filtros |
| **Workflow** | Expansiones de miembros de workflow usadas en Cube Views vinculados a workflow |
| **Other** | Funciones de member filter para filas/columnas calculadas o parametros personalizados |

**Sintaxis de Member Filters:**
- Miembro simple: `T#2022`
- Con expansion: `A#[Income Statement].Descendants`
- Multiples dimensiones separadas por `:` -> `Cb#GolfStream:E#Houston:S#Budget:T#2022M3`
- Multiples scripts separados por `,` -> `S#Actual, S#Budget, S#Forecast`

**Member Expansions destacadas:**
- `Children`, `ChildrenInclusive`, `Descendants`, `TreeDescendants`
- Variantes Reverse: `ChildrenInclusiveR`, `TreeDescendantsR`
- `Where` clauses: filtrar por Text Properties, Name Contains/StartsWith/EndsWith, HasChildren, AccountType, IsIntercompany, In Use property, Has Member Formula, Security, Specific Currency
- `Remove` functions: eliminar miembros del resultado
- `Parents`, `Ancestors`

**Where Clauses disponibles:**
- Text Properties
- Porciones del nombre/descripcion (StartsWith, Contains, EndsWith)
- Security
- Account Types (solo dimension Account)
- Intercompany (dimensiones Entity y Account)
- Specific Currency (solo dimension Entity)
- In Use property
- HasChildren (solo para dimensiones asignadas al cubo en el Scenario Type por defecto)
- Has Member Formula

**Time Functions importantes:**
- `T#POVPrior1` - Periodo anterior del POV
- `T#2022.Months` - Todos los meses de 2022
- `T#YearPrior1(|PovTime|)PeriodNext1(|PovTime|)` - Ano anterior, periodo siguiente
- Time Functions pivotan el miembro; Time Expansions extienden el miembro

**Substitution Variables comunes:**

| Variable | Descripcion |
|----------|------------|
| `|UserName|` | Nombre de usuario que ejecuto el reporte |
| `|CVName|` | Nombre del Cube View |
| `|POVTime|`, `|WFTime|`, `|GlobalTime|` | Miembro de tiempo segun fuente |
| `|WFTimeDesc|` | Descripcion (ej. "Feb 2022") |
| `|WFTimeShortDesc|` | Descripcion corta (ej. "Feb") |
| `|DateDDMMYYYY|` | Fecha actual |
| `|Text1|` | Propiedades de texto del miembro |
| `|WFProfile|` | Nombre del Workflow Profile actual |

Los prefijos indican de donde se toma el valor: CV (Cube View POV), WF (Workflow), POV (Cube POV), MF (Member Filter), Global. Agregar `Desc` como sufijo para mostrar la descripcion.

#### Rename de filas/columnas

Usar la funcion `:Name()` al final de un Member Filter:
- `A#CashBalance:Name(Cash Balance)` muestra "Cash Balance"
- Las comillas dobles alrededor del nombre son opcionales: `A#60999:Name("Net Sales")` o `A#60999:Name(Net Sales)`
- Con XFMemberProperty: `A#6009.base:Name(XFMemberProperty(DimType=Account, Member=|MFAccount|, Property=AccountType))`

#### Cube View Performance

**Database Sparsity:** Es la relacion entre volumen de registros y dimensiones modeladas. La ausencia de registros de datos afecta el rendimiento porque es dificil renderizar reportes con datos dispersos. Usar Analytic Blend, extensibility u otros frameworks para minimizarla.

**Row and Column Suppression:**

![Opciones de supresion de filas y columnas](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1073-4482.png)

| Propiedad | Descripcion |
|----------|------------|
| Suppress Invalid Rows/Columns | Suprime celdas invalidas |
| Suppress NoData Rows/Columns | Suprime celdas sin datos (poner False para entrada de datos) |
| Suppress Zero Rows/Columns | Suprime celdas con cero |
| Use Suppression Settings on Parent Rows/Columns | Controla si miembros parent usan la misma configuracion |
| Zero Suppression Threshold | Valor umbral; numeros debajo se tratan como cero (ej. 499.99 suprime todo menor) |
| Allow Insert Suppressed Member | Opciones: All, False, Nested, Innermost (solo filas) |
| Use to Determine Row Suppression | (Solo columnas) True mejora rendimiento en Cube Views grandes |
| Allow Sparse Row Suppression | (Solo columnas) Mejora rendimiento con multiples dimensiones anidadas en filas |

**Valores de Allow Insert Suppressed Member:**
- **All:** Visibilidad a todas las expansiones de filas del Cube View
- **False:** Todas las expansiones permanecen suprimidas
- **Nested:** Visibilidad de las expansiones de filas 2 a 4
- **Innermost:** Visibilidad de la expansion de fila en el nivel mas bajo

**Sparse Row Suppression:**
- Evalua registros de datos y filtra registros sin datos ANTES de renderizar el Cube View.
- Se habilita en General Settings > Common > `Allow Sparse Row Suppression = True`.
- **No puede aplicarse a datos calculados dinamicamente** (miembros calculados y Cube View math).
- Para columnas con datos dinamicos: establecer `Use to Determine Row Suppression = True` y `Allow Sparse Row Suppression = True`.
- Cualquier fila con un setting de supresion habilitado sera elegible para sparse row suppression. Si no se aplica supresion, sparse row suppression no se aplicara.

**Pasos para habilitar Sparse Row Suppression:**
1. Ir a Cube Views > seleccionar Cube View.
2. General Settings > Common > Suppression > `Allow Sparse Row Suppression = True`.
3. Seleccionar Rows and Columns > seleccionar la fila > tab Data.
4. Configurar las propiedades de supresion adicionales en True.
5. Seleccionar la columna > tab Data > `Allow Sparse Row Suppression = True`.
6. Si la columna tiene datos dinamicos, establecer `Use to Determine Row Suppression = True`.

**Cube View Paging:**

Se aplica solo al Data Explorer view cuando hay mas de 10,000 filas sin suprimir.

| Evaluacion | Descripcion |
|-----------|------------|
| **Evaluacion 1 - Enable Paging** | Evalua el total de filas potenciales sin suprimir. Si < 10,000, no se habilita paging. |
| **Evaluacion 2 - Paging Enabled** | Si >= 10,000 filas, se habilita paging. |
| **Evaluacion 3 - Paging** | Intenta devolver de 20 a 2,000 filas sin suprimir en un maximo de 20 segundos. |

Propiedades:
- `Max Unsuppressed Rows Per Page` (default -1, max 100,000): Determina cuantas filas se escriben antes de paginar.
- `Max Seconds To Process` (default -1, max 600): Determina cuantos segundos procesa antes de paginar.

Cuando hay dimensiones anidadas en filas, la evaluacion de paging se realiza en la dimension mas a la izquierda. El porcentaje mostrado no es una medicion precisa porque cada pagina se genera por tiempo de procesamiento.

#### Cube View General Settings importantes

| Propiedad | Descripcion |
|----------|------------|
| **Is Visible in Profiles** | Si es True, el Cube View es visible en el Profile; si False, solo en la pagina Cube Views |
| **Page Caption** | Texto en la parte superior del Data Explorer grid |
| **Is Shortcut** | Determina si es un shortcut a otro Cube View |
| **Is Dynamic** | Permite manipular formato y datos programaticamente en runtime via Workspace Assembly |
| **Can Modify Data** | True = editable en OnePlace; False = solo lectura |
| **Can Calculate/Translate/Consolidate** | True, True (Without Force), o False |
| **Can Modify Suppression** | Si True, el usuario puede cambiar la supresion en Data Explorer |

#### Cube View con Workflow

- **Workflow Entities:** Usar `E#Root.WFProfileEntities` en Rows/Columns (no en POV, porque puede haber mas de una entidad). WFProfileEntities muestra la entidad o entidades asignadas a un Workflow Profile en runtime.
- **Workflow Scenario:** Seleccionar WF Member en el POV Slider para Scenario, o usar `|WFScenario|` en Rows/Columns.
- **Workflow Time:** Seleccionar WF Member en el POV Slider para Time, o usar `|WFTime|` en Rows/Columns.

#### Cube View con Parameters

- Referir parametros en POV y Rows/Columns: `|!ParameterName!|`
- Si un parametro no se encuentra asociado al Form, OneStream busca en los Dashboard Parameters de la aplicacion.
- Los parametros se usan como filtros para enfocar los datos mostrados.

![Ejemplo de uso de parametros en el POV del Cube View](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1081-4507.png)

#### Cube View Shortcuts

Permiten abrir el mismo Cube View con diferentes valores de parametros sin mantener multiples Cube Views. Ejemplo: Income Statement con `ParamView = [YTD]` y otra version con `ParamView = [Periodic]`. Cada shortcut tiene el mismo nombre pero diferente valor de parametro literal.

#### Formatting de Cube Views

**Orden de prioridad del formato:**
1. Application Properties (standard report settings)
2. Cube View Default settings
3. Column settings
4. Row settings
5. Column Row overrides
6. Row Column overrides

La combinacion de formatos y overrides determina el formato final de la celda.

![Opciones de formatting en el tab de Formatting](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1083-4519.png)

**Salidas de formato:** Data Explorer, Excel, Report - cada uno con opciones especificas.

**Data Explorer Formatting:**
- Text: Font, color, size, bold, italic, NumberFormat, ZeroOffset, Scale, FlipSign, ShowPercentageSign, ShowCurrency
- Border: Background color, gridline color
- Column: IsColumnVisible, ColumnWidth

**Excel Formatting:**
- Text: Color, horizontal/vertical alignment, indent level, wrap, number format, use scale
- Border: Background color, border color/line styles
- Column: Column width

**Report Formatting:**
- Text: Color, alignment, size, underline, NoDataNumberFormat, UseNumericBinding
- Border: Background color, cell borders
- Lines: Top/bottom lines, padding, color, thickness
- Column: Column width, Row height

**Opciones de Cell Format:**

| Propiedad | Descripcion |
|----------|------------|
| NumberFormat | Formato numerico .NET (ej. `#,### ;(#,###);0` para positivo;negativo;nulo) |
| ZeroOffsetForFormatting | Relacionado con NegativeTextColor; permite mostrar numeros < X en rojo |
| Scale | -12 a +12 (3 = miles, 6 = millones) |
| FlipSign | Invierte positivo/negativo para visualizacion |
| ShowPercentageSign | Muestra simbolo de porcentaje |
| ShowCurrency | Muestra codigo de moneda (ej. EUR) |
| NegativeTextColor | Color para numeros negativos |
| WritableBackgroundColor | Color de fondo para celdas editables |
| ExcelNumberFormat | Formato numerico especifico para exportacion a Excel |
| ExcelUseScale | Si Excel usa la propiedad Scale |
| ReportNoDataNumberFormat | Formato para celdas NoData en reportes (ej. "NODATA" o "#" para vacio) |
| ReportUseNumericBinding | True = numeros en vez de texto al exportar reporte a Excel |

**Header Format detallado:**

| Propiedad | Descripcion |
|----------|------------|
| RowExpansionMode | Controla expansion de filas anidadas en Data Explorer |
| ShowDimensionImages | False oculta iconos de dimension en headers |
| HeaderWrapText | True = wrap de texto en headers de filas y columnas |
| IsColumnVisible | False oculta columnas especificas; se puede usar con parametro en runtime |
| ColumnWidth | Ancho de columna en pixeles |
| ColumnHeaderWrapText | True = wrap en headers de columna individuales (requiere ColumnWidth) |
| MergeAndCenterColumnHeaders | True = centrar headers de columnas |
| IsRowVisible | False oculta filas especificas |
| TreeExpansionLevel1-4 | Expande automaticamente hasta 4 niveles anidados (0 = colapsar todo) |
| RowHeaderWrapText | True = wrap en headers de fila individuales (requiere Row Header Width) |
| ExcelMaxOutlineLevelOnRows/Cols | Hasta 6 niveles de outline para Excel |
| ExcelExpandedOutlineLevelOnRows/Cols | Nivel de expansion inicial al exportar a Excel |
| ReportRowPageBreak | Aplica salto de pagina en reportes |

![Ejemplo de TreeExpansionLevel con niveles 1, 2 y 3 expandidos](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1093-4563.png)

**TreeExpansionLevels:** Estan vinculados a los Dimension Types de Member Filter en el tab Designer y requieren una expansion de miembro .Tree. Para que funcione, RowExpansionMode del Default Header debe estar en "Use Default". Usar Collapse All o Expand All sobreescribe este setting.

**Conditional Formatting:**

Se aplica a Default, Headers, Cells, Row/Column overrides. Sigue el mismo orden de operaciones que el formato basico.

![Dialogo de Conditional Formatting con secciones de condition, filter, operator y text](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1106-4601.png)

**Header Property Filters:**

| Filtro | Descripcion |
|--------|------------|
| IsRowHeader | Boolean, si es header de fila |
| IsColHeader | Boolean, si es header de columna |
| RowName | Nombre de la fila del Cube View |
| ColName | Nombre de la columna del Cube View |
| ExpansionLevel | Nivel de expansion para filas (1-4) y columnas (1-2) |
| HeaderDisplayText | Descripciones personalizadas con :Name() |
| MemberName | Etiquetas de miembros de metadata |
| MemberDescription | Descripciones de miembros de metadata |
| MemberShortDescription | Descripciones cortas (solo dimension Time) |
| IndentLevel | Nivel de indentacion (derivado de formato o generado por tree expansions) |

**Cell Format Property Filters:**

| Filtro | Descripcion |
|--------|------------|
| IsNoData | Prueba si no hay datos |
| IsRealData | Prueba datos almacenados, ignorando datos derivados Zero-View |
| IsDerivedData | Prueba datos derivados (comunmente de Scenario Zero-View) |
| IsRowNumberEven | Prueba si el numero de fila es par |
| ExpandedRowNum | Cuenta de filas expandidas (base cero, total del Cube View) |
| CellAmount | Prueba el monto de datos de la celda |
| CellStorageType | Prueba el metodo de almacenamiento de datos |

Se puede guardar formato condicional como Literal Parameter para reutilizar. Soporta If/ElseIf/Else para logica compleja.

**Ejemplo de Conditional "Traffic-Lighting":** Se aplica como Cell Format. El disenador elige si aplicar a fila o columna. Row overrides son la capa final de formato.

**Ejemplo de IndentLevel:** El filtro IndentLevel formatea dinamicamente desde filas o expansiones definidas. La indentacion es base cero.

![Ejemplo de IndentLevel conditional formatting](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1111-4613.png)

#### Data Explorer - Funcionalidades en OnePlace

![Cube View en OnePlace con toolbar](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1953-7353.png)

**Toolbar del Cube View:**
- **Consolidate:** Consolida los datos del Cube View
- **Translate:** Traduce los datos del Cube View
- **Calculate:** Ejecuta calculo en los datos del Cube View
- **Row Suppression:** Use Default, Suppress Rows, Unsuppress Rows
- **Show Report:** Formato pulido tipo PDF
- **Export to Excel:** Exporta el Cube View formateado a Excel (con numeracion secuencial)
- **Select Parameters:** Seleccionar nuevos parametros
- **Edit Cube View:** Lanzar Application tab para modificar (segun seguridad)
- **Find Next Row:** Buscar fila por keyword (funciona con filas colapsadas)

**Shortcuts de entrada de datos:**

| Shortcut | Funcion | Ejemplo |
|----------|---------|---------|
| `add` | Agregar un numero al valor | `add2k` agrega 2,000 |
| `sub` | Restar | `sub500` resta 500 |
| `div` | Dividir | `div2` divide por 2 |
| `mul` | Multiplicar | `mul3` multiplica por 3 |
| `in` / `increase` / `gr` | Incrementar por porcentaje | `in10` incrementa 10% |
| `de` / `decrease` | Decrementar por porcentaje | `de5` decrementa 5% |
| `per` / `percent` | Calcular porcentaje | `per50` calcula 50% |
| `pow` / `power` | Calcular potencia | `pow2` eleva al cuadrado |

**Scaling de entrada:** `k` = x1,000; `m` = x1,000,000; `b` = x1,000,000,000; `%` = dividir por 100

**Hotkeys:** CTRL+S = Guardar; CTRL+C/V = Copiar/Pegar valores en celdas (seleccionar multiples celdas con CTRL)

**Right-Click Options:**

**Expand/Collapse:** Si se usan filas anidadas, right-click en header de fila para expandir/colapsar. Funciona con RowExpansionMode.

**Calculate/Translate/Consolidate:** Se pueden habilitar/deshabilitar individualmente en cada Cube View. Incluye opciones Force y con Logging.

**Spreading:**

![Ejemplo de Accumulate Spreading con rate 1.5](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1960-7382.png)

| Tipo | Descripcion |
|------|------------|
| **Fill** | Llena cada celda seleccionada con el valor en Amount to Spread |
| **Clear Data** | Limpia todos los datos de las celdas seleccionadas |
| **Factor** | Multiplica el valor de la celda por el rate especificado |
| **Accumulate** | Toma el valor de la primera celda, lo multiplica por el rate, y aplica ese resultado a la siguiente celda sucesivamente |
| **Even Distribution** | Distribuye el Amount to Spread de forma pareja entre celdas seleccionadas |
| **Proportional Distribution** | Distribuye proporcionalmente segun los valores existentes en las celdas |
| **445 Distribution** | Peso de 4 a las primeras 2 celdas, peso de 5 a la tercera |
| **454 Distribution** | Peso de 4, 5, 4 respectivamente |
| **544 Distribution** | Peso de 5, 4, 4 respectivamente |

**Propiedades de Spreading:**
- **Amount to Spread:** Valor a distribuir (default = ultima celda seleccionada)
- **Rate:** Solo para Factor y Accumulate
- **Retain Amount in Flagged Input Cells:** True = spreading no aplica a celdas flagged
- **Include Flagged Read only Cells in Totals:** True (default) incluye celdas locked en totales
- **Flag Selected Cells:** Retiene el valor original durante spreading
- **Clear Flags:** Limpia celdas flagged

El dialogo de Spreading puede dejarse abierto durante data entry. Para seleccionar multiples celdas, usar CTRL+click. El spreading ocurre primero a traves de filas y luego hacia abajo en columnas. Cuando se hace spreading en time periods, se puede hacer doble clic en el Parent Time Member (ej. Q1) para seleccionar automaticamente sus periodos base.

![Ejemplo de spreading en Q1 con periodos base](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1964-7397.png)

**Allocation:**

| Tipo | Descripcion |
|------|------------|
| **Clear Data** | Limpia datos de Form para el Destination POV especificado |
| **Even Distribution** | Distribucion pareja entre Destination Members |
| **445/454/544 Distribution** | Misma logica que Spreading pero para allocations |
| **Weighted Distribution** | Aplica pesos calculados a cada Destination Member |
| **Advanced** | Similar a Weighted pero permite override de 2 dimensiones destino y control de pesos |

**Propiedades de Allocation:**
- **Source POV:** Determina la interseccion fuente (default = ultima celda seleccionada). Se puede drag-and-drop desde celda.
- **Source Amount or Calculation Script:** Override del valor Source POV (ej. `A#SourcePOV*0.90`)
- **Destination POV:** Interseccion destino para la allocation. Se requiere un miembro por cada dimension.
- **Dimension Type/Dimension Type 2:** Dimensiones adicionales no incluidas en Destination POV (hasta 2 para Clear Data y Advanced)
- **Member Filter/Member Filter 2:** Miembros especificos que override el Destination POV
- **Save Zeroes as Not Data:** True (default) para suprimir ceros al guardar
- **Weight Calculation Script:** Script para calcular pesos usando `|SourceAmount|`, `|Weight|`, `|TotalWeight|`
- **Destination Calculation Script:** Como calcular el Weight (default: `|SourceAmount| * (|Weight|/|TotalWeight|)`)
- **Translate Destination If Different Currency:** True para traducir si la moneda destino es diferente

**Offset en Allocations:**
- **Source Transfer POV:** Entrada de transferencia que zerifica el Source Amount
- **Source Transfer Offset POV:** Entrada de balance en el Source (usualmente diferente Account)
- **Destination Offset POV:** Entrada de balance en el Destination

![Ejemplo de allocation con offset entries](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1970-7415.png)

**Data Attachments:**
- Se agregan a nivel de celda o Data Unit completo
- Tipos: Standard, Annotation, Assumptions, Audit Comment, Footnote, Variance Explanation
- Puede haber muchos Standard attachments por celda, pero solo uno de cada otro tipo
- Los tipos no-Standard son parte de la View Dimension y pueden mostrarse en filas/columnas del Cube View
- **Spell Check:** Disponible solo en Windows Application, solo en ingles, via right-click en Data Attachments

![Data Attachments dialog](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1975-7432.png)

**Cell Detail:**
- Disponible en O#Forms o O#BeforeAdj
- Se puede cargar via Excel/CSV template
- Incluye: Amount, Aggregation Weight (ej. -1 para invertir), Classification (via Dashboard Parameter `CellDetailClassifications`), Description
- **Apply Import Offset:** Para presupuestos, aplica el reverso del Origin Member Import
- **Remove Import Offset:** Elimina un Import Offset previamente aplicado

![Cell Detail toolbar y form](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1980-7450.png)

**Cell POV Information:** Hover sobre tick mark muestra miembros del POV en tooltip. Right-click > Cell POV Information muestra resumen detallado con formulas XFGetCell y XFCell copiables.

![Cell POV Information dialog](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1983-7461.png)

**Cell Status:** Right-click > Cell Status muestra propiedades de status e informacion dimensional de la celda.

**Data Unit Statistics:** Right-click > Data Unit Statistics muestra: celdas cero, real, derivadas, NODATA, calculadas, consolidadas, traducidas, journal, input, etc.

**Drill Down:**

![Drill Down con resultados y bread crumb trail](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1987-7473.png)

Se puede hacer drill desde cualquier celda; **NO necesita ser base level**. Celdas blancas = base amounts. Celdas verdes = se puede seguir drilling.

| Opcion | Descripcion |
|--------|------------|
| **Entity Children Contribution** | (Solo Entity) Muestra contribucion de cada Entity al Parent |
| **Local Currency** | (Solo Consolidation) Muestra Entities en moneda local |
| **Member Children** | Drill a los children del miembro |
| **Base** | Drill a celdas base (blancas) |
| **All Aggregated Data Below Cell** | Drill a celdas blancas en cada dimension |
| **All Stored Data in Data Unit** | Drill a todos los datos base del Data Unit completo |
| **Copy POV from Data Cell** | Copia el POV de la celda seleccionada |
| **Create Quick View Using POV** | Crea Quick View basado en el POV de la fila seleccionada |
| **Cell POV Information** | Ver POV completo de la celda |
| **Cell Status** | Informacion sobre la celda (hijos, calculada, lock status) |
| **Calculation Inputs** | Detalles de formula source accounts |
| **Load Results for Imported Cell** | Drill back a datos fuente importados |
| **Audit History for Forms/Adjustment Cell** | Drill back a datos de Form o Journal |

**Origin Audit Drill Down:** Right-click Origin cell (Top) > Origin Base revela datos por cada Origin Member. Luego se puede drill en Import, Forms, o AdjInput (Journals).

**Show Cube View As Report:** Genera reporte formateado tipo PDF. Controlar anchos de columna y alturas de fila desde el Cube View. Se puede exportar a PDF, HTML, RTF, CSV, Text, XPS, MHT o Excel.

![Cube View como Report formateado](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1992-7488.png)

---

### 201.4.2: Alcance (Scope) de Workspaces

#### Que es un Workspace?

Un Workspace es un espacio de desarrollo estructurado tipo sandbox donde los equipos pueden crear, modificar y probar soluciones sin interferir con otros flujos de trabajo. Almacenan **Maintenance Units**, que sirven como contenedores organizativos que agrupan componentes, configuraciones y mejoras relacionadas.

![Ejemplo de organizacion de Workspaces por area funcional](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p30-796.png)

#### Proposito principal

- Facilitar colaboracion y desarrollo eficiente entre equipos, aplicaciones y ambientes
- Promover modularidad y reutilizacion de soluciones
- Actuar como contenedores de desarrollo controlados que preservan la integridad de la aplicacion
- Permitir experimentar sin impactar sistemas de produccion
- Acelerar ciclos de desarrollo y mejorar eficiencia

#### Estructura jerarquica

```
Workspace
  -> Maintenance Units
       -> Dashboard Groups
            -> Dashboards
                 -> Components, Data Adapters, Parameters, Files, Strings
       -> Cube View Groups
            -> Cube Views
       -> Data Management Groups
            -> Sequences -> Steps
       -> Assemblies
```

![Estructura del Workspace con Maintenance Units, Dashboard Groups, Cube View Groups y Data Management Groups](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p45-991.png)

- **Default Workspace:** Contiene todos los objetos legacy (pre-Workspaces). Los Cube View Groups y Data Management Groups que no estan en un Workspace nuevo se ubican bajo el Default Maintenance Unit del Default Workspace.
- **Dashboard Profiles y Cube View Profiles** son globales (no especificos de un Workspace) y permanecen en la misma ubicacion.

![Profiles globales en la estructura del Workspace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p45-989.png)

#### Donde se encuentran los Cube Views (3 ubicaciones)

1. Default Workspace > Default Maintenance Unit (Cube Views legacy)

![Cube Views legacy bajo Default Workspace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p59-1104.png)

2. Default Workspace > Maintenance Unit especifico (nuevos Cube Views en Default)

![Cube Views en Maintenance Unit especifico del Default Workspace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p60-1109.png)

3. Non-Default Workspace > Maintenance Unit especifico (Cube Views de nuevos Workspaces)

![Cube Views en Workspace no-default](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p60-1112.png)

**Importante:** Cube Views creados fuera del Default Workspace NO se pueden acceder desde la pagina Cube View de Application tab.

#### Propiedades del Workspace

**General:**

| Propiedad | Descripcion |
|----------|------------|
| **Name** | Nombre del Workspace. Convenciones: por grupo de desarrollo, funcionalidad, workstream, persona, o equipo. |
| **Description** | Contexto adicional sobre el proposito del Workspace. |
| **Notes** | Campo libre para documentar fechas de finalizacion, estado, recordatorios. |
| **Substitution Variable Items** | Variables reutilizables dentro del Workspace. Se accede via (Collection) con boton de elipsis. |

**Substitution Variables del Workspace:**
- Sintaxis: `|WSSVNombreSufijo|` (prefijo `WSSV` seguido del nombre del sufijo)
- Ejemplo: Variable con sufijo `Developer` -> se referencia como `|WSSVDeveloper|`
- Se crean desde el dialogo Substitution Variable Items: click Add Item, ingresar Suffix y Value
- Se usan en dashboards, Page Caption, y otros objetos del Workspace

![Dialogo de Substitution Variable Items del Workspace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p53-1043.png)

![Dashboard mostrando valores de substitution variables en el Page Caption](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p56-1080.png)

**Security (dos niveles):**
- **Access Group:** Usuarios que pueden ver el Workspace y sus dashboards, pero NO modificar. Texto en gris = read-only.
- **Maintenance Group:** Usuarios que pueden acceder, ver Y modificar el Workspace y sus objetos. Dependiendo de la estructura de seguridad, usuarios pueden pertenecer a multiples grupos o grupos anidados.

![Configuracion de Security en el Workspace - Access y Maintenance Groups](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p54-1058.png)

**Sharing:**

| Propiedad | Descripcion |
|----------|------------|
| **Is Shareable Workspace** | Si es True, otros Workspaces pueden reutilizar objetos de este Workspace. Facilita sharing "outward". |
| **Shared Workspace Names** | Lista separada por comas de Workspaces de los que se desea reutilizar objetos. Facilita sharing "inward". Ordenar la lista segun como se quiere que aparezcan en busquedas. |

![Propiedad Is Shareable Workspace configurada como True](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p26-767.png)

![Propiedad Shared Workspace Names con tooltip](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p27-776.png)

**Requisitos para compartir objetos entre Workspaces:**
1. El Workspace fuente debe tener `Is Shareable Workspace = True`
2. El Workspace destino debe listar el nombre del Workspace fuente en `Shared Workspace Names`
3. El Default Workspace siempre esta compartido con cualquier otro Workspace, sin necesidad de listarlo

**Assemblies:**

| Propiedad | Descripcion |
|----------|------------|
| **Namespace Prefix** | Nombre corto para referenciar el Workspace en Assemblies. Permite escribir syntax que ejecuta codigo de otros Workspaces. |
| **Imports Namespace 1-8** | Referencia a Assemblies de otros Workspaces. Cuando un Assembly depende de otro Workspace, estas propiedades permiten referenciar dinamicamente esa dependencia. |
| **Workspace Assembly Service** | Nombre del Assembly Service Factory (`AssemblyName.FactoryName`). FactoryName es el nombre de la clase (sin extension). |

**Text 1-8:** Campos para almacenar valores string referenciables desde Assemblies. Permite que los Assemblies sean mas dinamicos sin necesidad de modificar el codigo cuando el valor cambia.

Ejemplo de referencia en Assembly:
```
Dim WSText1 As String = BRApi.Dashboards.Workspaces.GetWorkspace(si, False, args.PrimaryDashboard.WorkspaceID).Text1
```

#### Naming Conventions en Workspaces

Se puede usar el **mismo nombre de objeto** multiples veces, siempre que esten en Workspaces separados. Esto aplica a: Maintenance Units, Dashboards, Cube Views, Data Management Jobs, Components, Data Adapters, Files, Strings y Assemblies.

#### Security Roles para Workspaces

| Security Role | Tipo | Permiso |
|--------------|------|---------|
| **AdministerApplicationWorkspaceAssemblies** | Application Security Role | Crear y modificar Workspace Assemblies |
| **ManageApplicationWorkspaces** | Application Security Role | Crear y modificar Workspaces. Se puede limitar la creacion a ciertos usuarios. |
| **WorkspaceAdminPage** | Application UI Role | Acceso a la pagina Workspaces desde Application tab |

![Security Roles de Workspace en Application Security Roles](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p50-1017.png)

#### Pasos para crear un Workspace

1. Navegar a Application tab > Presentation > Workspaces.
2. Hacer click en Workspaces y luego click en Create Workspace.
3. Escribir el nombre del Workspace y hacer click en Save.
4. Configurar Description y Notes (opcional).
5. Configurar Substitution Variables (opcional).
6. Asignar Access Group y Maintenance Group.
7. Configurar Is Shareable Workspace y Shared Workspace Names segun necesidad.
8. Guardar.

![Pasos de creacion de Workspace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p52-1032.png)

Al renombrar Workspaces, verificar el inventario de Workspaces que listen su nombre como Shared Workspace para actualizar las referencias.

#### Ejemplo de configuracion de seguridad

| Grupo | Nombre | Rol | Workspace |
|-------|--------|-----|-----------|
| DeveloperGroup1 | Eric Telhiard | Maintenance | Annual Operating Plan |
| DeveloperGroup2 | Jessica Toner | Maintenance | Close and Consolidation |
| DevelopmentQA | Tom Shea | Access | Ambos Workspaces |

![Usuario de Maintenance viendo solo su Workspace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p34-833.png)

![Usuario de QA con Access (read-only) viendo ambos Workspaces](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p35-838.png)

Cada developer solo ve los Workspaces donde tiene derechos. El usuario QA (Access) ve ambos Workspaces pero no puede modificar nada (texto en gris = read-only).

#### Beneficios clave de Workspaces

| Beneficio | Descripcion |
|-----------|------------|
| **Desarrollo aislado** | Sandboxes dedicados sin impactar dashboards existentes |
| **Resuelve conflictos de nombres** | Mismo nombre de objeto en diferentes Workspaces |
| **Elimina incertidumbre de ambientes** | No mas guesswork al migrar entre sandbox y produccion |
| **Empaquetado de soluciones** | Exportar/importar un Workspace completo como un solo archivo XML |
| **Desarrollo paralelo** | Multiples desarrolladores trabajan simultaneamente sin disrupciones |
| **QA en tiempo real** | Pruebas dentro de la misma instancia sin migracion de ambientes |
| **Compartir selectivamente** | Solo contenido marcado como shareable se comparte |
| **Retrocompatibilidad** | No requieren reconstruir soluciones existentes |

![Empaquetado de solucion - Exportar un solo item Application Workspaces](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p29-789.png)

#### Workspace vs Traditional Development

| Aspecto | Tradicional | Workspaces |
|---------|------------|------------|
| Migracion | Requiere tomar app offline, copiar BD de sandbox a produccion | Desarrollo, testing y deploy dentro de la misma instancia |
| Disrupciones | Usuarios bloqueados durante update | Desarrollo paralelo sin afectar operaciones |
| Exportacion | Multiples objetos individuales (Cube Views, DM jobs, etc.) | Un solo archivo XML con toda la solucion |
| Conflictos de nombres | Requiere nombres unicos en toda la aplicacion | Permite nombres duplicados en Workspaces separados |
| QA | Requiere migracion a otro ambiente | Pruebas en la misma instancia con refresh |

#### Workspace Filter Functionality

Los Workspace Filters permiten a los usuarios refinar los Workspaces mostrados en sus selecciones de filtro, facilitando la navegacion y organizacion. Los desarrolladores pueden enfocarse en sus Workspaces dedicados sin ver otros Workspaces.

![Workspace Filter Functionality](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p56-1079.png)

#### Dashboard Types

| Tipo | Descripcion |
|------|------------|
| **Use Default** | Configuracion estandar por defecto |
| **Top Level** | Dashboard expuesto en el nivel superior del menu OnePlace. Controla cual dashboard es visible en el menu principal. |
| **Top Level Without Parameter Prompts** | Top Level sin solicitar parametros al usuario. Util para mostrar vista especifica sin dar opcion de modificar parametros. |
| **Embedded** | Dashboard anidado dentro de otro dashboard (no visible directamente en OnePlace). Mas comun para crear dashboards jerarquicos. |
| **Embedded Dynamic** | Para objetos definidos en Workspace Assemblies |
| **Embedded Dynamic Repeater** | Muestra multiples instancias de un objeto usando Component Template Repeat Items |
| **Custom Control** | Usa dashboard templates con Event Listeners |

![Dashboard Type configurado como Top Level](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p40-867.png)

#### Componentes de Workspaces

**Maintenance Units:** Grupos de dashboards y componentes. Permiten separacion de workstreams.

![Workspace con cuatro Maintenance Units](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p58-1097.png)

**Dashboard Groups:** Dashboards agrupados por funcionalidad u otro criterio. El dashboard top-level se expone en OnePlace via dashboard profile.

**Data Management Groups:** Automatizan tareas (cargar datos, ejecutar calculos, activar business rules).
- **Steps:** Bloques individuales de proceso.
- **Sequences:** Series ordenadas de uno o mas steps que se ejecutan en orden. El nombre de la Sequence se usa para trigger actions desde componentes de dashboard.

**Data Adapters (5 tipos):**

| Tipo | Descripcion |
|------|------------|
| **Cube View** | Cube View preconfigured como data source (mas comun). Conexion viva a datos del cubo. |
| **Cube View MD** | Cube View como tabla de hechos multidimensional (para BI Viewer) |
| **Method** | Business Rule personalizado o consultas out-of-the-box |
| **SQL** | Consultas contra la BD de aplicacion o framework |
| **BI-Blend** | Interfaz predefinida para consultar tablas BI Blend |

![Data Adapter tipo Cube View](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p63-1142.png)

**Parameters (6 tipos):**

| Tipo | Descripcion |
|------|------------|
| **Literal Value** | Valor explicito, actualizable via business rules o Assemblies |
| **Input Value** | Holding parameter para pasar valores via dashboard actions |
| **Delimited List** | Lista manual de items disponibles en seleccion |
| **Bound List** | Listas de miembros y propiedades usando Method Types |
| **Member List** | Usa Member Filter Builder para crear lista de miembros de metadata |
| **Member Dialog** | Lista de miembros con arbol de seleccion (member tree component) |

![Parameter tipo Literal Value](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p66-1164.png)

**Components (38 tipos, principales):**

| Categoria | Componentes |
|-----------|------------|
| **Data & Visualization** | BI Viewer, Book Viewer, Chart (Basic), Chart (Advanced), Cube View, Data Explorer, Data Explorer Report, Gantt View, Grid View, Large Data Pivot Grid, Map, Pivot Grid, Report, Sankey Diagram, Spreadsheet |
| **Navigation & Input** | Button, Check Box, Combo Box, Date Selector, Embedded Dashboard, Filter Editor, etc. |

![Lista de los 38 Component Types disponibles](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p69-1186.png)

**Diferencia clave: Cube View Component vs Cube View:**
El Cube View Component es un objeto colocado en un dashboard que muestra un Cube View. Permite controlar que sucede cuando el usuario interactua con el Cube View (ej. refrescar otra parte del dashboard).

---

### 201.4.3: Pasos para crear un Report Book

#### Que es un Report Book?

Permite combinar varios reportes y archivos en un solo documento. Se usan comunmente para crear estados financieros y paquetes de reportes gerenciales. Tipos: **PDF Books**, **Excel Books**, **Zip File Books**.

#### Extensiones de archivo

| Tipo | Extension |
|------|----------|
| Excel Book | `ReportBookName.xfDoc.xlBook` |
| PDF Book | `ReportBookName.xfDoc.pdfBook` |
| Zip File Book | `ReportBookName.xfDoc.zipBook` |

#### Ubicaciones para guardar

- File in OneStream File System (File Explorer)
- File on Local Folder
- Application Workspace File (en Maintenance Unit)
- System Workspace File

![Book Designer Toolbar](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1034-4347.png)

#### Book Properties

| Propiedad | Descripcion |
|----------|------------|
| **Determine Parameters from Content** | Si True, determina automaticamente los parametros requeridos. Para libros grandes, poner False y especificar manualmente (mejora rendimiento). |
| **Required Input Parameters** | Lista separada por comas de nombres de parametros (dejar vacio si Determine Parameters = True). |

#### Items que se pueden agregar a un Book

| Item | Descripcion |
|------|------------|
| **File** | Archivo de fuente externa (URL, Workspace File, Database File, File Share File) |
| **Excel Export Item** | Para XLBooks - especificar Cube View a exportar (cada uno en tab separado) |
| **Report** | Cube View, Dashboard Chart, Dashboard Report, System Dashboard Chart, System Dashboard Report |
| **Loop** | Secuencia que repite un proceso |
| **If / Else If / Else** | Declaraciones condicionales |
| **Change Parameters** | Personalizar salida sin alterar la fuente |

**Nota sobre Excel Export Items:** Report y File items NO son soportados por Excel Books y se ignoran si se agregan.

**Propiedades de Report:**
- Cube View or Component Name: Nombre del Cube View o Dashboard componente
- Output Name: Nombre para el reporte en Zip books (opcional)
- Include Report Margins/Report Header/Page Header/Report Footer/Page Footer: True/False

#### File Source Types

| Tipo | Descripcion |
|------|------------|
| **URL** | Archivo de pagina web interna o externa (URL completa) |
| **Application Workspace File** | Archivo en Maintenance Unit File Section |
| **System Workspace File** | Archivo en System Workspace Maintenance Unit File Section |
| **Application Database File** | Archivo en Application Database Share |
| **System Database File** | Archivo en System Database Share |
| **File Share File** | Archivo del File Share |

Muchos tipos de archivo se pueden agregar: Word, PDF, CSV, XLSX.

#### Loops

**3 tipos de Loop:**

| Tipo | Descripcion | Ejemplo de definicion |
|------|------------|----------------------|
| **Comma Separated List** | Lista de valores separados por coma | `Houston, Clubs, [Houston Heights]` |
| **Dashboard Parameter** | Parametro pre-configurado | `ParamSalesRegions` |
| **Member Filter** | Member Filter Builder basado en miembros de dimension | `E#Frankfurt, E#Houston` o `E#[NA Clubs].Base` |

**Loop Variables:**

| Variable | Descripcion |
|----------|------------|
| `|Loop1Variable|` a `|Loop4Variable|` | Referencia valores del loop por nombre (hasta 4 loops anidados) |
| `|Loop1Display|` a `|Loop4Display|` | Referencia valores por descripcion |
| `|Loop1Index|` a `|Loop4Index|` | Asigna numero secuencial empezando en 1 |

#### Change Parameters

Se usan para mejorar los datos en Report Books. Loops **requieren** Change Parameters para ejecutarse. Cuando se encuentra un Change Parameter en un loop, la Loop Variable se actualiza al siguiente valor.

![Change Parameters dentro de un Loop](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1042-4364.png)

| Categoria | Propiedades |
|-----------|------------|
| **Workflow** | Change Workflow (True/False), Workflow Profile, Workflow Scenario, Workflow Time |
| **POV** | Change POV (True/False), Member Script (ej. `E#[|Loop1Variable|]:A#Sales`). El POV tab del Cube View NO debe tener miembros seleccionados. |
| **Variables** | Change Variables, Variable Values (hasta 10 variables: `Variable1=Red, Variable2=Large`) |
| **Parameters** | Change Parameters, Parameter Values (override de parametros: `MyParam=Red, MyOtherParam=[|Loop1Variable|]`) |

**Nota:** Usar `!!` para mostrar Display Items en headers. Usar `!` para mostrar Value items.

#### If Statements

Proporcionan logica condicional a los Report Books. Se combinan frecuentemente con Loops.

**Ejemplos:**
- `(|Loop1Variable| = [Frankfurt])` - Si el Loop Variable es Frankfurt
- `(|!UserName!| = Administrator)` - Si el usuario es Administrator
- `(|!UserName!| = Administrator) Or (|!UserName!| = JSmith)` - Combinar con Or/And

**Else y Else If:** Requieren un If statement previo. Permiten logica adicional.

![Ejemplo de Loop con If/Else If/Else](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1047-4379.png)

#### Pasos completos para crear un Report Book

1. **Abrir OneStream** y navegar a Application tab > Presentation > Books.
2. **Hacer clic en Create New Book** en el toolbar.
3. **Configurar Book Properties:**
   - Determine Parameters from Content: True o False
   - Required Input Parameters: lista de parametros si Determine = False
4. **Agregar items** con el boton Add Item:
   - File, Excel Export Item, Report, Loop, If Statement, Change Parameters
5. **Configurar Loops (si aplica):**
   - Seleccionar Loop Type (Comma Separated List, Dashboard Parameter, Member Filter)
   - Definir Loop Definition
   - Agregar Loop Variables
6. **Configurar Change Parameters dentro de Loops:**
   - Workflow, POV, Variables, Parameters segun necesidad
7. **Configurar If Statements (si aplica)**
8. **Remover items** innecesarios con Remove Item.
9. **Drag and drop** para reordenar items.
10. **Guardar** con Save As seleccionando ubicacion y tipo de libro (extension).
11. **Previsualizar** en la pestana Preview.
12. **Cerrar** con Close Book.

**Nota sobre PDF Embedded Fonts:** Pueden aumentar el tamano del PDF. Usar PDF Embedded Fonts to Remove en Application Server Configuration File. Default: Arial; Calibri; Segoe UI; Tahoma; Times New Roman; Verdana.

#### Book Preview Toolbar

![Book Preview Toolbar](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1049-4397.png)

| Funcion | Descripcion |
|---------|------------|
| Page Navigation | Navegar a pagina especifica |
| First/Last | Ir a primera/ultima pagina |
| Previous/Next | Navegar adelante/atras una pagina |
| **Combine All Items** | Combina todas las paginas como un solo documento. Necesario para guardar/imprimir libro completo. |
| **Download Combined PDF File** | Genera PDF combinado usando Adobe rendering (metodo recomendado). No requiere Combine All Items. |
| Refresh | Actualizar y seleccionar nuevos parametros |
| Close | Cerrar el report book en Preview |
| Open | Abrir report book desde desktop o carpeta |
| Save | Guardar pagina actual (o todo el libro si Combine All Items activo) |
| Print | Imprimir (todo el libro si Combine All Items activo) |
| Find | Buscar palabras clave |
| Zoom | Zoom in/out del report book |

**Right-Click Options en Preview:**
- **Select Tool:** Seleccionar porciones para copiar/pegar (CTRL+C/CTRL+V)
- **Hand Tool:** Scroll por el reporte
- **Marquee Zoom:** Seleccionar area para hacer zoom (Alt+Left para regresar, Ctrl+0 para vista de pagina)
- Print, Find, Select All

#### Uso de Report Books

Despues de crearlos, se pueden:
- Agregar a otros books
- Agregar a dashboards via Book Viewer o File Viewer component
- Enviar por email via OneStream Parcel Service
- Generar ejecutando una secuencia de Data Management
- Almacenar en: FileShare, desktop, dashboard file

#### Dashboards en OnePlace

![Dashboards en OnePlace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch22-p1993-7491.png)

Los Dashboards son series combinadas de reportes, grids, charts y graficos. Al seleccionar un Dashboard, puede solicitarse la entrada de Parameters predefinidos.

**Dashboard Toolbar:**

![Dashboard Toolbar](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch22-p1994-7494.png)

| Icono | Funcion |
|-------|---------|
| **Select Parameters** | Seleccionar parametros especificos |
| **Reset Parameter Selections and Refresh** | Cambiar parametros y refrescar |
| **Edit Dashboard** | Lanzar Application tab para modificar (segun seguridad) |
| **Print** | Imprimir desde web o via PDF. Para libro completo: right-click > Combined PDF File o PDFs In Zip File. |

---

## Puntos Criticos a Memorizar

### Cube Views
- Son los **"building blocks of reporting"** de OneStream.
- Se organizan en **Groups** (contenedor) y **Profiles** (exposicion/acceso).
- El **POV slider tiene prioridad** sobre el POV Pane.
- Se pueden anidar hasta **4 dimensiones en filas** y **2 dimensiones en columnas**.
- **WFProfileEntities** se usa en Rows/Columns (no en POV, porque puede haber multiples entidades).
- **Substitution Variables** se referencian con pipes: `|WFTime|`, `|UserName|`, `|POVTime|`.
- **Parameters** en Cube Views se referencian con pipes y exclamation: `|!ParameterName!|`.
- **Sparse Row Suppression:** Mejora rendimiento filtrando registros sin datos antes de renderizar. No aplica a datos calculados dinamicamente. Se habilita en General Settings Y en columnas.
- **Paging:** Se activa automaticamente con mas de 10,000 filas sin suprimir. Intenta devolver 20-2000 filas en max 20 segundos.
- **Can Modify Data = False** hace el Cube View de solo lectura, pero las anotaciones siguen permitidas.
- **Rename con :Name()** permite cambiar el nombre mostrado en encabezados.
- **Conditional Formatting** sigue el orden: Application Properties > Default > Column > Row > Column Override > Row Override.
- **Spreading Types:** Fill, Clear Data, Factor, Accumulate, Even Distribution, Proportional, 445/454/544.
- **Allocation Types:** Clear Data, Even Distribution, 445/454/544, Weighted, Advanced.
- **Drill Down** funciona desde cualquier celda, NO necesita ser base level. Celdas verdes = se puede seguir drilling.
- **Cube Views creados fuera del Default Workspace** NO son accesibles desde la pagina Cube Views del Application tab.
- **Data Attachments:** Standard (multiples), otros tipos (solo uno por celda). Son parte de la View Dimension.
- **Cell Detail:** Solo disponible en O#Forms o O#BeforeAdj.
- **TreeExpansionLevel:** 0 = colapsar todo, 1-4 = expandir niveles. Requiere RowExpansionMode = "Use Default".
- **Where(HasChildren = True):** Solo funciona para dimensiones asignadas al cubo en el Scenario Type por defecto.

### Workspaces
- Son **sandboxes de desarrollo aislados** dentro de una misma aplicacion.
- Dos niveles de seguridad: **Access Group** (ver, no modificar) y **Maintenance Group** (ver y modificar).
- **Is Shareable Workspace = True** + listar en **Shared Workspace Names** del Workspace destino para compartir objetos.
- El **Default Workspace siempre esta compartido** con cualquier otro Workspace.
- Permiten **reutilizar nombres de objetos** en Workspaces separados.
- **3 Security Roles:** AdministerApplicationWorkspaceAssemblies, ManageApplicationWorkspaces, WorkspaceAdminPage.
- Exportar un Workspace = un solo archivo XML con todos los objetos empaquetados.
- **Substitution Variables del Workspace** usan prefijo `WSSV`: `|WSSVNombreSufijo|`.
- No requieren reconstruir soluciones existentes; son retrocompatibles.
- **Dashboard Profiles y Cube View Profiles son globales**, no especificos de Workspace.
- **Cube Views en 3 ubicaciones:** Default/Default MU, Default/Specific MU, Non-Default/Specific MU.
- **Text 1-8 fields:** Almacenan strings para usar en Assemblies sin modificar codigo.
- **Workspace Filter:** Permite a usuarios refinar los Workspaces mostrados.
- **Dashboard Types:** Use Default, Top Level, Top Level Without Parameter Prompts, Embedded, Embedded Dynamic, Embedded Dynamic Repeater, Custom Control.

### Report Books
- **3 tipos:** PDF (`.pdfBook`), Excel (`.xlBook`), Zip (`.zipBook`).
- **Excel Books** usan Excel Export Items (NO soportan Report ni File items).
- **Loops:** 3 tipos (Comma Separated List, Dashboard Parameter, Member Filter).
- **Loop Variables:** `|Loop1Variable|` hasta `|Loop4Variable|` para anidamiento.
- **Change Parameters** son obligatorios dentro de Loops para actualizar la variable.
- **If Statements** permiten logica condicional (se pueden combinar con Or/And).
- **Determine Parameters from Content = False** mejora rendimiento en libros grandes.
- **Download Combined PDF File** es el metodo recomendado para descargar books como PDF (no requiere Combine All Items).
- Se pueden agregar archivos de multiples fuentes: URL, Application/System Workspace File, Application/System Database File, File Share File.
- **PDF Embedded Fonts:** Configurar en Application Server Config para reducir tamano.
- Las etiquetas `!!` en headers muestran Display Items; `!` muestra Value Items.

---

## Mapeo de Fuentes

| Objetivo | Libro / Capitulo |
|----------|-----------------|
| 201.4.1 - Configuraciones de Cube View | Design Reference Guide, Cap. 12 (Build Reports Through Cube Views, Member Filter Builder, Cube View Performance, Format a Cube View, Advanced Cube Views); Cap. 21 (Using OnePlace Cube Views - Toolbar, Shortcuts, Spreading, Allocation, Drill Down, Data Attachments, Cell Detail) |
| 201.4.2 - Alcance de Workspaces | Workspaces & Assemblies, Cap. 2 (Understanding Workspaces - Definition, Benefits, Security, Sharing, Dashboard Types); Cap. 3 (Setting Up Your Workspace - Properties, Security Roles, Creating, Filter); Cap. 4 (Components - Maintenance Units, Dashboard Groups, Cube View Groups, Data Management Groups, Data Adapters, Parameters, Components) |
| 201.4.3 - Crear un Report Book | Design Reference Guide, Cap. 12 (Presenting Data Using Report Books - Book Designer, Book Properties, Loops, Change Parameters, If Statements, Create a Report Book); Cap. 22 (Using OnePlace Dashboards) |
