FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

WORKDIR /App

COPY . ./

RUN dotnet restore

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine

WORKDIR /App
COPY --from=build /App/out .

ENTRYPOINT ["dotnet", "OrderServiceBot.dll"]

