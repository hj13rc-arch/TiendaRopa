using System.Data;
using Microsoft.Data.SqlClient;

namespace TiendaRopa;

public partial class Form1 : Form
{
    internal const string CadenaConexion = ProductosDatos.CadenaConexion;

    private readonly ProductosServicio productosServicio = new();

    private readonly TextBox txtCodigo = new() { MaxLength = 30, Width = 110 };
    private readonly TextBox txtNombre = new() { MaxLength = 100, Width = 160 };
    private readonly TextBox txtTalla = new() { MaxLength = 15, Width = 65 };
    private readonly TextBox txtColor = new() { MaxLength = 30, Width = 100 };
    private readonly NumericUpDown numPrecio = new()
    {
        DecimalPlaces = 2,
        Maximum = 99999999.99m,
        Width = 110
    };
    private readonly NumericUpDown numStock = new()
    {
        Maximum = int.MaxValue,
        Width = 85
    };
    private readonly TextBox txtBuscar = new()
    {
        MaxLength = 100,
        Width = 190,
        PlaceholderText = "Código o nombre"
    };

    private int? idSeleccionado;

    public Form1()
    {
        InitializeComponent();

        Text = "Tienda de Ropa - Productos";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(820, 400);

        var campos = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 64 };
        campos.Controls.AddRange(new Control[]
        {
            Campo("Código", txtCodigo), Campo("Nombre", txtNombre),
            Campo("Talla", txtTalla), Campo("Color", txtColor),
            Campo("Precio (C$)", numPrecio), Campo("Stock", numStock)
        });
       

        var acciones = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42 };
        acciones.Controls.AddRange(new Control[]
        {Boton("Clientes", () =>
{
    using var formulario = new FormClientes(CadenaConexion);
    formulario.ShowDialog(this);
}),
            Boton("Nuevo", LimpiarCampos),
            Boton("Guardar", () => GuardarProducto(false)),
            Boton("Editar", () => GuardarProducto(true)),
            Boton("Eliminar", EliminarProducto),
            txtBuscar,
            Boton("Buscar", CargarProductos),
            Boton("Ver todos", () =>
            {
                txtBuscar.Clear();
                CargarProductos();
            })
        });

        var panel = new Panel { Dock = DockStyle.Top, Height = 116, Padding = new Padding(8) };
        panel.Controls.Add(acciones);
        panel.Controls.Add(campos);
        Controls.Add(panel);

        dgvProductos.Dock = DockStyle.Fill;
        dgvProductos.BringToFront();
        dgvProductos.ReadOnly = true;
        dgvProductos.AllowUserToAddRows = false;
        dgvProductos.AllowUserToDeleteRows = false;
        dgvProductos.MultiSelect = false;
        dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvProductos.CellClick += SeleccionarProducto;

        Load += (_, _) => CargarProductos();
    }

    private static Control Campo(string titulo, Control entrada)
    {
        var campo = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true
        };

        campo.Controls.Add(new Label { Text = titulo, AutoSize = true });
        campo.Controls.Add(entrada);
        return campo;
    }

    private static Button Boton(string texto, Action accion)
    {
        var boton = new Button { Text = texto, AutoSize = true };
        boton.Click += (_, _) => accion();
        return boton;
    }

    private void CargarProductos()
    {
        try
        {
            dgvProductos.DataSource = productosServicio.Buscar(txtBuscar.Text);
            dgvProductos.Columns["IdProducto"]!.Visible = false;
            LimpiarCampos();
        }
        catch (SqlException ex)
        {
            MessageBox.Show(ex.Message, "Error al cargar productos");
        }
    }

    private void SeleccionarProducto(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 ||
            dgvProductos.Rows[e.RowIndex].DataBoundItem is not DataRowView fila)
        {
            return;
        }

        idSeleccionado = (int)fila["IdProducto"];
        txtCodigo.Text = (string)fila["Codigo"];
        txtNombre.Text = (string)fila["Nombre"];
        txtTalla.Text = (string)fila["Talla"];
        txtColor.Text = (string)fila["Color"];
        numPrecio.Value = (decimal)fila["Precio"];
        numStock.Value = (int)fila["Stock"];
    }

    private void LimpiarCampos()
    {
        idSeleccionado = null;
        txtCodigo.Clear();
        txtNombre.Clear();
        txtTalla.Clear();
        txtColor.Clear();
        numPrecio.Value = 0;
        numStock.Value = 0;
        dgvProductos.ClearSelection();
    }

    private void GuardarProducto(bool editar)
    {
        if (editar && idSeleccionado is null)
        {
            MessageBox.Show("Hacé clic en la fila del producto que querés editar.");
            return;
        }

        try
        {
            if (!productosServicio.Guardar(editar ? idSeleccionado : null,
                txtCodigo.Text, txtNombre.Text, txtTalla.Text, txtColor.Text,
                numPrecio.Value, (int)numStock.Value))
            {
                MessageBox.Show("El producto ya no está disponible. Se actualizará la lista.");
            }

            txtBuscar.Clear();
            CargarProductos();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            MessageBox.Show("Ese código ya existe. Usá un código diferente.");
        }
        catch (SqlException ex)
        {
            MessageBox.Show(ex.Message, "Error al guardar el producto");
        }
    }

    private void EliminarProducto()
    {
        if (idSeleccionado is not int id)
        {
            MessageBox.Show("Hacé clic en la fila del producto que querés eliminar.");
            return;
        }

        var respuesta = MessageBox.Show(
            $"¿Querés desactivar el producto {txtCodigo.Text}?",
            "Confirmar eliminación", MessageBoxButtons.YesNo,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

        if (respuesta != DialogResult.Yes)
        {
            return;
        }

        try
        {
            productosServicio.Desactivar(id);
            CargarProductos();
        }
        catch (SqlException ex)
        {
            MessageBox.Show(ex.Message, "Error al eliminar el producto");
        }
    }
}