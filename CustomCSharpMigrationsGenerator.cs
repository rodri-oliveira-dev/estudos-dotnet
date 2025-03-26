using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

public class CustomCSharpMigrationsGenerator : CSharpMigrationsGenerator
{
    public CustomCSharpMigrationsGenerator(
        MigrationsCodeGeneratorDependencies dependencies,
        CSharpMigrationsGeneratorDependencies csharpDependencies)
        : base(dependencies, csharpDependencies)
    {
    }

    protected override void GenerateMigrationClass(
        string migrationNamespace,
        string migrationName,
        IReadOnlyList<MigrationOperation> upOperations,
        IReadOnlyList<MigrationOperation> downOperations,
        string activeProvider,
        IndentedStringBuilder builder)
    {
        builder.AppendLine("using System.Diagnostics.CodeAnalysis;");

        base.GenerateMigrationClass(
            migrationNamespace,
            migrationName,
            upOperations,
            downOperations,
            activeProvider,
            builder
        );
    }

    protected override void GenerateClassAttributes(
        string migrationNamespace,
        string migrationName,
        IndentedStringBuilder builder)
    {
        builder.AppendLine("[ExcludeFromCodeCoverage]");
        base.GenerateClassAttributes(migrationNamespace, migrationName, builder);
    }
}

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;

public class YourDesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
    {
        services.AddSingleton<IMigrationsCodeGenerator, CustomCSharpMigrationsGenerator>();
    }
}

