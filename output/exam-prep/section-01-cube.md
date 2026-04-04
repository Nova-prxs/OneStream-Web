# Seccion 1: Cube (15% del examen)

## Objetivos del Examen

- **201.1.1:** Dada una especificacion de diseno, aplicar cambios a la configuracion del Cube
- **201.1.2:** Dada una Calculation de larga duracion para una Entity, analizar estadisticas del Data Unit para determinar la causa raiz
- **201.1.3:** Demostrar como configurar y aplicar FX Rates
- **201.1.4:** Demostrar comprension de la configuracion de Dimension Members
- **201.1.5:** Demostrar comprension de Stored y Dynamic Calculations

---

## Conceptos Clave

### Objetivo 201.1.1: Aplicar cambios a la configuracion del Cube

#### Estructura general del Cube

Un Cube es una estructura multidimensional que contiene datos para reportes y analisis. Consiste en **18 Dimensions** moldeadas a traves de Extensible Dimensionality. Los Cubes se crean en **Application > Cube > Cubes** y se configuran en el Cube Profile.

![Navegacion al Dimension Library donde se crean y mantienen las Dimensions del Cube](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p537-2785.png)

Un Cube tiene las siguientes pestanas de configuracion:
- **Cube Properties** (propiedades generales, seguridad, Workflow, calculo, FX Rates, Business Rules)
- **Cube Dimensions** (asignacion de Dimensions por Scenario Type)
- **Cube References** (vinculacion de Linked Cubes)
- **Data Access** (seguridad a nivel de celda de datos)
- **Integration** (configuracion de carga de datos)

#### Cube Properties - General

![Propiedades generales del Cube mostrando Name, Description, Cube Type y Time Dimension Profile](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p640-3179.png)

- **Name:** Maximo 100 caracteres. **No se puede cambiar una vez creado.** Se accede via API: `Dim sValue As String = objCube.Name`.
- **Description:** Maximo 200 caracteres. Se puede cambiar en cualquier momento.
- **Cube Type:** Etiqueta opcional para agrupar Cubes similares. Opciones:
  - Standard (valor por defecto al crear), Tax, Treasury, Supplemental, What If, Cube Type 1-8
  - Los Cube Types se almacenan como enteros en la tabla `dbo.Cube`:
    - Standard=0, Tax=1, Treasury=2, Supplemental=3, What If=4
    - CubeType1=11, CubeType2=12, CubeType3=13, CubeType4=14
    - CubeType5=15, CubeType6=16, CubeType7=17, CubeType8=18
  - Permite que diferentes Cubes compartan Dimensions con diferentes Constraints por Cube Type
  - Los nombres de los Cube Types son irrelevantes funcionalmente; no cambian el comportamiento del sistema
- **Time Dimension Profile:** Asigna el perfil temporal (calendario fiscal) al Cube. Lista desplegable de Time Profiles disponibles.

#### Cube Properties - Security

![Seccion de seguridad del Cube con Access Group, Maintenance Group y Use Parent Security](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p643-3188.png)

- **Access Group:** Grupo que puede ver el Cube (por defecto: Everyone). Es la segunda capa de seguridad despues del acceso a la aplicacion.
- **Maintenance Group:** Grupo que puede editar configuracion del Cube, crear nuevos objetos, y eliminar.
- **Use Parent Security for Relationship Consolidation:**
  - **False (defecto):** Todos los que tienen acceso a Parent Entities acceden a todos los Consolidation Members.
  - **True:** Controla acceso a Consolidation Members como Share, Elimination, OwnerPostAdj, OwnerPreAdj, basado en seguridad de la Parent Entity. El usuario puede ver Local y Translated pero no los relationship members.
  - Se determina Cube por Cube.

#### Cube Properties - Workflow

![Configuracion de Workflow del Cube mostrando Is Top Level Cube y Suffix settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p645-3195.png)

- **Is Top Level Cube for Workflow:**
  - Por defecto **False** cuando se crea un nuevo Cube.
  - Debe ser **True** para que el Cube pueda crear y mantener Workflow Profiles a traves de un Cube Root Profile.
  - Solo **un Cube** puede controlar los Workflow Profiles.
  - Si se usan Linked Cubes con Extensible Dimensionality, solo 1 Cube tiene True; los demas False.

- **Suffix for Varying Workflow by Scenario Type:**
  - Agrupa Scenarios por Scenario Type para disponibilizarlos en el Workflow.
  - Un setting por cada Scenario Type.
  - Ejemplo: asignar "ACT" a Actual y "PLAN" a Budget/Forecast/Plan.

![Ejemplo de Cube Root Workflow Profiles creados despues de configurar suffix](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p647-3200.png)

  - **IMPORTANTE:** Una vez que un Scenario tiene datos cargados, no se puede crear un nuevo Cube Root ni cambiar el suffix para ese Scenario Type. Se requiere un **Reset Scenario** (Data Management step) para hacer cambios.
  - Si un Scenario Type esta en blanco y no tiene datos, se puede agregar un suffix en cualquier momento.

#### Cube Properties - Calculation

![Seccion de Calculation del Cube con Consolidation Algorithm Type, Translation Algorithm Type y Business Rules](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p649-3211.png)

##### Consolidation Algorithm Type

Define como se tratan Share y Elimination durante la Consolidation:

| Algorithm Type | Share | Elimination | Caso de uso |
|---|---|---|---|
| **Standard (Calc-on-the-fly Share and Hierarchy Elimination)** | Dinamico (no almacena) | Algoritmos integrados | El mas usado. Solo en circunstancias raras no se usa Standard. |
| **Stored Share** | Almacenado | Algoritmos integrados | Minority interest donde Share no puede derivarse solo de Percent Consolidation. Requiere BR con `FinanceFunctionType.ConsolidateShare`. |
| **Org-By-Period Elimination** | Dinamico | Considera Percent Consolidation en cada relacion de la jerarquia | Cuando un IC Member puede no ser descendiente si Percent Consolidation = 0. |
| **Stored Share and Org-By-Period Elimination** | Almacenado | Org-By-Period | Combinacion de ambos. |
| **Custom** | Via Business Rules | Via Business Rules | Logica totalmente personalizada con `ConsolidateShare` y `ConsolidateElimination`. Eliminaciones custom en UD Dimensions o intersecciones especificas. |

**Impacto en rendimiento de Stored Share:**
- Incrementa registros en Data Record tables
- Aumenta tamano de Data Units
- El engine debe ejecutar logica custom y escribir datos adicionales

![Opciones de Consolidation Algorithm Type en la interfaz](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p37-764.png)

##### Translation Algorithm Type

![Opciones de Translation Algorithm Type](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p38-771.png)

| Translation Type | Descripcion | Caso de uso |
|---|---|---|
| **Standard** | Traduccion basada en FX Rates asignados al Cube/Scenario | Mas usado. |
| **Standard Using Business Rules for FX Rates** | Standard + BR para especificar tasas por interseccion | Traducir Actual a tasas de Budget. Aplicar tasas diferentes a ciertas Accounts. Forecast/Constant Currency. |
| **Custom** | Traduccion completamente via Business Rules | Raro. Metodos de traduccion no estandar. |

##### Business Rules 1-8

![Hasta 8 Finance Business Rules pueden asignarse al Cube](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p653-3221.png)

- Se ejecutan dentro del DUCS en orden logico intercalado con Member Formulas.
- La misma Finance Business Rule puede compartirse entre Cubes.
- La BR debe estar creada como Finance Business Rule para aparecer en la lista.

##### NoData Calculate Settings

![NoData Calculate Settings para cada Consolidation Member](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p38-772.png)

Controlan si el Finance Engine ejecuta Calculations contra celdas sin datos para cada Consolidation Member:
- **Valor por defecto:** False (excepto Local)
- **Poner en True** cuando se copia data de otro Time period o Scenario
- Si no se pone True, Data Units vacios en el Target no ejecutaran el DUCS, aunque haya datos en el Source
- En la mayoria de situaciones donde se copia datos, solo copiar `C#Local` es suficiente ya que el algoritmo de Consolidation traducira y consolidara

#### Cube Properties - FX Rates

![Seccion FX Rates del Cube con Default Currency, Rate Types y Rule Types](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p653-3222.png)

- **Default Currency:** Moneda de reporte por defecto del Cube. Usada para triangulacion de FX Rates e IC Matching.
- **Rate Type for Revenues and Expenses:** Por defecto AverageRate
- **Rate Type for Assets and Liabilities:** Por defecto ClosingRate
- **Rule Type for Revenues and Expenses:** Por defecto **Periodic**
- **Rule Type for Assets and Liabilities:** Por defecto **Direct**
- Rate Types predefinidos: AverageRate, OpeningRate, ClosingRate, HistoricalRate
- Se pueden crear nuevos FX Rate Types en **Cube > FX Rates** (disponibles inmediatamente)
- **Direct:** `Translated Value = Local Value * FX Rate del Periodo Actual`
- **Periodic:** `Translated Value = Prior Period Translated + [(Current Local - Prior Local) * Current Rate]`

#### Cube Dimensions - Asignacion

![Asignacion de Dimensions por Scenario Type en el Cube - Default](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p655-3228.png)

![Asignacion de Dimensions por Scenario Type - detalle por tipo](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p656-3231.png)

Las Dimensions se asignan por **Scenario Type**. Reglas criticas:

1. **Entity y Scenario** solo se asignan en el **(Default)** Scenario Type. En los demas Scenario Types estan grayed out.
2. **Configuracion recomendada:** Asignar **RootXXXDim** a Dimension Types no utilizados en cada Scenario Type (en lugar de dejar "Use Default"). Esto permite flexibilidad futura.

![Configuracion recomendada con RootXXXDim para dimensiones no usadas](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p657-3235.png)

3. **CRITICO:** Una vez que las Dimensions se asignan y hay datos cargados, no se pueden desasignar ni cambiar. Para cambiar, se requiere un **Reset Scenario**.
4. Cambiar de RootXXXDim a una Dimension especifica es un cambio **unico e irreversible** si hay datos.
5. Si se deja **(Use Default)** y se cargan datos, el Dimension Type queda bloqueado a lo que tenga el Default.

**Escenario de uso comun (Use Case del examen):**
- Un usuario quiere agregar una Customer dimension en UD4 solo para el Budget Scenario Type.
- Si siguio la configuracion recomendada (RootUD4Dim en Budget), puede cambiar UD4 de RootUD4Dim a la nueva dimension.
- Si dejo (Use Default), debe asignar la nueva dimension en el (Default) Scenario Type, afectando TODOS los Scenario Types.

![Ejemplo exitoso de cambio de dimension cuando se uso configuracion recomendada](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p662-3254.png)

#### Cube Design Options

| Opcion | Descripcion | Caso de uso |
|---|---|---|
| **Monolithic Cube** | Un solo Cube simple | Solo aplicaciones muy pequenas |
| **Super Cube (Linked Cubes)** | Cube padre con Cubes hijos enlazados via Entity Dimension | Diseno mas comun. Mejor uso de extensibilidad. |
| **Paired Cubes** | Split/shared entities en multiples Cubes | Misma Entity en multiples Cubes |
| **Specialty Cubes** | Cubes monoliticos limitados | Driver cubes, admin cubes |

- **Regla general:** Mas Cubes no ralentizan la aplicacion; mejoran el rendimiento al reducir el tamano de Data Units.

#### Cube References

![Cube References mostrando vinculacion de Entity Dimensions entre Cubes padre e hijos](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p668-3282.png)

Se usa para vincular Linked Cubes:
- El Cube padre tiene la pestana **Cube References** configurada
- Cuando Entity Dimensions de Cubes hijos crean relaciones con la Entity Dimension del Cube padre, aparecen automaticamente en la lista
- Esto asegura que los datos se consoliden en la jerarquia del Cube padre

#### Data Access Tab

![Estructura del Data Access tab con tres secciones de seguridad](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p670-3287.png)

Tres secciones de seguridad a nivel de Cube:

**1. Data Cell Access Security ("Slice Security"):**
- Control de acceso (No Access, Read Only, All Access) a grupos de celdas
- Requiere Member Filters + Access Groups
- **Category** opcional para agrupar reglas (maximo 100 caracteres)
- Ocho comportamientos disponibles:
  - Skip Item and Continue (default para "not in Group/not in Filter")
  - Skip Item and Stop
  - Apply Access and Continue (default para "in Group and in Filter")
  - Apply Access and Stop
  - Increase/Decrease Access and Continue/Stop
- Se ejecuta **despues** de la seguridad de Application > Cube > Entity > Scenario
- Para activar categories en una Entity: `Use Cube Data Access Security = True`

**2. Data Cell Conditional Input:**
- Controla si **todos los usuarios** pueden ingresar datos en celdas especificas
- Mismas propiedades que Data Cell Access Security **excepto** que no tiene Access Group
- Aplica a todos los usuarios por igual

**3. Data Management Access Security:**
- Controla acceso cuando se ejecutan procesos de Data Management
- Enfocado en **Data Units** (Entity y Scenario), no en celdas individuales
- Solo permite Member Filters de Entity y Scenario Dimension Types

#### Integration Tab

![Integration tab mostrando Cube Dimensions, Label Dimensions y configuracion por Scenario Type](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p679-3317.png)

Controla la estructura de datos para Workflow:
- **Cube Dimensions:** Asignadas automaticamente desde Cube Dimensions tab
- **Label Dimensions:** Label, SourceID, TextValue - campos auxiliares para Stage
- **Attribute Dimensions / Attribute Value Dimensions:** Para BI Blend
- Cada Dimension Type tiene: Cube Dimension Name (read-only), Transformation Sequence, Enabled
- **Deshabilitar** Dimension Types no usados para que no aparezcan en Data Sources ni Transformation Rules

---

### Objetivo 201.1.2: Analizar estadisticas del Data Unit para determinar causa raiz

#### Que es un Data Unit

Un Data Unit es la **unidad de particion y procesamiento** del Cube. Se define como todos los datos dentro de una combinacion unica de:
- **Cube + Entity + Parent + Consolidation + Scenario + Time**

El Finance Engine procesa todos los datos dentro de cada Data Unit a la vez. Es la unidad por la cual el Cube se particiona y el mecanismo que divide los datos en partes manejables que caben en la memoria del servidor.

![Diagrama de intersecciones potenciales de un Cube mostrando por que los Data Units son necesarios](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch03-p62-923.png)

#### Data Unit Calculation Sequence (DUCS)

El DUCS es la serie de pasos que ocurre cada vez que se ejecuta una Calculation o Consolidation:

1. Clear previously calculated data (si `Clear Calculated Data During Calc = True` en el Scenario; no limpia Durable Data)
2. Run Scenario Member Formula
3. Perform reverse Translations (Flow Members de otras monedas)
4. Execute Business Rules 1 y 2
5. Execute Formula Passes 1-4 (Account > Flow > UD1 > UD2...UD8)
6. Execute Business Rules 3 y 4
7. Execute Formula Passes 5-8
8. Execute Business Rules 5 y 6
9. Execute Formula Passes 9-12
10. Execute Business Rules 7 y 8
11. Execute Formula Passes 13-16

**El DUCS es "all or nothing":** Todos los pasos se ejecutan cada vez; no se puede elegir ejecutar solo un paso. Esto preserva la integridad de datos y asegura que las dependencias entre calculos nunca se comprometan.

#### Consolidation Dimension y el DUCS

El DUCS se ejecuta para cada Consolidation Member. Algunos Finance Function Types solo se ejecutan en ciertos Consolidation Members:

![Tabla mostrando que Finance Function Types se ejecutan en cada Consolidation Member](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p40-782.png)

Durante una Consolidation de una Parent Entity, el DUCS puede ejecutarse hasta **7 veces**:
1. Calculate Local Currency
2. Translate Local to Parent's Default Currency (`C#Translated`)
3. Calculate Translated Currency
4. Calculate OwnerPreAdj
5. Calculate/Default Share
6. Execute Elimination, then Calculate Elimination
7. Calculate OwnerPostAdj
8. Combine child data to parent local
9. Calculate parent local

#### Triggering del DUCS

El DUCS se puede disparar desde:

![Data Management step para Calculate mostrando Step Type y Calculation Type](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p42-795.png)

1. **Data Management:** Step types Calculate o Custom Calculate con Calculation Types: Calculate, Translate, Consolidate (con variantes With Logging y Force)
2. **Cube Views:** Habilitando Calculate/Consolidate en propiedades del Cube View

![Propiedades de Cube View para habilitar Calculate y Consolidate](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p44-810.png)

3. **Dashboard Button:** Server Task con DM Sequence o Calculate directo
4. **Workflow Process Step:** Via Calculation Definitions en Workflow Name

![Workflow Process Step con Calculation Definitions](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p47-831.png)

#### Causas de Data Units lentos

- **Tamano excesivo del Data Unit:** Data Units que exceden ~1 millon de registros merecen inspeccion. Mas de 2 millones son "red flags".
- **Data Units densos en niveles superiores:** A medida que datos se consolidan, los Parent Entity Data Units se vuelven mas densos y lentos.
- **Pocas threads en la parte superior:** Sibling Entities se procesan en paralelo. En la cima de la jerarquia hay pocos siblings = pocas threads.
- **Reglas mal escritas:** Business Rules que generan datos en intersecciones innecesarias.
- **UD Dimensions con poca superposicion:** Si UD1 tiene muchos miembros pero poco overlap entre entidades base, el Parent Data Unit se infla innecesariamente.
- **Datos innecesarios en el Cube:** Cubes no estan disenados para volumenes grandes de datos transaccionales.

#### Calculation Status

- **OK:** No Calculation Needed, Data Has Not Changed.
- **CN:** Calculation Needed, Data Has Changed (por Form input, data load, o Journal).
- **Force Calculate/Consolidate:** Procesa todos los Data Units dependientes ignorando el Calculation Status. **No usar innecesariamente** - desperdicia tiempo procesando Data Units sin cambios.
- **Calculate/Consolidate normal:** Verifica primero el Calculation Status y no ejecuta el DUCS si es OK.

#### Parallel Processing

![Diagrama de procesamiento paralelo de sibling Entities](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p48-838.png)

- Sibling Entities se procesan simultaneamente usando multiples threads (multi-threading)
- El orden de procesamiento entre siblings puede variar en cada Consolidation
- Las Consolidations usualmente alcanzan >90% rapidamente, y el ultimo 10% tarda (porque hay menos siblings en niveles superiores)
- **Sibling Consolidation Pass:** Permite forzar orden entre siblings (usado para Equity Pickup). Pass 2+ fuerza que la Entity se consolide despues de siblings en Pass 1.
- **Sibling Repeat Calculation Pass:** Para calculos circulares entre entities (propiedad circular)
- **Auto Translation Currencies:** Para traduccion a monedas de siblings durante Consolidation

#### Calculate With Logging

Para diagnosticar Calculations lentas:

![Calculate With Logging desde Data Management](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch08-p170-1721.png)

1. Ejecutar **Consolidate With Logging** o **Calculate With Logging**
2. Ir a Task Activity > Child Steps
3. Drill into cada Data Unit para ver duracion de cada paso del DUCS

![Child Steps mostrando duracion de cada paso del DUCS](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch08-p173-1745.png)

4. **CreateDataUnitCache** muestra el numero de registros traidos a memoria (= tamano del Data Unit)
5. Drill into Formula Passes y Business Rules individuales para encontrar el culpable

![Detalle de Formula Passes individuales con duracion](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch08-p174-1751.png)

**Nota:** Calculate With Logging agrega tiempo significativo de procesamiento.

#### Mejores practicas para rendimiento

- Usar extensibilidad (multiples Cubes) para reducir tamano de Data Units
- Limitar intersecciones validas con Constraints
- Usar `C#Aggregated` para Planning (hasta 90% mas rapido)
- No almacenar porcentajes o ratios (calcularlos dinamicamente)
- Monitorear Data Units mayores a 1 millon de registros
- Optimizar Entity hierarchies (evitar estructuras planas con muchos hijos)
- Evitar relaciones Entity-Parent 1-a-1 (crean storage points innecesarios)
- No almacenar datos transaccionales en Cubes (usar BI Blend o Specialty Planning)
- Aumentar numero de Entities para distribuir datos y aprovechar multi-threading

---

### Objetivo 201.1.3: Configurar y aplicar FX Rates

#### Donde se almacenan las FX Rates

Las FX Rates se almacenan en el **System Cube** como repositorio central que todos los otros Cubes referencian. Esto evita duplicar tasas en cada Cube.

#### FX Rate Types predefinidos

| Rate Type | Descripcion |
|---|---|
| **Average Rate** | Tasa promedio del periodo (primer dia al ultimo dia del mes) |
| **Opening Rate** | Tasa al inicio del periodo |
| **Closing Rate** | Tasa al cierre del periodo |
| **Historical Rate** | Tasa historica de una fecha especifica |

Se pueden crear nuevos FX Rate Types en **Cube > FX Rates**, que automaticamente quedan disponibles para seleccion en Cube Properties y Scenario Properties.

#### Configuracion a nivel de Cube

En **Cube Properties > FX Rates:**
- **Default Currency:** Moneda de reporte por defecto (usada para triangulacion y IC Matching)
- **Rate Type for Revenues and Expenses:** Por defecto AverageRate
- **Rate Type for Assets and Liabilities:** Por defecto ClosingRate
- **Rule Type for Revenues and Expenses:** Por defecto Periodic
- **Rule Type for Assets and Liabilities:** Por defecto Direct

#### Configuracion a nivel de Scenario

En Scenario Member Properties > FX Rates:

![Configuracion FX Rates a nivel de Scenario con Use Cube FX Settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p577-2935.png)

- **Use Cube FX Settings:**
  - **True (defecto):** Usa los Rate Types por defecto del Cube
  - **False:** Habilita configuracion personalizada a nivel de Scenario:
    - Rate Type y Rule Type para Revenue/Expenses y Assets/Liabilities
    - **Constant Year for FX Rates:** Permite usar tasas de un ano especifico (util para Constant Currency Scenarios)

#### Rule Types (metodos de traduccion) - Detalle

| Rule Type | Formula | Caso de uso |
|---|---|---|
| **Direct** | `Translated = Local * FX Rate` | Calculo directo con valor y tasa del periodo actual. Usado para Assets/Liabilities por defecto. |
| **Periodic** | `Translated = Prior Translated + [(Current Local - Prior Local) * Current Rate]` | Metodo de promedio ponderado que considera periodos anteriores. Usado para Revenue/Expenses por defecto. |

#### Translation Algorithm Types en el Cube

- **Standard:** Usa los FX Rate Types asignados al Cube/Scenario
- **Standard Using Business Rules for FX Rates:** Permite Business Rules para especificar tasas diferentes por interseccion. Intersecciones no especificadas usan Standard. Ejemplos:
  - Traducir Actual a tasas de Budget del ano actual
  - Aplicar tasas diferentes a ciertas Accounts
  - Tasa ya existe en FX Rate table en otro Rate Type/Time; la BR la determina dinamicamente
  - Reduce mantenimiento al eliminar la necesidad de copiar o duplicar tasas
- **Custom:** Traduccion totalmente via Business Rules (raro)

#### Lock FX Rates

- Permite bloquear FX Rate Types por Time para evitar cambios accidentales
- File Loads o funciones XFSetRate fallan si un FX Rate Type esta bloqueado
- Seguridad: Roles **ManageFXRates**, **LockFxRates**, **UnLockFxRates** en Application > Security Roles
- Administrators tienen derechos completos por defecto
- Se genera Task Activity para auditar bloqueos/desbloqueos

#### Alternate Input Currency (Override en Flow Dimension)

Para ingresar datos en una moneda diferente a la local de la Entity:

1. En la Account: `Use Alternate Input Currency In Flow = True`
2. En el Flow Member: configurar **Flow Processing Type**:

| Flow Processing Type | Descripcion | Requiere Account Setting |
|---|---|---|
| **Is Alternate Input Currency** | Solo para Accounts configuradas con Use Alternate Input Currency = True | Si |
| **Is Alternate Input Currency for All Accounts** | Para todas las Accounts | No |
| **Translate using Alternate Input Currency, Input Local** | Sobreescribe valor traducido con el monto de moneda local | Si |
| **Translate using Alternate Input Currency, Derive Local** | Sobreescribe y deriva valor local segun la tasa. No usar en trial balance. | Si |

3. **Alternate Input Currency:** Seleccionar la moneda (USD, EUR, etc.)
4. **Source Member for Alternate Input Currency:** El Flow member del cual tomar el valor a sobreescribir

---

### Objetivo 201.1.4: Configuracion de Dimension Members

#### Las 18 Dimensions del Cube

| Token | Dimension | Tipo |
|---|---|---|
| E# | Entity | Configurable |
| S# | Scenario | Configurable |
| A# | Account | Configurable |
| F# | Flow | Configurable |
| U1#-U8# | User Defined 1-8 | Configurable |
| P# | Parent | Derivada (de Entity) |
| I# | Intercompany | Derivada (de Entity) |
| T# | Time | Configurable pre-app |
| C# | Consolidation | Via App Properties |
| O# | Origin | System-defined |
| V# | View | System-defined |

#### Dimension Library Navigation

![Dimension Library con toolbar y opciones de busqueda](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p537-2786.png)

Las Dimensions se crean y mantienen en **Application > Cube > Dimensions**. Los iconos del toolbar incluyen:
- Create Dimension / Delete Dimension / Save / Rename / Move
- Create Member / Delete Member / Rename Member / Save Member
- Add Relationship / Remove Relationship
- Search Hierarchy / Collapse Hierarchy
- Navigate to Security

Las pestanas disponibles son: **Members**, **Member Properties**, **Relationship Properties**, **Dimension Properties**, **Grid View**.

#### Entity Dimension - Propiedades detalladas

![Entity Member Properties mostrando todas las categorias](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p542-2816.png)

**General:**
- Dimension Type, Dimension, Member Dimension, Id (auto-asignado, inmutable), Name, Alias (nombres alternativos, comma-delimited)

**Security:**
- Display Member Group, Read Data Group, Read Data Group 2, Read and Write Data Group, Read and Write Data Group 2
- Use Cube Data Access Security (True/False)
- Cube Data Cell Access Categories, Cube Conditional Input Categories, Cube Data Management Access Categories

**Settings:**
- **Currency:** Moneda local de la Entity
- **Is Consolidated:** True = datos se consolidan a Parent; False = solo agrupacion (mejora rendimiento)
- **Is IC Entity:** True = visible en IC Dimension para transacciones intercompanias. Ambas entities deben tener True para transaccionar entre si.

**Vary By Cube Type:**
- Flow Constraint, IC Constraint, UD1-UD8 Constraints
- IC Member Filter: Lista de IC partners permitidos
- UD1-UD8 Default: Asigna un miembro por defecto a la Entity (datos se dirigen a EntityDefault)

![Ejemplo de EntityDefault en UD con asignacion automatica de cost center](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p562-7957.png)

**Vary By Scenario Type:**
- **Sibling Consolidation Pass:** Para holding companies (Equity Pickup). Pass 2+ fuerza orden. Default = Pass 1.
- **Sibling Repeat Calculation Pass:** Para propiedad circular. Default = Use Default (no repeat).
- **Auto Translation Currencies:** Lista de monedas (comma-separated) para traduccion a siblings.

**Vary By Scenario Type and Time:**
- **In Use:** False desactiva la Entity (mantiene datos historicos, se ignora durante Consolidation)
- **Allow Adjustments:** True (defecto) = habilita modulo de Journals para AdjInput
- **Allow Adjustments From Children:** True (defecto) = permite OwnerPostAdj y OwnerPreAdj desde hijos directos
- **Text 1-8:** Atributos personalizados para Business Rules y Member Filters (pueden variar por Scenario Type y Time)

**Relationship Properties:**
- **Percent Consolidation:** Porcentaje de consolidacion al padre (100 = 100%). Puede variar por Scenario Type y Time.
- **Percent Ownership:** Porcentaje de propiedad (usado por Business Rules, no afecta Consolidation por si solo).
- **Ownership Type:** Full Consolidation, Holding, Equity, Non-Controlling Interest, Custom (es solo una etiqueta para BRs).
- **Text 1-8:** Atributos adicionales en relaciones.
- **Parent Sort Order:** Determina el default parent cuando hay multiples jerarquias.

#### Account Dimension - Propiedades detalladas

![Account Dimension en el Dimension Library](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p582-2954.png)

**Settings:**

![Settings de Account mostrando Account Type, Formula Type, Allow Input, Is Consolidated](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p584-2968.png)

- **Account Type:** Determina como se agregan a un Parent:
  - **Group:** Solo organizacional, no agrega datos
  - **Revenue:** Agrega como positivo
  - **Expense:** Agrega como negativo (no necesita ingresarse negativo)
  - **Asset:** Tag de activos
  - **Liability:** Tag de pasivos (agrega negativo). **NO existe tipo Equity; usar Liability para patrimonio.**
  - **Flow:** Valor tipo income statement, tiene periodic y YTD. No se traduce.
  - **Balance:** Valor tipo balance sheet en un punto del tiempo. No se traduce.
  - **BalanceRecurring:** Balance sheet que no cambia en el tiempo (opening balance). No se traduce.
  - **NonFinancial:** Informacional (headcount, square footage). Legacy. No se traduce.
  - **DynamicCalc:** Calcula on-the-fly, no necesita formulas de otros para ejecutar.

- **Formula Type:** Determina comportamiento de formula:
  - **FormulaPass1-16:** Controla orden de ejecucion en el DUCS
  - **DynamicCalc:** Calcula cada vez que se muestra, no almacena resultado
  - **DynamicCalcTextInput:** Dynamic + permite input de texto (Annotations)

- **Allow Input:** True (defecto) permite input. **False** = read-only. Poner False en Parents y cuentas calculadas.

- **Is Consolidated:**
  - **Conditional (True if no Formula Type) (defecto):** Si tiene Formula Type, **NO se consolida**. Si no tiene Formula Type, si se consolida.
  - **True:** Siempre se consolida, independientemente de Formula Type. **Cambiar a True si se quiere consolidar datos calculados.**
  - **False:** Nunca se consolida. Mejora rendimiento.

- **Is IC Account:**
  - **Conditional (True if Entity not same as IC):** Mas comun. Previene que una Entity registre IC consigo misma.
  - **True:** Siempre es IC (permite IC con si mismo)
  - **False (defecto):** No es cuenta IC.

- **Use Alternate Input Currency In Flow:** True para usar override de moneda historica.
- **Plug Account:** Selecciona cuenta para transacciones IC no eliminadas.

**Aggregation:**

![Aggregation settings por dimension](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p589-2981.png)

- Activar/desactivar agregacion por dimension: Entity, Consolidation, Flow, Origin, IC, UD1-UD8
- **Aggregation Weight (Relationship Properties):** Controla el peso de agregacion al padre.
  - Default = 1 (100%)
  - 0 = no agrega (evita doble conteo en jerarquias multiples)
  - 0.50 = agrega 50%

**Vary By Scenario Type and Time:**
- **In Use:** True/False (desactivar cuenta sin perder historico)
- **Formula / Formula for Calculation Drill Down:** Formulas pueden variar por Scenario Type y Time
- **Adjustment Type:** Not Allowed, Journals, Data Entry
- **Text 1-8:** Atributos personalizados

#### Scenario Dimension - Propiedades detalladas

**Scenario Types disponibles (20+):**
Actual, Forecast, Operational, Variance, ScenarioType1-8, Administration, Budget, Control, Flash, FXModel, History, LongTerml, Model, Plan, Sustainability, Target, Tax, Default

- Los nombres de Scenario Types son **irrelevantes funcionalmente** - no definen comportamiento del sistema
- No se pueden cambiar los nombres ni la cantidad de Scenario Types
- Multiples Scenarios pueden compartir el mismo Scenario Type

![Scenario Security settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p568-2904.png)

**Workflow Settings:**

![Workflow settings del Scenario](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p569-2908.png)

- **Use in Workflow:** True/False. False = no visible en Workflow (pero datos aun pueden ingresarse via Forms/Excel Add-In)
- **Workflow Tracking Frequency:** All Time Periods, Monthly, Quarterly, Half Yearly, Yearly, Range. **No puede cambiarse si hay steps procesados.**
- **Number of No Input Periods Per Workflow Unit:** Periodos read-only (ej: 3 primeros meses copiados de Actual)

**Settings principales:**

![Settings del Scenario con Input Frequency, Default View, etc.](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p571-2913.png)

- **Scenario Type:** Asigna el tipo
- **Input Frequency:** Monthly, Quarterly, Half Yearly, Yearly (puede variar por ano)
- **Default View:** Periodic o YTD (controla calculos y limpieza de datos)
- **Retain Next Period Data Using Default View:** True = periodos futuros cambian si se altera periodo anterior
- **Clear Calculated Data During Calc:** True (defecto) limpia datos calculados durante el DUCS. False = limpiar manualmente.
- **Use Two Pass Elimination:** Habilita `IsSecondPassEliminationCalc` en Business Rules.
- **Consolidation View:** Periodic o YTD. YTD mejora rendimiento de Consolidation.

#### Flow Dimension - Propiedades detalladas

![Flow Dimension con jerarquia simple y compleja](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p595-3005.png)

**Settings:**
- **Switch Sign:** True cambia el signo de datos para el Flow member
- **Switch Type:** True cambia el tipo de dato segun atributo de Account (ej: Asset a Revenue). Util para roll forward accounts.
- **Formula Type / Allow Input / Is Consolidated:** Igual que Account

**Flow Processing:** Para moneda alternativa (ver seccion FX Rates arriba)

![Flow Processing settings para Alternate Input Currency](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p598-3026.png)

#### User Defined Dimensions 1-8

![UD Dimensions en el Dimension Library](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p602-3040.png)

- 8 dimensions disponibles; no todas necesitan usarse
- **Asignar dimensiones mas grandes/significativas a UD1 o UD2** (performance basado en registros almacenados, no en intersecciones posibles)
- Cada UD tiene un **EntityDefault** member (asignable por Entity en Vary By Cube Type)
- UD2-UD8 tambien tienen **UD1Default** (usable en Constraints y Transformation Rules)

**Is Attribute Member:** Convierte el UD member en atributo calculado dinamicamente:
- Requiere: Source Member For Data, Expression Type, Related Dimension Type, Related Property, Comparison Text, Comparison Operator
- **No almacenan datos;** calculan on-the-fly
- **Precaucion:** Mas de ~2000 attribute members impactan rendimiento
- No impactan Data Unit size excepto cuando Related Dimension = Entity y se consolida
- No pueden recibir input ni tener formulas
- Los datos de atributos no se pueden exportar a archivos
- Los attribute results no se pueden bloquear para integridad de datos
- Pueden referenciarse en Business Rules via `includeUDAttributeMembersWhenUsingAll = True` en GetDataBuffer

#### Nombres de Dimension Members

- Unicos dentro de un Dimension Type (ej: no puede haber dos GrossIncome en Account dimensions)
- Maximo 500 caracteres
- Usar guiones bajos en lugar de espacios y puntos
- Si incluyen espacios o puntos, usar corchetes: `E#[Quebec.City]`
- **Caracteres restringidos:** & * @ \ { } [ ] ^ , = ! > < - # % | + ? " ; /
- **Alias:** Nombres alternativos (comma-delimited). No pueden duplicarse en el mismo Dimension Type.

**Palabras reservadas:** Account, All, Cons, Consolidation, Default, DimType, Entity, EntityDefault, Flow, IC, None, Parent, POV, Root, RootXXXDim, Scenario, Time, UD1-UD8, UD1Default, Unknown, View, WF, Workflow, XFCommon, Origin

#### Origin Dimension (System-defined)

![Jerarquia de la Origin Dimension](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p624-3118.png)

Jerarquia fija:
- **Top** > BeforeElim > BeforeAdj > **Import** (datos cargados), **Forms** (datos de formularios)
- **Adjustments** > **AdjInput** (journals/forms), **AdjConsolidated** (consolidado desde hijos)
- **Elimination** > **DirectElim** (informacional, en primer common parent), **IndirectElim** (informacional, eliminaciones previas)

#### View Dimension (System-defined)

![Jerarquia de la View Dimension](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p627-3126.png)

- Datos siempre almacenados en **V#YTD** en la base de datos
- Periodic, MTD, QTD, HTD se calculan dinamicamente (included calculations)
- **CalcStatus:** Muestra el calculation status del Data Unit
- Miembros para texto: Annotation, Assumptions, AuditComment, Footnote, VarianceExplanation

#### Time Profiles

![Time Profile configuration con Profile tab y Time Periods tab](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p631-3140.png)

- **Standard Time Profile:** Calendario Enero-Diciembre (no se puede eliminar ni renombrar)
- Se crean Time Profiles adicionales para anos fiscales diferentes
- **Propiedades del Profile:**
  - **Name:** Maximo 100 caracteres, libre
  - **Fiscal Year Start Date:** Fecha de inicio del ano fiscal
  - **Fiscal Year is First Period's Calendar Year:** True/False (defecto False)
  - **Fiscal Year Month Type:** Calendar Month, Calendar Quarter, Fixed Weeks 4-4-5 / 4-5-4 / 5-4-4, Custom Start Dates
- **Variables de formato:** `|fyfy|` = ano fiscal 4 digitos, `|cycy|` = ano calendario 4 digitos, `|fy|`/`|cy|` = 2 digitos
- **CRITICO:** La Time Dimension se configura antes de crear la aplicacion y **no se puede alterar despues**. Se necesita crear una nueva aplicacion si se requiere una dimension temporal diferente.

---

### Objetivo 201.1.5: Stored y Dynamic Calculations

#### Stored Calculations (en el DUCS)

Se ejecutan dentro del Data Unit Calculation Sequence (DUCS) cuando se lanza Calculate o Consolidate. Los datos resultantes **se almacenan** en las tablas DataRecord de la base de datos (tablas `DataRecord1996` a `DataRecord2100`, una por ano).

**Finance Business Rules:**
- Se asignan al Cube (hasta 8 Business Rules)
- Tienen acceso a **Finance Function Types:**
  - `FinanceFunctionType.Calculate` (solo en C#Local)
  - `FinanceFunctionType.CustomCalculate`
  - `FinanceFunctionType.Translate`
  - `FinanceFunctionType.ConsolidateShare`
  - `FinanceFunctionType.ConsolidateElimination`
  - `FinanceFunctionType.DataCell` (para Cube Views)
  - `FinanceFunctionType.MemberList`
  - `FinanceFunctionType.ConditionalInput`
  - `FinanceFunctionType.OnDemand`
- Ventajas: Control via Finance Function Types, logica centralizada, variables compartidas

![Finance Business Rule Editor mostrando la estructura de codigo](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p31-720.png)

![Finance Function Types disponibles en Business Rules](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p31-721.png)

**Member Formulas:**
- Se asignan en la propiedad Formula de Account, Flow, o UD Members
- **Formula Pass (1-16)** define el orden de ejecucion dentro del DUCS
- Formulas en el mismo Pass y misma dimension se ejecutan en paralelo (multi-threaded)
- Pueden variar por Scenario Type y/o Time sin codigo adicional
- Solo admiten `FinanceFunctionType.Calculate` y `FinanceFunctionType.DynamicCalc`

![Member Formula Editor mostrando formula con variacion por Scenario Type y Time](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p32-728.png)

- **Is Consolidated:** Debe ser **True** si se desea que datos calculados se consoliden (el default "Conditional" NO consolida datos con Formula Type)

![Member Formula con Formula Pass asignado](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p41-789.png)

**Diferencia Consolidation vs Calculation:**
- **Calculation:** Solo ejecuta el DUCS para el Data Unit seleccionado
- **Consolidation:** DUCS + Agregacion a Parent Entities + Currency Translations + IC Eliminations

#### Custom Calculate (fuera del DUCS)

![Custom Calculate Step en Data Management](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p49-844.png)

- Se ejecuta solo via **Data Management Step** (Custom Calculate Step Type)
- No ejecuta todo el DUCS; solo el script definido
- **No considera Calculation Status;** se procesan solo los Data Units definidos explicitamente
- Requiere definir **Business Rule Name** y **Function Name**
- Parametros opcionales (Name Value Pairs) y POV de Account-level Dimensions

![Custom Calculate con definicion de Data Unit y parametros](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p49-845.png)

- Requiere **clear data script** al inicio (`api.Data.ClearCalculatedData`) - no se limpia automaticamente
- Los datos pueden marcarse como **isDurable = True** para protegerlos de la limpieza del DUCS
- Ideal para Planning; permite calculos "quirurgicos" sin reprocessar todo
- **No se puede llamar** desde Cube Views (solo desde DM o Workflow Process Step via DM)

**Storage Types de datos:**
- Input, Journal, Calculation, DurableCalculation, Consolidation, Translation, StoredButNoActivity

#### Dynamic Calculations

![Dynamic Calc con Formula Type y Account Type = DynamicCalc](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch06-p118-1346.png)

- **No se ejecutan** en el DUCS ni via Custom Calculate
- Se calculan **on-the-fly** solo cuando se consultan en un reporte (Cube View, Excel Quick View)
- Se configuran con `Formula Type = DynamicCalc` en el Member
- Para Accounts: **Account Type Y Formula Type** deben ser DynamicCalc
- **No almacenan resultado;** calculan cada vez que se muestran
- **DynamicCalcTextInput:** Formula dinamica + permite input de texto (Annotations)
- Uso ideal: Ratios, porcentajes, metricas derivadas que no requieren almacenamiento
- **Precaucion:** Uso excesivo impacta rendimiento de reportes
- No ejecutan en: Data Exports, Data Buffers, Data Unit Method Queries

#### DUCS vs. Custom Calculate - Cuando usar cada uno

| Caracteristica | DUCS (Consolidation) | Custom Calculate |
|---|---|---|
| Ambito | Todo el Data Unit | Solo el script definido |
| Limpieza automatica | Si (Clear Calculated Data) | No (manual con script) |
| Calculation Status | Respeta OK/CN | No considera |
| Dependencias | Automaticas | Manual |
| Data Storage | Calculation | DurableCalculation (si isDurable=True) |
| Uso recomendado | **Consolidation solutions** | **Planning solutions** |
| Trigger | DM, Cube View, Dashboard, Workflow | Solo Data Management |
| Performance | Todo el DUCS cada vez | Solo lo necesario |

**Regla general:** Consolidation usa principalmente DUCS; Planning usa principalmente Custom Calculate + `C#Aggregated`.

#### C#Aggregated

![C#Aggregated en DM mostrando como especificar el Consolidation Member](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p51-859.png)

- Version simplificada del algoritmo de Consolidation
- DUCS solo se ejecuta en Base Entities
- **No realiza:** IC Eliminations, Share, Ownership Calculations, Parent Journals
- Solo Direct Method Translation
- Datos se almacenan en `C#Aggregated` en Parent Entities
- Puede ser hasta **90% mas rapido** que Consolidation normal

![Datos en C#Aggregated vs C#Local mostrando que Local no tiene datos en Parent](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch08-p195-1917.png)

#### Data Buffers

Un Data Buffer es un subconjunto de celdas dentro de un Data Unit. Es un objeto VB.NET con propiedades y metodos.

![Anatomia de un Data Buffer mostrando Common Members, Cell POV, Amount, Status](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch03-p66-956.png)

- Se declaran con `api.Data.GetDataBuffer()` o `api.Data.GetDataBufferUsingFormula()`
- **Common Members:** Dimensions compartidos por todas las filas del buffer (los no-Data Unit Dimensions)
- Funciones utiles: `FilterMembers`, `RemoveMembers`, `RemoveNoData`, `RemoveZeros`
- **Siempre** usar `RemoveZeros` o `RemoveNoData` al declarar un Data Buffer (mejora rendimiento)
- `FilterMembers` y `RemoveZeros` solo funcionan con `GetDataBufferUsingFormula` (no con `GetDataBuffer`)

#### Referencia entre Business Rules

- Se puede llamar Business Rules desde otras Rules o Member Formulas
- Configurar `Contains Global Functions for Formulas = True` en la Rule compartida

![Configuracion de Contains Global Functions for Formulas = True](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p35-750.png)

- Instanciar: `Dim sharedFinanceBR As New OneStream.BusinessRule.Finance.RuleName.MainClass`

---

## Puntos Criticos a Memorizar

### Cube Properties
- El nombre del Cube **no se puede cambiar** despues de crearlo (maximo 100 caracteres)
- `Is Top Level Cube for Workflow = True` solo en **un** Cube
- Una vez que un Scenario Type tiene datos, no se puede crear un nuevo Cube Root Workflow ni cambiar su suffix sin Reset Scenario
- Consolidation Algorithm Type por defecto: **Standard** (Calc-on-the-fly Share)
- Translation Algorithm Type por defecto: **Standard**
- Rule Type por defecto: Revenue/Expenses = **Periodic**, Assets/Liabilities = **Direct**
- Hasta **8 Business Rules** se pueden asignar al Cube
- `Clear Calculated Data During Calc` por defecto = **True** en el Scenario
- NoData Calculate Settings: False por defecto (excepto Local). True cuando se copia datos de otro periodo/scenario.

### Cube Dimensions
- Entity y Scenario solo se asignan en el **(Default)** Scenario Type
- **Siempre asignar RootXXXDim** a Dimension Types no utilizados (no dejar "Use Default")
- Cambio de RootXXXDim a una Dimension especifica = **irreversible** si hay datos
- (Use Default) con datos = **bloqueado permanentemente**
- Una vez asignada y con datos, la Dimension **no se puede cambiar** sin Reset Scenario

### Data Units
- Definicion: Cube + Entity + Parent + Consolidation + Scenario + Time
- Data Units > 1 millon registros = inspeccion necesaria
- Data Units > 2 millones = red flag
- El DUCS es **all or nothing** (todos los pasos, cada vez)
- El DUCS puede ejecutarse hasta **7 veces** por Entity durante Consolidation
- Sibling Entities se procesan en **paralelo**
- Use **Calculate With Logging** para diagnosticar (agrega tiempo significativo)

### FX Rates
- Almacenadas en el **System Cube** como repositorio central
- 4 Rate Types predefinidos: Average, Opening, Closing, Historical
- Se pueden crear Rate Types personalizados
- **Direct:** Valor * Tasa (periodo actual) - Assets/Liabilities por defecto
- **Periodic:** Considera traduccion de periodos anteriores - Revenue/Expenses por defecto
- **Lock FX Rates:** Previene cambios accidentales; se audita en Task Activity
- Scenario puede sobreescribir FX settings del Cube con `Use Cube FX Settings = False`
- **Constant Year for FX Rates** en Scenario para Constant Currency

### Dimension Members
- Account Type: **No existe tipo Equity** - usar Liability
- `Is Consolidated = Conditional` significa que datos calculados **NO se consolidan** (cambiar a True si se desea)
- DynamicCalc no se almacena; calcula cada vez que se consulta
- Para Accounts: DynamicCalc requiere **Account Type Y Formula Type** = DynamicCalc
- `Allow Input = False` en Parents y cuentas calculadas
- `Aggregation Weight = 0` evita doble conteo en jerarquias multiples
- Nombres unicos dentro de Dimension Type; maximo 500 caracteres
- Datos siempre almacenados en **V#YTD** (aunque se ingresen en Periodic)
- UD Attribute Members: ~2000 = limite de rendimiento. No almacenan datos.
- Time Dimension se configura **antes** de crear la aplicacion y **no se puede alterar despues**

### Calculations
- **DUCS:** All or nothing, orden fijo, automatiza limpieza y dependencias
- **Custom Calculate:** Solo via Data Management, requiere script de limpieza manual, permite isDurable
- **Dynamic Calc:** On-the-fly, no se almacena, solo al consultar reportes
- **Formula Pass 1-16:** Orden de ejecucion dentro del DUCS
- Formulas en el mismo Pass y misma Dimension se ejecutan en **paralelo** (multi-threaded)
- Business Rules se ejecutan **secuencialmente** como estan escritas
- Consolidation = DUCS + Agregacion + Traduccion + Eliminaciones
- Calculation = Solo DUCS
- Planning --> Custom Calculate + C#Aggregated (hasta 90% mas rapido)
- Consolidation --> DUCS completo

---

## Mapeo de Fuentes

| Objetivo | Libro/Capitulo |
|----------|---------------|
| 201.1.1 | Design Reference Guide - Chapter 8: Cubes (Cube Properties, Cube Dimensions, Data Access, Integration); Foundation Handbook - Chapter 3: Design and Build (Cube Design Options) |
| 201.1.2 | Finance Rules - Chapter 1: Finance Engine Basics (Data Unit, DUCS, Parallel Processing, Calculation Status); Finance Rules - Chapter 2: Cube Data (Data Unit, Data Buffers); Finance Rules - Chapter 7: Troubleshooting (Calculate With Logging, Performance); Foundation Handbook - Chapter 3 (Data Unit sizing, extensibility) |
| 201.1.3 | Design Reference Guide - Chapter 8: Cubes (FX Rates, Lock FX Rates, Scenario FX Rates, Flow Processing); Finance Rules - Chapter 1 (Translation Algorithm Types); Foundation Handbook - Chapter 3 (Flow Processing, Alternate Input Currency) |
| 201.1.4 | Design Reference Guide - Chapter 8: Cubes (Entity, Account, Scenario, Flow, UD Dimensions, Origin, View, Time Profiles, Attribute Members); Foundation Handbook - Chapter 3 (Dimension Design considerations) |
| 201.1.5 | Finance Rules - Chapter 1: Finance Engine Basics (DUCS, Custom Calculate, Dynamic Calculations, C#Aggregated, Business Rules vs Member Formulas); Finance Rules - Chapter 2: Cube Data (Data Buffers, Storage Types); Foundation Handbook - Chapter 8: Rules and Calculations (DUCS detail, performance comparison) |
