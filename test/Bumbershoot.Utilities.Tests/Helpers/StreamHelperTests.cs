using System.IO;
using System.Text;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Helpers
{
    [TestFixture]
    public class StreamHelperTests
    {
        private string _getTempFileName;

        #region Setup/Teardown

        [OneTimeSetUp]
        public void Setup()
        {
            _getTempFileName = Path.GetTempFileName();
        }

        #endregion

        [Test]
        public void ReadToString_GivenString_ShouldInStream()
        {
            // arrange
            var input = "test".ToStream();
            // action
            var output = input.ReadToString();
            // assert
            output.Should().Be("test");
        }

        [Test]
        public void SaveTo_GivenSteam_ShouldCreateFile()
        {
            // arrange
            var input = "test".ToStream();
            // action
            var output = input.SaveTo(_getTempFileName);
            // assert
            output.Exists.Should().BeTrue();
        }

        [Test]
        public void ToBytes_GivenFileStream_ShouldConvertToBytes()
        {
            // arrange
            byte[] output;
            using (var input = GetFileStream())
            {
                output = input.ToBytes();
            }

            // assert
            output.Length.Should().BeGreaterThan(1);
        }

        [Test]
        public void ToBytes_GivenMemoryStream_ShouldConvertToBytes()
        {
            // arrange
            var input = "test".ToStream();
            // action
            var output = input.ToBytes();
            // assert
            output.Length.Should().Be(4);
        }

        [Test]
        public void ToMemoryStream_GivenFileStream_ShouldConvertToBytes()
        {
            // arrange
            MemoryStream output;
            using (var input = GetFileStream())
            {
                output = input.ToMemoryStream();
            }

            // assert
            output.Length.Should().BeGreaterThan(1);
            output.Position.Should().Be(0);
        }

        [Test]
        public void ToMemoryStream_GivenMemoryStream_ShouldConvertToBytes()
        {
            // arrange
            var input = "test".ToStream();
            // action
            var output = input.ToMemoryStream();
            // assert
            output.Length.Should().Be(4);
            output.Position.Should().Be(0);
        }

        [Test]
        public void Combine_GivenTwoByteArrays_ShouldCombineThem()
        {
            // arrange
            var input = "one".ToBytes();
            var second = "2".ToBytes();
            // action
            var output = input.Combine(second);
            // assert
            output.Length.Should().Be(4);
            output.AsUtf8String().Should().Be("one2");
        }

        [Test]
        public void Split_GivenTwoByteArrays_ShouldCombineThem()
        {
            // arrange
            var input = "one".ToBytes();
            var second = "2".ToBytes();
            // action
            var (one,two) = input.Combine(second).Split(3);
            // assert
            one.AsUtf8String().Should().Be("one");
            two.AsUtf8String().Should().Be("2");
        }



        [Test]
        public void ToStream_GivenString_ShouldInStream()
        {
            // arrange
            const string input = "test";
            // action
            var output = input.ToStream();
            // assert
            output.Length.Should().Be(4);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (File.Exists(_getTempFileName))
                File.Delete(_getTempFileName);
        }

        #region Private Methods

        private FileStream GetFileStream()
        {
            var fileInfo = "Test".ToStream().SaveTo(_getTempFileName);
            return fileInfo.OpenRead();
        }

        #endregion
    }
}