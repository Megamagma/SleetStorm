using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm  //this reads the amount of ink on an enemy and then outputs a number to be used by a new status
{
    public class ScriptableAmountTargetsNull : ScriptableAmount
    {
        public override int Get(Entity target)
        {
            List<Entity> entities = (from enemy in target.GetAllEnemies()
                                     where target.targetMode.GetTargets(target, null, null).Contains(enemy)
                                     select enemy).ToList();
            int count = 0;
            foreach (Entity enemy in entities)
            {
                if (enemy.FindStatus("ink") != default(StatusEffectData))
                    count += enemy.FindStatus("ink").count;
            }
            return count;
        }
    }
}
