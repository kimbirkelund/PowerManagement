using System;
using Sekhmet.PowerManagement;

namespace PowerSetter
{
    internal class Program
    {
        private static void Main()
        {
            foreach (var powerScheme in new DefaultPowerSchemeProvider().GetPowerSchemes())
            {
                Console.WriteLine(new
                                  {
                                      powerScheme.Id,
                                      powerScheme.Name
                                  });

                foreach (var settingsGroup in powerScheme.SettingsGroups)
                {
                    Console.WriteLine("\t" + new
                                             {
                                                 settingsGroup.Id,
                                                 settingsGroup.Name
                                             });

                    foreach (var setting in settingsGroup.Settings)
                    {
                        Console.WriteLine("\t\t" + new
                                                   {
                                                       setting.Id,
                                                       setting.Name,
                                                       setting.Value
                                                   });
                    }
                }
                break;
            }

            Console.ReadLine();
        }
    }
}