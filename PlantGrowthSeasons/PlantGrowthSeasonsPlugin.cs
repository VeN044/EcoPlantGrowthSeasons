﻿using Eco.Core.Plugins.Interfaces;
using Eco.Core.Plugins;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eco.Core.Utils;
using Eco.Gameplay.GameActions;
using Eco.Simulation.WorldLayers.Pushers;
using Eco.Simulation.Time;

namespace PlantGrowthSeasons
{

    [Localized(true, false, "", false)]
    [TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
    public class PlantGrowthSeasonsConfig : Singleton<PlantGrowthSeasonsConfig>
    {
        [LocCategory("Global")]
        [LocDescription("Enable mod activity")]
        public bool Enable { get; set; } = false;

        [LocCategory("Global")]
        [LocDisplayName("GrowRatte change")]
        [LocDescription("Enable changing grow rate")]
        public bool GrowthRateChangeEnable { get; set; } = true;

        [LocCategory("Seasons")]
        [LocDisplayName("Spring days")]
        [LocDescription("The number of days in the spring season.")]
        public float SpringDays { get; set; } = 5f;

        [LocCategory("Seasons")]
        [LocDisplayName("Summer days")]
        [LocDescription("The number of days in the summer season.")]
        public float SummerDays { get; set; } = 10f;

        [LocCategory("Seasons")]
        [LocDisplayName("Autumn  days")]
        [LocDescription("The number of days in the autumn season.")]
        public float AutumnDays { get; set; } = 5f;

        [LocCategory("Seasons")]
        [LocDisplayName("Winter  days")]
        [LocDescription("The number of days in the winter season.")]
        public float WinterDays { get; set; } = 5f;

        [LocCategory("Seasons")]
        [LocDisplayName("Seasons shift")]
        [LocDescription("The number of days to shift seasons. If you like to start not form Spring")]
        public float ShiftDays { get; set; } = 5f;
    }


    public class PlantGrowthSeasonsPlugin :
     IModKitPlugin,
     IDisplayablePlugin,
     IInitializablePlugin,
     IShutdownablePlugin,
     IConfigurablePlugin,
     IHasDisplayTabs,
     IGUIPlugin,
     IDisplayTab
    {
        PluginConfig<PlantGrowthSeasonsConfig> config;
        public IPluginConfig PluginConfig => this.config;
        public ThreadSafeAction<object, string> ParamChanged { get; set; } = new();
        string status = string.Empty;
        public string GetStatus() => this.status;
        public string GetCategory() => "Mods";
        public override string ToString() => "PlantGrowthSeasons";

        public TimerTask timer = new TimerTask(TimeSpan.FromSeconds(10));
        public PlantGrowthSeasons plantGroupSeasons = new PlantGrowthSeasons();
        public void Initialize(TimedTask timer)
        {
            this.status = "Ready.";
            this.config = new PluginConfig<PlantGrowthSeasonsConfig>("PlantGrowthSeasonsConfig");  // Load our plugin configuration
            this.plantGroupSeasons.config = this.config;

            OnParamEnableChanged();

            //UserManager.OnUserLoggedIn.Add(this.OnUserLogin);                   // Register our OnUserLoggedIn event handler for showing players our welcome message.
        }
        public Task ShutdownAsync()
        {
            //UserManager.OnUserLoggedIn.Remove(this.OnUserLogin);                // Remove our OnUserLoggedIn event handler
            return Task.CompletedTask;
        }

        public string GetDisplayTitle() => "Status";
        public string GetDisplayText()
        {
            StringBuilder stringBuilder = new StringBuilder(1024);

            // Перенести вызовы в публичные функции этот код удалить ---------
            double curentDay = TimeUtil.SecondsToDays(WorldTime.Seconds);
            double yearLength = this.config.Config.SpringDays + this.config.Config.SummerDays + this.config.Config.AutumnDays + this.config.Config.WinterDays;
            double dayOfYear = curentDay % yearLength;
            //ConsoleLogWriter.Instance.Write("curentDay % yearLength = " + curentDay % yearLength + "\n");

            double shift = this.config.Config.ShiftDays % yearLength;

            double shiftedDayOfYear = 0;
            if (shift >= 0) shiftedDayOfYear = dayOfYear + shift > yearLength ? dayOfYear + shift - yearLength : dayOfYear + shift;
            else shiftedDayOfYear = dayOfYear + shift < 0 ? dayOfYear + shift + yearLength : dayOfYear + shift;
            stringBuilder.AppendLine($"dayOfYear is {dayOfYear}");
            stringBuilder.AppendLine($"yearLength is {yearLength}");
            stringBuilder.AppendLine($"shiftedDayOfYear is {shiftedDayOfYear}");
            // -----------------------------------------------------------------


            stringBuilder.AppendLine($"Current season id {this.plantGroupSeasons.GetSeason(WorldTime.Seconds)}");
            stringBuilder.AppendLine($"PlantGrower.GrowthRateModifier =  {PlantGrower.GrowthRateModifier}");
            //foreach (var line in this.recipeThrottle.recipeFamilyGroup)
            //    stringBuilder.AppendLine($"{line.Value} | {line.Key.DisplayName.ToString()}");
            stringBuilder.AppendLine();

            return stringBuilder.ToString();
        }
        public object GetEditObject() => this.config.Config;
        public void OnEditObjectChanged(object o, string param)
        {
            if (param == "Enable")
                OnParamEnableChanged();
        }

        private void OnParamEnableChanged()
        {
            if (this.config.Config.Enable)
            {
                this.status = "Enabled";
                this.timer.Start(() => {
                    this.plantGroupSeasons.TickParametrs();
                }, "PlantGrowthSeasonsTimer");

            }
            else
            {
                this.status = "Disabled";
                this.timer.StopAsync();
            }
        }

    }
}
