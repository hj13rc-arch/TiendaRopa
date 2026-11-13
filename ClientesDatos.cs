using System.Data;
using Microsoft.Data.SqlClient;

namespace TiendaRopa;

internal sealed class ClientesDatos(string cadenaConexion)
{
    internal DataTable Buscar(string filtro)
    {
        const string sql = """
            SELECT IdCliente, Nombre, Telefono AS [Teléfono]
            FROM dbo.Clientes
            WHERE @buscar = N''
               OR CHARINDEX(@buscar, Nombre) > 0
               OR CHARINDEX(@buscar, Telefono) > 0
            ORDER BY Nombre, IdCliente;
            """;

        using var conexion = new SqlConnection(cadenaConexion);
        using var adaptador = new SqlDataAdapter(sql, conexion);
        adaptador.SelectCommand.Parameters.Add("@buscar", SqlDbType.NVarChar, 100)
            .Value = filtro;

        var clientes = new DataTable();
        adaptador.Fill(clientes);
        return clientes;
    }

    internal bool Guardar(int? id, string nombre, string telefono)
    {
        string sql = id.HasValue
            ? "UPDATE dbo.Clientes SET Nombre = @nombre, Telefono = @telefono WHERE IdCliente = @id;"
            : "INSERT INTO dbo.Clientes (Nombre, Telefono) VALUES (@nombre, @telefono);";

        using var conexion = new SqlConnection(cadenaConexion);
        using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = nombre;
        comando.Parameters.Add("@telefono", SqlDbType.NVarChar, 20).Value =
            telefono.Length == 0 ? DBNull.Value : (object)telefono;

        if (id is int idCliente)
        {
            comando.Parameters.Add("@id", SqlDbType.Int).Value = idCliente;
        }

        conexion.Open();
        return comando.ExecuteNonQuery() > 0;
    }

    internal bool Eliminar(int id)
    {
        const string sql = "DELETE FROM dbo.Clientes WHERE IdCliente = @id;";

        using var conexion = new SqlConnection(cadenaConexion);
        using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@id", SqlDbType.Int).Value = id;

        conexion.Open();
        return comando.ExecuteNonQuery() > 0;
    }
}