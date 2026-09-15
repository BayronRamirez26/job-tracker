# Multi-stage build: compile on the SDK image, run on the slim ASP.NET runtime image.
# Build context is this service's folder (services/applications). Publishing the Api project
# restores only it and its project references (Domain/Application/Infrastructure), not the tests.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/JobTracker.Applications.Api/JobTracker.Applications.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "JobTracker.Applications.Api.dll"]
