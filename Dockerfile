FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

# Copy everything
RUN git clone https://github.com/microsoft/FHIR-Converter.git

# Build MS FHIR-Converter
WORKDIR /App/FHIR-Converter
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Copy our FHIRConverter to /App
WORKDIR /App
COPY ./FHIRConverter.csproj ./FHIR-Converter/src/Firemetrics.Fhir.Converter/FHIRConverter.csproj
COPY ./Models ./FHIR-Converter/src/Firemetrics.Fhir.Converter/Models
COPY ./Properties ./FHIR-Converter/src/Firemetrics.Fhir.Converter/Properties
COPY ./Program.cs ./FHIR-Converter/src/Firemetrics.Fhir.Converter/Program.cs
COPY ./HttpContextExtensions.cs ./FHIR-Converter/src/Firemetrics.Fhir.Converter/HttpContextExtensions.cs
COPY ./ConverterLogicHandler.cs ./FHIR-Converter/src/Firemetrics.Fhir.Converter/ConverterLogicHandler.cs
COPY ./appsettings.json ./FHIR-Converter/src/Firemetrics.Fhir.Converter/appsettings.json

# Build our FHIRConverter
WORKDIR /App/FHIR-Converter/src/Firemetrics.Fhir.Converter
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=build-env /App/FHIR-Converter/src/Firemetrics.Fhir.Converter/out .
COPY --from=build-env /App/FHIR-Converter/data/Templates ./templates
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "FHIRConverter.dll"]