# Azure Functions Todo List Sample

Note: this application is referenced in the blog post [A Serverless ToDo List](https://ssemyan.github.io/serverless/2018/09/03/serverless-todo-list.html)

This sample demonstrates a single page application (SPA) hosted on Azure Storage, with an api backend built using Azure Functions. The site uses proxies to route requests for static content to the storage account, CosmosDB to store data, and Azure Active Directory for authentication.

![Screenshot](https://github.com/ssemyan/TodoServerless/raw/master/Screenshot.png)

This code can be run locally (using the Azure Functions CLI and CosmosDB emulator) as well as in Azure. Instructions for both are below.

The application is a simple Todo list where users can add items "todo". The items are stored in a single CosmosDB document collection but each user can only access their items (user identification is via the claims from the authentication mechanism). 

The SPA is pretty simple with Bootstrap for styles, Knockout.js for data binding, and JQuery for ajax calls. 

Users can add new items to their list, or mark existing items as complete (which deletes them). The inital call to the API pulls the current list of items for the user, along with the user's display name (from the auth claims). 

Note: if you are looking for a Functions 2.0 version, refer to https://github.com/ssemyan/TodoServerless2 (uses Cosmos DB for data storage and implements authentication) or https://github.com/ssemyan/TodoServerless3 (uses Azure Storage Tables for data storage and does not implement authentication).

## Setup steps on Localhost

1. Install the Azure CLI tools from here: https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local

1. If you want to use the emulator for local development, install the CosmosDB emulator from here: https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator

1. In the emulator or in Azure, create a new document collection called 'TodoItems' in a new database called 'ServerlessTodo' and set the Partition Key to '/ItemOwner'

1. Update the connection string in **_local.settings.json_** to the one for the emulator or Azure

1. Right click the solution, choose properties, and set both the ui and api project to start. 

## Setup steps on Azure

1. Create a new Azure Functions app

1. Create a CosmosDB account

1. Create a new document collection called 'TodoItems' in a new database called 'ServerlessTodo' and set the Partition Key to '/ItemOwner'. You can do this by using the Data Explorer blade and clicking 'New Container'

1. Copy the connetion string for the CosmosDB account (found in the Keys tab) and paste it into a new application setting in the function app called 'AzureWebJobsDocumentDBConnectionString' (this is found in the Configuration settings of the Function App Settings - remember to click 'Save').

1. Update the remoteUrl locations in **_vars.js_** to point to the functions endpoint

1. In the storage account for the functions app (or in a different storage account or CDN), upload the static content into a new blob container and mark the container as Public Access Level - Blob. 

1. Update the **_proxies.json_** file to point to the location where the static files are located

1. **Optional** Enable AAD authentication in the Functions App and ensure the option to Login with Azure Active Directory is selected. If you don't do this, you will appear logged in as 'Dev User' and the logout link will not work. 

1. Publish the function code to the Functions App in Azure (e.g. using Visual Studio)

1. Navigate to the site. **Note** because we are using a proxy, the URL for the site is the base URL for the Functions App and you do not need to set CORS (since the URL for the site and API are the same). You can read about how this works in the blog post referenced at the top of this file. 
