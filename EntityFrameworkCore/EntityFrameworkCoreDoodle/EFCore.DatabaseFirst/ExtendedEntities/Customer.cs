namespace EFCore.DatabaseFirst.Entities;

public partial class Customer
{
    public string FullName
    {
        get
        {
            return NameStyle
                ? $"{LastName}, {FirstName} {MiddleName ?? string.Empty} {Suffix ?? string.Empty}".Trim()
                : $"{FirstName} {MiddleName ?? string.Empty} {LastName} {Suffix ?? string.Empty}".Trim();
        }
    }
}