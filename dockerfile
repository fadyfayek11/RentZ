FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 9020

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["RentZ.DAL/RentZ.DAL.csproj", "RentZ.DAL/"]
COPY ["RentZ.Application/RentZ.Application.csproj", "RentZ.Application/"]
COPY ["RentZ.Infrastructure/RentZ.Infrastructure.csproj", "RentZ.Infrastructure/"]
COPY ["RentZ/RentZ.API.csproj", "RentZ.API/"]
COPY ["RentZ.DTO/RentZ.DTO.csproj", "RentZ.DTO/"]
RUN dotnet restore "RentZ.API/RentZ.API.csproj"  

# Build the project
WORKDIR "/src/."
RUN dotnet build "RentZ.API.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "RentZ.API.csproj" -c Release -o /app/publish

# Final stage: Use the base image and copy the published app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RentZ.API.dll"]