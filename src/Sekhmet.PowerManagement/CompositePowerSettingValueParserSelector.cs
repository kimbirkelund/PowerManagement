using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sekhmet.PowerManagement
{
    public class CompositePowerSettingValueParserSelector : IPowerSettingValueParserSelector
    {
        private readonly IEnumerable<IPowerSettingValueParserSelector> _selectors;

        public CompositePowerSettingValueParserSelector(IEnumerable<IPowerSettingValueParserSelector> selectors)
        {
            _selectors = (selectors ?? Enumerable.Empty<IPowerSettingValueParserSelector>()).ToImmutableList();
        }


        public IPowerSettingValueParser Select(Guid powerSchemeId, Guid powerSettingsGroupId, Guid powerSettingId, string powerSettingValueUnitSpecifier)
        {
            return _selectors.Select(s => s.Select(powerSchemeId, powerSettingsGroupId, powerSettingId, powerSettingValueUnitSpecifier))
                             .Where(p => p != null)
                             .FirstOrDefault();
        }
    }
}