﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EStore.Domain.Entities;
using System.Linq;
using Moq;
using EStore.Domain.Abstract;
using EStore.WebUI.Controllers;
using EStore.WebUI.Models;
using System.Web.Mvc;

namespace EStore.UnitTests
{
    [TestClass]
    public class CartTests
    {
        [TestMethod]
        public void Can_Add_New_Lines()
        {
            //Arrange - Create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };

            Cart cart = new Cart();

            //Act
            cart.AddItem(p1, 1);
            cart.AddItem(p2, 1);
            CartLine[] results = cart.Lines.ToArray();

            //Assert
            Assert.AreEqual(results.Length, 2);
            Assert.AreEqual(results[0].Product, p1);
            Assert.AreEqual(results[1].Product, p2);
        }

        [TestMethod]
        public void Can_Add_Quantity_For_Existing_Lines()
        {
            //Arrange - create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };

            Cart cart = new Cart();

            //Act
            cart.AddItem(p1, 1);
            cart.AddItem(p2, 1);
            cart.AddItem(p1, 9);

            CartLine[] results = cart.Lines.OrderBy(p => p.Product.ProductID).ToArray();

            //Assert
            Assert.AreEqual(results.Length, 2);
            Assert.AreEqual(results[0].Quantity, 10);
            Assert.AreEqual(results[1].Quantity, 1);
        }

        [TestMethod]
        public void Can_Remove_Line()
        {
            //Arrange - Create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };
            Product p3 = new Product { ProductID = 3, Name = "P3" };

            Cart cart = new Cart();

            cart.AddItem(p1, 1);
            cart.AddItem(p2, 3);
            cart.AddItem(p3, 5);
            cart.AddItem(p2, 1);

            //Act
            cart.RemoveLine(p2);

            //Assert
            Assert.AreEqual(cart.Lines.Count(), 2);
            Assert.AreEqual(cart.Lines.Where(p => p.Product == p2).Count(), 0);
        }

        [TestMethod]
        public void Calculate_Cart_Total()
        {
            //Arrange - create sample products
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };

            Cart cart = new Cart();

            //Act
            cart.AddItem(p1, 1);
            cart.AddItem(p2, 1);
            cart.AddItem(p1, 3);

            decimal result = cart.ComputeTotalValue();

            //Assert
            Assert.AreEqual(result, 450M);
        }

        [TestMethod]
        public void Can_Clear_Contents()
        {
            //Arrange - create sample products
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };

            Cart cart = new Cart();

            cart.AddItem(p1, 1);
            cart.AddItem(p2, 1);
            
            //Act
            cart.Clear();

            //Assert
            Assert.AreEqual(cart.Lines.Count(), 0);
        }

        [TestMethod]
        public void Can_Add_To_Cart()
        {
            //Arrange - create the mock repository
            Mock<IProductsRepository> mock = new Mock<IProductsRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]{
                new Product {ProductID = 1, Name = "P1", Category = "Apples"},
            }.AsQueryable());

            //Arrange - create a cart
            Cart cart = new Cart();

            //Arrange - create the controller
            CartController target = new CartController(mock.Object, null);

            //Act - add a product to the cart
            target.AddToCart(cart, 1, null);

            //Assert
            Assert.AreEqual(cart.Lines.Count(), 1);
            Assert.AreEqual(cart.Lines.ToArray()[0].Product.ProductID, 1);
        }

        [TestMethod]
        public void Adding_Product_To_Cart_Goes_To_Cart_Screen()
        {
            //Arrange - creat the mock repository
            Mock<IProductsRepository> mock = new Mock<IProductsRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product {ProductID = 1, Name = "P1", Category = "Apples"},
            }.AsQueryable());

            //Arrange - create a Cart
            Cart cart = new Cart();

            //Arrange - create the controller
            CartController target = new CartController(mock.Object, null);

            //Act - add a product to the cart
            RedirectToRouteResult result = target.AddToCart(cart, 2, "myUrl");

            //Assert 
            Assert.AreEqual(result.RouteValues["action"], "Index");
            Assert.AreEqual(result.RouteValues["returnUrl"], "myUrl");
        }

        [TestMethod]
        public void Can_View_Cart_Contents()
        {
            //Arrange - create a Cart
            Cart cart = new Cart();

            //Arrange - create the controller
            CartController target = new CartController(null, null);

            //Act - call the Index action method
            CartIndexViewModel result = (CartIndexViewModel)target.Index(cart, "myUrl").ViewData.Model;

            //Assert 
            Assert.AreSame(result.Cart, cart);
            Assert.AreEqual(result.ReturnUrl, "myUrl");
        }
    }
}
