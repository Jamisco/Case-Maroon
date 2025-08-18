using CaseMaroon.GameSystem;
using System;
using UnityEngine;

namespace CaseMaroon.Units
{
    public enum UnitType
    {
        Infantry,
        Armored,
        Artillery,
        //Aircraft,
        //Naval,
        //Structure,
    }

    public enum MovementType
    {
        Feet,
        Tracked
    }
    
    public struct CombatPoints
    {
        public int AgainstInfantry;
        public int AgainstArmored;
        public int AgainstArtillery;
        public int AgainstAircraft;
        public int AgainstNaval;
        public int AgainstStructure;
    }

    public abstract class Unit : IEquatable<Unit>
    {
        public virtual int UnitId { get; set; }
        public virtual int PlayerId { get; set; } = (GameManager.Instance.PlayerId);
        public virtual string UnitName { get; set; }
        public abstract UnitType UnitType { get; }
        public virtual int HealthPoints { get; set; }
        public virtual int EnergyPoints { get; set; }
        public virtual int MovementPoints { get; set; }
        public virtual Vector2Int GridPosition { get; set; } = Vector2Int.left; // Default value indicating no position set
        public virtual CombatPoints AtkPoints { get; set; }
        public virtual CombatPoints DefPoints { get; set; }
        public virtual MovementType MovementType { get; set; }
        public virtual Sprite Image { get; set; }



        /// <summary>
        /// Copy fields from another UnitData instance to this instance.
        /// </summary>
        /// <param name="other"></param>
        public virtual void CopyFields(Unit other)
        {
            Image = other.Image;
            UnitName = other.UnitName;
            UnitId = other.UnitId;
            AtkPoints = other.AtkPoints;
            DefPoints = other.DefPoints;
            HealthPoints = other.HealthPoints;
            EnergyPoints = other.EnergyPoints;
            MovementPoints = other.MovementPoints;
        }

        private static int idLink = 111;
        public virtual void CreateUniqueId()
        {
            UnitId = idLink;
            idLink += 111;
        }
        public static T CreateUnit<T>() where T : Unit
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Object:
                    if (typeof(T) == typeof(Infantry))
                    {
                        Infantry inf = new Infantry();
                        return (T)(object)inf;
                    }
                    else if (typeof(T) == typeof(Tank))
                    {
                        Tank tank = new Tank();
                        return (T)(object)tank;
                    }
                    else if (typeof(T) == typeof(Artillery))
                    {
                        Artillery art = new Artillery();
                        return (T)(object)art;
                    }
                    break;
            }

            return null;
        }

        public static Unit CreateUnit(UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.Infantry:
                    return new Infantry();
                case UnitType.Armored:
                    return new Tank();
                case UnitType.Artillery:
                    return new Artillery();
                default:
                    throw new ArgumentException($"Unsupported unit type: {unitType}");
            }
        }

        public bool Equals(Unit other)
        {
            return UnitId == other.UnitId;
        }
    }


}
