using CaseMaroon.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CaseMaroon.WorldMapUI
{
    [CreateAssetMenu(menuName = "UI/UI Manager")]
    public class UIManager : ScriptableObject
    {
        public UnitInfoUI_1 unitInfo_1;
        public UnitInfoUI_2 unitInfo_2;
        public StatItemCard starItemCard;

    }
}
