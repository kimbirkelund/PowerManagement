using System.Collections.Immutable;

namespace Kimb.PowerManagement
{
    public interface IPowerSchemeProvider
    {
        IImmutableList<IPowerScheme> GetPowerSchemes();
    }
}