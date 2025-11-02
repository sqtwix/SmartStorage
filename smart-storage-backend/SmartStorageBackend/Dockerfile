FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файл проекта из текущей директории
COPY ["SmartStorageBackend.csproj", "."]
RUN dotnet restore "SmartStorageBackend.csproj"

# Копируем весь исходный код
COPY . .
RUN dotnet build "SmartStorageBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartStorageBackend.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5171
ENTRYPOINT ["dotnet", "SmartStorageBackend.dll"]