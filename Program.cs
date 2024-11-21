using System.Xml.Linq;

abstract class LivestockSupply
{
    protected string ID { get; set; }
    protected string Name { get; set; }

    public LivestockSupply(string id, string name)
    {
        ID = id;
        Name = name;
    }

    public abstract void Display();
}

// Supplier inheriting from LivestockSupply
class Supplier : LivestockSupply
{
    private string ContactNumber { get; set; }

    public Supplier(string id, string name, string contactNumber)
        : base(id, name)
    {
        ContactNumber = contactNumber;
    }

    public override void Display()
    {
        Console.WriteLine($"Supplier ID: {ID}, Name: {Name}, Contact: {ContactNumber}");
    }

    public override string ToString()
    {
        return $"{ID},{Name},{ContactNumber}";
    }
}

// InventoryItem inheriting from LivestockSupply
class InventoryItem : LivestockSupply
{
    protected int Quantity { get; private set; }
    protected string SupplierID { get; set; }

    public InventoryItem(string id, string name, int quantity, string supplierID)
        : base(id, name)
    {
        Quantity = quantity;
        SupplierID = supplierID;
    }

    public override void Display()
    {
        Console.WriteLine($"Item ID: {ID}, Name: {Name}, Quantity: {Quantity}, Supplier ID: {SupplierID}");
    }


    public override string ToString()
    {
        return $"{ID},{Name},{Quantity},{SupplierID}";
    }
}

interface IManagementActions
{
    void AddInventoryItem();
    void ViewAllInventory();
    void DeleteInventoryItem();
    void AddSupplier();
    void ViewAllSuppliers();
    void DeleteSupplier();
    void SearchByID();
    void UpdateSupplier();
    void UpdateInventoryItem();

}

class SupplyChainSystem : IManagementActions
{
    private readonly string inventoryFilePath = "inventory.txt";
    private readonly string supplierFilePath = "suppliers.txt";

    //add a new inventory item
    public void AddInventoryItem()
    {
        try
        {
            Console.Write("Enter Item ID: ");
            string itemID = Console.ReadLine();
            Console.Write("Enter Item Name: ");
            string itemName = Console.ReadLine();
            Console.Write("Enter Quantity: ");
            int quantity = int.Parse(Console.ReadLine());
            Console.Write("Enter Supplier ID: ");
            string supplierID = Console.ReadLine();

            var item = new InventoryItem(itemID, itemName, quantity, supplierID);
            File.AppendAllLines(inventoryFilePath, new[] { item.ToString() });

            Console.WriteLine("Inventory item added successfully!");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"An error occurred while accessing the file: {ex.Message}");
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid input for quantity. Please enter a valid integer.");
        }
    }
      
    //view all inventory items
    public void ViewAllInventory()
    {
        if (File.Exists(inventoryFilePath))
        {
            var items = File.ReadAllLines(inventoryFilePath);
            Console.WriteLine();
            Console.WriteLine("╔═════════╦═══════════╦══════════╦════════════╗");
            Console.WriteLine("║ Item ID ║ Item Name ║ Quantity ║ Supplier ID║");
            Console.WriteLine("╚═════════╩═══════════╩══════════╩════════════╝");

            foreach (var line in items)
            {
                var details = line.Split(',');
                string itemID = details[0].PadRight(9);
                string itemName = details[1].PadRight(9);
                string quantity = details[2].PadRight(8);
                string supplierID = details[3].PadRight(10);

                Console.WriteLine($"║{itemID}║ {itemName} ║ {quantity} ║ {supplierID} ║");
                Console.WriteLine("╚═════════╩═══════════╩══════════╩════════════╝");
            }
            

            
        }
        else
        {
            Console.WriteLine("No inventory items found.");
        }
    }

    //delete an inventory item
    public void DeleteInventoryItem()
    {
        try
        {
            Console.Write("Enter the Item ID to delete: ");
            string itemID = Console.ReadLine();

            if (File.Exists(inventoryFilePath))
            {
                var items = new List<string>(File.ReadAllLines(inventoryFilePath));
                bool itemFound = false;

                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].StartsWith(itemID + ","))
                    {
                        Console.Write("Are you sure you want to delete this item? (y/n): ");
                        if (Console.ReadLine().ToLower() == "y")
                        {
                            items.RemoveAt(i);
                            File.WriteAllLines(inventoryFilePath, items);
                            Console.WriteLine("Inventory item deleted successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Deletion canceled.");
                        }
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

    //add a new supplier
    public void AddSupplier()
    {
        Console.WriteLine();
        Console.Write("Enter Supplier ID: ");
        string supplierID = Console.ReadLine();
        Console.Write("Enter Supplier Name: ");
        string name = Console.ReadLine();
        Console.Write("Enter Supplier Contact Number: ");
        string contactNumber = Console.ReadLine();

        var supplier = new Supplier(supplierID, name, contactNumber);
        File.AppendAllLines(supplierFilePath, new[] { supplier.ToString() });

        Console.WriteLine("Supplier added successfully!");
    }

    //view all suppliers
    public void ViewAllSuppliers()
    {
        if (File.Exists(supplierFilePath))
        {
            var suppliers = File.ReadAllLines(supplierFilePath);
            Console.WriteLine();
            Console.WriteLine("╔════════════╦════════════════════╦════════════════════╗");
            Console.WriteLine("║ Supplier ID║ Supplier Name      ║ Contact Number     ║");
            Console.WriteLine("╠════════════╬════════════════════╬════════════════════╣");
            foreach (var line in suppliers)
            {
                var details = line.Split(',');
                string supplierID = details[0].PadRight(12);
                string supplierName = details[1].PadRight(18);
                string contactNumber = details[2].PadRight(18);

                Console.WriteLine($"║{supplierID}║ {supplierName} ║ {contactNumber} ║");
                Console.WriteLine("╚════════════╩════════════════════╩════════════════════╝");
            }

        
        }
        else
        {
            Console.WriteLine("No suppliers found.");
        }
    }

    // Method to delete a supplier
    public void DeleteSupplier()
    {
        Console.Write("Enter the Supplier ID to delete: ");
        string supplierID = Console.ReadLine();

        if (File.Exists(supplierFilePath))
        {
            var suppliers = new List<string>(File.ReadAllLines(supplierFilePath));
            bool supplierFound = false;

            for (int i = 0; i < suppliers.Count; i++)
            {
                if (suppliers[i].StartsWith(supplierID + ","))
                {
                    Console.Write("Are you sure you want to delete this supplier? (y/n): ");
                    if (Console.ReadLine().ToLower() == "y")
                    {
                        suppliers.RemoveAt(i);
                        File.WriteAllLines(supplierFilePath, suppliers);
                        Console.WriteLine("Supplier deleted successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Deletion canceled.");
                    }
                    supplierFound = true;
                    break;
                }
            }

            if (!supplierFound)
            {
                Console.WriteLine("Supplier not found.");
            }
        }
        else
        {
            Console.WriteLine("No suppliers found.");
        }
    }
    public void SearchByID()
    {
        Console.Write("Are you searching for an Inventory Item or Supplier? (Enter 'Item' or 'Supplier'): ");
        string searchType = Console.ReadLine().ToLower();
        Console.Write("Enter the ID to search: ");
        string id = Console.ReadLine();

        // determine file path and header based on search type
        string filePath, header;
        if (searchType == "item")
        {
            filePath = inventoryFilePath;
            header = "╔═════════╦═══════════╦══════════╦════════════╗\n║ Item ID ║ Item Name ║ Quantity ║ Supplier ID║\n╠═════════╬═══════════╬══════════╬════════════╣";
        }
        else if (searchType == "supplier")
        {
            filePath = supplierFilePath;
            header = "╔════════════╦════════════════╦════════════╗\n║ Supplier ID║ Supplier Name ║ Contact    ║\n╠════════════╬════════════════╬════════════╣";
        }
        else
        {
            Console.WriteLine("Invalid search type.");
            return;
        }

        // search the file
        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            bool found = false;

            foreach (var line in lines)
            {
                if (line.StartsWith(id + ","))
                {
                    Console.WriteLine("\n" + header);
                    Console.WriteLine(line.Replace(",", " ║ "));
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Console.WriteLine("No record found with the specified ID.");
            }
        }
        else
        {
            Console.WriteLine("No records found.");
        }
    }
    // update inventory
    public void UpdateInventoryItem()
    {
        try
        {
            Console.Write("Enter the Item ID to update: ");
            string itemID = Console.ReadLine();

            if (File.Exists(inventoryFilePath))
            {
                var items = new List<string>(File.ReadAllLines(inventoryFilePath));
                bool itemFound = false;

                for (int i = 0; i < items.Count; i++)
                {
                    var details = items[i].Split(',');
                    string currentItemID = details[0];

                    if (currentItemID == itemID)
                    {
                        // displays  the  items to be updated
                        string itemName = details[1];
                        string quantity = details[2];
                        string supplierID = details[3];

                        Console.WriteLine();
                        Console.WriteLine("╔═════════╦═══════════╦══════════╦════════════╗");
                        Console.WriteLine("║ Item ID ║ Item Name ║ Quantity ║ Supplier ID║");
                        Console.WriteLine("╠═════════╬═══════════╬══════════╬════════════╣");
                        Console.WriteLine($"║{currentItemID.PadRight(9)}║ {itemName.PadRight(9)} ║ {quantity.PadRight(8)} ║ {supplierID.PadRight(10)} ║");
                        Console.WriteLine("╚═════════╩═══════════╩══════════╩════════════╝");

                        // asks for value
                        Console.Write("Enter new Item Name (Press ENTER to keep current): ");
                        string newItemName = Console.ReadLine();
                        Console.Write("Enter new Quantity (Press ENTER to keep current): ");
                        string newQuantity = Console.ReadLine();
                        Console.Write("Enter new Supplier ID (Press ENTER to keep current): ");
                        string newSupplierID = Console.ReadLine();

                        // updates the value
                        if (!string.IsNullOrEmpty(newItemName)) details[1] = newItemName;
                        if (!string.IsNullOrEmpty(newQuantity)) details[2] = newQuantity;
                        if (!string.IsNullOrEmpty(newSupplierID)) details[3] = newSupplierID;

                        // saves the update
                        items[i] = string.Join(",", details);

                        // writes the update to the file
                        File.WriteAllLines(inventoryFilePath, items);
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
            Console.Write("Enter the Supplier ID to update: ");
            string supplierID = Console.ReadLine();

            if (File.Exists(supplierFilePath))
            {
                var suppliers = new List<string>(File.ReadAllLines(supplierFilePath));
                bool supplierFound = false;

                for (int i = 0; i < suppliers.Count; i++)
                {
                    var details = suppliers[i].Split(',');
                    string currentSupplierID = details[0];

                    if (currentSupplierID == supplierID)
                    {
                        // displays the info
                        string supplierName = details[1];
                        string contactNumber = details[2];

                        Console.WriteLine();
                        Console.WriteLine("╔════════════╦════════════════════╦════════════════════╗");
                        Console.WriteLine("║ Supplier ID║ Supplier Name      ║ Contact Number     ║");
                        Console.WriteLine("╠════════════╬════════════════════╬════════════════════╣");
                        Console.WriteLine($"║{currentSupplierID.PadRight(12)}║ {supplierName.PadRight(18)} ║ {contactNumber.PadRight(18)} ║");
                        Console.WriteLine("╚════════════╩════════════════════╩════════════════════╝");

                        // ask new values
                        Console.Write("Enter new Supplier Name (Press ENTER to keep current): ");
                        string newSupplierName = Console.ReadLine();
                        Console.Write("Enter new Contact Number (Press ENTER to keep current): ");
                        string newContactNumber = Console.ReadLine();

                        // update values
                        if (!string.IsNullOrEmpty(newSupplierName)) details[1] = newSupplierName;
                        if (!string.IsNullOrEmpty(newContactNumber)) details[2] = newContactNumber;

                        // saves the update
                        suppliers[i] = string.Join(",", details);

                        // writes the update to the file
                        File.WriteAllLines(supplierFilePath, suppliers);
                        Console.WriteLine("Supplier updated successfully.");
                        supplierFound = true;
                        break;
                    }
                }

                if (!supplierFound)
                {
                    Console.WriteLine("Supplier not found.");
                }
            }
            else
            {
                Console.WriteLine("No suppliers found.");
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"An error occurred while accessing the file: {ex.Message}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            SupplyChainSystem system = new SupplyChainSystem();

            while (true)
            {
                DisplayWelcomeScreen();
                DisplayMenu();

                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Clear();
                        system.SearchByID();
                        break;
                    case "2":
                        Console.Clear();
                        system.ViewAllSuppliers();
                        break;
                    case "3":
                        Console.Clear();
                        system.ViewAllInventory();
                        break;
                    case "4":
                        Console.Clear();
                        system.AddSupplier();
                        break;
                    case "5":
                        Console.Clear();
                        system.AddInventoryItem();
                        break;
                    case "6":
                        Console.Clear();
                        system.UpdateInventoryItem();
                        break;
                    case "7":
                        Console.Clear();
                        system.UpdateSupplier();
                        break;
                    case "8":
                        Console.Clear();
                        system.DeleteInventoryItem();
                        break;
                    case "9":
                        Console.Clear();
                        system.DeleteSupplier();
                        break;
                    case "0":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.Clear();
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
                Console.WriteLine("\nPress any key to return to the main menu...");
                Console.ReadKey();
            }
        }

        static void DisplayWelcomeScreen()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║       Supply Chain Management System         ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        static void DisplayMenu()
        {
            Console.WriteLine("╔═════════════════════════════════════════════╗");
            Console.WriteLine("║                  Main Menu                  ║");
            Console.WriteLine("╠═════════════════════════════════════════════╣");
            Console.WriteLine("║  1 ║ Search ID                              ║");
            Console.WriteLine("║  2 ║ View All Suppliers                     ║");
            Console.WriteLine("║  3 ║ View All Inventory                     ║");
            Console.WriteLine("║  4 ║ Add Supplier                           ║");
            Console.WriteLine("║  5 ║ Add Inventory Item                     ║");
            Console.WriteLine("║  6 ║ Update Inventory Item                  ║");
            Console.WriteLine("║  7 ║ Update Supplier                        ║");
            Console.WriteLine("║  8 ║ Delete Inventory Item                  ║");
            Console.WriteLine("║  9 ║ Delete Supplier                        ║");
            Console.WriteLine("║  0 ║ Exit                                   ║");
            Console.WriteLine("╚═════════════════════════════════════════════╝");
            Console.Write("Please select an option: ");
        }
    }
}