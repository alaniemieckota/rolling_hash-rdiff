FROM mcr.microsoft.com/dotnet/sdk:5.0
WORKDIR /src
COPY src .
#RUN ls -la
RUN dotnet restore
RUN dotnet build
RUN dotnet test

