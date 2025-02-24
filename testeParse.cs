using System;
using System.Text.Json;
using System.Text.Json.Nodes;

public class JsonExtractor
{
    /// <summary>
    /// Valida se a string é um JSON válido e verifica se o caminho especificado contém o valor esperado.
    /// </summary>
    /// <param name="jsonString">A string JSON para validação e extração.</param>
    /// <param name="jsonPathValue">O caminho e valor esperado no formato "Chave: Valor", com níveis dinâmicos. Ex: "glossary:GlossDiv:GlossList:GlossEntry:GlossDef:para: A meta-markup language, used to create markup languages such as DocBook."</param>
    /// <returns>Confirmação do valor encontrado ou mensagem de erro.</returns>
    public static T ExtractAndValidateValue<T>(string jsonString, string jsonPathValue)
    {
        // Validação do JSON
        JsonNode jsonNode;
        try
        {
            jsonNode = JsonNode.Parse(jsonString);
        }
        catch (JsonException)
        {
            throw new ArgumentException("JSON inválido.");
        }

        // Separando o caminho e o valor esperado
        var splitPathValue = jsonPathValue.Split(':', StringSplitOptions.TrimEntries);
        if (splitPathValue.Length < 1)
        {
            throw new ArgumentException("Formato inválido. Use 'Chave1:Chave2:...'.");
        }

        var jsonPath = splitPathValue; // Todo o caminho

        // Extração do valor pelo caminho dinâmico
        JsonNode currentNode = jsonNode;
        foreach (var part in jsonPath)
        {
            if (part.Contains('[') && part.Contains(']'))
            {
                // Tratamento para Arrays no formato GlossSeeAlso[0]
                var arrayPart = part.Split('[', ']', StringSplitOptions.RemoveEmptyEntries);
                var key = arrayPart[0];
                var index = int.Parse(arrayPart[1]);

                if (currentNode is JsonObject obj && obj[key] is JsonArray arr && arr.Count > index)
                {
                    currentNode = arr[index];
                }
                else
                {
                    throw new ArgumentException($"Caminho '{string.Join(":", jsonPath)}' não encontrado.");
                }
            }
            else
            {
                if (currentNode is JsonObject obj && obj[part] != null)
                {
                    currentNode = obj[part];
                }
                else
                {
                    throw new ArgumentException($"Caminho '{string.Join(":", jsonPath)}' não encontrado.");
                }
            }
        }

        // Tenta fazer o cast do valor para o tipo T
        try
        {
            if (currentNode is JsonArray jsonArray)
            {
                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listType = typeof(T).GetGenericArguments()[0];
                    var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));

                    foreach (var item in jsonArray)
                    {
                        var deserializedItem = item.Deserialize(listType);
                        list.Add(deserializedItem);
                    }

                    return (T)list;
                }
                else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var keyType = typeof(T).GetGenericArguments()[0];
                    var valueType = typeof(T).GetGenericArguments()[1];
                    var dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));

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
                    throw new InvalidCastException($"Não foi possível converter o array JSON para o tipo '{typeof(T)}'.");
                }
            }
            else
            {
                return currentNode.Deserialize<T>()!;
            }
        }
        catch (Exception)
        {
            throw new InvalidCastException($"Não foi possível converter o valor para o tipo '{typeof(T)}'.");
        }
    }
    {
        // Validação do JSON
        JsonNode jsonNode;
        try
        {
            jsonNode = JsonNode.Parse(jsonString);
        }
        catch (JsonException)
        {
            return "JSON inválido.";
        }

        // Separando o caminho e o valor esperado
        var splitPathValue = jsonPathValue.Split(':', StringSplitOptions.TrimEntries);
        if (splitPathValue.Length < 2)
        {
            return "Formato inválido. Use 'Chave1:Chave2:...:Valor'.";
        }

        var expectedValue = splitPathValue[^1]; // Último item é o valor esperado
        var jsonPath = splitPathValue[..^1]; // Todos os anteriores são o caminho

        // Extração do valor pelo caminho dinâmico
        JsonNode currentNode = jsonNode;
        foreach (var part in jsonPath)
        {
            if (part.Contains('[') && part.Contains(']'))
            {
                // Tratamento para Arrays no formato GlossSeeAlso[0]
                var arrayPart = part.Split('[', ']', StringSplitOptions.RemoveEmptyEntries);
                var key = arrayPart[0];
                var index = int.Parse(arrayPart[1]);

                if (currentNode is JsonObject obj && obj[key] is JsonArray arr && arr.Count > index)
                {
                    currentNode = arr[index];
                }
                else
                {
                    return $"Caminho '{string.Join(":", jsonPath)}' não encontrado.";
                }
            }
            else
            {
                if (currentNode is JsonObject obj && obj[part] != null)
                {
                    currentNode = obj[part];
                }
                else
                {
                    return $"Caminho '{string.Join(":", jsonPath)}' não encontrado.";
                }
            }
        }

        // Validação do valor
        if (currentNode?.ToString() == expectedValue)
        {
            return $"Valor encontrado: {currentNode}";
        }
        else
        {
            return $"Valor esperado '{expectedValue}' não encontrado em '{string.Join(":", jsonPath)}'. Valor atual: '{currentNode}'";
        }
    }
}

class Program
{
    static void Main()
    {
        string jsonString = @"{
            ""glossary"": {
                ""title"": ""example glossary"",
                ""GlossDiv"": {
                    ""title"": ""S"",
                    ""GlossList"": {
                        ""GlossEntry"": {
                            ""ID"": ""SGML"",
                            ""SortAs"": ""SGML"",
                            ""GlossTerm"": ""Standard Generalized Markup Language"",
                            ""Acronym"": ""SGML"",
                            ""Abbrev"": ""ISO 8879:1986"",
                            ""GlossDef"": {
                                ""para"": ""A meta-markup language, used to create markup languages such as DocBook."",
                                ""GlossSeeAlso"": [
                                    ""GML"",
                                    ""XML""
                                ]
                            },
                            ""GlossSee"": ""markup""
                        }
                    }
                }
            }
        }";

        // Testando diferentes caminhos e valores
        string jsonPathValue = "glossary.title: example glossary";
        var result = JsonExtractor.ExtractAndValidateValue<string>(jsonString, jsonPathValue);
        Console.WriteLine($"Resultado: {result}");

        jsonPathValue = "glossary.GlossDiv.GlossList.GlossEntry.GlossSee: markup";
        result = JsonExtractor.ExtractAndValidateValue(jsonString, jsonPathValue);
        Console.WriteLine($"Resultado: {result}");

        jsonPathValue = "glossary.GlossDiv.GlossList.GlossEntry.GlossDef.GlossSeeAlso[1]: XML";
        result = JsonExtractor.ExtractAndValidateValue(jsonString, jsonPathValue);
        Console.WriteLine($"Resultado: {result}");

        jsonPathValue = "glossary.GlossDiv.GlossList.GlossEntry.ID: HTML";
        result = JsonExtractor.ExtractAndValidateValue(jsonString, jsonPathValue);
        Console.WriteLine($"Resultado: {result}");
    }
}
