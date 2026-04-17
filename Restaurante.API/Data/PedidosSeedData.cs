using Restaurante.API.Models;

namespace Restaurante.API.Data
{
    public static class PedidosSeedData
    {
        public static IReadOnlyList<PedidoSeedDefinition> ItensPadrao { get; } =
            new List<PedidoSeedDefinition>
            {
                new(
                    "cliente1@restaurante.com",
                    PeriodoRefeicao.Almoco,
                    new TimeOnly(12, 30),
                    TipoAtendimento.Presencial,
                    1,
                    null,
                    null,
                    new List<PedidoItemSeedDefinition>
                    {
                        new(1, 1),
                        new(2, 1)
                    }),
                new(
                    "cliente2@restaurante.com",
                    PeriodoRefeicao.Almoco,
                    new TimeOnly(13, 15),
                    TipoAtendimento.DeliveryProprio,
                    null,
                    null,
                    "Rua das Flores, 150 - Centro",
                    new List<PedidoItemSeedDefinition>
                    {
                        new(2, 2),
                        new(3, 1)
                    }),
                new(
                    "cliente3@restaurante.com",
                    PeriodoRefeicao.Jantar,
                    new TimeOnly(19, 40),
                    TipoAtendimento.DeliveryAplicativo,
                    null,
                    "IFood",
                    "Avenida Brasil, 980 - Jardim América",
                    new List<PedidoItemSeedDefinition>
                    {
                        new(1, 1),
                        new(2, 2)
                    })
            };
    }

    public sealed record PedidoSeedDefinition(
        string EmailCliente,
        PeriodoRefeicao Periodo,
        TimeOnly Horario,
        TipoAtendimento TipoAtendimento,
        int? NumeroMesa,
        string? NomeAplicativo,
        string? EnderecoEntrega,
        IReadOnlyList<PedidoItemSeedDefinition> Itens);

    public sealed record PedidoItemSeedDefinition(
        int OrdemItemNoPeriodo,
        int Quantidade);
}
