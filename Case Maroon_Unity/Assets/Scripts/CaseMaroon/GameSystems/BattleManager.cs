using CaseMaroon.WorldMap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaseMaroon.Units
{
    [Serializable]
    public class BattleManager : MonoBehaviour
    {
        public Worldmap worldMap;
        public GameAssets unitCreator;
        public UnitInfoUI_1 prefab;

        public Canvas UnitCanvas;

        public Dictionary<Vector2Int, UnitInfoUI_1> battleUnits = new Dictionary<Vector2Int, UnitInfoUI_1>();


        private GameObject AllUnitsParent;

        private void Awake()
        {

        }
        private void Start()
        {
        }

       
        public void Update()
        {
        }


    }
}



