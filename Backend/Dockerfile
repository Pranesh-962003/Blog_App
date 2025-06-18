# Use the .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore
COPY Backend/*.csproj ./Backend/
RUN dotnet restore ./Backend/Backend.csproj

# Copy the rest of the source code
COPY . .

# Publish
RUN dotnet publish ./Backend/Backend.csproj -c Release -o out

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Use port 80
EXPOSE 80
ENTRYPOINT ["dotnet", "Backend.dll"]
