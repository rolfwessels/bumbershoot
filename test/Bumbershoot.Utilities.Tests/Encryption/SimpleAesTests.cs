using Bumbershoot.Utilities.Encryption;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Encryption;

public class SimpleAesTests
{
    [Test]
    [TestCase("shortKey", "shortValue")]
    [TestCase("validKey12332111", "shortValue")]
    [TestCase("longkeylongkeylongkeylongkeylongkeylongkeylongkeylongkeylongkey", "shortValue")]
    [TestCase("key", "longValuelongValuelongValuelongValuelongValuelongValuelongValue")]
    public void EncryptDecrypt_GivenInput_ShouldConsistentyEncryptAndDecrypt(string key,string decryptedValue)
    {
        // arrange
        var simpleAes = new SimpleAes();
        // action
        var encrypt = simpleAes.Encrypt(key,decryptedValue);
        var decrypt = simpleAes.Decrypt(key, encrypt);
        // assert
        encrypt.Should().NotBe(decrypt);
        decrypt.Should().Be(decryptedValue);

    }


}