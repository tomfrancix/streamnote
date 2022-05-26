﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using streamnote.Data;
using streamnote.Mapper;
using streamnote.Models;
using streamnote.Repository;
using streamnote.Repository.Interface;

namespace streamnote.Controllers
{
    /// <summary>
    /// Controller for item actions.
    /// </summary>
    [Authorize]
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext Context;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly ItemMapper ItemMapper;
        private readonly CommentMapper CommentMapper;
        private readonly ImageProcessingHelper ImageProcessingHelper;
        private readonly IItemRepository ItemRepository;
        private readonly ITopicRepository TopicRepository;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userManager"></param>
        /// <param name="itemMapper"></param>
        /// <param name="commentMapper"></param>
        /// <param name="imageProcessingHelper"></param>
        public ItemController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ItemMapper itemMapper, CommentMapper commentMapper, ImageProcessingHelper imageProcessingHelper, IItemRepository itemRepository, ITopicRepository topicRepository)
        {
            Context = context;
            UserManager = userManager;
            ItemMapper = itemMapper;
            CommentMapper = commentMapper;
            ImageProcessingHelper = imageProcessingHelper;
            ItemRepository = itemRepository;
            TopicRepository = topicRepository;
        }

        /// <summary>
        /// Get the items created by the user.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {              
            var user = await UserManager.GetUserAsync(User);

            var model = ItemRepository.QueryUsersItems(user)
                .OrderByDescending(i => i.Id)
                .ToList();

            var items = ItemMapper.MapDescriptors(model, user.Id);

            return View(items);
        }

        /// <summary>
        /// Get the details for an item.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id)
        {
            var user = await UserManager.GetUserAsync(User);

            if (id == null)
            {
                return NotFound();
            }

            var item = await ItemRepository.QueryAllItems().FirstOrDefaultAsync(m => m.Id == id);

            var itemDescriptor = ItemMapper.MapDescriptor(item, user.Id);

            var comments = Context.Comments
                .Where(c => c.Item.Id == item.Id)
                .ToList();

            itemDescriptor.Comments = CommentMapper.MapDescriptors(comments, user.Id);

            return View(itemDescriptor);
        }

        /// <summary>
        /// Create an item view.
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Create an item action.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="image"></param>
        /// <param name="selectedTopics"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,IsPublic,Image")] Item item, IFormFile image, string selectedTopics)
        {
            item.Created = DateTime.UtcNow;
            item.Modified = DateTime.UtcNow;
            item.User = await UserManager.GetUserAsync(User);

            item = await AppendTopics(item, selectedTopics);

            item = AppendImage(item, image);

            if (ModelState.IsValid)
            {
                await ItemRepository.CreateItem(item);

                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        /// <summary>
        /// Edit an item.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await Context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        /// <summary>
        /// Edit an item action.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <param name="image"></param>
        /// <param name="selectedTopics"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item item, IFormFile image, string selectedTopics)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            var existing = ItemRepository.Read(id);

            if (existing != null)
            {
                existing.Image = existing.Image;
                existing.ImageContentType = existing.ImageContentType;
                existing.Modified = DateTime.UtcNow;

                existing = await AppendTopics(existing, selectedTopics);

                existing = AppendImage(existing, image);

                if (ModelState.IsValid)
                {
                    await ItemRepository.UpdateItemAsync(existing);

                    return RedirectToAction(nameof(Index));
                }
            }

            return View(item);
        }

        /// <summary>
        /// Delete an item.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = ItemRepository.Read((int)id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        /// <summary>
        /// Confirm that an item was deleted.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await Context.Items.FindAsync(id);
            Context.Items.Remove(item);
            await Context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<Item> AppendTopics(Item item, string selectedTopics)
        {
            if (selectedTopics != null)
            {
                var listOfTopics = selectedTopics.Split(",");

                foreach (var topic in listOfTopics)
                {
                    var existing = TopicRepository.QueryExistingTopic(topic);

                    if (existing is { Name: { } })
                    {
                        await TopicRepository.IncrementItemCount(existing);

                    }
                    else
                    {
                        var newTopic = new Topic
                        {
                            Name = topic.ToLower(),
                            ItemCount = 1
                        };

                        existing = await TopicRepository.CreateTopic(newTopic);
                    }

                    item.Topics.Add(existing);
                }
            }

            return item;
        }

        /// <summary>
        /// Append an image to the item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public Item AppendImage(Item item, IFormFile image)
        {
            if (image != null)
            {
                item.ImageContentType = image.ContentType;
                using (var fs = image.OpenReadStream())
                {
                    using (var br = new BinaryReader(fs))
                    {
                        item.Image = br.ReadBytes((int)fs.Length);
                    }
                }
            }

            return item;
        }
    }
}
