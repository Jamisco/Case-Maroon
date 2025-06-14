using UnityEngine;

namespace CaseMaroon.Units
{
    public enum UnitType
    {
        Infantry,
        Armored,
        Artillery,
        Aircraft,
        Naval,
        Structure,
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

    public abstract class UnitData
    {
        public virtual Sprite Image { get; set; }
        public virtual string UnitName { get; set; }
        public virtual string UnitId { get; set; }
        public virtual CombatPoints AtkPoints { get; set; }
        public virtual CombatPoints DefPoints { get; set; }
        public virtual int HealthPoints { get; set; }
        public virtual int EnergyPoints { get; set; }
        public virtual MovementType MovementType { get; set; } 
        public virtual int MovementPoints { get; set; }

        /// <summary>
        /// Copy fields from another UnitData instance to this instance.
        /// </summary>
        /// <param name="other"></param>
        public virtual void CopyFields(UnitData other)
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
    }


}
