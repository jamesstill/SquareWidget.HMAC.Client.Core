# SquareWidget.HMAC.Client.Core

HttpClient base class for HMAC authentication in ASP.NET Core 2.1

### Status

[![Build status](https://jamesstill.visualstudio.com/SquareWidget.HMAC.Client.Core/_apis/build/status/SquareWidget.HMAC.Client.Core-ASP.NET%20Core-CI)](https://jamesstill.visualstudio.com/SquareWidget.HMAC.Client.Core/_build/latest?definitionId=14)

### Prerequisites

ASP.NET Core 2.1

### Getting Started

See the [documentation](https://squarewidget.com/squarewidget-hmac-middleware) for usage. Download the NuGet package in your ASP.NET Core 2.1 client. 

```
var baseUri = "https://localhost:44320";
var credentials = new ClientCredentials
{
    ClientId = "testClient",
    ClientSecret = "P@ssw0rd"
};

var requestUri = "api/widgets/1";
using (var client = new HmacHttpClient(baseUri, credentials))
{
    var widget = client.Get<Widget>(requestUri).Result;
    // do something with widget ID 1...
}
```

### Server Side

Use SquareWidget.HMAC.Server.Core package. See the [documentation](https://squarewidget.com/squarewidget-hmac-middleware).


## Versioning

Version 2.1.0 targeting ASP.NET Core 2.1 

## Authors

[James Still](http://www.squarewidget.com)

## License

None