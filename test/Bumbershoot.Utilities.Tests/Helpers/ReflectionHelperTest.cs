using System;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Helpers
{
    [TestFixture]
    public class ReflectionHelperTest
    {
        [Test]
        public void GetMember_GivenExpression_ShouldReturnValue()
        {
            // arrange
            var member = ReflectionHelper.GetPropertyInfo<User, Guid>(x => x.Id);
            // assert
            member.Name.Should().Be("Id");
            member.PropertyType.Name.Should().Be("Guid");
        }


        [Test]
        public void GetMemberString_GivenExpression_ShouldReturnValue()
        {
            // assert
            ReflectionHelper.GetPropertyString<User, MyClass>(x => x.Cl).Should().Be("Cl");
            ReflectionHelper.GetPropertyString<User, string>(x => x.Cl.S1).Should().Be("Cl.S1");
            ReflectionHelper.GetPropertyString<User, string>(x => x.Cl.Cl.S1).Should().Be("Cl.Cl.S1");
        }

        [Test]
        public void ExpressionToAssign_GivenUpdate_ShouldSetTheValueOnTheObject()
        {
            // arrange
            var user = new User();
            var newGuid = Guid.NewGuid();
            // action
            ReflectionHelper.ExpressionToAssign(user, x => x.Id, newGuid);
            // assert
            user.Id.Should().Be(newGuid);
        }

        #region Nested type: MyClass

        private class MyClass
        {
            public string S1 { get; set; }
            public MyClass Cl { get; set; }
        }

        #endregion

        #region Nested type: User

        private class User
        {
            public Guid Id { get; set; }
            public MyClass Cl { get; set; }
        }

        #endregion
    }
}