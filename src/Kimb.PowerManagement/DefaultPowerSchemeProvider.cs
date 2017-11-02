using System.Collections.Immutable;

namespace Kimb.PowerManagement
{
    public class DefaultPowerSchemeProvider : IPowerSchemeProvider
    {
        public IImmutableList<IPowerScheme> GetPowerSchemes()
        {
            return PowerApi.GetPowerSchemes();
        }
    }
}