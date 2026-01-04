FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/AdoptionSystem/TheKittySaver.AdoptionSystem.API/TheKittySaver.AdoptionSystem.API.csproj", "src/AdoptionSystem/TheKittySaver.AdoptionSystem.API/"]
COPY ["src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/TheKittySaver.AdoptionSystem.Domain.csproj", "src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain/"]
COPY ["src/AdoptionSystem/TheKittySaver.AdoptionSystem.Primitives/TheKittySaver.AdoptionSystem.Primitives.csproj", "src/AdoptionSystem/TheKittySaver.AdoptionSystem.Primitives/"]
COPY ["src/AdoptionSystem/TheKittySaver.AdoptionSystem.Persistence/TheKittySaver.AdoptionSystem.Persistence.csproj", "src/AdoptionSystem/TheKittySaver.AdoptionSystem.Persistence/"]
COPY ["src/AdoptionSystem/TheKittySaver.AdoptionSystem.ReadModels/TheKittySaver.AdoptionSystem.ReadModels.csproj", "src/AdoptionSystem/TheKittySaver.AdoptionSystem.ReadModels/"]
COPY ["src/AdoptionSystem/TheKittySaver.AdoptionSystem.ReadModels.EntityFramework/TheKittySaver.AdoptionSystem.ReadModels.EntityFramework.csproj", "src/AdoptionSystem/TheKittySaver.AdoptionSystem.ReadModels.EntityFramework/"]
COPY ["src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain.EntityFramework/TheKittySaver.AdoptionSystem.Domain.EntityFramework.csproj", "src/AdoptionSystem/TheKittySaver.AdoptionSystem.Domain.EntityFramework/"]
COPY ["src/AdoptionSystem/TheKittySaver.AdoptionSystem.Infrastructure/TheKittySaver.AdoptionSystem.Infrastructure.csproj", "src/AdoptionSystem/TheKittySaver.AdoptionSystem.Infrastructure/"]
COPY ["src/AdoptionSystem/TheKittySaver.AdoptionSystem.Contracts/TheKittySaver.AdoptionSystem.Contracts.csproj", "src/AdoptionSystem/TheKittySaver.AdoptionSystem.Contracts/"]
COPY ["src/AdoptionSystem/TheKittySaver.AdoptionSystem.Calculators/TheKittySaver.AdoptionSystem.Calculators.csproj", "src/AdoptionSystem/TheKittySaver.AdoptionSystem.Calculators/"]

RUN dotnet restore "src/AdoptionSystem/TheKittySaver.AdoptionSystem.API/TheKittySaver.AdoptionSystem.API.csproj"

COPY . .
WORKDIR "/src/src/AdoptionSystem/TheKittySaver.AdoptionSystem.API"
RUN dotnet publish "TheKittySaver.AdoptionSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .

USER app

ENTRYPOINT ["dotnet", "TheKittySaver.AdoptionSystem.API.dll"]