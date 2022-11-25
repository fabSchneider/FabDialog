using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog
{
    public static class StateGraphGUI
    {
        public static readonly string nodeClassname = "dialog-node";
        public static readonly string mainClassname = nodeClassname + "__main-container";
        public static readonly string titleClassname = nodeClassname + "__title-container";
        public static readonly string extensionClassname = nodeClassname + "__extension-container";

        public static readonly string buttonClassname = nodeClassname + "__button";
        public static readonly string customDataClassname = nodeClassname + "__custom-data-container";
        public static readonly string helpButtonClassName = nodeClassname + "__text-help-button";

        public static readonly string nameTextFieldClassname = nodeClassname + "__name-text-field";
        public static readonly string textFieldClassname = nodeClassname + "__text-field";
        public static readonly string textFieldHiddenClassname = textFieldClassname + "--hidden";

        public static readonly string quoteTextFieldClassname = nodeClassname + "__quote-text-field";
        public static readonly string choiceTextFieldClassname = nodeClassname + "__choice-text-field";

        public static VisualElement CreateCustomDataContainer()
        {
            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList(customDataClassname);
            return customDataContainer;
        }

        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = text
            };

            button.AddToClassList(buttonClassname);
            return button;
        }

        public static Button CreateHelpButton(Action onClick = null)
        {
            Button helpButton = new Button(onClick);
            helpButton.text = "i";
            helpButton.AddToClassList(helpButtonClassName);
            return helpButton;
        }

        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            Foldout foldout = new Foldout()
            {
                text = title,
                value = !collapsed
            };
            return foldout;
        }

        public static TextField CreateTextField(string value = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new TextField()
            {
                value = value
            };

            if (onValueChanged != null)
                textField.RegisterCallback(onValueChanged);

            textField.AddToClassList(textFieldClassname);
            textField.AddToClassList(textFieldHiddenClassname);

            return textField;
        }

        public static VisualElement CreateQuoteTextEditor(string title, string value = null, bool collapsed = false, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            Foldout textFoldout = CreateFoldout(title, collapsed);


            Button textHelpButton = CreateHelpButton(() => Debug.Log("Help!!"));
            textFoldout.Q(className: "unity-foldout__input").Add(textHelpButton);

            TextField editorTextField = CreateTextField(value, onValueChanged);
            editorTextField.multiline = true;

            editorTextField.AddToClassList(quoteTextFieldClassname);

            editorTextField.RegisterCallback<MouseUpEvent>(evt =>
            {
                editorTextField.panel.contextualMenuManager.DisplayMenu(evt, evt.target);

                evt.StopPropagation();
                evt.PreventDefault();
            });

            textFoldout.Add(editorTextField);
            return textFoldout;
        }
    }
}
