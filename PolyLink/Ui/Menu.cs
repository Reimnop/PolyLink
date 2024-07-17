using PolyLink.Patch;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
using UnityEngine.UI;

namespace PolyLink.Ui;

public static class Menu
{
    private const string I18nTable = "Localization";
    
    public static void Register()
    {
        Log.Info("Registering menu");
        UiBookPatch.Initialize += UiBookPatchOnInitialize;
    }

    private static void UiBookPatchOnInitialize(UI_Book book)
    {
        AddMenuButton(book);
        AddMenuPage(book);
    }

    private static void AddMenuButton(UI_Book book)
    {
        Log.Info("Adding menu button to main menu");

        var buttonsPanelTransform = GameObject.Find("Canvas/Window/Content/Main Menu/Buttons 2").transform;
        Object.Destroy(buttonsPanelTransform.Find("Credits").gameObject); // Credits button messes up the layout, so we remove it

        var communityButton = buttonsPanelTransform.Find("Community").gameObject;
        var polyLinkButton = Object.Instantiate(communityButton, buttonsPanelTransform);

        polyLinkButton.name = "PolyLink";
        book.Pages[0].SubElements.Add(polyLinkButton.GetComponent<UI_Button>());

        const string keyPrefix = $"{PluginInfo.PLUGIN_GUID}.mainMenu";
        
        polyLinkButton.transform.Find("Title Wrapper").GetComponentInChildren<GameObjectLocalizer>()
            .TrackedObjects[0].GetTrackedProperty<LocalizedStringProperty>("m_text").LocalizedString
            .SetReference(I18nTable, $"{keyPrefix}.title");
        
        polyLinkButton.transform.Find("Content").GetComponentInChildren<GameObjectLocalizer>()
            .TrackedObjects[0].GetTrackedProperty<LocalizedStringProperty>("m_text").LocalizedString
            .SetReference(I18nTable, $"{keyPrefix}.content");
        
        var multiElementButton = polyLinkButton.GetComponent<MultiElementButton>();
        multiElementButton.onClick = new Button.ButtonClickedEvent(); // Clear existing event
        multiElementButton.onClick.AddListener((UnityAction)(() => book.SwapPage("PolyLink")));
    }
    
    private static void AddMenuPage(UI_Book book)
    {
        var windowContentTransform = GameObject.Find("Canvas/Window/Content").transform;
        var settingsPageContainer = windowContentTransform.Find("Settings").gameObject;
        var polyLinkPageContainer = Object.Instantiate(settingsPageContainer, windowContentTransform);

        var polyLinkPage = new UI_Book.Page
        {
            _ID = "PolyLink",
            Title = "PolyLink",
            PageContainer = polyLinkPageContainer,
            PrimaryPage = false,
            AllowNewSelection = false,
        };
        book.Pages.Add(polyLinkPage);
    
        // destroy all children of the menu page container
        while (polyLinkPageContainer.transform.childCount > 0)
            Object.DestroyImmediate(polyLinkPageContainer.transform.GetChild(0).gameObject);
    }
}