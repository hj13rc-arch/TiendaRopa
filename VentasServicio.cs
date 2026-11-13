using System.Data;

namespace TiendaRopa;

internal sealed record LineaVenta(
    int IdProducto, string Producto, int Cantidad, decimal PrecioUnitario)
{
    public decimal Subtotal => Cantidad * PrecioUnitario;
}

internal sealed class VentasServicio(string cadenaConexion)
{
    private readonly VentasDatos datos = new(cadenaConexion);
    private readonly List<LineaVenta> carrito = new();

    internal IReadOnlyList<LineaVenta> Lineas => carrito;
    internal decimal Total => carrito.Sum(linea => linea.Subtotal);

    internal (DataTable Clientes, DataTable Productos) CargarCatalogos() => datos.CargarCatalogos();

    internal bool Agregar(int id, string descripcion, int unidades, int stock, decimal precio)
    {
        int indice = carrito.FindIndex(linea => linea.IdProducto == id);
        int acumulado = indice >= 0 ? carrito[indice].Cantidad : 0;

        if (unidades > stock - acumulado)
        {
            return false;
        }

        if (indice >= 0)
        {
            carrito[indice] = carrito[indice] with { Cantidad = acumulado + unidades };
        }
        else
        {
            carrito.Add(new LineaVenta(
                id, descripcion, unidades, precio));
        }

        return true;
    }

    internal void Quitar(LineaVenta linea) => carrito.Remove(linea);

    internal void Limpiar() => carrito.Clear();

    internal (int IdVenta, decimal Importe) Registrar(int idCliente, int idUsuario)
    {
        if (carrito.Count == 0)
        {
            throw new InvalidOperationException("Agregá al menos un producto.");
        }

        decimal importe = Total;
        return (datos.Registrar(idCliente, idUsuario, carrito), importe);
    }
}
