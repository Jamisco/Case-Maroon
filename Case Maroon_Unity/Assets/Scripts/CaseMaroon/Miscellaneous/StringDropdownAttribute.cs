using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static CaseMaroon.GameSystem.PlayerStateManager;

namespace CaseMaroon.Miscellaneous
{
    public class StringDropdownAttribute : PropertyAttribute
    {
        public static List<string> stringIds = new List<string>();


        public static void UpdateIds()
        {
            // loop through message enums
            // 
            stringIds.Clear();

            stringIds.AddRange(Enum.GetNames(typeof(InitMessage))); 
        }
        
    }
}
