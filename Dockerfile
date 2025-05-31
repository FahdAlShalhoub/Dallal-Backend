FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ENV PATH="${PATH}:/root/.dotnet/tools"
ENV NODE_VERSION=23.3.0

RUN dotnet tool install -g Volo.Abp.Cli --version 9.0.0

RUN curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.1/install.sh | bash
RUN . "/root/.nvm/nvm.sh" && nvm install ${NODE_VERSION}
RUN . "/root/.nvm/nvm.sh" && nvm use v${NODE_VERSION}
RUN . "/root/.nvm/nvm.sh" && nvm alias default v${NODE_VERSION}
ENV PATH="/root/.nvm/versions/node/v${NODE_VERSION}/bin/:${PATH}"
RUN npm install -g yarn

WORKDIR /src



# Copy csproj files and restore dependencies
COPY *.sln .
COPY NuGet.Config .
COPY common.props .
COPY src/Dallal.Domain/*.csproj ./src/Dallal.Domain/
COPY src/Dallal.Domain.Shared/*.csproj ./src/Dallal.Domain.Shared/
COPY src/Dallal.Application/*.csproj ./src/Dallal.Application/
COPY src/Dallal.Application.Contracts/*.csproj ./src/Dallal.Application.Contracts/
COPY src/Dallal.EntityFrameworkCore/*.csproj ./src/Dallal.EntityFrameworkCore/
COPY src/Dallal.HttpApi/*.csproj ./src/Dallal.HttpApi/
COPY src/Dallal.HttpApi.Client/*.csproj ./src/Dallal.HttpApi.Client/
COPY src/Dallal.DbMigrator/*.csproj ./src/Dallal.DbMigrator/
COPY src/Dallal.Web/*.csproj ./src/Dallal.Web/
COPY src/Dallal.Integrations.Contracts/*.csproj ./src/Dallal.Integrations.Contracts/
COPY src/Dallal.Integrations/*.csproj ./src/Dallal.Integrations/
# Copy package.json and package-lock.json
COPY src/Dallal.Web/package.json ./src/Dallal.Web/
COPY src/Dallal.Web/package-lock.json ./src/Dallal.Web/
COPY src/Dallal.Web/yarn.lock ./src/Dallal.Web/
COPY src/Dallal.Web/abp.resourcemapping.js ./src/Dallal.Web/

# Add test projects
COPY test/Dallal.Domain.Tests/*.csproj ./test/Dallal.Domain.Tests/
COPY test/Dallal.Application.Tests/*.csproj ./test/Dallal.Application.Tests/
COPY test/Dallal.Web.Tests/*.csproj ./test/Dallal.Web.Tests/
COPY test/Dallal.TestBase/*.csproj ./test/Dallal.TestBase/
COPY test/Dallal.EntityFrameworkCore.Tests/*.csproj ./test/Dallal.EntityFrameworkCore.Tests/
COPY test/Dallal.HttpApi.Client.ConsoleTestApp/*.csproj ./test/Dallal.HttpApi.Client.ConsoleTestApp/

RUN abp install-libs

RUN dotnet restore

# Copy all the source code and build
COPY src src
COPY test test

# # Generate development certificate for OpenId
# RUN dotnet dev-certs https -v -ep ./src/Dallal.Web/openiddict.pfx -p b216371b-aada-4679-91fa-31e2b849f7c2

# Build the application
# WORKDIR /src
RUN dotnet build -c Release 

# Publish the DbMigrator
WORKDIR /src/src/Dallal.DbMigrator
RUN dotnet publish -c Release -o /app/migrator

# Publish the Web application
WORKDIR /src/src/Dallal.Web
RUN dotnet publish --no-build -c Release -o /app/web

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy the migrator
COPY --from=build /app/migrator /app/migrator

# Copy the Web application
COPY --from=build /app/web /app/web

# # Copy certificate 
# COPY --from=build /src/src/Dallal.Web/openiddict.pfx /app/web/

# Copy the entrypoint script
COPY fly-entrypoint.sh /app/
RUN chmod +x /app/fly-entrypoint.sh

WORKDIR /app/web
EXPOSE 8080

ENTRYPOINT ["/app/fly-entrypoint.sh"] 