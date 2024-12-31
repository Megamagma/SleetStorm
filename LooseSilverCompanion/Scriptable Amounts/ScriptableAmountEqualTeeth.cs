using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    public class ScriptableAmountEqualTeeth : ScriptableAmount
    {
        public override int Get(Entity target)
        {
            List<Entity> entities = (from ally in target.GetAlliesInRow()
                                     select ally).ToList();
            int count = 0;
            foreach (Entity ally in entities)
            {
                if (ally.FindStatus("teeth") != default(StatusEffectData))
                    count += ally.FindStatus("teeth").count;
            }
            return count;
        }
    }
}