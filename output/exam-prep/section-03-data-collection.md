# Seccion 3: Data Collection (13% del examen)

## Objetivos del Examen

- **201.3.1:** Demostrar comprension de los diferentes tipos de Data Source
- **201.3.2:** Dados los requisitos de datos fuente, aplicar una configuracion de Data Source XFD
- **201.3.3:** Demostrar comprension del Import Process Log
- **201.3.4:** Dada una situacion, describir los tipos de Transformation Rules, nombres y ejemplos que funcionan de manera mas eficiente

---

## Conceptos Clave

### 201.3.1: Tipos de Data Source

Los Data Sources son plantillas (blueprints) de los tipos de importaciones requeridas y definen como analizar (parse) e importar datos. Un Data Source puede asignarse a uno o muchos Workflow Profiles que compartan un formato de archivo comun.

![Pantalla principal de Data Sources](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p872-3859.png)

#### Tipos principales de Data Source

| Tipo | Descripcion | Formato | Configuracion especifica |
|------|------------|---------|--------------------------|
| **Fixed Files** | Archivos en formato de columna fija con datos en posiciones predefinidas. Generalmente reportes de sistemas fuente impresos a archivo. | Texto plano con columnas fijas | Start Position y Length |
| **Delimited Files** | Archivos con campos separados por un caracter comun (coma, punto y coma, tab, etc.). | CSV, TSV, etc. | Delimiter y Quote Character |
| **Connector** | Conexion directa a base de datos externa (ODBC/OLEDB). Importa datos sin necesidad de archivo fisico. | SQL Query, View, Stored Procedure | Connector Business Rule |
| **Data Management Export Sequence** | Usa una secuencia de Data Management en lugar de archivo o conector. | Interno de OneStream | Data Export Sequence Name |

**Otros metodos de entrada de datos (no son Data Sources propiamente):**

| Metodo | Descripcion | Origin Member |
|--------|------------|---------------|
| **Excel Template** | Archivo Excel con Named Ranges (XFD, XFF, XFJ, XFC). Requiere Allow Dynamic Excel Loads = True. | Import (XFD), Forms (XFF), AdjInput (XFJ) |
| **Manual Input (Forms)** | Entrada manual a traves de formularios en Cube Views o Dashboards. | Forms |
| **Excel Add-In** | Usa formulas XFSetCell para cargar datos a celdas especificas del cubo. | Forms |

#### Propiedades generales de Data Sources - Detalle completo

##### General
- **Name:** Nombre del Data Source
- **Description:** Descripcion detallada

##### Security
- **Access Group:** Miembros con autoridad para acceder al Data Source
- **Maintenance Group:** Miembros con autoridad para mantener el Data Source

##### Settings
- **Cube Name:** El cubo asociado, que dicta las dimensiones disponibles
- **Scenario Type:** Permite asignar a un Scenario Type especifico o a todos. Si se asigna a uno especifico, solo esta disponible cuando se asigna a un Workflow Profile de ese tipo.

##### Data Structure Settings

| Propiedad | Descripcion | Opciones |
|-----------|------------|----------|
| **Type** | Estructura del archivo fuente | Fixed, Delimited, Connector, Data Mgmt Export Sequences |
| **Data Structure Type** | Formato de los datos | **Tabular** (una linea = una interseccion con un monto) o **Matrix** (multiples montos por linea usando filas y columnas) |
| **Allow Dynamic Excel Loads** | Permite cargar Excel templates ademas del archivo configurado | True/False |

#### Fixed Files - Detalles

Archivos en formato de columna fija. Para cada dimension se configura:
- **Start Position:** Posicion numerica inicial del campo
- **Length:** Cantidad de caracteres a tomar desde la posicion inicial

![Ejemplo de archivo Fixed File](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch06-p201-2187.png)

Se puede asociar un archivo fuente al Data Source haciendo clic en el toolbar y seleccionando campos visualmente resaltando areas en rojo.

#### Delimited Files - Detalles

Archivos con campos separados por un caracter delimitador.

| Propiedad | Descripcion |
|-----------|------------|
| **Delimiter** | Caracter separador (coma, punto y coma, tab, etc.) |
| **Quote Character** | Caracter de comillas para proteger campos que contienen el delimitador |

![Ejemplo de archivo Delimited](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch06-p202-2194.png)

**Precaucion:** Asegurarse de que el delimitador no sea un caracter usado en nombres de campos (ej: "New Company, Inc." con delimitador coma).

#### Connector Data Source - Detalles importantes

Un **Connector Business Rule** define la conexion, conjuntos de resultados y capacidades de Drill Back.

##### Connector Information Request Types

| Request Type | Funcion | Retorna |
|-------------|---------|---------|
| **GetFieldList** | Devuelve la lista de campos disponibles del Data Source externo. Se ejecuta al seleccionar el Connector en la pantalla de Data Source. | List(Of String) |
| **GetData** | Se ejecuta al hacer clic en "Load and Transform". Ejecuta las consultas de datos. | Rows de datos (fields deben coincidir con GetFieldList) |
| **GetDrillBackTypes** | Devuelve las opciones de Drill Back disponibles. Se ejecuta al hacer doble-clic o right-click > Drill Back. | List(Of DrillBackTypeInfo) |
| **GetDrillBack** | Ejecuta el tipo de Drill Back seleccionado por el usuario. | DrillBackResultInfo |

##### Tipos de Drill Back

| Tipo | Descripcion |
|------|------------|
| **DataGrid** | Presenta una grilla de datos al usuario |
| **TextMessage** | Presenta un mensaje de texto |
| **WebUrl** | Presenta un sitio web o HTML personalizado |
| **WebUrlPopOutDefaultBrowser** | Abre un sitio web en el navegador externo por defecto |
| **FileViewer** | Presenta contenido de archivo (FileShareFile, AppDBFile, SysDBFile) |

![Ejemplo de Drill Back en datos importados](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p889-3902.png)

![Drill Back anidado a segundo nivel](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p890-3905.png)

##### Prerequisitos de integracion con Connector

| Paso | Detalle |
|------|---------|
| 1. Inventario de sistemas fuente | Tipo de BD, ubicacion (Oracle, SQL, DB2, etc.) |
| 2. Metodo de consulta | SQL Query, SQL View, Stored Procedure |
| 3. Credenciales de acceso | Solo lectura contra produccion |
| 4. Client Data Provider 64-bits | Instalado en **cada** Application Server |
| 5. Connection String | Configurado en `XFAppServerConfig.xml` bajo Database Server Connections |

##### Creacion de Connection String

1. Crear archivo .udl en el Desktop del Application Server (renombrar .txt a .udl)
2. Configurar Provider, Server, Credentials, Database
3. Guardar y renombrar a .txt para ver el connection string resultante

![Creacion de archivo UDL para Connection String](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p880-3877.png)

![Resultado del Connection String](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p881-3880.png)

##### Ejemplo de Connection Strings

| Sistema | Connection String |
|---------|------------------|
| **SQL Server** | Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=DBName;Data Source=SQLSERVERNAME |
| **Oracle** | Provider=OraOLEDB.Oracle.1;Password=xxxxx;Persist Security Info=True;User ID=username;Data Source=frepro.world |
| **DB2** | Provider=IBMDA400.DataSource.1;Password=xxxxx;Persist Security Info=True;User ID=OSuser;Data Source=HUTCH400 |
| **MS Access** | Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\\\UNCFileShare\\DB1.accdb;Mode=Read |

##### Configuracion en Server Configuration

![Server Configuration - Database Connections](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p886-3891.png)

![Detalle de Connection String Settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p886-3892.png)

##### Prototipar Queries con Dashboard Data Adapters

Best practice: Crear Dashboard Maintenance Unit "EXS_ConnectorName" con Data Adapters para probar queries antes de crear el Connector Business Rule.

![Dashboard Data Adapter para probar queries externas](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p887-3895.png)

![Resultado de query de prueba](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p888-3899.png)

#### Data Management Export Sequence Data Source

Permite usar una secuencia de Data Management en un Import Workflow para:
- Copiar datos de un cubo/escenario a otro cubo/escenario con diferente dimensionalidad
- Exportar datos de OneStream a un sistema externo aplicando Transformation Rules

##### Caso de uso 1: Copiar datos dentro de OneStream

1. Configurar Data Management Step con tipo Export Data
2. Crear Data Source con Type = "Data Mgmt Export Sequences" y establecer Data Export Sequence Name
3. Configurar Source Dimensions
4. Crear Transformation Profile para mapear source a target
5. Crear Import Workflow Profile con Workflow Name = Import, Validate, Load

![Configuracion de Data Management Export Step](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p903-7963.png)

![Data Source configurado como DM Export Sequence](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p904-3943.png)

![Source Dimensions del DM Export Sequence](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p905-3946.png)

##### Caso de uso 2: Exportar datos de OneStream

1. Configurar Data Management Step con Export Data
2. Crear Data Source con DM Export Sequences
3. Crear Transformation Rules con Source Value y Target Value para sistema externo
4. Crear Import Workflow con **Import Stage Only** (solo importa al Stage)
5. Configurar metodo de recuperacion: Workflow Event Handler, Transformation Event Handler, Extender Rule, o Data Adapter con REST API

![Workflow Profile para exportacion](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p912-3968.png)

#### Origin Dimension - Concepto fundamental

La dimension Origin segmenta los datos de cada entidad en tres grupos aislados:

![Diagrama de Origin Dimension](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch06-p195-2152.png)

| Origin Member | Datos | Metodo de entrada |
|--------------|-------|-------------------|
| **Import** | Datos importados de archivos planos, consultas o plantillas Excel (XFD) | Load and Transform en Workflow |
| **Forms** | Datos ingresados manualmente o via plantilla Excel (XFF) / Add-In | Forms en Workflow, Cube Views, XFSetCell |
| **AdjInput (Journal Entries)** | Asientos de diario manuales, templates o cargados por Excel (XFJ) | Journals en Workflow |

La dimension Origin actua como barrera: reimportar datos no sobreescribe datos de Forms, y viceversa. Esta aislacion es fundamental para la proteccion de datos.

![Capas de proteccion de datos por Origin](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch06-p205-2217.png)

#### Consideraciones de datos fuente

- Los archivos fuente deben estar en **signo natural de debito y credito** (facilita mapeo y validacion)
- El campo **Amount** es el mas importante de cualquier archivo; determina si un registro es aceptado o rechazado
- Los montos pueden estar en columna unica (formato Tabular) o en formato Matrix
- OneStream es un sistema de **pull** (extraccion), no permite push de datos
- Para datasets grandes (mas de 1 millon de registros): ajustar Cache Page Size = 500, Cache Pages In Memory Limit = 2000, Cache Page Rule Breakout Interval = 0

![Configuracion de Cache para datasets grandes](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch06-p199-2175.png)

#### Smart Integration Connector (SIC)

Se usa cuando ODBC/REST API no estan expuestos publicamente. Usa un agente local (SIC Gateway) y Azure Relay para conectar de forma segura a sistemas detras de firewalls.

---

### 201.3.2: Configuracion de Data Source XFD (Excel Template)

#### Dimension Tokens para Import Excel Template

La plantilla debe usar Named Ranges que comiencen con **XFD** para importar datos al Stage. Los tokens de dimension deben estar en la primera fila del Named Range.

![Ejemplo de Excel Template con tokens](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch06-p203-2202.png)

| Token | Significado | Notas |
|-------|------------|-------|
| `E#` | Entity | Cada fila lista la entity |
| `A#` | Account | Cada fila lista la cuenta |
| `AMT#` | Amount | Usar `AMT.ZS#` para zero suppression automatica (tambien aplica a Matrix) |
| `F#` | Flow | |
| `IC#` | Intercompany | |
| `C#` | Consolidation | |
| `S#` | Scenario | |
| `T#` | Time Period | |
| `V#` | View | |
| `O#` | Origin | |
| `UD1#` a `UD8#` | User Defined Dimensions | Cada fila debe tener valor, usar `None` si no aplica. Se puede usar `UX#` o `UDX#` |
| `LB#` | Label | Descripcion de referencia, no se almacena en el cubo |
| `SI#` | Source ID | Clave para datos en Stage. Best practice: un Source ID por Named Range |
| `TV#` | Text Value | Almacena grandes cantidades de texto |
| `A1#` a `A20#` | Attribute Dimensions | Hasta 100 caracteres cada una |
| `AV1#` a `AV12#` | Attribute Value Dimensions | Datos numericos |

#### Abreviaturas de encabezado (Header Abbreviations)

| Abreviatura | Sintaxis | Descripcion |
|-------------|---------|-------------|
| **Static Value** | `F#:[None]` | Fija un miembro a toda la columna |
| **Business Rule** | `AMT#:[]:[NombreRegla]` | Ejecuta Business Rule para asignar valor |
| **Matrix Member** | `T#:[]:[]:[2012M3]` | Se repite por cada miembro en Matrix |
| **Current Workflow Time** | `T.C#` | Retorna Time del Workflow actual |
| **Current Workflow Scenario** | `S.C#` | Retorna Scenario del Workflow actual |
| **Global Time** | `T.G#` | Retorna Time del Global POV |
| **Global Scenario** | `S.G#` | Retorna Scenario del Global POV |
| **Annotation** | `TV#:[#Annotation]` | Agrega fila de anotacion |
| **VarianceExplanation** | `TV#:[#VarianceExplanation]` | Agrega fila de explicacion de varianza |

#### Tipos de Named Range segun tipo de datos

| Named Range | Uso | Detalles |
|-------------|-----|---------|
| **XFD** | Carga de datos al Stage (Import) | Dimension Tokens en primera fila |
| **XFF** | Datos de Forms | Incluye Property Tokens (Form Template Name, Workflow Name, Scenario, Time) |
| **XFJ** | Datos de Journals | Incluye Property Tokens (Template Name, Journal Type, Balance Type, etc.) |
| **XFC** | Cell Details | Incluye los 18 dimension members + AMT#, LIT#, AW#, CL#, LB# |

Se pueden usar multiples Named Ranges del mismo tipo en multiples pestanas de un libro Excel.

#### Form Excel Template - Detalles adicionales

Las primeras 4 filas del Named Range XFF deben incluir:

![Property Tokens de Form Excel Template](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p972-4158.png)

| Property Token | Descripcion |
|----------------|------------|
| **Form Template Name** | Nombre del Form Template |
| **Workflow Name** | Ej: Houston.Forms |
| **Workflow Scenario** | Actual, Budget, etc. o `\|WFScenario\|` |
| **Workflow Time** | Periodo o `\|WFTime\|` |

Tokens adicionales de Form:
- `HD#` = Has Data (Yes/No)
- `AN#` = Annotation
- `AS#` = Assumption
- `AD#` = Audit Comment
- `FN#` = Footnote
- `VE#` = Variance Explanation

![Ejemplo de Named Range XFF con datos](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p975-4169.png)

#### Journal Excel Template - Detalles adicionales

Las primeras 11 filas del Named Range XFJ deben incluir:

![Property Tokens de Journal Excel Template](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p979-4186.png)

| Property Token | Descripcion |
|----------------|------------|
| **Template Name** | Nombre del template (vacio si free-form) |
| **Name** | Nombre del journal |
| **Description** | Descripcion |
| **Journal Type** | Standard o Auto-reversing |
| **Balance Type** | Balanced, Balanced by Entity, Unbalanced |
| **Is Single Entity** | True/False |
| **Entity Filter** | Member Filter para Entities |
| **Consolidation Member** | Moneda o Local |
| **Workflow Name** | Ej: Houston.Journals |
| **Workflow Scenario** | Actual, etc. o `\|WFScenario\|` |
| **Workflow Time** | Periodo o `\|WFTime\|` (campo adicional opcional: CubeTimeName) |

Tokens de Journal:
- `AMTDR#` = Monto de debito
- `AMTCR#` = Monto de credito

![Ejemplo de Named Range XFJ](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p982-4199.png)

#### Source Dimension Properties - Detalle completo

Cada Data Source tiene Source Dimensions asignadas con propiedades configurables.

##### Data Types disponibles

| Data Type | Descripcion | Aplica a |
|-----------|------------|----------|
| **DataKey Text** | Lee el valor del archivo segun position settings | Todas las dimensiones |
| **Stored DataKey Text** | Fuerza un valor constante para Time en cada linea | Time |
| **Global DataKey Time** | Usa el valor Time del Global POV | Time |
| **Current DataKey Time** | Usa Time del Workflow POV actual | Time |
| **Current DataKey Scenario** | Usa Scenario del Workflow POV actual | Scenario |
| **Matrix DataKey Text** | Para Matrix Load con multiples periodos | Time (Matrix) |
| **Text** | Lee el valor del archivo segun position settings | Dimensiones no-key |
| **Stored Text** | Fuerza un valor constante para cada linea | Dimensiones no-key |
| **Matrix Text** | Para Matrix con multiples columnas de misma dimension | Dimensiones no-key (Matrix) |
| **Label** | Lee el valor como etiqueta | Label |
| **Stored Label** | Fuerza un valor constante como etiqueta | Label |
| **Numeric** | Define el campo numerico de monto | Amount |

##### Position Settings

| Tipo de archivo | Configuracion |
|----------------|---------------|
| **Fixed Files** | Start Position y Length |
| **Delimited Files** | Column number |
| **Connector** | Source Field Name (proporcionado por el Business Rule) |

Se puede seleccionar visualmente en el archivo adjunto: resaltar area (aparece en rojo), hacer clic en el icono de asignacion.

![Herramienta de seleccion visual de posicion](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p917-3982.png)

##### Logical Operator

| Opcion | Descripcion |
|--------|------------|
| **None** | Sin script (default) |
| **Complex Expression** | Script .NET solo disponible en esta dimension |
| **Business Rule** | Script .NET desde la Business Rule Library |

##### Numeric Settings (solo para Amount)

| Propiedad | Descripcion |
|-----------|------------|
| **Thousand Indicator** | Caracter separador de miles (ej: ",") |
| **Decimal Indicator** | Caracter separador decimal |
| **Currency Indicator** | Simbolo de moneda |
| **Positive Sign Indicator** | Caracter(es) para valor positivo |
| **Negative Sign Indicator** | Caracter(es) para valor negativo |
| **Debit/Credit Mid-Point Position** | Si debitos y creditos estan en el mismo campo, el punto medio para distinguirlos |
| **Factor Value** | Multiplica el monto importado (ej: 1000 para convertir de miles) |
| **Rounding** | Not Rounded o valores 1-10 |
| **Zero Suppression** | True = no importar ceros; False = importar ceros |
| **Text Criteria for Valid Numbers** | Criterio para numeros validos |

##### Bypass Settings

Permite saltar lineas completas basandose en un valor encontrado:

| Bypass Type | Descripcion |
|------------|------------|
| **Contains at Position** | Salta la linea si el Bypass Value se encuentra en la posicion especificada |
| **Contains Within Line** | Salta la linea si el Bypass Value aparece en cualquier lugar |

![Configuracion de Bypass Settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p923-4005.png)

**Tip:** Para bypass de espacios en blanco en Fixed, usar corchetes dobles con la cantidad de espacios en el Bypass Value.

##### Substitution Settings

| Propiedad | Descripcion |
|-----------|------------|
| **Old Value (Find)** | Valor a buscar. Multiples valores separados con `^` |
| **New Value (Replace)** | Valor de reemplazo. Multiples valores separados con `^`. Para string vacio: `\|Empty String\|`, `\|Null\|`, `\|Single Space\|`, `\|Space\|` |

##### Text Fill Settings

| Propiedad | Descripcion | Ejemplo |
|-----------|------------|---------|
| **Leading Fill Value** | Caracteres que preceden al valor importado | Mask=xxx, Valor=00, Resultado=x00 |
| **Trailing Fill Value** | Caracteres que siguen al valor importado | Mask=xxx, Valor=00, Resultado=00x |

##### Stored Text Settings

| Propiedad | Descripcion |
|-----------|------------|
| **Text Criteria to Bypass in Storage Buffer** | Valor(es) que causan bypass del registro. Multiples con `^` |
| **Stored Value Line #** | Numero de linea del que se obtiene el valor repetido |

##### Matrix Settings (solo cuando Data Structure Type = Matrix)

| Propiedad | Descripcion |
|-----------|------------|
| **Matrix Header Values Line #** | Numero de fila donde buscar los miembros de la dimension Matrix |

![Ejemplo de Matrix Settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p920-3990.png)

---

### 201.3.3: Import Process Log

#### El proceso de importacion en Workflow - Paso a paso detallado

##### 1. Import (Load and Transform)

El sistema importa datos al **Stage Engine**. El archivo se parsea en formato tabular limpio con informacion de Amounts, Source ID y cada Dimension.

![Paso Import en el Workflow con Load and Transform](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1932-7269.png)

**Load Methods disponibles:**

| Metodo | Comportamiento | Restricciones |
|--------|---------------|---------------|
| **Replace** | Borra datos del Source ID anterior y reemplaza con nuevo archivo | Puede hacerse incluso si datos previos ya cargados al Cube |
| **Replace (All Time)** | Reemplaza todos los Workflow Units en el Workflow View seleccionado | Para multi-periodo |
| **Replace Background (All Time, All Source IDs)** | Reemplaza todos en hilo de fondo. **Siempre elimina TODOS los Source IDs** | NO usar si workflow usa multiples Source IDs para reemplazo parcial |
| **Append** | Agrega solo filas nuevas sin modificar datos existentes | Solo agrega lo que no estaba antes |

![Dialogo de Load Method](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1933-7272.png)

**Iconos adicionales del paso Import:**

| Icono | Funcion |
|-------|---------|
| Re-Import | Repite importacion cuando hay cambios y necesita recalcular |
| Clear Stage | Limpia todos los datos del Stage |
| View Last Source File | Ve el ultimo archivo fuente procesado |
| View Last Log File | Ve el ultimo log de procesamiento |

##### 2. Validate

Dos acciones especificas durante Validate:

**Paso 1 - Verificacion de mapeo:** OneStream verifica que cada dato tenga un mapa (Transformation Rule).

**Paso 2 - Verificacion de intersecciones:** OneStream verifica que la combinacion de dimensiones pueda cargarse en el cubo (ej: restricciones de Intercompany).

![Paso Validate en el Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1935-7286.png)

**Transformation Errors:**
- Muestra la dimension con error, Source Value y Target Value (Unassigned)
- El sistema sugiere One-To-One Transformation Rule por defecto
- Se puede buscar el target correcto con el filtro en el panel derecho
- Guardar cambios y hacer clic en **Retransform** (re-valida automaticamente)

![Pantalla de Transformation Errors con Target Value Unassigned](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1935-7288.png)

**Intersection Errors:**
- Algo no es correcto con la interseccion completa (ej: Customer mapeado a Salary Grade Account)
- Para corregir: clic en la interseccion mala, drill down para ver GL/ERP Account
- Right-click > View Transformation Rules para investigar cada regla por dimension
- Editar la regla incorrecta, guardar, hacer clic en Validate nuevamente

##### 3. Load

Carga los datos limpios del Stage al **Analytic Engine** (Consolidation Engine).

![Paso Load - clic en Load Cube](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1936-7293.png)

![Dialogo de progreso de Load Cube](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1937-7296.png)

#### Right-Click Options en Import

| Opcion | Descripcion |
|--------|------------|
| **View Source Document** | Abre el archivo fuente importado |
| **View Processing Log** | Abre el log de procesamiento con informacion de cuando y como se importo |
| **View Transformation Rules** | Muestra todas las reglas de mapeo para la interseccion especifica |
| **Drill Back** | Solo disponible con Connector Data Source; navega al sistema fuente |
| **Export** | Exporta datos a Excel XML, CSV, Text o HTML |

#### Workflow Icons y estados

| Color | Significado |
|-------|-----------|
| **Azul** | Tarea pendiente por completar |
| **Verde** | Tarea completada exitosamente |
| **Rojo** | Error que debe corregirse antes de continuar |

#### Controles de carga globales

| Control | Descripcion | Configuracion |
|---------|------------|---------------|
| **Enforce Global POV** | Carga limitada al Global POV | Application > Tools > Application Properties |
| **Allow Loads Before Workflow View Year** | Controla cargas a periodos anteriores | Application > Tools > Application Properties |
| **Allow Loads After Workflow View Year** | Controla cargas a periodos posteriores | Application > Tools > Application Properties |

#### Audit Workflow Process

Al hacer clic derecho en cualquier canal de Workflow, proporciona una auditoria completa:
- Fecha y hora del proceso
- Usuario que ejecuto el proceso
- Duracion del proceso
- Errores ocurridos
- Historial de bloqueo (Lock History > Workflow Lock/Unlock)

#### Data Load Execution Steps (Clear and Replace)

Cuando se ejecuta el proceso de carga, el engine sigue estos pasos:

1. **Verifica estado del Workflow:**
   - Implicitly locked (parent workflow certificado)?
   - Explicitly locked?
2. **Verifica switches de carga:**
   - Can Load Unrelated Entities
   - Flow/Balance Type No Data Zero View Override
   - Force Balance Accounts to YTD View
3. **Analiza cargas previas:**
   - Evalua Data Units cargados previamente para determinar que limpiar
4. **Ejecuta clear data:**
   - Limpia Workflow Data Units cargados por el Workflow Unit (a nivel de Account)
5. **Ejecuta load data:**
   - Carga usando parallel processing por entity

---

### 201.3.4: Transformation Rules - Tipos, nombres y eficiencia

#### Orden de ejecucion de las Transformation Rules

Las reglas se ejecutan en el siguiente orden durante el paso **Validate** del Workflow:

1. **Source Derivative Rules** (logica sobre datos fuente entrantes)
2. **One-to-One** (mapeo explicito)
3. **Composite** (mapeo condicional con tags dimensionales)
4. **Range** (rango de valores)
5. **List** (lista delimitada)
6. **Mask** (wildcards)
7. **Target Derivative Rules** (logica sobre datos post-transformados)

![Barra de herramientas de Transformation Rules](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p942-4066.png)

#### Tipos de Transformation Rules - Detalle completo

##### One-to-One

Un miembro fuente se mapea a un miembro destino explicitamente. Sin wildcards ni scripts.

| Propiedad | Descripcion |
|-----------|------------|
| **Source Value** | Valor del archivo fuente |
| **Description** | Descripcion opcional |
| **Target Value** | Miembro de la Dimension Library de destino |
| **Order** | Orden de procesamiento (default: alfanumerico) |

**Regla critica:** Es el **unico tipo valido para Scenario, Time y View**. Estas tres dimensiones solo soportan One-to-One.

**Ejemplo:** `Actual -> Actual`, `23099 -> 23000`

##### Composite

Mapeo condicional usando tags dimensionales (`D#[valor]`). Usa `*` (cualquier numero de caracteres) y `?` (un caracter).

**Comportamiento importante:** **Se detiene cuando encuentra una coincidencia.** Si el registro cumple mas de una regla, configurar el Order de la mas restrictiva a la mas amplia.

**Ejemplo:** `A#[199?-???*]:E#[Texas]` -- Cuentas que empiezan con 199 con entity Texas.

![Ejemplo de Composite con Order de narrow a broad](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p946-4084.png)

![Resultado de mapeo Composite con different targets](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p947-4088.png)

**Ejemplo de Order correcto:**
1. `E#[*]:A#[H???]` -- 4 caracteres o menos (Head) -> Rhode Island
2. `E#[*]:A#[H????]` -- 5 caracteres o menos (Heads) -> Maine
3. `E#[*]:A#[H?????????]` -- 10 caracteres o menos (Headcount) -> Bypass

##### Range

Define limite superior e inferior con `~` como separador. Usa **character sets, no integers**.

**Comportamiento importante:** **NO se detiene al encontrar coincidencia.** Si los rangos se superponen, aplica el **ultimo rango procesado**. Configurar Order para que el ultimo sea el deseado.

**Ejemplo:** `11202~11209` -> Account 12000

**Regla critica:** Usar character sets balanceados. `4~3000` debe ser `0004~3000`.

##### List

Lista delimitada de miembros que mapean al mismo destino. Usa `;` como separador.

**Ejemplo:** `41137;41139;41145` -> Account 61000

##### Mask

Wildcards: `*` = cualquier numero de caracteres, `?` = un caracter.

| Pattern | Captura | No captura |
|---------|---------|-----------|
| `27*` | 270, 2709, 27XX-009 | - |
| `27??` | 2709 | 27999, 2700-101 |
| `*000*` | Cualquiera con 000 en medio | - |

**Restricciones de Target Value:**
- `?` en Target Value **NO se soporta**
- `*` como prefijo (izquierda) en Target Value **NO se soporta**
- `*` como sufijo (derecha) en Target Value devuelve el **Source value**

**Propiedades comunes a Composite, Range, List, Mask:**
- Rule Name, Description, Rule Expression, Target Value, Logical Operator, Order

##### Source Derivative Rules

Logica aplicada a datos fuente entrantes. Crea nuevos registros en Stage basados en datos de entrada.

![Pantalla de Derivative Rules](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p950-4095.png)

##### Target Derivative Rules

Logica aplicada a datos post-transformados. Crea registros adicionales en Stage. Como son post-transformacion, los registros Final no pasan por Transformation Rules.

##### Lookup

Tabla de busqueda versatil para formulas, business rules o lookup simple.

#### Derivative Types

| Derivative Type | Almacenado en Stage | Usado en otros derivados | Descripcion |
|----------------|--------------------|--------------------------|----|
| **Interim** | No | Si | Temporal para uso en derivados posteriores |
| **Interim (Exclude Calc)** | No | No | Excluido de otros calculos derivados |
| **Final** | Si | Si | Se almacena y puede mapearse a target |
| **Final (Exclude Calc)** | Si | No | Almacenado pero excluido de otros calculos |
| **Check Rule** | N/A | N/A | Regla de validacion personalizada durante Validate |
| **Check Rule (Exclude Calc)** | N/A | No | Check Rule excluido de otros calculos |

#### Derivative Rule Expressions - Ejemplos de sintaxis

![Tabla de ejemplos de Derivative Rule Expressions](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p951-4098.png)

| Rule Expression | Tipo | Notas |
|----------------|------|-------|
| `A#[11*]=Cash` | Final | Accounts que empiezan con 11 agregan a Cash (almacenado en Stage) |
| `A#[12*]=AR` | Interim | Accounts que empiezan con 12 agregan a AR (no almacenado) |
| `A#[1300-000;Cash]=CashNoCalc` | Interim (Exclude Calc) | Cash excluido porque calc excluido |
| `A#[1000~1999]<<New_:E#[Tex*]=TX` | Final | Crea nuevas filas con prefijo "New_" para accounts 1000-1999 |
| `A#[2000~2999]>>_:Liability:U2#[*]=None` | Final | Crea filas con sufijo "_Liability", UDs a None |
| `A#[3000~3999]@3:E#[Tex*]@1,1` | Final | Toma primeros 3 digitos de Account; @1,1 = posicion 1 longitud 1 de Entity |

#### Logical Operators (Expression Types)

| Operator | Descripcion |
|----------|------------|
| **None** (default) | Sin script |
| **Business Rule** | Script .NET desde Business Rule Library (tipo Derivative) |
| **Complex Expression** | Script .NET local |
| **Multiply** | Multiplica valor resultante por Math Value |
| **Divide** | Divide valor resultante por Math Value |
| **Add** | Suma Math Value |
| **Subtract** | Resta Math Value |
| **Create If > x** | Crea derivado solo si valor > umbral |
| **Create If < x** | Crea derivado solo si valor < umbral |
| **Lag (Years/Months/Days/Hours/Minutes/Seconds)** | Retorna valor pasado segun intervalo |
| **Lag Change (Years/Months/Days/Hours/Minutes/Seconds/Step Back)** | Retorna diferencia entre valor actual y pasado |

![Ejemplo de Lag Years](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p958-4113.png)

![Ejemplo de Lag Change Step Back](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p962-4132.png)

#### Rendimiento de Transformation Rules (de mas a menos eficiente)

| Tipo de Regla | Procesamiento | Costo | Color |
|--------------|--------------|-------|-------|
| **One-to-One** | Simple update pass through a la BD | Minimo | Verde |
| **Range** | Simple update pass through a la BD | Minimo | Verde |
| **List** | Simple update pass through a la BD | Minimo | Verde |
| **Mask `*` (un lado)** | Simple update pass through. Bajo overhead | Minimo | Verde |
| **Mask `*` a `*`** | Simple update pass through | Minimo | Verde |
| **Mask `?`** | Simple update pero **table scans**. Minimizar `?` | Medio | Amarillo |
| **Range/List/Mask (Conditional)** | Mucha transferencia de datos y memoria. Devuelve record set completo | Alto | Rojo |
| **Derivative** | Mucha transferencia y memoria. SQL con LIKE. Inserta registros uno por uno | Alto | Rojo |

**Recomendaciones clave de rendimiento:**
- Usar **One-to-One, Range y List** siempre que sea posible (costo minimo)
- Las reglas con `?` son mas costosas que `*` porque requieren table scans
- Los `?` en ambos lados (fuente y destino) son extremadamente costosos
- Las reglas **Conditional** (Composite, Range, List, Mask con condiciones dimensionales) devuelven todos los campos al app server, consumiendo mucha memoria
- Para listas grandes en Conditional: dividir en listas mas pequenas
- Para Mask Conditional: mantener criterios restrictivos (ej: `1*`, `2*`, `A*`, `B*`)
- Un one-to-one map con muchas reglas puede ser mas eficiente que pocas mask rules con `?`
- Si todo tiene mapeo one-to-one, puede ser senal de que el ledger se esta replicando innecesariamente

#### Transformation Rule Profiles

Las Transformation Rule Groups se organizan en **Transformation Rule Profiles**, que luego se asignan a **Workflow Profiles** (Application > Workflow Profiles > Import > Integration Settings).

![Pantalla de Transformation Rule Profiles](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p967-4146.png)

**Atajo rapido:** El boton **"Create and Populate Rule Profile"** crea automaticamente un Group y un Profile con el mismo nombre, ya poblado con cada Dimension Rule Group y se actualiza cuando el Group se actualiza.

![Boton Create and Populate Rule Profile](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p942-4070.png)

**TRX Files:** Se pueden exportar e importar Groups como archivos TRX (comma delimited). Limitacion: no soporta Logical Operators ni Complex Expressions. Para manejo completo usar Application Tools Load/Extract XML.

**Reglas globales reutilizables:** Las dimensiones Scenario, Time y View solo usan One-to-One y pueden compartir grupos entre multiples cubes. Nombrarlas como "Global" para facil identificacion.

#### Form Templates - Configuracion

##### Form Template Properties

| Propiedad | Descripcion |
|-----------|------------|
| **Form Type** | Cube View, Dashboard, o Spreadsheet (Windows App only) |
| **Form Requirement Level** | Not Used (deprecated), Optional, Required |
| **Form Frequency** | All Time Periods, Monthly, Quarterly, Half Yearly, Yearly, Member Filter |
| **Time Filter for Complete Form** | Filtro que dicta frecuencia del Complete Form |

![Ejemplo de Time Filter for Complete Form](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p938-4056.png)

##### Parameter Types para Cube View Forms

| Parameter Type | Descripcion |
|---------------|------------|
| **Literal Value** | Valor hard coded |
| **Input Value** | Usuario puede cambiar |
| **Delimited List** | Lista de valores predefinidos |
| **Member List** | Lista plana de Members |
| **Member Dialog** | Dialog con busqueda y jerarquia |

##### Form Allocations - Advanced Distribution

Permite distribuir montos usando fuente, destino, pesos y calculos personalizados:

![Dialogo de Allocation con Source/Destination POV](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p926-4015.png)

1. **Source POV:** Interseccion de origen (puede drag & drop desde grid)
2. **Source Amount/Calculation Script:** Override del valor fuente con script
3. **Destination POV:** Interseccion de destino
4. **Dimension Type/Member Filters:** Override de dimensiones del destino
5. **Weight Calculation Script:** Como se pesa la distribucion
6. **Destination Calculation Script:** Default = `|SourceAmount|*(|Weight|/|TotalWeight|)`
7. **Offset:** Propiedades opcionales de compensacion
8. **Generate Allocation Data:** Preview antes de aplicar

![Resultado de allocation aplicada](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p929-4024.png)

#### Applying Literal Value Parameters to Form Templates

Tecnica para usar un solo Dashboard con multiples Form Templates via parametros:

1. Disenar Cube Views para data entry
2. Crear Dashboard con Delimited List Parameter de nombres de Cube Views
3. Crear Cube View Component con parametro `|!ParameterName!|`
4. Crear Supplied Parameter Component bound al parametro
5. En Form Template, set Form Type = Dashboard y especificar Name Value Pair

![Delimited List Parameter en Dashboard](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p931-4030.png)

![Name Value Pair en Form Template](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p932-4036.png)

---

## Puntos Criticos a Memorizar

### Data Source Types
- **4 tipos principales de Data Source:** Fixed, Delimited, Connector, Data Management Export Sequence
- **2 estructuras de datos:** Tabular (un monto por linea) y Matrix (multiples montos por linea)
- **Allow Dynamic Excel Loads = True** es necesario para cargar plantillas Excel al Stage
- OneStream es un sistema de **pull** (extraccion), no permite push de datos
- Connection String se configura en `XFAppServerConfig.xml` bajo Database Server Connections
- Cada Application Server necesita el Client Data Provider de 64 bits instalado
- **Connector Uses Files = True** para procesar archivos con formato complejo via codigo

### Origin Dimension
- **3 miembros principales:** Import, Forms, AdjInput (Journal Entries) -- aislados entre si
- Reimportar no sobreescribe Forms; entrada manual no sobreescribe Import
- La Origin dimension es fundamental para proteccion y layering de datos

### Excel Templates (Named Ranges)
- **XFD** = Import al Stage; **XFF** = Forms; **XFJ** = Journals; **XFC** = Cell Details
- Multiples Named Ranges del mismo tipo pueden existir en multiples tabs de un workbook
- **Source ID** es la clave para datos en Stage; best practice: un Source ID por Named Range
- `AMT.ZS#` aplica zero suppression automatica (tambien en Matrix)
- `T.C#` y `S.C#` = Current Workflow Time/Scenario; `T.G#` y `S.G#` = Global POV
- XFF requiere 4 property tokens; XFJ requiere 11 property tokens
- `AMTDR#` y `AMTCR#` son tokens exclusivos de Journals
- Para CSV templates: Column A especifica Row Type (H=Header, D=Detail)

### Import Process
- **Load Methods:** Replace, Replace (All Time), Replace Background (All Time, All Source IDs), Append
- **Replace Background** siempre elimina TODOS los Source IDs -- no usar con Source IDs parciales
- **Validate** hace dos cosas: (1) verifica mapeo, (2) verifica intersecciones validas en el cubo
- **Retransform** re-valida automaticamente despues de corregir Transformation Rules
- **Enforce Global POV** limita carga al Global POV
- Audit Workflow Process muestra fecha, usuario, duracion y errores de cada tarea

### Transformation Rules
- **Orden de ejecucion:** Source Derivative > One-to-One > Composite > Range > List > Mask > Target Derivative
- **Scenario, Time y View SOLO pueden usar One-to-One mapping**
- **Composite** se detiene al encontrar coincidencia; **Range** NO se detiene (aplica el ultimo rango)
- **Range** usa character sets no integers (`0004~3000` no `4~3000`)
- **`?` en Target Value no se soporta**; `*` como prefijo en Target no se soporta; `*` como sufijo en Target retorna Source value
- Caracteres `!`, `?` y `|` estan reservados y causan errores en Source/Target Members

### Rendimiento de Transformation Rules
- **Reglas mas eficientes:** One-to-One, Range, List, Mask con `*` de un solo lado (simple update pass through)
- **Reglas medias:** Mask con `?` (table scans)
- **Reglas menos eficientes:** Cualquier regla Conditional (devuelve record set completo), Derivatives (SQL LIKE + insert uno por uno)
- **Create and Populate Rule Profile:** Crea Group + Profile automaticamente, se actualiza con el Group
- TRX import/export no soporta Logical Operators ni Complex Expressions

### Derivative Rules
- **Interim:** No almacenado, usado en derivados posteriores
- **Final:** Almacenado en Stage, mapeable a target
- **Check Rule:** Validacion personalizada durante Validate
- **Exclude Calc:** Version de cada tipo excluida de otros calculos
- **Lag/Lag Change:** Operadores para valores historicos con granularidad de Years a Seconds

### Connector Data Source
- **4 Request Types:** GetFieldList, GetData, GetDrillBackTypes, GetDrillBack
- **5 Drill Back types:** DataGrid, TextMessage, WebUrl, WebUrlPopOutDefaultBrowser, FileViewer
- Best practice: prototipar queries con Dashboard Data Adapters antes de crear Business Rule
- Connection String name en XFAppServerConfig.xml se usa como referencia en el Business Rule

### Batch Processing
- Archivos en directorio `Harvest` con formato `FileID-WFProfileName-ScenarioName-TimeName-LoadMethod.txt`
- `;` reemplaza `.` para delimitar Parent y Child Profile names
- `C` = Current (de la funcion), `G` = Global, `R` = Replace, `A` = Append

### Cache settings para datasets grandes (>1M registros)
- Cache Page Size = 500
- Cache Pages In Memory Limit = 2000
- Cache Page Rule Breakout Interval = 0 (evalua todas las reglas en todas las paginas)

---

## Mapeo de Fuentes

| Objetivo | Libro / Capitulo |
|----------|-----------------|
| 201.3.1 - Tipos de Data Source | Design Reference Guide, Cap. 10 (Data Sources, Fixed Files, Delimited Files, Connector, DM Export Sequences, Source Dimensions); Foundation Handbook, Cap. 6 (Data Source Types, Origin Dimension, Staging Engine) |
| 201.3.2 - Configuracion XFD | Design Reference Guide, Cap. 10 (Loading Data With Excel Templates, Source Dimension Properties, Loading Form Data, Loading Journal Data, Loading Cell Detail); Foundation Handbook, Cap. 6 (Excel Templates, Data Parser) |
| 201.3.3 - Import Process Log | Design Reference Guide, Cap. 20 (Using OnePlace Workflow - Import, Validate, Load, Right-Click Options); Design Reference Guide, Cap. 9 (Data Load Execution Steps, Workflow Stage Import Methods); Foundation Handbook, Cap. 6 (Staging, Data Quality) |
| 201.3.4 - Transformation Rules | Design Reference Guide, Cap. 10 (Transformation Rules - One-to-One, Composite, Range, List, Mask, Derivative, Logical Operators, Derivative Types, Rule Profiles); Foundation Handbook, Cap. 6 (Transformation Performance, Working Through Problems) |
