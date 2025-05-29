# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/PersonManagement.API/PersonManagement.API.csproj", "src/PersonManagement.API/"]
COPY ["src/PersonManagement.Application/PersonManagement.Application.csproj", "src/PersonManagement.Application/"]
COPY ["src/PersonManagement.Domain/PersonManagement.Domain.csproj", "src/PersonManagement.Domain/"]
COPY ["src/PersonManagement.Infrastructure/PersonManagement.Infrastructure.csproj", "src/PersonManagement.Infrastructure/"]
COPY ["src/PersonManagement.Logging/PersonManagement.Logging.csproj", "src/PersonManagement.Logging/"]

RUN dotnet restore "src/PersonManagement.API/PersonManagement.API.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/PersonManagement.API"
RUN dotnet build "PersonManagement.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PersonManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p logs

EXPOSE 8080
ENTRYPOINT ["dotnet", "PersonManagement.API.dll"]