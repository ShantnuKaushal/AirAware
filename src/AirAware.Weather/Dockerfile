FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/AirAware.Shared/*.csproj src/AirAware.Shared/
COPY src/AirAware.Weather/*.csproj src/AirAware.Weather/
RUN dotnet restore src/AirAware.Weather/AirAware.Weather.csproj

COPY src/AirAware.Shared/ src/AirAware.Shared/
COPY src/AirAware.Weather/ src/AirAware.Weather/
RUN dotnet publish src/AirAware.Weather/AirAware.Weather.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "AirAware.Weather.dll"]