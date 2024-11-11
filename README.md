# .NET OpenAI Samples
.NET samples for the [official OpenAI library for .NET](https://github.com/openai/openai-dotnet) with Azure OpenAI. Require [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or later. 

## REST Client samples
The repo also includes sample HTTP requests for the Azure OpenAI REST API. Many current IDEs and editors support these.

### Visual Studio Code

1. Install the [REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client).
2. Copy `.vscode/settings.template.json` to `.vscode/settings.json` and update all settings with your deployment details such as endpoint, API key etc.
3. Select the REST Client `dev` environment.


### JetBrains IDEs (IntelliJ, Rider etc.)
1. Install the [HTTP Client plugin](https://www.jetbrains.com/help/idea/http-client-in-product-code-editor.html).
2. Copy `rest-api/http-client.private.env.template.json` to `rest-api/http-client.private.env.json` and update all settings with your deployment details such as endpoint, API key etc.
3. Select the HTTP Client `dev` environment.
