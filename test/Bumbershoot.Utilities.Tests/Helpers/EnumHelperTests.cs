using System;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Helpers
{
    [TestFixture]
    public class EnumHelperTests
    {
        #region Setup/Teardown

        private void Setup()
        {
        }

        #endregion

        [Test]
        public void ToArray_GivenEnum_ShouldBuildArrayOfEnums()
        {
            // arrange
            Setup();
            // action
            var dayOfWeeks = EnumHelper.ToArray<DayOfWeek>();
            // assert
            dayOfWeeks.Should().Contain(DayOfWeek.Friday).And.HaveCount(7);
        }
    }
}