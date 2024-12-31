using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    internal class ScriptableTargetsOnBoard : ScriptableAmount
    {
        public bool allies = false;
        public bool enemies = false;
        public bool inRow = false;
        public CardType cardType = null;

        public override int Get(Entity entity)
        {
            var result = 0;
            var rows = References.Battle.GetRowIndices(entity);

            if (inRow)
            {
                foreach (var row in rows)
                {
                    if (allies) result += entity.GetAlliesInRow(row).Count(e => cardType is null || e.data.cardType == cardType);
                    if (enemies) result += entity.GetEnemiesInRow(row).Count(e => cardType is null || e.data.cardType == cardType);
                }

                return result;
            }

            if (allies) result += entity.GetAllies().Count(e => cardType is null || e.data.cardType == cardType);
            if (enemies) result += entity.GetEnemies().Count(e => cardType is null || e.data.cardType == cardType);
            return result;
        }
    }
}