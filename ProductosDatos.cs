using System.Data;
using Microsoft.Data.SqlClient;

namespace TiendaRopa;

internal sealed class ProductosDatos
{
    internal const string CadenaConexion =
        "Server=localhost;Database=TiendaRopa;Integrated Security=True;TrustServerCertificate=True;";

    internal DataTable Buscar(string filtro)
    {
        const string sql = """
            SELECT IdProducto, Codigo, Nombre, Talla, Color, Precio, Stock
            FROM dbo.Productos
            WHERE Activo = 1
              AND (@buscar = N'' OR CHARINDEX(@buscar, Codigo) > 0
                                 OR CHARINDEX(@buscar, Nombre) > 0)
            ORDER BY Codigo;
            """;
        using var conexion = new SqlConnection(CadenaConexion);
        using var adaptador = new SqlDataAdapter(sql, conexion);
        adaptador.SelectCommand.Parameters.Add("@buscar", SqlDbType.NVarChar, 100)
            .Value = filtro;

        var productos = new DataTable();
        adaptador.Fill(productos);
        return productos;
    }

    internal bool Guardar(int? id, string codigo, string nombre, string talla,
        string color, decimal importe, int stock)
    {
        string sql = id.HasValue
            ? """
              UPDATE dbo.Productos
              SET Codigo = @codigo, Nombre = @nombre, Talla = @talla,
                  Color = @color, Precio = @precio, Stock = @stock
              WHERE IdProducto = @id AND Activo = 1;
              """
            : """
              INSERT INTO dbo.Productos (Codigo, Nombre, Talla, Color, Precio, Stock)
              VALUES (@codigo, @nombre, @talla, @color, @precio, @stock);
              """;

        using var conexion = new SqlConnection(CadenaConexion);
        using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@codigo", SqlDbType.NVarChar, 30).Value = codigo;
        comando.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = nombre;
        comando.Parameters.Add("@talla", SqlDbType.NVarChar, 15).Value = talla;
        comando.Parameters.Add("@color", SqlDbType.NVarChar, 30).Value = color;

        var precio = comando.Parameters.Add("@precio", SqlDbType.Decimal);
        precio.Precision = 10;
        precio.Scale = 2;
        precio.Value = importe;

        comando.Parameters.Add("@stock", SqlDbType.Int).Value = stock;

        if (id is int idProducto)
        {
            comando.Parameters.Add("@id", SqlDbType.Int).Value = idProducto;
        }

        conexion.Open();
        return comando.ExecuteNonQuery() != 0;
    }

    internal void Desactivar(int id)
    {
        const string sql = """
            UPDATE dbo.Productos
            SET Activo = 0
            WHERE IdProducto = @id AND Activo = 1;
            """;

        using var conexion = new SqlConnection(CadenaConexion);
        using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@id", SqlDbType.Int).Value = id;

        conexion.Open();
        comando.ExecuteNonQuery();
    }
}
