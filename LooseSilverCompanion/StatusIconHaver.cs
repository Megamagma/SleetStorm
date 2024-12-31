using Deadpan.Enums.Engine.Components.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.Localization.Tables;
using UnityEngine;

namespace sleetstorm
{
    static class StatusIconHaver
    {
        public static StringTable Collection => LocalizationHelper.GetCollection("Card Text", SystemLanguage.English);
        public static StringTable KeyCollection => LocalizationHelper.GetCollection("Tooltips", SystemLanguage.English);


        // Creates the GameObject that is the icon
        // Make sure type is set to the same string as what you set type to for your status effect
        // copyTextFrom copies the text formating from an existing icon
        public static GameObject CreateIcon(this WildfrostMod mod, string name, Sprite sprite, string type, string copyTextFrom, Color textColor, KeywordData[] keys)
        {
            return CreateIcon(mod, name, sprite, type, copyTextFrom, textColor, shadowColor: new Color(0, 0, 0), keys);
        }

        /// <param name="type">has to equal the image filename</param>
        public static GameObject CreateIcon(this WildfrostMod mod, string name, Sprite sprite, string type, string copyTextFrom, Color textColor, Color shadowColor, KeywordData[] keys)
        {
            GameObject gameObject = new GameObject(name);
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
            StatusIconExt icon = gameObject.AddComponent<StatusIconExt>();
            Dictionary<string, GameObject> cardIcons = CardManager.cardIcons;
            if (!copyTextFrom.IsNullOrEmpty())
            {
                GameObject text = cardIcons[copyTextFrom].GetComponentInChildren<TextMeshProUGUI>().gameObject.InstantiateKeepName();
                text.transform.SetParent(gameObject.transform);
                icon.textElement = text.GetComponent<TextMeshProUGUI>();
                icon.textColour = textColor;
                icon.textColourAboveMax = textColor;
                icon.textColourBelowMax = textColor;
                icon.textElement.fontMaterial.SetColor("_UnderlayColor", shadowColor);
                // Credit to Hopeful :3
            }
            icon.onCreate = new UnityEngine.Events.UnityEvent();
            icon.onDestroy = new UnityEngine.Events.UnityEvent();
            icon.onValueDown = new UnityEventStatStat();
            icon.onValueUp = new UnityEventStatStat();
            icon.afterUpdate = new UnityEngine.Events.UnityEvent();

            UnityEngine.UI.Image image = gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;

            CardHover cardHover = gameObject.AddComponent<CardHover>();
            cardHover.enabled = false;
            cardHover.IsMaster = false;

            CardPopUpTarget cardPopUp = gameObject.AddComponent<CardPopUpTarget>();
            cardPopUp.keywords = keys;
            cardHover.pop = cardPopUp;
            cardPopUp.posX = -1; // Display keyword to the left of the card

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.sizeDelta *= 0.01f;

            gameObject.SetActive(true);
            icon.type = type;
            cardIcons[type] = gameObject;

            return gameObject;
        }

        //This creates the keyword
        public static KeywordData CreateIconKeyword(this WildfrostMod mod, string name, string title, string desc, string icon, Color body, Color titleC, Color note, Color panel)
        {
            KeywordData data = ScriptableObject.CreateInstance<KeywordData>();
            data.name = name;
            KeyCollection.SetString(data.name + "_text", title);
            data.titleKey = KeyCollection.GetString(data.name + "_text");
            KeyCollection.SetString(data.name + "_desc", desc);
            data.descKey = KeyCollection.GetString(data.name + "_desc");
            data.showIcon = true;
            data.showName = false;
            data.iconName = icon;
            data.ModAdded = mod;
            data.bodyColour = body;
            data.titleColour = titleC;
            data.noteColour = note;
            data.panelColor = panel;
            AddressableLoader.AddToGroup<KeywordData>("KeywordData", data);
            return data;
        }


        //This custom class extends the StatusIcon class to automatically add listeners so that the number on the icon will update automatically
        public class StatusIconExt : StatusIcon
        {
            public override void Assign(Entity entity)
            {
                base.Assign(entity);
                SetText();
                onValueDown.AddListener(delegate { Ping(); });
                onValueUp.AddListener(delegate { Ping(); });
                afterUpdate.AddListener(SetText);
                onValueDown.AddListener(CheckDestroy);

                Debug.Log("Why you not loading StatusIconExt?");
            }
        }

    }
}

