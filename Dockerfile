FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

# Clone MS FHIR-Converter repo
RUN git clone https://bitbucket.org/elixionmedical/fhir-converter.git

# Build MS FHIR-Converter
#WORKDIR /App/FHIR-Converter
#RUN dotnet restore
#RUN dotnet publish -c Release -o out

# Copy our FHIRConverter into MS FHIR-Converter
WORKDIR /App
COPY ./FHIRConverter.csproj ./fhir-converter/src/UME.Fhir.Converter/FHIRConverter.csproj
COPY ./Models ./fhir-converter/src/UME.Fhir.Converter/Models
COPY ./Properties ./fhir-converter/src/UME.Fhir.Converter/Properties
COPY ./Program.cs ./fhir-converter/src/UME.Fhir.Converter/Program.cs
COPY ./HttpContextExtensions.cs ./fhir-converter/src/UME.Fhir.Converter/HttpContextExtensions.cs
COPY ./ConverterLogicHandler.cs ./fhir-converter/src/UME.Fhir.Converter/ConverterLogicHandler.cs
COPY ./appsettings.json ./fhir-converter/src/UME.Fhir.Converter/appsettings.json
# fixes some CI bug with nuget authorization (see https://stackoverflow.com/a/76662452)
COPY ./nuget.config ./fhir-converter/nuget.config

# Build our FHIRConverter
WORKDIR /App/fhir-converter/src/UME.Fhir.Converter
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=build-env /App/fhir-converter/src/UME.Fhir.Converter/out .
COPY --from=build-env /App/fhir-converter/data/Templates ./templates
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "FHIRConverter.dll"]
