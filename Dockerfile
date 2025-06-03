FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Dallal-Backend-v2/Dallal-Backend-v2.csproj", "Dallal-Backend-v2/"]
RUN dotnet restore "Dallal-Backend-v2/Dallal-Backend-v2.csproj"
COPY . .
WORKDIR "/src/Dallal-Backend-v2"
RUN dotnet build "Dallal-Backend-v2.csproj" -c $BUILD_CONFIGURATION -o /app/build

ENV EF_BUNDLE_EXECUTION=true
RUN dotnet ef migrations bundle --self-contained -o /app/build/efbundle

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Dallal-Backend-v2.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /app/build/efbundle .

ENTRYPOINT ["dotnet", "Dallal-Backend-v2.dll"]
