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
COPY scripts/*.sh ./
RUN ls && chmod +x ubuntu.sh && ./ubuntu.sh
COPY --from=build-env /App/out .
ENTRYPOINT ["./dotnet-minimum-api"]