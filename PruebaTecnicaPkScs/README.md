# PruebaTecnicaPkScs



Aplicación web desarrollada con \*\*ASP.NET Core MVC y .NET 8\*\* que consume la API publica de Pokémon (PokeAPI). Permite consultar Pokémon, aplicar filtros, navegar mediante paginación manual, consultar detalles individuales, exportar resultados a Excel y preparar correos electronicos.



\## 1. Tecnologías utilizadas



\- C# y .NET 8.

\- ASP.NET Core MVC.

\- PokeAPI: \[https://pokeapi.co/](https://pokeapi.co/)

\- HTML, CSS y Bootstrap.

\- HttpClient e inyección de dependencias.

\- IMemoryCache para almacenamiento temporal en memoria.

\- ClosedXML para generar archivos de Excel.

\- MailKit para el envío de correos mediante SMTP.

\- Mailtrap Sandbox para probar los mensajes sin enviarlos a destinatarios reales.



\## 2. Funcionalidades



\- Listado de Pokémones con imagen, nombre e identificador.

\- Filtro por nombre, con coincidencias parciales y sin distinguir mayúsculas y minúsculas.

\- Filtro por especie cargado desde PokeAPI.

\- Paginación manual de 20 registros por página.

\- Conservación de los filtros al navegar entre páginas.

\- Vista de detalles con imagen, altura, peso y tipos del Pokémon.

\- Exportación a Excel de los Pokémones correspondientes a la página actual y a los filtros seleccionados.

\- Envío individual de información de un Pokémon por correo.

\- Envío por correo de los Pokémon de la página actual.

\- Manejo de errores HTTP, tiempos de espera y respuestas no validas.

\- Cache temporal del catalogo de Pokémon, catalogo de especies y detalles individuales de especies y Pokémon.



\## 3. Requisitos previos



Para ejecutar el proyecto se necesita:



\- Windows u otro sistema compatible con .NET 8.

\- SDK de .NET 8.

\- Visual Studio con la carga de trabajo de desarrollo de ASP.NET y web, o una herramienta compatible con .NET 8.

\- Conexión a Internet para consultar PokeAPI.

\- Una cuenta de Mailtrap para realizar pruebas de correo.



\## 4. Cómo ejecutar el proyecto



1\. Clonar el repositorio o descargar el código fuente.

2\. Abrir la solución en Visual Studio.

3\. Restaurar los paquetes NuGet.

4\. Configurar los secretos de usuario para el correo, siguiendo la sección correspondiente de este README.

5\. Compilar la solución.

6\. Ejecutar el proyecto.

7\. Abrir la ruta `/Pokemon` utilizando el dominio y puerto que proporcione Visual Studio.



El servicio HTTP utiliza un tiempo máximo de espera de 15 segundos para las solicitudes a PokeAPI.



\## 5. Configuración del correo



La aplicación utiliza MailKit y SMTP. Para el entorno de desarrollo se propone Mailtrap Sandbox, que permite verificar los mensajes capturados sin entregarlos a destinatarios reales.



\### Configurar los secretos



En Visual Studio:



1\. Hacer clic derecho sobre el proyecto.

2\. Seleccionar \*\*Administrar secretos de usuario\*\*.

3\. Configurar la sección `EmailSettings` con las credenciales SMTP proporcionadas por Mailtrap.



Ejemplo de configuración:



```json

{

&#x20; "EmailSettings": {

&#x20;   "SmtpHost": "sandbox.smtp.mailtrap.io",

&#x20;   "SmtpPort": 587,

&#x20;   "SenderName": "Pokemon API",

&#x20;   "SenderEmail": "pokemon-api@example.com",

&#x20;   "Username": "USUARIO\_SMTP",

&#x20;   "Password": "CLAVE\_SMTP"

&#x20; }

}

```



\### Probar el envío de correo



1\. Iniciar la aplicación.

2\. Abrir `/Pokemon`.

3\. Introducir una dirección de destinatario en el formulario de una tarjeta y seleccionar \*\*Enviar este Pokémon\*\*.

4\. Revisar el mensaje capturado en Mailtrap Sandbox.

5\. Repetir la prueba con el formulario \*\*Enviar página actual\*\*.

6\. Verificar que el contenido incluya los Pokémon de la página seleccionada.



Mailtrap Sandbox captura los mensajes para pruebas. Un envio aceptado por el Sandbox no significa que el correo haya sido entregado a una bandeja real.



Para utilizar un proveedor de correo real, deben configurarse sus credenciales, remitentes autorizados y requisitos de autenticación correspondientes.



\## 6. Decisiones técnicas



\### Cache en memoria



Se utiliza `IMemoryCache` para almacenar el catalogo de Pokémon, el catalogo de especies y los detalles consultados durante 30 minutos.



Se eligió esta estrategia para reducir las solicitudes repetidas a PokeAPI y mejorar el tiempo de respuesta al aplicar filtros o volver a consultar una especie.



La cache se pierde cuando se reinicia la aplicación.



\### Exportación a Excel con ClosedXML



Se utiliza ClosedXML para generar archivos `.xlsx` sin necesitar Microsoft Excel instalado en el servidor.



El archivo se genera en memoria y se devuelve como una descarga HTTP. Se decidió exportar únicamente los Pokémones de la página actual, una vez aplicados los filtros, tomando como supuesto que los registros visibles corresponden a la página seleccionada y no a todas las coincidencias del catalogo.



Esta decisión evita descargar accidentalmente datos que el usuario no está consultando en ese momento.



\### Consumo de PokeAPI y manejo de tiempos de espera



Las llamadas HTTP se centralizan en `PokeApiService`, registrado mediante `IHttpClientFactory`. Se configuró un tiempo máximo de espera de 15 segundos para evitar que una solicitud quede esperando indefinidamente.



Se utilizan operaciones asíncronas y manejo de errores para distinguir fallos de conexión, tiempos de espera y respuestas JSON invalidas.



El catalogo se obtiene mediante los parámetros de paginación de PokeAPI y se almacena temporalmente en memoria.



\### Envío de correo con MailKit



Se eligió MailKit para implementar el envío SMTP y mantener esa responsabilidad separada del controlador MVC.



Las credenciales se obtienen de la configuración de la aplicación y de los secretos de usuario, no están escritas directamente en el servicio ni deben incluirse en el repositorio.



Se utiliza Mailtrap Sandbox durante el desarrollo para comprobar el contenido de los mensajes sin enviarlos a destinatarios reales.



