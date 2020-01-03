FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
EXPOSE 80
EXPOSE 443
WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o output

# Runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/output .

ENTRYPOINT ["dotnet", "EMoos.Server.dll"]