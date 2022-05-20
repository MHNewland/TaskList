using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace GameDevTV
{
    [Serializable]
    public class TaskItem : VisualElement
    {
        
        Toggle taskToggle;
        Label taskLabel;

        [SerializeField]
        string task;
        [SerializeField]
        bool completed;
        public const string path = "Assets/GameDev.tv Assets/TaskList/Editor/EditorWindow/";

        
        public TaskItem(string taskName)
        {
            //set up visual tree
            VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "TaskItem.uxml");
            this.Add(original.Instantiate());

            //add Stylesheet
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path + "TaskListEditor.uss");
            this.styleSheets.Add(styleSheet);

            //add toggle
            taskToggle = this.Q<Toggle>("TaskToggle");

            //add label
            taskLabel = this.Q<Label>("TaskLabel");

            taskLabel.text = taskName;

            task = taskName;
            completed = false;

        }

        public TaskItem(string taskName, bool completed) : this(taskName)
        {
            taskToggle.value = completed;
            this.completed = completed;
        }

        public Toggle GetTaskToggle()
        {
            return taskToggle;
        }

        public Label GetTaskLabel()
        {
            return taskLabel;
        }

        public string GetTaskName()
        {
            return task;
        }

        public bool GetCompleted()
        {
            return completed;
        }
   
    }
}