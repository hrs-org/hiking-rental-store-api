# Use the official .NET runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["HRS.API/HRS.API.csproj", "HRS.API/"]
COPY ["HRS.Domain/HRS.Domain.csproj", "HRS.Domain/"]
COPY ["HRS.Infrastructure/HRS.Infrastructure.csproj", "HRS.Infrastructure/"]
COPY ["HRS.Migrations/HRS.Migrations.csproj", "HRS.Migrations/"]
RUN dotnet restore "HRS.API/HRS.API.csproj"

# Copy the rest of the source code (selective copying for security)
COPY ["HRS.API/", "HRS.API/"]
COPY ["HRS.Domain/", "HRS.Domain/"]
COPY ["HRS.Infrastructure/", "HRS.Infrastructure/"]
COPY ["HRS.Migrations/", "HRS.Migrations/"]
WORKDIR "/src/HRS.API"
RUN dotnet build "HRS.API.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

# Publish the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HRS.API.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

# Final stage - runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "HRS.API.dll"]
