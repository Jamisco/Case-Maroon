using CaseMaroon.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Units
{
    public static class DefaultUnitData
    {
        // Here we shall put all the default data for the units in one easily accessible class....maybe
        public static Infantry DefaultInfantry => new Infantry()
        {
            UnitName = "Infantry",
            UnitId = "INF-001",
            Image = null,
            AtkPoints = new CombatPoints()
            {
                AgainstInfantry = 20,
                AgainstArmored = 5,
                AgainstArtillery = 15,
                AgainstAircraft = 0,
                AgainstNaval = 10,
                AgainstStructure = 10
            },
            DefPoints = new CombatPoints()
            {
                AgainstInfantry = 15,
                AgainstArmored = 3,
                AgainstArtillery = 5,
                AgainstAircraft = 2,
                AgainstNaval = 5,
                AgainstStructure = 10
            },
            HealthPoints = 100,
            EnergyPoints = 50,

            MovementType = MovementType.Feet,
            MovementPoints = 100
        };
        public static Tank DefaultTank => new Tank(null)
        {
            UnitName = "Tank",
            UnitId = "TNK-001",
            Image = null,
            AtkPoints = new CombatPoints()
            {
                AgainstInfantry = 35,
                AgainstArmored = 25,
                AgainstArtillery = 30,
                AgainstAircraft = 0,
                AgainstNaval = 10,
                AgainstStructure = 25
            },
            DefPoints = new CombatPoints()
            {
                AgainstInfantry = 30,
                AgainstArmored = 25,
                AgainstArtillery = 10,
                AgainstAircraft = 5,
                AgainstNaval = 10,
                AgainstStructure = 20
            },
            HealthPoints = 200,
            EnergyPoints = 70,
            MovementType = MovementType.Tracked,
            MovementPoints = 200
        };
        public static Artillery DefaultArtillery => new Artillery(null)
        {
            UnitName = "Artillery",
            UnitId = "ART-001",
            Image = null,
            AtkPoints = new CombatPoints()
            {
                AgainstInfantry = 30,
                AgainstArmored = 20,
                AgainstArtillery = 25,
                AgainstAircraft = 0,
                AgainstNaval = 15,
                AgainstStructure = 50
            },
            DefPoints = new CombatPoints()
            {
                AgainstInfantry = 5,
                AgainstArmored = 3,
                AgainstArtillery = 5,
                AgainstAircraft = 2,
                AgainstNaval = 3,
                AgainstStructure = 5
            },
            HealthPoints = 80,
            EnergyPoints = 60,
            MovementType = MovementType.Feet,
            MovementPoints = 60
        };

        public static T CreateDefaultUnit<T>(Sprite image) where T : UnitData
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Object:
                    if (typeof(T) == typeof(Infantry))
                    {
                        Infantry inf = DefaultInfantry;
                        inf.Image = image;
                        return (T)(object)inf;
                    }
                    else if (typeof(T) == typeof(Tank))
                    {
                        Tank tank = DefaultTank;
                        tank.Image = image;
                        return (T)(object)tank;
                    }
                    else if (typeof(T) == typeof(Artillery))
                    {
                        Artillery art = DefaultArtillery;
                        art.Image = image;
                        return (T)(object)art;
                    }
                    break;
            }

            return null;
        }
    }
}
