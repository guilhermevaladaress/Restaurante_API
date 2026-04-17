using System.Globalization;
using System.Text;

namespace Restaurante.API.Data
{
    public static class CardapioIngredientesSeedData
    {
        private static readonly IReadOnlyList<RegraIngrediente> Regras =
            new List<RegraIngrediente>
            {
                new("Abóbora", "abobora"),
                new("Açafrão", "acafrao"),
                new("Alho", "alho"),
                new("Angu", "angu"),
                new("Arroz", "arroz"),
                new("Batata", "batata"),
                new("Carne", "carne", "bife", "almondega", "picadinho"),
                new("Carne de Sol", "carne de sol"),
                new("Carne Seca", "carne seca"),
                new("Cebola", "cebola"),
                new("Cenoura", "cenoura"),
                new("Cheiro-verde", "cheiro-verde", "cheiro verde"),
                new("Costela Bovina", "costela bovina", "vaca atolada"),
                new("Costelinha Suína", "costelinha suina", "costelinha"),
                new("Couve", "couve"),
                new("Cuscuz", "cuscuz"),
                new("Farinha", "farinha", "tropeiro", "farofa"),
                new("Feijão", "feijao"),
                new("Filé de Tilápia", "tilapia"),
                new("Frango", "frango", "canja", "galinha"),
                new("Galinha Caipira", "galinha caipira", "galinhada"),
                new("Leite de Coco", "leite de coco"),
                new("Legumes", "legumes"),
                new("Limão", "limao"),
                new("Linguiça", "linguica"),
                new("Macarrão", "macarrao"),
                new("Macaxeira", "macaxeira"),
                new("Mandioca", "mandioca", "escondidinho"),
                new("Manteiga de Garrafa", "manteiga de garrafa"),
                new("Molho de Tomate", "molho de tomate", "molho vermelho", "molho"),
                new("Nhoque", "nhoque"),
                new("Ovo", "ovo", "omelete", "ovos"),
                new("Pão", "pao"),
                new("Parmesão", "parmesao"),
                new("Pimentão", "pimentao", "pimentoes"),
                new("Queijo", "queijo", "gratinado"),
                new("Requeijão", "requeijao"),
                new("Salada", "salada"),
                new("Tomate", "tomate", "vinagrete"),
                new("Torresmo", "torresmo")
            };

        public static IReadOnlyList<string> ObterIngredientes(ItemCardapioSeedDefinition item)
        {
            var textoNormalizado = Normalizar($"{item.Nome} {item.Descricao}");
            var ingredientes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var regra in Regras)
            {
                if (regra.PalavrasChave.Any(textoNormalizado.Contains))
                    ingredientes.Add(regra.Ingrediente);
            }

            return ingredientes
                .OrderBy(nome => nome)
                .ToList();
        }

        private static string Normalizar(string valor)
        {
            var texto = valor.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(texto.Length);

            foreach (var c in texto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(char.ToLowerInvariant(c));
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private sealed class RegraIngrediente
        {
            public RegraIngrediente(string ingrediente, params string[] palavrasChave)
            {
                Ingrediente = ingrediente;
                PalavrasChave = palavrasChave
                    .Select(p => p.Trim())
                    .Where(p => p.Length > 0)
                    .ToList();
            }

            public string Ingrediente { get; }
            public IReadOnlyList<string> PalavrasChave { get; }
        }
    }
}
