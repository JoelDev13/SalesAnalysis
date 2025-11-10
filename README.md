# SalesAnalysis ETL Architecture

## 1. Descripción general

El proyecto sigue una arquitectura Onion/Clean para aislar la lógica de dominio del acceso a datos y de los adaptadores externos. Los componentes principales son:

- **Dominio (SalesAnalysis.Domain)**: Entidades, contratos (`IExtractor`, `ITransformer`, `IDataLoader`, `ICustomerReadRepository`, `ILoggerService`, `IStagingWriter`, `IEtlService`) y opciones de configuración (`CustomerEtlOptions`).
- **Aplicación (SalesAnalysis.Application)**: Casos de uso `CustomerService` que orquestan el ETL expuesto a través de la interfaz `ICustomerService`. Devuelve DTOs y encapsula reglas de negocio.
- **Persistencia (SalesAnalysis.Persistence)**: Implementaciones concretas para cada contrato. Incluye extractores (CSV, DB, API), transformadores, repositorios EF Core, loader de datos, logger y un `StagingFileWriter` que deja los datos en archivos temporales antes de cargarlos.
- **Worker (SalesAnalysis.Worker)**: Servicio hospedado en segundo plano que ejecuta el proceso ETL de forma periódica usando las capas anteriores y respetando los intervalos configurados.
- **API y Web**: Adaptadores REST/MVC que permiten disparar o visualizar la información (por completar en fases posteriores si se requiere UI o dashboards adicionales).

## 3.1 Arquitectura

![Diagrama de Arquitectura](Recursos/Diagrama/Diagrama%20de%20arquitectura.png)

## 2. Flujo ETL

1. **Extracción**
   - `CsvExtractor<CustomerCsv>` lee encuestas/clientes desde archivos CSV con `CsvHelper`, validando encabezados y escribiendo logs con `ILogger`.
   - `DatabaseExtractor<Customer>` ejecuta consultas SQL sobre la base relacional usando ADO.NET (`SqlClient`).
   - `ApiExtractor<CustomerApiResponse>` consume una API REST con `HttpClientFactory`, deserializa JSON y aplica políticas de logging.

2. **Transformación**
   - `CustomerTransformer` normaliza los datos provenientes de CSV y API hacia la entidad `Customer` del DWH (trim, normalización básica).

3. **Carga**
   - `StagingFileWriter` persiste los registros transformados en archivos JSON dentro del directorio configurado (`CustomerEtl:StagingDirectory`).
   - `EfCoreDataLoader<Customer>` inserta los datos en la base analítica usando EF Core y transacciones.

4. **Orquestación**
   - `EtlService.ExecuteAsync` coordina los pasos anteriores, registra métricas con `Stopwatch` y delega en `ILoggerService` para registrar eventos/errores.
   - `CustomerService.RunEtlAsync` (Application) actúa como fachada para Worker/API/Web.
   - `SalesAnalysis.Worker.Worker` ejecuta el caso de uso de manera periódica, respetando la configuración `CustomerEtl:RunIntervalMinutes`.

## 3. Atributos de calidad

| Atributo      | Prácticas implementadas |
|---------------|--------------------------|
| **Rendimiento** | Extracción y carga asíncronas; uso de `Stopwatch` y logs para medir tiempos; inserción en lote con EF Core y transacciones. |
| **Escalabilidad** | Fábrica de extractores (`IExtractorFactory`) y configuración modular (`CustomerEtlOptions`) permiten agregar nuevas fuentes o cambiar endpoints sin modificar la lógica central. |
| **Seguridad** | Credenciales y endpoints se manejan vía `appsettings.json`/User Secrets (`UserSecretsId` en Worker). No se hardcodean keys ni cadenas. `HttpClientFactory` admite cerraduras adicionales (auth handlers). |
| **Mantenibilidad** | Onion Architecture: dominio sin dependencias externas, Application usa interfaces, Persistence implementa detalles. `ServiceRegistration` centraliza DI. El Worker es un adaptador del caso de uso. |

## 4. Configuración centralizada

- `appsettings.json` (Worker/API) contiene `ConnectionStrings` y la sección `CustomerEtl` (rutas, queries, base URL, directorio de staging, intervalo).
- `SalesAnalysis.Worker` habilita `UserSecretsId` para almacenar credenciales sensibles fuera del control de código.
- HttpClient nombrado (`CustomerEtlOptionsDefaults.ApiClientName`) configura la URL base automáticamente.



