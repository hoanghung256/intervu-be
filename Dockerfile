## Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files to leverage Docker layer caching
COPY Intervu.sln ./
COPY Intervu.API/Intervu.API.csproj Intervu.API/
COPY Intervu.Application/Intervu.Application.csproj Intervu.Application/
COPY Intervu.Domain/Intervu.Domain.csproj Intervu.Domain/
COPY Intervu.Infrastructure/Intervu.Infrastructure.csproj Intervu.Infrastructure/
COPY Intervu.API.Test/Intervu.API.Test.csproj Intervu.API.Test/

# Restore dependencies
RUN dotnet restore Intervu.sln

# Copy the rest of the source
COPY . .

# Publish the API project
RUN dotnet publish Intervu.API/Intervu.API.csproj -c Release -o /app/publish /p:UseAppHost=false

## Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Listen on 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["dotnet", "Intervu.API.dll"]
