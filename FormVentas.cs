using System.Data;
using Microsoft.Data.SqlClient;

namespace TiendaRopa;

public sealed class FormVentas : Form
{
    private readonly int idUsuario;
    private readonly ComboBox clientes = new()
    {
        Width = 220,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly ComboBox productos = new()
    {
        Width = 320,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly NumericUpDown cantidad = new()
    {
        Width = 80,
        Minimum = 1,
        Maximum = int.MaxValue,
        Value = 1
    };
    private readonly Label total = new() { AutoSize = true };
    private readonly DataGridView tabla = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    private readonly VentasServicio servicio = new(Form1.CadenaConexion);

    public FormVentas(int idUsuario)
    {
        this.idUsuario = idUsuario;
        Text = "Tienda de Ropa - Ventas";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 460);
        MinimumSize = new Size(840, 360);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 128,
            Padding = new Padding(8)
        };

        panel.Controls.AddRange(new Control[]
          {
            Etiqueta("Cliente"), clientes,
            Etiqueta("Producto"), productos,
            Etiqueta("Cantidad"), cantidad
        });
        panel.SetFlowBreak(cantidad, true);

        panel.Controls.AddRange(new Control[]
        {
            Boton("Agregar", Agregar),
            Boton("Quitar seleccionado", Quitar),
            Boton("Registrar venta", Registrar),
            total
        });

        Controls.Add(tabla);
        Controls.Add(panel);
        tabla.BringToFront();

        MostrarCarrito();
        Shown += (_, _) => CargarCatalogos();
    }

    private static Label Etiqueta(string texto) => new()
    {
        Text = texto,
        AutoSize = true,
        Margin = new Padding(3, 8, 3, 0)
    };

    private static Button Boton(string texto, Action accion)
    {
        var boton = new Button { Text = texto, AutoSize = true, Height = 36 };
        boton.Click += (_, _) => accion();
        return boton;
    }

    private void CargarCatalogos()
    {
        try
        {
            var (datosClientes, datosProductos) = servicio.CargarCatalogos();

            clientes.DisplayMember = "Nombre";
            clientes.ValueMember = "IdCliente";
            clientes.DataSource = datosClientes;

            productos.DisplayMember = "Descripcion";
            productos.ValueMember = "IdProducto";
            productos.DataSource = datosProductos;
        }
        catch (SqlException ex)
        {
            MessageBox.Show(this, ex.Message, "Error al cargar catálogos");
        }
    }

    private void Agregar()
    {
        if (productos.SelectedItem is not DataRowView fila)
        {
            MessageBox.Show(this, "Seleccioná una prenda con existencias.");
            return;
        }

        if (!servicio.Agregar((int)fila["IdProducto"], (string)fila["Descripcion"],
            (int)cantidad.Value, (int)fila["Stock"], (decimal)fila["Precio"]))
        {
            MessageBox.Show(this, "La cantidad supera el stock disponible.");
            return;
        }

        cantidad.Value = 1;
        MostrarCarrito();
    }

    private void Quitar()
    {
        if (tabla.CurrentRow?.DataBoundItem is LineaVenta linea)
        {
            servicio.Quitar(linea);
            MostrarCarrito();
        }
    }

    private void MostrarCarrito()
    {
        tabla.DataSource = servicio.Lineas.ToList();
        tabla.Columns["IdProducto"]!.Visible = false;
        tabla.Columns["PrecioUnitario"]!.HeaderText = "Precio unitario (C$)";
        tabla.Columns["PrecioUnitario"]!.DefaultCellStyle.Format = "N2";
        tabla.Columns["Subtotal"]!.DefaultCellStyle.Format = "N2";
        total.Text = $"Total: C$ {servicio.Total:N2}";
    }

    private void Registrar()
    {
        if (servicio.Lineas.Count == 0)
        {
            MessageBox.Show(this, "Agregá al menos un producto.");
            return;
        }

        if (clientes.SelectedValue is not int idCliente)
        {
            MessageBox.Show(this, "Seleccioná un cliente o Consumidor final.");
            return;
        }

        int idVenta;
        decimal importe;

        try
        {
            (idVenta, importe) = servicio.Registrar(idCliente, idUsuario);
        }
        catch (SqlException ex) when (ex.Number == 50001)
        {
            MessageBox.Show(this, ex.Message, "Venta cancelada");
            servicio.Limpiar();
            MostrarCarrito();
            CargarCatalogos();
            return;
        }
        catch (SqlException ex)
        {
            MessageBox.Show(this, ex.Message, "Error al registrar la venta");
            return;
        }

        servicio.Limpiar();
        MostrarCarrito();
        CargarCatalogos();
        MessageBox.Show(this, $"Venta #{idVenta} registrada por C$ {importe:N2}.");
    }
}