using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace TiendaRopa;

public sealed class FormLogin : Form
{
    private readonly UsuariosServicio servicio = new(Form1.CadenaConexion);

    private readonly TextBox txtUsuario = new() { Width = 330, MaxLength = 50 };
    private readonly TextBox txtClave = new()
    {
        Width = 330,
        MaxLength = 128,
        UseSystemPasswordChar = true
    };
    private readonly Button btnContinuar = new()
    {
        Width = 330,
        Height = 38,
        Enabled = false
    };

    private bool configuracionInicial;
    public int IdUsuario { get; private set; }
    public string Rol { get; private set; } = "";
    public FormLogin(bool crearCajero = false)
    {
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(390, 220);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        AcceptButton = btnContinuar;

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        panel.Controls.AddRange(new Control[]
        {
            new Label { Text = "Usuario", AutoSize = true },
            txtUsuario,
            new Label { Text = "Contraseña", AutoSize = true },
            txtClave,
            btnContinuar
        });

        Controls.Add(panel);
        if (crearCajero)
        {
            Text = "Crear cuenta de cajero";
            btnContinuar.Text = "Crear cajero";
            btnContinuar.Enabled = true;
            btnContinuar.Click += (_, _) => CrearCajero();
        }
        else
        {
            btnContinuar.Click += (_, _) => Continuar();
            Shown += (_, _) => ComprobarUsuarios();
        }
    }

    private void ComprobarUsuarios()
    {
        try
        {
            configuracionInicial = !servicio.HayUsuarios();
            Text = configuracionInicial ? "Crear administrador" : "Iniciar sesión";
            btnContinuar.Text = configuracionInicial ? "Crear administrador" : "Ingresar";
            btnContinuar.Enabled = true;
        }
        catch (SqlException ex)
        {
            MessageBox.Show(this, ex.Message, "Error al consultar usuarios");
        }
    }

    private void Continuar()
    {
        string usuario = txtUsuario.Text.Trim();
        string clave = txtClave.Text;

        if (usuario.Length == 0 || string.IsNullOrWhiteSpace(clave))
        {
            MessageBox.Show(this, "Escribí el usuario y la contraseña.");
            return;
        }

        try
        {
            if (configuracionInicial)
            {
                CrearAdministrador(usuario, clave);
                return;
            }

            if (servicio.Autenticar(usuario, clave) is { } cuenta)
            {
                IdUsuario = cuenta.Id;
                Rol = cuenta.Rol;
                DialogResult = DialogResult.OK;
                return;
            }
          

            MessageBox.Show(this, "Usuario o contraseña incorrectos.");
            txtClave.SelectAll();
            txtClave.Focus();
        }
        catch (SqlException ex)
        {
            MessageBox.Show(this, ex.Message, "Error al acceder a TiendaRopa");
        }
    }

    private void CrearAdministrador(string usuario, string clave)
    {
        if (clave.Length < 12)
        {
            MessageBox.Show(this, "Usá una contraseña de al menos 12 caracteres.");
            return;
        }

        bool creado = servicio.CrearAdministrador(usuario, clave);
        MessageBox.Show(this, creado
            ? "Administrador creado. Escribí tu contraseña para ingresar."
            : "Ya existe una cuenta. Iniciá sesión con esa cuenta.");

        txtClave.Clear();
        ComprobarUsuarios();
        txtClave.Focus();
    }

    private void CrearCajero()
    {
        string usuario = txtUsuario.Text.Trim();
        string clave = txtClave.Text;

        if (usuario.Length == 0 || string.IsNullOrWhiteSpace(clave) || clave.Length < 12)
        {
            MessageBox.Show(this, "Escribí un usuario y una contraseña de al menos 12 caracteres.");
            return;
        }

        try
        {
            servicio.CrearCajero(usuario, clave);

            MessageBox.Show(this, "Cuenta de cajero creada.");
            DialogResult = DialogResult.OK;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            MessageBox.Show(this, "Ese nombre de usuario ya existe.");
        }
        catch (SqlException ex)
        {
            MessageBox.Show(this, ex.Message, "Error al crear el cajero");
        }
    }
}
