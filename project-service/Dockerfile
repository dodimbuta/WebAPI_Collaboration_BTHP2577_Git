#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Basic step
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Construction step
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["project-service/project-service.csproj", "project-service/"]
#COPY ["webapi_collaboration_BTHP2577/", "webapi_collaboration_BTHP2577/"]
RUN dotnet restore "./project-service/project-service.csproj"
COPY . .
WORKDIR "/src/project-service"
RUN dotnet build "./project-service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publication step
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./project-service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final step
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "project-service.dll"]