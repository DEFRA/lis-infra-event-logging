ARG PARENT_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${PARENT_VERSION} AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Worker/Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:${PARENT_VERSION} AS production
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Defra.Lis.EventLogging.Worker.dll"]
