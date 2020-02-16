FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
EXPOSE 80
EXPOSE 443
WORKDIR /app
COPY ./src/EMoos.Client/EMoos.Client.fsproj ./src/EMoos.Client/EMoos.Client.fsproj
COPY ./src/EMoos.Server/EMoos.Server.fsproj ./src/EMoos.Server/EMoos.Server.fsproj
COPY EMoos.sln EMoos.sln
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o output

# Runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/output .

ENTRYPOINT ["dotnet", "EMoos.Server.dll"]