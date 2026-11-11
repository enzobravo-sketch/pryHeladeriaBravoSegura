using System.Text.Json;
using pryHeladeriaBravoSegura.Models;

namespace pryHeladeriaBravoSegura.Data;

public sealed class RepositorioJson
{
    private readonly object bloqueo = new();

    public ConfiguracionPersistencia Configuracion { get; }
    public DatosApp Datos { get; private set; }

    public RepositorioJson(ConfiguracionPersistencia? configuracion = null)
    {
        Configuracion = configuracion ?? new ConfiguracionPersistencia();
        Datos = CargarDatos();
    }

    public void Guardar()
    {
        lock (bloqueo)
        {
            GuardarDatos(Datos);
        }
    }

    public void EjecutarTransaccion(Action<DatosApp> operacion)
    {
        ArgumentNullException.ThrowIfNull(operacion);

        lock (bloqueo)
        {
            DatosApp respaldoEnMemoria = Clonar(Datos);

            try
            {
                operacion(Datos);
                GuardarDatos(Datos);
            }
            catch
            {
                Datos = respaldoEnMemoria;
                throw;
            }
        }
    }

    public int ObtenerProximoId<T>(IEnumerable<T> elementos, Func<T, int> selectorId)
    {
        ArgumentNullException.ThrowIfNull(elementos);
        ArgumentNullException.ThrowIfNull(selectorId);

        return elementos.Select(selectorId).DefaultIfEmpty(0).Max() + 1;
    }

    private DatosApp CargarDatos()
    {
        Directory.CreateDirectory(Configuracion.CarpetaDatos);

        if (!File.Exists(Configuracion.RutaDatos) && !File.Exists(Configuracion.RutaRespaldo))
        {
            DatosApp datosSemilla = SemillaDatos.Crear();
            GuardarDatos(datosSemilla);
            return datosSemilla;
        }

        if (File.Exists(Configuracion.RutaDatos))
        {
            try
            {
                return Normalizar(LeerArchivo(Configuracion.RutaDatos));
            }
            catch (Exception excepcionPrincipal) when (EsErrorDeLectura(excepcionPrincipal))
            {
                if (File.Exists(Configuracion.RutaRespaldo))
                {
                    try
                    {
                        DatosApp datosRespaldo = Normalizar(LeerArchivo(Configuracion.RutaRespaldo));
                        File.Copy(Configuracion.RutaRespaldo, Configuracion.RutaDatos, true);
                        return datosRespaldo;
                    }
                    catch (Exception excepcionRespaldo) when (EsErrorDeLectura(excepcionRespaldo))
                    {
                        throw new ErrorPersistenciaException(
                            "No se pudo leer el archivo de datos ni su respaldo. Verifique los permisos de la carpeta de la aplicación.",
                            excepcionRespaldo);
                    }
                }

                throw new ErrorPersistenciaException(
                    "El archivo de datos está dañado y no existe un respaldo válido para restaurarlo.",
                    excepcionPrincipal);
            }
        }

        try
        {
            DatosApp datosRespaldo = Normalizar(LeerArchivo(Configuracion.RutaRespaldo));
            File.Copy(Configuracion.RutaRespaldo, Configuracion.RutaDatos, true);
            return datosRespaldo;
        }
        catch (Exception excepcion) when (EsErrorDeLectura(excepcion))
        {
            throw new ErrorPersistenciaException(
                "No se pudo restaurar el archivo de datos desde el respaldo.",
                excepcion);
        }
    }

    private DatosApp LeerArchivo(string ruta)
    {
        string contenido = File.ReadAllText(ruta);
        DatosApp? datos = JsonSerializer.Deserialize<DatosApp>(contenido, Configuracion.OpcionesJson);

        return datos ?? throw new JsonException("El archivo no contiene datos válidos.");
    }

    private void GuardarDatos(DatosApp datos)
    {
        Directory.CreateDirectory(Configuracion.CarpetaDatos);
        string rutaTemporal = Path.Combine(
            Configuracion.CarpetaDatos,
            $"datos-{Guid.NewGuid():N}.tmp");

        try
        {
            string contenido = JsonSerializer.Serialize(datos, Configuracion.OpcionesJson);
            File.WriteAllText(rutaTemporal, contenido);

            if (File.Exists(Configuracion.RutaDatos))
            {
                File.Copy(Configuracion.RutaDatos, Configuracion.RutaRespaldo, true);
            }

            File.Move(rutaTemporal, Configuracion.RutaDatos, true);
        }
        catch (Exception excepcion)
        {
            throw new ErrorPersistenciaException(
                "No se pudieron guardar los datos. Los cambios no fueron confirmados.",
                excepcion);
        }
        finally
        {
            if (File.Exists(rutaTemporal))
            {
                File.Delete(rutaTemporal);
            }
        }
    }

    private static DatosApp Clonar(DatosApp datos)
    {
        string contenido = JsonSerializer.Serialize(datos);
        return JsonSerializer.Deserialize<DatosApp>(contenido)
            ?? throw new InvalidOperationException("No se pudo crear una copia de los datos en memoria.");
    }

    private static DatosApp Normalizar(DatosApp datos)
    {
        datos.Tipos ??= [];
        datos.Productos ??= [];
        datos.Stocks ??= [];
        datos.Remitos ??= [];
        datos.Salidas ??= [];
        datos.Usuarios ??= [];
        datos.Pedidos ??= [];
        datos.DetallesPedido ??= [];
        return datos;
    }

    private static bool EsErrorDeLectura(Exception excepcion)
    {
        return excepcion is IOException
            or UnauthorizedAccessException
            or JsonException
            or NotSupportedException;
    }
}
