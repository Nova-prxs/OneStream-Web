# Seccion 7: Administration (9% del examen)

## Objetivos del Examen

- **201.7.1**: Demonstrate an understanding of the items available on the system tab
- **201.7.2**: Demonstrate troubleshooting abilities to solve issues

---

## Conceptos Clave

### Objetivo 201.7.1: Elementos disponibles en la pestana System

La pestana **System** proporciona acceso a herramientas de administracion, logging, seguridad y gestion del entorno. Los elementos principales se agrupan en tres grandes areas: **Logging**, **Tools** y **Environment**. La visibilidad de las pestanas esta determinada por las configuraciones de seguridad del usuario.

---

#### 1. LOGGING (System > Logging)

El sistema de logging permite a los administradores monitorear la actividad de usuarios, tareas y errores. Todo el contenido se muestra en grids que se pueden ordenar (clic en encabezado de columna para ascendente/descendente), filtrar y exportar (clic derecho > Export, seleccionar tipo de archivo).

![Ordenar grids de logging](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1823-6922.png)

![Filtrar contenido de grids](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1823-6923.png)

Se puede navegar entre paginas con botones de pagina (primera, anterior, siguiente, ultima) ubicados en la parte inferior izquierda del grid. Para exportar datos: clic derecho > Export > seleccionar formato.

![Exportar datos del grid](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1823-6924.png)

##### Logon Activity

Muestra quien esta conectado y quien se desconecto.

![Pantalla de Logon Activity](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1824-6927.png)

**Columnas del grid:**

| Columna | Descripcion |
|---------|-------------|
| **Logon Status** | Muestra cuando los usuarios iniciaron y cerraron sesion |
| **User** | ID del usuario |
| **Application** | Aplicacion a la que el usuario esta conectado |
| **Client Module** | Tipo de cliente (ej. Excel, Windows App, Scheduler) |
| **Client Version** | Version de la aplicacion en uso |
| **Client IP Address** | Direccion IP del usuario final |
| **Logon Time** | Timestamp de inicio de sesion |
| **Last Activity Time** | Timestamp de ultima actividad |
| **Logoff Time** | Timestamp de cierre de sesion |
| **Primary App Server** | Servidor de aplicacion utilizado |

**Funciones de administrador:**

| Boton | Descripcion |
|-------|-------------|
| ![](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1824-6928.png) **Logoff Selected Session** | Desconectar cualquier sesion de usuario. Solo disponible para administradores |
| **Clear Logon Activity** | Limpiar todo el registro de actividad de logon |

**System Security Roles relacionados:**
- **ViewAllLogonActivity**: Si se asigna el acceso requerido al System tab y a la pagina Logon Activity, los usuarios del grupo asignado pueden ver la actividad de logon de **todos** los usuarios.
- **LogonActivityPage**: Permite acceder a la pagina. Puede ver todos los usuarios pero no puede hacer logoff.

##### Task Activity

Se accede desde **dos lugares**:
- Icono **Task Activity** en la esquina superior derecha del web client
- **System > Logging > Task Activity**

![Pantalla de Task Activity](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1825-6931.png)

**Botones principales:**

| Boton | Descripcion |
|-------|-------------|
| ![](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1825-6932.png) **Clear task activity for current user** | Limpiar actividad del usuario actual |
| ![](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1825-6933.png) **Clear task activity for all users** | Limpiar actividad de TODOS los usuarios |
| ![](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1825-6934.png) **Selected task information** | Drill down de pasos de la actividad seleccionada |
| ![](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1825-6935.png) **Running task progress** | Ver progreso de actividades de otros usuarios |

**Columnas del grid:**

| Columna | Descripcion |
|---------|-------------|
| **Task Type** | Tipo de actividad (Consolidate, Process Cube, Load and Transform, Clear Stage Data, etc.) |
| **Description** | Detalles (POV, Multiple Data Units, etc.) |
| **Duration** | Duracion de la actividad |
| **Task Status** | Estado (Completed, Failed, Canceling, Canceled, Running) |
| **User** | ID del usuario |
| **Application** | Aplicacion donde se proceso la tarea |
| **Server** | Servidor de aplicacion utilizado |
| **Start Time** | Timestamp de inicio |
| **End Time** | Timestamp de fin |
| **Queued CPU** | % de utilizacion CPU cuando la tarea fue iniciada |
| **Start CPU** | % de utilizacion CPU cuando el job comenzo desde la cola |

**Child Steps**: Dentro del grid, hay dos iconos a la izquierda de cada fila. Si estan resaltados, se puede hacer drill down:

![Iconos de Child Steps](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1827-6945.png)

- El **primer icono** muestra los pasos hijos de una tarea particular.
- El **segundo icono** muestra informacion detallada del error cuando esta presente.

**System Security Roles relacionados:**
- **ViewAllTaskActivity**: Los usuarios del grupo asignado pueden ver las tareas y child-steps detallados a traves del icono Task Activity en la barra de herramientas. Tambien pueden verlo en System > Logging > Task Activity si tienen acceso al System tab y pagina.

##### Cancelacion de tareas de larga duracion

**Cube Views:**
Si una Cube View tarda mas de **10 segundos**, aparece un dialogo con barra de progreso indeterminada y botones **Cancel Task** y **Close**.

![Dialogo de cancelacion de Cube View](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1832-6959.png)

- Si se hace clic en **Close**: el dialogo se cierra, Task Activity parpadea (indicando tarea en background), y el reporte se abre cuando se completa.
- Si se hace clic en **Cancel Task**: aparece mensaje de cancelacion y el reporte no se ejecuta.
- Si no se hace clic en nada: el dialogo se cierra solo cuando el reporte termina de cargar.

**Comportamiento del icono Task Activity:**
- El icono **parpadea** cuando hay tareas corriendo en segundo plano que llevan mas de 10 segundos y no se esta en el dialogo de Task Activity.
- El icono **no parpadea** si un dialogo de Task Activity esta abierto.
- Para todas las tareas no-UI (consolidacion u otras tareas de larga duracion), el icono comenzara a parpadear dentro de unos minutos.

![Task Activity con opciones de cancelacion](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1832-6961.png)

**Tabla de comportamiento de cancelacion por tipo:**

| Tipo | Comportamiento al exceder 10 segundos | Metodo de cancelacion |
|------|---------------------------------------|----------------------|
| **Cube View** (Data Explorer, Refresh) | Dialogo con barra de progreso | Cancel Task o Close en el dialogo |
| **Show Report** | Dialogo con barra de progreso | Cancel Task o Close en el dialogo |
| **Export to Excel** | Dialogo con barra de progreso | Cancel Task o Close en el dialogo |
| **Dashboard con Cube View Components** | **NO aparece dialogo emergente** | Usar Task Activity para cancelar cada Cube View individual |
| **Quick Views** | Via Task Activity en Excel Add-In o Spreadsheet | Running Task Progress > Cancel Task |
| **XFGetCell Refresh** | Via Task Activity | Running Task Progress > Cancel Task (muestra barra con porcentaje) |

**Nota**: Cancelar la carga de la siguiente pagina (Next Page) en Cube View/Quick View muestra texto **#REFRESH** en las celdas.

**Task Activity en Excel:**

![Task Activity en Excel Add-In](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1828-6948.png)

Se puede acceder al icono Task Activity en Excel Add-In en la seccion Tasks del ribbon de OneStream. Los administradores pueden cancelar tareas de otros usuarios. Non-admins tienen la opcion de mostrar solo tareas en ejecucion.

En Task Activity, la columna Task Status muestra si la tarea fue cancelada por un User o Administrator. Cada tarea tiene su task type (Quick View, Cube View, Get Excel Data).

##### Detailed Logging (para Cube Views)

Configuracion **individual en cada Cube View**, en las pestanas Designer y Advanced. **Ya no esta** en el App Server Config file ni en TALogCubeViews.

- Propiedad **Use Detailed Logging**: por defecto **False**.
- Cuando es **True**, se registran pasos individuales e informacion adicional sobre la Cube View en Task Activity.

![Ubicacion de Use Detailed Logging en Cube View](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1838-6982.png)

**Procedimiento para habilitar:**
1. En el tab Application, bajo Presentation, hacer clic en Workspaces.
2. En Application Workspaces, bajo Workspaces, expandir Default > Maintenance Unit > Default > Cube View Groups.
3. Seleccionar una Cube View.
4. Hacer clic en el tab Designer y seleccionar Common bajo General settings.
5. Cambiar Use Detailed Logging a True.

![Resultado de Detailed Logging en Task Activity](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1839-6986.png)

##### Cube View Paging y Quick View Paging

**Paging Controls**: Se navega entre paginas con flechas (primera, anterior, siguiente). Cada pagina muestra numero de pagina y porcentaje de datos cargados.

![Paging controls de Cube View](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1840-6989.png)

![Porcentaje de datos cargados](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1840-6990.png)

**Tooltip de paginacion**: Muestra total rows, rows processed, unsuppressed rows en una pagina.

![Tooltip de paginacion](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1840-6991.png)

**Quick View Paging**: Los controles de paginacion ahora se ubican **debajo de las tres pestanas** (ya no en el tab Quick Views).

![Quick View paging controls](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1843-6998.png)

**Configuraciones de Cube View que impactan paging (System Configuration):**

| Propiedad | Default | Descripcion |
|-----------|---------|-------------|
| Max Number of Expanded Cube View Rows | Server Config | Maximo de filas al expandir una Cube View |
| Max Number of Unsuppressed Rows Per Cube View Page | 2000 | Maximo de filas por pagina. Valor maximo: 100,000 |
| Max Number Seconds To Process Cube View | 20 | Impacta comportamiento de paging. Valor maximo: 600 segundos |

Estas configuraciones pueden sobrescribirse por Cube View individual usando las propiedades General Settings/Common/Paging en cada Cube View.

**Task Activity Configurations (System Configuration):**

| Propiedad | Default | Descripcion |
|-----------|---------|-------------|
| Log Books | True | Registra en Task Activity cuando items del libro son incluidos como pasos |
| Log Cube Views | False | Registra cuando una Cube View se abre, reporte se ejecuta o exporta a Excel |
| Log Quick Views | False | Registra cuando un Quick View se crea o filas/columnas se mueven |
| Log Get Data Cells Threshold | N/A | Registra llamadas a GetDataCells si el numero de celdas es >= al valor |

##### Error Logs

Acceso: **System > Logging > Error Logs**.

![Pantalla de Error Logs](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1846-7011.png)

**Columnas del grid:**

| Columna | Descripcion |
|---------|-------------|
| **Description** | Descripcion breve del error |
| **Error Time** | Timestamp del error |
| **Error Level** | Tipo de error (Unknown, Fatal, Warning, etc.) |
| **User** | ID del usuario |
| **Application** | Aplicacion donde ocurrio el error |
| **Tier** | Capa de la aplicacion (App Server, Web Server, Client) |
| **App Server** | Servidor de aplicacion al que el usuario estaba conectado |

**Funciones:**

| Boton | Descripcion |
|-------|-------------|
| ![](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1847-7014.png) **Clear error log for current user** | Limpiar logs del usuario actual |
| **Clear error log for all users** | Limpiar logs de TODOS los usuarios |

**System Security Roles relacionados:**
- **ViewAllErrorLog**: Los usuarios del grupo asignado pueden ver el Error Log de todos los usuarios.
- **ErrorLogPage**: Permite acceder a la pagina de Error Log.

---

#### 2. TOOLS (System > Tools)

##### Database

Acceso: **System > Tools > Database**.

![Pantalla de Database](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1851-7025.png)

Permite ver tablas de la base de datos:
- **Application Database > Tables**: Tablas de la aplicacion actual
- **System Database > Tables**: Tablas del sistema
- Acceso de **solo lectura** a las tablas de datos, util para debugging.
- Tablas importadas de MarketPlace Solutions usan el schema name como prefijo (ej. `rcm.AccessGroup`, `txm.AccessGroup`).

**Data Records**: Application Database > Tools > Data Records, para ver datos del sistema completo con filtro de miembros.

**System Security Role**: **DatabasePage** permite acceso a esta seccion. Solo system administrators por defecto.

##### Environment

Acceso: **System > Tools > Environment** (solo accesible via **OneStream Windows App**, no browser).

![Pantalla principal de Environment](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1852-7028.png)

La pagina Environment esta diseñada para dar a usuarios de IT y power business users una forma de gestionar y optimizar sus aplicaciones y el entorno. Permite monitorear el entorno, aislar cuellos de botella, ver propiedades y cambios de configuracion, y escalar servidores y recursos de base de datos.

**Secciones principales del Environment:**

##### Monitoring

![Monitoring page](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1853-7031.png)

Proporciona acceso a KPIs en tiempo real, graficos interactivos y actividad de usuarios. En lugar de iniciar sesion en el servidor para recolectar metricas, se usa esta pagina.

**Acciones disponibles:**
- **Open**: Acceder a archivo de metricas y configuraciones desde File System o carpeta local.
- **Save As**: Guardar metricas y configuraciones localmente o en el File System.
- **Settings**: Especificar tipos de metricas KPI para monitorear: Environment, Application Servers, Database Servers, Server Sets.
- **Zoom**: Acercar parte del grafico para ver actividades en ejecucion o en cola.
- **Refresh Automatically Using a Timer**: Recuperar metricas basado en el intervalo Play Update Frequency.

**Configuraciones generales de Monitoring:**

| Propiedad | Descripcion |
|-----------|-------------|
| Play Update Frequency (seconds) | Frecuencia de actualizacion de graficos de rendimiento |
| Metric and Task Time Range | Cantidad de datos historicos a recuperar. Ayuda a identificar la causa de un evento |
| Y-Axis Auto Range | Si seleccionado, el sistema establece Min/Max automaticamente |
| Secondary Y-Axis | Para series con diferente rango de valores o tipos mixtos de datos |

**Configuraciones de filtro de Monitoring:**

| Propiedad | Descripcion |
|-----------|-------------|
| Filter Type | Tipo de servidores para los cuales recolectar metricas |
| Source Filter | Filtro para la Source List |
| Source List | Servidores que cumplen los criterios |
| Result List | Items seleccionados para recolectar metricas |

**Metricas recopilables (configuradas en Server Configuration Utility):**

![Configuracion de metricas](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1855-7036.png)

| Categoria | Metricas |
|-----------|----------|
| **Environment** | CPU, Task, Login |
| **Server Set** | CPU, Task |
| **Server** | Disk, Memory, Network Card |
| **SQL Server** | CPU, Page (Page Life Expectancy), Memory, Connection, Query (Deletes/Inserts), File Growth |
| **SQL Elastic Pool** | CPU, DTU, Storage, Workload |

##### Web Servers

Lista todos los servidores web del entorno con configuracion y auditoria.

![Web Server Configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1857-7042.png)

**Propiedades de configuracion:**

| Propiedad | Descripcion |
|-----------|-------------|
| Identity Provider | Proveedor de SSO si se usa |
| Server Heartbeat Update Interval (seconds) | Frecuencia de actualizacion del heartbeat |
| Name | Nombre del servidor definido en el archivo de configuracion web |
| WCF Address | URL completa del servidor |
| WCF Connection | Estado de la conexion (Ok = conectado) |
| Used for General Access | True/False. Si el servidor esta configurado para acceso general |
| Used for Stage Load | True/False. Si esta configurado para Stage |
| Used for Consolidation | True/False. Si esta configurado para consolidacion |
| Used for Data Management | True/False. Si esta configurado para Data Management |

**Audit**: Muestra cambios de propiedades con Property Type, Property Name, Value From/To, Description From/To, Timestamp From/To, User From/To.

![Web Server Audit](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1859-7047.png)

##### Web to App Server Connections

Lista todas las conexiones combinadas desde archivos de configuracion web a todos los servidores de aplicacion.

![Web to App Server Connections](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1862-7056.png)

| Accion | Descripcion |
|--------|-------------|
| **Pause** | Pausar cualquier solicitud a una conexion WCF Address. La conexion puede ser un Application Server o un Load Balancer |
| **Resume** | Reanudar solicitudes a la conexion |

Las mismas propiedades de configuracion que Web Servers (Name, WCF Address, WCF Connection, Used for General Access/Stage Load/Consolidation/Data Management).

##### Application Server Sets

![Application Server Sets](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1864-7061.png)

Muestra los server sets del entorno. Una "X" roja indica servidores offline.

**Behavior tab:**

![Server Sets Behavior](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1865-7064.png)

- **Scale Out** (remover) y **Scale In** (agregar y configurar): Disponible si Scaling Type es Manual o ManualAndBusinessRule.
- Propiedades configurables en Server Configuration Utility, pero sobrescribibles a nivel de servidor individual:
  - Process Queued Consolidation Tasks
  - Process Queued Data Management Tasks
  - Process Queued Stage Tasks
  - Queued Tasks Require Named Application Server

**Configuration tab:**

![Server Sets Configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1866-7067.png)

| Propiedad | Descripcion |
|-----------|-------------|
| Azure Resource Group Name | (Azure Scale Set Only) Nombre del resource group |
| Azure Scale Set Name | (Azure Scale Set Only) Nombre del scale set en el resource group |
| Can Change Queuing Options On Servers | Si True, Admins pueden cambiar comportamiento de queuing |
| Can Pause or Resume Servers | Si True, el usuario puede pausar/reanudar desde Environment |
| Can Stop or Start Servers | (Azure Scale Set Only) Si True, puede detener/reiniciar |
| Maximum/Minimum Capacity | (Azure Scale Set Only) Numero max/min de servidores |

**Audit tab:** Identifica cambios en XFAppServerConfig.xml. Los cambios se resaltan en **amarillo**.

![Server Sets Audit](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1868-7072.png)

##### Application Server Behavior

![Application Server Behavior](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1869-7075.png)

| Accion | Descripcion |
|--------|-------------|
| **Pause** | Detiene la aceptacion de nuevas tareas de la cola, pero permite que las tareas ya iniciadas terminen |
| **Resume** | El servidor reanuda la aceptacion de tareas de la cola |
| **Recycle App Pool** | Reset IIS para un servidor especifico |
| **Stop** (Azure Only) | Detiene el servidor. Continua incurriendo cargos Azure compute. IP publica e interna se preservan |
| **Stop (Deallocate)** (Azure Only) | Detiene sin cargos de VM, pero IP publica e interna se eliminan |

La disponibilidad de estos botones depende de configuraciones en OneStream Server Configuration Utility.

**Application Server tabs adicionales:**

| Tab | Descripcion |
|-----|-------------|
| **Configuration** | Configuraciones del servidor desde Server Configuration Utility |
| **Hardware** | Informacion de hardware de la maquina |
| **Audit** | Historial de cambios de hardware y configuracion |
| **Performance** | Metricas del servidor y Environment |

![Application Server Hardware](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1871-7081.png)

##### Database Servers (Connection Items)

Lista conexiones de bases de datos basadas en Server Configuration Utility.

| Tab | Descripcion |
|-----|-------------|
| **Behavior** | Disponible si SQL Server Azure y Elastic Pool configurados. Permite aumentar/disminuir recursos |
| **Configuration** | Propiedades de configuracion de SQL Server |
| **Hardware** | Informacion de hardware de SQL Server |
| **Audit** | Cambios a propiedades de SQL Server |
| **Performance** | Metricas de SQL Server |
| **Diagnostics** | Comandos de diagnostico SQL |

![Database Server Diagnostics](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1879-7105.png)

**Diagnostics disponibles:**
- **SQL Deadlock information**: Lista deadlocks en la instancia SQL Server
- **Top SQL Commands**: Lista los top comandos SQL por Total Logical Reads, Total Logical Writes, o Total Worker Time

![Top SQL Commands](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1880-7108.png)

##### OneStream Database Servers (Schema Items)

Lista todos los esquemas de base de datos.

| Tab | Descripcion |
|-----|-------------|
| **Configuration** | Informacion especifica de la aplicacion |
| **Audit** | Cambios de configuracion de la aplicacion |
| **Diagnostic** | Reporte de fragmentacion de tablas del esquema actual |

![Table Fragmentation Report](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1882-7115.png)

##### System Business Rules

Se usan **System Extender Business Rules** con Azure Server Sets para escalabilidad mejorada a nivel de Azure Database y Server Sets. Permiten Server y eDTU scaling manual o via Business Rules.

![System Extender Business Rule vacia](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1849-7019.png)

Si System Business Rules es seleccionado como Scaling Type, una regla Extender definida por el usuario determina si se necesita escalar. Las metricas de Environment y Scale Set (para server scaling) o SQL Server Elastic Pool (para database scaling) se pasan a la funcion.

##### File Explorer

Acceso: System > Tools > File Explorer, o icono File Explorer en la barra de herramientas.

![File Explorer principal](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1891-7139.png)

**Tres raices de almacenamiento:**

| Raiz | Descripcion |
|------|-------------|
| **Application Database** | Documentos de la aplicacion actual |
| **System Database** | Documentos del sistema completo sin afectar la aplicacion actual |
| **File Share** | Directorio self-service externo a las bases de datos |

**Carpetas de Application:**

![Application Folders](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1895-7154.png)

| Carpeta | Descripcion |
|---------|-------------|
| **Batch/Harvest** | Automatizacion de Connector Data Loads. Harvest se limpia automaticamente al cargar y se crea carpeta archivo. Requiere Admin o ManageFileShare para agregar/eliminar archivos |
| **Content** | Almacenamiento seguro para archivos grandes. Gestionado por System Security Roles |
| **Data Management** | Carpeta default para exportaciones de Data Management |
| **Groups** | Archivos por Security Group para dashboard components con File Source Type de File Share |
| **Incoming** | Archivos fuente para importacion a Stage. Tambien usado por File Viewer Component |
| **Internal** | Contenido de archivos de Application/System Database |
| **Outgoing** | Disponible para procesos personalizados |

**File Share:**

![File Share](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1892-7144.png)

File Share es un directorio self-service que soporta almacenamiento de archivos externo a las bases de datos de OneStream. Los archivos almacenados en una carpeta Incoming son accesibles solo a traves de ese Workflow Profile.

**Content Folders (Application y System):**
- Carpeta autogenerada para almacenar archivos **mayores de 300 MB**.
- Area de almacenamiento segura que puede usarse en lugar del File Explorer application database.
- Permisos controlados por System Security Roles:
  - **Administrator** y **ManageFileShare**: Derechos completos
  - Non-Administrators se asignan via File Share Security Roles (ManageFileShareContents, AccessFileShareContents, RetrieveFileShareContents)

![Content Folder Permissions](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1894-7150.png)

**Tamaños de archivo soportados:**

| Interfaz | Uploads (App/System) | Uploads (Content) | Downloads |
|----------|---------------------|--------------------|-----------|
| **Windows Application** | Hasta 300 MB | Hasta **2 GB** | Hasta 2 GB |

**POV guardados:** Se pueden guardar en Public (todos los usuarios) o User directory. Clic derecho en Cube POV > Save Cube POV to Favorites.

![Guardar POV en File Explorer](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1892-7143.png)

##### Whitelist File Extensions

Define que extensiones de archivo se permiten en File Explorer. Ayuda a aliviar el riesgo de subir tipos de archivo maliciosos.

![Whitelist File Extensions configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1903-7181.png)

**Procedimiento paso a paso:**

1. Ir a Start > OneStream Software > OneStream Server Configuration Utility (Run as Administrator).
2. File > Open Application Server Configuration File.
3. Buscar el XFAppServerConfig.xml (tipicamente en `C:\OneStreamShare\Config`).
4. Localizar **Whitelist File Extensions** y hacer clic en los puntos suspensivos (...).
5. Hacer clic en (+) y escribir la extension (ej. txt).
6. Continuar agregando extensiones.
7. Hacer clic en OK, luego Save.
8. **Reiniciar IIS** (obligatorio).

![Agregar extensiones](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1904-7184.png)

**Nota**: Clientes cloud deben contactar soporte para este cambio. Cuando el whitelist esta vacio, cualquier tipo de archivo puede guardarse. Cuando esta definido, archivos con extensiones no incluidas seran rechazados al intentar subir.

**Tambien configurable via System Configuration** (General > White List File Extensions), lo cual evita el reinicio de IIS ya que los cambios se aplican automaticamente cada 2 minutos.

##### Load/Extract System Artifacts

Solo para **System Administrators**. Importacion/exportacion de secciones del sistema en formato XML.

![Load/Extract System Artifacts](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1764-8030.png)

**Opciones de extraccion:**

| Opcion | Descripcion |
|--------|-------------|
| **Extract** | Exportar a archivo XML en ubicacion especificada |
| **Load** | Importar desde archivo XML |
| **Extract and Edit** | Extraer y modificar el XML directamente |

**Items extraibles:**

| Item | Descripcion |
|------|-------------|
| **Security** | System Roles, Users, Security Groups, Exclusion Groups. Opcion Extract Unique IDs disponible |
| **System Dashboards** | Maintenance Units, Groups, Dashboard Components, Adapters, Parameters, Profiles |
| **Error Log** | Con filtro de Start Time y End Time |
| **Task Activity** | Con filtro de Start Time y End Time |
| **Logon Activity** | Con filtro de Start Time y End Time |

**Extract Unique IDs**: Si esta marcada, se extraen los IDs unicos de OneStream. Al mover entre entornos donde ya existen registros, **desmarcar** esta opcion para evitar errores.

##### Profiler

Herramienta para **desarrolladores** que captura cada evento procesado en una sesion de usuario.

![Pantalla de Profiler](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1910-7199.png)

**Security Roles:**

| Rol | Tipo | Descripcion |
|-----|------|-------------|
| **ManageProfiler** | System Security Role | Ejecutar sesiones de Profiler y ver Profiler Events. Default: Administrator |
| **ProfilerPage** | System UI Role | Ver la pagina de Profiler. NO puede ejecutar sesiones ni ver eventos. Default: Administrator |

Los administradores pueden terminar cualquier sesion activa de Profiler.

**Propiedades de sesion:**

| Propiedad | Default | Max | Descripcion |
|-----------|---------|-----|-------------|
| Description | (vacio) | N/A | Descripcion de la sesion |
| Number of Minutes to Run | 20 | **60** | Duracion maxima. Valores > 60 se resetean al default |
| Number of Hours to Retain Before Deletion | 24 | **168** | Horas antes de eliminar. Valores > 168 se resetean al default. Se elimina en el primer server restart despues del tiempo |

**Filtros de Profiler:**

| Filtro | Descripcion |
|--------|-------------|
| Include Top Level Methods | Captura entry points de alto nivel (acciones de usuario, API calls, workflow steps) |
| Include Adapters | Incluye llamadas de Data Adapter |
| Include Business Rules | Incluye Business Rules |
| Include Formulas | Incluye formulas |
| Include Assembly Factory | Incluye Assembly Factory calls (etiquetadas como "Factory") |
| Include Assembly Methods | Incluye Assembly Method calls (etiquetadas como "WSAS") |
| Business Rule Filter | Filtro por nombre de Business Rule |
| Formula Filter | Filtro por nombre de Formula |
| Assembly Method Filter | Filtro por nombre de metodo |

**Wildcards en filtros:**
- `?` = cualquier caracter individual (ej. `SS?B` encuentra SSIB, SSVB)
- `*` = cualquier cadena (ej. `*Validation` encuentra reglas que terminan en Validation)
- Separados por coma para multiples filtros

**Profiler Events:**

![Profiler Events window](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1916-7223.png)

| Columna | Descripcion |
|---------|-------------|
| **Event Type** | Top, Queue, Adapter, Formula, BR, Factory, WSAS, Manual |
| **Workspace** | Nombre del Workspace que contiene el item |
| **Source** | Origen del evento (varia segun tipo: nombre de regla, adaptador, formula, metodo) |
| **Method** | Metodo llamado |
| **Description** | Descripcion del tipo de evento |
| **Entity** | Miembro de Entity asociado (vacio si no aplica) |
| **Cons** | Miembro de Consolidation (vacio si no aplica) |
| **Duration** | Duracion del evento |
| **Server** | Servidor |
| **Thread ID** | ID del thread del sistema operativo |

**Post Processing:**

![Post Processing window](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1919-7233.png)

Calculate Cumulative Durations permite ver informacion en diferentes agrupaciones, totalizar items filtrados y ver resultados. Util para mejorar queries de rendimiento: filtrar por event types especificos y agruparlos para ver que metodos o eventos toman mas tiempo.

**Event Information:**
- **Method Inputs**: Parametros pasados al metodo
- **Method Result**: Resultado o valores retornados
- **Method Error**: Errores encontrados durante la sesion
- **Objects** y **Text**: Habilitados para Manual event types si se loguean via BRApi calls

![Event Information window](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1922-7242.png)

**BRApi.Profiler Methods:**

| Metodo | Descripcion |
|--------|-------------|
| `BRApi.Profiler.IsProfiling(si)` | Determina si profiling esta habilitado para la sesion |
| `BRApi.Profiler.LogMessage(si, brProfilerSettings, "Description", "Detail")` | Registra un mensaje personalizado en Profiler Events |
| `BRApi.Profiler.LogObjects(si, brProfilerSettings, "Description", [objects])` | Registra el estado de objetos para diagnostico |

**IMPORTANTE**: El rendimiento de la aplicacion puede verse impactado si multiples usuarios ejecutan Profiler simultaneamente o realizan tareas de larga duracion mientras otros ejecutan Profiler.

##### Time Dimensions

Las aplicaciones pueden tener una Time Dimension mensual o semanal.

**Time Dimension Types:**

| Tipo | Descripcion |
|------|-------------|
| **Standard** | Mensual. Almacena datos por mes. Aplicaciones creadas antes de 4.1.0 usan este tipo |
| **StandardUsingBinaryData** | Mensual con datos en tabla binaria. Usar si se puede necesitar convertir a Weekly despues |
| **M12_3333_W52_445** | 12 meses, 4 trimestres, 52 semanas, calendario 445 |
| **M12_3333_W52_454** | 12 meses, 4 trimestres, 52 semanas, calendario 454 |
| **M12_3333_W52_544** | 12 meses, 4 trimestres, 52 semanas, calendario 544 |
| **M12_3333_W53_445** | 12 meses, 4 trimestres, 53 semanas, calendario 445 |
| **M12_3333_W53_454** | 12 meses, 4 trimestres, 53 semanas, calendario 454 |
| **M12_3333_W53_544** | 12 meses, 4 trimestres, 53 semanas, calendario 544 |
| **Custom** | Permite especificar meses por trimestre y semanas por mes. Solo para nuevas aplicaciones |

**Propiedades de Custom:**
- **Use Weeks**: Si True, define Weekly Time Dimension. Si False, Monthly.
- **Vary Settings By Year**: Si True, especificar semanas por mes por año. Si False, aplicar a todos los años.
- **M1-M16 Number of Weeks**: Semanas por mes.

**Nota critica**: Una vez creada la aplicacion, **no se puede editar** la Time Dimension (pero se puede ver en POV). Se genera un archivo XML que nuevas aplicaciones usan para implementar la Time Dimension deseada.

##### Azure Configurations

Solo aplicable a Azure o si se usa Azure Elastic Pool.

**Azure Subscription Settings**: Deben completarse cuando se usa Azure Elastic Pool o Scale Sets.

![Azure Subscription Settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1883-7118.png)

**Environment Monitoring:**

![Environment Monitoring settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1884-7121.png)

**Propiedades clave de Environment Monitoring:**

| Propiedad | Default | Descripcion |
|-----------|---------|-------------|
| URL for Automatic Recycle Service | N/A | URL del servicio de reciclaje. Puerto default 50002 |
| Number of Running Hours Before Automatic Recycle | 24.0 | Frecuencia de reciclaje. 0.0 para desactivar |
| Start Hour for Automatic Recycle (UTC) | 5 | Hora mas temprana para reciclar (solo si Running Hours = 24.0) |
| End Hour for Automatic Recycle (UTC) | 7 | Hora mas tardia para reciclar |
| Maximum Number of Minutes to Pause Before Automatic Recycle | 30 | Tiempo para permitir tareas activas terminar antes de reciclar |
| Active Check Update Interval (seconds) | N/A | Frecuencia de chequeos del sistema (deadlocks, etc.) |
| Metric Update Interval (seconds) | N/A | Frecuencia de actualizacion de metricas |
| Server Heartbeat Update Interval (seconds) | N/A | Frecuencia de actualizacion de heartbeat |

**Task Load Balancing:**

![Task Load Balancing](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1888-7130.png)

| Propiedad | Descripcion |
|-----------|-------------|
| Maximum Queued Processing Interval (seconds) | Frecuencia con que el thread de queuing busca nuevas tareas |
| Number Past Metric Reading For Analysis | Numero de lecturas de metricas para analisis de demanda del servidor |
| Maximum Queued Time (minutes) | **Tiempo maximo antes de cancelar** una tarea en cola |
| Maximum Average CPU Utilization | CPU maximo promedio antes de determinar que un servidor no puede tomar una tarea |
| Task Logging Only | Si True, solo registra tareas recogidas |
| Detailed Logging | Si True, registra cada entrada/salida de la funcion de Task Load Balancing |

---

#### 3. NAVEGACION (OnePlace Layout)

##### Estructura principal de la interfaz

![OnePlace Layout completo](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p323-2047.png)

- **Navigation Pane**: Tres pestanas (System, Application, OnePlace). Visibilidad basada en seguridad del usuario. Cada barra puede mostrarse fija (pinned) o con Auto Hide.
- **Home**: Icono grande de OneStream para navegar a la pantalla de inicio del usuario.

##### Application Tray (barra de herramientas superior)

| Icono | Funcion |
|-------|---------|
| **Hamburger Menu** (Navigation Pane) | Ocultar/mostrar pestanas Application, System, OnePlace |
| **Navigate Recent Pages** | Dialogo para volver a paginas recientes |
| **File Explorer** | Acceso a carpetas publicas, documentos y File Share |
| **Environment Name** | Nombre personalizable por entorno (Development, Test, Production). Configurable en Application Server Config file |
| **User ID and Application** | Muestra usuario actual y aplicacion |
| **Logon/Logoff Icon** | End Session (cierra sesion y remueve password guardado) o Change Application (mantiene sesion) |
| **Task Activity** | Muestra todas las tareas realizadas |
| **Refresh Application** | Refresca la aplicacion y verifica el primer tab abierto |
| **Pin/Unpin Navigation Pane/POV Pane** | Ocultar/mostrar paneles |
| **Clipboard** | Hasta 10 items (data cells, text, rule scripts) |
| **Create Windows Shortcut** | Crear acceso directo de escritorio |
| **Help** | Documentacion OneStream |

![Application Tray icons](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p324-2051.png)

##### Point of View (POV)

Panel en el lado derecho de la aplicacion. Puede fijarse (pin) o ocultarse automaticamente.

![Point of View panel](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p330-2082.png)

| Seccion | Descripcion | Editable por usuario |
|---------|-------------|---------------------|
| **Global POV** | Scenario y Time. Configurado por administrador | **No** |
| **Workflow POV** | Workflow, Scenario, Time. Basado en Time Dimension Profile del Cube asignado al Workflow Profile | **No** |
| **Cube POV** | Todas las dimensiones del Cube. Activo y actualizable | **Si** |

**Guardar POV favorito**: Clic derecho en Cube POV > Save Cube POV to Favorites. Se guarda en Application > Documents > Users > (User Name) > Favorites.

![Seleccionar User POV](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p332-2087.png)

**User Defined Descriptions**: Al pasar el cursor sobre un miembro seleccionable del POV, se muestra la descripcion definida. Dimensiones fijas (no seleccionables) muestran la descripcion y añaden "Not Used by Current Page".

##### Page Settings

| Opcion | Descripcion |
|--------|-------------|
| **Refresh Page** | Refrescar la pagina actual |
| **Close Page** | Cerrar la pagina actual |
| **Create Shortcut** | Crear acceso directo en la carpeta Favorites del usuario. Funciona para Cube Views, Dashboards, o paginas de Application/System |
| **Set Current Page as Home Page** | Controla pagina default y pinning de Navigation/POV panels al iniciar sesion. Funciona en browser y Windows App |
| **Clear Home Page Setting** | Remover configuracion actual de home page |
| **Save Home Page Setting As Default For New Users** | Guardar como default para nuevos usuarios |
| **Close All Pages** | Cerrar todas las paginas abiertas |
| **Close All Pages Except Current Page** | Cerrar todas excepto la actual |

**Workflow Bar**: Muestra posicion en el proceso de Workflow. Verde = completado, azul = incompleto, icono blanco = tarea actual.

##### Acceso al OneStream Windows App

El Windows App es util porque:
- Se actualiza automaticamente cuando se actualiza el servidor.
- No requiere derechos de admin para descargar o usar.
- Ofrece funcionalidad de spreadsheet robusta (puede no necesitar Excel Add-in).

**Crear acceso directo de escritorio:**

![Create Windows Shortcut](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p318-7939.png)

##### Pantallas de Logon

La pantalla de logon varia segun la configuracion de autenticacion del entorno:

| Configuracion | Pantalla |
|--------------|----------|
| Solo Native Authentication | Campos de usuario y contraseña |
| Solo External Identity Provider | Boton External Provider Sign In |
| Native + External | Ambos disponibles |

![Native Authentication Only](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p320-2038.png)

![Native + External](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p321-2042.png)

**Nota**: SSO con external identity provider puede configurarse para requerir un verification code de una sola vez para autenticacion.

---

### Objetivo 201.7.2: Habilidades de troubleshooting para resolver problemas

#### Herramientas de Troubleshooting

##### Task Activity como punto de partida

- Primer lugar para verificar si una Calculation/Consolidation se completo exitosamente o fallo.
- Filtrar por Task Type.
- Drill down en pasos hijos para ver detalles de error.
- Task Status: Completed, Failed, Canceling, Canceled, Running.

**Child Steps detallados:**
Los dos iconos a la izquierda de cada fila permiten:
1. Ver pasos hijos de la tarea (duracion de cada paso)
2. Ver informacion detallada del error

##### Error Log

- **System > Logging > Error Logs**.
- Es un repositorio de TODOS los errores de OneStream, no solo Calculations.
- Usar filtros para encontrar el error relevante.
- Contiene: Description, Error Time, Error Level, User, Application, Tier, App Server.

##### Escribir en el Error Log (Logging manual)

| Metodo | Disponibilidad | Rendimiento |
|--------|---------------|-------------|
| `api.LogMessage` | Solo Finance Business Rules | **Mejor rendimiento** |
| `BRApi.ErrorLog.LogMessage` | TODOS los tipos de reglas | Menor rendimiento |

Se puede registrar:
- **Strings**: Directamente
- **Decimales**: Convertidos con `.ToString()` o `.XFToString()`
- **Listas**: Con `String.Join` o `For Each`
- **Data Buffers**: Con `.LogDataBuffer` (requiere API, string para nombre, integer para max celdas - **recomendado 100**)

**Error Level** puede cambiarse con argumento `XFErrorLevel`.

**PRECAUCION**: Al escribir en el error log, evitar incluir informacion sensible. OneStream intenta filtrar y redactar informacion sensible, reemplazandola con `[Redacted]`.

##### Stopwatch (cronometro)

- Clase VB.NET `System.Diagnostics.Stopwatch`.
- Agregar `Imports System.Diagnostics` al header.
- `Stopwatch.StartNew()` para iniciar, `.Elapsed` para registrar tiempo transcurrido.
- Util para identificar que parte del codigo toma mas tiempo.

##### Calculate With Logging (Calculation Drill Down)

- Se ejecuta desde Data Management (Consolidate With Logging / Calculate With Logging) o Cube View (Force Consolidate With Logging).
- Permite drill down en Child Steps de Task Activity para ver cada paso del DUCS con duracion.
- Se puede ver: Data Units dependientes, cada Consolidation Member procesado, Formula Passes, Business Rules, formulas individuales.
- El paso `CreateDataUnitCache` muestra el numero de data records en cache (Data Unit Size).
- **Solo disponible** para Calculations que corren dentro del DUCS. Custom Calculations se analizan individualmente via Data Management.
- **ADVERTENCIA**: Consolidate/Calculate With Logging agrega tiempo significativo de procesamiento.

##### Profiler

- Captura eventos de Business Rules, Formulas, Workspace Assemblies.
- Ver inputs y outputs de metodos.
- Util para troubleshooting de rendimiento.
- Post Processing permite agrupar por Event Type y ver duraciones acumulativas.
- Limitado a **60 minutos** max por sesion y **168 horas** de retencion.
- Si default values se eliminan y no se ingresa valor, Profiler defaultea a 20 min/24 hrs. Si se ingresa cero, defaultea a 1 min/1 hr.

##### Rubber Duck Debugging

- Metodo de troubleshooting donde se articula el problema paso a paso a un objeto inanimado.
- Util cuando todas las demas opciones se han agotado.

#### Gestion de entornos - Mejores practicas

- **Cambios al sistema** deben seguir procedimientos de mejores practicas: primero desplegarse y probarse en un entorno de desarrollo.
- Se recomienda hacer una copia reciente de la base de datos de produccion, renombrarla y usarla como base para cambios.
- Antes de desplegar a produccion, extraer cambios del desarrollo y evaluarlos en un entorno de prueba separado.
- **Desplegar cambios a produccion** debe evitar tiempos de alta carga y alta actividad.

**Artefactos que especialmente NO deben cambiarse en produccion durante alta actividad:**
- Business Rules (especialmente con Global functions)
- Confirmation Rules
- Metadata (especialmente con member formulas)

**Recomendaciones adicionales:**
- Entornos estandar: programar cambios de produccion durante periodos lentos o fuera de horario.
- Entornos grandes: considerar funcionalidad de **Pause** dentro del Environment tab.
- Considerar el OneStream Solution **Process Blocker**, que permite pausar procesos criticos para mantenimiento sin cerrar la aplicacion.
- IIS tiene un **Idle Time-Out** setting para OneStreamAppAppPool. Debe establecerse en **0** ya que OneStream tiene otras configuraciones para reciclar IIS.
- Para entornos activos y globales con Data Management Sequences ejecutandose regularmente, se recomienda **reciclar IIS cada 24 horas**.

#### Errores Comunes

| Error | Causa | Solucion |
|-------|-------|----------|
| Calculation no produce resultados | Dimensiones no coinciden en Data Buffers | Usar LogDataBuffer, verificar Common Members, ajustar Member Scripts |
| Resultados inconsistentes | Problema de Formula Pass (dependencias en el mismo pass) | Cambiar a Formula Pass 16 para aislar; reasignar passes correctamente |
| Compilation Error | Typo o sintaxis incorrecta | Ver linea y razon del error, corregir y recompilar |
| Invalid Member Name | Typo en formula string (ej. `A#Priec`) | Corregir el nombre del miembro |
| Unclosed Parentheses | Parentesis sin cerrar en formula | Verificar matching de parentesis |
| Unbalanced Buffer | Data Buffers con diferentes Common Members | Usar funciones Unbalanced (MultiplyUnbalanced, etc.) |
| Data Explosion | Dimensiones en source no presentes en destination | Nunca usar `#All`; usar Unbalanced functions o colapsar dimensiones |
| Object Not Set to Instance | Variable declarada pero no definida | Definir la variable; prestar atencion a warnings de compilacion |
| Given Key Not Present in Dictionary | Parametros no definidos en Data Management Step | Usar `XFGetValue` con default; verificar parametros en DM Step |
| Invalid Destination Script | Data Unit Dimensions en el destination Member Script | Usar `If` statements para filtrar Data Units; solo Account-level Dimensions en destination |
| Duplicate Members in Filter | Miembro aparece dos veces en el filtro | Verificar que filtros no incluyan miembros duplicados |

---

## Puntos Criticos a Memorizar

### Logging:
- **Task Activity** muestra estado de tareas (Completed, Failed, Canceled) y permite cancelar tareas de larga duracion.
- Se accede desde **dos lugares**: icono en barra superior y System > Logging > Task Activity.
- El icono de Task Activity **parpadea** cuando una tarea lleva mas de 10 segundos en background.
- El icono **no parpadea** si un dialogo de Task Activity esta abierto.
- **Detailed Logging** se configura **por Cube View individual** (no en el config file del servidor).
- El **Error Log** es accesible en System > Logging y recibe tanto errores automaticos como mensajes manuales via `api.LogMessage`.
- **ViewAllLogonActivity**, **ViewAllErrorLog**, **ViewAllTaskActivity** permiten ver informacion de todos los usuarios (no solo la propia).
- Para cancelar Dashboards con Cube View Components: **no hay dialogo emergente**, usar Task Activity.
- Cancelar Next Page en Cube View/Quick View muestra **#REFRESH** en las celdas.

### Environment:
- **Environment** solo es accesible via **OneStream Windows App** (no browser).
- Permite monitorear, aislar cuellos de botella, ver configuraciones y escalar servidores.
- **Pause** en un Application Server detiene la aceptacion de nuevas tareas de la cola, pero permite que las tareas en curso terminen.
- **Recycle App Pool** = reset IIS para un servidor especifico.
- **Stop (Azure)** mantiene cargos compute e IPs; **Stop (Deallocate)** no cobra VM pero elimina IPs.
- SQL Diagnostics: Deadlock information y Top SQL Commands (Total Logical Reads/Writes/Worker Time).
- Table Fragmentation report disponible en Schema Items > Diagnostic.

### Tools:
- **Database** proporciona acceso **solo lectura** a tablas de datos. Util para debugging.
- **Profiler** esta limitado a **60 minutos** max por sesion y **168 horas** de retencion.
- Las **Whitelist File Extensions** se configuran en el **Application Server Configuration File** o via System Configuration. Config file requiere IIS restart; System Configuration aplica automaticamente.
- **File Share Content folders** soportan archivos hasta **2 GB** (Windows App), mientras que Application/System Database soportan hasta **300 MB** (uploads).
- **Load/Extract System Artifacts** solo esta disponible para **System Administrators** y usa formato XML.
- La opcion **Extract Unique IDs** debe desmarcarse al mover seguridad entre entornos donde ya existen registros.

### System Configuration:
- Cambios se aplican cada **2 minutos** sin IIS restart.
- **Recycling**: Default 24 hrs, entre 05:00-07:00 UTC, 30 min de pausa antes de reciclar.
- **Task Load Balancing**: Maximum Queued Time es el tiempo maximo antes de cancelar una tarea en cola.
- **Cube View settings** (Max Rows, Max Unsuppressed Rows Per Page, Max Seconds) se pueden sobrescribir por Cube View individual.

### Troubleshooting:
- **Calculate With Logging** es la herramienta clave para troubleshooting de rendimiento de Calculations, pero agrega tiempo significativo.
- Usar `api.LogMessage` en Finance Rules (no `BRApi.ErrorLog.LogMessage`) para mejor rendimiento.
- `LogDataBuffer` con max **100 celdas** para evitar sobrecarga del servidor.
- **Profiler** con BRApi.Profiler.LogMessage y LogObjects para diagnostico avanzado.
- No cambiar artefactos criticos (Business Rules, Confirmation Rules, Metadata) durante alta actividad en produccion.
- Reciclar IIS cada 24 horas para entornos activos con DM Sequences regulares.
- IIS Idle Time-Out debe estar en **0** para OneStreamAppAppPool.

### Navegacion:
- **Navigation Pane** tiene 3 pestanas: System, Application, OnePlace. Visibilidad por seguridad.
- **Global POV** (Scenario, Time) no es editable por usuario final.
- **Cube POV** es activo y actualizable por el usuario.
- **Set Current Page as Home Page** controla pagina default Y pinning de Navigation/POV panels.
- Funciona en browser y Windows App; cambios se transfieren entre ambos.
- **Workflow Bar**: Verde = completado, azul = incompleto, icono blanco = tarea actual.
- Windows App se actualiza automaticamente y no requiere admin rights para descargar.

---

## Mapeo de Fuentes

| Objetivo | Libro/Capitulo |
|----------|---------------|
| 201.7.1 (System Tab items) | Design Reference Guide - Chapter 18: Logging (Logon Activity, Task Activity, Cancel tasks, Detailed Logging, Paging, Error Logs) |
| 201.7.1 (System Tab items) | Design Reference Guide - Chapter 19: System Tools (Database, Environment, Monitoring, Web Servers, Application Server Sets, Application Server Behavior, Database Servers, File Explorer, File Share, Whitelist, Load/Extract, Profiler, Time Dimensions, Azure Configurations, Task Load Balancing) |
| 201.7.1 (System Tab items) | Design Reference Guide - Chapter 3: Navigation (OnePlace Layout, Application Tray, POV, Page Settings, Logon screens) |
| 201.7.1 (System Tab items) | Design Reference Guide - Chapter 17: System Security Roles (System Configuration, General/Environment/Recycling/Database settings, Audits) |
| 201.7.2 (Troubleshooting) | Design Reference Guide - Chapter 18: Logging (Task Activity drill down, Error Logs, Cancel mechanisms) |
| 201.7.2 (Troubleshooting) | Design Reference Guide - Chapter 19: System Tools (Environment diagnostics, Profiler events/post-processing/BRApi, Database Diagnostics) |
| 201.7.2 (Troubleshooting) | Design Reference Guide - Chapter 7: Implementing Security (Managing Environment best practices, IIS recycling) |
| 201.7.2 (Troubleshooting) | Finance Rules - Chapter 7: Troubleshooting and Performance (Error types, Logging, Calculation Drill Down, Stopwatch) |
