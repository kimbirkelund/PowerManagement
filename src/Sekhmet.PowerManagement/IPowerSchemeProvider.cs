using System.Collections.Immutable;

namespace Sekhmet.PowerManagement
{
    public interface IPowerSchemeProvider
    {
        IImmutableList<IPowerScheme> GetPowerSchemes();
    }
}