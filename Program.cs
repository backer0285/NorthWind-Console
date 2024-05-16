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

// TODO: better formatting
// TODO: refactor redundant coce, separate into methods

try
{
    var db = new NWContext();
    string choice;
    do
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("1) Display Categories");
        Console.WriteLine("2) Add Category");
        Console.WriteLine("3) Display Category and related products");
        Console.WriteLine("4) Display all Categories and their related products");
        Console.WriteLine("5) Add new record to Products table");
        Console.WriteLine("6) Edit a Product");
        Console.WriteLine("7) Display Products");
        Console.WriteLine("8) Display Product details");
        Console.WriteLine("9) Edit a Category");
        Console.WriteLine("10) Display all Categories and their related active products");
        Console.WriteLine("11) Display Category and its related active products");
        Console.WriteLine("12) Delete a product record");
        Console.WriteLine("13) Delete a category record");
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

            bool invalidEntry = true;
            while (invalidEntry)
            {
                Console.WriteLine("Enter Category Name:");
                var userEntry = Console.ReadLine();
                if (userEntry != null && userEntry.Length > 0)
                {
                    category.CategoryName = userEntry;
                    invalidEntry = false;
                }
            }

            invalidEntry = true;
            while (invalidEntry)
            {
                Console.WriteLine("Enter Category Description:");
                var userEntry = Console.ReadLine();
                if (userEntry != null && userEntry.Length > 0)
                {
                    category.Description = userEntry;
                    invalidEntry = false;
                }
            }

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

            var userEntry = Console.ReadLine();
            if (int.TryParse(userEntry, out int id))
            {
                if (db.Categories.Any(s => s.CategoryId.Equals(id)))
                {
                    Console.Clear();
                    logger.Info($"CategoryId {id} selected");
                    Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                    Console.WriteLine($"{category.CategoryName} - {category.Description}");
                    foreach (Product p in category.Products)
                    {
                        Console.WriteLine($"\t{p.ProductName}");
                    }
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
            var query = db.Categories.OrderBy(c => c.CategoryId);

            Console.WriteLine("Select the category whose product you want to edit:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;


            var userEntry = Console.ReadLine();
            if (int.TryParse(userEntry, out int id))
            {
                if (db.Categories.Any(s => s.CategoryId.Equals(id)))
                {
                    Console.Clear();
                    logger.Info($"CategoryId {id} selected");
                    Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                    Console.WriteLine($"{category.CategoryName} - {category.Description}");
                    Console.WriteLine("Select a product number");
                    foreach (Product p in category.Products)
                    {
                        Console.WriteLine($"\t{p.ProductId} - {p.ProductName}");
                    }

                    userEntry = Console.ReadLine();
                    if (int.TryParse(userEntry, out int productId))
                    {
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

                            bool invalidEntry = true;
                            while (invalidEntry)
                            {
                                Console.WriteLine("Enter Product Name:");
                                var entry = Console.ReadLine();
                                if (userEntry != null && entry.Length > 0)
                                {
                                    updatedProduct.ProductName = entry;
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
                                var entry = Console.ReadLine();

                                if (int.TryParse(entry, out int supplierID))
                                {
                                    if (db.Suppliers.Any(s => s.SupplierId.Equals(supplierID)))
                                    {
                                        updatedProduct.SupplierId = supplierID;
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
                                var entry = Console.ReadLine();

                                if (int.TryParse(entry, out int categoryID))
                                {
                                    if (db.Categories.Any(c => c.CategoryId.Equals(categoryID)))
                                    {
                                        updatedProduct.CategoryId = categoryID;
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
                                var entry = Console.ReadLine();
                                if (entry != null && userEntry.Length > 0)
                                {
                                    updatedProduct.QuantityPerUnit = userEntry;
                                    invalidEntry = false;
                                }
                            }

                            invalidEntry = true;
                            while (invalidEntry)
                            {
                                Console.WriteLine("Enter unit price:");
                                var entry = Console.ReadLine();
                                if (decimal.TryParse(entry, out decimal unitPrice))
                                {
                                    if (unitPrice >= 0)
                                    {
                                        updatedProduct.UnitPrice = unitPrice;
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
                                var entry = Console.ReadLine();
                                if (short.TryParse(entry, out short unitsInStock))
                                {
                                    if (unitsInStock >= 0)
                                    {
                                        updatedProduct.UnitsInStock = unitsInStock;
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
                                var entry = Console.ReadLine();
                                if (short.TryParse(entry, out short unitsOnOrder))
                                {
                                    if (unitsOnOrder >= 0)
                                    {
                                        updatedProduct.UnitsOnOrder = unitsOnOrder;
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
                                var entry = Console.ReadLine();
                                if (short.TryParse(entry, out short reorderLevel))
                                {
                                    if (reorderLevel >= 0)
                                    {
                                        updatedProduct.ReorderLevel = reorderLevel;
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
                                var entry = Console.ReadLine();
                                if (userEntry.ToLower() == "y")
                                {
                                    updatedProduct.Discontinued = true;
                                    invalidEntry = false;
                                }
                                else if (entry.ToLower() == "n")
                                {
                                    updatedProduct.Discontinued = false;
                                    invalidEntry = false;
                                }
                                else
                                {
                                    logger.Error("Discontinued status must be entered as either 'y' or 'n'.");
                                }
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
            Console.WriteLine("Which Product Id would you like to know more about?");
            var query = db.Products.OrderBy(p => p.ProductId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.ProductId}) {item.ProductName}");
            }

            var userEntry = Console.ReadLine();
            if (int.TryParse(userEntry, out int id))
            {
                if (db.Products.Any(p => p.ProductId.Equals(id)))
                {
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
                else
                {
                    logger.Error("There are no Products with that Id");
                }
            }
            else
            {
                logger.Error("Invalid Product Id");
            }
        }
        else if (choice == "9")
        {
            var query = db.Categories.OrderBy(c => c.CategoryId);

            Console.WriteLine("Select the category you want to edit:");
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }

            var userEntry = Console.ReadLine();
            if (int.TryParse(userEntry, out int id))
            {
                if (db.Categories.Any(c => c.CategoryId.Equals(id)))
                {
                    Console.Clear();
                    logger.Info($"CategoryId {id} selected");
                    Category category = db.Categories.FirstOrDefault(c => c.CategoryId == id);
                    Console.WriteLine($"{category.CategoryName} - {category.Description}");

                    if (category != null)
                    {
                        Category updatedCategory = new Category();

                        bool invalidEntry = true;
                        while (invalidEntry)
                        {
                            Console.WriteLine("Enter Category Name:");
                            var entry = Console.ReadLine();
                            if (entry != null && entry.Length > 0)
                            {
                                updatedCategory.CategoryName = entry;
                                invalidEntry = false;
                            }
                        }

                        invalidEntry = true;
                        while (invalidEntry)
                        {
                            Console.WriteLine("Enter Category Description:");
                            var entry = Console.ReadLine();
                            if (entry != null && entry.Length > 0)
                            {
                                updatedCategory.Description = entry;
                                invalidEntry = false;
                            }
                        }

                        ValidationContext context = new ValidationContext(updatedCategory, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(updatedCategory, context, results, true);
                        if (isValid)
                        {
                            // check for unique name
                            if (db.Categories.Any(c => c.CategoryName == updatedCategory.CategoryName))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                updatedCategory.CategoryId = category.CategoryId;
                                db.EditCategory(updatedCategory);
                                logger.Info($"Category (id: {category.CategoryId}) updated");
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
                        logger.Error("Invalid Category Id");
                    }
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
        else if (choice == "10")
        {
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products)
                {
                    if (p.Discontinued == false)
                    {
                        Console.WriteLine($"\t{p.ProductName}");
                    }
                }
            }
        }
        else if (choice == "11")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose active products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;

            var userEntry = Console.ReadLine();
            if (int.TryParse(userEntry, out int id))
            {
                if (db.Categories.Any(s => s.CategoryId.Equals(id)))
                {
                    Console.Clear();
                    logger.Info($"CategoryId {id} selected");
                    Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                    Console.WriteLine($"{category.CategoryName} - {category.Description}");
                    foreach (Product p in category.Products)
                    {
                        if (p.Discontinued == false)
                        {
                            Console.WriteLine($"\t{p.ProductName}");
                        }
                    }
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
        else if (choice == "12")
        {
            Console.WriteLine("Which Product Id would you like to delete (please note only products that do not show on historic orders are eligible for deletion");
            var query = db.Products.OrderBy(p => p.ProductId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.ProductId}) {item.ProductName}");
            }
            var userEntry = Console.ReadLine();

            Product product = new Product();
            if (int.TryParse(userEntry, out int id))
            {
                if (db.Products.Any(p => p.ProductId.Equals(id)))
                {
                    logger.Info($"ProductId {id} selected");
                    product = db.Products.FirstOrDefault(p => p.ProductId == id);

                    if (!db.OrderDetails.Any(od => od.ProductId == product.ProductId))
                    {
                        logger.Info($"ProductId {id} deleted");
                        db.DeleteProduct(product);
                    }
                    else
                    {
                        logger.Error("This product exists in historic records and cannot be deleted for data integrity purposes");
                    }
                }
                else
                {
                    logger.Error("There are no Products with that Id");
                }
            }
            else
            {
                logger.Error("Invalid Product Id");
            }
        }
        else if (choice == "13")
        {
            Console.WriteLine("Which Category Id would you like to delete (please note only categories unassociated with still existing products are eligible for deletion.");
            var query = db.Categories.OrderBy(c => c.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            var userEntry = Console.ReadLine();

            Category category = new Category();
            if (int.TryParse(userEntry, out int id))
            {
                if (db.Categories.Any(c => c.CategoryId.Equals(id)))
                {
                    logger.Info($"CategoryId {id} selected");
                    category = db.Categories.FirstOrDefault(c => c.CategoryId == id);

                    if (!db.Products.Any(p => p.CategoryId == category.CategoryId))
                    {
                        logger.Info($"CategoryId {id} deleted");
                        db.DeleteCategory(category);
                    }
                    else
                    {
                        logger.Error("This category exists in historic records and cannot be deleted for data integrity purposes");
                    }
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
            Console.WriteLine();

        } while (choice.ToLower() != "q") ;
    }
catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");
