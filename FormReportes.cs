using System.Data;
using Microsoft.Data.SqlClient;

namespace TiendaRopa;

public sealed class FormReportes : Form
{
    private readonly ReportesServicio servicio = new(Form1.CadenaConexion);

    public FormReportes()
    {
        Text = "Tienda de Ropa - Reportes";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(940, 480);

        var pestañas = new TabControl { Dock = DockStyle.Fill };
        pestañas.TabPages.Add(CrearReporte("Historial de ventas", servicio.Historial, "Total (C$)"));
        pestañas.TabPages.Add(CrearReporte("Inventario", servicio.Inventario, "Precio (C$)"));
        Controls.Add(pestañas);
    }

    private TabPage CrearReporte(string titulo, Func<DataTable> consultar, string columnaImporte)
    {
        var pagina = new TabPage(titulo);
        var tabla = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false
        };

        var actualizar = new Button
        {
            Text = "Actualizar",
            Dock = DockStyle.Top,
            Height = 38
        };

        pagina.Controls.Add(tabla);
        pagina.Controls.Add(actualizar);

        actualizar.Click += (_, _) => Cargar(tabla, consultar, columnaImporte, titulo);
        if (titulo == "Historial de ventas")
        {
            tabla.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex >= 0 &&
                    tabla.Rows[e.RowIndex].DataBoundItem is DataRowView fila &&
                    fila["N.º de venta"] is int idVenta)
                {
                    MostrarComprobante(idVenta);
                }
            };
        }
        Shown += (_, _) => Cargar(tabla, consultar, columnaImporte, titulo);

        return pagina;
    }

    private void MostrarComprobante(int idVenta)
    {
        try
        {
            string? texto = servicio.Comprobante(idVenta);
            if (texto is null)
            {
                MessageBox.Show(this, "La venta no tiene detalles disponibles.");
                return;
            }

            using var ventana = new Form
            {
                Text = $"Comprobante de venta #{idVenta}",
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(600, 400)
            };
            var btnGuardar = new Button
            {
                Text = "Guardar comprobante",
                Dock = DockStyle.Bottom,
                Height = 42
            };

            btnGuardar.Click += (_, _) =>
            {
                using var guardar = new SaveFileDialog
                {
                    Title = "Guardar comprobante de venta",
                    Filter = "Documento HTML (*.html)|*.html",
                    FileName = $"Comprobante_{idVenta}.html"
                };

                if (guardar.ShowDialog(ventana) != DialogResult.OK)
                {
                    return;
                }

                string contenido = System.Net.WebUtility.HtmlEncode(texto);

                string html = $$"""
    <!DOCTYPE html>
    <html lang="es">
    <head>
        <meta charset="utf-8">
        <title>Comprobante de venta #{{idVenta}}</title>
        <style>
            body { font: 16px Arial, sans-serif; margin: 32px; }
            pre { font: inherit; line-height: 1.6; white-space: pre-wrap; }
        </style>
    </head>
    <body>
        <pre>{{contenido}}</pre>
    </body>
    </html>
    """;

                try
                {
                    System.IO.File.WriteAllText(
                        guardar.FileName, html, System.Text.Encoding.UTF8);

                    MessageBox.Show(ventana,
                        "Comprobante guardado. Abrilo en el navegador y presioná Ctrl + P.",
                        "TiendaRopa");
                }
                catch (System.IO.IOException ex)
                {
                    MessageBox.Show(ventana, ex.Message, "Error al guardar");
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show(ventana, ex.Message, "Error al guardar");
                }
            };

            ventana.Controls.Add(btnGuardar);
            ventana.Controls.Add(new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Text = texto
            });

            ventana.ShowDialog(this);
        }
        catch (SqlException ex)
        {
            MessageBox.Show(this, ex.Message, "Error al consultar el comprobante");
        }
    }
    private void Cargar(DataGridView tabla, Func<DataTable> consultar, string columnaImporte, string titulo)
    {
        try
        {
            tabla.DataSource = consultar();
            tabla.Columns[columnaImporte]!.DefaultCellStyle.Format = "N2";
        }
        catch (SqlException ex)
        {
            MessageBox.Show(this, ex.Message, $"Error al cargar {titulo}");
        }
    }
}