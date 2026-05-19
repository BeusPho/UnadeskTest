FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["UnadeskTestApp/UnadeskTestApp.csproj", "UnadeskTestApp/"]
COPY ["UnadeskTestCommon/UnadeskTestCommon.csproj", "UnadeskTestCommon/"]
RUN dotnet restore "UnadeskTestApp/UnadeskTestApp.csproj"

COPY . .
WORKDIR "/src/UnadeskTestApp"
RUN dotnet publish "UnadeskTestApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Worker не слушает HTTP, поэтому EXPOSE не нужен
ENTRYPOINT ["dotnet", "UnadeskTestApp.dll"]