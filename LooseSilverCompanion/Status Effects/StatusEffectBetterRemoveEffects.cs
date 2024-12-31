using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    public class StatusEffectBetterRemoveEffects : StatusEffectInstant
    {

        public override IEnumerator Process()
        {

            for (int j = target.statusEffects.Count - 1; j >= 0; j--)
            {

                if (target.statusEffects[j].HasDescOrIsKeyword)
                {
                    if (target.statusEffects[j].GetType() == typeof(StatusEffectWhileActiveX))
                    {
                        StatusEffectWhileActiveX activeEff = target.statusEffects[j] as StatusEffectWhileActiveX;
                        if (activeEff.active == true)
                        {
                            yield return activeEff.Deactivate();
                        }
                    }

                    foreach (Entity.TraitStacks trait in applier.traits)
                    {
                        trait.count = 0;
                        foreach (StatusEffectData passiveEffect in trait.passiveEffects)
                            yield return (object)passiveEffect.Remove();
                        trait.passiveEffects.Clear();
                        trait.effectsDisabled = true;
                        trait.init = 0;

                    }

                    yield return applier.UpdateTraits();

                    yield return target.statusEffects[j].Remove();
                }

            }

            target.traits.Clear();
            target.attackEffects = new List<CardData.StatusEffectStacks> { };
            target.data.desc = "";
            target.display.promptUpdateDescription = true;
            target.PromptUpdate();
            yield return base.Process();

        }

    }
}
