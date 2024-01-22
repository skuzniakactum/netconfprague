# .NET Conf Prague 2023
This repository contains code of the demo application for my talk "Azure AI for Fun and Profit: Unleash the Potential of Your Applications" during .NET Conference Prague.

## Azure subscription

The demos require Azure subscription.

* ChickenOnHead project requires Face API resource - https://portal.azure.com/#view/Microsoft_Azure_ProjectOxford/CognitiveServicesHub/~/Face
* LiveTranslator project required Speech service resource - https://portal.azure.com/#view/Microsoft_Azure_ProjectOxford/CognitiveServicesHub/~/SpeechServices

## Running the demos

1. In each project, copy the `appsettings.local.json.example` file and rename it to `appsettings.local.json`.
2. Navigate to corresponding Azure resource and copy one of the keys and the other property (either region or endpoind), required to authenticate with the service
3. Run the application

## Remarks

* Both applications require internet connection
* ChickenOnFace app will be accessing camera by ID, which is a bit random and depends on cameras currently connected to the computer. Default camera has an ID of 0, but if there are more cameras connected it could be different.

