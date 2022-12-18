FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ARGarden.Backend/ARGarden.Backend.csproj", "ARGarden.Backend/"]

RUN dotnet restore "ARGarden.Backend/ARGarden.Backend.csproj"
COPY . .
WORKDIR "/src/ARGarden.Backend"
RUN dotnet build "ARGarden.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ARGarden.Backend.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ARGarden.Backend.dll"]