# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Change this line to use the ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["Services/Service.Background/Service.Background.csproj", "Services/Service.Background/"]
COPY ["Services/Service.Common/Service.Common.csproj", "Services/Service.Common/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["DBContexts/EgmIoT/EgmIoT.csproj", "DBContexts/EgmIoT/"]
COPY ["CommonLibrary/CommonLibrary.csproj", "CommonLibrary/"]
COPY ["CQRS.Core/CQRS.Core.csproj", "CQRS.Core/"]
RUN dotnet restore "./Services/Service.Background/Service.Background.csproj"
COPY . .
WORKDIR "/src/Services/Service.Background"
RUN dotnet build "./Service.Background.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Service.Background.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service.Background.dll"]