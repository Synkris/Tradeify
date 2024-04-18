using Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DB
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<CommonDropdowns> CommonDropdowns { get; set; }
        public DbSet<Packages> Packages { get; set; }
        public DbSet<UserPackages> UserPackages { get; set; }

        public DbSet<PaymentForm> PaymentForms { get; set; }
		public DbSet<Cordinator> Cordinators { get; set; }
        public DbSet<Impersonation> Impersonations { get; set; }
        public DbSet<UserVerification> UserVerifications { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<PvWallet> PvWallets { get; set; }
        public DbSet<GrantWallet> GrantWallets { get; set; }
        public DbSet<AGCWallet> AGCWallets { get; set; }
        public DbSet<UserGrantHistory> UserGrantHistories { get; set; }
        public DbSet<AGCWalletHistory> AGCWalletHistories { get; set; }
        public DbSet<GrantWalletHistory> GrantWalletHistories { get; set; }
        public DbSet<PvWalletHistory> PvWalletHistories { get; set; }
        public DbSet<WalletHistory> WalletHistories { get; set; }
        //public DbSet<MatchingRequest> MatchingRequests { get; set; }
        public DbSet<News> News { get; set; }
    }
}
