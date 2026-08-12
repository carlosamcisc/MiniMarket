# MiniMarket

Sistema de punto de venta (POS) para un minimarket, desarrollado con **ASP.NET Core 9 MVC** y **SQL Server**. Permite administrar productos e inventario, registrar ventas y generar el ticket de compra en PDF.

## Tabla de contenido

- [Características](#características)
- [Tecnologías](#tecnologías)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Requisitos previos](#requisitos-previos)
- [Configuración e instalación](#configuración-e-instalación)
- [Ejecución](#ejecución)
- [Ejecución con Docker](#ejecución-con-docker)
- [Modelo de datos](#modelo-de-datos)
- [Roadmap / Pendientes](#roadmap--pendientes)

## Características

- **Gestión de productos**: alta, edición, eliminación, detalle y búsqueda por nombre o código.
- **Control de inventario**: cada producto lleva su stock asociado y se descuenta automáticamente al vender.
- **Registro de ventas**: carrito de venta con validación de stock disponible y transacción atómica en base de datos.
- **Ticket en PDF**: al confirmar una venta se genera un comprobante imprimible (formato de recibo angosto) con [iText7](https://itextpdf.com/).
- **Autenticación de usuarios**: login contra la tabla `usuarios` con contraseñas hasheadas (PBKDF2-HMACSHA256) y sesión por cookie; redirección según rol (`admin`, `vendedor`, `gerente`). Las rutas de productos, ventas e inicio requieren sesión iniciada.

## Tecnologías

- [.NET 9](https://dotnet.microsoft.com/) / ASP.NET Core MVC
- Entity Framework Core 9 (`Microsoft.EntityFrameworkCore.SqlServer`)
- SQL Server
- [itext7](https://itextpdf.com/) para generación de PDF
- Newtonsoft.Json
- Docker (imagen `nanoserver`, contenedores Windows)

## Estructura del proyecto

```
MiniMarket/
├── Controllers/         # ProductoController, VentaController, AutenticacionController, HomeController
├── Models/               # Entidades EF Core (Producto, Venta, DetalleVentum, Inventario, Movimiento, Usuario, Role)
│   └── ViewModels/
├── Views/                # Vistas Razor (.cshtml) por controlador
├── wwwroot/              # Archivos estáticos (css, js, lib)
├── Dockerfile
├── Program.cs            # Punto de entrada y configuración de servicios
└── appsettings.json      # Configuración (cadena de conexión, logging)
db_minimarket.sql          # Script de creación de la base de datos
```

## Requisitos previos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express o instancia completa)
- (Opcional) Docker Desktop, si se desea ejecutar en contenedor

## Configuración e instalación

1. **Clonar el repositorio**

   ```bash
   git clone <url-del-repositorio>
   cd MiniMarket
   ```

2. **Crear la base de datos**

   Ejecuta el script `db_minimarket.sql` en tu instancia de SQL Server (por ejemplo desde SQL Server Management Studio o `sqlcmd`). El script crea la base `minimarket` con las tablas `productos`, `inventario`, `ventas`, `detalle_venta`, `movimientos`, `roles` y `usuarios`.

3. **Configurar la cadena de conexión**

   Edita `MiniMarket/appsettings.json` (o `appsettings.Development.json`, o usa [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)) con los datos de tu servidor:

   ```json
   "ConnectionStrings": {
     "miniMarket": "Server=TU_SERVIDOR;Database=minimarket;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

4. **Restaurar dependencias**

   ```bash
   dotnet restore
   ```

## Ejecución

Desde la carpeta `MiniMarket/`:

```bash
dotnet run
```

La aplicación quedará disponible en:

- HTTP: `http://localhost:5277`
- HTTPS: `https://localhost:7266`

Al iniciar, la ruta por defecto redirige a `Autenticacion/Login`.

## Ejecución con Docker

El proyecto incluye un `Dockerfile` basado en imágenes Windows (`nanoserver`):

```bash
docker build -t minimarket -f MiniMarket/Dockerfile .
docker run -p 8080:8080 -p 8081:8081 minimarket
```

> Nota: al usar contenedores, la cadena de conexión debe apuntar a un SQL Server accesible desde el contenedor (no `localhost` del host).

## Modelo de datos

| Tabla            | Descripción                                                        |
|-------------------|---------------------------------------------------------------------|
| `productos`       | Catálogo de productos (nombre, código, precio)                     |
| `inventario`      | Stock disponible por producto                                      |
| `ventas`          | Encabezado de venta (fecha, usuario, total)                        |
| `detalle_venta`   | Líneas de cada venta (producto, cantidad, precio)                  |
| `movimientos`     | Historial de entradas/salidas de inventario                        |
| `usuarios`        | Usuarios del sistema (correo único, contraseña, rol)                |
| `roles`           | Roles de usuario                                                    |

## Usuarios de prueba

El script `db_minimarket.sql` incluye usuarios semilla (contraseñas ya hasheadas, no en texto plano) para poder iniciar sesión tras crear la base de datos:

| Correo                     | Contraseña   | Rol       |
|-----------------------------|--------------|-----------|
| admin@minimarket.com        | admin123     | admin     |
| vendedor@minimarket.com     | vender123    | vendedor  |
| gerente@minimarket.com      | gerente123   | gerente   |

> Cámbialas antes de usar el sistema en un entorno real.

## Roadmap / Pendientes

- Registrar movimientos de inventario (`movimientos`) al vender o reabastecer.
- Migraciones de EF Core automatizadas en lugar del script `.sql` manual.
- Agregar protección CSRF (antiforgery token) al formulario de login y a las peticiones POST de `Venta`.
