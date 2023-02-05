FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:7.0
WORKDIR /App
RUN apt update -y
RUN apt install libnss3-tools -y
RUN chmod +x scripts/ubuntu.sh && ./scripts/ubuntu.sh
COPY --from=build-env /App/out .
ENTRYPOINT ["./dotnet-minimum-api"]