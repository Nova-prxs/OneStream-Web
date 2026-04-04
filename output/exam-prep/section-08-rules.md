# Seccion 8: Rules (16% del examen)

**ESTA ES LA SECCION CON MAYOR PESO DEL EXAMEN.**

## Objetivos del Examen

- **201.8.1**: Demonstrate an understanding of the proper use case for various business rule types
- **201.8.2**: Demonstrate an understanding of the proper use case for various function types

---

## Conceptos Clave

### Objetivo 201.8.1: Casos de uso para los distintos tipos de Business Rules

OneStream organiza las Business Rules por el Engine con el que interactuan. Cada tipo tiene un proposito especifico y se aplica de manera diferente en la aplicacion. La Business Rule Library categoriza las reglas por Engine.

![Business Rule Library - categorias por Engine](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p25-684.png)

![Business Rule Editor - componentes y funciones clave](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p25-685.png)

**Lenguaje de programacion**: VB.NET es el lenguaje estandar para Business Rules. C# esta disponible tambien para Business Rules (pero NO para Member Formulas, que requieren VB.NET). La decision del lenguaje depende del cliente y sus recursos internos. OneStream recomienda consistencia en el lenguaje elegido para toda la aplicacion.

![Opciones de lenguaje de programacion - VB.NET vs C#](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p234-2402.png)

---

#### 1. Finance Business Rules

**Proposito**: Calculos financieros, listas de miembros, traducciones personalizadas, logica de ownership y eliminacion, input condicional, reglas on-demand, y mucho mas. Es el tipo de regla mas versatil y fundamental del Finance Engine.

**Donde se crean**: Application > Tools > Business Rules > Finance.

![Finance Rules en la Business Rule Library](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p30-713.png)

![Finance Rules agrupadas - cada regla es un objeto independiente con codigo VB.NET](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p30-714.png)

**Como se activan**: Se asignan al Cube (hasta **8 Finance Business Rules** por Cube). Se ejecutan cuando ocurre un evento financiero en el Cube (Calculation, Consolidation, Translation, o cuando se renderiza un reporte).

![Asignacion de hasta 8 Finance Business Rules al Cube](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p40-783.png)

**Excepciones de activacion**: Los Finance Function Types `MemberList` y `DataCell` no requieren asignacion al Cube; se pueden activar directamente desde una Cube View. El Finance Function Type `Confirmation` se puede activar desde una Confirmation Rule.

**Estructura del codigo**:
- Pre-defined Namespace, Public Class, Public Function que el platform invoca.
- Cada regla tiene acceso a la API library especifica del Finance Engine.
- IntelliSense disponible para autocompletado.
- In-Solution Documentation con context-sensitive help, snippets y code examples.

![Estructura de codigo: Namespace, Class, Function pre-definidos](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p31-720.png)

![Finance Function Types disponibles en Business Rules](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p31-721.png)

![Finance Function Types - diagrama visual de casos de uso](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p236-2416.png)

**In-Solution Documentation y IntelliSense**:

![In-Solution Documentation - context-sensitive help y snippets](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p28-700.png)

![IntelliSense - autocompletado de funciones y parametros](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p28-701.png)

**Ventajas sobre Member Formulas**:
- Acceso a TODOS los Finance Function Types (especialmente Custom Calculate, que NO esta disponible en Member Formulas).
- Reutilizacion de variables y objetos a lo largo de toda la regla.
- Todas las Calculations en un solo lugar cuando el volumen es alto.
- Capacidad de compartir logica entre reglas (`Contains Global Functions for Formulas = True`).
- Mayor flexibilidad para Finance Function Types especializados: Translate, ConsolidateShare, ConsolidateElimination, ConditionalInput, OnDemand.

**Compartir logica entre reglas**:

![Crear Shared Business Rule](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p35-749.png)

![Propiedad Contains Global Functions for Formulas = True](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p35-750.png)

![Funciones publicas dentro de Shared Rule](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p35-751.png)

Para llamar desde un Member Formula:
```vb
Dim sharedFinanceBR As New
OneStream.BusinessRule.Finance.A1_SharedRules.MainClass
```

**Cuando usar Finance Rules vs Member Formulas** (IMPORTANTE para el examen):

| Criterio | Finance Business Rules | Member Formulas |
|----------|----------------------|-----------------|
| **Finance Function Types** | TODOS (Calculate, Custom Calculate, Translate, Share, Elimination, DataCell, MemberList, ConditionalInput, OnDemand) | Solo Calculate y DynamicCalc |
| **Lenguaje** | VB.NET o C# | Solo VB.NET |
| **Ubicacion** | Business Rule Library centralizada | Propiedad Formula del miembro individual |
| **Variacion por Scenario/Time** | Requiere codigo (If statements) | Out-of-the-box via propiedades (dropdown menus) |
| **Formula Pass** | Se asigna posicion 1-8 en el Cube | Se asigna FormulaPass 1-16 al miembro |
| **Multi-threading** | Se ejecutan secuencialmente como estan escritas | Multithreaded dentro de cada pass (paralelo) |
| **Rendimiento out-of-the-box** | Menor (secuencial) | Mayor (paralelo) |
| **Mantenimiento alto volumen** | Mas facil (todo en un lugar) | Mas engorroso (miembro por miembro) |
| **Uso tipico** | Planning/Forecast (driver-based, factor-based, grupos grandes) | Consolidation/Actual (calculos especificos: retained earnings, KPIs) |
| **Custom Calculate** | SI | NO |
| **Calculation Drilldown** | No directamente | SI (Formula for Calculation Drill Down) |

**Regla general**:
- **Consolidation/Actual**: tienden a usar mas **Member Formulas** (calculos especificos por miembro: retained earnings, KPIs, DSO, DPO).
- **Planning/Forecast**: tienden a usar mas **Business Rules** (calculos que abarcan grandes grupos de miembros: driver-based, factor-based, zero-based).
- En la practica, se usa una **mezcla de ambos** en cualquier aplicacion bien configurada.

---

#### 2. Parser Business Rules

**Proposito**: Parsear y transformar datos entrantes durante un evento de importacion. Usos comunes: parsear campos debit/credit de un archivo GL fuente, trimming de caracteres, concatenacion, derivar un source ID del nombre del archivo fuente.

**Como se aplican**: Se configuran en una Data Source Dimension estableciendo Logical Operator = Business Rule y definiendo el nombre de la Parser Rule en Logical Expression.

![Parser Rule - configuracion en Data Source Dimension](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p236-2418.png)

**Cuando se activa**: Cuando el Stage Engine lee la Data Source configurada durante un evento de importacion.

---

#### 3. Connector Business Rules

**Proposito**: Facilitar integraciones para extraer datos de bases de datos externas, data warehouses o tablas auxiliares de OneStream hacia un Workflow. Tambien habilitan drill back a datos de detalle del sub-ledger.

**Como se aplican**: Se asignan directamente a una Connector Data Source; se activan cuando el usuario hace clic en Import en un Workflow. Tipicamente usan una External Connection configurada en el Application Server.

**ConnectorActionTypes** (4 tipos - MEMORIZAR):

| ConnectorActionType | Descripcion | Cuando se activa |
|---------------------|-------------|------------------|
| `GetFieldList` | Define los nombres de campos que se devuelven en la tabla de datos fuente | Cuando el Stage Engine lee la Data Source al importar |
| `GetData` | Procesa (consulta) los datos fuente entrantes | Durante la importacion de datos |
| `GetDrillbackTypes` | Habilita drill back personalizado a detalle externo | Al configurar tipos de drill back |
| `GetDrillBack` | Maneja el procesamiento de drill back | Cuando el usuario ejecuta drill back |

![Connector Business Rule - 4 ConnectorActionTypes](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p237-2427.png)

---

#### 4. Smart Integration Function Rules

**Proposito**: Ejecutar funciones remotas para integraciones con Smart Integration Connector. Permite conectividad segura entre OneStream Cloud y data sources en la red del cliente sin VPN.

**Disponibilidad**: Private preview en 7.2, mas usada en 7.4, GA en 8.0+.

**Capacidades**:
- Conectividad segura sin VPN entre OneStream Cloud y data sources del cliente.
- Crear y gestionar conexiones de data sources via interfaces de administracion de OneStream.
- Gestion local de credenciales de base de datos y archivos auxiliares.
- Permite codificar y almacenar centralmente funciones remotas llamadas desde Connector o Extender Rules.

---

#### 5. Conditional Rules

**Proposito**: Mapear condicionalmente un valor fuente a un valor destino usando logica de codigo. Caso comun: establecer el target value dinamicamente basado en el transformed target o source member de una dimension de Stage.

**Como se aplican**: Se asignan a una Transformation Rule individual (composite, range, list, mask). Se activan cuando la transformacion se ejecuta durante un evento de importacion.

**Argumentos clave**: `Args.GetTarget()`, `Args.GetSource()`.

![Conditional Rule - uso de Args.GetTarget() y Args.GetSource()](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p238-2434.png)

**Otros use cases**: Usar propiedades de metadata de un target dimension member para determinar el mapeo de otra dimension. Ejemplo: mapear UD1 members dependiendo de si el registro se mapea a una intercompany account.

**NOTA CRITICA**: Son **muy intensivas en procesamiento y rendimiento**. Usar con extremo cuidado y solo cuando no hay alternativa.

---

#### 6. Derivative Business Rules

**Proposito**: Dos funciones principales:
1. **Derivar o agregar un registro** a Stage y calcular su monto:
   - **Interim** = temporal, no se transforma, se usa para validaciones de check rules.
   - **Final** = se transforma y carga al Cube.
2. **Habilitar check rules** de validacion de datos (pass/fail) que se ejecutan en el paso Validate del Workflow (ej. verificar que el trial balance este balanceado antes de permitir al usuario completar el paso Validate).

**Como se aplican**: Se asignan a una Derivative Transformation Rule en el Logical Operator setting. Se activan en Import (derivacion de registros) y Validate (check rules).

---

#### 7. Cube View Extender Business Rules

**Proposito**: Personalizar y formatear reportes PDF de Cube Views (logo, numero de pagina, titulo, header font, word wrapping, font color, cell value, etc.). Capacidad de formateo practicamente ilimitada.

**IMPORTANTE**: La logica SOLO se aplica cuando la Cube View se ejecuta como **PDF report**. **NO funciona** en modo Data Explorer grid.

**Como se aplican**: En la Cube View, Custom Report Task = Execute Cube View Extender Business Rule, y se asigna el nombre de la regla.

![Cube View Extender - configuracion en Cube View](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p239-2442.png)

---

#### 8. Dashboard Dataset Business Rules

**Proposito**: Crear datasets y data tables personalizados para parametros avanzados, reportes y dashboards. Pueden ejecutar SQL queries, Method queries y BRAPIs para construir data tables desde cero o personalizar data tables existentes.

**Como se aplican** (dos maneras):
1. Directamente en un **Data Adapter** para reportes de dashboard.
2. En un **Bound List Parameter** para crear listas personalizadas de selecciones.

![Dashboard Dataset en Data Adapter](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p239-2444.png)

![Dashboard Dataset en Bound List Parameter](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p240-2452.png)

**Caso de uso ideal**: Cuando se necesita un data table personalizado para un parametro, o consultar una tabla de OneStream/externa para un reporte. Si puedes dominar esta tecnica, podras entregar practicamente cualquier solicitud de reportes personalizados.

---

#### 9. Dashboard Extender Business Rules

**Proposito**: Crear dashboards interactivos altamente personalizados, incluyendo acciones de clic en botones. Casos comunes: enviar emails, ejecutar procesos de Workflow, presentar mensajes personalizados a usuarios.

**Tres Function Types (use cases) - MEMORIZAR**:

| Function Type | Se activa cuando... | Donde se aplica |
|--------------|---------------------|-----------------|
| `LoadDashboard` | El dashboard intenta renderizarse | Directamente al dashboard (solo principal, NO nested) |
| `ComponentSelectionChanged` | Se hace clic/seleccion en un componente (boton, combo box, chart, Cube View, grid view) | En casi cualquier componente de dashboard |
| `SQLTableEditorSaveData` | Se hace clic en Save en un SQL Table Editor | En un SQL Table Editor component |

![Dashboard Extender - 3 Function Types](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p240-2454.png)

**Restriccion importante**: El Dashboard Extender solo puede activarse en un **dashboard principal**. NO puede aplicarse ni activarse en dashboards que estan **nested** dentro de un dashboard principal.

---

#### 10. Dashboard XFBR String Business Rules

**Proposito**: Reglas que **retornan texto basado en logica**. Pueden usarse practicamente en cualquier lugar de la aplicacion que espera texto: report books, formatting properties, headers de Cube Views, componentes de dashboards, etc.

**Casos de uso comunes**:
- POV member dinamico en Cube View
- Formato dinamico de Cube View o dashboard
- Member Filter de fila/columna en Cube View
- Shared template de fila/columna en Cube View
- Default value de parametro
- Obtener data cell amounts para calculos de Specialty Planning

**Ejemplo practico**: Reporte con 3 columnas (Actual, Current Forecast, Prior Forecast). El usuario solo selecciona el mes actual. Un XFBR String Rule determina dinamicamente cual es el Prior Forecast basado en la seleccion del usuario, eliminando la necesidad de seleccion manual.

**Recomendacion**: Es el **mejor punto de partida** para personas sin experiencia en codificacion, ya que no requiere entender Data Units, data buffers, ni data explosion. Solo retorna texto basado en logica simple.

---

#### 11. Extensibility Business Rules

Dos tipos principales:

**a) Extender Rules**:
- Facilitan la ejecucion de tareas automatizadas personalizadas.
- Casos comunes: automatizar importacion de datos GL en un Workflow, file management (FTP - pick up/place files), exportar datasets a CSV u otros formatos.
- Son uno de solo **dos tipos** de Business Rules que se pueden llamar directamente desde un **Data Management step**. Frecuentemente se combinan con DM para crear soluciones totalmente automatizadas.

**b) Event Handler Rules**:
- Se activan automaticamente cuando ocurre un evento especifico en la plataforma.
- Son el **UNICO tipo** de regla que NO necesita ser llamado desde otro artifact. OneStream tiene un Event Engine que escucha eventos y activa automaticamente el codigo.
- **7 tipos de Event Handler** (MEMORIZAR):
  1. **Transformation Event Handler** - intercepta eventos de transformacion (import, validate, load cube, clear Stage data)
  2. **Journal Event Handler** - eventos de journals (submit, approve)
  3. **Data Quality Event Handler** - eventos de calidad de datos
  4. **Data Management Event Handler** - eventos de Data Management
  5. **Forms Event Handler** - eventos de formularios
  6. **Workflow Event Handler** - eventos de Workflow (certify, lock, unlock)
  7. **WCF Event Handler** - eventos WCF
- Casos comunes: seed de Scenario en Process Cube, email cuando falla import, email cuando se certifica Workflow, auto-crear miembros en Dimension Library.
- Pueden **bloquear procesos** (ej. prevenir lock/certify de Workflow, lanzar mensajes de error personalizados).
- Los sub-eventos disponibles se documentan en la seccion Event Listing del OneStream API Guide.

---

#### 12. Spreadsheet Business Rules

**Proposito**: Leer o escribir en tablas de base de datos de OneStream dentro de la herramienta Spreadsheet. Permite read/write y analisis en datos de tablas auxiliares (Specialty Planning registers, custom data tables, Solution Exchange tables).

**Como se aplican**: Se crean directamente en un Spreadsheet file mediante una Table View Definition.

![Spreadsheet Business Rule - Table View Definition](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p243-2471.png)

---

#### 13. System Extender Business Rules

**Proposito**: Usadas con Azure Server Sets para escalabilidad a nivel de Azure Database y Server Sets. Determinan si se necesita scaling del servidor o base de datos.

---

#### Tabla Resumen de Todos los Tipos de Business Rules

| Tipo | Proposito Principal | Donde se aplica | Activacion |
|------|--------------------|-----------------|-----------|
| **Finance** | Calculos financieros, member lists, translations | Se asigna al Cube (hasta 8) | Calculation, Consolidation, Translation, Report rendering |
| **Parser** | Parsear datos durante importacion | Data Source Dimension (Logical Operator = BR) | Import event |
| **Connector** | Integracion con sistemas externos + drill back | Connector Data Source | Import click en Workflow |
| **Smart Integration Function** | Funciones remotas sin VPN | Llamada desde Connector/Extender | Desde otras reglas |
| **Conditional** | Mapeo condicional en transformaciones | Transformation Rule individual | Import event (transformacion) |
| **Derivative** | Derivar registros Stage + check rules | Derivative Transformation Rule | Import (derivacion) / Validate (checks) |
| **Cube View Extender** | Formateo PDF de Cube Views | Cube View (Custom Report Task) | **Solo PDF report** |
| **Dashboard Dataset** | Datasets personalizados | Data Adapter / Bound List Parameter | Dashboard rendering / Parameter |
| **Dashboard Extender** | Dashboards interactivos | Dashboard / Component / SQL Table Editor | Load, Click, Save |
| **Dashboard XFBR String** | Retorna texto basado en logica | Cualquier lugar que espera texto | Cuando se necesita el texto |
| **Extensibility Extender** | Tareas automatizadas | Data Management step | Ejecucion del DM step |
| **Extensibility Event Handler** | Tareas por evento | Ninguno (auto-activacion) | Evento de plataforma |
| **Spreadsheet** | Read/write tablas en Spreadsheet | Spreadsheet file (Table View) | Apertura del Spreadsheet |

---

#### Member Formulas (no son Business Rules, pero son la otra categoria de reglas)

**Proposito**: Calculos escritos directamente en miembros de dimensiones (Scenario, Flow, Account, UD1-UD8).

**Ubicacion**: Propiedad **Formula** del miembro en el Dimension Library.

![Member Formula Editor - acceso desde Dimension Library](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p32-727.png)

![Variacion por Scenario Type y Time - menus dropdown](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p32-728.png)

![Formula Editor - similar al Business Rule Editor](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p33-735.png)

**Tipos de Member Formulas**:
- **Stored Formula**: se ejecuta durante Calculation/Consolidation (DUCS). Se asigna a un Formula Pass.
- **Dynamic Calculation (DynamicCalc)**: se calcula on-the-fly cuando se referencia en un reporte. Formula Type = DynamicCalc.

**Propiedades clave del miembro** (CRITICO para el examen):

| Propiedad | Descripcion | Valores / Notas |
|-----------|-------------|-----------------|
| **Formula Pass** | Determina orden de ejecucion en el DUCS | FormulaPass 1-16. Miembros en el mismo pass y dimension corren en **paralelo** (multithreaded). |
| **Is Consolidated** | Controla si datos calculados se consolidan | Default: `Conditional (True if no Formula Type)` = si tiene Formula Pass, NO consolida. **Cambiar a True** si se quiere consolidar. |
| **Allow Input** | Controla si permite input de usuario | Cambiar a **False** si el Account solo tendra datos calculados. |
| **Formula Type** | Tipo de formula | `DynamicCalc` para calculos dinamicos, vacío para stored. |
| **Account Type** | Tipo de account | Tambien debe ser `DynamicCalc` para Accounts con calculos dinamicos. |
| **Switch Type** | Comportamiento de view member | True = se comporta como Revenue/Expense (Activity, QTD, MTD funcionan correctamente). |

![Member Formula - Formula Pass property asignada](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p41-789.png)

**Calculation Drilldown**: Se puede habilitar en la propiedad `Formula for Calculation Drill Down` del miembro, usando funciones de drill down API. Permite al usuario ver los inputs del calculo al hacer drill down.

![Calculation Drilldown - propiedad Formula for Calculation Drill Down](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p33-736.png)

![Calculation Drilldown - funciones API de drill down](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p34-743.png)

![Member Formula y Calculation Drilldown - vista general](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p243-2473.png)

**Ventajas**:
- Organizacion natural: el codigo esta directamente en el miembro.
- Variacion por Scenario Type y/o Time sin necesidad de codigo.
- Formula Passes multithreaded (miembros en el mismo pass corren en paralelo).
- **Mejor rendimiento out-of-the-box** vs Business Rules.

**Desventajas**:
- Solo soportan Finance Function Types: **Calculate** y **Dynamic Calc** (no Custom Calculate, Translate, Share, Elimination, etc.).
- Formula Passes no pueden variar por Scenario Type o Time (un miembro solo puede tener un Formula Pass).
- Con alto volumen de calculos, ir miembro por miembro puede ser engorroso.
- Si cambia la logica, hay que modificar cada miembro individualmente.

---

#### Assemblies (evolucion de Business Rules)

**Concepto**: Assemblies son la nueva forma de organizar Business Rules dentro de Workspaces. Casi todos los tipos de Business Rules estan disponibles como Assemblies.

**Dos tipos de Assembly**:

| Tipo | Descripcion | Finance Rules | Dynamic Content | Conversion |
|------|-------------|---------------|-----------------|------------|
| **Assembly Business Rules** | Transicion directa de Business Rules tradicionales. Mismo codigo, diferente ubicacion (Workspace > Maintenance Unit > Assembly) | NO soportan Finance BRs | NO | Conversion 1-a-1 |
| **Assembly Services** | Mas avanzadas. Usan Service Factory para routing. Recomendadas por OneStream para logica sofisticada | SI (Finance logic) | SI (Dynamic Dashboards, Dynamic Cubes) | Requiere refactoring |

**Beneficios clave de Assemblies**:
- Organizacion por area funcional (no por tipo de regla).
- Portabilidad mejorada entre entornos.
- Soporte de multithreading.
- Desarrollo con herramientas externas (Visual Studio).
- Seguridad mejorada (encryption con password).
- Compiler Language se define a nivel de Assembly (VB.NET o C#).
- Nombres de Assembly deben ser unicos dentro del Workspace.

**Referencia de invocacion** (3 formas):
1. **Traditional**: `BRName` (central repository).
2. **Assembly Business Rule**: `WS:WorkspaceName/Assembly:AssemblyName/BRName`.
3. **Assembly Service**: via Service Factory routing.

---

### Objetivo 201.8.2: Casos de uso para los distintos Finance Function Types

Los Finance Function Types determinan CUANDO y COMO se ejecuta la logica dentro de un Finance Business Rule. Son exclusivos de Business Rules (Member Formulas solo soportan Calculate y DynamicCalc).

---

#### Finance Function Types - Tabla Completa

| Finance Function Type | Descripcion | Consolidation Member | Cuando se ejecuta |
|----------------------|-------------|---------------------|-------------------|
| **Calculate** | Calculo estandar dentro del DUCS | `C#Local` | Calculation/Consolidation |
| **Translate** | Logica de traduccion de moneda personalizada | `C#Translated` | Consolidation (Translation) |
| **ConsolidateShare** | Calculo de ownership % personalizado | `C#Share` | Consolidation (Share) |
| **ConsolidateElimination** | Logica de eliminacion IC personalizada | `C#Elimination` | Consolidation (Elimination) |
| **CustomCalculate** | Calculo fuera del DUCS (on-demand) | Definido en DM step | Solo via Data Management step |
| **DynamicCalc / DataCell** | Calculo dinamico en memoria para reportes | N/A (en memoria) | Cube View/Report rendering |
| **MemberList** | Retorna lista de miembros para Cube Views | N/A | Cube View row/column rendering |
| **ConditionalInput** | Controla input condicional | N/A | Input de datos por usuario |
| **OnDemand** | Ejecucion bajo demanda | N/A | Via Data Management o dashboard |
| **Confirmation** | Logica de confirmacion | N/A | Confirmation rule trigger |

![Finance Function Types vs Consolidation Members - cuando se ejecuta cada uno](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p40-782.png)

---

#### Calculate (dentro del DUCS) - DETALLADO

Se ejecuta como parte del Data Unit Calculation Sequence (DUCS). Se activa mediante Consolidation o Calculation del Cube. El DUCS es **todo o nada**: TODOS los pasos se ejecutan cada vez, sin excepcion. No se puede elegir ejecutar solo un Formula Pass.

**Mecanismos de activacion del DUCS** (4 formas):
1. **Data Management**: Create Step > Calculate o Consolidate Step Type.
2. **Cube View**: Habilitando Calculate/Consolidate en propiedades de la Cube View.
3. **Dashboard Button**: Server Task que ejecuta DM Sequence o Calculation directamente.
4. **Workflow Process Step**: via Workflow Name con Process/Pre-Process steps.

![Data Management - Create Calculate Step](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p42-795.png)

![Calculation Type options: Calculate, Translate, Consolidate (con variaciones With Logging y Force)](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p43-803.png)

![Define Data Unit dimensions en el DM Step](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p43-804.png)

![Cube View - habilitar Calculate/Consolidate en propiedades](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p44-810.png)

![Dashboard Button - Server Task para Calculation](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p46-824.png)

![Workflow Process Step - Calculation Definitions tab](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p47-831.png)

**Secuencia completa del DUCS** (MEMORIZAR):
1. Clear previously calculated data (no limpia Durable Data; requiere `Clear Calculated Data During Calc = True` en Scenario).
2. Run Scenario Member Formula.
3. Perform reverse Translations by calculating Flow Members from alternate currency input Flow Members.
4. Execute Business Rules 1 y 2.
5. Execute Formula Passes 1-4 (Account formulas > Flow formulas > UD1 > UD2 > ... > UD8).
6. Execute Business Rules 3 y 4.
7. Execute Formula Passes 5-8 (Account formulas > Flow formulas > UD1 > UD2 > ... > UD8).
8. Execute Business Rules 5 y 6.
9. Execute Formula Passes 9-12 (Account formulas > Flow formulas > UD1 > UD2 > ... > UD8).
10. Execute Business Rules 7 y 8.
11. Execute Formula Passes 13-16 (Account formulas > Flow formulas > UD1 > UD2 > ... > UD8).

**Formula Passes dentro del DUCS**: Miembros en el mismo pass y misma dimension son **multithreaded** (corren en paralelo). El orden dentro de un pass es: Account formulas > Flow formulas > UD1 > UD2 > ... > UD8.

**El DUCS puede ejecutarse hasta 7+ veces por Entity durante Consolidation**:
1. Calculate Local Currency (`C#Local`)
2. Translate Local to Parent's Default Currency (`C#Translated`)
3. Calculate Translated Currency
4. Calculate `OwnerPreAdj`
5. Perform Share functions (`C#Share`)
6. Execute Elimination functions (`C#Elimination`)
7. Calculate `OwnerPostAdj`
8. Combine data from child entities to parent entity's `C#Local`
9. Calculate parent entity's local currency

**Calculation Status**: Determina si un Data Unit necesita recalcularse.
- **OK** = No Calculation Needed, Data Has Not Changed.
- **CN** = Calculation Needed, Data Has Changed.
- Regular Consolidate/Calculate respeta Calculation Status (no procesa Data Units OK).
- **Force** Consolidate/Calculate ignora Calculation Status y procesa todos los Data Units.

**Parallel Processing**: El Finance Engine procesa Sibling Data Units en paralelo (multi-threading) para optimizar tiempos. Sibling Entities se procesan simultaneamente usando multiples threads.

![Parallel Processing - Sibling Entities procesadas en paralelo](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p48-838.png)

---

#### Custom Calculate (fuera del DUCS) - DETALLADO

Se ejecuta **solo** via Data Management step (Custom Calculate Step Type). Es la herramienta principal para Planning solutions.

![Custom Calculate Step Type en Data Management](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p49-844.png)

**Diferencias clave con Calculate (DUCS)**:

| Aspecto | DUCS (Calculate) | Custom Calculate |
|---------|-------------------|-----------------|
| **Activacion** | Calculation, Consolidation, Cube View, Workflow | Solo via DM step |
| **Scope** | Todo el Data Unit (all-or-nothing) | Solo lo definido en el script |
| **Calculation Status** | Se verifica antes de ejecutar | NO se verifica; solo procesa DUs definidos |
| **Clear Data** | Automatico (paso 1 del DUCS) | Manual (`api.Data.ClearCalculatedData` al inicio) |
| **Durable Data** | No aplica (el DUCS limpia datos calculados) | `isDurable = True` para proteger datos |
| **Parametros** | No soporta | SI - parametros y POV de dimensiones Account-level |
| **Uso tipico** | Consolidation, Actual | Planning, Forecast, on-demand |
| **Formula Pass** | Determinado por posicion en el DUCS | N/A |

![Custom Calculate - definir Data Unit, Business Rule Name, Function Name](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p49-845.png)

![Custom Calculate - parametros opcionales](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p49-846.png)

![Custom Calculate - Dimension POV Members](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p50-852.png)

**Configuracion del Custom Calculate DM Step**:
- **Business Rule Name**: nombre de la Finance Business Rule.
- **Function Name**: nombre de la funcion Custom Calculate dentro de la regla.
- **Parameters**: Name Value Pairs opcionales pasados al script. Usar `XFGetValue` para recuperar con default value.
- **Dimension POV Members**: dimensiones Account-level referenciables via `api.Pov` en el script.
- **Entity, Consolidation, Time filters**: permiten multiples Members (funciona como Force Calculate).

**Reglas CRITICAS para Custom Calculate**:
1. SIEMPRE incluir `api.Data.ClearCalculatedData` al inicio del script (no se limpia automaticamente).
2. Marcar datos como `isDurable = True` para que NO sean limpiados por el DUCS.
3. NUNCA usar Data Unit Dimensions en el destination script (lado izquierdo del ADC).
4. Puede vincularse a **Dashboards** con botones que ejecutan DM Sequences con parametros de usuario.

**Cuando usar Custom Calculate vs DUCS** (PREGUNTA FRECUENTE EN EL EXAMEN):
- **Consolidation solutions**: principalmente DUCS (dependencias cascada, integridad de datos, clear & replace, eliminaciones IC).
- **Planning solutions**: principalmente Custom Calculate (iterativo, multiples usuarios, calculo selectivo, on-demand).
- **Regla general**: Consolidation = DUCS; Planning = Custom Calculate + `C#Aggregated`.

---

#### C#Aggregated - DETALLADO

Alternativa mas rapida a Consolidation completa. Usa una version modificada del algoritmo estandar de Consolidation.

![C#Aggregated - alternativa rapida a Consolidation](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p51-858.png)

![C#Aggregated - definir en Consolidation Filter del DM Step](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p51-859.png)

**Que hace `C#Aggregated`**:
- Solo ejecuta el DUCS en **Base Entities**.
- Solo Translation de metodo directo.
- Calcula percentage ownership.
- Datos se almacenan en `C#Aggregated` en Parent Entities.

**Que NO hace `C#Aggregated`**:
- NO realiza Intercompany elimination logic.
- NO ejecuta Share/Ownership Calculations complejos.
- NO procesa otros Consolidation Members (Local, Translated, Share, Elimination).
- NO considera Parent Journal adjustments.

**Rendimiento**: Puede ser hasta **90% mas rapido** que Consolidation normal.

**Uso principal**: **Planning solutions** donde no se necesitan eliminaciones IC ni ownership complejo. Combinado con Custom Calculate es la configuracion tipica para Planning.

---

#### Dynamic Calc / DataCell - DETALLADO

Se ejecuta en memoria cuando se referencia en un reporte (Cube View, Quick View). NO almacena datos en el Cube.

**Caracteristicas fundamentales**:
- NO se ejecuta durante DUCS ni Data Management.
- Trabaja celda por celda (como Excel), NO con Data Buffers.
- Hereda el POV de cada celda del reporte (18 dimensiones disponibles via `api.Pov`).
- Requiere statement `Return` seguido de un objeto (numerico o textual).
- NO agrega naturalmente a Parent Members. Para Parents, se debe escribir un Dynamic Calc directamente en el Parent.

**Donde ubicar Dynamic Calculations** (3 opciones):

| Ubicacion | Como configurar | Ventajas | Desventajas |
|-----------|----------------|----------|-------------|
| **Member Formula** | Formula Type = DynamicCalc (Account Type tambien = DynamicCalc para Accounts) | Se ejecuta siempre que el miembro se referencia. Reutilizable en cualquier Cube View. Soporta texto. | Codigo atado al miembro individual. |
| **Business Rule (DataCell)** | FinanceFunctionType = DataCell. Se llama con `GetDataCell(BR#[BRName=...,FunctionName=...])` | Flexible, no atado a miembro. Soporta Name Value Pairs. | Sintaxis mas compleja. No retorna texto. |
| **Directamente en Cube View** | `GetDataCell(...)` en filas/columnas. Soporta substitution variables y Column/Row math. | Rapido para calculos ad-hoc. | No reutilizable en otros Cube Views. No funciona en Excel Add-in ni Spreadsheet. |

**View Members para datos numericos**: YTD, Periodic, QTD, MTD, HTD.
**View Members para datos textuales**: Annotation, Assumptions, AuditComment, Footnote, VarianceExplanation.

**Funciones disponibles en Dynamic Calcs**:
- `Divide(A, B)`: division con proteccion contra division por zero.
- `Variance(A, B)`: (A - B) / |B|.
- `VariancePercent(A, B)`: (A - B) / |B| * 100.
- `BWDiff(A, B)` / `BWPercent(A, B)`: Better/Worse considerando Account Type.
- `api.Functions.GetDSODataCell("AcctsRec", "Sales")`: Days Sales Outstanding.

**Tecnica comun UD8**: Almacenar Dynamic Calcs en UD8 (dimension menos usada) para que hereden el POV de todas las demas dimensiones. No necesita asignarse al Cube.

**Name Value Pairs**: En DataCell Business Rules, permiten pasar valores desde la Cube View al Business Rule via `args.DataCellArgs.NameValuePairs`. La logica del BR puede cambiar segun el valor pasado.

**Relational Blending**: Combinar datos del Cube con datos de tablas relacionales (Stage, custom tables). Tres metodos:
- **Drill-Back Blending** (1-a-muchos): via drill back a datos de detalle.
- **Application Blending** (1-a-muchos): para MarketPlace solutions.
- **Model Blending** (1-a-1): via Finance Engine API.
- Funcion clave: `api.Functions.GetStageBlendTextUsingCurrentPOV` con caching inteligente.

---

#### Funciones y APIs clave para Calculations

##### api.Data.Calculate (ADC) - LA FUNCION MAS UTILIZADA

La funcion API mas utilizada para calculos financieros. Realiza aritmetica de Data Buffers. Un solo ADC puede crear miles de celdas calculadas.

**Sintaxis basica**:
```text
api.Data.Calculate("A#Result = A#Source1 * A#Source2")
```

**Overloaded** con tres versiones:
1. `api.Data.Calculate(FormulaString, IsDurableCalculatedData)` - basica.
2. `api.Data.Calculate(FormulaString, OnEvalDataBuffer, UserState)` - con Eval.
3. `api.Data.Calculate(FormulaString, accountFilter, flowFilter, originFilter, icFilter, ud1Filter...ud8Filter, OnEvalDataBuffer, UserState, IsDurable)` - completa con 12 filtros.

**Reglas CRITICAS del ADC** (MEMORIZAR):
- La formula string **NO es case sensitive** (`a#price` = `A#Price`).
- Errores de sintaxis dentro del formula string solo se detectan en **runtime** (no en compilacion).
- **NUNCA** incluir Data Unit Dimensions en el destination script (lado izquierdo).
- Data Unit Dimensions SI pueden usarse en source scripts (lado derecho).
- Cuando se referencia Entity en un source script, incluir `C#Local` para evitar problemas de currency.

##### Data Buffer Math - COMO FUNCIONA

OneStream analiza cada Data Buffer, empareja celdas basandose en **Common Members** (dimensiones comunes), y realiza la aritmetica. Solo celdas con **Primary Keys identicos** (excepto la dimension del Account) se operan entre si. Si no hay celdas coincidentes, no se produce resultado.

**Common Members**: Dimensiones Account-level que son compartidas por todas las celdas del Data Buffer. Solo Data Buffers con los **mismos Common Members** pueden operarse directamente (Data Buffers "balanceados").

##### Unbalanced Buffers - IMPORTANTE

Data Buffers con diferentes dimensiones comunes = **Unbalanced**. OneStream lanza error si se intenta operar Data Buffers desbalanceados directamente (para prevenir **data explosion**).

**Funciones Unbalanced**:
- `MultiplyUnbalanced(DB1, DB2, UnbalancedScript)`
- `DivideUnbalanced(DB1, DB2, UnbalancedScript)`
- `AddUnbalanced(DB1, DB2, UnbalancedScript)`
- `SubtractUnbalanced(DB1, DB2, UnbalancedScript)`

**Regla critica**: El Data Buffer con **MAS dimensiones** siempre debe ser el **segundo argumento**.

**Truco para DivideUnbalanced con numerador desbalanceado**: Convertir a MultiplyUnbalanced con `Divide(1, Denominator)`.

**Truco para SubtractUnbalanced con primer operando desbalanceado**: Usar SubtractUnbalanced con operandos invertidos y multiplicar por `-1`.

**Double-Unbalanced** (ambos buffers desbalanceados por dimensiones diferentes): NO se puede resolver con funciones Unbalanced. Usar **Data Buffer Cell Loop** con nested loops.

##### Data Explosion - QUE ES Y COMO EVITARLA

Data explosion ocurre cuando una formula resulta en la creacion masiva de celdas no deseadas. Se dispara cuando un source script contiene Dimensions no contenidas en el destination script.

**`#All` NUNCA debe usarse** en destination scripts. En el peor caso causa data explosion, en el mejor caso hay una alternativa mejor.

##### RemoveZeros / RemoveNoData

- `RemoveZeros`: remueve celdas con amount 0 Y celdas con Cell Status NoData.
- `RemoveNoData`: remueve solo celdas con Cell Status NoData.
- **Deben usarse en TODOS los calculos como practica estandar**.
- Solo funcionan con `api.Data.GetDataBufferUsingFormula` (no con `api.Data.GetDataBuffer`).
- Funcionan tambien con `FilterMembers` y `RemoveMembers`.

##### Dimension Filters en ADC

Argumentos opcionales para filtrar celdas del Data Buffer resultante. Posiciones: accountFilter, flowFilter, originFilter, icFilter, ud1Filter...ud8Filter.

**Member Filter Builder**: herramienta GUI para construir filtros correctos.

**Reglas de Dimension Filters**:
- Filtros deben contener solo **Base Members** para mejor rendimiento.
- Filtros con **miembros duplicados** causan error (Dictionary no permite keys duplicados).
- Ejemplo: `A#IncomeStatement.Base`, `U1#Top.Base.Where(Name Contains Bug)`.

##### Collapsing Detail

Incluir dimensiones en TODOS los operands del formula para colapsar detalle:
- Origin, Flow, Intercompany casi siempre se colapsan.
- `O#Import` preserva la capacidad del usuario de ajustar via BeforeAdj.
- `O#Forms` sobreescribe los datos.
- **Nunca** tener datos importados y calculados en la misma interseccion.

##### Formula Variables

Declaran un Data Buffer como variable reutilizable:
```text
api.Data.FormulaVariables.SetDataBufferVariable("VarName", dataBuffer, True)
```
- Referenciar en formula string con `$VarName`.
- Ultimo argumento `True` para "Use Indexes To Optimize Repeat Filtering" (mejora rendimiento al reutilizar con FilterMembers).
- Mejora rendimiento al llamar Data Buffer a memoria una sola vez y reutilizar multiples veces.

##### Eval y Eval2

- **Eval**: evalua celdas individuales de UN Data Buffer dentro de un ADC.
- **Eval2**: evalua y compara celdas de DOS Data Buffers.
- Se usa via subfunction (`OnEvalDataBuffer` / `OnEvalDataBuffer2`).
- `EventArgs` proporciona acceso al Data Buffer, DestinationInfo y ResultDataBuffer.
- Caso de uso Eval: filtrar celdas por Cell Amount (ej. remover > 500).
- Caso de uso Eval2: comparar Actual vs Budget para identificar productos nuevos.

##### Data Buffer Cell Loop (DBCL) - TECNICA AVANZADA

Metodo manual, long-hand de lo que ADC hace automaticamente. Maximo control y flexibilidad sobre cada celda individual.

**Ingredientes del DBCL** (MEMORIZAR):
1. New DataBuffer (Result Data Buffer vacio)
2. DestinationInfo (equivalente al lado izquierdo del ADC)
3. GetDataBuffer o GetDataBufferUsingFormula (Data Buffer fuente)
4. For Each/Next loop
5. GetDataCell (obtener valores de otras celdas)
6. New Result Cell (declarar DENTRO del loop como `New`)
7. SetCell (agregar result cell al Result Data Buffer)
8. SetDataBuffer (escribir al Cube UNA VEZ despues del loop)

**Tres formas de GetDataCell**:
1. **Member Names**: string con nombres de miembros. Mas lento (conversion name > ID).
2. **DataCellPk** (IDs): mejor rendimiento, IDs ya en memoria.
3. **MemberScriptBuilder**: segundo mejor rendimiento, convierte source cell PK.

**Reglas criticas del DBCL**:
- Result cell debe declararse **DENTRO del loop** como `New`. Si se declara fuera, solo la primera celda se agrega.
- `SetCell` con `AccumulateIfCellAlreadyExists = True` si pueden haber intersecciones duplicadas.
- `SetDataBuffer` se llama **UNA VEZ** despues del loop (NUNCA dentro).
- Todas las dimensiones del result cell deben estar definidas (via DestinationInfo o explicitamente).

**Cuando usar DBCL** (vs ADC):
- Transformar dimensiones (mapeo de una dimension a otra).
- Analizar Cell Status o Cell Amounts (logica condicional por celda).
- Nested loops para **double unbalanced** (ambos buffers desbalanceados).
- Crear multiples result cells dentro del mismo loop (combinar calculos, mejor rendimiento).
- Cuando se necesita logica que no puede expresarse con ADC.

**Performance guidelines del DBCL**:
- Mover lookups constantes (Member IDs, data cells globales) **FUERA** del loop.
- **NUNCA** usar `api.Data.Calculate` dentro del loop (multiples reads/writes a DB).
- **NUNCA** usar `api.Data.SetDataBuffer` o `api.Data.SetDataCell` dentro del loop.
- Escribir al Cube **UNA VEZ** despues del loop (wheelbarrow method).
- Usar `Count` para verificar el numero de celdas antes de loopear.

##### Durable Data

- Storage Type = `DurableCalculation`.
- No se limpia durante el DUCS (paso Clear Previously Calculated Data).
- Se activa con `isDurableData = True` en ADC o DBCL.
- Casi siempre usado con Custom Calculate.
- Siempre incluir `api.Data.ClearCalculatedData` al inicio (no se limpia automaticamente).

##### ConvertDataBufferExtendedMembers

- Convierte Data Buffer de un Cube/Scenario a la dimensionalidad de otro.
- Agrega automaticamente datos para Extended Members para crear celdas de Parent Members.
- Util para copiar datos entre Scenarios con diferente nivel de detalle (Extensibility).

---

#### Gestion y Mantenimiento de Calculations

##### Calculation Matrix

Documento centralizado para inventariar todas las Calculations. Debe crearse desde la fase de Design & Requirements y mantenerse como **living document** durante todo el proyecto.

**Campos basicos**: Nombre, Categoria, Tipo (stored/dynamic), Finance Function Type, Ubicacion (BR/Member Formula), Ejecucion, Formula (descripcion), Dependencias, Scope (Data Unit + Account-level).

![Calculation Matrix - informacion basica](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch07-p145-1536.png)

![Calculation Matrix - scope information](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch07-p146-1540.png)

**Beneficios**: tracking (reducir riesgo de olvidar calculos criticos), diseno (revision iterativa con cliente), building/testing/approving (tracking de progreso), transferencia de conocimiento (referencia para administradores).

##### Comments en codigo

- **Header**: nombre, proposito, autor, fecha, modificaciones (que se cambio y quien).
- **Inline**: comentario por cada linea o bloque de 3-5 lineas. Explicar el POR QUE, no solo el QUE.
- Siempre errar del lado de "over-commenting".

![Header comments - estructura recomendada](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch07-p151-1573.png)

##### Regions

`#Region "NombreRegion"` / `#End Region` para organizar bloques de codigo en Business Rules grandes. Permite expandir/colapsar secciones en el Business Rule Editor.

##### Tecnicas de mantenimiento

- **Time Functions**: NUNCA hardcodear periodos de tiempo. Usar `POVPriorYear`, `POVYear`, `POVPrior1`, `POVNext1`, `POVFirstInYear`, `POVLastInYear`. Preferir `T#POVLastInYear` y `T#POVFirstInYear` sobre `T#POVPriorYearM12` (funcionan en Scenarios de cualquier frecuencia).

![Time Functions disponibles en Member Filter Builder](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch07-p153-1591.png)

- **Hierarchias alternativas**: crear Parents de agrupacion para soportar Calculations y reducir mantenimiento de codigo. Crear bajo `AlternateHierarchies` (sibling de `Top`) para evitar double-counting.

- **Text Properties**: usar propiedades Text1-Text8 de miembros con clausulas `Where` en Member Filters. Ejemplo: `U1#Top.Base.Where(Text2 = SalesCalculation)`.

- **Custom SQL Tables**: almacenar logica/inputs de Calculation en tablas SQL, referenciar con `api.Functions.GetCustomBlendDataTable`. Mantenimiento se reduce a cambiar campos de tabla o agregar filas.

![Custom SQL Table - crear en Dashboard SQL Table Editor](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch07-p157-1621.png)

##### Application Reports

- **Formula Statistics**: breakdown de miembros con Member Formulas/Dynamic Calcs por dimension.
- **Formula List**: cada Member Formula con su sintaxis.

---

#### Troubleshooting y Diagnosticos

##### Task Activity

Punto de partida para verificar si un Calculation/Consolidation se completo exitosamente. Muestra status de todos los server tasks con filtrado por Task Type.

![Task Activity - acceso desde top-right menu](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch08-p161-1648.png)

##### Logging

La herramienta mas importante para debugging. Permite escribir mensajes propios al Error Log.

**Dos funciones de logging**:
- `api.LogMessage` - disponible en Finance Business Rules (MEJOR rendimiento).
- `BRApi.ErrorLog.LogMessage` - disponible en TODOS los tipos de reglas (abre nueva conexion a DB, PEOR rendimiento).
- **SIEMPRE usar `api.LogMessage` en Finance Rules**.

**LogDataBuffer**: Funcion para ver el contenido de un Data Buffer en el Error Log. Extremadamente util para diagnosticar problemas de dimensiones que no matchean.

##### Stopwatch

Para identificar que parte del codigo tarda mas. Importar `System.Diagnostics`, crear instancia con `StartNew`, log elapsed time.

##### Calculate With Logging

Habilita drill down detallado a cada paso del DUCS con duracion por paso. Permite identificar la Calculation exacta que causa lentitud.

![Calculate With Logging - activar desde Data Management](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch08-p170-1721.png)

**NOTA**: Agrega tiempo de procesamiento significativo. Solo usar para diagnosticos.

##### Errores Comunes (PARA EL EXAMEN)

| Error | Causa | Solucion |
|-------|-------|----------|
| **Calculation no produce resultados** | Data Buffers no matchean dimensionalmente | LogDataBuffer para comparar Common Members |
| **Resultados inconsistentes** | Formula Pass incorrecto (dependencia no resuelta) | Cambiar a FormulaPass 16 para aislar; revisar dependencias |
| **Compilation Error** | Typo en sintaxis VB.NET | Corregir y compilar de nuevo |
| **Invalid Formula Script** | Typo dentro del formula string de ADC | Revisar nombres de miembros (runtime error) |
| **Unbalanced Buffer** | Data Buffers con diferentes Common Members | Usar funciones Unbalanced |
| **Data Explosion warning** | Source script tiene dimensiones no en destination | Nunca usar `#All`; incluir dimension en destination |
| **Result cell solo 1 registro** | New result cell declarada fuera del loop | Mover `New` DENTRO del loop |
| **Duplicate Members in Filter** | Filtro contiene miembros duplicados (Parent + Child) | Verificar que no haya overlap |
| **Undefined Members** | DestinationInfo vacio y result cell no hereda source | Definir todas las dimensiones explicitamente |
| **Object Not Set** | Variable no inicializada usada en funcion | Inicializar variable antes de usar |
| **Given Key Not Present** | Parametro de Custom Calculate no definido en DM step | Definir parametro en DM step; usar `XFGetValue` con default |
| **Invalid Destination Script** | Data Unit Dimension en lado izquierdo del ADC | Remover; usar If statements para filtrar Data Unit |

---

#### Performance de Calculations

##### Factores que afectan el rendimiento:
1. **Hardware y Server Settings**: CPU specs (3.7 GHz hasta 2x mas rapido que 2.0 GHz), multi-threading settings en Application Server Config.
2. **Cube Design**: Data Unit Size y Volume, Extensibility, numero de Entities, Entity hierarchies, datos innecesarios en el Cube.
3. **Formula Efficiency**: codigo eficiente, eliminacion de procesamiento innecesario.

##### Server Structure tipica:
- **General Application Server**: navegacion, Cube Views, Dashboards (single-threaded, no CPU-intensive).
- **Stage Application Server**: Stage Engine (multi-threaded, CPU-intensive).
- **Consolidation Application Server**: Finance Engine (multi-threaded, CPU-intensive).
- **Data Management Server**: secuencias DM, tareas de larga duracion.

##### Cube Design para Performance:
- **Reducir Data Unit Size**: usar Extensibility, aumentar numero de Entities, eliminar datos innecesarios.
- **Optimizar Entity Hierarchies**: evitar estructuras planas con muchos children; evitar relaciones 1-a-1.
- **No guardar datos transaccionales en Cubes**: usar Specialty Planning o BI Blend.
- **Alinear Entity Dimensions con Calculations por Scenario Type**: si Actuals calculan por Legal Entity y Planning calcula por Department, considerar diferentes Entity Dimensions.

##### Cosas que HACER (Best Practices - MEMORIZAR):
1. Usar **Custom Calculate** cuando sea posible (narrower scope).
2. Alinear **Entity Dimensions** con Calculations por Scenario Type.
3. Usar **Dynamic Calculations** en vez de stored cuando sea posible (y viceversa cuando reportes son lentos).
4. Usar **RemoveZeros** en TODOS los Data Buffers.
5. **Limitar Data Unit Scope** con If statements (`api.Entity.IsBaseEntity AndAlso api.Cons.IsLocalCurrencyForEntity`).
6. **Limitar Account-level Dimension Scope** con Dimension Filters.
7. Usar **Global Variables** (`globals.SetObject`/`GetObject`) para variables que no cambian entre Data Units.
8. Usar **Formula Variables** para reutilizar Data Buffers en multiples ADC functions.
9. Usar **DimConstants** en vez de string comparisons para miembros default.
10. Usar **C#Aggregated** cuando sea posible (hasta 90% mas rapido).
11. NO usar Force Consolidate/Calculate innecesariamente.
12. Usar `api.LogMessage` en vez de `BRApi.ErrorLog.LogMessage` en Finance Rules.
13. Usar **Time Functions** (nunca hardcodear periodos).

![Data Unit If condition - best practice](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p248-2500.png)

##### Cosas que EVITAR (MEMORIZAR):
1. **api.Data.Calculate dentro de loops**: causa multiples reads/writes al database. Usar ADC con filtros o DBCL.
2. **api.Data.SetCell/SetDataCell dentro de loops**: usar "wheelbarrow method" (acumular en Result Data Buffer, escribir una vez).
3. **api.Data.ClearCalculatedData dentro de loops**: limpiar ANTES del loop.
4. **Lookup de constantes dentro de loops**: mover fuera del loop.
5. **api.Data.ClearCalculatedData en DUCS**: innecesario, ya es el primer paso del DUCS.
6. **Stacking ADC functions con logica similar**: condensar en uno solo con filtros.
7. **BRApi calls en Finance Rules**: abren nuevas conexiones al database, causan overload en multi-threading. Usar equivalentes API.
8. **Hardcodear periodos de tiempo**: usar Time Functions.
9. **Olvidar comentar/remover logging en produccion**: puede crashear servidores.
10. **Copiar datos en el DUCS**: usar Custom Calculate con isDurable = True.
11. **Datos transaccionales en Cubes**: usar Specialty Planning o BI Blend.
12. **Force Consolidate innecesariamente**: procesa Data Units que no han cambiado.

##### System Diagnostics (MarketPlace)
- Instalar en cada entorno.
- **Application Analysis**: metricas de diseno, tamano y eficiencia de formulas.
- **Data Volume Statistics**: Data Units con alto numero de registros.
- **Long Running Formulas Report**: identifica Calculations lentas en el DUCS.

---

#### Ejemplos Comunes de Calculations

##### Balance Sheet Calculations

**Current Year Net Income**:
```text
api.Data.Calculate("A#CurYearNetIncome:O#Import:F#EndBal:I#None:U1#None:U2#None =
V#YTD:A#NetIncome:O#Top:F#EndBal:I#Top:U1#Top:U2#Top")
```
- Formula Pass 2 (despues de cualquier calculo del Income Statement en Pass 1).
- Is Consolidated = **True**.
- Allow Input = **False**.
- Colapsa detalle de Origin, Flow, IC, UD1, UD2.
- Scope: Base Entities (no filtrar a Local only - debe capturar translated amounts).

**Retained Earnings Beginning Balance**:
- Copiar ending balance del ano anterior.
- Usar `api.Time.IsFirstPeriodInYear` para solo referenciar prior year una vez (mejor rendimiento).
- Para periodos subsiguientes, carry forward del periodo anterior.
- Formula Pass 1 (sin dependencias).
- Is Consolidated = **True**.
- Allow Input = **False**.

##### Flow Calculations

**Beginning Balance (BegBalCalcYTD)**:
- Pull ending balance de prior year en primer periodo.
- Carry forward para periodos subsiguientes.
- Filtrar a Balance Sheet Accounts only.

**BegBalDynamic**:
- Dynamic Calc que muestra balance correcto basado en View Member (MTD, QTD, YTD).
- Formula Type y Account Type = **DynamicCalc**.
- Resuelve el problema de que BegBalCalcYTD siempre muestra YTD.

**ActivityCalc**:
- Cambio YTD en el Account desde el beginning balance: EndBal - BegBal.
- Switch Type = **True** (para que MTD/QTD funcionen correctamente).
- Aggregation Weight = 0 para Beginning Balance y Activity (solo EndBal agrega al Top Member).

##### FX Calculations (solo para foreign currency Entities)

- **FXOpen**: efecto del cambio de tasa en el opening balance.
- **FXMovement**: efecto FX en la actividad del Account (current movement, prior movement, override movement).
- **CTA (Cumulative Translation Adjustment)**: suma del FX de todos los Balance Sheet Accounts. Se reporta en OCI.

---

## Puntos Criticos a Memorizar

### Tipos de Business Rules y sus use cases:
- **Finance**: calculos financieros, se asigna al Cube (hasta 8). UNICA con acceso a Custom Calculate, Translate, Share, Elimination.
- **Parser**: parsear datos durante importacion. Se asigna a Data Source Dimension.
- **Connector**: integracion con sistemas externos + drill back. Se asigna a Connector Data Source. 4 ConnectorActionTypes.
- **Smart Integration Function**: funciones remotas via Smart Integration Connector (sin VPN).
- **Conditional**: mapeo condicional en transformaciones. Muy intensiva en rendimiento.
- **Derivative**: derivar registros en Stage (interim/final) + check rules de validacion.
- **Cube View Extender**: formateo PDF de Cube Views **UNICAMENTE**.
- **Dashboard Dataset**: datasets personalizados. Se usa en Data Adapters y Bound List Parameters.
- **Dashboard Extender**: 3 function types (LoadDashboard, ComponentSelectionChanged, SQLTableEditorSaveData). Solo dashboard principal (no nested).
- **Dashboard XFBR String**: retorna texto basado en logica, mejor punto de partida para novatos.
- **Extensibility Extender**: tareas automatizadas, se llama desde Data Management steps.
- **Extensibility Event Handler**: 7 tipos, se activan automaticamente por eventos. **UNICO** tipo que no necesita ser llamado.
- **Spreadsheet**: read/write de tablas en Spreadsheet tool.

### Finance Function Types:
- **Calculate**: dentro del DUCS, `C#Local`.
- **Custom Calculate**: fuera del DUCS, solo via DM step, requiere `isDurable` + `ClearCalculatedData`.
- **DataCell/DynamicCalc**: en memoria, para reportes, celda por celda.
- **C#Aggregated**: hasta 90% mas rapido que Consolidation, sin eliminaciones IC.
- **Translate/ConsolidateShare/ConsolidateElimination**: para logica personalizada de Translation y Consolidation.

### DUCS (Data Unit Calculation Sequence):
- Todo o nada; 8 Business Rules intercaladas con 16 Formula Passes.
- Puede ejecutarse hasta **7+ veces** por Entity durante Consolidation (Local, Translated, Share, Elimination, etc.).
- Member Formulas son **multithreaded** dentro de cada pass.
- Business Rules se ejecutan **secuencialmente** como estan escritas.
- Primer paso: Clear Previously Calculated Data (no limpia Durable Data).
- Formula Pass orden dentro de cada pass: Account > Flow > UD1 > UD2 > ... > UD8.

### Data Buffers:
- Subconjunto de celdas dentro de un Data Unit.
- Operaciones aritmeticas solo entre Data Buffers **balanceados** (mismos Common Members).
- **RemoveZeros** siempre.
- Funciones Unbalanced: segundo argumento = Data Buffer con mas dimensiones.
- **NUNCA** usar `#All` en destination scripts.
- Double-unbalanced: usar DBCL con nested loops.

### ADC vs DBCL:
- **ADC**: mas simple, automatico, ideal para la mayoria de calculos.
- **DBCL**: mas flexible, control celda por celda, necesario para transformaciones de dimensiones, analisis de Cell Status, nested loops.
- **Performance**: un DBCL puede reemplazar multiples ADC (una sola escritura vs multiples).

### Assemblies:
- Assembly Business Rules: transicion 1-a-1, sin Finance Rules, sin Dynamic content.
- Assembly Services: incluyen Finance, Dynamic content. Usan Service Factory.
- Compiler Language se define a nivel de Assembly (VB.NET o C#).

### Best Practices de Performance:
- Consolidation = DUCS + Member Formulas.
- Planning = Custom Calculate + `C#Aggregated` + Business Rules.
- NUNCA `api.Data.Calculate` dentro de loops.
- NUNCA escribir al Cube dentro de loops.
- SIEMPRE limitar Data Unit scope con If statements.
- SIEMPRE usar RemoveZeros.
- SIEMPRE usar Time Functions (no hardcodear periodos).
- Usar Global Variables para objetos costosos que no cambian entre Data Units.
- Usar `api.LogMessage` (no `BRApi.ErrorLog.LogMessage`) en Finance Rules.

---

## Mapeo de Fuentes

| Objetivo | Libro/Capitulo |
|----------|---------------|
| 201.8.1 (Business Rule types) | Foundation Handbook - Chapter 8: Rules and Calculations (tipos de reglas, use cases) |
| 201.8.1 (Business Rule types) | Finance Rules - Chapter 1: Introduction + Finance Engine Basics (Finance Rules, Member Formulas, Engines) |
| 201.8.1 (Business Rule types) | Workspaces & Assemblies - Chapter 5: Understanding Assemblies |
| 201.8.1 (Business Rule types) | Workspaces & Assemblies - Chapter 6: Managing Assembly Business Rules |
| 201.8.1 (Business Rule types) | Design Reference Guide - Chapter 8: Cubes (Business Rules section) |
| 201.8.2 (Function types) | Finance Rules - Chapter 1: Finance Engine Basics (DUCS, Finance Function Types, Custom Calculate, C#Aggregated) |
| 201.8.2 (Function types) | Finance Rules - Chapter 2: Cube Data (Data Cells, Data Units, Data Buffers, Storage Types) |
| 201.8.2 (Function types) | Finance Rules - Chapter 3: api.Data.Calculate (ADC, Data Buffer Math, Unbalanced, Filters, Eval, Durable Data) |
| 201.8.2 (Function types) | Finance Rules - Chapter 4: Data Buffer Cell Loop (DBCL, GetDataCell, SetDataBuffer) |
| 201.8.2 (Function types) | Finance Rules - Chapter 5: Reporting Calculations (Dynamic Calcs, GetDataCell, DataCell BR, UD8, Relational Blending) |
| 201.8.2 (Function types) | Finance Rules - Chapter 6: Managing Calculations (Calculation Matrix, Comments, Maintenance, SQL Tables) |
| 201.8.2 (Function types) | Finance Rules - Chapter 7: Troubleshooting and Performance (Logging, Errors, Performance best practices) |
| 201.8.2 (Function types) | Finance Rules - Chapter 8: Common Rule Examples (Balance Sheet, Flow, Retained Earnings, CTA, EPU) |
| 201.8.2 (Function types) | Foundation Handbook - Chapter 8: Rules and Calculations (DUCS detail, api.Data.Calculate overloads, best practices) |
