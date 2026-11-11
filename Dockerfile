# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["src/CrudApp/CrudApp.csproj", "src/CrudApp/"]
RUN dotnet restore "src/CrudApp/CrudApp.csproj"

# Copy the remaining source code
COPY . .
WORKDIR "/src/src/CrudApp"

# Publish the application
RUN dotnet publish "CrudApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Configure Kestrel
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Run as a non-root user
USER $APP_UID

ENTRYPOINT ["dotnet", "CrudApp.dll"]