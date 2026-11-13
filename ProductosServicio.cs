using System.Data;

namespace TiendaRopa;

internal sealed class ProductosServicio
{
    private readonly ProductosDatos datos = new();

    internal DataTable Buscar(string filtro) => datos.Buscar(filtro.Trim());

    internal bool Guardar(int? id, string codigo, string nombre, string talla,
        string color, decimal precio, int stock)
    {
        codigo = codigo.Trim();
        nombre = nombre.Trim();
        talla = talla.Trim();
        color = color.Trim();

        if (codigo.Length == 0 || nombre.Length == 0 || talla.Length == 0 ||
            color.Length == 0 || precio <= 0)
        {
            throw new ArgumentException(
                "Completá código, nombre, talla y color. El precio debe ser mayor que cero.");
        }

        return datos.Guardar(id, codigo, nombre, talla, color, precio, stock);
    }

    internal void Desactivar(int id) => datos.Desactivar(id);
}
