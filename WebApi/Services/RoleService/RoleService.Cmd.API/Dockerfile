# 請參閱 https://aka.ms/customizecontainer 了解如何自訂您的偵錯容器，以及 Visual Studio 如何使用此 Dockerfile 來組建您的映像，以加快偵錯速度。

# 此階段用於以快速模式從 VS 執行時 (偵錯設定的預設值)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080


# 此階段是用來組建服務專案
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["Services/RoleService/RoleService.Cmd.API/UserRoleService.Cmd.API.csproj", "Services/RoleService/RoleService.Cmd.API/"]
COPY ["CQRS.Core/CQRS.Core.csproj", "CQRS.Core/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["Services/Service.Common/Service.Common.csproj", "Services/Service.Common/"]
COPY ["CommonLibrary/CommonLibrary.csproj", "CommonLibrary/"]
COPY ["DBContexts/EgmIoT/EgmIoT.csproj", "DBContexts/EgmIoT/"]
COPY ["Services/RoleService/RoleService.Cmd.Domain/UserRoleService.Cmd.Domain.csproj", "Services/RoleService/RoleService.Cmd.Domain/"]
COPY ["Services/RoleService/RoleService.Cmd.Infrastructure/UserRoleService.Cmd.Infrastructure.csproj", "Services/RoleService/RoleService.Cmd.Infrastructure/"]
RUN dotnet restore "./Services/RoleService/RoleService.Cmd.API/UserRoleService.Cmd.API.csproj"
COPY . .
WORKDIR "/src/Services/RoleService/RoleService.Cmd.API"
RUN dotnet build "./UserRoleService.Cmd.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 此階段可用來發佈要複製到最終階段的服務專案
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserRoleService.Cmd.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 此階段用於生產環境，或以一般模式從 VS 執行時 (未使用偵錯設定時的預設值)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserRoleService.Cmd.API.dll"]