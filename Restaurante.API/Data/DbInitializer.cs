using Restaurante.API.Models;

namespace Restaurante.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.ItensCardapio.Any())
                return;

            var pratosAlmoco = new[]
            {
                "Arroz, feijão, bife acebolado",
                "Galinhada caipira",
                "Frango caipira com angu",
                "Carne de panela com batata",
                "Costela bovina com mandioca",
                "Feijão tropeiro",
                "Linguiça artesanal com farofa",
                "Picadinho de carne com ovo",
                "Frango ensopado com legumes",
                "Arroz carreteiro tradicional",
                "Vaca atolada",
                "Tilápia frita com arroz e salada",
                "Carne de sol com macaxeira",
                "Frango à passarinho com arroz",
                "Bife à milanesa com feijão",
                "Almôndegas caseiras ao molho",
                "Escondidinho de carne seca",
                "Moqueca de peixe simples",
                "Carne moída com abóbora",
                "Feijão com costelinha suína"
            };

            var pratosJantar = new[]
            {
                "Caldo verde",
                "Sopa de legumes caseira",
                "Canja de galinha",
                "Escondidinho de carne seca",
                "Escondidinho de frango",
                "Macarrão caseiro ao molho",
                "Nhoque rústico ao molho",
                "Arroz com linguiça artesanal",
                "Omelete caipira",
                "Panqueca de carne caseira",
                "Pão com carne desfiada",
                "Pão com linguiça artesanal",
                "Caldo de feijão",
                "Caldo de mandioca com carne",
                "Cuscuz com carne de sol",
                "Tapioca recheada com frango",
                "Tábua de frios rústica",
                "Frango grelhado com legumes",
                "Purê com carne de panela",
                "Arroz com costelinha desfiada"
            };

            var itens = new List<ItemCardapio>();

            for (int i = 0; i < pratosAlmoco.Length; i++)
            {
                var nome = pratosAlmoco[i];
                itens.Add(new ItemCardapio
                {
                    Nome = nome,
                    Descricao = nome,
                    PrecoBase = 26 + i,
                    Periodo = PeriodoRefeicao.Almoco,
                    Ativo = true
                });
            }

            for (int i = 0; i < pratosJantar.Length; i++)
            {
                var nome = pratosJantar[i];
                itens.Add(new ItemCardapio
                {
                    Nome = nome,
                    Descricao = nome,
                    PrecoBase = 31 + i,
                    Periodo = PeriodoRefeicao.Jantar,
                    Ativo = true
                });
            }

            context.ItensCardapio.AddRange(itens);
            await context.SaveChangesAsync();
        }
    }
}  
