using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ChestConfigEditor : EditorWindow
{
    private ChestConfig _chestConfig;
    private List<RewardToggleInfo> _rewardInfo;
    private int _rewardsCount = 0;

    private Vector2 _scrollPos;

    [MenuItem("Window/Chest config")]
    private static void OpenWindow()
    {
        ChestConfigEditor wnd = (ChestConfigEditor)GetWindow(typeof(ChestConfigEditor));
        wnd.Show();
    }

    private void OnEnable()
    {
        _chestConfig = (ChestConfig)CreateInstance(typeof(ChestConfig));
        _rewardInfo = new List<RewardToggleInfo>();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150));

        if (GUILayout.Button("Create Config"))
        {
            CreateAsset();
        }


        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10, false);


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Количество наград:");
        _rewardsCount = EditorGUILayout.IntField(_rewardsCount);
        EditorGUILayout.EndHorizontal();

        if (_rewardsCount > 0)
        {
            if (_rewardInfo.Count < _rewardsCount)
            {
                for (int i = _rewardInfo.Count - 1; i < _rewardsCount; i++)
                {
                    _rewardInfo.Add(new RewardToggleInfo() { reward = new ChestConfig.RewardInfo(), isExpanded = true });
                }
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            for (int i = 0; i < _rewardsCount; i++)
            {
                _rewardInfo[i].isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_rewardInfo[i].isExpanded, $"Награда {i + 1}");

                if (_rewardInfo[i].isExpanded)
                {
                    EditorGUI.indentLevel++;
                    var reward = _rewardInfo[i].reward;
                    DrawItem(reward);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space(15);
            }

            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void CreateAsset()
    {
        var serObj = new SerializedObject(_chestConfig);
        var prop = serObj.FindProperty("_rewards");

        var type = typeof(ChestConfig);
        FieldInfo fieldInfo = type.GetField("_rewards", BindingFlags.Instance | BindingFlags.NonPublic);
        var value = fieldInfo.GetValue(_chestConfig);
        fieldInfo.SetValue(_chestConfig, _rewardInfo.Take(_rewardsCount).Select(r => r.reward).ToArray());

        AssetDatabase.CreateAsset(_chestConfig, "Assets/Chest_config.asset");
    }

    private void DrawItem(ChestConfig.RewardInfo reward)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Вес:");
        reward.randomWeight = EditorGUILayout.Slider(reward.randomWeight, 0, 1);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Тип награды:");
        reward.type = (ChestConfig.RewardType)EditorGUILayout.EnumPopup(reward.type);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        switch (reward.type)
        {
            case ChestConfig.RewardType.Soft:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Soft min:");
                reward.softMin = EditorGUILayout.IntSlider(reward.softMin, 0, 1000);

                EditorGUILayout.LabelField("Soft max:");
                reward.softMax = EditorGUILayout.IntSlider(reward.softMax, reward.softMin, 1000);
                EditorGUILayout.EndHorizontal();
                break;
            case ChestConfig.RewardType.Hard:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Hard min:");
                reward.hardMin = EditorGUILayout.Slider(reward.hardMin, 0, 1000);

                EditorGUILayout.LabelField("Hard max:");
                reward.hardMax = EditorGUILayout.Slider(reward.hardMax, reward.hardMin, 1000);
                EditorGUILayout.EndHorizontal();
                break;
            case ChestConfig.RewardType.Chest:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Chest config:");

                if (reward.chest == null)
                {
                    GUI.color = Color.red;
                }

                reward.chest = (ChestConfig)EditorGUILayout.ObjectField(reward.chest, typeof(ChestConfig), false);
                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("chestCount:");
                if (reward.chestCount == 0)
                {
                    GUI.color = Color.red;
                }
                reward.chestCount = EditorGUILayout.IntField(reward.chestCount);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();

                break;
            default:
                break;
        }
    }

    private class RewardToggleInfo
    {
        public ChestConfig.RewardInfo reward;
        public bool isExpanded;
    }


}
