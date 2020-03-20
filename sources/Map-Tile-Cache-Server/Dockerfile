FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Map-Tile-Cache-Server/Map-Tile-Cache-Server.csproj", "Map-Tile-Cache-Server/"]
RUN dotnet restore "Map-Tile-Cache-Server/Map-Tile-Cache-Server.csproj"
COPY . .
WORKDIR "/src/Map-Tile-Cache-Server"
RUN dotnet build "Map-Tile-Cache-Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Map-Tile-Cache-Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Map-Tile-Cache-Server.dll"]