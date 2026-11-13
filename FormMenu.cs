namespace TiendaRopa;

public sealed class FormMenu : Form
{
    public FormMenu(int idUsuario, string rol)
    {
        bool esAdministrador = rol == "Administrador";

        Text = $"Tienda de Ropa - {rol}";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(480, esAdministrador ? 310 : 180);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        if (esAdministrador)
        {
            panel.Controls.Add(Boton("Productos", () =>
            {
                using var productos = new Form1();
                productos.ShowDialog(this);
            }));
        }

        panel.Controls.Add(Boton("Clientes", () =>
        {
            using var clientes = new FormClientes(Form1.CadenaConexion);
            clientes.ShowDialog(this);
        }));

        panel.Controls.Add(Boton("Ventas", () =>
        {
            using var ventas = new FormVentas(idUsuario);
            ventas.ShowDialog(this);
        }));

        if (esAdministrador)
        {
            panel.Controls.Add(Boton("Reportes", () =>
            {
                using var reportes = new FormReportes();
                reportes.ShowDialog(this);
            }));

            panel.Controls.Add(Boton("Crear cajero", () =>
            {
                using var registro = new FormLogin(crearCajero: true);
                registro.ShowDialog(this);
            }));
        }

        panel.Controls.Add(Boton("Salir", Close));
        Controls.Add(panel);
    }

    private static Button Boton(string texto, Action accion)
    {
        var boton = new Button { Text = texto, Width = 180, Height = 36 };
        boton.Click += (_, _) => accion();
        return boton;
    }
}