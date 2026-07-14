FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY api/customer/Core/Core.csproj api/customer/Core/
COPY api/customer/Infrastructure/Infrastructure.csproj api/customer/Infrastructure/
COPY api/customer/Api/Api.csproj api/customer/Api/

RUN dotnet restore api/customer/Api/Api.csproj

COPY api/customer/ api/customer/

RUN dotnet publish api/customer/Api/Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Api.dll"]
