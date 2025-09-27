using Common.Crypto;

namespace Common.Tests;

public class HashHelperTests
{
    [Fact]
    public void ComputeSha256Hash_ReturnsCorrectHash_ForValidInput()
    {
        var input = "HelloWorld";
        var expected = "872e4e50ce9990d8b041330c47c9ddd11bec6b503ae9386a99da8584e9bb12c4";
        var result = HashHelper.ComputeSha256Hash(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeSha256Hash_ReturnsDifferentHashes_ForDifferentInputs()
    {
        var input1 = "HelloWorld";
        var input2 = "HelloWorld!";
        var hash1 = HashHelper.ComputeSha256Hash(input1);
        var hash2 = HashHelper.ComputeSha256Hash(input2);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeSha256Hash_ReturnsSameHash_ForSameInput()
    {
        var input = "ConsistentInput";
        var hash1 = HashHelper.ComputeSha256Hash(input);
        var hash2 = HashHelper.ComputeSha256Hash(input);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeSha256Hash_ReturnsHash_ForEmptyString()
    {
        var input = string.Empty;
        var expected = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
        var result = HashHelper.ComputeSha256Hash(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeSha256Hash_ThrowsArgumentNullException_ForNullInput()
    {
        Assert.Throws<ArgumentNullException>(() => HashHelper.ComputeSha256Hash(null));
    }
}