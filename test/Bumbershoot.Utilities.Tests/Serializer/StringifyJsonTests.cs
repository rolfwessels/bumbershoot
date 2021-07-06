using Bumbershoot.Utilities.Serializer;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Serializer
{
    [TestFixture]
    public class StringifyJsonTests
    {
        private StringifyJson _stringifyJson;

        #region Setup/Teardown

        public void Setup()
        {
            _stringifyJson = new StringifyJson();
        }

        [TearDown]
        public void TearDown()
        {

        }

        #endregion
        [Test]
        public void Serialize_GivenSampleValue_ShouldSerializeToString()
        {
            // arrange
            Setup();
            // action
            var serialize = _stringifyJson.Serialize(new SampleSerialize { One = 1, Two = "Two" });
            // assert
            serialize.Should().Be(@"{""One"":1,""Two"":""Two""}");
        }

        [Test]
        public void Deserialize_GivenSampleValue_ShouldDeserializeFromString()
        {
            // arrange
            Setup();
            // action
            var serialize = _stringifyJson.Deserialize<SampleSerialize>(@"{""One"":1,""Two"":""Two""}");
            // assert
            serialize.One.Should().Be(1);
            serialize.Two.Should().Be("Two");
        }

        public class SampleSerialize
        {
            public int One { get; set; }
            public string Two { get; set; }
        }

    }
}