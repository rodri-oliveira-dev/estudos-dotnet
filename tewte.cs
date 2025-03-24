using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Text.RegularExpressions;

public class CommandLogger : DbCommandInterceptor
{
    private static readonly Regex TableRegex = new Regex(
        @"(FROM|INTO|UPDATE|JOIN)\s+(\[?\w+\]?\.?\[?\w+\]?)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private void LogCommand(string commandText)
    {
        string operation = "UNKNOWN";

        if (commandText.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            operation = "SELECT";
        else if (commandText.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
            operation = "INSERT";
        else if (commandText.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
            operation = "UPDATE";
        else if (commandText.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
            operation = "DELETE";

        var matches = TableRegex.Matches(commandText);
        var tables = matches.Select(m => m.Groups[2].Value).Distinct().ToList();

        Console.WriteLine($"Operação: {operation}");
        Console.WriteLine($"Tabelas: {string.Join(", ", tables)}");
        Console.WriteLine("SQL:");
        Console.WriteLine(commandText);
        Console.WriteLine("------");
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        LogCommand(command.CommandText);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData,
        InterceptionResult<int> result)
    {
        LogCommand(command.CommandText);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command, CommandEventData eventData,
        InterceptionResult<object> result)
    {
        LogCommand(command.CommandText);
        return base.ScalarExecuting(command, eventData, result);
    }
}