FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/AirAware.Shared/*.csproj src/AirAware.Shared/
COPY src/AirAware.API/*.csproj src/AirAware.API/
RUN dotnet restore src/AirAware.API/AirAware.API.csproj

COPY src/AirAware.Shared/ src/AirAware.Shared/
COPY src/AirAware.API/ src/AirAware.API/
RUN dotnet publish src/AirAware.API/AirAware.API.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "AirAware.API.dll"]