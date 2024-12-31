using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace sleetstorm
{
    public static class EyeDataAdder
    {

        public static void Eyes()
        {
            List<EyeData> list = new List<EyeData>()
            {
                Eyes("megamarine.wildfrost.sleetstorm.baph", (-0.26f, 1.52f, 1.00f, 1.00f, 0f), (0.07f, 1.55f, 1.00f, 1.00f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.ami", (-0.05f, 1.46f, 1.24f, 1.24f, 0f), (0.35f, 1.56f, 1.24f, 1.24f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.biff", (0.44f, 1.44f, 1.00f, 0.63f, 348f), (0.76f, 1.40f, 1.00f, 0.63f, 2f)),
                Eyes("megamarine.wildfrost.sleetstorm.blink", (-0.26f, 1.43f, 1.02f, 1.05f, 20f), (-0.02f, 1.58f, 0.83f, 0.92f, 20f)),
                Eyes("megamarine.wildfrost.sleetstorm.cainann", (0.58f, 1.53f, 0.74f, 0.75f, 0f), (0.29f, 1.56f, 1.00f, 0.97f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.cassiusblackmont", (-0.28f, 1.96f, 1.00f, 1.00f, 17f), (-0.58f, 1.79f, 0.80f, 0.80f, 17f)),
                Eyes("megamarine.wildfrost.sleetstorm.craigory", (0.14f, 1.60f, 0.90f, 0.86f, 13f), (-0.18f, 1.45f, 0.90f, 0.86f, 13f)),
                Eyes("megamarine.wildfrost.sleetstorm.duster", (0.08f, 1.73f, 1.00f, 0.62f, 353f)),
                Eyes("megamarine.wildfrost.sleetstorm.grimtome", (-0.13f, 1.95f, 0.53f, 0.52f, 350f), (-0.29f, 2.04f, 0.53f, 0.52f, 350f)),
                Eyes("megamarine.wildfrost.sleetstorm.hazetrailblazer", (0.00f, 1.82f, 0.73f, 0.70f, 0f), (-0.34f, 1.82f, 1.00f, 1.00f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.higgs", (0.22f, 1.55f, 1.00f, 1.00f, 0f), (-0.15f, 1.52f, 1.00f, 1.00f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.lemon", (-0.23f, 1.69f, 0.78f, 0.75f, 0f), (-0.50f, 1.62f, 0.60f, 0.57f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.lemonsis", (0.49f, 1.53f, 1.00f, 1.00f, 0f), (0.06f, 1.52f, 1.00f, 1.00f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.liltinkerson", (-0.10f, 2.25f, 1.00f, 0.53f, 351f), (-0.45f, 2.19f, 1.00f, 0.53f, 14f)),
                Eyes("megamarine.wildfrost.sleetstorm.loosesilver", (0.72f, 1.78f, 0.77f, 0.76f, 354f), (0.43f, 1.86f, 0.80f, 0.80f, 351f)),
                Eyes("megamarine.wildfrost.sleetstorm.2ufo", (1.01f, 1.88f, 1.07f, 0.55f, 42f), (-0.34f, 0.95f, 1.00f, 0.39f, 79f), (-0.57f, 0.81f, 1.00f, 0.53f, 5f)),
                Eyes("megamarine.wildfrost.sleetstorm.mersel", (0.54f, 1.64f, 0.86f, 0.86f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.oriane", (0.20f, 1.49f, 0.74f, 0.73f, 0f), (-0.09f, 1.48f, 0.64f, 0.64f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.phileap", (0.12f, 1.77f, 1.00f, 1.00f, 0f), (-0.20f, 1.70f, 1.00f, 1.00f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.pip", (0.15f, 1.73f, 0.74f, 1.00f, 351f), (-0.16f, 1.80f, 0.74f, 1.00f, 351f)),
                Eyes("megamarine.wildfrost.sleetstorm.puffle", (0.71f, 1.22f, 1.97f, 2.32f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.racca", (-0.39f, 1.43f, 1.00f, 1.00f, 0f), (-0.69f, 1.66f, 1.00f, 1.00f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.rowan", (0.49f, 1.65f, 0.87f, 1.07f, 0f), (0.31f, 1.62f, 0.87f, 1.07f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.sizzle", (0.33f, 1.62f, 0.50f, 0.50f, 0f), (-0.10f, 1.54f, 0.60f, 0.61f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.skalemoji", (0.05f, 1.51f, 0.60f, 0.60f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.sorrelmist", (0.21f, 1.82f, 0.70f, 0.74f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.starlight", (-0.29f, 1.38f, 0.70f, 0.57f, 26f), (-0.55f, 1.29f, 0.70f, 0.57f, 356f)),
                Eyes("megamarine.wildfrost.sleetstorm.talon", (0.20f, 1.86f, 0.64f, 0.65f, 8f), (-0.05f, 1.88f, 0.64f, 0.65f, 350f)),
                Eyes("megamarine.wildfrost.sleetstorm.tobias", (0.33f, 1.59f, 0.46f, 1.00f, 27f)),
                Eyes("megamarine.wildfrost.sleetstorm.yuura", (0.41f, 1.79f, 0.64f, 0.62f, 355f), (0.20f, 1.92f, 0.72f, 0.73f, 338f)),
                Eyes("megamarine.wildfrost.sleetstorm.ShellPet", (0.70f, 0.90f, 0.79f, 0.86f, 0f), (0.19f, 0.85f, 1.00f, 1.00f, 0f)),
                Eyes("megamarine.wildfrost.sleetstorm.zoomie", (0.36f, 1.38f, 1.22f, 0.74f, 43f), (0.08f, 1.37f, 1.22f, 0.74f, 323f)),
                Eyes("megamarine.wildfrost.sleetstorm.dire", (-0.41f, 0.60f, 0.48f, 0.46f, 0f), (-0.63f, 0.62f, 0.54f, 0.48f, 354f)),
                Eyes("megamarine.wildfrost.sleetstorm.ailyn", (-0.23f,1.07f,0.54f,-0.50f,0f), (-0.53f,1.07f,0.64f,-0.60f,0f)),
                Eyes("megamarine.wildfrost.sleetstorm.sallyleader", (0.36f,2.07f,1.08f,1.08f,350f), (0.08f,2.12f,1.08f,1.18f,347f)),
                Eyes("megamarine.wildfrost.sleetstorm.abbykleader", (0.05f,2.02f,0.47f,1.02f,359f), (-0.21f,2.02f,0.47f,0.73f,359f)),
                Eyes("megamarine.wildfrost.sleetstorm.vavaleader", (-0.08f,1.98f,1.08f,-0.34f,337f)),
                Eyes("megamarine.wildfrost.sleetstorm.marizleader", (-0.30f,1.88f,1.08f,1.08f,345f)),
                Eyes("megamarine.wildfrost.sleetstorm.bobleader", (0.44f,2.09f,0.90f,0.65f,26f), (0.15f,2.21f,1.45f,0.91f,299f)),
                Eyes("megamarine.wildfrost.sleetstorm.bunnileader", (0.26f,1.86f,0.81f,1.21f,6f), (-0.02f,1.8f,0.81f,1.21f,3f)),
                Eyes("megamarine.wildfrost.sleetstorm.mintyleader", (0.19f,2.13f,0.61f,1.08f,10f), (-0.02f,2.11f,0.61f,1.08f,9f)),
                Eyes("megamarine.wildfrost.sleetstorm.camilleleader", (0.56f,1.98f,0.98f,0.98f,0f), (0.22f,1.95f,1.08f,1.08f,0f)),


            };

            AddressableLoader.AddRangeToGroup("EyeData", list);

        }
            
        public static EyeData Eyes(string cardName, params (float, float, float, float, float)[] data)
        {
            EyeData eyeData = ScriptableObject.CreateInstance<EyeData>();
            eyeData.cardData = cardName;
            eyeData.name = eyeData.cardData + "_EyeData";
            eyeData.eyes = data.Select((e) => new EyeData.Eye
            {
                position = new Vector2(e.Item1, e.Item2),
                scale = new Vector2(e.Item3, e.Item4),
                rotation = e.Item5
            }).ToArray();

            return eyeData;
        }
        
    }
}
