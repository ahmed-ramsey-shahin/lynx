# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

# Copy project files
COPY ["src/Lynx.RedirectionService.Api/Lynx.RedirectionService.Api.csproj", "src/Lynx.RedirectionService.Api/"]
COPY ["src/Lynx.RedirectionService.Application/Lynx.RedirectionService.Application.csproj", "src/Lynx.RedirectionService.Application/"]
COPY ["src/Lynx.RedirectionService.Domain/Lynx.RedirectionService.Domain.csproj", "src/Lynx.RedirectionService.Domain/"]
COPY ["src/Lynx.RedirectionService.Infrastructure/Lynx.RedirectionService.Infrastructure.csproj", "src/Lynx.RedirectionService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Lynx.RedirectionService.Api/Lynx.RedirectionService.Api.csproj"

# Copy all source code
COPY . .

# Build and publish
# TODO: Change Debug to Realease in production
RUN dotnet publish "src/Lynx.RedirectionService.Api/Lynx.RedirectionService.Api.csproj" -c Debug -o /app

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
ENTRYPOINT [ "dotnet", "Lynx.RedirectionService.Api.dll" ]
