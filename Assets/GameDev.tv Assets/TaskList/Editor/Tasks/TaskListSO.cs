using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevTV.Tasks
{
    [CreateAssetMenu(menuName = "Task List", fileName = "New Task List")]
    public class TaskListSO : ScriptableObject
    {
        public List<TaskItem> tasks = new List<TaskItem>();

        // Returns lists of tasks as strings
        public List<TaskItem> GetTasks()
        {
            return tasks;
        }


        // clears task list and saves new list
        public void AddTasks(List<TaskItem> savedTasks)
        {
            tasks.Clear();
            foreach(TaskItem t in savedTasks)
            {
                tasks.Add(new TaskItem(t.GetTaskLabel().text, t.GetTaskToggle().value));
            }
        }
    }
}