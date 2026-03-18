# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FembStockTicker/FembStockTicker.csproj FembStockTicker/
RUN dotnet restore FembStockTicker/FembStockTicker.csproj

COPY FembStockTicker/ FembStockTicker/
RUN dotnet publish FembStockTicker/FembStockTicker.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FembStockTicker.dll"]
