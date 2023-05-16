# FHIR Data Conversion Web Service

Our FHIR Data Conversion Web Service is designed to implement the `$convert-data` custom FHIR (Fast Healthcare Interoperability Resources) operation, enabling the conversion of HL7v2, CCDA, JSON, and FHIR STU3 into FHIR R4 format.

## About

This service is built using the [Microsoft FHIR Converter](https://github.com/microsoft/FHIR-Converter) and is called similarly to the custom `$convert-data` FHIR operation described in [Microsoft FHIR Server documentation](https://github.com/microsoft/fhir-server/blob/main/docs/ConvertDataOperation.md). You send a request to our FHIR server specifying the data conversion you want to perform. The server then converts your input data into the FHIR R4 format, making it compatible with systems and applications that support this standard.

## Why Use This Service

The primary benefit of this service is the standardization of your healthcare data. If your existing data is in different formats (HL7v2, CCDA, JSON, FHIR STU3), you can convert it into the FHIR R4 format. This enables you to maintain a single, consistent data format across your systems, simplifying data management and interoperability.

## Build and Run

To build and run this service, you'll need to have [Docker](https://www.docker.com/) installed on your machine. Once you've installed Docker, you can build and run the service using the following commands:

```bash
docker build -t fhirconverter-image -f Dockerfile .
```

```bash
docker run -p 5001:5000 fhirconverter-image
```

**About custom templates**: If you want to use custom [Liquid](https://shopify.github.io/liquid/) templates, you'll need to make them accessible in your docker image and point the ENV var `TEMPLATE_ROOT_DIR` to that folder. If you don't want to use custom templates, you can leave the ENV var `TEMPLATE_ROOT_DIR` empty and the service will use the default templates from the Microsoft FHIR Converter. 

You can find more information about custom templates in the [Microsoft FHIR Converter documentation](https://github.com/microsoft/FHIR-Converter#hl7v2-to-fhir-conversion-templates).

## Usage

1. **Prepare Your Data**: Make sure your data is in one of the supported formats (HL7v2, CCDA, JSON, FHIR STU3).

2. **Send a Request**: Send a POST request to our FHIR server at the endpoint `https://your-fhir-server/$convert-data`. 

   The body of the request should be formatted according to the [FHIR Parameters](https://build.fhir.org/parameters.html) resource and include the input data, the input data type and the [root template](https://github.com/microsoft/FHIR-Converter#supported-parameters). Here's an example:

**Sample request for HL7v2 input data:**
```json
{
    "resourceType": "Parameters",
    "parameter": [
        {
            "name": "inputData",
            "valueString": "MSH|^~\\&|SIMHOSP|SFAC|RAPP|RFAC|20200508131015||ADT^A01|517|T|2.3|||AL||44|ASCII\nEVN|A01|20200508131015|||C005^Whittingham^Sylvia^^^Dr^^^DRNBR^PRSNL^^^ORGDR|\nPID|1|3735064194^^^SIMULATOR MRN^MRN|3735064194^^^SIMULATOR MRN^MRN~2021051528^^^NHSNBR^NHSNMBR||Kinmonth^Joanna^Chelsea^^Ms^^CURRENT||19870624000000|F|||89 Transaction House^Handmaiden Street^Wembley^^FV75 4GJ^GBR^HOME||020 3614 5541^HOME|||||||||C^White - Other^^^||||||||\nPD1|||FAMILY PRACTICE^^12345|\nPV1|1|I|OtherWard^MainRoom^Bed 183^Simulated Hospital^^BED^Main Building^4|28b|||C005^Whittingham^Sylvia^^^Dr^^^DRNBR^PRSNL^^^ORGDR|||CAR|||||||||16094728916771313876^^^^visitid||||||||||||||||||||||ARRIVED|||20200508131015||"
        },
        {
            "name": "inputDataType",
            "valueString": "Hl7v2"
        },
        {
            "name": "rootTemplate",
            "valueString": "ADT_A01"
        }
    ]
}
```

3. **Receive the Converted Data**: Our server will process your request, convert your data into the FHIR R4 format, and return it in the response.

## Support
This service is designed to support healthcare providers, healthcare IT professionals, and other users who need to convert healthcare data into the FHIR R4 format. It is not intended for use by the general public.

If you encounter any issues or have any questions about using the service, please feel free to reach out to our support team. We're here to help you make the most of your healthcare data.