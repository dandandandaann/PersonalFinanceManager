using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.UnitTest;

public class CategoryServiceTests
{
    [Fact]
    public async Task DecideCategoryAsync_ShouldReturnUserCategory_WhenValidUserCategoryProvided()
    {
        // Arrange
        var spreadsheetId = "spreadsheetId";
        var userCategory = "locomoção";
        var description = "uber";
        var fakeCategories = new List<string> { "locomoção", "alimentação", "saúde" };

        // mock the sheetsAccessor to return the fake categories
        var sheetsAccessor = Substitute.For<ISheetsDataAccessor>();
        sheetsAccessor.ReadColumnValuesAsync(
              spreadsheetId,
              Arg.Any<string>(),
              Arg.Any<string>(),
              Arg.Any<int>()
         ).Returns(fakeCategories);

        var logger = Substitute.For<ILogger<CategoryService>>();

        var categoryService = new CategoryService(sheetsAccessor, logger);

        // ACT
        var result = await categoryService.DecideCategoryAsync(spreadsheetId, userCategory, description);

        // Assert
        result.ShouldBe(userCategory);
    }

    [Fact]
    public async Task DecideCategoryAsync_ShouldReturnEmpty_WhenInvalidUserCategoryProvided()
    {
        // Arrange
        var spreadsheetId = "spreadsheetId";
        var userCategory = "invalidCategory";
        var description = "uber";
        var fakeCategories = new List<string> { "locomoção", "alimentação", "saúde" };
        // mock the sheetsAccessor to return the fake categories
        var sheetsAccessor = Substitute.For<ISheetsDataAccessor>();
        sheetsAccessor.ReadColumnValuesAsync(
              spreadsheetId,
              Arg.Any<string>(),
              Arg.Any<string>(),
              Arg.Any<int>()
         ).Returns(fakeCategories);
        var logger = Substitute.For<ILogger<CategoryService>>();

        var categoryService = new CategoryService(sheetsAccessor,logger);

        // ACT
        var result = await categoryService.DecideCategoryAsync(spreadsheetId, userCategory, description);
        
        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task DecideCategoryAsync_ShouldReturnAutoCategory_WhenNoUserCategoryProvided()
    {
        // Arrange
        var spreadsheetId = "spreadsheetId";
        var userCategory = string.Empty;
        var description = "peguei um uber para o trabalho";
        var autoCategoryNames = new List<string>
        {
            "comida",
            "locomoção",
            "alimentação",
            "Transporte",
            "Transporte",
            "Lazer"
        };

        var autoCategoryPatterns = new List<string>
        {
            "jantar",
            "uber",
            "restaurante",
            "Uber",
            "99 Taxi",
            "Cinema"
        };

        var sheetsAccessor = Substitute.For<ISheetsDataAccessor>();
        
        sheetsAccessor.ReadColumnValuesAsync(
            spreadsheetId,
            "Categorizador",
            "A",
            2
        ).Returns(autoCategoryNames);

        
        sheetsAccessor.ReadColumnValuesAsync(
            spreadsheetId,
            "Categorizador",
            "B",
            2
        ).Returns(autoCategoryPatterns);

        var logger = Substitute.For<ILogger<CategoryService>>();
        var categoryService = new CategoryService(sheetsAccessor, logger);

        // Act
        var result = await categoryService.DecideCategoryAsync(spreadsheetId, userCategory, description);

        // Assert
        var expectedCategory = "locomoção";
        result.ShouldBe(expectedCategory);

    }

    [Fact]
    public async Task DecideCategoryAsync_ShouldReturnEmpty_WhenNoCategoryMatchesDescription()
    {
        // Arrange
        var spreadsheetId = "spreadsheetId";
        var userCategory = string.Empty;
        var description = "peguei um carro para o trabalho";
        var autoCategoryNames = new List<string> { "comida", "locomoção", "saúde" };
        var autoCategoryPatterns = new List<string> { "jantar", "uber", "restaurante" };
        
        var sheetsAccessor = Substitute.For<ISheetsDataAccessor>();
        
        sheetsAccessor.ReadColumnValuesAsync(
            spreadsheetId,
            "Categorizador",
            "A",
            2
        ).Returns(autoCategoryNames);
        
        sheetsAccessor.ReadColumnValuesAsync(
            spreadsheetId,
            "Categorizador",
            "B",
            2
        ).Returns(autoCategoryPatterns);

        var logger = Substitute.For<ILogger<CategoryService>>();

        var categoryService = new CategoryService(sheetsAccessor, logger);

        // Act
        var result = await categoryService.DecideCategoryAsync(spreadsheetId, userCategory, description);
        // Assert
        result.ShouldBe(string.Empty);
    }

    // quando a descrição é vazia ou nulo o codigo description = description.Trim().Normalize(); lanca uma execao
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task DecideCategoryAsync_ShouldReturnEmpty_WhenDescriptionIsNullOrEmptyOrWhitespace
(string description)
    {
        // Arrange
        var spreadsheetId = "spreadsheetId";
        var userCategory = string.Empty;

        var autoCategoryNames = new List<string> { "comida", "locomoção", "saúde" };
        var autoCategoryPatterns = new List<string> { "jantar", "uber", "restaurante" };

        var sheetsAccessor = Substitute.For<ISheetsDataAccessor>();
        sheetsAccessor.ReadColumnValuesAsync(spreadsheetId, "Categorizador", "A", 2).Returns(autoCategoryNames);
        sheetsAccessor.ReadColumnValuesAsync(spreadsheetId, "Categorizador", "B", 2).Returns(autoCategoryPatterns);

        var logger = Substitute.For<ILogger<CategoryService>>();
        var categoryService = new CategoryService(sheetsAccessor, logger);

        // Act
        var result = await categoryService.DecideCategoryAsync(spreadsheetId, userCategory, description);

        // Assert
        result.ShouldBe(string.Empty);
    }

}
