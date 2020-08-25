#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
ENV DB_MANAGER_ADDR=http://dbmanager
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Orchestrus/Orchestrus.csproj", "Orchestrus/"]
COPY ["Worker/WorkerObj.csproj", "Worker/"]
COPY ["DBManager/DBManager.csproj", "DBManager/"]
RUN dotnet restore "Orchestrus/Orchestrus.csproj"
COPY . .
WORKDIR "/src/Orchestrus"
RUN dotnet build "Orchestrus.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orchestrus.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orchestrus.dll"]