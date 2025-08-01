using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KwendaMoney.Models;
using KwendaMoney.Data;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace KwendaMoney.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SolicitacaoDeposito> SolicitacoesDeposito { get; set; }
        public DbSet<ContaAdmin> ContasAdmin { get; set; }
        public DbSet<Saque> Saques { get; set; }
        public DbSet<CarteiraInvestimento> CarteirasInvestimento { get; set; }
        public DbSet<PacoteInvestimento> PacotesInvestimento { get; set; }

        public DbSet<RelatorioFinanceiroSistema> RelatorioFinanceiroSistema { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Pacotes de investimento
            modelBuilder.Entity<PacoteInvestimento>().HasData(
                new PacoteInvestimento { Id = 1, Nome = "Básico", Valor = 1000, DiasMaximos = 10, LucroDiario = 100 },
                new PacoteInvestimento { Id = 2, Nome = "Prata", Valor = 3000, DiasMaximos = 15, LucroDiario = 200 },
                new PacoteInvestimento { Id = 3, Nome = "Ouro", Valor = 5000, DiasMaximos = 20, LucroDiario = 250 },
                new PacoteInvestimento { Id = 4, Nome = "Platina", Valor = 10000, DiasMaximos = 30, LucroDiario = 350 },
                new PacoteInvestimento { Id = 5, Nome = "Diamante", Valor = 20000, DiasMaximos = 30, LucroDiario = 670 },
                new PacoteInvestimento { Id = 6, Nome = "Titânio", Valor = 30000, DiasMaximos = 30, LucroDiario = 1000 }
            );

            // Seed do relatório financeiro inicial
            modelBuilder.Entity<RelatorioFinanceiroSistema>().HasData(
                new RelatorioFinanceiroSistema
                {
                    Id = 1,
                    LucroTaxaSaqueCarteiraGeral = 0,
                    LucroTaxaSaqueCarteiraInvestimento = 0,
                    LucroAviator = 0,
                    LucroGeral = 0,
                    TotalCarteirasGeraisUsuarios = 0,
                    TotalInvestidoCarteirasInvestimento = 0,
                    TotalLucroCarteirasInvestimento = 0,
                    TotalGeralCarteiraInvestimento = 0
                });
        }
    }
}