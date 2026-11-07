using CraftFlow.SharedKernel.Enums;

namespace CraftFlow.Api.Core.Behaviors;

public interface IRequireModule
{
    AppModule RequiredModule { get; }
}