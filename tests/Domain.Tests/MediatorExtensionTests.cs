using System.Reflection;
using Domain.Abstraction.Mediator;
using Domain.Extension;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Domain.Tests;

public class MediatorExtensionTests
{
    [Fact]
    public void AddMediator_RegistersMediatorAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.AddMediator(assembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var mediator = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);
        Assert.IsType<Mediator>(mediator);
    }

    [Fact]
    public void AddMediator_RegistersCommandHandlersAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.AddMediator(assembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var handler = serviceProvider.GetService<ICommandHandler<SampleCommand>>();
        Assert.NotNull(handler);
        Assert.IsType<SampleCommandHandler>(handler);
    }

    [Fact]
    public void AddMediator_DoesNotRegisterAbstractOrInterfaceTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.AddMediator(assembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var abstractHandler = serviceProvider.GetService<ICommandHandler<AbstractCommand>>();
        Assert.Null(abstractHandler);
    }

    [Fact]
    public void AddMediator_IgnoresNonCommandHandlerTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.AddMediator(assembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var nonHandler = serviceProvider.GetService<INonHandler>();
        Assert.Null(nonHandler);
    }
    
[Fact]
    public void Send_ThrowsInvalidOperationException_WhenHandlerIsNotRegistered()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ICommandHandler<SampleCommand>)))
            .Returns(null);
    
        var mediator = new Mediator(serviceProviderMock.Object);
    
        Assert.Throws<InvalidOperationException>(() => mediator.Send(new SampleCommand()));
    }
    
    [Fact]
    public void Send_CallsHandleOnRegisteredHandler()
    {
        var handlerMock = new Mock<ICommandHandler<SampleCommand>>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ICommandHandler<SampleCommand>)))
            .Returns(handlerMock.Object);
    
        var mediator = new Mediator(serviceProviderMock.Object);
        var command = new SampleCommand();
    
        mediator.Send(command);
    
        handlerMock.Verify(h => h.Handle(command), Times.Once);
    }
    
[Fact]
    public void SendWithResult_ThrowsInvalidOperationException_WhenHandlerIsNotRegistered()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ICommandHandler<SampleQueryCommand, string>)))
            .Returns(null);
    
        var mediator = new Mediator(serviceProviderMock.Object);
    
        Assert.Throws<InvalidOperationException>(() => mediator.Send<SampleQueryCommand, string>(new SampleQueryCommand()));
    }
    
    [Fact]
    public void SendWithResult_CallsHandleOnRegisteredHandlerAndReturnsResult()
    {
        var expectedResult = "Success";
        var handlerMock = new Mock<ICommandHandler<SampleQueryCommand, string>>();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<SampleQueryCommand>()))
            .Returns(expectedResult);
    
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ICommandHandler<SampleQueryCommand, string>)))
            .Returns(handlerMock.Object);
    
        var mediator = new Mediator(serviceProviderMock.Object);
        var command = new SampleQueryCommand();
    
        var result = mediator.Send<SampleQueryCommand, string>(command);
    
        Assert.Equal(expectedResult, result);
        handlerMock.Verify(h => h.Handle(command), Times.Once);
    }
    
    
    
    // Sample command and handler for testing
    public class SampleCommand : ICommand<SampleCommand> { }
    public class SampleCommandHandler : ICommandHandler<SampleCommand>
    {
        public void Handle(SampleCommand command) { }
    }
    public abstract class AbstractCommand : ICommand<AbstractCommand> { }
    public interface INonHandler { }
    
    public class SampleQueryCommand : ICommand<SampleQueryCommand, string> { }
    
    
    

}