using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace sleetstorm
{
    internal class StatusInstantIncreaseCounter : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            VfxStatusSystem system = GameObject.FindObjectOfType<VfxStatusSystem>();
            VfxStatusSystem.Profile profile = system.profileLookup["counter down"];
            Transform transform = target.transform;
            system.CreateEffect(profile.applyEffectPrefab, transform.position, transform.lossyScale);
            target.counter.current = Math.Min(target.counter.current + count, target.counter.max);
            SfxSystem.OneShotCheckCooldown("event:/sfx/status/sun");
            yield return base.Process();
        }
    }
}
