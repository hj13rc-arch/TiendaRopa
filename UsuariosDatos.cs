using System.Data;
using Microsoft.Data.SqlClient;

namespace TiendaRopa;

internal sealed class UsuariosDatos(string cadenaConexion)
{
    internal bool HayUsuarios()
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Usuarios) THEN 1 ELSE 0 END;
            """;

        using var conexion = new SqlConnection(cadenaConexion);
        conexion.Open();
        using var comando = new SqlCommand(sql, conexion);
        return (int)comando.ExecuteScalar()! != 0;
    }

    internal (int Id, string Hash, string Rol)? Buscar(string usuario)
    {
        using var conexion = new SqlConnection(cadenaConexion);
        conexion.Open();
        const string sql = """
            SELECT IdUsuario, ClaveHash, Rol

          FROM dbo.Usuarios
          WHERE NombreUsuario = @usuario;
        """;

        using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = usuario;

        using var lector = comando.ExecuteReader();
        return lector.Read()
            ? (lector.GetInt32(0), lector.GetString(1), lector.GetString(2))
            : null;
    }

    internal bool CrearAdministrador(string usuario, string claveHash)
    {
        const string sql = """
            INSERT INTO dbo.Usuarios (NombreUsuario, ClaveHash, Rol)
            SELECT @usuario, @hash, N'Administrador'
            WHERE NOT EXISTS (
                SELECT 1 FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK)
            );
            """;

        using var conexion = new SqlConnection(cadenaConexion);
        conexion.Open();
        using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = usuario;
        comando.Parameters.Add("@hash", SqlDbType.NVarChar, 255).Value = claveHash;

        return comando.ExecuteNonQuery() == 1;
    }

    internal void CrearCajero(string usuario, string claveHash)
    {
        const string sql = """
        INSERT INTO dbo.Usuarios (NombreUsuario, ClaveHash, Rol)
        VALUES (@usuario, @hash, N'Cajero');
        """;

        using var conexion = new SqlConnection(cadenaConexion);
        conexion.Open();
        using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = usuario;
        comando.Parameters.Add("@hash", SqlDbType.NVarChar, 255).Value = claveHash;
        comando.ExecuteNonQuery();
    }
}
