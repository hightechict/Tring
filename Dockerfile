#
#  Build image
#
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
ARG version=0.0.0-local

COPY src/Tring.Common/Tring.Common.csproj /src/Tring.Common/
COPY src/Tring/Tring.csproj /src/Tring/

#COPY *.sln /
#COPY nuget.config /

RUN dotnet restore /src/Tring

COPY src /src/

RUN dotnet publish --no-restore -c Release -o /pub /p:VERSION=${version} /src/Tring

#
#  Final image
#
FROM mcr.microsoft.com/dotnet/runtime:7.0

ENV COMPlus_EnableDiagnostics=0

COPY --from=build-env /pub app/

WORKDIR /app
ENTRYPOINT ["./Tring"]
