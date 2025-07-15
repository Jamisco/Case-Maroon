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

    public abstract class Unit
    {
        public virtual int UnitId { get; set; }
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
    }


}
