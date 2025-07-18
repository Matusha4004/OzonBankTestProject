# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# копим проект приложения
COPY OzonBankTestProject/*.csproj ./OzonBankTestProject/
RUN dotnet restore ./OzonBankTestProject/OzonBankTestProject.csproj

# копим весь исходник
COPY . .
WORKDIR /src/OzonBankTestProject
RUN dotnet publish -c Release -o /app/publish

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OzonBankTestProject.dll"]
