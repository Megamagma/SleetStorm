using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace sleetstorm
{
    internal class StatusEffectAppearOnTop : StatusEffectData
    {
        public override void Init()
        {
            PatchShuffle.PostShuffle += CheckToContainer;
            Events.OnCardDraw += CheckCardDraw;
        }

        public void OnDestroy()
        {
            PatchShuffle.PostShuffle -= CheckToContainer;
            Events.OnCardDraw -= CheckCardDraw;
        }

        protected void CheckToContainer(CardContainer from, CardContainer to)
        {
            if (References.Player?.drawContainer == to)
            {
                MoveToTop();
            }
        }

        protected void CheckCardDraw(int _)
        {
            MoveToTop();
        }

        private void MoveToTop()
        {
            if (target == null || target.containers == null || GetAmount() == 0 || References.Player?.drawContainer == null) { return; }

            if (target.containers.Contains(References.Player.drawContainer))
            {
                CardContainer container = References.Player.drawContainer;
                container.Remove(target);
                container.Add(target);
            }
        }

        [HarmonyPatch(typeof(Sequences), "ShuffleTo", new Type[]
        {
        typeof(CardContainer),
        typeof(CardContainer),
        typeof(float)
        })]
        internal class PatchShuffle
        {
            internal static UnityAction<CardContainer, CardContainer> PostShuffle;
            static IEnumerator Postfix(IEnumerator __result, CardContainer fromContainer, CardContainer toContainer)
            {
                yield return __result;
                PostShuffle?.Invoke(fromContainer, toContainer);
            }
        }
    }
}
