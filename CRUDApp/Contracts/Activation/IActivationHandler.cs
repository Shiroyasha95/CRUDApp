using System.Threading.Tasks;

namespace CRUDApp.Contracts.Activation
{
    public interface IActivationHandler
    {
        bool CanHandle();

        Task HandleAsync();
    }
}
