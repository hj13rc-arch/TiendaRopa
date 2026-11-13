using System.Data;
using Microsoft.Data.SqlClient;

namespace TiendaRopa;

internal sealed class ReportesDatos(string cadenaConexion)
{
    private const string sqlVentas = """
        SELECT v.IdVenta AS [N.º de venta],
               v.Fecha,
               COALESCE(c.Nombre, N'Consumidor final') AS Cliente,
               u.NombreUsuario AS [Registrada por],
               SUM(d.Cantidad) AS Unidades,
               SUM(d.Subtotal) AS [Total (C$)]
        FROM dbo.Ventas AS v
        INNER JOIN dbo.DetalleVenta AS d ON d.IdVenta = v.IdVenta
        INNER JOIN dbo.Usuarios AS u ON u.IdUsuario = v.IdUsuario
        LEFT JOIN dbo.Clientes AS c ON c.IdCliente = v.IdCliente
        GROUP BY v.IdVenta, v.Fecha, c.Nombre, u.NombreUsuario
        ORDER BY v.Fecha DESC, v.IdVenta DESC;
        """;

    private const string sqlInventario = """
        SELECT Codigo AS [Código], Nombre, Talla, Color,
               Precio AS [Precio (C$)], Stock AS Existencias
        FROM dbo.Productos
        WHERE Activo = 1
        ORDER BY Codigo;
        """;

    internal DataTable Historial() => Consultar(sqlVentas);

    internal DataTable Inventario() => Consultar(sqlInventario);

    private DataTable Consultar(string sql)
    {
        using var conexion = new SqlConnection(cadenaConexion);
        using var adaptador = new SqlDataAdapter(sql, conexion);
        var filas = new DataTable();
        adaptador.Fill(filas);
        return filas;
    }

    internal DataTable DetalleVenta(int idVenta)
    {
        const string sql = """
        SELECT v.Fecha,
               COALESCE(c.Nombre, N'Consumidor final') AS Cliente,
               u.NombreUsuario,
               p.Codigo,
               p.Nombre,
               d.Cantidad,
               d.PrecioUnitario,
               d.Subtotal
        FROM dbo.Ventas AS v
        INNER JOIN dbo.DetalleVenta AS d ON d.IdVenta = v.IdVenta
        INNER JOIN dbo.Productos AS p ON p.IdProducto = d.IdProducto
        INNER JOIN dbo.Usuarios AS u ON u.IdUsuario = v.IdUsuario
        LEFT JOIN dbo.Clientes AS c ON c.IdCliente = v.IdCliente
        WHERE v.IdVenta = @venta
        ORDER BY d.IdDetalle;
        """;

        using var conexion = new SqlConnection(cadenaConexion);
        using var adaptador = new SqlDataAdapter(sql, conexion);
        adaptador.SelectCommand.Parameters.Add("@venta", SqlDbType.Int).Value = idVenta;
        var filas = new DataTable();
        adaptador.Fill(filas);
        return filas;
    }
}
