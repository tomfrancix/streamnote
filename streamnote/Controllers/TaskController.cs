﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Streamnote.Relational;
using Streamnote.Relational.Data;
using Streamnote.Relational.Models;
using Streamnote.Relational.Models.Descriptors;
using Streamnote.Web.Mapper;

namespace Streamnote.Web.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext Context;
        private readonly TaskMapper TaskMapper;
        private readonly UserManager<ApplicationUser> UserManager;

        public TaskController(ApplicationDbContext context, TaskMapper taskMapper, UserManager<ApplicationUser> userManager)
        {
            Context = context;
            TaskMapper = taskMapper;
            UserManager = userManager;
        }

        // GET: Task
        public async Task<IActionResult> Index()
        {
            return View(await Context.Tasks.ToListAsync());
        }

        // GET: Task/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await Context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // GET: Task/Create
        public async Task<IActionResult> New(int projectId)
        {
            var user = await UserManager.GetUserAsync(User);

            var task = new TaskDescriptor
            {         
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                Title = null,
                Description = null,
                Status = TodoStatus.Unstarted,
                IsPublic = false,
                CreatedBy = user,
                OwnedByUsername = null,
                Steps = null,
                ProjectId = projectId 
            };

            return PartialView("_CreateTask", task);
        }

        // POST: Task/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(string title, string description, bool isPublic, int projectId)
        {                
            var user = await UserManager.GetUserAsync(User);
            var now = DateTime.UtcNow;  
            var project = Context.Projects.Find(projectId);
            var task = new TaskItem
            {
                Created = now,
                Modified = now,
                Title = title,
                Description = description,
                Status = TodoStatus.Unstarted,
                IsPublic = isPublic,
                Project = project,
                CreatedBy = user
            };

            if (ModelState.IsValid)
            {
                Context.Add(task);
                await Context.SaveChangesAsync();
                var newTask = Context.Tasks
                    .Include(t => t.CreatedBy)
                    .FirstOrDefault(i => i.Created == now && i.CreatedBy.Id == user.Id);

                return PartialView("_Task", TaskMapper.MapDescriptor(newTask, user.Id)); ;
            }
            return View(task);
        }

        // GET: Task/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await Context.Tasks.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            return View(taskItem);
        }

        // POST: Task/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]              
        public async Task<IActionResult> ChangeStatus(int taskId, int status)
        {
            var user = await UserManager.GetUserAsync(User);
            var task = Context.Tasks.Include(t => t.Project).FirstOrDefault(t => t.Id == taskId);
            task.Status = (TodoStatus)status;
            if (status > 0)
            {
                task.OwnedByUsername = user.UserName;
            }
            else
            {
                task.OwnedByUsername = "";
            }

            try
                {
                    Context.Update(task);
                    await Context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(task.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
            }
                var newTask = Context.Tasks
                    .Include(t => t.CreatedBy)
                    .Include(t => t.Steps)
                    .Include(t => t.Comments).ThenInclude(c => c.User)
                    .FirstOrDefault(i => i.Id == taskId);

            return PartialView("_Task", TaskMapper.MapDescriptor(newTask, user.Id)); ;

        }

        // GET: Task/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await Context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // POST: Task/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await Context.Tasks.FindAsync(id);
            Context.Tasks.Remove(taskItem);
            await Context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskItemExists(int id)
        {
            return Context.Tasks.Any(e => e.Id == id);
        }
    }
}