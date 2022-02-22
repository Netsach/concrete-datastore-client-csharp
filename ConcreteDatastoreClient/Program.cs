using System;
using ConnectorConcrete;
using System.Threading;

string url = "http://10.0.0.125:8000/api/v1.1/";
string token = "2f90e3e062ea286e69fe7e928c9fa414062a3a44";

ConcreteDatastoreClient client = new ConcreteDatastoreClient(url: url, token: token);

string[] channels_ids = { "1234", "3456", "5678" };
Data postData = new Data() {
    { "audit_data", channels_ids },
    { "config", channels_ids },
    { "generate_status", "WAITING" },
    { "model_name", "" },
    { "prepare_status", "PENDING" },
    { "security_coeff", 0.0 },
    { "template", channels_ids },
    { "user_data", channels_ids },
    { "audit_uid", "98d7ef22-1f84-46c3-b0a6-c98aaf9bc223" },
    // {"blanking_operation", "START" },
    // {"audit", channels_ids },
    // {"racetrack_id", "9879" },
    // {"status", "PENDING" }
};

Console.WriteLine("Ready to post");
Console.WriteLine(postData);

Data obj = client.Post(objectName: "audit-report", requestBody: postData);
