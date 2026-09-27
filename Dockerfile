# Stage 1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY ["CatalogAPI/CatalogAPI.csproj", "CatalogAPI/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

RUN dotnet restore "CatalogAPI/CatalogAPI.csproj"

COPY . .

RUN dotnet publish "CatalogAPI/CatalogAPI.csproj" -c Release -o /app/publish

# Stage 2
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CatalogAPI.dll"]

