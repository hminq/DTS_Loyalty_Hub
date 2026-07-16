FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Persistence/Persistence.csproj shared/Persistence/
COPY worker/Core/Core.csproj worker/Core/
COPY worker/Infrastructure/Infrastructure.csproj worker/Infrastructure/
COPY worker/Scheduler/Scheduler.csproj worker/Scheduler/

RUN dotnet restore worker/Scheduler/Scheduler.csproj

COPY shared/Persistence/ shared/Persistence/
COPY worker/ worker/

RUN dotnet publish worker/Scheduler/Scheduler.csproj \
    --configuration Release \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

ENV DOTNET_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Scheduler.dll"]
