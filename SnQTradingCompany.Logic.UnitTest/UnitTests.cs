using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnQTradingCompany.Contracts.Persistence.App;
using SnQTradingCompany.Contracts.Persistence.ItemMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnQTradingCompany.Logic.UnitTest
{
    [TestClass]
    public class PersistenceTest
    {
        [TestInitialize]
        public void CreateMockData()
        {

            Task.Run(async () =>
            {

                using var ctrl = Logic.Factory.Create<ICustomer>();

                var create1 = await ctrl.CreateAsync().ConfigureAwait(false);

                create1.Name = "Test1";
                create1.Number = "1";
                create1 = await ctrl.InsertAsync(create1).ConfigureAwait(false);

                var create2 = await ctrl.CreateAsync().ConfigureAwait(false);

                create2.Name = "Test2";
                create2.Number = "22";
                create2 = await ctrl.InsertAsync(create2).ConfigureAwait(false);

                await ctrl.SaveChangesAsync();

                using var ctrlProduct = Logic.Factory.Create<IProduct>();

                var product1 = await ctrlProduct.CreateAsync().ConfigureAwait(false);
                product1.Name = "A";
                product1.Number = "1";
                product1.Price = 10;

                product1 = await ctrlProduct.InsertAsync(product1).ConfigureAwait(false);

                var product2 = await ctrlProduct.CreateAsync().ConfigureAwait(false);
                product2.Name = "B";
                product2.Number = "22";
                product2.Price = 20;

                product2 = await ctrlProduct.InsertAsync(product2).ConfigureAwait(false);

                await ctrlProduct.SaveChangesAsync();

                using var ctrlCondition = Logic.Factory.Create<ICondition>();

                var condition = await ctrlCondition.CreateAsync().ConfigureAwait(false);
                condition.ConditionType = Contracts.Modules.Common.ConditionType.PieceDiscountRelative;
                condition.CustomerID = create1.Id;
                condition.ProductID = product1.Id;
                condition.Quantity = 5;
                condition.Value = 10;

                await ctrlCondition.InsertAsync(condition).ConfigureAwait(false);

                var condition2 = await ctrlCondition.CreateAsync().ConfigureAwait(false);
                condition2.ConditionType = Contracts.Modules.Common.ConditionType.PieceDiscountAbsolute;
                condition2.CustomerID = create1.Id;
                condition2.ProductID = product1.Id;
                condition2.Quantity = 10;
                condition2.Value = 200;

                await ctrlCondition.InsertAsync(condition2).ConfigureAwait(false);

                var condition3 = await ctrlCondition.CreateAsync().ConfigureAwait(false);
                condition3.ConditionType = Contracts.Modules.Common.ConditionType.ValueDiscountRelative;
                condition3.CustomerID = create1.Id;
                condition3.ProductID = product1.Id;
                condition3.Quantity = 3300;
                condition3.Value = 15;

                await ctrlCondition.InsertAsync(condition3).ConfigureAwait(false);

                var condition4 = await ctrlCondition.CreateAsync().ConfigureAwait(false);
                condition4.ConditionType = Contracts.Modules.Common.ConditionType.ValueDiscountAbsolute;
                condition4.CustomerID = create1.Id;
                condition4.ProductID = product1.Id;
                condition4.Quantity = 4500;
                condition4.Value = 500;

                await ctrlCondition.InsertAsync(condition4).ConfigureAwait(false);

                var condition5 = await ctrlCondition.CreateAsync().ConfigureAwait(false);
                condition5.ConditionType = Contracts.Modules.Common.ConditionType.PieceDiscountRelative;
                condition5.CustomerID = create2.Id;
                condition5.ProductID = product1.Id;
                condition5.Quantity = 4;
                condition5.Value = 25;

                await ctrlCondition.InsertAsync(condition5).ConfigureAwait(false);

                var condition6 = await ctrlCondition.CreateAsync().ConfigureAwait(false);
                condition6.ConditionType = Contracts.Modules.Common.ConditionType.PieceDiscountAbsolute;
                condition6.CustomerID = create2.Id;
                condition6.ProductID = product2.Id;
                condition6.Quantity = 4;
                condition6.Value = 10;

                await ctrlCondition.InsertAsync(condition6).ConfigureAwait(false);


                await ctrlCondition.SaveChangesAsync().ConfigureAwait(false);
            }).Wait();

        }

        [TestMethod]
        public void InsertOrder_LessProducts_NoCondition()
        {
            Task.Run(async () =>
            {
 
                using var ctrlC = Logic.Factory.Create<ICustomer>();

                var costumer = (await ctrlC.GetAllAsync().ConfigureAwait(false)).Where(item => item.Name == "Test1").FirstOrDefault();

                using var ctrlP = Logic.Factory.Create<IProduct>();

                var product = (await ctrlP.GetAllAsync().ConfigureAwait(false)).Where(item => item.Name == "A").FirstOrDefault();


                using var ctrl = Logic.Factory.Create<IOrder>();

                var newOrder = await ctrl.CreateAsync().ConfigureAwait(false);

                newOrder.CustomerId = costumer.Id;
                newOrder.ProductId = product.Id;
                newOrder.Count = 2;

                newOrder = await ctrl.InsertAsync(newOrder).ConfigureAwait(false);

                Assert.AreEqual(20, newOrder.PriceNet);
                Assert.AreEqual(0, newOrder.Discount);
            }).Wait();
        }

        [TestMethod]
        public void InsertOrderWithPieceDiscountRelative_OnlyCount5_InsertWith10PercentDiscount()
        {
            Task.Run(async () =>
            {

                using var ctrlC = Logic.Factory.Create<ICustomer>();

                var costumer = (await ctrlC.GetAllAsync().ConfigureAwait(false)).Where(item => item.Name == "Test1").FirstOrDefault();

                using var ctrlP = Logic.Factory.Create<IProduct>();

                var product = (await ctrlP.GetAllAsync().ConfigureAwait(false)).Where(item => item.Name == "A").FirstOrDefault();


                using var ctrl = Logic.Factory.Create<IOrder>();

                var newOrder = await ctrl.CreateAsync().ConfigureAwait(false);

                newOrder.CustomerId = costumer.Id;
                newOrder.ProductId = product.Id;
                newOrder.Count = 6;

                newOrder = await ctrl.InsertAsync(newOrder).ConfigureAwait(false);

                Assert.AreEqual(60, newOrder.PriceNet);
                Assert.AreEqual(6, newOrder.Discount);
            }).Wait();
        }

        [TestMethod]
        public void InsertOrderWithPieceDiscountRelative_SameProductDiffrentUser_QueryForRightCondition()
        {
            Task.Run(async () =>
            {

                using var ctrlC = Logic.Factory.Create<ICustomer>();

                var costumer = (await ctrlC.GetAllAsync().ConfigureAwait(false)).Where(item => item.Name == "Test2").FirstOrDefault();

                using var ctrlP = Logic.Factory.Create<IProduct>();

                var product = (await ctrlP.GetAllAsync().ConfigureAwait(false)).Where(item => item.Name == "A").FirstOrDefault();


                using var ctrl = Logic.Factory.Create<IOrder>();

                var newOrder = await ctrl.CreateAsync().ConfigureAwait(false);

                newOrder.CustomerId = costumer.Id;
                newOrder.ProductId = product.Id;
                newOrder.Count = 5;

                newOrder = await ctrl.InsertAsync(newOrder).ConfigureAwait(false);

                Assert.AreEqual(50, newOrder.PriceNet);
                Assert.AreEqual(12.5m, newOrder.Discount);
            }).Wait();
        }

        [TestMethod]
        public void InsertOrderWithPieceDiscountAbsolute__GetA10Discount()
        {
            Task.Run(async () =>
            {

                using var ctrlC = Logic.Factory.Create<ICustomer>();

                var costumer = (await ctrlC.GetAllAsync().ConfigureAwait(false)).Where(item => item.Name == "Test2").FirstOrDefault();

                using var ctrlP = Logic.Factory.Create<IProduct>();

                var product = (await ctrlP.GetAllAsync().ConfigureAwait(false)).Where(item => item.Name == "B").FirstOrDefault();


                using var ctrl = Logic.Factory.Create<IOrder>();

                var newOrder = await ctrl.CreateAsync().ConfigureAwait(false);

                newOrder.CustomerId = costumer.Id;
                newOrder.ProductId = product.Id;
                newOrder.Count = 6;

                newOrder = await ctrl.InsertAsync(newOrder).ConfigureAwait(false);

                Assert.AreEqual(120, newOrder.PriceNet);
                Assert.AreEqual(10, newOrder.Discount);
            }).Wait();
        }

        [TestMethod]
        public void InsertOrderWithPieceDiscountAbsolute_BetterConditionThenRelative_Get200Discount()
        {
            Task.Run(async () =>
            {

                using var ctrlC = Logic.Factory.Create<ICustomer>();

                var costumer = (await ctrlC.GetAllAsync().ConfigureAwait(false)).Where(item => item.Name == "Test1").FirstOrDefault();

                using var ctrlP = Logic.Factory.Create<IProduct>();

                var product = (await ctrlP.GetAllAsync().ConfigureAwait(false)).Where(item => item.Name == "A").FirstOrDefault();


                using var ctrl = Logic.Factory.Create<IOrder>();

                var newOrder = await ctrl.CreateAsync().ConfigureAwait(false);

                newOrder.CustomerId = costumer.Id;
                newOrder.ProductId = product.Id;
                newOrder.Count = 11;

                newOrder = await ctrl.InsertAsync(newOrder).ConfigureAwait(false);

                Assert.AreEqual(110, newOrder.PriceNet);
                Assert.AreEqual(200, newOrder.Discount);
            }).Wait();
        }

        [TestMethod]
        public void test()
        {

        }


        [TestCleanup]
        public void CleanUp()
        {
            Task.Run(async () =>
            {
                using var ctrlOrder = Logic.Factory.Create<IOrder>();

                var allOrders = await ctrlOrder.GetAllAsync().ConfigureAwait(false);

                foreach (var item in allOrders)
                {
                    await ctrlOrder.DeleteAsync(item.Id).ConfigureAwait(false);
                }

                await ctrlOrder.SaveChangesAsync().ConfigureAwait(false);

                using var ctrlCondition = Logic.Factory.Create<ICondition>();

                var allConditions = await ctrlCondition.GetAllAsync().ConfigureAwait(false);

                foreach (var item in allConditions)
                {
                    await ctrlCondition.DeleteAsync(item.Id).ConfigureAwait(false);
                }

                await ctrlCondition.SaveChangesAsync().ConfigureAwait(false);

                using var ctrlProduct = Logic.Factory.Create<IProduct>();

                var allProducts = await ctrlProduct.GetAllAsync().ConfigureAwait(false);

                foreach (var item in allProducts)
                {
                    await ctrlProduct.DeleteAsync(item.Id).ConfigureAwait(false);
                }

                await ctrlProduct.SaveChangesAsync().ConfigureAwait(false);

                using var ctrlCostumer = Logic.Factory.Create<ICustomer>();

                var allCostumer = await ctrlCostumer.GetAllAsync().ConfigureAwait(false);

                foreach (var item in allCostumer)
                {
                    await ctrlCostumer.DeleteAsync(item.Id).ConfigureAwait(false);
                }

                await ctrlCostumer.SaveChangesAsync().ConfigureAwait(false);
            }).Wait();

        }
    }
}
