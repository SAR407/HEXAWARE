using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
namespace TechShop_Final
{
    // Task 1–4: Core OOP Classes

    public class Customer
    {
        public int CustomerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public Customer(int id, string firstName, string lastName, string email, string phone, string address)
        {
            if (!email.Contains("@")) throw new InvalidDataException("Invalid email format.");
            CustomerID = id; FirstName = firstName; LastName = lastName; Email = email; Phone = phone; Address = address;
        }

        public void GetCustomerDetails()
        {
            Console.WriteLine($"Customer: {FirstName} {LastName}, Email: {Email}, Phone: {Phone}, Address: {Address}");
        }
        public void UpdateCustomerInfo(string email, string phone, string address)
        {
            if (!email.Contains("@")) throw new InvalidDataException("Invalid email.");
            Email = email; Phone = phone; Address = address;
        }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public Product(int id, string name, string description, decimal price)
        {
            if (price < 0) throw new InvalidDataException("Price cannot be negative.");
            ProductID = id; ProductName = name; Description = description; Price = price;
        }
        public void GetProductDetails()
        {
            Console.WriteLine($"Product: {ProductName}, Description: {Description}, Price: ₹{Price}");
        }

        public void UpdateProductInfo(string description, decimal price)
        {
            if (price < 0) throw new InvalidDataException("Price must be non-negative");
            Description = description; Price = price;
        }

        public bool IsProductInStock(Inventory inventory)
        {
            return inventory.QuantityInStock > 0;
        }
    }

    public class Order
    {
        public int OrderID { get; set; }
        public Customer Customer { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public string Status { get; private set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public Order(int id, Customer customer)
        {
            OrderID = id; Customer = customer;
        }

        public void GetOrderDetails()
        {
            Console.WriteLine($"Order ID: {OrderID} Date: {OrderDate} Total: ₹{TotalAmount}");
        }

        public void CalculateTotalAmount()
        {
            TotalAmount = OrderDetails.Sum(detail => detail.CalculateSubtotal());
        }

        public void UpdateOrderStatus(string status)
        {
            Status = status;
        }

        public void CancelOrder()
        {
            Status = "Cancelled";
            foreach (var detail in OrderDetails)
            {
                detail.Inventory.AddToInventory(detail.Quantity);
            }
        }
    }

    public class OrderDetail
    {
        public int OrderDetailID { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; } = 0;
        public Inventory Inventory { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();


        public OrderDetail(int id, Order order, Product product, int qty, Inventory inventory)
        {
            if (qty <= 0) throw new InvalidDataException("Quantity must be > 0");
            OrderDetailID = id; Order = order; Product = product; Quantity = qty; Inventory = inventory;
        }

        public decimal CalculateSubtotal()
        {
            return Product.Price * Quantity * (1 - Discount);
        }


        public void GetOrderDetailInfo()
        {
            Console.WriteLine($"OrderDetail ID: {OrderDetailID} Product: {Product.ProductName} Quantity: {Quantity} Subtotal: ₹{CalculateSubtotal()}");
        }

        public void UpdateQuantity(int qty)
        {
            if (qty <= 0) throw new InvalidDataException("Quantity must be > 0");
            Quantity = qty;
        }

        public void AddDiscount(decimal discount)
        {
            if (discount < 0 || discount > 1) throw new InvalidDataException("Discount must be between 0 and 1");
            Discount = discount;
        }
        

    }

    public class Inventory
    {
        public int InventoryID { get; set; }
        public Product Product { get; set; }
        public int QuantityInStock { get; set; }
        public DateTime LastStockUpdate { get; set; }

        public Inventory(int id, Product product, int quantity)
        {
            if (quantity < 0) throw new InvalidDataException("Stock must be >= 0");
            InventoryID = id; Product = product; QuantityInStock = quantity; LastStockUpdate = DateTime.Now;
        }

        public Product GetProduct() => Product;
        public int GetQuantityInStock() => QuantityInStock;

        public void AddToInventory(int quantity)
        {
            if (quantity < 0) throw new InvalidDataException("Invalid quantity to add");
            QuantityInStock += quantity;
            LastStockUpdate = DateTime.Now;
        }

        public void RemoveFromInventory(int quantity)
        {
            if (quantity > QuantityInStock) throw new InsufficientStockException("Not enough stock to remove");
            QuantityInStock -= quantity;
            LastStockUpdate = DateTime.Now;
        }

        public void UpdateStockQuantity(int newQuantity)
        {
            if (newQuantity < 0) throw new InvalidDataException("Invalid quantity");
            QuantityInStock = newQuantity;
            LastStockUpdate = DateTime.Now;
        }

        public bool IsProductAvailable(int quantityToCheck)
        {
            return QuantityInStock >= quantityToCheck;
        }

        public decimal GetInventoryValue()
        {
            return Product.Price * QuantityInStock;
        }

        public static List<Product> ListLowStockProducts(List<Inventory> inventories, int threshold)
        {
            return inventories.Where(i => i.QuantityInStock < threshold).Select(i => i.Product).ToList();
        }

        public static List<Product> ListOutOfStockProducts(List<Inventory> inventories)
        {
            return inventories.Where(i => i.QuantityInStock == 0).Select(i => i.Product).ToList();
        }
    }

    public class InvalidDataException : Exception { public InvalidDataException(string message) : base(message) { } }
    public class InsufficientStockException : Exception { public InsufficientStockException(string message) : base(message) { } }


    public class DatabaseConnector
    {
        private string connectionString = "Data Source=localhost;Initial Catalog=TechShop;Integrated Security=True";

        public void InsertCustomer(Customer customer)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Customers (Customer_id, Firstname, Lastname, Email, Phone, Address) VALUES (@CustomerID, @FirstName, @LastName, @Email, @Phone, @Address)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
                cmd.Parameters.AddWithValue("@FirstName", customer.FirstName);
                cmd.Parameters.AddWithValue("@LastName", customer.LastName);
                cmd.Parameters.AddWithValue("@Email", customer.Email);
                cmd.Parameters.AddWithValue("@Phone", customer.Phone);
                cmd.Parameters.AddWithValue("@Address", customer.Address);
                conn.Open(); cmd.ExecuteNonQuery();
            }
        }

        public void InsertProduct(Product product)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Products (Product_id, Product_name, Description, Price) VALUES (@ProductID, @ProductName, @Description, @Price)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProductID", product.ProductID);
                cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                cmd.Parameters.AddWithValue("@Description", product.Description);
                cmd.Parameters.AddWithValue("@Price", product.Price);
                conn.Open(); cmd.ExecuteNonQuery();
            }
        }

        public void InsertOrder(Order order)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Orders (Order_id, Customer_id, Order_date, Total_amount) VALUES (@OrderId, @CustId, @OrderDate, @TotalAmount)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderId", order.OrderID);
                cmd.Parameters.AddWithValue("@CustId", order.Customer.CustomerID);
                cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                conn.Open(); cmd.ExecuteNonQuery();
            }
        }

        public void InsertOrderDetail(OrderDetail detail)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO OrderDetails (Oredr_DetailID, Order_id, Product_id, Quantity) VALUES (@DetailID, @OrderID, @ProductID, @Quantity)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@DetailID", detail.OrderDetailID);
                cmd.Parameters.AddWithValue("@OrderID", detail.Order.OrderID);
                cmd.Parameters.AddWithValue("@ProductID", detail.Product.ProductID);
                cmd.Parameters.AddWithValue("@Quantity", detail.Quantity);
                conn.Open(); cmd.ExecuteNonQuery();
            }
        }

        public void InsertInventory(Inventory inventory)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Inventory (Inventory_id, Product_id, Quantity_Instock, Last_Stock_Update) VALUES (@InvID, @ProdID, @Qty, @LastUpdate)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@InvID", inventory.InventoryID);
                cmd.Parameters.AddWithValue("@ProdID", inventory.Product.ProductID);
                cmd.Parameters.AddWithValue("@Qty", inventory.QuantityInStock);
                cmd.Parameters.AddWithValue("@LastUpdate", inventory.LastStockUpdate);
                conn.Open(); cmd.ExecuteNonQuery();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Write("Enter Customer ID: "); int custId = int.Parse(Console.ReadLine());
                Console.Write("First Name: "); string fname = Console.ReadLine();
                Console.Write("Last Name: "); string lname = Console.ReadLine();
                Console.Write("Email: "); string email = Console.ReadLine();
                Console.Write("Phone: "); string phone = Console.ReadLine();
                Console.Write("Address: "); string address = Console.ReadLine();

                Customer customer = new Customer(custId, fname, lname, email, phone, address);

                Console.WriteLine("-----------------------------------------------------------");
                customer.GetCustomerDetails();
                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine("\nUpdating customer information...");
                Console.Write("New Email: ");
                string newEmail = Console.ReadLine();
                Console.Write("New Phone: ");
                string newPhone = Console.ReadLine();
                Console.Write("New Address: ");
                string newAddress = Console.ReadLine();

                customer.UpdateCustomerInfo(newEmail, newPhone, newAddress);
                Console.WriteLine("\n Updated customer info:");
                customer.GetCustomerDetails();
                Console.WriteLine("-----------------------------------------------------------");


                Console.Write("Enter Product ID: "); int prodId = int.Parse(Console.ReadLine());
                Console.Write("Product Name: "); string prodName = Console.ReadLine();
                Console.Write("Description: "); string prodDesc = Console.ReadLine();
                Console.Write("Price: "); decimal price = decimal.Parse(Console.ReadLine());

                Product product = new Product(prodId, prodName, prodDesc, price);
                Console.WriteLine("-----------------------------------------------------------");
                product.GetProductDetails();
                Console.WriteLine("-----------------------------------------------------------");
                Console.Write("Inventory ID: "); int invId = int.Parse(Console.ReadLine());
                Console.Write("Quantity in Stock: "); int stock = int.Parse(Console.ReadLine());
                Inventory inventory = new Inventory(invId, product, stock);
                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine("Is Product in Stock: " + product.IsProductInStock(inventory));
                Console.WriteLine("Inventory Value: ₹" + inventory.GetInventoryValue());
                Console.WriteLine("-----------------------------------------------------------");

                Console.Write("Order ID: "); int orderId = int.Parse(Console.ReadLine());
                Order order = new Order(orderId, customer);

                Console.Write("Order Detail ID: "); int detailId = int.Parse(Console.ReadLine());
                Console.Write("Order Quantity: "); int qty = int.Parse(Console.ReadLine());
                OrderDetail detail = new OrderDetail(detailId, order, product, qty,inventory);
                Console.WriteLine("-----------------------------------------------------------");

                detail.GetOrderDetailInfo();
                detail.UpdateQuantity(qty + 1);
                detail.AddDiscount(0.1m);
                Console.WriteLine("Subtotal after discount: ₹" + detail.CalculateSubtotal());

                order.CalculateTotalAmount();
                order.GetOrderDetails();
                order.UpdateOrderStatus("Processing");
                Console.WriteLine("Order Status Updated.");

                inventory.RemoveFromInventory(qty);
                inventory.AddToInventory(5);
                inventory.UpdateStockQuantity(stock);

                List<Inventory> inventoryList = new List<Inventory> { inventory };
                Console.WriteLine("Low Stock Products:");
                foreach (var p in Inventory.ListLowStockProducts(inventoryList, 10))
                    Console.WriteLine(p.ProductName);

                Console.WriteLine("Out of Stock Products:");
                foreach (var p in Inventory.ListOutOfStockProducts(inventoryList))
                    Console.WriteLine(p.ProductName);

                
               

                DatabaseConnector db = new DatabaseConnector();
                db.InsertCustomer(customer);
                db.InsertProduct(product);
                db.InsertInventory(inventory);
                db.InsertOrder(order);
                db.InsertOrderDetail(detail);

                Console.WriteLine("All data inserted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}

