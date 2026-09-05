# 1. Етап збірки (Build Stage)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копіюємо файл проекту та відновлюємо залежності
COPY *.csproj ./
RUN dotnet restore

# Копіюємо решту файлів та виконуємо публікацію
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# 2. Етап запуску (Runtime Stage)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Копіюємо зібрані файли з етапу build
COPY --from=build /app/publish .

# Налаштовуємо стандартний порт ASP.NET Core для контейнерів
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Запуск додатку
ENTRYPOINT ["dotnet", "aspnetcore_server.dll"]
