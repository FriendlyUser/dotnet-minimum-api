FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Debug -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:7.0
WORKDIR /App
RUN apt update -y
RUN apt install libnss3-tools -y
COPY scripts/*.sh ./
RUN ls /etc/ssl/certs && chmod +x ubuntu.sh && ./ubuntu.sh
COPY --from=build-env /App/out .
RUN cp dotnet-devcert.key /etc/ssl/certs/dotnet-devcert.key
RUN chmod 644 /etc/ssl/certs/dotnet-devcert.key
RUN chmod 644 /etc/ssl/certs/dotnet-devcert.pem
RUN ASPNETCORE_ENVIRONMENT=Development dotnet run
ENTRYPOINT ["./dotnet-minimum-api"]