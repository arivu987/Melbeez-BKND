FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /melbeez-bknd
COPY . .
RUN dotnet build -c Release
EXPOSE 8000
ENTRYPOINT ["dotnet", "run", "--project", "/melbeez-bknd/Melbeez/Melbeez.csproj"]