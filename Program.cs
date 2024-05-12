using NLog;
using NorthWind_Console.Model;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "\\nlog.config";

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

try
{
    var db = new NWContext();
    string choice;
    do
    {
        Console.WriteLine("1) Display Categories");
        Console.WriteLine("2) Add Category");
        Console.WriteLine("3) Display Category and related products");
        Console.WriteLine("4) Display all Categories and their related products");
        Console.WriteLine("5) Add new record to Products table");
        Console.WriteLine("6) Edit a Product");
        Console.WriteLine("7) Display Products");
        Console.WriteLine("8) Display Product details");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info($"Option {choice} selected");
        if (choice == "1")
        {
            var query = db.Categories.OrderBy(p => p.CategoryName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (choice == "2")
        {
            Category category = new Category();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    db.AddCategory(category);
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
        else if (choice == "3")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"\t{p.ProductName}");
            }
        }
        else if (choice == "4")
        {
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
        }
        else if (choice == "5")
        {
            Product product = new Product();

            // TODO - format error messages

            bool invalidEntry = true;
            while (invalidEntry)
            {
                Console.WriteLine("Enter Product Name:");
                var userEntry = Console.ReadLine();
                if (userEntry != null && userEntry.Length > 0)
                {
                    product.ProductName = userEntry;
                    invalidEntry = false;
                }
            }

            invalidEntry = true;
            while (invalidEntry)
            {
                Console.WriteLine("Enter the supplier ID:");
                var supplierQuery = db.Suppliers.OrderBy(s => s.SupplierId);
                foreach (var item in supplierQuery)
                {
                    Console.WriteLine(item.SupplierId + ") " + item.CompanyName);
                }
                var userEntry = Console.ReadLine();

                if (int.TryParse(userEntry, out int supplierID))
                {
                    if (db.Suppliers.Any(s => s.SupplierId.Equals(supplierID)))
                    {
                        product.SupplierId = supplierID;
                        invalidEntry = false;
                    }
                    else
                    {
                        logger.Error("There are no Suppliers with that Id");
                    }
                }
                else
                {
                    logger.Error("Invalid Supplier Id");
                }
            }

            invalidEntry = true;
            while (invalidEntry)
            {
                Console.WriteLine("Enter the category ID:");
                var categoryQuery = db.Categories.OrderBy(c => c.CategoryId);
                foreach (var item in categoryQuery)
                {
                    Console.WriteLine(item.CategoryId + ") " + item.CategoryName);
                }
                var userEntry = Console.ReadLine();

                if (int.TryParse(userEntry, out int categoryID))
                {
                    if (db.Categories.Any(c => c.CategoryId.Equals(categoryID)))
                    {
                        product.CategoryId = categoryID;
                        invalidEntry = false;
                    }
                    else
                    {
                        logger.Error("There are no Categories with that Id");
                    }
                }
                else
                {
                    logger.Error("Invalid Category Id");
                }

            }

            invalidEntry = true;
            while (invalidEntry)
            {
                Console.WriteLine("Enter quantity per unit:");
                var userEntry = Console.ReadLine();
                if (userEntry != null && userEntry.Length > 0)
                {
                    product.QuantityPerUnit = userEntry;
                    invalidEntry = false;
                }
            }

            invalidEntry = true;
            while (invalidEntry)
            {
                Console.WriteLine("Enter unit price:");
                var userEntry = Console.ReadLine();
                if (decimal.TryParse(userEntry, out decimal unitPrice))
                {
                    if (unitPrice >= 0)
                    {
                        product.UnitPrice = unitPrice;
                        invalidEntry = false;
                    }
                    else
                    {
                        logger.Error("Price cannot be negative");
                    }
                }
                else
                {
                    logger.Error("Price must be a decimal");
                }
            }

            invalidEntry = true;
            while (invalidEntry)
            {
                Console.WriteLine("Enter units in stock:");
                var userEntry = Console.ReadLine();
                if (short.TryParse(userEntry, out short unitsInStock))
                {
                    if (unitsInStock >= 0)
                    {
                        product.UnitsInStock = unitsInStock;
                        invalidEntry = false;
                    }
                    else
                    {
                        logger.Error("Units in stock cannot be negative");
                    }
                }
                else
                {
                    logger.Error("Units must be an integer");
                }
            }

            invalidEntry = true;
            while (invalidEntry)
            {
                Console.WriteLine("Enter units on order:");
                var userEntry = Console.ReadLine();
                if (short.TryParse(userEntry, out short unitsOnOrder))
                {
                    if (unitsOnOrder >= 0)
                    {
                        product.UnitsOnOrder = unitsOnOrder;
                        invalidEntry = false;
                    }
                    else
                    {
                        logger.Error("Units on order cannot be negative");
                    }
                }
                else
                {
                    logger.Error("Units must be an integer");
                }
            }

            invalidEntry = true;
            while (invalidEntry)
            {
                Console.WriteLine("What is the reorder level:");
                var userEntry = Console.ReadLine();
                if (short.TryParse(userEntry, out short reorderLevel))
                {
                    if (reorderLevel >= 0)
                    {
                        product.ReorderLevel = reorderLevel;
                        invalidEntry = false;
                    }
                    else
                    {
                        logger.Error("Reorder level cannot be negative.");
                    }
                }
                else
                {
                    logger.Error("Units must be an integer");
                }
            }

            invalidEntry = true;
            while (invalidEntry)
            {
                Console.WriteLine("Has this product been discontinued (y/n)?");
                var userEntry = Console.ReadLine();
                if (userEntry.ToLower() == "y")
                {
                    product.Discontinued = true;
                    invalidEntry = false;
                }
                else if (userEntry.ToLower() == "n")
                {
                    product.Discontinued = false;
                    invalidEntry = false;
                }
                else
                {
                    logger.Error("Discontinued status must be entered as either 'y' or 'n'.");
                }
            }

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Products.Any(p => p.ProductName == product.ProductName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    db.AddProduct(product);
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
        else if (choice == "6")
        {
            // TODO: better validation

            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose product you want to edit:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            Console.WriteLine("Select a product number");
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"\t{p.ProductId} - {p.ProductName}");
            }

            id = int.Parse(Console.ReadLine());
            logger.Info($"ProductId {id} selected");
            Product product = db.Products.FirstOrDefault(p => p.ProductId == id);
            Console.WriteLine($"Product Name: {product.ProductName}");
            Console.WriteLine($"Supplier ID: {product.SupplierId}");
            Console.WriteLine($"Category ID: {product.CategoryId}");
            Console.WriteLine($"Quantity per Unit: {product.QuantityPerUnit}");
            Console.WriteLine($"Unit Price: {product.UnitPrice}");
            Console.WriteLine($"Units in Stock: {product.UnitsInStock}");
            Console.WriteLine($"Units on Order: {product.UnitsOnOrder}");
            Console.WriteLine($"Reorder Level: {product.ReorderLevel}");
            Console.WriteLine($"Discontinued Status: {product.Discontinued}");

            if (product != null)
            {
                Product updatedProduct = new Product();
                Console.WriteLine("Enter the new product name.");
                updatedProduct.ProductName = Console.ReadLine();
                Console.WriteLine("Enter the new supplier ID.");
                updatedProduct.SupplierId = int.Parse(Console.ReadLine());
                Console.WriteLine("Enter the new category ID.");
                updatedProduct.CategoryId = int.Parse(Console.ReadLine());
                Console.WriteLine("Enter the new quantity per unit.");
                updatedProduct.QuantityPerUnit = Console.ReadLine();
                Console.WriteLine("Enter the new unit price.");
                updatedProduct.UnitPrice = decimal.Parse(Console.ReadLine());
                Console.WriteLine("Enter the new units in stock.");
                updatedProduct.UnitsInStock = short.Parse(Console.ReadLine());
                Console.WriteLine("Enter the new units on order.");
                updatedProduct.UnitsOnOrder = short.Parse(Console.ReadLine());
                Console.WriteLine("Enter the new reorder level.");
                updatedProduct.ReorderLevel = short.Parse(Console.ReadLine());
                Console.WriteLine("Enter the new discontinued status (y/n) or.");
                string discontinued = Console.ReadLine();
                if (discontinued.ToLower() == "y")
                {
                    updatedProduct.Discontinued = true;
                }
                else if (discontinued.ToLower() == "n")
                {
                    updatedProduct.Discontinued = false;
                }

                ValidationContext context = new ValidationContext(updatedProduct, null, null);
                List<ValidationResult> results = new List<ValidationResult>();

                var isValid = Validator.TryValidateObject(updatedProduct, context, results, true);
                if (isValid)
                {
                    // check for unique name
                    if (db.Products.Any(p => p.ProductName == updatedProduct.ProductName))
                    {
                        // generate validation error
                        isValid = false;
                        results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                    }
                    else
                    {
                        logger.Info("Validation passed");
                        updatedProduct.ProductId = product.ProductId;
                        db.EditProduct(updatedProduct);
                        logger.Info($"Product (id: {product.ProductId}) updated");
                    }
                }
                if (!isValid)
                {
                    foreach (var result in results)
                    {
                        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                    }
                }
            }
            else
            {
                logger.Error("Invalid Product Id");
            }
        }
        else if (choice == "7")
        {
            string productDisplayChoice;
            do
            {
                Console.WriteLine("Which products would you like to view.");
                Console.WriteLine("1) All Products");
                Console.WriteLine("2) Discontinued Products");
                Console.WriteLine("3) Active Products");
                Console.WriteLine("\"q\" to quit");
                productDisplayChoice = Console.ReadLine();

                if (productDisplayChoice == "1")
                {
                    var query = db.Products.OrderBy(p => p.ProductId);

                    Console.WriteLine($"{query.Count()} records returned");
                    Console.WriteLine("Please note discontinued products appear in red.");

                    foreach (var item in query)
                    {
                        if (item.Discontinued == true)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        else if (item.Discontinued == false)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        Console.WriteLine($"{item.ProductName}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (productDisplayChoice == "2")
                {
                    var query = db.Products.Where(p => p.Discontinued == true);
                    Console.WriteLine("Discontinued Items");
                    foreach (var item in query)
                    {
                        Console.WriteLine($"{item.ProductName}");
                    }
                }
                else if (productDisplayChoice == "3")
                {
                    var query = db.Products.Where(p => p.Discontinued == false);
                    Console.WriteLine("Active Items");
                    foreach (var item in query)
                    {
                        Console.WriteLine($"{item.ProductName}");
                    }
                }

            } while (productDisplayChoice.ToLower() != "q");
        }
        else if (choice == "8")
        {
            Console.WriteLine("Which Product Id would you like to know more abouit?");
            var query = db.Products.OrderBy(p => p.ProductId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.ProductId}) {item.ProductName}");
            }

            int id = int.Parse(Console.ReadLine());
            logger.Info($"ProductId {id} selected");
            Product product = db.Products.FirstOrDefault(p => p.ProductId == id);
            Console.WriteLine("Product Name: " + product.ProductName);
            Console.WriteLine("Supplier ID: " + product.SupplierId);
            Supplier supplier = db.Suppliers.FirstOrDefault(s => s.SupplierId == product.SupplierId);
            Console.WriteLine("Supplier: " + supplier.CompanyName);
            Console.WriteLine("Category ID: " + product.CategoryId);
            Category category = db.Categories.FirstOrDefault(c => c.CategoryId == product.CategoryId);
            Console.WriteLine("Category: " + category.CategoryName);
            Console.WriteLine("Quantity per unit: " + product.QuantityPerUnit);
            Console.WriteLine("Unit price: " + product.UnitPrice);
            Console.WriteLine("Units in stock: " + product.UnitsInStock);
            Console.WriteLine("Units on order: " + product.UnitsOnOrder);
            Console.WriteLine("Reorder level: " + product.ReorderLevel);
            Console.WriteLine("Discontinued status: " + product.Discontinued);
        }
        Console.WriteLine();

    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");
