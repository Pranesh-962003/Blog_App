FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY Client/*.csproj ./Client/
RUN dotnet restore ./Client/Client.csproj

COPY . .
RUN dotnet publish ./Client/Client.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80
ENTRYPOINT ["dotnet", "Client.dll"]
