public class JsonExtractor
{
    /// <summary>
    /// Extrai o valor de um JSON baseado em um caminho dinâmico.
    /// O caminho deve ser informado no formato "Chave1:Chave2:[índice]" sem valor esperado.
    /// </summary>
    /// <typeparam name="T">Tipo para o qual o valor extraído será convertido.</typeparam>
    /// <param name="jsonString">A string JSON para extração.</param>
    /// <param name="jsonPath">O caminho no JSON, delimitado por ":", podendo incluir índices para arrays, ex: "glossary:GlossDiv:GlossList:GlossEntry:GlossDef:para".</param>
    /// <returns>O valor extraído convertido para o tipo T.</returns>
    /// <exception cref="ArgumentException">Caso o JSON seja inválido ou o caminho não seja encontrado.</exception>
    /// <exception cref="InvalidCastException">Caso não seja possível converter o valor extraído para o tipo T.</exception>
    public static T ExtractValue<T>(string jsonString, string jsonPath)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            throw new ArgumentException("jsonString não pode ser nulo ou vazio.");
        if (string.IsNullOrWhiteSpace(jsonPath))
            throw new ArgumentException("jsonPath não pode ser nulo ou vazio.");

        JsonNode jsonNode;
        try
        {
            jsonNode = JsonNode.Parse(jsonString);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("JSON inválido.", ex);
        }

        string[] pathParts = jsonPath.Split(':', StringSplitOptions.TrimEntries);
        if (pathParts.Length == 0)
            throw new ArgumentException("Formato de caminho inválido. Use 'Chave1:Chave2:...'.");
        
        JsonNode targetNode = NavigateJsonPath(jsonNode, pathParts);

        try
        {
            return DeserializeNode<T>(targetNode);
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Falha ao converter o valor para o tipo '{typeof(T)}'.", ex);
        }
    }

    /// <summary>
    /// Valida se o valor presente em um JSON corresponde ao valor esperado.
    /// O parâmetro jsonPathWithExpected deve estar no formato "Chave1:Chave2:...:ValorEsperado".
    /// </summary>
    /// <param name="jsonString">A string JSON para validação.</param>
    /// <param name="jsonPathWithExpected">Caminho e valor esperado, onde o último token é o valor esperado.</param>
    /// <returns>Uma mensagem informando se o valor esperado foi encontrado ou não.</returns>
    public static string ValidateValue(string jsonString, string jsonPathWithExpected)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return "JSON inválido ou vazio.";
        if (string.IsNullOrWhiteSpace(jsonPathWithExpected))
            return "Caminho e valor esperado não podem ser vazios.";

        JsonNode jsonNode;
        try
        {
            jsonNode = JsonNode.Parse(jsonString);
        }
        catch (JsonException)
        {
            return "JSON inválido.";
        }

        string[] parts = jsonPathWithExpected.Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
            return "Formato inválido. Use 'Chave1:Chave2:...:ValorEsperado'.";

        string expectedValue = parts[^1];
        string[] pathParts = parts.Take(parts.Length - 1).ToArray();

        JsonNode targetNode;
        try
        {
            targetNode = NavigateJsonPath(jsonNode, pathParts);
        }
        catch (ArgumentException ex)
        {
            return ex.Message;
        }

        if (targetNode == null)
            return "Valor não encontrado.";

        string actualValue = targetNode.ToString();
        if (actualValue == expectedValue)
            return $"Valor encontrado: {actualValue}";
        else
            return $"Valor esperado '{expectedValue}' não encontrado em '{string.Join(":", pathParts)}'. Valor atual: '{actualValue}'";
    }

    /// <summary>
    /// Navega pelo JSON utilizando o caminho especificado.
    /// Suporta acesso a arrays com a notação "Chave[índice]".
    /// </summary>
    /// <param name="currentNode">O nó atual do JSON.</param>
    /// <param name="pathParts">Array de chaves e índices que definem o caminho.</param>
    /// <returns>O nó encontrado após navegar pelo caminho.</returns>
    /// <exception cref="ArgumentException">Caso alguma parte do caminho não seja encontrada ou esteja em formato inválido.</exception>
    private static JsonNode NavigateJsonPath(JsonNode currentNode, string[] pathParts)
    {
        foreach (var part in pathParts)
        {
            if (part.Contains('[') && part.Contains(']'))
            {
                var arrayParts = part.Split('[', ']', StringSplitOptions.RemoveEmptyEntries);
                if (arrayParts.Length != 2)
                    throw new ArgumentException($"Formato inválido para array na parte: '{part}'.");

                var key = arrayParts[0];
                if (!int.TryParse(arrayParts[1], out int index))
                    throw new ArgumentException($"Índice inválido na parte: '{part}'.");

                if (index < 0)
                    throw new ArgumentException($"Índice negativo na parte: '{part}'.");

                if (currentNode is JsonObject obj && obj.TryGetPropertyValue(key, out JsonNode? arrNode) && arrNode is JsonArray arr)
                {
                    if (index >= arr.Count)
                        throw new ArgumentException($"Índice {index} fora do intervalo para o array '{key}'.");
                    currentNode = arr[index];
                }
                else
                {
                    throw new ArgumentException($"Caminho '{key}' não encontrado ou não é um array.");
                }
            }
            else
            {
                if (currentNode is JsonObject obj && obj.TryGetPropertyValue(part, out JsonNode? nextNode) && nextNode != null)
                {
                    currentNode = nextNode;
                }
                else
                {
                    throw new ArgumentException($"Caminho '{part}' não encontrado.");
                }
            }
        }
        return currentNode;
    }

    /// <summary>
    /// Converte o JsonNode para o tipo especificado.
    /// Suporta conversão para List<> e Dictionary&lt;,&gt; quando o nó é um array.
    /// </summary>
    /// <typeparam name="T">Tipo para o qual o nó será convertido.</typeparam>
    /// <param name="node">O nó do JSON a ser convertido.</param>
    /// <returns>O valor convertido para o tipo T.</returns>
    /// <exception cref="InvalidCastException">Caso a conversão não seja suportada ou falhe.</exception>
    private static T DeserializeNode<T>(JsonNode node)
    {
        if (node is JsonArray jsonArray)
        {
            Type targetType = typeof(T);
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = targetType.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
                foreach (var item in jsonArray)
                {
                    var deserializedItem = item.Deserialize(elementType);
                    list.Add(deserializedItem);
                }
                return (T)list;
            }
            else if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = targetType.GetGenericArguments()[0];
                var valueType = targetType.GetGenericArguments()[1];
                var dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType))!;
                foreach (var item in jsonArray)
                {
                    if (item is JsonObject jsonObject)
                    {
                        foreach (var kvp in jsonObject)
                        {
                            var key = Convert.ChangeType(kvp.Key, keyType);
                            var value = kvp.Value.Deserialize(valueType);
                            dict.Add(key, value);
                        }
                    }
                }
                return (T)dict;
            }
            else
            {
                throw new InvalidCastException($"Conversão de array JSON para o tipo '{typeof(T)}' não é suportada.");
            }
        }
        else
        {
            return node.Deserialize<T>()!;
        }
    }
}
