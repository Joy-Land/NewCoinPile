using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UITemplateGenerator : EditorWindow
{

    [MenuItem("ThinRedLine/UI代码模板生成")]
    public static void ShowWindow()
    {
        Rect windowRect = new Rect(0, 0, 600, 800);
        UITemplateGenerator window = (UITemplateGenerator)EditorWindow.GetWindow<UITemplateGenerator>("UITemplateGenerator", true);
        window.minSize = new Vector2(500, 800);
        window.Show();
    }

    string declareRes = "";
    string impRes = "";
    string defaultFuncRes = "";
    string funcRes = "";
    string registFuncRes = "";
    string unregistFuncRes = "";
    private void OnGUI()
    {
        var objcets = Selection.gameObjects;
        var assetGUID = Selection.assetGUIDs;

        EditorGUILayout.BeginVertical();
        var path = "";
        bool isPanel = false;
        if (objcets.Length > 0)
        {
            Debug.Log(objcets[0].name);
            var n = objcets[0].name;
            if (n.Contains("UI"))
            {
                if (n.Contains("Comp"))
                {
                    isPanel = false;
                }
                else
                {
                    isPanel = true;
                }
            }
        }
        else
        {
            //EditorGUILayout.EndVertical();
        }
        if (assetGUID.Length != 0)
        {
            Debug.Log(assetGUID[0]);
            path = AssetDatabase.GUIDToAssetPath(assetGUID[0]);
            //AssetDatabase.
            //if (path.Contains("/Panel"))
            //{
            //    isPanel = true;
            //}
            //else
            //{
            //    isPanel = false;
            //}
        }


        EditorGUILayout.LabelField($"当前预制体路径：{path}");
        if (GUILayout.Button("生成"))
        {
            declareRes = "";
            impRes = "";
            defaultFuncRes = "";
            funcRes = "";
            registFuncRes = "";
            unregistFuncRes = "";
            if (objcets.Length > 1)
            {
                EditorUtility.DisplayDialog("错误", "请选择一个物体生成", "确定");
                EditorGUILayout.EndVertical();
                return;
            }
            var rootName = isPanel == true ? objcets[0].name + "(Clone)" : "";
            var startStep = isPanel == true ? 0 : -1;


            var c = objcets[0].transform.childCount;
            //GenerateTemplate(objcets[0], "", isPanel, startStep);
            for (int i = 0; i < c; i++)
            {
                var child = objcets[0].transform.GetChild(i);
                if (isPanel && child.name.Contains("Component"))
                {
                    continue;
                }
                GenerateTemplate(child.gameObject, child.name, isPanel, 0);
            }

            registFuncRes = "public void RegistEvent()\n{ \n " + registFuncRes + "\n} \n ";
            unregistFuncRes = "public void UnregistEvent()\n{ \n " + unregistFuncRes + "\n} \n ";

            PackDefaultFunc(isPanel);
        }

        EditorGUILayout.LabelField("声明区");
        EditorGUILayout.TextArea(declareRes, GUILayout.MaxHeight(220));
        EditorGUILayout.LabelField("查找声明");
        EditorGUILayout.TextArea(impRes, GUILayout.MaxHeight(220));
        EditorGUILayout.LabelField("函数区");
        //GUILayout.TextField(defaultFuncRes + "\n" + registFuncRes + "\n" + funcRes + "\n" + unregistFuncRes + "\n");
        EditorGUILayout.TextArea(defaultFuncRes+"\n"+registFuncRes+"\n"+funcRes+"\n"+unregistFuncRes+"\n", GUILayout.MaxHeight(220));
        EditorGUILayout.EndVertical();
    }

    private void GenerateTemplate(GameObject root, string path, bool isPanel, int step)
    {
        var c = root.transform.childCount;
        Debug.Log("fzy c:" + root.name+"  "+step + "  "+path);
        if (step >= 0)
        {
            if (root.name.Contains("Img_"))
            {
                declareRes += $"public Image {root.name}; \n";
                if (isPanel)
                {
                    impRes += $"{root.name} = transform.Find(\"{path}\").GetComponent<Image>(); \n";
                }
                else
                {
                    impRes += $"{root.name} = transform.Find(\"{path}\").GetComponent<Image>(); \n";
                }
            }
            else if (root.name.Contains("RwImg_") || root.name.Contains("RawImg_"))
            {
                declareRes += $"public RawImage {root.name}; \n";
                if (isPanel)
                {
                    impRes += $"{root.name} = transform.Find(\"{path}\").GetComponent<RawImage>(); \n";
                }
                else
                {
                    impRes += $"{root.name} = transform.Find(\"{path}\").GetComponent<RawImage>(); \n";
                }
            }
            else if (root.name.Contains("Obj_"))
            {
                declareRes += $"public GameObject {root.name}; \n";
                if (isPanel)
                {
                    impRes += $"{root.name} = transform.Find(\"{path}\"); \n";
                }
                else
                {
                    impRes += $"{root.name} = transform.Find(\"{path}\"); \n";
                }
            }
            else if (root.name.Contains("Txt_"))
            {
                declareRes += $"public Text {root.name}; \n";
                if (isPanel)
                {
                    impRes += $"{root.name} = transform.Find(\"{path}\").GetComponent<Text>(); \n";
                }
                else
                {
                    impRes += $"{root.name} = transform.Find(\"{path}\").GetComponent<Text>(); \n";
                }

            }
            else if (root.name.Contains("Btn_"))
            {
                declareRes += $"public Button {root.name}; \n";
                if (isPanel)
                {
                    impRes += $"{root.name} = transform.Find(\"{path}\").GetComponent<Button>(); \n";
                }
                else
                {
                    impRes += $"{root.name} = transform.Find(\"{path}\").GetComponent<Button>(); \n";
                }

                registFuncRes += $"{root.name}.onClick.AddListener(On{root.name}Clicked);";

                funcRes += $"\n public void On{root.name}Clicked()" + "\n{ \n\n }\n";

                unregistFuncRes += $"{root.name}.onClick.RemoveListener(On{root.name}Clicked);";

            }
            else if (root.name.Contains("ScrView_"))
            {
                var name = root.name.Replace(" ", "_");
                declareRes += $"public ScrollRect {name}; \n";
                if (isPanel)
                {
                    impRes += $"{name} = transform.Find(\"{path}\").GetComponent<ScrollRect>(); \n";
                }
                else
                {
                    impRes += $"{name} = transform.Find(\"{path}\").GetComponent<ScrollRect>(); \n";
                }
            }
            else if (root.name.Contains("Slider_") || root.name.Contains("Sli_"))
            {
                var name = root.name.Replace(" ", "_");
                declareRes += $"public Slider {name}; \n";
                if (isPanel)
                {
                    impRes += $"{name} = transform.Find(\"{path}\").GetComponent<Slider>(); \n";
                }
                else
                {
                    impRes += $"{name} = transform.Find(\"{path}\").GetComponent<Slider>(); \n";
                }
                Debug.Log(isPanel.ToString() + " " + path + " " + impRes);
            }
        }


        for (int i = 0; i < c; i++)
        {
            var child = root.transform.GetChild(i);
            if (isPanel && child.name.Contains("Component"))
            {
                continue;
            }
            GenerateTemplate(child.gameObject, path + $"/{child.gameObject.name}", isPanel, step);
        }
    }

    private void PackDefaultFunc(bool isPanel)
    {
        if (isPanel)
        {
            defaultFuncRes += "public override void OnViewAwake(EventArgsPack args)\n" +
                "{\n" +
                "\tbase.OnViewAwake(args);\n" +
                $"{impRes}\n" +
                "\n}\n";

            defaultFuncRes += "public override void OnViewShow(EventArgsPack args)\n" +
                "{\n" +
                "\tbase.OnViewShow(args);\n" +
                "\tRegistEvent();" +
                "\n}\n";

            defaultFuncRes += "public override void OnViewUpdate()\n" +
                "{\n" +
                "\tbase.OnViewUpdate();\n" +
                "\n}\n";

            defaultFuncRes += "public override void OnViewUpdateBySecond()\n" +
                "{\n" +
                "\tbase.OnViewUpdateBySecond();\n" +
                "\n}\n";

            defaultFuncRes += "public override void OnViewHide()\n" +
                "{\n" +
                "\tbase.OnViewHide();\n" +
                "\nUnregistEvent();" +
                "\n}\n";

            defaultFuncRes += "public override void OnViewDestroy()\n" +
                "{\n" +
                "\tbase.OnViewDestroy();\n" +
                "\n}\n";

        }

        else
        {
            defaultFuncRes += "public override void OnComponentInit()\n" +
                "{\n" +
                "\tbase.OnComponentInit();\n" +
                $"{impRes}\n" +
                "\n}\n";

            defaultFuncRes += "public override void OnComponentShow()\n" +
                "{\n" +
                "\tbase.OnComponentShow();\n" +
                "\n}\n";

            defaultFuncRes += "public override void SetData(EventArgsPack args)\n" +
                "{\n" +
                "\tbase.SetData(args);\n" +
                "\n}\n";

            defaultFuncRes += "public override void OnComponentHide()\n" +
                "{\n" +
                "\tbase.OnComponentHide();\n" +
                "\n}\n";

            defaultFuncRes += "public override void OnComponentDestroy()\n" +
                "{\n" +
                "\tbase.OnComponentDestroy();\n" +
                "\n}\n";
        }
    }


}
