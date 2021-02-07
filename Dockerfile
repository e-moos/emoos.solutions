FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app
COPY ./src/EMoos.Server/EMoos.Server.csproj ./src/EMoos.Server/EMoos.Server.csproj
COPY EMoos.sln EMoos.sln
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o output

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0

EXPOSE 80
EXPOSE 443
WORKDIR /app

COPY --from=build-env /app/output .

ENTRYPOINT ["dotnet", "EMoos.Server.dll"]