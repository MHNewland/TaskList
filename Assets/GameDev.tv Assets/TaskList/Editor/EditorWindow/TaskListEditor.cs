using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;

namespace GameDevTV.Tasks
{
    public class TaskListEditor : EditorWindow
    {
        #region UIElements
        VisualElement container;
        ObjectField savedTasksObjectField;
        Button loadTasksButton;
        TextField taskText;
        Button addTextButton;
        ScrollView taskListScrollView;
        Button saveProgressButton;
        ProgressBar taskProgressBar;
        ToolbarSearchField taskSearchBox;
        Button removeCompletedButton;
        TextField newListName;
        Button newListButton;
        #endregion

        TaskListSO taskListSO;

        public const string path = "Assets/GameDev.tv Assets/TaskList/Editor/EditorWindow/";

        //sets a new menu item to show window
        [MenuItem("GameDev.TV/Task List")]
        public static void ShowWindow()
        {
            TaskListEditor window = GetWindow<TaskListEditor>();

            window.titleContent = new GUIContent("Task List");
            window.minSize = new Vector2(300, 300);
        }

        #region createGUI
        public void CreateGUI()
        {
            //Set up Visual Tree
            container = rootVisualElement;
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "TaskListEditor.uxml");
            container.Add(visualTree.Instantiate());

            //add Stylesheet
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path + "TaskListEditor.uss");
            container.styleSheets.Add(styleSheet);

            //query tree for each field
            taskText = container.Q<TextField>("TaskText");
            addTextButton = container.Q<Button>("AddTaskButton");
            taskListScrollView = container.Q<ScrollView>("TaskList");
            savedTasksObjectField = container.Q<ObjectField>("SavedTasksObjectField");
            loadTasksButton = container.Q<Button>("LoadTasksButton");
            saveProgressButton = container.Q<Button>("SaveProgressButton");
            taskProgressBar = container.Q<ProgressBar>("TaskProgress");
            taskSearchBox = container.Q<ToolbarSearchField>("TaskSearchBox");
            removeCompletedButton = container.Q<Button>("RemoveCompleted");
            newListName = container.Q<TextField>("NewListName");
            newListButton = container.Q<Button>("NewListButton");

            //Set ObjectField to allow TaskListSO
            savedTasksObjectField.objectType = typeof(TaskListSO);

            //Load saved Tasks
            loadTasksButton.clicked += LoadTasks;

            //Add task
            addTextButton.clicked += AddTask;
            taskText.RegisterCallback<KeyDownEvent>(AddTask);

            //save Progress
            saveProgressButton.clicked += SaveProgress;

            //searching
            taskSearchBox.RegisterValueChangedCallback(HighlightTask);

            //remove completed tasks
            removeCompletedButton.clicked += RemoveCompleted;

            //make new list
            newListButton.clicked += CreateNewList;
            newListName.RegisterCallback<KeyDownEvent>(CreateNewList);
        }
        #endregion

        #region Create/Load Tasks
        private void CreateNewList()
        {
            //make sure there's something in the name field
            if (!string.IsNullOrEmpty(newListName.text))
            {
                //create the path if doesn't exist
                if (!AssetDatabase.IsValidFolder("Assets/Task Lists/"))
                {
                    AssetDatabase.CreateFolder("Assets", "Task Lists");
                }

                //make sure there's not an asset there with that name already
                Debug.Log("Assets/Task Lists/" + newListName.text);
                bool assetFound = false;
                string[] assetNames = AssetDatabase.FindAssets(newListName.text.Trim(), new string[] { "Assets/Task Lists" });
                foreach(string name in assetNames)
                {
                    if ((AssetDatabase.GUIDToAssetPath(name)).Equals("Assets/Task Lists/" + newListName.text+".asset"))
                    {
                        assetFound = true;
                        break;
                    }
                }

                if (!assetFound)
                {
                    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance("TaskListSO"), "Assets/Task Lists/" + newListName.text.Trim() + ".asset");
                }
                else
                {
                    Debug.Log("list already exists");
                }

                //move new list into list field and load it.
                savedTasksObjectField.value = AssetDatabase.LoadAssetAtPath<TaskListSO>("Assets/Task Lists/" + newListName.text.Trim() + ".asset");
            }

        }

        private void CreateNewList(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return)
            {
                CreateNewList();
            }
        }

        private void LoadTasks()
        {
            taskListSO = savedTasksObjectField.value as TaskListSO;

            if (taskListSO != null)
            {
                taskListScrollView.Clear();
                List<TaskItem> tasks = taskListSO.GetTasks();
                foreach (TaskItem task in tasks)
                {
                    taskListScrollView.Add(CreateTask(task));
                }
            }
            isChecked();
            UpdateProgress();
        }
        #endregion

        #region Make new Tasks
        private void AddTask()
        {
            if (!string.IsNullOrEmpty(taskText.value))
            {
                //setting the text puts the text to the right
                //setting the label during the contructor puts the text to the left.
                taskListScrollView.Add(CreateTask(new TaskItem(taskText.value)));
                taskText.value = "";
                taskText.Focus();
                UpdateProgress();

            }
        }

        private void AddTask(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Return)
            {
                AddTask();
            }
        }

        TaskItem CreateTask(TaskItem task)
        {
            if (task != null)
            {
                TaskItem taskItem = new TaskItem(task.GetTaskName(), task.GetCompleted());
                taskItem.GetTaskToggle().RegisterValueChangedCallback(UpdateProgress);
                taskItem.GetTaskToggle().RegisterValueChangedCallback(isChecked);
                return taskItem;
            }
            return new TaskItem("sampleTask");
        }
        #endregion

        #region Auto update UI
        private void isChecked(ChangeEvent<bool> evt)
        {
            isChecked();
        }

        private void isChecked()
        {
            foreach (TaskItem task in taskListScrollView.Children())
            {
                if (task.GetTaskToggle().value)
                {
                    task.GetTaskToggle().AddToClassList("TaskCheckboxChecked");
                    task.GetTaskLabel().AddToClassList("TaskCheckboxChecked");
                }
                else
                {
                    if (task.GetTaskLabel().ClassListContains("TaskCheckboxChecked"))
                    {
                        task.GetTaskLabel().RemoveFromClassList("TaskCheckboxChecked");

                    }
                    if (task.GetTaskToggle().ClassListContains("TaskCheckboxChecked"))
                    {
                        task.GetTaskToggle().RemoveFromClassList("TaskCheckboxChecked");

                    }
                }
            }
        }

        private void HighlightTask(ChangeEvent<string> e)
        {
            if (taskListSO != null)
            {
                foreach (TaskItem task in taskListScrollView.Children())
                {
                    if (!String.IsNullOrEmpty(taskSearchBox.value) && task.GetTaskLabel().text.ToUpper().Contains(taskSearchBox.value.ToUpper()))
                    {
                        task.GetTaskLabel().AddToClassList("Highlight");
                    }
                    else if (task.GetTaskLabel().ClassListContains("Highlight")) { 
                        task.GetTaskLabel().RemoveFromClassList("Highlight");
                    }
                }
            }
        }

        private void UpdateProgress()
        {
            if (taskListSO != null)
            {
                float numTasks = taskListScrollView.childCount;
                float completedTasks = 0;
                if (numTasks == 0)
                {
                    taskProgressBar.value = 0;
                    taskProgressBar.title = "0%";
                    return;
                }
                foreach (TaskItem task in taskListScrollView.Children())
                {
                    if (task.GetTaskToggle().value)
                    {
                        completedTasks++;
                    }
                }
                taskProgressBar.value = completedTasks / numTasks;
                taskProgressBar.title = ((int)(taskProgressBar.value * 100)).ToString() + "%";
                AssetDatabase.Refresh();
            }
        }

        private void UpdateProgress(ChangeEvent<bool> e)
        {
            UpdateProgress();
        }
        #endregion

        #region completing tasks
        private void SaveProgress()
        {
            if (taskListSO != null)
            {
                if (savedTasksObjectField.value != null && savedTasksObjectField.value.name != taskListSO.name)
                {
                    taskListSO = savedTasksObjectField.value as TaskListSO;
                }
                List<TaskItem> tasks = new List<TaskItem>();

                foreach(TaskItem task in taskListScrollView.Children())
                {
                    tasks.Add(task);
                }
                taskListSO.AddTasks(tasks);
                EditorUtility.SetDirty(taskListSO);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                LoadTasks();
            }
        }

        private void RemoveCompleted()
        {

            if (taskListSO != null)
            {
                List<TaskItem> tasks = new List<TaskItem>();

                foreach (TaskItem task in taskListScrollView.Children())
                {
                    //save only the tasks that aren't checked
                    if(!task.GetTaskToggle().value) tasks.Add(task);
                }
                taskListSO.AddTasks(tasks);
                EditorUtility.SetDirty(taskListSO);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                LoadTasks();
            }
        }
        #endregion
    }
}