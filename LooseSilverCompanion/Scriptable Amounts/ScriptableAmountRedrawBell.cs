using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace sleetstorm
{
    internal class ScriptableAmountRedrawBell : ScriptableAmount
    {
        public RedrawBellSystem _redrawBellSystem;
        public RedrawBellSystem redrawBellSystem => _redrawBellSystem ?? (_redrawBellSystem = UnityEngine.Object.FindObjectOfType<RedrawBellSystem>());
        public override int Get(Entity entity)
        {
            return (Sleetstorm.counter);
        }
    }
}
