#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Basic step
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Construction step
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
#COPY ["webapi_collaboration_BTHP2577/", "webapi_collaboration_BTHP2577/"]
COPY ["user-service/user-service.csproj", "user-service/"]
RUN dotnet restore "./user-service/user-service.csproj"
COPY . .
WORKDIR "/src/user-service"
RUN dotnet build "./user-service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publication step
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./user-service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final step
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "user-service.dll"]
