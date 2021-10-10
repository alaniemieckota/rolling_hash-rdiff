FROM mcr.microsoft.com/dotnet/sdk:5.0
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build
RUN dotnet test

