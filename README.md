# TechnicalAssessment
Technical Assessment for Konica Minolta

This program is run in ASP.NET Core 3.1
The only Nuget package that is needed to be installed is Newtonsoft.Json

Open in VS 2019, and do not run using IIS Express, just run the application by itself to start the server

As this is built in C# it is using the WebSockets API, so according to the given instructions please change 
the second argument of the Elm.Main.embed function in the client/init.js file to:

const app = Elm.Main.embed(node, {
    api: 'WebSocket',
    hostname: 'ws://localhost:8081',
});
