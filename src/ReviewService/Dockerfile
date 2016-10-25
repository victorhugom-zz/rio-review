FROM microsoft/dotnet:onbuild

COPY . /app
WORKDIR /app

ENV ASPNETCORE_URLS http://*:5060
EXPOSE 5060

RUN dotnet restore
RUN dotnet build -c Release

ENTRYPOINT ["dotnet", "run"]
