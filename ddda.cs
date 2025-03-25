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