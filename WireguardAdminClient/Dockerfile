#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

COPY *.csproj ./
RUN dotnet restore WireguardAdminClient.csproj

COPY . ./
RUN dotnet publish WireguardAdminClient.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0

WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "WireguardAdminClient.dll", "--urls=http://0.0.0.0:5003"]