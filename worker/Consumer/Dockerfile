FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Persistence/Persistence.csproj shared/Persistence/
COPY shared/Campaign.Contracts/Campaign.Contracts.csproj shared/Campaign.Contracts/
COPY shared/Messaging.Contracts/Messaging.Contracts.csproj shared/Messaging.Contracts/
COPY worker/Consumer/Core/Consumer.Core.csproj worker/Consumer/Core/
COPY worker/Consumer/Infrastructure/Consumer.Infrastructure.csproj worker/Consumer/Infrastructure/
COPY worker/Consumer/Consumer.csproj worker/Consumer/

RUN dotnet restore worker/Consumer/Consumer.csproj

COPY shared/Persistence/ shared/Persistence/
COPY shared/Campaign.Contracts/ shared/Campaign.Contracts/
COPY shared/Messaging.Contracts/ shared/Messaging.Contracts/
COPY worker/ worker/

RUN dotnet publish worker/Consumer/Consumer.csproj \
    --configuration Release \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

ENV DOTNET_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Consumer.dll"]
