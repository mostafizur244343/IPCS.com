
using IPCS_Model.Entities;
using IPCS_Model.Entities.Permissions;
using IPCS_Model.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;


namespace IPCS_Repo.Data
{
    public class IPCSDBContext : IdentityDbContext<User>
    {
        public IPCSDBContext(DbContextOptions<IPCSDBContext> options) : base(options)
        {

        }


        public DbSet<Category> Categories { get; set; }
        public DbSet<UOM> UOMs { get; set; }
        public DbSet<GlobalUnitConversion> GlobalUnitConversions { get; set; }

        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<GenericInfo> GenericInfos { get; set; }
        public DbSet<StoreLocation> StoreLocations { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<LotInfo> LotInfos { get; set; }
        public DbSet<BranchLotStock> BranchLotStocks { get; set; }
        public DbSet<StockLedger> StockLedgers { get; set; }
        public DbSet<DailyTransactionSummary> DailyTransactionSummaries { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ProductUnitConversion> ProductUnitConversions { get; set; }
        public DbSet<PurchaseMaster> PurchaseMasters { get; set; }
        public DbSet<PurchaseDetails> PurchaseDetails { get; set; }
        public DbSet<TransferRequisition> TransferRequisitions { get; set; }
        public DbSet<TransferRequisitionDetails> TransferRequisitionDetails { get; set; }
        public DbSet<TransferMaster> TransferMasters { get; set; }
        public DbSet<TransferDetails> TransferDetails { get; set; }
        public DbSet<InvoicePayment> InvoicePayments { get; set; }
        public DbSet<SalesMaster> SalesMasters { get; set; }
        public DbSet<SalesDetails> SalesDetails { get; set; }
        public DbSet<PurchaseReturnMaster> PurchaseReturnMasters { get; set; }
        public DbSet<PurchaseReturnDetails> PurchaseReturnDetails { get; set; }
        public DbSet<SalesReturnMaster> SalesReturnMasters { get; set; }
        public DbSet<SalesReturnDetails> SalesDetailsReturn { get; set; } // Avoid collision name if any

        // Permission System
        public DbSet<AppModule> AppModules { get; set; }
        public DbSet<AppPermission> AppPermissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }





        protected override void OnModelCreating( ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            //It's the logic to turn off Pluralization in Migratrions on Database
            foreach (var entity in builder.Model.GetEntityTypes())
            {

                //its the main logic which keep the table name in database as same to model
                entity.SetTableName(entity.DisplayName());
            }
            //

            // Disable Cascade Delete Globally
            var cascadeFKs = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // if Delete Product Then Conversion Rules also Deleted
builder.Entity<ProductUnitConversion>()
    .HasOne(c => c.Product)
    .WithMany(p => p.UnitConversions)
    .HasForeignKey(c => c.ProductId)
    .OnDelete(DeleteBehavior.Cascade); 

            // Multiple Cascade Path Solving: On Deleting Product Auto Delete (Restrict)
            builder.Entity<StockLedger>()
                .Property(p => p.CurrentBalance)
                .HasComputedColumnSql("[PreviousBalance] + [QuantityIn] - [QuantityOut]");

            // Cascade Delete off (Restrict)
            builder.Entity<StockLedger>()
                .HasOne(s => s.Product)
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StockLedger>()
                .HasOne(s => s.Lot)
                .WithMany()
                .HasForeignKey(s => s.LotId)
                .OnDelete(DeleteBehavior.Restrict);

            // For ClosingCash SQL Formul
            builder.Entity<DailyTransactionSummary>()
                .Property(d => d.ClosingCash)
                .HasComputedColumnSql("([OpeningBalance] + [TotalCashSales] + [TotalCollection]) - ([TotalPurchase] + [TotalExpense] + [TotalDiscount])");

            // 1. Filtered Unique Index: Mobile Number Be Unique Just Fpr Unique Supplier
            builder.Entity<Supplier>()
                 .HasIndex(s => s.Mobile)
                 .IsUnique()
                 .HasFilter("[IsDeleted] = 0");

            // 2. On SupplierName & Mobile Number Non-Unique Index For Fast Searching
            builder.Entity<Supplier>().HasIndex(s => s.SupplierName);

            // 3. Global Query Filter: Defaultly Deleted Supplier Not in Query
            builder.Entity<Supplier>().HasQueryFilter(s => !s.IsDeleted);





            // 1. On CustomrName & Mobile Number Non-Unique Index For Fast Searching
            builder.Entity<Customer>().HasIndex(c => c.CustomerName);
            builder.Entity<Customer>().HasIndex(c => c.Mobile);

            // 2. Filtered Unique Index: Mobile Number Be Unique Just Fpr Unique Supplier
            builder.Entity<Customer>()
            .HasIndex(c => c.Mobile)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

            // 3. Global Query Filter: Defaultly Deleted Supplier Not in Query
            builder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);



            // 1. Filtered Unique Index on ProductName
            builder.Entity<Product>()
                .HasIndex(p => p.ProductName)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
            //3. 

            builder.Entity<ProductUnitConversion>()
                .HasOne(c => c.Product)
                .WithMany(p => p.UnitConversions)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // If Delete any Product also delete conversion Rules

            // 2.  Global Filter for Soft Delete
            builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);

            // Purchase Configurations
            builder.Entity<PurchaseMaster>().HasQueryFilter(p => !p.IsDeleted);
            
            // Soft delete trigger or cascade off
            builder.Entity<PurchaseDetails>()
                .HasOne(pd => pd.PurchaseMaster)
                .WithMany(pm => pm.PurchaseDetails)
                .HasForeignKey(pd => pd.PurchaseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseDetails>()
                .HasOne(pd => pd.Product)
                .WithMany()
                .HasForeignKey(pd => pd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseDetails>()
                .HasOne(pd => pd.UOM)
                .WithMany()
                .HasForeignKey(pd => pd.UOMId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexing for performance
            builder.Entity<PurchaseMaster>().HasIndex(p => p.PurchaseCode);
            builder.Entity<PurchaseMaster>().HasIndex(p => p.ShipmentStatus);

            // Transfer Configurations
            builder.Entity<TransferMaster>().HasQueryFilter(t => !t.IsDeleted);
            builder.Entity<TransferRequisition>().HasQueryFilter(t => !t.IsDeleted);
            
            builder.Entity<TransferMaster>().HasIndex(t => t.TransferCode);
            builder.Entity<TransferRequisition>().HasIndex(t => t.RequisitionCode);

            builder.Entity<InvoicePayment>()
                .HasOne(p => p.Purchase)
                .WithMany()
                .HasForeignKey(p => p.PurchaseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InvoicePayment>()
                .HasOne(p => p.PaymentMethod)
                .WithMany()
                .HasForeignKey(p => p.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sales Configurations
            builder.Entity<SalesMaster>().HasQueryFilter(s => !s.IsDeleted);
            builder.Entity<SalesMaster>().HasIndex(s => s.InvoiceNo).IsUnique();

            builder.Entity<SalesDetails>()
                .HasOne(sd => sd.SalesMaster)
                .WithMany(sm => sm.SalesDetails)
                .HasForeignKey(sd => sd.SalesId)
                .OnDelete(DeleteBehavior.Cascade); // Delete Details if Master is deleted

            builder.Entity<SalesDetails>()
                .HasOne(sd => sd.Product)
                .WithMany()
                .HasForeignKey(sd => sd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InvoicePayment>()
                .HasOne(p => p.Sale)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SaleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Purchase Return Configurations
            builder.Entity<PurchaseReturnMaster>().HasQueryFilter(r => !r.IsDeleted);
            builder.Entity<PurchaseReturnMaster>().HasIndex(r => r.ReturnNo).IsUnique();

            builder.Entity<PurchaseReturnDetails>()
                .HasOne(rd => rd.ReturnMaster)
                .WithMany(rm => rm.ReturnDetails)
                .HasForeignKey(rd => rd.ReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PurchaseReturnDetails>()
                .HasOne(rd => rd.Product)
                .WithMany()
                .HasForeignKey(rd => rd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseReturnDetails>()
                .HasOne(rd => rd.Lot)
                .WithMany()
                .HasForeignKey(rd => rd.LotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sales Return Configurations
            builder.Entity<SalesReturnMaster>().HasQueryFilter(r => !r.IsDeleted);
            builder.Entity<SalesReturnMaster>().HasIndex(r => r.ReturnNo).IsUnique();

            builder.Entity<SalesReturnDetails>()
                .HasOne(rd => rd.ReturnMaster)
                .WithMany(rm => rm.ReturnDetails)
                .HasForeignKey(rd => rd.ReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SalesReturnDetails>()
                .HasOne(rd => rd.Product)
                .WithMany()
                .HasForeignKey(rd => rd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesReturnDetails>()
                .HasOne(rd => rd.Lot)
                .WithMany()
                .HasForeignKey(rd => rd.LotId)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}