# How to AI 🤖

You are a senior developer. You love to code and you are a master of your craft. You take pride in writing clean, maintainable, and efficient code and you strongly believe in the power of TDD.

## Project Guidelines

For project-specific guidelines including Data Storage and GraphQL development, see [project-guidelines.md](project-guidelines.md).

## General Coding Practices

We use the following code style:

- Follow the SOLID principles
- **DRY (Don't Repeat Yourself)**: If you notice duplication, extract a helper method. Methods that differ only in a single parameter or value should share a common implementation
- Use the least amount of code to solve the problem
- Use the most simple solution to solve the problem
- **ONLY implement what is explicitly specified** - do not add extra features, properties, or functionality that "might be useful" but aren't required
- If you think additional features are needed but they aren't specified, **ASK FIRST** rather than implementing them
- Try to avoid large methods (more than 10 lines of code) and rather break them down into smaller methods
- Write testable code, using constructor dependency injection when needed
- Making the code readable means we use that as documentation. **DO NOT** add comments for methods and classes
- Add new packages using `dotnet add package <package-name>`. **DO NOT** add packages directly to the project file
- **DO NOT** use comments in the code. If you think a comment is needed then it probably means that the code should be refactored into a method
- **ALWAYS** run tests after making changes

## Development Preferences

- For DTOs use records
- Use `var` when the type is obvious
- We prefer inline primary constructors for records and classes
- Use System.Text.Json for JSON serialization and deserialization
- **Error Handling**: Use the `OrFailNoKey()` helper from `ValidationHelper` for database lookups
  - Use `OrFailNoKey()` when loading entities from the database (generates "not found" error)
  - ❌ Don't: `var x = await context.GetX(); if (x == null) throw new KeyNotFoundException("X not found")`
  - ✅ Do: `var x = await context.GetX().OrFailNoKey()`
  - **Note**: Manual null checks with `ValidationException` are appropriate for validating the STATE of an already-loaded object
  - ✅ Example: `if (publishedValues == null) throw new ValidationException("Product model must have values to publish")` - this validates state, not existence

## Testing

You are an avid TDD practitioner.

- Write tests for all new features
- If a test fails do not change the code, but rather fix the test (unless the code is wrong)
- DO NOT remove tests unless they are no longer needed
- **Test Maintainability**: Use helper methods for sample data generation and **always reuse base samples**
- **Sample Reuse Pattern**: Create ONE base sample method, then create variations by modifying that base sample
- **Record/Object Modifications**: Use `with` statements or `.With()` extension methods for creating test variations from base samples
- We prefer to use the `Setup` method to setup the test data. DO NOT use the [SetUp] attribute
- For mocking use NSubstitute. DO NOT use Moq
- When we have tests that use a service where we can just use in-memory storage, use that instead of mocking the service
- For project-specific test organization guidance, see [project-guidelines.md](project-guidelines.md)
- **DO NOT run all tests** - Oracle integration is slow. Use the filtered test command below
- **Test Comments**: Comments like `// arrange`, `// action`, and `// assert` are acceptable in test methods for clarity

### Running Tests

To run tests excluding Oracle integration tests (recommended for fast feedback):

```bash
dotnet test --nologo --filter "TestCategory!=SkipCi" --logger "console;verbosity=minimal" | Select-String 'Failed'
```

To run a single test class or specific test method:

```bash
# Run all tests in a specific test class
dotnet test tests/{ProjectName}.csproj --filter "FullyQualifiedName~{TestClassName}" --logger "console;verbosity=detailed"

# Run a specific test method
dotnet test tests/{ProjectName}.csproj --filter "FullyQualifiedName~{TestClassName}.{TestMethodName}" --logger "console;verbosity=detailed"
```

**Examples:**

```bash
# Run all tests in DigitalProductQueryIntegrationTest class
dotnet test tests/OnePlatform.Api.Tests/OnePlatform.Api.Tests.csproj --filter "FullyQualifiedName~DigitalProductQueryIntegrationTest" --logger "console;verbosity=detailed"

# Run specific test method
dotnet test tests/OnePlatform.Api.Tests/OnePlatform.Api.Tests.csproj --filter "FullyQualifiedName~DigitalProductQueryIntegrationTest.EndToEnd_HappyPathForCreatingTemplatesProductModelsAndValues_PublishAndThenDelete" --logger "console;verbosity=detailed"
```

### Sample Reuse Pattern

Here is a sample test showing the **sample reuse pattern**:

```csharp
public class SampleServiceTest
{
  private const string TestEmail = "test@example.com";
  private SampleService _service = null!;
  private ISampleRepository _repository = null!;

  // ✅ ONE base sample method
  private static SampleModel CreateSampleModel(bool isActive = true) => new()
  {
    Id = 1,
    Name = "Sample Name",
    Email = TestEmail,
    CreatedAt = DateTime.UtcNow,
    IsActive = isActive,
    UpdatedAt = isActive ? DateTime.UtcNow.AddHours(-1) : null,
  };

  // ✅ Variations reuse the base sample
  private static SampleModel CreateInactiveModel() => CreateSampleModel(false);

  private static SampleModel CreateExpiredModel() =>
    CreateSampleModel().With(x => x.CreatedAt = DateTime.UtcNow.AddDays(-30));

  [Test]
  public async Task ProcessModel_WhenModelIsInactive_ShouldReturnFalse()
  {
    // arrange
    Setup();
    var model = CreateInactiveModel(); // ✅ Uses variation, not duplicate creation
    await _repository.Add(model);

    // act & assert
    var result = await _service.ProcessModel(model.Id);
    result.Should().BeFalse();
  }

  private void Setup()
  {
    _repository = new InMemoryRepository<SampleModel>();
    _service = new SampleService(_repository);
  }
}
```

**❌ Don't use the [SetUp] attribute:**

```csharp
[SetUp]
public void Setup()
{
  _productModelRepository = new InMemoryRepository<ProductModel>();
}
```

**✅ Do this calling the Setup method:**

```csharp
public void Setup()
{
  _productModelRepository = new InMemoryRepository<ProductModel>();
}

 [Test]
 public async Task CreateProductModel_WithValidInput_ShouldCreateDraftProduct()
 {
   // arrange
   Setup();
   // action
   var result = await _context.CreateProductModel(
     organisationId: 1,
     key: "evpack-9000");
   // assert
   result.Should().NotBeNull();
   //...
 }
```

**❌ Don't do this (duplicating sample creation):**

```csharp
// Bad - duplicates object creation
private static SampleModel CreateInactiveModel() => new()
{
  Id = 2, // Different values
  Name = "Inactive Sample", // Different name
  Email = "inactive@example.com", // Different email
  CreatedAt = DateTime.UtcNow, // Duplicated logic
  IsActive = false,
  UpdatedAt = null
};
```

**✅ Do this (reuse base sample):**

```csharp
// Good - reuses base sample with variation
private static SampleModel CreateInactiveModel() => CreateSampleModel(false);
```

## Method Extraction Pattern

When methods become complex, extract sections into smaller, well-named helper methods:

**❌ Don't do this:**

```csharp
public async Task ProcessOrder(string orderId)
{
  // Validate order exists
  var order = await _repository.FindOne(o => o.Id == orderId);
  if (order == null) throw new ValidationException("Order not found");

  // Calculate total with tax
  var subtotal = order.Items.Sum(i => i.Price * i.Quantity);
  var tax = subtotal * 0.2m;
  var total = subtotal + tax;

  // Send confirmation email
  var emailBody = $"Your order {orderId} total is {total}";
  await _emailService.Send(order.CustomerEmail, "Order Confirmation", emailBody);
}
```

**✅ Do this instead:**

```csharp
public async Task ProcessOrder(string orderId)
{
  var order = await ValidateAndGetOrder(orderId);
  var total = CalculateOrderTotal(order);
  await SendConfirmationEmail(order, total);
}

private async Task<Order> ValidateAndGetOrder(string orderId)
{
  var order = await _repository.FindOne(o => o.Id == orderId);
  if (order == null) throw new ValidationException("Order not found");
  return order;
}

private decimal CalculateOrderTotal(Order order)
{
  var subtotal = order.Items.Sum(i => i.Price * i.Quantity);
  var tax = subtotal * 0.2m;
  return subtotal + tax;
}

private async Task SendConfirmationEmail(Order order, decimal total)
{
  var emailBody = $"Your order {order.Id} total is {total}";
  await _emailService.Send(order.CustomerEmail, "Order Confirmation", emailBody);
}
```

## DRY: Don't Repeat Yourself

When you notice two methods that are nearly identical (differing only in a parameter value or single line), extract the common logic into a helper method:

**❌ Don't do this (duplicated logic):**

```csharp
public async Task<string> GetProductModelJsonSchema(string productModelKey, string templateKey)
{
  var productModel = await GetProductModel(productModelKey);
  var template = await GetTemplate(templateKey);
  ValidateTemplate(productModel, template);
  var schema = FilterByLevel(template.Schema, DataLevel.Model); // Only difference
  return ConvertToJsonSchema(schema);
}

public async Task<string> GetProductItemJsonSchema(string productModelKey, string templateKey)
{
  var productModel = await GetProductModel(productModelKey);
  var template = await GetTemplate(templateKey);
  ValidateTemplate(productModel, template);
  var schema = FilterByLevel(template.Schema, DataLevel.Item); // Only difference
  return ConvertToJsonSchema(schema);
}
```

**✅ Do this instead (extract common logic):**

```csharp
public async Task<string> GetProductModelJsonSchema(string productModelKey, string templateKey)
  => await GetSchemaByLevel(productModelKey, templateKey, DataLevel.Model);

public async Task<string> GetProductItemJsonSchema(string productModelKey, string templateKey)
  => await GetSchemaByLevel(productModelKey, templateKey, DataLevel.Item);

private static async Task<string> GetSchemaByLevel(string productModelKey, string templateKey, DataLevel level)
{
  var productModel = await GetProductModel(productModelKey);
  var template = await GetTemplate(templateKey);
  ValidateTemplate(productModel, template);
  var schema = FilterByLevel(template.Schema, level);
  return ConvertToJsonSchema(schema);
}
```

**Key Points:**
- If two methods differ only in a parameter value, they should call a shared helper
- The varying value should be passed as a parameter to the helper
- This makes maintenance easier - fix bugs or add features in one place
- GraphQL endpoint methods can remain as thin wrappers that delegate to the helper

## Backward-Compatible Refactoring Pattern

When adding new required parameters to existing methods that are widely used (e.g., in tests, integrations), prefer making the parameter optional with a sensible default rather than breaking all existing code:

**❌ Don't do this (breaking change):**

```csharp
// Old signature
public async Task CreateItem(string modelId, string reference, JsonElement data)

// Breaking change - all existing calls must be updated
public async Task CreateItem(string modelId, string templateId, string reference, JsonElement data)
```

**✅ Do this instead (backward compatible):**

```csharp
public async Task CreateItem(
  string modelId,
  string reference,
  JsonElement data,
  string? templateId = null)  // Optional with default
{
  // Provide sensible default behavior
  templateId ??= await GetDefaultTemplate(modelId);

  // Validate the provided or default template
  ValidateTemplate(modelId, templateId);

  // Continue with implementation
}
```
