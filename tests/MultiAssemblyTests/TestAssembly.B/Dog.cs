using TestAssembly.A;

namespace TestAssembly.B;

/// <summary>
/// This is a class which represents a dog as defined by the interface <see cref="IDog"/>.
/// </summary>
public class Dog : Animal, IDog
{
    /// <summary>See <see cref="T:System.String"/> for details.</summary>
    public int PropertyWIthSingleSee { get; set; }

    /// <summary>
    /// See <see cref=""""/> and
    /// <see cref=""T:System.String""/> for details.
    /// </summary>
    public string PropertyWIthInalidCref { get; set; }

    /// <summary>
    /// Dog's Age
    /// </summary>
    public int Age;

    /// <summary>
    /// Class B Name of specific <see cref="Dog"/>.
    /// Not all <see cref="Animal"/>s can have a name.
    /// <example>
    ///     Dog: Dog.Name = "Rex"
    /// </example>
    /// <remarks></remarks>
    /// Be carefully with this property.
    /// </summary>
    public string Name { get; set; } = "";

    /// <inheritdoc />
    public override string Property1 { get; set; } = "";

    /// 123 <inheritdoc cref="Name" />
    public string Field2 { get; set; } = "";

    /// <summary>
    /// Field three
    /// </summary>
    public double Field3 { get; set; }


    /// <summary>
    /// Get some about <see cref="Dog"/>
    /// </summary>
    /// <returns></returns>
    public string GetInfo()
    {
        return $"Field1: {Property1}, Field2: {Field2}";
    }
}