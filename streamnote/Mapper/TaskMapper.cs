﻿using System.Collections.Generic;
using Streamnote.Relational.Helpers;
using Streamnote.Relational.Models;
using Streamnote.Relational.Models.Descriptors;

namespace Streamnote.Web.Mapper
{
    /// <summary>
    /// Mapper for tasks.
    /// </summary>
    public class TaskMapper
    {
        private readonly UserMapper UserMapper;
        private readonly DateTimeHelper DateTimeHelper;
        private readonly StepMapper StepMapper;
        private readonly TaskCommentMapper TaskCommentMapper;

        public TaskMapper(DateTimeHelper dateTimeHelper, UserMapper userMapper, StepMapper stepMapper, TaskCommentMapper taskCommentMapper)
        {
            DateTimeHelper = dateTimeHelper;
            UserMapper = userMapper;
            StepMapper = stepMapper;
            TaskCommentMapper = taskCommentMapper;
        }

        /// <summary>
        /// Map a list of tasks.
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<TaskDescriptor> MapDescriptors(List<TaskItem> tasks, string userId)
        {
            var taskDescriptors = new List<TaskDescriptor>();

            foreach (var task in tasks)
            {
                taskDescriptors.Add(MapDescriptor(task, userId));
            }

            return taskDescriptors;
        }

        /// <summary>
        /// Map a single task.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public TaskDescriptor MapDescriptor(TaskItem task, string userId)
        {
            var time = (task.Created > task.Modified) ? task.Created : task.Modified;

            var timeString = DateTimeHelper.GetFriendlyDateTime(time);

            return new TaskDescriptor
            {
                Id = task.Id,
                Created = task.Created,
                Modified = task.Modified,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                IsPublic = task.IsPublic,
                CreatedBy = task.CreatedBy,
                OwnedByUsername = task.OwnedByUsername,
                Steps = StepMapper.MapDescriptors(task.Steps, userId),
                Comments = TaskCommentMapper.MapDescriptors(task.Comments, userId),
                ProjectId = task.Project != null ? task.Project.Id : 0,
                TaskIdentifier = "task" + task.Id,
                TaskBoxIdentifier = "taskBox" + task.Id,
                StepsIdentifier = "steps" + task.Id,
                CommentsIdentifier = "comments" + task.Id,
            };
        }
    }
}