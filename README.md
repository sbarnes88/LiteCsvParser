# LiteCsvParser
 
Attribute your models with fields.
Add additional attributes such as if boolean what a true value is considered.
Add Max and Min length checks.
Add ability to be nullable.
See sample model

```C#
//Example
public class MySampleClass {
    [Column("name", true, MinLength = 0, MaxLength = 10)]
    public string Name { get; set; }
    //... for berevity
}
```

**In your CSV**
```csv
name,age,date
Talan,30,2021-05-20
Cindy Lou Boregard,50, 2021-09-12
```