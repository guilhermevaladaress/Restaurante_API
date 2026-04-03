using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Restaurante.API.Models;

namespace Restaurante.API.Data
{
    public class AppDbContext : IdentityDbContext<Usuario>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        // Cada DbSet vira uma tabela no banco
        public DbSet<EnderecoEntrega> EnderecosEntrega { get; set; }
        public DbSet<ItemCardapio> ItensCardapio { get; set; }
        public DbSet<Ingrediente> Ingredientes { get; set; }
        public DbSet<ItemCardapioIngrediente> ItemCardapioIngredientes { get; set; }
        public DbSet<SugestaoChefe> SugestoesChefe { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoItem> PedidoItens { get; set; }
        public DbSet<Atendimento> Atendimentos { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Chave composta da tabela intermediária ItemCardapio x Ingrediente
            modelBuilder.Entity<ItemCardapioIngrediente>()
                .HasKey(ii => new { ii.ItemCardapioId, ii.IngredienteId });

            // Chave composta da tabela intermediária Pedido x ItemCardapio
            modelBuilder.Entity<PedidoItem>()
                .HasKey(pi => new { pi.PedidoId, pi.ItemCardapioId });

            // Herança: todas as subclasses de Atendimento ficam na mesma tabela
            // Uma coluna "Tipo" diferencia cada registro
            modelBuilder.Entity<Atendimento>()
                .HasDiscriminator<TipoAtendimento>("Tipo")
                .HasValue<AtendimentoPresencial>(TipoAtendimento.Presencial)
                .HasValue<AtendimentoDeliveryProprio>(TipoAtendimento.DeliveryProprio)
                .HasValue<AtendimentoDeliveryAplicativo>(TipoAtendimento.DeliveryAplicativo);

            // Precisão dos campos decimais para evitar aviso do EF
            modelBuilder.Entity<ItemCardapio>()
                .Property(i => i.PrecoBase)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PedidoItem>()
                .Property(pi => pi.PrecoUnitario)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PedidoItem>()
                .Property(pi => pi.PercentualDesconto)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Pedido>()
                .Property(p => p.ValorTotal)
                .HasPrecision(10, 2);

            modelBuilder.Entity<AtendimentoDeliveryProprio>()
                .Property(a => a.TaxaEntrega)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SugestaoChefe>()
                .Property(s => s.PercentualDesconto)
                .HasPrecision(5, 2);

            modelBuilder.Entity<SugestaoChefe>()
                .HasIndex(s => new { s.Data, s.Periodo })
                .IsUnique();
        }

    }
}
