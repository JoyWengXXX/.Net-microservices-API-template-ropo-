FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# 複製必要的專案檔
COPY Directory.Build.props Directory.Build.props
COPY Directory.Build.targets Directory.Build.targets
COPY Directory.Packages.props Directory.Packages.props
COPY CommonLibrary/CommonLibrary.csproj CommonLibrary/
COPY DBContexts/EgmIoT/EgmIoT.csproj DBContexts/EgmIoT/
COPY tests/ConfigurationHelper.Test/ConfigurationHelper.Test.csproj tests/ConfigurationHelper.Test/
COPY tests/ConfigurationHelper.Test/Directory.Build.props tests/ConfigurationHelper.Test/
COPY tests/ConfigurationHelper.Test/Directory.Build.targets tests/ConfigurationHelper.Test/

# 還原 NuGet 套件
RUN dotnet restore tests/ConfigurationHelper.Test/ConfigurationHelper.Test.csproj

# 複製所有原始碼
COPY CommonLibrary/ CommonLibrary/
COPY DBContexts/EgmIoT/ DBContexts/EgmIoT/
COPY tests/ConfigurationHelper.Test/ tests/ConfigurationHelper.Test/

# 建置測試程式
RUN dotnet build tests/ConfigurationHelper.Test/ConfigurationHelper.Test.csproj -c Release -o /app

# 執行階段
FROM mcr.microsoft.com/dotnet/runtime:9.0

WORKDIR /app

# 複製編譯結果
COPY --from=build /app .
COPY --from=build /src/DBContexts/EgmIoT/*.json /app/

# 執行測試程式
ENTRYPOINT ["dotnet", "ConfigurationHelper.Test.dll"]
