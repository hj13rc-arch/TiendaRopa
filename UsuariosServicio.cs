using System.Security.Cryptography;

namespace TiendaRopa;

internal sealed class UsuariosServicio(string cadenaConexion)
{
    private const int Iteraciones = 600_000;
    private readonly UsuariosDatos datos = new(cadenaConexion);

    internal bool HayUsuarios() => datos.HayUsuarios();

    internal (int Id, string Rol)? Autenticar(string usuario, string clave)
    {
        var cuenta = datos.Buscar(usuario.Trim());
        return cuenta is { } registro && VerificarClave(clave, registro.Hash)
            ? (registro.Id, registro.Rol)
            : null;
    }

    internal bool CrearAdministrador(string usuario, string clave) =>
        datos.CrearAdministrador(usuario.Trim(), CrearHash(clave));

    internal void CrearCajero(string usuario, string clave) =>
        datos.CrearCajero(usuario.Trim(), CrearHash(clave));

    private static string CrearHash(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave) || clave.Length < 12)
        {
            throw new ArgumentException("Usá una contraseña de al menos 12 caracteres.");
        }

        byte[] sal = RandomNumberGenerator.GetBytes(16);
        byte[] hash = DerivarClave(clave, sal);
        return $"{Convert.ToBase64String(sal)}:{Convert.ToBase64String(hash)}";
    }

    private static byte[] DerivarClave(string clave, byte[] sal) =>
        Rfc2898DeriveBytes.Pbkdf2(clave, sal, Iteraciones, HashAlgorithmName.SHA256, 32);

    private static bool VerificarClave(string clave, string claveHash)
    {
        string[] partes = claveHash.Split(':');
        if (partes.Length != 2)
        {
            return false;
        }

        try
        {
            byte[] sal = Convert.FromBase64String(partes[0]);
            byte[] hashGuardado = Convert.FromBase64String(partes[1]);
            return CryptographicOperations.FixedTimeEquals(
                DerivarClave(clave, sal), hashGuardado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
