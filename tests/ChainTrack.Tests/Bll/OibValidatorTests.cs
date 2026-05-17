using ChainTrack.BLL.Validation;

namespace ChainTrack.Tests.Bll;

/// <summary>
/// JEDINIČNI TESTOVI POSLOVNOG SLOJA (BLL) - složena validacija OIB-a.
/// Provjerava se algoritam kontrolne znamenke (ISO 7064, MOD 11,10).
/// </summary>
public class OibValidatorTests
{
    [Theory]
    [InlineData("12345678903")]
    [InlineData("26039951106")]
    [InlineData("65120987306")]
    [InlineData("99887766550")]
    [InlineData("10293847565")]
    public void JeValjan_ispravanOib_vracaTrue(string oib)
    {
        Assert.True(OibValidator.JeValjan(oib));
    }

    [Theory]
    [InlineData("12345678900")]   // pogrešna kontrolna znamenka
    [InlineData("12345678901")]   // pogrešna kontrolna znamenka
    [InlineData("00000000000")]   // pogrešna kontrolna znamenka
    [InlineData("1234567890")]    // samo 10 znamenki
    [InlineData("123456789031")]  // 12 znamenki
    [InlineData("1234567890X")]   // sadrži slovo
    [InlineData("abcdefghijk")]   // nije broj
    [InlineData("")]              // prazno
    [InlineData("   ")]           // samo razmaci
    [InlineData(null)]            // null
    public void JeValjan_neispravanOib_vracaFalse(string? oib)
    {
        Assert.False(OibValidator.JeValjan(oib));
    }

    [Fact]
    public void JeValjan_zanemarujeRubneRazmake()
    {
        Assert.True(OibValidator.JeValjan("  12345678903  "));
    }
}
