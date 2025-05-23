# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects
COPY BackendService.sln .
COPY API/API.csproj ./API/
COPY Application/Application.csproj ./Application/
COPY Domain/Domain.csproj ./Domain/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/

# Restore dependencies
RUN dotnet restore ./API/API.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/API
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
