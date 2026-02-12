# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos solo el proyecto de la API
COPY ["minimalAPI.csproj", "./"]
RUN dotnet restore "minimalAPI.csproj"

# Copiamos todo el contenido de la carpeta de la API
COPY . .

# Publicamos la aplicación
RUN dotnet publish "minimalAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final (Producción)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .


# Variables de entorno  
ENV DB_CONNECTION=""
ENV AZURE_STORAGE_CONNECTION=""
ENV KEYCLOAK_AUTHORITY=""
ENV KEYCLOAK_ISSUER=""

ENTRYPOINT ["dotnet", "minimalAPI.dll"]