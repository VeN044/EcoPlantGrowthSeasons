using Eco.Simulation.WorldLayers.Pushers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eco.Core.Plugins;
using Eco.Simulation.Time;
using Eco.Shared.Utils;
using Eco.Core.Utils.Logging;
using Eco.Simulation.WorldLayers;
using Eco.Simulation.WorldLayers.Layers;
using Eco.Simulation.WorldLayers.Components;
using Eco.World.Blocks;
using Eco.Gameplay.Players;

namespace PlantGrowthSeasons
{
    public enum SeasonOfYear
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }
    public class PlantGrowthSeasons
    {
        public PluginConfig<PlantGrowthSeasonsConfig>? config;

        public PlantGrowthSeasons() 
        { 
            this.config = null;
        }

        public void TickParametrs()
        {
            SeasonOfYear season = this.GetSeason(WorldTime.Seconds);

            switch (season) 
            {
                case SeasonOfYear.Spring:
                    PlantGrower.GrowthRateModifier = 0.6f;
                    break;
                case SeasonOfYear.Summer:
                    PlantGrower.GrowthRateModifier = 1f;
                    break;
                case SeasonOfYear.Autumn:
                    PlantGrower.GrowthRateModifier = 0.4f;
                    break;
                case SeasonOfYear.Winter:
                    PlantGrower.GrowthRateModifier = 0.01f;
                    break;
            }

            foreach (WorldLayerSettings layerSettings in Singleton<WorldLayerManager>.Obj.Settings)
            {
                if (layerSettings.Name == "GrasslandBiome") 
                {
                    layerSettings.Components.Clear();
                    switch (GetSeason(WorldTime.Seconds))
                    {
                        case SeasonOfYear.Spring:
                            layerSettings.Components.Add(new BiomeComponent() { TopBlock = typeof(DirtBlock) });
                            break;
                        case SeasonOfYear.Summer:
                            layerSettings.Components.Add(new BiomeComponent() { TopBlock = typeof(GrassBlock) });
                            break;
                        case SeasonOfYear.Autumn:
                            layerSettings.Components.Add(new BiomeComponent() { TopBlock = typeof(ColdForestSoilBlock) });
                            break;
                        case SeasonOfYear.Winter:
                            layerSettings.Components.Add(new BiomeComponent() { TopBlock = typeof(SnowBlock) });
                            break;
                    }
                }
            }
        }

        public SeasonOfYear GetSeason ( double Time)
        {
            double curentDay = TimeUtil.SecondsToDays(Time);
            double yearLength = this.config.Config.SpringDays + this.config.Config.SummerDays +this.config.Config.AutumnDays + this.config.Config.WinterDays;
            double dayOfYear = curentDay % yearLength;

            double shift = this.config.Config.ShiftDays % yearLength;

            double shiftedDayOfYear = 0;
            if (shift >= 0) shiftedDayOfYear = dayOfYear + shift > yearLength ? dayOfYear + shift - yearLength : dayOfYear + shift;
            else shiftedDayOfYear = dayOfYear + shift < 0 ? dayOfYear + shift + yearLength : dayOfYear + shift;

            if (shiftedDayOfYear < this.config.Config.SpringDays) return SeasonOfYear.Spring;
            else if (shiftedDayOfYear < (this.config.Config.SummerDays + this.config.Config.SpringDays)) return SeasonOfYear.Summer;
            else if (shiftedDayOfYear < (this.config.Config.AutumnDays + this.config.Config.SummerDays + this.config.Config.SpringDays)) return SeasonOfYear.Autumn;
            else if (shiftedDayOfYear < (this.config.Config.WinterDays + this.config.Config.AutumnDays + this.config.Config.SummerDays + this.config.Config.SpringDays)) return SeasonOfYear.Winter;
            else return SeasonOfYear.Summer;

        }

    }
}
