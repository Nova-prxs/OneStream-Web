# Seccion 6: Security (10% del examen)

## Objetivos del Examen

- **201.6.1**: Demostrar comprension del data cell access (acceso a celdas de datos)
- **201.6.2**: Describir las caracteristicas de los Application Security Roles, Application User Interface Roles y System Security Roles
- **201.6.3**: Demostrar la capacidad de solucionar problemas de acceso de seguridad

---

## Conceptos Clave

### Objetivo 201.6.1: Data Cell Access (acceso a celdas de datos)

#### Donde reside la seguridad en OneStream

La seguridad de OneStream se almacena en su propia **base de datos SQL del framework**. Esta base de datos es compartida por **todas las aplicaciones** dentro de un mismo entorno OneStream. Idealmente, los clientes tendran entornos separados para produccion, QA y desarrollo, cada uno con su propia base de datos del framework. Como parte de la oferta SAAS de OneStream, los clientes reciben **dos entornos** con bases de datos del framework separadas.

Si una empresa tiene un solo entorno con multiples aplicaciones de desarrollo, se puede asegurar cada aplicacion a un grupo restringido de usuarios mediante el rol **OpenApplication**.

#### Enfoque de seguridad de cuatro capas

La seguridad de aplicacion en OneStream utiliza un enfoque de cuatro capas:

1. **Workflow Security**: Controla quien puede ejecutar procesos de Workflow (importar, certificar, aprobar journals).
2. **Entity Security**: Controla el acceso de lectura/escritura a los datos de las entidades.
3. **Account Security**: Controla quien puede ver miembros especificos de dimensiones.
4. **Security Roles**: Determina que datos se pueden acceder o editar.

La seguridad se puede implementar sobre cuentas o dimensiones, permitiendo controlar quien puede revisar miembros especificos de dimensiones. La seguridad se determina a traves de usuarios y grupos, donde los usuarios reciben roles especificos para determinar que datos pueden accederse o editarse.

#### Flujo de verificacion de seguridad para acceso a datos

Cuando un usuario intenta acceder a datos, OneStream verifica en este orden:

![Flujo de verificacion de seguridad - Figura 9.1 del Foundation Handbook](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p279-2727.png)

1. **OpenApplication**: El usuario debe estar en un grupo asignado a este rol para abrir la aplicacion.
2. **Cube Access**: El usuario debe estar en el Access Group o Maintenance Group del Cube.
3. **Scenario Access**: Verifica Read Data Group y Read/Write Data Group del Scenario.
4. **Entity Access**: Verifica Read Data Group y Read/Write Data Group de la Entity.
5. **Data Cell Access Security** (Slice Security): Verifica filtros adicionales a nivel de Cube.
6. **Workflow Verification**: Si se importan datos, verifica que la entidad pertenezca a un Workflow Profile activo que no este bloqueado o certificado.

Si **cualquiera** de estos pasos resulta en "sin acceso", el proceso se detiene inmediatamente.

**Punto critico**: Existen varias formas de garantizar la seguridad de los datos. A traves de las dimensiones, diferentes grupos de seguridad estan disponibles, y un administrador decide que usuarios pertenecen a cada grupo.

#### Entity Security

Entity Security controla el acceso general de lectura/escritura a los datos de las entidades y controla si se debe usar la seguridad del Cube.

- Cada Entity tiene dos grupos de seguridad principales:
  - **Read Data Group**: Acceso de solo lectura a los datos de la entidad.
  - **Read/Write Data Group**: Acceso de lectura y escritura.
- OneStream tambien soporta un **segundo Read Data Group** y **segundo Read/Write Data Group**.
- **Convencion de nombres recomendada**: `XXXX_View` (lectura) y `XXXX_Mod` (escritura).

**Procedimiento de diseño de Entity Security:**

1. **Diseñar primero el Read/Write Data Group** porque es necesario para la carga de datos en Workflows.
2. El Workflow Execution Security Group debe asignarse al Read/Write Security Group de todas las entidades del Workflow para obtener acceso de carga.
3. Al configurar View Security Groups para entidades, considerar primero como los usuarios necesitan ver sus datos (por segmento o region).
4. Determinar si tiene mas sentido tener un Entity View Group por entidad, o crear un Entity View Group por segmento y aplicarlo a multiples entidades.
5. **Todos los Entity View Groups debajo del padre** deben asignarse al Entity View Group del nivel padre para tener acceso a datos a nivel padre.
6. Minimizar la cantidad de View Entity Security Groups donde sea posible.

**Ejemplo practico del Foundation Handbook - Data Loader:**

En el ejemplo de GolfStream, un data loader necesita acceso para importar, validar y cargar datos para dos entidades (HoustonHeights y SouthHouston) que forman parte del mismo workflow:

![Configuracion de grupos para data loader - Figura 9.7](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p287-2790.png)

![Relacion usuario-grupo-workflow - Figura 9.8](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p288-2802.png)

El usuario se asigna a un solo grupo (`WF_HoustonWorkflow`). El acceso para modificar las entidades y acceder al workflow se anida asignandolos al grupo `WF_HoustonWorkflow`.

**Ejemplo de error cuando falta Entity Access:**

Si un usuario tiene el workflow group pero no tiene el entity modify group (`M_HoustonHeights`), podra acceder al workflow e importar el archivo, pero obtendra un error al intentar validar contra esa entidad:

![Error por falta de entity access - Figura 9.13](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p290-2820.png)

#### Scenario Security

- El Scenario tiene **Read Data Group** y **Read/Write Data Group**.
- El acceso al escenario se verifica despues del acceso al Cube y antes de la Entity.
- La dimension scenario tambien usa ambos grupos de seguridad del scenario y añade Display Member Group y Use Cube Data Access.

#### Data Cell Access Security ("Slice" Security)

Es una capa adicional de seguridad a nivel de Cube que permite controlar acceso granular a intersecciones especificas de datos. Un "cube slice" filtra un formulario de entrada de datos al conjunto correcto de miembros, como elegir el centro de costos donde un usuario puede ingresar datos.

**Principios fundamentales:**

- Data Cell Access **solo funciona** si al usuario ya se le ha concedido acceso a una entidad y sus datos.
- Primero se concede acceso, luego se **disminuye** a un subconjunto, y luego opcionalmente se puede **aumentar** acceso a intersecciones especificas.
- **No puede aumentar acceso** que no fue concedido inicialmente a traves de Users y Groups.
- La seguridad se aplica en **orden**: el orden de las reglas es critico.
- Un administrador puede bloquear mas de una dimension usando cube data access.

**Configuracion:**

- Ubicacion: Application > Cube > Cubes > Data Access > Data Cell Conditional Input.
- Cada regla tiene:
  - **Member Filter**: Dimension intersection a restringir.
  - **In Filter**: Comportamiento (In Group In Filter, etc.).
  - **Access Level**: Read Only, No Access, etc.
  - **Behavior**: Decrease Access, Increase Access, Increase Access And Stop, etc.

**"Increase Access And Stop"**: Si la celda coincide con el filtro, se aumenta el acceso y se ignoran todas las reglas subsiguientes. Esto es critico para el orden de evaluacion.

![Configuracion de Data Cell Conditional Input](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p501-2674.png)

![Comportamientos de Data Cell Access](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p501-2676.png)

**Ejemplo practico de slice security (Foundation Handbook):**

Un data loader tiene acceso a HoustonHeights y SouthHouston, pero necesita ver solo los datos de Balance Sheet de Frankfurt (no P&L). Se usa data cell access para restringir:

![Data Cell Access con restriccion de cuentas - Figura 9.14](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p291-2828.png)

![Antes y despues de aplicar slice security - Figura 9.15](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p292-2837.png)

**Orden de slice security:**

![Orden de reglas de slice security - Figura 9.32](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p304-2950.png)

En la Figura 9.32, el grupo `Slice_2` se aplica a la segunda y tercera linea; sin embargo, los usuarios en este grupo **nunca llegaran a la tercera linea** porque la segunda tiene el comportamiento `Increase Access and Stop`, que dice al sistema que no busque reglas adicionales de slice.

**Importante sobre ViewAllData:**

- Si un usuario tiene el rol **ViewAllData**, Data Cell Access Security **no lo restringe**. Este rol otorga acceso de lectura a todo el Cube sin excepcion.
- Para restringir datos a un usuario con ViewAllData, se tendria que construir todo el acceso a entidades individualmente y luego disminuirlo.
- La unica forma de restringir lo que un usuario ViewAllData ve en un Member Filter es estableciendo el display access a Nobody. Sin embargo, esto no impide el acceso a datos via XFGetCell o entrada freeform.

#### Data Cell Conditional Input

- **No es seguridad** propiamente dicha; es una restriccion funcional.
- **Impacta a TODOS** los usuarios, no solo a grupos especificos.
- Se usa para hacer celdas de solo lectura en intersecciones especificas.
- No debe confundirse con slice security, ya que no se diseña con usuarios especificos en mente.

**Ejemplo - Restringir entrada de datos por Origin:**

Puede haber casos donde los datos deben cargarse a traves del Import origin pero no a traves del Forms origin. Por ejemplo, usuarios cargan datos de trial balance por Import, pero otros envian datos estadisticos por Forms. El data cell conditional input asegura que los datos estadisticos no sobrescriban los datos de Trial Balance.

**Procedimiento paso a paso:**

1. Ir a Application > Cube > Cubes > Data Access.
2. Ir a Data Cell Conditional Input.
3. Hacer clic para crear una nueva linea.
4. Hacer doble clic en la celda para editar el member filter. Agregar la interseccion de dimension a restringir.
5. En el campo In Filter, elegir un comportamiento y el nivel de acceso Read Only.

![Configuracion de restriccion por Origin](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p501-2674.png)

#### Omitiendo Data Cell Conditional Input por Scenario

Cuando hay muchas reglas de data cell conditional input que no fueron definidas por scenario al momento de creacion, pero un scenario se vuelve factor para datos historicos:

**Procedimiento:**

1. Crear una nueva regla de data cell conditional input para el scenario completo.
2. Establecer el comportamiento a **Increase Access And Stop**.
3. Establecer el Access Level a Read Only.
4. **Posicionar esta nueva regla al inicio** (primera posicion) del Data Cell Conditional Input.

![Regla para omitir por scenario](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p503-2681.png)

![Posicion de la regla al inicio](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p503-2682.png)

En este caso, el Preserve Scenario tiene acceso a todo, y las reglas subsiguientes de Data Cell Conditional access son ignoradas para Preserve.

#### Relationship Security

Controla acceso a miembros de relacion en la Consolidation Dimension.

![Configuracion de Relationship Security en Cube Properties](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p494-2654.png)

Se configura en Cube Properties: **Use Parent Security for Relationship Consolidation Dimension Members**.

| Valor | Comportamiento |
|-------|---------------|
| **False** (default) | Los derechos del usuario sobre la entidad controlan acceso a todos los miembros de consolidacion. Este es el modelo de seguridad por defecto. |
| **True** | Los derechos a los miembros de relacion se determinan por los derechos al padre inmediato de la entidad. |

**Procedimiento para cambiar Relationship Security:**

1. Desde el tab Application, bajo Cube, hacer clic en Cubes.
2. Seleccionar un Cube.
3. En el tab Cube Properties, en Use Parent Security for Relationship Consolidation Dimension Members, seleccionar True.
4. Hacer clic en Save.
5. Hacer clic en Refresh para refrescar la aplicacion.
6. En Cube Views, ir al tab Designer y hacer clic en Open Data Explorer.

![Cambio de Relationship Security paso a paso](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p495-2657.png)

![Resultado despues de cambiar Relationship Security](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p497-2664.png)

**Efectos observados:**
- Con la nueva seguridad activada, si el usuario tiene derechos a USClubs, al ver Houston, todos los miembros de consolidacion estan disponibles.
- Si el usuario **no tiene derechos a Texas**, basado en NoAccess a Local y Translated, Houston muestra los miembros de relacion como NoAccess.
- USClubs se muestra como NoAccess porque el usuario no tiene derechos al padre NAClubs.

**Nota importante**: Esto es estrictamente una relacion padre-hijo. **ViewAllData** y Administrators **no se ven afectados** por esta configuracion.

![Miembros de consolidacion - Figura 9.23](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p299-2902.png)

#### Display Member Group

- Controla la **visibilidad** de miembros de dimension en listas, **no el acceso a datos**.
- Disponible en Account, Flow y User Defined Dimensions.
- **No debe confundirse con seguridad de datos**: Un usuario puede no ver el miembro en una lista pero podria acceder los datos via XFGetCell o entrada freeform.
- Establecer Display Access a "Nobody" oculta el miembro de Member Filters, pero no restringe datos.
- Tanto display access como workflow channels son para propositos **funcionales** y no deben usarse como nivel de seguridad en el diseño general.

---

### Objetivo 201.6.2: Application Security Roles, Application User Interface Roles y System Security Roles

#### Application Security Roles

Controlan quien puede **gestionar o ejecutar acciones** sobre objetos y datos dentro de una aplicacion especifica. Estos roles abordan todo, desde quien puede acceder a la aplicacion, hasta modificar datos, descertificar y desbloquear workflows, hasta acceder y modificar artefactos.

![Application Security Roles con defaults - Figura 9.2](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p280-2734.png)

**Tabla completa de Application Security Roles:**

| Rol | Descripcion | Default |
|-----|-------------|---------|
| **AdministerApplication** | Administrar la aplicacion y cargar archivos ZIP | Administrator |
| **AdministratorDatabase** | Eliminacion masiva de metadata y datos via Database page. Administrators NO tienen acceso automatico | **Nobody** |
| **OpenApplication** | Ver y abrir la aplicacion | **Everyone** |
| **ModifyData** | Modificar datos (sin esto, el usuario es read-only). Puede dejarse en Everyone si el resto de la seguridad esta correctamente configurada | Everyone |
| **ViewAllData** | Ver todos los datos. No puede ser restringido con Data Cell Access | Administrator |
| **CreateAuditAttachments** | Crear adjuntos de auditoria | Administrator |
| **CreateFootnoteAttachments** | Crear notas al pie | Administrator |
| **CertifyAndLockDescendants** | Certificar y bloquear descendientes desde Workflow | Administrator |
| **UnlockAndUncertifyAncestors** | Desbloquear y descertificar ancestros | Administrator |
| **PreserveImportData** | Preservar datos importados cuando se hacen cambios con Workflow bloqueado | Administrator |
| **RestoreImportData** | Restaurar datos importados al estado original desde Preserve | Administrator |
| **UnlockWorkflowUnit** | Desbloquear una unidad de Workflow (tambien requiere Workflow Execution access) | Administrator |
| **ViewSourceDataAudit** | Ver el Source Data Audit Report en Import Workflow | Administrator |
| **EncryptBusinessRules** | Encriptar y desencriptar Business Rules | Administrator |
| **ManageApplicationProperties** | Actualizar Application Properties | Administrator |
| **ManageMetadata** | Editar metadata en Dimension Library | Administrator |
| **ManageFXRates** | Actualizar FX Rates | Administrator |
| **ManageData** | Gestionar datos (exportar, limpiar via Data Management). Funcion de administrador | Administrator |
| **ManageCubeViews** | Crear y gestionar Cube Views, Groups y Profiles | Administrator |
| **ManageDataSources** | Crear Data Sources | Administrator |
| **ManageTransformationRules** | Crear y gestionar Transformation Rules | Administrator |
| **ManageConfirmationRules** | Crear y gestionar Confirmation Rules | Administrator |
| **ManageCertificationQuestions** | Crear y gestionar Certification Questions | Administrator |
| **ManageWorkflowChannels** | Crear Workflow Channels | Administrator |
| **ManageWorkflowProfiles** | Crear Workflow Profiles | Administrator |
| **ManageJournalTemplates** | Crear y gestionar Journal Templates | Administrator |
| **ManageFormTemplates** | Crear y gestionar Form Templates | Administrator |
| **ManageApplicationDashboards** | Crear y gestionar Application Dashboards | Administrator |
| **ManageApplicationDatabaseFiles** | Acceso completo de lectura/escritura a archivos en la BD de aplicacion | Administrator |
| **ManageTaskScheduler** | Gestion completa de Task Scheduler incluyendo load/extract | Administrator |
| **TaskScheduler** | Crear y editar tareas propias. Sin load/extract | Administrator |
| **AnalyticsApi** | Acceso a conectores de OneStream (ej. Power BI Connector) | Administrator |

**Nota critica**: Cuando se crea una nueva aplicacion, todos los roles defaults son **Administrator** excepto **OpenApplication** (Everyone) y **AdministratorDatabase** (Nobody). Estos dos son excepciones para actividades menos comunes donde se quiere que la capacidad de administrar sea deliberada.

**Sobre ModifyData y Everyone**: Puede establecerse en Everyone porque la combinacion de seguridad de Cube, Scenario, Entity y Workflow ya controla efectivamente quien puede modificar datos. Si un usuario no tiene acceso de escritura a una entidad, el ModifyData role no le otorgara ese acceso.

#### Application User Interface Roles

Controlan **acceso a paginas** dentro de la aplicacion. Otorgan visibilidad pero **no capacidad de gestion**. Estan en la seccion inferior/derecha de Application Security Roles.

| Rol | Pagina que controla |
|-----|---------------------|
| **ApplicationLoadExtractPage** | Application > Tools > Load/Extract |
| **ApplicationPropertiesPage** | Application > Tools > Application Properties |
| **ApplicationSecurityRolesPage** | Application > Tools > Security Roles |
| **BookAdminPage** | Application > Presentation > Book Designer |
| **BusinessRulesPage** | Application > Tools > Business Rules |
| **CertificationQuestionsPage** | Application > Workflow > Certification Questions |
| **ConfirmationRulesPage** | Application > Workflow > Confirmation Rules |
| **CubeAdminPage** | Application > Cube > Cube Admin |
| **CubeViewsPage** | Application > Presentation > Cube Views |
| **DashboardAdminPage** | Application > Presentation > Dashboard Admin |
| **DataManagementAdminPage** | Application > Tools > Data Management Admin |
| **DataSourcesPage** | Application > Data Collection > Data Sources |
| **DimensionLibraryPage** | Application > Cube > Dimension Library |
| **FXRatesPage** | Application > Cube > FX Rates |
| **FormTemplatesPage** | Application > Data Collection > Form Templates |
| **JournalTemplatesPage** | Application > Data Collection > Journal Templates |
| **TransformationRulesPage** | Application > Data Collection > Transformation Rules |
| **WorkflowChannelsPage** | Application > Workflow > Workflow Channels |
| **WorkflowProfilesPage** | Application > Workflow > Workflow Profiles |
| **TaskSchedulerPage** | Application > Tools > Task Scheduler |
| **SpreadsheetPage** | Application > Spreadsheet |

**Punto clave**: Un usuario puede tener acceso a la pagina (UI Role) pero NO tener permiso para gestionar el contenido (Security Role). Ambos roles trabajan juntos. Reconocer como se puede dar visibilidad a los usuarios a objetos en la aplicacion mientras se restringe el acceso de gestion es clave para cualquier linea de cuestionamiento sobre quien necesita ver elementos versus quien necesita actualizarlos.

**Ejemplo practico - Application Admin con acceso limitado:**

Imaginemos un grupo `R_ApplicationAdmin` que necesita gestionar metadata y FX rates pero no es administrador completo:
- Asignar `ManageMetadata`, `ManageFXRates`, `LockFXRates`, `UnlockFXRates` al grupo `R_ApplicationAdmin`.
- Tambien asignar `DimensionLibraryPage` y `FXRatesPage` al mismo grupo (o a un grupo anidado).
- Mantener `FXRatesPage` abierto a un grupo separado de usuarios con acceso de solo lectura.

![Ejemplo de roles aplicados - Figura 9.4](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p283-2754.png)

![Relacion de roles y acceso - Figura 9.5](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p284-2763.png)

#### System Security Roles

Se aplican a **todo el sistema**, no solo a una aplicacion. El Administrator tiene todos estos roles por defecto y no se le pueden revocar. Los system security roles son tipicamente reservados para administradores.

![System Security Roles - Figura 9.3](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p281-2741.png)

**Tabla completa de System Security Roles:**

| Rol | Descripcion | Default |
|-----|-------------|---------|
| **ManageSystemDashboards** | Gestionar todos los System Dashboards. Requiere tambien SystemPane Role | Administrator |
| **ManageSystemDatabaseFiles** | Lectura y escritura completa en archivos del Framework database. Se vincula con FileExplorerPage | Administrator |
| **ManageFileShare** | Editar carpetas y archivos en File Share via File Explorer | Administrator |
| **ManageSystemConfiguration** | Cambiar configuraciones del servidor | **Nobody** (NO automatico para Administrator) |
| **ManageProfiler** | Ejecutar sesiones de Profiler y ver eventos | Administrator |
| **EncryptSystemBusinessRules** | Encriptar/desencriptar Business Rules del System tab | Administrator |
| **ViewAllLogonActivity** | Ver actividad de login de todos los usuarios | Administrator |
| **ViewAllErrorLog** | Ver Error Log de todos los usuarios | Administrator |
| **ViewAllTaskActivity** | Ver Task Activity de todos los usuarios | Administrator |
| **ManageSystemSecurityUsers** | Crear, modificar y eliminar usuarios | Administrator |
| **ManageSystemSecurityGroups** | Definir, modificar y gestionar grupos y exclusion groups | Administrator |
| **ManageSystemSecurityRoles** | Gestionar asignacion de System Security Roles | Administrator |

#### File Share Security Roles

| Rol | Descripcion |
|-----|-------------|
| **ManageFileShareContents** | Expone la carpeta Contents en File Explorer/FileShare. Acceso completo: crear, subir, descargar, eliminar carpetas |
| **AccessFileShareContents** | Expone la carpeta Contents. Solo permite ver y descargar |
| **RetrieveFileShareContents** | La carpeta Contents NO se expone al usuario en File Explorer. Todos los archivos son accesibles a traves de la aplicacion OneStream (Dashboards, Business Rules) |

#### System User Interface Roles

Controlan acceso a paginas del System tab.

| Rol | Descripcion | Default |
|-----|-------------|---------|
| **SystemAdministrationLogon** | Acceso a la aplicacion System Administration. Se vuelve disponible en la lista de aplicaciones durante logon | Administration |
| **SystemPane** | Acceso al System Tab (abajo a la izquierda) | Administrator |
| **ApplicationAdminPage** | Acceso al Application Tab | Administrator |
| **SecurityAdminPage** | Ver (no modificar) seguridad en System > Administration. Solo puede cambiar su propia contraseña | Administrator |
| **SystemDashboardAdminPage** | Acceso a System > Administration > Dashboards. Se vincula con ManageSystemDashboards | Administrator |
| **ApplicationServersPage** | Acceso a System > Tools > Application Servers | Administrator |
| **DatabasePage** | Acceso a System > Tools > Database | Administrator |
| **FileExplorerPage** | Acceso a System > Tools > File Explorer. Se vincula con ManageSystemDatabaseFiles y ManageFileShare | Administrator |
| **SystemLoadExtractPage** | Acceso a System > Tools > Load/Extract (ver, pero no importar/extraer sin roles adicionales) | Administrator |
| **ErrorLogPage** | Acceso a System > Logging > Error Log | Administrator |
| **LogonActivityPage** | Acceso a System > Logging > Logon Activity. Puede ver todos los usuarios pero no puede hacer logoff | Administrator |
| **TaskActivityPage** | Acceso a System > Logging > Task Activity | Administrator |
| **TimeDimensionPage** | Acceso a System > Tools > Time Dimension | Administrator |
| **SystemConfigurationPage** | Acceso a System Configuration. Solo lectura. No se concede automaticamente con ManageSystemConfiguration | Administrator Group |
| **ProfilerPage** | Ver la pagina de Profiler (sin ejecutar sesiones ni ver eventos) | Administrator |

#### Jerarquia de acceso (de mayor a menor poder):

1. **System Security Role** (ej. ManageSystemDashboards): Privilegio mas alto. El usuario no necesita estar en ningun Maintenance Group o Access Group para ver, editar o eliminar todos los objetos de ese tipo.
2. **Maintenance Group**: Puede ver, crear, editar y eliminar objetos en Groups. No necesita estar en Access Group. Tambien controla el contenido de Profiles.
3. **Access Group**: Solo puede ver el objeto y leer su contenido. Es el nivel mas bajo de poder a nivel de grupo.

#### Application Server Configurations (System Configuration)

Las configuraciones del servidor de aplicaciones ahora pueden ser realizadas por Administrators y personal avanzado de IT, eliminando la necesidad de llamadas a soporte y reinicios de IIS.

![General System Configurations](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1794-6831.png)

**Dos roles controlan el acceso:**

| Rol | Tipo | Default | Nota |
|-----|------|---------|------|
| **ManageSystemConfiguration** | System Security Role | **Nobody** | NO se otorga automaticamente al Administrator |
| **SystemConfigurationPage** | System User Interface Role | **Administrator Group** | Solo lectura para todos los asignados |

**Caracteristicas clave:**
- Los cambios se aplican automaticamente **cada 2 minutos** (sin necesidad de IIS restart).
- Se registran en la **Audit** tab.
- 6 categorias: General, Environment, Memory, Multithreading, Recycling, Database Server Connections.
- Memory y Multithreading deben ser habilitados por soporte antes de poder cambiarlos.
- Para mitigar mal uso, Customer Support puede deshabilitar features completas, secciones y cambios de propiedades via XML/App config.

**General System Configurations - Propiedades detalladas:**

| Propiedad | Descripcion |
|-----------|-------------|
| Use Detailed Error Logging | Cuando True, se muestra stack trace. Cuando False, no se muestra |
| User Inactivity Timeout (minutes) | Minutos antes de timeout por inactividad del usuario |
| Task Inactivity Timeout (minutes) | Minutos antes de timeout por inactividad de tarea |
| Logon Inactivity Threshold (days) | Dias de inactividad antes de deshabilitar usuario. -1 para desactivar |
| Task Scheduler Validation Frequency (days) | Dias entre validaciones del Task Scheduler |
| Culture Codes | Codigo de configuracion regional (ej. en-US) |
| White List File Extensions | Tipos de archivo permitidos en File Explorer |
| Num Seconds Before Logging Slow Formulas | Segundos antes de registrar formulas lentas. Impacta rendimiento |
| Number Seconds Before Logging Get Data Cells | Default 180. Solo aumentar para debug |

**Environment System Configurations:**

![Environment Configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1797-6838.png)

| Propiedad | Descripcion |
|-----------|-------------|
| Environment Name | Nombre personalizado del entorno (hasta 150 caracteres) |
| Environment Color | Color hex para el nombre del entorno |
| Logon Agreement Type | Seleccionar Custom para mostrar un mensaje personalizado al iniciar sesion |
| Logon Agreement Message | Texto del mensaje de acuerdo de inicio de sesion |
| Full Width Banner Message | Mensaje que se muestra en un banner superior |
| Full Banner Display Type | Informational, Warning, Successful o Critical |
| Can Close Full Banner | True permite cerrar el banner; False lo mantiene permanente |

![Tipos de banner](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1799-6844.png)

**Recycling System Configurations:**

![Recycling Configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1801-6855.png)

| Propiedad | Default | Descripcion |
|-----------|---------|-------------|
| Auto Recycle Num Running Hours | 24 | Horas entre reciclajes automaticos |
| Auto Recycle Start Hour UTC | 5 | Hora mas temprana (UTC) para reciclar |
| Auto Recycle End Hour UTC | 7 | Hora mas tardia (UTC) para reciclar |
| Auto Recycle Max Pause Time (minutes) | 30 | Minutos para pausar antes de reciclar, permitiendo tareas activas terminar |

**Audits de configuracion:**

Los cambios a las configuraciones se rastrean via el Audit tab, visible en System > System Configuration > Audit.

![Audit tab de System Configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1805-6868.png)

---

### Objetivo 201.6.3: Troubleshooting de acceso de seguridad

#### Flujo de diagnostico

Cuando un usuario no puede acceder a algo, seguir este orden de verificacion:

1. **Verificar que el usuario este habilitado**: System > Security > Users > Is Enabled = True.
2. **Verificar Logon Inactivity Threshold**: Si Remaining Allowed Inactivity = 0 Days, el usuario esta deshabilitado por inactividad.
3. **Verificar OpenApplication role**: Que el grupo del usuario este asignado a OpenApplication.
4. **Verificar acceso al Cube**: Access Group o Maintenance Group del Cube.
5. **Verificar Scenario security**: Read Data Group y/o Read/Write Data Group del Scenario.
6. **Verificar Entity security**: Read Data Group y/o Read/Write Data Group de la Entity.
7. **Verificar Data Cell Access** (Slice Security): Revisar el orden de las reglas y los comportamientos.
8. **Verificar Workflow Security**: Workflow Execution Group, Data Group, Approver Group.
9. **Verificar Application Security Roles**: ModifyData, ManageData, y roles especificos.
10. **Verificar Application User Interface Roles**: Acceso a las paginas correspondientes.
11. **Verificar System Security Roles y System User Interface Roles** si el problema es en el System tab.

#### Creacion y gestion de usuarios

**Procedimiento paso a paso para crear usuarios:**

1. Hacer clic en System > Security > Users.
2. Hacer clic en Create User.
3. Ingresar nombre y descripcion.
4. En User Type, seleccionar el tipo de licencia adquirido.
5. Establecer Is Enabled en True para activar el usuario.
6. Revisar Status e Inactivity Threshold.
7. Configurar la autenticacion (ver siguiente seccion).

![Asignacion de roles para no-administradores](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch16-p1773-8031.png)

**User Types** (tipo de licencia, NO controla acceso):

| Tipo | Descripcion |
|------|-------------|
| **Interactive** | Acceso completo a todas las funciones y herramientas |
| **View** | Solo acceso a datos, reportes y dashboards. No puede cargar, calcular, consolidar, certificar o cambiar datos |
| **Restricted** | No puede usar algunas funcionalidades de Solution Exchange (Lease, Account Reconciliation, etc.) debido a limitaciones contractuales |
| **Third Party Access** | Acceso via aplicacion de terceros con cuenta nombrada. No puede cambiar datos, modificar artefactos ni acceder a Windows app o browser |
| **Financial Close** | Puede usar Account Reconciliation y Transaction Matching |

**Propiedades de preferencia del usuario:**

| Propiedad | Descripcion |
|-----------|-------------|
| Email | Direccion de email para alertas y mensajes generados por business rules |
| Culture | Configuracion regional del usuario |
| Grid Rows Per Page | Numero de filas por pagina en grids. Considerar conectividad y resolucion de pantalla |
| Custom Text (Text 1-8) | Textos personalizables para metadata tags, filtros de distribucion, funcionalidad personalizada |
| Group Membership | Grupos a los que pertenece el usuario |

![Preferencias y Group Membership del usuario](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch16-p1776-6766.png)

**Gestion de usuarios:**

| Accion | Descripcion |
|--------|-------------|
| Delete Selected Item | Eliminar permanentemente el usuario |
| Copy Selected Item | Crear nuevo usuario basado en las configuraciones del seleccionado |
| Show all parent groups for user | Ver todos los grupos en los que el usuario es miembro. Util para identificar acceso |

**Copiar usuarios:**
1. System > Security > Users.
2. Seleccionar usuario, hacer clic en Copy Selected Item.
3. Ingresar nuevo nombre.
4. Seleccionar **Copy References made by parent groups** para agregar al nuevo usuario a los grupos del original (excepto exclusion groups).
5. Hacer clic en OK y modificar configuraciones si es necesario.

#### Autenticacion

Dos metodos de autenticacion disponibles:

| Metodo | External Authentication Provider | External Provider User Name | Internal Provider Password |
|--------|----------------------------------|----------------------------|---------------------------|
| **Native** | Seleccionar (Not Used) | Dejar en blanco | Ingresar contraseña |
| **External** | Seleccionar el IdP externo | Ingresar el nombre de usuario en el IdP (debe ser unico y coincidir) | No aplica |

**Proveedores externos soportados:**
- Microsoft Active Directory (MSAD)
- Lightweight Directory Access Protocol (LDAP)
- OpenID Connect (OIDC): Azure AD (Microsoft Entra ID), Okta, PingFederate
- SAML 2.0: Okta, PingFederate, ADFS, Salesforce

**Pantallas de logon segun configuracion de autenticacion:**

| Configuracion | Pantalla |
|--------------|----------|
| Solo Native | Campos de usuario y contraseña |
| Solo External IdP | Boton "External Provider Sign In" |
| Native + External | Campos de usuario/contraseña Y boton de External Sign In |

En entornos hosted por OneStream, se usa **OneStream IdentityServer**.

#### Groups y seguridad heredada

**No se puede asignar acceso a usuarios individuales** directamente a herramientas y artefactos. Los System Security Roles asignados a Groups determinan este acceso. Se crean grupos para otorgar acceso personalizado por funcion a grandes cantidades de usuarios.

- Los Groups pueden anidarse jerarquicamente. Los child groups heredan el acceso del parent group, desde un nivel padre hacia abajo.
- **Groups no pueden ser autenticados externamente**.
- Eliminar child groups de parent groups revoca el acceso a las herramientas y artefactos que el parent group provee.

**Procedimiento para crear Groups:**

1. System > Security.
2. Hacer clic en Groups, luego Create Group.
3. Ingresar nombre intuitivo y descripcion.
4. En Group Membership, especificar usuarios o child groups, o seleccionar un parent group.
5. Hacer clic en Save.

#### Exclusion Groups

Para otorgar acceso a casi todos excepto algunos usuarios especificos.

**Procedimiento para crear Exclusion Groups:**

1. System > Administration > Security.
2. Hacer clic en Groups, luego Create Exclusion Group.
3. Ingresar nombre descriptivo (ej. `Everyone-But-<Department>`).
4. En Group Membership, agregar child groups o usuarios.
5. Establecer miembros a **Deny Access** o **Allow Access**.
6. **Usar las flechas para ordenar las exclusiones cuidadosamente**.

![Ejemplo de Exclusion Group mal configurado](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch16-p1786-6806.png)

**El orden es critico**: Se evalua segun el orden especificado, independientemente de la membresia del usuario. Ejemplo:
- Si Amelia y Bob estan en el grupo Frankfurt Controller, y se listan primero individualmente con Deny Access pero Frankfurt Controller esta despues con Allow Access, **no seran restringidos** porque el Allow del grupo sobrescribe.
- **Solucion correcta**: Poner Frankfurt Controller primero (Allow Access), luego Amelia y Bob debajo (Deny Access).

#### Manage System Security (para no-Administrators)

![Manage System Security roles](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1808-6878.png)

**ManageSystemSecurityUsers - Permite:**
- Crear, modificar, eliminar y deshabilitar usuarios
- Especificar propiedades: General, Authentication, Preferences, Custom Text

**ManageSystemSecurityUsers - Limitaciones:**
- NO puede crear, modificar o eliminar administradores (directa o indirectamente)
- NO puede agregarse o removerse a si mismo de grupos o roles
- NO puede eliminarse a si mismo
- NO puede agregar otros usuarios a privilegios de Manage System Security
- Para gestionar membresia de grupos o copiar usuarios, se requiere ManageSystemSecurityGroups

**ManageSystemSecurityGroups - Permite:**
- Gestionar grupos y exclusion groups
- Agregar o remover miembros de grupos
- Copiar grupos (excepto grupos con privilegios de Administrator)

**ManageSystemSecurityGroups - Limitaciones:**
- NO puede modificar el grupo Administrators
- NO puede asignar usuarios a grupos con privilegios de Administrator
- NO puede modificar su propia membresia en otros grupos
- NO puede modificar el parent group de un grupo del cual es miembro

**ManageSystemSecurityRoles - Permite:**
- Gestionar asignaciones de system security roles

**ManageSystemSecurityRoles - Limitaciones:**
- NO puede modificar el ManageSystemSecurityRole (requiere Administrator)
- NO puede asignar los grupos Everyone o Nobody (requiere Administrator)
- NO puede agregar un grupo a un rol del cual es miembro

**Roles combinados**: Tener mas de un rol Manage System Security amplifica las capacidades. Por ejemplo, con roles de Users y Groups juntos, se puede copiar un usuario o agregar un usuario a un grupo.

**Load/Extract de Security requiere:**
- ManageSystemSecurityUsers
- ManageSystemSecurityGroups
- ManageSystemSecurityRoles
- SystemLoadExtractPage (UI Role)

Los usuarios con Manage System Security obtienen acceso automatico a SystemAdministrationLogon y SystemPane UI Roles.

![BRApi para gestion de seguridad](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1811-6886.png)

**Validacion de Load/Extract para usuarios con Manage System Security:**
La validacion ocurre comparando el estado actual de seguridad en el entorno destino con el estado cambiado determinado por el procesamiento del archivo fuente. Por lo tanto, la seguridad debe **pre-existir** en el entorno destino para determinar el estado cambiado. Para entornos nuevos o vacios, un **Administrator** debe realizar la carga.

#### El usuario Administrator

- **No puede ser deshabilitado** ni eliminado.
- **No se ve afectado** por Logon Inactivity Threshold.
- **Bypasa toda la seguridad** de la aplicacion. Esto no se puede cambiar.
- Asignar otros grupos a roles NO revoca el acceso del Administrator.
- Consideracion importante: Para datos sensibles (ej. People Planning), se necesitan **Event Handlers y BRAPI calls** para excluir al Administrator de ver ciertos datos, ya que el grupo Administrator tiene autoridad sobrescritora incluso si se cambian todos los roles que por defecto estan en Administrator.

#### Disable Inactive Users

Permite crear una politica de autorizacion para deshabilitar usuarios despues de un periodo especifico de inactividad de login.

**Procedimiento paso a paso para configurar el Inactivity Threshold:**

1. Hacer clic en File > Open Application Server Configuration File.

![Abrir archivo de configuracion](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p507-2691.png)

2. Abrir el archivo de configuracion.

![Configuracion abierta](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p507-2692.png)

3. Hacer clic en Security.

![Seccion Security del config](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p508-2695.png)

4. Establecer el Logon Inactivity Threshold (days) al numero de dias de inactividad antes de que el usuario sea deshabilitado.

![Establecer threshold](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p508-2696.png)

5. Hacer clic en OK, luego Save.
6. **Reset IIS** para reconocer los cambios.

**Revision del estado de usuarios:**

![Revision de Inactivity Threshold en usuarios](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p510-2703.png)

- Si **Remaining Allowed Inactivity = 0 Days**, el usuario ya no tiene acceso.

![Usuario con 0 dias restantes](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p511-2706.png)

- El usuario recibe un mensaje indicando que ha sido deshabilitado al intentar loguearse.

![Mensaje de usuario deshabilitado](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p511-2707.png)

- Para rehabilitar: System > Security > Users > Is Enabled = True.

![Re-habilitacion de usuario](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p512-2710.png)

- **No aplica al Administrator**.
- Se aplica a usuarios nativos y externos.
- Tambien se puede configurar via System Configuration (General > Logon Inactivity Threshold). Establecer a -1 para desactivar.

#### Troubleshooting de Workflow Security

| Grupo de Workflow | Funcion | Efecto si falta |
|-------------------|---------|-----------------|
| **Workflow Execution Group** | Permite ejecutar Import, Forms, Journals | El usuario no ve el Workflow (solo Cube Root) |
| **Data Group** | Acceso a datos dentro del Workflow | Error al validar/cargar datos |
| **Approver Group** | Permite certificar el Workflow | Botones de certificacion aparecen grisados |

![Data loader exitoso - Figura 9.9](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p288-2805.png)

![Certificacion deshabilitada para data loader - Figura 9.10](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p288-2804.png)

**Ejemplo - Usuario sin workflow group:**

Si un usuario no tiene el workflow group, solo vera el Cube Root, ningun workflow debajo:

![Usuario sin workflow access - Figura 9.12](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p289-2815.png)

**Ejemplo - View User con Workflow Responsibility:**

Un Houston Approver necesita certificar el workflow parent despues de revisar cifras, pero no necesita cargar datos. Necesita:
- Mismo nivel de acceso al workflow pero diferente nivel de ejecucion (certificacion)
- Solo view access a las entidades (no edit)

![Configuracion de approver - Figura 9.19](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p296-2869.png)

![Approver con acceso correcto - Figura 9.20](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p297-2887.png)

#### Troubleshooting de Dashboards y Cube Views

**Dashboards:**
- El usuario necesita acceso al **Dashboard Profile** Y al **Dashboard Group**.
- Si tiene acceso al Group pero no al Profile, no vera los Dashboards en OnePlace.
- Si tiene acceso al Profile pero no a ciertos Dashboard Groups en ese Profile, no vera los Dashboards de ese grupo.
- Si un Dashboard apunta a datos que el usuario no puede ver: muestra NoAccess, celda vacia, o "No Data Series".

![Dashboard security - Figura 9.28](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p302-2929.png)

**Cube Views:**
- Cube View Groups se asignan a Cube View Profiles. Access Group se asigna en el Profile.
- Setting **Visible in Profiles** (True/False) en el Cube View mismo.

![Cube View Visible in Profiles - Figura 9.29 y 9.30](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p302-2934.png)

- Propiedades del Cube View que deben estar en **False** para usuarios no-administradores:
  - **Can Modify Data**
  - **Can Calculate**
  - **Can Translate**
  - **Can Consolidate**

![Restricciones de Cube View - Figura 9.31](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p303-2944.png)

#### Carga masiva de Security

**Tres formas de agregar usuarios y grupos a una aplicacion:**

1. **Definirlos manualmente** en System Security.
2. **Cargar en lote** usando un archivo XML generado desde el **SecurityTemplate.xlsx** del SampleTemplates OneStream Solution (opcion recomendada).
3. **Usar APIs** (BRApi functions).

**Procedimiento de carga (Load):**
1. System > Tools > Load/Extract.
2. Seleccionar Load y buscar el archivo XML.
3. Hacer clic en Load.
4. Revisar la lista de usuarios/grupos para verificar que estan correctamente definidos.

**Procedimiento de extraccion (Extract):**
1. System > Tools > Load/Extract.
2. Hacer clic en Extract, seleccionar File Type > Security.
3. En Items to Extract, seleccionar Users, Security Groups o All.
4. **Extract Unique IDs**: Si esta marcada, se extraen los IDs unicos. **Desmarcar al mover entre entornos** donde ya existen registros para evitar errores de duplicados.
5. Extract o Extract and Edit (para modificar XML).

**Requisitos para usuarios no-Administrator que necesitan hacer Load/Extract de Security:**
- SystemLoadExtractPage (UI Role)
- ManageSystemSecurityUsers
- ManageSystemSecurityGroups
- ManageSystemSecurityRoles

**BRApi functions disponibles para gestion de seguridad:**

```
BRApi.Security.Admin.GetUsers / GetUser / SaveUser / RenameUser / DeleteUser / CopyUser
BRApi.Security.Admin.GetGroups / GetGroup / SaveGroup / RenameGroup / DeleteGroup / CopyGroup
BRApi.Security.Admin.GetExclusionGroups / GetExclusionGroup / SaveExclusionGroup / RenameExclusionGroup / DeleteExclusionGroup / CopyExclusionGroup
BRApi.Security.Admin.GetSystemRoles / GetApplicationRoles / GetRole
```

Estas funciones estan controladas por el Manage System Security role asignado. Si un dashboard ejecuta un BRApi para insertar un usuario, el sistema valida que el usuario haciendo clic sea Administrator o tenga ManageSystemSecurityUsers.

#### Restringir usuarios a una aplicacion

**Procedimiento paso a paso:**

1. Ir a System > Administration > Security.
2. Crear un nuevo security group.
3. Asignar todos los usuarios que deben tener acceso a la aplicacion al nuevo grupo.
4. Refrescar la aplicacion para que el nuevo grupo aparezca en los menus dropdown.
5. Ir a Application > Tools > Security Roles > OpenApplication.

![Configuracion de OpenApplication](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p500-2671.png)

6. Hacer clic en el dropdown y seleccionar el nuevo security group.
7. Hacer clic en Save.

---

## Mejores practicas de seguridad por tipo de objeto

| Objeto | Access Group | Maintenance Group | Nota |
|--------|-------------|-------------------|------|
| **Confirmation Rules** (Groups y Profiles) | Everyone | Administrators | Runtime access depende del Workflow Profile asignado |
| **Certification Questions** (Groups y Profiles) | Everyone | Administrators | Si el usuario tiene Workflow Execution Access, puede ejecutarlas |
| **Data Sources** | N/A (sin security a nivel objeto) | N/A | Controlado por ManageDataSources role |
| **Transformation Rules** (core/shared) | Everyone | Administrators | Para grupos especificos (ej. por ubicacion), asignar grupos apropiados |
| **Form/Journal Templates** (Groups y Profiles) | Everyone | Administrators | Runtime access depende del Workflow Profile |
| **Cube View Groups** | Everyone | Administrators + constructores | Mantener grupos pequeños para flexibilidad |
| **Dashboard Groups** | Everyone | Administrators + constructores | Mantener grupos pequeños; usar multiples Dashboard Maintenance Units |

---

## Puntos Criticos a Memorizar

### Data Cell Access:
- Flujo de seguridad: Application > Cube > Scenario > Entity > Data Cell Access > Workflow.
- **Data Cell Access** solo puede **disminuir** acceso ya concedido; luego opcionalmente aumentar intersecciones especificas.
- **ViewAllData** bypasa Data Cell Access Security completamente. No se puede restringir datos a un usuario con ViewAllData usando slice.
- **Orden de las reglas** de Data Cell Access es critico (se evaluan secuencialmente).
- **"Increase Access And Stop"** detiene la evaluacion de reglas subsiguientes.
- **Data Cell Conditional Input** impacta a TODOS los usuarios (no es seguridad basada en grupos).
- **Relationship Security** (Use Parent Security) es una configuracion a nivel de Cube, no por usuario.
- Una vez activada, afecta a TODOS los usuarios excepto Administrators y ViewAllData.

### Security Roles:
- **Application Security Roles**: Controlan gestion/accion sobre objetos (ManageMetadata, ModifyData, etc.)
- **Application User Interface Roles**: Controlan acceso a paginas (CubeViewsPage, BusinessRulesPage, etc.)
- Ambos deben trabajar juntos: UI Role da visibilidad, Security Role da capacidad.
- Cuando se crea nueva aplicacion: todo defaults a Administrator excepto **OpenApplication** (Everyone) y **AdministratorDatabase** (Nobody).
- **ModifyData** puede dejarse en Everyone si el resto de seguridad esta bien configurada.

### System Security Roles:
- Se aplican a todo el sistema/framework, no a una aplicacion.
- **ManageSystemConfiguration**: Default a **Nobody**, NO automatico para Administrator.
- **ManageSystemSecurityUsers/Groups/Roles**: Los tres se necesitan para Load/Extract de Security.
- Los usuarios con Manage System Security obtienen acceso automatico a SystemAdministrationLogon y SystemPane UI Roles.
- **Exclusion Groups**: Orden determina acceso (Allow/Deny). Poner grupo general primero (Allow), individuos despues (Deny).
- Roles NO son excluyentes o limitantes. Si se otorgan, los usuarios obtienen funcionalidad adicional.

### System Configuration:
- Cambios se aplican automaticamente cada 2 minutos sin IIS restart.
- 6 categorias: General, Environment, Memory, Multithreading, Recycling, Database Server Connections.
- Memory y Multithreading deben ser habilitados por soporte.
- Los cambios se auditan automaticamente.

### Troubleshooting:
- Verificar IsEnabled, Inactivity Threshold, OpenApplication, Cube Access, Scenario, Entity, Slice, Workflow.
- El Administrator **no se puede deshabilitar** ni es afectado por Inactivity Threshold.
- **User Type** (Interactive, View, Restricted, etc.) es tipo de licencia, NO controla acceso.
- Si un usuario ve "NoAccess" en Data Explorer: verificar Entity Read Data Group o Data Cell Access.
- **Show All Parent Groups for User**: Herramienta util para ver todos los grupos del usuario y entender su acceso.
- **Logon Activity** en System > Logging muestra metodo de login (Client Module: Scheduler, Windows App, etc.).
- Si un usuario no ve workflows: verificar que tenga el Workflow Execution Group asignado.
- Si un usuario ve datos de entities en reports pero no puede cargar: verificar Read/Write Data Group y Workflow Execution.

### Autenticacion:
- Native: External Provider = (Not Used), password interno. La primera vez que el usuario inicia sesion, puede cambiar su contraseña.
- External: Seleccionar proveedor y configurar External Provider User Name (debe ser unico y coincidir con el IdP).
- Proveedores: MSAD, LDAP, Azure AD (Microsoft Entra ID), Okta, PingFederate, SAML 2.0.
- En entornos hosted por OneStream, se usa OneStream IdentityServer.
- La pantalla de logon varia dependiendo de la configuracion de autenticacion.

### Mejores practicas:
- No sobre-aplicar seguridad; es mas facil agregar despues que deshacer.
- Mantener convencion de nombres consistente (ej. `V_Entity` para vista, `M_Entity` para modificacion, `WF_WorkflowName`).
- Usar nesting de groups para facilitar administracion, pero no sobrecomplicar.
- Access Group en Everyone + Maintenance en Administrators es la mejor practica para la mayoria de objetos.
- Para Cube Views: Can Modify Data, Can Calculate, Can Translate, Can Consolidate en False para reportes.
- Asignar ManageData solo a administradores.
- Limitar acceso a File Explorer; cualquier persona con acceso a una carpeta ve todo a traves de OnePlace.
- Diseñar seguridad como parte natural de la implementacion, no como un paso posterior.
- Tener un "security check" para determinar como implementar seguridad a medida que la construccion progresa.
- Para datos sensibles de empleados (People Planning), planificar Event Handlers y BRAPI calls desde el diseño.

---

## Mapeo de Fuentes

| Objetivo | Libro/Capitulo |
|----------|---------------|
| 201.6.1 | Design Reference Guide, Chapter 7 - Implementing Security (Data Cell Access, Entity Security, Relationship Security); Foundation Handbook, Chapter 9 - Security (Data Loaders, Slice Security, View Users) |
| 201.6.2 | Design Reference Guide, Chapter 7 - Implementing Security (Application Security); Chapter 17 - System Security Roles (System Security Roles, System UI Roles, Manage System Security, Application Server Configurations); Foundation Handbook, Chapter 9 - Security (Application Security Roles, System Security Roles) |
| 201.6.3 | Design Reference Guide, Chapter 7 - Implementing Security (Security Configurations, Disable Inactive Users); Chapter 14 - User Authentication and SSO; Chapter 15 - How Users are Configured for Authentication; Chapter 16 - About Managing Users and Groups (Users, Groups, Exclusion Groups, Load/Extract, BRApi); Chapter 17 - System Security Roles (Manage System Security, Combined Roles, File Share Security); Foundation Handbook, Chapter 9 - Security (Common Types of Users, Troubleshooting examples, Relationship Security, Reporting Access) |
