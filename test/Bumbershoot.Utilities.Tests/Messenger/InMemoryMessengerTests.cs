#nullable enable
using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Messenger;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Messenger;

public class InMemoryMessengerTests
{
    private IMessenger _messenger = null!;

    public void Setup()
    {
        _messenger = new Utilities.Messenger.InMemoryMessenger();
    }

    [TearDown]
    public void TearDown()
    {
    }


    [Test]
    public async Task Send_Given_Object_ShouldBeReceived()
    {
        // arrange
        Setup();
        var o = new object();
        string? received = null;
        _messenger.Register<SampleMessage>(o, m => received = m.Message);
        // action
        await _messenger.Send(new SampleMessage("String"));
        // assert
        received.Should().NotBeNull();
    }

    [Test]
    public async Task Send_GivenObject_ShouldBeReceivedOnOtherListener()
    {
        // arrange
        Setup();
        var o = new object();
        object? received = null;
        _messenger.Register(typeof(SampleMessage), o, m => received = m);
        // action
        await _messenger.Send(new SampleMessage("String"));
        // assert
        received.Should().NotBeNull();
    }

    [Test]
    public void Send_GivenRegisteredAndThenUnRegister_ShouldNotRelieveMessage()
    {
        // arrange
        Setup();
        var o = new object();
        string? received = null;
        _messenger.Register<SampleMessage>(o, m => received = m.Message);
        _messenger.UnRegister<SampleMessage>(o);
        // action
        _messenger.Send(new SampleMessage("String"));
        // assert
        received.Should().BeNull();
    }

    [Test]
    public void Send_GivenRegisteredAndThenUnRegisterAll_ShouldNotRelieveMessage()
    {
        // arrange
        Setup();
        var o = new object();
        string? received = null;
        _messenger.Register<SampleMessage>(o, m => received = m.Message);
        _messenger.UnRegister(o);
        // action
        _messenger.Send(new SampleMessage("String"));
        // assert
        received.Should().BeNull();
    }

    public class SampleMessage : IDisposable
    {
        public SampleMessage(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }

        public void Dispose()
        {
            Message = null!;
        }
    }
}