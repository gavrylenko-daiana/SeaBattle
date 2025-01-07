namespace SeaBattle.Domain.Helpers;

[AttributeUsage(AttributeTargets.Property)]
public class TypeEntityAttribute : Attribute
{
    public string Name { get; set; }
}