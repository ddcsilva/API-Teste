using Microsoft.EntityFrameworkCore;
using PersonManagement.Domain.Entities;

namespace PersonManagement.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Pessoa> Pessoas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Pessoa
        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Sobrenome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Documento)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.DataNascimento)
                .IsRequired();

            entity.Property(e => e.EstaAtivo)
                .IsRequired();

            entity.Property(e => e.DataCriacao)
                .IsRequired();

            entity.Property(e => e.DataAtualizacao);

            // Índices
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Documento).IsUnique();
        });
    }
}