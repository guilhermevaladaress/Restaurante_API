using Restaurante.API.Models;

namespace Restaurante.API.Data
{
    public static class CardapioSeedData
    {
        public static IReadOnlyList<ItemCardapioSeedDefinition> ItensPadrao { get; } =
            new List<ItemCardapioSeedDefinition>
            {
                new("Arroz, feijão, bife acebolado", "Prato caseiro com arroz branco, feijão temperado, bife acebolado e cebolas douradas.", 26m, PeriodoRefeicao.Almoco),
                new("Galinhada caipira", "Arroz cozido no caldo da galinha caipira, com temperos frescos e toque de açafrão.", 27m, PeriodoRefeicao.Almoco),
                new("Frango caipira com angu", "Frango cozido lentamente com molho encorpado, servido com angu cremoso.", 28m, PeriodoRefeicao.Almoco),
                new("Carne de panela com batata", "Carne macia cozida na pressão com batatas, cenoura e caldo bem temperado.", 29m, PeriodoRefeicao.Almoco),
                new("Costela bovina com mandioca", "Costela bovina desmanchando, acompanhada de mandioca cozida e cheiro-verde.", 30m, PeriodoRefeicao.Almoco),
                new("Feijão tropeiro", "Feijão tropeiro com farinha, ovos mexidos, couve e pedaços de linguiça.", 31m, PeriodoRefeicao.Almoco),
                new("Linguiça artesanal com farofa", "Linguiça artesanal grelhada, servida com farofa crocante e arroz soltinho.", 32m, PeriodoRefeicao.Almoco),
                new("Picadinho de carne com ovo", "Cubos de carne ao molho com ovo frito, arroz branco e feijão fresco.", 33m, PeriodoRefeicao.Almoco),
                new("Frango ensopado com legumes", "Frango ensopado com legumes da estação e caldo caseiro bem aromatizado.", 34m, PeriodoRefeicao.Almoco),
                new("Arroz carreteiro tradicional", "Arroz carreteiro com carne desfiada, pimentões e tempero rústico.", 35m, PeriodoRefeicao.Almoco),
                new("Vaca atolada", "Costela bovina cozida com mandioca em caldo grosso e bem temperado.", 36m, PeriodoRefeicao.Almoco),
                new("Tilápia frita com arroz e salada", "Filé de tilápia dourado, arroz branco e salada fresca da casa.", 37m, PeriodoRefeicao.Almoco),
                new("Carne de sol com macaxeira", "Carne de sol salteada com macaxeira macia e manteiga de garrafa.", 38m, PeriodoRefeicao.Almoco),
                new("Frango à passarinho com arroz", "Pedaços de frango crocantes, temperados com alho, servidos com arroz e limão.", 39m, PeriodoRefeicao.Almoco),
                new("Bife à milanesa com feijão", "Bife empanado e crocante, com feijão fresquinho e arroz caseiro.", 40m, PeriodoRefeicao.Almoco),
                new("Almôndegas caseiras ao molho", "Almôndegas macias em molho de tomate da casa, com arroz branco.", 41m, PeriodoRefeicao.Almoco),
                new("Escondidinho de carne seca", "Purê cremoso de mandioca com recheio de carne seca desfiada e gratinada.", 42m, PeriodoRefeicao.Almoco),
                new("Moqueca de peixe simples", "Peixe cozido em molho leve com tomate, pimentão e leite de coco.", 43m, PeriodoRefeicao.Almoco),
                new("Carne moída com abóbora", "Carne moída refogada com abóbora macia, cebola e cheiro-verde.", 44m, PeriodoRefeicao.Almoco),
                new("Feijão com costelinha suína", "Feijão encorpado com costelinha suína cozida lentamente e arroz branco.", 45m, PeriodoRefeicao.Almoco),
                new("Caldo verde", "Caldo verde cremoso com batata, couve fininha e rodelas de linguiça.", 31m, PeriodoRefeicao.Jantar),
                new("Sopa de legumes caseira", "Sopa leve com legumes variados, ervas frescas e caldo caseiro.", 32m, PeriodoRefeicao.Jantar),
                new("Canja de galinha", "Canja reconfortante com frango desfiado, arroz e legumes picados.", 33m, PeriodoRefeicao.Jantar),
                new("Escondidinho de carne seca", "Purê de mandioca gratinado com carne seca refogada e queijo dourado.", 34m, PeriodoRefeicao.Jantar),
                new("Escondidinho de frango", "Camadas de purê cremoso com frango desfiado e requeijão da casa.", 35m, PeriodoRefeicao.Jantar),
                new("Macarrão caseiro ao molho", "Macarrão caseiro servido com molho de tomate artesanal e ervas.", 36m, PeriodoRefeicao.Jantar),
                new("Nhoque rústico ao molho", "Nhoque rústico macio com molho vermelho encorpado e parmesão.", 37m, PeriodoRefeicao.Jantar),
                new("Arroz com linguiça artesanal", "Arroz cremoso com linguiça artesanal salteada e cebola caramelizada.", 38m, PeriodoRefeicao.Jantar),
                new("Omelete caipira", "Omelete farta com queijo, tomate, cebola e ervas frescas.", 39m, PeriodoRefeicao.Jantar),
                new("Panqueca de carne caseira", "Panqueca recheada com carne moída ao molho e toque de queijo.", 40m, PeriodoRefeicao.Jantar),
                new("Pão com carne desfiada", "Pão macio recheado com carne desfiada suculenta e molho especial.", 41m, PeriodoRefeicao.Jantar),
                new("Pão com linguiça artesanal", "Pão tostado com linguiça artesanal grelhada e vinagrete fresco.", 42m, PeriodoRefeicao.Jantar),
                new("Caldo de feijão", "Caldo de feijão cremoso com torresmo crocante e cheiro-verde.", 43m, PeriodoRefeicao.Jantar),
                new("Caldo de mandioca com carne", "Caldo espesso de mandioca com carne desfiada e tempero caseiro.", 44m, PeriodoRefeicao.Jantar),
                new("Cuscuz com carne de sol", "Cuscuz macio servido com carne de sol desfiada e manteiga.", 45m, PeriodoRefeicao.Jantar),
                new("Tapioca recheada com frango", "Tapioca leve recheada com frango cremoso e ervas frescas.", 46m, PeriodoRefeicao.Jantar),
                new("Tábua de frios rústica", "Seleção rústica de frios, pães e acompanhamentos para compartilhar.", 47m, PeriodoRefeicao.Jantar),
                new("Frango grelhado com legumes", "Frango grelhado servido com legumes salteados e molho leve.", 48m, PeriodoRefeicao.Jantar),
                new("Purê com carne de panela", "Purê de batata aveludado com carne de panela desmanchando.", 49m, PeriodoRefeicao.Jantar),
                new("Arroz com costelinha desfiada", "Arroz soltinho com costelinha suína desfiada e temperos da casa.", 50m, PeriodoRefeicao.Jantar)
            };
    }

    public sealed record ItemCardapioSeedDefinition(
        string Nome,
        string Descricao,
        decimal PrecoBase,
        PeriodoRefeicao Periodo);
}
