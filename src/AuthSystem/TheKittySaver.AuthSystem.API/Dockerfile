FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AuthSystem/TheKittySaver.AuthSystem.API/TheKittySaver.AuthSystem.API.csproj", "src/AuthSystem/TheKittySaver.AuthSystem.API/"]
RUN dotnet restore "src/AuthSystem/TheKittySaver.AuthSystem.API/TheKittySaver.AuthSystem.API.csproj"
COPY . .
WORKDIR "/src/src/AuthSystem/TheKittySaver.AuthSystem.API"
RUN dotnet build "./TheKittySaver.AuthSystem.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TheKittySaver.AuthSystem.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER app

ENTRYPOINT ["dotnet", "TheKittySaver.AuthSystem.API.dll"]
