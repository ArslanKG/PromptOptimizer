# Multi-stage build for production optimization
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000

# Create data directory for SQLite database persistence
RUN mkdir -p /app/data && chmod 755 /app/data

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for dependency resolution
COPY ["PromptOptimizer.API/PromptOptimizer.API.csproj", "PromptOptimizer.API/"]
COPY ["PromptOptimizer.Application/PromptOptimizer.Application.csproj", "PromptOptimizer.Application/"]
COPY ["PromptOptimizer.Core/PromptOptimizer.Core.csproj", "PromptOptimizer.Core/"]
COPY ["PromptOptimizer.Infrastructure/PromptOptimizer.Infrastructure.csproj", "PromptOptimizer.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "PromptOptimizer.API/PromptOptimizer.API.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/PromptOptimizer.API"
RUN dotnet build "PromptOptimizer.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "PromptOptimizer.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables for Render.com
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://*:10000

# Create non-root user for security
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser && \
    chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

ENTRYPOINT ["dotnet", "PromptOptimizer.API.dll"]