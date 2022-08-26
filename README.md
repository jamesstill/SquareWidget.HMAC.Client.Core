# SquareWidget.HMAC.Client.Core

HttpClient base class for HMAC authentication in .NET Core 6.0

### Prerequisites

.NET Core 6.0

### Getting Started

See the [documentation](https://squarewidget.com/squarewidget-hmac-middleware) for usage. 
Download the NuGet package in your .NET client solution. 

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

Version 6.0.0 targets .NET 6.0 

## Authors

[James Still](http://www.squarewidget.com)

## License

This project is licensed under the MIT License.