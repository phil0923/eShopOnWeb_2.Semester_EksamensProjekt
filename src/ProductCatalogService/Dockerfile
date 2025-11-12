# 1) Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src/ProductCatalogService

# Copy only csproj files first (better layer caching)
COPY ProductCatalog.Application/ProductCatalog.Application.csproj ProductCatalog.Application/
COPY ProductCatalog.Infrastructure/ProductCatalog.Infrastructure.csproj ProductCatalog.Infrastructure/
COPY ProductCatalog.API/ProductCatalog.API.csproj ProductCatalog.API/

# Restore API (pulls transitive project refs too)
RUN dotnet restore ProductCatalog.API/ProductCatalog.API.csproj

# Copy the rest of the source
COPY src/ProductCatalogService/ src/ProductCatalogService/

# Publish (no apphost => smaller image)
RUN dotnet publish src/ProductCatalogService/ProductCatalog.API/ProductCatalog.API.csproj \
    -c Release -o /app /p:UseAppHost=false

# 2) Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app ./

# ASP.NET binding + port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Recommended: pass connection string via env var at run/deploy time
# Example (DON'T hardcode in image):
# ENV ConnectionStrings__CatalogConnection="Server=sqlserver,1433;Database=CatalogDb_Split;User ID=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"

ENTRYPOINT ["dotnet", "ProductCatalog.API.dll"]
