using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WildfrostHopeMod.SFX;
using WildfrostHopeMod.VFX;

namespace sleetstorm
{
    public static class VFXHelper
    {
        public static GIFLoader VFX;
        public static SFXLoader SFX;

        public static Dictionary<string, string> minibossJingles = new Dictionary<string, string>
        {
            {"megamarine.wildfrost.sleetstorm.frankenlemonphase1", "jolt"}, //CHANGE
            {"internalName2", "jingle2"}, //CHANGE
            {"internalName3", "jingle3"}  //CHANGE
        };

        public static void MinibossIntro(Entity target)
        {
            Debug.Log($"[Test] {target.data.name}");
                if (minibossJingles.ContainsKey(target.data.name))
                {
                    SFX.TryPlaySound(minibossJingles[target.data.name]);
                }
        }
    }
}
