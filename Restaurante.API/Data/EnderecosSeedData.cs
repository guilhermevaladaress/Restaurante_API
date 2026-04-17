namespace Restaurante.API.Data
{
    public static class EnderecosSeedData
    {
        public static IReadOnlyList<EnderecoSeedDefinition> ItensPadrao { get; } =
            new List<EnderecoSeedDefinition>
            {
                new(
                    "cliente1@restaurante.com",
                    "Rua das Acácias",
                    "120",
                    "Centro",
                    "Belo Horizonte",
                    "30110-000",
                    "Apto 201"),
                new(
                    "cliente2@restaurante.com",
                    "Avenida Brasil",
                    "980",
                    "Jardim América",
                    "Belo Horizonte",
                    "30421-000",
                    null),
                new(
                    "cliente3@restaurante.com",
                    "Rua dos Ipês",
                    "45",
                    "Savassi",
                    "Belo Horizonte",
                    "30140-110",
                    "Casa fundos")
            };
    }

    public sealed record EnderecoSeedDefinition(
        string EmailCliente,
        string Logradouro,
        string Numero,
        string Bairro,
        string Cidade,
        string CEP,
        string? Complemento);
}
