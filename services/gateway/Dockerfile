# Multi-stage build: compile on the SDK image, run on the slim ASP.NET runtime image.
# Build context is this service's folder (services/gateway).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/JobTracker.Gateway/JobTracker.Gateway.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# The .NET 8 runtime image listens on 8080 by default.
EXPOSE 8080
ENTRYPOINT ["dotnet", "JobTracker.Gateway.dll"]
