using System.Data;

namespace TiendaRopa;

internal sealed class ClientesServicio(string cadenaConexion)
{
    private readonly ClientesDatos datos = new(cadenaConexion);

    internal DataTable Buscar(string filtro) => datos.Buscar(filtro.Trim());

    internal bool Guardar(int? id, string nombre, string telefono)
    {
        nombre = nombre.Trim();

        if (nombre.Length == 0)
        {
            throw new ArgumentException("Escribí el nombre del cliente.");
        }

        return datos.Guardar(id, nombre, telefono.Trim());
    }

    internal bool Eliminar(int id) => datos.Eliminar(id);
}