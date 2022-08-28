# Introduction 
MongoDbLongQueryKiller is a simple Azure Function app that periodically connects to a MongoDB database and checks for long-running queries meeting specific criteria, then terminates them.

# Getting Started
You can run the project locally for debugging. The assumption is that you have either installed the MongoDB Community Edition server, or you know the connection details for an existing hosted MongoDB server.

1. In the local.settings.json file:
    * Configure the 'MongoDbDetails:ConnectionString' string.
    * Set the desired 'MaxSeconds' value. When the main function runs, any detected queries exceeding this value will be terminated.
2. Run the project in Debug mode - you can put breakpoints in the main Run method within the LongQueryKiller class.
3. By default, the project runs on a 2 minute timer which is configured using NCRONTAB expressions. You can read more about Azure Function timers here, which includes NCRONTAB expression examples near the bottom: https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=csharp
4. When the project is running, you can bypass the requirement of waiting for the timer to tick by calling a hidden admin function endpoint in Postman. To do this, create and send a Postman request with the following settings:
    * Method: POST
    * URI: http://localhost:7071/admin/functions/LongQueryKiller
    * Headers: Content-Type:application/json
    * Body: { "input": "test" }

Additional info:
* Publishing to Azure: https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs?tabs=in-process#publish-to-azure
* Finding your settings in Azure: https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings?tabs=portal#get-started-in-the-azure-portal