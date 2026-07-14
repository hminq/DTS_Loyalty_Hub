FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY api/admin/Core/Core.csproj api/admin/Core/
COPY api/admin/Infrastructure/Infrastructure.csproj api/admin/Infrastructure/
COPY api/admin/Api/Api.csproj api/admin/Api/

RUN dotnet restore api/admin/Api/Api.csproj

COPY api/admin/ api/admin/

RUN dotnet publish api/admin/Api/Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Api.dll"]
