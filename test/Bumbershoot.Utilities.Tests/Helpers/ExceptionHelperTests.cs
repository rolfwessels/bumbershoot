using System;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Helpers
{
    [TestFixture]
    public class ExceptionHelperTests
    {
        [Test]
        public void ToFirstExceptionOfException_GivenMultipleException_ShouldDispayAsSingleString()
        {
            // arrange
            var argumentException = new ArgumentException("Test");
            var aggregateException = new AggregateException(argumentException);
            // action
            var simpleException = aggregateException.ToFirstExceptionOfException();
            // assert
            simpleException.Should().Be(argumentException);
        }

        [Test]
        public void ToSimpleException_GivenAggregateExceptionWithOneException_ShouldReturnTheFirst()
        {
            // arrange
            var argumentException = new ArgumentException("Test");
            var aggregateException = new AggregateException(argumentException);
            // action
            var simpleException = aggregateException.ToSimpleException();
            // assert
            simpleException.Should().Be(argumentException);
        }

        [Test]
        public void ToSimpleException_GivenMultipleException_ShouldConcatTheMessages()
        {
            // arrange
            var aggregateException =
                new AggregateException(new ArgumentException("Test"), new ArgumentException("Test1"));
            // action
            var simpleException = aggregateException.ToSimpleException();
            // assert
            simpleException.Message.Should().Be("Test, Test1");
        }

        [Test]
        public void ToSingleExceptionString_GivenMultipleException_ShouldDispayAsSingleString()
        {
            // arrange
            var aggregateException = new ArgumentException("Test");
            // action
            var simpleException = aggregateException.ToSingleExceptionString();
            // assert
            simpleException.Trim().Should().Be("Test");
        }
    }
}