using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConfigManager
{
#if (UNITY_EDITOR)
    private static string CONFIG_PATH = "Assets/NPCSystem/PersistentData/NPC.config";
#endif

#if !(UNITY_EDITOR)
    private static string CONFIG_PATH = "PersistentData/NPC.config";
#endif

    private enum ConfigType
    {
        vehiclesAmount,
        trucksAmount,
        pedestriansAmount,
        carsIgnoreLongRoadBool,
        shouldSpawnPedestriansBool
    };

    private struct ConfigData
    {
        public uint AmountOfVehicles;
        public uint AmountOfTrucks;
        public uint AmountOfPedestrians;
        public bool AllCarsIgnoreLongRoad;
        public bool SpawnPedestrians;
    };

    private static ConfigData configDataStruct;

    public static uint AmountOfVehicles { get { return ReadData(ConfigType.vehiclesAmount).AmountOfVehicles; } }
    public static uint AmountOfTrucks { get { return ReadData(ConfigType.trucksAmount).AmountOfTrucks; } }
    public static uint AmountOfPedestrians { get { return ReadData(ConfigType.pedestriansAmount).AmountOfPedestrians; } }
    public static bool AllCarsIgnoreLongRoad { get { return ReadData(ConfigType.carsIgnoreLongRoadBool).AllCarsIgnoreLongRoad; } }
    public static bool SpawnPedestrians { get { return ReadData(ConfigType.shouldSpawnPedestriansBool).SpawnPedestrians; } }

    private static ConfigData ReadData(ConfigType ct)
    {
        string configData = System.IO.File.ReadAllText(CONFIG_PATH);
        string[] configDataArr = configData.Split(';');

        string targetProperty;
        switch (ct)
        {
            case ConfigType.carsIgnoreLongRoadBool:
                targetProperty = "AllCarsIgnoreLongRoad";
                break;
            case ConfigType.pedestriansAmount:
                targetProperty = "AmountOfPedestrians";
                break;
            case ConfigType.shouldSpawnPedestriansBool:
                targetProperty = "SpawnPedestrians";
                break;
            case ConfigType.trucksAmount:
                targetProperty = "AmountOfTrucks";
                break;
            case ConfigType.vehiclesAmount:
                targetProperty = "AmountOfVehicles";
                break;
            default:
                targetProperty = "NAN";
                break;
        }

        string dataValue = "";

        foreach (string s in configDataArr)
        {
            string ns = s.Trim(System.Environment.NewLine.ToCharArray());

            string[] str = ns.Split(':');
            if (str[0] == targetProperty)
            {
                dataValue = str[1];
                break;
            }

        }

        switch (ct)
        {
            case ConfigType.carsIgnoreLongRoadBool:
                configDataStruct.AllCarsIgnoreLongRoad = (dataValue == "true" ? true : false);
                break;
            case ConfigType.pedestriansAmount:
                configDataStruct.AmountOfPedestrians = uint.Parse("0" + dataValue);
                break;
            case ConfigType.shouldSpawnPedestriansBool:
                configDataStruct.SpawnPedestrians = (dataValue == "true" ? true : false);
                break;
            case ConfigType.trucksAmount:
                configDataStruct.AmountOfTrucks = uint.Parse("0" + dataValue);
                break;
            case ConfigType.vehiclesAmount:
                configDataStruct.AmountOfVehicles = uint.Parse("0" + dataValue);
                break;
            default:
                break;
        }


        return configDataStruct;
    }

}
