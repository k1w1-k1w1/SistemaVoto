# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solución completa
COPY . .

# Restaurar desde la solución
RUN dotnet restore

# Publicar SOLO el proyecto API
RUN dotnet publish SistemaVoto.API/SistemaVoto.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ===== RUNTIME =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SistemaVoto.API.dll"]
