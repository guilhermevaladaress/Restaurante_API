using Microsoft.EntityFrameworkCore;
using Restaurante.API.Models;

namespace Restaurante.API.Data
{
    public static class DbInitializer
    {
        private const int LimiteItensPorPeriodo = 20;
        private static readonly TimeSpan HorarioReservaPadraoInicio = new(11, 0, 0);
        private static readonly TimeSpan HorarioReservaPadraoFim = new(15, 0, 0);
        private static readonly IReadOnlyList<MesaSeedDefinition> MesasPadrao =
        [
            new(1, 2),
            new(2, 2),
            new(3, 4),
            new(4, 4),
            new(5, 4),
            new(6, 6),
            new(7, 6),
            new(8, 8)
        ];

        public static async Task SeedAsync(AppDbContext context)
        {
            ValidarLimiteDoSeed();

            await GarantirTabelaMesasAsync(context);
            await GarantirMesasPadraoAsync(context);
            await GarantirConfiguracaoReservaPadraoAsync(context);

            var ingredientesExistentes = await context.Ingredientes.ToListAsync();
            var itensExistentes = await context.ItensCardapio.ToListAsync();
            var houveAlteracao = false;

            var chavesSeed = CardapioSeedData.ItensPadrao
                .Select(s => $"{(int)s.Periodo}:{s.Nome}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var ingredienteNome in IngredientesSeedData.ItensPadrao)
            {
                var ingredienteExiste = ingredientesExistentes.Any(i =>
                    string.Equals(i.Nome, ingredienteNome, StringComparison.OrdinalIgnoreCase));

                if (ingredienteExiste)
                    continue;

                var novoIngrediente = new Ingrediente
                {
                    Nome = ingredienteNome
                };

                context.Ingredientes.Add(novoIngrediente);
                ingredientesExistentes.Add(novoIngrediente);
                houveAlteracao = true;
            }

            foreach (var seed in CardapioSeedData.ItensPadrao)
            {
                var itemExistente = itensExistentes.FirstOrDefault(item =>
                    item.Periodo == seed.Periodo &&
                    string.Equals(item.Nome, seed.Nome, StringComparison.OrdinalIgnoreCase));

                if (itemExistente is null)
                {
                    var novoItem = new ItemCardapio
                    {
                        Nome = seed.Nome,
                        Descricao = seed.Descricao,
                        PrecoBase = seed.PrecoBase,
                        Periodo = seed.Periodo,
                        Ativo = true
                    };

                    context.ItensCardapio.Add(novoItem);
                    itensExistentes.Add(novoItem);
                    houveAlteracao = true;
                    continue;
                }

                if (itemExistente.Descricao != seed.Descricao)
                {
                    itemExistente.Descricao = seed.Descricao;
                    houveAlteracao = true;
                }

                if (itemExistente.PrecoBase != seed.PrecoBase)
                {
                    itemExistente.PrecoBase = seed.PrecoBase;
                    houveAlteracao = true;
                }

                if (!itemExistente.Ativo)
                {
                    itemExistente.Ativo = true;
                    houveAlteracao = true;
                }
            }

            foreach (var item in itensExistentes)
            {
                var chaveItem = $"{(int)item.Periodo}:{item.Nome}";
                if (!chavesSeed.Contains(chaveItem) && item.Ativo)
                {
                    item.Ativo = false;
                    houveAlteracao = true;
                }
            }

            if (houveAlteracao)
                await context.SaveChangesAsync();

            var ingredientesPorNome = await context.Ingredientes
                .AsNoTracking()
                .ToDictionaryAsync(i => i.Nome, StringComparer.OrdinalIgnoreCase);

            var itensPorChave = await context.ItensCardapio
                .AsNoTracking()
                .ToDictionaryAsync(
                    i => $"{(int)i.Periodo}:{i.Nome}",
                    i => i.Id,
                    StringComparer.OrdinalIgnoreCase);

            var relacionamentosExistentes = await context.ItemCardapioIngredientes
                .AsNoTracking()
                .Select(ii => new { ii.ItemCardapioId, ii.IngredienteId })
                .ToListAsync();

            var chavesRelacionamento = relacionamentosExistentes
                .Select(ii => $"{ii.ItemCardapioId}:{ii.IngredienteId}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var houveAlteracaoRelacionamentos = false;

            foreach (var itemSeed in CardapioSeedData.ItensPadrao)
            {
                var chaveItem = $"{(int)itemSeed.Periodo}:{itemSeed.Nome}";
                if (!itensPorChave.TryGetValue(chaveItem, out var itemId))
                    continue;

                var ingredientesDoItem = CardapioIngredientesSeedData.ObterIngredientes(itemSeed);

                foreach (var ingredienteNome in ingredientesDoItem)
                {
                    if (!ingredientesPorNome.TryGetValue(ingredienteNome, out var ingrediente))
                        continue;

                    var chaveRelacionamento = $"{itemId}:{ingrediente.Id}";
                    if (!chavesRelacionamento.Add(chaveRelacionamento))
                        continue;

                    context.ItemCardapioIngredientes.Add(new ItemCardapioIngrediente
                    {
                        ItemCardapioId = itemId,
                        IngredienteId = ingrediente.Id
                    });

                    houveAlteracaoRelacionamentos = true;
                }
            }

            if (houveAlteracaoRelacionamentos)
                await context.SaveChangesAsync();

            await GarantirEnderecosIniciaisAsync(context);
            await GarantirPedidosIniciaisAsync(context);
        }

        private static async Task GarantirMesasPadraoAsync(AppDbContext context)
        {
            var mesasExistentes = await context.Mesas.ToDictionaryAsync(m => m.Numero);
            var houveAlteracao = false;

            foreach (var seed in MesasPadrao)
            {
                if (!mesasExistentes.TryGetValue(seed.Numero, out var mesaExistente))
                {
                    context.Mesas.Add(new Mesa
                    {
                        Numero = seed.Numero,
                        Capacidade = seed.Capacidade,
                        Ativa = true
                    });

                    houveAlteracao = true;
                    continue;
                }

                if (mesaExistente.Capacidade != seed.Capacidade)
                {
                    mesaExistente.Capacidade = seed.Capacidade;
                    houveAlteracao = true;
                }

                if (!mesaExistente.Ativa)
                {
                    mesaExistente.Ativa = true;
                    houveAlteracao = true;
                }
            }

            if (houveAlteracao)
                await context.SaveChangesAsync();
        }

        private static async Task GarantirPedidosIniciaisAsync(AppDbContext context)
        {
            var pedidosSeed = PedidosSeedData.ItensPadrao;
            if (pedidosSeed.Count == 0)
                return;

            var emailsClientesSeed = pedidosSeed
                .Select(p => p.EmailCliente)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var clientes = await context.Users
                .AsNoTracking()
                .Where(u => u.Email != null && emailsClientesSeed.Contains(u.Email))
                .OrderBy(u => u.Email)
                .ToListAsync();

            if (clientes.Count == 0)
                return;

            var clientesPorEmail = clientes.ToDictionary(u => u.Email!, StringComparer.OrdinalIgnoreCase);
            if (!emailsClientesSeed.All(clientesPorEmail.ContainsKey))
                return;

            var idsClientes = clientes.Select(c => c.Id).ToList();

            var clientesComPedido = await context.Pedidos
                .AsNoTracking()
                .Where(p => idsClientes.Contains(p.UsuarioId))
                .Select(p => p.UsuarioId)
                .Distinct()
                .ToListAsync();

            var clientesComPedidoSet = clientesComPedido.ToHashSet(StringComparer.Ordinal);

            if (clientesComPedidoSet.Count == emailsClientesSeed.Count)
                return;

            var itensCardapioAtivos = await context.ItensCardapio
                .AsNoTracking()
                .Where(i => i.Ativo)
                .OrderBy(i => i.Id)
                .Select(i => new ItemCardapioAtivoSeed(i.Id, i.Periodo, i.PrecoBase))
                .ToListAsync();

            var itensPorPeriodo = itensCardapioAtivos
                .GroupBy(i => i.Periodo)
                .ToDictionary(g => g.Key, g => g.Select(i => new ItemPedidoSeed(i.ItemCardapioId, i.PrecoBase)).ToList());

            var mesasAtivas = await context.Mesas
                .AsNoTracking()
                .Where(m => m.Ativa)
                .OrderBy(m => m.Numero)
                .Select(m => m.Numero)
                .ToListAsync();

            if (mesasAtivas.Count == 0)
                return;

            var mesasAtivasSet = mesasAtivas.ToHashSet();
            var numeroMesaPadrao = mesasAtivas[0];

            var pedidosParaAdicionar = new List<Pedido>();
            var hoje = DateTime.Today;

            foreach (var pedidoSeed in pedidosSeed)
            {
                var cliente = clientesPorEmail[pedidoSeed.EmailCliente];
                if (clientesComPedidoSet.Contains(cliente.Id))
                    continue;

                if (!itensPorPeriodo.TryGetValue(pedidoSeed.Periodo, out var itensDoPeriodo))
                    continue;

                var itensPedido = new List<PedidoItemSeed>();
                var pedidoInvalido = false;

                foreach (var itemSeed in pedidoSeed.Itens)
                {
                    var indice = itemSeed.OrdemItemNoPeriodo - 1;
                    if (indice < 0 || indice >= itensDoPeriodo.Count)
                    {
                        pedidoInvalido = true;
                        break;
                    }

                    var itemCardapio = itensDoPeriodo[indice];
                    itensPedido.Add(new PedidoItemSeed(itemCardapio.ItemCardapioId, itemSeed.Quantidade, itemCardapio.PrecoBase));
                }

                if (pedidoInvalido || itensPedido.Count == 0)
                    continue;

                var dataHoraPedido = new DateTime(
                    hoje.Year,
                    hoje.Month,
                    hoje.Day,
                    pedidoSeed.Horario.Hour,
                    pedidoSeed.Horario.Minute,
                    pedidoSeed.Horario.Second);

                Atendimento atendimento;
                switch (pedidoSeed.TipoAtendimento)
                {
                    case TipoAtendimento.Presencial:
                        var numeroMesa = pedidoSeed.NumeroMesa ?? numeroMesaPadrao;
                        if (!mesasAtivasSet.Contains(numeroMesa))
                            continue;

                        atendimento = new AtendimentoPresencial
                        {
                            DataHora = dataHoraPedido,
                            NumerMesa = numeroMesa
                        };
                        break;

                    case TipoAtendimento.DeliveryProprio:
                        if (string.IsNullOrWhiteSpace(pedidoSeed.EnderecoEntrega))
                            continue;

                        atendimento = new AtendimentoDeliveryProprio
                        {
                            DataHora = dataHoraPedido,
                            TaxaEntrega = AtendimentoDeliveryProprio.TaxaFixaEntrega,
                            EnderecoEntrega = pedidoSeed.EnderecoEntrega
                        };
                        break;

                    case TipoAtendimento.DeliveryAplicativo:
                        if (string.IsNullOrWhiteSpace(pedidoSeed.NomeAplicativo)
                            || string.IsNullOrWhiteSpace(pedidoSeed.EnderecoEntrega))
                        {
                            continue;
                        }

                        atendimento = new AtendimentoDeliveryAplicativo
                        {
                            DataHora = dataHoraPedido,
                            NomeAplicativo = pedidoSeed.NomeAplicativo,
                            EnderecoEntrega = pedidoSeed.EnderecoEntrega
                        };
                        break;

                    default:
                        continue;
                }

                pedidosParaAdicionar.Add(CriarPedido(
                    cliente.Id,
                    dataHoraPedido,
                    atendimento,
                    itensPedido));

                clientesComPedidoSet.Add(cliente.Id);
            }

            if (pedidosParaAdicionar.Count == 0)
                return;

            context.Pedidos.AddRange(pedidosParaAdicionar);
            await context.SaveChangesAsync();
        }

        private static async Task GarantirEnderecosIniciaisAsync(AppDbContext context)
        {
            var enderecosSeed = EnderecosSeedData.ItensPadrao;
            if (enderecosSeed.Count == 0)
                return;

            var emailsClientesSeed = enderecosSeed
                .Select(e => e.EmailCliente)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var clientes = await context.Users
                .AsNoTracking()
                .Where(u => u.Email != null && emailsClientesSeed.Contains(u.Email))
                .Select(u => new { u.Id, Email = u.Email! })
                .ToListAsync();

            if (clientes.Count == 0)
                return;

            var clientesPorEmail = clientes.ToDictionary(u => u.Email, u => u.Id, StringComparer.OrdinalIgnoreCase);

            var idsClientes = clientes.Select(c => c.Id).ToList();
            var enderecosExistentes = await context.EnderecosEntrega
                .Where(e => idsClientes.Contains(e.UsuarioId))
                .ToListAsync();

            var houveAlteracao = false;

            foreach (var seed in enderecosSeed)
            {
                if (!clientesPorEmail.TryGetValue(seed.EmailCliente, out var usuarioId))
                    continue;

                var enderecoExiste = enderecosExistentes.Any(e =>
                    e.UsuarioId == usuarioId
                    && string.Equals(e.Logradouro, seed.Logradouro, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(e.Numero, seed.Numero, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(e.Bairro, seed.Bairro, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(e.Cidade, seed.Cidade, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(e.CEP, seed.CEP, StringComparison.OrdinalIgnoreCase));

                if (enderecoExiste)
                    continue;

                var novoEndereco = new EnderecoEntrega
                {
                    UsuarioId = usuarioId,
                    Logradouro = seed.Logradouro,
                    Numero = seed.Numero,
                    Bairro = seed.Bairro,
                    Cidade = seed.Cidade,
                    CEP = seed.CEP,
                    Complemento = seed.Complemento
                };

                context.EnderecosEntrega.Add(novoEndereco);
                enderecosExistentes.Add(novoEndereco);
                houveAlteracao = true;
            }

            if (houveAlteracao)
                await context.SaveChangesAsync();
        }

        private static Pedido CriarPedido(
            string usuarioId,
            DateTime dataHora,
            Atendimento atendimento,
            IReadOnlyList<PedidoItemSeed> itens)
        {
            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                DataHora = dataHora,
                Status = StatusPedido.Confirmado,
                Atendimento = atendimento
            };

            foreach (var item in itens)
            {
                pedido.PedidoItens.Add(new PedidoItem
                {
                    ItemCardapioId = item.ItemCardapioId,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario
                });
            }

            var subtotal = pedido.PedidoItens.Sum(i => i.PrecoFinal);
            var taxa = atendimento.CalcularTaxa(subtotal);
            pedido.ValorTotal = subtotal + taxa;

            return pedido;
        }

        private static async Task GarantirConfiguracaoReservaPadraoAsync(AppDbContext context)
        {
            await GarantirTabelaConfiguracaoReservaAsync(context);

            var configuracaoExistente = await context.ConfiguracoesHorarioReserva
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();
            if (configuracaoExistente is not null)
                return;

            context.ConfiguracoesHorarioReserva.Add(new ConfiguracaoHorarioReserva
            {
                HoraInicio = HorarioReservaPadraoInicio,
                HoraFim = HorarioReservaPadraoFim
            });

            await context.SaveChangesAsync();
        }

        private static async Task GarantirTabelaConfiguracaoReservaAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ConfiguracoesHorarioReserva]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [ConfiguracoesHorarioReserva] (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [HoraInicio] time NOT NULL,
                        [HoraFim] time NOT NULL,
                        CONSTRAINT [PK_ConfiguracoesHorarioReserva] PRIMARY KEY ([Id])
                    );
                END;
                """);
        }

        private static async Task GarantirTabelaMesasAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[Mesas]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Mesas] (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [Numero] int NOT NULL,
                        [Capacidade] int NOT NULL,
                        [Ativa] bit NOT NULL,
                        CONSTRAINT [PK_Mesas] PRIMARY KEY ([Id])
                    );

                    CREATE UNIQUE INDEX [IX_Mesas_Numero] ON [Mesas] ([Numero]);
                END;
                """);
        }

        private static void ValidarLimiteDoSeed()
        {
            var quantidadeAlmoco = CardapioSeedData.ItensPadrao.Count(i => i.Periodo == PeriodoRefeicao.Almoco);
            var quantidadeJantar = CardapioSeedData.ItensPadrao.Count(i => i.Periodo == PeriodoRefeicao.Jantar);

            if (quantidadeAlmoco != LimiteItensPorPeriodo || quantidadeJantar != LimiteItensPorPeriodo)
            {
                throw new InvalidOperationException(
                    $"Cardápio fixo inválido. O seed deve conter exatamente {LimiteItensPorPeriodo} itens de almoço e {LimiteItensPorPeriodo} itens de jantar.");
            }
        }

        private sealed record MesaSeedDefinition(int Numero, int Capacidade);
        private sealed record ItemCardapioAtivoSeed(int ItemCardapioId, PeriodoRefeicao Periodo, decimal PrecoBase);
        private sealed record ItemPedidoSeed(int ItemCardapioId, decimal PrecoBase);
        private sealed record PedidoItemSeed(int ItemCardapioId, int Quantidade, decimal PrecoUnitario);
    }
}
