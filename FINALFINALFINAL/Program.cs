using System.Xml.Linq;
using Spectre.Console;

abstract class LivestockSupply
{
    public string Name { get; set; }

    public LivestockSupply(string name)
    {
        Name = name;
    }

    public abstract void Display();
}

// Supplier inheriting from LivestockSupply
class Supplier : LivestockSupply
{
    private string ContactNumber { get; set; }

    public Supplier(string name, string contactNumber)
        : base(name)
    {
        ContactNumber = contactNumber;
    }

    public override void Display()
    {
        Console.WriteLine($"Supplier Name: {Name}, Contact: {ContactNumber}");
    }

    public override string ToString()
    {
        return $"{Name},{ContactNumber}";
    }
}

// InventoryItem inheriting from LivestockSupply
class InventoryItem : LivestockSupply
{
    public int Quantity { get; private set; }
    public string SupplierName { get; set; }
    public decimal Price { get; set; }

    public InventoryItem(string name, int quantity, decimal price, string supplierName) : base(name)
    {
        Quantity = quantity;
        Price = price;
        SupplierName = supplierName;
    }
    public override void Display()
    {
        Console.WriteLine($"Item Name: {Name}, Quantity: {Quantity}, Price: {Price:C}, Supplier: {SupplierName}");
    }
    public override string ToString()
    {
        return $"{Name},{Quantity},{Price},{SupplierName}";
    }
    public void UpdateQuantity(int quantityChange)
    {
        Quantity += quantityChange;
        if (Quantity < 0)
        {
            Quantity = 0;
            Console.WriteLine("Warning: Quantity cannot be less than zero. It has been set to 0.");
        }
    }
}
class Sales
{
    public string ItemName { get; set; }
    public int QuantitySold { get; set; }
    public double Price { get; set; }
    public DateTime DateOfSale { get; set; }

    public Sales(string itemName, int quantitySold, double price, DateTime dateOfSale)
    {
        ItemName = itemName;
        QuantitySold = quantitySold;
        Price = price;
        DateOfSale = dateOfSale;
    }

    public double TotalSale()
    {
        return QuantitySold * Price;
    }

    public new string ToString()
    {
        return $"{ItemName},{QuantitySold},{Price},{DateOfSale:yyyy-MM-dd}";
    }
}

public class SalesRecord
{
    public int InventoryItemId { get; set; }
    public int QuantitySold { get; set; }
    public DateTime SaleDate { get; set; }
}

interface IManagementActions
{
    void AddInventoryItem();
    void ViewAllInventory();
    void UpdateInventoryItem();
    void DeleteInventoryItem();
    void AddSupplier();
    void ViewAllSuppliers();
    void UpdateSupplier();
    void DeleteSupplier();
    void AddSale();
    void ViewAllSales();
    void DeleteSale();
    void GenerateWeeklySalesReport();
    void GenerateMonthlySalesReport();
    void GenerateYearlySalesReport();
    void SearchByName();
}


class SupplyChainSystem : IManagementActions
{
    private readonly string inventoryFilePath = "inventory.txt";
    private readonly string supplierFilePath = "suppliers.txt";
    private readonly string salesFilePath = "sales.txt";
    private List<InventoryItem> inventoryItems = new List<InventoryItem>();
    private List<SalesRecord> salesRecords = new List<SalesRecord>();

    //add a new inventory item
    public void AddInventoryItem()
    {
        Console.Write("Enter Item Name: ");
        string itemName = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(itemName))
        {
            Console.WriteLine("Item name cannot be empty.");
            return;
        }

        Console.Write("Enter Quantity: ");
        if (!int.TryParse(Console.ReadLine(), out int quantity))
        {
            Console.WriteLine("Invalid quantity. Please enter a valid number.");
            return;
        }

        Console.Write("Enter Price: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price < 0)
        {
            Console.WriteLine("Invalid price. Please enter a valid positive number.");
            return;
        }
        ViewAllSuppliers();
        Console.Write("Enter Supplier Name: ");
        string supplierName = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(supplierName))
        {
            Console.WriteLine("Supplier name cannot be empty.");
            return;
        }

        var item = new InventoryItem(itemName, quantity, price, supplierName);

        File.AppendAllLines(inventoryFilePath, new[] { item.ToString() });
        inventoryItems.Add(item);

        Console.WriteLine("Inventory item added successfully!");
    }

    //view all inventory items
    public void ViewAllInventory()
    {
        if (File.Exists(inventoryFilePath))
        {
            var items = File.ReadAllLines(inventoryFilePath);
            var table = new Table().AddColumn("Item Name").AddColumn("Quantity (in sack)").AddColumn("Price (per sack)").AddColumn("Supplier Name");

            foreach (var item in items)
            {
                var details = item.Split(',');
                if (details.Length == 4)
                {
                    table.AddRow(details[0], details[1], details[2], details[3]);
                }
            }

            AnsiConsole.Write(table);
        }
        else
        {
            Console.WriteLine("No inventory items found.");
        }
    }
    //delete an inventory item
    public void DeleteInventoryItem()
    {
        ViewAllInventory();
        Console.Write("Enter the Item Name to delete: ");
        string itemName = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(itemName))
        {
            Console.WriteLine("Item name cannot be empty.");
            return;
        }

        if (File.Exists(inventoryFilePath))
        {
            var items = new List<string>(File.ReadAllLines(inventoryFilePath));
            var updatedItems = items.Where(item => !item.StartsWith(itemName + ",", StringComparison.OrdinalIgnoreCase)).ToList();

            if (items.Count == updatedItems.Count)
            {
                Console.WriteLine("Item not found.");
            }
            else
            {
                File.WriteAllLines(inventoryFilePath, updatedItems);
                Console.WriteLine("Inventory item deleted successfully.");
            }
            LoadInventoryItems();
        }
        else
        {
            Console.WriteLine("No inventory items found.");
        }
    }

    //add a new supplier
    public void AddSupplier()
    {
        Console.WriteLine();
        Console.Write("Enter Supplier Name: ");
        string name = Console.ReadLine();
        Console.Write("Enter Supplier Contact Number: ");
        string contactNumber = Console.ReadLine();

        var supplier = new Supplier(name, contactNumber);
        File.AppendAllLines(supplierFilePath, new[] { supplier.ToString() });

        Console.WriteLine("Supplier added successfully!");
    }
    //view all suppliers
    public void ViewAllSuppliers()
    {
        if (File.Exists(supplierFilePath))
        {
            var suppliers = File.ReadAllLines(supplierFilePath);
            var table = new Table();
            table.AddColumn("Supplier Name");
            table.AddColumn("Contact Number");
            table.Border(TableBorder.Rounded);
            table.Title("Suppliers");
            table.Expand();

            foreach (var supplier in suppliers)
            {
                var details = supplier.Split(',');
                if (details.Length >= 2)
                {
                    table.AddRow(details[0], details[1]);
                }
            }
            AnsiConsole.Write(table);
        }
        else
        {
            Console.WriteLine("No suppliers found.");
        }
    }
    //delete a supplier
    public void DeleteSupplier()
    {
        ViewAllSuppliers();
        Console.Write("Enter the Supplier Name to delete: ");
        string supplierName = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(supplierName))
        {
            Console.WriteLine("Supplier name cannot be empty.");
            return;
        }

        if (File.Exists(supplierFilePath))
        {
            var suppliers = new List<string>(File.ReadAllLines(supplierFilePath));
            var updatedSuppliers = suppliers.Where(supplier => !supplier.StartsWith(supplierName + ",", StringComparison.OrdinalIgnoreCase)).ToList();

            if (suppliers.Count == updatedSuppliers.Count)
            {
                Console.WriteLine("Supplier not found.");
            }
            else
            {
                File.WriteAllLines(supplierFilePath, updatedSuppliers);
                Console.WriteLine("Supplier deleted successfully.");
            }
            LoadSuppliers();
        }
        else
        {
            Console.WriteLine("No suppliers found.");
        }
    }
    public void SearchByName()
    {
        string searchType = AnsiConsole.Prompt(
            new TextPrompt<string>("Are you searching for an Inventory Item or Supplier? ")
                .Validate(input => input.ToLower() == "item" || input.ToLower() == "supplier"
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Please enter 'Item' or 'Supplier'."))
                .DefaultValue("Enter 'Item' or 'Supplier'"));

        Console.Write("Enter the name to search: ");
        string nameToSearch = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(nameToSearch))
        {
            Console.WriteLine("Search term cannot be empty.");
            return;
        }

        string filePath = searchType.ToLower() == "item" ? inventoryFilePath : supplierFilePath;

        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            bool found = false;
            var table = new Table();

            if (searchType.ToLower() == "item")
            {
                table.AddColumn("Item Name").AddColumn("Quantity").AddColumn("Price (per sack)").AddColumn("Supplier Name");
                foreach (var line in lines)
                {
                    var details = line.Split(',');

                    // Wildcard
                    if (details.Length == 4 && details[0].IndexOf(nameToSearch, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        table.AddRow(
                            HighlightMatch(details[0], nameToSearch),
                            details[1],
                            details[2],
                            details[3]
                        );
                        found = true;
                    }
                }
            }
            else if (searchType.ToLower() == "supplier")
            {
                table.AddColumn("Supplier Name").AddColumn("Contact Number");
                foreach (var line in lines)
                {
                    var details = line.Split(',');

                    if (details.Length == 2 && details[0].IndexOf(nameToSearch, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        table.AddRow(
                            HighlightMatch(details[0], nameToSearch), 
                            details[1]
                        );
                        found = true;
                    }
                }
            }

            if (found)
            {
                AnsiConsole.Write(table);
            }
            else
            {
                Console.WriteLine("No records found matching the search term.");
            }
        }
        else
        {
            Console.WriteLine($"File not found: {filePath}");
        }
    }
    private string HighlightMatch(string text, string search)
    {
        return text.Replace(search, $"[bold yellow]{search}[/]", StringComparison.OrdinalIgnoreCase);
    }
    // update inventory
    public void UpdateInventoryItem()
    {
        try
        {
            ViewAllInventory();

            Console.Write("Enter the Item Name to update: ");
            string itemName = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(itemName))
            {
                Console.WriteLine("Item name cannot be empty.");
                return;
            }

            if (File.Exists(inventoryFilePath))
            {
                var items = new List<string>(File.ReadAllLines(inventoryFilePath));
                bool itemFound = false;

                for (int i = 0; i < items.Count; i++)
                {
                    var details = items[i].Split(',');

                    if (details.Length == 4 && details[0].Equals(itemName, StringComparison.OrdinalIgnoreCase))
                    {
                        var table = new Table();
                        table.AddColumn("Attribute");
                        table.AddColumn("Current Value");
                        table.AddRow("Item Name", details[0]);
                        table.AddRow("Quantity", details[1]);
                        table.AddRow("Price (per sack)", $"{decimal.Parse(details[2]):C}");
                        table.AddRow("Supplier Name", details[3]);
                        table.Title = new TableTitle("Current Item Details");
                        table.Border = TableBorder.Rounded;

                        AnsiConsole.Write(table);

                        string newItemName = AnsiConsole.Ask<string>("Enter new Item Name (Press ENTER to keep current):", details[0]);
                        string newQuantity = AnsiConsole.Ask<string>("Enter new Quantity (Press ENTER to keep current):", details[1]);
                        string newPrice = AnsiConsole.Ask<string>("Enter new Price (Press ENTER to keep current):", details[2]);
                        ViewAllSuppliers();
                        string newSupplierName = AnsiConsole.Ask<string>("Enter new Supplier Name (Press ENTER to keep current):", details[3]);

                        details[0] = string.IsNullOrWhiteSpace(newItemName) ? details[0] : newItemName;
                        details[1] = string.IsNullOrWhiteSpace(newQuantity) ? details[1] : newQuantity;
                        details[2] = string.IsNullOrWhiteSpace(newPrice) ? details[2] : newPrice;
                        details[3] = string.IsNullOrWhiteSpace(newSupplierName) ? details[3] : newSupplierName;

                        items[i] = string.Join(",", details);
                        File.WriteAllLines(inventoryFilePath, items);

                        LoadInventoryItems();

                        Console.WriteLine("Inventory item updated successfully.");
                        itemFound = true;
                        break;
                    }
                }
                if (!itemFound)
                {
                    Console.WriteLine("Item not found.");
                }
            }
            else
            {
                Console.WriteLine("No inventory items found.");
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"An error occurred while accessing the file: {ex.Message}");
        }
    }
    // updating supplier
    public void UpdateSupplier()
    {
        try
        {
            ViewAllSuppliers();
            Console.Write("Enter the Supplier Name to update: ");
            string supplierName = Console.ReadLine();

            if (File.Exists(supplierFilePath))
            {
                var suppliers = new List<string>(File.ReadAllLines(supplierFilePath));
                bool supplierFound = false;

                for (int i = 0; i < suppliers.Count; i++)
                {
                    var details = suppliers[i].Split(',');
                    string currentSupplierName = details[0];
                    string currentContactNumber = details[1];

                    if (currentSupplierName.Equals(supplierName, StringComparison.OrdinalIgnoreCase))
                    {
                        var table = new Table();
                        table.Title = new TableTitle("Current Supplier Details");
                        table.AddColumn("Supplier Name").Centered();
                        table.AddColumn("Contact Number").Centered();
                        table.Border = TableBorder.Rounded;

                        table.AddRow(currentSupplierName, currentContactNumber);

                        AnsiConsole.Write(table);

                        string newSupplierName = AnsiConsole.Prompt(
                            new TextPrompt<string>("Enter new Supplier Name (Press ENTER to keep current):")
                                .AllowEmpty()
                                .DefaultValue(currentSupplierName)
                        );

                        string newContactNumber = AnsiConsole.Prompt(
                            new TextPrompt<string>("Enter new Contact Number (Press ENTER to keep current):")
                                .AllowEmpty()
                                .DefaultValue(currentContactNumber)
                        );

                        if (!string.IsNullOrWhiteSpace(newSupplierName))
                            details[0] = newSupplierName;
                        if (!string.IsNullOrWhiteSpace(newContactNumber))
                            details[1] = newContactNumber;

                        suppliers[i] = string.Join(",", details);

                        File.WriteAllLines(supplierFilePath, suppliers);

                        AnsiConsole.MarkupLine("[green]Supplier updated successfully![/]");
                        supplierFound = true;
                        break;
                    }
                }

                if (!supplierFound)
                {
                    AnsiConsole.MarkupLine("[red]Supplier not found.[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]No suppliers found.[/]");
            }
        }
        catch (IOException ex)
        {
            AnsiConsole.MarkupLine($"[red]An error occurred while accessing the file: {ex.Message}[/]");
        }
    }

    public void AddSale()
    {
        ViewAllInventory();

        Console.Write("Enter the inventory item name for the sale: ");
        string itemName = Console.ReadLine()?.Trim();

        //find item by name
        var inventoryItem = inventoryItems.FirstOrDefault(item => item.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

        if (inventoryItem != null)
        {
            Console.Write("Enter the quantity sold: ");
            if (int.TryParse(Console.ReadLine(), out int quantitySold))
            {
                if (inventoryItem.Quantity >= quantitySold)
                {
                    decimal totalPrice = inventoryItem.Price * quantitySold;
                    Console.WriteLine($"Total Price: {totalPrice}");

                    Console.Write("Enter the amount paid: ");
                    if (decimal.TryParse(Console.ReadLine(), out decimal amountPaid))
                    {
                        if (amountPaid < totalPrice)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Not enough amount. Transaction failed.");
                        }
                        else if (amountPaid == totalPrice)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Transaction successful. No change needed.");
                        }
                        else
                        {
                            decimal change = amountPaid - totalPrice;
                            Console.WriteLine();
                            Console.WriteLine($"Transaction successful. Your change is {change:C}.");
                        }
                        if (amountPaid >= totalPrice)
                        {
                            inventoryItem.UpdateQuantity(-quantitySold);
                            var sale = new SalesRecord
                            {
                                InventoryItemId = inventoryItems.IndexOf(inventoryItem),
                                QuantitySold = quantitySold,
                                SaleDate = DateTime.Now
                            };

                            salesRecords.Add(sale);

                            SaveSalesData(sale);
                            SaveUpdatedInventory();
                            Console.WriteLine();
                            Console.WriteLine($"Sale of {quantitySold} {inventoryItem.Name}(s) recorded successfully.");
                            Console.WriteLine($"Remaining Quantity of {inventoryItem.Name}: {inventoryItem.Quantity}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid amount entered.");
                    }
                }
                else
                {
                    Console.WriteLine("Not enough stock available.");
                }
            }
            else
            {
                Console.WriteLine("Invalid quantity entered.");
            }
        }
        else
        {
            Console.WriteLine("Inventory item not found.");
        }
    }
    private void SaveSalesReportToFile(string reportContent, string folderName, string fileName)
    {
        string folderPath = Path.Combine("SalesReports", folderName);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string filePath = Path.Combine(folderPath, fileName);
        File.WriteAllText(filePath, reportContent);

        Console.WriteLine($"Sales report saved to: {filePath}");
    }
    private void SaveSalesData(SalesRecord sale)
    {
        string saleData = $"{inventoryItems[sale.InventoryItemId].Name},{sale.QuantitySold},{inventoryItems[sale.InventoryItemId].Price},{sale.SaleDate:yyyy-MM-dd}";
        File.AppendAllLines(salesFilePath, new[] { saleData });
    }
    private void SaveUpdatedInventory()
    {
        var updatedInventory = inventoryItems.Select(item => item.ToString()).ToList();
        File.WriteAllLines(inventoryFilePath, updatedInventory);
    }

    // View all sales
    public void ViewAllSales()
    {
        LoadSalesData();
        if (File.Exists(salesFilePath))
        {
            var sales = File.ReadAllLines(salesFilePath);
            var table = new Table();
            table.AddColumn("Item Name");
            table.AddColumn("Quantity Sold");
            table.AddColumn("Price (per sack)");
            table.AddColumn("Date of Sale");
            table.Border(TableBorder.Rounded);
            table.Title("Sales Report");
            table.Expand();

            foreach (var sale in sales)
            {
                var details = sale.Split(',');
                if (details.Length == 4)
                {
                    table.AddRow(details[0], details[1], details[2], details[3]);
                }
            }

            AnsiConsole.Write(table);
        }
        else
        {
            Console.WriteLine("No sales found.");
        }
    }
    public void DeleteSale()
    {
        ViewAllSales();

        Console.Write("Enter the Item Name in the sale record to delete: ");
        string itemName = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(itemName))
        {
            Console.WriteLine("Item name cannot be empty.");
            return;
        }

        Console.Write("Enter the Sale Date (yyyy-MM-dd) to delete: ");
        string saleDateInput = Console.ReadLine()?.Trim();

        if (!DateTime.TryParse(saleDateInput, out DateTime saleDate))
        {
            Console.WriteLine("Invalid date format. Please use yyyy-MM-dd.");
            return;
        }

        if (File.Exists(salesFilePath))
        {
            var sales = new List<string>(File.ReadAllLines(salesFilePath));

            var updatedSales = sales.Where(sale =>
            {
                var details = sale.Split(',');

                if (details.Length == 4 &&
                    details[0].Equals(itemName, StringComparison.OrdinalIgnoreCase) &&
                    DateTime.TryParse(details[3], out DateTime existingDate) &&
                    existingDate == saleDate)
                {
                    return false;
                }
                return true;
            }).ToList();

            if (sales.Count == updatedSales.Count)
            {
                Console.WriteLine("Sale record not found.");
            }
            else
            {
                File.WriteAllLines(salesFilePath, updatedSales);
                Console.WriteLine("Sale record deleted successfully.");
            }
            LoadSalesData();
        }
        else
        {
            Console.WriteLine("No sales data found.");
        }
    }
    private void LoadSalesData()
    {
        salesRecords.Clear();

        if (File.Exists(salesFilePath))
        {
            var lines = File.ReadAllLines(salesFilePath);

            foreach (var line in lines)
            {
                var details = line.Split(',');

                if (details.Length == 4 &&
                    int.TryParse(details[1], out int quantitySold) &&
                    decimal.TryParse(details[2], out decimal price) &&
                    DateTime.TryParse(details[3], out DateTime saleDate))
                {
                    salesRecords.Add(new SalesRecord
                    {
                        InventoryItemId = inventoryItems.FindIndex(item => item.Name == details[0]),
                        QuantitySold = quantitySold,
                        SaleDate = saleDate
                    });
                }
                else
                {
                    Console.WriteLine("Invalid data format in sales file.");
                }
            }
        }
        else
        {
            Console.WriteLine("Sales file not found.");
        }
    }
    //weekly sales report
    public void GenerateWeeklySalesReport()
    {
        Console.Write("Enter a date (yyyy-MM-dd) within the week for the sales report: ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime selectedDate))
        {
            Console.WriteLine("Invalid date format.");
            return;
        }

        DateTime startOfWeek = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);
        DateTime endOfWeek = startOfWeek.AddDays(6);

        if (File.Exists(salesFilePath))
        {
            var sales = File.ReadAllLines(salesFilePath);
            var weeklySales = new List<string>();

            foreach (var sale in sales)
            {
                var details = sale.Split(',');
                if (details.Length == 4 &&
                    DateTime.TryParse(details[3], out DateTime saleDate) &&
                    saleDate >= startOfWeek &&
                    saleDate <= endOfWeek)
                {
                    weeklySales.Add(sale);
                }
            }

            if (weeklySales.Count > 0)
            {
                string title = $"Weekly Sales Report ({startOfWeek:yyyy-MM-dd} to {endOfWeek:yyyy-MM-dd})";
                string reportContent = GenerateReceiptContent(weeklySales, title);

                Console.WriteLine(reportContent);

                HandleSaveOption(reportContent, $"Week-{startOfWeek:yyyy-MM-dd}", "WeeklySalesReport.txt");
            }
            else
            {
                Console.WriteLine("No sales found for the selected week.");
            }
        }
        else
        {
            Console.WriteLine("No sales data found.");
        }
    }

    //monthly sales report
    public void GenerateMonthlySalesReport()
    {
        Console.Write("Enter the month (1-12) for the sales report: ");
        if (!int.TryParse(Console.ReadLine(), out int selectedMonth) || selectedMonth < 1 || selectedMonth > 12)
        {
            Console.WriteLine("Invalid month. Please enter a number between 1 and 12.");
            return;
        }

        Console.Write("Enter the year for the sales report: ");
        if (!int.TryParse(Console.ReadLine(), out int selectedYear))
        {
            Console.WriteLine("Invalid year.");
            return;
        }

        if (File.Exists(salesFilePath))
        {
            var sales = File.ReadAllLines(salesFilePath);
            var monthlySales = new List<string>();

            foreach (var sale in sales)
            {
                var details = sale.Split(',');
                if (details.Length == 4 &&
                    DateTime.TryParse(details[3], out DateTime saleDate) &&
                    saleDate.Month == selectedMonth &&
                    saleDate.Year == selectedYear)
                {
                    monthlySales.Add(sale);
                }
            }

            if (monthlySales.Count > 0)
            {
                string title = $"Monthly Sales Report - {new DateTime(selectedYear, selectedMonth, 1):MMMM yyyy}";
                string reportContent = GenerateReceiptContent(monthlySales, title);

                Console.WriteLine(reportContent);

                HandleSaveOption(reportContent, new DateTime(selectedYear, selectedMonth, 1).ToString("MMMM"), "MonthlySalesReport.txt");
            }
            else
            {
                Console.WriteLine("No sales found for the selected month and year.");
            }
        }
        else
        {
            Console.WriteLine("No sales data found.");
        }
    }
    public void GenerateYearlySalesReport()
    {
        Console.Write("Enter the year for the sales report: ");
        if (!int.TryParse(Console.ReadLine(), out int selectedYear))
        {
            Console.WriteLine("Invalid year.");
            return;
        }

        if (File.Exists(salesFilePath))
        {
            var sales = File.ReadAllLines(salesFilePath);
            var yearlySales = new List<string>();

            foreach (var sale in sales)
            {
                var details = sale.Split(',');
                if (details.Length == 4 &&
                    DateTime.TryParse(details[3], out DateTime saleDate) &&
                    saleDate.Year == selectedYear)
                {
                    yearlySales.Add(sale);
                }
            }

            if (yearlySales.Count > 0)
            {
                string title = $"Yearly Sales Report - {selectedYear}";
                string reportContent = GenerateReceiptContent(yearlySales, title);
                Console.WriteLine(reportContent);
                HandleSaveOption(reportContent, selectedYear.ToString(), "YearlySalesReport.txt");
            }
            else
            {
                Console.WriteLine("No sales found for the selected year.");
            }
        }
        else
        {
            Console.WriteLine("No sales data found.");
        }
    }

    public SupplyChainSystem()
    {
        LoadInventoryItems();
        LoadSalesRecords();
    }
    private void LoadInventoryItems()
    {
        inventoryItems.Clear();

        if (File.Exists(inventoryFilePath))
        {
            var lines = File.ReadAllLines(inventoryFilePath);
            foreach (var line in lines)
            {
                var details = line.Split(',');

                if (details.Length == 4 &&
                    int.TryParse(details[1], out int quantity) &&
                    decimal.TryParse(details[2], out decimal price))
                {
                    inventoryItems.Add(new InventoryItem(details[0], quantity, price, details[3]));
                }
                else
                {
                    Console.WriteLine("Invalid data format in inventory file.");
                }
            }
        }
        else
        {
            Console.WriteLine("Inventory file not found.");
        }
    }
    private void LoadSalesRecords()
    {
        if (File.Exists(salesFilePath))
        {
            var lines = File.ReadAllLines(salesFilePath);
            foreach (var line in lines)
            {
                var details = line.Split(',');
                if (details.Length == 4 &&
                    int.TryParse(details[1], out int quantitySold) &&
                    double.TryParse(details[2], out double price) &&
                    DateTime.TryParse(details[3], out DateTime saleDate))
                {
                    salesRecords.Add(new SalesRecord
                    {
                        InventoryItemId = inventoryItems.FindIndex(item => item.Name == details[0]),
                        QuantitySold = quantitySold,
                        SaleDate = saleDate
                    });
                }
            }
        }
    }
    private void LoadSuppliers()
    {
        // suppliers.Clear(); 

        if (File.Exists(supplierFilePath))
        {
            var lines = File.ReadAllLines(supplierFilePath);
            foreach (var line in lines)
            {
                var details = line.Split(',');

                if (details.Length == 2)
                {
                    string name = details[0];
                    string contactNumber = details[1];

                    // suppliers.Add(new Supplier(name, contactNumber));
                }
                else
                {
                    Console.WriteLine("Invalid data format in suppliers file.");
                }
            }
        }
        else
        {
            Console.WriteLine("Suppliers file not found.");
        }
    }
    private string GenerateReceiptContent(List<string> sales, string title)
    {
        decimal totalProfit = 0;
        var builder = new System.Text.StringBuilder();
        Console.WriteLine();
        Console.WriteLine();
        builder.AppendLine("=========================================");
        builder.AppendLine($"{title}");
        builder.AppendLine("=========================================");
        builder.AppendLine($"{"Item Name",-15}{"Qty",-5}{"Price",-10}{"Total"}");
        builder.AppendLine("-----------------------------------------");

        foreach (var sale in sales)
        {
            var details = sale.Split(',');
            if (details.Length == 4 &&
                int.TryParse(details[1], out int quantitySold) &&
                decimal.TryParse(details[2], out decimal price))
            {
                decimal total = quantitySold * price;
                totalProfit += total;

                builder.AppendLine($"{details[0],-15}{quantitySold,-5}{price,-10}{total}");
            }
        }

        builder.AppendLine("-----------------------------------------");
        builder.AppendLine($"Total:   {totalProfit,25}");
        builder.AppendLine("=========================================");

        return builder.ToString();
    }
    private void HandleSaveOption(string reportContent, string folderName, string fileName)
    {
        Console.Write("Do you want to save the report as a text file? (yes/no): ");
        string saveOption = Console.ReadLine()?.Trim().ToLower();

        if (saveOption == "yes" || saveOption == "y")
        {
            SaveSalesReportToFile(reportContent, folderName, fileName);
        }
        else
        {
            Console.WriteLine("The report will not be saved to a file.");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            SupplyChainSystem system = new SupplyChainSystem();

            while (true)
            {
                Console.Clear();
                DisplayWelcomeScreen();
                DisplayMainMenu();

                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Clear();
                        system.SearchByName();
                        break;
                    case "2":
                        InventoryMenu(system);
                        break;
                    case "3":
                        SupplierMenu(system);
                        break;
                    case "4":
                        SalesMenu(system);
                        break;
                    case "0":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine("\nPress any key to return to the main menu...");
                Console.ReadKey();
            }
        }

        static void DisplayWelcomeScreen()
        {
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║       Supply Chain Management System         ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        static void DisplayMainMenu()
        {
            Console.WriteLine("╔═════════════════════════════════════════════╗");
            Console.WriteLine("║                  Main Menu                  ║");
            Console.WriteLine("╠═════════════════════════════════════════════╣");
            Console.WriteLine("║  1 ║ Search                                 ║");
            Console.WriteLine("║  2 ║ Inventory                              ║");
            Console.WriteLine("║  3 ║ Suppliers                              ║");
            Console.WriteLine("║  4 ║ Sales                                  ║");
            Console.WriteLine("║  0 ║ Exit                                   ║");
            Console.WriteLine("╚═════════════════════════════════════════════╝");
            Console.Write("Please select an option: ");
        }
        static void InventoryMenu(SupplyChainSystem system)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔═════════════════════════════════════════════╗");
                Console.WriteLine("║                Inventory Menu               ║");
                Console.WriteLine("╠═════════════════════════════════════════════╣");
                Console.WriteLine("║  1 ║ View All Inventory                     ║");
                Console.WriteLine("║  2 ║ Add Inventory Item                     ║");
                Console.WriteLine("║  3 ║ Delete Inventory Item                  ║");
                Console.WriteLine("║  4 ║ Update Inventory Item                  ║");
                Console.WriteLine("║  0 ║ Back to Main Menu                      ║");
                Console.WriteLine("╚═════════════════════════════════════════════╝");
                Console.Write("Please select an option: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Clear();
                        system.ViewAllInventory();
                        break;
                    case "2":
                        Console.Clear();
                        system.AddInventoryItem();
                        break;
                    case "3":
                        Console.Clear();
                        system.DeleteInventoryItem();
                        break;
                    case "4":
                        Console.Clear();
                        system.UpdateInventoryItem();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine("\nPress any key to return to the Inventory Menu...");
                Console.ReadKey();
            }
        }
        static void SupplierMenu(SupplyChainSystem system)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔═════════════════════════════════════════════╗");
                Console.WriteLine("║                Supplier Menu                ║");
                Console.WriteLine("╠═════════════════════════════════════════════╣");
                Console.WriteLine("║  1 ║ View All Suppliers                     ║");
                Console.WriteLine("║  2 ║ Add Supplier                           ║");
                Console.WriteLine("║  3 ║ Delete Supplier                        ║");
                Console.WriteLine("║  4 ║ Update Supplier                        ║");
                Console.WriteLine("║  0 ║ Back to Main Menu                      ║");
                Console.WriteLine("╚═════════════════════════════════════════════╝");
                Console.Write("Please select an option: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Clear();
                        system.ViewAllSuppliers();
                        break;
                    case "2":
                        Console.Clear();
                        system.AddSupplier();
                        break;
                    case "3":
                        Console.Clear();
                        system.DeleteSupplier();
                        break;
                    case "4":
                        Console.Clear();
                        system.UpdateSupplier();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine("\nPress any key to return to the Supplier Menu...");
                Console.ReadKey();
            }
        }
        static void SalesMenu(SupplyChainSystem system)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔═════════════════════════════════════════════╗");
                Console.WriteLine("║                 Sales Menu                  ║");
                Console.WriteLine("╠═════════════════════════════════════════════╣");
                Console.WriteLine("║  1 ║ View All Sales                         ║");
                Console.WriteLine("║  2 ║ Add Sale                               ║");
                Console.WriteLine("║  3 ║ Weekly Sales Report                    ║");
                Console.WriteLine("║  4 ║ Monthly Sales Report                   ║");
                Console.WriteLine("║  5 ║ Yearly Sales Report                    ║"); 
                Console.WriteLine("║  0 ║ Back to Main Menu                      ║");
                Console.WriteLine("╚═════════════════════════════════════════════╝");
                Console.Write("Please select an option: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        system.ViewAllSales();
                        break;
                    case "2":
                        system.AddSale();
                        break;
                    case "3":
                        system.GenerateWeeklySalesReport();
                        break;
                    case "4":
                        system.GenerateMonthlySalesReport();
                        break;
                    case "5":
                        system.GenerateYearlySalesReport(); 
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine("\nPress any key to return to the Sales Menu...");
                Console.ReadKey();
            }
        }

    }
}