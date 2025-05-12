using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

public static class IdempotencyHashGenerator
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _cache =
        new ConcurrentDictionary<Type, PropertyInfo[]>();

    public static string GenerateHash<T>(T obj) where T : class, new()
    {
        var type = typeof(T);
        var props = GetIdempotencyKeyProperties(type);

        var values = props.ToDictionary(p => p.Name, p => p.GetValue(obj));

        var json = JsonSerializer.Serialize(values, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hashBytes);
    }

    private static PropertyInfo[] GetIdempotencyKeyProperties(Type type)
    {
        var properties = _cache.GetOrAdd(type, t =>
            t.GetProperties()
             .Where(p => Attribute.IsDefined(p, typeof(IdempotencyKeyPartAttribute)))
             .OrderBy(p => p.Name)
             .ToArray()
        );

        if (properties.Length == 0)
            throw new InvalidOperationException(
                $"A classe '{type.Name}' não possui nenhuma propriedade marcada com [IdempotencyKeyPart].");

        return properties;
    }
}

/// <summary>
/// Gera uma hash SHA-256 em hexadecimal a partir dos valores das propriedades
/// de um objeto que estão marcadas com o atributo <see cref="IdempotencyKeyPartAttribute"/>.
/// </summary>
/// <typeparam name="T">Tipo da classe que contém propriedades marcadas para compor a hash.</typeparam>
/// <param name="obj">Instância do objeto a ser processado.</param>
/// <returns>
/// Uma string hexadecimal com 64 caracteres representando a hash dos valores relevantes
/// para idempotência.
/// </returns>
/// <exception cref="InvalidOperationException">
/// Lançada se o tipo <typeparamref name="T"/> não tiver nenhuma propriedade marcada com
/// <see cref="IdempotencyKeyPartAttribute"/>.
/// </exception>
/// <remarks>
/// As propriedades são ordenadas por nome antes da serialização para garantir consistência
/// na geração da hash. Campos mutáveis ou irrelevantes devem ser omitidos do cálculo.
/// </remarks>
public static string GenerateHash<T>(T obj) where T : class, new()
{
    var type = typeof(T);
    var props = GetIdempotencyKeyProperties(type);

    var values = props.ToDictionary(p => p.Name, p => p.GetValue(obj));

    var json = JsonSerializer.Serialize(values, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    });

    using var sha = SHA256.Create();
    var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
    return Convert.ToHexString(hashBytes);
}

