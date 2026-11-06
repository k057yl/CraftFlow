using CraftFlow.SharedKernel.Enums.Saas;

namespace CraftFlow.Api.Common.Behaviors;

public interface IRequireModule
{
    AppModule RequiredModule { get; }
}