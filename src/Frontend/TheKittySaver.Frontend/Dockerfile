FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/Frontend/TheKittySaver.Frontend/TheKittySaver.Frontend.csproj", "src/Frontend/TheKittySaver.Frontend/"]

RUN dotnet restore "src/Frontend/TheKittySaver.Frontend/TheKittySaver.Frontend.csproj"

COPY . .
WORKDIR "/src/src/Frontend/TheKittySaver.Frontend"
RUN dotnet publish "TheKittySaver.Frontend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .

USER app

ENTRYPOINT ["dotnet", "TheKittySaver.Frontend.dll"]