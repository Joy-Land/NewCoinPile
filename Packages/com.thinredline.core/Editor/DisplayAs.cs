using UnityEngine;
using UnityEditor;

namespace ThinRL.AssetBuilder.Editor
{

    //public class DisplayAs : PropertyAttribute
    //{
    //    private string _label;

    //    public DisplayAs(string label)
    //    {
    //        this._label = label;
    //    }

    //    [CustomPropertyDrawer(typeof(DisplayAs))]
    //    public class ThisPropertyDrawer : PropertyDrawer
    //    {
    //        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //        {
    //            try
    //            {
    //                var propertyAttribute = attribute as DisplayAs;
    //                label.text = propertyAttribute?._label;

    //                EditorGUI.PropertyField(position, property, label);
    //            }
    //            catch (System.Exception ex)
    //            {
    //                Debug.LogException(ex);
    //            }
    //        }
    //    }
    //}


    [CustomPropertyDrawer(typeof(InspectorNameAttribute))]
    public class InspectorNamePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                var propertyAttribute = attribute as InspectorNameAttribute;
                label.text = propertyAttribute?.displayName;

                EditorGUI.PropertyField(position, property, label);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}