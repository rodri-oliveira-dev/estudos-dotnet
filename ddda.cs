using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Xunit;

public class EndpointMetadataTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EndpointMetadataTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Should_Have_Swagger_Metadata_On_Endpoints()
    {
        // Arrange
        var app =
        // Arrange
        var app = _factory.Services;

        var endpointDataSource = app.GetRequiredService<EndpointDataSource>();
        var endpoints = endpointDataSource.Endpoints;

        // Act & Assert
        Assert.NotEmpty(endpoints);

        foreach (var endpoint in endpoints)
        {
            var routeEndpoint = endpoint as RouteEndpoint;
            if (routeEndpoint == null) continue;

            var path = routeEndpoint.RoutePattern.RawText;

            // Aqui você pode verificar metadados específicos
            var swaggerMetadata = endpoint.Metadata
                .OfType<SwaggerOperationAttribute>()
                .FirstOrDefault();

            if (path == "/hello")
            {
                Assert.NotNull(swaggerMetadata);
                Assert.Equal("Say hello", swaggerMetadata.Summary);
                Assert.Equal("Returns a greeting", swaggerMetadata.Description);
            }
        }
    }
}