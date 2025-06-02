FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Dallal-Backend-v2/Dallal-Backend-v2.csproj", "Dallal-Backend-v2/"]
RUN dotnet restore "Dallal-Backend-v2/Dallal-Backend-v2.csproj"
COPY . .
WORKDIR "/src/Dallal-Backend-v2"
RUN dotnet build "Dallal-Backend-v2.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Dallal-Backend-v2.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dallal-Backend-v2.dll"]
