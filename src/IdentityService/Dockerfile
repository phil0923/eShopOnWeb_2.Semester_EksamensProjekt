# ========================================
# Stage 1: Build the application
# ========================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["IdentityService.csproj", "."]
RUN dotnet restore "IdentityService.csproj"

# Copy the remaining source
COPY . .


# Build the project
RUN dotnet build "IdentityService.csproj" -c Release -o /app/build

# ========================================
# Stage 2: Publish the application
# ========================================
FROM build AS publish
RUN dotnet publish "IdentityService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ========================================
# Stage 3: Runtime image
# ========================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "IdentityService.dll"]
