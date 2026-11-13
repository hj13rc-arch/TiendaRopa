using System.Data;
using Microsoft.Data.SqlClient;

namespace TiendaRopa;

public sealed class FormClientes : Form
{
    private readonly string cadenaConexion;
    private readonly TextBox txtNombre = new() { Width = 220, MaxLength = 100 };
    private readonly TextBox txtTelefono = new() { Width = 150, MaxLength = 20 };
    private readonly TextBox txtBuscar = new()
    {
        Width = 230,
        MaxLength = 100,
        PlaceholderText = "Nombre o teléfono"
    };
    private readonly DataGridView tabla = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    private int? idSeleccionado;

    public FormClientes(string cadenaConexion)
    {
        this.cadenaConexion = cadenaConexion;
        Text = "Tienda de Ropa - Clientes";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(760, 430);
        MinimumSize = new Size(640, 350);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 140,
            Padding = new Padding(8)
        };
        panel.Controls.AddRange(new Control[]
        {
            Etiqueta("Nombre"), txtNombre, Etiqueta("Teléfono"), txtTelefono
        });
        panel.SetFlowBreak(txtTelefono, true);
        panel.Controls.AddRange(new Control[]
        {
            Boton("Nuevo", Limpiar),
            Boton("Guardar", () => Guardar(false)),
            Boton("Editar", () => Guardar(true)),
            Boton("Eliminar", Eliminar),
            txtBuscar,
            Boton("Buscar", Cargar),
            Boton("Ver todos", () =>
            {
                txtBuscar.Clear();
                Cargar();
            })
        });

        Controls.Add(tabla);
        Controls.Add(panel);
        tabla.BringToFront();
        tabla.CellClick += Seleccionar;
        Load += (_, _) => Cargar();
    }

    private static Label Etiqueta(string texto) => new()
    {
        Text = texto,
        AutoSize = true,
        Margin = new Padding(3, 7, 3, 0)
    };

    private static Button Boton(string texto, Action accion)
    {
        var boton = new Button { Text = texto, AutoSize = true };
        boton.Click += (_, _) => accion();
        return boton;
    }

    private void Cargar()
    {
        try
        {
            var servicio = new ClientesServicio(cadenaConexion);
            tabla.DataSource = servicio.Buscar(txtBuscar.Text);
            tabla.Columns["IdCliente"]!.Visible = false;
            Limpiar();
        }
        catch (SqlException ex)
        {
            MessageBox.Show(ex.Message, "Error al cargar clientes");
        }
    }

    private void Seleccionar(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 ||
            tabla.Rows[e.RowIndex].DataBoundItem is not DataRowView fila)
        {
            return;
        }

        idSeleccionado = (int)fila["IdCliente"];
        txtNombre.Text = (string)fila["Nombre"];
        txtTelefono.Text = fila["Teléfono"] as string ?? "";
    }

    private void Limpiar()
    {
        idSeleccionado = null;
        txtNombre.Clear();
        txtTelefono.Clear();
        tabla.ClearSelection();
    }
    private void Guardar(bool editar)
    {
        if (editar && idSeleccionado is null)
        {
            MessageBox.Show("Seleccioná la fila del cliente que querés editar.");
            return;
        }

        try
        {
            var servicio = new ClientesServicio(cadenaConexion);
            bool guardado = servicio.Guardar(
                editar ? idSeleccionado : null, txtNombre.Text, txtTelefono.Text);

            if (!guardado)
            {
                MessageBox.Show("El cliente seleccionado ya no está en la base de datos.");
            }

            txtBuscar.Clear();
            Cargar();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message);
            txtNombre.Focus();
        }
        catch (SqlException ex)
        {
            MessageBox.Show(ex.Message, "Error al guardar el cliente");
        }
    }

    private void Eliminar()
    {
        if (idSeleccionado is not int id)
        {
            MessageBox.Show("Seleccioná la fila del cliente que querés eliminar.");
            return;
        }

        var respuesta = MessageBox.Show(
            "¿Querés eliminar el cliente seleccionado?",
            "Confirmar eliminación", MessageBoxButtons.YesNo,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

        if (respuesta != DialogResult.Yes)
        {
            return;
        }

        try
        {
            var servicio = new ClientesServicio(cadenaConexion);

            if (!servicio.Eliminar(id))
            {
                MessageBox.Show("El cliente seleccionado ya no está en la base de datos.");
            }

            Cargar();
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            MessageBox.Show("Este cliente tiene ventas asociadas. Se conserva su registro.");
        }
        catch (SqlException ex)
        {
            MessageBox.Show(ex.Message, "Error al eliminar el cliente");
        }
    }
}
