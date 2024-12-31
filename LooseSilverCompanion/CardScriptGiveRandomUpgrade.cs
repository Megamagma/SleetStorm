using Deadpan.Enums.Engine.Components.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace sleetstorm
{
    internal class CardScriptGiveRandomUpgrade : CardScript
    {
        [SerializeField]
        public CardUpgradeData[] upgradePool;
        public override void Run(CardData target)
        {
            if (upgradePool != null && upgradePool.RemoveFromArray(item => item.CanAssign(target) == true).Count() > 0)
            {
                upgradePool.RemoveFromArray(item => item.CanAssign(target) == true).RandomItem().Clone().Assign(target);
            }

        }
    }
}
