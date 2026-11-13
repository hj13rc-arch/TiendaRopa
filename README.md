# TiendaRopa

Aplicación de escritorio para registrar ventas, gestionar clientes y productos y consultar inventario. Proyecto de Henry de Jesús Roa Carrión.

## Requisitos

- Windows, Visual Studio con la carga de trabajo Desarrollo de escritorio de .NET y SDK compatible con .NET 9.
- SQL Server y SQL Server Management Studio (SSMS).
- Permisos de Windows para crear la base durante la instalación y para acceder a sus tablas desde la aplicación.

## Instalación

1. En SSMS, conectarse a la instancia de SQL Server y ejecutar [TiendaRopa.sql](TiendaRopa.sql).
2. Abrir TiendaRopa.sln en Visual Studio y restaurar los paquetes NuGet. El proyecto usa Microsoft.Data.SqlClient.
3. La conexión apunta a localhost y usa autenticación integrada de Windows. Si se usa otra instancia, ajustar el valor Server en ProductosDatos.cs.
4. Ejecutar con F5. En una base nueva, la primera pantalla permite crear el administrador. Usar una contraseña de al menos 12 caracteres y entrar con esa cuenta.
5. Registrar productos desde el menú de administrador. Para probar el rol Cajero, crear una cuenta desde el botón Crear cajero.

## Demostración

1. Registrar una prenda con precio y stock desde Productos.
2. Abrir Ventas, elegir Consumidor final, agregar la prenda y registrar la venta.
3. Abrir Reportes como administrador. Consultar el historial y el inventario actualizado.
4. Hacer doble clic sobre la venta para ver el comprobante. Guardarlo como HTML y abrirlo en el navegador para imprimirlo o guardarlo como PDF.

## Arquitectura

| Capa | Archivos |
| --- | --- |
| Presentación | Formularios WinForms |
| Lógica | ProductosServicio, ClientesServicio, VentasServicio, ReportesServicio y UsuariosServicio |
| Datos | ProductosDatos, ClientesDatos, VentasDatos, ReportesDatos y UsuariosDatos |

La venta, sus detalles y el ajuste de existencias se guardan en una sola transacción.
