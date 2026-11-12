using System.Security.Cryptography;

namespace pryHeladeriaBravoSegura.Models;

public static class SemillaDatos
{
    public static DatosApp Crear()
    {
        const int idTipoHelados = 1;
        const int idTipoInsumos = 2;
        const int stockMinimo = 5;

        var datos = new DatosApp
        {
            Tipos =
            [
                new Tipo { IdTipo = idTipoHelados, Nombre = "Helados" },
                new Tipo { IdTipo = idTipoInsumos, Nombre = "Insumos" }
            ],
            Usuarios =
            [
                new Usuario
                {
                    IdUsuario = 1,
                    Nombre = "admin",
                    Contrasena = CrearHash("Admin123"),
                    EsAdministrador = true
                },
                new Usuario
                {
                    IdUsuario = 2,
                    Nombre = "Juan",
                    Contrasena = CrearHash("Juan123")
                }
            ]
        };

        var productos = new (int Id, string Descripcion, int IdTipo, int Cantidad)[]
        {
            (1, "Dulce de Leche Granizado", idTipoHelados, 12),
            (2, "Frutilla a la Crema", idTipoHelados, 2),
            (3, "Vasitos 1/4 kg", idTipoInsumos, 0),
            (4, "Cucuruchos Especiales", idTipoInsumos, 45),
            (5, "Tramontana", idTipoHelados, 8),
            (6, "Chocotorta", idTipoHelados, 10),
            (7, "Dulce de Leche c/ Brownie", idTipoHelados, 3)
        };

        foreach (var producto in productos)
        {
            datos.Productos.Add(new Producto
            {
                IdProducto = producto.Id,
                Descripcion = producto.Descripcion,
                StockMinimo = stockMinimo,
                IdTipo = producto.IdTipo
            });

            datos.Stocks.Add(new Stock
            {
                IdStock = producto.Id,
                IdProducto = producto.Id,
                Cantidad = producto.Cantidad
            });
        }

        return datos;
    }

    public static string CrearHash(string contrasena)
    {
        byte[] sal = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            contrasena,
            sal,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        return $"{Convert.ToBase64String(sal)}:{Convert.ToBase64String(hash)}";
    }
}
