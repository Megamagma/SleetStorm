using Deadpan.Enums.Engine.Components.Modding;
using HarmonyLib;
using LooseSilverCompanion;
using sleetstorm;
using sleetstorm.Status_Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WildfrostHopeMod.SFX;
using WildfrostHopeMod.Utils;
using WildfrostHopeMod.VFX;
using BattleEditor;
using static UnityEngine.UI.CanvasScaler;

public class Sleetstorm : WildfrostMod
{
    public static Sleetstorm Instance;
    public Sleetstorm(string modDirectory) : base(modDirectory)
    {
        Instance = this;
    }

    public override string GUID => "megamarine.wildfrost.sleetstorm";
    public override string[] Depends => new string[] { "hope.wildfrost.vfx", "mhcdc9.wildfrost.battle" };
    public override string Title => "Sleet Storm";
    public override string Description => "More units join the fight against the Frost!" +
        "\n\n" +
        "A mod meant to feel like the original game, and balanced around it!" +
        "\n\n" +
        "Currently Adds:\r\n\r\n1 Tribe\r\n6 Pets\r\n30 Units\r\n26 Items\r\n18 Charms\r\n11 Clunkers\r\n6 Summons\r\n11 Keywords" +
        "\n\n" +
        "Credits" +
        "\n\n" +
        "Megamarine - Artist and Main Coder\r\nArtist Of Obsessions - Artist and concept design\r\nRedmondalizarin47 - Artist and concept design\r\nOwlart18 - https://mapletail.carrd.co/ - Owner of Talon and Oriane, Which are used with permision\r\nHeartless - Artist\r\nLizzybutt - Artist\r\nPsych0naut - Artist\r\nMocksalad - Artist\r\nMagicalTophat - Artist\r\nWhirlwindGale - Artist\r\nAviaviator - Artist\r\nSuspiciously Spiffy Crow (FungEMP) - For the Steamed Crabs idea\r\n\r\nAnd thank you! To the wonderful people of the modding community of Wildfrost Discord. Whom this would not be made without. I'd like to specifically thank Josh, Micheal, Abigail, Hopeful, Lost, and Shortcake. Thank you guys. Im glad to have met you all."
        ;


    public static List<object> assets = new List<object>();
    private bool preLoaded = false;

    //this is here to allow our icon to appear in the text box of cards
    public TMP_SpriteAsset assetSprites;
    public override TMP_SpriteAsset SpriteAsset => assetSprites;

    private T TryGet<T>(string name) where T : DataFile
    {
        T data;
        if (typeof(StatusEffectData).IsAssignableFrom(typeof(T)))
            data = base.Get<StatusEffectData>(name) as T;
        else
            data = base.Get<T>(name);

        if (data == null)
            throw new Exception($"TryGet Error: Could not find a [{typeof(T).Name}] with the name [{name}] or [{Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID(name, this)}]");

        return data;
    }
    private CardData.StatusEffectStacks SStack(string name, int amount) => new CardData.StatusEffectStacks(TryGet<StatusEffectData>(name), amount);
    private CardData.TraitStacks TStack(string name, int count) => new CardData.TraitStacks(TryGet<TraitData>(name), count);

    //PhotoMode - Remember to turn it on in the Load() and Unload()

    //private void CompanionPhoto(Scene scene)
    //{
    //    if (scene.name == "Town")
    //    {
    //        References.instance.StartCoroutine(CompanionPhoto2());
    //    }
    //}

    //private static IEnumerator CompanionPhoto2()
    //{
    //    string[] newcompanions = 
    //    {
    //        "megamarine.wildfrost.sleetstorm.bluebertha",
    //        "megamarine.wildfrost.sleetstorm.willowthetrain",
    //        "megamarine.wildfrost.sleetstorm.teethjar",
    //        "megamarine.wildfrost.sleetstorm.tattooneedle",
    //        "megamarine.wildfrost.sleetstorm.frostberries",
    //        "megamarine.wildfrost.sleetstorm.cubecake",
    //        "megamarine.wildfrost.sleetstorm.jaysonmask",
    //        "megamarine.wildfrost.sleetstorm.hogsteria",
    //        "megamarine.wildfrost.sleetstorm.berryboom",
    //        "megamarine.wildfrost.sleetstorm.jayson",
    //        
    //    };
    
    
    //    yield return SceneManager.WaitUntilUnloaded("CardFramesUnlocked");
    //    yield return SceneManager.Load("CardFramesUnlocked", SceneType.Temporary);
    //    CardFramesUnlockedSequence sequence = GameObject.FindObjectOfType<CardFramesUnlockedSequence>();
    //    TextMeshProUGUI titleObject = sequence.GetComponentInChildren<TextMeshProUGUI>(true);
    //    titleObject.text = $"New Items, Clunkers and Shades!";
    //    yield return sequence.StartCoroutine("CreateCards", newcompanions);
    //}

    private StatusEffectDataBuilder
        StatusCopy(string oldName, string newName)
    {
        StatusEffectData data = TryGet<StatusEffectData>(oldName).InstantiateKeepName(); //Copies the status effect
        data.name = newName; //Changes its name
        StatusEffectDataBuilder builder = data.Edit<StatusEffectData, StatusEffectDataBuilder>(); //Wraps the status effect in a builder
        builder.Mod = this;  //Gives the builder context.
        return builder;
    }

    private CardDataBuilder CardCopy(string oldName, string newName)
    {
        CardData data = TryGet<CardData>(oldName).InstantiateKeepName();
        data.name = GUID + "." + newName;
        CardDataBuilder builder = data.Edit<CardData, CardDataBuilder>();
        builder.Mod = this;
        return builder;
    }

    private ClassDataBuilder TribeCopy(string oldName, string newName)
    {
        ClassData data = TryGet<ClassData>(oldName).InstantiateKeepName();
        data.name = GUID + "." + newName;
        ClassDataBuilder builder = data.Edit<ClassData, ClassDataBuilder>();
        builder.Mod = this;
        return builder;
    }

    private CardScript GiveUpgrade(string name = "Crown") //Give a crown
    {
        CardScriptGiveUpgrade script = ScriptableObject.CreateInstance<CardScriptGiveUpgrade>(); //This is the standard way of creating a ScriptableObject
        script.name = $"Give {name}";                               //Name only appears in the Unity Inspector. It has no other relevance beyond that.
        script.upgradeData = TryGet<CardUpgradeData>(name);
        return script;
    }

    private CardScript AddRandomHealth(int min, int max) //Boost health by a random amount
    {
        CardScriptAddRandomHealth health = ScriptableObject.CreateInstance<CardScriptAddRandomHealth>();
        health.name = "Random Health";
        health.healthRange = new Vector2Int(min, max);
        return health;
    }

    private CardScript AddRandomDamage(int min, int max) //Boost damage by a ranom amount
    {
        CardScriptAddRandomDamage damage = ScriptableObject.CreateInstance<CardScriptAddRandomDamage>();
        damage.name = "Give Damage";
        damage.damageRange = new Vector2Int(min, max);
        return damage;
    }

    private CardScript AddRandomCounter(int min, int max) //Increase counter by a random amount
    {
        CardScriptAddRandomCounter counter = ScriptableObject.CreateInstance<CardScriptAddRandomCounter>();
        counter.name = "Give Counter";
        counter.counterRange = new Vector2Int(min, max);
        return counter;
    }

    private T[] DataList<T>(params string[] names) where T : DataFile => names.Select((s) => TryGet<T>(s)).ToArray();

    public static Sleetstorm instance;

    private void FixImage(Entity entity)
    {
        if (entity.display is Card card && !card.hasScriptableImage) //These cards should use the static image
        {
            card.mainImage.gameObject.SetActive(true);               //And this line turns them on
        }
    }

    private RewardPool CreateRewardPool(string name, string type, DataFile[] list)
    {
        RewardPool pool = ScriptableObject.CreateInstance<RewardPool>();
        pool.name = name;
        pool.type = type;            //The usual types are Units, Items, Charms, and Modifiers.
        pool.list = list.ToList();
        return pool;
    }

    public static string TribeTitleKey => "megamarine.wildfrost.sleetstorm" + ".TribeTitle";
    public static string TribeDescKey => "megamarine.wildfrost.sleetstorm" + ".TribeDesc";

    //Call this method in Load()
    private void CreateLocalizedStrings()
    {
        StringTable uiText = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English);
        uiText.SetString(TribeTitleKey, "Toonkind");                                       //Create the title
        uiText.SetString(TribeDescKey, 
            "Coming from the world of Einquell are a race of, well, Toons! Here to cause trouble for the Frost" +
            "\n\n" +
            "The tribe draws strangth from ink and practical bits, and has a variety of viable strategies." +
            "The laws of physics bend to their will, all in the service of \"The Gag\".");                                         //Create the description.


    }



    private void CreateModAssets()
    {
        VFXHelper.VFX = new GIFLoader(null, ImagePath("Anim"));
        VFXHelper.VFX.RegisterAllAsApplyEffect();

        VFXHelper.SFX = new SFXLoader(this.ImagePath("Sounds"));
        VFXHelper.SFX.RegisterAllSoundsToGlobal();

        //Tribes Code/Assets go here

        assets.Add(TribeCopy("Clunk", "Toonkind")                   //Snowdweller = "Basic", Shadmancer = "Magic"
        .WithFlag("Images/ToonkindFlag.png")                    //Loads your DrawFlag.png in your Images subfolder of your mod folder
        .WithSelectSfxEvent(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/status/ink"))    //Shuffling sound
                                                                                                       //The above line may need one of the FMOD references
            .SubscribeToAfterAllBuildEvent(   //New lines start here
            (data) =>                         //Other tutorials typically write out delegate here, this is the condensed notation (data is assumed to be ClassData)
            {
                GameObject gameObject = data.characterPrefab.gameObject.InstantiateKeepName();   //Copy the previous prefab
                UnityEngine.Object.DontDestroyOnLoad(gameObject);                                //GameObject may be destroyed if their scene is unloaded. This ensures that will never happen to our gameObject
                gameObject.name = "Player (Tutorial.Toonkind)";                                      //For comparison, the clunkmaster character is named "Player (Clunk)"
                data.characterPrefab = gameObject.GetComponent<Character>();                     //Set the characterPrefab to our new prefab

                data.leaders = DataList<CardData>(
                    "bobleader",
                    "marizleader",
                    "ondanleader",
                    "sallyleader",
                    "abbykleader",
                    "mintyleader",
                    "vavaleader",
                    "bunnileader",
                    "camilleleader");  //Leaders Go Here

                Inventory inventory = ScriptableObject.CreateInstance<Inventory>();
                inventory.deck.list = DataList<CardData>(
                    "megamarine.wildfrost.sleetstorm.fountainfalchion",
                    "megamarine.wildfrost.sleetstorm.fountainfalchion",
                    "megamarine.wildfrost.sleetstorm.springpoweredmallet",
                    "megamarine.wildfrost.sleetstorm.vaudevillehook",
                    "megamarine.wildfrost.sleetstorm.handbuzzer",
                    "megamarine.wildfrost.sleetstorm.snowpieslinshot",
                    "megamarine.wildfrost.sleetstorm.sunkazoo",
                    "megamarine.wildfrost.sleetstorm.wellofinspiration",
                    "megamarine.wildfrost.sleetstorm.blackinkhoney").ToList(); //Starting Deck
                data.startingInventory = inventory;

                RewardPool toonkindunitpool = CreateRewardPool("ToonkindUnitPool", "Units", DataList<CardData>(
                    "pip",
                    "duster",
                    "lemon",
                    "loosesilver",
                    "theengineer",
                    "sorrelmist",
                    "baph",
                    "higgs"
                    ));

                RewardPool toonkinditempool = CreateRewardPool("ToonkindItemPool", "Items", DataList<CardData>(
                    "BoltHarpoon",
                    "FlashWhip",
                    "PepperFlag",
                    "SporePack",
                    "Nullifier",
                    "Dittostone",
                    "DragonflamePepper",
                    "HongosHammer",
                    "OhNo",
                    "Madness",
                    "willowtotem",
                    "truffletotem",
                    "willowthetrain",
                    "pinkberrydispenser",
                    "FoggyBrew",
                    "LuminShard",
                    "EyeDrops",
                    "Peppereaper",
                    "Peppering",
                    "ShellShield",
                    "Junberry",
                    "Wrenchy",
                    "frostberries",
                    "toxictonic",
                    "tattooneedle",
                    "bluebertha",
                    "bonesculpture",
                    "teethjar"));

                RewardPool toonkindcharmpool = CreateRewardPool("ToonkindCharmPool", "Charms", DataList<CardUpgradeData>(
                    "CardUpgradeCordyceps",
                    "CardUpgradeSpiky",
                    "CardUpgradeTeethWhenHit",
                    "CardUpgradeAcorn",
                    "CardUpgradeBom",
                    "CardUpgradeConsumeOverload",
                    "CardUpgradeOverload",
                    "CardUpgradeEffigy",
                    "CardUpgradeMime",
                    "CardUpgradeScrap",
                    "CardUpgradeShellOnKill",
                    "CardUpgradeShroom",
                    "CardUpgradeSpice",
                    "CardUpgradeInk",
                    "CardUpgradeShroomReduceHealth",
                    "CardUpgradeReel"));

                data.rewardPools = new RewardPool[]
                {
                    toonkindunitpool,
                    toonkinditempool,
                    toonkindcharmpool,
                    Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("GeneralUnitPool"),
                    Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("GeneralItemPool"),
                    Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("GeneralCharmPool"),
                    Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("GeneralModifierPool"),
                    Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("SnowUnitPool"),         //
                    Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("SnowItemPool"),         //The snow pools are not Snowdwellers, there are general snow units/cards/charms.
                    Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("SnowCharmPool"),        //
                };
            })
        );

        //Tribe Leaders go here

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("ondanleader", "Ondan")
            .SetSprites("Ondan.png", "Ondan BG.png")
            .SetStats(10, 4, 4)
            .WithCardType("Leader")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("On Card Played Apply Spice To Allies", 1),
                    SStack("On Card Played Reduce Counter AllyAhead", 1)
                };
                data.createScripts = new CardScript[]
                {
                    GiveUpgrade(),
                    AddRandomHealth(-1,3),
                    AddRandomDamage(-1,2),
                    AddRandomCounter(0,1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind",1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("marizleader", "Mariz Meadowglow")
            .SetSprites("Mariz.png", "Mariz BG.png")
            .SetStats(6, 1, 4)
            .WithCardType("Leader")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Wildmagic", 3),
                    TStack("Toonkind",1)
                };
                data.createScripts = new CardScript[]
                {
                    GiveUpgrade(),
                    AddRandomHealth(0,2),
                    AddRandomDamage(0,1),
                    AddRandomCounter(-1,0)
                };

            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("bobleader", "Bob")
            .SetSprites("Bob.png", "Bob BG.png")
            .SetStats(3, null, 3)
            .WithCardType("Leader")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("On Turn Heal Allies In Row", 2),
                    SStack("On Card Deployed RadRadio To Board", 1),
                    SStack("On Card Played Trigger Rad Radio", 1)
                };
                data.createScripts = new CardScript[]
                {
                    GiveUpgrade(),
                    AddRandomHealth(0,2),
                    AddRandomCounter(0,1)
                };

            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("sallyleader", "Sally Forth")
            .SetSprites("SallyForth.png", "SallyForth BG.png")
            .SetStats(4, 10, 8)
            .WithCardType("Leader")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Omni Immunity", 1),
                    SStack("Block", 4)
                };
                data.createScripts = new CardScript[]
                {
                    GiveUpgrade(),
                    AddRandomHealth(-1,0),
                    AddRandomDamage(-2,2),
                    AddRandomCounter(-1,2)
                };

            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("abbykleader", "Abby K")
            .SetSprites("AbbyK.png", "AbbyK BG.png")
            .SetStats(14, 3, 5)
            .WithCardType("Leader")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("When Hit Apply Overload To Attacker", 1)
                };
                data.createScripts = new CardScript[]
                {
                    GiveUpgrade(),
                    AddRandomHealth(-2,2),
                    AddRandomDamage(-1,2),
                    AddRandomCounter(-1,1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind",1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("mintyleader", "Minty")
            .SetSprites("Minty.png", "Minty BG.png")
            .SetStats(10, 3, 3)
            .WithCardType("Leader")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("On Kill Calm Ally Behind", 2)
                };
                data.createScripts = new CardScript[]
                {
                    GiveUpgrade(),
                    AddRandomHealth(-2,1),
                    AddRandomDamage(-1,1),
                    AddRandomCounter(0,1)
                };

            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("vavaleader", "Vava")
            .SetSprites("VavaVonVixen.png", "VavaVonVixen BG.png")
            .SetStats(7, 0, 3)
            .WithCardType("Leader")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("MultiHit", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Protective", 2),
                    TStack("Toonkind",1)
                };
                data.createScripts = new CardScript[]
                {
                    GiveUpgrade(),
                    AddRandomHealth(-1,1),
                    AddRandomDamage(0,0),
                    AddRandomCounter(0,1)
                };

            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("bunnileader", "Bunni")
            .SetSprites("Bunni.png", "Bunni BG.png")
            .SetStats(7, 5, 6)
            .WithCardType("Leader")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("On Turn Increase Max Health To AllyBehind", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Longshot", 1)
                };
                data.createScripts = new CardScript[]
                {
                    GiveUpgrade(),
                    AddRandomHealth(-2,1),
                    AddRandomDamage(-1,1),
                    AddRandomCounter(-1,0)
                };

            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("camilleleader", "Camille")
            .SetSprites("Camille.png", "Camille BG.png")
            .SetStats(7, 3, 3)
            .WithCardType("Leader")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Investigate The Draw Pile When Deployed", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind", 1)
                };
                data.createScripts = new CardScript[]
                {
                    GiveUpgrade(),
                    AddRandomHealth(-1,1),
                    AddRandomDamage(-1,1),
                    AddRandomCounter(0,1)
                };

            })
            );


        //Sprite Assets go here

        assetSprites = HopeUtils.CreateSpriteAsset("assetSprites", directoryWithPNGs: this.ImagePath("Sprites"), textures: new Texture2D[] { }, sprites: new Sprite[] { ImagePath("jolted.png").ToSprite() });

        foreach (var character in assetSprites.spriteCharacterTable)
        {
            character.scale = 1.3f;
        }

        // Make sure the Icon variable in CreateIconKeyword, the name in CreateIcon and the .png name are all the same. in this case jolted.
        this.CreateIconKeyword("joltedkeyword", "Jolted", "Take damage after triggering | Does not count down!", "jolted"
        , Color(255, 255, 255), Color(255, 232, 101), Color(255, 232, 101), Color(255, 232, 101));

        this.CreateIconKeyword("maniakeyword", "Mania", "Trigger multiple times | Clears after triggering", "mania"
        , Color(255, 255, 255), Color(153, 50, 50), Color(153, 50, 50), Color(153, 50, 50));

        this.CreateIconKeyword("omniimmunekeyword", "Omni Immunity", "Immune to <keyword=megamarine.wildfrost.sleetstorm.debuffed>", "omniimmune"
        , Color(255, 255, 255), Color(222, 81, 66), Color(222, 81, 66), Color(222, 81, 66));

        this.CreateIconKeyword("inkimmunekeyword", "Ink Immunity", "Immune to <sprite name=ink>", "inkimmune"
        , Color(255, 255, 255), Color(222, 81, 66), Color(222, 81, 66), Color(222, 81, 66));

        this.CreateIconKeyword("inkresistkeyword", "Resists Ink", "Can only have a maximum of 1<sprite name=ink>", "inkresist"
        , Color(255, 255, 255), Color(222, 81, 66), Color(222, 81, 66), Color(222, 81, 66));

        this.CreateIconKeyword("calmkeyword", "Calm", "Reduce max <keyword=counter> for every three <sprite name=calm>|Halves when damage is taken", "calm"
        , Color(245, 169, 184), Color(91, 206, 250), Color(255, 255, 255), Color(255, 255, 255));


        //make sure you icon is in both the images folder and the sprites subfolder
        this.CreateIcon("jolted", ImagePath("jolted.png")
            .ToSprite(), "jolted", "counter", Color(134, 75, 20), Color(232, 141, 56), new KeywordData[] { TryGet<KeywordData>("joltedkeyword") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

        this.CreateIcon("mania", ImagePath("mania.png")
            .ToSprite(), "mania", "spice", Color(44, 8, 22), Color(153, 50, 50), new KeywordData[] { TryGet<KeywordData>("maniakeyword") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

        this.CreateIcon("omniimmune", ImagePath("omniimmune.png")
            .ToSprite(), "omniimmune", "spice", UnityEngine.Color.black, new KeywordData[] { TryGet<KeywordData>("omniimmunekeyword") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIcon("inkimmune", ImagePath("inkimmune.png")
            .ToSprite(), "inkimmune", "spice", UnityEngine.Color.black, new KeywordData[] { TryGet<KeywordData>("inkimmunekeyword") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIcon("inkresist", ImagePath("inkresist.png")
            .ToSprite(), "inkresist", "spice", UnityEngine.Color.black, new KeywordData[] { TryGet<KeywordData>("inkresistkeyword") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = false;

        this.CreateIcon("calm", ImagePath("calm.png")
            .ToSprite(), "calm", "counter", UnityEngine.Color.white, Color(238, 151, 172), new KeywordData[] { TryGet<KeywordData>("calmkeyword") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

        this.CreateIcon("fakecalm", ImagePath("calm.png")
            .ToSprite(), "fakecalm", "counter", UnityEngine.Color.white, Color(238, 151, 172), new KeywordData[] { TryGet<KeywordData>("calmkeyword") })
            .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

        //Add new status effects here


        assets.Add(  //Increase the damage of the companion for that one hit using the ink from the target, then return it to normal
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectBonusDamageEqualToX>("Bonus Damage Equal to Target's Null")
            .WithText("Before attacking gain temporary damage equal to targeted units <keyword=null>")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectBonusDamageEqualToX)data).on = StatusEffectBonusDamageEqualToX.On.ScriptableAmount;
                ((StatusEffectBonusDamageEqualToX)data).scriptableAmount = ScriptableAmount.CreateInstance<ScriptableAmountTargetsNull>();
                ((StatusEffectBonusDamageEqualToX)data).effectType = "ink";
            }));

        assets.Add(  //On hit, add teeth to random ally
            StatusCopy("When Hit Apply Block To RandomAlly", "When Hit Apply Teeth To RandomAlly")
            .WithText("When hit, apply <{a}><keyword=teeth> to a random ally")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenHit)data).effectToApply = TryGet<StatusEffectData>("Teeth");
            })

            );

        assets.Add(  //increase the damage of the companion for that one hit using the teeth in ally row, then return to normal.
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectBonusDamageEqualToX>("Bonus Damage Equal to Allies in Row Teeth")
            .WithText("Deal additional damage equal to <keyword=teeth> in ally row")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectBonusDamageEqualToX)data).on = StatusEffectBonusDamageEqualToX.On.ScriptableAmount;
                ((StatusEffectBonusDamageEqualToX)data).scriptableAmount = ScriptableAmount.CreateInstance<ScriptableAmountEqualTeeth>();
            })

            );

        assets.Add(  //take health from allies in row
            StatusCopy("On Card Played Take Health From Allies", "On Card Played Take Health From Allies In Row")
            .WithText("Take {e} from allies in row")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;
                ((StatusEffectApplyXOnCardPlayed)data).countsAsHit = true;
            })

            );

        assets.Add(  //when hit apply 1 of four different status to a random ally, Spice, Teeth, Shell or Frost
        new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyRandomWhenHit>("When Hit Apply Spice Shell Teeth Or Frost To RandomAlly")
            .WithText("When hit, apply <{a}> of either <keyword=spice>, <keyword=teeth>, <keyword=shell>, <keyword=frost> to a random ally")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyRandomWhenHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomAlly;
                ((StatusEffectApplyRandomWhenHit)data).effectsToapply = new StatusEffectData[] { TryGet<StatusEffectData>("Spice"), TryGet<StatusEffectData>("Shell"), TryGet<StatusEffectData>("Teeth"), TryGet<StatusEffectData>("Frost") };
            })

            );

        assets.Add(  //Boss Effect: when hit apply 1 of four different status to a random enemy, Bom, Ink, Shroom or Block
        new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyRandomWhenHit>("When Hit Apply Vim Null Shroom Or Block To RandomEnemy")
            .WithText("When hit, apply <{a}> of either <keyword=weakness>, <keyword=null>, <keyword=shroom>, <keyword=block> to a random enemy")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyRandomWhenHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
                ((StatusEffectApplyRandomWhenHit)data).effectsToapply = new StatusEffectData[] { TryGet<StatusEffectData>("Weakness"), TryGet<StatusEffectData>("Null"), TryGet<StatusEffectData>("Shroom"), TryGet<StatusEffectData>("Block") };
            })

            );

        assets.Add(  //Apply Demonize to enemy row
             StatusCopy("On Card Played Apply Snow To EnemiesInRow", "On Card Played Apply Demonize To EnemiesInRow")
             .WithText("Apply <{a}><keyword=demonize> to enemies in the row")
             .WithCanBeBoosted(true)
             .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
             {
                 ((StatusEffectApplyXOnCardPlayed)data).effectToApply = TryGet<StatusEffectData>("Demonize");
             })

        );

        assets.Add( //Summon Berryboom
            StatusCopy("Summon Beepop", "Summon Berryboom")
            .WithText("Summon <card=megamarine.wildfrost.sleetstorm.berryboom>")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectSummon)data).summonCard = TryGet<CardData>("megamarine.wildfrost.sleetstorm.berryboom");
            })
        );

        assets.Add( //On kill heal all allies
            StatusCopy("On Kill Heal To Self & AlliesInRow", "On Kill Heal Allies")
            .WithText("On kill, restore <{a}><keyword=health> to all allies")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnKill)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                ((StatusEffectApplyXOnKill)data).targetMustBeAlive = false;
            })
        );

        assets.Add( //Summon perplotoo on the enemy side
            StatusCopy("Summon Enemy Popper", "Summon Enemy Perplotoo")
            .WithText("Summon <card=megamarine.wildfrost.sleetstorm.perplotoo> on the enemy side")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectSummon)data).summonCard = TryGet<CardData>("megamarine.wildfrost.sleetstorm.perplotoo");
            })
        );

        assets.Add( //Summon perplotoo on the enemy side
            StatusCopy("Summon Enemy Popper", "Summon Enemy Jayson")
            .WithText("Summon <card=megamarine.wildfrost.sleetstorm.jayson> on the enemy side")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectSummon)data).summonCard = TryGet<CardData>("megamarine.wildfrost.sleetstorm.jayson");
            })
        );

        assets.Add
            (new StatusEffectDataBuilder(this)  //Apply haze to a random ally
            .Create<StatusEffectApplyXOnTurn>("On Action Apply Haze To Random Ally")
            .WithCanBeBoosted(true)
            .WithText("Apply <{a}><keyword=haze> to random ally")
            .WithStackable(false)
            .WithVisible(false)
            .FreeModify(delegate (StatusEffectApplyXOnTurn data)
            {
                data.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomAlly;
                data.effectToApply = TryGet<StatusEffectData>("Haze");
            })

        );

        assets.Add(  // When health lost apply equal frost to attacker
            StatusCopy("When Health Lost Apply Equal Spice To Self", "When Health Lost Apply Equal Frost To Attacker")
            .WithText("When <keyword=health> lost, apply equal <keyword=frost> to the attacker")
            .WithCanBeBoosted(false)
            .FreeModify(delegate (StatusEffectApplyXWhenHealthLost data)
            {
                data.applyToFlags = StatusEffectApplyXWhenHealthLost.ApplyToFlags.Attacker;
                data.effectToApply = TryGet<StatusEffectData>("Frost");
                data.eventPriority = 1;
            })

            );

        assets.Add(  //Heal allies in row
            StatusCopy("On Turn Heal Allies", "On Turn Heal Allies In Row")
            .WithText("Heal {e} to allies in row")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnTurn)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;
            })

            );

        assets.Add(
            new StatusEffectDataBuilder(this) //Triggers when self or ally loses scrap
            .Create<StatusEffectBetterNova>("Trigger When Self Or Ally Loses Scrap")
            .WithCanBeBoosted(false)
            .WithText("Trigger when self or ally loses <keyword=scrap>")
            .WithStackable(true)
            .WithVisible(false)
            .WithIsReaction(true)
            .FreeModify(delegate (StatusEffectBetterNova data)
            {
                data.applyToFlags = StatusEffectBetterNova.ApplyToFlags.Self;
                data.statusType = "scrap";
                data.allies = true;
                data.affectedBySnow = true;
                data.descColorHex = "F99C61";
                data.eventPriority = 999;
                data.isReaction = true;
                data.stackable = true;
                data.textInsert = "<keyword=scrap>";
                data.stackable = true;
                data.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
            })

            );

        assets.Add(
        new StatusEffectDataBuilder(this)
            .Create<StatusEffectBetterRemoveEffects>("Remove Effects Of Target")
            .WithCanBeBoosted(false)
            .WithText("Remove the effects and status of the target")

            );

        assets.Add(  //Trigger Clunker Ahead
            StatusCopy("On Card Played Trigger RandomAlly", "On Card Played Trigger Clunker Ahead")
            .WithText("Trigger clunker ahead")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;
                ((StatusEffectApplyXOnCardPlayed)data).eventPriority = 999;
                TargetConstraintIsCardType clunkerconstraint = new TargetConstraintIsCardType();
                clunkerconstraint.allowedTypes = new CardType[] { TryGet<CardType>("Clunker") };
                ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[] { clunkerconstraint };

            })
            );

        assets.Add( //When hit Add Lost Health as Shell to a random ally
            StatusCopy("When Hit Add Health Lost To RandomAlly", "When Hit Add Health Lost as Shell To Random Ally")
            .WithText("When hit, add lost <keyword=health> as <keyword=shell> to a random ally")
            .WithCanBeBoosted(false)
            .FreeModify(delegate (StatusEffectApplyXWhenHit data)
            {
                data.effectToApply = TryGet<StatusEffectData>("Shell");
            })

            );

        //Summon Aberrant Shade effect

        assets.Add(
            StatusCopy("Summon Dregg", "Summon Aberrant Shade")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectSummon)data).summonCard = TryGet<CardData>("megamarine.wildfrost.sleetstorm.aberrantshade");
            })
            );

        assets.Add(
            StatusCopy("Instant Summon Dregg", "Instant Summon Aberrant Shade")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon Aberrant Shade");
            })
            );

        assets.Add(
            StatusCopy("When Destroyed Summon Dregg", "When Destroyed Summon Aberrant Shade")
            .WithText("When destroyed, summon {0}")
            .WithTextInsert("<card=megamarine.wildfrost.sleetstorm.aberrantshade>")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenDestroyed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Aberrant Shade");
            })
            );

        //End of summon aberrant shade effect

        assets.Add(  //reduce the max counter of a summon
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyToSummon>("Reduce Max Counter To Summon")
            .WithCanBeBoosted(true)
            .WithType("")
            .FreeModify(data =>
            {
                var status = data as StatusEffectApplyToSummon;
                status.effectToApply = TryGet<StatusEffectData>("Reduce Max Counter");
            })
            );

        assets.Add(  //count down counter of allies in row
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectReduceCounter>("On Card Played Reduce Counter Row")
            .WithCanBeBoosted(true)
            .WithText("Count down <keyword=counter> by <{a}> of allies in row")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;
                ((StatusEffectApplyXOnCardPlayed)data).effectToApply = TryGet<StatusEffectData>("Reduce Counter");
            })

            );

        assets.Add(  //apply demonize to self
            StatusCopy("On Turn Apply Shell To Self", "On Turn Apply Demonize To Self")
            .WithText("Gain <{a}><keyword=demonize>")
            .WithCanBeBoosted(true)
            .FreeModify(delegate (StatusEffectApplyXOnTurn data)
            {
                data.effectToApply = TryGet<StatusEffectData>("Demonize");
            })

            );

        assets.Add(
            StatusCopy("On Card Played Apply Shroom To RandomEnemy", "On Card Played Shroom Equal To Health To RandomEnemy")
            .WithText("Apply <keyword=shroom> equal to <keyword=health> to random enemy")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).scriptableAmount = ScriptableAmount.CreateInstance<ScriptableCurrentHealth>();
            })

            );

        assets.Add(  // When health lost apply equal teeth to self
            StatusCopy("When Health Lost Apply Equal Spice To Self", "When Health Lost Apply Equal Teeth To Self")
            .WithText("When <keyword=health> lost, gain equal <keyword=teeth>")
            .WithCanBeBoosted(false)
            .FreeModify(delegate (StatusEffectApplyXWhenHealthLost data)
            {
                data.effectToApply = TryGet<StatusEffectData>("Teeth");
                data.eventPriority = 1;
            })

            );

        assets.Add(  // Draws this card first whenever deck is drawn
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectAppearOnTop>("Shuffles To Top")
            .WithCanBeBoosted(false)
            .WithType("")
            );

        assets.Add( // Knock target back 1 space
            StatusCopy("On Hit Push Target", "Knockback")
            .WithCanBeBoosted(false)
            .WithText($"<keyword={Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID("push", this)}>")
            .WithType("")
            );

        //assets.Add(
        //    new StatusEffectDataBuilder(this)
        //    .Create<StatusEffectUnderhanded>("Bypass On Hit")
        //    .WithCanBeBoosted(false)
        //    .WithText($"")
        //    .WithType("")
        //    .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
        //    {
        //        ((StatusEffectUnderhanded)data).doesDamage = true;
        //        ((StatusEffectUnderhanded)data).target = ;
        //    })
        //    );

        assets.Add( //Adds Zoomlin to the rightmost card in hand
            StatusCopy("On Card Played Add Zoomlin To Random Card In Hand", "On Card Played Add Zoomlin To Rightmost Card In Hand")
            .WithCanBeBoosted(false)
            .WithText("Add <keyword=zoomlin> to the rightmost card in your hand")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                var WithoutZoomlinOrNoomlin = ScriptableObject.CreateInstance<TargetConstraintRightMostWithoutTrait>();
                WithoutZoomlinOrNoomlin.withoutTraits = new TraitData[]
                {
                    TryGet<TraitData>("Zoomlin"),
                    TryGet<TraitData>("Noomlin")
                };
                WithoutZoomlinOrNoomlin.name = "Does Not Have Zoomlin or Noomlin";

                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Zoomlin");
                ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Hand;
                ((StatusEffectApplyX)data).applyConstraints = new TargetConstraintRightMostWithoutTrait[]
                {
                WithoutZoomlinOrNoomlin
                };
            })

            );

        assets.Add( //when hit apply ink to the attacker fixed
            StatusCopy("When Hit Apply Ink To Attacker", "When Hit Apply Ink To The Attacker 2")
            .WithCanBeBoosted(true)
            .WithText("When hit, apply <{a}><keyword=null> to the attacker")
            .WithType("")

            );

        assets.Add(  //damages debuffed tagets
            StatusCopy("On Hit Damage Snowed Target", "On Hit Damage Debuffed Target")
            .WithCanBeBoosted(true)
            .WithText("Deal <{a}> additional damage to targets with <keyword=megamarine.wildfrost.sleetstorm.debuffed>")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {

                var snowconstraint = ScriptableObject.CreateInstance<TargetConstraintHasStatusType>();
                snowconstraint.statusType = "snow";
                var bomconstraint = ScriptableObject.CreateInstance<TargetConstraintHasStatusType>();
                bomconstraint.statusType = "vim";
                var demonconstraint = ScriptableObject.CreateInstance<TargetConstraintHasStatusType>();
                demonconstraint.statusType = "demonize";
                var frostconstraint = ScriptableObject.CreateInstance<TargetConstraintHasStatusType>();
                frostconstraint.statusType = "frost";
                var hazeconstraint = ScriptableObject.CreateInstance<TargetConstraintHasStatusType>();
                hazeconstraint.statusType = "haze";
                var inkconstraint = ScriptableObject.CreateInstance<TargetConstraintHasStatusType>();
                inkconstraint.statusType = "ink";
                var overburnconstraint = ScriptableObject.CreateInstance<TargetConstraintHasStatusType>();
                overburnconstraint.statusType = "overload";
                var shroomconstraint = ScriptableObject.CreateInstance<TargetConstraintHasStatusType>();
                shroomconstraint.statusType = "shroom";
                var joltedconstraint = ScriptableObject.CreateInstance<TargetConstraintHasStatusType>();
                joltedconstraint.statusType = "jolted";


                TargetConstraintOr sufferingfromstatus = ScriptableObject.CreateInstance<TargetConstraintOr>(); ;
                sufferingfromstatus.constraints = new TargetConstraint[] {
                    snowconstraint,
                    bomconstraint,
                    demonconstraint,
                    frostconstraint,
                    hazeconstraint,
                    inkconstraint,
                    overburnconstraint,
                    shroomconstraint,
                    joltedconstraint };
                ((StatusEffectApplyXOnHit)data).applyConstraints = new TargetConstraint[] { sufferingfromstatus };
            })
            );

        assets.Add( //makes shroom not count down
            StatusCopy("Halt Spice", "Halt Shroom")
            .WithCanBeBoosted(false)
            .WithType("")
            .FreeModify(delegate (StatusEffectHaltX data)
            {
                data.effectToHalt = TryGet<StatusEffectData>("Shroom");
                data.ignoreSilence = false;
            })
            );

        assets.Add( //temporary keyword sick
            StatusCopy("Temporary Unmovable", "Temporary Sick")
            .WithCanBeBoosted(false)
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectTemporaryTrait)data).trait = TryGet<TraitData>("Sick");
            })
            );

        assets.Add( //While active, adds keyword sick to all enemies
            StatusCopy("While Active Unmovable To Enemies", "While Active Sick to Enemies")
            .WithCanBeBoosted(false)
            .WithText($"While active, add <keyword={GUID}.sick> to all enemies")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Sick");

            })
            );

        assets.Add( //heals and cleanses allies in the row
            StatusCopy("On Card Played Heal & Cleanse To Allies", "On Card Played Heal & Cleanse Allies In Row")
            .WithCanBeBoosted(true)
            .WithText("Restore <{a}><keyword=health> and Cleanse allies in the row")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;
            })
            );

        assets.Add( //Summon Larry Samuel
            StatusCopy("Summon Beepop", "Summon Larry")
            .WithText("Summon <card=megamarine.wildfrost.sleetstorm.larrysamuel>")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectSummon)data).summonCard = TryGet<CardData>("megamarine.wildfrost.sleetstorm.larrysamuel");
            })
        );

        assets.Add(  //lose scrap after all triggers
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXAfterTurn>("On Card Played Lose Scrap To Self After Turn")
            .WithText("Lose <{a}><keyword=scrap> after triggering")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Lose Scrap");
                ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

            })
            );

        assets.Add(  //Temporary wild
            StatusCopy("Temporary Soulbound", "Temporary Wild")
            .WithCanBeBoosted(false)
            .WithType("")
            .FreeModify(delegate (StatusEffectTemporaryTrait effect)
            {
                effect.trait = TryGet<TraitData>("Wild");
            })
            );

        assets.Add(  //while active, adds temporary wild to all summoned allies
            StatusCopy("While Active Barrage To Allies", "While Active Wild To Summoned Allies")
            .WithText("While active, add <keyword=wild> to all <keyword=summoned> allies")
            .WithCanBeBoosted(false)
            .FreeModify(delegate (StatusEffectWhileActiveX effect)
            {
                effect.applyToFlags = StatusEffectWhileActiveX.ApplyToFlags.Allies;
                var summoned = ScriptableObject.CreateInstance<TargetConstraintHasTrait>();
                summoned.trait = TryGet<TraitData>("Summoned");
                effect.applyConstraints = new TargetConstraint[] { summoned };
            })
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Wild");
            })
            );

        assets.Add( //Summon Hogsteria
            StatusCopy("Summon Beepop", "Summon Hogsteria")
            .WithText("Summon <card=megamarine.wildfrost.sleetstorm.hogsteria>")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectSummon)data).summonCard = TryGet<CardData>("megamarine.wildfrost.sleetstorm.hogsteria");
            })
            );

        assets.Add(  //damage cannot go up
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectCannotGainDamage>("Cannot Gain Damage")
            .WithCanBeBoosted(false)
            );

        assets.Add(
            StatusCopy("On Turn Apply Demonize To RandomEnemy", "On Turn Apply Demonize To RandomEnemy 2")
            .WithText("Apply <{a}><keyword=demonize> to a random enemy")
            .WithCanBeBoosted(true)
            );

        assets.Add(
            StatusCopy("Trigger Against Ally When Ally Is Hit", "Trigger Against Attacker When Ally Is Hit")
            .WithText("Trigger against the attacker when an ally is hit")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenAllyIsHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Attacker;
                ((StatusEffectApplyXWhenAllyIsHit)data).eventPriority = 1;
            })
            );

        assets.Add(
            StatusCopy("Trigger Against Ally When Ally Is Hit", "Trigger Against RandomEnemy When Ally Is Hit")
            .WithText("Trigger against a random enemy when an ally is hit")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenAllyIsHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
                ((StatusEffectApplyXWhenAllyIsHit)data).eventPriority = 1;
            })
            );

        assets.Add( //makes ink not count down
            StatusCopy("Halt Spice", "Halt Null")
            .WithCanBeBoosted(false)
            .WithType("")
            .FreeModify(delegate (StatusEffectHaltX data)
            {
                data.effectToHalt = TryGet<StatusEffectData>("Null");
            })
            );

        assets.Add(  //Temporary tattooed
            StatusCopy("Temporary Soulbound", "Temporary Tattooed")
            .WithCanBeBoosted(false)
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectTemporaryTrait)data).trait = TryGet<TraitData>("Tattooed");
            })
            );

        assets.Add(
            StatusCopy("On Card Played Add Soulbound To RandomEnemy", "On Card Played Add Tattooed To Enemy")
            .WithCanBeBoosted(false)
            .WithText($"Add <keyword={GUID}.tattooed> to the target")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
                ((StatusEffectApplyXOnCardPlayed)data).effectToApply = TryGet<StatusEffectData>("Temporary Tattooed");
            })
            );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectWhileActiveX>("While Active Multihit Equal Health")
            .WithCanBeBoosted(false)
            .WithText("While active gain <keyword=frenzy> equal to <keyword=health>")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectWhileActiveX)data).scriptableAmount = ScriptableAmount.CreateInstance<ScriptableCurrentHealth>();
                ((StatusEffectWhileActiveX)data).effectToApply = TryGet<StatusEffectData>("MultiHit");
                ((StatusEffectWhileActiveX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            })
            );

        assets.Add(
            StatusCopy("When Enemies Attack Apply Demonize To Attacker", "When Enemies Attack Apply Overburn To Attacker")
            .WithCanBeBoosted(true)
            .WithText("Before an enemy attacks, apply <{a}><keyword=overload> to them")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenEnemiesAttack)data).effectToApply = TryGet<StatusEffectData>("Overload");
                ((StatusEffectApplyXWhenEnemiesAttack)data).eventPriority = 999999;
            })
            );

        assets.Add(
            StatusCopy("On Card Played Add Soulbound To RandomEnemy", "On Card Played Add Sick To Enemy")
            .WithCanBeBoosted(false)
            .WithText($"Add <keyword={GUID}.sick> to the target")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
                ((StatusEffectApplyXOnCardPlayed)data).effectToApply = TryGet<StatusEffectData>("Temporary Sick");
            })
            );

        assets.Add(  //Trigger random enemy in row
            StatusCopy("On Card Played Trigger RandomAlly", "On Card Played Trigger RandomEnemyInRow")
            .WithText("Trigger a random enemy in the row")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemyInRow;
            })

            );

        assets.Add
            (new StatusEffectDataBuilder(this)  //Apply haze to a random ally
            .Create<StatusEffectApplyXOnTurn>("On Action Apply Haze To RandomEnemy")
            .WithCanBeBoosted(true)
            .WithText("Apply <{a}><keyword=haze> to a random enemy")
            .WithStackable(false)
            .WithVisible(false)
            .FreeModify(delegate (StatusEffectApplyXOnTurn data)
            {
                data.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
                data.effectToApply = TryGet<StatusEffectData>("Haze");
            })

        );

        assets.Add  // Status Effect for Jolted
            (new StatusEffectDataBuilder(this)
            .Create<StatusEffectJolted>("Jolted")
            .WithDoesDamage(true)
            .WithStackable(true)
            .WithOffensive(true)
            .WithTextInsert("{a}")
            .WithIcon_VFX("jolted", "jolted", "joltedkeyword", VFXMod_StatusEffectHelpers.LayoutGroup.health)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                data.applyFormatKey = TryGet<StatusEffectData>("Shroom").applyFormatKey;
                data.targetConstraints = new TargetConstraint[] { ScriptableObject.CreateInstance<TargetConstraintIsUnit>() };
                data.removeOnDiscard = true;
                data.applierOwner = default;
            })
            );

        assets.Add
            (new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenDrawn>("When Drawn Count Down Random Ally")
            .WithCanBeBoosted(true)
            .WithText("When Drawn, count down <keyword=counter> by <{a}> of a random ally")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenDrawn)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomAlly;
                ((StatusEffectApplyXWhenDrawn)data).effectToApply = TryGet<StatusEffectData>("Reduce Counter");
            })
            );

        assets.Add(
            StatusCopy("On Hit Equal Overload To Target", "On Hit Equal Null To Target")
            .WithText("Apply <keyword=null> equal to damage dealt")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnHit)data).effectToApply = TryGet<StatusEffectData>("Null");
            })
            );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectInstantSwapToOtherRow>("Instant Swap Target Row")
            .WithCanBeBoosted(false)
            );

        assets.Add(
            StatusCopy("On Hit Pull Target", "On Hit Bump Target")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
                ((StatusEffectApplyXOnHit)data).effectToApply = TryGet<StatusEffectData>("Instant Swap Target Row");
            })
            );

        assets.Add(
            StatusCopy("While Active Increase Attack To Junk In Hand", "While Active Increase Attack To fountainfalchion In Hand")
            .WithCanBeBoosted(true)
            .WithText("While active, add <+{a}><keyword=attack> to all <card=megamarine.wildfrost.sleetstorm.fountainfalchion> in your hand")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                var fountainfalchion = ScriptableObject.CreateInstance<TargetConstraintIsSpecificCard>();
                fountainfalchion.allowedCards = new CardData[] { TryGet<CardData>("fountainfalchion") };
                ((StatusEffectWhileActiveX)data).applyConstraints = new TargetConstraint[] { fountainfalchion };
            })
            );

        assets.Add
            (new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenDrawn>("When Drawn Snow Random Enemy")
            .WithCanBeBoosted(false)
            .WithText("When Drawn, apply <{a}><keyword=snow> to a random enemy")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenDrawn)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
                ((StatusEffectApplyXWhenDrawn)data).effectToApply = TryGet<StatusEffectData>("Snow");
            })
            );

        assets.Add
            (new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenRedrawHitForItems>("When Redraw Hit Apply Mania To Self")
            .WithCanBeBoosted(true)
            .WithText("When <Redraw Bell> is hit, gain <x{a}><keyword=maniakeyword>")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenRedrawHitForItems)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                ((StatusEffectApplyXWhenRedrawHitForItems)data).effectToApply = TryGet<StatusEffectData>("Mania");
            })
            );

        assets.Add(  //count down counter of ally ahead
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectReduceCounter>("On Card Played Reduce Counter AllyAhead")
            .WithCanBeBoosted(true)
            .WithText("Count down <keyword=counter> ally ahead by <{a}>")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;
                ((StatusEffectApplyXOnCardPlayed)data).effectToApply = TryGet<StatusEffectData>("Reduce Counter");
            })

            );

        assets.Add(  //Applies a random status from a set list, used for Wild Magic Trait
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyRandomOnCardPlayed>("Apply Random Status Instant Wild Magic")
            .WithType("")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyRandomOnCardPlayed)data).effectsToapply = new StatusEffectData[]
                {
                    TryGet<StatusEffectData>("Weakness"),
                    TryGet<StatusEffectData>("Null"),
                    TryGet<StatusEffectData>("Shroom"),
                    TryGet<StatusEffectData>("Snow"),
                    TryGet<StatusEffectData>("Jolted"),
                    TryGet<StatusEffectData>("Demonize"),
                    TryGet<StatusEffectData>("Overload"),
                    TryGet<StatusEffectData>("Frost")
                };
                ((StatusEffectApplyRandomOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
                TargetConstraintIsCardType enemyconstraint = new TargetConstraintIsCardType();
                enemyconstraint.allowedTypes = new CardType[]
                {
                    TryGet<CardType>("Enemy"),
                    TryGet<CardType>("Boss"),
                    TryGet<CardType>("BossSmall"),
                    TryGet<CardType>("Clunker"),
                    TryGet<CardType>("Miniboss")
                };
                ((StatusEffectApplyRandomOnCardPlayed)data).applyConstraints = new TargetConstraint[] { enemyconstraint };
            })
            );

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectInstantFillXBoardSlots>("Instant Fill Board With RadRadio")
            .WithText("Deploy <card=megamarine.wildfrost.sleetstorm.radtheradio>")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var se = data as StatusEffectInstantFillXBoardSlots;
                se.withCards = new CardData[1] { TryGet<CardData>("megamarine.wildfrost.sleetstorm.radtheradio") };
                se.random = true;
                se.targetConstraints = new TargetConstraint[0];
                se.type = "";
            })
            );

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectInstantFillXBoardSlots>("Instant Fill Board With EvilRadRadio")
            .WithText("Deploy <card=megamarine.wildfrost.sleetstorm.evilradtheradio>")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var se = data as StatusEffectInstantFillXBoardSlots;
                se.withCards = new CardData[1] { TryGet<CardData>("megamarine.wildfrost.sleetstorm.evilradtheradio") };
                se.random = true;
                se.targetConstraints = new TargetConstraint[0];
                se.type = "";
            })
            );

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenDeployed>("On Card Deployed RadRadio To Board")
            .WithText("When deployed, deploy <card=megamarine.wildfrost.sleetstorm.radtheradio>")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var se = data as StatusEffectApplyXWhenDeployed;
                se.effectToApply = TryGet<StatusEffectData>("Instant Fill Board With RadRadio");
                se.stackable = true;
                se.canBeBoosted = true;
                se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                se.type = "";
            })
            );

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenDeployed>("On Card Deployed EvilRadRadio To Board")
            .WithText("When deployed, deploy <card=megamarine.wildfrost.sleetstorm.evilradtheradio>")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var se = data as StatusEffectApplyXWhenDeployed;
                se.effectToApply = TryGet<StatusEffectData>("Instant Fill Board With EvilRadRadio");
                se.stackable = true;
                se.canBeBoosted = true;
                se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                se.type = "";
            })
            );

        assets.Add( //heals and cleanses allies in the row
            StatusCopy("On Turn Heal Allies", "On Turn Heal Allies In Row")
            .WithCanBeBoosted(true)
            .WithText("Restore <{a}><keyword=health> to allies in the row")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnTurn)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;
            })
            );

        assets.Add( //Deal damage to self after turn
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXAfterTurn>("On Card Played Damage To Self After Turn")
            .WithText("Half <keyword=health> after triggering")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Lose Half Health");
                ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;

            })
            );

        assets.Add(  //Trigger Rad The Radio
            StatusCopy("On Card Played Trigger RandomAlly", "On Card Played Trigger Rad Radio")
            .WithText("Trigger <Rad The Radio>")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).eventPriority = 999;
                ((StatusEffectApplyXOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                var radradio = ScriptableObject.CreateInstance<TargetConstraintIsSpecificCard>();
                radradio.allowedCards = new CardData[] { TryGet<CardData>("radtheradio") };
                ((StatusEffectApplyXOnCardPlayed)data).applyConstraints = new TargetConstraint[] { radradio };

            })
            );

        assets.Add(  //Trigger Rad The Radio
            StatusCopy("On Card Played Trigger RandomAlly", "On Card Played Trigger Evil Rad Radio")
            .WithText("Trigger <Possesed Rad The Radio>")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).eventPriority = 999;
                var radradio = ScriptableObject.CreateInstance<TargetConstraintIsSpecificCard>();
                radradio.allowedCards = new CardData[] { TryGet<CardData>("evilradtheradio") };
                ((StatusEffectApplyXOnCardPlayed)data).applyConstraints = new TargetConstraint[] { radradio };

            })
            );

        assets.Add(
            StatusCopy("Summon Junk", "Summon Snow Pie")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectSummon)data).summonCard = TryGet<CardData>("megamarine.wildfrost.sleetstorm.snowpie");
            })
            );

        assets.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Snow Pie In Hand")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon Snow Pie");
            })
            );

        assets.Add(
            StatusCopy("On Card Played Add Junk To Hand", "On Card Played Add Snow Pie To Hand")
            .WithText("Add <{a}> <card=megamarine.wildfrost.sleetstorm.snowpie> to your hand")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Snow Pie In Hand");
            })
            );

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectMania>("Mania")
            .WithVisible(true)
            .WithIsStatus(true)
            .WithStackable(true)
            .WithOffensive(false)
            .WithTextInsert("{a}")
            .WithIcon_VFX("mania", "mania", "maniakeyword", VFXMod_StatusEffectHelpers.LayoutGroup.counter)
               .FreeModify<StatusEffectWhileActiveX>(
                    delegate (StatusEffectWhileActiveX data)
                    {
                        data.applyEqualAmount = true;
                        data.effectToApply = TryGet<StatusEffectData>("MultiHit");
                        data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                        data.targetMustBeAlive = false;
                        var script = ScriptableObject.CreateInstance<ScriptableCurrentStatus>();
                        script.statusType = "mania";
                        ((StatusEffectWhileActiveX)data).scriptableAmount = script;

                    }

           ));

        assets.Add(
            StatusCopy("When Shell Applied To Self Gain Spice Instead", "Omni Immunity")
            .WithVisible(true)
            .WithIsStatus(true)
            .WithStackable(true)
            .WithOffensive(false)
            .WithIcon_VFX("omniimmune", "omniimmune", "omniimmunekeyword", VFXMod_StatusEffectHelpers.LayoutGroup.counter)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenYAppliedTo)data).whenAppliedTypes = new string[] { "ink", "shroom", "vim", "demonize", "frost", "haze", "overload", "snow", "jolted" };
                ((StatusEffectApplyXWhenYAppliedTo)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.None;
                ((StatusEffectApplyXWhenYAppliedTo)data).effectToApply = TryGet<StatusEffectData>("Double Ink");
            })
            );

        assets.Add(
            StatusCopy("When Shell Applied To Self Gain Spice Instead", "Ink Immunity")
            .WithVisible(true)
            .WithIsStatus(true)
            .WithStackable(true)
            .WithOffensive(false)
            .WithIcon_VFX("inkimmune", "inkimmune", "inkimmunekeyword", VFXMod_StatusEffectHelpers.LayoutGroup.counter)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenYAppliedTo)data).whenAppliedTypes = new string[] { "ink" };
                ((StatusEffectApplyXWhenYAppliedTo)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.None;
                ((StatusEffectApplyXWhenYAppliedTo)data).effectToApply = TryGet<StatusEffectData>("Double Ink");
            })
            );

        assets.Add(
            StatusCopy("ImmuneToSnow", "Ink Resistance")
            .WithVisible(true)
            .WithIsStatus(true)
            .WithStackable(true)
            .WithOffensive(false)
            .WithIcon_VFX("inkresist", "inkresist", "inkresistkeyword", VFXMod_StatusEffectHelpers.LayoutGroup.counter)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectImmuneToX)data).immunityType = "ink";
            })
            );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXOnCardPlayed>("Apply Ink Equal To Spice")
            .WithText("Apply <keyword=null> equal to <keyword=spice>")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                var spiceamount = ScriptableAmount.CreateInstance<ScriptableCurrentStatus>();
                spiceamount.statusType = TryGet<StatusEffectData>("Spice").type;
                ((StatusEffectApplyXOnCardPlayed)data).scriptableAmount = spiceamount;
                ((StatusEffectApplyXOnCardPlayed)data).effectToApply = TryGet<StatusEffectData>("Null");
                ((StatusEffectApplyXOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;

            })
            );

        assets.Add(
            StatusCopy("When Enemies Attack Apply Demonize To Attacker", "When Enemies Attack Apply Jolted To Attacker")
            .WithCanBeBoosted(true)
            .WithText("Before an enemy attacks, apply <{a}><keyword=joltedkeyword>to them")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenEnemiesAttack)data).effectToApply = TryGet<StatusEffectData>("Jolted");
                ((StatusEffectApplyXWhenEnemiesAttack)data).eventPriority = 999999;
            })
            );

        assets.Add(
            StatusCopy("On Card Played Apply Block To RandomEnemy", "On Card Played Apply Jolted To RandomEnemy")
            .WithCanBeBoosted(true)
            .WithText("Apply <{a}><keyword=joltedkeyword> to a random enemy")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).effectToApply = TryGet<StatusEffectData>("Jolted");
            })
            );

        assets.Add(
            StatusCopy("While Active Unmovable To Enemies", "While Active Indoctrinated to Allies")
            .WithCanBeBoosted(false)
            .WithText("While active, when an enemy kills an ally, it counts as <Sacrifice>")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("On Kill Count As Sacrifice For Your Team");
                ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
            })
            );

        assets.Add(
            StatusCopy("Activate Sacrifice Effects For Other Team", "Enemy Sacrifice Effects For Your Team")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectInstantActivateSacrificeEffects)data).forOtherTeam = false;
            })
            );

        assets.Add(
            StatusCopy("On Kill Count As Sacrifice For Other Team", "On Kill Count As Sacrifice For Your Team")
            .WithCanBeBoosted(false)
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnKill)data).effectToApply = TryGet<StatusEffectData>("Enemy Sacrifice Effects For Your Team");
            })
            );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenAllyIsKilled>("When Ally Is Killed Activate Sacrifice Effects")
            .WithText("When an ally is killed, activate <Sacrifice> effects")
            .SubscribeToAfterAllBuildEvent(d =>
            {
                var data = (StatusEffectApplyXWhenAllyIsKilled)d;
                data.effectToApply = TryGet<StatusEffectData>("Enemy Sacrifice Effects For Your Team");
                data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
                data.targetMustBeAlive = false;
            })
            );

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectCalm>("Calm")
            .WithVisible(true)
            .WithIsStatus(true)
            .WithStackable(true)
            .WithOffensive(false)
            .WithTextInsert("{a}")
            .WithIcon_VFX("calm", "calm", "calmkeyword", VFXMod_StatusEffectHelpers.LayoutGroup.counter)
            .SubscribeToAfterAllBuildEvent(d =>
            {
                var data = (StatusEffectCalm)d;
                data.effectToApply = TryGet<StatusEffectData>("Reduce Max Counter");
                data.applyEqualAmount = true;
                data.eventPriority = -10;
                data.countDownEffect = TryGet<StatusEffectData>("Reduce Counter");
                data.counterIncreaseEffect = TryGet<StatusEffectData>("Increase Max Counter");
                data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            })
            );

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenDeployedOnce>("FakeCalm")
            .WithVisible(true)
            .WithIsStatus(true)
            .WithStackable(true)
            .WithOffensive(false)
            .WithTextInsert("{a}")
            .WithIcon_VFX("fakecalm", "calm", "calmkeyword", VFXMod_StatusEffectHelpers.LayoutGroup.counter)
            .SubscribeToAfterAllBuildEvent(d =>
            {
                var data = (StatusEffectApplyXWhenDeployedOnce)d;
                data.effectToApply = TryGet<StatusEffectData>("Calm");
                data.eventPriority = -10;
                data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            })
            );

        assets.Add
            (new StatusEffectDataBuilder(this)
            .Create<StatusInstantIncreaseCounter>("Increase Counter")
            .WithCanBeBoosted(true)
            .WithText("Count up <keyword=counter> by <{a}>")
            .WithType("")
            );

        assets.Add
            (new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenDrawn>("When Drawn Count Up Random Enemy")
            .WithCanBeBoosted(true)
            .WithText("When drawn, count up <keyword=counter> by <{a}> of a random enemy")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenDrawn)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
                ((StatusEffectApplyXWhenDrawn)data).effectToApply = TryGet<StatusEffectData>("Increase Counter");
                var atleastxcounter = ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>();
                atleastxcounter.moreThan = 0;
                ((StatusEffectApplyXWhenDrawn)data).applyConstraints = new TargetConstraint[] { atleastxcounter };
            })
            );

        assets.Add(
            StatusCopy("When Hit Apply Demonize To Attacker", "When Hit Apply Overburn To Enemy Row")
            .WithCanBeBoosted(true)
            .WithText("When hit, apply <{a}><keyword=overload> to the enemy row")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
                ((StatusEffectApplyXWhenHit)data).effectToApply = TryGet<StatusEffectData>("Overload");

            })
            );

        assets.Add( //On kill calm to ally behind
            StatusCopy("On Kill Heal To Self & AlliesInRow", "On Kill Calm Ally Behind")
            .WithText("On kill, apply <{a}><keyword=calmkeyword> to ally behind")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnKill)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyBehind;
                ((StatusEffectApplyXOnKill)data).effectToApply = TryGet<StatusEffectData>("Calm");
            })
        );

        assets.Add(
            StatusCopy("On Card Played Apply Snow To EnemiesInRow", "On Card Played Apply Vim To EnemiesInRow")
            .WithCanBeBoosted(true)
            .WithText("Apply <{a}><keyword=weakness> to enemies in the row")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).effectToApply = TryGet<StatusEffectData>("Weakness");

            })
            );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectBonusDamageEqualToXBoostable>("Bonus Damage Equal To Allies In Row")
            .WithCanBeBoosted(true)
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                var scriptableamount = ScriptableAmount.CreateInstance<ScriptableTargetsOnBoard>();
                scriptableamount.allies = true;
                scriptableamount.inRow = true;
                ((StatusEffectBonusDamageEqualToXBoostable)data).on = StatusEffectBonusDamageEqualToXBoostable.On.ScriptableAmount;
                ((StatusEffectBonusDamageEqualToXBoostable)data).scriptableAmount = scriptableamount;
            })
            );

        assets.Add( //Summon Scarechick
            StatusCopy("Summon Beepop", "Summon Scarechick")
            .WithText("Summon <card=megamarine.wildfrost.sleetstorm.scarechick>")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectSummon)data).summonCard = TryGet<CardData>("megamarine.wildfrost.sleetstorm.scarechick");
            })
        );

        assets.Add
            (new StatusEffectDataBuilder(this)
            .Create<StatusEffectAlternateTrait>("Tempo Targeting Mode")
            .WithCanBeBoosted(true)
            .WithText("Switches between <Tempo 1, 2 and 3>")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectAlternateTrait)data).trait = TryGet<TraitData>("Tempo1");
                ((StatusEffectAlternateTrait)data).traits = new TraitData[]
                {
                    TryGet<TraitData>("Tempo1"),
                    TryGet<TraitData>("Tempo2"),
                    TryGet<TraitData>("Tempo3")
                };
            })
            );

        assets.Add(  //count down counter of ally ahead
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectReduceCounter>("On Card Played Reduce Counter RandomAlly")
            .WithCanBeBoosted(true)
            .WithText("Count down <keyword=counter> of a random ally by <{a}>")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomAlly;
                ((StatusEffectApplyXOnCardPlayed)data).effectToApply = TryGet<StatusEffectData>("Reduce Counter");
            })

            );

        assets.Add(
            StatusCopy("When Card Destroyed, Gain Frenzy", "On Enemy Killed Mania To Self")
            .WithText("When an <Item> is destroyed, gain <x{a}><keyword=maniakeyword>")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenCardDestroyed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                ((StatusEffectApplyXWhenCardDestroyed)data).effectToApply = TryGet<StatusEffectData>("Mania");
                var isitem = ScriptableObject.CreateInstance<TargetConstraintIsItem>();
                ((StatusEffectApplyXWhenCardDestroyed)data).constraints = new TargetConstraint[] { isitem }; ;
            })
        );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenYAppliedToButInHand>("When Ink Applied To Self Gain Equal Spice To Self")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenYAppliedToButInHand)data).whenAppliedToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                ((StatusEffectApplyXWhenYAppliedToButInHand)data).whenAppliedTypes = new string[] { "ink" };
                ((StatusEffectApplyXWhenYAppliedToButInHand)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                ((StatusEffectApplyXWhenYAppliedToButInHand)data).effectToApply = TryGet<StatusEffectData>("Spice");
                ((StatusEffectApplyXWhenYAppliedToButInHand)data).instead = true;
            })
        );

        assets.Add(
            StatusCopy("On Turn Apply Ink To Enemies", "On Turn Apply Ink To Self")
            .WithText("Gain <{a}><keyword=null>")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnTurn)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            })
        );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectTriggerWhenAllyBehindAttacks>("Trigger When Ally Behind Attacks")
            .WithIsReaction(true)
            .WithCanBeBoosted(false)
            .WithText("Trigger when ally behind attacks")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData d)
            {
                StatusEffectTriggerWhenAllyBehindAttacks data = (StatusEffectTriggerWhenAllyBehindAttacks)d;
                data.descColorHex = "F99C61";
            })
        );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectTriggerWhenDamageType>("Charged Trigger")
            .WithIsReaction(true)
            .WithCanBeBoosted(false)
            .WithText("<keyword=megamarine.wildfrost.sleetstorm.charged> Trigger")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectTriggerWhenDamageType)data).triggerdamagetype = ("jolt");
                ((StatusEffectTriggerWhenDamageType)data).descColorHex = "F99C61";
                ((StatusEffectTriggerWhenDamageType)data).affectedBySnow = true;
            })
        );

        assets.Add(
            StatusCopy("When Destroyed Apply Spice To Allies", "When Destroyed Apply Demonize To Enemies")
            .WithText("When consumed, apply <{a}><keyword=demonize> to all enemies")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenDestroyed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
                ((StatusEffectApplyXWhenDestroyed)data).effectToApply = TryGet<StatusEffectData>("Demonize");
            })
        );

        assets.Add(
            StatusCopy("When Destroyed Add Health To Allies", "When Destroyed Restore Health To Allies")
            .WithText("When consumed, restore <{a}><keyword=health> to all allies")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenDestroyed)data).effectToApply = TryGet<StatusEffectData>("Heal (No Ping)");
            })
        );

        assets.Add(
            StatusCopy("On Card Played Apply Teeth To AlliesInRow", "On Card Played Apply Teeth To AlliesInRow Better")
            .WithText("Apply <{a}><keyword=teeth> to allies in the row")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).targetMustBeAlive = false;
            })
        );

        assets.Add( //temporary keyword faith
            StatusCopy("Temporary Unmovable", "Temporary Effigy")
            .WithCanBeBoosted(false)
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectTemporaryTrait)data).trait = TryGet<TraitData>("Effigy");
            })
            );

        assets.Add( //temporary keyword sin
            StatusCopy("Temporary Unmovable", "Temporary Sin")
            .WithCanBeBoosted(false)
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectTemporaryTrait)data).trait = TryGet<TraitData>("Sin");
            })
            );

        assets.Add( //While active, adds keyword sin to cards in hand that can summon
            StatusCopy("While Active Unmovable To Enemies", "While Active Sin to All Summon Cards In Hand")
            .WithCanBeBoosted(true)
            .WithText($"While active, add <keyword={GUID}.sin> and <keyword=effigy> <{{a}}> to all cards that can <Summon>")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Hand;
                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Sin");
                var doessummon = ScriptableObject.CreateInstance<TargetConstraintDoesSummon>();
                ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[] { doessummon };
            })
            );

        assets.Add( //While active, adds keyword faith to cards in hand that can summon
            StatusCopy("While Active Unmovable To Enemies", "While Active Faith to All Summon Cards In Hand")
            .WithCanBeBoosted(true)
            .WithText("")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Hand;
                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Effigy");
                var doessummon = ScriptableObject.CreateInstance<TargetConstraintDoesSummon>();
                ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[] { doessummon };
            })
            );

        assets.Add( //While active, adds keyword sin to allies that can summon
            StatusCopy("While Active Unmovable To Enemies", "While Active Sin to All Summon Card Allies")
            .WithCanBeBoosted(true)
            .WithText("")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Sin");
                var doessummon = ScriptableObject.CreateInstance<TargetConstraintDoesSummon>();
                ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[] { doessummon };
            })
            );

        assets.Add( //While active, adds keyword faith to allies in hand that can summon
            StatusCopy("While Active Unmovable To Enemies", "While Active Faith to All Summon Card Allies")
            .WithCanBeBoosted(true)
            .WithText("")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Effigy");
                var doessummon = ScriptableObject.CreateInstance<TargetConstraintDoesSummon>();
                ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[] { doessummon };
            })
            );

        assets.Add( //While active, adds keyword zoomlin to summon items in hand
            StatusCopy("While Active Zoomlin When Drawn To Allies In Hand", "While Active Zoomlin When Drawn To Summon Items In Hand")
            .WithCanBeBoosted(true)
            .WithText("While active, <Summon Items> gain <keyword=zoomlin> when drawn")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Hand;
                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Zoomlin");
                var doessummon = ScriptableObject.CreateInstance<TargetConstraintDoesSummon>();
                var isitem = ScriptableObject.CreateInstance<TargetConstraintIsItem>();
                ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[] { doessummon, isitem };
            })
            );

        assets.Add( //While active, adds keyword faith to allies in hand that can summon
            StatusCopy("On Card Played Trigger RandomAlly", "On Card Played Count Down Random Summoned Unit")
            .WithCanBeBoosted(true)
            .WithText("Count down a random <Summon's> <keyword=counter> by <{a}>")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomUnit;
                ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Counter");
                TargetConstraintIsCardType summonconstraint = new TargetConstraintIsCardType();
                summonconstraint.allowedTypes = new CardType[] { TryGet<CardType>("Summoned") };
                ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[] { summonconstraint };
            })
            );

        assets.Add( //While active, adds keyword faith to allies in hand that can summon
            StatusCopy("On Card Played Trigger Against AllyBehind", "On Card Played Trigger Against AlliesInRow")
            .WithCanBeBoosted(false)
            .WithText("Also hits allies in row")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;

            })
            );

        assets.Add( //Increase health of ally behind
            StatusCopy("On Turn Apply Spice To AllyBehind", "On Turn Increase Max Health To AllyBehind")
            .WithCanBeBoosted(true)
            .WithText("Increase <keyword=health> of ally behind by <{a}>")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnTurn)data).effectToApply = TryGet<StatusEffectData>("Increase Max Health");

            })
            );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenDiscarded>("Set Health When Discarded")
            .WithCanBeBoosted(true)
            .WithText("Set <keyword=health> to <{a}> when discarded")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenDiscarded)data).effectToApply = TryGet<StatusEffectData>("Set Health");
                ((StatusEffectApplyXWhenDiscarded)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
            })
        );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectInstantTutor>("Investigate Draw Pile")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                var realData = data as StatusEffectInstantTutor;
                realData.eventPriority = 5;
                realData.source = StatusEffectInstantTutor.CardSource.Draw;
                realData.title = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English).GetString("sleetstorm.tutorACard");
            })
        );

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenDeployed>("Investigate The Draw Pile When Deployed")
            .WithText("When deployed, <keyword=megamarine.wildfrost.sleetstorm.investigate> the <Draw> pile")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var se = data as StatusEffectApplyXWhenDeployed;
                se.effectToApply = TryGet<StatusEffectData>("Investigate Draw Pile");
                se.stackable = true;
                se.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                se.type = "";
            })
            );

        assets.Add( 
            StatusCopy("On Turn Apply Spice To AllyBehind", "On Turn Apply Spice To AlliesInRow")
            .WithCanBeBoosted(true)
            .WithText("Apply <{a}><keyword=spice> to allies in the row")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnTurn)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;

            })
            );

        assets.Add( 
            StatusCopy("When Hit Apply Gold To Attacker (No Ping)", "When Hit Apply Gold To Attacker (No Ping) By Gunk")
            .WithCanBeBoosted(true)
            .WithText("Drop <{a}><keyword=blings> when hit by a <Gunk Fruit>")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                var gunkfruit = ScriptableObject.CreateInstance<TargetConstraintIsSpecificCard>();
                gunkfruit.allowedCards = new CardData[] { TryGet<CardData>("Deadweight") };
                ((StatusEffectApplyXWhenHit)data).attackerConstraints = new TargetConstraint[] { gunkfruit };

            })
            );

        assets.Add(
            StatusCopy("Summon Junk", "Summon Gunk Fruit")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectSummon)data).summonCard = TryGet<CardData>("Deadweight");
            })
            );

        assets.Add(
            StatusCopy("Instant Summon Junk In Hand", "Instant Summon Gunk Fruit In Hand")
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectSummon>("Summon Gunk Fruit");
            })
            );

        assets.Add(
            StatusCopy("On Card Played Add Junk To Hand", "On Card Played Add Gunk Fruit To Hand")
            .WithText("Add <{a}> <Gunk Fruit> to your hand")
            .WithCanBeBoosted(true)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXOnCardPlayed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Gunk Fruit In Hand");
            })
            );

        assets.Add(
            StatusCopy("While Active Increase Attack To Junk In Hand", "While Active Set Attack To Gunk Fruit In Hand")
            .WithCanBeBoosted(true)
            .WithText("While active, reduce <keyword=attack> of <Gunk Fruits> in hand by <{a}>")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                var gunkfruit = ScriptableObject.CreateInstance<TargetConstraintIsSpecificCard>();
                gunkfruit.allowedCards = new CardData[] { TryGet<CardData>("Deadweight") };
                ((StatusEffectWhileActiveX)data).applyConstraints = new TargetConstraint[] { gunkfruit };
                ((StatusEffectWhileActiveX)data).effectToApply = TryGet<StatusEffectData>("Ongoing Reduce Attack");
            })
            );

        assets.Add(
            StatusCopy("On Turn Apply Spice To Allies", "On Turn Apply Ink To Toonkind Allies")
            .WithCanBeBoosted(true)
            .WithText($"Apply <{{a}}><keyword=null> to all <keyword={Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID("toonkind", this)}> allies")
            .WithType("")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                var doeshavetoon = ScriptableObject.CreateInstance<TargetConstraintHasTrait>();
                doeshavetoon.trait = TryGet<TraitData>("Toonkind");
                doeshavetoon.name = "Does Have Toonkind";
                ((StatusEffectApplyXOnTurn)data).applyConstraints = new TargetConstraint[] { doeshavetoon };
                ((StatusEffectApplyXOnTurn)data).effectToApply = TryGet<StatusEffectData>("Null");
            })
            );

        assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXOnCardPlayedWithPet>("Free Play With Trash")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Junk In Hand");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    //Sadly we cannot set the petPrefab here as it gets destroyed, thus we will copy/edit the prefab in the Load method
                    ((StatusEffectApplyXOnCardPlayedWithPet)data).petPrefab = ((StatusEffectFreeAction)TryGet<StatusEffectData>("Free Action")).petPrefab.InstantiateKeepName();
                    ((StatusEffectApplyXOnCardPlayedWithPet)data).hasEffect = true;
                })
                );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectInstantCountDownStatus>("Instant Remove Jolted")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectInstantCountDownStatus)data).types = new string[] { "jolted" };
                ((StatusEffectInstantCountDownStatus)data).remove = true;
            })
            );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectApplyXWhenHit>("Remove Jolted Of EnemyRow When Hit")
            .WithText("When hit, remove <keyword=joltedkeyword> of the enemy row")
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                ((StatusEffectApplyXWhenHit)data).effectToApply = TryGet<StatusEffectData>("Instant Remove Jolted");
                ((StatusEffectApplyXWhenHit)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
            })
            );

        assets.Add(
            new StatusEffectDataBuilder(this)
            .Create<StatusEffectNextPhase>("FrankenLemonPhase2")
            .WithStackable(true)
            .WithCanBeBoosted(false)
            .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
            {
                var realData = data as StatusEffectNextPhase;
                realData.goToNextPhase = true;
                realData.nextPhase = TryGet<CardData>("frankenlemonphase2");
                realData.preventDeath = true;
                realData.animation = TryGet<StatusEffectNextPhase>("FinalBossPhase2").animation;

            })
            );


        //


        //Add new Targeting Modes here

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectChangeTargetMode>("Hit All Allies")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var se = data as StatusEffectChangeTargetMode;
                se.targetMode = new TargetModeAllAllies();

            }));

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectChangeTargetMode>("Hit Random Ally")
            .SubscribeToAfterAllBuildEvent(data =>
            {

                var se = data as StatusEffectChangeTargetMode;
                se.targetMode = new TargetModeRandomAlly();

            }));

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectChangeTargetMode>("Hit Middle Enemy")
            .SubscribeToAfterAllBuildEvent(data =>
            {

                var se = data as StatusEffectChangeTargetMode;
                se.targetMode = new TargetModeCenter();

            }));

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectChangeTargetMode>("Longshot Copy")
            .SubscribeToAfterAllBuildEvent(data =>
            {

                var se = data as StatusEffectChangeTargetMode;
                se.targetMode = new TargetModeBack();

            }));

        assets.Add(new StatusEffectDataBuilder(this)
            .Create<StatusEffectChangeTargetMode>("Basic Target Mode Copy")
            .SubscribeToAfterAllBuildEvent(data =>
            {

                var se = data as StatusEffectChangeTargetMode;
                se.targetMode = new TargetModeBasic();

            }));

        //

        //Add new keywords here

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("sin")
            .WithTitle("Sin")
            .WithShowName(true)
            .WithDescription("Decreases summon's <keyword=counter>")
            .WithCanStack(true)
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("ace")
            .WithTitle("Ace")
            .WithShowName(true)
            .WithDescription("Always shows up in your hand whenever cards are drawn|Does not pull cards directly from discard until deck reshuffle!")
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("push")
            .WithTitle("Push")
            .WithShowName(true)
            .WithDescription("Pushes target back")
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("sick")
            .WithTitle("Sick")
            .WithShowName(true)
            .WithDescription("Current <keyword=shroom> does not count down")
            );

        assets.Add(new KeywordDataBuilder(this)
            .Create("weak")
            .WithCanStack(false)
            .WithShow(true)
            .WithTitle("Weak")
            .WithDescription("Base <keyword=attack> can not be increased during battles")
            .WithShowName(true)
            .FreeModify<KeywordData>(data =>
            {
                data.name = data.name.ToLower();
            }
            ));

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("debuffed")
            .WithTitle("Negative Effects")
            .WithShowName(true)
            .WithDescription("Afflicted with either <sprite name=ink>, <sprite name=jolted>, <sprite name=overload>, <sprite name=snow>, <sprite name=demonize>, <sprite name=frost>, <sprite name=haze>, <sprite name=shroom> or <sprite name=vim>")
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("tattooed")
            .WithTitle("Tattooed")
            .WithShowName(true)
            .WithDescription("Current <keyword=null> does not count down|Not affected by Ink!")
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("bump")
            .WithTitle("Bump")
            .WithShowName(true)
            .WithDescription("Swap row of target|If row is occupied, swap targets!")
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("electrified")
            .WithTitle("Electrified")
            .WithShowName(true)
            .WithDescription("When the <Redraw Bell is rung, apply <sprite name=jolted> to a random ally and enemy equal to the <Redraw Bell>'s counter")
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("wildmagic")
            .WithTitle("Wild Magic")
            .WithShowName(true)
            .WithDescription("Apply either <sprite name=jolted>, <sprite name=ink>, <sprite name=overload>, <sprite name=snow>, <sprite name=demonize>, <sprite name=frost>, <sprite name=shroom> or <sprite name=vim>")
            .WithCanStack(true)
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("friendlyfire")
            .WithTitle("Friendly Fire")
            .WithShowName(true)
            .WithDescription("Target a random ally instead of enemies")
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("protective")
            .WithTitle("Protective")
            .WithShowName(true)
            .WithDescription("Deal additional damage equal to allies in the row")
            .WithCanStack(true)
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("tempo2")
            .WithTitle("Tempo 2")
            .WithShowName(true)
            .WithDescription("Hit the middle target in the row")
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("tempo3")
            .WithTitle("Tempo 3")
            .WithShowName(true)
            .WithDescription("Hit the furthest target in the row")
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("tempo1")
            .WithTitle("Tempo 1")
            .WithShowName(true)
            .WithDescription("Hit the nearest target in the row")
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("toonkind")
            .WithTitle("Toonkind")
            .WithShowName(true)
            .WithDescription("When <sprite name=ink>'d, gain equal <sprite name=spice> instead")
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("charged")
            .WithTitle("Charged")
            .WithShowName(true)
            .WithDescription("Trigger when an enemy takes <keyword=joltedkeyword> damage")
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("sheher")
            .WithTitle("")
            .WithShowName(true)
            .WithDescription("She/Her")
            .WithBodyColour(Color(188, 188, 224))
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("hehim")
            .WithTitle("")
            .WithShowName(true)
            .WithDescription("He/Him")
            .WithBodyColour(Color(188, 188, 224))
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("hethey")
            .WithTitle("")
            .WithShowName(true)
            .WithDescription("He/They")
            .WithBodyColour(Color(188, 188, 224))
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("theyshe")
            .WithTitle("")
            .WithShowName(true)
            .WithDescription("They/She")
            .WithBodyColour(Color(188, 188, 224))
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("theythem")
            .WithTitle("")
            .WithShowName(true)
            .WithDescription("They/Them")
            .WithBodyColour(Color(188, 188, 224))
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("shethey")
            .WithTitle("")
            .WithShowName(true)
            .WithDescription("She/They")
            .WithBodyColour(Color(188, 188, 224))
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("investigate")
            .WithTitle("Investigate")
            .WithShowName(true)
            .WithDescription("Search for a card")
            .WithCanStack(true)
            );

        assets.Add(
            new KeywordDataBuilder(this)
            .Create("goomlin")
            .WithTitle("Goomlin")
            .WithShowName(true)
            .WithDescription("Does not end your turn when played and <keyword=trash> <1>")
            );

        //assets.Add(
        //    new KeywordDataBuilder(this)
        //    .Create("underhanded")
        //    .WithTitle("Underhanded")
        //    .WithShowName(true)
        //    .WithDescription("Does not trigger 'on hit' reactions and bypasses <keyword=shell>, <keyword=block> and <keyword=teeth>. Cannot damage clunkers.")
        //    .WithCanStack(true)
        //    );

        //

        //Add out traits here

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Ace")
            .SubscribeToAfterAllBuildEvent(
        (trait) =>
        {
            trait.keyword = TryGet<KeywordData>("ace");
            trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Shuffles To Top") };
        })
        );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Weak")
            .SubscribeToAfterAllBuildEvent(delegate (TraitData data)
            {
                data.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Cannot Gain Damage") };
                data.keyword = TryGet<KeywordData>("weak");
            })
            );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Sick")
            .SubscribeToAfterAllBuildEvent(
                (trait) =>
                {
                    trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Halt Shroom") };
                    trait.keyword = TryGet<KeywordData>("sick");

                })
                );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Tattooed")
            .SubscribeToAfterAllBuildEvent(
                (trait) =>
                {
                    trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Halt Null") };
                    trait.keyword = TryGet<KeywordData>("tattooed");

                })
                );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Bump")
            .SubscribeToAfterAllBuildEvent(
                (trait) =>
                {
                    trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("On Hit Bump Target") };
                    trait.keyword = TryGet<KeywordData>("bump");

                })
                );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Wildmagic")
            .SubscribeToAfterAllBuildEvent(
                (trait) =>
                {
                    trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Apply Random Status Instant Wild Magic") };
                    trait.keyword = TryGet<KeywordData>("wildmagic");

                })
                );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Friendlyfire")
            .SubscribeToAfterAllBuildEvent(
                (trait) =>
                {
                    trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Hit Random Ally") };
                    trait.keyword = TryGet<KeywordData>("friendlyfire");

                })
                );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Protective")
            .SubscribeToAfterAllBuildEvent(
                (trait) =>
                {
                    trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Bonus Damage Equal To Allies In Row") };
                    trait.keyword = TryGet<KeywordData>("protective");
                })
                );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Tempo2")
            .WithOverrides(Get<TraitData>("Aimless"), Get<TraitData>("Longshot"), Get<TraitData>("Barrage"))
            .SubscribeToAfterAllBuildEvent(
                (trait) =>
                {
                    trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Hit Middle Enemy") };
                    trait.keyword = TryGet<KeywordData>("tempo2");

                })
                );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Tempo3")
            .WithOverrides(Get<TraitData>("Aimless"), Get<TraitData>("Longshot"), Get<TraitData>("Barrage"))
            .SubscribeToAfterAllBuildEvent(
                (trait) =>
                {
                    trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Longshot Copy") };
                    trait.keyword = TryGet<KeywordData>("tempo3");

                })
                );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Tempo1")
            .WithOverrides(Get<TraitData>("Aimless"), Get<TraitData>("Longshot"), Get<TraitData>("Barrage"))
            .SubscribeToAfterAllBuildEvent(
                (trait) =>
                {
                    trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Basic Target Mode Copy") };
                    trait.keyword = TryGet<KeywordData>("tempo1");

                })
                );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Toonkind")
            .SubscribeToAfterAllBuildEvent(
                (trait) =>
                {
                    trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("When Ink Applied To Self Gain Equal Spice To Self") };
                    trait.keyword = TryGet<KeywordData>("toonkind");

                })
                );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Sin")
            .SubscribeToAfterAllBuildEvent(
                (trait) =>
                {
                    trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Reduce Max Counter To Summon") };
                    trait.keyword = TryGet<KeywordData>("sin");

                })
                );

        assets.Add(
            new TraitDataBuilder(this)
            .Create("Goomlin")
            .SubscribeToAfterAllBuildEvent(
                (trait) =>
                {
                    trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Free Play With Trash") };
                    trait.keyword = TryGet<KeywordData>("goomlin");

                })
                );


        //assets.Add(
        //    new TraitDataBuilder(this)
        //    .Create("Underhanded")
        //    .SubscribeToAfterAllBuildEvent(
        //        (trait) =>
        //        {
        //            trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Bypass On Hit") };
        //            trait.keyword = TryGet<KeywordData>("underhanded");
        //
        //        })
        //        );

        //Add our cards here

        //Companions

        assets.Add(
        new CardDataBuilder(this)
                .CreateUnit(name: "loosesilver", englishTitle: "Loose Silver", bloodProfile: "Blood Profile Husk")
                .SetStats(7, 0, 3)
                .SetSprites("LooseSilver.png", "LooseSilver BG.png")
                .AddPool("ClunkUnitPool")
                .SetAttackEffect(new CardData.StatusEffectStacks(TryGet<StatusEffectData>("Null"), 3))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Bonus Damage Equal to Target's Null", 1),
                        SStack("On Card Played Add Tattooed To Enemy", 1)
                    };
                })

                );

        assets.Add(
            new CardDataBuilder(this)
                .CreateUnit(name: "cainann", englishTitle: "Cainann", idleAnim: "SquishAnimationProfile")
                .SetStats(10, 1, 4)
                .SetSprites("Cainann.png", "Cainann BG.png")
                .AddPool("MagicUnitPool")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("When Hit Apply Teeth To RandomAlly", 2),
                        SStack("Bonus Damage Equal to Allies in Row Teeth", 1)
                    };

                })

                );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit(name: "sorrelmist", englishTitle: "Sorrel Mist")
            .SetStats(8, 0, 5)
            .SetSprites("SorrelMist.png", "SorrelMist BG.png")
            .AddPool("MagicUnitPool")
            .AddPool("BasicUnitPool")
            .WithFlavour("Character by Redmond Alizarin on Discord!")
            .SetAttackEffect(new CardData.StatusEffectStacks(TryGet<StatusEffectData>("Shroom"), 2))
            .SetStartWithEffect(new CardData.StatusEffectStacks(TryGet<StatusEffectData>("When Hit Apply Overload To Attacker"), 2))
            .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Barrage"), 1))
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit(name: "cassiusblackmont", englishTitle: "Cassius Blackmont")
            .SetStats(1, 1, 4)
            .SetSprites("CassiusBlackmont.png", "CassiusBlackmont BG.png")
            .AddPool("MagicUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[2]
                {
                    SStack("On Card Played Take Health From Allies In Row", 4),
                    SStack("Damage Equal To Health", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit(name: "craigory", englishTitle: "Craigory")
            .SetStats(10, 2, 3)
            .SetSprites("Craigory.png", "Craigory BG.png")
            .AddPool("GeneralUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind", 1)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("When Hit Apply Spice Shell Teeth Or Frost To RandomAlly", 2)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("ames", "Ames", idleAnim: "PulseAnimationProfile", bloodProfile: "Blood Profile Pink Wisp")
            .SetSprites("Ames.png", "Ames BG.png")
            .SetStats(12, 3, 4)
            .AddPool("GeneralUnitPool")
            .WithFlavour("Art, BG, and Idea by Redmond Alizarin on Discord!")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("When Health Lost Apply Equal Frost To Attacker", 1)
                };
            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("theengineer", "The Engineer")
            .SetSprites("TheEngineer.png", "TheEngineer BG.png")
            .SetStats(10, 4, 0)
            .AddPool("ClunkUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind", 1)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[4]
                {
                    SStack("Weakness", 2),
                    SStack("Trigger When Redraw Hit", 1),
                    SStack("On Turn Apply Spice To Allies", 2),
                    SStack("On Card Played Trigger Clunker Ahead", 1)
                };
            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("lemon", "Lemon", idleAnim: "SquishAnimationProfile")
            .SetSprites("Lemon.png", "Lemon BG.png")
            .SetStats(4, 0, 4)
            .WithCardType("Friendly")
            .AddPool("BasicUnitPool")
            .SetAttackEffect(new CardData.StatusEffectStacks(TryGet<StatusEffectData>("Shroom"), 3))
            .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Longshot"), 1))
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("When Destroyed Summon Aberrant Shade", 1)
                };
            })
            );


        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("starlight", "Starlight")
            .SetSprites("Starlight.png", "Starlight BG.png")
            .SetStats(14, null, 4)
            .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Frontline"), 1))
            .AddPool("GeneralUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[2]
                {
                    SStack("On Card Played Reduce Counter Row", 1),
                    SStack("On Turn Apply Demonize To Self", 1)
                };
            })

            );


        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("ailyn", "Ailyn", bloodProfile: "Blood Profile Fungus")
            .SetSprites("Ailyn.png", "Ailyn BG.png")
            .SetStats(1, null, 5)
            .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Heartburn"), 1))
            .AddPool("BasicUnitPool")
            .WithFlavour("Art, BG, and Idea by Redmond Alizarin on Discord!")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[2]
                {
                    SStack("On Card Played Shroom Equal To Health To RandomEnemy", 1),
                    SStack("MultiHit", 1)
                };
            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("skalemoji", "Skalemoji")
            .SetSprites("Skalemoji.png", "Skalemoji BG.png")
            .SetStats(6, null, 0)
            .AddPool("MagicUnitPool")
            .WithFlavour("Art, BG, and Idea by Redmond Alizarin on Discord!")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Ace", 1)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[2]
                {
                    SStack("When Health Lost Apply Equal Teeth To Self", 1),
                    SStack("Teeth", 2),
                };

            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("biff", "Biff", idleAnim: "GiantAnimationProfile")
            .SetSprites("Biff.png", "Biff BG.png")
            .SetStats(15, 4, 6)
            .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Pigheaded"), 1))
            .AddPool("GeneralUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind", 1),
                    TStack("Bump", 1)
                };
            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("yuura", "Yuura", idleAnim: "PulseAnimationProfile")
            .SetSprites("Yuura.png", "Yuura BG.png")
            .SetStats(4, null, 8)
            .AddPool("GeneralUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Block", 2),
                    SStack("When Hit Apply Ink To The Attacker 2", 3),
                    SStack("On Card Played Apply Block To RandomAlly", 1)
                };
            })

            );

        //assets.Add(
        //    new CardDataBuilder(this)
        //    .CreateUnit("jubi", "Jubi")
        //    .SetSprites("ShadeSerpent.png", "ShadeSerpent BG.png")
        //    .SetStats(5, null, 4)
        //    .WithCardType("Friendly")
        //    //.AddPool("BasicUnitPool")
        //    .SubscribeToAfterAllBuildEvent(data =>
        //    {
        //        data.startWithEffects = new CardData.StatusEffectStacks[1]
        //        {
        //            SStack("MultiHit", 1)
        //        };
        //        data.traits = new List<CardData.TraitStacks>()
        //        {
        //            TStack("Underhanded", 2)
        //        };
        //
        //    })
        //    );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("mersel", "Mersel")
            .SetSprites("Mersel.png", "Mersel BG.png")
            .SetStats(6, 2, 4)
            .WithCardType("Friendly")
            .AddPool("GeneralUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind", 1)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("On Hit Damage Debuffed Target", 5)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("pip", "Pipsqueak Dovran")
            .SetSprites("Pip.png", "Pip BG.png")
            .SetStats(4, 2, 3)
            .WithCardType("Friendly")
            .AddPool("BasicUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("MultiHit", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind", 1),
                    TStack("Longshot", 1),
                    TStack("Pull", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("talon", "Talon")
            .SetSprites("Talon.png", "Talon BG.png")
            .SetStats(8, 2, 6)
            .WithCardType("Friendly")
            .AddPool("MagicUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("When Hit Gain Teeth To Self", 1),
                    SStack("Teeth", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Smackback", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("oriane", "Oriane")
            .SetSprites("Oriane.png", "Oriane BG.png")
            .SetStats(8, 0, 0)
            .WithCardType("Friendly")
            .AddPool("MagicUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("Overload", 4)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Smackback", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("liltinkerson", "Lil' Tinkerson")
            .SetSprites("LilTinkerson.png", "LilTinkerson BG.png")
            .SetStats(4, 2, 3)
            .WithCardType("Friendly")
            .AddPool("ClunkUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Bonus Damage Equal To Scrap On Board", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("duster", "Duster")
            .SetSprites("Duster.png", "Duster BG.png")
            .SetStats(5, 1, 5)
            .WithCardType("Friendly")
            .AddPool("BasicUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("While Active Multihit Equal Health", 1),
                    SStack("On Card Played Damage To Self After Turn", 2),
                    SStack("Set Health When Discarded", 5)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("phileap", "Phileap")
            .SetSprites("Phileap.png", "Phileap BG.png")
            .SetStats(5, 3, 3)
            .WithCardType("Friendly")
            .AddPool("GeneralUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Snow", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Fury", 5),
                    TStack("Toonkind", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("hazetrailblazer", "Haze Trailblazer")
            .SetSprites("HazeTrailblazer.png", "HazeTrailblazer BG.png")
            .SetStats(5, null, 5)
            .WithCardType("Friendly")
            .AddPool("BasicUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("On Action Apply Haze To RandomEnemy", 1),
                    SStack("On Card Played Trigger RandomEnemyInRow", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("baph", "Baph")
            .SetSprites("Baph.png", "Baph BG.png")
            .SetStats(8, 2, 4)
            .WithCardType("Friendly")
            .AddPool("BasicUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind", 1)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("On Turn Apply Ink To Toonkind Allies", 4)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("higgs", "Higgs")
            .SetSprites("Higgs.png", "Higgs BG.png")
            .SetStats(6, 2, 4)
            .WithCardType("Friendly")
            .AddPool("BasicUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("On Card Played Apply Vim To EnemiesInRow", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Friendlyfire", 1),
                    TStack("Toonkind", 1)
                };
                data.attackEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Increase Attack", 3)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("blink", "Blink Lightly")
            .SetSprites("BlinkLightly.png", "BlinkLightly BG.png")
            .SetStats(8, 0, 5)
            .WithCardType("Friendly")
            .AddPool("GeneralUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Barrage", 1),
                    TStack("Toonkind", 1)
                };
                data.attackEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Jolted", 2)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Charged Trigger", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("2ufo", "M1-A And C0-D3")
            .SetSprites("2ufo.png", "2ufo BG.png")
            .SetStats(4, 0, 3)
            .WithCardType("Friendly")
            .AddPool("GeneralUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Aimless", 1)
                };
                data.attackEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Jolted", 3)
               };
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Scrap", 2),
                    SStack("MultiHit", 1)
                };
        
            })
            );

        //assets.Add(
        //    new CardDataBuilder(this)
        //    .CreateUnit("pipandstonks", "Pip And Stonks")
        //    .SetSprites("Higgs.png", "Higgs BG.png")
        //    .SetStats(8, 2, 4)
        //    .WithCardType("Friendly")
        //    .AddPool("GeneralUnitPool")
        //    .SubscribeToAfterAllBuildEvent(data =>
        //    {
        //        data.traits = new List<CardData.TraitStacks>()
        //        {
        //            TStack("Greed", 1)
        //        };
        //        data.startWithEffects = new CardData.StatusEffectStacks[]
        //        {
        //            SStack("On Kill Apply Gold To Self", 4)
        //        };
        //
        //    })
        //    );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("grimtome", "Grim Tome")
            .SetSprites("Tome.png", "Tome BG.png")
            .SetStats(8, 1, 3)
            .WithCardType("Friendly")
            .AddPool("GeneralUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Protective", 2),
                    TStack("Toonkind", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("tobias", "Tobias")
            .SetSprites("Tobias.png", "Tobias BG.png")
            .SetStats(8, 5, 0)
            .WithCardType("Friendly")
            .AddPool("GeneralUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Tempo Targeting Mode", 1),
                    SStack("Trigger When Enemy Is Killed", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("rowan", "Rowan")
            .SetSprites("Rowan.png", "Rowan BG.png")
            .SetStats(10, 1, 0)
            .WithCardType("Friendly")
            .AddPool("SnowUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Snow", 1)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("On Enemy Killed Mania To Self", 1),
                    SStack("When Hit Trigger To Self", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind", 1)

                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("lemonsis", "Lemon Sis")
            .SetSprites("LemonSis.png", "LemonSis BG.png")
            .SetStats(12, 1, 5)
            .WithCardType("Friendly")
            .AddPool("ClunkUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Jolted", 2),
                    SStack("Weakness", 1)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Charged Trigger", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("ami", "Ami The Angel")
            .SetSprites("Ami.png", "Ami BG.png")
            .SetStats(8, 2, 4)
            .WithCardType("Friendly")
            .AddPool("GeneralUnitPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Increase Counter", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind", 1)
                };
            })
            );

        //Pets

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("puffle", "Puffle", idleAnim: "SquishAnimationProfile")
            .SetSprites("Puffle.png", "Puffle BG.png")
            .SetStats(2, 0, 0)
            .IsPet((ChallengeData)null, true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("Shroom", 3)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("Trigger When Ally Behind Attacks", 1)
                };

            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("zoomie", "Zoomie", idleAnim: "SquishAnimationProfile")
            .SetSprites("Zoomie.png", "Zoomie BG.png")
            .SetStats(4, 1, 3)
            .IsPet((ChallengeData)null, true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("On Card Played Add Zoomlin To Rightmost Card In Hand", 1)
                };
            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("sizzle", "Sizzle", idleAnim: "FlyAnimationProfile")
            .SetSprites("Sizzle.png", "Sizzle BG.png")
            .SetStats(5, 0, 4)
            .IsPet((ChallengeData)null, true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("Overload", 2)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Longshot", 1)
                };
            })
            );

        if (!moreFrostLoaded)
        {
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("ShellPet", "Weev", bloodProfile: "Blood Profile Fungus", idleAnim: "PingAnimationProfile")
                .SetStats(3, 1, 4)
                .SetStartWithEffect(CreateEffectStack("On Turn Apply Shell To AllyBehind", 2), CreateEffectStack("On Turn Apply Shell To Self", 1), CreateEffectStack("Shell", 2))
                .SetSprites("Weev.png", "Weev BG.png")
                .IsPet((ChallengeData)null, true)
                );
        }

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("dire", "Dire", idleAnim: "FloatAnimationProfile")
            .SetSprites("Dire.png", "Dire BG.png")
            .SetStats(4, 1, 4)
            .IsPet((ChallengeData)null, true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("On Turn Apply Spice To AlliesInRow", 2)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("racca", "Racca", idleAnim: "FloatAnimationProfile")
            .SetSprites("Racca.png", "Racca BG.png")
            .SetStats(4, 5, 6)
            .IsPet((ChallengeData)null, true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("When Hit Apply Gold To Attacker (No Ping) By Gunk", 4),
                    SStack("On Card Played Add Gunk Fruit To Hand", 1),
                    SStack("While Active Set Attack To Gunk Fruit In Hand", 1)
                };
            })
            );


        //Summons

        void AddShades()
        {
            for (int i = 0; i < summoner.Length; i++)
            {
                CreatedByLookup.Add(GUID + "." + summoned[i], GUID + "." + summoner[i]);
            }
        }
        AddShades();

        assets.Add(
        new CardDataBuilder(this)
            .CreateUnit("berryboom", "Berryboom", bloodProfile: "Blood Profile Berry", idleAnim: "Heartbeat2AnimationProfile")
            .SetSprites("BerryBoom.png", "BerryBoom BG.png")
            .SetStats(1, null, 0)
            .WithCardType("Summoned")
            .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Explode"), 8))
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("On Kill Heal Allies", 4)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("perplotoo", "Perplotoo", idleAnim: "FlyAnimationProfile")
            .SetSprites("Perplotoo.png", "Perplotoo BG.png")
            .SetStats(8, null, 4)
            .WithCardType("Summoned")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("On Action Apply Haze To Random Ally", 1)
                };
            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("aberrantshade", "Aberrant Shade", idleAnim: "HeartbeatAnimationProfile")
            .SetSprites("AberrantShade.png", "AberrantShade BG.png")
            .SetStats(14, 6, 8)
            .WithCardType("Summoned")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("When Hit Add Health Lost as Shell To Random Ally", 1)
                };
            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("larrysamuel", "Larry Samuel")
            .SetSprites("LarrySamuel.png", "LarrySamuel BG.png")
            .SetStats(10, 5, 5)
            .WithCardType("Summoned")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("Pre Trigger Copy Effects Of RandomAlly", 1)
                };
            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("hogsteria", "Hogsteria")
            .SetSprites("Hogsteria.png", "Hogsteria BG.png")
            .SetStats(5, 2, 5)
            .WithCardType("Summoned")
            .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Wild"), 1))
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[2]
                {
                    SStack("While Active Wild To Summoned Allies", 1),
                    SStack("MultiHit", 1)
                };
            })
            );

        assets.Add(
        new CardDataBuilder(this)
            .CreateUnit("scarechick", "Scarechick", idleAnim: "Heartbeat2AnimationProfile")
            .SetSprites("Scarechick.png", "Scarechick BG.png")
            .SetStats(4, null, 4)
            .WithCardType("Summoned")
            .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Explode"), 8))
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Backline", 1)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("While Active Indoctrinated to Allies", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("jayson", "Jayson", idleAnim: "Heartbeat2AnimationProfile")
            .SetSprites("Jayson.png", "Jayson BG.png")
            .SetStats(10, 10, 0)
            .WithCardType("Summoned")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Friendlyfire", 1)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("When Hit Trigger To Self", 1)
                };
            })

            );

        //Clunkers

        assets.Add(
        new CardDataBuilder(this)
            .CreateUnit("pinkberrydispenser", "Pinkberry Dispenser", idleAnim: "PulseAnimationProfile", bloodProfile: "Blood Profile Berry")
            .SetSprites("PinkberryDispenser.png", "PinkberryDispenser BG.png")
            .SetStats(null, null, 0)
            .WithValue(50)
            .WithCardType("Clunker")
            .AddPool("ClunkItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[4]
                {
                    SStack("Scrap", 2),
                    SStack("Trigger When Self Or Ally Loses Scrap", 1),
                    SStack("On Turn Heal Allies In Row", 2),
                    SStack("When Destroyed Add Health To Allies", 4)
                };
            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("willowthetrain", "Will O' The Wisp 209 Express", idleAnim: "ShakeAnimationProfile")
            .SetSprites("Willow.png", "Willow BG.png")
            .SetStats(null, 0, 8)
            .WithValue(50)
            .WithCardType("Clunker")
            .AddPool("ClunkItemPool")
            .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Barrage"), 1))
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("Overload", 2),
                };
                data.startWithEffects = new CardData.StatusEffectStacks[3]
                {
                    SStack("Scrap", 3),
                    SStack("Pre Trigger Gain Frenzy Equal To Scrap", 1),
                    SStack("Destroy Self After Turn", 1)
                };
            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("truffletotem", "Truffle Totem", idleAnim: "FloatAnimationProfile", bloodProfile: "Blood Profile Fungus")
            .SetSprites("TruffleTotem.png", "TruffleTotem BG.png")
            .SetStats(null, null, 4)
            .WithValue(50)
            .WithCardType("Clunker")
            .AddPool("BasicItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[3]
                {
                SStack("Scrap", 2),
                SStack("On Card Played Lose Scrap To Self", 1),
                SStack("While Active Sick to Enemies", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("shadebuster", "Shade Buster Cannon", idleAnim: "FloatAnimationProfile", bloodProfile: "Blood Profile Black")
            .SetSprites("ShadeBusterCannon.png", "ShadeBusterCannon BG.png")
            .SetStats(null, 0, 4)
            .WithValue(50)
            .WithCardType("Clunker")
            .AddPool("MagicItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[4]
                {
                SStack("Scrap", 2),
                SStack("Pre Turn Kill Summoned Allies & Gain Attack For Each", 3),
                SStack("On Card Played Lose Scrap To Self After Turn", 1),
                SStack("MultiHit", 2)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("willowtotem", "Willow Totem", idleAnim: "WaveAnimationProfile")
            .SetSprites("WillowTotem.png", "WillowTotem BG.png")
            .SetStats(null, null, 0)
            .WithValue(50)
            .WithCardType("Clunker")
            .AddPool("MagicItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                SStack("Scrap", 1),
                SStack("When Enemies Attack Apply Overburn To Attacker", 2)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("wellofinspiration", "Well Of Inspiration", idleAnim: "Heartbeat2AnimationProfile")
            .SetSprites("WellOfInspiration.png", "WellOfInspiration BG.png")
            .SetStats(null, null, 0)
            .WithValue(50)
            .WithCardType("Clunker")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[2]
                {
                SStack("Scrap", 2),
                SStack("While Active Increase Attack To fountainfalchion In Hand", 2)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("radtheradio", "Rad The Radio", idleAnim: "SwayAnimationProfile")
            .SetSprites("RadTheRadio.png", "RadTheRadio BG.png")
            .SetStats(null, 3, 5)
            .WithValue(50)
            .WithCardType("Clunker")
            .AddPool("ClunkItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                SStack("Scrap", 2),
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind",1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("lightningrod", "Lighting Rod", idleAnim: "PulseAnimationProfile")
            .SetSprites("LightingRod.png", "LightingRod BG.png")
            .SetStats(null, null, 0)
            .WithValue(50)
            .WithCardType("Clunker")
            .AddPool("GeneralItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                SStack("Scrap", 1),
                SStack("When Enemies Attack Apply Jolted To Attacker", 2)
                };
            })
            );

        //assets.Add(
        //    new CardDataBuilder(this)
        //    .CreateUnit("electrifiedredrawbell", "Electrified Redraw Bell", idleAnim: "HangAnimationProfile")
        //    .SetSprites("WillowTotem.png", "WillowTotem BG.png")
        //    .SetStats(null, null, 0)
        //    .WithCardType("Clunker")
        //    .AddPool("GeneralItemPool")
        //    .SubscribeToAfterAllBuildEvent(data =>
        //    {
        //        data.startWithEffects = new CardData.StatusEffectStacks[]
        //        {
        //        SStack("Scrap", 2),
        //        SStack("Trigger When Redraw Hit", 1),
        //        SStack("On Card Played Apply Jolted To RandomEnemy", 3)
        //        };
        //    })
        //    );

        assets.Add(
        new CardDataBuilder(this)
            .CreateUnit("bonesculpture", "Bone Sculpture", idleAnim: "Heartbeat2AnimationProfile")
            .SetSprites("BoneSculpture.png", "BoneSculpture BG.png")
            .SetStats(null, null, 0)
            .WithValue(50)
            .WithCardType("Clunker")
            .AddPool("MagicItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Scrap", 2),
                    SStack("Teeth", 2),
                    SStack("Trigger When Self Or Ally Loses Scrap", 1),
                    SStack("On Card Played Apply Teeth To AlliesInRow Better", 2)
                };
            })

            );

        assets.Add(
        new CardDataBuilder(this)
            .CreateUnit("shadefountain", "Shade Fountain", idleAnim: "PingAnimationProfile")
            .SetSprites("ShadeFountain.png", "ShadeFountain BG.png")
            .SetStats(null, null, 0)
            .WithValue(50)
            .WithCardType("Clunker")
            .AddPool("MagicItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Scrap", 1),
                    SStack("While Active Sin to All Summon Cards In Hand", 1),
                    SStack("While Active Faith to All Summon Cards In Hand", 1),
                    SStack("While Active Sin to All Summon Card Allies", 1),
                    SStack("While Active Faith to All Summon Card Allies", 1),
                };
            })

            );

        assets.Add(
        new CardDataBuilder(this)
            .CreateUnit("maskmakerstable", "Mask Makers Table")
            .SetSprites("MaskMakersTable.png", "MaskMakersTable BG.png")
            .SetStats(null, null, 0)
            .WithValue(50)
            .WithCardType("Clunker")
            .AddPool("MagicItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Scrap", 1),
                    SStack("While Active Zoomlin When Drawn To Summon Items In Hand", 1)
                };
            })

            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("Sinsongbox", "Sin Song Box", idleAnim: "FloatAnimationProfile")
            .SetSprites("SinSongBox.png", "SinSongBox BG.png")
            .SetStats(null, null, 4)
            .WithValue(50)
            .WithCardType("Clunker")
            .AddPool("MagicItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[4]
                {
                    SStack("Scrap", 4),
                    SStack("On Card Played Count Down Random Summoned Unit", 2),
                    SStack("On Card Played Lose Scrap To Self After Turn", 1),
                    SStack("MultiHit", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("bluebertha", "Blue Bertha")
            .SetSprites("BlueBertha.png", "BlueBertha BG.png")
            .SetStats(null, 0, 5)
            .WithValue(50)
            .WithCardType("Clunker")
            .AddPool("ClunkItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Null", 3)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Scrap", 1),
                    SStack("On Turn Heal Allies In Row", 3),
                    SStack("On Card Played Trigger Against AlliesInRow", 1),
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Barrage",1)
                };
            })
            );

        //Items

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "goatskull", englishTitle: "Goat Skull", idleAnim: "FloatAnimationProfile")
            .SetStats(null, 0, 0)
            .SetSprites("GoatSkull.png", "GoatSkull BG.png")
            .WithValue(50)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(false)
            .CanPlayOnFriendly(true)
            .AddPool("MagicItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("Sacrifice Ally", 1),
                };
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("On Card Played Apply Demonize To EnemiesInRow", 2)
                };
            })
            );

        assets.Add(
           new CardDataBuilder(this)
           .CreateItem(name: "berryboommask", englishTitle: "Berryboom Mask", idleAnim: "FloatAnimationProfile")
           .SetSprites("BerryBoomMask.png", "BerryBoomMask BG.png")
           .WithValue(50)
           .CanPlayOnBoard(true)
           .CanPlayOnEnemy(false)
           .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Consume"), 1))
           .AddPool("MagicItemPool")
           .FreeModify(delegate (CardData c)
           {
               c.playOnSlot = true;
           })
           .SubscribeToAfterAllBuildEvent(data =>
           {
               data.startWithEffects = new CardData.StatusEffectStacks[1]
               {
                   SStack("Summon Berryboom", 1)
               };
           })
           );

        assets.Add(
           new CardDataBuilder(this)
           .CreateItem(name: "perplotoomask", englishTitle: "Perplotoo Mask", idleAnim: "FloatAnimationProfile")
           .SetSprites("PerplotooMask.png", "PerplotooMask BG.png")
           .WithValue(50)
           .CanPlayOnBoard(true)
           .CanPlayOnEnemy(true)
           .CanPlayOnFriendly(false)
           .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Consume"), 1))
           .AddPool("MagicItemPool")
           .FreeModify(delegate (CardData c)
           {
               c.playOnSlot = true;
           })
           .SubscribeToAfterAllBuildEvent(data =>
           {
               data.startWithEffects = new CardData.StatusEffectStacks[1]
               {
                   SStack("Summon Enemy Perplotoo", 1)
               };
           })
           );

        assets.Add(
           new CardDataBuilder(this)
           .CreateItem(name: "jaysonmask", englishTitle: "Jayson's Mask", idleAnim: "FloatAnimationProfile")
           .SetSprites("JaysonMask.png", "JaysonMask BG.png")
           .WithValue(50)
           .CanPlayOnBoard(true)
           .CanPlayOnEnemy(true)
           .CanPlayOnFriendly(false)
           .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Consume"), 1))
           .AddPool("MagicItemPool")
           .FreeModify(delegate (CardData c)
           {
               c.playOnSlot = true;
           })
           .SubscribeToAfterAllBuildEvent(data =>
           {
               data.startWithEffects = new CardData.StatusEffectStacks[1]
               {
                   SStack("Summon Enemy Jayson", 1)
               };
           })
           );

        //assets.Add(
        //    new CardDataBuilder(this)
        //    .CreateItem(name: "durianbom", englishTitle: "Durian Bom", idleAnim: "ShakeAnimationProfile")
        //    .SetSprites("DurianBom.png", "DurianBom BG.png")
        //    .SetStats(null, 3, 0)
        //    .WithValue(50)
        //    .CanPlayOnEnemy(true)
        //    .CanPlayOnFriendly(true)
        //    .SetTraits(TStack("Aimless", 1), TStack("Consume", 2))
        //    .AddPool("GeneralItemPool")
        //    .SubscribeToAfterAllBuildEvent(data =>
        //    {
        //        data.attackEffects = new CardData.StatusEffectStacks[1]
        //        {
        //           SStack("Remove Effects Of Target", 1)
        //        };
        //    })
        //    );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "sunderbell", englishTitle: "Sunder Bell", idleAnim: "FloatAnimationProfile")
            .SetSprites("SunderBell.png", "SunderBell BG.png")
            .SetStats(null, 1, 0)
            .WithValue(50)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .AddPool("GeneralItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[2]
                {
                   SStack("Reduce Counter", 3),
                   SStack("Demonize", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "frostberries", englishTitle: "Frost Berries", idleAnim: "FlyAnimationProfile")
            .SetSprites("FrostBerries.png", "FrostBerries BG.png")
            .WithValue(50)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .AddPool("BasicItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[2]
                {
                   SStack("Heal", 6),
                   SStack("Frost", 2)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "cubecake", englishTitle: "Cubecake", idleAnim: "ShakeAnimationProfile")
            .SetSprites("Cubecake.png", "Cubecake BG.png")
            .WithValue(50)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .AddPool("GeneralItemPool")
            .SetTraits(TStack("Consume", 1))
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Block", 1)
                };
            })
            );


        assets.Add(
           new CardDataBuilder(this)
           .CreateItem(name: "larrymask", englishTitle: "Larry Mask", idleAnim: "FloatAnimationProfile")
           .SetSprites("LarryMask.png", "LarryMask BG.png")
           .WithValue(50)
           .CanPlayOnBoard(true)
           .CanPlayOnEnemy(false)
           .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Consume"), 1))
           .AddPool("GeneralItemPool")
           .FreeModify(delegate (CardData c)
           {
               c.playOnSlot = true;
           })
           .SubscribeToAfterAllBuildEvent(data =>
           {
               data.startWithEffects = new CardData.StatusEffectStacks[1]
               {
                   SStack("Summon Larry", 1)
               };
           })
           );


        assets.Add(
           new CardDataBuilder(this)
           .CreateItem(name: "hogsteriamask", englishTitle: "Hogsteria Mask", idleAnim: "FloatAnimationProfile")
           .SetSprites("HogsteriaMask.png", "HogsteriaMask BG.png")
           .WithValue(50)
           .CanPlayOnBoard(true)
           .CanPlayOnEnemy(false)
           .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Consume"), 1))
           .AddPool("MagicItemPool")
           .FreeModify(delegate (CardData c)
           {
               c.playOnSlot = true;
           })
           .SubscribeToAfterAllBuildEvent(data =>
           {
               data.startWithEffects = new CardData.StatusEffectStacks[1]
               {
                   SStack("Summon Hogsteria", 1)
               };
           })
           );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "zoomlinwafflemaker", englishTitle: "Zoomlin Waffle Maker", idleAnim: "FloatAnimationProfile")
            .SetSprites("ZoomlinWaffleMaker.png", "ZoomlinWaffleMaker BG.png")
            .WithValue(50)
            .CanPlayOnFriendly(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnBoard(true)
            .CanPlayOnHand(true)
            .NeedsTarget(false)
            .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Recycle"), 1))
            .AddPool("ClunkItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("On Card Played Add Zoomlin To Random Card In Hand", 1),
                    SStack("MultiHit", 2)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "tattooneedle", englishTitle: "Tattoo Needle", idleAnim: "FloatAnimationProfile")
            .SetSprites("TattooGun.png", "TattooGun BG.png")
            .WithValue(50)
            .SetStats(null, 1, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .CanPlayOnHand(true)
            .AddPool("ClunkItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Null", 1)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("On Card Played Add Tattooed To Enemy", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Consume", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "toxictonic", englishTitle: "Toxic Tonic", idleAnim: "FloatAnimationProfile")
            .SetSprites("ToxicTonic.png", "ToxicTonic BG.png")
            .WithValue(50)
            .SetStats(null, 0, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .CanPlayOnHand(true)
            .AddPool("ClunkItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Shroom", 3)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("On Card Played Add Sick To Enemy", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "sunkazoo", englishTitle: "Sunstruck Kazoo", idleAnim: "FloatAnimationProfile")
            .SetSprites("SunKazoo.png", "SunKazoo BG.png")
            .WithValue(50)
            .SetStats(null, null, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Reduce Counter", 2)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("When Drawn Count Up Random Enemy", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "fountainfalchion", englishTitle: "Fountain Falchion", idleAnim: "WaveAnimationProfile")
            .SetSprites("FountainFalchion.png", "FountainFalchion BG.png")
            .WithValue(50)
            .SetStats(null, 2, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("On Hit Equal Null To Target", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "springpoweredmallet", englishTitle: "Spring Powered Mallet", idleAnim: "FloatAnimationProfile")
            .SetSprites("SpringLoadedHammer.png", "SpringLoadedHammer BG.png")
            .SetStats(null, 3, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Bump", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "vaudevillehook", englishTitle: "Vaudeville Hook", idleAnim: "WaveAnimationProfile")
            .SetSprites("VaudevilleHook.png", "VaudevilleHook BG.png")
            .SetStats(null, 3, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Pull", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "snowpuff", englishTitle: "Snow Puff", idleAnim: "FloatAnimationProfile")
            .SetSprites("SnowPuff.png", "SnowPuff BG.png")
            .WithValue(50)
            .SetStats(null, 0, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .CanPlayOnHand(true)
            .WithPools("SnowItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Snow", 2)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("When Drawn Snow Random Enemy", 2)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "handbuzzer", englishTitle: "Hand Buzzer", idleAnim: "FloatAnimationProfile")
            .SetSprites("HandBuzzer.png", "HandBuzzer BG.png")
            .SetStats(null, 0, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .CanPlayOnHand(true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Jolted", 3)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Combo", 1)
                };

            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "snowballcrossbow", englishTitle: "Snowball Crossbow", idleAnim: "FloatAnimationProfile")
            .SetSprites("SnowballCrossbow.png", "SnowballCrossbow BG.png")
            .WithValue(60)
            .SetStats(null, 0, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .CanPlayOnHand(true)
            .WithPools("SnowItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Snow", 1)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("When Redraw Hit Apply Mania To Self", 1)
                };

            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "snowpie", englishTitle: "Snow Pie", idleAnim: "FloatAnimationProfile")
            .SetSprites("SnowPie.png", "SnowPie BG.png")
            .WithValue(30)
            .SetStats(null, 0, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .CanPlayOnHand(true)
            .WithPools("SnowItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Snow", 2)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Zoomlin", 1),
                    TStack("Consume", 1)
                };

            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "snowpieslinshot", englishTitle: "Snow Pie Slingshot", idleAnim: "FloatAnimationProfile")
            .SetSprites("PieSlingshot.png", "PieSlingshot BG.png")
            .SetStats(null, 0, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .CanPlayOnHand(true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Snow", 2)
                };
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("On Card Played Add Snow Pie To Hand", 1)
                };

            })
            );

        assets.Add(
           new CardDataBuilder(this)
           .CreateItem(name: "smellingsalts", englishTitle: "Smelling Salts", idleAnim: "FloatAnimationProfile")
           .SetSprites("SmellingSalts.png", "SmellingSalts BG.png")
           .WithValue(50)
           .CanPlayOnBoard(true)
           .CanPlayOnEnemy(true)
           .CanPlayOnFriendly(true)
           .CanShoveToOtherRow(true)
           .CanPlayOnHand(true)
           .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Consume"), 1))
           .AddPool("GeneralItemPool")
           .SubscribeToAfterAllBuildEvent(data =>
           {
               data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Mania", 2)
                };

           })
           );

        assets.Add(
           new CardDataBuilder(this)
           .CreateItem(name: "lotusflowertea", englishTitle: "Lotus Flower Tea", idleAnim: "FloatAnimationProfile")
           .SetSprites("LotusFlowerTea.png", "LotusFlowerTea BG.png")
           .WithValue(50)
           .CanPlayOnBoard(true)
           .CanPlayOnEnemy(true)
           .CanPlayOnFriendly(true)
           .CanShoveToOtherRow(true)
           .CanPlayOnHand(true)
           .AddPool("GeneralItemPool")
           .SetTraits(new CardData.TraitStacks(TryGet<TraitData>("Consume"), 1))
           .SubscribeToAfterAllBuildEvent(data =>
           {
               data.attackEffects = new CardData.StatusEffectStacks[1]
               {
                   SStack("Calm", 3)
               };
               var atleastxcounter = ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>();
               atleastxcounter.moreThan = 0;
               data.targetConstraints = new TargetConstraint[] { atleastxcounter };
           })
           );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "teethjar", englishTitle: "Teeth Jar", idleAnim: "ShakeAnimationProfile")
            .SetSprites("TeethJar.png", "TeethJar BG.png")
            .WithValue(50)
            .SetStats(null, 2, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .AddPool("MagicItemPool")
            .SetTraits(TStack("Consume", 1))
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Teeth", 4)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Barrage", 1)
                };
            })
            );

        assets.Add(
           new CardDataBuilder(this)
           .CreateItem(name: "scarechickmask", englishTitle: "Scarechick Mask", idleAnim: "FloatAnimationProfile")
           .SetSprites("ScarechickMask.png", "ScarechickMask BG.png")
           .WithValue(50)
           .CanPlayOnBoard(true)
           .CanPlayOnEnemy(false)
           .AddPool("MagicItemPool")
           .FreeModify(delegate (CardData c)
           {
               c.playOnSlot = true;
           })
           .SubscribeToAfterAllBuildEvent(data =>
           {
               data.startWithEffects = new CardData.StatusEffectStacks[1]
               {
                   SStack("Summon Scarechick", 1)
               };
               data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Consume", 1)
                };
           })
           );


        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "zapgum", englishTitle: "Zap Gum", idleAnim: "FloatAnimationProfile")
            .SetSprites("ZapGum.png", "ZapGum BG.png")
            .SetStats(null, 0, 0)
            .WithValue(50)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .CanPlayOnHand(true)
            .AddPool("GeneralItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Jolted", 2)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Noomlin", 1)
                };

            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "blackinkhoney", englishTitle: "Black Ink Honey", idleAnim: "FloatAnimationProfile")
            .SetSprites("BlackInkHoney.png", "BlackInkHoney BG.png")
            .WithValue(50)
            .SetStats(null, 0, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .CanPlayOnHand(true)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[]
                {
                   SStack("Null", 2),
                   SStack("Heal", 3)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateItem(name: "steamedcrabs", englishTitle: "Steamed Crabs", idleAnim: "FloatAnimationProfile")
            .SetSprites("SteamedCrabs.png", "SteamedCrabs BG.png")
            .WithValue(50)
            .SetStats(null, 0, 0)
            .CanPlayOnBoard(true)
            .CanPlayOnEnemy(true)
            .CanPlayOnFriendly(true)
            .CanShoveToOtherRow(true)
            .CanPlayOnHand(true)
            .AddPool("GeneralItemPool")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.attackEffects = new CardData.StatusEffectStacks[]
                {
                   SStack("Block", 2),
                   SStack("Haze", 2)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Consume", 1)
                };
            })
            );

        //Enemies

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("evilradtheradio", "Possesed Rad The Radio", idleAnim: "SwayAnimationProfile")
            .SetSprites("RadTheRadio.png", "RadTheRadio BG.png")
            .SetStats(null, 3, 5)
            .WithValue(2500)
            .WithCardType("Enemy")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Scrap", 5),
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Toonkind", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("lemonbatteryenemypet", "Lemon Battery Pet")
            .SetSprites("", "")
            .SetStats(10, 1, 2)
            .WithValue(150)
            .WithCardType("Enemy")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("MultiHit", 1)
                };
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Jolted", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("rubberpuffballoon", "Rubber Puff Balloon")
            .SetSprites("", "")
            .SetStats(12, 1, 5)
            .WithValue(250)
            .WithCardType("Enemy")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Remove Jolted Of EnemyRow When Hit", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Explode", 3),
                    TStack("Frontline", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("lemonbatteryenemymonster", "Lemon Battery Monster")
            .SetSprites("", "")
            .SetStats(6, 4, 5)
            .WithValue(500)
            .WithCardType("Enemy")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("Charged Trigger", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("frankenlemonphase1", "Frankenzest's Monster")
            .SetSprites("", "")
            .SetStats(18, 1, 3)
            .WithValue(800)
            .WithCardType("Boss")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("ImmuneToSnow", 1),
                    SStack("FrankenLemonPhase2", 1),
                    SStack("Ink Resistance", 1)
                };
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Jolted", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Backline", 1)
                };
            })
            );

        assets.Add(
            new CardDataBuilder(this)
            .CreateUnit("frankenlemonphase2", "Frankenzest's Monster")
            .SetSprites("", "")
            .SetStats(30, 4, 6)
            .WithValue(800)
            .WithCardType("Boss")
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.startWithEffects = new CardData.StatusEffectStacks[]
                {
                    SStack("ImmuneToSnow", 1),
                    SStack("Charged Trigger", 1),
                    SStack("Ink Immunity", 1),
                    SStack("Jolted", 1)
                };
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                   SStack("Jolted", 1)
                };
                data.traits = new List<CardData.TraitStacks>()
                {
                    TStack("Backline", 1)
                };
            })
            );

        //

        //Add new card upgrades (charms) here

        var gaincursecrownscript = ScriptableObject.CreateInstance<CardScriptGiveUpgrade>();
        gaincursecrownscript.upgradeData = TryGet<CardUpgradeData>("CrownCursed");

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeKing")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("King Charm")
            .WithImage("KingCharm.png")
            .WithText($"Gain a Cursed Crown")
            .AddPool("GeneralCharmPool")
            .WithTier(2)
            .SetScripts(gaincursecrownscript)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var doesnothaveacrown = ScriptableObject.CreateInstance<TargetConstraintHasCrown>();
                doesnothaveacrown.name = "Does Not Have A Crown";
                doesnothaveacrown.not = true;
                data.targetConstraints = new TargetConstraint[] { doesnothaveacrown };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeSin")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Sunshade Charm")
            .WithImage("SunshadeCharm.png")
            .WithText($"Gain <keyword={Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID("sin", this)}>")
            .AddPool("MagicCharmPool")
            .WithTier(2)
            .SetConstraints(ScriptableObject.CreateInstance<TargetConstraintDoesSummon>())
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.giveTraits = new CardData.TraitStacks[1]
                {
                    TStack("Sin", 1)
                };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeSpade")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Ace Of Spades Charm")
            .WithImage("AceOfSpadesCharm.png")
            .WithText($"Gain <keyword={Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID("ace", this)}>")
            .AddPool("GeneralCharmPool")
            .WithTier(2)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var doesnothaveace = ScriptableObject.CreateInstance<TargetConstraintHasTrait>();
                doesnothaveace.trait = TryGet<TraitData>("Ace");
                doesnothaveace.name = "Does Not Have Ace";
                doesnothaveace.not = true;
                data.giveTraits = new CardData.TraitStacks[1]
                {
                    TStack("Ace", 1)
                };
                data.targetConstraints = new TargetConstraint[] { doesnothaveace };

            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeBonerattle")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Bonerattle Charm")
            .WithImage("BonerattleCharm.png")
            .WithText("<+1><keyword=attack>\nGain killing an enemy also counts as <Sacrificing> an ally")
            .AddPool("MagicCharmPool")
            .WithTier(2)
            .ChangeDamage(+1)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var doesnothavesacotherteam = ScriptableObject.CreateInstance<TargetConstraintHasStatus>();
                doesnothavesacotherteam.status = TryGet<StatusEffectData>("On Kill Count As Sacrifice For Other Team");
                doesnothavesacotherteam.name = "Does Not Have On Kill Count As Sacrifice For Other Team";
                doesnothavesacotherteam.not = true;
                data.effects = new CardData.StatusEffectStacks[1]
                {
                SStack("On Kill Count As Sacrifice For Other Team", 1)
                };
                data.targetConstraints = new TargetConstraint[] { doesnothavesacotherteam };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeHeartberry")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Heartberry Charm")
            .WithImage("HeartyberryCharm.png")
            .WithText("<-1><keyword=attack>\nGain restore <3><keyword=health> and <keyword=cleanse> allies in the row")
            .AddPool("GeneralCharmPool")
            .WithTier(2)
            .ChangeDamage(-1)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var atleastxattack = ScriptableObject.CreateInstance<TargetConstraintAttackMoreThan>();
                atleastxattack.value = 0;
                data.effects = new CardData.StatusEffectStacks[1]
                {
                    SStack("On Card Played Heal & Cleanse Allies In Row", 3)
                };
                data.targetConstraints = new TargetConstraint[] { atleastxattack };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeBlockade")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Blockade Charm")
            .WithImage("BlockadeCharm.png")
            .WithText($"Start with <3><keyword=block>\nGain <keyword=frontline>")
            .AddPool("GeneralCharmPool")
            .WithTier(2)
            .SetConstraints(ScriptableObject.CreateInstance<TargetConstraintIsUnit>())
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var doesnothavefrontline = ScriptableObject.CreateInstance<TargetConstraintHasTrait>();
                doesnothavefrontline.trait = TryGet<TraitData>("Frontline");
                doesnothavefrontline.name = "Does Not Have Frontline";
                doesnothavefrontline.not = true;
                var isunit = ScriptableObject.CreateInstance<TargetConstraintIsUnit>();
                data.effects = new CardData.StatusEffectStacks[1]
                {
                    SStack("Block", 3)
                };
                data.giveTraits = new CardData.TraitStacks[1]
                {
                    TStack("Frontline", 1)
                };
                data.targetConstraints = new TargetConstraint[] { doesnothavefrontline, isunit };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeInkbottle")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Ink Bottle Charm")
            .WithImage("InkBottleCharm.png")
            .WithText("When hit, apply <3><keyword=null> to the attacker")
            .AddPool("GeneralCharmPool")
            .WithTier(1)
            .SetConstraints(ScriptableObject.CreateInstance<TargetConstraintCanBeHit>())
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.effects = new CardData.StatusEffectStacks[1]
                {
                    SStack("When Hit Apply Ink To The Attacker 2", 3)
                };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeGlassSword")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Glass Sword Charm")
            .WithImage("GlassSwordCharm.png")
            .WithText($"<+3><keyword=attack>\nGain <keyword={Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID("weak", this)}>")
            .AddPool("GeneralCharmPool")
            .WithTier(2)
            .ChangeDamage(+3)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var doesnothaveweak = ScriptableObject.CreateInstance<TargetConstraintHasTrait>();
                doesnothaveweak.trait = TryGet<TraitData>("Weak");
                doesnothaveweak.name = "Does Not Have Weak";
                doesnothaveweak.not = true;
                data.giveTraits = new CardData.TraitStacks[1]
                {
                    TStack("Weak", 1)
                };
                data.targetConstraints = new TargetConstraint[] { doesnothaveweak };

            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeLuminGear")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Lumin Gear Charm")
            .WithImage("LuminGearCharm.png")
            .WithText("Boost effects of an <Item> by <3>\nGain <Recycle> <1>")
            .AddPool("ClunkCharmPool")
            .WithTier(1)
            .ChangeEffectBonus(3)
            .SetConstraints(ScriptableObject.CreateInstance<TargetConstraintCanBeBoosted>())
            .SetConstraints(ScriptableObject.CreateInstance<TargetConstraintIsItem>())
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var isitem = ScriptableObject.CreateInstance<TargetConstraintIsItem>();
                var canbeboosted = ScriptableObject.CreateInstance<TargetConstraintCanBeBoosted>();
                data.giveTraits = new CardData.TraitStacks[1]
                {
                    TStack("Recycle", 1)
                };
                data.targetConstraints = new TargetConstraint[] { canbeboosted, isitem };
            })
            );

        var replaceattackwithshroom = ScriptableObject.CreateInstance<CardScriptReplaceAttackWithApply>();
        replaceattackwithshroom.effect = TryGet<StatusEffectData>("Shroom");

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeCordyceps")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Cordyceps Charm")
            .WithImage("CordycepsCharm.png")
            .WithText($"Replace current <keyword=attack> with apply <keyword=shroom>")
            .AddPool("BasicCharmPool")
            .WithTier(2)
            .SetScripts(replaceattackwithshroom)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var atleastxattack2 = ScriptableObject.CreateInstance<TargetConstraintAttackMoreThan>();
                atleastxattack2.value = 0;
                data.targetConstraints = new TargetConstraint[] { atleastxattack2 };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeTadpole1")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Axel Lottie Charm")
            .WithImage("TadpoleCharm3.png")
            .WithText("Gain 2 <keyword=fury>\nDoes not take up a charm slot")
            .AddPool("BasicCharmPool")
            .WithTier(1)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.takeSlot = false;
                data.giveTraits = new CardData.TraitStacks[1]
                {
                    TStack("Fury", 2)
                };
                var atleastxattack3 = ScriptableObject.CreateInstance<TargetConstraintIsOffensive>();
                var isunit = ScriptableObject.CreateInstance<TargetConstraintIsUnit>();
                data.targetConstraints = new TargetConstraint[] { isunit, atleastxattack3 };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeTadpole2")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Newton Charm")
            .WithImage("TadpoleCharm2.png")
            .WithText("Gain 2 <keyword=fury>\nDoes not take up a charm slot")
            .AddPool("ClunkCharmPool")
            .WithTier(1)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.takeSlot = false;
                data.giveTraits = new CardData.TraitStacks[1]
                {
                    TStack("Fury", 2)
                };
                var atleastxattack3 = ScriptableObject.CreateInstance<TargetConstraintIsOffensive>();
                var isunit = ScriptableObject.CreateInstance<TargetConstraintIsUnit>();
                data.targetConstraints = new TargetConstraint[] { isunit, atleastxattack3 };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeTadpole3")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Polly Wog Charm")
            .WithImage("TadpoleCharm1.png")
            .WithText("Gain 2 <keyword=fury>\nDoes not take up a charm slot")
            .AddPool("MagicCharmPool")
            .WithTier(1)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.takeSlot = false;
                data.giveTraits = new CardData.TraitStacks[1]
                {
                    TStack("Fury", 2)
                };
                var atleastxattack3 = ScriptableObject.CreateInstance<TargetConstraintIsOffensive>();
                var isunit = ScriptableObject.CreateInstance<TargetConstraintIsUnit>();
                data.targetConstraints = new TargetConstraint[] { isunit, atleastxattack3 };

            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeEel")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Slippery Eel Charm")
            .WithImage("SlipperyEelCharm.png")
            .WithText($"<-1><keyword=attack>\nAdd <x1><keyword=frenzy> and gain <keyword={Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID("bump", this)}>")
            .AddPool("GeneralCharmPool")
            .WithTier(2)
            .ChangeDamage(-1)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var doesnothavebump = ScriptableObject.CreateInstance<TargetConstraintHasTrait>();
                doesnothavebump.trait = TryGet<TraitData>("Bump");
                doesnothavebump.name = "Does Not Have Bump";
                doesnothavebump.not = true;
                var atleastxcounter = ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>();
                atleastxcounter.moreThan = 0;
                var isitem = ScriptableObject.CreateInstance<TargetConstraintIsItem>();
                var hasreaction = ScriptableObject.CreateInstance<TargetConstraintHasReaction>();
                var atleastmorethan1effect = ScriptableObject.CreateInstance<TargetConstraintEffectsMoreThan>();
                var cantrigger = ScriptableObject.CreateInstance<TargetConstraintOr>();
                cantrigger.constraints = new TargetConstraint[] { atleastxcounter, hasreaction, isitem };
                data.giveTraits = new CardData.TraitStacks[1]
                {
                    TStack("Bump", 1)
                };
                data.effects = new CardData.StatusEffectStacks[1]
                {
                    SStack("MultiHit", 1)
                };
                data.targetConstraints = new TargetConstraint[] { doesnothavebump, cantrigger };

            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeLantern")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Lantern Charm")
            .WithImage("LanternCharm.png")
            .WithText("Start with <6><keyword=calmkeyword>\nGain <keyword=frontline>")
            .AddPool("GeneralCharmPool")
            .WithTier(2)
            .SetConstraints(ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>())
            .SubscribeToAfterAllBuildEvent(data =>
            {

                data.effects = new CardData.StatusEffectStacks[1]
                {
                    SStack("FakeCalm", 6)
                };
                data.giveTraits = new CardData.TraitStacks[1]
                {
                    TStack("Frontline", 1)
                };
                var doesnothavefrontline = ScriptableObject.CreateInstance<TargetConstraintHasTrait>();
                doesnothavefrontline.trait = TryGet<TraitData>("Frontline");
                doesnothavefrontline.name = "Does Not Have Frontline";
                doesnothavefrontline.not = true;
                var atleastxcounter = ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>();
                atleastxcounter.moreThan = 0;
                data.targetConstraints = new TargetConstraint[] { atleastxcounter, doesnothavefrontline };

            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeMania")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Mania Charm")
            .WithImage("ManiaCharm.png")
            .WithText("Start with <x4><keyword=maniakeyword>\nReduce effects by <1>")
            .AddPool("GeneralCharmPool")
            .WithTier(2)
            .ChangeEffectBonus(-1)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var atleastxcounter = ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>();
                atleastxcounter.moreThan = 0;
                var isitem = ScriptableObject.CreateInstance<TargetConstraintIsItem>();
                var hasreaction = ScriptableObject.CreateInstance<TargetConstraintHasReaction>();
                var atleastmorethan1effect = ScriptableObject.CreateInstance<TargetConstraintEffectsMoreThan>();
                var cantrigger = ScriptableObject.CreateInstance<TargetConstraintOr>();
                cantrigger.constraints = new TargetConstraint[] { atleastxcounter, hasreaction, isitem };
                atleastmorethan1effect.amount = 0;
                data.targetConstraints = new TargetConstraint[] { atleastmorethan1effect, cantrigger };
                data.effects = new CardData.StatusEffectStacks[1]
                {
                    SStack("Mania", 4)
                };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgrade9Volt")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Battery Charm")
            .WithImage("BatteryCharm.png")
            .WithText("Apply <3><keyword=joltedkeyword>")
            .AddPool("GeneralCharmPool")
            .WithTier(1)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.becomesTargetedCard = true;
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("Jolted", 3)
                };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeTeacup")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Teacup Charm")
            .WithImage("TeacupCharm.png")
            .WithText("On kill, apply <1><keyword=calmkeyword> to ally behind")
            .AddPool("GeneralCharmPool")
            .WithTier(2)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.effects = new CardData.StatusEffectStacks[1]
                {
                    SStack("On Kill Calm Ally Behind", 1)
                };
                var atleastxattack3 = ScriptableObject.CreateInstance<TargetConstraintIsOffensive>();
                data.targetConstraints = new TargetConstraint[] { atleastxattack3 };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradePoker")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Poker Chip Charm")
            .WithImage("PokerChipCharm.png")
            .WithText($"Gain <keyword={Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID("wildmagic", this)}> <2>")
            .AddPool("GeneralCharmPool")
            .WithTier(1)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var atleastxattack3 = ScriptableObject.CreateInstance<TargetConstraintIsOffensive>();
                data.targetConstraints = new TargetConstraint[] { atleastxattack3 };
                data.giveTraits = new CardData.TraitStacks[1]
                {
                    TStack("Wildmagic", 2)
                };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeReel")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Film Reel Charm")
            .WithImage("FilmReelCharm.png")
            .WithText($"Gain <keyword={Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID("toonkind", this)}>")
            .AddPool("ClunkCharmPool")
            .AddPool("BasicCharmPool")
            .WithTier(2)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var doesnothavetoon = ScriptableObject.CreateInstance<TargetConstraintHasTrait>();
                doesnothavetoon.trait = TryGet<TraitData>("Toonkind");
                doesnothavetoon.name = "Does Not Have Toonkind";
                doesnothavetoon.not = true;
                var isunit = ScriptableObject.CreateInstance<TargetConstraintIsUnit>();
                var atleastxattack4 = ScriptableObject.CreateInstance<TargetConstraintIsOffensive>();
                data.targetConstraints = new TargetConstraint[] { doesnothavetoon, isunit, atleastxattack4 };
                data.giveTraits = new CardData.TraitStacks[1]
                {
                    TStack("Toonkind", 1)
                };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeCursedBom")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Broken Bom Charm")
            .WithImage("BrokenBom.png")
            .WithText("Start with <1><keyword=weakness>")
            .WithTier(-2)
            .SetConstraints(ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>())
            .SubscribeToAfterAllBuildEvent(data =>
            {

                data.effects = new CardData.StatusEffectStacks[1]
                {
                    SStack("Weakness", 1),
                };
                var hashealth = ScriptableObject.CreateInstance<TargetConstraintHealthMoreThan>();
                hashealth.value = 0;
                data.targetConstraints = new TargetConstraint[] { hashealth };

            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradePurpleHeart")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Purple Heart Charm")
            .WithImage("PurpleHeartCharm.png")
            .WithText("When consumed, apply <3><keyword=demonize> to all enemies\nWhen consumed, restore <2><keyword=health> to all allies")
            .AddPool("GeneralCharmPool")
            .WithTier(2)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                var doesnothaveconsume = ScriptableObject.CreateInstance<TargetConstraintHasTrait>();
                doesnothaveconsume.trait = TryGet<TraitData>("Consume");
                doesnothaveconsume.name = "Does Have Consume";
                doesnothaveconsume.not = false;
                data.targetConstraints = new TargetConstraint[] { doesnothaveconsume };
                data.effects = new CardData.StatusEffectStacks[]
                {
                    SStack("When Destroyed Restore Health To Allies", 3),
                    SStack("When Destroyed Apply Demonize To Enemies", 2)
                };
            })
            );

        assets.Add(
            new CardUpgradeDataBuilder(this)
            .Create("CardUpgradeConduit")
            .WithType(CardUpgradeData.Type.Charm)
            .WithTitle("Lightbulb Charm")
            .WithImage("LighgtbulbCharm.png")
            .WithText("Apply <1><keyword=joltedkeyword>\nGain <keyword=megamarine.wildfrost.sleetstorm.charged> Trigger")
            .AddPool("GeneralCharmPool")
            .WithTier(2)
            .SubscribeToAfterAllBuildEvent(data =>
            {
                data.becomesTargetedCard = true;
                data.attackEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("Jolted", 1)
                };
                data.effects = new CardData.StatusEffectStacks[]
                {
                    SStack("Charged Trigger", 1)
                };
                var isunit = ScriptableObject.CreateInstance<TargetConstraintIsUnit>();
                data.targetConstraints = new TargetConstraint[] { isunit };

            })
            );

        // Add Sunbells here

        var addRandomCharmScript = ScriptableObject.CreateInstance<CardScriptGiveRandomUpgrade>();
        addRandomCharmScript.upgradePool = AddressableLoader.GetGroup<CardUpgradeData>("CardUpgradeData")
            .ToArray()
            .RemoveFromArray(item => item.tier >= 0)
            .RemoveFromArray(item => item.type == CardUpgradeData.Type.Charm)
            .RemoveFromArray(item => item.name != "CardUpgradeMuncher")
            .RemoveFromArray(item => item.name != "CardUpgradeEffigy")
            .RemoveFromArray(item => item.name != "bethanw10.wildfrost.allcharms.CardUpgradeScissors")
            .RemoveFromArray(item => item.name != "absentabigail.wildfrost.absentavalanche.CardUpgradeChangeLeader");

        var charmBellScript = ScriptableObject.CreateInstance<ScriptRunScriptsOnDeckFixed>();
        charmBellScript.constraints = new TargetConstraint[] { };
        charmBellScript.countRange = new Vector2Int(3, 3);
        charmBellScript.scriptRefs = new CardScript[] { addRandomCharmScript };

        assets.Add(new GameModifierDataBuilder(this)
            .Create("BlessingCharms")
            .WithTitle("Charming Sun Bell", SystemLanguage.English)
            .WithDescription("Add a random <Charm> to <3> random cards in your deck", SystemLanguage.English)
            .WithBellAndDinger(this, "ModifierBell3Charm.png", "ModifierDinger3Charm.png")
            .WithStartScripts(charmBellScript)
            .WithSetupScripts()
            .WithSystemsToAdd()
            .WithVisible()
            .WithValue(50)
            .WithRingSfxEvent(Get<GameModifierData>("BoostAllEffects").ringSfxEvent)
            .WithRingSfxPitch(Get<GameModifierData>("BoostAllEffects").ringSfxPitch)
            .SubscribeToAfterAllBuildEvent(
            (data) =>
            {
                foreach (ClassData classData in AddressableLoader.GetGroup<ClassData>("ClassData"))
                {
                    foreach (RewardPool rewardPool in classData.rewardPools)
                    {
                        if (rewardPool == null || rewardPool.list == null)
                            continue;

                        if (rewardPool.name == "GeneralModifierPool" && !rewardPool.list.Contains(data))
                            rewardPool.list.Add(data);
                    }
                }
                addRandomCharmScript.upgradePool = AddressableLoader.GetGroup<CardUpgradeData>("CardUpgradeData")
                    .ToArray()
                    .RemoveFromArray(item => item.tier >= 0)
                    .RemoveFromArray(item => item.type == CardUpgradeData.Type.Charm)
                    .RemoveFromArray(item => item.name != "CardUpgradeMuncher")
                    .RemoveFromArray(item => item.name != "CardUpgradeEffigy")
                    .RemoveFromArray(item => item.name != "bethanw10.wildfrost.allcharms.CardUpgradeScissors");


            })
            );

        //


        preLoaded = true;
    }

    public bool moreFrostLoaded = false;

    [HarmonyPatch(typeof(FinalBossGenerationSettings), "ProcessEffects", new Type[]
    {
            typeof(IList<CardData>)
    })]

    internal static class AppendEffectSwapper
    {
        internal static void Prefix(FinalBossGenerationSettings __instance)
        {

            List<FinalBossEffectSwapper> swappers = new List<FinalBossEffectSwapper>
            {
                //CreateSwapper("","",minBoost: 0, maxBoost: 0),
                CreateSwapper("On Card Played Add Zoomlin To Rightmost Card In Hand", "On Card Played Destroy Rightmost Card In Hand", minBoost: 0, maxBoost: 0),
                CreateSwapper("When Hit Apply Spice Shell Teeth Or Frost To RandomAlly", "When Hit Apply Vim Null Shroom Or Block To RandomEnemy", minBoost: -1, maxBoost: -1),
                CreateSwapper("On Turn Apply Spice To Self","On Card Played Apply Spice To RandomAlly",minBoost: 0, maxBoost: 0),
                CreateSwapper("On Turn Apply Demonize To Self","On Turn Apply Demonize To RandomEnemy 2",minBoost: 0, maxBoost: 0),
                CreateSwapper("On Card Played Shroom Equal To Health To RandomEnemy","On Card Played Apply Shroom To RandomEnemy",minBoost: 3, maxBoost: 3),
                CreateSwapper("Bonus Damage Equal To Scrap On Board","While Active Increase Attack by Current To Allies",minBoost: 0, maxBoost: 0),
                CreateSwapper("On Card Played Trigger RandomEnemyInRow","On Card Played Reduce Counter Row",minBoost: 1, maxBoost: 1),
                CreateSwapper("On Card Played Reduce Counter AllyAhead","On Card Played Reduce Counter Row",minBoost: 1, maxBoost: 1),
                CreateSwapper("On Card Deployed RadRadio To Board","On Card Deployed EvilRadRadio To Board",minBoost: 0, maxBoost: 0),
                CreateSwapper("On Card Played Trigger Rad Radio","On Card Played Trigger Evil Rad Radio",minBoost: 0, maxBoost: 0),
                CreateSwapper("Investigate The Draw Pile When Deployed","On Card Played Destroy Rightmost Card In Hand",minBoost: 0, maxBoost: 0),

            };
            __instance.effectSwappers = __instance.effectSwappers.AddRangeToArray(swappers.ToArray()).ToArray();
            __instance.cardModifiers = __instance.cardModifiers.AddRangeToArray(new FinalBossCardModifier[]
            {

                new FinalBossCardModifier()
                {
                    card = Instance.TryGet<CardData>("yuura"),
                    runAll = new CardScript[]
                    {
                        new CardScriptAddPassiveEffect()
                        {
                            effect = Instance.TryGet < StatusEffectData >("Block"),
                            countRange = new Vector2Int(4, 4)
                        },
                    }
                },
                new FinalBossCardModifier()
                {
                    card = Instance.TryGet<CardData>("theengineer"),
                    runAll = new CardScript[]
                    {
                        new CardScriptRemovePassiveEffect()
                        {
                            toRemove = new StatusEffectData[] { Instance.TryGet<StatusEffectData>("Weakness") }

                        },
                        new CardScriptRemovePassiveEffect()
                        {
                            toRemove = new StatusEffectData[] { Instance.TryGet<StatusEffectData>("On Card Played Trigger Clunker Ahead") }

                        },
                    }
                },
                new FinalBossCardModifier()
                {
                    card = Instance.TryGet<CardData>("ailyn"),
                    runAll = new CardScript[]
                    {
                        new CardScriptRemovePassiveEffect()
                        {
                            toRemove = new StatusEffectData[] { Instance.TryGet<StatusEffectData>("MultiHit") }

                        },
                        new CardScriptRemoveTrait()
                        {
                            toRemove = new TraitData[] { Instance.TryGet<TraitData>("Heartburn") }
                        },
                    }
                },
                new FinalBossCardModifier()
                {
                    card = Instance.TryGet<CardData>("biff"),
                    runAll = new CardScript[]
                    {
                        new CardScriptRemoveTrait()
                        {
                            toRemove = new TraitData[] { Instance.TryGet<TraitData>("Bump") }
                        },


                    }
                },
                new FinalBossCardModifier()
                {
                    card = Instance.TryGet<CardData>("skalemoji"),
                    runAll = new CardScript[]
                    {
                        new CardScriptRemoveTrait()
                        {
                            toRemove = new TraitData[] { Instance.TryGet<TraitData>("Ace") }
                        },
                        new CardScriptAddPassiveEffect()
                        {
                            effect = Instance.TryGet < StatusEffectData >("Teeth"),
                            countRange = new Vector2Int(4, 4)
                        },
                        new CardScriptRemovePassiveEffect()
                        {
                            toRemove = new StatusEffectData[] { Instance.TryGet<StatusEffectData>("When Health Lost Apply Equal Teeth To Self") }

                        },
                    }
                },
                new FinalBossCardModifier()
                {
                    card = Instance.TryGet<CardData>("duster"),
                    runAll = new CardScript[]
                    {
                        new CardScriptRemovePassiveEffect()
                        {
                            toRemove = new StatusEffectData[]
                            {
                                Instance.TryGet<StatusEffectData>("While Active Multihit Equal Health"),
                                Instance.TryGet<StatusEffectData>("On Card Played Damage To Self After Turn")
                            }

                        },
                        new CardScriptAddPassiveEffect()
                        {
                            effect = Instance.TryGet < StatusEffectData >("MultiHit"),
                            countRange = new Vector2Int(5, 5)
                        },
                    }
                },
                new FinalBossCardModifier()
                {
                    card = Instance.TryGet<CardData>("phileap"),
                    runAll = new CardScript[]
                    {
                        new CardScriptRemoveTrait()
                        {
                            toRemove = new TraitData[] { Instance.TryGet<TraitData>("Fury") }
                        },
                        new CardScriptAddPassiveEffect()
                        {
                            effect = Instance.TryGet < StatusEffectData >("On Turn Apply Attack To Self"),
                            countRange = new Vector2Int(1, 1)
                        },
                    }
                },
                new FinalBossCardModifier()
                {
                    card = Instance.TryGet<CardData>("sallyleader"),
                    runAll = new CardScript[]
                    {
                        new CardScriptAddPassiveEffect()
                        {
                            effect = Instance.TryGet < StatusEffectData >("Block"),
                            countRange = new Vector2Int(0, 0)
                        },
                    }
                },
                new FinalBossCardModifier()
                {
                    card = Instance.TryGet<CardData>("abbykleader"),
                    runAll = new CardScript[]
                    {
                        new CardScriptRemovePassiveEffect()
                        {
                            toRemove = new StatusEffectData[]
                            {
                                Instance.TryGet<StatusEffectData>("When Hit Apply Overload To Attacker")
                            }
                        },
                        new CardScriptAddAttackEffect()
                        {
                            effect = Instance.TryGet < StatusEffectData >("Overload"),
                            countRange = new Vector2Int(4, 4)
                        },
                    }

                },
                new FinalBossCardModifier()
                {
                    card = Instance.TryGet<CardData>("vavaleader"),
                    runAll = new CardScript[]
                    {
                        new CardScriptAddPassiveEffect()
                        {
                            effect = Instance.TryGet < StatusEffectData >("MultiHit"),
                            countRange = new Vector2Int(2, 2)
                        },
                    }
                },
                new FinalBossCardModifier()
                {
                    card = Instance.TryGet<CardData>("racca"),
                    runAll = new CardScript[]
                    {
                        new CardScriptRemovePassiveEffect()
                        {
                            toRemove = new StatusEffectData[]
                            {
                                Instance.TryGet<StatusEffectData>("When Hit Apply Gold To Attacker (No Ping) By Gunk")
                            }
                        }
                    }

                },

            }
            );

        }

        internal static FinalBossEffectSwapper CreateSwapper(string effect, string replaceOption = null, string attackOption = null, int minBoost = 0, int maxBoost = 0)
        {
            FinalBossEffectSwapper swapper = ScriptableObject.CreateInstance<FinalBossEffectSwapper>();
            swapper.effect = Instance.TryGet<StatusEffectData>(effect);
            swapper.replaceWithOptions = new StatusEffectData[0];
            String s = "";
            if (!replaceOption.IsNullOrEmpty())
            {
                swapper.replaceWithOptions = swapper.replaceWithOptions.Append(Instance.TryGet<StatusEffectData>(replaceOption)).ToArray();
                s += swapper.replaceWithOptions[0].name;
            }
            if (!attackOption.IsNullOrEmpty())
            {
                swapper.replaceWithAttackEffect = Instance.TryGet<StatusEffectData>(attackOption);
                s += swapper.replaceWithAttackEffect.name;
            }
            if (s.IsNullOrEmpty())
            {
                s = "Nothing";
            }
            swapper.boostRange = new Vector2Int(minBoost, maxBoost);
            Debug.Log($"[sleetstorm] {swapper.effect.name} => {s} + {swapper.boostRange}");
            return swapper;
        }
    }

    [HarmonyPatch(typeof(References), nameof(References.Classes), MethodType.Getter)]
    static class FixClassesGetter
    {
        static void Postfix(ref ClassData[] __result) => __result = AddressableLoader.GetGroup<ClassData>("ClassData").ToArray();
    }

    [HarmonyPatch(typeof(WildfrostMod.DebugLoggerTextWriter), nameof(WildfrostMod.DebugLoggerTextWriter.WriteLine))]
    class PatchHarmony
    {
        static bool Prefix() { Postfix(); return false; }
        static void Postfix() => HarmonyLib.Tools.Logger.ChannelFilter = HarmonyLib.Tools.Logger.LogChannel.Warn | HarmonyLib.Tools.Logger.LogChannel.Error;
    }

    [HarmonyPatch(typeof(ShoveSystem), nameof(ShoveSystem.CanShoveToOtherRow))]
    class ShoveToOtherRowFrontline
    {
        static bool Prefix(Entity shovee, out Dictionary<Entity, List<CardSlot>> shoveData, ref bool __result)
        {
            shoveData = new Dictionary<Entity, List<CardSlot>>();

            if (!Instance.HasLoaded || shovee.owner == Battle.instance.enemy) return true;

            var cardSlotLane = shovee.containers[0] as CardSlotLane;
            var index = cardSlotLane.IndexOf(shovee);
            var otherLane = cardSlotLane.shoveTo?[0] as CardSlotLane;

            if (otherLane != null && otherLane[index] != null && otherLane[index].positionPriority > shovee.positionPriority)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    public class ExtraPopups
    {
        static readonly Dictionary<string, (string keyword, PopGroup group)[]> flavours = new()
    {
        { $"{Sleetstorm.Instance.GUID}.starlight", [
            ($"{Sleetstorm.Instance.GUID}.sheher", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.ondanleader", [
            ($"{Sleetstorm.Instance.GUID}.theyshe", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.tobias", [
            ($"{Sleetstorm.Instance.GUID}.hethey", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.mersel", [
            ($"{Sleetstorm.Instance.GUID}.hethey", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.duster", [
            ($"{Sleetstorm.Instance.GUID}.hehim", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.larrysamuel", [
            ($"{Sleetstorm.Instance.GUID}.hehim", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.craigory", [
            ($"{Sleetstorm.Instance.GUID}.hehim", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.mintyleader", [
            ($"{Sleetstorm.Instance.GUID}.sheher", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.vavaleader", [
            ($"{Sleetstorm.Instance.GUID}.sheher", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.baph", [
            ($"{Sleetstorm.Instance.GUID}.sheher", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.loosesilver", [
            ($"{Sleetstorm.Instance.GUID}.hethey", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.lemon", [
            ($"{Sleetstorm.Instance.GUID}.sheher", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.sallyleader", [
            ($"{Sleetstorm.Instance.GUID}.sheher", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.sorrelmist", [
            ($"{Sleetstorm.Instance.GUID}.hethey", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.phileap", [
            ($"{Sleetstorm.Instance.GUID}.hethey", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.pip", [
            ($"{Sleetstorm.Instance.GUID}.theythem", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.abbykleader", [
            ($"{Sleetstorm.Instance.GUID}.sheher", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.cassiusblackmont", [
            ($"{Sleetstorm.Instance.GUID}.hehim", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.2ufo", [
            ($"{Sleetstorm.Instance.GUID}.theythem", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.willowthetrain", [
            ($"{Sleetstorm.Instance.GUID}.sheher", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.theengineer", [
            ($"{Sleetstorm.Instance.GUID}.hehim", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.rowan", [
            ($"{Sleetstorm.Instance.GUID}.hehim", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.higgs", [
            ($"{Sleetstorm.Instance.GUID}.hehim", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.biff", [
            ($"{Sleetstorm.Instance.GUID}.sheher", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.blink", [
            ($"{Sleetstorm.Instance.GUID}.hethey", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.yuura", [
            ($"{Sleetstorm.Instance.GUID}.hethey", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.camilleleader", [
            ($"{Sleetstorm.Instance.GUID}.hehim", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.bunnileader", [
            ($"{Sleetstorm.Instance.GUID}.sheher", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.ailyn", [
            ($"{Sleetstorm.Instance.GUID}.sheher", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.ames", [
            ($"{Sleetstorm.Instance.GUID}.shethey", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.skalemoji", [
            ($"{Sleetstorm.Instance.GUID}.hethey", PopGroup.Left)
        ] },
        { $"{Sleetstorm.Instance.GUID}.ami", [
            ($"{Sleetstorm.Instance.GUID}.sheher", PopGroup.Left)
        ] },
    };

        public enum PopGroup
        {
            Left,
            LeftOverflow,
            Right,
            RightOverflow,
            Bottom
        }
        static Transform GetGroup(CardInspector inspector, PopGroup popGroup) =>
            popGroup switch
            {
                PopGroup.Left => inspector.leftPopGroup,
                PopGroup.LeftOverflow => inspector.leftOverflowPopGroup,
                PopGroup.Right => inspector.rightPopGroup,
                PopGroup.RightOverflow => inspector.rightOverflowPopGroup,
                PopGroup.Bottom => inspector.bottomPopGroup
            };
        static Transform GetGroup(InspectSystem inspector, PopGroup popGroup) =>
        popGroup switch
        {
            PopGroup.Left => inspector.leftPopGroup,
            PopGroup.LeftOverflow => inspector.leftOverflowPopGroup,
            PopGroup.Right => inspector.rightPopGroup,
            PopGroup.RightOverflow => inspector.rightOverflowPopGroup,
            PopGroup.Bottom => inspector.bottomPopGroup
        };


        [HarmonyPatch(typeof(InspectSystem), nameof(InspectSystem.CreatePopups))]
        static void Postfix(InspectSystem __instance)
        {
            Entity inspect = __instance.inspect;
            if (inspect.display is not Card card) return;
            if (!flavours.TryGetValue(inspect.name, out var flavour)) return;

            foreach ((string keyword, PopGroup group) in flavour)
            {
                KeywordData data = Text.ToKeyword(keyword);
                __instance.Popup(data, GetGroup(__instance, group));
            }
        }

        [HarmonyPatch(typeof(CardInspector), nameof(CardInspector.CreatePopups))]
        static void Postfix(CardInspector __instance, Entity inspect)
        {
            if (inspect.display is not Card card) return;
            if (!flavours.TryGetValue(inspect.name, out var flavour)) return;

            foreach ((string keyword, PopGroup group) in flavour)
            {
                KeywordData data = Text.ToKeyword(keyword);
                __instance.Popup(data, GetGroup(__instance, group));
            }
        }
    }

    //[HarmonyPatch(typeof(FinalBossGenerationSettings), methodName:"GenerateBonusEnemies", [
    //    typeof(int), typeof(IEnumerable<CardData>), typeof(CardData[])
    //    ])]
    //
    //internal static class AppendCardReplacement
    //{
    //
    //    internal static void Prefix(FinalBossGenerationSettings __instance)
    //    {
    //        var enemyGenerator = ScriptableObject.CreateInstance<FinalBossEnemyGenerator>();
    //        enemyGenerator.enemy = Sleetstorm.TryGet<CardData>(.name);
    //    }
    //}

    [HarmonyPatch(typeof(ShoveSystem), nameof(ShoveSystem.CanShoveTo))]
    class ShoveToFrontline
    {
        static bool Prefix(Entity shovee, Entity shover, int dir, CardSlot[] slots, out Dictionary<Entity, List<CardSlot>> shoveData, ref bool __result)
        {
            shoveData = new Dictionary<Entity, List<CardSlot>>();
            var hasSwappedDirection = false;

            if (!Instance.HasLoaded || shovee.owner == Battle.instance.enemy) return true;

            int num = 1;
            Queue<KeyValuePair<Entity, CardSlot[]>> queue = new Queue<KeyValuePair<Entity, CardSlot[]>>();
            queue.Enqueue(new KeyValuePair<Entity, CardSlot[]>(shovee, slots));
            List<Entity> list = new List<Entity>();
            bool result = false;
            while (queue.Count > 0)
            {
                KeyValuePair<Entity, CardSlot[]> keyValuePair = queue.Dequeue();
                Entity key = keyValuePair.Key;
                list.Add(key);
                CardSlot[] value = keyValuePair.Value;
                if (value == null || value.Length == 0)
                {
                    break;
                }
                // positionPriority is -1 for Backline, 2 for Frontline
                if (shovee.positionPriority > shover.positionPriority)
                {
                    break;
                }
                List<CardSlot> list2 = new List<CardSlot>();
                foreach (CardSlot cardSlot in value)
                {
                    if (shoveData.ContainsKey(key))
                    {
                        shoveData[key].Add(cardSlot);
                    }
                    else
                    {
                        shoveData[key] = new List<CardSlot>
                {
                    cardSlot
                };
                    }
                    Entity top = cardSlot.GetTop();
                    if (top != null && top != shover)
                    {
                        list2.Add(cardSlot);
                    }
                }
                num--;

                foreach (CardSlot cardSlot2 in list2)
                {
                    Entity blockingEntity = cardSlot2.GetTop();
                    if (!list.Contains(blockingEntity) && !queue.Any((KeyValuePair<Entity, CardSlot[]> p) => p.Key == blockingEntity))
                    {
                        CardSlot[] value2 = ShoveSystem.FindSlots(blockingEntity, dir);

                        // scuffed solution to frontline shenanigans :3
                        if ((value2 is null || value2.Length == 0) && blockingEntity._containers.Count == 1 && shover.positionPriority > 1)
                        {
                            if (!hasSwappedDirection)
                            {
                                var lane = blockingEntity.containers[0] as CardSlotLane;
                                var index = lane.IndexOf(blockingEntity);
                                if (lane.shoveTo != null && lane.shoveTo.Count > 0)
                                {
                                    var laneAcross = lane.shoveTo[0] as CardSlotLane;
                                    if (laneAcross != null && !(lane.Contains(shovee) && lane.Contains(shover)) && !(laneAcross.Contains(shovee) && laneAcross.Contains(shover)))
                                    {
                                        var targetSlot = laneAcross.slots[index];
                                        if (targetSlot != null && (targetSlot.entities.Count == 0 || targetSlot.GetTop().positionPriority <= shover.positionPriority))
                                        {
                                            hasSwappedDirection = true;
                                            dir *= -1;
                                            value2 = new CardSlot[] { targetSlot };
                                        }
                                    }
                                }
                            }
                        }

                        queue.Enqueue(new KeyValuePair<Entity, CardSlot[]>(blockingEntity, value2));
                        num++;
                    }
                }
            }
            if (num <= 0)
            {
                result = true;
            }
            __result = result;
            return false;
        }
    }

    //Tribe Hut Patch Part 1
    [HarmonyPatch(typeof(TribeHutSequence), "SetupFlags")]
    class PatchTribeHut
    {
        static string TribeName = "Toonkind";
        static void Postfix(TribeHutSequence __instance)                                            //After it unlocks the base mods, it'll move on to ours.
        {
            GameObject gameObject = GameObject.Instantiate(__instance.flags[0].gameObject);         //Clone the Snowdweller flag
            gameObject.transform.SetParent(__instance.flags[0].gameObject.transform.parent, false); //Place it in the same group as the others
            TribeFlagDisplay flagDisplay = gameObject.GetComponent<TribeFlagDisplay>();
            ClassData tribe = Sleetstorm.instance.TryGet<ClassData>(TribeName);
            flagDisplay.flagSprite = tribe.flag;                                                    //Replace the flag with our tribe flag
            __instance.flags = __instance.flags.Append(flagDisplay).ToArray();                      //Add it the flag to the list to check
            flagDisplay.SetAvailable();                                                             //Set it available
            flagDisplay.SetUnlocked();                                                              //And unlocked
            TribeDisplaySequence sequence2 = GameObject.FindObjectOfType<TribeDisplaySequence>(true);   //TribeDisplaySequence sequence should be unique, so Find should find the right one.
            GameObject gameObject2 = GameObject.Instantiate(sequence2.displays[1].gameObject);          //Copy one of them (Shademancers)
            gameObject2.transform.SetParent(sequence2.displays[2].gameObject.transform.parent, false);  //Place the copy in the right place in the hieracrhy
            sequence2.tribeNames = sequence2.tribeNames.Append(TribeName).ToArray();                    //Add the name to the list
            sequence2.displays = sequence2.displays.Append(gameObject2).ToArray();                      //Add the display itself to the list

            Button button = flagDisplay.GetComponentInChildren<Button>();                               //Find the button component on our flagDisplay
            button.onClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);   //Deactivate the cloned listener (which opens the Snowdweller display)
            button.onClick.AddListener(() => { sequence2.Run(TribeName); });                            //Add our own listener that opens our display

            //(SfxOneShot)
            gameObject2.GetComponent<SfxOneshot>().eventRef = FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/status/ink"); //Ink noise

            //0: Flag (ImageSprite)
            gameObject2.transform.GetChild(0).GetComponent<ImageSprite>().SetSprite(tribe.flag);        //Set the sprite of the ImageSprite component to our tribe flag

            //1: Left (ImageSprite)
            Sprite needle = Sleetstorm.instance.TryGet<CardData>("mersel").mainSprite;             //Find Mersel's sprite
            gameObject2.transform.GetChild(1).GetComponent<ImageSprite>().SetSprite(needle);            //and set it as the left image

            //2: Right (ImageSprite)
            Sprite muncher = Sleetstorm.instance.TryGet<CardData>("starlight").mainSprite;           //Find Starlight's sprite
            gameObject2.transform.GetChild(2).GetComponent<ImageSprite>().SetSprite(muncher);           //and set it as the right image
            gameObject2.transform.GetChild(2).localScale *= 1.4f;                                       //and make it 40% bigger

            //3: Textbox (Image)
            gameObject2.transform.GetChild(3).GetComponent<UnityEngine.UI.Image>().color = Color(47, 42, 61); //Change the color of the textbox background

            //3-0: Text (LocalizeStringEvent)
            StringTable collection = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English);   //Find a string table (in the desired language)
            gameObject2.transform.GetChild(3).GetChild(0).GetComponent<LocalizeStringEvent>().StringReference = collection.GetString(Sleetstorm.TribeDescKey);
            //Set the string in the LocaliseStringEvent

            //4:Title Ribbon (Image)
            //4-0: Text (LocalizeStringEvent)
            gameObject2.transform.GetChild(4).GetChild(0).GetComponent<LocalizeStringEvent>().StringReference = collection.GetString(Sleetstorm.TribeTitleKey);
            //Set the string in the LocaliseStringEvent

        }
    }

    private static readonly string[] summoned = { "berryboom", "perplotoo", "aberrantshade", "larrysamuel", "hogsteria", "scarechick", "jayson" };
    private static readonly string[] summoner = { "berryboommask", "perplotoomask", "lemon", "larrymask", "hogsteriamask", "scarechickmask", "jaysonmask" };

    public static int counter = 0;
    private void SaveCounter(RedrawBellSystem arg0)
    {
        counter = arg0.counter.current;
    }

    public static Color Color(int r, int b, int g)
    {
        Color color = new Color(
            r / 255F,
            b / 255F,
            g / 255F
        );

        return color;
    }

    private void AddCharmsToGloomBell()
    {
        foreach (var hardModifier in MonoBehaviourSingleton<References>.instance.hardModeModifiers)
        {
            if (hardModifier.name == "7.CardsStartWithNegativeCharms")
            {
                (hardModifier.modifierData.startScripts[0] as ScriptDowngradeCardRewards).charms = (hardModifier.modifierData.startScripts[0] as ScriptDowngradeCardRewards).charms.AddRangeToArray
                    (new CardUpgradeData[]
                    {
                        TryGet<CardUpgradeData>("CardUpgradeCursedBom")
                    });

            }
        }
    }

    public void CreateBattles()
    {
        new BattleDataEditor(this, "Frankenzest's Monster")
            .SetSprite("")
            .SetNameRef("Frankenzest's Laboratory")
            .EnemyDictionary(
                ('L', "frankenlemonphase1"),
                ('P', "lemonbatteryenemypet"),
                ('M', "lemonbatteryenemymonster"),
                ('R', "rubberpuffballoon"))
            .StartWavePoolData(0, "Wave 1:Frankensteins Monster is Here")
            .ConstructWaves(3, 0, "LPR")
            .StartWavePoolData(1, "Wave 2:More Monsters Arrive")
            .ConstructWaves(3, 1, "PMP", "PMM")
            .StartWavePoolData(2, "Wave 3:Final Phase")
            .ConstructWaves(2, 9, "RP")
            .AddBattleToLoader().LoadBattle(2)
            .ToggleBattle(active:false)
            .GiveMiniBossesCharms(new string[] { "frankenlemonphase1" }, "CardUpgradeBoost", "CardUpgradeBlock");
            
    }

    public override void Load()
    {
        //Events.OnSceneChanged += CompanionPhoto;
        if (Bootstrap.Mods.ToList().Where(mod => mod.GUID == "KDeveloper.Wildfrost.MFMod").Count() > 0)
        {
            if (Deadpan.Enums.Engine.Components.Modding.Extensions.GetModFromGuid("KDeveloper.Wildfrost.MFMod").HasLoaded) moreFrostLoaded = true;
        }
        if (!preLoaded) { CreateModAssets(); }
        base.Load();
        EyeDataAdder.Eyes();
        FloatingText ftext = GameObject.FindObjectOfType<FloatingText>(true);
        ftext.textAsset.spriteAsset.fallbackSpriteAssets.Add(assetSprites);
        Events.OnRedrawBellHit += SaveCounter;
        instance = this;
        GameMode gameMode = TryGet<GameMode>("GameModeNormal"); //GameModeNormal is the standard game mode. 
        gameMode.classes = gameMode.classes.Append(TryGet<ClassData>("Toonkind")).ToArray();
        Events.OnEntityCreated += FixImage;
        CreateLocalizedStrings();
        AddCharmsToGloomBell();
        CreateBattles();
        Events.OnMinibossIntro += VFXHelper.MinibossIntro;

        //We need to make our petPrefab here, otherwise the Prefab gets destroyed leading to a crash
        Events.OnSceneChanged += LoadOomlin;


    }

    private void LoadOomlin(Scene scene)
    {
        if (scene.name == "Town")
        {
            ItemHolderPetNoomlin oomlin = ((StatusEffectFreeAction)TryGet<StatusEffectData>("Free Action")).petPrefab.InstantiateKeepName() as ItemHolderPetNoomlin;

            //Different head options, you can have as many/little as you like
            oomlin.headOptions = new Sprite[] { ImagePath("GoomlinBigHat.png").ToSprite(), ImagePath("GoomlinCrossStitch.png").ToSprite(), ImagePath("GoomlinEgg.png").ToSprite(), ImagePath("GoomlinStar.png").ToSprite(), ImagePath("GoomlinSwirl.png").ToSprite(), ImagePath("GoomlinDrill.png").ToSprite(), ImagePath("GoomlinWave.png").ToSprite() };

            foreach (Image image in oomlin.showTween.GetComponentsInChildren<Image>()) //Prefab for when oomlin sits on the card
            {
                //To get the right offsets do the following:
                //(1) Give a card your effect
                //(2) Hover over the card and inspect this
                //(3) Click Inspect GameObject
                //(4) Click the arrow on Wobber, Flipper, CurveAnimator, Offset, Canvas, Front, FrameOutline, ItemHolderPetCreater, Noomlin(Clone)
                //Click on Head to adjust head scale/position of head with ears
                //Click on the arrow by Head and then click Head/EarLeft/EarRight to edit ears/head individuallys
                switch (image.name)
                {
                    case "Body": //Body that hangs on top of card
                        image.sprite = ImagePath("GoomlinBody.png").ToSprite(); break;
                    case "EarLeft": //Left ear
                        image.sprite = ImagePath("GoomlinEar_Left.png").ToSprite();
                        image.transform.localPosition = new Vector3(-0.4562f, 0.2244f, 0); break;
                    case "EarRight": //Right ear
                        image.sprite = ImagePath("GoomlinEar_Right.png").ToSprite();
                        image.transform.localPosition = new Vector3(0.4816f, 0.2644f, 0f); break;
                    case "Tail": //Tail for when the -oomlin jumps off
                        image.sprite = ImagePath("GoomlinTail.png").ToSprite(); break;
                    case "Head": //Head, done just to rescale it
                        image.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
                        image.transform.Translate(new Vector3(0f, 0.25f, 0f)); break;
                }

            }

        ((StatusEffectApplyXOnCardPlayedWithPet)TryGet<StatusEffectData>("Free Play With Trash")).petPrefab = oomlin;
        }
    }

    public override void Unload()
    {
        Events.OnRedrawBellHit -= SaveCounter;
        //Events.OnSceneChanged -= CompanionPhoto;
        base.Unload();
        GameMode gameMode = TryGet<GameMode>("GameModeNormal");
        gameMode.classes = RemoveNulls(gameMode.classes); //Without this, a non-restarted game would crash on tribe selection
        UnloadFromClasses();
        Events.OnEntityCreated -= FixImage;
        Events.OnSceneChanged -= LoadOomlin;
        Events.OnMinibossIntro -= VFXHelper.MinibossIntro;
    }

    public override List<T> AddAssets<T, Y>()
    {
        if (assets.OfType<T>().Any())
            Debug.LogWarning($"[{Title}] adding {typeof(Y).Name}s: {assets.OfType<T>().Select(a => a._data.name).Join()}");
        return assets.OfType<T>().ToList();
    }
    public void UnloadFromClasses()
    {
        List<ClassData> tribes = AddressableLoader.GetGroup<ClassData>("ClassData");
        foreach (ClassData tribe in tribes)
        {
            if (tribe == null || tribe.rewardPools == null) { continue; } //This isn't even a tribe; skip it.

            foreach (RewardPool pool in tribe.rewardPools)
            {
                if (pool == null) { continue; }; //This isn't even a reward pool; skip it.

                pool.list.RemoveAllWhere((item) => item == null || item.ModAdded == this); //Find and remove everything that needs to be removed.
            }
        }
    }
    internal T[] RemoveNulls<T>(T[] data) where T : DataFile
    {
        List<T> list = data.ToList();
        list.RemoveAll(x => x == null || x.ModAdded == this);
        return list.ToArray();
    }
}