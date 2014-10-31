using System.Collections.Immutable;

namespace Sekhmet.PowerManagement
{
    public class DefaultPowerSchemeProvider : IPowerSchemeProvider
    {
        public IImmutableList<IPowerScheme> GetPowerSchemes()
        {
            return PowerApi.GetPowerSchemes();
        }
    }
}