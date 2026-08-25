    using Common.Contracts.authentication;
    using Common.Contracts.inventory;
    using Common.Utilities;
    using Microsoft.EntityFrameworkCore;
    using Module.Sales.Application.Abstraction;
    using Module.Sales.Application.UseCases.Registers.Close;
    using Module.Sales.Application.UseCases.Registers.GetById;
    using Module.Sales.Domain;
    using Moq;

    namespace Test.Sales;

    public class ClosureDetailTests
    {
        private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid BranchId = Guid.NewGuid();
        private static readonly Guid UserId = Guid.NewGuid();

        [Fact]
        public async Task ExecuteCurrent_ShouldExcludeSoftDeletedMovements_AndNotAffectTotals()
        {
            var dbName = $"ClosureTest_{Guid.NewGuid()}";
            var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);

            // 1. Open closure
            using (var db = TestSalesDbContextFactory.Create(tenantCtx, dbName))
            {
                var closure = CashRegisterClosure.Open(BranchId, 1000m, UserId, "Test User");
                db.CashRegisterClosures.Add(closure);
                await db.SaveChangesAsync();

                var saleCash = CreateSale(closure.Id, PaymentMethod.Cash, 500m);
                var saleQr = CreateSale(closure.Id, PaymentMethod.QrCode, 300m);
                db.Sales.AddRange(saleCash, saleQr);
                await db.SaveChangesAsync();

                var outflowKeep = CashRegisterMovement.Create(closure.Id, 100m, "Gasto valido", CashRegisterMovementType.Outflow, UserId, "Test User");
                var outflowDeleted = CashRegisterMovement.Create(closure.Id, 999m, "Gasto eliminado", CashRegisterMovementType.Outflow, UserId, "Test User");
                var inflow = CashRegisterMovement.Create(closure.Id, 50m, "Ingreso", CashRegisterMovementType.Inflow, UserId, "Test User");
                db.CashRegisterMovements.AddRange(outflowKeep, outflowDeleted, inflow);
                await db.SaveChangesAsync();

                outflowDeleted.DeletedAt = DateTime.UtcNow;
                outflowDeleted.DeletedBy = UserId;
                await db.SaveChangesAsync();
            }

            // 2. Query current details with fresh context (simulates new request scope, query filters apply)
            using (var db = TestSalesDbContextFactory.Create(tenantCtx, dbName))
            {
                var inventoryMock = new Mock<IInventoryIntegrationService>();
                inventoryMock.Setup(s => s.GetVariantsWithStock(It.IsAny<List<Guid>>(), It.IsAny<Guid>()))
                    .ReturnsAsync(new List<ProductVariantStockDto>());

                var sut = new GetClosureDetail(db, inventoryMock.Object);
                var result = await sut.ExecuteCurrent(CreateActorContext());

                Assert.True(result.IsSuccess, result.Error?.Message);
                var dto = result.Value;

                Assert.Equal(2, dto.Movements.Count);
                Assert.DoesNotContain(dto.Movements, m => m.Description == "Gasto eliminado");
                Assert.Equal(100m, dto.TotalExpenses);
                Assert.Equal(800m, dto.TotalSales);
                Assert.Equal(500m, dto.CashSales);
                Assert.Equal(2, dto.Sales.Count);
            }
        }

        [Fact]
        public async Task Close_ShouldExcludeSoftDeletedMovements_FromSystemSalesAmount()
        {
            var dbName = $"ClosureTest_{Guid.NewGuid()}";
            var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
            Guid closureId;

            using (var db = TestSalesDbContextFactory.Create(tenantCtx, dbName))
            {
                var closure = CashRegisterClosure.Open(BranchId, 1000m, UserId, "Test User");
                closureId = closure.Id;
                db.CashRegisterClosures.Add(closure);
                await db.SaveChangesAsync();

                var saleCash = CreateSale(closure.Id, PaymentMethod.Cash, 400m);
                db.Sales.Add(saleCash);
                await db.SaveChangesAsync();

                var outflowKeep = CashRegisterMovement.Create(closure.Id, 80m, "Gasto keep", CashRegisterMovementType.Outflow, UserId, "Test User");
                var outflowDeleted = CashRegisterMovement.Create(closure.Id, 500m, "Gasto deleted", CashRegisterMovementType.Outflow, UserId, "Test User");
                db.CashRegisterMovements.AddRange(outflowKeep, outflowDeleted);
                await db.SaveChangesAsync();

                outflowDeleted.DeletedAt = DateTime.UtcNow;
                outflowDeleted.DeletedBy = UserId;
                await db.SaveChangesAsync();
            }

            // Close with fresh context
            using (var db = TestSalesDbContextFactory.Create(tenantCtx, dbName))
            {
                var directCount = await db.CashRegisterMovements.CountAsync();
                Assert.Equal(1, directCount);

                var closeSut = new CloseCashRegister(db);
                var closeResult = await closeSut.Execute(CreateActorContext(), new CloseCashRegisterDto { RealCountedAmount = 1320m });

                Assert.True(closeResult.IsSuccess, closeResult.Error?.Message);
                Assert.Equal(1320m, closeResult.Value.ExpectedCash);
                Assert.Equal(80m, closeResult.Value.OutflowsTotal);
                Assert.Equal(0m, closeResult.Value.Difference);
            }

            // Verify via GetClosureDetail after close also excludes deleted
            using (var db = TestSalesDbContextFactory.Create(tenantCtx, dbName))
            {
                var inventoryMock = new Mock<IInventoryIntegrationService>();
                inventoryMock.Setup(s => s.GetVariantsWithStock(It.IsAny<List<Guid>>(), It.IsAny<Guid>()))
                    .ReturnsAsync(new List<ProductVariantStockDto>());
                var detailSut = new GetClosureDetail(db, inventoryMock.Object);
                var detail = await detailSut.Execute(CreateActorContext(), closureId);
                Assert.True(detail.IsSuccess);
                Assert.Equal(80m, detail.Value.TotalExpenses);
                Assert.Single(detail.Value.Movements);
            }
        }

        private static ActorContext CreateActorContext()
            => new(TenantId, UserId, "Test User", BranchId, [BranchId]);

        private static Sale CreateSale(Guid closureId, PaymentMethod payment, decimal total)
        {
            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                BranchId = BranchId,
                SoldById = UserId,
                SoldByName = "Test User",
                CreatedBy = UserId,
                CreatedByName = "Test User",
                CashRegisterClosureId = closureId,
                PaymentMethod = payment,
                DocumentType = DocumentType.Ticket,
                Type = SaleType.Sale,
                TotalAmount = total,
                TenantId = TenantId,
                CreatedAt = DateTime.UtcNow,
                SaleItems =
                [
                    new SaleItem
                    {
                        ProductVariantId = Guid.NewGuid(),
                        ProductSku = "SKU-TEST",
                        ProductDisplayName = "Test Product",
                        Quantity = 1,
                        UnitPrice = total,
                        UnitCost = 10m,
                        DiscountAmount = 0,
                        FinalPrice = total,
                        TenantId = TenantId
                    }
                ]
            };
            return sale;
        }
    }
