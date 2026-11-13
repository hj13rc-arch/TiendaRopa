using System.Data;
using Microsoft.Data.SqlClient;

namespace TiendaRopa;

internal sealed class VentasDatos(string cadenaConexion)
{
    internal (DataTable Clientes, DataTable Productos) CargarCatalogos()
    {
        using var conexion = new SqlConnection(cadenaConexion);

        const string sqlClientes = """
            SELECT 0 AS IdCliente, N'Consumidor final' AS Nombre
            UNION ALL
            SELECT IdCliente, Nombre FROM dbo.Clientes
            ORDER BY IdCliente;
            """;

        const string sqlProductos = """
            SELECT IdProducto, Stock, Precio,
                   CONCAT(Codigo, N' - ', Nombre, N' / ', Talla, N' / ', Color,
                          N' (stock: ', Stock, N')') AS Descripcion
            FROM dbo.Productos
            WHERE Activo = 1 AND Stock > 0
            ORDER BY Codigo;
            """;

        using var adaptadorClientes = new SqlDataAdapter(sqlClientes, conexion);
        using var adaptadorProductos = new SqlDataAdapter(sqlProductos, conexion);

        var datosClientes = new DataTable();
        var datosProductos = new DataTable();
        adaptadorClientes.Fill(datosClientes);
        adaptadorProductos.Fill(datosProductos);        return (datosClientes, datosProductos);
    }

    internal int Registrar(int idCliente, int idUsuario, IReadOnlyList<LineaVenta> carrito)
    {
        using var conexion = new SqlConnection(cadenaConexion);
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();

        const string sqlVenta = """
            INSERT INTO dbo.Ventas (IdCliente, IdUsuario)
            OUTPUT INSERTED.IdVenta
            VALUES (@cliente, @usuario);
            """;

        using var venta = new SqlCommand(sqlVenta, conexion, transaccion);
        venta.Parameters.Add("@cliente", SqlDbType.Int).Value =
            idCliente == 0 ? DBNull.Value : (object)idCliente;
        venta.Parameters.Add("@usuario", SqlDbType.Int).Value = idUsuario;
        int idVenta = (int)venta.ExecuteScalar()!;

        foreach (var linea in carrito.OrderBy(item => item.IdProducto))
        {
            const string sqlDetalle = """
                UPDATE dbo.Productos
                SET Stock = Stock - @cantidad
                WHERE IdProducto = @producto AND Activo = 1
                  AND Stock >= @cantidad AND Precio = @precio;

                IF @@ROWCOUNT = 0
                    THROW 50001, N'Cambió el precio o el stock de una prenda, o fue desactivada. Volvé a armar la venta.', 1;

                INSERT INTO dbo.DetalleVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario)
                VALUES (@venta, @producto, @cantidad, @precio);
                """;

            using var detalle = new SqlCommand(sqlDetalle, conexion, transaccion);
            detalle.Parameters.Add("@venta", SqlDbType.Int).Value = idVenta;
            detalle.Parameters.Add("@producto", SqlDbType.Int).Value = linea.IdProducto;
            detalle.Parameters.Add("@cantidad", SqlDbType.Int).Value = linea.Cantidad;

            var precio = detalle.Parameters.Add("@precio", SqlDbType.Decimal);
            precio.Precision = 10;
            precio.Scale = 2;
            precio.Value = linea.PrecioUnitario;

            detalle.ExecuteNonQuery();
        }

        transaccion.Commit();
        return idVenta;
    }
}
