FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002

# Устанавливаем Python
RUN apt-get update && apt-get install -y python3 python3-pip && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["py_bot_mod.csproj", "./"]
RUN dotnet restore "py_bot_mod.csproj"
COPY . .
RUN dotnet build "py_bot_mod.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "py_bot_mod.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "py_bot_mod.dll"]