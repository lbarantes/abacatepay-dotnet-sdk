# abacatepay-dotnet-sdk

> Uma biblioteca .NET para integração de seu sistema com o serviço de pagamento da AbacatePay inicialmente desenvolvida por [Lucas Beliene](https://github.com/lbarantes).

## Instalação

Através da .NET Cli:
```bash
> dotnet add package AbacatePay --version 2.0.0
```

## Testado com
.NET 9.0

## Uso básico
```c#
using Abacatepay;
...
dynamic abacate = new AbacatePay("apiKey", true);

var body = new 
{
    frequency = "ONE_TIME",
    methods = new[] { "PIX" },
    products = new[]
    {
        new
        {
            externalId = "prod-1234",
            name = "Produto",
            description = "Descrição do produto",
            quantity = 1,
            price = 1000
        }
    },
    returnUrl = "https://example.com/billing",
    completionUrl = "https://example.com/completion",
    customer = new
    {
        name = "Lucas Beliene",
        cellphone = "(22) 0000-0000",
        email = "lucasbeliene@email.com",
        taxId = "123.456.789-01"
    }
};

var response = abacate.PixBillingCreate(null, body);
Console.WriteLine(response);
```

## Exemplos
Você pode rodar e testar alguns exemplos contidos no projeto `Examples` retirando os comentários das funções no arquivo `Program.cs`.
Lembre-se de colocar suas credenciais dentro de `Examples/credentials.json` antes de rodar

## Documentação adicional
A documentação completa com todos endpoints pode ser encontrada em https://abacatepay.readme.io/reference/oi.

## Contribuição
Report de bugs e pull request são bem-vindas, para isso acesse o repositório [aqui](https://github.com/lbarantes/abacatepay-dotnet-sdk).
Este projeto tem a intenção de ser um espaço seguro para colaboração.

## Licença
A biblioteca está disponível como código aberto sob os termos da [Licença MIT](LICENSE).