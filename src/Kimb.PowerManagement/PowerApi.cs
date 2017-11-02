using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Kimb.PowerManagement
{
    internal static class PowerApi
    {
        private static readonly IPowerSettingValueParserSelector _powerSettingValueParserSelector = PowerSettingValueParserSelectors.FromCurrentAssembly();

        public static IImmutableList<IPowerScheme> GetPowerSchemes()
        {
            var elementsByType = GetElementsByType(PowerEnumerateType.PowerSchemes);
            return elementsByType.Select(p => (IPowerScheme)new DefaultPowerScheme(p.Key, p.Value))
                                 .ToImmutableList();
        }

        public static IImmutableList<IPowerSetting> GetPowerSettings(Guid powerSchemeId, Guid powerSettingsGroupId)
        {
            var elementsByType = GetElementsByType(PowerEnumerateType.PowerSetting, powerSchemeId, powerSettingsGroupId);
            return elementsByType.Select(p => (IPowerSetting)new DefaultPowerSetting(powerSchemeId, powerSettingsGroupId, p.Key, p.Value))
                                 .ToImmutableList();
        }

        public static IImmutableList<IPowerSettingsGroup> GetPowerSettingsGroups(Guid powerSchemeId)
        {
            var elementsByType = GetElementsByType(PowerEnumerateType.PowerSettingsGroup, powerSchemeId);
            return elementsByType.Select(p => (IPowerSettingsGroup)new DefaultPowerSettingsGroup(powerSchemeId, p.Key, p.Value))
                                 .ToImmutableList();
        }

        public static IPowerSettingValue ReadPowerSettingValue(Guid powerSchemeId, Guid powerSettingsGroupId, Guid powerSettingId)
        {
            var unitSpecifier = ReadPowerSettingUnitSpecifier(powerSettingsGroupId, powerSettingId);
            var valuePluggedIn = ParsePowerSettingValue(powerSchemeId, powerSettingsGroupId, powerSettingId, unitSpecifier, ReadPowerSettingAcValue, ReadPowerSettingAcValueIndex);
            var valueOnBattery = ParsePowerSettingValue(powerSchemeId, powerSettingsGroupId, powerSettingId, unitSpecifier, ReadPowerSettingDcValue, ReadPowerSettingDcValueIndex);

            return new DefaultPowerSettingValue(valuePluggedIn, valueOnBattery);
        }

        private static IEnumerable<Guid> GetElementIdsByType(PowerEnumerateType type, Guid powerSchemeId = default(Guid), Guid settingSubgroupId = default(Guid))
        {
            var outputId = Guid.Empty;
            var outputIdSize = (uint)Marshal.SizeOf(typeof(Guid));

            uint index = 0;
            while (PowerEnumerate(type, powerSchemeId, settingSubgroupId, (uint)type, index++, ref outputId, ref outputIdSize) == 0)
                yield return outputId;
        }

        private static IEnumerable<KeyValuePair<Guid, string>> GetElementsByType(PowerEnumerateType type, Guid powerSchemeId = default(Guid), Guid settingSubgroupId = default(Guid))
        {
            var elementIdsByType = GetElementIdsByType(type, powerSchemeId, settingSubgroupId);

            IEnumerable<KeyValuePair<Guid, string>> elementsByType;
            switch (type)
            {
                case PowerEnumerateType.PowerSchemes:
                    elementsByType = elementIdsByType.Select(id => new KeyValuePair<Guid, string>(id, ReadFriendlyName(type, id, null, null)));
                    break;
                case PowerEnumerateType.PowerSettingsGroup:
                    elementsByType = elementIdsByType.Select(id => new KeyValuePair<Guid, string>(id, ReadFriendlyName(type, powerSchemeId, id, null)));
                    break;
                case PowerEnumerateType.PowerSetting:
                    elementsByType = elementIdsByType.Select(id => new KeyValuePair<Guid, string>(id, ReadFriendlyName(type, powerSchemeId, settingSubgroupId, id)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
            return elementsByType;
        }

        private static object ParsePowerSettingValue(Guid powerSchemeId,
                                                     Guid powerSettingsGroupId,
                                                     Guid powerSettingId,
                                                     string powerSettingValueUnitSpecifier,
                                                     Func<Guid, Guid, Guid, object> readPowerSettingValue,
                                                     Func<Guid, Guid, Guid, uint> readPowerSettingValueIndex)
        {
            var valueReader = new Func<object>(() => readPowerSettingValue(powerSchemeId, powerSettingsGroupId, powerSettingId));
            var valueIndexReader = new Func<uint>(() => readPowerSettingValueIndex(powerSchemeId, powerSettingsGroupId, powerSettingId));
            var valueMinReader = new Func<uint>(() => ReadPowerSettingMinValue(powerSettingsGroupId, powerSettingId));
            var valueMaxReader = new Func<uint>(() => ReadPowerSettingMaxValue(powerSettingsGroupId, powerSettingId));
            var valueIncrementReader = new Func<uint>(() => ReadPowerSettingIncrementValue(powerSettingsGroupId, powerSettingId));

            var parser = _powerSettingValueParserSelector.Select(powerSchemeId, powerSettingsGroupId, powerSettingId, powerSettingValueUnitSpecifier);
            if (parser == null)
            {
                return new RawValueWithDescription(valueReader(),
                                                   valueIndexReader(),
                                                   new
                                                   {
                                                       UnitSpecifier = powerSettingValueUnitSpecifier,
                                                       ValueMin = valueMinReader(),
                                                       ValueMax = valueMaxReader(),
                                                       ValueIndrement = valueIncrementReader()
                                                   });
            }

            return parser.Parse(valueReader, valueIndexReader, valueMinReader, valueMaxReader, valueIncrementReader);
        }

        private static uint PowerEnumerate(PowerEnumerateType type,
                                           Guid powerSchemeId,
                                           Guid powerSettingsGroupId,
                                           uint accessFlags,
                                           uint index,
                                           ref Guid buffer,
                                           ref uint bsufferSize)
        {
            switch (type)
            {
                case PowerEnumerateType.PowerSchemes:
                    return PowerEnumerate(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, accessFlags, index, ref buffer, ref bsufferSize);
                case PowerEnumerateType.PowerSettingsGroup:
                    return PowerEnumerate(IntPtr.Zero, ref powerSchemeId, IntPtr.Zero, accessFlags, index, ref buffer, ref bsufferSize);
                case PowerEnumerateType.PowerSetting:
                    return PowerEnumerate(IntPtr.Zero, ref powerSchemeId, ref powerSettingsGroupId, accessFlags, index, ref buffer, ref bsufferSize);
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        [DllImport("PowrProf.dll")]
        private static extern uint PowerEnumerate(IntPtr rootPowerKey,
                                                  IntPtr schemeGuid,
                                                  IntPtr subGroupOfPowerSettingGuid,
                                                  uint accessFlags,
                                                  uint index,
                                                  ref Guid buffer,
                                                  ref uint bsufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerEnumerate(IntPtr rootPowerKey,
                                                  ref Guid powerSchemeId,
                                                  IntPtr subGroupOfPowerSettingGuid,
                                                  uint accessFlags,
                                                  uint index,
                                                  ref Guid buffer,
                                                  ref uint bsufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerEnumerate(IntPtr rootPowerKey,
                                                  ref Guid powerSchemeId,
                                                  ref Guid powerSettingsGroupId,
                                                  uint accessFlags,
                                                  uint index,
                                                  ref Guid buffer,
                                                  ref uint bsufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadACValue(IntPtr rootPowerKey,
                                                    ref Guid powerSchemeId,
                                                    ref Guid powerSettingsGroupId,
                                                    ref Guid powerSettingId,
                                                    ref long type,
                                                    IntPtr buffer,
                                                    ref uint bufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadACValueIndex(IntPtr rootPowerKey,
                                                         ref Guid powerSchemeId,
                                                         ref Guid powerSettingsGroupId,
                                                         ref Guid powerSettingId,
                                                         ref uint valueIndex);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadDCValue(IntPtr rootPowerKey,
                                                    ref Guid powerSchemeId,
                                                    ref Guid powerSettingsGroupId,
                                                    ref Guid powerSettingId,
                                                    ref long type,
                                                    IntPtr buffer,
                                                    ref uint bufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadDCValueIndex(IntPtr rootPowerKey,
                                                         ref Guid powerSchemeId,
                                                         ref Guid powerSettingsGroupId,
                                                         ref Guid powerSettingId,
                                                         ref uint valueIndex);

        private static void PowerReadFriendlyName(Guid schemeGuid, Guid subGroupOfPowerSettingGuid, Guid powerSettingGuid, IntPtr buffer, ref uint bufferSize)
        {
            PowerReadFriendlyName(IntPtr.Zero,
                                  ref schemeGuid,
                                  ref subGroupOfPowerSettingGuid,
                                  ref powerSettingGuid,
                                  buffer,
                                  ref bufferSize);
        }

        private static void PowerReadFriendlyName(Guid schemeGuid, Guid subGroupOfPowerSettingGuid, IntPtr buffer, ref uint bufferSize)
        {
            PowerReadFriendlyName(IntPtr.Zero,
                                  ref schemeGuid,
                                  ref subGroupOfPowerSettingGuid,
                                  IntPtr.Zero,
                                  buffer,
                                  ref bufferSize);
        }

        private static void PowerReadFriendlyName(Guid schemeGuid, IntPtr buffer, ref uint bufferSize)
        {
            PowerReadFriendlyName(IntPtr.Zero,
                                  ref schemeGuid,
                                  IntPtr.Zero,
                                  IntPtr.Zero,
                                  buffer,
                                  ref bufferSize);
        }

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadFriendlyName(IntPtr rootPowerKey,
                                                         ref Guid schemeGuid,
                                                         ref Guid subGroupOfPowerSettingGuid,
                                                         ref Guid powerSettingGuid,
                                                         IntPtr buffer,
                                                         ref uint bufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadFriendlyName(IntPtr rootPowerKey,
                                                         ref Guid schemeGuid,
                                                         ref Guid subGroupOfPowerSettingGuid,
                                                         IntPtr powerSettingGuid,
                                                         IntPtr buffer,
                                                         ref uint bufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadFriendlyName(IntPtr rootPowerKey,
                                                         ref Guid schemeGuid,
                                                         IntPtr subGroupOfPowerSettingGuid,
                                                         IntPtr powerSettingGuid,
                                                         IntPtr buffer,
                                                         ref uint bufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadValueIncrement(IntPtr rootPowerKey,
                                                           ref Guid powerSettingsGroupId,
                                                           ref Guid powerSettingId,
                                                           ref uint valueIncrement);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadValueMax(IntPtr rootPowerKey,
                                                     ref Guid powerSettingsGroupId,
                                                     ref Guid powerSettingId,
                                                     ref uint valueMax);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadValueMin(IntPtr rootPowerKey,
                                                     ref Guid powerSettingsGroupId,
                                                     ref Guid powerSettingId,
                                                     ref uint valueMin);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadValueUnitsSpecifier(IntPtr rootPowerKey,
                                                                ref Guid powerSettingsGroupId,
                                                                ref Guid powerSettingId,
                                                                IntPtr buffer,
                                                                ref uint bufferSize);

        private static string ReadFriendlyName(PowerEnumerateType type, Guid? powerSchemeId, Guid? settingSubgroupId, Guid? individualSettingId)
        {
            uint nameSize = 1024;
            IntPtr namePointer = Marshal.AllocHGlobal((int)nameSize);

            try
            {
                switch (type)
                {
                    case PowerEnumerateType.PowerSchemes:
                        Debug.Assert(powerSchemeId != null, "powerSchemeId != null");
                        PowerReadFriendlyName(powerSchemeId.Value, namePointer, ref nameSize);
                        break;
                    case PowerEnumerateType.PowerSettingsGroup:
                        Debug.Assert(settingSubgroupId != null, "settingSubgroupId != null");
                        Debug.Assert(powerSchemeId != null, "powerSchemeId != null");
                        PowerReadFriendlyName(powerSchemeId.Value, settingSubgroupId.Value, namePointer, ref nameSize);
                        break;
                    case PowerEnumerateType.PowerSetting:
                        Debug.Assert(settingSubgroupId != null, "settingSubgroupId != null");
                        Debug.Assert(individualSettingId != null, "individualSettingId != null");
                        Debug.Assert(powerSchemeId != null, "powerSchemeId != null");
                        PowerReadFriendlyName(powerSchemeId.Value, settingSubgroupId.Value, individualSettingId.Value, namePointer, ref nameSize);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("type");
                }

                string friendlyName = Marshal.PtrToStringUni(namePointer);
                return friendlyName;
            }
            finally
            {
                Marshal.FreeHGlobal(namePointer);
            }
        }

        private static object ReadPowerSettingAcValue(Guid powerSchemeId, Guid powerSettingsGroupId, Guid powerSettingId)
        {
            uint valueSize = 1024;
            IntPtr valuePointer = Marshal.AllocHGlobal((int)valueSize);
            long type = 0;
            PowerReadACValue(IntPtr.Zero,
                             ref powerSchemeId,
                             ref powerSettingsGroupId,
                             ref powerSettingId,
                             ref type,
                             valuePointer,
                             ref valueSize);

            var value = Marshal.ReadInt32(valuePointer);
            return value;
        }

        private static uint ReadPowerSettingAcValueIndex(Guid powerSchemeId, Guid powerSettingsGroupId, Guid powerSettingId)
        {
            uint valueIndex = 0;
            PowerReadACValueIndex(IntPtr.Zero,
                                  ref powerSchemeId,
                                  ref powerSettingsGroupId,
                                  ref powerSettingId,
                                  ref valueIndex);
            return valueIndex;
        }

        private static object ReadPowerSettingDcValue(Guid powerSchemeId, Guid powerSettingsGroupId, Guid powerSettingId)
        {
            uint valueSize = 1024;
            IntPtr valuePointer = Marshal.AllocHGlobal((int)valueSize);
            long type = 0;
            PowerReadDCValue(IntPtr.Zero,
                             ref powerSchemeId,
                             ref powerSettingsGroupId,
                             ref powerSettingId,
                             ref type,
                             valuePointer,
                             ref valueSize);

            var value = Marshal.ReadInt32(valuePointer);
            return value;
        }

        private static uint ReadPowerSettingDcValueIndex(Guid powerSchemeId, Guid powerSettingsGroupId, Guid powerSettingId)
        {
            uint valueIndex = 0;
            PowerReadDCValueIndex(IntPtr.Zero,
                                  ref powerSchemeId,
                                  ref powerSettingsGroupId,
                                  ref powerSettingId,
                                  ref valueIndex);
            return valueIndex;
        }

        private static uint ReadPowerSettingIncrementValue(Guid powerSettingsGroupId, Guid powerSettingId)
        {
            uint valueMin = 0;
            PowerReadValueIncrement(IntPtr.Zero,
                                    ref powerSettingsGroupId,
                                    ref powerSettingId,
                                    ref valueMin);

            return valueMin;
        }

        private static uint ReadPowerSettingMaxValue(Guid powerSettingsGroupId, Guid powerSettingId)
        {
            uint valueMin = 0;
            PowerReadValueMax(IntPtr.Zero,
                              ref powerSettingsGroupId,
                              ref powerSettingId,
                              ref valueMin);

            return valueMin;
        }

        private static uint ReadPowerSettingMinValue(Guid powerSettingsGroupId, Guid powerSettingId)
        {
            uint valueMin = 0;
            PowerReadValueMin(IntPtr.Zero,
                              ref powerSettingsGroupId,
                              ref powerSettingId,
                              ref valueMin);

            return valueMin;
        }

        private static string ReadPowerSettingUnitSpecifier(Guid powerSettingsGroupId, Guid powerSettingId)
        {
            uint unitSpecifierSize = 1024;
            IntPtr unitSpecifierPointer = Marshal.AllocHGlobal((int)unitSpecifierSize);

            var hresult = PowerReadValueUnitsSpecifier(IntPtr.Zero,
                                                       ref powerSettingsGroupId,
                                                       ref powerSettingId,
                                                       unitSpecifierPointer,
                                                       ref unitSpecifierSize);
            if (hresult != 0)
                return null;

            string unitSpecifier = Marshal.PtrToStringUni(unitSpecifierPointer);
            return unitSpecifier;
        }

        private enum PowerEnumerateType : uint
        {
            PowerSchemes = 16,
            PowerSettingsGroup = 17,
            PowerSetting = 18
        }
    }
}