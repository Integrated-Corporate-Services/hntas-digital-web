using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HNTAS.Web.UI.Models;
using Xunit;

public class HeatNetworkModelTests
{
    private IList<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }

    [Theory]
    [InlineData("ValidName123", true)]
    [InlineData("", false)]
    [InlineData("NameWith$Symbol", false)]
    [InlineData("NameWith Space", false)]
    [InlineData("ThisNameIsWayTooLongToBeValidBecauseItExceedsTheMaximumAllowedLengthOfOneHundredCharactersWhichIsNotPermitted", false)]
    public void EnterHNName_Validation(string input, bool isValid)
    {
        var model = new HeatNetworkNameModel { HeatNetworkName = input };
        var results = ValidateModel(model);
        Assert.Equal(isValid, results.Count == 0);
    }

    [Theory]
    [InlineData("https://what3words.com/filled.count.soap", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void EnterHNLocation_Validation(string input, bool isValid)
    {
        var model = new HeatNetworkLocationModel { HeatNetworkLocation = input };
        var results = ValidateModel(model);
        Assert.Equal(isValid, results.Count == 0);
    }
}
