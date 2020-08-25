#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["WorkerApp/WorkerApp.csproj", "WorkerApp/"]
RUN dotnet restore "WorkerApp/WorkerApp.csproj"
COPY . .
WORKDIR "/src/WorkerApp"
RUN dotnet build "WorkerApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WorkerApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkerApp.dll"]