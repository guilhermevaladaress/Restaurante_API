using Microsoft.EntityFrameworkCore;
using Restaurante.API.Models;

namespace Restaurante.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var ingredientesExistentes = await context.Ingredientes.ToListAsync();
            var itensExistentes = await context.ItensCardapio.ToListAsync();
            var houveAlteracao = false;

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

            if (houveAlteracao)
                await context.SaveChangesAsync();
        }
    }
}
