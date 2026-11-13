using System.Data;
using System.Text;

namespace TiendaRopa;

internal sealed class ReportesServicio(string cadenaConexion)
{
    private readonly ReportesDatos datos = new(cadenaConexion);

    internal DataTable Historial() => datos.Historial();

    internal DataTable Inventario() => datos.Inventario();

    internal string? Comprobante(int idVenta)
    {
        using var filas = datos.DetalleVenta(idVenta);
        if (filas.Rows.Count == 0)
        {
            return null;
        }

        DataRow primera = filas.Rows[0];
        var texto = new StringBuilder();
        texto.AppendLine($"TIENDA DE ROPA - COMPROBANTE #{idVenta}");
        texto.AppendLine($"Fecha: {primera.Field<DateTime>(0):dd/MM/yyyy HH:mm}");
        texto.AppendLine($"Cliente: {primera.Field<string>(1)}");
        texto.AppendLine($"Atendido por: {primera.Field<string>(2)}");
        texto.AppendLine();

        decimal total = 0;
        foreach (DataRow fila in filas.Rows)
        {
            decimal subtotal = fila.Field<decimal>(7);
            texto.AppendLine($"{fila.Field<string>(3)} - {fila.Field<string>(4)}");
            texto.AppendLine(
                $"  {fila.Field<int>(5)} x C$ {fila.Field<decimal>(6):N2} = C$ {subtotal:N2}");
            total += subtotal;
        }

        texto.AppendLine();
        texto.AppendLine($"TOTAL: C$ {total:N2}");
        return texto.ToString();
    }
}
