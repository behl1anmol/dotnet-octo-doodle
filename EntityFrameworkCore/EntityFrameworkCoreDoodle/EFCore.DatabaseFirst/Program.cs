// See https://aka.ms/new-console-template for more information

using EFCore.DatabaseFirst.Context;

Console.WriteLine("Hello, World!");

var context = new AdventureWorksContext();

var query = context.Customers.Where(c=>c.FirstName.StartsWith("C"))
    .Select(c => new
    {
        c.FullName,
        c.SalesOrderHeaders.Count
    });

foreach (var item in query)
{
    Console.WriteLine($"Customer: {item.FullName} Orders: {item.Count}");
}    