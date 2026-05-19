# 1. Сборка
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем только csproj файлы для кэширования слоев NuGet
COPY ["UnadeskTestApiGateway/UnadeskTestApiGateway.csproj", "UnadeskTestApiGateway/"]
COPY ["UnadeskTestCommon/UnadeskTestCommon.csproj", "UnadeskTestCommon/"]
RUN dotnet restore "UnadeskTestApiGateway/UnadeskTestApiGateway.csproj"

# Копируем весь исходный код и публикуем
COPY . .
WORKDIR "/src/UnadeskTestApiGateway"
RUN dotnet publish "UnadeskTestApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Рантайм
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "UnadeskTestApiGateway.dll"]