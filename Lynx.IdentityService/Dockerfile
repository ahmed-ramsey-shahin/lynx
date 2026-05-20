# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

# Copy project files
COPY ["src/Lynx.IdentityService.Api/Lynx.IdentityService.Api.csproj", "src/Lynx.IdentityService.Api/"]
COPY ["src/Lynx.IdentityService.Application/Lynx.IdentityService.Application.csproj", "src/Lynx.IdentityService.Application/"]
COPY ["src/Lynx.IdentityService.Domain/Lynx.IdentityService.Domain.csproj", "src/Lynx.IdentityService.Domain/"]
COPY ["src/Lynx.IdentityService.Infrastructure/Lynx.IdentityService.Infrastructure.csproj", "src/Lynx.IdentityService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Lynx.IdentityService.Api/Lynx.IdentityService.Api.csproj"

# Copy all source code
COPY . .

# Build and publish
# TODO: Change Debug to Realease in production
RUN dotnet publish "src/Lynx.IdentityService.Api/Lynx.IdentityService.Api.csproj" -c Debug -o /app

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update && apt-get install -y tzdata && \
    ln -fs /usr/share/zoneinfo/America/Montreal /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata && \
    rm -rf /var/lib/apt/lists/*

ENV TZ=America/Montreal
# TODO: Remove this line in production
ENV ASPNETCORE_ENVIRONMENT=development

WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT [ "dotnet", "Lynx.IdentityService.Api.dll" ]
