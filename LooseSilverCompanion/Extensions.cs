using Deadpan.Enums.Engine.Components.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace sleetstorm
{
    public static class Extensions
    {
        public static GameModifierDataBuilder WithBellAndDinger(this GameModifierDataBuilder b, WildfrostMod mod, string bell = null, string dinger = null)
        {

            //Bell Sprite: dimensions-252x352px
            Texture2D bellTex = mod.ImagePath(bell).ToTex();
            Sprite bellSprite = Sprite.Create(bellTex, new Rect(0, 0, bellTex.width, bellTex.height), new Vector2(0.5f, 0.9f), 327);
            //Tune the numbers above (0.5, 0.9, 327) if your bell sprite is not of the right dimensions.
            b.WithBellSprite(bellSprite);

            //Dinger Sprite: dimensions-90x160px
            Texture2D dingerTex = mod.ImagePath(dinger).ToTex();
            Sprite dingerSprite = Sprite.Create(dingerTex, new Rect(0, 0, dingerTex.width, dingerTex.height), new Vector2(0.5f, 1.5f), 327);
            //Tune the numbers above (0.5, 1.5, 327) if your bell dinger is not of the right dimensions.
            b.WithDingerSprite(dingerSprite);
            return b;
        }
    }
}
