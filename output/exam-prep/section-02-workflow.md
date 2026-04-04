# Seccion 2: Workflow (14% del examen)

## Objetivos del Examen

- **201.2.1:** Aplicar el Workflow Name apropiado para una situacion dada
- **201.2.2:** Aplicar una Calculation/Consolidation en el Workflow
- **201.2.3:** Demostrar comprension de la configuracion del Workflow Profile
- **201.2.4:** Discutir el proposito principal de varias configuraciones de Workflow menos utilizadas

---

## Conceptos Clave

### Objetivo 201.2.1: Aplicar el Workflow Name apropiado para una situacion dada

#### Que es el Workflow Engine

El **Workflow Engine** es el coordinador de toda la actividad en el sistema. Sus responsabilidades principales son:

- Proteger al usuario final de las complejidades del modelo analitico multidimensional
- Gestionar y auditar la recoleccion de datos
- Gestionar y hacer cumplir el proceso de calidad junto con la certificacion de datos
- Gestionar y coordinar inteligentemente el proceso de consolidacion
- Entregar informacion en tiempo real y analisis guiado
- Crear una experiencia estandarizada para el usuario con capacidad de personalizacion via Workspaces

La razon primaria de existencia del Workflow es alimentar y cuidar los Analytic Cubes. Por ello, antes de crear una jerarquia Workflow, debe existir al menos un Cube marcado como **Is Top Level Cube For Workflow = True**.

![Pantalla principal de Workflow en OnePlace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1928-7259.png)

#### Que es el Workflow Name

El **Workflow Name** controla las tareas que los usuarios deben completar en el Workflow. Se configura en **Workflow Settings** de cada Input Child Profile. Las tareas pueden variar por **Scenario** y **Input Type**.

Cada tipo de Input Child (Import, Form, Journal) tiene su propio conjunto de Workflow Names disponibles.

![Configuracion de Workflow Name en propiedades del Profile](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p813-3695.png)

![Tareas resultantes segun Workflow Name seleccionado](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p813-3696.png)

#### Workflow Names para Import

| Workflow Name | Tareas incluidas | Caso de uso |
|---|---|---|
| **Import, Validate, Load** | Importar archivo, Validar mapeos/intersecciones, Cargar al Cube | Carga basica sin certificacion ni procesamiento |
| **Import, Validate, Load, Certify** | Import + Validate + Load + Certificacion | Carga con firma de certificacion simple |
| **Import, Validate, Process, Certify** | Import + Validate + Process Cube + Certificacion | Carga con calculo automatico antes de certificar |
| **Import, Validate, Process, Confirm, Certify** | Import + Validate + Process + Confirmacion + Certificacion | Proceso completo con Data Quality checks |
| **Central Import** | Para imports centralizados (corporativo) | Cuando corporativo carga datos a entities ajenas |
| **Workspace** | Usa un Dashboard personalizado como interfaz | Workflows de planning con interfaz personalizada |
| **Workspace, Certify** | Dashboard personalizado + Certificacion | Workspace con paso de certificacion |
| **Import Stage Only** | Solo importa datos al Stage (no carga al Cube) | Exportacion de datos de OneStream a sistema externo |
| **Import, Verify Stage Only** | Importa y valida en Stage solamente | Validacion sin carga al Cube |
| **Import, Verify, Certify Stage Only** | Importa, valida en Stage y certifica | Validacion y certificacion sin carga al Cube |

**Workflow Names para Direct Load (sin almacenamiento en Stage):**

| Workflow Name | Descripcion |
|---|---|
| **Direct Load** | Carga directa in-memory sin almacenar en Stage |
| **Direct Load, Certify** | Direct Load con certificacion |
| **Direct Load, Process, Certify** | Direct Load con procesamiento y certificacion |
| **Direct Load, Process, Confirm, Certify** | Direct Load completo con confirmacion |

![Configuracion Direct Load Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p850-3799.png)

#### Workflow Names para Forms

| Workflow Name | Tareas incluidas | Caso de uso |
|---|---|---|
| **Form Input** | Solo entrada de datos en formulario | Entrada simple sin pasos adicionales |
| **Form Input, Certify** | Entrada + Certificacion | Entrada con firma de certificacion |
| **Form Input, Process, Certify** | Entrada + Process Cube + Certificacion | Entrada con calculo y certificacion |
| **Form Input, Process, Confirm, Certify** | Entrada + Process + Confirmacion + Certificacion | Proceso completo con DQ |
| **Pre-Process, Form Input** | Pre-proceso (calculo previo) + Entrada | El Pre-Process calcula datos antes de mostrar el formulario |
| **Pre-Process, Form Input, Certify** | Pre-proceso + Entrada + Certificacion | Pre-calculo + entrada + certificacion |
| **Pre-Process, Form Input, Process, Certify** | Pre-proceso + Entrada + Process + Certificacion | Ciclo completo con pre-calculo |
| **Pre-Process, Form Input, Process, Confirm, Certify** | Pre-proceso + Entrada + Process + Confirm + Certificacion | Ciclo mas completo disponible |
| **Central Form Input** | Para forms centralizados | Corporativo controlando forms de otras entities |
| **Workspace** | Dashboard personalizado | Interfaz completamente personalizada |
| **Workspace, Certify** | Dashboard personalizado + Certificacion | Workspace con certificacion |

#### Workflow Names para Journals (Adjustments)

| Workflow Name | Tareas incluidas | Caso de uso |
|---|---|---|
| **Journal Input** | Solo entrada de journal | Asientos simples sin validacion |
| **Journal Input, Certify** | Entrada + Certificacion | Asientos con firma |
| **Journal Input, Process, Certify** | Entrada + Process + Certificacion | Asientos con calculo post-entrada |
| **Journal Input, Process, Confirm, Certify** | Entrada + Process + Confirm + Certificacion | Ciclo completo con DQ |
| **Central Journal Input** | Para journals centralizados | Corporativo ajustando entities ajenas |
| **Workspace** | Dashboard personalizado | Interfaz personalizada |
| **Workspace, Certify** | Dashboard personalizado + Certificacion | Workspace con certificacion |

#### Workflow Names para Review Profiles

Los Review Profiles no reciben datos; solo revisan, confirman y certifican:

| Workflow Name | Descripcion |
|---|---|
| **Process, Certify** | Ejecuta calculo/consolidacion y certifica |
| **Process, Confirm, Certify** | Ejecuta calculo, verifica reglas de confirmacion y certifica |

#### Como elegir el Workflow Name correcto (guia de decision)

| Escenario | Workflow Name recomendado | Razon |
|---|---|---|
| Solo carga de datos sin validacion posterior | Import, Validate, Load | Minimo de pasos para carga basica |
| Carga con certificacion simple | Import, Validate, Load, Certify | Agrega accountability con firma |
| Carga con calculo automatico | Import, Validate, Process, Certify | Process Cube ejecuta DUCS automaticamente |
| Proceso completo con Data Quality | Import, Validate, Process, Confirm, Certify | Incluye Confirmation Rules para DQ |
| Planning con formularios | Pre-Process, Form Input, Process, Confirm, Certify | Pre-Process calcula datos antes de mostrar el form |
| Solo revision sin carga | Process, Certify o Process, Confirm, Certify | Para Review Profiles |
| Dashboard personalizado | Workspace o Workspace, Certify | Interfaz totalmente a medida |
| Corporativo controlando entities ajenas | Central Import / Central Form Input / Central Journal Input | Requiere Can Load Unrelated Entities = True |
| Datos de alta frecuencia sin auditoria | Direct Load (variante apropiada) | In-memory, sin almacenamiento en Stage |
| Exportacion de datos a sistema externo | Import Stage Only | Solo importa al Stage para recuperar datos |

#### Workspace Dashboard Name

Cuando se selecciona un Workflow Name que contiene "Workspace", la propiedad **Workspace Dashboard Name** permite definir cualquier Dashboard de la pagina Application Dashboards como interfaz del Workflow. Esta es la base para la creacion de Workflows completamente personalizados, comunmente usados en:

- People Planning
- OneStream Financial Close
- Task Manager
- Actor Workspaces

#### Workflow Tasks en OnePlace - Procedimiento paso a paso

Las tareas del Workflow en orden tipico:

##### 1. Import

El usuario importa datos al **Stage Engine**. El archivo se parsea en formato tabular limpio.

![Icono Import en Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1932-7269.png)

**Load Methods disponibles:**

| Metodo | Comportamiento | Cuando usar |
|---|---|---|
| **Replace** | Borra datos del Source ID anterior, reemplaza con nuevo archivo | Carga estandar - reemplazo por Source ID |
| **Replace (All Time)** | Reemplaza todos los Workflow Units en el Workflow View | Carga multi-periodo |
| **Replace Background (All Time, All Source IDs)** | Reemplaza todo en hilo de fondo. **Siempre elimina TODOS los Source IDs** | Solo si no hay Source IDs parciales |
| **Append** | Agrega filas nuevas sin modificar datos existentes | Cuando se agregan registros adicionales |

![Opciones de Load Method](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1933-7272.png)

**Iconos adicionales de Import:**

![Icono Re-Import - repetir importacion](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1933-7273.png)

![Icono Clear Stage - limpiar datos del Stage](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1933-7274.png)

**Controles de carga:**

- ![Enforce Global POV](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1934-7281.png) - Cuando aparece, la carga se limita al Global POV
- ![No loads before Workflow Year](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1934-7282.png) - No se pueden importar datos a periodos anteriores al ano del Workflow

##### 2. Validate

Verifica dos cosas: (1) que cada dato tenga un mapa (Transformation Rule) y (2) que la combinacion de dimensiones sea valida en el cubo.

![Paso Validate en el Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1935-7286.png)

**Transformation Errors:** La dimension con error, el Source Value y el Target Value (Unassigned). El sistema sugiere One-To-One Transformation Rule por defecto.

![Pantalla de errores de transformacion](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1935-7288.png)

**Intersection Errors:** Algo no es correcto con la interseccion completa de datos (ej: un Customer mapeado a un Salary Grade Account). Para corregir: editar regla, guardar y hacer clic en **Retransform**.

##### 3. Load

Carga los datos limpios del Stage al **Analytic Engine** (Consolidation Engine).

![Paso Load en el Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1936-7293.png)

![Dialogo de progreso de carga](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1937-7296.png)

##### 4. Pre-Process

Igual que Process, puede usarse como primer paso en el canal Forms. Ejecuta Calculation Definitions antes de mostrar el formulario al usuario.

##### 5. Input Forms

Entrada manual con formularios. Opciones bajo Workflow Forms: **Required** y **Optional**.

![Pantalla de Input Forms](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1938-7303.png)

Funcionalidades clave:
- **Complete Form:** Marca un formulario como completado
- **Revert Form:** Reabre un formulario para hacer cambios
- **Calculate:** Ejecuta calculo para un formulario especifico
- **Open Excel Form:** Exporta form a Excel, completa y reimporta
- **Attachments:** Adjuntar archivos suplementarios
- **Import Form Cells:** Cargar datos de Form via Excel template o CSV

##### 6. Input Journals

Entrada de journals. Opciones bajo Workflow Journal Templates: **Required** y **Optional**.

![Barra de herramientas de Journals](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1000-7970.png)

**Flujo de journals con seguridad completa:**
1. **Create** - Crear journal (libre o desde template)
2. **Submit** - Enviar para aprobacion
3. **Approve** - Aprobar (o rechazar)
4. **Post** - Publicar al Cube

**Quick Post:** Opcion de un clic que combina Submit + Approve + Post. Disponible cuando el usuario tiene todos los roles de seguridad y no hay restricciones de self-post/self-approval.

![Opciones de Journal en el Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1000-7971.png)

**Validate Journals:** Verifica que los miembros seleccionados sean intersecciones validas basadas en las constraints del Dimension Library.

![Validacion de journals](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1004-7982.png)

##### 7. Process

Ejecuta **Process Cube** segun las Calculation Definitions configuradas.

![Icono Process Cube](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1006-7985.png)

##### 8. Confirm

Ejecuta las **Confirmation Rules**. Dos tipos de resultado:
- **Warning:** No bloquea el proceso, pero alerta al usuario
- **Error:** Bloquea el proceso, el paso se pone rojo

![Icono Confirm](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1006-7984.png)

##### 9. Certify

Certificacion final con cuestionarios opcionales.

![Icono Certify con cuestionario](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1007-7987.png)

![Quick Certify - certificacion de un clic](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1007-7989.png)

**Dependent Status:** Muestra el estado de cada tarea Workflow requerida, incluyendo Profile Name, input types, Workflow Channel, status, porcentaje OK/In Process/Not Started/Errors, y ultima actividad.

![Pantalla Dependent Status](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1008-7990.png)

#### Workflow Icons y estados

![Iconos de estado del Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1940-7317.png)

- **Azul:** Tarea pendiente por completar
- **Verde:** Tarea completada exitosamente
- **Rojo:** Error que debe corregirse antes de continuar
- **Gris:** Tarea de Central Input (corporativo)

#### Right-Click Options en Workflow

| Opcion | Descripcion |
|---|---|
| **Status and Assigned Entities** | Muestra el Workflow Status de cada Origin process |
| **Audit Workflow Process** | Auditoria de cada tarea con fecha, usuario, duracion, errores |
| **Lock/Lock Descendants** | Bloquea un periodo o Origin process |
| **Unlock/Unlock Descendants** | Desbloquea (requiere UnlockWorkflowUnit Security Role) |
| **Edit Transformation Rules** | Navega a Transformation Rules para corregir errores |
| **Clear All Import/Forms Data From Cube** | Limpia datos del Cube pero permanecen en Stage |
| **Corporate Certification Management** | Unlock/uncertify ancestors o lock/certify descendants |
| **Corporate Data Control** | Preservar datos y restaurar si es necesario |

#### Multi-Period Processing

Al hacer clic en el Year en el Navigation Pane, se activan opciones de Multi-Period Processing que permiten ejecutar multiples tareas Workflow para uno o varios periodos a la vez.

![Multi-Period Processing](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1008-7991.png)

---

### Objetivo 201.2.2: Aplicar Calculation/Consolidation en el Workflow

#### Calculation Definitions

Todos los tipos de Workflow Profile (**excepto Cube Root**) pueden tener Calculation Definitions. Una Calculation Definition es un conjunto de instrucciones que se ejecutan cuando el usuario presiona **Process Cube** durante el Workflow.

Las Calculation Definitions se configuran en la **segunda pestana** del Workflow Profile.

![Pestana de Calculation Definitions](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p826-3738.png)

#### Opciones de calculo en Process Cube

| Calc Type | Descripcion | Uso |
|---|---|---|
| **No Calculate** | No ejecuta calculo directo; permite asignar Data Management Sequences | Para custom processing via Filter Value |
| **Calculate** | Ejecuta DUCS para el Data Unit seleccionado | Calculo basico de entidad |
| **Calculate with Logging** | Calculate + registro detallado | Debugging de calculos |
| **Translate** | Ejecuta traduccion de moneda | Conversion FX |
| **Translate with Logging** | Translate + registro detallado | Debugging de traduccion |
| **Consolidate** | Calculate + agregacion a Parents + Translations + Eliminations | Consolidacion completa |
| **Consolidate with Logging** | Consolidate + registro detallado | Debugging de consolidacion |
| **Force Calculate** | Calculate sin verificar Calculation Status | Cuando cambios no detectados por status |
| **Force Calculate with Logging** | Force Calculate + registro detallado | Debugging |
| **Force Consolidate** | Consolidate sin verificar Calculation Status | Fuerza reprocessing completo |
| **Force Consolidate with Logging** | Force Consolidate + registro detallado | Debugging |

![Configuracion de Calculation Definitions con opciones](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p823-3729.png)

#### Entity Placeholder Variables por tipo de Profile

##### Review Profile:
| Variable | Descripcion |
|---|---|
| **Dependent Entities** | Todas las Entities asignadas a los Workflow Profiles dependientes, incluyendo Named Dependents |
| **Named Dependent Filter** | Filtra Entities especificas de Named Dependents (necesario porque Shared Services suelen tener entities que abarcan multiples Review Profiles) |

##### Input Parent y Input Child:
| Variable | Descripcion |
|---|---|
| **Assigned Entities** | Entities directamente asignadas al Workflow Profile |
| **Loaded Entities** | Entities importadas por Import Child Workflow Profiles dependientes del Input Parent |
| **Journal Input Entities** | Entities ajustadas con journals por los Adjustment Child dependientes |
| **User Cell Input Entities** | Entities afectadas por data entry del usuario ejecutando el Workflow (retorna lista diferente por usuario; usado en Central Input) |

**Input Child:** Si no tiene Calculation Definition explicita, hereda las del Input Parent.

#### Confirmed Switch

Cada Calculation Definition tiene un **Confirmed Switch**:
- Determina si las Entities definidas deben ser sometidas a la **Confirmation Workflow Step**
- Da control sobre cuales Entities son validadas por Confirmation Rules
- La auto-asignacion de Entities se hace via (Assigned Entities) o (Loaded Entities)

![Calculation Definitions con Confirmed checkbox](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p826-3737.png)

#### Filter Value para Data Management

Se puede asignar un Data Management Sequence a Calculation Definitions:
1. Poner el nombre del Sequence en **Filter Value**
2. Configurar **Calc Type = No Calculate**
3. Crear un **DataQualityEventHandler** Extensibility Business Rule que lea el Sequence name y lo ejecute durante Process Cube

#### Consolidate vs Calculate vs Force - Comparacion detallada

| Aspecto | Calculate | Consolidate | Force Calculate | Force Consolidate |
|---|---|---|---|---|
| **Scope** | Solo Data Unit seleccionado | Data Unit + Parents + Translations + Eliminations | Todos los Data Units sin verificar | Todo sin verificar |
| **Verifica Calc Status** | Si | Si | No | No |
| **Eficiencia** | Alta | Alta | Baja (procesa todo) | Baja (procesa todo) |
| **Cuando usar** | Calculo basico de entidad | Consolidacion completa | Cambios en FX Rates/Member Formulas post-calculo | Data Units extremadamente grandes |
| **Riesgo** | Podria omitir cambios no reflejados en status | Podria omitir cambios no reflejados | Procesamiento excesivo | Procesamiento excesivo y redundante |

#### Mejores practicas de Calculation Definitions

- Embedding Consolidate/Calculate en la mayoria de Workflow Profiles reduce tiempos de espera para revisores y consolidaciones corporativas
- **Consolidate/Calculate (normal):** Recomendado en la mayoria de los casos -- verifica calc status primero
- **Force Consolidate/Calculate:** Solo usar cuando:
  - Se cambiaron FX Rates o Member Formulas despues del ultimo calculo
  - Data Units extremadamente grandes donde verificar calc status es mas costoso que recalcular
- Para **Planning:** Considerar Custom Calculate via Data Management en el Workflow (usando No Calculate + Filter Value)
- Definir claramente que tipo de calculo/consolidacion/traduccion debe ejecutarse y en que momento

---

### Objetivo 201.2.3: Configuracion del Workflow Profile

#### Tipos de Workflow Profile (8 tipos)

##### 1. Cube Root Profile

- Define la estructura Workflow para un Cube completo o un suffix group
- Requiere un Cube con `Is Top Level Cube For Workflow = True`
- Al crear, genera automaticamente un **Default Input Profile** (prefijo: CubeRootName_Default)
- Controla el **estado del Workflow:**

![Estado Open del Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p796-3653.png)

| Estado | Comportamiento | Acceso a datos | Recomendacion |
|---|---|---|---|
| **Open** | Workflow disponible, lock controlado individualmente | Desde memoria (cache) -- alto rendimiento | Estado normal de operacion |
| **Closed** | Lock a alto nivel, snapshot historico almacenado | Desde base de datos -- penalizacion de rendimiento | Solo para cambios mayores o operaciones discontinuadas |

![Estado Closed del Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p797-3656.png)

**IMPORTANTE:** Cerrar un Workflow toma un snapshot de la jerarquia y lo almacena en tabla historica. Se debe leer desde BD en vez de cache. Solo cerrar para discontinuaciones.

##### 2. Review Profile

- Checkpoint de revision; **no tiene relacion directa con Entity ni Origin Member**
- Puede tener Calculation Definitions
- **Named Dependents:** Capacidad unica de establecer dependencia con Input Parent Profiles que NO son descendientes directos

![Ejemplo de Named Dependent - Canada Clubs depende de Eagle Drivers](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p798-3659.png)

##### 3. Default Input Profile

- Se crea automaticamente con el Cube Root
- **No se puede eliminar**
- Las Entities no asignadas explicitamente a otros Profiles son implicitamente asignadas aqui
- No se pueden asignar Entities explicitamente al Default Input

##### 4. Parent Input Profile

- Para ajustes a Parent Entities (solo via Forms o AdjInput)
- **Import Child se crea automaticamente pero forzado a inactivo** (Profile Active = False)
- Solo permite Forms y Journals, no Import
- Parent Entities no necesitan asignarse a Parent Input a menos que requieran ajustes

##### 5. Base Input Profile

- El **tipo mas comun** (workhorse)
- Controla todos los metodos de data entry para Base Entities
- Tiene los 3 tipos de Input Children: Import, Forms, Journals

##### 6-8. Input Child Profiles (Import, Forms, Adjustment)

- Siempre son base Members de la jerarquia Workflow
- Solo pueden ser hijos de Default, Parent o Base Input Profiles
- **Mapeados directamente a Origin Members:**

![Relacion entre Input Children y Origin Members](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p801-7961.png)

| Input Child | Origin Member | Control |
|---|---|---|
| Import Child | `O#Import` | Importacion de archivos, connectors, Excel templates |
| Forms Child | `O#Forms` | Entrada manual, Excel XFSetCell, Cube Views |
| Adjustment Child | `O#AdjInput` | Journals manuales, templates, cargados por Excel |

- **Profile Active = False** desactiva el canal de input completo
- Reimportar datos no sobreescribe datos de Forms, y viceversa (aislamiento por Origin)

#### Barra de herramientas de Workflow Profiles

![Barra de herramientas del Workflow Profile](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p792-3624.png)

| Icono | Funcion |
|---|---|
| Create Cube Root | Inicia una nueva jerarquia de Workflow Profile |
| Create Child | Crea Review, Base Input o Parent Input bajo el Profile actual |
| Create Sibling | Crea un hermano del Profile actual |
| Delete | Elimina el Profile seleccionado y sus hijos |
| Rename | Renombra un Profile o Input Type |
| Move as Child/Sibling | Mueve Profiles entre posiciones de la jerarquia |
| Move Up/Down | Reordena siblings |
| Work with Templates | Navega a la pantalla de Workflow Templates |
| Update Input Children Using Template | Aplica template a Input Children |

#### Pestana Profile Properties - Detalle completo

##### General
- **Name:** Nombre del Profile
- **Description:** Descripcion breve. **Si se agrega en el Default Scenario, se muestra en el Workflow Profile POV dialog en OnePlace**

![Seleccion de Workflow Profile con descripcion](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1931-7266.png)

##### Security - Todas las propiedades

| Propiedad | Funcion | Aplica a |
|---|---|---|
| **Access Group** | Usuarios que ven el Workflow Profile en runtime | Todos |
| **Maintenance Group** | Usuarios que mantienen/administran | Todos |
| **Workflow Execution Group** | Data loaders que ejecutan Workflow | Todos |
| **Certification SignOff Group** | Certificadores que firman el Workflow | Todos |
| **Journal Process Group** | Usuarios que procesan journals | Solo Journals |
| **Journal Approval Group** | Usuarios que aprueban journals | Solo Journals |
| **Journal Post Group** | Usuarios que publican journals | Solo Journals |
| **Prevent Self-Post** | Impide publicar propios journals | Solo Journals |
| **Prevent Self-Approval** | Impide aprobar propios journals | Solo Journals |
| **Require Journal Template** | Restringe creacion de free-form journals | Solo Journals |

**Prevent Self-Post opciones:**
- **True:** Miembros del Journal Post Group no pueden publicar sus propios journals. Admins conservan Quick Post.
- **True (includes Admins):** Admins tampoco pueden publicar sus propios journals.
- **False (default):** Sin restriccion.

**Prevent Self-Approval opciones:**
- **True:** Miembros del Journal Approval Group no pueden aprobar sus propios journals. Admins conservan Quick Post.
- **True (includes Admins):** Admins tampoco pueden aprobar sus propios journals. Auto Approved y Auto Reversing journals excluidos.
- **False (default):** Sin restriccion.

**Require Journal Template:** Cuando es True, deshabilita el boton Create Journal y previene carga de archivos sin template. System y Application admins pueden seguir creando free-form journals.

##### Workflow Settings

| Propiedad | Descripcion |
|---|---|
| **Workflow Channel** | Capa adicional de seguridad vinculada a Workflow Channels (Application > Workflow > Workflow Channels). Por defecto: Standard |
| **Workflow Name** | Las tareas del Workflow (ver objetivo 201.2.1) |
| **Workspace Dashboard Name** | Solo funciona con Workflow Names que contengan "Workspace" |

##### Integration Settings (Import) - Todas las propiedades

| Propiedad | Descripcion | Default |
|---|---|---|
| **Data Source Name** | Fuente de datos (Fixed Width, Delimited, DM Export, SQL Connector) | - |
| **Transformation Profile Name** | Perfil de mapeo (Transformation Rules) | - |
| **Import Dashboard Profile Name** | Dashboards para la fase Import | - |
| **Validate Dashboard Profile Name** | Dashboards para la fase Validate | - |
| **Is Optional Data Load** | True agrega icono "Complete Workflow" para completar sin cargar datos | False |
| **Can Load Unrelated Entities** | True permite cargar datos a Entities no asignadas al Input Parent | False |
| **Flow Type No Data Zero View Override** | Override para Zero No Data en Flow Accounts | YTD/Periodic |
| **Balance Type No Data Zero View Override** | Override para Zero No Data en Balance Accounts | YTD/Periodic |
| **Force Balance Accounts to YTD View** | True fuerza View YTD para Balance Accounts en la carga | False |
| **Cache Page Size** | Tamano de pagina de cache en registros | 20000 |
| **Cache Pages In-Memory Limit** | Limite de paginas en memoria | 200 |
| **Cache Page Rule Breakout Interval** | Intervalo de pausa para verificar si la pagina esta mapeada | 0 |

![Configuracion Integration Settings para YTD/MTD](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p806-3679.png)

##### Form Settings
- **Input Forms Profile Name:** Templates de formularios. Configurados en Application > Data Collection > Form Templates.

##### Journal Settings
- **Journal Template Profile Name:** Templates de journals. Configurados en Application > Data Collection > Journal Templates.

##### Data Quality Settings

| Propiedad | Descripcion |
|---|---|
| **Cube View Profile Name** | Cube Views en el Analysis Pane (Process, Confirm, Certify) |
| **Process Cube Dashboard Profile Name** | Dashboards durante Process |
| **Confirmation Profile Name** | Confirmation Rules para el paso Confirm |
| **Confirmation Dashboard Profile Name** | Dashboards durante Confirmation |
| **Certification Profile Name** | Certification Questions para el paso Certify |
| **Certification Dashboard Profile Name** | Dashboards durante Certification |

##### Intercompany Matching Settings

| Propiedad | Descripcion |
|---|---|
| **Matching Enabled** | True/False. Cuando True, configurar Matching Parameters |
| **Currency Filter** | Moneda de reporte |
| **View Filter** | Como se muestran los datos IC (V#YTD o V#Periodic) |
| **Plug Account Filter** | Plug Account para el IC match |
| **Suppress Matches** | Aplicar supresion de matches |
| **Matching Tolerance** | Tolerancia para offsets (0.0 = al centavo) |
| **Entity Filter** | Entities del Workflow (E#Root.WFProfileEntities) |
| **Partner Filter** | Member Filter de Partner Entities |
| **Detail Dims Filter** | Dimensiones a nivel de Account (Flow, Origin, UD1-UD8) |

![Configuracion de Intercompany Matching Parameters](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p818-3707.png)

#### Entity Assignment (tercera pestana, solo en Cube Root)

- Se asignan Entities a Workflow Profiles
- Solo habilitado para data loading profiles
- Entities no asignadas van al Default Input Profile
- Busqueda por "contains"
- Una Entity asignada a un Profile ya no aparece en la ventana de busqueda

![Pantalla de Entity Assignment](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p819-3710.png)

#### Workflow Profile Grid View

- Permite cambios masivos a multiples Workflow Profiles
- Solo disponible cuando **Cube Root Profile esta seleccionado**
- Permite drag & drop de columnas para agrupar datos
- Puede pivotar la tabla para ver exactamente lo necesario

![Grid View de Workflow Profiles](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p820-3715.png)

---

### Objetivo 201.2.4: Configuraciones de Workflow menos utilizadas

#### Workflow Suffix Groups

Permiten multiples jerarquias Workflow para un solo Cube, alineadas con los **Scenario Types**:
- Se configuran en Cube Properties > Suffix for Varying Workflow by Scenario Type
- Cada Scenario Type se asigna a un Suffix
- El Suffix identifica la jerarquia Workflow para ese proceso de negocio

![Configuracion de Suffix por Scenario Type](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p790-3617.png)

**Ejemplo:** Un Cube llamado FinancialReporting con tres procesos:

| Proceso | Data Source | Suffix |
|---|---|---|
| Actual | System Interfaces | Sys |
| Plan Data | Keyed/Excel por Entity | Ent |
| Other Data | Desconocido | Oth |

![Seleccion de Cube Root con opciones de Suffix](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p791-3621.png)

**IMPORTANTE:** Una vez que un Scenario tiene datos, el suffix no se puede cambiar ni crear nuevo Cube Root para ese Scenario Type. Asignar suffixes al inicio del proyecto.

#### Workflow Templates

Utiles para construir Base Input Profiles con configuraciones similares:

![Pantalla de Workflow Templates](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p821-3719.png)

**Procedimiento:**
1. Crear template (desde Default o nuevo)
2. Personalizar: renombrar inputs, agregar/deshabilitar input types por Scenario, configurar IC, asignar Cube Views
3. Navegar a Workflow Profiles y crear Base Input
4. Aplicar template al nuevo Base Input

![Aplicacion de template a Base Input](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p822-3724.png)

**Limitacion critica:** Despues de aplicar, cambios a input types existentes del template **no** se propagan al Profile. Solo **nuevos** Input Types se pueden aplicar posteriormente via el icono Update Input Children Using Template.

![Update Input Children Using Template](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p823-3728.png)

#### Central Input

Para que corporativo haga ajustes finales despues de la carga de datos por las subsidiarias:

![Ejemplo de Central Input con marca gris](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p821-3718.png)

- Se configura con un Workflow Channel que muestra marca gris
- Corporativo puede hacer actualizaciones porque el Workflow "es dueno" de la Entity
- Toda la actividad se rastrea en audit history
- Requiere **Can Load Unrelated Entities = True**
- El Workflow Engine respeta el estado de lock/certificacion del Profile que posee la Entity

#### Named Dependents (en Review Profiles)

Permiten que un Review Profile establezca dependencia sobre Input Parent Profiles que **no son** sus descendientes directos:
- Necesario cuando Shared Services carga datos para entities que pertenecen a diferentes Review Profiles
- Ejemplo: Site A tiene Entity 1, 2, 3 y Site B tiene Entity 4, 5, 6. Un revisor es responsable de Entity 1, 2 y 6. Named Dependents resuelve esto.

#### Multiple Input Workflow Profiles per Type

Se pueden tener multiples Import/Forms/Journal Profiles del mismo tipo dentro de un periodo:

![Ejemplo de 8 Form channels en un Budget](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p804-3672.png)

- Util cuando multiples sistemas fuente alimentan las mismas Entities
- Diferentes grupos de personas completan diferentes Forms
- Para Import con multiples siblings: **automaticamente maneja clear y merge de datos**

##### Comportamiento de carga con Multiple Import Siblings:

| Condicion | Comportamiento |
|---|---|
| Sin overlap entre siblings | Clear and replace estandar individual |
| Con overlap entre siblings | Clear ALL datos para ambos siblings, reload ambos con metodo accumulate (valores se suman) |

![Behavior 2: Multiple Import Children](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p833-3756.png)

![Behavior 3: Central Input con entities comunes](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p834-3761.png)

#### Load YTD y MTD en el mismo Workflow Profile

Para cargar datos YTD en un Origin y MTD/Periodic en otro dentro del mismo Profile:

![Ejemplo Houston.Import YTD y Houston.Sales Detail MTD](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p805-3675.png)

Configurar en **Integration Settings** del Input Child:
- **Flow Type No Data Zero View Override:** Periodic
- **Balance Type No Data Zero View Override:** YTD
- **Force Balance Accounts to YTD View:** True

#### Load Overlapped Siblings

Controla el comportamiento de sibling Import Children para procesamiento paralelo:

![Configuracion Load Overlapped Siblings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p836-3766.png)

| Valor | Comportamiento | Cuando usar |
|---|---|---|
| **True (defecto)** | Verifica overlap entre sibling channels | Cuando siblings pueden tener datos que se superponen |
| **False** | No verifica overlap. El ultimo canal procesado sobreescribe | Mejora rendimiento en procesamiento paralelo sin overlap garantizado |

#### Data Locking

**Tipos de Lock:**

| Tipo | Cuando se crea | Como se limpia |
|---|---|---|
| **Explicit Lock** | Al bloquear un Workflow Unit manualmente | Desbloqueo manual |
| **Implicit Lock** | Al certificar el Parent Workflow | Des-certificar el Parent Workflow |
| **Workflow Only Lock** | Cuando el Profile no tiene Entities asignadas | Desbloqueo manual |

**Granularidad de Lock:**

| Nivel | Scope | Descripcion |
|---|---|---|
| **Input Parent (Level 1)** | Todas las celdas de Entities asignadas | Lock mas amplio -- bloquea todo |
| **Input Child (Origin Lock)** | Por Origin Member (Import, Forms, AdjInput) | Lock por canal de input |
| **Input Child (Workflow Channel Lock)** | Por Workflow Channel + Origin | Granularidad mas fina |

![Diagrama de Workflow Channel Lock](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p840-3775.png)

**Caracteristica clave:** OneStream usa control **bidireccional** entre Workflow Engine, Staging Engine y Analytic Engine. Cualquier intento de actualizar una celda directamente (Forms, Excel) activa la validacion del Workflow Engine. Esto NO existe en sistemas unidireccionales donde la celda puede ser modificada ignorando el estado del Workflow.

**Seguridad de Lock:**
- Para bloquear: necesita Workflow Execution Group membership (herencia desde ancestros)
- Para desbloquear como no-admin: necesita Workflow Execution Group + Application Security Role **UnlockWorkflowUnit**

#### Data Unit Levels en Workflow

| Nivel | Miembros | Uso | Operaciones |
|---|---|---|---|
| **Level 1 - Cube Data Unit** | Cube, Entity, Parent, Consolidation, Scenario, Time | Lock mas amplio | Clear, Load, Lock, Copy, Calculate, Translate, Consolidate |
| **Level 2 - Workflow Data Unit** | + Account (default para Workflow Engine) | Control mas granular por cuenta | Clear, Load, Lock |
| **Level 3 - Workflow Channel Data Unit** | + 1 User Defined dimension | Granularidad maxima | Clear, Load, Lock |

El Workflow Channel Data Unit es un subset del Workflow Data Unit. Se selecciona la User Defined dimension en Application Properties (una sola por aplicacion). Comunes: Cost Center, Version.

#### Workflow Channels

Tres Workflow Channels predefinidos:

| Channel | Proposito | Se asigna a |
|---|---|---|
| **Standard** | Default sin proposito especial | Account Members y Workflow Profile Input Children |
| **NoDataLock** | Remueve el Member del proceso de Workflow Channel | Solo Metadata Members (Account o UDx), NO a Workflows |
| **AllChannelInput** | Indica que el Workflow puede controlar cualquier Channel | Solo Workflow Profile Input Children, NO a Metadata |

**Account Phasing:** Usar Workflow Channels con Accounts para control independiente de grupos de cuentas.

![Diagrama de Account Phasing con Workflow Channels](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p858-3824.png)

**User Defined Phasing:** Usar Workflow Channels con UD Dimension para control por dimension de usuario.

![Configuracion de UD Type for Workflow Channel](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p860-3829.png)

#### Workflow Stage Import Methods

| Metodo | Almacenamiento | Auditoria | Uso |
|---|---|---|---|
| **Standard** | Almacena source y target en Stage | Completa (drill-down, historia) | Consolidaciones, book-of-record |
| **Direct** | In-memory, no almacena detalles | Limitada (sin drill-down, max 1000 errores) | Planning, datos operacionales de alta frecuencia |
| **Blend** | In-memory, escribe a tablas relacionales | Especifica de BI Blend | BI Blend, datos transaccionales |

![Comparacion Standard vs In-Memory](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p843-3782.png)

**Limitaciones de Direct Load:**
- No almacena source/target records
- No soporta drill-down desde Finance Engine
- No soporta Re-Transform (datos deben recargarse)
- Maximo 1000 errores de validacion por carga
- Data files no pueden cargar a Time/Scenarios mas alla del Workflow actual
- No soporta Append (solo Replace)

**Volumenes y limites:**

| Limite | Valor |
|---|---|
| Row Limit Per Workflow | 24 millones de registros sumarizados |
| Best Practice | 1 millon de registros sumarizados |

#### Batch File Loading

Permite importar y procesar archivos automaticamente hasta la certificacion:

1. Crear **Extender Business Rule** que llame `BRApi.Utilities.ExecuteFileHarvestBatch`
2. Crear **Data Management Sequence** con Business Rule Step
3. Formatear nombres de archivo: `FileID-ProfileName-ScenarioName-TimeName-LoadMethod.txt`
4. Copiar archivos a `Batch\Harvest` folder
5. Ejecutar el Data Management Sequence

**Formato de nombre de archivo:**
- `aTrialBalance-Houston;Import-Actual-2011M1-R.txt`
- `;` delimita Parent y Child Profile names
- `C` = Current Scenario/Time, `G` = Global Scenario/Time
- `R` = Replace, `A` = Append

**Parallel Batch:** `BRApi.Utilities.ExecuteFileHarvestBatchParallel` permite multiples procesos paralelos.

#### Confirmation Rules

Reglas de control para verificar la validez de los datos procesados.

**Propiedades de Confirmation Rules:**

| Propiedad | Descripcion |
|---|---|
| **Scenario Type Name** | Disponible para un Scenario Type o todos |
| **Order** | Orden de procesamiento |
| **Rule Name** | Nombre descriptivo (visible en Workflow) |
| **Frequency** | All Time Periods, Monthly, Quarterly, Half Yearly, Yearly, Member Filter |
| **Rule Text** | Descripcion textual de la regla |
| **Action** | Warning (Pass) o Error (Fail) o No Action (informacional) |
| **Failure Message** | Mensaje si error (max 2000 caracteres) |
| **Warning Message** | Mensaje si warning |

![Editor de Rule Formula para Confirmation Rules](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p867-3845.png)

#### Certification Questions

Cuestionarios que los usuarios responden al certificar datos:

| Propiedad | Descripcion |
|---|---|
| **Order** | Orden de aparicion |
| **Name** | Nombre descriptivo |
| **Category** | Tipo/categoria de pregunta |
| **Risk Level** | Nivel de riesgo (importancia) |
| **Frequency** | Mismas opciones que Confirmation Rules |
| **Question Text** | Pregunta (respuesta Yes/No + comentarios libres) |
| **Response Optional** | True = pregunta opcional |
| **Deactivated** | True = no aparece pero historial preservado |

#### Workflow Entity Relationship Member Filters

| Filtro | Retorna |
|---|---|
| `E#Root.WFProfileEntities` | Todas las Entities asociadas al Workflow Unit seleccionado |
| `E#Root.WFCalculationEntities` | Entities definidas en Calculation Definitions |
| `E#Root.WFConfirmationEntities` | Entities con Confirmed Switch = True |

#### Consideraciones de rendimiento para Workflow

- **Siempre particionar por Entity** -- hay una relacion inherente entre workflow y entity que se alinea con las estructuras de datos
- **Cargar time en orden secuencial** -- Enero, Febrero, etc.
- No superponer Entities por Time al cargar en paralelo
- Para datasets grandes (>1 millon registros): Cache Page Size = 500, Cache Pages In Memory Limit = 2000
- Para parallel processing: considerar separar cada entity con su propio parent
- **Load Overlapped Siblings = False** cuando sibling imports no se superponen
- Los Stage tables usan 250 particiones en SQL Server (automatico, basado en GUID)

#### Buenas practicas de Workflow

- **No modificar el Default Workflow** (excepto asignar Security Groups)
- Construir estructura "Do Not Use" para el Default WF Profile
- **No cerrar Workflows** para uso general; solo para operaciones discontinuadas
- Usar **Grid View** bajo Cube Root Profile para cambios masivos
- **Asignar Scenario Suffixes al inicio del proyecto**
- Elegir convencion de nombres para Security Groups y mantenerla
- Embedding Calculation Definitions en la mayoria de Workflow Profiles (especialmente Planning)
- Prototipar un workflow automatizado temprano, probando manualmente primero
- Considerar Workspace para Planning donde el workflow tradicional es demasiado restrictivo
- Crear ubicacion dedicada (admin/history) para cargas historicas con Can Load Unrelated Entities = True
- Un solo propietario por entity (con proxy como backup) evita sobreescritura de datos

---

## Puntos Criticos a Memorizar

### Workflow Names
- Import tiene 10 opciones de Workflow Name (Standard); Form tiene 11; Journal tiene 7; Review tiene 2
- Direct Load agrega 4 opciones mas para Import (sin Stage)
- "Central" en el nombre = para corporativo controlando entities ajenas
- "Pre-Process" = calculo antes de mostrar el formulario (solo en Forms)
- "Workspace" = Dashboard personalizado como interfaz
- **No Calculate** en Process permite asignar Data Management Sequences via Filter Value
- "Stage Only" = datos solo llegan al Stage, no al Cube (util para exportar de OneStream)

### Calculation Definitions
- Disponibles en **todos** los Profile Types excepto Cube Root
- Input Child hereda de Input Parent si no tiene definicion explicita
- **Confirmed Switch** controla si la Entity pasa por Confirmation Rules
- **Filter Value + No Calculate** permite ejecutar Data Management Sequences via DataQualityEventHandler
- Placeholder variables: Dependent Entities, Named Dependent Filter, Assigned Entities, Loaded Entities, Journal Input Entities, User Cell Input Entities
- Consolidate/Calculate verifica calc status (eficiente); Force no verifica (excesivo)

### Workflow Profile Settings
- **8 tipos de Profile:** Cube Root, Review, Default Input, Parent Input, Base Input, Import Child, Forms Child, Adjustment Child
- Import Child --> O#Import; Forms Child --> O#Forms; Adjustment Child --> O#AdjInput
- Parent Input **no permite Import** (Profile Active = False forzado)
- Default Input **no se puede eliminar** y recibe Entities no asignadas (implicitas)
- **Profile Active = False** desactiva todo el canal de input
- **Can Load Unrelated Entities = True** necesario para Central Input
- **Prevent Self-Post** y **Prevent Self-Approval** = separacion de responsabilidades en Journals (3 opciones: True, True includes Admins, False)
- **Require Journal Template = True** deshabilita Create Journal (admins exceptuados)

### Data Locking
- **Bidireccional:** Workflow Engine valida cada intento de actualizar celdas (no existe en sistemas unidireccionales)
- **Explicit Lock:** Al bloquear un Workflow Unit
- **Implicit Lock:** Al certificar el Parent Workflow
- **Workflow Only Lock:** Profile sin Entities asignadas
- Granularidad: Input Parent > Input Child (Origin) > Input Child (Channel)
- Cerrar Workflow = snapshot historico en base de datos (penalizacion de rendimiento)
- Para desbloquear como no-admin: necesita Execution Group + UnlockWorkflowUnit role

### Configuraciones menos usadas
- **Workflow Suffix Groups:** Multiples jerarquias por Cube alineadas con Scenario Types. Asignar al inicio porque despues no se pueden cambiar si hay datos.
- **Workflow Templates:** Ahorra tiempo pero cambios posteriores a types existentes no se propagan. Solo nuevos Input Types.
- **Named Dependents:** Solo en Review Profiles, para Shared Services con entities en diferentes Reviews.
- **Load Overlapped Siblings = False:** Mejora rendimiento cuando sibling imports no se superponen.
- **Workflow Channel Data Unit:** Agrega UD dimension para granularidad maxima. Una sola UD por aplicacion.
- **NoDataLock:** Para metadata members que no participan en Workflow Channel. **AllChannelInput:** Para Workflow Profiles que controlan cualquier channel.
- **Direct Load:** In-memory sin Stage. Max 1000 errores por carga. No soporta drill-down ni Re-Transform.
- **Batch File Loading:** Automatiza import hasta certificacion. Formato: FileID-ProfileName-ScenarioName-TimeName-LoadMethod.txt

### Stage Import Methods
- **Standard:** Almacena source+target en Stage. Para consolidaciones y book-of-record.
- **Direct:** In-memory, sin almacenamiento. Para planning y datos operacionales.
- **Blend:** In-memory, escribe a tablas relacionales externas. Para BI Blend.
- Limite por workflow: 24M registros sumarizados. Best practice: 1M.

### Confirmation Rules y Certification Questions
- Confirmation Rules: Warning (no bloquea) o Error (bloquea proceso). Frecuencia configurable.
- Certification Questions: Yes/No con comentarios. Response Optional y Deactivated disponibles.
- Mensajes truncados a 2000 caracteres.

### Workflow Entity Member Filters
- `E#Root.WFProfileEntities` = Entities del Workflow Unit actual
- `E#Root.WFCalculationEntities` = Entities en Calculation Definitions
- `E#Root.WFConfirmationEntities` = Entities con Confirmed = True

---

## Mapeo de Fuentes

| Objetivo | Libro/Capitulo |
|----------|---------------|
| 201.2.1 | Design Reference Guide - Chapter 9: Workflow (Workflow Profile Types, Base Input - Import/Form/Journal Names); Chapter 20: Using OnePlace Workflow (Workflow Tasks, Import, Validate, Load, Forms, Journals, Process, Confirm, Certify); Foundation Handbook - Chapter 7: Workflow (Workspace, Automation) |
| 201.2.2 | Design Reference Guide - Chapter 9: Workflow (Using Calculation Definitions, Calculation Definition Entity Placeholder Variables, Confirmed Switch, Filter Value); Foundation Handbook - Chapter 7: Workflow (Performance Considerations, Consolidate vs Force Consolidate) |
| 201.2.3 | Design Reference Guide - Chapter 9: Workflow (Profile Properties, Security, Integration Settings, Form Settings, Journal Settings, Data Quality Settings, IC Matching, Entity Assignment, Grid View); Chapter 20: Using OnePlace Workflow (Right-Click Options, Multi-Period) |
| 201.2.4 | Design Reference Guide - Chapter 9: Workflow (Suffix Groups, Templates, Central Input, Named Dependents, Multiple Input Profiles, Load Overlapped Siblings, Data Locking, Data Loading Behaviors, Workflow Channels, Direct Load, Blend, Batch File Loading, Confirmation Rules, Certification Questions); Foundation Handbook - Chapter 7: Workflow (Parallel processing, locking vs closing, best practices, naming conventions) |
